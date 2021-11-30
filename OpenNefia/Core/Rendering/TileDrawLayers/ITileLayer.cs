using OpenNefia.Core.Maps;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering.TileDrawLayers
{
    public interface ITileLayer : IDrawable
    {
        void OnThemeSwitched();
        void RedrawAll();
        void RedrawDirtyTiles(HashSet<MapCoordinates> dirtyTilesThisTurn);
    }
}
