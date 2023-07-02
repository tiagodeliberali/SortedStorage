namespace SortedStorage.Application;

using SortedStorage.Application.Port.Out;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

public class StorageService
{
    public static ManualResetEvent switchingMemtables = new ManualResetEvent(true);

    private readonly IFileManagerPort fileManager;
    private readonly LinkedList<SSTable> sstables;
    private Memtable mainMemtable;
    private ImutableMemtable transferMemtable;

    private bool isTransferingMemtable = false;

    private readonly object lockRef = new object();

    private StorageService(IFileManagerPort fileManager)
    {
        this.fileManager = fileManager;
        sstables = new LinkedList<SSTable>();
    }

    /// <summary>
    ///     Build a new instance of storage service. It verifies if there are memtable, transfer memtable and sstable files in disk to guarantee
    ///     that data will be reloaded correctly. If a transfer memtable file is found, the convertion to a memtable is finished.
    ///     You should keep only one instance running. It will (hopefully) deal with concurrency.
    /// </summary>
    /// <param name="fileManager">Reference to the implementation of file manager port</param>
    /// <returns>A loaded storage service instance</returns>
    public static async Task<StorageService> LoadFromFiles(IFileManagerPort fileManager)
    {
        Console.WriteLine($"[{nameof(StorageService)}] Starting database...");
        var stopwatch = Stopwatch.StartNew();
        var service = new StorageService(fileManager)
        {
            mainMemtable = await Memtable.LoadFromFile(fileManager.OpenOrCreateToWriteSingleFile(FileType.MemtableWriteAheadLog))
        };

        await service.LoadSSTables();
        await service.LoadPendingTransferTable();
        stopwatch.Stop();
        Console.WriteLine($"[{nameof(StorageService)}] Database started in {stopwatch.ElapsedMilliseconds} ms");
        return service;
    }

    private async Task LoadSSTables()
    {
        foreach (var indexFile in fileManager.OpenToReadAll(FileType.SSTableIndex))
        {
            var dataFile = fileManager.OpenToRead(Path.GetFileNameWithoutExtension(indexFile.Name), FileType.SSTableData);
            sstables.AddLast(await SSTable.Load(indexFile, dataFile));
        }
    }

    private async Task LoadPendingTransferTable()
    {
        var transferTableFile = fileManager.OpenToReadSingleFile(FileType.MemtableReadOnly);

        if (transferTableFile != null)
        {
            Console.WriteLine($"[{nameof(StorageService)}] Found memtable readonly file. Finishing pending SSTable creation.");
            transferMemtable = await ImutableMemtable.BuildFromFile(transferTableFile);
            await ConvertTransferMemtableToSSTable();
        }
    }

    public void Add(string key, string value)
    {
        var stopwatch = Stopwatch.StartNew();
        switchingMemtables.WaitOne();
        mainMemtable.Add(key, value);
        stopwatch.Stop();
        SortedStorageApplicationEventSource.Log.ReportUpdateDurationInMs(stopwatch.Elapsed.Ticks);
    }

    public void Remove(string key)
    {
        var stopwatch = Stopwatch.StartNew();
        switchingMemtables.WaitOne();
        mainMemtable.Remove(key);
        stopwatch.Stop();
        SortedStorageApplicationEventSource.Log.ReportUpdateDurationInMs(stopwatch.Elapsed.Ticks);
    }

    /// <summary>
    ///     Transforms the main memtable into a sstable. This process locks to prevent Add and Remove to avoid try to add data after
    ///     main memtable is turned into readonnly. If locks fails, Add and Remove to the readonly memtable will thrown an exception.
    ///     Those methods are unlocked as soon as the exchange between main and transfer memtable finishes. The process of converting
    ///     the transfer memtable into a sstable occurs later.
    /// </summary>
    public async Task TransferMemtableToSSTable()
    {
        if (mainMemtable.Size > StorageConfiguration.MaxMemtableSize && !isTransferingMemtable)
            await StoreMainMemtable();
    }

