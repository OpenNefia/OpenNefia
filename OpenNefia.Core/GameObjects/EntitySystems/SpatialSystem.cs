﻿using OpenNefia.Core.IoC;
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
            SubscribeLocalEvent<SpatialComponent, EntityTangibilityChangedEvent>(HandleTangibilityChanged, nameof(HandleTangibilityChanged));
        }

        private void HandlePositionChanged(EntityUid uid, SpatialComponent spatial, ref EntityPositionChangedEvent args)
        {
            var (oldMap, oldMapCoords) = args.OldPosition.ToMap(_mapManager, _entityManager);
            var (newMap, newMapCoords) = args.NewPosition.ToMap(_mapManager, _entityManager);

            if (oldMap != null)
                oldMap.RefreshTileEntities(oldMapCoords.Position, _lookup.GetLiveEntitiesAtPos(oldMapCoords));
            if (newMap != null)
                newMap.RefreshTileEntities(newMapCoords.Position, _lookup.GetLiveEntitiesAtPos(newMapCoords));
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

        private void HandleTangibilityChanged(EntityUid uid, SpatialComponent spatial, ref EntityTangibilityChangedEvent args)
        {
            RefreshTileOfEntity(uid, spatial);
        }
    }
}
