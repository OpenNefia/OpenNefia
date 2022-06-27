using OpenNefia.Content.Logic;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Weather
{
    public interface IWeatherSystem : IEntitySystem
    {
        void ChangeFromWorldMap(IMap newMap);
    }

    public sealed class WeatherSystem : EntitySystem, IWeatherSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public override void Initialize()
        {
        }

        public void ChangeFromWorldMap(IMap newMap)
        {
            // TODO
        }
    }
}