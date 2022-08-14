using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Rendering.TileDrawLayers;
using System;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maths;
using static OpenNefia.Core.Rendering.AssetInstance;

namespace OpenNefia.Content.Maps.Debris
{
    [RegisterTileLayer(renderAfter: new[] { typeof(TileAndChipTileLayer) })]
    public sealed class MapDebrisTileLayer : BaseTileLayer
    {
        [Dependency] private readonly ICoords _coords = default!;

        private IAssetInstance _bloodAsset = default!;
        private IAssetInstance _fragmentAsset = default!;

        private Love.SpriteBatch? _bloodBatch = null;
        private Love.SpriteBatch? _fragmentBatch = null;

        public override void Initialize()
        {
            _bloodAsset = Assets.GetAsset(Protos.Asset.DebrisBlood);
            _fragmentAsset = Assets.GetAsset(Protos.Asset.DebrisFragment);
        }

        public override void OnThemeSwitched()
        {
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
            _bloodBatch?.Dispose();
            _bloodBatch = null;
            _fragmentBatch?.Dispose();
            _fragmentBatch = null;

            if (Map == null || !EntityManager.TryGetComponent<MapDebrisComponent>(Map.MapEntityUid, out var mapDebris))
                return;

            var bloodParts = new List<AssetBatchPart>();
            var fragmentParts = new List<AssetBatchPart>();

            for (var x = 0; x < Map.Width; x++)
            {
                for (var y = 0; y < Map.Height; y++)
                {
                    var debris = mapDebris.DebrisMemory[x, y];
                    var screenPos = _coords.TileToScreen((x, y));

                    if (debris.Blood > 0)
                    {
                        bloodParts.Add(new AssetBatchPart((debris.Blood - 1).ToString(), screenPos.X, screenPos.Y));
                    }
                    if (debris.Fragments > 0)
                    {
                        fragmentParts.Add(new AssetBatchPart((debris.Fragments - 1).ToString(), screenPos.X, screenPos.Y));
                    }
                }
            }

            _bloodBatch = _bloodAsset.MakeBatch(bloodParts);
            _fragmentBatch = _fragmentAsset.MakeBatch(fragmentParts);
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Color.White);

            if (_bloodBatch != null)
                Love.Graphics.Draw(_bloodBatch, PixelX, PixelY);

            if (_fragmentBatch != null)
                Love.Graphics.Draw(_fragmentBatch, PixelX, PixelY);
        }
    }
}
