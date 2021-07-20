using SortedStorage.Application.Port.Out;
using System.IO;

namespace SortedStorage.Adapter.Out
{
    public class FileWriterAdapter : IFileWriterPort
    {
        private readonly FileStream file;

        public string Name { get; }
        public long Position
        {
            get => file.Position;
            set => file.Seek(value, SeekOrigin.Begin);
        }

        public FileWriterAdapter(string path)
        {
            Name = path;
            file = new FileStream(path, FileMode.Append, FileAccess.Write);
        }

        public long Append(byte[] keyValue)
        {
            long position = file.Seek(0, SeekOrigin.End);

            file.Write(keyValue, 0, keyValue.Length);
            file.Flush();

            return position;
        }

        public void Delete()
        {
            file.Dispose();
            File.Delete(Name);
        }

        public void Dispose() => file?.Dispose();

        public byte[] Read(long position, int size)
        {
            var data = new byte[size];

            file.Seek(position, SeekOrigin.Begin);
            file.Read(data, 0, size);

            return data;
        }

        public bool HasContent() => file.Position < file.Length - 1;
    }
}
