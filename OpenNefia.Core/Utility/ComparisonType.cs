﻿using System;
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
        GreaterThan = 2,
        GreaterThanOrEqual = 3,
        LessThan = 4,
        LessThanOrEqual = 5
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
                ComparisonType.GreaterThan => a.CompareTo(b) > 0,
                ComparisonType.GreaterThanOrEqual => a.CompareTo(b) >= 0,
                ComparisonType.LessThan => a.CompareTo(b) < 0,
                ComparisonType.LessThanOrEqual => a.CompareTo(b) <= 0,
                _ => false
            };
        }
    }
}
