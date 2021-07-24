using SortedStorage.Application.Port.Out;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SortedStorage.Application
{
    public class StorageService
    {
        public static ManualResetEvent switchingMemtables = new ManualResetEvent(true);

        private readonly IFileManagerPort fileManager;
        private readonly LinkedList<SSTable> sstables;
        private Memtable mainMemtable;
        private ImutableMemtable transferMemtable;

        private bool isTransferingMemtable = false;

        private readonly Object lockRef = new object();

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
            Debug.WriteLine($"[StorageService] Starting database...");
            var service = new StorageService(fileManager);

            service.mainMemtable = await Memtable.LoadFromFile(fileManager.OpenOrCreateToWriteSingleFile(FileType.MemtableWriteAheadLog));

            await service.LoadSSTables();
            await service.LoadPendingTransferTable();
            Debug.WriteLine($"[StorageService] Database started!");

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
                Debug.WriteLine($"[StorageService] Found memtable readonly file. Finishing pending SSTable creation.");
                transferMemtable = await ImutableMemtable.BuildFromFile(transferTableFile);
                await ConvertTransferMemtableToSSTable();
            }
        }

        public void Add(string key, string value)
        {
            switchingMemtables.WaitOne();

            Debug.WriteLine($"[StorageService] Adding key {key}");
            mainMemtable.Add(key, value);
        }

        public void Remove(string key)
        {
            switchingMemtables.WaitOne();

            Debug.WriteLine($"[StorageService] Removing key {key} by adding tombstone value");
            mainMemtable.Remove(key);
        }

        /// <summary>
        ///     Transforms the main memtable into a sstable. This process locks to prevent Add and Remove to avoid try to add data after
        ///     main memtable is turned into readonnly. If locks fails, Add and Remove to the readonly memtable will thrown an exception.
        ///     Those methods are unlocked as soon as the exchange between main and transfer memtable finishes. The process of converting
        ///     the transfer memtable into a sstable occurs later.
        /// </summary>
        public async Task TransferMemtableToSSTable()
        {
            if (mainMemtable.IsFull() && !isTransferingMemtable)
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
                Debug.WriteLine($"[StorageService] main memtable is full");

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
            Debug.WriteLine($"[StorageService] transfer table started for file {transferMemtable.GetFileName()}");
            sstables.AddFirst(await SSTable.From(transferMemtable, fileManager));
            transferMemtable.Delete();
            transferMemtable = null;
            Debug.WriteLine($"[StorageService] transfer table completed");
        }

        /// <summary>
        ///     Takes the last two sstables and merge than, to create a new sstable. The new sstable is added to list before the removal
        ///     of the old ones, to guarantee that information will be always available
        /// </summary>
        public async Task MergeLastSSTables()
        {
            if (sstables.Count > 2)
            {
                Debug.WriteLine($"[StorageService] merge tables started");
                SSTable older = sstables.Last.Value;
                SSTable newer = sstables.Last.Previous.Value;

                SSTable result = await older.Merge(newer, fileManager);
                sstables.AddLast(result);
                sstables.Remove(sstables.Last.Previous);
                sstables.Remove(sstables.Last.Previous);
                older.Delete();
                newer.Delete();
                Debug.WriteLine($"[StorageService] merge tables completed with file {result.GetFileName()}");
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
            string result = mainMemtable.Get(key)
                ?? transferMemtable?.Get(key)
                ?? await GetFromSSTables(key);

            return result == StorageConfiguration.TOMBSTONE
                ? null
                : result;
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
}
