using Love;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// A tile batch for rendering "strips" of map tile/chip batches with proper Z-ordering.
    /// </summary>
    internal class TileAndChipBatch : BaseDrawable
    {
        private readonly IAtlasManager _atlasManager;

        private int _tiledWidth;
        private int _tiledHeight;

        private string[,] _tiles;
        internal Dictionary<int, ChipBatchEntry> _chipsByIndex;
        internal Dictionary<int, int> _memoryIndexToRowIndex;
        internal Stack<ChipBatchEntry> _deadEntries;
        private TileBatchRow[] _rows;
        private HashSet<int> _dirtyRows;
        private bool _redrawAll;
        private ICoords _coords;
        private TileAtlas _tileAtlas;
        private TileAtlas _chipAtlas;


        public TileAndChipBatch(int width, int height, ICoords coords, IAtlasManager atlasManager)
        {
            _atlasManager = atlasManager;

            _tiledWidth = width;
            _tiledHeight = height;
            _tileAtlas = atlasManager.GetAtlas(AtlasNames.Tile);
            _chipAtlas = atlasManager.GetAtlas(AtlasNames.Chip);
            _coords = coords;
            _tiles = new string[width, height];
            _chipsByIndex = new Dictionary<int, ChipBatchEntry>();
            _memoryIndexToRowIndex = new Dictionary<int, int>();
            _deadEntries = new Stack<ChipBatchEntry>();
            _rows = new TileBatchRow[height];
            _dirtyRows = new HashSet<int>();
            _redrawAll = true;
            for (int tileY = 0; tileY < height; tileY++)
            {
                _rows[tileY] = new TileBatchRow(_tileAtlas, _chipAtlas, _coords, width, tileY);
            }
        }

        public void AddOrUpdateChipEntry(MapObjectMemory memory)
        {
            ChipBatchEntry? entry;

            var tile = _chipAtlas.GetTile(memory.ChipIndex);
            if (tile == null)
                throw new Exception($"Missing chip {memory.ChipIndex}");


            if (this._chipsByIndex.TryGetValue(memory.Index, out entry))
            {
                // This memory is being reused for a potentially different object. This means that it exists in one of the Z layer strips already.
                // Since the Y coordinate of the memory might have changed since then, we have to make sure it gets put into the correct batch.

                var prevRowIndex = this._memoryIndexToRowIndex[entry.Memory.Index];
                var newRowIndex = memory.Coords.Y;

                if (prevRowIndex != newRowIndex)
                {
                    // Y coordinate of the object changed, move the entry between the two rows.
                    var prevRow = this._rows[prevRowIndex];
                    prevRow.ChipBatch.RemoveChipEntry(entry);
                    _dirtyRows.Add(prevRowIndex);
                }

                entry.Memory = memory;
                entry.RowIndex = newRowIndex;
                entry.AtlasTile = tile;
            }
            else
            {
                // This memory was newly allocated, so allocate a new entry or reuse one.

                if (this._deadEntries.Count > 0)
                {
                    entry = this._deadEntries.Pop();
                    entry.Memory = memory;
                    entry.RowIndex = memory.Coords.Y;
                    entry.AtlasTile = tile;
                }
                else
                {
                    entry = new ChipBatchEntry(tile, memory);
                }

                // Add to top level, for tracking purposes.
                this._chipsByIndex.Add(entry.Memory.Index, entry);
            }

            // Add to the appropriate Z layer strip.
            this._rows[entry.RowIndex].ChipBatch.AddOrUpdateChipEntry(entry);
            _dirtyRows.Add(entry.RowIndex);

            // And track the row it's placed into.
            this._memoryIndexToRowIndex[entry.Memory.Index] = entry.RowIndex;
        }

        public ChipBatchEntry? GetChipEntry(MapObjectMemory memory)
            => _chipsByIndex.GetValueOrDefault(memory.Index);

        public void RemoveChipEntry(MapObjectMemory memory)
        {
            var entry = GetChipEntry(memory);

            // First remove the reference at the top level.
            this._chipsByIndex.Remove(memory.Index);

            this._memoryIndexToRowIndex.Remove(memory.Index);

            if (entry != null)
            {
                // Now remove it from the Z layer strip it's in.
                var row = this._rows[entry.RowIndex];
                row.ChipBatch.RemoveChipEntry(entry);
                this._deadEntries.Push(entry);
                _dirtyRows.Add(entry.RowIndex);
            }
        }

        public void OnThemeSwitched(ICoords coords)
        {
            _tileAtlas = _atlasManager.GetAtlas(AtlasNames.Tile);
            _chipAtlas = _atlasManager.GetAtlas(AtlasNames.Chip);
            _coords = coords;
            
            this.Clear();

            foreach (var row in _rows)
            {
                row.OnThemeSwitched(_tileAtlas, _chipAtlas, coords);
            }
        }

        public void Clear()
        {
            this._deadEntries.Clear();
            this._redrawAll = true;

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
                    row.UpdateTileBatches(_tiles, y, _tiledWidth);
                    row.UpdateChipBatch();
                }
            }
            else
            {
                foreach (int y in _dirtyRows)
                {
                    var row = _rows[y];
                    row.UpdateTileBatches(_tiles, y, _tiledWidth);
                    row.UpdateChipBatch();
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
                row.Draw(X, Y);
            }
        }
    }

    internal class TileBatchRow
    {
        internal SpriteBatch TileBatch;
        internal ChipBatch ChipBatch;
        internal SpriteBatch TileOverhangBatch;
        private int TileWidth;
        private int RowYIndex;
        private int ScreenWidth;
        private bool HasOverhang = false;
        private ICoords Coords;

        private TileAtlas TileAtlas;
        private TileAtlas ChipAtlas;

        public TileBatchRow(TileAtlas tileAtlas, TileAtlas chipAtlas, ICoords coords, int widthInTiles, int rowYIndex)
        {
            TileAtlas = tileAtlas;
            ChipAtlas = chipAtlas;
            Coords = coords;

            TileBatch = Love.Graphics.NewSpriteBatch(tileAtlas.Image, 2048, Love.SpriteBatchUsage.Dynamic);
            ChipBatch = new ChipBatch(chipAtlas, coords);
            TileOverhangBatch = Love.Graphics.NewSpriteBatch(tileAtlas.Image, 2048, Love.SpriteBatchUsage.Dynamic);
            
            TileWidth = Coords.TileSize.Y;
            RowYIndex = rowYIndex;
            ScreenWidth = widthInTiles * TileWidth;
        }

        internal void OnThemeSwitched(TileAtlas tileAtlas, TileAtlas chipAtlas, ICoords coords)
        {
            TileAtlas = tileAtlas;
            ChipAtlas = chipAtlas;
            Coords = coords;

            TileBatch = Love.Graphics.NewSpriteBatch(tileAtlas.Image, 2048, Love.SpriteBatchUsage.Dynamic);
            ChipBatch = new ChipBatch(chipAtlas, coords);
            TileOverhangBatch = Love.Graphics.NewSpriteBatch(tileAtlas.Image, 2048, Love.SpriteBatchUsage.Dynamic);
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
                var tile = TileAtlas.GetTile(tileId);
                if (tile == null)
                {
                    Logger.Log(LogLevel.Error, $"Missing tile {tileId}");
                }
                else
                {
                    Coords.TileToScreen(new Vector2i(x, RowYIndex), out var screenPos);
                    TileBatch.Add(tile.Quad, screenPos.X, screenPos.Y);

                    if (tile.HasOverhang)
                    {
                        HasOverhang = true;
                        TileOverhangBatch.Add(tile.Quad, screenPos.X, screenPos.Y);
                    }
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
                Love.Graphics.SetScissor(screenX, screenY + RowYIndex * Coords.TileSize.Y - overhangHeight, ScreenWidth, overhangHeight);
                GraphicsEx.DrawSpriteBatch(TileOverhangBatch, screenX, screenY - overhangHeight);
                Love.Graphics.SetScissor();
            }

            Love.Graphics.Draw(TileBatch, screenX, screenY);
            ChipBatch.Draw(screenX, screenY);
        }
    }
}
