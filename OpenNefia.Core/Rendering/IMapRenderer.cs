using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    public interface IMapRenderer : IDrawable
    {
        void Initialize();
        void RegisterTileLayers();
        void RefreshAllLayers();
        void SetMap(IMap map);
        void SetTileLayerEnabled<T>(bool enabled) where T : ITileLayer;
        void SetTileLayerEnabled(Type type, bool enabled);
        T GetTileLayer<T>() where T : ITileLayer;
    }
}