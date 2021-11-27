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

        public int TileWidth { get => TILE_SIZE; }
        public int TileHeight { get => TILE_SIZE; }

        public void GetTiledSize(Vector2i screenSize, ref Vector2i tiledSize)
        {
            tiledSize.X = (screenSize.X / TileWidth);
            tiledSize.Y = (screenSize.Y / TileHeight);
        }

        public void TileToScreen(Vector2i tilePos, ref Vector2i screenPos)
        {
            screenPos.X = ((tilePos.X - tilePos.Y) * (TILE_SIZE / 2) - 1) + (TILE_SIZE / 4);
            screenPos.Y = ((tilePos.X + tilePos.Y) * (TILE_SIZE / 4) - 1);
        }

        public void ScreenToTile(Vector2i screenPos, ref Vector2i tilePos)
        {
            // TODO
            tilePos.X = screenPos.X / TileWidth;
            tilePos.Y = screenPos.Y / TileHeight;
        }

        public void BoundDrawPosition(Vector2i screenPos, Vector2i tiledSize, Vector2i viewportSize, ref Vector2i drawPos)
        {
            drawPos.X = screenPos.X;
            drawPos.Y = screenPos.Y;
        }
    }
}
