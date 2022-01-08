using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Stats
{
    public class Stat<T> : IEquatable<Stat<T>>
    {
        public T Base { get; set; }
        public T Buffed { get; set; }

        public Stat(T baseValue) : this (baseValue, baseValue) {}

        public Stat(T baseValue, T buffedValue)
        {
            Base = baseValue;
            Buffed = buffedValue;
        }

        public void Reset()
        {
            Buffed = Base;
        }

        public bool Equals(Stat<T>? other)
        {
            if (other is null)
                return false;

            if (Base is IEquatable<T>)
            {
                return Base.Equals(other.Base);
            }

            return false;
        }

        public override bool Equals(object? other)
        {
            if (other is not Stat<T> otherStat)
                return false;

            return this.Equals(otherStat);
        }

        public override int GetHashCode()
        {
            return Base?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Stat<T>? left, Stat<T>? right)
        {
            return left?.Equals(right) ?? false;
        }

        public static bool operator !=(Stat<T>? left, Stat<T>? right)
        {
            return !(left == right);
        }

        public static implicit operator T(Stat<T> stat)
        {
            return stat.Base;
        }

        public static implicit operator Stat<T>(T value)
        {
            return new(value);
        }
    }
}
