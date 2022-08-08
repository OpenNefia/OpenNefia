using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Utility
{
    public enum ComparisonType : byte
    {
        Equal = 0,
        NotEqual = 1,
        Greater = 2,
        GreaterOrEqual = 3,
        Less = 4,
        LessOrEqual = 5
    }

    public static class ComparisonUtils
    {
        public static bool EvaluateComparison<T>(T a, T b, ComparisonType comparison)
            where T : IComparable<T>, IEquatable<T>
        {
            return comparison switch
            {
                ComparisonType.Equal => a.Equals(b),
                ComparisonType.NotEqual => !a.Equals(b),
                ComparisonType.Greater => a.CompareTo(b) > 0,
                ComparisonType.GreaterOrEqual => a.CompareTo(b) >= 0,
                ComparisonType.Less => a.CompareTo(b) < 0,
                ComparisonType.LessOrEqual => a.CompareTo(b) <= 0,
                _ => false
            };
        }
    }
}
