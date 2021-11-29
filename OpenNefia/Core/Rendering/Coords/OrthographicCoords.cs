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

        private static Vector2i _tileSize = new Vector2i(TILE_SIZE, TILE_SIZE);
        public Vector2i TileSize => _tileSize;

        public void GetTiledSize(Vector2i screenSize, out Vector2i tiledSize)
        {
            tiledSize.X = (screenSize.X / TileSize.X) + 1;
            tiledSize.Y = (screenSize.Y / TileSize.Y) + 1;
        }

        public void TileToScreen(Vector2i tilePos, out Vector2i screenPos)
        {
            screenPos.X = tilePos.X * TileSize.X;
            screenPos.Y = tilePos.Y * TileSize.Y;
        }

        public void ScreenToTile(Vector2i screenPos, out Vector2i tilePos)
        {
            tilePos.X = screenPos.X / TileSize.X;
            tilePos.Y = screenPos.Y / TileSize.Y;
        }

        public void BoundDrawPosition(Vector2i screenPos, Vector2i tiledSize, Vector2i viewportSize, out Vector2i drawPos)
        {
            var mapScreenWidth = tiledSize.X * TileSize.X;
            var mapScreenHeight = tiledSize.Y * TileSize.Y;

            var maxX = Math.Max(mapScreenWidth - viewportSize.X, 0);
            var maxY = Math.Max(mapScreenHeight - viewportSize.Y, 0);

            drawPos.X = Math.Clamp(-screenPos.X + viewportSize.X / 2, -maxX, 0);
            drawPos.Y = Math.Clamp(-screenPos.Y + viewportSize.Y / 2, -maxY, 0);
        }
    }
}
