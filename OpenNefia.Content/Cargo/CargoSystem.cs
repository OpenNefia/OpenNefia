using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Cargo
{
    public interface ICargoSystem : IEntitySystem
    {
        int GetTotalCargoWeight(EntityUid ent, InventoryComponent? inv = null);
        int? GetMaxCargoWeight(EntityUid ent, CargoHolderComponent? cargoHolder = null);
        bool IsBurdenedByCargo(EntityUid ent, CargoHolderComponent? cargoHolder = null, InventoryComponent? inv = null);
    }

    public class CargoSystem : EntitySystem, ICargoSystem
    {
        [Dependency] private readonly IInventorySystem _invSys = default!;

        public int GetCargoWeight(EntityUid item, CargoComponent? cargo = null)
        {
            if (!Resolve(item, ref cargo, logMissing: false))
                return 0;

            // TODO sum container item weights here too.

            return cargo.CargoWeight;
        }

        public int GetTotalCargoWeight(EntityUid ent, InventoryComponent? inv = null)
        {
            if (!Resolve(ent, ref inv))
                return 0;

            return _invSys.EnumerateLiveItems(ent, inv)
                .Select(item => GetCargoWeight(item))
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
    }
}
