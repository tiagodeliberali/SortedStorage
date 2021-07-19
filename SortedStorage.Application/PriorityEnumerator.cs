using System.Collections.Generic;
using System.Linq;

namespace SortedStorage.Application
{
    public class PriorityEnumerator
    {
        private readonly IEnumerable<IEnumerable<KeyValuePair<string, string>>> enumerables;

        public PriorityEnumerator(IEnumerable<IEnumerable<KeyValuePair<string, string>>> enumerables)
        {
            this.enumerables = enumerables;
        }

        public IEnumerable<KeyValuePair<string, string>> GetAll()
        {
            SortedDictionary<string, IEnumerator<KeyValuePair<string, string>>> enumerators = new SortedDictionary<string, IEnumerator<KeyValuePair<string, string>>>();

            foreach (var item in enumerables)
            {
                var enumerator = item.GetEnumerator();

                if (enumerator.MoveNext())
                {
                    enumerators.Add(enumerator.Current.Key, enumerator);
                }
            }

            while (enumerators.Count > 0)
            {
                var nextEnumerator = enumerators.First();
                enumerators.Remove(nextEnumerator.Key);

                var keyValueResult = nextEnumerator.Value.Current;

                if (nextEnumerator.Value.MoveNext())
                {
                    enumerators.Add(nextEnumerator.Value.Current.Key, nextEnumerator.Value);
                }

                yield return keyValueResult;
            }
        }
    }
}
