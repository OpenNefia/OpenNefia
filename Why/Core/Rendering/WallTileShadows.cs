using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Maps;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public class WallTileShadows : BaseDrawable
    {
        private Map Map;
        private ICoords Coords;

        private HashSet<int> TopShadows;
        private HashSet<int> BottomShadows;

        public WallTileShadows(Map map, ICoords coords)
        {
            this.Map = map;
            this.Coords = coords;
            TopShadows = new HashSet<int>();
            BottomShadows = new HashSet<int>();
        }

        public void OnThemeSwitched(ICoords coords)
        {
            this.Coords = coords;
        }

        public void SetTile(TileRef tileRef)
        {
            var tile = tileRef.Tile;
            var tileIndex = tile.Image.TileIndex;
            if (tile.WallImage != null)
            {
                var oneTileDown = Map.GetTile(x, y + 1);
                if (oneTileDown != null && oneTileDown.WallImage == null && Map.IsMemorized(x, y + 1))
                {
                    tileIndex = tile.WallImage.TileIndex;
                    BottomShadows.Add(y * Map.Width + x);
                }
                else
                {
                    BottomShadows.Remove(y * Map.Width + x);
                    TopShadows.Remove((y + 1) * Map.Width + x);
                }

                var oneTileUp = Map.GetTile(x, y - 1);
                if (oneTileUp != null && oneTileUp.WallImage != null && Map.IsMemorized(x, y - 1))
                {
                    TopShadows.Remove(y * Map.Width + x);
                    BottomShadows.Remove((y - 1) * Map.Width + x);
                }
                else
                {
                    TopShadows.Add(y * Map.Width + x);
                }
            }
            else
            {
                TopShadows.Remove(y * Map.Width + x);
                var oneTileUp = Map.GetTile(x, y - 1);
                if (oneTileUp != null && oneTileUp.WallImage != null && Map.IsMemorized(x, y - 1))
                {
                    BottomShadows.Add((y - 1) * Map.Width + x);
                }
                else
                {
                    BottomShadows.Remove((y - 1) * Map.Width + x);
                }
            }
        }

        public void Clear()
        {
            TopShadows.Clear();
            BottomShadows.Clear();
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            var tileW = Coords.TileWidth;
            var tileH = Coords.TileHeight;

            Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
            GraphicsEx.SetColor(255, 255, 255, 20);

            foreach (var index in TopShadows)
            {
                var tileX = index % Map.Width;
                var tileY = index / Map.Width;
                GraphicsEx.FilledRect(tileX * tileW + X, tileY * tileH + Y - 20, tileW, tileH / 6);
            }

            foreach (var index in BottomShadows)
            {
                var tileX = index % Map.Width;
                var tileY = index / Map.Width;

                GraphicsEx.SetColor(255, 255, 255, 16);
                GraphicsEx.FilledRect(tileX * tileW + X, (tileY + 1) * tileH + Y, tileW, tileH / 2);

                GraphicsEx.SetColor(255, 255, 255, 12);
                GraphicsEx.FilledRect(tileX * tileW + X, (tileY + 1) * tileH + Y + tileW / 2, tileW, tileH / 4);
            }

            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            GraphicsEx.SetColor(255, 255, 255, 255);
        }
    }
}
