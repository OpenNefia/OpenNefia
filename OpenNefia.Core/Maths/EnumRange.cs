using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Maths
{
    public struct EnumRange<T> : IEquatable<EnumRange<T>>
        where T : Enum
    {
        public T Min;
        public T Max;

        public EnumRange(T min, T max)
        {
            Min = min;
            Max = max;
        }

        public T TrueMin => EnumHelpers.Min(Min, Max);
        public T TrueMax => EnumHelpers.Max(Min, Max);

        public override string ToString()
        {
            return $"({Min}~{Max})";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Min, Max);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not EnumRange<T> other)
                return false;

            return Equals(other);
        }

        public bool Equals(EnumRange<T> other)
        {
            return Min.Equals(other.Min) && Max.Equals(other.Max);
        }

        public static bool operator ==(EnumRange<T> lhs, EnumRange<T> rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(EnumRange<T> lhs, EnumRange<T> rhs)
        {
            return !(lhs == rhs);
        }

        public readonly void Deconstruct(out T min, out T max)
        {
            min = Min;
            max = Max;
        }

        public bool Includes(T val)
        {
            return Clamp(val).Equals(val);
        }

        public T Clamp(T val)
        {
            return EnumHelpers.Clamp(val, Min, Max);
        }
    }
}
