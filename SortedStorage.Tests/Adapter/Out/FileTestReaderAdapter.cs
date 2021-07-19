using SortedStorage.Application.Port.Out;
using System.Collections.Generic;
using System.Linq;

namespace SortedStorage.Tests.Adapter.Out
{
    class FileTestReaderAdapter : IFileReaderPort
    {
        private readonly string name;
        private readonly List<byte> data = new List<byte>();

        public FileTestReaderAdapter(string name)
        {
            this.name = name;
        }

        public byte[] Read(long position, int size)
        {
            return data
                .Skip((int)position)
                .Take(size)
                .ToArray();
        }

        public void Dispose()
        {
        }

        public string GetName() => name;

        public void LoadForTest(IEnumerable<byte> bytes) => data.AddRange(bytes);
    }
}
