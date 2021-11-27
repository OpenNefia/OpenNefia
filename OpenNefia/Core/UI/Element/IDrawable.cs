using System;

namespace OpenNefia.Core.UI.Element
{
    public interface IDrawable : IDisposable
    {
        int Width { get; }
        int Height { get; }
        int X { get; }
        int Y { get; }

        void SetSize(int width, int height);
        void SetPosition(int x, int y);
        bool ContainsPoint(int x, int y);

        void Update(float dt);
        void Draw();
    }
}
