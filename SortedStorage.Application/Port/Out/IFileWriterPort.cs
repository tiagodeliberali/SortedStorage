namespace SortedStorage.Application.Port.Out;

using System.Threading.Tasks;

public interface IFileWriterPort : IFileReaderPort
{
    Task<long> Append(byte[] keyValue);
    IFileReaderPort ToReadOnly(FileType destinationType);
}
