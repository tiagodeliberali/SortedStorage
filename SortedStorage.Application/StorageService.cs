using SortedStorage.Application.Port.Out;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SortedStorage.Application
{
    public class StorageService
    {
        private readonly IFileManagerPort fileManager;
        private readonly LinkedList<SSTable> sstables;
        private Memtable mainMemtable;
        private ImutableMemtable transferMemtable;

        private StorageService(IFileManagerPort fileManager)
        {
            this.fileManager = fileManager;
            sstables = new LinkedList<SSTable>();
        }

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
                ConvertTransferMemtableToSSTable();
            }
        }

        public async Task Add(string key, string value)
        {
            Debug.WriteLine($"[StorageService] Adding key {key}");
            await mainMemtable.Add(key, value);

            if (mainMemtable.IsFull()) await StoreMainMemtable();
        }

        public async Task Remove(string key)
        {
            Debug.WriteLine($"[StorageService] Removing key {key} by adding tombstone value");
            await mainMemtable .Remove(key);

            if (mainMemtable.IsFull()) await StoreMainMemtable();
        }

        private async Task StoreMainMemtable()
        {
            Debug.WriteLine($"[StorageService] main memtable is full");

            // TODO: if main memtable gets full before finishing to create sstable from transfer memtable
            // we are going to have problems... (must define which kind of problem)
            transferMemtable = mainMemtable.ToImutable();
            mainMemtable = await Memtable.LoadFromFile(fileManager.OpenOrCreateToWrite(Guid.NewGuid().ToString(), FileType.MemtableWriteAheadLog));

            // TODO: start a new Task to transform transfer memtable to a sstable
            ConvertTransferMemtableToSSTable();

            // TODO: Move to a concurrent task
            await MergeLastSSTables();
        }

        private void ConvertTransferMemtableToSSTable()
        {
            Debug.WriteLine($"[StorageService] transfer table started for file {transferMemtable.GetFileName()}");
            sstables.AddFirst(SSTable.From(transferMemtable, fileManager));
            transferMemtable.Delete();
            transferMemtable = null;
            Debug.WriteLine($"[StorageService] transfer table completed");
        }

        private async Task MergeLastSSTables()
        {
            // TODO: Improve execution criteria and allow better usage of concurrency
            if (sstables.Count > 2)
            {
                Debug.WriteLine($"[StorageService] merge tables started");
                SSTable older = sstables.Last.Value;
                sstables.RemoveLast();
                Debug.WriteLine($"[StorageService] removing table {older.GetFileName()}");

                SSTable newer = sstables.Last.Value;
                sstables.RemoveLast();
                Debug.WriteLine($"[StorageService] removing table {newer.GetFileName()}");

                SSTable result = await older.Merge(newer, fileManager);
                sstables.AddLast(result);
                Debug.WriteLine($"[StorageService] merge tables completed with file {result.GetFileName()}");
            }
        }

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
