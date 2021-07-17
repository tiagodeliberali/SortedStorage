using System;
using System.Collections.Generic;
using System.Text;

namespace SortedStorage
{
    class Memtable<TKey, TValue>
    {
        internal bool IsFull()
        {
            return false;
        }

        internal void Add(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        internal TValue Get(TKey key)
        {
            throw new NotImplementedException();
        }
    }
}
