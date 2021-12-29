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
        private UIBox2i _pixelBounds;
        public UIBox2i PixelBounds { get => _pixelBounds; }

        public Vector2i PixelSize { get => PixelBounds.Size; }
        public Vector2i PixelPosition { get => PixelBounds.TopLeft; }

        public int Width { get => PixelBounds.Width; }
        public int Height { get => PixelBounds.Height; }
        public int X { get => PixelBounds.Left; }
        public int Y { get => PixelBounds.Top; }

        public virtual void SetSize(int width, int height)
        {
            _pixelBounds = UIBox2i.FromDimensions(X, Y, width, height);
        }

        public virtual void SetPosition(int x, int y)
        {
            _pixelBounds = UIBox2i.FromDimensions(x, y, Width, Height);
        }

        public bool ContainsPoint(int x, int y)
        {
            return ContainsPoint(new Vector2(x, y));
        }

        public bool ContainsPoint(Vector2 point)
        {
            return PixelBounds.Contains((Vector2i)point);
        }

        public abstract void Update(float dt);
        public abstract void Draw();

        public virtual void Dispose()
        {
        }
    }
}
