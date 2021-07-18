using SortedStorage.Application.Port.Out;
using System;

namespace SortedStorage.Application
{
    class SSTable : IDisposable
    {
        private readonly IFileReadPort dataFile, indexFile;

        private SSTable(IFileReadPort dataFile, IFileReadPort indexFile)
        {
            this.dataFile = dataFile;
            this.indexFile = indexFile;
        }

        public static SSTable Build(IFileReadPort dataFile, IFileReadPort indexFile)
        {
            return new SSTable(dataFile, indexFile);
        }

        internal string Get(string key) => throw new NotImplementedException();

        internal static SSTable From(Memtable memtable, IFileManagerPort fileManager)
        {
            string filename = Guid.NewGuid().ToString();

            using (IFileWritePort dataFile = fileManager.OpenOrCreateToWrite($"{filename}.dat"))
            using (IFileWritePort indexFile = fileManager.OpenOrCreateToWrite($"{filename}.idx"))
            {

                foreach (var keyValue in memtable.GetData())
                {
                    long position = dataFile.Append(KeyValueEntry.ToBytes(keyValue.Key, keyValue.Value));
                    indexFile.Append(IndexEntry.ToBytes(keyValue.Key, position));
                }
            }

            return SSTable.Build(fileManager.OpenToRead($"{filename}.dat"), fileManager.OpenToRead($"{filename}.idx"));
        }

        public void Dispose()
        {
            dataFile?.Dispose();
            indexFile?.Dispose();
        }
    }
}
