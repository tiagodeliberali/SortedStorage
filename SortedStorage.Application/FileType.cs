namespace SortedStorage.Application;

public enum FileType
{
    MemtableWriteAheadLog,
    MemtableReadOnly,
    SSTableData,
    SSTableIndex
}