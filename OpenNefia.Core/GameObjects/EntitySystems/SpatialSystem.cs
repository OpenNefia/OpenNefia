using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// Handles updating the solidity/opacity of tiles on the map when an entity's
    /// state changes.
    /// </summary>
    public class SpatialSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<SpatialComponent, EntityPositionChangedEvent>(HandlePositionChanged, nameof(HandlePositionChanged));
            SubscribeLocalEvent<SpatialComponent, EntityMapInitEvent>(HandleMapInit, nameof(HandleMapInit));
            SubscribeLocalEvent<SpatialComponent, EntityLivenessChangedEvent>(HandleLivenessChanged, nameof(HandleLivenessChanged));
            SubscribeLocalEvent<SpatialComponent, EntityTerminatingEvent>(HandleEntityTerminating, nameof(HandleEntityTerminating));
            SubscribeLocalEvent<SpatialComponent, EntityTangibilityChangedEvent>(HandleTangibilityChanged, nameof(HandleTangibilityChanged));
        }

        /// <summary>
        /// Refreshes the solidity/opacity of the tiles this entity moved between.
        /// </summary>
        private void HandlePositionChanged(EntityUid uid, SpatialComponent spatial, ref EntityPositionChangedEvent args)
        {
            var (oldMap, oldMapCoords) = args.OldPosition.ToMap(_mapManager, _entityManager);
            var (newMap, newMapCoords) = args.NewPosition.ToMap(_mapManager, _entityManager);

            if (oldMap != null)
            {
                RemoveEntityIndex(uid, oldMapCoords);
                oldMap.RefreshTileEntities(oldMapCoords.Position, _lookup.GetLiveEntitiesAtCoords(oldMapCoords));
            }
            if (newMap != null)
            {
                AddEntityIndex(uid, newMapCoords, spatial);
                newMap.RefreshTileEntities(newMapCoords.Position, _lookup.GetLiveEntitiesAtCoords(newMapCoords));
            }
        }

        /// <summary>
        /// Refreshes the solidity/opacity of the tiles this entity is standing on.
        /// </summary>
        private void RefreshTileOfEntity(EntityUid uid, SpatialComponent spatial)
        {
            if (!_mapManager.TryGetMap(spatial.MapID, out var map))
                return;

            var mapPos = spatial.MapPosition;
            map.RefreshTileEntities(mapPos.Position, _lookup.GetLiveEntitiesAtCoords(mapPos));
        }

        private void AddEntityIndex(EntityUid entity, MapCoordinates coords, SpatialComponent spatial)
        {
            if (!_mapManager.TryGetMap(coords.MapId, out var map))
                return;

            // The map entity shouldn't be spatially tracked.
            if (entity == map.MapEntityUid)
                return;

            // NOTE: Only track entities directly in the map for now.
            // This does not track children in containers.
            if (spatial.ParentUid != map.MapEntityUid)
                return;

            if (!map.IsInBounds(coords.Position))
                return;

            var lookup = EntityManager.EnsureComponent<MapEntityLookupComponent>(map.MapEntityUid);
            lookup.EntitySpatial[coords.Position.X, coords.Position.Y].Add(entity);
        }

        private void RemoveEntityIndex(EntityUid entity, MapCoordinates coords, SpatialComponent? spatial = null)
        {
            if (!_mapManager.TryGetMap(coords.MapId, out var map))
                return;

            // We might be trying to delete the map/map entity itself from the entity
            // terminating event, so don't proceed if the map is dead.
            if (!EntityManager.IsAlive(map.MapEntityUid))
                return;

            // The map entity shouldn't be spatially tracked.
            if (entity == map.MapEntityUid)
                return;

            // NOTE: Only track entities directly in the map for now.
            // This does not track children in containers.
            if (spatial != null && spatial.ParentUid != map.MapEntityUid)
                return;

            if (!map.IsInBounds(coords.Position))
                return;

            var lookup = EntityManager.EnsureComponent<MapEntityLookupComponent>(map.MapEntityUid);
            lookup.EntitySpatial[coords.Position.X, coords.Position.Y].Remove(entity);
        }

        private void HandleMapInit(EntityUid uid, SpatialComponent spatial, ref EntityMapInitEvent args)
        {
            AddEntityIndex(uid, spatial.MapPosition, spatial);
            RefreshTileOfEntity(uid, spatial);
        }

        private void HandleEntityTerminating(EntityUid uid, SpatialComponent spatial, ref EntityTerminatingEvent args)
        {
            RemoveEntityIndex(uid, spatial.MapPosition, spatial);
            RefreshTileOfEntity(uid, spatial);
        }

        private void HandleLivenessChanged(EntityUid uid, SpatialComponent spatial, ref EntityLivenessChangedEvent args)
        {
            RefreshTileOfEntity(uid, spatial);
        }

        private void HandleTangibilityChanged(EntityUid uid, SpatialComponent spatial, ref EntityTangibilityChangedEvent args)
        {
            RefreshTileOfEntity(uid, spatial);
        }
    }
}
