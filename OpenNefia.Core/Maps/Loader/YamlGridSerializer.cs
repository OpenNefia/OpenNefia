using Love;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using System.Collections;
using System.IO;
using System.Text;

namespace OpenNefia.Core.Maps
{
    public static class YamlGridSerializer
    {
        public static Tile[,] DeserializeGrid(string gridString,
            Dictionary<string, PrototypeId<TilePrototype>> tileMap,
            ITileDefinitionManager tileDefs,
            out Vector2i size)
        {
            var lines = gridString.Split("\n").ToArray();
            if (lines.Length == 0)
            {
                throw new InvalidDataException("Grid string was blank.");
            }
            var width = lines[0].Length;
            var height = lines.Length;

            size = new Vector2i(width, height);

            var tiles = new Tile[width, height];

            var x = 0;
            var y = 0;

            foreach (var line in gridString.Split("\n"))
            {
                if (line.Length != width)
                {
                    throw new InvalidDataException($"Line at Y position {y} has incorrect width {line.Length}, expected {width}");
                }

                foreach (var rune in line.EnumerateRunes())
                {
                    if (!tileMap.TryGetValue(rune.ToString(), out var tileId))
                    {
                        throw new InvalidDataException($"Tile at ({x},{y}) is missing from tilemap: {rune}");
                    }

                    tiles[x, y] = new Tile(tileDefs[tileId]);

                    x++;
                }

                x = 0;
                y++;
            }

            return tiles;
        }

        private static Rune[] TileRunes =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!#$%&\'()*+,-./0123456789:;=?@^_~ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïð"
            .EnumerateRunes()
            .ToArray();

        public static Dictionary<PrototypeId<TilePrototype>, string> BuildProtoToRuneTileMap(IMap map, ITileDefinitionManager tileDefs)
        {
            var tilesSeen = new HashSet<PrototypeId<TilePrototype>>();

            foreach (var tile in map.Tiles)
            {
                tilesSeen.Add(tileDefs[tile.Type].GetStrongID());
            }
            foreach (var tileMemory in map.TileMemory)
            {
                tilesSeen.Add(tileDefs[tileMemory.Type].GetStrongID());
            }

            var result = new Dictionary<PrototypeId<TilePrototype>, string>();
            int index = 0; // TODO beautify based on wall/floor

            foreach (var tileId in tilesSeen)
            {
                if (index > TileRunes.Length)
                {
                    throw new InvalidOperationException("Maximum tilemap tile count exceeded.");
                }

                var rune = TileRunes[index];
                result.Add(tileId, rune.ToString());
                index++;
            }

            return result;
        }

        public static string SerializeGrid(Tile[,] tiles, Vector2i size,
            Dictionary<PrototypeId<TilePrototype>, string> protoToRune,
            ITileDefinitionManager tileDefinitionManager)
        {
            var sb = new StringBuilder();

            for (int y = 0; y < size.Y; y++)
            {
                for (int x = 0; x < size.X; x++)
                {
                    var tile = tiles[x, y];
                    var protoId = tileDefinitionManager[tile.Type].GetStrongID();
                    if (!protoToRune.TryGetValue(protoId, out var rune))
                    {
                        throw new InvalidDataException($"Tile ID '{protoId}' has no tilemap rune.");
                    }
                    sb.Append(rune[0]);
                }
                sb.Append("\n");
            }

            return sb.ToString();
        }

        public static string SerializeInSight(uint[,] inSight, Vector2i size)
        {
            var nTiles = inSight.Length * sizeof(uint);
            var barr = new byte[nTiles];

            using (var stream = new MemoryStream(barr))
            using (var writer = new BinaryWriter(stream))
            {
                for (int y = 0; y < size.Y; y++)
                {
                    for (var x = 0; x < size.X; x++)
                    {
                        writer.Write(inSight[x, y]);
                    }
                }
            }

            return Convert.ToBase64String(barr);
        }

        public static uint[,] DeserializeInSight(string base64, Vector2i size)
        {
            var inSightBytes = Convert.FromBase64String(base64);
            var inSight = new uint[size.X, size.Y];

            using var stream = new MemoryStream(inSightBytes);
            using var reader = new BinaryReader(stream);

            for (ushort y = 0; y < size.Y; y++)
            {
                for (ushort x = 0; x < size.X; x++)
                {
                    inSight[x, y] = reader.ReadUInt32();
                }
            }

            return inSight;
        }
    }
}