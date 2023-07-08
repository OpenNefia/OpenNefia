using Love;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI.Element;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// A tile batch for rendering "strips" of map tile/chip batches with proper Z-ordering.
    /// This drawable is intended to replicate vanilla's original tile renderer.
    /// </summary>
    /// <remarks>
    /// The strips of tiles are necessary to make sure things like wall overhangs properly occlude
    /// tiles/chips in adjacent tile rows.
    /// </remarks>
    internal class TileAndChipBatch : BaseDrawable
    {
        private ITileAtlasManager _atlasManager = default!;
        private ICoords _coords = default!;
        private IConfigurationManager _config = default!;

        private TileAtlas _tileAtlas = default!;
        private TileAtlas _chipAtlas = default!;

        private Vector2i _tiledSize;
        private string[,] _tiles = new string[0, 0];
        private Stack<ChipBatchEntry> _deadEntries = new();
        private TileBatchRow[] _rows = new TileBatchRow[0];
        private HashSet<int> _dirtyRows = new();
        private bool _redrawAll;
        private ChipPrototype _shadowChip = default!;
        private AtlasTile _shadowTile = default!;

        /// <summary>
        /// Tint to apply to tiles (not chips). Subtracted from the rendered
        /// tile image. 
        /// </summary>
        public Color TileShadow { get; set; } = Color.Black;

        public void Initialize(ITileAtlasManager atlasManager, ICoords coords, IConfigurationManager config)
        {
            _atlasManager = atlasManager;
            _coords = coords;
            _config = config;

            _tileAtlas = _atlasManager.GetAtlas(AtlasNames.Tile);
            _chipAtlas = _atlasManager.GetAtlas(AtlasNames.Chip);
            _shadowChip = IoCManager.Resolve<IPrototypeManager>().Index(new PrototypeId<ChipPrototype>("Elona.EntityShadow")); // TODO move
            _shadowTile = _chipAtlas.GetTile(_shadowChip.Image.AtlasIndex);

            _config.OnValueChanged(CVars.DisplayTileScale, OnTileScaleChanged);
        }

        private void OnTileScaleChanged(float scale)
        {
            foreach (var row in _rows)
            {
                row.SetScale(scale);
            }
        }

        private float GetScale()
        {
            return _config.GetCVar(CVars.DisplayTileScale);
        }

        public void OnThemeSwitched()
        {
            _tileAtlas = _atlasManager.GetAtlas(AtlasNames.Tile);
            _chipAtlas = _atlasManager.GetAtlas(AtlasNames.Chip);
            _shadowTile = _chipAtlas.GetTile(_shadowChip.Image.AtlasIndex);
            var scale = GetScale();

            for (int tileY = 0; tileY < _tiledSize.Y; tileY++)
            {
                _rows[tileY] = new TileBatchRow(_tileAtlas, _chipAtlas, _coords, _tiledSize.X, tileY, scale);
            }
        }

        public void SetMapSize(Vector2i size)
        {
            var (width, height) = size;
            var scale = GetScale();

            _tiledSize = size;
            _tiles = new string[width, height];

            _deadEntries.Clear();
            _rows = new TileBatchRow[height];
            _dirtyRows.Clear();

            _redrawAll = true;

            for (int tileY = 0; tileY < height; tileY++)
            {
                _rows[tileY] = new TileBatchRow(_tileAtlas, _chipAtlas, _coords, width, tileY, scale);
            }
        }

        public void AddChipEntry(MapObjectMemory memory)
        {
            ChipBatchEntry? entry;

            if (!_chipAtlas.TryGetTile(memory.AtlasIndex, out var tile))
            {
                Logger.ErrorS("tile.chipBatch", $"Missing chip {memory.AtlasIndex}");
                return;
            }

            // Allocate a new chip batch entry.
            entry = new ChipBatchEntry(tile, memory);

            // Add shadow.
            if (memory.IsVisible)
            {
                var screenPos = _coords.TileToScreen(memory.Coords.Position);
                var scale = GetScale();
                switch (memory.ShadowType)
                {
                    case ShadowType.None:
                    default:
                        break;
                    case ShadowType.Normal:
                        _rows[entry.RowIndex].ShadowBatch.Add(_shadowTile.Quad, screenPos.X + 8, screenPos.Y + 36);
                        break;
                    case ShadowType.DropShadow:
                        var viewport = tile.Quad.GetViewport();
                        // TODO no idea what the actual rotation amounts should be
                        // TODO this needs to be rendered as a solid color instead of the texture of the chip
                        _rows[entry.RowIndex].ShadowBatch.Add(tile.Quad, screenPos.X + memory.ScreenOffset.X + (viewport.Height / _coords.TileSize.Y) * 8 + 2, screenPos.Y + memory.ScreenOffset.Y - 4 - (viewport.Height - _coords.TileSize.Y), angle: memory.ShadowRotationRads);
                        break;
                }
            }

            // Add to the appropriate Z layer strip.
            _rows[entry.RowIndex].ChipBatch.AddOrUpdateChipEntry(entry);
            _dirtyRows.Add(entry.RowIndex);
        }

        public void Clear()
        {
            _deadEntries.Clear();
            _redrawAll = true;

            foreach (var row in _rows)
            {
                row.Clear();
            }
        }

        public void SetTile(Vector2i pos, string tile)
        {
            _tiles[pos.X, pos.Y] = tile;
            _dirtyRows.Add(pos.Y);
        }

        public void UpdateBatches()
        {
            if (_redrawAll)
            {
                for (int y = 0; y < _rows.Length; y++)
                {
                    var row = _rows[y];
                    row.UpdateTileBatches(_tiles, y, _tiledSize.X);
                    row.UpdateChipBatch();
                    row.TileShadow = TileShadow;
                }
            }
            else
            {
                foreach (int y in _dirtyRows)
                {
                    var row = _rows[y];
                    row.UpdateTileBatches(_tiles, y, _tiledSize.X);
                    row.UpdateChipBatch();
                    row.TileShadow = TileShadow;
                }
            }
            _redrawAll = false;
            _dirtyRows.Clear();
        }

        public override void Update(float dt)
        {
            foreach (var row in _rows)
            {
                row.Update(dt);
            }
        }

        public override void Draw()
        {
            for (int tileY = 0; tileY < _rows.Length; tileY++)
            {
                var row = _rows[tileY];
                row.Draw(PixelX, PixelY);
            }
        }
    }

    internal class TileBatchRow
    {
        internal SpriteBatch TileBatch;
        internal ChipBatch ChipBatch;
        internal SpriteBatch ShadowBatch;
        internal SpriteBatch TileOverhangBatch;
        private int TileWidth;
        private int RowYIndex;
        private int ScreenWidth;
        private int ScreenHeight;
        private float Scale;
        private bool HasOverhang = false;
        private ICoords Coords;

        private TileAtlas TileAtlas;
        private TileAtlas ChipAtlas;
        public Color TileShadow { get; set; }

        public TileBatchRow(TileAtlas tileAtlas, TileAtlas chipAtlas, ICoords coords, int widthInTiles, int rowYIndex, float scale)
        {
            TileAtlas = tileAtlas;
            ChipAtlas = chipAtlas;
            Coords = coords;
            Scale = scale;

            TileBatch = Love.Graphics.NewSpriteBatch(tileAtlas.Image, 2048, Love.SpriteBatchUsage.Dynamic);
            ChipBatch = new ChipBatch(chipAtlas, coords);
            ShadowBatch = Love.Graphics.NewSpriteBatch(chipAtlas.Image, 2048, Love.SpriteBatchUsage.Dynamic);
            TileOverhangBatch = Love.Graphics.NewSpriteBatch(tileAtlas.Image, 2048, Love.SpriteBatchUsage.Dynamic);

            TileWidth = Coords.TileSize.Y;
            RowYIndex = rowYIndex;
            ScreenWidth = widthInTiles * TileWidth;
        }

        public void SetScale(float scale)
        {
            Scale = scale;
        }

        public void OnThemeSwitched()
        {
        }

        internal void UpdateTileBatches(string[,] tiles, int y, int widthInTiles)
        {
            ScreenWidth = widthInTiles * TileWidth;
            TileBatch.Clear();
            TileOverhangBatch.Clear();
            HasOverhang = false;

            for (int x = 0; x < widthInTiles; x++)
            {
                var tileId = tiles[x, y];
                if (TileAtlas.TryGetTile(tileId, out var tile))
                {
                    var screenPos = Coords.TileToScreen(new Vector2i(x, RowYIndex));
                    TileBatch.Add(tile.Quad, screenPos.X, screenPos.Y);

                    if (tile.HasOverhang)
                    {
                        HasOverhang = true;
                        TileOverhangBatch.Add(tile.Quad, screenPos.X, screenPos.Y);
                    }
                }
                else
                {
                    Logger.ErrorS("tile.chipBatch", $"Missing tile {tileId}");
                }
            }

            TileBatch.Flush();
            TileOverhangBatch.Flush();
        }

        internal void UpdateChipBatch()
        {
            ChipBatch.UpdateBatches();
        }

        public void Clear()
        {
            TileBatch.Clear();
            TileOverhangBatch.Clear();
            ChipBatch.Clear();
            ShadowBatch.Clear();
        }

        public void Update(float dt)
        {
            ChipBatch.Update(dt);
        }

        public void Draw(int screenX, int screenY)
        {
            if (HasOverhang)
            {
                var overhangHeight = Coords.TileSize.Y / 4;
                Love.Graphics.SetScissor(screenX, screenY + RowYIndex * Coords.TileSize.Y - overhangHeight, (int)(ScreenWidth * Scale), (int)(overhangHeight * Scale));
                Love.Graphics.Draw(TileOverhangBatch, screenX, screenY - overhangHeight, 0, Scale, Scale);

                Love.Graphics.SetBlendMode(BlendMode.Subtract);
                Love.Graphics.SetColor(TileShadow);
                Love.Graphics.Draw(TileOverhangBatch, screenX, screenY - overhangHeight, 0, Scale, Scale);
                Love.Graphics.SetBlendMode(BlendMode.Alpha);

                Love.Graphics.SetScissor();
            }

            Love.Graphics.SetColor(Color.White);
            Love.Graphics.Draw(TileBatch, screenX, screenY, 0, Scale, Scale);

            // TODO: The original HSP code uses the gfdec2 function. gfdec2
            // decrements colors but prevents them from reaching a 0 value, so
            // the colors here are inaccurate.
            Love.Graphics.SetBlendMode(BlendMode.Subtract);
            Love.Graphics.SetColor(TileShadow);
            //Love.Graphics.Rectangle(DrawMode.Fill, screenX, screenY + RowYIndex * Coords.TileSize.Y, ScreenWidth, Coords.TileSize.Y);
            Love.Graphics.Draw(TileBatch, screenX, screenY, 0, Scale, Scale);

            Love.Graphics.SetColor(Color.White.WithAlphaB(110));
            Love.Graphics.Draw(ShadowBatch, screenX, screenY, 0, Scale, Scale);
            Love.Graphics.SetBlendMode(BlendMode.Alpha);

            Love.Graphics.SetColor(Color.White);
            ChipBatch.Draw(screenX, screenY, Scale);
        }
    }
}
