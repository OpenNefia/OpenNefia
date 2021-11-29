using OpenNefia.Core.Maths;
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

        private static Vector2i _tileSize = new Vector2i(TILE_SIZE, TILE_SIZE);
        public Vector2i TileSize => _tileSize;

        public void GetTiledSize(Vector2i screenSize, out Vector2i tiledSize)
        {
            tiledSize.X = (screenSize.X / TileSize.X);
            tiledSize.Y = (screenSize.Y / TileSize.Y);
        }

        public void TileToScreen(Vector2i tilePos, out Vector2i screenPos)
        {
            screenPos.X = ((tilePos.X - tilePos.Y) * (TILE_SIZE / 2) - 1) + (TILE_SIZE / 4);
            screenPos.Y = ((tilePos.X + tilePos.Y) * (TILE_SIZE / 4) - 1);
        }

        public void ScreenToTile(Vector2i screenPos, out Vector2i tilePos)
        {
            // TODO
            tilePos.X = screenPos.X / TileSize.X;
            tilePos.Y = screenPos.Y / TileSize.Y;
        }

        public void BoundDrawPosition(Vector2i screenPos, Vector2i tiledSize, Vector2i viewportSize, out Vector2i drawPos)
        {
            drawPos.X = screenPos.X;
            drawPos.Y = screenPos.Y;
        }
    }
}
