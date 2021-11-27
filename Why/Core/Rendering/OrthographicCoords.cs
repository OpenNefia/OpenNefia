using OpenNefia.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    internal class OrthographicCoords : ICoords
    {
        public const int TILE_SIZE = 48;

        public int TileWidth { get => TILE_SIZE; }
        public int TileHeight { get => TILE_SIZE; }

        public void GetTiledSize(int screenWidth, int screenHeight, out int tiledWidth, out int tiledHeight)
        {
            tiledWidth = (screenWidth / TileWidth) + 1;
            tiledHeight = (screenHeight / TileHeight) + 1;
        }

        public void TileToScreen(int tileX, int tileY, out int screenX, out int screenY)
        {
            screenX = tileX * TileWidth;
            screenY = tileY * TileHeight;
        }

        public void ScreenToTile(int screenX, int screenY, out int tileX, out int tileY)
        {
            tileX = screenX / TileWidth;
            tileY = screenY / TileHeight;
        }

        public void BoundDrawPosition(int screenX, int screenY, int tiledWidth, int tiledHeight, int viewportWidth, int viewportHeight, out int drawX, out int drawY)
        {
            var mapScreenWidth = tiledWidth * TileWidth;
            var mapScreenHeight = tiledHeight * TileHeight;

            var maxX = Math.Max(mapScreenWidth - viewportWidth, 0);
            var maxY = Math.Max(mapScreenHeight - viewportHeight, 0);

            drawX = Math.Clamp(-screenX + viewportWidth / 2, -maxX, 0) ;
            drawY = Math.Clamp(-screenY + viewportHeight / 2, -maxY, 0) ;
        }
    }
}
