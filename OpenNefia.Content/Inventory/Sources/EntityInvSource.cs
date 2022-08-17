using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Prototypes;

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

        public void OnDraw(float uiScale, float x, float y)
        {
        }   
    }
}
