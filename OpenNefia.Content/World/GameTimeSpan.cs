using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.World
{
    /// <summary>
    /// Time span struct for Elona's in-game world.
    /// </summary>
    [DataDefinition]
    public struct GameTimeSpan
    {
        public const long SecondsPerMinute = 60;
        public const long SecondsPerHour   = 60 * 60;
        public const long SecondsPerDay    = 60 * 60 * 24;
        public const long SecondsPerMonth  = 60 * 60 * 24 * 31;
        public const long SecondsPerYear   = 60 * 60 * 24 * 31 * 12;

        public static GameTimeSpan Zero => new(0);
        public static GameTimeSpan MinValue => new(long.MinValue);
        public static GameTimeSpan MaxValue => new(long.MaxValue);

        public static GameTimeSpan FromSeconds(int seconds) => new(0, 0, seconds);
        public static GameTimeSpan FromMinutes(int minutes) => new(0, minutes, 0);
        public static GameTimeSpan FromHours(int hours) => new(hours, 0, 0);
        public static GameTimeSpan FromDays(int days) => new(0, 0, days, 0, 0, 0);
        public static GameTimeSpan FromMonths(int months) => new(0, months, 0, 0, 0, 0);
        public static GameTimeSpan FromYears(int years) => new(years, 0, 0, 0, 0, 0);

        public GameTimeSpan()
        {
            TotalSeconds = 0;
        }

        public GameTimeSpan(int hours, int minutes, int seconds)
            : this(0, 0, 0, hours, minutes, seconds) {}

        public GameTimeSpan(int years, int months, int days, int hours, int minutes, int seconds)
        {
            TotalSeconds = seconds
                + minutes * SecondsPerMinute
                + hours * SecondsPerHour
                + days * SecondsPerDay
                + months * SecondsPerMonth
                + years * SecondsPerYear;
        }

        public GameTimeSpan(long totalSeconds)
        {
            TotalSeconds = Math.Max(totalSeconds, 0);
        }

        public GameTimeSpan(GameTimeSpan other)
        {
            TotalSeconds = other.TotalSeconds;
        }

        /// <summary>
        /// Number of seconds since 0001/01/01 in in-game time.
        /// </summary>
        [DataField]
        public long TotalSeconds { get; private set; }

        /// <summary>
        /// Gets the years component of the time interval this time span represents.
        /// </summary>
        public int Year => (int)(TotalSeconds / SecondsPerYear);

        /// <summary>
        /// Gets the months component of the time interval this time span represents.
        /// </summary>
        public int Month => (int)((TotalSeconds / SecondsPerMonth) % 12);

        /// <summary>
        /// Gets the days component of the time interval this time span represents.
        /// </summary>
        public int Day    => (int)((TotalSeconds / SecondsPerDay) % 31);

        /// <summary>
        /// Gets the hours component of the time interval this time span represents.
        /// </summary>
        public int Hour   => (int)((TotalSeconds / SecondsPerHour) % 24);

        /// <summary>
        /// Gets the minutes component of the time interval this time span represents.
        /// </summary>
        public int Minute => (int)((TotalSeconds / SecondsPerMinute) % 60);

        /// <summary>
        /// Gets the seconds component of the time interval this time span represents.
        /// </summary>
        public int Second => (int)(TotalSeconds % 60);

        public int TotalYears => (int)(TotalSeconds / SecondsPerYear);
        public int TotalMonths => (int)(TotalSeconds / SecondsPerMonth);
        public int TotalDays => (int)(TotalSeconds / SecondsPerDay);
        public int TotalHours => (int)((TotalSeconds / SecondsPerHour));
        public int TotalMinutes => (int)((TotalSeconds / SecondsPerMinute));

        public static bool operator ==(GameTimeSpan? lhs, GameTimeSpan? rhs)
        {
            return lhs?.TotalSeconds == rhs?.TotalSeconds;
        }

        public static bool operator !=(GameTimeSpan? lhs, GameTimeSpan? rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >(GameTimeSpan lhs, GameTimeSpan rhs)
        {
            return lhs.TotalSeconds > rhs.TotalSeconds;
        }

        public static bool operator <(GameTimeSpan lhs, GameTimeSpan rhs)
        {
            return lhs.TotalSeconds < rhs.TotalSeconds;
        }

        public static bool operator >=(GameTimeSpan lhs, GameTimeSpan rhs)
        {
            return lhs.TotalSeconds >= rhs.TotalSeconds;
        }

        public static bool operator <=(GameTimeSpan lhs, GameTimeSpan rhs)
        {
            return lhs.TotalSeconds <= rhs.TotalSeconds;
        }

        public static GameTimeSpan operator +(GameTimeSpan lhs, GameTimeSpan rhs)
        {
            return new GameTimeSpan(lhs.TotalSeconds + rhs.TotalSeconds);
        }

        public static GameTimeSpan operator -(GameTimeSpan lhs, GameTimeSpan rhs)
        {
            return new GameTimeSpan(lhs.TotalSeconds - rhs.TotalSeconds);
        }

        public override bool Equals(object? other)
        {
            if (other is GameTimeSpan otherTime)
                return this == otherTime;
            
            return false;
        }

        public override int GetHashCode()
        {
            return TotalSeconds.GetHashCode();
        }
    }
}
