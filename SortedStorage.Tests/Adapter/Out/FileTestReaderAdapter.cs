using SortedStorage.Application.Port.Out;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SortedStorage.Tests.Adapter.Out
{
    class FileTestReaderAdapter : IFileReaderPort
    {
        private List<byte> data;

        public string Name { get; }

        public long Position { get; set; }

        public FileTestReaderAdapter(string name, List<byte> data = null)
        {
            Name = name;
            this.data = data ?? new List<byte>();
        }

        public Task<byte[]> Read(int size)
        {
            var result = data
                .Skip((int)Position)
                .Take(size)
                .ToArray();

            Position += size;

            return Task.FromResult(result);
        }

        public bool HasContent() => Position < data.Count - 1;

        public void Delete()
        {
            data.Clear();
            data = null;
        }

        public void Dispose()
        {
        }

        public void LoadForTest(IEnumerable<byte> bytes) => data.AddRange(bytes);
    }
}
