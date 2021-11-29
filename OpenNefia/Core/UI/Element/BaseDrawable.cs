using Love;
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
        private Box2i _bounds;
        public Box2i Bounds { get => _bounds; }

        public Vector2i Size { get => Bounds.Size; }

        public Vector2i BottomLeft { get => Bounds.BottomLeft; }
        public Vector2i TopRight { get => Bounds.TopRight; }

        public Vector2i BottomRight { get => Bounds.BottomRight; }
        public Vector2i TopLeft { get => Bounds.TopLeft; }

        public int Width { get => Bounds.Width; }
        public int Height { get => Bounds.Height; }

        public int Left { get => Bounds.Left; }
        public int Top { get => Bounds.Top; }
        public int Right { get => Bounds.Right; }
        public int Bottom { get => Bounds.Bottom; }

        public void SetSize(int width, int height)
        {
            SetSize(new Vector2i(width, height));
        }

        public virtual void SetSize(Vector2i size)
        {
            _bounds.Right = _bounds.Left + size.X;
            _bounds.Bottom = _bounds.Top + size.Y;
        }

        public void SetPosition(int x, int y)
        {
            SetPosition(new Vector2i(x, y));
        }

        public virtual void SetPosition(Vector2i pos)
        {
            _bounds.Left = pos.X;
            _bounds.Top = pos.Y;
        }

        public bool ContainsPoint(int x, int y)
        {
            return ContainsPoint(new Vector2i(x, y));
        }

        public bool ContainsPoint(Vector2i point)
        {
            return Bounds.Contains(point);
        }

        public abstract void Update(float dt);
        public abstract void Draw();

        public virtual void Dispose()
        {
        }
    }
}
