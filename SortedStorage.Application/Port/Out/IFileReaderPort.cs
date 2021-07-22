using System;

namespace SortedStorage.Application.Port.Out
{
    public interface IFileReaderPort : IDisposable
    {
        string Name { get; }
        long Position { get; set; }

        byte[] Read(int size);
        bool HasContent();
        void Delete();
    }
}
