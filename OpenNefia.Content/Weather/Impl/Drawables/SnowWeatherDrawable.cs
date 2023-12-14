using Love;
using OpenNefia.Content.Weather.Impl.Drawables;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector2 = OpenNefia.Core.Maths.Vector2;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Core.Random;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.Weather
{
    public class SnowWeatherDrawable : BaseWeatherDrawable
    {
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IHudLayer _hudLayer = default!;
        [Dependency] private readonly IRandom _rand = default!;

        // >>>>>>>> shade2/screen.hsp:1187 *screen_snow ...
        private FrameCounter _counter = new();
        private IAssetInstance _assetSnowEtherwind = default!;
        private int _maxRain = 0;
        private List<Vector2> _rain = new();

        public override void Initialize()
        {
            var wait = _config.GetCVar(CCVars.AnimeBackgroundEffectWait) * 0.25f;
            _counter = new FrameCounter(wait);
            _assetSnowEtherwind = Assets.Get(Protos.Asset.WeatherSnowEtherwind);
            _rain.Clear();
        }

        public override void Update(float dt)
        {
            _counter.Update(dt);

            _maxRain = (int)((_hudLayer.GamePixelBounds.Width * _hudLayer.GamePixelBounds.Height) / _coords.TileScale / 3500) * 2;

            var delta = _counter.LastFramesPassed;

            for (var i = 0; i < _maxRain; i++)
            {
                if (_rain.Count <= i)
                    _rain.Add(Vector2.Zero);

                var rain = _rain[i];
                if (rain.Y == 0)
                {
                    var rx = _rand.NextFloat(_hudLayer.GamePixelBounds.Width / _coords.TileScale);
                    var ry = -_rand.NextFloat((_hudLayer.GamePixelBounds.Bottom / _coords.TileScale) / 2);
                    _rain[i] = new Vector2(rx, ry);
                }
                else
                {
                    _rain[i] = new Vector2(rain.X + (_rand.NextFloat(3 * delta) - 1f * delta), rain.Y + (_rand.NextFloat(2 * delta) + i % 5) * delta);
                    if (_rain[i].Y > _hudLayer.GamePixelBounds.Bottom / _coords.TileScale || _rand.OneIn(500))
                    {
                        _rain[i] = Vector2i.Zero;
                    }
                }
            }

            if (_maxRain < _rain.Count)
                _rain = _rain.GetRange(0, _maxRain);
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Love.Color.Black);

            for (var i = 0; i < _rain.Count; i++)
            {
                if (i % 30 == 0)
                    Love.Graphics.SetColor(Core.Maths.Color.White.WithAlphaB((byte)(100 + (i % 150))));

                var rain = _rain[i];
                var idx = Math.Clamp((int)(rain.X % 2 + (i % 6) * 4), 0, _assetSnowEtherwind.Regions.Count - 1);
                _assetSnowEtherwind.DrawRegion(_coords.TileScale, idx.ToString(), rain.X, rain.Y);
            }
        }
        // <<<<<<<< shade2/screen.hsp:1204 	return ..
    }
}