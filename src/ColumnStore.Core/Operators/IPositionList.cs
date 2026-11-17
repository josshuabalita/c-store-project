namespace ColumnStore.Core.Operators;

/// <summary>A set of row positions, ascending. Filters return these and multi-column filters combine them.</summary>
public interface IPositionList
{
    int Count { get; }

    /// <summary>The positions, ascending.</summary>
    int[] ToArray();

    /// <summary>Positions that are in both lists.</summary>
    IPositionList And(IPositionList other);
}
