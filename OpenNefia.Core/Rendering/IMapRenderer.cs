using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.UI.Element;
using System.Diagnostics.CodeAnalysis;

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

        /// <summary>
        /// Tries to get a tile layer accounting for headless/unit test modes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tileLayer"></param>
        /// <returns></returns>
        bool TryGetTileLayer<T>([NotNullWhen(true)] out T? tileLayer) where T : class, ITileLayer;
    }
}