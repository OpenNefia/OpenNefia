﻿using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
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
        public EntityUid MapEntityUid { get; internal set; }
        public MapId Id { get; internal set; }

        public int Width { get; }
        public int Height { get; }
        public Vector2i Size => new Vector2i(Width, Height);
        public UIBox2i Bounds => UIBox2i.FromDimensions(Vector2i.Zero, Size);

        public Tile[,] Tiles { get; }
        public Tile[,] TileMemory { get; }
        private TileFlag[,] _tileFlagsTiles { get; }
        private TileFlag[,] _tileFlagsEntities { get; }
        private TileFlag[,] _tileFlags { get; }
        internal int[,] _InSight;
        internal int _LastSightId;
        public ShadowMap ShadowMap { get; }
        public MapObjectMemoryStore MapObjectMemory { get; }

        public HashSet<Vector2i> DirtyTilesThisTurn { get; } = new();
        public bool RedrawAllThisTurn { get; set; }
        public bool NeedsRedraw { get => DirtyTilesThisTurn.Count > 0 || RedrawAllThisTurn; }

        private readonly List<Entity> _entities = new List<Entity>();
        public IEnumerable<Entity> Entities => _entities.ToList();

        public Map(int width, int height)
        {
            MapEntityUid = EntityUid.Invalid;
            Id = MapId.Nullspace;

            Width = width;
            Height = height;
            Tiles = new Tile[width, height];
            TileMemory = new Tile[width, height];
            _tileFlagsTiles = new TileFlag[width, height];
            _tileFlagsEntities = new TileFlag[width, height];
            _tileFlags = new TileFlag[width, height];
            MapObjectMemory = new MapObjectMemoryStore(this);
            _InSight = new int[width, height];
            ShadowMap = new ShadowMap(this, IoCManager.Resolve<ICoords>());
        }

        public void Clear(PrototypeId<TilePrototype> tile)
        {
            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    this.SetTile(new Vector2i(x, y), tile);
                }
            }
        }

        public void ClearMemory(PrototypeId<TilePrototype> tile)
        {
            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    this.SetTileMemory(new Vector2i(x, y), tile);
                }
            }
            this.RedrawAllThisTurn = true;
        }

        public void MemorizeAllTiles()
        {
            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    TileMemory[x, y] = Tiles[x, y];
                    _InSight[x, y] = _LastSightId;
                }
            }
            this.RedrawAllThisTurn = true;
        }

        public bool IsInBounds(Vector2i pos)
        {
            return pos.X >= 0 && pos.Y >= 0 && pos.X < Width && pos.Y < Height;
        }

        public void SetTile(Vector2i pos, PrototypeId<TilePrototype> tileId)
        {
            if (!IsInBounds(pos))
                return;

            Tiles[pos.X, pos.Y] = new Tile(tileId.ResolvePrototype().TileIndex);
            this.RefreshTile(pos);
        }

        public void SetTileMemory(Vector2i pos, PrototypeId<TilePrototype> tileId)
        {
            if (!IsInBounds(pos))
                return;

            TileMemory[pos.X, pos.Y] = new Tile(tileId.ResolvePrototype().TileIndex);
            DirtyTilesThisTurn.Add(pos);
        }

        public void MemorizeTile(Vector2i pos)
        {
            if (!IsInBounds(pos))
                return;

            TileMemory[pos.X, pos.Y] = Tiles[pos.X, pos.Y];
            DirtyTilesThisTurn.Add(pos);
            MapObjectMemory.RevealObjects(pos);
            _InSight[pos.X, pos.Y] = _LastSightId;
        }

        public void RefreshTile(Vector2i pos)
        {
            if (!IsInBounds(pos))
                return;

            var flags = TileFlag.None;

            // TODO
            var tileDefinitions = IoCManager.Resolve<ITileDefinitionManager>();
            
            var tile = Tiles[pos.X, pos.Y];
            var tileProto = tileDefinitions[tile.Type];
            var isSolid = tileProto.IsSolid;
            var isOpaque = tileProto.IsOpaque;

            if (isSolid)
                flags |= TileFlag.IsSolid;
            if (isOpaque)
                flags |= TileFlag.IsOpaque;

            _tileFlagsTiles[pos.X, pos.Y] = flags;
            _tileFlags[pos.X, pos.Y] = flags | _tileFlagsEntities[pos.X, pos.Y];
            this._tileFlags[pos.X, pos.Y] = flags;
            this.DirtyTilesThisTurn.Add(pos);
        }

        public void RefreshTileEntities(Vector2i pos, IEnumerable<Entity> entities)
        {
            if (!IsInBounds(pos))
                return;

            var isSolid = false;
            var isOpaque = false;
            var flags = TileFlag.None;

            foreach (var obj in entities)
            {
                var spatial = obj.Spatial;
                isSolid |= spatial.IsSolid;
                isOpaque |= spatial.IsOpaque;
            }

            if (isSolid)
                flags |= TileFlag.IsSolid;
            if (isOpaque)
                flags |= TileFlag.IsOpaque;

            _tileFlagsEntities[pos.X, pos.Y] = flags;
            _tileFlags[pos.X, pos.Y] = flags | _tileFlagsTiles[pos.X, pos.Y];
            this.DirtyTilesThisTurn.Add(pos);
        }

        public IEnumerable<MapCoordinates> AllTiles
        {
            get
            {
                for (var x = 0; x < Width; x++)
                {
                    for (var y = 0; y < Height; y++)
                    {
                        yield return new MapCoordinates(x, y, Id);
                    }
                }
            }
        }

        public Tile GetTile(Vector2i pos)
        {
            if (!IsInBounds(pos))
                return Tile.Empty;

            return Tiles[pos.X, pos.Y];
        }

        public Tile GetTileMemory(Vector2i pos)
        {
            if (!IsInBounds(pos))
                return Tile.Empty;

            return TileMemory[pos.X, pos.Y];
        }

        public MapCoordinates AtPos(Vector2i pos)
        {
            return new MapCoordinates(pos, this.Id);
        }

        public MapCoordinates AtPos(int x, int y)
        {
            return new MapCoordinates(x, y, this.Id);
        }

        public void RefreshVisibility()
        {
            _LastSightId += 1;
            this.ShadowMap.RefreshVisibility();

            MapObjectMemory.AllMemory.Values
                .Where(memory => memory.HideWhenOutOfSight && !IsInWindowFov(memory.Coords.Position))
                .Select(memory => memory.Coords)
                .Distinct()
                .ForEach(coords => MapObjectMemory.HideObjects(coords.Position));
        }

        public bool IsInWindowFov(Vector2i pos)
        {
            if (!IsInBounds(pos))
                return false;

            return _InSight[pos.X, pos.Y] == _LastSightId;
        }

        public bool IsMemorized(Vector2i pos)
        {
            if (!IsInBounds(pos))
                return false;

            return TileMemory[pos.X, pos.Y] == Tiles[pos.X, pos.Y];
        }

        public bool CanAccess(Vector2i pos)
        {
            return IsInBounds(pos) && (_tileFlags[pos.X, pos.Y] & TileFlag.IsSolid) == TileFlag.None;
        }

        public bool CanSeeThrough(Vector2i pos)
        {
            return IsInBounds(pos) && (_tileFlags[pos.X, pos.Y] & TileFlag.IsOpaque) == TileFlag.None;
        }

        public bool HasLos(Vector2i from, Vector2i to)
        {
            if (!IsInBounds(from) || !IsInBounds(to))
                return false;

            foreach (var pos in PosHelpers.EnumerateLine(from, to))
            {
                // In Elona, the final tile is visible even if it is solid.
                if (!CanSeeThrough(pos) && pos != to)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
