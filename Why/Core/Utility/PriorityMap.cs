using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace OpenNefia.Core.Utility
{
    /// <summary>
    /// A dictionary where each key can be assigned a non-unique priority value for enumeration.
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    /// <typeparam name="P"></typeparam>
    public class PriorityMap<K, V, P> : IDictionary<K, V>, IEnumerable<KeyValuePair<K, V>> 
        where K : notnull 
        where P : notnull, IComparable<P>, IEquatable<P>, new()
    {
        private IDictionary<K, V> Inner = new Dictionary<K, V>();
        private Dictionary<K, P> Priorities = new Dictionary<K, P>();
        private List<K> SortedKeys = new List<K>();
        private bool Dirty = true;

        public ICollection<K> Keys => Inner.Keys;

        public ICollection<V> Values => Inner.Values;

        public int Count => Inner.Count;

        public bool IsReadOnly => Inner.IsReadOnly;

        public void Clear()
        {
            this.Inner.Clear();
            this.Priorities.Clear();
            this.SortedKeys.Clear();
            this.Dirty = true;
        }

        public void Add(K key, V value, P priority)
        {
            Inner.Add(key, value);
            this.Priorities[key] = priority;
            this.Dirty = true;
        }

        public void Add(K key, V value) => Add(key, value, new P());

        public void Add(KeyValuePair<K, V> item, P priority)
        {
            Inner.Add(item);
            this.Priorities[item.Key] = priority;
            this.Dirty = true;
        }

        public void Add(KeyValuePair<K, V> item) => Add(item, new P());

        public bool Remove(K key)
        {
            this.Dirty = true;
            this.Priorities.Remove(key);
            return Inner.Remove(key);
        }

        public bool Remove(KeyValuePair<K, V> item)
        {
            this.Dirty = true;
            this.Priorities.Remove(item.Key);
            return Inner.Remove(item);
        }

        public V this[K key]
        {
            get => this.Inner[key];
            set {
                this.Inner[key] = value;
                this.Priorities[key] = new P();
                this.Dirty = true;
            }
        }

        internal void SetPriority(K key, P priority)
        {
            if (!this.Priorities[key].Equals(priority))
            {
                this.Priorities[key] = priority;
                this.Dirty = true;
            }
        }

        private void UpdateSorting()
        {
            var pairs = Priorities.ToList();
            SortedKeys = pairs.OrderBy(pair => pair.Value).Select(pair => pair.Key).ToList();
            this.Dirty = false;
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            if (this.Dirty)
                UpdateSorting();

            foreach (var key in this.SortedKeys) 
            {
                yield return new KeyValuePair<K, V>(key, Inner[key]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (this.Dirty)
                UpdateSorting();

            foreach (var key in this.SortedKeys)
            {
                yield return new KeyValuePair<K, V>(key, Inner[key]);
            }
        }

        public bool ContainsKey(K key)
        {
            return Inner.ContainsKey(key);
        }

        public bool TryGetValue(K key, [MaybeNullWhen(false)] out V value)
        {
            return Inner.TryGetValue(key, out value);
        }

        public bool Contains(KeyValuePair<K, V> item)
        {
            return Inner.Contains(item);
        }

        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            Inner.CopyTo(array, arrayIndex);
        }
    }
}
