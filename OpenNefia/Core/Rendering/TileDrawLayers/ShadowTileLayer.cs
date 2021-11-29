using OpenNefia.Core.Game;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Rendering.TileDrawLayers
{
    public class ShadowTileLayer : BaseTileLayer
    {
        private Map Map;
        private ICoords Coords;
        private ShadowBatch Batch;

        public ShadowTileLayer(Map map, IAssetManager assetManager)
        {
            this.Map = map;
            this.Coords = GameSession.Coords;
            this.Batch = new ShadowBatch(map.Size, Coords, assetManager);
        }

        public override void OnThemeSwitched()
        {
        }

        public override void SetSize(int width = 0, int height = 0)
        {
            base.SetSize(width, height);
            this.Batch.SetSize(width, height);
        }

        public override void SetPosition(int x = 0, int y = 0)
        {
            base.SetPosition(x, y);
            this.Batch.SetPosition(x, y);
        }

        public override void RedrawAll()
        {
            this.Batch.SetAllTileShadows(Map.ShadowMap.ShadowTiles, Map.ShadowMap.ShadowBounds);
            this.Batch.UpdateBatches();
        }

        public override void RedrawDirtyTiles(HashSet<MapCoordinates> dirtyTilesThisTurn)
        {
            this.Batch.SetAllTileShadows(Map.ShadowMap.ShadowTiles, Map.ShadowMap.ShadowBounds);
            this.Batch.UpdateBatches();
        }

        public override void Update(float dt)
        {
            this.Batch.Update(dt);
        }

        public override void Draw()
        {
            this.Batch.Draw();
        }
    }
}
