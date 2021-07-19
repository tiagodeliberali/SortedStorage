using SortedStorage.Application.Port.Out;
using System.Collections.Generic;

namespace SortedStorage.Tests.Adapter.Out
{
    public class FileTestWriterAdapter : IFileWriterPort
    {
        private readonly string name;
        private List<byte> data = new List<byte>();

        public FileTestWriterAdapter(string name)
        {
            this.name = name;
        }

        public long Append(byte[] keyValue)
        {
            long position = data.Count;

            data.AddRange(keyValue);

            return position;
        }

        public void Delete()
        {
            data.Clear();
            data = null;
        }

        public void Dispose()
        {
        }

        public string GetName() => name;

        public void LoadForTest(IEnumerable<byte> bytes) => data.AddRange(bytes);
    }
}
