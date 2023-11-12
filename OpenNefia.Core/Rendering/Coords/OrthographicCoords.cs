using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
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
        [Dependency] private IConfigurationManager _config = default!;

        public const int TILE_SIZE = 48;

        private static Vector2i _tileSize = new Vector2i(TILE_SIZE, TILE_SIZE);
        public Vector2i TileSize => _tileSize;
        public Vector2i TileSizeScaled => (Vector2i)(TileSize * TileScale);
        public float TileScale => _config.GetCVar(CVars.DisplayTileScale);

        public Vector2i GetTiledSize(Vector2i screenSize)
        {
            return screenSize / TileSize + Vector2i.One;
        }

        public Vector2i TileToScreen(Vector2i tilePos)
        {
            return tilePos * TileSize;
        }

        public Vector2i ScreenToTile(Vector2i screenPos)
        {
            return screenPos / TileSize;
        }

        public Vector2i BoundDrawPosition(Vector2i screenPos, Vector2i tiledSize, Vector2i viewportSize)
        {
            tiledSize = (Vector2i)(tiledSize * TileScale);

            var maxX = Math.Max(tiledSize.X - viewportSize.X, 0);
            var maxY = Math.Max(tiledSize.Y - viewportSize.Y, 0);

            return new(Math.Clamp(-screenPos.X + viewportSize.X / 2, -maxX, 0),
                       Math.Clamp(-screenPos.Y + viewportSize.Y / 2, -maxY, 0));
        }
    }
}
