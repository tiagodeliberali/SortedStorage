using SortedStorage.Application.Port.Out;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SortedStorage.Application
{
    class Memtable : IDisposable
    {
        private const int MAX_SIZE = 3;
        private readonly SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
        private readonly IFileWritePort filePort;

        public Memtable(IFileWritePort filePort) => this.filePort = filePort;

        public bool IsFull() => sortedDictionary.Count >= MAX_SIZE;

        public void Add(string key, string value)
        {
            filePort.Append(KeyValueEntry.ToBytes(key, value));
            sortedDictionary.Add(key, value);
        }

        public IEnumerable<KeyValuePair<string, string>> GetData() => sortedDictionary.AsEnumerable();

        public string Get(string key) => sortedDictionary.GetValueOrDefault(key);

        public void Dispose() => filePort?.Dispose();
    }
}
