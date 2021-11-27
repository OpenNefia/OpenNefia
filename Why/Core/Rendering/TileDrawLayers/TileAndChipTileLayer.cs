using OpenNefia.Core.Data.Types;
using OpenNefia.Core.UI;
using OpenNefia.Game;
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
        private InstancedMap Map;
        private TileAtlas TileAtlas;
        private TileAtlas ChipAtlas;
        private TileAndChipBatch TileAndChipBatch;
        private ICoords Coords;
        private WallTileShadows WallShadows;

        public TileAndChipTileLayer(InstancedMap map)
        {
            Map = map;
            TileAtlas = Atlases.Tile;
            ChipAtlas = Atlases.Chip;
            Coords = GraphicsEx.Coords;
            TileAndChipBatch = new TileAndChipBatch(map.Width, map.Height, Coords);
            WallShadows = new WallTileShadows(map, Coords);
        }

        public override void OnThemeSwitched()
        {
            var coords = Current.Game.Coords;
            TileAtlas = Atlases.Tile;
            ChipAtlas = Atlases.Chip;
            Coords = coords;
            this.TileAndChipBatch.OnThemeSwitched(TileAtlas, ChipAtlas, coords);
            this.WallShadows.OnThemeSwitched(coords);
        }

        public override void SetSize(int width = 0, int height = 0)
        {
            base.SetSize(width, height);
            this.TileAndChipBatch.SetSize(width, height);
            this.WallShadows.SetSize(width, height);
        }

        public override void SetPosition(int x = 0, int y = 0)
        {
            base.SetPosition(x, y);
            this.TileAndChipBatch.SetPosition(x, y);
            this.WallShadows.SetPosition(x, y);
        }

        private string ModifyWalls(int x, int y, TileDef tile)
        {
            // If the tile is a wall, convert the displayed tile to that of
            // the bottom wall if appropriate.
            var tileIndex = tile.Image.TileIndex;
            if (tile.WallImage != null)
            {
                var oneTileDown = Map.GetTile(x, y + 1);
                if (oneTileDown != null && oneTileDown.WallImage == null && Map.IsMemorized(x, y + 1))
                {
                    tileIndex = tile.WallImage.TileIndex;
                }

                var oneTileUp = Map.GetTile(x, y - 1);
                if (oneTileUp != null && oneTileUp.WallImage != null && Map.IsMemorized(x, y - 1))
                {
                    this.TileAndChipBatch.SetTile(x, y - 1, oneTileUp.Image.TileIndex);
                }
            }
            else if (y > 0)
            {
                var oneTileUp = Map.GetTile(x, y - 1);
                if (oneTileUp != null && oneTileUp.WallImage != null && Map.IsMemorized(x, y - 1))
                {
                    this.TileAndChipBatch.SetTile(x, y - 1, oneTileUp.WallImage.TileIndex);
                }
            }

            return tileIndex;
        }

        private void SetMapTile(int x, int y, TileDef tile)
        {
            var tileIndex = ModifyWalls(x, y, tile);

            this.WallShadows.SetTile(x, y, tile);
            this.TileAndChipBatch.SetTile(x, y, tileIndex);
        }

        public void RedrawMapObjects()
        {
            foreach (var removed in Map._MapObjectMemory.Removed)
            {
                this.TileAndChipBatch.RemoveChipEntry(removed);
            }

            foreach (var added in Map._MapObjectMemory.Added)
            {
                this.TileAndChipBatch.AddOrUpdateChipEntry(added);
            }
        }

        public override void RedrawAll()
        {
            this.WallShadows.Clear();
            this.TileAndChipBatch.Clear();

            foreach (var (x, y, tileDef) in Map.TileMemory)
            {
                SetMapTile(x, y, tileDef);
            }

            RedrawMapObjects();

            this.TileAndChipBatch.UpdateBatches();
        }

        public override void RedrawDirtyTiles(HashSet<int> dirtyTilesThisTurn)
        {
            foreach (var index in dirtyTilesThisTurn)
            {
                var x = index % Map.Width;
                var y = index / Map.Width;
                SetMapTile(x, y, Map.GetTileMemory(x, y)!);
            }

            RedrawMapObjects();

            this.TileAndChipBatch.UpdateBatches();
        }

        public override void Update(float dt)
        {
            this.TileAndChipBatch.Update(dt);
            this.WallShadows.Update(dt);
        }

        public override void Draw()
        {
            this.TileAndChipBatch.Draw();
            this.WallShadows.Draw();
        }
    }
}
