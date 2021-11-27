using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;

namespace OpenNefia.Core.Rendering
{
    public class MapRenderer : BaseDrawable
    {
        private List<ITileLayer> TileLayers = new List<ITileLayer>();
        private InstancedMap Map;

        public MapRenderer(InstancedMap map)
        {
            Map = map;
            TileLayers.Add(new TileAndChipTileLayer(map));
            TileLayers.Add(new ShadowTileLayer(map));
            RefreshAllLayers();
        }

        public void SetMap(InstancedMap map)
        {
            Map = map;
            TileLayers.Clear();
            TileLayers.Add(new TileAndChipTileLayer(map));
            TileLayers.Add(new ShadowTileLayer(map));
            RefreshAllLayers();
        }

        public void OnThemeSwitched()
        {
            foreach (var layer in TileLayers)
            {
                layer.OnThemeSwitched();
            }
        }

        public void RefreshAllLayers()
        {
            if (this.Map._RedrawAllThisTurn)
            {
                foreach (var layer in TileLayers)
                {
                    layer.RedrawAll();
                }
            }
            else if(this.Map._DirtyTilesThisTurn.Count > 0)
            {
                foreach (var layer in TileLayers)
                {
                    layer.RedrawDirtyTiles(this.Map._DirtyTilesThisTurn);
                }
            }

            this.Map._RedrawAllThisTurn = false;
            this.Map._DirtyTilesThisTurn.Clear();
            this.Map._MapObjectMemory.Flush();
        }

        public override void SetSize(int width = 0, int height = 0)
        {
            base.SetSize(width, height);
            foreach (var layer in this.TileLayers)
            {
                layer.SetSize(width, height);
            }
        }

        public override void SetPosition(int x = 0, int y = 0)
        {
            base.SetPosition(x, y);
            foreach (var layer in this.TileLayers)
            {
                layer.SetPosition(x, y);
            }
        }

        public override void Update(float dt)
        {
            foreach (var layer in TileLayers)
            {
                layer.Update(dt);
            }
        }

        public override void Draw()
        {
            foreach (var layer in TileLayers)
            {
                layer.Draw();
            }
        }
    }
}
