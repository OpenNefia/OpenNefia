using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Maps
{
    public static class MapExtensions
    {
        public static bool IsFloor(this IMap map, Vector2i pos)
        {
            return !map.GetTile(pos)?.Tile.ResolvePrototype().IsSolid ?? false;
        }

        public static PrototypeId<TilePrototype>? GetTileID(this IMap map, Vector2i pos)
        {
            return map.GetTile(pos)?.Tile.GetStrongID();
        }

        public static TilePrototype? GetTilePrototype(this IMap map, Vector2i pos)
        {
            return map.GetTile(pos)?.Tile.ResolvePrototype();
        }
    }
}
