using OpenNefia.Content.Maps;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Utility;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.GameObjects
{
    public class MapEntranceSystem : EntitySystem
    {
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;

        public TurnResult UseMapEntrance(EntityUid entranceUid, EntityUid user, MapEntranceComponent? mapEntrance = null)
        {
            if (!Resolve(entranceUid, ref mapEntrance))
                return TurnResult.Failed;

            // TODO: This will be replaced with the 'area' system at some point.
            // Instead of map prototypes, there will be "area prototypes" with more than one floor
            // where the area will generate/store a map for each floor.
            if (mapEntrance.Entrance.DestinationMapId == null)
            {
                var proto = mapEntrance.MapPrototype.ResolvePrototype();
                mapEntrance.Entrance.DestinationMapId = _mapLoader.LoadBlueprint(proto.BlueprintPath).Id;
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

            if (!TryMapLoad(entrance.DestinationMapId.Value, out var map))
                return TurnResult.Failed;

            var newPos = entrance.StartLocation.GetStartPosition(user, map)
                .BoundWithin(map.Bounds);

            spatial.Coordinates = new EntityCoordinates(map.MapEntityUid, newPos);

            return TurnResult.Succeeded;
        }

        /// <summary>
        /// Loads the map from memory or disk, in order to ensure there is a map entity for the
        /// travelling entity to be parented to.
        /// </summary>
        private bool TryMapLoad(MapId mapToLoad, [NotNullWhen(true)] out IMap? map)
        {
            Logger.InfoS("map.entrance", $"Attempting to load map {mapToLoad} from map entrance.");

            // See if this map is still in memory and hasn't been flushed yet.
            if (_mapManager.TryGetMap(mapToLoad, out map))
                return true;

            // Let's try to load the map from disk, using the current save.
            var save = _saveGameManager.CurrentSave;
            if (save == null)
            {
                Logger.ErrorS("map.entrance", $"No active save game!");
                return false;
            }

            if (!_mapLoader.TryLoadMap(mapToLoad, save, out map))
            {
                Logger.ErrorS("map.entrance", $"Failed to load map {mapToLoad} from disk!");
                return false;
            }

            return true;
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