using System.Collections.Generic;

namespace SortedStorage
{
    public class StorageManager
    {
        private Memtable mainMemtable;
        private Memtable transferMemtable;
        private LinkedList<SSTable> sstables;

        public StorageManager()
        {
            mainMemtable = new Memtable();
            transferMemtable = new Memtable();
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
            mainMemtable = new Memtable();

            // start a new thread to transform transfer memtable to a sstable
        }

        public string Get(string key)
        {
            string result = mainMemtable.Get(key);

            if (result != null)
            {
                return result;
            }

            if (transferMemtable != null)
            {
                result = transferMemtable.Get(key);

                if (result != null)
                {
                    return result;
                }
            }

            foreach (var table in sstables)
            {
                result = table.Get(key);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}
