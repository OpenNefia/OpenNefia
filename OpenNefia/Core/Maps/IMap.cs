using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Core.Maps
{
    public interface IMap
    {
        MapId Id { get; set; }
        int Width { get; }
        int Height { get; }
        Tile[,] Tiles { get; }
        Tile[,] TileMemory { get; }

        public List<IEntity> Entities { get; }
        TileFlag[,] TileFlags { get; }
        MapObjectMemoryStore MapObjectMemory { get; }

        MapCoordinates AtPos(int x, int y);
        TileRef GetTileMemoryRef(Vector2i pos);
        TileRef GetTileRef(Vector2i pos);
        void MemorizeTile(Vector2i position);
    }
}