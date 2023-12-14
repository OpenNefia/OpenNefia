using Melanchall.DryWetMidi.MusicTheory;
using OpenNefia.Core.Game;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Rendering
{
    internal class ChipBatch
    {
        private class ChipBatchDrawable
        {
            public ChipBatchDrawable(TrackedSpriteBatch spriteBatch)
            {
                Batch = spriteBatch;
            }

            public ChipBatchDrawable(IEntityDrawable drawable, Vector2i tilePosition, Vector2 screenOffset, Vector2 scrollOffset)
            {
                Drawable = drawable;
                TilePosition = tilePosition;
                ScreenOffset = screenOffset;
                ScrollOffset = scrollOffset;
            }

            public TrackedSpriteBatch? Batch { get; }
            public IEntityDrawable? Drawable { get; }
            public Vector2i TilePosition { get; }
            public Vector2 ScreenOffset { get; }
            public Vector2 ScrollOffset { get; }
        }

        /// <summary>
        /// Tracks a sprite added to a <see cref="Love.SpriteBatch"/> for smooth scrolling.
        /// </summary>
        private class TrackedSprite
        {
            public TrackedSprite(int index, Love.Quad quad, Vector2 position, Vector2 targetPosition, Vector2 lerpSpeed)
            {
                Index = index;
                Quad = quad;
                Position = position;
                TargetPosition = targetPosition;
                LerpSpeed = lerpSpeed;
            }

            /// <summary>
            /// Index in the <see cref="Love.SpriteBatch"/>.
            /// </summary>
            public int Index { get; }

            /// <summary>
            /// Quad used for this sprite.
            /// </summary>
            public Love.Quad Quad { get; }

            /// <summary>
            /// Current position of the sprite.
            /// </summary>
            public Vector2 Position { get; set; }

            /// <summary>
            /// Target position the sprite will lerp towards.
            /// </summary>
            public Vector2 TargetPosition { get; set; }

            /// <summary>
            /// Speed of linear interpolation.
            /// </summary>
            public Vector2 LerpSpeed { get; set; }
        }

        private class TrackedSpriteBatch
        {
            public Love.SpriteBatch SpriteBatch { get; }
            public int Index { get; }
            public List<TrackedSprite> Sprites { get; } = new();

            public TrackedSpriteBatch(Love.SpriteBatch spriteBatch, int index)
            {
                SpriteBatch = spriteBatch;
                Index = index;
            }

            public void Add(Love.Quad quad, Vector2 startPos, Vector2 endPos, Vector2 lerpSpeed, float angle = 0, float sx = 1, float sy = 1, float ox = 0, float oy = 0, float kx = 0, float ky = 0)
            {
                var index = SpriteBatch.Add(quad, startPos.X, startPos.Y, angle, sx, sy, ox, oy, kx, ky);
                Sprites.Add(new TrackedSprite(index, quad, startPos, endPos, lerpSpeed));
            }

            public void Clear()
            {
                SpriteBatch.Clear();
                Sprites.Clear();
            }
        }

        internal PriorityMap<int, ChipBatchEntry, int> ByIndex = new PriorityMap<int, ChipBatchEntry, int>();
        private List<ChipBatchDrawable> ToDraw = new();
        private List<TrackedSpriteBatch> SpriteBatches = new();
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

        private TrackedSpriteBatch GetOrCreateSpriteBatch(int i)
        {
            if (i < this.SpriteBatches.Count)
                return this.SpriteBatches[i]!;

            var batch = Love.Graphics.NewSpriteBatch(this.Atlas.Image, 2048, Love.SpriteBatchUsage.Dynamic);
            var tracked = new TrackedSpriteBatch(batch, i);
            this.SpriteBatches.Add(tracked);
            return tracked;
        }

        private Vector2 GetScreenPos(MapCoordinates coords, ChipBatchEntry entry)
        {
            var screenPos = Coords.TileToScreen(coords.Position);

            var tile = entry.AtlasTile;
            var finalPos = screenPos + entry.Memory.ScreenOffset + entry.ScrollOffset;

            var rect = tile!.Quad.GetViewport();
            var posX = finalPos.X;
            var posY = finalPos.Y + tile.YOffset;
            return new Vector2(posX, posY);
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
            TrackedSpriteBatch? currentBatch = null;
            foreach (var (index, entry) in ByIndex)
            {
                var memory = entry.Memory;

                if (entry.AtlasTile != null)
                {
                    if (currentBatch == null)
                    {
                        currentBatch = GetOrCreateSpriteBatch(i);
                        i += 1;
                        ToDraw.Add(new(currentBatch));
                    }

                    currentBatch.SpriteBatch.SetColor(memory.Color.R,
                        memory.Color.G,
                        memory.Color.B,
                        memory.Color.A);

                    var quad = entry.AtlasTile.Quad;
                    var rect = quad.GetViewport();

                    Vector2 startPos, endPos, lerpSpeed;
                    if (memory.Coords.MapId == memory.PreviousCoords.MapId && memory.Coords != memory.PreviousCoords)
                    {
                        startPos = GetScreenPos(memory.PreviousCoords, entry);
                        endPos = GetScreenPos(memory.Coords, entry);
                        lerpSpeed = (endPos - startPos) / 10f;
                    }
                    else
                    {
                        startPos = GetScreenPos(memory.Coords, entry);
                        endPos = startPos + (rect.Width / 2, rect.Height / 2);
                        lerpSpeed = Vector2.Zero;
                    }
                    memory.PreviousCoords = memory.Coords;

                    currentBatch.Add(quad,
                        startPos,
                        endPos,
                        lerpSpeed,
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
                        currentBatch.SpriteBatch.SetColor(1, 1, 1, 1);
                        currentBatch.SpriteBatch.Flush();
                        currentBatch = null;
                    }

                    foreach (var drawable in memory.Drawables)
                        ToDraw.Add(new(drawable, memory.Coords.Position, memory.ScreenOffset, entry.ScrollOffset));
                }
            }

            if (currentBatch != null)
            {
                currentBatch.SpriteBatch.SetColor(1, 1, 1, 1);
                currentBatch.SpriteBatch.Flush();
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
                if (entry.Batch != null)
                {
                    foreach (var sprite in entry.Batch.Sprites)
                    {
                        if (!sprite.Position.EqualsApprox(sprite.TargetPosition, 0.01))
                        {
                            sprite.Position = sprite.Position + sprite.LerpSpeed / 2;
                            entry.Batch.SpriteBatch.Set(sprite.Index, sprite.Quad, sprite.Position.X, sprite.Position.Y);
                        }
                    }
                }

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
                if (entry.Batch != null)
                {
                    Love.Graphics.Draw(entry.Batch.SpriteBatch, screenX, screenY, 0, tileScale, tileScale);
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