using OpenNefia.Content.Areas;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Maps
{
    public class AreaFloorMapIdSpecifier : IMapIdSpecifier
    {
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IAreaEntranceSystem _areaEntrances = default!;

        [DataField(required: true)]
        public AreaId AreaId { get; set; }

        [DataField]
        public AreaFloorId? FloorId { get; set; }

        public AreaFloorMapIdSpecifier()
        {
        }

        public AreaFloorMapIdSpecifier(AreaId areaId, AreaFloorId? floorId = null)
        {
            AreaId = areaId;
            FloorId = floorId;
        }

        public MapId? GetMapId()
        {
            EntitySystem.InjectDependencies(this);

            var area = _areaManager.GetArea(AreaId);

            var startingFloor = _areaEntrances.GetStartingFloor(area, FloorId);

            if (startingFloor == null)
            {
                Logger.ErrorS("area.mapIds", $"Area {AreaId} has no starting floor!");
                return null;
            }

            // TODO: maybe change this for dungeons?
            // or have a DungeonMapIdSpecifier that autopopulates the given area with a new floor?
            if (!area.ContainedMaps.TryGetValue(startingFloor.Value, out var floor))
            {
                Logger.ErrorS("area.mapIds", $"Area {AreaId} is missing floor {floor}!");
                return null;
            }

            var mapId = floor.MapId;

            if (mapId == null)
            {
                mapId = _areaManager.GenerateMapForFloor(AreaId, startingFloor.Value);
            }

            return mapId;
        }
    }
}
