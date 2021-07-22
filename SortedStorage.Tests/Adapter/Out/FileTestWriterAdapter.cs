using SortedStorage.Application;
using SortedStorage.Application.Port.Out;
using System.Collections.Generic;
using System.Linq;

namespace SortedStorage.Tests.Adapter.Out
{
    public class FileTestWriterAdapter : IFileWriterPort
    {
        private List<byte> data = new List<byte>();

        public string Name { get; }
        public long Position { get; set; }

        public FileTestWriterAdapter(string name)
        {
            Name = name;
        }

        public long Append(byte[] keyValue)
        {
            long position = data.Count;

            data.AddRange(keyValue);

            return position;
        }

        internal IFileReaderPort GetReader() => new FileTestReaderAdapter(Name, data);

        public void Delete()
        {
            data.Clear();
            data = null;
        }

        public byte[] Read(int size)
        {
            var result = data
                .Skip((int)Position)
                .Take(size)
                .ToArray();

            Position += size;

            return result;
        }

        public bool HasContent() => Position < data.Count - 1;

        public IFileReaderPort ToReadOnly(FileType destinationType)
        {
            return new FileTestReaderAdapter(Name, data);
        }

        public void Dispose()
        {
        }

        public void LoadForTest(IEnumerable<byte> bytes) => data.AddRange(bytes);
    }
}
