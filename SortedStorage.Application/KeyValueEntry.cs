﻿namespace SortedStorage.Application;

using Force.Crc32;

using SortedStorage.Application.Port.Out;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class KeyValueEntry
{
    public string Key { get; }
    public string Value { get; }
    public int ByteSize => Encoding.UTF8.GetBytes(Key).Length + Encoding.UTF8.GetBytes(Value).Length + 12;

    public KeyValueEntry(string key, string value)
    {
        Key = key ?? string.Empty;
        Value = value ?? string.Empty;
    }

    public override bool Equals(object obj) => obj is KeyValueEntry register
                && Key == register.Key
                && Value == register.Value;

    public override int GetHashCode() => HashCode.Combine(Key, Value);

    public uint GetCrc32() => Crc32Algorithm.Compute(Encoding.UTF8.GetBytes($"{Key}{Value}"));

    public byte[] ToBytes()
    {
        List<byte> data = new List<byte>();

        data.AddRange(BitConverter.GetBytes(GetCrc32()));

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

    public static async Task<KeyValueEntry> FromFileReader(IFileReaderPort file)
    {
        byte[] header = await file.Read(12);

        uint checksum = BitConverter.ToUInt32(header, 0);
        int keySize = BitConverter.ToInt32(header, 4);
        int valueSize = BitConverter.ToInt32(header, 8);

        string keyData = Encoding.UTF8.GetString(await file.Read(keySize));
        string valueData = Encoding.UTF8.GetString(await file.Read(valueSize));

        KeyValueEntry keyValue = new KeyValueEntry(keyData, valueData);

        if (checksum != keyValue.GetCrc32())
        {
            throw new InvalidEntryParseException("Checksum not match");
        }

        return keyValue;
    }

    public static KeyValueEntry FromBytes(byte[] data)
    {
        uint checksum = BitConverter.ToUInt32(data, 0);
        int keySize = BitConverter.ToInt32(data, 4);
        int valueSize = BitConverter.ToInt32(data, 8);

        string keyData = Encoding.UTF8.GetString(data.Skip(12).Take(keySize).ToArray());
        string valueData = Encoding.UTF8.GetString(data.Skip(12 + keySize).Take(valueSize).ToArray());

        KeyValueEntry keyValue = new KeyValueEntry(keyData, valueData);

        if (checksum != keyValue.GetCrc32())
        {
            throw new InvalidEntryParseException("Checksum not match");
        }

        return keyValue;
    }
}
