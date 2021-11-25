using Why.Core.GameObjects;

namespace Why.Core.Maps
{
    public interface IMap
    {
        MapId Id { get; set; }
        int Width { get; }
        int Height { get; }
        Tile[,] Tiles { get; }
        Tile[,] TileMemory { get; }

        public List<IEntity> Entities { get; }

        MapCoordinates AtPos(int x, int y);
    }
}