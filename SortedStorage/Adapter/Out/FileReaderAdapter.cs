using SortedStorage.Application.Port.Out;
using System.IO;

namespace SortedStorage.Adapter.Out
{
    class FileReaderAdapter : IFileReadPort
    {
        private readonly FileStream file;

        public FileReaderAdapter(string path)
        {
            file = new FileStream(path, FileMode.Open, FileAccess.Read);
        }

        public byte[] Read(int position, int size)
        {
            var data = new byte[size];

            file.Read(data, position, size);

            return data;
        }

        public void Dispose() => file?.Dispose();
    }
}
