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
        
        protected IMap? _map;

        public virtual void Initialize() { }
        public virtual void OnThemeSwitched() { }
        public virtual void SetMap(IMap map) => _map = map;
        public virtual void RedrawAll() {}
        public virtual void RedrawDirtyTiles(HashSet<Vector2i> dirtyTilesThisTurn) {}
    }
}
