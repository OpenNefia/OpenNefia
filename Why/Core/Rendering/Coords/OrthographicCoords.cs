using OpenNefia.Core.Maths;
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

        public void GetTiledSize(Vector2i screenSize, ref Vector2i tiledSize)
        {
            tiledSize.X = (screenSize.X / TileWidth) + 1;
            tiledSize.Y = (screenSize.Y / TileHeight) + 1;
        }

        public void TileToScreen(Vector2i tilePos, ref Vector2i screenPos)
        {
            screenPos.X = tilePos.X * TileWidth;
            screenPos.Y = tilePos.Y * TileHeight;
        }

        public void ScreenToTile(Vector2i screenPos, ref Vector2i tilePos)
        {
            tilePos.X = screenPos.X / TileWidth;
            tilePos.Y = screenPos.Y / TileHeight;
        }

        public void BoundDrawPosition(Vector2i screenPos, Vector2i tiledSize, Vector2i viewportSize, ref Vector2i drawPos)
        {
            var mapScreenWidth = tiledSize.X * TileWidth;
            var mapScreenHeight = tiledSize.Y * TileHeight;

            var maxX = Math.Max(mapScreenWidth - viewportSize.X, 0);
            var maxY = Math.Max(mapScreenHeight - viewportSize.Y, 0);

            drawPos.X = Math.Clamp(-screenPos.X + viewportSize.X / 2, -maxX, 0);
            drawPos.Y = Math.Clamp(-screenPos.Y + viewportSize.Y / 2, -maxY, 0);
        }
    }
}
