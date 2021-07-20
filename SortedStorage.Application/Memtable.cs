using SortedStorage.Application.Port.Out;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SortedStorage.Application
{
    public class Memtable : IDisposable
    {
        private const int MAX_SIZE = 2;
        private readonly SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
        private readonly IFileWriterPort filePort;

        public Memtable(IFileWriterPort filePort)
        {
            this.filePort = filePort;
            LoadFile();
        }

        private void LoadFile()
        {
            filePort.Position = 0;
            while (filePort.HasContent())
            {
                KeyValueEntry entry = KeyValueEntry.FromFileReader(filePort);
                sortedDictionary.Add(entry.Key, entry.Value);
            }
        }

        public bool IsFull() => sortedDictionary.Count >= MAX_SIZE;

        public void Add(string key, string value)
        {
            filePort.Append(KeyValueEntry.ToBytes(key, value));
            sortedDictionary.Add(key, value);
        }

        public IEnumerable<KeyValuePair<string, string>> GetData() => sortedDictionary.AsEnumerable();

        public string Get(string key) => sortedDictionary.GetValueOrDefault(key);

        public void Delete() => filePort?.Delete();

        public void Dispose() => filePort?.Dispose();

        public string GetFileName() => filePort.Name;
    }
}
