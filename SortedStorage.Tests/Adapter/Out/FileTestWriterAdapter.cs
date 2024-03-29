﻿namespace SortedStorage.Tests.Adapter.Out;

using SortedStorage.Application;
using SortedStorage.Application.Port.Out;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class FileTestWriterAdapter : IFileWriterPort
{
    private List<byte> data;

    public string Name { get; }
    public long Position { get; set; }

    public FileTestWriterAdapter(string name, List<byte> data = null)
    {
        this.data = data ?? new List<byte>();
        Name = name;
    }

    public Task<long> Append(byte[] keyValue)
    {
        long position = data.Count;

        data.AddRange(keyValue);

        return Task.FromResult(position);
    }

    internal IFileReaderPort GetReader() => new FileTestReaderAdapter(Name, data);

    public void Delete()
    {
        data.Clear();
        data = null;
    }

    public Task<byte[]> Read(int size)
    {
        var result = data
            .Skip((int)Position)
            .Take(size)
            .ToArray();

        Position += size;

        return Task.FromResult(result);
    }

    public bool HasContent() => Position < data.Count - 1;

    public IFileReaderPort ToReadOnly(FileType destinationType)
    {
        return new FileTestReaderAdapter(Name, data);
    }

    public void Dispose()
    {
    }

    public void LoadForTest(IEnumerable<byte> bytes) => data.AddRange(bytes);
}
