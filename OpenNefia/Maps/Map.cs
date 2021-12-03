using OpenNefia.Core.Game;
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
        public MapId Id { get; set; }

        public int Width { get; }
        public int Height { get; }
        public Vector2i Size => new Vector2i(Width, Height);

        public Tile[,] Tiles { get; }
        public Tile[,] TileMemory { get; }
        public TileFlag[,] TileFlags { get; }
        internal int[,] _InSight;
        internal int _LastSightId;
        public ShadowMap ShadowMap { get; }
        public MapObjectMemoryStore MapObjectMemory { get; }

        public HashSet<MapCoordinates> DirtyTilesThisTurn { get; } = new();
        public bool RedrawAllThisTurn { get; set; }
        public bool NeedsRedraw { get => DirtyTilesThisTurn.Count > 0 || RedrawAllThisTurn; }

        private List<IEntity> _entities { get; } = new List<IEntity>();

        public IEnumerable<IEntity> Entities => _entities.ToList();

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new Tile[width, height];
            TileMemory = new Tile[width, height];
            TileFlags = new TileFlag[width, height];
            MapObjectMemory = new MapObjectMemoryStore(this);
            _InSight = new int[width, height];
            ShadowMap = new ShadowMap(this);
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

        public void MemorizeAll()
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
            DirtyTilesThisTurn.Add(this.AtPos(pos));
        }

        public void MemorizeTile(Vector2i pos)
        {
            if (!IsInBounds(pos))
                return;

            Tiles[pos.X, pos.Y] = TileMemory[pos.X, pos.Y];
            MapObjectMemory.RevealObjects(pos);
            _InSight[pos.X, pos.Y] = _LastSightId;
            DirtyTilesThisTurn.Add(AtPos(pos));
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

            foreach (var obj in AtPos(pos).GetEntities())
            {
                var spatial = obj.Spatial;
                isSolid |= spatial.IsSolid;
                isOpaque |= spatial.IsOpaque;
            }

            if (isSolid)
                flags |= TileFlag.IsSolid;
            if (isOpaque)
                flags |= TileFlag.IsOpaque;

            this.TileFlags[pos.X, pos.Y] = flags;
            this.DirtyTilesThisTurn.Add(AtPos(pos));
        }

        public IEnumerable<MapCoordinates> AllTiles
        {
            get
            {
                for (var x = 0; x < Width; x++)
                {
                    for (var y = 0; y < Height; y++)
                    {
                        yield return new MapCoordinates(this, x, y);
                    }
                }
            }
        }

        public MapCoordinates AtPos(Vector2i pos)
        {
            return new MapCoordinates(this, pos);
        }

        public MapCoordinates AtPos(int x, int y)
        {
            return new MapCoordinates(this, x, y);
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

        public bool CanAccess(Vector2i pos)
        {
            return IsInBounds(pos) && (TileFlags[pos.X, pos.Y] & TileFlag.IsSolid) == TileFlag.None;
        }

        public void AddEntity(IEntity entity)
        {
            if (entity.Spatial.Map != null)
            {
                throw new ArgumentException($"Entity is already in map {entity.Spatial.Map.Id}", nameof(entity));
            }
            entity.Spatial.ChangeMap(this);
            _entities.Add(entity);
        }

        public void RemoveEntity(IEntity entity)
        {
            if (entity.Spatial.Map != this)
            {
                throw new ArgumentException($"Entity is in map {entity.Spatial.Map?.Id}", nameof(entity));
            }
            entity.Spatial.ChangeMap(null);
            _entities.Remove(entity);
            RefreshTile(entity.Spatial.Pos);
        }
    }
}
