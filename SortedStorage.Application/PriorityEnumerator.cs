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
            var enumerators = new List<IAsyncEnumerator<KeyValuePair<string, string>>>();

            foreach (var item in enumerables)
            {
                var enumerator = item.GetAsyncEnumerator();
                if (await enumerator.MoveNextAsync())
                {
                    enumerators.Add(enumerator);
                }
            }

            while (enumerators.Count > 0)
            {
                var keyValueResult = enumerators
                    .Select((enumerator, index) => new PriorityEnumeratorEntry(index, enumerator))
                    .OrderBy(x => x.Enumerator.Current.Key)
                    .ThenByDescending(x => x.Order)
                    .First()
                    .Enumerator
                    .Current;

                var sameKeyEnumerators = enumerators
                    .Where(x => x.Current.Key == keyValueResult.Key)
                    .ToList();

                foreach (var item in sameKeyEnumerators)
                {
                    if (!(await item.MoveNextAsync()))
                    {
                        enumerators.Remove(item);
                    }
                }


                yield return keyValueResult;
            }
        }

        class PriorityEnumeratorEntry
        {
            public int Order { get; }
            public IAsyncEnumerator<KeyValuePair<string, string>> Enumerator { get; }

            public PriorityEnumeratorEntry(int order, IAsyncEnumerator<KeyValuePair<string, string>> enumerator)
            {
                Order = order;
                Enumerator = enumerator;
            }
        }
    }
}
