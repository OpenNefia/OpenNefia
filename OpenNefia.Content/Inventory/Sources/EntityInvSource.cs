using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Inventory
{
    public class EntityInvSource : IInventorySource
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;

        public EntityUid Entity { get; }

        public EntityInvSource(EntityUid entity)
        {
            Entity = entity;
        }

        public IEnumerable<EntityUid> GetEntities()
        {
            if (!_entityManager.TryGetComponent(Entity, out InventoryComponent? inv))
                return Enumerable.Empty<EntityUid>();

            return inv.Container.ContainedEntities;
        }

        public void OnDraw()
        {
        }
    }
}
