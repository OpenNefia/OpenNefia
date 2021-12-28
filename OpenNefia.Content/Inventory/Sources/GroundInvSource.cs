using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.Inventory
{
    public class GroundInvSource : IInventorySource
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public EntityUid Entity { get; }

        public GroundInvSource(EntityUid entity)
        {
            Entity = entity;
        }

        public IEnumerable<EntityUid> GetEntities()
        {
            if (!_entityManager.TryGetComponent(Entity, out SpatialComponent? spatial))
                return Enumerable.Empty<EntityUid>();

            return _lookup.GetLiveEntitiesAtCoords(spatial.MapPosition)
                .Select(ent => ent.Uid);
        }

        public void OnDraw()
        {
        }
    }
}