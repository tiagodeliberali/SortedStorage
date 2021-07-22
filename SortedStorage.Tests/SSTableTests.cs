using FluentAssertions;
using SortedStorage.Application;
using SortedStorage.Tests.Adapter.Out;
using Xunit;

namespace SortedStorage.Tests
{
    public class SSTableTests
    {
        [Fact]
        public void Build_from_imutable_table_keeps_data()
        {
            FileTestManagerAdapter fileManager = new FileTestManagerAdapter();
            var file = fileManager.OpenOrCreateToWriteSingleFile(FileType.MemtableWriteAheadLog);
            Memtable memtable = new Memtable(file);

            memtable.Add("key1", "value1");
            memtable.Add("key2", "value2");


            SSTable sstable = SSTable.From(memtable.ToImutable(), fileManager);

            sstable.Get("key1").Should().Be("value1");
            sstable.Get("key2").Should().Be("value2");
        }
    }
}
