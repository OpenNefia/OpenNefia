using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Stats
{
    public abstract class SetStat<TSet, TReadOnlySet, TValue> : ISet<TValue>, IReadOnlySet<TValue>
        where TSet : ISet<TValue>, TReadOnlySet, new()
        where TReadOnlySet : IReadOnlySet<TValue>
    {
        private TSet _base;
        private TSet _buffed;

        public bool IsBuffed { get; private set; }

        public TReadOnlySet Base => _base;
        public TReadOnlySet Buffed => _buffed;

        // Needed for copying in de/serialization
        public SetStat() : this(new(), new()) { }

        public SetStat(TSet baseValue)
        {
            _base = baseValue;
            _buffed = new TSet();
            foreach (var item in _base)
                _buffed.Add(item);
        }

        public SetStat(TSet baseValue, TSet buffedValue)
        {
            _base = baseValue;
            _buffed = buffedValue;

            if (_base.Equals(_buffed))
            {
                _buffed = new TSet();
                foreach (var item in _base)
                    _buffed.Add(item);
            }

            IsBuffed = !((ISet<TValue>)baseValue)?.SetEquals(buffedValue) ?? false;
        }

        public virtual void Reset()
        {
            _buffed.Clear();
            foreach (var item in _base)
                _buffed.Add(item);

            _buffed = _base;
            IsBuffed = false;
        }

        public virtual bool Equals(SetStat<TSet, TReadOnlySet, TValue>? other)
        {
            if (other is null)
                return false;

            if (Base is IEquatable<TSet>)
            {
                return Base.Equals(other.Base);
            }

            return false;
        }

        public override bool Equals(object? other)
        {
            if (other is not SetStat<TSet, TReadOnlySet, TValue> otherStat)
                return false;

            return this.Equals(otherStat);
        }

        public override int GetHashCode()
        {
            return Base?.GetHashCode() ?? 0;
        }

        public bool Add(TValue item)
        {
            IsBuffed = true;
            return _buffed.Add(item);
        }

        public bool AddBase(TValue item)
        {
            var result = _base.Add(item);
            if (!IsBuffed)
                _buffed.Add(item);
            return result;
        }

        public void ExceptWith(IEnumerable<TValue> other)
        {
            IsBuffed = true;
            _buffed.ExceptWith(other);
        }

        public void IntersectWith(IEnumerable<TValue> other)
        {
            IsBuffed = true;
            _buffed.IntersectWith(other);
        }

        public bool IsProperSubsetOf(IEnumerable<TValue> other) => Buffed.IsProperSubsetOf(other);
        public bool IsProperSupersetOf(IEnumerable<TValue> other) => Buffed.IsProperSupersetOf(other);
        public bool IsSubsetOf(IEnumerable<TValue> other) => Buffed.IsSubsetOf(other);
        public bool IsSupersetOf(IEnumerable<TValue> other) => Buffed.IsSupersetOf(other);
        public bool Overlaps(IEnumerable<TValue> other) => Buffed.Overlaps(other);
        public bool SetEquals(IEnumerable<TValue> other) => Buffed.SetEquals(other);

        public void SymmetricExceptWith(IEnumerable<TValue> other)
        {
            IsBuffed = true;
            _buffed.SymmetricExceptWith(other);
        }

        public void UnionWith(IEnumerable<TValue> other)
        {
            IsBuffed = true;
            _buffed.UnionWith(other);
        }

        void ICollection<TValue>.Add(TValue item)
        {
            IsBuffed = true;
            _buffed.Add(item);
        }

        public void Clear()
        {
            IsBuffed = true;
            _buffed.Clear();
        }

        public void ClearBase()
        {
            _base.Clear();
            if (!IsBuffed)
                _buffed.Clear();
        }

        public bool Contains(TValue item) => Buffed.Contains(item);

        public void CopyTo(TValue[] array, int arrayIndex) => _buffed.CopyTo(array, arrayIndex);

        public bool Remove(TValue item)
        {
            IsBuffed = true;
            return _buffed.Remove(item);
        }

        public bool RemoveBase(TValue item)
        {
            var result = _base.Remove(item);
            if (!IsBuffed)
                _buffed.Remove(item);
            return result;
        }

        public int Count => Buffed.Count;

        public bool IsReadOnly => false;

        public IEnumerator<TValue> GetEnumerator() => _buffed.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _buffed.GetEnumerator();
    }

    public class HashSetStat<T> : SetStat<HashSet<T>, HashSet<T>, T>
    {
        public HashSetStat()
        {
        }

        public HashSetStat(HashSet<T> baseValue) : base(baseValue)
        {
        }

        public HashSetStat(HashSet<T> baseValue, HashSet<T> buffedValue) : base(baseValue, buffedValue)
        {
        }
    }
}
