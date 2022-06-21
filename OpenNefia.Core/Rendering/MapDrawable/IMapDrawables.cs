using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// Displays and updates animatable graphics in worldspace.
    /// </summary>
    public interface IMapDrawables : IDrawable
    {
        void Clear();
        void Enqueue(IMapDrawable drawable, MapCoordinates pos, int zOrder = 0);
        void Enqueue(IMapDrawable drawable, EntityUid ent, int zOrder = 0);
        bool HasActiveDrawables();
        void WaitForDrawables();
    }
}