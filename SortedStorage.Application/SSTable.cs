using SortedStorage.Application.Port.Out;
using System;
using System.Collections.Generic;

namespace SortedStorage.Application
{
    class SSTable : IDisposable
    {
        private readonly IFileReaderPort dataFile;
        private readonly Dictionary<string, long> index;

        private SSTable(IFileReaderPort dataFile, Dictionary<string, long> index)
        {
            this.dataFile = dataFile;
            this.index = index;
        }

        public string GetFileName() => dataFile.GetName();

        public string Get(string key)
        {
            if (!index.ContainsKey(key)) return null;

            var position = index[key];

            var keyValue = KeyValueEntry.FromBytes(dataFile, position);

            return keyValue.Value;
        }

        public IEnumerable<KeyValuePair<string, string>> GetAll()
        {
            foreach (var item in index)
            {
                var keyValue = KeyValueEntry.FromBytes(dataFile, item.Value);
                yield return KeyValuePair.Create(keyValue.Key, keyValue.Value);
            }
        }

        public SSTable Merge(SSTable otherTable, IFileManagerPort fileManager)
        {
            PriorityEnumerator priorityEnumerator = new PriorityEnumerator(
                new IEnumerable<KeyValuePair<string, string>>[] { otherTable.GetAll(), GetAll() });

            string filename = Guid.NewGuid().ToString();
            Dictionary<string, long> index = new Dictionary<string, long>();

            using (IFileWriterPort dataFile = fileManager.OpenOrCreateToWrite($"{filename}.dat"))
            using (IFileWriterPort indexFile = fileManager.OpenOrCreateToWrite($"{filename}.idx"))
            {
                foreach (var item in priorityEnumerator.GetAll())
                {
                    BuildFile(dataFile, indexFile, item, index);
                }
            }

            return new SSTable(fileManager.OpenToRead($"{filename}.dat"), index);
        }

        public static SSTable From(Memtable memtable, IFileManagerPort fileManager)
        {
            string filename = Guid.NewGuid().ToString();
            Dictionary<string, long> index = new Dictionary<string, long>();

            using (IFileWriterPort dataFile = fileManager.OpenOrCreateToWrite($"{filename}.dat"))
            using (IFileWriterPort indexFile = fileManager.OpenOrCreateToWrite($"{filename}.idx"))
            {
                foreach (var keyValue in memtable.GetData())
                {
                    BuildFile(dataFile, indexFile, keyValue, index);
                }
            }

            return new SSTable(fileManager.OpenToRead($"{filename}.dat"), index);
        }

        private static void BuildFile(IFileWriterPort dataFile, IFileWriterPort indexFile, KeyValuePair<string, string> keyValue, Dictionary<string, long> index)
        {
            long position = dataFile.Append(KeyValueEntry.ToBytes(keyValue.Key, keyValue.Value));
            index.Add(keyValue.Key, position);
            indexFile.Append(IndexEntry.ToBytes(keyValue.Key, position));
        }

        public void Dispose() => dataFile?.Dispose();
    }
}
