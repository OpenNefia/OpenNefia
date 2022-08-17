using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;

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
                .Select(spatial => spatial.Owner);
        }

        public void ModifyEntityName(ref string name)
        {
            name += Loc.GetString("Elona.Inventory.Common.NameModifiers.Ground");
        }

        public void OnDraw(float uiScale, float x, float y)
        {
        }
    }
}