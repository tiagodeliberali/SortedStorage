using System.Collections.Generic;

namespace SortedStorage
{
    class Memtable
    {
        private const int MAX_SIZE = 3;
        private readonly SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();

        internal bool IsFull() => sortedDictionary.Count > MAX_SIZE;

        internal void Add(string key, string value) => sortedDictionary.Add(key, value);

        internal string Get(string key) => sortedDictionary.GetValueOrDefault(key);
    }
}
