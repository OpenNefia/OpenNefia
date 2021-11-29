using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Game;
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
            _coords = GameSession.Coords;
            _tileAndChipBatch = new TileAndChipBatch(map.Width, map.Height, _coords, atlasManager);
            _wallShadows = new WallTileShadows(_coords);
        }

        public override void OnThemeSwitched()
        {
            _coords = GameSession.Coords;
            this._tileAndChipBatch.OnThemeSwitched(_coords);
            this._wallShadows.OnThemeSwitched(_coords);
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

        private string ModifyWalls(MapCoordinates coords, TilePrototype tile)
        {
            // If the tile is a wall, convert the displayed tile to that of
            // the bottom wall if appropriate.
            var tileIndex = tile.Image.Identifier;

            var oneDown = coords.Offset(0, 1);
            var oneTileDown = oneDown.GetTile();

            var oneUp = coords.Offset(0, -1);
            var oneTileUp = oneUp.GetTile();

            if (tile.WallImage != null)
            {
                if (oneTileDown != null && oneTileDown.Value.Prototype.WallImage == null && oneDown.IsMemorized())
                {
                    tileIndex = tile.WallImage.Identifier;
                }

                if (oneTileUp != null && oneTileUp.Value.Prototype.WallImage != null && oneDown.IsMemorized())
                {
                    this._tileAndChipBatch.SetTile(oneUp.Position, oneTileUp.Value.Prototype.Image.Identifier);
                }
            }
            else if (coords.Y > 0)
            {
                if (oneTileUp != null && oneTileUp.Value.Prototype.WallImage != null && oneDown.IsMemorized())
                {
                    this._tileAndChipBatch.SetTile(oneUp.Position, oneTileUp.Value.Prototype.WallImage.Identifier);
                }
            }

            return tileIndex;
        }

        private void SetMapTile(MapCoordinates coords, TilePrototype tile)
        {
            var tileIndex = ModifyWalls(coords, tile);

            this._wallShadows.SetTile(coords, tile);
            this._tileAndChipBatch.SetTile(coords.Position, tile.Image.Identifier);
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

            foreach (var coords in _map.AllTiles)
            {
                SetMapTile(coords, coords.GetTileMemory()!.Value.Prototype);
            }

            RedrawMapObjects();

            this._tileAndChipBatch.UpdateBatches();
        }

        public override void RedrawDirtyTiles(HashSet<MapCoordinates> dirtyTilesThisTurn)
        {
            foreach (var coords in dirtyTilesThisTurn)
            {
                SetMapTile(coords, coords.GetTileMemory()!.Value.Prototype);
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
