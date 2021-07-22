﻿using FluentAssertions;
using SortedStorage.Application;
using SortedStorage.Tests.Adapter.Out;
using Xunit;

namespace SortedStorage.Tests
{
    public class MemtableTests
    {
        [Fact]
        public void Add_existent_keys_overrides_content()
        {
            FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
            Memtable memtable = new Memtable(fileWriter);

            memtable.Add("key", "ação test");
            memtable.Add("key", "new content");

            memtable.Get("key").Should().Be("new content");
        }

        [Fact]
        public void Cannot_add_register_with_tombstone_value()
        {
            FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
            Memtable memtable = new Memtable(fileWriter);

            Assert.Throws<InvalidEntryValueException>(() => memtable.Add("key", StorageConfiguration.TOMBSTONE));
        }

        [Fact]
        public void Delete_entry_add_value_with_tombstone()
        {
            FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
            Memtable memtable = new Memtable(fileWriter);

            memtable.Add("key", "ação test");
            memtable.Remove("key");

            memtable.Get("key").Should().Be(StorageConfiguration.TOMBSTONE);

            var reader = fileWriter.GetReader();
            reader.Position = 0;
            KeyValueEntry entry1 = KeyValueEntry.FromFileReader(reader);
            entry1.Key.Should().Be("key");
            entry1.Value.Should().Be("ação test");

            KeyValueEntry entry2 = KeyValueEntry.FromFileReader(reader);
            entry2.Key.Should().Be("key");
            entry2.Value.Should().Be(StorageConfiguration.TOMBSTONE);
        }

        [Fact]
        public void Add_key_values_replicate_to_file()
        {
            FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
            Memtable memtable = new Memtable(fileWriter);

            memtable.Add("firt key", "ação test");
            memtable.Add("second key", "você know");

            var reader = fileWriter.GetReader();
            reader.Position = 0;
            KeyValueEntry entry1 = KeyValueEntry.FromFileReader(reader);
            entry1.Key.Should().Be("firt key");
            entry1.Value.Should().Be("ação test");

            KeyValueEntry entry2 = KeyValueEntry.FromFileReader(reader);
            entry2.Key.Should().Be("second key");
            entry2.Value.Should().Be("você know");
        }

        [Fact]
        public void Load_file_with_data_makes_it_available_to_search()
        {
            FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
            Memtable memtable = new Memtable(fileWriter);

            memtable.Add("firt key", "ação test");
            memtable.Add("second key", "você know");

            Memtable loadedMemtable = new Memtable(fileWriter);

            loadedMemtable.Get("firt key").Should().Be("ação test");
            loadedMemtable.Get("second key").Should().Be("você know");
        }

        [Fact]
        public void Convert_to_imutable_preserve_existing_data()
        {
            FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
            Memtable memtable = new Memtable(fileWriter);

            memtable.Add("firt key", "ação test");
            memtable.Add("second key", "você know");

            ImutableMemtable imutableMemtable = memtable.ToImutable();

            imutableMemtable.Get("firt key").Should().Be("ação test");
            imutableMemtable.Get("second key").Should().Be("você know");
        }

        [Fact]
        public void Load_imutable_memtable_from_file_with_data_makes_it_available_to_search()
        {
            FileTestWriterAdapter fileWriter = new FileTestWriterAdapter("test");
            Memtable memtable = new Memtable(fileWriter);

            memtable.Add("firt key", "ação test");
            memtable.Add("second key", "você know");

            ImutableMemtable imutableMemtable = new ImutableMemtable(fileWriter.ToReadOnly(FileType.MemtableReadOnly));

            imutableMemtable.Get("firt key").Should().Be("ação test");
            imutableMemtable.Get("second key").Should().Be("você know");
        }
    }
}
