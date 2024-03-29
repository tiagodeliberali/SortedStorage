namespace SortedStorage.Tests;

using FluentAssertions;

using SortedStorage.Application;
using SortedStorage.Tests.Adapter.Out;

using System;
using System.Text;
using System.Threading.Tasks;

using Xunit;

public class EntryTests
{
    [Fact]
    public void Keyvalue_build_byte_array()
    {
        KeyValueEntry keyValue = new KeyValueEntry("key", "a��o");

        byte[] data = keyValue.ToBytes();

        BitConverter.ToUInt32(data, 0).Should().Be(keyValue.GetCrc32());
        BitConverter.ToInt32(data, 4).Should().Be(3);
        BitConverter.ToInt32(data, 8).Should().Be(6);
        Encoding.UTF8.GetString(data, 12, 9).Should().Be("keya��o");
    }

    [Fact]
    public async Task Keyvalue_build_from_file_reader()
    {
        FileTestReaderAdapter fileReader = new FileTestReaderAdapter("test");
        KeyValueEntry keyValue = new KeyValueEntry("key", "a��o");
        fileReader.LoadForTest(keyValue.ToBytes());

        fileReader.Position = 0;
        KeyValueEntry result = await KeyValueEntry.FromFileReader(fileReader);

        result.Key.Should().Be("key");
        result.Value.Should().Be("a��o");
    }

    [Fact]
    public void Indice_buid_from_byte_array()
    {
        IndexEntry index = new IndexEntry("a��o", 123456789);

        byte[] data = index.ToBytes();

        BitConverter.ToInt64(data, 0).Should().Be(123456789);
        BitConverter.ToInt32(data, 8).Should().Be(6);
        Encoding.UTF8.GetString(data, 12, 6).Should().Be("a��o");
    }
}
