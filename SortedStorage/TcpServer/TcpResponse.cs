using SortedStorage.Application;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SortedStorage.TcpServer
{
    public class TcpResponse
    {
        public bool Success { get; }
        public IEnumerable<KeyValueEntry> Entries { get; }

        private TcpResponse(bool success, IEnumerable<KeyValueEntry> entries = null)
        {
            Success = success;
            Entries = entries ?? new List<KeyValueEntry>();
        }

        public static TcpResponse SuccessGet(IEnumerable<KeyValueEntry> entries) => new TcpResponse(true, entries);

        public static TcpResponse SuccessResult() => new TcpResponse(true);

        public static TcpResponse FailureResult() => new TcpResponse(false);

        public IEnumerable<byte> ToBytes()
        {
            var data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(Success));
            data.AddRange(BitConverter.GetBytes(Entries.Count()));
            foreach (var item in Entries)
            {
                data.AddRange(item.ToBytes());
            }
            return data;
        }

        public static TcpResponse FromBytes(byte[] data)
        {
            bool success = BitConverter.ToBoolean(data);
            int totalEntries = BitConverter.ToInt32(data.Skip(1).Take(4).ToArray());

            var entries = new List<KeyValueEntry>();
            int toSkip = 5;
            for (int i = 0; i < totalEntries; i++)
            {
                var entry = KeyValueEntry.FromBytes(data.Skip(toSkip).ToArray());
                entries.Add(entry);
                toSkip += entry.ByteSize;
            }

            return new TcpResponse(success, entries);
        }
    }
}
