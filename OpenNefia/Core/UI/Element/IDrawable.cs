using OpenNefia.Core.Maths;
using System;

namespace OpenNefia.Core.UI.Element
{
    public interface IDrawable : IDisposable
    {
        Box2i Bounds { get; }

        public Vector2i Position { get; }
        public Vector2i Size { get; }

        int Width { get; }
        int Height { get; }

        int X { get; }
        int Y { get; }

        void SetSize(int width, int height);
        void SetPosition(int x, int y);

        bool ContainsPoint(Vector2i point);

        void Update(float dt);
        void Draw();
    }
}
