namespace SortedStorage.Application.Port.Out
{
    public interface IFileManagerPort
    {
        IFileReadPort OpenToRead(string name);
        IFileWritePort OpenOrCreateToWrite(string name);
    }
}
