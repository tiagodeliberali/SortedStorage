﻿namespace SortedStorage.Application;

using SortedStorage.Application.Port.Out;
using SortedStorage.Application.SymbolTable;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Memtable : IDisposable
{
    private readonly RedBlackTree<string, string> sortedDictionary = new RedBlackTree<string, string>();
    private readonly IFileWriterPort file;

    private bool isReadOnly = false;

    public int Size => sortedDictionary.Size;

    private Memtable(IFileWriterPort file)
    {
        this.file = file;
    }

    public static async Task<Memtable> LoadFromFile(IFileWriterPort file)
    {
        var memtable = new Memtable(file);
        await memtable.LoadFile();

        return memtable;
    }

    private async Task LoadFile()
    {
        file.Position = 0;
        while (file.HasContent())
        {
            KeyValueEntry entry = await KeyValueEntry.FromFileReader(file);
            sortedDictionary[entry.Key] = entry.Value;
        }
    }

    public void Add(string key, string value)
    {
        if (value == StorageConfiguration.Tombstone)
            throw new InvalidEntryValueException($"Invalid value '{value}'. It is used as tombstone.");

        AddEntryWithLock(key, value);
    }

    public void Remove(string key) => AddEntryWithLock(key, StorageConfiguration.Tombstone);

    private void AddEntryWithLock(string key, string value)
    {
        lock (file)
        {
            if (isReadOnly)
                throw new InvalidWriteToReadOnlyException("Tried to add entry to read only memtable");

            file.Append(KeyValueEntry.ToBytes(key, value));
            sortedDictionary[key] = value;
        }
    }

    public string Get(string key) => sortedDictionary.Get(key);

    public IEnumerable<KeyValuePair<string, string>> GetData()
    {
        foreach (var item in sortedDictionary.GetAll())
        {
            yield return KeyValuePair.Create(item.Key, item.Value);
        }
    }

    public async IAsyncEnumerable<KeyValuePair<string, string>> GetInRange(string start, string end)
    {
        foreach (var item in sortedDictionary.GetInRange(start, end))
        {
            yield return await Task.FromResult(KeyValuePair.Create(item.Key, item.Value));
        }
    }

    public void DeleteFile() => file?.Delete();

    public string GetFileName() => file.Name;

    public void Dispose() => file?.Dispose();

    public ImutableMemtable ToImutable()
    {
        lock (file)
        {
            isReadOnly = true;
            return new ImutableMemtable(file.ToReadOnly(FileType.MemtableReadOnly), sortedDictionary);
        }
    }
}
