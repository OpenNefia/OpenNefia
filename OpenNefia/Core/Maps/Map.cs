using System;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Maps
{
    public enum TileFlag : int
    {
        None = 0b00000000,

        IsSolid = 0b00000001,
        IsOpaque = 0b00000010,
    }

    public sealed class Map : IMap
    {
        public MapId Id { get; set; }

        public int Width { get; }
        public int Height { get; }

        public Tile[,] Tiles { get; }
        public Tile[,] TileMemory { get; }
        public TileFlag[,] TileFlags { get; }
        internal int[] _InSight;
        internal int _LastSightId;
        public ShadowMap ShadowMap { get; }
        public MapObjectMemoryStore MapObjectMemory { get; }

        internal HashSet<Vector2i> _DirtyTilesThisTurn;
        internal bool _RedrawAllThisTurn;
        internal bool _NeedsRedraw { get => _DirtyTilesThisTurn.Count > 0 || _RedrawAllThisTurn; }

        public List<IEntity> Entities { get; } = new List<IEntity>();

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new Tile[width, height];
            TileMemory = new Tile[width, height];
            TileFlags = new TileFlag[width, height];
            MapObjectMemory = new MapObjectMemoryStore(this);
        }

        public void Clear(TileDef tile)
        {
            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    this.SetTile(x, y, tile);
                }
            }
        }

        public void ClearMemory(TileDef tile)
        {
            for (int i = 0; i < _TileInds.Length; i++)
            {
                _TileMemoryInds[i] = _TileIndexMapping.TileDefIdToIndex[tile.Id];
            }
            this._redrawAllThisTurn = true;
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

        public TileRef GetTileRef(Vector2i pos)
        {
            return new TileRef(Id, pos.X, pos.Y, Tiles[pos.X, pos.Y]);
        }

        public TileRef GetTileMemoryRef(Vector2i pos)
        {
            return new TileRef(Id, pos.X, pos.Y, TileMemory[pos.X, pos.Y]);
        }

        public void MemorizeTile(Vector2i pos)
        {
            Tiles[pos.X, pos.Y] = TileMemory[pos.X, pos.Y];
        }
    }

    public static class MapQuery
    {
        /// <summary>
        /// uh
        /// </summary>
        public static IMap GetMap(this MapCoordinates coords)
        {
            return IoCManager.Resolve<IMapManager>().GetMap(coords.MapId);
        }

        private static bool IsInBounds(IMap map, Vector2i coords)
        {
            return coords.X >= 0 && coords.Y >= 0 && coords.X < map.Width && coords.Y < map.Height;
        }

        public static bool IsInBounds(this MapCoordinates coords)
        {
            return IsInBounds(GetMap(coords), coords.Position);
        }

        private static bool CanSeeThrough(IMap map, Vector2i coords)
        {
            if (!IsInBounds(map, coords))
                return false;

            return (map.TileFlags[coords.X, coords.Y] & TileFlag.IsOpaque) == TileFlag.None;
        }

        public static bool CanSeeThrough(this MapCoordinates coords)
        {
            return CanSeeThrough(GetMap(coords), coords.Position);
        }

        private static bool CanPassThrough(IMap map, Vector2i coords)
        {
            if (!IsInBounds(map, coords))
                return false;

            return (map.TileFlags[coords.X, coords.Y] & TileFlag.IsSolid) == TileFlag.None;
        }

        public static bool CanPassThrough(this MapCoordinates coords)
        {
            return CanPassThrough(GetMap(coords), coords.Position);
        }

        public static bool HasLos(MapCoordinates from, MapCoordinates to)
        {
            if (from.MapId != to.MapId)
                return false;

            var map = from.GetMap();

            foreach (var pos in PosHelpers.EnumerateLine(from.Position, to.Position))
            {
                // In Elona, the final tile is visible even if it is solid.
                if (!CanSeeThrough(map, pos) && pos != to.Position)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
