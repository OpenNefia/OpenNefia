using System;
using Why.Core.GameObjects;

namespace Why.Core.Maps
{
    public sealed class Map : IMap
    {
        internal enum TileFlags : int
        {
            None = 0b00000000,

            IsSolid = 0b00000001,
            IsOpaque = 0b00000010,
        }

        public MapId Id { get; set; }

        public int Width { get; }
        public int Height { get; }

        public Tile[,] Tiles { get; }
        public Tile[,] TileMemory { get; }

        public List<IEntity> Entities { get; } = new List<IEntity>();

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new Tile[width, height];
            TileMemory = new Tile[width, height];
        }

        public IEnumerable<TileRef> AllTiles
        {
            get
            {
                for (var x = 0; x < Width; x++)
                {
                    for (var y = 0; y < Height; y++)
                    {
                        yield return new TileRef(Id, x, y, Tiles[x, y]);
                    }
                }
            }
        }

        public IEnumerable<TileRef> AllTileMemory
        {
            get
            {
                for (var x = 0; x < Width; x++)
                {
                    for (var y = 0; y < Height; y++)
                    {
                        yield return new TileRef(Id, x, y, Tiles[x, y]);
                    }
                }
            }
        }

        public MapCoordinates AtPos(int x, int y)
        {
            return new MapCoordinates(Id, x, y);
        }
    }
}
