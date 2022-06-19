using OpenNefia.Content.Rendering;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Rendering.TileDrawLayers;

namespace OpenNefia.Content.MapVisibility
{
    [RegisterTileLayer(renderAfter: new[] { typeof(TileAndChipTileLayer) })]
    public sealed class ShadowTileLayer : BaseTileLayer
    {
        [Dependency] private readonly IAssetManager _assetManager = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        private IMap _map = default!;
        private MapVisibilityComponent _mapVis = default!;
        private ShadowBatch _batch = new();

        public override void Initialize()
        {
            _batch.Initialize(_assetManager, _coords);
        }

        public override void OnThemeSwitched()
        {
            _batch.OnThemeSwitched();
        }

        public override void SetMap(IMap map)
        {
            _map = map;
            _mapVis = _entityManager.GetComponent<MapVisibilityComponent>(map.MapEntityUid);
            _batch.SetMapSize(map.Size);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            _batch.SetSize(width, height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            _batch.SetPosition(x, y);
        }

        public override void RedrawAll()
        {
            _batch.SetAllTileShadows(_mapVis.ShadowMap.ShadowTiles, _mapVis.ShadowMap.ShadowBounds);
            _batch.UpdateBatches();
        }

        public override void RedrawDirtyTiles(HashSet<Vector2i> dirtyTilesThisTurn)
        {
            _batch.SetAllTileShadows(_mapVis.ShadowMap.ShadowTiles, _mapVis.ShadowMap.ShadowBounds);
            _batch.UpdateBatches();
        }

        public override void Update(float dt)
        {
            _batch.Update(dt);
        }

        public override void Draw()
        {
            _batch.Draw();
        }
    }
}
