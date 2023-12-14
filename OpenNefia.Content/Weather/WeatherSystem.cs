using OpenNefia.Content.Logic;
using OpenNefia.Content.Stayers;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.Areas;
using OpenNefia.Core.Game;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.World;
using OpenNefia.Content.EtherDisease;
using OpenNefia.Content.UI;
using static System.Runtime.InteropServices.JavaScript.JSType;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Inventory;
using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.Weather
{
    public interface IWeatherSystem : IEntitySystem
    {
        bool TryChangeWeather(PrototypeId<EntityPrototype> protoId, GameTimeSpan? duration = null);
        bool TryChangeWeather(PrototypeId<EntityPrototype> protoId, GameTimeSpan? duration, [NotNullWhen(true)] out WeatherComponent? weather);
        bool TryGetActiveWeather([NotNullWhen(true)] out WeatherComponent? weather);
        void ChangeWeatherFromWorldMapClimate();
        bool IsBadWeather();
        bool IsRaining();
        bool IsWeatherActive<T>() where T : class, IComponent;
        WeatherIDAndTurns PickRandomWeatherIDAndTurns();

        /// <summary>
        /// Given an entity inside a map, tries to retrieve the position they are in inside the world map.
        /// </summary>
        bool TryGetPositionInWorldMap(EntityUid player, [NotNullWhen(true)] out MapCoordinates? coords);
        GameTimeSpan CalcRandomWeatherDuration();
        void ChangeWeatherRandomly();
    }

    public sealed record class WeatherIDAndTurns(PrototypeId<EntityPrototype> ProtoID, GameTimeSpan? Duration = null);

    public sealed partial class WeatherSystem : EntitySystem, IWeatherSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IAreaKnownEntrancesSystem _areaKnownEntrances = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IEtherDiseaseSystem _etherDiseases = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;

        [RegisterSaveData("Elona.WeatherSystem.WeatherEntity")]
        private EntityUid _weatherEntity { get; set; } = EntityUid.Invalid;
        private ContainerSlot _weatherContainer => EnsureComp<WeatherHolderComponent>(_weatherEntity).Container;

        [RegisterSaveData("Elona.WeatherSystem.DateOfLastEtherwind")]
        public GameDateTime DateOfLastEtherwind { get; set; } = GameDateTime.Zero;

        private void EnsureWeatherContainer(GameInitiallyLoadedEventArgs ev)
        {
            EnsureWeatherContainer();
        }

        private void EnsureWeatherContainer()
        {
            if (!IsAlive(_weatherEntity))
            {
                Logger.WarningS("weather", "Creating weather container entity");
                _weatherEntity = EntityManager.SpawnEntity(null, MapCoordinates.Global);
                DebugTools.Assert(IsAlive(_weatherEntity), "Could not initialize weather container!");
                EnsureComp<WeatherHolderComponent>(_weatherEntity);
            }
        }

        public bool TryGetActiveWeather([NotNullWhen(true)] out WeatherComponent? weather)
        {
            EnsureWeatherContainer();
            if (!IsAlive(_weatherContainer.ContainedEntity))
                TryChangeWeather(Protos.Weather.Sunny);

            return TryComp(_weatherContainer.ContainedEntity, out weather);
        }

        public bool TryChangeWeather(PrototypeId<EntityPrototype> protoId, GameTimeSpan? duration = null)
         => TryChangeWeather(protoId, duration, out _);

        public bool TryChangeWeather(PrototypeId<EntityPrototype> protoId, GameTimeSpan? duration, [NotNullWhen(true)] out WeatherComponent? weather)
        {
            EnsureWeatherContainer();

            if (!_protos.TryIndex(protoId, out var entityProto))
            {
                weather = null;
                return false;
            }

            if (!entityProto.Components.HasComponent<WeatherComponent>())
            {
                Logger.ErrorS("weather", $"Entity prototype {protoId} has no {nameof(WeatherComponent)}!");
                weather = null;
                return false;
            }

            if (IsAlive(_weatherContainer.ContainedEntity))
                EntityManager.DeleteEntity(_weatherContainer.ContainedEntity!.Value);

            var spatial = Spatial(_weatherEntity);
            var newWeather = EntityManager.SpawnEntity(protoId, MapCoordinates.Global);
            if (!IsAlive(newWeather) || !_weatherContainer.Insert(newWeather))
            {
                Logger.ErrorS("weather", $"Failed to spawn weather {protoId}");
                EntityManager.DeleteEntity(newWeather);
                weather = null;
                return false;
            }

            Logger.DebugS("weather", $"Changed weather to: {protoId}");
            weather = EnsureComp<WeatherComponent>(newWeather);

            weather.TimeUntilNextChange = duration ?? CalcRandomWeatherDuration();

            if (_mapRenderer.TryGetTileLayer<WeatherTileDrawLayer>(out var layer))
            {
                var ev = new WeatherGetDrawableEvent();
                RaiseEvent(weather.Owner, ev);
                layer.WeatherDrawable = ev.OutDrawable;
            }

            WeatherPlayAmbientSound(_mapManager.ActiveMap);

            return true;
        }

        public void ChangeWeatherFromWorldMapClimate()
        {
            // >>>>>>>> shade2/main.hsp:564 *weather_change ...
            if (!TryGetPositionInWorldMap(_gameSession.Player, out var coords))
            {
                // Logger.DebugS("weather", "Can't find player position in world map");
                return;
            }

            if (!TryGetActiveWeather(out var weather))
            {
                Logger.ErrorS("weather", "No active weather!");
                return;
            }

            // TODO hardcoded to north tyris
            if (HasComp<WeatherTypeSnowComponent>(weather.Owner))
            {
                if (coords.Value.X < 65 && coords.Value.Y > 10)
                {
                    if (TryChangeWeather(Protos.Weather.Rain))
                    {
                        weather.TimeUntilNextChange += GameTimeSpan.FromHours(3);
                        _mes.Display(Loc.GetString("Elona.Weather.Changes"));
                    }
                }
            }
            else if (HasComp<WeatherTypeRainComponent>(weather.Owner)
                || HasComp<WeatherTypeHardRainComponent>(weather.Owner))
            {
                if (coords.Value.X > 65 || coords.Value.Y < 10)
                {
                    if (TryChangeWeather(Protos.Weather.Snow))
                    {
                        weather.TimeUntilNextChange += GameTimeSpan.FromHours(3);
                        _mes.Display(Loc.GetString("Elona.Weather.Changes"));
                    }
                }
            }

            // <<<<<<<< shade2/main.hsp:567 	return ..
        }

        public bool IsBadWeather()
        {
            if (!TryGetActiveWeather(out var weather))
                return false;

            return weather.IsBadWeather;
        }

        public bool IsRaining()
        {
            if (!TryGetActiveWeather(out var weather))
                return false;

            return HasComp<WeatherTypeRainComponent>(weather.Owner)
                || HasComp<WeatherTypeHardRainComponent>(weather.Owner);
        }

        public bool IsWeatherActive<T>() where T : class, IComponent
        {
            if (!TryGetActiveWeather(out var weather))
                return false;

            return HasComp<T>(weather.Owner);
        }

        /// <inheritdoc />
        public bool TryGetPositionInWorldMap(EntityUid player, [NotNullWhen(true)] out MapCoordinates? coords)
        {
            if (!TryMap(player, out var map) || !TryArea(map.MapEntityUid, out var area))
            {
                coords = null;
                return false;
            }

            if (HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
            {
                coords = Spatial(player).MapPosition;
                return true;
            }

            var entrances = _areaKnownEntrances.EnumerateKnownEntrancesTo(map.Id);
            if (!entrances.TryFirstOrDefault(out var entrance))
            {
                // Logger.DebugS("weather", $"Can't find entrance to {map} in world map");
                coords = null;
                return false;
            }

            coords = entrance.MapCoordinates;
            return true;
        }

        public WeatherIDAndTurns PickRandomWeatherIDAndTurns()
        {
            // >>>>>>>> shade2/main.hsp:582 		if gMonth¥3=0{ ...
            if (!TryGetActiveWeather(out var weather))
            {
                Logger.ErrorS("weather", "No active weather!");
                return new WeatherIDAndTurns(Protos.Weather.Sunny);
            }
            var ev = new WeatherOnChangeEvent(weather);
            Raise(weather.Owner, ev);

            return new WeatherIDAndTurns(ev.OutNextWeatherId ?? Protos.Weather.Sunny, ev.OutNextWeatherDuration);
            // <<<<<<<< shade2/main.hsp:612 			} ..
        }

        public GameTimeSpan CalcRandomWeatherDuration()
        {
            return GameTimeSpan.FromHours(_random.Next(22) + 2);
        }

        public void ChangeWeatherRandomly()
        {
            var nextWeather = PickRandomWeatherIDAndTurns();
            TryChangeWeather(nextWeather.ProtoID, duration: nextWeather.Duration);
        }
    }
}