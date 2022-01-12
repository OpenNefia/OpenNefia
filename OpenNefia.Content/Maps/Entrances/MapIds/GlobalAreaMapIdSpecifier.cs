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
    /// <summary>
    /// Points to an area floor that's defined in a global area.
    /// </summary>
    /// <remarks>
    /// This should only be used for prototype data. After the areas are generated,
    /// it should function identically to <see cref="AreaFloorMapIdSpecifier"/>.
    /// </remarks>
    public class GlobalAreaMapIdSpecifier : IMapIdSpecifier
    {
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IAreaEntranceSystem _areaEntrances = default!;

        [DataField(required: true)]
        public GlobalAreaId GlobalAreaId { get; set; }

        [DataField]
        public AreaFloorId? FloorId { get; set; }

        /// <summary>
        /// Actual area ID that the specifid global area uses. This field will be set 
        /// once this entrance has been used for the first time.
        /// </summary>
        [DataField]
        public AreaId? ResolvedAreaId { get; set; }

        public GlobalAreaMapIdSpecifier()
        {
        }

        public GlobalAreaMapIdSpecifier(GlobalAreaId globalAreaId, AreaFloorId? floorId = null)
        {
            GlobalAreaId = globalAreaId;
            FloorId = floorId;
        }

        public MapId? GetMapId()
        {
            EntitySystem.InjectDependencies(this);

            if (ResolvedAreaId == null)
            {
                if (!_areaManager.TryGetGlobalArea(GlobalAreaId, out var globalArea))
                {
                    Logger.ErrorS("area.mapIds", $"Global area {GlobalAreaId} does not exist!");
                    return null;
                }
                ResolvedAreaId = globalArea.Id;
            }

            var area = _areaManager.GetArea(ResolvedAreaId.Value);
            var startingFloor = _areaEntrances.GetStartingFloor(area, FloorId);

            if (startingFloor == null)
            {
                Logger.ErrorS("area.mapIds", $"Area {GlobalAreaId} has no starting floor!");
                return null;
            }

            // TODO: maybe change this for dungeons?
            // or have a DungeonMapIdSpecifier that autopopulates the given area with a new floor?
            if (!area.ContainedMaps.TryGetValue(startingFloor.Value, out var floor))
            {
                Logger.ErrorS("area.mapIds", $"Area {GlobalAreaId} is missing floor {floor}!");
                return null;
            }

            var mapId = floor.MapId;

            if (floor.MapId == null)
            {
                mapId = _areaManager.GenerateMapForFloor(area.Id, startingFloor.Value);
            }

            return mapId;
        }
    }
}
