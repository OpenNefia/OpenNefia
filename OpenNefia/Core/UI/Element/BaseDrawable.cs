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
        public Vector2i Position { get => Bounds.TopLeft; }

        public int Width { get => Bounds.Width; }
        public int Height { get => Bounds.Height; }
        public int X { get => Bounds.Left; }
        public int Y { get => Bounds.Top; }

        public virtual void SetSize(int width, int height)
        {
            _bounds = Box2i.FromDimensions(X, Y, width, height);
        }

        public virtual void SetPosition(int x, int y)
        {
            _bounds = Box2i.FromDimensions(x, y, Width, Height);
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
