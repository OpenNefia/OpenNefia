using OpenNefia.Core.Maths;
using System;

namespace OpenNefia.Core.UI.Element
{
    public interface IDrawable : IDisposable
    {
        UIBox2i GlobalPixelBounds { get; }

        public Vector2i GlobalPixelPosition { get; }
        public Vector2i PixelSize { get; }

        int Width { get; }
        int Height { get; }

        int X { get; }
        int Y { get; }

        void SetSize(int width, int height);
        void SetPosition(int x, int y);

        bool ContainsPoint(Vector2 point);

        void Update(float dt);
        void Draw();
    }
}
