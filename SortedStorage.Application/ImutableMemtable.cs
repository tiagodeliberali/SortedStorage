using SortedStorage.Application.Port.Out;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SortedStorage.Application
{
    public class ImutableMemtable
    {
        private readonly IFileReaderPort file;
        private SortedDictionary<string, string> sortedDictionary;

        public ImutableMemtable(IFileReaderPort file, SortedDictionary<string, string> sortedDictionary)
        {
            this.file = file;
            this.sortedDictionary = sortedDictionary ?? new SortedDictionary<string, string>();
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
            sortedDictionary = new SortedDictionary<string, string>();

            while (file.HasContent())
            {
                KeyValueEntry entry = await KeyValueEntry.FromFileReader(file);
                sortedDictionary[entry.Key] = entry.Value;
            }
        }

        public IEnumerable<KeyValuePair<string, string>> GetData() => sortedDictionary.AsEnumerable();

        public void Delete() => file?.Delete();

        public string GetFileName() => file.Name;

        public string Get(string key) => sortedDictionary.GetValueOrDefault(key);
    }
}
