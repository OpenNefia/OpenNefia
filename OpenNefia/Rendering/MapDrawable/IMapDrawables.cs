using OpenNefia.Core.Maps;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    internal interface IMapDrawables : IDrawable
    {
        void Clear();
        void Enqueue(IMapDrawable drawable, MapCoordinates? pos, int zOrder = 0);
        bool HasActiveDrawables();
        void WaitForDrawables();
    }
}