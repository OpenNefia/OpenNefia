using OpenNefia.Core.Game;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Rendering.TileDrawLayers
{
    [RegisterTileLayer(renderAfter: new Type[1] { typeof(TileAndChipTileLayer) })]
    public sealed class ShadowTileLayer : BaseTileLayer
    {
        [Dependency] private readonly IAssetManager _assetManager = default!;
        [Dependency] private readonly ICoords _coords = default!;

        private IMap _map = default!;

        private ShadowBatch _batch = new();

        public override void Initialize()
        {
            _batch.Initialize(_assetManager, _coords);
        }

        public override void SetMap(IMap map)
        {
            _map = map;
            _batch.SetMapSize(map.Size);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            _batch.SetSize(width, height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            _batch.SetPosition(x, y);
        }

        public override void RedrawAll()
        {
            _batch.SetAllTileShadows(_map.ShadowMap._ShadowTiles, _map.ShadowMap._ShadowBounds);
            _batch.UpdateBatches();
        }

        public override void RedrawDirtyTiles(HashSet<MapCoordinates> dirtyTilesThisTurn)
        {
            _batch.SetAllTileShadows(_map.ShadowMap._ShadowTiles, _map.ShadowMap._ShadowBounds);
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
