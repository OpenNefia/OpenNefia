using OpenNefia.Core.Maths;
using System;

namespace OpenNefia.Core.UI.Element
{
    public interface IDrawable : IDisposable
    {
        int Width { get; }
        int Height { get; }
        Vector2i Size => new Vector2i(Width, Height);

        int X { get; }
        int Y { get; }
        Vector2i Position => new Vector2i(X, Y);

        void SetSize(int width, int height);
        void SetPosition(int x, int y);
        bool ContainsPoint(int x, int y);

        void Update(float dt);
        void Draw();
    }
}
