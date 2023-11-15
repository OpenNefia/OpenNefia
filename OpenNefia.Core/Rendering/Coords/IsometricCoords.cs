using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
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
        [Dependency] private IConfigurationManager _config = default!;

        public const int TILE_SIZE = 64;

        private static Vector2i _tileSize = new Vector2i(TILE_SIZE, TILE_SIZE);
        public Vector2i TileSize => _tileSize;
        public Vector2i TileSizeScaled => (Vector2i)(TileSize * TileScale);
        public float TileScale => _config.GetCVar(CVars.DisplayTileScale);

        public Vector2i GetTiledSize(Vector2i screenSize)
        {
            return screenSize / TileSize;
        }

        public Vector2i TileToScreen(Vector2i tilePos)
        {
            return new(((tilePos.X - tilePos.Y) * (TILE_SIZE / 2) - 1) + (TILE_SIZE / 4),
                       ((tilePos.X + tilePos.Y) * (TILE_SIZE / 4) - 1));
        }

        public Vector2i ScreenToTile(Vector2i screenPos)
        {
            // TODO
            return screenPos / TileSize;
        }

        public Vector2i BoundDrawPosition(Vector2i screenPos, Vector2i tiledSize, Vector2i viewportSize)
        {
            return (Vector2i)(screenPos * TileScale);
        }
    }
}
