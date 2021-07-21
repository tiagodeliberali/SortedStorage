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
        private readonly IFileWriterPort file;

        public Memtable(IFileWriterPort file)
        {
            this.file = file;
            LoadFile();
        }

        private void LoadFile()
        {
            file.Position = 0;
            while (file.HasContent())
            {
                KeyValueEntry entry = KeyValueEntry.FromFileReader(file);
                sortedDictionary.Add(entry.Key, entry.Value);
            }
        }

        public bool IsFull() => sortedDictionary.Count >= MAX_SIZE;

        public void Add(string key, string value)
        {
            file.Append(KeyValueEntry.ToBytes(key, value));
            sortedDictionary.Add(key, value);
        }

        public IEnumerable<KeyValuePair<string, string>> GetData() => sortedDictionary.AsEnumerable();

        public string Get(string key) => sortedDictionary.GetValueOrDefault(key);

        public void Delete() => file?.Delete();

        public string GetFileName() => file.Name;

        public void Dispose() => file?.Dispose();

        public ImutableMemtable ToImutable() => new ImutableMemtable(file.ToReadOnly(FileType.MemtableReadOnly), sortedDictionary);
    }
}
