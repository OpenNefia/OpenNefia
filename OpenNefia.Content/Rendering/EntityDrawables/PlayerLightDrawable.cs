using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Configuration;
using OpenNefia.Content.World;
using OpenNefia.Core.Random;

namespace OpenNefia.Content.Rendering
{
    public sealed class PlayerLightDrawable : IEntityDrawable
    {
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IConfigurationManager _configuration = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IRandom _rand = default!;

        private float _frame;
        private Vector2 _offset;
        private Color _color;
        private IAssetInstance _assetPlayerLight = default!;
        private int _screenRefresh;

        public void Initialize(IResourceCache cache)
        {
            _frame = 0;
            _color = Color.White.WithAlphaB(50);
            _offset = _coords.TileSize / 2;
            _assetPlayerLight = Assets.Get(Protos.Asset.PlayerLight);
            _screenRefresh = _configuration.GetCVar(CCVars.AnimeScreenRefresh);
        }

        public void Update(float dt)
        {
            _frame += (dt * 50) / _screenRefresh;
            if (_frame > 1)
            {
                _frame %= 1;
                var hour = _world.State.GameDate.Hour;
                int flicker;
                if (hour > 17 || hour < 6)
                {
                    flicker = _rand.Next(10);
                }
                else
                {
                    flicker = -15;
                }
                _color = _color.WithAlphaB((byte)(flicker + 50));
            }
        }

        public void Draw(float scale, float screenX, float screenY, bool centered = false)
        {
            Love.Graphics.SetBlendMode(Love.BlendMode.Add);
            Love.Graphics.SetColor(_color);
            _assetPlayerLight.DrawUnscaled(screenX + _offset.X * scale, screenY + _offset.Y * scale,
                _assetPlayerLight.PixelWidth * scale, _assetPlayerLight.PixelHeight * scale,
                centered: true);
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            Love.Graphics.SetColor(Color.White);
        }

        public void Dispose()
        {
        }
    }
}
