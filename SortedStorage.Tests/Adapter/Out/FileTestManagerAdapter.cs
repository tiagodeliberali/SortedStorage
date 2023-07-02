namespace SortedStorage.Tests.Adapter.Out;

using SortedStorage.Application;
using SortedStorage.Application.Port.Out;

using System;
using System.Collections.Generic;

class FileTestManagerAdapter : IFileManagerPort
{
    Dictionary<string, List<byte>> files = new Dictionary<string, List<byte>>();

    public IFileWriterPort OpenOrCreateToWrite(string name, FileType type)
    {
        string path = $"{name}.{type}";
        files.Add(path, new List<byte>());
        return new FileTestWriterAdapter(name, files[path]);
    }

    public IFileWriterPort OpenOrCreateToWriteSingleFile(FileType type)
    {
        string path = $"{Guid.NewGuid()}.{type}";
        files.Add(path, new List<byte>());
        return new FileTestWriterAdapter(path, files[path]);
    }

    public IFileReaderPort OpenToRead(string name, FileType type)
    {
        string path = $"{name}.{type}";
        return new FileTestReaderAdapter(path, files[path]);
    }

    public IEnumerable<IFileReaderPort> OpenToReadAll(FileType type) => throw new NotImplementedException();

    public IFileReaderPort OpenToReadSingleFile(FileType type) => throw new NotImplementedException();
}
