using OpenNefia.Core.GameObjects;

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

        MapCoordinates AtPos(int x, int y);
    }
}