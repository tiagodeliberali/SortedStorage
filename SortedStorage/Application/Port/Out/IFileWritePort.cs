using System;

namespace SortedStorage.Application.Port.Out
{
    public interface IFileWritePort : IDisposable
    {
        long Append(byte[] keyValue);
    }
}
