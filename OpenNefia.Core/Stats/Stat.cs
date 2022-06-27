using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Stats
{
    public class Stat<T> : IEquatable<Stat<T>>
    {
        private T _base;
        private T _buffed;
     
        public bool IsBuffed { get; private set; }

        public T Base
        {
            get => _base;
            set 
            {
                _base = value;
                if (!IsBuffed)
                    _buffed = value;
            }
        }
        public T Buffed
        {
            get => _buffed;
            set
            {
                if (!_buffed?.Equals(value) ?? false)
                    IsBuffed = true;
                
                _buffed = value;
            }
        }

        // Needed for copying in de/serialization
        public Stat() : this(default!, default!) { }

        public Stat(T baseValue) : this (baseValue, baseValue) {}

        public Stat(T baseValue, T buffedValue)
        {
            _base = baseValue;
            _buffed = buffedValue;
            IsBuffed = !baseValue?.Equals(buffedValue) ?? false;
        }

        public virtual void Reset()
        {
            _buffed = _base;
            IsBuffed = false;
        }

        public virtual bool Equals(Stat<T>? other)
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

        public static bool operator >(Stat<T>? left, Stat<T>? right)
        {
            if (left is null)
                return false;
            if (right is null)
                return true;
            if (left.Buffed is IComparable<T> leftComp)
            {
                return leftComp.CompareTo(right.Buffed) > 0;
            }
            return false;
        }

        public static bool operator <(Stat<T>? left, Stat<T>? right)
        {
            if (left is null)
                return true;
            if (right is null)
                return false;
            if (left is IComparable<T> leftComp)
            {
                return leftComp.CompareTo(right.Buffed) < 0;
            }
            return false;
        }

        public static bool operator >=(Stat<T>? left, Stat<T>? right)
        {
            if (left is null)
                return false;
            if (right is null)
                return true;
            if (left.Buffed is IComparable<T> leftComp)
            {
                return leftComp.CompareTo(right.Buffed) >= 0;
            }
            return false;
        }

        public static bool operator <=(Stat<T>? left, Stat<T>? right)
        {
            if (left is null)
                return true;
            if (right is null)
                return false;
            if (left is IComparable<T> leftComp)
            {
                return leftComp.CompareTo(right.Buffed) <= 0;
            }
            return false;
        }

        public static bool operator ==(Stat<T>? left, T? right)
        {
            return left?.Equals(right) ?? false;
        }

        public static bool operator !=(Stat<T>? left, T? right)
        {
            return !(left == right);
        }

        public static bool operator >(Stat<T>? left, T? right)
        {
            if (left is null)
                return false;
            if (right is null)
                return true;
            if (left.Buffed is IComparable<T> leftComp)
            {
                return leftComp.CompareTo(right) > 0;
            }
            return false;
        }

        public static bool operator <(Stat<T>? left, T? right)
        {
            if (left is null)
                return true;
            if (right is null)
                return false;
            if (left is IComparable<T> leftComp)
            {
                return leftComp.CompareTo(right) < 0;
            }
            return false;
        }

        public static bool operator >=(Stat<T>? left, T? right)
        {
            if (left is null)
                return false;
            if (right is null)
                return true;
            if (left is IComparable<T> leftComp)
            {
                return leftComp.CompareTo(right) >= 0;
            }
            return false;
        }

        public static bool operator <=(Stat<T>? left, T? right)
        {
            if (left is null)
                return true;
            if (right is null)
                return false;
            if (left is IComparable<T> leftComp)
            {
                return leftComp.CompareTo(right) <= 0;
            }
            return false;
        }

        public override string ToString()
        {
            return $"{Buffed}({Base})";
        }
    }
}
