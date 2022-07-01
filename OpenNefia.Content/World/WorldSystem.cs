using OpenNefia.Analyzers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.SaveGames;
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

        void PassTime(GameTimeSpan time, bool noEvents = false);
    }

    public class WorldSystem : EntitySystem, IWorldSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;

        [RegisterSaveData("Elona.WorldSystem.State")]
        public WorldState State { get; } = new();

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

            State.GameDate.Set(newYears, newMonths, newDays, newHours, newMinutes, newSeconds);

            if (!noEvents)
            {
                var ev = new MapOnTimePassedEvent(map, yearsPassed, monthsPassed, daysPassed, hoursPassed, minutesPassed, secondsPassed);
                RaiseEvent(map.MapEntityUid, ref ev);

            }
        }
    }

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

        public MapOnTimePassedEvent(IMap map, int yearsPassed, int monthsPassed, int daysPassed, int hoursPassed, int minutesPassed, int secondsPassed)
        {
            Map = map;
            YearsPassed = yearsPassed;
            MonthsPassed = monthsPassed;
            DaysPassed = daysPassed;
            HoursPassed = hoursPassed;
            MinutesPassed = minutesPassed;
            SecondsPassed = secondsPassed;
        }
    }
}