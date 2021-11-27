using Love;
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
        int TiledWidth;
        int TiledHeight;

        string[] Tiles;
        internal Dictionary<int, ChipBatchEntry> ChipsByIndex;
        internal Dictionary<int, int> MemoryIndexToRowIndex;
        internal Stack<ChipBatchEntry> DeadEntries;
        TileBatchRow[] Rows;
        HashSet<int> DirtyRows;
        bool RedrawAll;
        private ICoords Coords;
        private TileAtlas TileAtlas;
        private TileAtlas ChipAtlas;


        public TileAndChipBatch(int width, int height, ICoords coords)
        {
            TiledWidth = width;
            TiledHeight = height;
            TileAtlas = Atlases.Tile;
            ChipAtlas = Atlases.Chip;
            Coords = coords;
            Tiles = new string[width * height];
            ChipsByIndex = new Dictionary<int, ChipBatchEntry>();
            MemoryIndexToRowIndex = new Dictionary<int, int>();
            DeadEntries = new Stack<ChipBatchEntry>();
            Rows = new TileBatchRow[height];
            DirtyRows = new HashSet<int>();
            RedrawAll = true;
            for (int tileY = 0; tileY < height; tileY++)
            {
                Rows[tileY] = new TileBatchRow(TileAtlas, ChipAtlas, Coords, width, tileY);
            }
        }

        public void AddOrUpdateChipEntry(MapObjectMemory memory)
        {
            ChipBatchEntry? entry;

            var tile = ChipAtlas.GetTile(memory.ChipIndex);
            if (tile == null)
                throw new Exception($"Missing chip {memory.ChipIndex}");


            if (this.ChipsByIndex.TryGetValue(memory.Index, out entry))
            {
                // This memory is being reused for a potentially different object. This means that it exists in one of the Z layer strips already.
                // Since the Y coordinate of the memory might have changed since then, we have to make sure it gets put into the correct batch.

                var prevRowIndex = this.MemoryIndexToRowIndex[entry.Memory.Index];
                var newRowIndex = memory.TileY;

                if (prevRowIndex != newRowIndex)
                {
                    // Y coordinate of the object changed, move the entry between the two rows.
                    var prevRow = this.Rows[prevRowIndex];
                    prevRow.ChipBatch.RemoveChipEntry(entry);
                    DirtyRows.Add(prevRowIndex);
                }

                entry.Memory = memory;
                entry.RowIndex = newRowIndex;
                entry.AtlasTile = tile;
            }
            else
            {
                // This memory was newly allocated, so allocate a new entry or reuse one.

                if (this.DeadEntries.Count > 0)
                {
                    entry = this.DeadEntries.Pop();
                    entry.Memory = memory;
                    entry.RowIndex = memory.TileY;
                    entry.AtlasTile = tile;
                }
                else
                {
                    entry = new ChipBatchEntry(tile, memory);
                }

                // Add to top level, for tracking purposes.
                this.ChipsByIndex.Add(entry.Memory.Index, entry);
            }

            // Add to the appropriate Z layer strip.
            this.Rows[entry.RowIndex].ChipBatch.AddOrUpdateChipEntry(entry);
            DirtyRows.Add(entry.RowIndex);

            // And track the row it's placed into.
            this.MemoryIndexToRowIndex[entry.Memory.Index] = entry.RowIndex;
        }

        public ChipBatchEntry? GetChipEntry(MapObjectMemory memory)
            => ChipsByIndex.GetValueOrDefault(memory.Index);

        public void RemoveChipEntry(MapObjectMemory memory)
        {
            var entry = GetChipEntry(memory);

            // First remove the reference at the top level.
            this.ChipsByIndex.Remove(memory.Index);

            this.MemoryIndexToRowIndex.Remove(memory.Index);

            if (entry != null)
            {
                // Now remove it from the Z layer strip it's in.
                var row = this.Rows[entry.RowIndex];
                row.ChipBatch.RemoveChipEntry(entry);
                this.DeadEntries.Push(entry);
                DirtyRows.Add(entry.RowIndex);
            }
        }

        public void OnThemeSwitched(TileAtlas tileAtlas, TileAtlas chipAtlas, ICoords coords)
        {
            TileAtlas = tileAtlas;
            ChipAtlas = chipAtlas;
            Coords = coords;
            
            this.Clear();

            foreach (var row in Rows)
            {
                row.OnThemeSwitched(TileAtlas, ChipAtlas, coords);
            }
        }

        public void Clear()
        {
            this.DeadEntries.Clear();
            this.RedrawAll = true;

            foreach (var row in Rows)
            {
                row.Clear();
            }
        }

        public void SetTile(int x, int y, string tile)
        {
            Tiles[y * TiledWidth + x] = tile;
            DirtyRows.Add(y);
        }

        public void UpdateBatches()
        {
            if (RedrawAll)
            {
                for (int y = 0; y < Rows.Length; y++)
                {
                    var row = Rows[y];
                    row.UpdateTileBatches(Tiles, y * TiledWidth, TiledWidth);
                    row.UpdateChipBatch();
                }
            }
            else
            {
                foreach (int y in DirtyRows)
                {
                    var row = Rows[y];
                    row.UpdateTileBatches(Tiles, y * TiledWidth, TiledWidth);
                    row.UpdateChipBatch();
                }
            }
            RedrawAll = false;
            DirtyRows.Clear();
        }

        public override void Update(float dt)
        {
            foreach (var row in Rows)
            {
                row.Update(dt);
            }
        }

        public override void Draw()
        {
            for (int tileY = 0; tileY < Rows.Length; tileY++)
            {
                var row = Rows[tileY];
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
            
            TileWidth = Coords.TileWidth;
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

        internal void UpdateTileBatches(string[] tiles, int startIndex, int widthInTiles)
        {
            ScreenWidth = widthInTiles * TileWidth;
            TileBatch.Clear();
            TileOverhangBatch.Clear();
            HasOverhang = false;
            
            for (int x = 0; x < widthInTiles; x++)
            {
                var index = startIndex + x;
                var tileId = tiles[index];
                var tile = TileAtlas.GetTile(tileId);
                if (tile == null)
                {
                    Logger.Error($"Missing tile {tileId}");
                }
                else
                {
                    Coords.TileToScreen(x, RowYIndex, out var screenX, out var screenY);
                    TileBatch.Add(tile.Quad, screenX, screenY);

                    if (tile.HasOverhang)
                    {
                        HasOverhang = true;
                        TileOverhangBatch.Add(tile.Quad, screenX, screenY);
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
                var overhangHeight = Coords.TileHeight / 4;
                Love.Graphics.SetScissor(screenX, screenY + RowYIndex * Coords.TileHeight - overhangHeight, ScreenWidth, overhangHeight);
                GraphicsEx.DrawSpriteBatch(TileOverhangBatch, screenX, screenY - overhangHeight);
                Love.Graphics.SetScissor();
            }

            Love.Graphics.Draw(TileBatch, screenX, screenY);
            ChipBatch.Draw(screenX, screenY);
        }
    }
}
