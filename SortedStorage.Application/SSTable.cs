using SortedStorage.Application.Port.Out;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SortedStorage.Application
{
    public class SSTable : IDisposable
    {
        private readonly IFileReaderPort dataFile;
        private readonly IFileReaderPort indexFile;
        private readonly Dictionary<string, long> index;

        private SSTable(IFileReaderPort dataFile, IFileReaderPort indexFile, Dictionary<string, long> index)
        {
            this.dataFile = dataFile;
            this.indexFile = indexFile;
            this.index = index;
        }

        public string GetFileName() => dataFile.Name;

        public async Task<string> Get(string key)
        {
            if (!index.ContainsKey(key)) return null;

            var position = index[key];

            dataFile.Position = position;
            var keyValue = await KeyValueEntry.FromFileReader(dataFile);

            return keyValue.Value;
        }

        public async IAsyncEnumerable<KeyValuePair<string, string>> GetAll()
        {
            dataFile.Position = 0;
            while (dataFile.HasContent())
            {
                var keyValue = await KeyValueEntry.FromFileReader(dataFile);
                yield return KeyValuePair.Create(keyValue.Key, keyValue.Value);
            }
        }

        public async Task<SSTable> Merge(SSTable otherTable, IFileManagerPort fileManager)
        {
            PriorityEnumerator priorityEnumerator = new PriorityEnumerator(
                new IAsyncEnumerable<KeyValuePair<string, string>>[] { GetAll(), otherTable.GetAll() });

            string filename = Guid.NewGuid().ToString();
            Dictionary<string, long> index = new Dictionary<string, long>();

            using (IFileWriterPort dataFile = fileManager.OpenOrCreateToWrite(filename, FileType.SSTableData))
            using (IFileWriterPort indexFile = fileManager.OpenOrCreateToWrite(filename, FileType.SSTableIndex))
            {
                await foreach (var item in priorityEnumerator.GetAll())
                {
                    await BuildFiles(dataFile, indexFile, item, index);
                }
            }

            return new SSTable(
                fileManager.OpenToRead(filename, FileType.SSTableData),
                fileManager.OpenToRead(filename, FileType.SSTableIndex),
                index);
        }

        public static async Task<SSTable> From(ImutableMemtable memtable, IFileManagerPort fileManager)
        {
            string filename = Guid.NewGuid().ToString();
            Dictionary<string, long> index = new Dictionary<string, long>();

            using (IFileWriterPort dataFile = fileManager.OpenOrCreateToWrite(filename, FileType.SSTableData))
            using (IFileWriterPort indexFile = fileManager.OpenOrCreateToWrite(filename, FileType.SSTableIndex))
            {
                foreach (var keyValue in memtable.GetData())
                {
                    await BuildFiles(dataFile, indexFile, keyValue, index);
                }
            }

            return new SSTable(
                fileManager.OpenToRead(filename, FileType.SSTableData),
                fileManager.OpenToRead(filename, FileType.SSTableIndex),
                index);
        }

        public static async Task<SSTable> Load(IFileReaderPort indexFile, IFileReaderPort dataFile)
        {
            Dictionary<string, long> index = new Dictionary<string, long>();

            indexFile.Position = 0;
            while (indexFile.HasContent())
            {
                IndexEntry entry = await IndexEntry.FromIndexFileReader(indexFile);
                index.Add(entry.Key, entry.Position);
            }

            return new SSTable(dataFile, indexFile, index);
        }

        private static async Task BuildFiles(IFileWriterPort dataFile, IFileWriterPort indexFile, KeyValuePair<string, string> keyValue, Dictionary<string, long> index)
        {
            long position = await dataFile.Append(KeyValueEntry.ToBytes(keyValue.Key, keyValue.Value));
            index[keyValue.Key] = position;
            await indexFile.Append(IndexEntry.ToBytes(keyValue.Key, position));
        }

        public void Dispose() => dataFile?.Dispose();

        public void Delete()
        {
            index.Clear();
            dataFile.Delete();
            indexFile.Delete();
        }
    }
}
