using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering.TileDrawLayers
{
    public abstract class BaseTileLayer : BaseDrawable, ITileLayer
    {
        [Dependency] protected readonly IAssetManager Assets = default!;
        [Dependency] protected readonly IEntityManager EntityManager = default!;

        public virtual void Initialize() { }
        public virtual void OnThemeSwitched() { }
        public abstract void SetMap(IMap map);
        public abstract void RedrawAll();
        public abstract void RedrawDirtyTiles(HashSet<Vector2i> dirtyTilesThisTurn);
    }
}
