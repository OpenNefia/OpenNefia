using OpenNefia.Content.Activity;
using OpenNefia.Content.Maps;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Content.UI;
using OpenNefia.Content.World;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Areas;

namespace OpenNefia.Content.Weather
{
    public sealed partial class WeatherSystem
    {
        public override void Initialize()
        {
            SubscribeBroadcast<GameInitiallyLoadedEventArgs>(EnsureWeatherContainer);
            SubscribeEntity<WeatherOnChangeEvent>(OnWeatherChange_ProcEtherwind, priority: EventPriorities.High);
            SubscribeBroadcast<MapOnTimePassedEvent>(WeatherPassTurn, priority: EventPriorities.High);
            SubscribeEntity<MapEnterEvent>(OnMapEnter_ProcWeather);
            SubscribeBroadcast<OnTravelInWorldMapEvent>(ProcWeatherOnTravel);
            SubscribeBroadcast<CalcMapShadowEvent>(ProcWeatherCalcMapShadow);
            SubscribeEntity<EntityTurnStartingEventArgs>(ProcWeatherOnTurnStart);
        }

        private void OnWeatherChange_ProcEtherwind(EntityUid uid, WeatherOnChangeEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> shade2/main.hsp:582 		if gMonth¥3=0{ ...
            var date = _world.State.GameDate;
            if (date.Month % 3 == 0)
            {
                if (date.Day >= 1 && date.Day <= 10 && DateOfLastEtherwind.Month != date.Month)
                {
                    if (_random.Next(15) < date.Day + 5)
                    {
                        DateOfLastEtherwind = date;
                        _mes.Display(Loc.GetString("Elona.Weather.Types.Etherwind.Starts"), color: UiColors.MesRed);
                        args.Handle(Protos.Weather.Etherwind, GameTimeSpan.FromHours(100));
                        return;
                    }
                }
            }
            // <<<<<<<< shade2/main.hsp:612 			} ..
        }

        private void ProcWeatherOnTravel(ref OnTravelInWorldMapEvent ev)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:808 	if (gWeather=weatherSnow)or(tRole(map(cx(cc),cy(c ...
            if (!TryGetActiveWeather(out var weather))
                return;

            var ev2 = new WeatherOnTravelEvent(ev.Activity);
            RaiseEvent(weather.Owner, ref ev2);
            // <<<<<<<< elona122/shade2/proc.hsp:846 		} ...
        }

        private void ProcWeatherCalcMapShadow(ref CalcMapShadowEvent ev)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:808 	if (gWeather=weatherSnow)or(tRole(map(cx(cc),cy(c ...
            if (!TryGetActiveWeather(out var weather))
                return;

            var ev2 = new WeatherCalcMapShadowEvent(ev.OutShadow);
            RaiseEvent(weather.Owner, ref ev2);
            ev.OutShadow = ev2.OutShadow;
            // <<<<<<<< elona122/shade2/proc.hsp:846 		} ...
        }

        private void ProcWeatherOnTurnStart(EntityUid uid, EntityTurnStartingEventArgs ev)
        {
            if (TryGetActiveWeather(out var weather))
            {
                var ev2 = new WeatherOnTurnStartEvent(uid);
                RaiseAndPropagate(weather.Owner, ev2, propagateTo: ev);
            }
        }

        private void WeatherPassTurn(ref MapOnTimePassedEvent ev)
        {
            // >>>>>>>> shade2/main.hsp:576 	gNextWeather-- ...
            if (TryGetActiveWeather(out var weather))
            {
                weather.TimeUntilNextChange -= ev.TotalTimePassed;
                ChangeWeatherFromWorldMapClimate();
                if (weather.TimeUntilNextChange <= GameTimeSpan.Zero)
                    ChangeWeatherRandomly();
            }
            //<<<<<<<< shade2/main.hsp:579 		gNextWeather=rnd(22)+2 ..
        }

        private void OnMapEnter_ProcWeather(EntityUid uid, MapEnterEvent args)
        {
            ChangeWeatherFromWorldMapClimate();
            WeatherPlayAmbientSound(args.Map);
        }

        public const string WeatherLoopingSoundTag = "Elona.Weather";

        private void WeatherPlayAmbientSound(IMap? map)
        {
            // >>>>>>>> shade2/sound.hsp:384 	if mField=mFieldOutdoor:dssetvolume lpFile,cfg_sv ...
            if (map == null || !IsAlive(map.MapEntityUid))
                return;

            _audio.StopLooping(WeatherLoopingSoundTag);

            if (!TryGetActiveWeather(out var weather) || weather.AmbientSound == null)
                return;

            var soundId = weather.AmbientSound.GetSound();
            if (soundId == null)
            {
                return;
            }

            var isIndoors = CompOrNull<MapCommonComponent>(map.MapEntityUid)?.IsIndoors ?? false;

            var volume = 1f;
            if (!isIndoors)
            {
                volume = 0.8f;
            }
            else
            {
                if (_areaManager.TryGetFloorOfMap(map.Id, out var floor))
                {
                    if (floor.Value.FloorNumber == AreaFloorId.DefaultFloorNumber)
                        volume = 0.2f;
                    else
                        volume = 0f;
                }
                else
                    volume = 0f;
            }

            _audio.PlayLooping(soundId.Value, WeatherLoopingSoundTag, audioParams: new AudioParams() { Volume = volume });
            // <<<<<<<< shade2/sound.hsp:384 	if mField=mFieldOutdoor:dssetvolume lpFile,cfg_sv ..
        }
    }

    /// <summary>
    /// Raised to determine the next weather to change to in the overarching
    /// weather state machine.
    /// </summary>
    [EventUsage(EventTarget.Weather)]
    public sealed class WeatherOnChangeEvent : HandledEntityEventArgs
    {
        public WeatherOnChangeEvent(WeatherComponent weather)
        {
        }

        public PrototypeId<EntityPrototype>? OutNextWeatherId { get; set; } = null;
        public GameTimeSpan? OutNextWeatherDuration { get; set; } = null;

        public void Handle(PrototypeId<EntityPrototype> weatherId, GameTimeSpan? duration = null)
        {
            Handled = true;
            OutNextWeatherId = weatherId;
            if (duration != null)
                OutNextWeatherDuration = duration;
        }
    }

    [ByRefEvent]
    [EventUsage(EventTarget.Weather)]
    public struct WeatherOnTravelEvent
    {
        public WeatherOnTravelEvent(ActivityComponent activity)
        {
            Activity = activity;
        }

        public ActivityComponent Activity { get; }
    }

    [ByRefEvent]
    [EventUsage(EventTarget.Weather)]
    public struct WeatherCalcMapShadowEvent
    {
        public WeatherCalcMapShadowEvent(Color shadow)
        {
            OutShadow = shadow;
        }

        public Color OutShadow { get; set; }
    }

    [EventUsage(EventTarget.Weather)]
    public sealed class WeatherCalcTravelSpeedModifierEvent : EntityEventArgs
    {
        public WeatherCalcTravelSpeedModifierEvent(int turns, MapCoordinates coords, PrototypeId<TilePrototype> tileID)
        {
            OutTurns = turns;
            Coords = coords;
            TileID = tileID;
        }

        public int OutTurns { get; set; }
        public MapCoordinates Coords { get; }
        public PrototypeId<TilePrototype> TileID { get; }
    }

    [EventUsage(EventTarget.Weather)]
    public sealed class WeatherOnTurnStartEvent : TurnResultEntityEventArgs
    {
        public WeatherOnTurnStartEvent(EntityUid entity)
        {
            Entity = entity;
        }

        public EntityUid Entity { get; }
    }

    [EventUsage(EventTarget.Weather)]
    public sealed class WeatherGetDrawableEvent
    {
        public IWeatherDrawable? OutDrawable { get; set; }
    }
}
