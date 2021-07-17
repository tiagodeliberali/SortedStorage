﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SortedStorage
{
    class KeyValueRegister
    {
        public string Key { get; private set; }
        public string Value { get; private set; }

        public override bool Equals(object obj)
        {
            return obj is KeyValueRegister register &&
                   Key == register.Key &&
                   Value == register.Value;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Key, Value);
        }

        internal byte[] ToBytes()
        {
            List<byte> data = new List<byte>();
            
            // TOOD: validate if it is good enough to be a validation checksum
            data.AddRange(BitConverter.GetBytes(GetHashCode()));

            data.AddRange(BitConverter.GetBytes(Key.Length));
            data.AddRange(BitConverter.GetBytes(Value.Length));

            data.AddRange(Encoding.UTF8.GetBytes(Key));
            data.AddRange(Encoding.UTF8.GetBytes(Value));

            return data.ToArray();
        }

        internal static byte[] ToBytes(string key, string value)
        {
            var keyValue = new KeyValueRegister()
            {
                Key = key,
                Value = value
            };

            return keyValue.ToBytes();
        }
    }
}
