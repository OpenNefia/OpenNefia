using System;
using OpenNefia.Core.Serialization;

namespace OpenNefia.Core.Input
{
    public enum BoundKeyState : byte
    {
        Up = 0,
        Down = 1
    }

    [Serializable]
    public struct BoundKeyFunction : IComparable, IComparable<BoundKeyFunction>, IEquatable<BoundKeyFunction>, ISelfSerialize
    {
        public readonly string FunctionName;

        public BoundKeyFunction(string name)
        {
            FunctionName = name;
        }

        public static implicit operator BoundKeyFunction(string name)
        {
            return new(name);
        }

        public override readonly string ToString()
        {
            return $"KeyFunction({FunctionName})";
        }

        #region Code for easy equality and sorting.

        public readonly int CompareTo(object? obj)
        {
            if (!(obj is BoundKeyFunction func))
            {
                return 1;
            }
            return CompareTo(func);
        }

        public readonly int CompareTo(BoundKeyFunction other)
        {
            return string.Compare(FunctionName, other.FunctionName, StringComparison.InvariantCultureIgnoreCase);
        }

        // Could maybe go dirty and optimize these on the assumption that they're singletons.
        public override readonly bool Equals(object? obj)
        {
            return obj is BoundKeyFunction func && Equals(func);
        }

        public readonly bool Equals(BoundKeyFunction other)
        {
            return other.FunctionName == FunctionName;
        }

        public override readonly int GetHashCode()
        {
            return FunctionName.GetHashCode();
        }

        public static bool operator ==(BoundKeyFunction a, BoundKeyFunction b)
        {
            return a.FunctionName == b.FunctionName;
        }

        public static bool operator !=(BoundKeyFunction a, BoundKeyFunction b)
        {
            return !(a == b);
        }

        #endregion

        public void Deserialize(string value)
        {
            this = new BoundKeyFunction(value);
        }

        public readonly string Serialize()
        {
            return FunctionName;
        }
    }

    /// <summary>
    ///     Makes all constant strings on this static class be added as input functions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class KeyFunctionsAttribute : Attribute { }
}
