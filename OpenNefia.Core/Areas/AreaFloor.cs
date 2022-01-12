using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Areas
{
    [DataDefinition]
    public sealed class AreaFloor
    {
        [DataField]
        public MapId? MapId { get; internal set; }

        [DataField]
        public PrototypeId<MapPrototype> DefaultGenerator { get; set; } = new("Blank");

        public AreaFloor()
        {
        }

        public AreaFloor(MapId mapId)
        {
            MapId = mapId;
        }

        public AreaFloor(PrototypeId<MapPrototype> defaultGenerator)
        {
            DefaultGenerator = defaultGenerator;
        }
    }
}