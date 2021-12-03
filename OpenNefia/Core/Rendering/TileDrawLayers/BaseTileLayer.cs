using OpenNefia.Core.Maps;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering.TileDrawLayers
{
    public abstract class BaseTileLayer : BaseDrawable, ITileLayer
    {
        public virtual void Initialize() { }
        public abstract void SetMap(IMap map);
        public abstract void RedrawAll();
        public abstract void RedrawDirtyTiles(HashSet<MapCoordinates> dirtyTilesThisTurn);
    }
}
