using SortedStorage.Application.Port.Out;
using System.Collections.Generic;
using System.Linq;

namespace SortedStorage.Application
{
    public class ImutableMemtable
    {
        private readonly IFileReaderPort file;
        private SortedDictionary<string, string> sortedDictionary;

        public ImutableMemtable(IFileReaderPort file, SortedDictionary<string, string> sortedDictionary = null)
        {
            this.file = file;
            if (sortedDictionary != null) this.sortedDictionary = sortedDictionary;
            else LoadDataFromFile();
        }

        private void LoadDataFromFile()
        {
            file.Position = 0;
            sortedDictionary = new SortedDictionary<string, string>();

            while (file.HasContent())
            {
                KeyValueEntry entry = KeyValueEntry.FromFileReader(file);
                sortedDictionary.Add(entry.Key, entry.Value);
            }
        }

        public IEnumerable<KeyValuePair<string, string>> GetData() => sortedDictionary.AsEnumerable();

        public void Delete() => file?.Delete();

        public string GetFileName() => file.Name;

        public string Get(string key) => sortedDictionary.GetValueOrDefault(key);
    }
}
