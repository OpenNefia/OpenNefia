using Melanchall.DryWetMidi.MusicTheory;
using OpenNefia.Core.Game;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Rendering
{
    internal class ChipBatch
    {
        private class ChipBatchDrawable
        {
            public ChipBatchDrawable(Love.Drawable spriteBatch)
            {
                SpriteBatch = spriteBatch;
            }

            public ChipBatchDrawable(IEntityDrawable drawable, Vector2i tilePosition, Vector2 screenOffset, Vector2 scrollOffset)
            {
                Drawable = drawable;
                TilePosition = tilePosition;
                ScreenOffset = screenOffset;
                ScrollOffset = scrollOffset;
            }

            public Love.Drawable? SpriteBatch { get; }
            public IEntityDrawable? Drawable { get; }
            public Vector2i TilePosition { get; }
            public Vector2 ScreenOffset { get; }
            public Vector2 ScrollOffset { get; }
        }

        internal PriorityMap<int, ChipBatchEntry, int> ByIndex = new PriorityMap<int, ChipBatchEntry, int>();
        private List<ChipBatchDrawable> ToDraw = new();
        private List<Love.SpriteBatch> SpriteBatches = new();
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
            foreach (var batch in SpriteBatches)
            {
                batch.Clear();
            }
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
                var screenPos = Coords.TileToScreen(memory.Coords.Position);

                if (entry.AtlasTile != null)
                {
                    if (currentBatch == null)
                    {
                        currentBatch = GetOrCreateSpriteBatch(i);
                        i += 1;
                        ToDraw.Add(new(currentBatch));
                    }

                    currentBatch.SetColor(memory.Color.R,
                        memory.Color.G,
                        memory.Color.B,
                        memory.Color.A);

                    var tile = entry.AtlasTile;
                    var finalPos = screenPos + memory.ScreenOffset + entry.ScrollOffset;

                    var rect = tile.Quad.GetViewport();
                    currentBatch.Add(tile.Quad,
                        finalPos.X + (rect.Width / 2),
                        finalPos.Y + tile.YOffset + (rect.Height / 2),
                        memory.Rotation,
                        1,
                        1,
                        rect.Width / 2,
                        rect.Height / 2);
                }
                if (memory.Drawables.Count > 0)
                {
                    if (currentBatch != null)
                    {
                        currentBatch.SetColor(1, 1, 1, 1);
                        currentBatch.Flush();
                        currentBatch = null;
                    }

                    foreach (var drawable in memory.Drawables)
                        ToDraw.Add(new(drawable, memory.Coords.Position, memory.ScreenOffset, entry.ScrollOffset));
                }
            }

            if (currentBatch != null)
            {
                currentBatch.SetColor(1, 1, 1, 1);
                currentBatch.Flush();
            }

            NeedsRedraw = false;
        }

        private HashSet<IEntityDrawable> _seen = new();

        public void Update(float dt)
        {
            // TODO chip animations
            _seen.Clear();
            foreach (var entry in ToDraw)
            {
                // The same drawable can be added to multiple memories; ensure
                // they're only updated once.
                if (entry.Drawable != null && !_seen.Contains(entry.Drawable))
                {
                    entry.Drawable.Update(dt);
                    _seen.Add(entry.Drawable);
                }
            }
        }

        public void Draw(int screenX, int screenY, float tileScale)
        {
            foreach (var entry in ToDraw)
            {
                if (entry.SpriteBatch != null)
                {
                    Love.Graphics.Draw(entry.SpriteBatch, screenX, screenY, 0, tileScale, tileScale);
                }

                if (entry.Drawable != null)
                {
                    var pos = Coords.TileToScreen(entry.TilePosition) * tileScale + entry.ScreenOffset + entry.ScrollOffset;
                    entry.Drawable.Draw(tileScale, screenX + pos.X, screenY + pos.Y);
                }
            }
        }
    }
}