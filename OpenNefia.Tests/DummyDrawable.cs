using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Tests
{
    public class DummyDrawable : IDrawable
    {
        public UIBox2i PixelRect => UIBox2i.FromDimensions(PixelPosition, PixelSize);
        public Vector2i PixelPosition => Vector2i.Zero;
        public Vector2i PixelSize => (Vector2i)(Size * UIScale);
    {
        public UIBox2i TileRect => UIBox2i.FromDimensions(TilePosition, TileSize);
        public Vector2i TilePosition => Vector2i.Zero;
        public Vector2i TileSize => (Vector2i)(Size * UIScale);

        public float Width => Size.X;
        public float Height => Size.Y;
        public float X => Position.X;
        public float Y => Position.Y;

        public bool IsLocalized => false;

        public UIBox2 Rect => UIBox2.FromDimensions(Position, Size);
        public Vector2 Size => new(800, 600);
        public Vector2 Position => new(0, 0);
        public float UIScale => 1f;
        public float TileScale => 1f;

        public UIBox2 SizeBox => UIBox2.FromDimensions(Vector2.Zero, Size);
        public UIBox2i PixelSizeBox => UIBox2i.FromDimensions(Vector2i.Zero, PixelSize);
        public int PixelWidth => PixelSize.X;
        public int PixelHeight => PixelSize.Y;
        public int PixelX => PixelPosition.X;
        public int PixelY => PixelPosition.Y;

        public UIBox2i TileSizeBox => UIBox2i.FromDimensions(Vector2i.Zero, TileSize);
        public int TileWidth => TileSize.X;
        public int TileHeight => TileSize.Y;
        public int TileX => TilePosition.X;
        public int TileY => TilePosition.Y;

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

        public void GetPreferredBounds(out UIBox2 bounds)
        {
            bounds = Rect;
        }

        public void GetPreferredSize(out Vector2 size)
        {
            size = Size;
        }

        public void SetSize(float width, float height)
        {
        }

        public void SetPosition(float x, float y)
        {
        }

        public void Update(float dt)
        {
        }
    }
}