using System;
using System.Threading.Tasks;

namespace SortedStorage.Application.Port.Out
{
    public interface IFileReaderPort : IDisposable
    {
        string Name { get; }
        long Position { get; set; }

        Task<byte[]> Read(int size);
        bool HasContent();
        void Delete();
    }
}
