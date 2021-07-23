using SortedStorage.Application.Port.Out;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SortedStorage.Application
{
    public class Memtable : IDisposable
    {
        private const int MAX_SIZE = 2;
        private readonly SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
        private readonly IFileWriterPort file;

        private bool isReadOnly = false;

        private Memtable(IFileWriterPort file)
        {
            this.file = file;
        }

        public static async Task<Memtable> LoadFromFile(IFileWriterPort file)
        {
            var memtable = new Memtable(file);
            await memtable.LoadFile();

            return memtable;
        }

        private async Task LoadFile()
        {
            file.Position = 0;
            while (file.HasContent())
            {
                KeyValueEntry entry = await KeyValueEntry.FromFileReader(file);
                sortedDictionary[entry.Key] = entry.Value;
            }
        }

        public bool IsFull() => sortedDictionary.Count >= MAX_SIZE;

        public async Task Add(string key, string value)
        {
            if (value == StorageConfiguration.TOMBSTONE)
                throw new InvalidEntryValueException($"Invalid value '{value}'. It is used as tombstone.");

            await AddEntryWithLock(key, value);
        }

        public async Task Remove(string key) => await AddEntryWithLock(key, StorageConfiguration.TOMBSTONE);

        private async Task AddEntryWithLock(string key, string value)
        {
            await Task.Factory.StartNew(() =>
            {
                lock (file)
                {
                    if (isReadOnly) 
                        throw new InvalidWriteToReadOnlyException("Tried to add entry to read only memtable");

                    file.Append(KeyValueEntry.ToBytes(key, value));
                    sortedDictionary[key] = value;
                }
            });
        }

        public IEnumerable<KeyValuePair<string, string>> GetData() => sortedDictionary.AsEnumerable();

        public string Get(string key) => sortedDictionary.GetValueOrDefault(key);

        public void DeleteFile() => file?.Delete();

        public string GetFileName() => file.Name;

        public void Dispose() => file?.Dispose();

        public ImutableMemtable ToImutable()
        {
            lock (file)
            {
                isReadOnly = true;
                return new ImutableMemtable(file.ToReadOnly(FileType.MemtableReadOnly), sortedDictionary);
            }
        }
    }
}
