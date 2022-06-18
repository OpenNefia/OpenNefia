using OpenNefia.Core.Maths;
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
    }
}
