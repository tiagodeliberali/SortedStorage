using FluentAssertions;
using SortedStorage.Application;
using System.Collections.Generic;
using Xunit;

namespace SortedStorage.Tests
{
    public class PriorityEnumeratorTests
    {
        [Fact]
        public void Return_ordered_elements_when_sources_are_ordered()
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
            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("a");

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("b");

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("c");

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("d");

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("e");

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("f");

            enumerator.MoveNext().Should().BeFalse();
        }

        [Fact]
        public void Enumerator_evaluates_after_first_move_next()
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
            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("a");
        }

        [Fact]
        public void After_move_next_enumerator_do_not_get_updated_with_new_items()
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
            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("b");

            enumerator.MoveNext().Should().BeFalse();
        }

        [Fact]
        public void Empty_enumerators_creates_an_empty_enumerator()
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
            enumerator.MoveNext().Should().BeFalse();
        }

        [Fact]
        public void Same_key_preserve_values_of_last_enumerator()
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

            PriorityEnumerator priorityEnumerator = new PriorityEnumerator(new List<IEnumerable<KeyValuePair<string, string>>>()
            {
                list3, list2, list1
            });

            // Act
            var enumerator = priorityEnumerator.GetAll().GetEnumerator();

            // Assert
            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("a");

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("b");

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("c");

            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Key.Should().Be("d");
            enumerator.Current.Value.Should().Be("test_d-3");

            enumerator.MoveNext().Should().BeFalse();
        }
    }
}
