using System.Collections.Generic;

namespace SortedStorage
{
    public class StorageManager<TKey, TValue>
        where TValue: class
    {
        private Memtable<TKey, TValue> mainMemtable;
        private Memtable<TKey, TValue> transferMemtable;
        private LinkedList<SSTable<TKey, TValue>> sstables;

        public StorageManager()
        {
            mainMemtable = new Memtable<TKey, TValue>();
            transferMemtable = new Memtable<TKey, TValue>();
            sstables = new LinkedList<SSTable<TKey, TValue>>();
        }

        public void Add(TKey key, TValue value)
        {
            // TODO: rethink concurrency....
            lock(mainMemtable)
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
            mainMemtable = new Memtable<TKey, TValue>();

            // start a new thread to transform transfer memtable to a sstable
        }

        public TValue Get(TKey key)
        {
            TValue result = mainMemtable.Get(key);

            if (result != null)
            {
                return result;
            }

            if (transferMemtable != null) {
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
