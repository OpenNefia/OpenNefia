using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Inventory
{
    public interface IInventorySystem : IEntitySystem
    {
        IEnumerable<EntityUid> EnumerateLiveItems(EntityUid entity, InventoryComponent? inv = null);

        int GetItemWeight(EntityUid item, WeightComponent? weight = null);

        int GetTotalInventoryWeight(EntityUid ent, InventoryComponent? inv = null);
        int? GetMaxInventoryWeight(EntityUid ent, InventoryComponent? inv = null);
    }

    /// <summary>
    /// Handles character items.
    /// </summary>
    public sealed class InventorySystem : EntitySystem, IInventorySystem
    {
        public IEnumerable<EntityUid> EnumerateLiveItems(EntityUid entity, InventoryComponent? inv = null)
        {
            if (!Resolve(entity, ref inv))
                return Enumerable.Empty<EntityUid>();

            return inv.Container.ContainedEntities
                .Where(x => EntityManager.IsAlive(x));
        }

        public int GetItemWeight(EntityUid item, WeightComponent? weight = null)
        {
            if (!Resolve(item, ref weight))
                return 0;

            // TODO sum container item weights here too.

            return weight.Weight;
        }

        public int GetTotalInventoryWeight(EntityUid ent, InventoryComponent? inv = null)
        {
            if (!Resolve(ent, ref inv))
                return 0;

            return EnumerateLiveItems(ent, inv)
                .Select(item => GetItemWeight(item))
                .Sum();
        }

        public int? GetMaxInventoryWeight(EntityUid ent, InventoryComponent? inv = null)
        {
            if (!Resolve(ent, ref inv))
                return null;

            return inv.MaxWeight;
        }
    }
}
