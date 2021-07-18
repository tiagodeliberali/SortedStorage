using SortedStorage.Application.Port.Out;
using System.IO;

namespace SortedStorage.Adapter.Out
{
    class FileReaderAdapter : IFileReaderPort
    {
        private readonly FileStream file;

        public FileReaderAdapter(string path) => file = new FileStream(path, FileMode.Open, FileAccess.Read);

        public byte[] Read(long position, int size)
        {
            var data = new byte[size];

            file.Seek(position, SeekOrigin.Begin);
            file.Read(data, 0, size);

            return data;
        }

        public void Dispose() => file?.Dispose();
    }
}
