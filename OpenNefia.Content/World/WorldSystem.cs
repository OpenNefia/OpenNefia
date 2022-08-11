using OpenNefia.Analyzers;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Parties;
using OpenNefia.Content.TitleScreen;
using OpenNefia.Content.UI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.SaveGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.World
{
    public interface IWorldSystem : IEntitySystem
    {
        WorldState State { get; }

        void PassTime(GameTimeSpan time, bool noEvents = false);
    }

    public class WorldSystem : EntitySystem, IWorldSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        [RegisterSaveData("Elona.WorldSystem.State")]
        public WorldState State { get; } = new();

        public override void Initialize()
        {
            SubscribeBroadcast<NewGameStartedEventArgs>(InitializeState);
            SubscribeEntity<CheckKillEvent>(IncrementTotalKills);
            SubscribeEntity<MapOnTimePassedEvent>(UpdateAwakeHours, priority: EventPriorities.Low);
        }

        private void InitializeState(NewGameStartedEventArgs ev)
        {
            State.RandomSeed = _rand.Next(800) + 2;
        }

        private void IncrementTotalKills(EntityUid victim, ref CheckKillEvent args)
        {
            if (_parties.IsInPlayerParty(args.Attacker) && !_parties.IsInPlayerParty(victim))
                State.TotalKills++;
        }

        private void UpdateAwakeHours(EntityUid uid, ref MapOnTimePassedEvent args)
        {
            if (args.HoursPassed <= 0)
                return;

            if (HasComp<MapTypeWorldMapComponent>(uid))
            {
                if (_rand.OneIn(3))
                {
                    State.AwakeTime += GameTimeSpan.FromHours(args.HoursPassed);
                }
                if (_rand.OneIn(15))
                {
                    _mes.Display(Loc.GetString("Elona.World.Nap"));
                    State.AwakeTime -= GameTimeSpan.FromHours(3);
                    if (State.AwakeTime.TotalSeconds < 0)
                        State.AwakeTime = GameTimeSpan.Zero;
                }
            }
            else
            {
                if (!HasComp<MapNoSleepAdvancementComponent>(args.Map.MapEntityUid))
                {
                    State.AwakeTime += GameTimeSpan.FromHours(args.HoursPassed);
                }
            }
            
            if (State.GameDate.Hour == 8)
            {
                _mes.Display(Loc.GetString("Elona.World.NewDay"), UiColors.MesYellow);
            }
        }

        public void PassTime(GameTimeSpan time, bool noEvents = false)
        {
            var map = _mapManager.ActiveMap;
            if (map == null)
                return;

            // This does not use DateTime's addition. Months in Elona are always 31 days long,
            // and there are no leap years.
            // Also, events are only fired when one of the counters rolls over to zero. That's
            // the reason why the addition is manual.
            var date = State.GameDate;

            var newSeconds = date.Second;
            var newMinutes = date.Minute;
            var newHours = date.Hour;
            var newDays = date.Day;
            var newMonths = date.Month;
            var newYears = date.Year;

            int secondsPassed = (int)time.TotalSeconds;
            int minutesPassed = 0;
            int hoursPassed = 0;
            int daysPassed = 0;
            int monthsPassed = 0;
            int yearsPassed = 0;
            
            newSeconds += secondsPassed;
            if (newSeconds >= 60)
            {
                minutesPassed = newSeconds / 60;
                newSeconds %= 60;
                newMinutes += minutesPassed;
            }

            if (newMinutes >= 60)
            {
                hoursPassed = newMinutes / 60;
                newMinutes %= 60;
                newHours += hoursPassed;
            }

            if (newHours >= 24)
            {
                daysPassed = newHours / 24;
                newHours %= 24;
                newDays += daysPassed;
            }

            if (newDays >= 31)
            {
                monthsPassed = (newDays - 1) / 30;
                newDays = (newDays - 1) % 30 + 1;
                newMonths += monthsPassed;
            }

            if (newMonths >= 13)
            {
                yearsPassed = (newMonths - 1) / 12;
                newMonths = (newMonths - 1) % 13 + 1;
                newYears += yearsPassed;
            }

            State.GameDate = new GameDateTime(newYears, newMonths, newDays, newHours, newMinutes, newSeconds);

            if (!noEvents)
            {
                var ev = new MapOnTimePassedEvent(map, yearsPassed, monthsPassed, daysPassed, hoursPassed, minutesPassed, secondsPassed, time);
                RaiseEvent(map.MapEntityUid, ref ev);

            }
        }
    }

    /// <summary>
    /// Raised when time passes.
    /// </summary>
    /// <remarks>
    /// The "time passed" properties on this class indicate when a time counter
    /// has rolled over to the next value. For example, if time passes from
    /// 8:59 to 9:01, then HoursPassed will be 1. This is to allow events to take place
    /// on the hour/day/month/etc.
    /// </remarks>
    [ByRefEvent]
    public struct MapOnTimePassedEvent
    {
        public IMap Map { get; }
        
        public int YearsPassed { get; }
        public int MonthsPassed { get; }
        public int DaysPassed { get; }
        public int HoursPassed { get; }
        public int MinutesPassed { get; }
        public int SecondsPassed { get; }
        
        /// <summary>
        /// Amount of time that was actually passed. Used if the event handler
        /// is accumulating time by adding up this value on every turn.
        /// </summary>
        public GameTimeSpan TotalTimePassed { get; }

        public MapOnTimePassedEvent(IMap map, int yearsPassed, int monthsPassed, int daysPassed, int hoursPassed, int minutesPassed, int secondsPassed, GameTimeSpan totalTimePassed)
        {
            Map = map;
            YearsPassed = yearsPassed;
            MonthsPassed = monthsPassed;
            DaysPassed = daysPassed;
            HoursPassed = hoursPassed;
            MinutesPassed = minutesPassed;
            SecondsPassed = secondsPassed;
            TotalTimePassed = totalTimePassed;
        }
    }
}