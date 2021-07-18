using SortedStorage.Application.Port.Out;
using System.IO;

namespace SortedStorage.Adapter.Out
{
    public class FileWriterAdapter : IFileWriterPort
    {
        private readonly FileStream file;

        public FileWriterAdapter(string path) => file = new FileStream(path, FileMode.Append, FileAccess.Write);

        public long Append(byte[] keyValue)
        {
            long position = file.Seek(0, SeekOrigin.End);

            file.Write(keyValue, 0, keyValue.Length);
            file.Flush();

            return position;
        }

        public void Dispose() => file?.Dispose();
    }
}
