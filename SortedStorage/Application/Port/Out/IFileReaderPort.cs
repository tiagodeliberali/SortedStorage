using System;

namespace SortedStorage.Application.Port.Out
{
    public interface IFileReaderPort : IDisposable
    {
        byte[] Read(long position, int size);
    }
}
