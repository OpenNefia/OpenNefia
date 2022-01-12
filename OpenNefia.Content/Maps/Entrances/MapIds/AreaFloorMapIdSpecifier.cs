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

        public AreaId? GetAreaId() => AreaId;

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

            var mapId = _areaManager.GetOrGenerateMapForFloor(area.Id, startingFloor.Value);
            if (mapId == null)
            {
                Logger.ErrorS("area.mapIds", $"Area {AreaId} is missing floor {startingFloor.Value}!");
                return null;
            }

            return mapId;
        }
    }
}
