using OpenNefia.Content.MapVisibility;
using OpenNefia.Content.WorldMap;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Maps;

namespace OpenNefia.Content.Weather
{
    [RegisterTileLayer(renderAfter: new[] { typeof(ShadowTileLayer) })]
    public sealed class WeatherTileDrawLayer : BaseTileLayer
    {
        [Dependency] private readonly IMapManager _mapManager = default!;

        private IWeatherDrawable? _weatherDrawable = null;
        public IWeatherDrawable? WeatherDrawable
        {
            get => _weatherDrawable;
            set
            {
                if (_weatherDrawable != value)
                {
                    _weatherDrawable = value;
                    if (_weatherDrawable != null)
                    {
                        EntitySystem.InjectDependencies(_weatherDrawable);
                        _weatherDrawable?.SetSize(Width, Height);
                        _weatherDrawable?.SetPosition(X, Y);
                        _weatherDrawable?.Initialize();
                    }
                }
            }
        }

        public override void Initialize()
        {
            WeatherDrawable = new EtherwindWeatherDrawable();
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            _weatherDrawable?.SetSize(width, height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            _weatherDrawable?.SetPosition(x, y);
        }

        public override void OnThemeSwitched()
        {
            _weatherDrawable?.Initialize();
        }

        public override void Update(float dt)
        {
            if (_weatherDrawable != null)
            {
                _weatherDrawable.Update(dt);
            }
        }

        public override void Draw()
        {
            if (_weatherDrawable != null)
            {
                if (CanDrawWeather())
                {
                    _weatherDrawable.Draw();
                }
            }
        }

        private bool CanDrawWeather()
        {
            if (_weatherDrawable?.CanRenderInIndoorMaps ?? false)
                return true;

            return _mapManager.ActiveMap != null
                && EntityManager.TryGetComponent<MapCommonComponent>(_mapManager.ActiveMap.MapEntityUid, out var mapCommon)
                && mapCommon.IsIndoors == false;
        }
    }
}
