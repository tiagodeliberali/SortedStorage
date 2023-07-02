namespace SortedStorage.Adapter.Out;

using SortedStorage.Application;
using SortedStorage.Application.Port.Out;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class FileManagerAdapter : IFileManagerPort
{
    private readonly string basePath;

    public FileManagerAdapter(string basePath) => this.basePath = basePath;

    public IFileWriterPort OpenOrCreateToWrite(string name, FileType type) => new FileWriterAdapter(BuildPath(name, type));

    public IFileReaderPort OpenToRead(string name, FileType type) => new FileReaderAdapter(BuildPath(name, type));

    private string BuildPath(string name, FileType type) => Path.Combine(basePath, BuildFileName(name, type));

    public static string BuildFileName(string name, FileType type)
    {
        string extension = type switch
        {
            FileType.MemtableReadOnly => "memtablereadonly",
            FileType.MemtableWriteAheadLog => "memtable",
            FileType.SSTableData => "sstable",
            FileType.SSTableIndex => "index",
            _ => throw new NotImplementedException(),
        };

        return Path.ChangeExtension(name, extension);
    }

    public IFileWriterPort OpenOrCreateToWriteSingleFile(FileType type)
    {
        string searchPattern = BuildFileName("*.", type);
        var files = Directory.GetFiles(basePath, searchPattern);

        if (files.Length > 1)
            throw new MoreThanOneOfTypeException($"Found more than one file of type {searchPattern} at {basePath}");

        if (files.Length == 1)
            return OpenOrCreateToWrite(files[0], type);

        return OpenOrCreateToWrite(Guid.NewGuid().ToString(), type);
    }

    public IFileReaderPort OpenToReadSingleFile(FileType type)
    {
        string searchPattern = BuildFileName("*.", type);
        var files = Directory.GetFiles(basePath, searchPattern);

        if (files.Length > 1)
            throw new MoreThanOneOfTypeException($"Found more than one file of type {searchPattern} at {basePath}");

        if (files.Length == 1)
            return OpenToRead(files[0], type);

        return null;
    }

    public IEnumerable<IFileReaderPort> OpenToReadAll(FileType type)
    {
        string searchPattern = BuildFileName("*.", type);
        return Directory
            .GetFiles(basePath, searchPattern)
            .OrderBy(x => new FileInfo(x).CreationTime)
            .Select(x => OpenToRead(x, type));
    }
}
