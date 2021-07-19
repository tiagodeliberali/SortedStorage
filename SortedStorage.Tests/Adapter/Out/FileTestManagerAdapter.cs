using SortedStorage.Application.Port.Out;

namespace SortedStorage.Tests.Adapter.Out
{
    class FileTestManagerAdapter : IFileManagerPort
    {
        public IFileWriterPort OpenOrCreateToWrite(string name) => new FileTestWriterAdapter(name);

        public IFileReaderPort OpenToRead(string name) => new FileTestReaderAdapter(name);
    }
}
