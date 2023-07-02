namespace SortedStorage.Tests;

using FluentAssertions;

using SortedStorage.Application.SymbolTable;

using Xunit;

public class RedBlackTreeTests
{
    [Fact]
    public void Symbol_table_iterate_keys_ordered()
    {
        // arrange
        var st = new RedBlackTree<string, string>();
        var keys = "S E A R C H E X A M P L E".Split(" ");

        // act
        for (int i = 0; i < keys.Length; i++)
        {
            st.Add(keys[i], i.ToString());
        }

        // assert
        st.IsEmpty().Should().BeFalse();
        st.Size.Should().Be(10);

        var enumerator = st.GetAll().GetEnumerator();

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("A");

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("C");

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("E");

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("H");

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("L");

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("M");

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("P");

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("R");

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("S");

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("X");

        enumerator.MoveNext().Should().BeFalse();
    }

    [Fact]
    public void Symbol_table_iterate_keys_in_range()
    {
        // arrange
        var st = new RedBlackTree<string, string>();
        var keys = "S E A R C H E X A M P L E".Split(" ");

        // act
        for (int i = 0; i < keys.Length; i++)
        {
            st.Add(keys[i], i.ToString());
        }

        // assert
        st.IsEmpty().Should().BeFalse();
        st.Size.Should().Be(10);

        var enumerator = st.GetInRange("F", "T").GetEnumerator();

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("H");

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("L");

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("M");

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("P");

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("R");

        enumerator.MoveNext().Should().BeTrue();
        enumerator.Current.Key.Should().Be("S");

        enumerator.MoveNext().Should().BeFalse();
    }

    [Fact]
    public void Allows_to_search_value_by_key()
    {
        // arrange
        var st = new RedBlackTree<string, string>();
        var keys = "S E A R C H E X A M P L E".Split(" ");

        // act
        for (int i = 0; i < keys.Length; i++)
        {
            st.Add(keys[i], i.ToString());
        }

        // assert
        st.Get("A").Should().Be("8");
        st.Get("C").Should().Be("4");
        st.Get("E").Should().Be("12");
        st.Get("H").Should().Be("5");
        st.Get("L").Should().Be("11");
        st.Get("M").Should().Be("9");
        st.Get("P").Should().Be("10");
        st.Get("R").Should().Be("3");
        st.Get("S").Should().Be("0");
        st.Get("X").Should().Be("7");
    }

    [Fact]
    public void Find_key_by_position()
    {
        // arrange
        var st = new RedBlackTree<string, string>();
        var keys = "S E A R C H E X A M P L E".Split(" ");

        // act
        for (int i = 0; i < keys.Length; i++)
        {
            st.Add(keys[i], i.ToString());
        }

        // assert
        st.Select(0).Should().Be("A");
        st.Select(3).Should().Be("H");
        st.Select(8).Should().Be("S");
        st.Select(10).Should().BeNull();
    }

    [Fact]
    public void Find_position_by_keys()
    {
        // arrange
        var st = new RedBlackTree<string, string>();
        var keys = "S E A R C H E X A M P L E".Split(" ");

        // act
        for (int i = 0; i < keys.Length; i++)
        {
            st.Add(keys[i], i.ToString());
        }

        // assert
        st.Rank("A").Should().Be(0);
        st.Rank("H").Should().Be(3);
        st.Rank("S").Should().Be(8);
        st.Rank("G").Should().Be(-1);
    }

    [Fact]
    public void Find_min_and_max_keys()
    {
        // arrange
        var st = new RedBlackTree<string, string>();
        var keys = "S E A R C H E X A M P L E".Split(" ");

        // act
        for (int i = 0; i < keys.Length; i++)
        {
            st.Add(keys[i], i.ToString());
        }

        // assert
        st.Min.Should().Be("A");
        st.Max.Should().Be("X");
    }

    [Fact]
    public void Floor_can_find_lower_or_equal_key()
    {
        // arrange
        var st = new RedBlackTree<string, string>();
        var keys = "S O M E T H I N G T O F I N D".Split(" ");

        // act
        for (int i = 0; i < keys.Length; i++)
        {
            st.Add(keys[i], i.ToString());
        }

        // assert
        st.GetFloor("M").Should().Be("M");
        st.GetFloor("J").Should().Be("I");
        st.GetFloor("A").Should().BeNull();
    }

    [Fact]
    public void Ceiling_can_find_greater_or_equal_key()
    {
        // arrange
        var st = new RedBlackTree<string, string>();
        var keys = "S E A R C H E X A M P L E".Split(" ");

        // act
        for (int i = 0; i < keys.Length; i++)
        {
            st.Add(keys[i], i.ToString());
        }

        // assert
        st.GetCeiling("C").Should().Be("C");
        st.GetCeiling("D").Should().Be("E");
        st.GetCeiling("Z").Should().BeNull();
    }
}
