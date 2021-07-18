using SortedStorage.Application.Port.Out;
using System;
using System.IO;

namespace SortedStorage.Adapter.Out
{
    public class FileAdapter : IFilePort
    {
        private readonly FileStream file;

        public FileAdapter(string basePath)
        {
            string path = Path.Combine(basePath, $"{Guid.NewGuid()}.dat");
            file = new FileStream(path, FileMode.Append, FileAccess.Write);
        }

        public void Append(byte[] keyValue)
        {
            file.Write(keyValue, 0, keyValue.Length);
            file.Flush();
        }

        public void Dispose()
        {
            if (file != null) file.Dispose();
        }
    }
}
