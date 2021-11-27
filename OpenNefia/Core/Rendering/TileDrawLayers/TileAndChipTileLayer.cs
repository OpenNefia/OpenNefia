using OpenNefia.Core.Data.Types;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Why.Core.Rendering;

namespace OpenNefia.Core.Rendering.TileDrawLayers
{
    // Needs to be interleaved per-row to support wall occlusion.
    // This would be a combination of tile_layer, tile_overhang_layer and chip_layer.
    public class TileAndChipTileLayer : BaseTileLayer
    {
        private readonly IAtlasManager _atlasManager;

        private Map _map;
        private TileAndChipBatch _tileAndChipBatch;
        private ICoords _coords;
        private WallTileShadows _wallShadows;

        public TileAndChipTileLayer(Map map, IAtlasManager atlasManager)
        {
            _atlasManager = atlasManager;

            _map = map;
            _coords = GraphicsEx.Coords;
            _tileAndChipBatch = new TileAndChipBatch(map.Width, map.Height, _coords, atlasManager);
            _wallShadows = new WallTileShadows(map, _coords);
        }

        public override void OnThemeSwitched()
        {
            var coords = Current.Game.Coords;
            _coords = coords;
            this._tileAndChipBatch.OnThemeSwitched(coords);
            this._wallShadows.OnThemeSwitched(coords);
        }

        public override void SetSize(int width = 0, int height = 0)
        {
            base.SetSize(width, height);
            this._tileAndChipBatch.SetSize(width, height);
            this._wallShadows.SetSize(width, height);
        }

        public override void SetPosition(int x = 0, int y = 0)
        {
            base.SetPosition(x, y);
            this._tileAndChipBatch.SetPosition(x, y);
            this._wallShadows.SetPosition(x, y);
        }

        private string ModifyWalls(TileRef tileRef)
        {
            // If the tile is a wall, convert the displayed tile to that of
            // the bottom wall if appropriate.
            var tile = tileRef.Tile;
            var tileIndex = tile.Image.TileIndex;
            if (tile.WallImage != null)
            {
                var oneTileDown = _map.GetTile(x, y + 1);
                if (oneTileDown != null && oneTileDown.WallImage == null && _map.IsMemorized(x, y + 1))
                {
                    tileIndex = tile.WallImage.TileIndex;
                }

                var oneTileUp = _map.GetTile(x, y - 1);
                if (oneTileUp != null && oneTileUp.WallImage != null && _map.IsMemorized(x, y - 1))
                {
                    this._tileAndChipBatch.SetTile(x, y - 1, oneTileUp.Image.TileIndex);
                }
            }
            else if (y > 0)
            {
                var oneTileUp = _map.GetTile(x, y - 1);
                if (oneTileUp != null && oneTileUp.WallImage != null && _map.IsMemorized(x, y - 1))
                {
                    this._tileAndChipBatch.SetTile(x, y - 1, oneTileUp.WallImage.TileIndex);
                }
            }

            return tileIndex;
        }

        private void SetMapTile(TileRef tileRef)
        {
            var tileIndex = ModifyWalls(tileRef);

            this._wallShadows.SetTile(tileRef);
            this._tileAndChipBatch.SetTile(tileRef);
        }

        public void RedrawMapObjects()
        {
            foreach (var removed in _map.MapObjectMemory.Removed)
            {
                this._tileAndChipBatch.RemoveChipEntry(removed);
            }

            foreach (var added in _map.MapObjectMemory.Added)
            {
                this._tileAndChipBatch.AddOrUpdateChipEntry(added);
            }
        }

        public override void RedrawAll()
        {
            this._wallShadows.Clear();
            this._tileAndChipBatch.Clear();

            foreach (var tileRef in _map.AllTileMemory)
            {
                SetMapTile(tileRef);
            }

            RedrawMapObjects();

            this._tileAndChipBatch.UpdateBatches();
        }

        public override void RedrawDirtyTiles(HashSet<Vector2i> dirtyTilesThisTurn)
        {
            foreach (var pos in dirtyTilesThisTurn)
            {
                SetMapTile(_map.GetTileMemoryRef(pos));
            }

            RedrawMapObjects();

            this._tileAndChipBatch.UpdateBatches();
        }

        public override void Update(float dt)
        {
            this._tileAndChipBatch.Update(dt);
            this._wallShadows.Update(dt);
        }

        public override void Draw()
        {
            this._tileAndChipBatch.Draw();
            this._wallShadows.Draw();
        }
    }
}
