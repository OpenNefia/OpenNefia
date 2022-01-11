using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Maps
{
    [ImplicitDataDefinitionForInheritors]
    public interface IMapIdSpecifier
    {
        public MapId? GetMapId();
    }

    public class NullMapIdSpecifier : IMapIdSpecifier
    {
        public MapId? GetMapId()
        {
            Logger.WarningS("area.mapIds", "No map ID specifier provided.");
            return null;
        }
    }

    public class BasicMapIdSpecifier : IMapIdSpecifier
    {
        [DataField]
        public MapId MapId { get; set; }

        public BasicMapIdSpecifier()
        {
        }

        public BasicMapIdSpecifier(MapId mapId)
        {
            MapId = mapId;
        }

        public MapId? GetMapId() => MapId;
    }

    public class AreaFloorMapIdSpecifier : IMapIdSpecifier
    {
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        [DataField]
        public AreaId AreaId { get; set; }

        [DataField]
        public AreaFloorId FloorId { get; set; }

        public MapId? GetMapId()
        {
            EntitySystem.InjectDependencies(this);

            var floor = _areaManager.GetArea(AreaId).ContainedMaps[FloorId];

            if (floor.MapId == null)
            {
                var proto = _prototypeManager.Index(floor.DefaultGenerator);
                floor.MapId = _mapLoader.LoadBlueprint(proto.BlueprintPath).Id;
            }

            return floor.MapId.Value;
        }
    }

    public class GlobalAreaMapIdSpecifier : IMapIdSpecifier
    {
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        [DataField(required: true)]
        public GlobalAreaId GlobalAreaId { get; set; }

        [DataField]
        public AreaFloorId? FloorId { get; set; }

        public MapId? GetMapId()
        {
            EntitySystem.InjectDependencies(this);

            var area = _areaManager.GetGlobalArea(GlobalAreaId);
            var startingFloor = FloorId.HasValue ? FloorId.Value : area.StartingFloor;

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

            if (floor.MapId == null)
            {
                var proto = _prototypeManager.Index(floor.DefaultGenerator);
                floor.MapId = _mapLoader.LoadBlueprint(proto.BlueprintPath).Id;
            }

            return floor.MapId.Value;
        }
    }
}
