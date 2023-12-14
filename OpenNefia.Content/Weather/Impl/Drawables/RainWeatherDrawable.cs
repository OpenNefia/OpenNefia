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
using OpenNefia.Core.Maps;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Maps;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Content.Weather
{
    public class RainWeatherDrawable : BaseWeatherDrawable
    {
        // >>>>>>>> shade2/screen.hsp:1165 *screen_rain ...
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IHudLayer _hudLayer = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;

        private FrameCounter _counter = new();
        private int _maxRain = 0;
        private List<Rain> _rain = new();

        private int _modY;
        private int _lengthY;
        private Vector2 _speed;

        public RainWeatherDrawable(int modY, int lengthY, Vector2 speed)
        {
            _modY = modY;
            _lengthY = lengthY;
            _speed = speed;
        }

        public override void Initialize()
        {
            var wait = _config.GetCVar(CCVars.AnimeBackgroundEffectWait) * 0.25f;
            _counter = new FrameCounter(wait);
            _rain.Clear();
        }

        public override void Update(float dt)
        {
            _counter.Update(dt);

            _maxRain = (int)((_hudLayer.GamePixelBounds.Width * _hudLayer.GamePixelBounds.Height) / _coords.TileScale / 3500);

            var delta = _counter.LastFramesPassed;

            var factor = 1;
            if (_entityManager.HasComponent<MapTypeWorldMapComponent>(_mapManager.ActiveMap!.MapEntityUid))
                factor = 2;

            for (var i = 0; i < _maxRain * factor; i++)
            {
                if (_rain.Count <= i)
                    _rain.Add(new());

                var rain = _rain[i];
                var colorDelta = _rand.Next(100);
                rain.Color = new Color((byte)170 - colorDelta, (byte)200 - colorDelta, (byte)250 - colorDelta);

                var rx = rain.Position.X;
                var ry = rain.Position.Y;
                if (rx <= 0)
                    rx = _rand.NextFloat(_hudLayer.GamePixelBounds.Width / _coords.TileScale) + 40;
                if (ry <= 0)
                    ry = _rand.NextFloat(_hudLayer.GamePixelBounds.Bottom / _coords.TileScale);

                var speed = new Vector2((_speed.X) * delta, (_speed.Y + i % 8) * delta);
                rain.Position = new Vector2(rx, ry) + speed;
                if (rain.Position.Y > _hudLayer.GamePixelBounds.Bottom / _coords.TileScale)
                    rain.Position = Vector2.Zero;
            }

            if (_maxRain * factor < _rain.Count)
                _rain = _rain.GetRange(0, _maxRain * factor);
        }

        public override void Draw()
        {
            Love.Graphics.SetLineWidth(float.Max(_coords.TileScale, 1f));
            for (var i = 0; i < _rain.Count; i++)
            {
                var rain = _rain[i];
                var (x, y) = rain.Position;
                Love.Graphics.SetColor(rain.Color);
                GraphicsS.LineS(_coords.TileScale, x - 40, y - (i % _modY) - _lengthY, x - 39 + (i % 2), y);
            }
            Love.Graphics.SetLineWidth(1f);
        }

        private class Rain
        {
            /// <summary>
            /// Position in virtual pixels
            /// </summary>
            public Vector2 Position { get; set; }
            public Color Color { get; set; }
        }
        // <<<<<<<< shade2/screen.hsp:1185 	return ..
    }
}