    private async Task StoreMainMemtable()
    {
        lock (lockRef)
        {
            if (isTransferingMemtable) return;
            isTransferingMemtable = true;
            switchingMemtables.Reset();
        }

        try
        {
            transferMemtable = mainMemtable.ToImutable();
            mainMemtable = await Memtable.LoadFromFile(fileManager.OpenOrCreateToWrite(Guid.NewGuid().ToString(), FileType.MemtableWriteAheadLog));

            switchingMemtables.Set();

            await ConvertTransferMemtableToSSTable();
        }
        finally
        {
            isTransferingMemtable = false;
        }
    }

    private async Task ConvertTransferMemtableToSSTable()
    {
        Console.WriteLine($"[{nameof(StorageService)}] transfer table started for file {transferMemtable.GetFileName()}");
        var stopwatch = Stopwatch.StartNew();
        sstables.AddFirst(await SSTable.From(transferMemtable, fileManager));
        transferMemtable.Delete();
        transferMemtable = null;
        stopwatch.Stop();
        Console.WriteLine($"[{nameof(StorageService)}] transfer table completed in {stopwatch.ElapsedMilliseconds} ms");
    }

    /// <summary>
    ///     Takes the last two sstables and merge than, to create a new sstable. The new sstable is added to list before the removal
    ///     of the old ones, to guarantee that information will be always available
    /// </summary>
    public async Task MergeLastSSTables()
    {
        if (sstables.Count > 2)
        {
            Console.WriteLine($"[{nameof(StorageService)}] merge tables started");
            var stopwatch = Stopwatch.StartNew();
            SSTable older = sstables.Last.Value;
            SSTable newer = sstables.Last.Previous.Value;

            SSTable result = await older.Merge(newer, fileManager);
            sstables.AddLast(result);
            sstables.Remove(sstables.Last.Previous);
            sstables.Remove(sstables.Last.Previous);
            older.Delete();
            newer.Delete();
            stopwatch.Stop();
            Console.WriteLine($"[{nameof(StorageService)}] merge tables completed with file {result.GetFileName()} in {stopwatch.ElapsedMilliseconds} ms");
        }
    }

    /// <summary>
    ///     Search for a key in all available sources. First, try yo get it from main mem table. If not found,
    ///     checks the transfer memtable if it existst. It will be available while a memtable is being translated to a sstable.
    ///     In the end, try to get the value from the list of sstables.
    ///     Search the value at memtables are executed sync, because they are in memory searchs. The search at sstables are async,
    ///     because search will follow a pointer to a file.
    /// </summary>
    /// <param name="key">The key being searched</param>
    /// <returns>
    ///     The value associated to the key. If not found, it returns null. If the key is associated with 
    ///     a tombstone, the result will be null.
    /// </returns>
    public async Task<string> Get(string key)
    {
        var stopwatch = Stopwatch.StartNew();
        string result = mainMemtable.Get(key)
            ?? transferMemtable?.Get(key)
            ?? await GetFromSSTables(key);
        stopwatch.Stop();
        SortedStorageApplicationEventSource.Log.ReportGetDurationInMs(stopwatch.Elapsed.Ticks);

        return result == StorageConfiguration.Tombstone
            ? null
            : result;
    }

    public async IAsyncEnumerable<KeyValuePair<string, string>> GetInRange(string start, string end)
    {
        var enumerables = new List<IAsyncEnumerable<KeyValuePair<string, string>>>
        {
            mainMemtable.GetInRange(start, end)
        };

        if (transferMemtable != null)
            enumerables.Add(transferMemtable.GetInRange(start, end));

        foreach (var table in sstables)
        {
            enumerables.Add(table.GetInRange(start, end));
        }

        PriorityEnumerator priorityEnumerator = new PriorityEnumerator(enumerables);

        await foreach (var item in priorityEnumerator.GetAll())
        {
            yield return item;
        }
    }

    private async Task<string> GetFromSSTables(string key)
    {
        foreach (var table in sstables)
        {
            string result = await table.Get(key);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}
