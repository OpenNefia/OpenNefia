using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;

namespace OpenNefia.Core.Rendering
{
    public class MapRenderer : BaseDrawable
    {
        [Dependency] private readonly IAtlasManager _atlasManager;
        [Dependency] private readonly IAssetManager _assetManager;

        private List<ITileLayer> _tileLayers = new List<ITileLayer>();
        private Map _map;

        public MapRenderer(Map map, IAtlasManager atlasManager, IAssetManager assetManager)
        {
            _atlasManager = atlasManager;
            _assetManager = assetManager;

            _map = map;
            _tileLayers.Add(new TileAndChipTileLayer(_map, _atlasManager));
            _tileLayers.Add(new ShadowTileLayer(_map, _assetManager));
            RefreshAllLayers();
        }

        public void SetMap(Map map)
        {
            _map = map;
            _tileLayers.Clear();
            _tileLayers.Add(new TileAndChipTileLayer(_map, _atlasManager));
            _tileLayers.Add(new ShadowTileLayer(_map, _assetManager));
            RefreshAllLayers();
        }

        public void OnThemeSwitched()
        {
            foreach (var layer in _tileLayers)
            {
                layer.OnThemeSwitched();
            }
        }

        public void RefreshAllLayers()
        {
            if (this._map._RedrawAllThisTurn)
            {
                foreach (var layer in _tileLayers)
                {
                    layer.RedrawAll();
                }
            }
            else if(this._map._DirtyTilesThisTurn.Count > 0)
            {
                foreach (var layer in _tileLayers)
                {
                    layer.RedrawDirtyTiles(this._map._DirtyTilesThisTurn);
                }
            }

            this._map._RedrawAllThisTurn = false;
            this._map._DirtyTilesThisTurn.Clear();
            this._map.MapObjectMemory.Flush();
        }

        public override void SetSize(Vector2i size)
        {
            base.SetSize(size);
            foreach (var layer in this._tileLayers)
            {
                layer.SetSize(size);
            }
        }

        public override void SetPosition(Vector2i pos)
        {
            base.SetPosition(pos);
            foreach (var layer in this._tileLayers)
            {
                layer.SetPosition(pos);
            }
        }

        public override void Update(float dt)
        {
            foreach (var layer in _tileLayers)
            {
                layer.Update(dt);
            }
        }

        public override void Draw()
        {
            foreach (var layer in _tileLayers)
            {
                layer.Draw();
            }
        }
    }
}
