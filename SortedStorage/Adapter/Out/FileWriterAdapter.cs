﻿namespace SortedStorage.Adapter.Out;

using SortedStorage.Application;
using SortedStorage.Application.Port.Out;

using System.IO;
using System.Threading.Tasks;

public class FileWriterAdapter : IFileWriterPort
{
    private readonly FileStream file;

    public string Name { get; }
    public long Position
    {
        get => file.Position;
        set => file.Seek(value, SeekOrigin.Begin);
    }

    public FileWriterAdapter(string path)
    {
        Name = path;
        file = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
    }

    public async Task<long> Append(byte[] keyValue)
    {
        long position = file.Seek(0, SeekOrigin.End);

        await file.WriteAsync(keyValue, 0, keyValue.Length);
        await file.FlushAsync();

        return position;
    }

    public void Delete()
    {
        file.Dispose();
        File.Delete(Name);
    }

    public async Task<byte[]> Read(int size)
    {
        var data = new byte[size];
        await file.ReadAsync(data, 0, size);

        return data;
    }

    public bool HasContent() => file.Position < file.Length - 1;

    public IFileReaderPort ToReadOnly(FileType destinationType)
    {
        file.Dispose();
        string readonlyFilePath = FileManagerAdapter.BuildFileName(Name, destinationType);
        File.Move(Name, readonlyFilePath);
        return new FileReaderAdapter(readonlyFilePath);
    }

    public void Dispose() => file?.Dispose();
}
