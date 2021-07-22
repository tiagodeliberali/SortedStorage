using SortedStorage.Application.Port.Out;
using System;
using System.Collections.Generic;

namespace SortedStorage.Application
{
    public class SSTable : IDisposable
    {
        private readonly IFileReaderPort dataFile;
        private readonly Dictionary<string, long> index;

        private SSTable(IFileReaderPort dataFile, Dictionary<string, long> index)
        {
            this.dataFile = dataFile;
            this.index = index;
        }

        public string GetFileName() => dataFile.Name;

        public string Get(string key)
        {
            if (!index.ContainsKey(key)) return null;

            var position = index[key];

            dataFile.Position = position;
            var keyValue = KeyValueEntry.FromFileReader(dataFile);

            return keyValue.Value;
        }

        public IEnumerable<KeyValuePair<string, string>> GetAll()
        {
            dataFile.Position = 0;
            while (dataFile.HasContent())
            {
                var keyValue = KeyValueEntry.FromFileReader(dataFile);
                yield return KeyValuePair.Create(keyValue.Key, keyValue.Value);
            }
        }

        public SSTable Merge(SSTable otherTable, IFileManagerPort fileManager)
        {
            PriorityEnumerator priorityEnumerator = new PriorityEnumerator(
                new IEnumerable<KeyValuePair<string, string>>[] { otherTable.GetAll(), GetAll() });

            string filename = Guid.NewGuid().ToString();
            Dictionary<string, long> index = new Dictionary<string, long>();

            using (IFileWriterPort dataFile = fileManager.OpenOrCreateToWrite(filename, FileType.SSTableData))
            using (IFileWriterPort indexFile = fileManager.OpenOrCreateToWrite(filename, FileType.SSTableIndex))
            {
                foreach (var item in priorityEnumerator.GetAll())
                {
                    BuildFiles(dataFile, indexFile, item, index);
                }
            }

            return new SSTable(fileManager.OpenToRead(filename, FileType.SSTableData), index);
        }

        public static SSTable From(ImutableMemtable memtable, IFileManagerPort fileManager)
        {
            string filename = Guid.NewGuid().ToString();
            Dictionary<string, long> index = new Dictionary<string, long>();

            using (IFileWriterPort dataFile = fileManager.OpenOrCreateToWrite(filename, FileType.SSTableData))
            using (IFileWriterPort indexFile = fileManager.OpenOrCreateToWrite(filename, FileType.SSTableIndex))
            {
                foreach (var keyValue in memtable.GetData())
                {
                    BuildFiles(dataFile, indexFile, keyValue, index);
                }
            }

            return new SSTable(fileManager.OpenToRead(filename, FileType.SSTableData), index);
        }

        public static SSTable Load(IFileReaderPort indexFile, IFileReaderPort dataFile)
        {
            Dictionary<string, long> index = new Dictionary<string, long>();

            indexFile.Position = 0;
            while (indexFile.HasContent())
            {
                IndexEntry entry = IndexEntry.FromIndexFileReader(indexFile);
                index.Add(entry.Key, entry.Position);
            }

            return new SSTable(dataFile, index);
        }

        private static void BuildFiles(IFileWriterPort dataFile, IFileWriterPort indexFile, KeyValuePair<string, string> keyValue, Dictionary<string, long> index)
        {
            long position = dataFile.Append(KeyValueEntry.ToBytes(keyValue.Key, keyValue.Value));
            index[keyValue.Key] = position;
            indexFile.Append(IndexEntry.ToBytes(keyValue.Key, position));
        }

        public void Dispose() => dataFile?.Dispose();
    }
}
