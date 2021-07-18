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

        internal string Get(string key)
        {
            if (!index.ContainsKey(key)) return null;

            var position = index[key];

            var keyValue = KeyValueEntry.FromBytes(dataFile, position);

            return keyValue.Value;
        }

        internal static SSTable From(Memtable memtable, IFileManagerPort fileManager)
        {
            string filename = Guid.NewGuid().ToString();
            Dictionary<string, long> index = new Dictionary<string, long>();

            using (IFileWriterPort dataFile = fileManager.OpenOrCreateToWrite($"{filename}.dat"))
            using (IFileWriterPort indexFile = fileManager.OpenOrCreateToWrite($"{filename}.idx"))
            {

                foreach (var keyValue in memtable.GetData())
                {
                    long position = dataFile.Append(KeyValueEntry.ToBytes(keyValue.Key, keyValue.Value));
                    index.Add(keyValue.Key, position);
                    indexFile.Append(IndexEntry.ToBytes(keyValue.Key, position));
                }
            }

            return new SSTable(fileManager.OpenToRead($"{filename}.dat"), index);
        }

        public void Dispose() => dataFile?.Dispose();
    }
}
