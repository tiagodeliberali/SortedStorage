namespace SortedStorage.Application.Port.Out
{
    public interface IFileManagerPort
    {
        IFileReaderPort OpenToRead(string name);
        IFileWriterPort OpenOrCreateToWrite(string name);
    }
}
