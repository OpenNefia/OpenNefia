using OpenNefia.Content.Equipment;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.MapVisibility;
using OpenNefia.Content.Items;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.Inventory;
using Content.Shared.Inventory.Events;
using OpenNefia.Content.GameObjects;
using LightenedTravelsProtos = OpenNefia.LightenedTravels.Prototypes.Protos;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Core.Game;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Identify;

namespace OpenNefia.LightenedTravels.FueledLight
{
    public interface IFueledLightSystem : IEntitySystem
    {
    }

    public sealed class FueledLightSystem : EntitySystem, IFueledLightSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IEquipmentSystem _equipment = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;

        public override void Initialize()
        {
            SubscribeComponent<FueledLightComponent, EntityBeingGeneratedEvent>(FueledLight_BeingGenerated);
            SubscribeComponent<FueledLightComponent, LocalizeItemNameExtraEvent>(FueledLight_LocalizeExtra);
            SubscribeComponent<FueledLightComponent, GetVerbsEventArgs>(FueledLight_GetVerbs);

            SubscribeComponent<VisibilityComponent, EntityRefreshEvent>(HandleRefresh);
            SubscribeComponent<FueledLightComponent, GotEquippedEvent>(FueledLight_GotEquipped);
            SubscribeComponent<VisibilityComponent, EntityTurnStartingEventArgs>(HandleTurnStarting);
            SubscribeBroadcast<MapInitializeEvent>(HandleMapInitialize);

            SubscribeBroadcast<NewGameStartedEventArgs>(HandleNewGame);
        }

        private void FueledLight_BeingGenerated(EntityUid uid, FueledLightComponent fueledLight, ref EntityBeingGeneratedEvent args)
        {
            fueledLight.FuelRemaining = fueledLight.MaxFuel.Buffed;
        }

        private void HandleNewGame(NewGameStartedEventArgs ev)
        {
            if (!TryComp<InventoryComponent>(_gameSession.Player, out var inv))
                return;

            var torch = _itemGen.GenerateItem(inv.Container, LightenedTravelsProtos.Item.FueledTorch, amount: 999);
            if (IsAlive(torch) && TryComp<IdentifyComponent>(torch.Value, out var identify))
                identify.IdentifyState = IdentifyState.Full;

            _equipSlots.TryAddEquipSlot(_gameSession.Player, LightenedTravelsProtos.EquipSlot.Light, out _, out _);
        }

