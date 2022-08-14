using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Maths
{
    public struct IntRange : IEquatable<IntRange>
    {
        public int Min;
        public int Max;

        public IntRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public static readonly IntRange Zero = new(0, 0);
        public static readonly IntRange One = new(1, 1);

        public int TrueMin => Math.Min(Min, Max);
        public int TrueMax => Math.Max(Min, Max);

        public override string ToString()
        {
            return $"{Min}~{Max}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Min, Max);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not IntRange other)
                return false;

            return Equals(other);
        }

        public bool Equals(IntRange other)
        {
            return Min == other.Min && Max == other.Max;
        }

        public static bool operator ==(IntRange lhs, IntRange rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(IntRange lhs, IntRange rhs)
        {
            return !(lhs == rhs);
        }

        public bool Includes(int val)
        {
            return val >= Min && val <= Max;
        }

        public int Clamp(int val)
        {
            return Math.Clamp(val, Min, Max);
        }
    }
}
