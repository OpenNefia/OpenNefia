using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.Rendering.TileRowDrawLayers;
using OpenNefia.Core.UI.Element;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.Rendering
{
    public interface IMapTileRowRenderer
    {
        void OnThemeSwitched();
        void RegisterTileLayers();
        void RefreshAllLayers();
        void SetMap(IMap map);
        IEnumerable<ITileRowLayer> GetTileRowLayers(TileRowLayerType type);
        bool TryGetTileRowLayer<T>([NotNullWhen(true)] out T? layer) where T : class, ITileRowLayer;
    }
}