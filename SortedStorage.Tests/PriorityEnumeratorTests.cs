using FluentAssertions;
using SortedStorage.Application;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace SortedStorage.Tests
{
    public class PriorityEnumeratorTests
    {
        [Fact]
        public async Task Return_ordered_elements_when_sources_are_ordered()
        {
            // Arrange
            var list1 = new Dictionary<string, string>()
            {
                ["a"] = "test_a",
                ["d"] = "test_d",
                ["f"] = "test_g",
            };

            var list2 = new Dictionary<string, string>()
            {
                ["b"] = "test_b",
            };

            var list3 = new Dictionary<string, string>()
            {
                ["c"] = "test_a",
                ["e"] = "test_d",
            };

            PriorityEnumerator priorityEnumerator = new PriorityEnumerator(new List<IAsyncEnumerable<KeyValuePair<string, string>>>()
            {
                ToAsyncEnumerable(list1), ToAsyncEnumerable(list2), ToAsyncEnumerable(list3)
            });

            // Act
            var enumerator = priorityEnumerator.GetAll().GetAsyncEnumerator();

            // Assert
            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.Key.Should().Be("a");

            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.Key.Should().Be("b");

            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.Key.Should().Be("c");

            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.Key.Should().Be("d");

            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.Key.Should().Be("e");

            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.Key.Should().Be("f");

            (await enumerator.MoveNextAsync()).Should().BeFalse();
        }

        [Fact]
        public async Task Enumerator_evaluates_after_first_move_next()
        {
            // Arrange
            var list1 = new Dictionary<string, string>();
            var list2 = new Dictionary<string, string>();

            PriorityEnumerator priorityEnumerator = new PriorityEnumerator(new IAsyncEnumerable<KeyValuePair<string, string>>[]
            {
                ToAsyncEnumerable(list1), ToAsyncEnumerable(list2)
            });

            // Act
            var enumerator = priorityEnumerator.GetAll().GetAsyncEnumerator();
            list1.Add("a", "test_a");

            // Assert
            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.Key.Should().Be("a");
        }

        [Fact]
        public async Task After_move_next_enumerator_do_not_get_updated_with_new_items()
        {
            // Arrange
            var list1 = new Dictionary<string, string>()
            {
                ["a"] = "test_a"
            };
            var list2 = new Dictionary<string, string>()
            {
                ["b"] = "test_b"
            };

            PriorityEnumerator priorityEnumerator = new PriorityEnumerator(new IAsyncEnumerable<KeyValuePair<string, string>>[]
            {
                ToAsyncEnumerable(list1), ToAsyncEnumerable(list2)
            });

            // Act
            var enumerator = priorityEnumerator.GetAll().GetAsyncEnumerator();
            await enumerator.MoveNextAsync();
            list1.Add("c", "test_c");

            // Assert
            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.Key.Should().Be("b");

            (await enumerator.MoveNextAsync()).Should().BeFalse();
        }

        [Fact]
        public async Task Empty_enumerators_creates_an_empty_enumerator()
        {
            // Arrange
            var list1 = new Dictionary<string, string>();
            var list2 = new Dictionary<string, string>();

            PriorityEnumerator priorityEnumerator = new PriorityEnumerator(new IAsyncEnumerable<KeyValuePair<string, string>>[]
            {
                ToAsyncEnumerable(list1), ToAsyncEnumerable(list2)
            });

            // Act
            var enumerator = priorityEnumerator.GetAll().GetAsyncEnumerator();

            // Assert
            (await enumerator.MoveNextAsync()).Should().BeFalse();
        }

        [Fact]
        public async Task Same_key_preserve_values_of_last_enumerator()
        {
            // Arrange
            var list1 = new Dictionary<string, string>()
            {
                ["a"] = "test_a",
                ["d"] = "test_d-1"
            };

            var list2 = new Dictionary<string, string>()
            {
                ["b"] = "test_b",
                ["d"] = "test_d-2"
            };

            var list3 = new Dictionary<string, string>()
            {
                ["c"] = "test_c",
                ["d"] = "test_d-3",
            };

            PriorityEnumerator priorityEnumerator = new PriorityEnumerator(new List<IAsyncEnumerable<KeyValuePair<string, string>>>()
            {
                ToAsyncEnumerable(list1), ToAsyncEnumerable(list2), ToAsyncEnumerable(list3)
            });

            // Act
            var enumerator = priorityEnumerator.GetAll().GetAsyncEnumerator();

            // Assert
            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.Key.Should().Be("a");

            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.Key.Should().Be("b");

            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.Key.Should().Be("c");

            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.Key.Should().Be("d");
            enumerator.Current.Value.Should().Be("test_d-3");

            (await enumerator.MoveNextAsync()).Should().BeFalse();
        }

        public static async IAsyncEnumerable<T> ToAsyncEnumerable<T>(IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
            {
                yield return await Task.FromResult(item);
            }
        }
    }
}
