using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.Rendering.TileRowDrawLayers;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    public interface IMapTileRowRenderer
    {
        void OnThemeSwitched();
        void RegisterTileLayers();
        void RefreshAllLayers();
        void SetMap(IMap map);
        IEnumerable<ITileRowLayer> GetTileRowLayers(TileRowLayerType type);
    }
}