using System;
using System.Collections.Generic;
using System.IO;

namespace SortedStorage
{
    class Memtable : IDisposable
    {
        private const int MAX_SIZE = 3;
        private readonly SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
        private readonly FileStream file;

        public Memtable(string basePath)
        {
            string path = Path.Combine(basePath, $"{Guid.NewGuid()}.dat");
            file = new FileStream(path, FileMode.Append, FileAccess.Write);
        }

        internal bool IsFull() => sortedDictionary.Count > MAX_SIZE;

        internal void Add(string key, string value)
        {
            AppendToFile(key, value);
            sortedDictionary.Add(key, value);
        }

        private void AppendToFile(string key, string value)
        {
            var keyValue = KeyValueRegister.ToBytes(key, value);
            file.Write(keyValue, 0, keyValue.Length);
            file.Flush();
        }

        internal string Get(string key) => sortedDictionary.GetValueOrDefault(key);

        public void Dispose()
        {
            if (file != null) file.Dispose();
        }
    }
}
