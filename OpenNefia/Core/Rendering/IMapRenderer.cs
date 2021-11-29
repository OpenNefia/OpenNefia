using OpenNefia.Core.Maps;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    internal interface IMapRenderer : IDrawable
    {
        void OnThemeSwitched();
        void RefreshAllLayers();
        void SetMap(IMap map);
    }
}