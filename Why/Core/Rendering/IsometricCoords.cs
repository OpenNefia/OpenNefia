using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public class IsometricCoords : ICoords
    {
        public const int TILE_SIZE = 64;

        public int TileWidth { get => TILE_SIZE; }
        public int TileHeight { get => TILE_SIZE; }

        public void GetTiledSize(int screenWidth, int screenHeight, out int tiledWidth, out int tiledHeight)
        {
            tiledWidth = (screenWidth / TileWidth);
            tiledHeight = (screenHeight / TileHeight);
        }

        public void TileToScreen(int tileX, int tileY, out int screenX, out int screenY)
        {
            screenX = ((tileX - tileY) * (TILE_SIZE / 2) - 1) + (TILE_SIZE / 4);
            screenY = ((tileX + tileY) * (TILE_SIZE / 4) - 1);
        }

        public void ScreenToTile(int screenX, int screenY, out int tileX, out int tileY)
        {
            // TODO
            tileX = screenX / TileWidth;
            tileY = screenY / TileHeight;
        }

        public void BoundDrawPosition(int screenX, int screenY, int tiledWidth, int tiledHeight, int viewportWidth, int viewportHeight, out int drawX, out int drawY)
        {
            drawX = screenX;
            drawY = screenY;
        }
    }
}
