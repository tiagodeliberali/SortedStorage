using SortedStorage.Application;
using SortedStorage.Application.Port.Out;
using System;
using System.Collections.Generic;

namespace SortedStorage.Tests.Adapter.Out
{
    class FileTestManagerAdapter : IFileManagerPort
    {
        public IFileWriterPort OpenOrCreateToWrite(string name, FileType type) => new FileTestWriterAdapter(name);

        public IFileWriterPort OpenOrCreateToWriteSingleFile(FileType type) => new FileTestWriterAdapter(Guid.NewGuid().ToString());

        public IFileReaderPort OpenToRead(string name, FileType type) => new FileTestReaderAdapter(name);

        public IEnumerable<IFileReaderPort> OpenToReadAll(FileType type) => throw new NotImplementedException();

        public IFileReaderPort OpenToReadSingleFile(FileType type) => throw new NotImplementedException();
    }
}
