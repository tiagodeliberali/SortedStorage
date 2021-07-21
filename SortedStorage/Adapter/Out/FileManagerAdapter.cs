using SortedStorage.Application;
using SortedStorage.Application.Port.Out;
using System;
using System.IO;

namespace SortedStorage.Adapter.Out
{
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
                FileType.MemtableReadOnly => "memtable",
                FileType.MemtableWriteAheadLog => "memtablereadonly",
                FileType.SSTableData => "sstable",
                FileType.SSTableIndex => "index",
                _ => throw new NotImplementedException(),
            };

            return Path.ChangeExtension(name, extension);
        }
    }
}
