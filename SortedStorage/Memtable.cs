using System;
using System.Collections.Generic;

namespace SortedStorage
{
    class Memtable : IDisposable
    {
        private const int MAX_SIZE = 3;
        private readonly SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
        private readonly IFilePort filePort;

        public Memtable(IFilePort filePort) => this.filePort = filePort;

        internal bool IsFull() => sortedDictionary.Count > MAX_SIZE;

        internal void Add(string key, string value)
        {
            filePort.Append(KeyValueRegister.ToBytes(key, value));
            sortedDictionary.Add(key, value);
        }

        internal string Get(string key) => sortedDictionary.GetValueOrDefault(key);

        public void Dispose()
        {
            if (filePort != null) filePort.Dispose();
        }
    }
}
