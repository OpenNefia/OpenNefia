using OpenNefia.Core.Configuration;
using OpenNefia.Core.Game;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.Rendering
{
    public class WallTileShadows : BaseDrawable
    {
        private ICoords _coords = default!;
        private IMap _map = default!;
        private IConfigurationManager _config = default!;

        private float _scale;

        private HashSet<Vector2i> TopShadows = new();
        private HashSet<Vector2i> BottomShadows = new();

        public void Initialize(ICoords coords, IConfigurationManager config)
        {
            _coords = coords;
            _config = config;

            _scale = config.GetCVar(CVars.DisplayTileScale);
            config.OnValueChanged(CVars.DisplayTileScale, OnTileScaleChanged);
        }

        private void OnTileScaleChanged(float scale)
        {
            _scale = scale;
        }

        public void SetMap(IMap map)
        {
            _map = map;
        }

        public void SetTile(Vector2i coords, TilePrototype tile)
        {
            var oneDown = coords + (0, 1);

            var oneUp = coords + (0, -1);
            var oneTileUp = _map.GetTile(oneUp);

            if (tile.WallImage != null)
            {
                var oneTileDown = _map.GetTile(oneDown);
                if (oneTileDown != null && oneTileDown.Value.Tile.ResolvePrototype().WallImage == null && _map.IsMemorized(oneDown))
                {
                    BottomShadows.Add(coords);
                }
                else
                {
                    BottomShadows.Remove(coords);
                    TopShadows.Remove(oneDown);
                }

                if (oneTileUp != null && oneTileUp.Value.Tile.ResolvePrototype().WallImage != null && _map.IsMemorized(oneUp))
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
                if (oneTileUp != null && oneTileUp.Value.Tile.ResolvePrototype().WallImage != null && _map.IsMemorized(oneUp))
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
            var (tileW, tileH) = (Vector2i)(_coords.TileSize * _scale);

            Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
            GraphicsEx.SetColor(255, 255, 255, 20);

            foreach (var (tileX, tileY) in TopShadows)
            {
                Love.Graphics.Rectangle(Love.DrawMode.Fill, tileX * tileW + X, tileY * tileH + Y - tileH / 4, tileW, tileH / 6);
            }

            foreach (var (tileX, tileY) in BottomShadows)
            {
                GraphicsEx.SetColor(255, 255, 255, 16);
                Love.Graphics.Rectangle(Love.DrawMode.Fill, tileX * tileW + X, (tileY + 1) * tileH + Y, tileW, tileH / 2);

                GraphicsEx.SetColor(255, 255, 255, 12);
                Love.Graphics.Rectangle(Love.DrawMode.Fill, tileX * tileW + X, (tileY + 1) * tileH + Y + tileW / 2, tileW, tileH / 4);
            }

            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            GraphicsEx.SetColor(255, 255, 255, 255);
        }
    }
}
