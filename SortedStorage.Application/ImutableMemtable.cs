using SortedStorage.Application.Port.Out;
using SortedStorage.Application.SymbolTable;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SortedStorage.Application
{
    public class ImutableMemtable
    {
        private readonly IFileReaderPort file;
        private RedBlackTree<string, string> sortedDictionary;

        public ImutableMemtable(IFileReaderPort file, RedBlackTree<string, string> sortedDictionary)
        {
            this.file = file;
            this.sortedDictionary = sortedDictionary ?? new RedBlackTree<string, string>();
        }

        public static async Task<ImutableMemtable> BuildFromFile(IFileReaderPort file)
        {
            var memtable = new ImutableMemtable(file, null);
            await memtable.LoadDataFromFile();
            return memtable;
        }

        private async Task LoadDataFromFile()
        {
            file.Position = 0;
            sortedDictionary = new RedBlackTree<string, string>();

            while (file.HasContent())
            {
                KeyValueEntry entry = await KeyValueEntry.FromFileReader(file);
                sortedDictionary[entry.Key] = entry.Value;
            }
        }

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

        public void Delete() => file?.Delete();

        public string GetFileName() => file.Name;

        public string Get(string key) => sortedDictionary.Get(key);
    }
}
