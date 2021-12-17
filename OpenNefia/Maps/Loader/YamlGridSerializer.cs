using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using System.Text;

namespace OpenNefia.Core.Maps
{
    public static class YamlGridSerializer
    {
        public static IMap DeserializeGrid(string gridString,
            Dictionary<string, PrototypeId<TilePrototype>> tileMap)
        {
            var lines = gridString.Split("\n").ToArray();
            if (lines.Length == 0)
            {
                throw new InvalidDataException("Grid string was blank.");
            }
            var width = lines[0].Length;
            var height = lines.Length;

            var map = new Map(width, height);

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

                    map.SetTile(new Vector2i(x, y), tileId);

                    x++;
                }

                x = 0;
                y++;
            }

            return map;
        }

        public static Dictionary<PrototypeId<TilePrototype>, string> BuildProtoToRuneTileMap(IMap map)
        {
            var tilesSeen = new HashSet<PrototypeId<TilePrototype>>();
            
            foreach (var tile in map.Tiles)
            {
                tilesSeen.Add(tile.ResolvePrototype().GetStrongID());
            }

            var result = new Dictionary<PrototypeId<TilePrototype>, string>();
            char c = 'a'; // TODO beautify based on wall/floor

            foreach (var tileId in tilesSeen)
            {
                result.Add(tileId, c.ToString());
                c++;
            }

            return result;
        }


        public static string SerializeGrid(IMap map, 
            Dictionary<PrototypeId<TilePrototype>, string> protoToRune,
            ITileDefinitionManager tileDefinitionManager)
        {
            var sb = new StringBuilder();

            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    var tile = map.GetTile(new Vector2i(x, y));
                    var protoId = tileDefinitionManager[tile.Type].GetStrongID();
                    if (!protoToRune.TryGetValue(protoId, out var rune))
                    {
                        throw new InvalidDataException($"Tile ID '{protoId}' has no tilemap rune.");
                    }
                    sb.Append(rune[0]);
                }
                sb.Append('\n');
            }

            return sb.ToString();
        }
    }
}