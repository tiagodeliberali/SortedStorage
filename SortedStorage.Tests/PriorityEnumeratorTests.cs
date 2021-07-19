using SortedStorage.Application;
using System.Collections.Generic;
using Xunit;

namespace SortedStorage.Tests
{
    public class PriorityEnumeratorTests
    {
        [Fact]
        public void should_return_ordered_elements_when_sources_are_ordered()
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

            PriorityEnumerator priorityEnumerator = new PriorityEnumerator(new List<IEnumerable<KeyValuePair<string, string>>>()
            {
                list3, list2, list1
            });

            // Act
            var enumerator = priorityEnumerator.GetAll().GetEnumerator();

            // Assert
            Assert.True(enumerator.MoveNext());
            Assert.Equal("a", enumerator.Current.Key);

            Assert.True(enumerator.MoveNext());
            Assert.Equal("b", enumerator.Current.Key);

            Assert.True(enumerator.MoveNext());
            Assert.Equal("c", enumerator.Current.Key);

            Assert.True(enumerator.MoveNext());
            Assert.Equal("d", enumerator.Current.Key);

            Assert.True(enumerator.MoveNext());
            Assert.Equal("e", enumerator.Current.Key);

            Assert.True(enumerator.MoveNext());
            Assert.Equal("f", enumerator.Current.Key);

            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void enumerator_evaluates_after_first_move_next()
        {
            // Arrange
            var list1 = new Dictionary<string, string>();
            var list2 = new Dictionary<string, string>();

            PriorityEnumerator priorityEnumerator = new PriorityEnumerator(new IEnumerable<KeyValuePair<string, string>>[]
            {
                list2, list1
            });

            // Act
            var enumerator = priorityEnumerator.GetAll().GetEnumerator();
            list1.Add("a", "test_a");

            // Assert
            Assert.True(enumerator.MoveNext());
            Assert.Equal("a", enumerator.Current.Key);
        }

        [Fact]
        public void after_move_next_enumerator_do_not_get_updated_with_new_items()
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

            PriorityEnumerator priorityEnumerator = new PriorityEnumerator(new IEnumerable<KeyValuePair<string, string>>[]
            {
                list2, list1
            });

            // Act
            var enumerator = priorityEnumerator.GetAll().GetEnumerator();
            enumerator.MoveNext();
            list1.Add("c", "test_c");

            // Assert
            Assert.True(enumerator.MoveNext());
            Assert.Equal("b", enumerator.Current.Key);

            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void empty_enumerators_creates_an_empty_enumerator()
        {
            // Arrange
            var list1 = new Dictionary<string, string>();
            var list2 = new Dictionary<string, string>();

            PriorityEnumerator priorityEnumerator = new PriorityEnumerator(new IEnumerable<KeyValuePair<string, string>>[]
            {
                list2, list1
            });

            // Act
            var enumerator = priorityEnumerator.GetAll().GetEnumerator();

            // Assert
            Assert.False(enumerator.MoveNext());
        }
    }
}
