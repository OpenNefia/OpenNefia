using OpenNefia.Core.Maps;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    public interface IMapRenderer : IDrawable
    {
        void RegisterTileLayers();
        void RefreshAllLayers();
        void SetMap(IMap map);
    }
}