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
        [Dependency] private readonly IGlobalAreaSystem _globalAreas = default!;

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

        public AreaId? GetOrGenerateAreaId()
        {
            EntitySystem.InjectDependencies(this);

            if (ResolvedAreaId == null)
            {
                var globalArea = _globalAreas.GetOrCreateGlobalArea(GlobalAreaId);
                ResolvedAreaId = globalArea.Id;
            }

            return ResolvedAreaId;
        }

        public MapId? GetOrGenerateMapId()
        {
            EntitySystem.InjectDependencies(this);

            var area = _areaManager.GetArea(GetOrGenerateAreaId()!.Value);
            var startingFloor = _areaEntrances.GetStartingFloor(area, FloorId);

            if (startingFloor == null)
            {
                Logger.ErrorS("area.mapIds", $"Area {GlobalAreaId} has no starting floor!");
                return null;
            }

            var map = _areaManager.GetOrGenerateMapForFloor(area.Id, startingFloor);
            if (map == null)
            {
                Logger.ErrorS("area.mapIds", $"Area {area.Id} is missing floor {startingFloor}!");
                return null;
            }

            return map.Id;
        }

        public MapId? GetMapId()
        {
            EntitySystem.InjectDependencies(this);

            var area = _areaManager.GetArea(GetOrGenerateAreaId()!.Value);
            var startingFloor = _areaEntrances.GetStartingFloor(area, FloorId);

            if (startingFloor == null)
            {
                Logger.ErrorS("area.mapIds", $"Area {GlobalAreaId} has no starting floor!");
                return null;
            }

            var map = _areaManager.GetMapForFloor(area.Id, startingFloor);
            if (map == null)
                return null;

            return map.Id;
        }
    }
}
