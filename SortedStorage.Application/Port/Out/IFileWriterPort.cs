using System;

namespace SortedStorage.Application.Port.Out
{
    public interface IFileWriterPort : IDisposable
    {
        string Name { get; }

        long Append(byte[] keyValue);
        void Delete();
    }
}
