using FluentAssertions;
using SortedStorage.Application;
using SortedStorage.Application.Port.Out;
using SortedStorage.Tests.Adapter.Out;
using System.Collections.Generic;
using Xunit;

namespace SortedStorage.Tests
{
    public class SSTableTests
    {
        [Fact]
        public void Build_from_imutable_table_keeps_data()
        {
            FileTestManagerAdapter fileManager = new FileTestManagerAdapter();
            SSTable sstable = BuildSSTableFromImutableMemtable(fileManager, new List<KeyValueEntry> { 
                new KeyValueEntry("key1", "value1"),
                new KeyValueEntry("key2", "value2")
            });

            sstable.Get("key1").Should().Be("value1");
            sstable.Get("key2").Should().Be("value2");
        }

        [Fact]
        public void Build_from_file_fills_index()
        {
            var dataList = new List<byte>();
            dataList.AddRange(KeyValueEntry.ToBytes("key1", "value1"));
            int secondRegisterPosition = dataList.Count;
            dataList.AddRange(KeyValueEntry.ToBytes("key2", "value2"));
            var dataFile = new FileTestReaderAdapter("data", dataList);

            var indexList = new List<byte>();
            indexList.AddRange(IndexEntry.ToBytes("key1", 0));
            indexList.AddRange(IndexEntry.ToBytes("key2", secondRegisterPosition));
            var indexFile = new FileTestReaderAdapter("index", indexList);

            SSTable sstable = SSTable.Load(indexFile, dataFile);

            sstable.Get("key1").Should().Be("value1");
            sstable.Get("key2").Should().Be("value2");
        }

        [Fact]
        public void Merge_files_guarantees_final_sstable_order()
        {
            FileTestManagerAdapter fileManager = new FileTestManagerAdapter();

            SSTable sstable1 = BuildSSTableFromImutableMemtable(fileManager, new List<KeyValueEntry> {
                new KeyValueEntry("key1", "value1"),
                new KeyValueEntry("key4", "value4"),
                new KeyValueEntry("key5", "value5")
            });

            SSTable sstable2 = BuildSSTableFromImutableMemtable(fileManager, new List<KeyValueEntry> {
                new KeyValueEntry("key2", "value2"),
                new KeyValueEntry("key3", "value3"),
                new KeyValueEntry("key6", "value6")
            });

            SSTable result = sstable2.Merge(sstable1, fileManager);

            var enumerator = result.GetAll().GetEnumerator();

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("key1");
            enumerator.Current.Value.Should().Be("value1");

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("key2");
            enumerator.Current.Value.Should().Be("value2");

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("key3");
            enumerator.Current.Value.Should().Be("value3");

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("key4");
            enumerator.Current.Value.Should().Be("value4");

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("key5");
            enumerator.Current.Value.Should().Be("value5");

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("key6");
            enumerator.Current.Value.Should().Be("value6");

            enumerator.MoveNext().Should().BeFalse();
        }

        [Fact]
        public void Merge_files_preserves_values_from_supplied_sstable_when_same_key()
        {
            FileTestManagerAdapter fileManager = new FileTestManagerAdapter();

            SSTable older = BuildSSTableFromImutableMemtable(fileManager, new List<KeyValueEntry> {
                new KeyValueEntry("key1", "value1"),
                new KeyValueEntry("key4", "value4-1"),
            });

            SSTable newer = BuildSSTableFromImutableMemtable(fileManager, new List<KeyValueEntry> {
                new KeyValueEntry("key2", "value2"),
                new KeyValueEntry("key4", "value4-2"),
            });

            SSTable result = older.Merge(newer, fileManager);

            result.Get("key4").Should().Be("value4-2");
        }

        private static SSTable BuildSSTableFromImutableMemtable(IFileManagerPort fileManager, IEnumerable<KeyValueEntry> entries)
        {
            var file = fileManager.OpenOrCreateToWriteSingleFile(FileType.MemtableWriteAheadLog);
            Memtable memtable = new Memtable(file);

            foreach (var item in entries)
            {
                memtable.Add(item.Key, item.Value);
            }

            SSTable sstable = SSTable.From(memtable.ToImutable(), fileManager);
            return sstable;
        }
    }
}
