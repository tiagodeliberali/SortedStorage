using System;
using System.Collections.Generic;
using System.Text;

namespace SortedStorage.Application
{
    public class IndexEntry
    {
        public string Key { get; }
        public long Position { get; }

        public IndexEntry(string key, long position)
        {
            Key = key;
            Position = position;
        }

        public byte[] ToBytes()
        {
            List<byte> data = new List<byte>();

            byte[] keyData = Encoding.UTF8.GetBytes(Key);

            data.AddRange(BitConverter.GetBytes(keyData.Length));
            data.AddRange(keyData);
            data.AddRange(BitConverter.GetBytes(Position));

            return data.ToArray();
        }

        public static byte[] ToBytes(string key, long position)
        {
            var keyValue = new IndexEntry(key, position);
            return keyValue.ToBytes();
        }
    }
}
