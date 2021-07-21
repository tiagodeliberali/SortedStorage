namespace SortedStorage.Application.Port.Out
{
    public interface IFileManagerPort
    {
        IFileReaderPort OpenToRead(string name, FileType type);
        IFileWriterPort OpenOrCreateToWrite(string name, FileType type);
        IFileWriterPort OpenOrCreateToWriteSingle(FileType type);
    }
}
