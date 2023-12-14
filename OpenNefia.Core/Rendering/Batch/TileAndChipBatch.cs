using Love;
using Melanchall.DryWetMidi.MusicTheory;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering.TileRowDrawLayers;
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
        private IMapTileRowRenderer _tileRowRenderer = default!;
        private IConfigurationManager _config = default!;

        private TileAtlas _tileAtlas = default!;
        private TileAtlas _chipAtlas = default!;

        private ITileRowLayer[] _afterTilesTileRowLayers = new ITileRowLayer[0];
        private ITileRowLayer[] _afterChipsTileRowLayers = new ITileRowLayer[0];

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

        public void Initialize(ITileAtlasManager atlasManager, ICoords coords, IMapTileRowRenderer tileRowRenderer, IConfigurationManager config)
        {
            _atlasManager = atlasManager;
            _coords = coords;
            _tileRowRenderer = tileRowRenderer;
            _config = config;

            _tileAtlas = _atlasManager.GetAtlas(AtlasNames.Tile);
            _chipAtlas = _atlasManager.GetAtlas(AtlasNames.Chip);
            _shadowChip = IoCManager.Resolve<IPrototypeManager>().Index(new PrototypeId<ChipPrototype>("Elona.EntityShadow")); // TODO move
            _shadowTile = _chipAtlas.GetTile(_shadowChip.Image.AtlasIndex);

            _afterTilesTileRowLayers = _tileRowRenderer.GetTileRowLayers(TileRowLayerType.Tile).ToArray();
            _afterChipsTileRowLayers = _tileRowRenderer.GetTileRowLayers(TileRowLayerType.Chip).ToArray();

            _config.OnValueChanged(CVars.DisplayTileScale, OnTileScaleChanged);
            _config.OnValueChanged(CVars.AnimeObjectMovementSpeed, OnObjMovementSpeedChanged);
        }

        private void OnTileScaleChanged(float scale)
        {
            foreach (var row in _rows)
            {
                row.SetScale(scale);
            }
        }

        private void OnObjMovementSpeedChanged(float speed)
        {
            foreach (var row in _rows)
            {
                row.ChipBatch.ObjectMovementSpeed = speed;
            }
        }

        private float GetScale()
        {
            return _config.GetCVar(CVars.DisplayTileScale);
        }

        private float GetObjectMovementSpeed()
        {
            return _config.GetCVar(CVars.AnimeObjectMovementSpeed);
        }

        public void OnThemeSwitched()
        {
            _tileAtlas = _atlasManager.GetAtlas(AtlasNames.Tile);
            _chipAtlas = _atlasManager.GetAtlas(AtlasNames.Chip);
            _shadowTile = _chipAtlas.GetTile(_shadowChip.Image.AtlasIndex);
            var scale = GetScale();

            for (int tileY = 0; tileY < _tiledSize.Y; tileY++)
            {
                _rows[tileY] = new TileBatchRow(_tileAtlas, _chipAtlas, _coords, _tiledSize.X, tileY, scale, _afterTilesTileRowLayers, _afterChipsTileRowLayers, _shadowTile, GetObjectMovementSpeed());
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
                _rows[tileY] = new TileBatchRow(_tileAtlas, _chipAtlas, _coords, width, tileY, scale, _afterTilesTileRowLayers, _afterChipsTileRowLayers, _shadowTile, GetObjectMovementSpeed());
            }
        }

        public void AddChipEntry(MapObjectMemory memory)
        {
            ChipBatchEntry? entry;
            AtlasTile? tile = null;

            if (memory.AtlasIndex != null)
            {
                if (!_chipAtlas.TryGetTile(memory.AtlasIndex, out tile))
                {
                    Logger.ErrorS("tile.chipBatch", $"Missing chip {memory.AtlasIndex}");
                    return;
                }
            }

            // Allocate a new chip batch entry.
            entry = new ChipBatchEntry(tile, memory);

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
                row.DrawBottom(PixelX, PixelY);
            }
            for (int tileY = 0; tileY < _rows.Length; tileY++)
            {
                var row = _rows[tileY];
                row.DrawTop(PixelX, PixelY);
            }
        }
    }

    internal class TileBatchRow
    {
        internal Love.SpriteBatch TileBatchBottom;
        internal Love.SpriteBatch TileBatchTop;
        internal ChipBatch ChipBatch;
        internal Love.SpriteBatch TileOverhangBatch;
        internal ITileRowLayer[] AfterTilesTileRowLayers;
        internal ITileRowLayer[] AfterChipsTileRowLayers;
        private int TileWidth;
        private int RowYIndex;
        private int ScreenWidth;
        private int ScreenHeight;
        private float TileScale;
        private bool HasOverhang = false;
        private ICoords Coords;

        private TileAtlas TileAtlas;
        private TileAtlas ChipAtlas;
        public Color TileShadow { get; set; }

        public TileBatchRow(TileAtlas tileAtlas, TileAtlas chipAtlas, ICoords coords, int widthInTiles, int rowYIndex, float scale,
            ITileRowLayer[] afterTiles, ITileRowLayer[] afterChips, AtlasTile shadowTile, float objMovementSpeed)
        {
            TileAtlas = tileAtlas;
            ChipAtlas = chipAtlas;
            Coords = coords;
            TileScale = scale;

            TileBatchBottom = Love.Graphics.NewSpriteBatch(tileAtlas.Image, 2048, Love.SpriteBatchUsage.Dynamic);
            TileBatchTop = Love.Graphics.NewSpriteBatch(tileAtlas.Image, 2048, Love.SpriteBatchUsage.Dynamic);
            ChipBatch = new ChipBatch(chipAtlas, coords, shadowTile, objMovementSpeed);
            TileOverhangBatch = Love.Graphics.NewSpriteBatch(tileAtlas.Image, 2048, Love.SpriteBatchUsage.Dynamic);

            AfterTilesTileRowLayers = afterTiles;
            AfterChipsTileRowLayers = afterChips;

            TileWidth = Coords.TileSize.Y;
            RowYIndex = rowYIndex;
            ScreenWidth = widthInTiles * TileWidth;
        }

        public void SetScale(float scale)
        {
            TileScale = scale;
        }

        public void OnThemeSwitched()
        {
        }

        internal void UpdateTileBatches(string[,] tiles, int y, int widthInTiles)
        {
            ScreenWidth = widthInTiles * TileWidth;
            TileBatchBottom.Clear();
            TileOverhangBatch.Clear();
            HasOverhang = false;

            for (int x = 0; x < widthInTiles; x++)
            {
                var tileId = tiles[x, y];
                if (TileAtlas.TryGetTile(tileId, out var tile))
                {
                    var screenPos = Coords.TileToScreen(new Vector2i(x, RowYIndex));

                    if (tile.HasOverhang)
                    {
                        HasOverhang = true;
                        TileBatchTop.Add(tile.Quad, screenPos.X, screenPos.Y);
                        TileOverhangBatch.Add(tile.Quad, screenPos.X, screenPos.Y);
                    }
                    else
                    {
                        TileBatchBottom.Add(tile.Quad, screenPos.X, screenPos.Y);
                    }
                }
                else
                {
                    Logger.ErrorS("tile.chipBatch", $"Missing tile {tileId}");
                }
            }

            TileBatchBottom.Flush();
            TileBatchTop.Flush();
            TileOverhangBatch.Flush();
        }

        internal void UpdateChipBatch()
        {
            ChipBatch.UpdateBatches();
        }

        public void Clear()
        {
            TileBatchBottom.Clear();
            TileBatchTop.Clear();
            TileOverhangBatch.Clear();
            ChipBatch.Clear();
        }

        public void Update(float dt)
        {
            ChipBatch.Update(dt);
        }

        public void DrawBottom(int screenX, int screenY)
        {
            // Draw wall overhang.
            if (HasOverhang)
            {
                var overhangHeight = Coords.TileSize.Y / 4;
                Love.Graphics.SetScissor(screenX, screenY + RowYIndex * Coords.TileSize.Y - overhangHeight, (int)(ScreenWidth * TileScale), (int)(overhangHeight * TileScale));
                Love.Graphics.Draw(TileOverhangBatch, screenX, screenY - overhangHeight, 0, TileScale, TileScale);

                // Darken tiles to simulate day/night.
                Love.Graphics.SetBlendMode(BlendMode.Subtract);
                Love.Graphics.SetColor(TileShadow);
                Love.Graphics.Draw(TileOverhangBatch, screenX, screenY - overhangHeight, 0, TileScale, TileScale);
                Love.Graphics.SetBlendMode(BlendMode.Alpha);

                Love.Graphics.SetScissor();
            }

            Love.Graphics.SetBlendMode(BlendMode.Alpha);
            Love.Graphics.SetColor(Color.White);
            Love.Graphics.Draw(TileBatchBottom, screenX, screenY, 0, TileScale, TileScale);
            
            // Darken tiles to simulate day/night.
            // TODO: The original HSP code uses the gfdec2 function. gfdec2
            // decrements colors but prevents them from reaching a 0 value, so
            // the colors here are inaccurate.
            Love.Graphics.SetBlendMode(BlendMode.Subtract);
            Love.Graphics.SetColor(TileShadow);
            //Love.Graphics.Rectangle(DrawMode.Fill, screenX, screenY + RowYIndex * Coords.TileSize.Y, ScreenWidth, Coords.TileSize.Y);
            Love.Graphics.Draw(TileBatchBottom, screenX, screenY, 0, TileScale, TileScale);
            Love.Graphics.SetBlendMode(BlendMode.Alpha);

            foreach (var rowLayer in AfterTilesTileRowLayers)
            {
                rowLayer.DrawRow(RowYIndex, screenX, screenY);
            }
        }

        public void DrawTop(int screenX, int screenY)
        {
            Love.Graphics.SetColor(Color.White);
            ChipBatch.Draw(screenX, screenY, TileScale);

            Love.Graphics.SetColor(Color.White);
            Love.Graphics.Draw(TileBatchTop, screenX, screenY, 0, TileScale, TileScale);

            // Darken tiles to simulate day/night.
            // TODO: The original HSP code uses the gfdec2 function. gfdec2
            // decrements colors but prevents them from reaching a 0 value, so
            // the colors here are inaccurate.
            Love.Graphics.SetBlendMode(BlendMode.Subtract);
            Love.Graphics.SetColor(TileShadow);
            //Love.Graphics.Rectangle(DrawMode.Fill, screenX, screenY + RowYIndex * Coords.TileSize.Y, ScreenWidth, Coords.TileSize.Y);
            Love.Graphics.Draw(TileBatchTop, screenX, screenY, 0, TileScale, TileScale);

            foreach (var rowLayer in AfterChipsTileRowLayers)
            {
                rowLayer.DrawRow(RowYIndex, screenX, screenY);
            }
        }
    }
}
