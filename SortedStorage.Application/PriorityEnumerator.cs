using System.Collections.Generic;
using System.Linq;

namespace SortedStorage.Application
{
    public class PriorityEnumerator
    {
        private readonly IEnumerable<IAsyncEnumerable<KeyValuePair<string, string>>> enumerables;

        public PriorityEnumerator(IEnumerable<IAsyncEnumerable<KeyValuePair<string, string>>> enumerables)
        {
            this.enumerables = enumerables;
        }

        public async IAsyncEnumerable<KeyValuePair<string, string>> GetAll()
        {
            SortedDictionary<string, IAsyncEnumerator<KeyValuePair<string, string>>> enumerators =
                new SortedDictionary<string, IAsyncEnumerator<KeyValuePair<string, string>>>();

            foreach (var item in enumerables)
            {
                var enumerator = item.GetAsyncEnumerator();

                if (await enumerator.MoveNextAsync())
                {
                    enumerators[enumerator.Current.Key] = enumerator;
                }
            }

            while (enumerators.Count > 0)
            {
                var nextEnumerator = enumerators.First();
                enumerators.Remove(nextEnumerator.Key);

                var keyValueResult = nextEnumerator.Value.Current;

                if (await nextEnumerator.Value.MoveNextAsync())
                {
                    enumerators[nextEnumerator.Value.Current.Key] = nextEnumerator.Value;
                }

                yield return keyValueResult;
            }
        }
    }
}