        private void FueledLight_GetVerbs(EntityUid uid, FueledLightComponent component, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new(UseInventoryBehavior.VerbTypeUse, "Use Fueled Light", () => UseFueledLight(args.Source, args.Target)));
        }

        private bool LightCalculationAppliesTo(IMap map)
        {
            return HasComp<MapTypeDungeonComponent>(map.MapEntityUid);
        }

        private IEnumerable<EntityUid> EnumerateLights(EntityUid chara)
        {
            foreach (var equipSlot in _equipSlots.GetEquipSlots(chara))
            {
                if (!_equipSlots.TryGetContainerForEquipSlot(chara, equipSlot, out var containerSlot))
                    continue;

                if (equipSlot.ID != LightenedTravelsProtos.EquipSlot.Light)
                    continue;

                if (!IsAlive(containerSlot.ContainedEntity))
                    continue;

                yield return containerSlot.ContainedEntity.Value;
            }
        }

        private void HandleTurnStarting(EntityUid chara, VisibilityComponent vis, EntityTurnStartingEventArgs args)
        {
            if (!TryMap(chara, out var map))
                return;

            foreach (var light in EnumerateLights(chara))
            {
                ConsumeFuel(chara, light);
            }

            RecalcLightFov(chara, map, vis);
        }

        private void FueledLight_GotEquipped(EntityUid uid, FueledLightComponent component, GotEquippedEvent args)
        {
            RecalcLightFov(args.Equipee);
        }

        private void HandleRefresh(EntityUid uid, VisibilityComponent component, ref EntityRefreshEvent args)
        {
            RecalcLightFov(uid);
        }

        private void HandleMapInitialize(MapInitializeEvent ev)
        {
            RecalcLightFov(_gameSession.Player, ev.Map);
        }

        private TurnResult UseFueledLight(EntityUid source, EntityUid target)
        {
            if (!_stacks.TrySplit(target, 1, out var split))
                return TurnResult.Aborted;

            if (!TryComp<FueledLightComponent>(split, out var fueledLight) || !TryComp<DungeonLightComponent>(split, out var dungeonLight))
                return TurnResult.Aborted;

            if (dungeonLight.IsLit)
            {
                dungeonLight.IsLit = false;
                _mes.Display(Loc.GetString("Elona.Item.Torch.PutOut", ("entity", source), ("item", split)), entity: split);
            }
            else
            {
                if (fueledLight.FuelRemaining <= 0)
                {
                    _mes.Display(Loc.GetString("LightenedTravels.FueledLight.HasNoFuel", ("wielder", source), ("item", split)));
                    return TurnResult.Aborted;
                }

                dungeonLight.IsLit = true;
                _mes.Display(Loc.GetString("Elona.Item.Torch.Light", ("entity", source), ("item", split)), entity: split);
            }

            RecalcLightFov(source);

            return TurnResult.Aborted;
        }

        private void FueledLight_LocalizeExtra(EntityUid uid, FueledLightComponent component, ref LocalizeItemNameExtraEvent args)
        {
            args.OutFullName.Append(Loc.Space + Loc.GetString("LightenedTravels.FueledLight.ItemName.FuelLeft", ("item", uid), ("fuel", component.FuelRemaining), ("maxFuel", component.MaxFuel.Buffed)));
        }

        private void ConsumeFuel(EntityUid chara, EntityUid item, FueledLightComponent? fueledLight = null, DungeonLightComponent? dungeonLight = null)
        {
            if (!Resolve(item, ref fueledLight) || !Resolve(item, ref dungeonLight))
                return;

            if (fueledLight.FuelRemaining <= 0 || !dungeonLight.IsLit)
            {
                dungeonLight.IsLit = false;
                return;
            }

            var prevPercentFuelLeft = (float)fueledLight.FuelRemaining / fueledLight.MaxFuel.Buffed;

            fueledLight.FuelRemaining = fueledLight.FuelRemaining - fueledLight.FuelConsumedPerTurn.Buffed;

            var percentFuelLeft = (float)fueledLight.FuelRemaining / fueledLight.MaxFuel.Buffed;

            fueledLight.FuelRemaining = Math.Max(fueledLight.FuelRemaining, 0);

            if (fueledLight.FuelRemaining <= 0)
            {
                if (prevPercentFuelLeft >= 0f)
                {
                    _mes.Display(Loc.GetString("LightenedTravels.FueledLight.RunsOutOfFuel", ("wielder", chara), ("item", fueledLight.Owner)), entity: chara);
                    dungeonLight.IsLit = false;
                }

            }
            else if (percentFuelLeft < 0.1f)
            {
                if (prevPercentFuelLeft >= 0.1f)
                {
                    _mes.Display(Loc.GetString("LightenedTravels.FueledLight.AboutToRunDry", ("wielder", chara), ("item", fueledLight.Owner)), entity: chara);
                }

            }
            else if (percentFuelLeft < 0.3f)
            {
                if (prevPercentFuelLeft >= 0.3f)
                {
                    _mes.Display(Loc.GetString("LightenedTravels.FueledLight.GettingDim", ("wielder", chara), ("item", fueledLight.Owner)), entity: chara);
                }
            }
        }

        private void ApplyLightFov(EntityUid chara, EntityUid item, VisibilityComponent? vis = null, FueledLightComponent? fueledLight = null, DungeonLightComponent? dungeonLight = null)
        {
            if (!Resolve(chara, ref vis) || !Resolve(item, ref fueledLight) || !Resolve(item, ref dungeonLight))
                return;

            if (fueledLight.FuelRemaining <= 0 || !dungeonLight.IsLit)
                return;

            int addFov;
            var percentFuelLeft = (float)fueledLight.FuelRemaining / fueledLight.MaxFuel.Buffed;

            if (percentFuelLeft < 0f)
            {
                addFov = 0;
            }
            else if (percentFuelLeft < 0.1f)
            {
                addFov = (int)(fueledLight.LightPower.Buffed * 0.25f);
            }
            else if (percentFuelLeft < 0.3f)
            {
                addFov = (int)(fueledLight.LightPower.Buffed * 0.5f);
            }
            else
            {
                addFov = fueledLight.LightPower.Buffed;
            }

            vis.FieldOfViewRadius.Buffed += addFov;
        }

        private void RecalcLightFov(EntityUid chara, IMap? map = null, VisibilityComponent? vis = null)
        {
            if (map == null && !TryMap(chara, out map))
                return;

            var shadowLayer = _mapRenderer.GetTileLayer<ShadowTileLayer>();
            if (LightCalculationAppliesTo(map))
                shadowLayer.ShadowStrength = 120;
            else
                shadowLayer.ShadowStrength = ShadowTileLayer.DefaultShadowStrength;

            if (!Resolve(chara, ref vis))
                return;

            if (_statusEffects.HasEffect(chara, Protos.StatusEffect.Blindness))
                return;

            vis.FieldOfViewRadius.Reset();

            if (!LightCalculationAppliesTo(map))
                return;

            vis.FieldOfViewRadius.Buffed = 3;

            foreach (var light in EnumerateLights(chara))
            {
                ApplyLightFov(chara, light, vis);
            }
        }
    }
}