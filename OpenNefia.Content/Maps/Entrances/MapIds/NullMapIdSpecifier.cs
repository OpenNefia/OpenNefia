using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.Maps
{
    public class NullMapIdSpecifier : IMapIdSpecifier
    {
        public MapId? GetMapId()
        {
            Logger.WarningS("area.mapIds", "No map ID specifier provided.");
            return null;
        }
    }
}
