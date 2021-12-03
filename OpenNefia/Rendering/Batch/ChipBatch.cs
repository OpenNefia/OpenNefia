using OpenNefia.Core.Game;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Rendering
{
    internal class ChipBatch
    {
        internal PriorityMap<int, ChipBatchEntry, int> ByIndex = new PriorityMap<int, ChipBatchEntry, int>();
        private List<Love.Drawable> ToDraw = new List<Love.Drawable>();
        private List<Love.SpriteBatch> SpriteBatches = new List<Love.SpriteBatch>();
        internal bool NeedsRedraw = false;

        public TileAtlas Atlas { get; }
        private ICoords Coords;

        public ChipBatch(TileAtlas atlas, ICoords coords)
        {
            this.Atlas = atlas;
            this.Coords = coords;
        }

        public void Clear()
        {
            ByIndex.Clear();
            ToDraw.Clear();
            SpriteBatches.Clear();
            NeedsRedraw = true;
        }

        public void AddOrUpdateChipEntry(ChipBatchEntry entry) 
        {
            if (!ByIndex.ContainsKey(entry.Memory.Index))
            {
                ByIndex.Add(entry.Memory.Index, entry, entry.Memory.ZOrder);
            }
            else
            {
                ByIndex.SetPriority(entry.Memory.Index, entry.Memory.ZOrder);
            }
            NeedsRedraw = true;
        }

        public void RemoveChipEntry(ChipBatchEntry entry)
        {
            if (ByIndex.ContainsKey(entry.Memory.Index))
            {
                ByIndex.Remove(entry.Memory.Index);
            }
            NeedsRedraw = true;
        }

        private Love.SpriteBatch GetOrCreateSpriteBatch(int i)
        {
            if (i < this.SpriteBatches.Count)
                return this.SpriteBatches[i]!;

            var batch = Love.Graphics.NewSpriteBatch(this.Atlas.Image, 2048, Love.SpriteBatchUsage.Dynamic);
            this.SpriteBatches.Add(batch);
            return batch;
        }

        public void UpdateBatches()
        {
            if (!NeedsRedraw)
                return;

            ToDraw.Clear();
            foreach (var spriteBatch in this.SpriteBatches)
            {
                spriteBatch.Clear();
            }

            var i = 0;
            Love.SpriteBatch? currentBatch = null;
            foreach (var (index, entry) in ByIndex)
            {
                var memory = entry.Memory;
                if (memory.IsVisible)
                {
                    if (currentBatch == null)
                    {
                        currentBatch = GetOrCreateSpriteBatch(i);
                        i += 1;
                        ToDraw.Add(currentBatch);
                    }

                    this.Coords.TileToScreen(memory.Coords.Position, out var screenPos);

                    var p = screenPos + memory.ScreenOffset + entry.ScrollOffset;

                    currentBatch.SetColor(memory.Color.R,
                        memory.Color.G,
                        memory.Color.B,
                        memory.Color.A);

                    var tile = entry.AtlasTile;

                    var rect = tile.Quad.GetViewport();
                    currentBatch.Add(tile.Quad,
                        p.X + (rect.Width / 2),
                        p.Y + tile.YOffset + (rect.Height / 2),
                        memory.Rotation,
                        1,
                        1,
                        rect.Width / 2,
                        rect.Height / 2);
                }
            }

            if (currentBatch != null)
            {
                currentBatch.SetColor(1, 1, 1, 1);
                currentBatch.Flush();
            }

            NeedsRedraw = false;
        }

        public void Update(float dt)
        {
            // TODO chip animations
        }

        public void Draw(int screenX, int screenY)
        {
            foreach (var drawable in ToDraw)
            {
                Love.Graphics.Draw(drawable, screenX, screenY);
            }
        }
    }
}
