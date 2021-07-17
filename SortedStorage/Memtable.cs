using System.Collections.Generic;

namespace SortedStorage
{
    class Memtable<TKey, TValue>
    {
        private const int MAX_SIZE = 3;
        private readonly SortedDictionary<TKey, TValue> sortedDictionary = new SortedDictionary<TKey, TValue>();

        internal bool IsFull() => sortedDictionary.Count > MAX_SIZE;

        internal void Add(TKey key, TValue value) => sortedDictionary.Add(key, value);

        internal TValue Get(TKey key) => sortedDictionary.GetValueOrDefault(key);
    }
}
