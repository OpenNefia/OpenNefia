using Melanchall.DryWetMidi.MusicTheory;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Element
{
    public abstract class BaseDrawable : IDrawable
    {
        /// <inheritdoc/>
        public virtual Vector2 Size { get; internal set; }

        /// <inheritdoc/>
        public virtual Vector2 Position { get; internal set; }

        /// <inheritdoc/>
        public Vector2i PixelSize => (Vector2i)(Size * UIScale);

        /// <inheritdoc/>
        public Vector2i PixelPosition => (Vector2i)(Position * UIScale);

        /// <inheritdoc/>
        public Vector2i TileSize => (Vector2i)(Size * TileScale);

        /// <inheritdoc/>
        public Vector2i TilePosition => (Vector2i)(Position * TileScale);

        /// <inheritdoc/>
        public UIBox2 Rect => UIBox2.FromDimensions(Position, Size);

        /// <inheritdoc/>
        public UIBox2i PixelRect => UIBox2i.FromDimensions(PixelPosition, PixelSize);

        /// <inheritdoc/>
        public UIBox2i TileRect => UIBox2i.FromDimensions(TilePosition, TileSize);

        /// <inheritdoc/>
        public UIBox2 SizeBox => new(Vector2.Zero, Size);

        /// <inheritdoc/>
        public UIBox2i PixelSizeBox => new(Vector2i.Zero, PixelSize);

        /// <inheritdoc/>
        public UIBox2i TileSizeBox => new(Vector2i.Zero, TileSize);

        /// <inheritdoc/>
        public float Width => Size.X;

        /// <inheritdoc/>
        public float Height => Size.Y;

        /// <inheritdoc/>
        public int PixelWidth => PixelSize.X;

        /// <inheritdoc/>
        public int PixelHeight => PixelSize.Y;

        /// <inheritdoc/>
        public int TileWidth => TileSize.X;

        /// <inheritdoc/>
        public int TileHeight => TileSize.Y;

        /// <inheritdoc/>
        public float X => Position.X;

        /// <inheritdoc/>
        public float Y => Position.Y;

        /// <inheritdoc/>
        public int PixelX => PixelPosition.X;

        /// <inheritdoc/>
        public int PixelY => PixelPosition.Y;

        /// <inheritdoc/>
        public int TileX => TilePosition.X;

        /// <inheritdoc/>
        public int TileY => TilePosition.Y;

        /// <inheritdoc/>
        public virtual float UIScale => 1f;

        /// <inheritdoc/>
        public virtual float TileScale => 1f;

        /// <inheritdoc/>
        public virtual void SetSize(float width, float height)
        {
            Size = new(width, height);
        }

        /// <inheritdoc/>
        public virtual void SetPosition(float x, float y)
        {
            Position = new(x, y);
        }

        /// <inheritdoc/>
        public bool ContainsPoint(float x, float y)
        {
            return ContainsPoint(new Vector2(x, y));
        }

        /// <inheritdoc/>
        public virtual bool ContainsPoint(Vector2 point)
        {
            return Rect.Contains(point);
        }

        // TODO make these protected internal.
        public abstract void Update(float dt);
        public abstract void Draw();

        public virtual void Dispose()
        {
        }
    }
}
