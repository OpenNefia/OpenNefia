using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Rendering.TileDrawLayers
{
    /// <summary>
    /// Draws map tiles and map object memory taken from each entity's
    /// <see cref="GameObjects.ChipComponent"/>.
    /// </summary>
    [RegisterTileLayer]
    public sealed class TileAndChipTileLayer : BaseTileLayer
    {
        [Dependency] private readonly ITileAtlasManager _atlasManager = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        private TileAndChipBatch _tileAndChipBatch = new();
        private WallTileShadows _wallShadows = new();

        public Color TileShadow { get => _tileAndChipBatch.TileShadow; set => _tileAndChipBatch.TileShadow = value; }

        public override void Initialize()
        {
            _tileAndChipBatch.Initialize(_atlasManager, _coords, _config);
            _wallShadows.Initialize(_coords, _config);
        }

        public override void OnThemeSwitched()
        {
            _tileAndChipBatch.OnThemeSwitched();
        }

        public override void SetMap(IMap map)
        {
            base.SetMap(map);

            _tileAndChipBatch.SetMapSize(map.Size);
            _wallShadows.SetMap(map);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            _tileAndChipBatch.SetSize(width, height);
            _wallShadows.SetSize(width, height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            _tileAndChipBatch.SetPosition(x, y);
            _wallShadows.SetPosition(x, y);
        }

        private string ModifyWalls(Vector2i pos, TilePrototype tile)
        {
            // If the tile is a wall, convert the displayed tile to that of
            // the bottom wall if appropriate.
            var tileIndex = tile.Image.AtlasIndex;

            var oneDown = pos + (0, 1);
            var oneTileDown = Map!.GetTile(oneDown);

            var oneUp = pos + (0, -1);
            var oneTileUp = Map.GetTile(oneUp);

            if (tile.WallImage != null)
            {
                if (oneTileDown != null && oneTileDown.Value.Tile.ResolvePrototype().WallImage == null && Map.IsMemorized(oneDown))
                {
                    tileIndex = tile.WallImage.AtlasIndex;
                }

                if (oneTileUp != null && oneTileUp.Value.Tile.ResolvePrototype().WallImage != null && Map.IsMemorized(oneUp))
                {
                    _tileAndChipBatch.SetTile(oneUp, oneTileUp.Value.Tile.ResolvePrototype().Image.AtlasIndex);
                }
            }

            return tileIndex;
        }

        private void SetMapTile(Vector2i pos, TilePrototype tile)
        {
            var tileIndex = ModifyWalls(pos, tile);

            _wallShadows.SetTile(pos, tile);
            _tileAndChipBatch.SetTile(pos, tileIndex);
        }

        public void RedrawMapObjects()
        {
            _tileAndChipBatch.Clear();
            foreach (var memory in Map!.MapObjectMemory.AllMemory.Values)
            {
                _tileAndChipBatch.AddChipEntry(memory);
            }
        }

        public override void RedrawAll()
        {
            _wallShadows.Clear();
            _tileAndChipBatch.Clear();

            foreach (var coords in Map!.AllTiles)
            {
                SetMapTile(coords.Position, Map.TileMemory[coords.X, coords.Y].ResolvePrototype());
            }

            RedrawMapObjects();

            _tileAndChipBatch.UpdateBatches();
        }

        public override void RedrawDirtyTiles(HashSet<Vector2i> dirtyTilesThisTurn)
        {
            foreach (var pos in dirtyTilesThisTurn)
            {
                SetMapTile(pos, Map!.TileMemory[pos.X, pos.Y].ResolvePrototype());
            }

            RedrawMapObjects();

            _tileAndChipBatch.UpdateBatches();
        }

        public override void Update(float dt)
        {
            _tileAndChipBatch.Update(dt);
            _wallShadows.Update(dt);
        }

        public override void Draw()
        {
            _tileAndChipBatch.Draw();
            _wallShadows.Draw();
        }
    }
}
