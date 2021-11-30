using OpenNefia.Core.GameController;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    public class MapRenderer : BaseDrawable, IMapRenderer
    {
        [Dependency] private readonly ITileAtlasManager _atlasManager = default!;
        [Dependency] private readonly IAssetManager _assetManager = default!;
        [Dependency] private readonly IGameController _gameController = default!;
        [Dependency] private readonly IMapDrawables _mapDrawables = default!;

        private List<ITileLayer> _tileLayers = new List<ITileLayer>();
        private IMap _map = default!;

        public void SetMap(IMap map)
        {
            _map = map;
            _tileLayers.Clear();
            _tileLayers.Add(new TileAndChipTileLayer(_map, _atlasManager));
            _tileLayers.Add(new ShadowTileLayer(_map, _assetManager));
            RefreshAllLayers();
            _mapDrawables.Clear();
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
            if (this._map.RedrawAllThisTurn)
            {
                foreach (var layer in _tileLayers)
                {
                    layer.RedrawAll();
                }
            }
            else if(this._map.DirtyTilesThisTurn.Count > 0)
            {
                foreach (var layer in _tileLayers)
                {
                    layer.RedrawDirtyTiles(this._map.DirtyTilesThisTurn);
                }
            }

            this._map.RedrawAllThisTurn = false;
            this._map.DirtyTilesThisTurn.Clear();
            this._map.MapObjectMemory.Flush();
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            foreach (var layer in this._tileLayers)
            {
                layer.SetSize(width, height);
            }
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            foreach (var layer in this._tileLayers)
            {
                layer.SetPosition(x, y);
            }
            _mapDrawables.SetPosition(x, y);
        }

        public override void Update(float dt)
        {
            foreach (var layer in _tileLayers)
            {
                layer.Update(dt);
            }
            _mapDrawables.Update(dt);
        }

        public override void Draw()
        {
            foreach (var layer in _tileLayers)
            {
                layer.Draw();
            }
            _mapDrawables.Draw();
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
