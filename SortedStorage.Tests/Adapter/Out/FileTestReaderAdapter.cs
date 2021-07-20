using SortedStorage.Application.Port.Out;
using System.Collections.Generic;
using System.Linq;

namespace SortedStorage.Tests.Adapter.Out
{
    class FileTestReaderAdapter : IFileReaderPort
    {
        private readonly List<byte> data;

        public string Name { get; }

        public long Position { get; set; }

        public FileTestReaderAdapter(string name, List<byte> data = null)
        {
            Name = name;
            this.data = data ?? new List<byte>();
        }

        public byte[] Read(long position, int size)
        {
            var result = data
                .Skip((int)position)
                .Take(size)
                .ToArray();

            Position += size;

            return result;
        }

        public bool HasContent() => Position < data.Count - 1;

        public void Dispose()
        {
        }

        public void LoadForTest(IEnumerable<byte> bytes) => data.AddRange(bytes);
    }
}
