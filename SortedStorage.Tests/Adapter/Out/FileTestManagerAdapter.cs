using SortedStorage.Application;
using SortedStorage.Application.Port.Out;

namespace SortedStorage.Tests.Adapter.Out
{
    class FileTestManagerAdapter : IFileManagerPort
    {
        public IFileWriterPort OpenOrCreateToWrite(string name, FileType type) => new FileTestWriterAdapter(name);

        public IFileReaderPort OpenToRead(string name, FileType type) => new FileTestReaderAdapter(name);
    }
}
