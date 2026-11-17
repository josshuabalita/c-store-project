namespace ColumnStore.Core.Compression;

/// <summary>A piece of a compressed column. Operators check the properties to skip decompression.</summary>
public interface ICompressionBlock
{
    /// <summary>Every row has the same value.</summary>
    bool IsOneValue { get; }

    /// <summary>Values never decrease in position order.</summary>
    bool IsValueSorted { get; }

    /// <summary>Covers every position from StartPosition to EndPosition.</summary>
    bool IsPosContiguous { get; }

    /// <summary>Number of rows.</summary>
    int Size { get; }

    int StartPosition { get; }

    int EndPosition { get; }

    /// <summary>Value at StartPosition.</summary>
    int StartValue { get; }

    /// <summary>All values, in position order.</summary>
    int[] AsArray();

    /// <summary>All positions, ascending.</summary>
    int[] GetPositions();

    /// <summary>One row at a time.</summary>
    IEnumerable<(int Position, int Value)> Iterate();
}
