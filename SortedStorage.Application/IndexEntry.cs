﻿namespace SortedStorage.Application;

using SortedStorage.Application.Port.Out;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

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

        data.AddRange(BitConverter.GetBytes(Position));
        data.AddRange(BitConverter.GetBytes(keyData.Length));
        data.AddRange(keyData);

        return data.ToArray();
    }

    public static byte[] ToBytes(string key, long position)
    {
        var keyValue = new IndexEntry(key, position);
        return keyValue.ToBytes();
    }

    public static async Task<IndexEntry> FromIndexFileReader(IFileReaderPort file)
    {
        byte[] header = await file.Read(12);

        long indexPosition = BitConverter.ToInt64(header, 0);
        int keySize = BitConverter.ToInt32(header, 8);
        string keyData = Encoding.UTF8.GetString(await file.Read(keySize));

        IndexEntry keyValue = new IndexEntry(keyData, indexPosition);

        return keyValue;
    }
}
