using OpenNefia.Core.Maps;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering.TileDrawLayers
{
    public interface ITileLayer : IDrawable
    {
        void Initialize();
        void SetMap(IMap map);
        void RedrawAll();
        void RedrawDirtyTiles(HashSet<MapCoordinates> dirtyTilesThisTurn);
    }
}
