namespace SortedStorage.Application;

using SortedStorage.Application.Port.Out;
using SortedStorage.Application.SymbolTable;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// TODO: Merge can have optional parameter to remove deleted (tombstone) register
public class SSTable : IDisposable
{
    private readonly IFileReaderPort dataFile;
    private readonly IFileReaderPort indexFile;
    private readonly RedBlackTree<string, long?> index;

    private SSTable(IFileReaderPort dataFile, IFileReaderPort indexFile, RedBlackTree<string, long?> index)
    {
        this.dataFile = dataFile;
        this.indexFile = indexFile;
        this.index = index;
    }

    public string GetFileName() => dataFile.Name;

    public async Task<string> Get(string key)
    {
        var position = index[key];
        if (!position.HasValue) return null;

        dataFile.Position = position.Value;
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

    public async IAsyncEnumerable<KeyValuePair<string, string>> GetInRange(string start, string end)
    {
        var initialKey = index.GetCeiling(start);

        if (initialKey == null)
            yield break;

        var position = index[initialKey];

        if (!position.HasValue)
            yield break;

        dataFile.Position = position.Value;
        while (dataFile.HasContent())
        {
            var keyValue = await KeyValueEntry.FromFileReader(dataFile);

            if (end.CompareTo(keyValue.Key) < 0)
                yield break;

            yield return KeyValuePair.Create(keyValue.Key, keyValue.Value);
        }
    }

    public async Task<SSTable> Merge(SSTable otherTable, IFileManagerPort fileManager)
    {
        PriorityEnumerator priorityEnumerator = new PriorityEnumerator(
            new IAsyncEnumerable<KeyValuePair<string, string>>[] { GetAll(), otherTable.GetAll() });

        string filename = Guid.NewGuid().ToString();
        var index = new RedBlackTree<string, long?>();

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
        var index = new RedBlackTree<string, long?>();

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
        var index = new RedBlackTree<string, long?>();

        indexFile.Position = 0;
        while (indexFile.HasContent())
        {
            IndexEntry entry = await IndexEntry.FromIndexFileReader(indexFile);
            index.Add(entry.Key, entry.Position);
        }

        return new SSTable(dataFile, indexFile, index);
    }

    private static async Task BuildFiles(IFileWriterPort dataFile, IFileWriterPort indexFile, KeyValuePair<string, string> keyValue, RedBlackTree<string, long?> index)
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
