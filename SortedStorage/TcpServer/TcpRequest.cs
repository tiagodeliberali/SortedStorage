using SortedStorage.Application;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SortedStorage.TcpServer
{
    public class TcpRequest
    {
        public RequestType Type { get; }

        public string Key { get; }

        public string Value { get; }

        public TcpRequest(RequestType type, string key = null, string value = null)
        {
            Type = type;
            Key = key;
            Value = value;
        }

        public static TcpRequest Get(string key) => new TcpRequest(RequestType.Get, key);

        public static TcpRequest GetInRange(string start, string end) => new TcpRequest(RequestType.GetInRange, start, end);

        public static TcpRequest Upsert(string key, string value) => new TcpRequest(RequestType.Upsert, key, value);

        public static TcpRequest Remove(string key) => new TcpRequest(RequestType.Remove, key);

        public static TcpRequest FromBytes(byte[] receivedData)
        {
            ushort typeId = BitConverter.ToUInt16(receivedData, 0);
            var entry = KeyValueEntry.FromBytes(receivedData.Skip(2).ToArray());

            return new TcpRequest(ParseType(typeId), entry.Key, entry.Value);
        }

        private static RequestType ParseType(ushort typeId)
        {
            return typeId switch
            {
                1 => RequestType.Get,
                2 => RequestType.Remove,
                4 => RequestType.Upsert,
                8 => RequestType.GetInRange,
                _ => throw new InvalidCastException(nameof(typeId)),
            };
        }

        private ushort ParseType()
        {
            return Type switch
            {
                RequestType.Upsert => 4,
                RequestType.Remove => 2,
                RequestType.Get => 1,
                RequestType.GetInRange => 8,
                _ => throw new InvalidCastException(nameof(Type)),
            };
        }

        public byte[] ToBytes()
        {
            List<byte> data = new List<byte>();
            data.AddRange(BitConverter.GetBytes(ParseType()));
            data.AddRange(KeyValueEntry.ToBytes(Key, Value));
            return data.ToArray();
        }
    }
}
