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

        public override void SetSize(Vector2i size)
        {
            base.SetSize(size);
            this.Batch.SetSize(size);
        }

        public override void SetPosition(Vector2i pos)
        {
            base.SetPosition(pos);
            this.Batch.SetPosition(pos);
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
