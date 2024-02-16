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
    public sealed class MapDebrisTileRowLayer : BaseTileRowLayer
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

            for (var y = 0; y < map.Height; y++)
            {
                _bloodBatches[y] = _bloodAsset.MakeSpriteBatch(usage: Love.SpriteBatchUsage.Dynamic);
                _fragmentBatches[y] = _fragmentAsset.MakeSpriteBatch(usage: Love.SpriteBatchUsage.Dynamic);
            }
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

            var bloodParts = new List<AssetBatchPart>();
            var fragmentParts = new List<AssetBatchPart>();

            for (var y = 0; y < Map.Height; y++)
            {
                bloodParts.Clear();
                fragmentParts.Clear();

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

                _bloodAsset.UpdateSpriteBatch(_bloodBatches[y], bloodParts);
                _fragmentAsset.UpdateSpriteBatch(_fragmentBatches[y], fragmentParts);
            }
        }

        public override void Update(float dt)
        {
        }

        public override void DrawRow(int tileY, int screenX, int screenY)
        {
            if (tileY < 0 || tileY >= Map!.Height)
                return;

            var scale = _coords.TileScale;
            var tileH = _coords.TileSize.Y;

            Love.Graphics.SetColor(Color.White);

            Love.Graphics.Draw(_bloodBatches[tileY], screenX, screenY + scale * tileH * tileY, 0, scale, scale);
            Love.Graphics.Draw(_fragmentBatches[tileY], screenX, screenY + scale * tileH * tileY, 0, scale, scale);
        }
    }
}
