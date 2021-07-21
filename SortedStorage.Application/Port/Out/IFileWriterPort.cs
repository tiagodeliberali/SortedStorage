using System;

namespace SortedStorage.Application.Port.Out
{
    public interface IFileWriterPort : IFileReaderPort
    {
        long Append(byte[] keyValue);
        IFileReaderPort ToReadOnly(FileType destinationType);
    }
}
