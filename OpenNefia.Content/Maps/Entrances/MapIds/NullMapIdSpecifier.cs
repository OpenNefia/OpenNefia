using OpenNefia.Core.Areas;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.Maps
{
    public class NullMapIdSpecifier : IMapIdSpecifier
    {
        public AreaId? GetOrGenerateAreaId() => null;

        public MapId? GetOrGenerateMapId()
        {
            Logger.WarningS("area.mapIds", "No map ID specifier provided.");
            return null;
        }

        public MapId? GetMapId()
        {
            Logger.WarningS("area.mapIds", "No map ID specifier provided.");
            return null;
        }
    }
}
