using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maths;
using static OpenNefia.Core.Rendering.AssetInstance;
using OpenNefia.Core.Rendering.TileRowDrawLayers;
using OpenNefia.Core.Maps;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Maps.Debris
{
    [RegisterTileRowLayer(TileRowLayerType.Tile)]
    public sealed class MapDebrisTileLayer : BaseTileRowLayer
    {
        [Dependency] private readonly ICoords _coords = default!;

        private IAssetInstance _bloodAsset = default!;
        private IAssetInstance _fragmentAsset = default!;

        private Love.SpriteBatch[] _bloodBatches = new Love.SpriteBatch[0];
        private Love.SpriteBatch[] _fragmentBatches = new Love.SpriteBatch[0];

        public override void Initialize()
        {
            _bloodAsset = Assets.GetAsset(Protos.Asset.DebrisBlood);
            _fragmentAsset = Assets.GetAsset(Protos.Asset.DebrisFragment);
        }

        public override void OnThemeSwitched()
        {
        }

        public override void SetMap(IMap map)
        {
            base.SetMap(map);
            _bloodBatches = new Love.SpriteBatch[map.Height];
            _fragmentBatches = new Love.SpriteBatch[map.Height];
        }

        public override void RedrawDirtyTiles(HashSet<Vector2i> dirtyTilesThisTurn)
        {
            if (Map == null || !EntityManager.TryGetComponent<MapDebrisComponent>(Map.MapEntityUid, out var mapDebris))
                return;

            foreach (var tile in dirtyTilesThisTurn)
            {
                if (Map.IsMemorized(tile))
                {
                    // TODO MapTileMemorizedEvent?
                    // maybe tile layers should not update component state
                    mapDebris.DebrisMemory[tile.X, tile.Y] = mapDebris.DebrisState[tile.X, tile.Y];
                }
            }

            RebuildBatches();
        }

        public override void RedrawAll()
        {
            if (Map == null || !EntityManager.TryGetComponent<MapDebrisComponent>(Map.MapEntityUid, out var mapDebris))
                return;

            foreach (var tile in Map.AllTiles)
            {
                if (Map.IsMemorized(tile.MapPosition.Position))
                {
                    // TODO MapTileMemorizedEvent?
                    // maybe tile layers should not update component state
                    mapDebris.DebrisMemory[tile.X, tile.Y] = mapDebris.DebrisState[tile.X, tile.Y];
                }
            }
            
            RebuildBatches();
        }

        private void RebuildBatches()
        {
            if (Map == null || !EntityManager.TryGetComponent<MapDebrisComponent>(Map.MapEntityUid, out var mapDebris))
                return;

            foreach (var batch in _bloodBatches)
            {
                if (batch != null)
                    batch.Dispose();
            }
            foreach (var batch in _fragmentBatches)
            {
                if (batch != null)
                    batch.Dispose();
            }

            _bloodBatches = new Love.SpriteBatch[Map.Height];
            _fragmentBatches = new Love.SpriteBatch[Map.Height];

            var bloodParts = new List<AssetBatchPart>();
            var fragmentParts = new List<AssetBatchPart>();

            for (var y = 0; y < Map.Height; y++)
            {
                for (var x = 0; x < Map.Width; x++)
                {
                    var debris = mapDebris.DebrisMemory[x, y];
                    var screenPos = _coords.TileToScreen((x, y));

                    if (debris.Blood > 0)
                    {
                        bloodParts.Add(new AssetBatchPart((debris.Blood - 1).ToString(), screenPos.X, 0));
                    }
                    if (debris.Fragments > 0)
                    {
                        fragmentParts.Add(new AssetBatchPart((debris.Fragments - 1).ToString(), screenPos.X, 0));
                    }
                }

                _bloodBatches[y] = _bloodAsset.MakeBatch(bloodParts);
                _fragmentBatches[y] = _fragmentAsset.MakeBatch(fragmentParts);
            }
        }

        public override void Update(float dt)
        {
        }

        public override void DrawRow(int y, int screenX, int screenY)
        {
            if (y < 0 || y >= Map!.Height)
                return;

            var scale = _coords.TileScale;

            Love.Graphics.SetColor(Color.White);

            Love.Graphics.Draw(_bloodBatches[y], screenX, screenY, 0, scale, scale);
            Love.Graphics.Draw(_fragmentBatches[y], screenX, screenY, 0, scale, scale);
        }
    }
}
