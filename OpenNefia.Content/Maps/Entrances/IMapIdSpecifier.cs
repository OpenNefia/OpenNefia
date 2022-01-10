using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
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
        public MapId GetMapId();
    }

    public class PrototypeMapIdSpecifier : IMapIdSpecifier
    {
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        [DataField]
        public MapId? MapId { get; set; }

        [DataField]
        public PrototypeId<MapPrototype> MapGenerator { get; set; } = new("Blank");

        [DataField]
        public AreaId AreaId { get; set; }

        [DataField]
        public AreaFloorId FloorId { get; set; }

        public PrototypeMapIdSpecifier()
        {
        }

        public PrototypeMapIdSpecifier(PrototypeId<MapPrototype> mapGenerator)
        {
            MapGenerator = mapGenerator;
        }

        public MapId GetMapId()
        {
            EntitySystem.InjectDependencies(this);

            if (MapId == null)
            {
                var proto = _prototypeManager.Index(MapGenerator);
                MapId = _mapLoader.LoadBlueprint(proto.BlueprintPath).Id;
            }

            return MapId.Value;
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

        public MapId GetMapId() => MapId;
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

        public MapId GetMapId()
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
}
