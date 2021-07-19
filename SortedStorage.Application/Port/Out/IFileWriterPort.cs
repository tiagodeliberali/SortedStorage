using System;

namespace SortedStorage.Application.Port.Out
{
    public interface IFileWriterPort : IDisposable
    {
        long Append(byte[] keyValue);
        void Delete();
        string GetName();
    }
}
