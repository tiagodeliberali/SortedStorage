using System;

namespace SortedStorage
{
    public interface IFilePort : IDisposable
    {
        void Append(byte[] keyValue);
    }
}
