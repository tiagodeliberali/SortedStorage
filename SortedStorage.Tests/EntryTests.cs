using FluentAssertions;
using SortedStorage.Application;
using SortedStorage.Tests.Adapter.Out;
using System;
using System.Text;
using Xunit;

namespace SortedStorage.Tests
{
    public class EntryTests
    {
        [Fact]
        public void Keyvalue_build_byte_array()
        {
            KeyValueEntry keyValue = new KeyValueEntry("key", "ação");

            byte[] data = keyValue.ToBytes();

            BitConverter.ToUInt32(data, 0).Should().Be(keyValue.GetCrc32());
            BitConverter.ToInt32(data, 4).Should().Be(3);
            BitConverter.ToInt32(data, 8).Should().Be(6);
            Encoding.UTF8.GetString(data, 12, 9).Should().Be("keyação");
        }

        [Fact]
        public void Keyvalue_build_from_file_reader()
        {
            FileTestReaderAdapter fileReader = new FileTestReaderAdapter("test");
            KeyValueEntry keyValue = new KeyValueEntry("key", "ação");
            fileReader.LoadForTest(keyValue.ToBytes());

            KeyValueEntry result = KeyValueEntry.FromFileReader(fileReader, 0);

            result.Key.Should().Be("key");
            result.Value.Should().Be("ação");
        }

        [Fact]
        public void Indice_buid_from_byte_array()
        {
            IndexEntry index = new IndexEntry("ação", 123456789);

            byte[] data = index.ToBytes();

            BitConverter.ToInt32(data, 0).Should().Be(6);
            Encoding.UTF8.GetString(data, 4, 6).Should().Be("ação");
            BitConverter.ToInt64(data, 10).Should().Be(123456789);
        }
    }
}
