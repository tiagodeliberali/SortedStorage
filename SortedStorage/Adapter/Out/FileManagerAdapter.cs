using SortedStorage.Application.Port.Out;
using System.IO;

namespace SortedStorage.Adapter.Out
{
    class FileManagerAdapter : IFileManagerPort
    {
        private readonly string basePath;

        public FileManagerAdapter(string basePath)
        {
            this.basePath = basePath;
        }

        public IFileWritePort OpenOrCreateToWrite(string name)
        {
            return new FileWriterAdapter(Path.Combine(basePath, name));
        }

        public IFileReadPort OpenToRead(string name)
        {
            return new FileReaderAdapter(Path.Combine(basePath, name));
        }
    }
}
