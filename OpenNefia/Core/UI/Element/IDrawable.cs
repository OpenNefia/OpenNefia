using OpenNefia.Core.Maths;
using System;

namespace OpenNefia.Core.UI.Element
{
    public interface IDrawable : IDisposable
    {
        Box2i Bounds { get; }

        public Vector2i Size { get; }

        public Vector2i BottomLeft { get; }
        public Vector2i TopRight { get; }

        public Vector2i BottomRight { get; }
        public Vector2i TopLeft { get; }

        int Width { get; }
        int Height { get; }

        int Left { get; }
        int Top { get; }
        int Right { get; }
        int Bottom { get; }

        void SetSize(Vector2i pos);
        void SetSize(int width, int height);
        void SetPosition(Vector2i size);
        void SetPosition(int x, int y);

        bool ContainsPoint(Vector2i point);

        void Update(float dt);
        void Draw();
    }
}
