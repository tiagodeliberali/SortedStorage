using SortedStorage.Application.Port.Out;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SortedStorage.Application
{
    // TODO: Load resources from disk: could add header to files
    // TODO: Add tests
    public class StorageService
    {
        private readonly IFileManagerPort fileManager;
        private readonly LinkedList<SSTable> sstables;
        private Memtable mainMemtable;
        private Memtable transferMemtable;

        public StorageService(IFileManagerPort fileManager)
        {
            this.fileManager = fileManager;

            mainMemtable = new Memtable(fileManager.OpenOrCreateToWrite($"{Guid.NewGuid()}.tmp"));
            sstables = new LinkedList<SSTable>();
        }

        public void Add(string key, string value)
        {
            Debug.WriteLine($"[StorageService] Adding key {key}");
            // TODO: rethink concurrency....
            lock (mainMemtable)
            {
                if (mainMemtable.IsFull())
                {
                    Debug.WriteLine($"[StorageService] mainMemtable is full");
                    StoreMainMemtable();
                }

                mainMemtable.Add(key, value);
            }
        }

        private void StoreMainMemtable()
        {
            // TODO: if main memtable gets full before finishing to create sstable from transfer memtable
            // we are going to have problems... (must define which kind of problem)
            transferMemtable = mainMemtable;
            mainMemtable = new Memtable(fileManager.OpenOrCreateToWrite($"{Guid.NewGuid()}.tmp"));

            // TODO: start a new Task to transform transfer memtable to a sstable
            Debug.WriteLine($"[StorageService] transfer table started for file {transferMemtable.GetFileName()}");
            sstables.AddFirst(SSTable.From(transferMemtable, fileManager));
            transferMemtable.Delete();
            transferMemtable = null;
            Debug.WriteLine($"[StorageService] transfer table completed");

            // TODO: Move to a concurrent task
            MergeLastSSTables();
        }

        private void MergeLastSSTables()
        {
            // TODO: Improve execution criteria and allow better usage of concurrency
            if (sstables.Count > 2)
            {
                Debug.WriteLine($"[StorageService] merge tables started");
                SSTable s1 = sstables.Last.Value;
                sstables.RemoveLast();
                Debug.WriteLine($"[StorageService] removing table {s1.GetFileName()}");

                SSTable s2 = sstables.Last.Value;
                sstables.RemoveLast();
                Debug.WriteLine($"[StorageService] removing table {s2.GetFileName()}");

                SSTable result = s1.Merge(s2, fileManager);
                sstables.AddLast(result);
                Debug.WriteLine($"[StorageService] merge tables completed with file {result.GetFileName()}");
            }
        }

        public string Get(string key) => mainMemtable.Get(key)
                ?? transferMemtable?.Get(key)
                ?? GetFromSSTables(key);

        private string GetFromSSTables(string key)
        {
            foreach (var table in sstables)
            {
                string result = table.Get(key);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
