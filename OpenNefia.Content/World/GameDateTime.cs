using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.World
{
    /// <summary>
    /// Date/time struct for Elona's in-game world.
    /// </summary>
    /// <remarks>
    /// <see cref="DateTime"/> assumes the days per month differ according to Gregorian rules, so it's
    /// inappropriate for tracking time according to Elona's world, where all months are 31 days long.
    /// </remarks>
    [DataDefinition]
    public struct GameDateTime
    {
        public const long SecondsPerMinute = 60;
        public const long SecondsPerHour   = 60 * 60;
        public const long SecondsPerDay    = 60 * 60 * 24;
        public const long SecondsPerMonth  = 60 * 60 * 24 * 31;
        public const long SecondsPerYear   = 60 * 60 * 24 * 31 * 12;

        public static GameDateTime Zero => new(0);
        public static GameDateTime MinValue => new(long.MinValue);
        public static GameDateTime MaxValue => new(long.MaxValue);

        public GameDateTime()
        {
            TotalSeconds = 0;
        }

        public GameDateTime(int year, int month, int day, int hour = 0, int minute = 0, int second = 0)
        {
            if (year < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(year));
            }
            if (month < 1 || month > 12)
            {
                throw new ArgumentOutOfRangeException(nameof(month));
            }
            if (day < 1 || day > 31)
            {
                throw new ArgumentOutOfRangeException(nameof(day));
            }
            if (hour < 0 || hour > 24)
            {
                throw new ArgumentOutOfRangeException(nameof(hour));
            }
            if (minute < 0 || minute > 60)
            {
                throw new ArgumentOutOfRangeException(nameof(minute));
            }
            if (second < 0 || second > 60)
            {
                throw new ArgumentOutOfRangeException(nameof(second));
            }

            TotalSeconds = second
                + minute * SecondsPerMinute
                + hour * SecondsPerHour
                + (day - 1) * SecondsPerDay
                + (month - 1) * SecondsPerMonth
                + (year - 1) * SecondsPerYear;
        }

        public GameDateTime(long totalSeconds)
        {
            if (totalSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalSeconds));
            }

            TotalSeconds = totalSeconds;
        }

        public GameDateTime(GameDateTime other)
        {
            TotalSeconds = other.TotalSeconds;
        }

        /// <summary>
        /// Number of seconds since 0001/01/01 in in-game time.
        /// </summary>
        [DataField]
        public long TotalSeconds { get; private set; }

        public long TotalYears => TotalSeconds / SecondsPerYear;

        public long TotalMonths => TotalSeconds / SecondsPerMonth;

        public long TotalDays => TotalSeconds / SecondsPerDay;

        public long TotalHours => TotalSeconds / SecondsPerHour;

        public long TotalMinutes => TotalSeconds / SecondsPerMinute;

        /// <summary>
        /// Current in-game year.
        /// </summary>
        public int Year   => (int)(TotalYears + 1);

        /// <summary>
        /// Current in-game month.
        /// </summary>
        public int Month  => (int)(TotalMonths % 12 + 1);

        /// <summary>
        /// Current in-game day.
        /// </summary>
        public int Day    => (int)(TotalDays % 31 + 1);

        /// <summary>
        /// Current in-game hour.
        /// </summary>
        public int Hour   => (int)(TotalHours % 24);

        /// <summary>
        /// Current in-game minute.
        /// </summary>
        public int Minute => (int)(TotalMinutes % 60);

        /// <summary>
        /// Current in-game second.
        /// </summary>
        public int Second => (int)(TotalSeconds % 60);

        public static bool operator ==(GameDateTime? lhs, GameDateTime? rhs)
        {
            return lhs?.TotalSeconds == rhs?.TotalSeconds;
        }

        public static bool operator !=(GameDateTime? lhs, GameDateTime? rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >(GameDateTime lhs, GameDateTime rhs)
        {
            return lhs.TotalSeconds > rhs.TotalSeconds;
        }

        public static bool operator <(GameDateTime lhs, GameDateTime rhs)
        {
            return lhs.TotalSeconds < rhs.TotalSeconds;
        }

        public static bool operator >=(GameDateTime lhs, GameDateTime rhs)
        {
            return lhs.TotalSeconds >= rhs.TotalSeconds;
        }

        public static bool operator <=(GameDateTime lhs, GameDateTime rhs)
        {
            return lhs.TotalSeconds <= rhs.TotalSeconds;
        }

        public static GameDateTime operator +(GameDateTime dateTime, GameTimeSpan timeSpan)
        {
            return new(dateTime.TotalSeconds + timeSpan.TotalSeconds);
        }

        public static GameDateTime operator -(GameDateTime dateTime, GameTimeSpan timeSpan)
        {
            return new(dateTime.TotalSeconds - timeSpan.TotalSeconds);
        }

        public static GameTimeSpan operator +(GameDateTime dateTime, GameDateTime timeSpan)
        {
            return new(dateTime.TotalSeconds + timeSpan.TotalSeconds);
        }

        public static GameTimeSpan operator -(GameDateTime dateTime, GameDateTime timeSpan)
        {
            return new(dateTime.TotalSeconds - timeSpan.TotalSeconds);
        }

        public override bool Equals(object? other)
        {
            if (other is GameDateTime otherTime)
                return this == otherTime;
            
            return false;
        }

        public override int GetHashCode()
        {
            return TotalSeconds.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Month}/{Day}/{Year} {Hour}:{Minute}:{Second}";
        }
    }
}
