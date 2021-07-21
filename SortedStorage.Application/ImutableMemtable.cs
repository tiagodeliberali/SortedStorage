using SortedStorage.Application.Port.Out;
using System.Collections.Generic;
using System.Linq;

namespace SortedStorage.Application
{
    public class ImutableMemtable
    {
        private readonly IFileReaderPort file;
        private readonly SortedDictionary<string, string> sortedDictionary;

        public ImutableMemtable(IFileReaderPort file, SortedDictionary<string, string> sortedDictionary)
        {
            this.file = file;
            this.sortedDictionary = sortedDictionary;
        }

        public IEnumerable<KeyValuePair<string, string>> GetData() => sortedDictionary.AsEnumerable();

        public void Delete() => file?.Delete();

        public string GetFileName() => file.Name;

        public string Get(string key) => sortedDictionary.GetValueOrDefault(key);
    }
}
