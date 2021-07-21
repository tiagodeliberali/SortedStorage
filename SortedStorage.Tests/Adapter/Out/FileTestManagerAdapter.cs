using SortedStorage.Application;
using SortedStorage.Application.Port.Out;
using System;

namespace SortedStorage.Tests.Adapter.Out
{
    class FileTestManagerAdapter : IFileManagerPort
    {
        public IFileWriterPort OpenOrCreateToWrite(string name, FileType type) => new FileTestWriterAdapter(name);

        public IFileWriterPort OpenOrCreateToWriteSingle(FileType type) => new FileTestWriterAdapter(Guid.NewGuid().ToString());

        public IFileReaderPort OpenToRead(string name, FileType type) => new FileTestReaderAdapter(name);
    }
}
