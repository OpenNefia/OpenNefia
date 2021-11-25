using System;

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

        public Tile[,] Tiles;
        public Tile[,] TileMemory;

        // public List<IEntity> Entities = new List<IEntity>();

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
                        yield return new TileRef(this.Id, x, y, Tiles[x, y]);
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
                        yield return new TileRef(this.Id, x, y, Tiles[x, y]);
                    }
                }
            }
        }
    }
}
