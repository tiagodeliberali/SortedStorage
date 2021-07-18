using SortedStorage.Application.Port.Out;
using System;
using System.Collections.Generic;

namespace SortedStorage.Application
{
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
            // TODO: rethink concurrency....
            lock (mainMemtable)
            {
                if (mainMemtable.IsFull())
                {
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
            sstables.AddLast(SSTable.From(transferMemtable, fileManager));
            transferMemtable.Dispose();
            transferMemtable = null;
        }

        public string Get(string key)
        {
            return mainMemtable.Get(key)
                ?? transferMemtable?.Get(key)
                ?? GetFromSSTables(key);
        }

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
