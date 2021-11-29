using OpenNefia.Core.Game;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    public class WallTileShadows : BaseDrawable
    {
        private HashSet<MapCoordinates> TopShadows = new();
        private HashSet<MapCoordinates> BottomShadows = new();
        private ICoords _coords;

        public WallTileShadows(ICoords coords)
        {
            _coords = coords;
        }

        public void OnThemeSwitched(ICoords coords)
        {
            _coords = coords;
        }

        public void SetTile(MapCoordinates coords, TilePrototype tile)
        {
            var oneDown = coords.Offset(0, 1);

            var oneUp = coords.Offset(0, -1);
            var oneTileUp = oneUp.GetTile();

            if (tile.WallImage != null)
            {
                var oneTileDown = oneDown.GetTile();
                if (oneTileDown != null && oneTileDown.Value.Prototype.WallImage == null && oneDown.IsMemorized())
                {
                    BottomShadows.Add(coords);
                }
                else
                {
                    BottomShadows.Remove(coords);
                    TopShadows.Remove(oneDown);
                }

                if (oneTileUp != null && oneTileUp.Value.Prototype.WallImage != null && oneDown.IsMemorized())
                {
                    TopShadows.Remove(coords);
                    BottomShadows.Remove(oneUp);
                }
                else
                {
                    TopShadows.Add(coords);
                }
            }
            else
            {
                TopShadows.Remove(coords);
                if (oneTileUp != null && oneTileUp.Value.Prototype.WallImage != null && oneDown.IsMemorized())
                {
                    BottomShadows.Add(oneUp);
                }
                else
                {
                    BottomShadows.Remove(oneUp);
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
            var (tileW, tileH) = _coords.TileSize;

            Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
            GraphicsEx.SetColor(255, 255, 255, 20);

            foreach (var (_, tileX, tileY) in TopShadows)
            {
                Love.Graphics.Rectangle(Love.DrawMode.Fill, tileX * tileW + Left, tileY * tileH + Top - 20, tileW, tileH / 6);
            }

            foreach (var (_, tileX, tileY) in BottomShadows)
            {
                GraphicsEx.SetColor(255, 255, 255, 16);
                Love.Graphics.Rectangle(Love.DrawMode.Fill, tileX * tileW + Left, (tileY + 1) * tileH + Top, tileW, tileH / 2);

                GraphicsEx.SetColor(255, 255, 255, 12);
                Love.Graphics.Rectangle(Love.DrawMode.Fill, tileX * tileW + Left, (tileY + 1) * tileH + Top + tileW / 2, tileW, tileH / 4);
            }

            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            GraphicsEx.SetColor(255, 255, 255, 255);
        }
    }
}
