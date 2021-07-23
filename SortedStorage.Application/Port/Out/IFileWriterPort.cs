using System.Threading.Tasks;

namespace SortedStorage.Application.Port.Out
{
    public interface IFileWriterPort : IFileReaderPort
    {
        Task<long> Append(byte[] keyValue);
        IFileReaderPort ToReadOnly(FileType destinationType);
    }
}
