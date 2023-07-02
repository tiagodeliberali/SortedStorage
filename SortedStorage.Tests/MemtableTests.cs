namespace SortedStorage.Tests;

using FluentAssertions;

using SortedStorage.Application;
using SortedStorage.Tests.Adapter.Out;

using System.Threading.Tasks;

using Xunit;

public class MemtableTests
{
    [Fact]
    public async Task Add_existent_keys_overrides_content()
    {
        FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
        Memtable memtable = await Memtable.LoadFromFile(fileWriter);

        memtable.Add("key", "ação test");
        memtable.Add("key", "new content");

        memtable.Get("key").Should().Be("new content");
    }

    [Fact]
    public async Task Cannot_add_register_with_tombstone_value()
    {
        FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
        Memtable memtable = await Memtable.LoadFromFile(fileWriter);

        Assert.Throws<InvalidEntryValueException>(() => memtable.Add("key", StorageConfiguration.Tombstone));
    }

    [Fact]
    public async Task Delete_entry_add_value_with_tombstone()
    {
        FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
        Memtable memtable = await Memtable.LoadFromFile(fileWriter);

        memtable.Add("key", "ação test");
        memtable.Remove("key");

        memtable.Get("key").Should().Be(StorageConfiguration.Tombstone);

        var reader = fileWriter.GetReader();
        reader.Position = 0;
        KeyValueEntry entry1 = await KeyValueEntry.FromFileReader(reader);
        entry1.Key.Should().Be("key");
        entry1.Value.Should().Be("ação test");

        KeyValueEntry entry2 = await KeyValueEntry.FromFileReader(reader);
        entry2.Key.Should().Be("key");
        entry2.Value.Should().Be(StorageConfiguration.Tombstone);
    }

    [Fact]
    public async Task Add_key_values_replicate_to_file()
    {
        FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
        Memtable memtable = await Memtable.LoadFromFile(fileWriter);

        memtable.Add("firt key", "ação test");
        memtable.Add("second key", "você know");

        var reader = fileWriter.GetReader();
        reader.Position = 0;
        KeyValueEntry entry1 = await KeyValueEntry.FromFileReader(reader);
        entry1.Key.Should().Be("firt key");
        entry1.Value.Should().Be("ação test");

        KeyValueEntry entry2 = await KeyValueEntry.FromFileReader(reader);
        entry2.Key.Should().Be("second key");
        entry2.Value.Should().Be("você know");
    }

    [Fact]
    public async Task Load_file_with_data_makes_it_available_to_search()
    {
        FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
        Memtable memtable = await Memtable.LoadFromFile(fileWriter);

        memtable.Add("firt key", "ação test");
        memtable.Add("second key", "você know");

        Memtable loadedMemtable = await Memtable.LoadFromFile(fileWriter);

        loadedMemtable.Get("firt key").Should().Be("ação test");
        loadedMemtable.Get("second key").Should().Be("você know");
    }

    [Fact]
    public async Task Convert_to_imutable_preserve_existing_data()
    {
        FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
        Memtable memtable = await Memtable.LoadFromFile(fileWriter);

        memtable.Add("firt key", "ação test");
        memtable.Add("second key", "você know");

        ImutableMemtable imutableMemtable = memtable.ToImutable();

        imutableMemtable.Get("firt key").Should().Be("ação test");
        imutableMemtable.Get("second key").Should().Be("você know");
    }

    [Fact]
    public async Task Load_imutable_memtable_from_file_with_data_makes_it_available_to_search()
    {
        FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
        Memtable memtable = await Memtable.LoadFromFile(fileWriter);

        memtable.Add("firt key", "ação test");
        memtable.Add("second key", "você know");

        ImutableMemtable imutableMemtable = await ImutableMemtable.BuildFromFile(fileWriter.ToReadOnly(FileType.MemtableReadOnly));

        imutableMemtable.Get("firt key").Should().Be("ação test");
        imutableMemtable.Get("second key").Should().Be("você know");
    }

    [Fact]
    public async Task Get_information_inside_range_containing_both_extremes()
    {
        FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
        Memtable memtable = await Memtable.LoadFromFile(fileWriter);

        memtable.Add("c", "value c");
        memtable.Add("e", "value e");
        memtable.Add("b", "value b");
        memtable.Add("d", "value d");
        memtable.Add("a", "value a");

        var enumerator = memtable.GetInRange("b", "d").GetAsyncEnumerator();

        (await enumerator.MoveNextAsync()).Should().BeTrue();
        enumerator.Current.Key.Should().Be("b");

        (await enumerator.MoveNextAsync()).Should().BeTrue();
        enumerator.Current.Key.Should().Be("c");

        (await enumerator.MoveNextAsync()).Should().BeTrue();
        enumerator.Current.Key.Should().Be("d");

        (await enumerator.MoveNextAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task Get_information_inside_range_when_keys_do_not_exist()
    {
        FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
        Memtable memtable = await Memtable.LoadFromFile(fileWriter);

        memtable.Add("c1", "value c");
        memtable.Add("e1", "value e");
        memtable.Add("b1", "value b");
        memtable.Add("d1", "value d");
        memtable.Add("a1", "value a");

        var enumerator = memtable.GetInRange("b", "d").GetAsyncEnumerator();

        (await enumerator.MoveNextAsync()).Should().BeTrue();
        enumerator.Current.Key.Should().Be("b1");

        (await enumerator.MoveNextAsync()).Should().BeTrue();
        enumerator.Current.Key.Should().Be("c1");

        (await enumerator.MoveNextAsync()).Should().BeFalse();
    }
}
