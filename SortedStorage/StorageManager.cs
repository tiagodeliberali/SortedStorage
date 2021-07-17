using System.Collections.Generic;

namespace SortedStorage
{
    public class StorageManager
    {
        private readonly IFilePort filePort;
        private Memtable mainMemtable;
        private Memtable transferMemtable;
        private LinkedList<SSTable> sstables;

        public StorageManager(IFilePort filePort)
        {
            this.filePort = filePort;

            mainMemtable = new Memtable(filePort);
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
            mainMemtable = new Memtable(filePort);

            // start a new thread to transform transfer memtable to a sstable
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
