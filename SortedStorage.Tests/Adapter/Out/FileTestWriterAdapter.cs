using SortedStorage.Application.Port.Out;
using System.Collections.Generic;

namespace SortedStorage.Tests.Adapter.Out
{
    public class FileTestWriterAdapter : IFileWriterPort
    {
        private List<byte> data = new List<byte>();

        public string Name { get; }

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

        public void Dispose()
        {
        }

        public void LoadForTest(IEnumerable<byte> bytes) => data.AddRange(bytes);
    }
}
