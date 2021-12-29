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
        private UIBox2i _globalPixelBounds;
        public UIBox2i GlobalPixelBounds { get => _globalPixelBounds; }
        
        /// <summary>
        /// Size of the drawable element.
        /// </summary>
        public Vector2i PixelSize { get => GlobalPixelBounds.Size; }

        /// <summary>
        /// Absolute position of the drawable element.
        /// </summary>
        public Vector2i GlobalPixelPosition { get => GlobalPixelBounds.TopLeft; }

        public int Width { get => GlobalPixelBounds.Width; }
        public int Height { get => GlobalPixelBounds.Height; }
        public int X { get => GlobalPixelBounds.Left; }
        public int Y { get => GlobalPixelBounds.Top; }

        public virtual void SetSize(int width, int height)
        {
            _globalPixelBounds = UIBox2i.FromDimensions(X, Y, width, height);
        }

        public virtual void SetPosition(int x, int y)
        {
            _globalPixelBounds = UIBox2i.FromDimensions(x, y, Width, Height);
        }

        public bool ContainsPoint(int x, int y)
        {
            return ContainsPoint(new Vector2(x, y));
        }

        public bool ContainsPoint(Vector2 point)
        {
            return GlobalPixelBounds.Contains((Vector2i)point);
        }

        public abstract void Update(float dt);
        public abstract void Draw();

        public virtual void Dispose()
        {
        }
    }
}
