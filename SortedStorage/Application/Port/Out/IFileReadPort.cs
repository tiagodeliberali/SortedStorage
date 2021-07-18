using System;

namespace SortedStorage.Application.Port.Out
{
    public interface IFileReadPort : IDisposable
    {
        byte[] Read(int position, int size);
    }
}
