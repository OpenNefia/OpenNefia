using Love;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Feats;
using OpenNefia.Content.Hud;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Mount;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.Weight;
using OpenNefia.Core.Containers;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using System.Diagnostics.CodeAnalysis;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Equipment;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Content.Inventory
{
    public interface IInventorySystem : IEntitySystem
    {
        IEnumerable<EntityUid> EnumerateInventory(EntityUid entity, InventoryComponent? inv = null);
        IEnumerable<EntityUid> EnumerateInventoryAndEquipment(EntityUid entity, InventoryComponent? inv = null, EquipSlotsComponent? equipSlots = null);

        IEnumerable<TComp> EntityQueryInInventory<TComp>(EntityUid entity, bool includeDead = false, InventoryComponent? inv = null)
            where TComp : IComponent;
        IEnumerable<(TComp1, TComp2)> EntityQueryInInventory<TComp1, TComp2>(EntityUid entity, bool includeDead = false, InventoryComponent? inv = null)
            where TComp1 : IComponent
            where TComp2 : IComponent;
        bool IsInventoryFull(EntityUid ent, InventoryComponent? inv = null);
        IEnumerable<(TComp1, TComp2, TComp3)> EntityQueryInInventory<TComp1, TComp2, TComp3>(EntityUid entity, bool includeDead = false, InventoryComponent? inv = null)
            where TComp1 : IComponent
            where TComp2 : IComponent
            where TComp3 : IComponent;
        IEnumerable<(TComp1, TComp2, TComp3, TComp4)> EntityQueryInInventory<TComp1, TComp2, TComp3, TComp4>(EntityUid entity, bool includeDead = false, InventoryComponent? inv = null)
            where TComp1 : IComponent
            where TComp2 : IComponent
            where TComp3 : IComponent
            where TComp4 : IComponent;

        /// <remarks>
        /// NOTE: Prefer <see cref="EntityQueryInInventory"/>; this function should
        /// be used when dealing with things like quests that require a specific
        /// item ID. Matching on components in an ECS manner is more flexible.
        /// </remarks>
        bool TryFindItemWithIDInInventory(EntityUid entity, PrototypeId<EntityPrototype> id, [NotNullWhen(true)] out EntityUid? item, InventoryComponent? inv = null);

        int GetTotalInventoryWeight(EntityUid ent, InventoryComponent? inv = null);
        int? GetMaxInventoryWeight(EntityUid ent, InventoryComponent? inv = null);
        bool TryGetInventoryContainer(EntityUid ent, [NotNullWhen(true)] out IContainer? inv, InventoryComponent? invComp = null);

        int CalcMaxInventoryWeight(EntityUid uid);
        void RefreshInventoryWeight(EntityUid ent, bool refreshSpeed = true, InventoryComponent? inv = null);

        /// <hsp>*chara_adjustInv</hsp>
        void EnsureFreeItemSlot(EntityUid ent, InventoryComponent? inv = null);
    }

    /// <summary>
    /// Handles character items.
    /// </summary>
    public sealed class InventorySystem : EntitySystem, IInventorySystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IMountSystem _mounts = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly ITurnOrderSystem _turnOrder = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly IWeightSystem _weights = default!;

        public override void Initialize()
        {
            SubscribeComponent<InventoryComponent, GetStatusIndicatorsEvent>(AddStatusIndicator);
            SubscribeComponent<InventoryComponent, BeforeMoveEventArgs>(ProcMovementPreventionOnBurden);
            SubscribeComponent<InventoryComponent, EntityRefreshSpeedEvent>(HandleRefreshSpeed, priority: EventPriorities.VeryHigh);
            SubscribeComponent<InventoryComponent, EntityTurnEndingEventArgs>(HandleTurnEnding);
            SubscribeComponent<InventoryComponent, EntityRefreshEvent>(HandleRefresh, priority: EventPriorities.VeryHigh);
            SubscribeComponent<InventoryComponent, ContainerIsInsertingAttemptEvent>(HandleAboutToInsert, priority: EventPriorities.VeryHigh);
            SubscribeComponent<InventoryComponent, EntInsertedIntoContainerEventArgs>(HandleInserted, priority: EventPriorities.VeryHigh);
            SubscribeComponent<InventoryComponent, EntRemovedFromContainerEventArgs>(HandleRemoved, priority: EventPriorities.VeryHigh);
        }

        private void AddStatusIndicator(EntityUid uid, InventoryComponent inv, GetStatusIndicatorsEvent args)
        {
            if (inv.BurdenType > BurdenType.None)
            {
                var color = new Color(0 / 255f, Math.Min((int)inv.BurdenType * 40, 255) / 255f, Math.Min((int)inv.BurdenType * 40, 255) / 255f, 255 / 255f);
                args.OutIndicators.Add(new()
                {
                    Text = Loc.GetString($"Elona.Inventory.Burden.Indicator.{inv.BurdenType}"),
                    Color = color
                });
            }
        }

        private void HandleRefreshSpeed(EntityUid uid, InventoryComponent component, ref EntityRefreshSpeedEvent args)
        {
            if (!_gameSession.IsPlayer(uid))
                return;

            if (component.BurdenType >= BurdenType.Heavy)
                args.OutSpeedModifier -= 0.5f;
            if (component.BurdenType >= BurdenType.Moderate)
                args.OutSpeedModifier -= 0.3f;
            if (component.BurdenType >= BurdenType.Light)
                args.OutSpeedModifier -= 0.1f;
        }

        private void HandleTurnEnding(EntityUid uid, InventoryComponent inv, EntityTurnEndingEventArgs args)
        {
            if (_gameSession.IsPlayer(uid))
            {
                if (inv.BurdenType >= BurdenType.Heavy)
                {
                    if (_rand.OneIn(20) && TryComp<SkillsComponent>(uid, out var skills) && inv.MaxWeight != null)
                    {
                        var totalWeight = GetTotalInventoryWeight(uid);
                        _mes.Display(Loc.GetString("Elona.Inventory.Burden.BackpackSquashing", ("entity", uid)));
                        var damage = skills.MaxHP * (totalWeight * 10 / inv.MaxWeight.Value + 10) / 200 + 1;
                        _damage.DamageHP(uid, damage, damageType: new GenericDamageType("Elona.DamageType.Burden"));
                    }
                }
            }
        }

        private void ProcMovementPreventionOnBurden(EntityUid uid, InventoryComponent inv, BeforeMoveEventArgs args)
        {
            if (args.Handled)
                return;

            if (inv.BurdenType >= BurdenType.Max)
            {
                if (_gameSession.IsPlayer(uid))
                    _mes.Display(Loc.GetString("Elona.Inventory.Burden.CarryTooMuch"), combineDuplicates: true);
                args.Handle(TurnResult.Failed);
                return;
            }
        }

        private void HandleRefresh(EntityUid uid, InventoryComponent component, ref EntityRefreshEvent args)
        {
            // Speed is refreshed by TurnOrderSystem later, no need to calculate it twice.
            RefreshInventoryWeight(uid, refreshSpeed: false, component);
        }

        private void HandleAboutToInsert(EntityUid uid, InventoryComponent component, ContainerIsInsertingAttemptEvent args)
        {
            if (args.Cancelled)
                return;

            if (IsInventoryFull(uid, component))
            {
                args.Cancel();
                return;
            }    
        }

        private void HandleInserted(EntityUid uid, InventoryComponent component, EntInsertedIntoContainerEventArgs args)
        {
            RefreshInventoryWeight(uid, refreshSpeed: true, component);
        }

        private void HandleRemoved(EntityUid uid, InventoryComponent component, EntRemovedFromContainerEventArgs args)
        {
            RefreshInventoryWeight(uid, refreshSpeed: true, component);
        }

        public int CalcMaxInventoryWeight(EntityUid uid)
        {
            return _skills.Level(uid, Protos.Skill.AttrStrength) * 500
                + _skills.Level(uid, Protos.Skill.AttrConstitution) * 250
                + _skills.Level(uid, Protos.Skill.WeightLifting) * 2000
                + 45000;
        }

        public void RefreshInventoryWeight(EntityUid uid, bool refreshSpeed = true, InventoryComponent? inv = null)
        {
            if (!Resolve(uid, ref inv))
                return;

            inv.BurdenType = BurdenType.None;
            inv.MaxWeight = CalcMaxInventoryWeight(uid);

            var weight = GetTotalInventoryWeight(uid);

            if (weight > inv.MaxWeight.Value * 2)
                inv.BurdenType = BurdenType.Max;
            else if (weight > inv.MaxWeight.Value)
                inv.BurdenType = BurdenType.Heavy;
            else if (weight > inv.MaxWeight.Value / 4 * 3)
                inv.BurdenType = BurdenType.Moderate;
            else if (weight > inv.MaxWeight.Value / 2)
                inv.BurdenType = BurdenType.Light;
            else
                inv.BurdenType = BurdenType.None;

            if (refreshSpeed)
                _turnOrder.RefreshSpeed(uid);
        }

        public IEnumerable<EntityUid> EnumerateInventory(EntityUid entity, InventoryComponent? inv = null)
        {
            if (!Resolve(entity, ref inv))
                return Enumerable.Empty<EntityUid>();

            return inv.Container.ContainedEntities
                .Where(x => EntityManager.IsAlive(x));
        }

        public IEnumerable<EntityUid> EnumerateInventoryAndEquipment(EntityUid entity, InventoryComponent? inv = null, EquipSlotsComponent? equipSlots = null)
        {
            return EnumerateInventory(entity, inv).Concat(_equipSlots.EnumerateEquippedEntities(entity, equipSlots));
        }

        public int GetTotalInventoryWeight(EntityUid ent, InventoryComponent? inv = null)
        {
            if (!Resolve(ent, ref inv))
                return 0;

            var baseWeight = EnumerateInventory(ent, inv)
                .Concat(_equipSlots.EnumerateEquippedEntities(ent))
                .Select(item => _weights.GetTotalWeight(item))
                .Sum();

            var modifiedWeight = (int)(baseWeight * (1f - _feats.Level(ent, Protos.Feat.EtherGravity) * 0.1f
                                                        + _feats.Level(ent, Protos.Feat.EtherFeather) * 0.2f));

            return modifiedWeight;
        }

        public int? GetMaxInventoryWeight(EntityUid ent, InventoryComponent? inv = null)
        {
            if (!Resolve(ent, ref inv))
                return null;

            return inv.MaxWeight;
        }

        public bool TryGetInventoryContainer(EntityUid ent, [NotNullWhen(true)] out IContainer? inv, InventoryComponent? invComp = null)
        {
            if (!Resolve(ent, ref invComp))
            {
                inv = null;
                return false;
            }

            inv = invComp.Container;
            return true;
        }

        public bool IsInventoryFull(EntityUid ent, InventoryComponent? invComp = null)
        {
            if (!Resolve(ent, ref invComp))
                return true;

            if (invComp.MaxItemCount == null)
                return false;

            return invComp.Container.ContainedEntities.Count >= invComp.MaxItemCount;
        }

        public IEnumerable<TComp> EntityQueryInInventory<TComp>(EntityUid entity, bool includeDead = false, InventoryComponent? inv = null)
            where TComp : IComponent
        {
            if (!Resolve(entity, ref inv))
                yield break;

            foreach (var ent in EntityManager.EntityQuery<TComp>())
            {
                if (inv.Container.Contains(ent.Owner)
                    && (includeDead || IsAlive(ent.Owner)))
                {
                    yield return ent;
                }
            }
        }

        public IEnumerable<(TComp1, TComp2)> EntityQueryInInventory<TComp1, TComp2>(EntityUid entity, bool includeDead = false, InventoryComponent? inv = null)
            where TComp1 : IComponent
            where TComp2 : IComponent
        {
            if (!Resolve(entity, ref inv))
                yield break;

            foreach (var ent in EntityManager.EntityQuery<TComp1, TComp2>())
            {
                if (inv.Container.Contains(ent.Item1.Owner)
                    && (includeDead || IsAlive(ent.Item1.Owner)))
                {
                    yield return ent;
                }
            }
        }

        public IEnumerable<(TComp1, TComp2, TComp3)> EntityQueryInInventory<TComp1, TComp2, TComp3>(EntityUid entity, bool includeDead = false, InventoryComponent? inv = null)
            where TComp1 : IComponent
            where TComp2 : IComponent
            where TComp3 : IComponent
        {
            if (!Resolve(entity, ref inv))
                yield break;

            foreach (var ent in EntityManager.EntityQuery<TComp1, TComp2, TComp3>())
            {
                if (inv.Container.Contains(ent.Item1.Owner)
                    && (includeDead || IsAlive(ent.Item1.Owner)))
                {
                    yield return ent;
                }
            }
        }

        public IEnumerable<(TComp1, TComp2, TComp3, TComp4)> EntityQueryInInventory<TComp1, TComp2, TComp3, TComp4>(EntityUid entity, bool includeDead = false, InventoryComponent? inv = null)
            where TComp1 : IComponent
            where TComp2 : IComponent
            where TComp3 : IComponent
            where TComp4 : IComponent
        {
            if (!Resolve(entity, ref inv))
                yield break;

            foreach (var ent in EntityManager.EntityQuery<TComp1, TComp2, TComp3, TComp4>())
            {
                if (inv.Container.Contains(ent.Item1.Owner)
                    && (includeDead || IsAlive(ent.Item1.Owner)))
                {
                    yield return ent;
                }
            }
        }

        public bool TryFindItemWithIDInInventory(EntityUid entity, PrototypeId<EntityPrototype> id, [NotNullWhen(true)] out EntityUid? item, InventoryComponent? inv = null)
        {
            return EnumerateInventory(entity, inv)
                .TryFirstOrNull(item => MetaData(item).EntityPrototype?.GetStrongID() == id, out item);
        }

        public void EnsureFreeItemSlot(EntityUid ent, InventoryComponent? inv = null)
        {
            if (!Resolve(ent, ref inv) || !IsInventoryFull(ent, inv))
                return;

            var items = EnumerateInventory(ent).ToList();
            if (items.Count == 0)
                return;

            var item = _rand.Pick(items);
            EntityManager.DeleteEntity(item);
        }
    }
}