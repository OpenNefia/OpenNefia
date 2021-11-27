using OpenNefia.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering.TileDrawLayers
{
    public class ShadowTileLayer : BaseTileLayer
    {
        private InstancedMap Map;
        private ICoords Coords;
        private ShadowBatch Batch;

        public ShadowTileLayer(InstancedMap map)
        {
            this.Map = map;
            this.Coords = GraphicsEx.Coords;
            this.Batch = new ShadowBatch(map.Width, map.Height, Coords);
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
            this.Batch.SetAllTileShadows(Map._ShadowMap.ShadowTiles, Map._ShadowMap.ShadowBounds);
            this.Batch.UpdateBatches();
        }

        public override void RedrawDirtyTiles(HashSet<int> dirtyTilesThisTurn)
        {
            this.Batch.SetAllTileShadows(Map._ShadowMap.ShadowTiles, Map._ShadowMap.ShadowBounds);
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
