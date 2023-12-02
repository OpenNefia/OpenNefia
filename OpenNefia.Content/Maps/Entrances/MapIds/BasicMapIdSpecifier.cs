using OpenNefia.Core.Areas;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Maps
{
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

        public AreaId? GetOrGenerateAreaId() => null;

        public MapId? GetOrGenerateMapId() => MapId;
        public MapId? GetMapId() => MapId;
    }
}
