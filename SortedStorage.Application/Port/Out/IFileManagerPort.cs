using System.Collections.Generic;

namespace SortedStorage.Application.Port.Out
{
    public interface IFileManagerPort
    {
        IFileReaderPort OpenToRead(string name, FileType type);
        IFileWriterPort OpenOrCreateToWrite(string name, FileType type);
        IFileWriterPort OpenOrCreateToWriteSingleFile(FileType type);
        IFileReaderPort OpenToReadSingleFile(FileType type);
        IEnumerable<IFileReaderPort> OpenToReadAll(FileType type);
    }
}
