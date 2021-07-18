using System;

namespace SortedStorage.Application.Port.Out
{
    public interface IFilePort : IDisposable
    {
        void Append(byte[] keyValue);
    }
}
