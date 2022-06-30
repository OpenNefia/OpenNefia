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
    /// Time span struct for Elona's in-game world.
    /// </summary>
    [DataDefinition]
    public class GameTimeSpan
    {
        public const long SecondsPerMinute = 60;
        public const long SecondsPerHour   = 60 * 60;
        public const long SecondsPerDay    = 60 * 60 * 24;
        
        public static GameTimeSpan Zero => new(0);
        public static GameTimeSpan MinValue => new(long.MinValue);
        public static GameTimeSpan MaxValue => new(long.MaxValue);

        public static GameTimeSpan FromHours(int hours) => new(hours, 0, 0);
        public static GameTimeSpan FromDays(int days) => new(days * 24, 0, 0);

        public GameTimeSpan()
        {
            TotalSeconds = 0;
        }

        public GameTimeSpan(int hours, int minutes, int seconds)
        {
            Set(hours, minutes, seconds);
        }

        public GameTimeSpan(long totalSeconds)
        {
            if (totalSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(totalSeconds));
            }

            TotalSeconds = totalSeconds;
        }

        public GameTimeSpan(GameTimeSpan other)
        {
            TotalSeconds = other.TotalSeconds;
        }

        public void Set(int hours, int minutes, int seconds)
        {
            TotalSeconds = seconds
                + minutes * SecondsPerMinute
                + hours * SecondsPerHour;
        }

        /// <summary>
        /// Number of seconds since 0001/01/01 in in-game time.
        /// </summary>
        [DataField]
        public long TotalSeconds { get; private set; }

        /// <summary>
        /// Gets the days component tof the time interval this time span represents.
        /// </summary>
        public int Day    => (int)(TotalSeconds / SecondsPerDay);

        /// <summary>
        /// Gets the hours component tof the time interval this time span represents.
        /// </summary>
        public int Hour   => (int)((TotalSeconds / SecondsPerHour) % 24);

        /// <summary>
        /// Gets the minutes component tof the time interval this time span represents.
        /// </summary>
        public int Minute => (int)((TotalSeconds / SecondsPerMinute) % 60);

        /// <summary>
        /// Gets the seconds component tof the time interval this time span represents.
        /// </summary>
        public int Second => (int)(TotalSeconds % 60);

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
