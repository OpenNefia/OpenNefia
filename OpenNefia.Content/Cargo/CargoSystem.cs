using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Items;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Maps;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.Logic;

namespace OpenNefia.Content.Cargo
{
    public interface ICargoSystem : IEntitySystem
    {
        int GetTotalCargoWeight(EntityUid ent, InventoryComponent? inv = null);
        int? GetMaxCargoWeight(EntityUid ent, CargoHolderComponent? cargoHolder = null);
        bool IsBurdenedByCargo(EntityUid ent, CargoHolderComponent? cargoHolder = null, InventoryComponent? inv = null);
        bool CanUseCargoItemsIn(IMap map);
    }

    public class CargoSystem : EntitySystem, ICargoSystem
    {
        [Dependency] private readonly IInventorySystem _invSys = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
            SubscribeComponent<CargoHolderComponent, EntityBeingGeneratedEvent>(InitializeCargoWeights);
            SubscribeComponent<CargoHolderComponent, EntityRefreshSpeedEvent>(HandleRefreshSpeed);
            SubscribeComponent<CargoComponent, LocalizeItemNameExtraEvent>(LocalizeExtra_Cargo);
            SubscribeComponent<CargoComponent, AfterMapEnterEventArgs>(ShowCargoBurdenMessage);
        }

        private void InitializeCargoWeights(EntityUid uid, CargoHolderComponent cargo, ref EntityBeingGeneratedEvent args)
        {
            if (!_gameSession.IsPlayer(uid))
                return;

            // >>>>>>>> shade2/chara.hsp:532 	if rc=pc{ ..
            cargo.InitialMaxCargoWeight = 80000;
            cargo.MaxCargoWeight = cargo.InitialMaxCargoWeight;
            // <<<<<<<< shade2/chara.hsp:534 		} ..
        }

        private bool CargoSpeedPenaltyAppliesTo(IMap map)
        {
            return HasComp<MapTypeWorldMapComponent>(map.MapEntityUid) 
                || HasComp<MapTypeFieldComponent>(map.MapEntityUid);
        }

        private void HandleRefreshSpeed(EntityUid uid, CargoHolderComponent cargoHolder, ref EntityRefreshSpeedEvent args)
        {
            if (!_gameSession.IsPlayer(uid) || !TryMap(uid, out var map))
                return;

            if (CargoSpeedPenaltyAppliesTo(map))
            {
                var cargoWeight = GetTotalCargoWeight(uid);
                if (cargoHolder.MaxCargoWeight != null && cargoWeight > cargoHolder.MaxCargoWeight)
                {
                    args.OutSpeedModifier -= 0.25f + 0.25f * (cargoWeight / 100f) / ((cargoHolder.MaxCargoWeight.Value / 100f) + 1f);
                }
            }
        }

        private void LocalizeExtra_Cargo(EntityUid uid, CargoComponent cargo, ref LocalizeItemNameExtraEvent args)
        {
            if (cargo.BuyingPrice != null)
            {
                args.OutFullName.Append(Loc.GetString("Elona.Cargo.ItemName.BuyingPrice", ("price", cargo.BuyingPrice.Value)));
            }
        }

        private void ShowCargoBurdenMessage(EntityUid uid, CargoComponent component, AfterMapEnterEventArgs args)
        {
            // >>>>>>>> elona122/shade2/map.hsp:218 		if gCargoWeight>gCargoLimit:if (areaType(gArea)= ...
            if (CargoSpeedPenaltyAppliesTo(args.NewMap))
            {
                if (GetTotalCargoWeight(_gameSession.Player) > GetMaxCargoWeight(_gameSession.Player))
                {
                    _mes.Display(Loc.GetString("Elona.Cargo.Burdened"));
                }
            }
            // <<<<<<<< elona122/shade2/map.hsp:218 		if gCargoWeight>gCargoLimit:if (areaType(gArea)= ...
        }

        public int GetCargoItemWeight(EntityUid item, CargoComponent? cargo = null)
        {
            if (!Resolve(item, ref cargo, logMissing: false))
                return 0;

            // TODO sum container item weights here too.

            return cargo.Weight;
        }

        public int GetTotalCargoWeight(EntityUid ent, InventoryComponent? inv = null)
        {
            if (!Resolve(ent, ref inv))
                return 0;

            return _invSys.EnumerateInventory(ent, inv)
                .Select(item => GetCargoItemWeight(item))
                .Sum();
        }

        public int? GetMaxCargoWeight(EntityUid ent, CargoHolderComponent? cargoHolder = null)
        {
            if (!Resolve(ent, ref cargoHolder))
                return 0;

            return cargoHolder.MaxCargoWeight;
        }

        public bool IsBurdenedByCargo(EntityUid ent, CargoHolderComponent? cargoHolder = null, InventoryComponent? inv = null)
        {
            if (!Resolve(ent, ref cargoHolder) || !Resolve(ent, ref inv))
                return false;

            return GetTotalCargoWeight(ent, inv) > GetMaxCargoWeight(ent, cargoHolder);
        }

        public bool CanUseCargoItemsIn(IMap map)
        {
            var uid = map.MapEntityUid;
            return HasComp<MapTypeWorldMapComponent>(uid)
                || HasComp<MapTypePlayerOwnedComponent>(uid)
                || HasComp<MapTypeTownComponent>(uid)
                || HasComp<MapTypeShelterComponent>(uid)
                || HasComp<MapTypeFieldComponent>(uid)
                || HasComp<MapTypeGuildComponent>(uid);
        }
    }
}
