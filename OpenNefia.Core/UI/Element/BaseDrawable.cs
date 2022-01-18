using Melanchall.DryWetMidi.MusicTheory;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NetVips.Enums;

namespace OpenNefia.Core.UI.Element
{
    public abstract class BaseDrawable : IDrawable
    {
        /// <inheritdoc/>
        public Vector2 Size { get; internal set; }

        /// <inheritdoc/>
        public Vector2 Position { get; internal set; }

        /// <inheritdoc/>
        public Vector2i PixelSize => (Vector2i)(Size * UIScale);

        /// <inheritdoc/>
        public Vector2i PixelPosition { get => PixelRect.TopLeft; }

        /// <inheritdoc/>
        public UIBox2 Rect => UIBox2.FromDimensions(Position, Size);

        /// <inheritdoc/>
        public UIBox2i PixelRect => UIBox2i.FromDimensions(PixelPosition, PixelSize);

        /// <inheritdoc/>
        public UIBox2 SizeBox => new(Vector2.Zero, Size);

        /// <inheritdoc/>
        public UIBox2i PixelSizeBox => new(Vector2i.Zero, PixelSize);

        /// <inheritdoc/>
        public float Width => Size.X;

        /// <inheritdoc/>
        public float Height => Size.Y;

        /// <inheritdoc/>
        public int PixelWidth => PixelSize.X;

        /// <inheritdoc/>
        public int PixelHeight => PixelSize.Y;

        /// <inheritdoc/>
        public float X => Position.X;

        /// <inheritdoc/>
        public float Y => Position.Y;

        /// <inheritdoc/>
        public int PixelX => PixelPosition.X;

        /// <inheritdoc/>
        public int PixelY => PixelPosition.Y;

        /// <inheritdoc/>
        public virtual float UIScale => 1f;

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
        public bool ContainsPoint(Vector2 point)
        {
            return Rect.Contains(point);
        }

        public abstract void Update(float dt);
        public abstract void Draw();

        public virtual void Dispose()
        {
        }
    }
}
