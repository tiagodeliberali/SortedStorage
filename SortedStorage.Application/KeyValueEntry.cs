using SortedStorage.Application.Port.Out;
using System;
using System.Collections.Generic;
using System.Text;

namespace SortedStorage.Application
{
    public class KeyValueEntry
    {
        public KeyValueEntry(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; }
        public string Value { get; }

        public override bool Equals(object obj)
        {
            return obj is KeyValueEntry register
                    && Key == register.Key
                    && Value == register.Value;
        }

        public override int GetHashCode() => HashCode.Combine(Key, Value);

        public byte[] ToBytes()
        {
            List<byte> data = new List<byte>();

            data.AddRange(BitConverter.GetBytes(GetHashCode()));

            byte[] keydata = Encoding.UTF8.GetBytes(Key);
            byte[] valueData = Encoding.UTF8.GetBytes(Value);

            data.AddRange(BitConverter.GetBytes(keydata.Length));
            data.AddRange(BitConverter.GetBytes(valueData.Length));

            data.AddRange(keydata);
            data.AddRange(valueData);

            return data.ToArray();
        }

        public static byte[] ToBytes(string key, string value)
        {
            var keyValue = new KeyValueEntry(key, value);
            return keyValue.ToBytes();
        }

        public static KeyValueEntry FromFileReader(IFileReaderPort file, long position)
        {
            byte[] header = file.Read(position, 12);

            int checksum = BitConverter.ToInt32(header, 0);
            int keySize = BitConverter.ToInt32(header, 4);
            int valueSize = BitConverter.ToInt32(header, 8);

            string keyData = Encoding.UTF8.GetString(file.Read(position + 12, keySize));
            string valueData = Encoding.UTF8.GetString(file.Read(position + 12 + keySize, valueSize));

            KeyValueEntry keyValue = new KeyValueEntry(keyData, valueData);

            if (checksum != keyValue.GetHashCode())
            {
                throw new InvalidEntryParseException("Checksum not match");
            }

            return keyValue;
        }
    }
}
