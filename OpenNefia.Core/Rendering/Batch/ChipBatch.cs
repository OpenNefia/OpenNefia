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
            public TrackedSprite(int index, Love.Quad quad, ShadowType shadowType, int? shadowIndex, Love.Quad shadowQuad, Vector2i screenOffset, Vector2 position, Vector2 targetPosition, bool lerp)
            {
                Index = index;
                Quad = quad;
                ShadowType = shadowType;
                ShadowIndex = shadowIndex;
                ShadowQuad = shadowQuad;
                ScreenOffset = screenOffset;
                Position = position;
                StartPosition = position;
                TargetPosition = targetPosition;
                LerpPercentage = lerp ? 0f : 1f;
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
            /// Type of shadow under the entity.
            /// </summary>
            public ShadowType ShadowType { get; }

            /// <summary>
            /// Index in the shadow <see cref="Love.SpriteBatch"/>.
            /// </summary>
            public int? ShadowIndex { get; }

            /// <summary>
            /// Shadow quad used for this sprite.
            /// </summary>
            public Love.Quad ShadowQuad { get; }

            /// <summary>
            /// Screen offset for use with shadows.
            /// </summary>
            public Vector2i ScreenOffset { get; set; }

            /// <summary>
            /// Current position of the sprite.
            /// </summary>
            public Vector2 Position { get; set; }

            /// <summary>
            /// Starting position of the sprite.
            /// </summary>
            public Vector2 StartPosition { get; set; }

            /// <summary>
            /// Target position the sprite will lerp towards.
            /// </summary>
            public Vector2 TargetPosition { get; set; }

            /// <summary>
            /// Percentage of linear interpolation (0f-1f).
            /// </summary>
            public float LerpPercentage { get; set; }
        }

        private class TrackedSpriteBatch
        {
            public Love.SpriteBatch SpriteBatch { get; }
            public int Index { get; }
            public List<TrackedSprite> Sprites { get; } = new();

            private readonly AtlasTile _shadowTile;

            public TrackedSpriteBatch(Love.SpriteBatch spriteBatch, int index, AtlasTile shadowTile)
            {
                SpriteBatch = spriteBatch;
                Index = index;
                _shadowTile = shadowTile;
            }

            public void Add(Love.Quad quad, Vector2 startPos, Vector2 endPos,  bool lerp, ShadowType shadowType, int? shadowIndex, Vector2i screenOffset, float angle = 0, float sx = 1, float sy = 1, float ox = 0, float oy = 0, float kx = 0, float ky = 0)
            {
                var index = SpriteBatch.Add(quad, startPos.X, startPos.Y, angle, sx, sy, ox, oy, kx, ky);
                Sprites.Add(new TrackedSprite(index, quad, shadowType, shadowIndex, _shadowTile.Quad, screenOffset, startPos, endPos, lerp));
            }

            public void Clear()
            {
                SpriteBatch.Clear();
                Sprites.Clear();
            }
        }

        private Love.SpriteBatch _entityShadowBatch;
        internal PriorityMap<int, ChipBatchEntry, int> ByIndex = new PriorityMap<int, ChipBatchEntry, int>();
        private List<ChipBatchDrawable> ToDraw = new();
        private List<TrackedSpriteBatch> SpriteBatches = new();
        internal bool NeedsRedraw = false;

        public TileAtlas Atlas { get; }
        public float ObjectMovementSpeed { get; set; } = 1f;
        private AtlasTile _shadowTile;
        private ICoords _coords;

        public ChipBatch(TileAtlas atlas, ICoords coords, AtlasTile shadowTile, float objMovementSpeed)
        {
            Atlas = atlas;
            _coords = coords;
            _shadowTile = shadowTile;
            _entityShadowBatch = Love.Graphics.NewSpriteBatch(atlas.Image, 2048, Love.SpriteBatchUsage.Dynamic);
            ObjectMovementSpeed = objMovementSpeed;
        }

        public void Clear()
        {
            ByIndex.Clear();
            ToDraw.Clear();
            _entityShadowBatch.Clear();
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
            var tracked = new TrackedSpriteBatch(batch, i, _shadowTile);
            this.SpriteBatches.Add(tracked);
            return tracked;
        }

        private Vector2 GetScreenPos(MapCoordinates coords, ChipBatchEntry entry)
        {
            var screenPos = _coords.TileToScreen(coords.Position);

            var tile = entry.AtlasTile;
            var finalPos = screenPos + entry.Memory.ScreenOffset + entry.ScrollOffset;

            var posX = finalPos.X;
            var posY = finalPos.Y + tile!.YOffset;
            return new Vector2(posX, posY);
        }

        private Vector2 GetShadowOffset(ShadowType type, AtlasTile? tile, Vector2i screenOffset)
        {
            switch (type)
            {
                case ShadowType.None:
                default:
                    return Vector2.Zero;
                case ShadowType.Normal:
                    return new(8, 36);
                case ShadowType.DropShadow:
                    if (tile != null)
                    {
                        var viewport = tile.Quad.GetViewport();
                        var x = screenOffset.X + (viewport.Height / _coords.TileSize.Y) * 8 + 2;
                        var y = screenOffset.Y - 4 - (viewport.Height - _coords.TileSize.Y);
                        return new(x, y);
                    }
                    return Vector2.Zero;
            }
        }

        private int? AddShadow(MapObjectMemory memory, AtlasTile? tile)
        {
            if (!memory.IsVisible)
                return null;

            var screenPos = _coords.TileToScreen(memory.Coords.Position) + GetShadowOffset(memory.ShadowType, tile, memory.ScreenOffset);

            switch (memory.ShadowType)
            {
                case ShadowType.None:
                default:
                    return null;
                case ShadowType.Normal:
                    return _entityShadowBatch.Add(_shadowTile.Quad, screenPos.X, screenPos.Y);
                case ShadowType.DropShadow:
                    if (tile != null)
                    {
                        // TODO no idea what the actual rotation amounts should be
                        // TODO this needs to be rendered as a solid color instead of the texture of the chip
                        return _entityShadowBatch.Add(tile.Quad, screenPos.X, screenPos.Y, angle: memory.ShadowRotationRads);
                    }
                    return null;
            }
        }

        public void UpdateBatches()
        {
            if (!NeedsRedraw)
                return;

            ToDraw.Clear();
            _entityShadowBatch.Clear();
            foreach (var spriteBatch in this.SpriteBatches)
            {
                spriteBatch.Clear();
            }

            var i = 0;
            TrackedSpriteBatch? currentBatch = null;
            foreach (var (index, entry) in ByIndex)
            {
                var memory = entry.Memory;

                int? shadowIndex = null;
                if (memory.IsVisible)
                {
                    shadowIndex = AddShadow(memory, entry.AtlasTile);
                }

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

                    Vector2 startPos, endPos;
                    bool lerp;
                    if (ObjectMovementSpeed > 0.01f && memory.Coords.MapId == memory.PreviousCoords.MapId)
                    {
                        lerp = true;
                        startPos = GetScreenPos(memory.PreviousCoords, entry);
                        endPos = GetScreenPos(memory.Coords, entry);
                    }
                    else
                    {
                        lerp = false;
                        startPos = GetScreenPos(memory.Coords, entry);
                        endPos = startPos;
                    }
                    memory.PreviousCoords = memory.Coords;

                    currentBatch.Add(quad,
                        startPos,
                        endPos,
                        lerp,
                        memory.ShadowType,
                        shadowIndex,
                        memory.ScreenOffset,
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
                        if (ObjectMovementSpeed <= 0.01f)
                        {
                            entry.Batch.SpriteBatch.Set(sprite.Index, sprite.Quad, sprite.TargetPosition.X, sprite.TargetPosition.Y);
                        }
                        else if (sprite.LerpPercentage < 1f)
                        {
                            sprite.LerpPercentage = float.Min(sprite.LerpPercentage + dt / ObjectMovementSpeed, 1f);
                            sprite.Position = new Vector2(
                                MathHelper.Lerp(sprite.StartPosition.X, sprite.TargetPosition.X, sprite.LerpPercentage),
                                MathHelper.Lerp(sprite.StartPosition.Y, sprite.TargetPosition.Y, sprite.LerpPercentage));
                            entry.Batch.SpriteBatch.Set(sprite.Index, sprite.Quad, sprite.Position.X, sprite.Position.Y);

                            if (sprite.ShadowIndex != null)
                            {
                                var offset = GetShadowOffset(sprite.ShadowType, _shadowTile, sprite.ScreenOffset);
                                _entityShadowBatch.Set(sprite.ShadowIndex.Value, sprite.ShadowQuad, sprite.Position.X + offset.X, sprite.Position.Y + offset.Y);
                            }
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
            Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
            Love.Graphics.SetColor(Color.White.WithAlphaB(110));
            Love.Graphics.Draw(_entityShadowBatch, screenX, screenY, 0, tileScale, tileScale);
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            Love.Graphics.SetColor(Color.White);

            foreach (var entry in ToDraw)
            {
                if (entry.Batch != null)
                {
                    Love.Graphics.Draw(entry.Batch.SpriteBatch, screenX, screenY, 0, tileScale, tileScale);
                }

                if (entry.Drawable != null)
                {
                    var pos = _coords.TileToScreen(entry.TilePosition) * tileScale + entry.ScreenOffset + entry.ScrollOffset;
                    entry.Drawable.Draw(tileScale, screenX + pos.X, screenY + pos.Y);
                }
            }
        }
    }
}