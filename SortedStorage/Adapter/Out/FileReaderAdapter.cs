namespace SortedStorage.Adapter.Out;

using SortedStorage.Application.Port.Out;

using System.IO;
using System.Threading.Tasks;

class FileReaderAdapter : IFileReaderPort
{
    private readonly FileStream file;

    public string Name { get; }

    public long Position
    {
        get => file.Position;
        set => file.Seek(value, SeekOrigin.Begin);
    }

    public FileReaderAdapter(string path)
    {
        Name = path;
        file = new FileStream(path, FileMode.Open, FileAccess.Read);
    }

    public async Task<byte[]> Read(int size)
    {
        var data = new byte[size];
        await file.ReadAsync(data, 0, size);

        return data;
    }

    public bool HasContent() => file.Position < file.Length - 1;

    public void Delete()
    {
        file.Dispose();
        File.Delete(Name);
    }

    public void Dispose() => file?.Dispose();
}
