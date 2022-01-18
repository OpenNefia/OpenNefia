using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Tests
{
    public class DummyDrawable : IDrawable
    {
        public UIBox2i PixelRect => UIBox2i.FromDimensions(PixelPosition, PixelSize);
        public Vector2i PixelPosition => Vector2i.Zero;
        public Vector2i PixelSize => new(Width, Height);
        public int Width => 800;
        public int Height => 600;
        public int X => 0;
        public int Y => 0;

        public bool IsLocalized => false;

        public bool ContainsPoint(Vector2 point)
        {
            return PixelRect.Contains((int)point.X, (int)point.Y);
        }

        public void Dispose()
        {
        }

        public void Draw()
        {
        }

        public void GetPreferredBounds(out UIBox2i bounds)
        {
            bounds = PixelRect;
        }

        public void GetPreferredSize(out Vector2i size)
        {
            size = PixelSize;
        }

        public void SetPosition(int x, int y)
        {
        }

        public void SetSize(int width, int height)
        {
        }

        public void Update(float dt)
        {
        }
    }
}