using OpenNefia.Content.Inventory;
using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    public class InventorySystem : EntitySystem
    {
        public override void Initialize()
        {
            SubscribeLocalEvent<InventoryComponent, EntityClonedEventArgs>(HandleCloned, nameof(HandleCloned));
        }

        private void HandleCloned(EntityUid source, InventoryComponent sourceInv, EntityClonedEventArgs args)
        {
            // TODO containers
        }

        public IEnumerable<EntityUid> EnumerateItems(EntityUid entity, InventoryComponent? inv = null)
        {
            if (!Resolve(entity, ref inv))
                return Enumerable.Empty<EntityUid>();

            return inv.Container.ContainedEntities
                .Where(x => EntityManager.IsAlive(x));
        }
    }
}
