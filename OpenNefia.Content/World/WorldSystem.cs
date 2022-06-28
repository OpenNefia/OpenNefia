using OpenNefia.Analyzers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.World
{
    public interface IWorldSystem
    {
        WorldState State { get; }

        void PassTime(TimeSpan time, bool noEvents = false);
    }

    public class WorldSystem : EntitySystem, IWorldSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;

        [RegisterSaveData("Elona.WorldSystem.State")]
        public WorldState State { get; } = new();

        public void PassTime(TimeSpan time, bool noEvents = false)
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

            int minutesPassed = 0;
            int hoursPassed = 0;
            int daysPassed = 0;
            int monthsPassed = 0;
            int yearsPassed = 0;

            newSeconds += (int)time.TotalSeconds;
            if (newSeconds > 0)
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

            State.GameDate.Set(newYears, newMonths, newDays, newHours, newMinutes, newSeconds);

            if (!noEvents)
            {
                if (hoursPassed > 0)
                {
                    var ev = new MapOnHoursPassedEvent(hoursPassed);
                    RaiseLocalEvent(map.MapEntityUid, ref ev);
                }
                if (daysPassed > 0)
                {
                    var ev = new MapOnDaysPassedEvent(daysPassed);
                    RaiseLocalEvent(map.MapEntityUid, ref ev);
                }
                if (monthsPassed > 0)
                {
                    var ev = new MapOnMonthsPassedEvent(monthsPassed);
                    RaiseLocalEvent(map.MapEntityUid, ref ev);
                }
                if (yearsPassed > 0)
                {
                    var ev = new MapOnYearsPassedEvent(yearsPassed);
                    RaiseLocalEvent(map.MapEntityUid, ref ev);
                }
            }
        }
    }

    [ByRefEvent]
    public struct MapOnHoursPassedEvent
    {
        public int HoursPassed { get; }

        public MapOnHoursPassedEvent(int hoursPassed)
        {
            HoursPassed = hoursPassed;
        }
    }

    [ByRefEvent]
    public struct MapOnDaysPassedEvent
    {
        public int DaysPassed { get; }

        public MapOnDaysPassedEvent(int hoursPassed)
        {
            DaysPassed = hoursPassed;
        }
    }

    [ByRefEvent]
    public struct MapOnMonthsPassedEvent
    {
        public int MonthsPassed { get; }

        public MapOnMonthsPassedEvent(int hoursPassed)
        {
            MonthsPassed = hoursPassed;
        }
    }

    [ByRefEvent]
    public struct MapOnYearsPassedEvent
    {
        public int YearsPassed { get; }

        public MapOnYearsPassedEvent(int hoursPassed)
        {
            YearsPassed = hoursPassed;
        }
    }

    [DataDefinition]
    public class WorldState
    {
        /// <summary>
        /// The date the game starts on when creating a new character.
        /// </summary>
        public static readonly GameDateTime DefaultDate = new(517, 8, 12, 1, 10, 0);

        /// <summary>
        /// Current in-game date.
        /// </summary>
        [DataField]
        public GameDateTime GameDate { get; set; } = DefaultDate;

        /// <summary>
        /// The date the game was initially started. Used for things like calculating
        /// time passed.
        /// </summary>
        [DataField]
        public GameDateTime InitialDate { get; set; } = DefaultDate;

        /// <summary>
        /// Number of in-game minutes that have passed across all maps so far.
        /// </summary>
        [DataField]
        public int PlayTurns { get; set; }

        /// <summary>
        /// Total number of creatures the player has killed.
        /// </summary>
        [DataField]
        public int TotalKills { get; set; }

        /// <summary>
        /// Random seed this save was initialized with.
        /// </summary>
        [DataField]
        public int RandomSeed { get; set; }
    
        /// <summary>
        /// The deepest dungeon level the player has traversed to.
        /// </summary>
        [DataField]
        public int DeepestLevel { get; set; }

        /// <summary>
        /// Date the player last entered the world map.
        /// </summary>
        [DataField]
        public GameDateTime TravelStartDate { get; set; } = GameDateTime.Zero;

        /// <summary>
        /// Total distance traveled on the world map so far. Reset when entering
        /// a new travel destination map (town/guild).
        /// </summary>
        [DataField]
        public int TravelDistance { get; set; }

        [DataField]
        public string? LastDepartedMapName { get; set; }
    }
}
