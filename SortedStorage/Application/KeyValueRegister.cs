using System;
using System.Collections.Generic;
using System.Text;

namespace SortedStorage.Application
{
    public class KeyValueRegister
    {
        public KeyValueRegister(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; private set; }
        public string Value { get; private set; }

        public override bool Equals(object obj)
        {
            return obj is KeyValueRegister register
                    && Key == register.Key
                    && Value == register.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value);
        }

        public byte[] ToBytes()
        {
            List<byte> data = new List<byte>();

            // TODO: validate if it is good enough to be a validation checksum
            data.AddRange(BitConverter.GetBytes(GetHashCode()));

            data.AddRange(BitConverter.GetBytes(Key.Length));
            data.AddRange(BitConverter.GetBytes(Value.Length));

            data.AddRange(Encoding.UTF8.GetBytes(Key));
            data.AddRange(Encoding.UTF8.GetBytes(Value));

            return data.ToArray();
        }

        public static byte[] ToBytes(string key, string value)
        {
            var keyValue = new KeyValueRegister(key, value);
            return keyValue.ToBytes();
        }
    }
}
