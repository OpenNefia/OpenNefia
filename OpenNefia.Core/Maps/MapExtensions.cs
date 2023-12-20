using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        public static bool IsFloor(this IMap map, MapCoordinates coords)
        {
            if (map.Id != coords.MapId)
                return false;

            return IsFloor(map, coords.Position);
        }

        public static PrototypeId<TilePrototype>? GetTileID(this IMap map, Vector2i pos)
        {
            return map.GetTile(pos)?.Tile.GetStrongID();
        }

        public static PrototypeId<TilePrototype>? GetTileID(this IMap map, MapCoordinates coords)
        {
            if (coords.MapId != map.Id)
                return null;

            return map.GetTileID(coords.Position);
        }

        public static TilePrototype? GetTilePrototype(this IMap map, Vector2i pos)
        {
            return map.GetTile(pos)?.Tile.ResolvePrototype();
        }

        public static TilePrototype? GetTilePrototype(this IMap map, MapCoordinates pos)
        {
            if (pos.MapId != map.Id)
                return null;

            return GetTilePrototype(map, pos.Position);
        }

        public static bool TryGetTileID(this IMap map, Vector2i pos, [NotNullWhen(true)] out PrototypeId<TilePrototype>? id)
        {
            id = GetTileID(map, pos);
            return id != null;
        }

        public static bool TryGetTilePrototype(this IMap map, Vector2i pos, [NotNullWhen(true)] out TilePrototype? proto)
        {
            proto = GetTilePrototype(map, pos);
            return proto != null;
        }
    }
}
