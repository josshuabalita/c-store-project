using ColumnStore.Core.Operators;

namespace ColumnStore.Tests;

public sealed class ArrayPositionListTests
{
    [Fact]
    public void Intersect_ReturnsCommonPositions()
    {
        var a = new ArrayPositionList([1, 3, 5, 7, 9]);
        var b = new ArrayPositionList([3, 4, 5, 9, 10]);

        Assert.Equal([3, 5, 9], a.Intersect(b).ToArray());
    }

    [Fact]
    public void Intersect_WithEmptyOrDisjointList_IsEmpty()
    {
        var a = new ArrayPositionList([1, 2, 3]);

        Assert.Equal(0, a.Intersect(new ArrayPositionList([])).Count);
        Assert.Equal(0, a.Intersect(new ArrayPositionList([4, 5])).Count);
    }

    [Fact]
    public void Intersect_WithItself_ReturnsSameList()
    {
        var a = new ArrayPositionList([2, 4, 6]);

        Assert.Equal([2, 4, 6], a.Intersect(a).ToArray());
    }

    [Theory]
    [InlineData(new[] { 3, 1 })]
    [InlineData(new[] { 1, 1 })]
    public void Constructor_RejectsUnsortedOrDuplicatePositions(int[] positions)
    {
        Assert.Throws<ArgumentException>(() => new ArrayPositionList(positions));
    }

    [Fact]
    public void ToArray_ReturnsACopy()
    {
        var list = new ArrayPositionList([1, 2]);

        list.ToArray()[0] = 99;

        Assert.Equal([1, 2], list.ToArray());
    }
}
