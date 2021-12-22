using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Content.GameObjects
{
    public class MapEntranceSystem : EntitySystem
    {
        [Dependency] private readonly IMapBlueprintLoader _mapBlueprints = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;

        public TurnResult UseMapEntrance(EntityUid entranceUid, EntityUid user, MapEntranceComponent? mapEntrance = null)
        {
            if (!Resolve(entranceUid, ref mapEntrance))
                return TurnResult.Failed;

            if (mapEntrance.Entrance.DestinationMapId == null)
            {
                var proto = mapEntrance.MapPrototype.ResolvePrototype();
                mapEntrance.Entrance.DestinationMapId = _mapBlueprints.LoadBlueprint(null, proto.BlueprintPath).Id;
            }

            return UseMapEntrance(user, mapEntrance.Entrance);
        }

        public TurnResult UseMapEntrance(EntityUid user, MapEntrance entrance,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(user, ref spatial))
                return TurnResult.Failed;

            if (entrance.DestinationMapId == null)
            {
                Logger.WarningS("map.entrance", "Entrance is missing destination map ID");
                return TurnResult.Failed;
            }

            if (!_mapManager.TryGetMap(entrance.DestinationMapId.Value, out var map))
                return TurnResult.Failed;

            var newPos = entrance.StartLocation.GetStartPosition(user, map)
                .BoundWithin(map.Bounds);

            spatial.Coordinates = new EntityCoordinates(map.MapEntityUid, newPos);

            return TurnResult.Succeeded;
        }

        /// <summary>
        /// Set the map to travel to when exiting the destination map via the edges.
        /// </summary>
        /// <param name="mapTravellingTo">Map that is being travelled to.</param>
        /// <param name="prevCoords">Location that exiting the given map from the edges should lead to.</param>
        public void SetPreviousMap(MapId mapTravellingTo, MapCoordinates prevCoords)
        {
            if (!_mapManager.TryGetMapEntity(mapTravellingTo, out var mapEntity))
            {
                return;
            }

            var mapMapEntrance = EntityManager.EnsureComponent<MapEntranceComponent>(mapEntity.Uid);
            mapMapEntrance.Entrance.DestinationMapId = prevCoords.MapId;
            mapMapEntrance.Entrance.StartLocation = new SpecificMapLocation(prevCoords.Position);
        }
    }
}