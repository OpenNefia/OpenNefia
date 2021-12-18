using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.GameObjects
{
    public class SpatialSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<SpatialComponent, EntityPositionChangedEvent>(HandlePositionChanged, nameof(HandlePositionChanged));
            SubscribeLocalEvent<SpatialComponent, MapInitEvent>(HandleMapInit, nameof(HandleMapInit));
            SubscribeLocalEvent<SpatialComponent, EntityLivenessChangedEvent>(HandleLivenessChanged, nameof(HandleLivenessChanged));
            SubscribeLocalEvent<SpatialComponent, EntTangibilityChangedEvent>(HandleTangibilityChanged, nameof(HandleTangibilityChanged));
        }

        private void HandlePositionChanged(EntityUid uid, SpatialComponent spatial, ref EntityPositionChangedEvent args)
        {
            if (!_mapManager.TryGetMap(spatial.MapID, out var map))
                return;

            var oldMap = args.OldPosition.ToMap(_entityManager);
            var newMap = args.NewPosition.ToMap(_entityManager);

            map.RefreshTileEntities(args.OldPosition.Position, _lookup.GetLiveEntitiesAtPos(oldMap));
            map.RefreshTileEntities(args.NewPosition.Position, _lookup.GetLiveEntitiesAtPos(newMap));
        }

        private void RefreshTileOfEntity(EntityUid uid, SpatialComponent spatial)
        {
            if (!_mapManager.TryGetMap(spatial.MapID, out var map))
                return;

            var mapPos = spatial.MapPosition;

            map.RefreshTileEntities(mapPos.Position, _lookup.GetLiveEntitiesAtPos(mapPos));
        }

        private void HandleMapInit(EntityUid uid, SpatialComponent spatial, ref MapInitEvent args)
        {
            RefreshTileOfEntity(uid, spatial);
        }

        private void HandleEntityTerminating(EntityUid uid, SpatialComponent spatial, ref EntityTerminatingEvent args)
        {
            RefreshTileOfEntity(uid, spatial);
        }

        private void HandleLivenessChanged(EntityUid uid, SpatialComponent spatial, ref EntityLivenessChangedEvent args)
        {
            RefreshTileOfEntity(uid, spatial);
        }

        private void HandleTangibilityChanged(EntityUid uid, SpatialComponent spatial, ref EntTangibilityChangedEvent args)
        {
            RefreshTileOfEntity(uid, spatial);
        }
    }
}
