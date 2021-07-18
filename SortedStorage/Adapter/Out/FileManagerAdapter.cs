using SortedStorage.Application.Port.Out;
using System.IO;

namespace SortedStorage.Adapter.Out
{
    class FileManagerAdapter : IFileManagerPort
    {
        private readonly string basePath;

        public FileManagerAdapter(string basePath) => this.basePath = basePath;

        public IFileWriterPort OpenOrCreateToWrite(string name) => new FileWriterAdapter(Path.Combine(basePath, name));

        public IFileReaderPort OpenToRead(string name) => new FileReaderAdapter(Path.Combine(basePath, name));
    }
}
