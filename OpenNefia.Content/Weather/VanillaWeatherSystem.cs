using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Content.Effects;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Logic;

namespace OpenNefia.Content.Weather
{
    public sealed class VanillaWeatherSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityLookup _entityLookup = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

        #region Elona.Sunny

        public void Sunny_OnWeatherChange(WeatherPrototype proto, ref P_WeatherOnWeatherChangeEvent ev)
        {
        }

        #endregion

        #region Elona.Etherwind

        public void Etherwind_CalcTravelSpeedModifier(WeatherPrototype proto, ref P_WeatherCalcTravelSpeedModifierEvent ev)
        {
        }

        public void Etherwind_OnWeatherChange(WeatherPrototype proto, ref P_WeatherOnWeatherChangeEvent ev)
        {
        }

        #endregion

        #region Elona.Snow

        public void Snow_CalcTravelSpeedModifier(WeatherPrototype proto, ref P_WeatherCalcTravelSpeedModifierEvent ev)
        {
        }

        public void Snow_OnTravel(WeatherPrototype proto, ref P_WeatherOnTravelEvent ev)
        {
        }

        public void Snow_OnWeatherChange(WeatherPrototype proto, ref P_WeatherOnWeatherChangeEvent ev)
        {
        }

        #endregion

        #region Elona.Rain

        public void Rain_CalcTravelSpeedModifier(WeatherPrototype proto, ref P_WeatherCalcTravelSpeedModifierEvent ev)
        {
        }

        public void Rain_OnWeatherChange(WeatherPrototype proto, ref P_WeatherOnWeatherChangeEvent ev)
        {
        }

        public void Rain_CalcOutdoorShadow(WeatherPrototype proto, ref P_WeatherCalcOutdoorShadowEvent ev)
        {
        }

        public void Rain_OnTurnStart(WeatherPrototype proto, ref P_WeatherOnTurnStartEvent ev)
        {
        }

        #endregion

        #region Elona.HardRain

        public void HardRain_CalcTravelSpeedModifier(WeatherPrototype proto, ref P_WeatherCalcTravelSpeedModifierEvent ev)
        {
        }

        public void HardRain_OnTravel(WeatherPrototype proto, ref P_WeatherOnTravelEvent ev)
        {
        }

        public void HardRain_OnWeatherChange(WeatherPrototype proto, ref P_WeatherOnWeatherChangeEvent ev)
        {
        }

        public void HardRain_CalcOutdoorShadow(WeatherPrototype proto, ref P_WeatherCalcOutdoorShadowEvent ev)
        {
        }

        public void HardRain_OnTurnStart(WeatherPrototype proto, ref P_WeatherOnTurnStartEvent ev)
        {
        }

        #endregion
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(WeatherPrototype))]
    public sealed class P_WeatherOnTravelEvent : PrototypeEventArgs
    {
        public P_WeatherOnTravelEvent()
        {
        }
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(WeatherPrototype))]
    public sealed class P_WeatherCalcTravelSpeedModifierEvent : PrototypeEventArgs
    {
        public P_WeatherCalcTravelSpeedModifierEvent()
        {
        }
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(WeatherPrototype))]
    public sealed class P_WeatherOnTurnStartEvent : PrototypeEventArgs
    {
        public P_WeatherOnTurnStartEvent()
        {
        }
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(WeatherPrototype))]
    public sealed class P_WeatherOnWeatherChangeEvent : PrototypeEventArgs
    {
        public P_WeatherOnWeatherChangeEvent()
        {
        }
    }

    [ByRefEvent]
    [PrototypeEvent(typeof(WeatherPrototype))]
    public sealed class P_WeatherCalcOutdoorShadowEvent : PrototypeEventArgs
    {
        public P_WeatherCalcOutdoorShadowEvent()
        {
        }
    }
}