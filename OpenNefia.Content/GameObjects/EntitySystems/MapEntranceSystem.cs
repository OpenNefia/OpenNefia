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
    }
}