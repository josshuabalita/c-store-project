namespace ColumnStore.Core.Compression;

/// <summary>
/// A piece of a compressed column that can describe itself without being decompressed
/// (Abadi, Madden, Ferreira, SIGMOD 2006, section 5). Operators check the properties and take
/// shortcuts, and fall back to <see cref="AsArray"/> / <see cref="Iterate"/> when no shortcut applies.
/// Positions are absolute row ordinals within the column. Blocks are never empty.
/// </summary>
public interface ICompressionBlock
{
    /// <summary>True when every row in the block holds the same value (<see cref="StartValue"/>).</summary>
    bool IsOneValue { get; }

    /// <summary>True when the values are non-decreasing in position order.</summary>
    bool IsValueSorted { get; }

    /// <summary>True when the block covers every position from <see cref="StartPosition"/> to <see cref="EndPosition"/>.</summary>
    bool IsPosContiguous { get; }

    /// <summary>Number of rows the block represents.</summary>
    int Size { get; }

    /// <summary>Position of the first row in the block.</summary>
    int StartPosition { get; }

    /// <summary>Position of the last row in the block.</summary>
    int EndPosition { get; }

    /// <summary>Value at <see cref="StartPosition"/>. For one-value blocks this is the value of every row.</summary>
    int StartValue { get; }

    /// <summary>Decompresses the whole block: the values in position order, length <see cref="Size"/>.</summary>
    int[] AsArray();

    /// <summary>The positions the block covers, in ascending order, length <see cref="Size"/>.</summary>
    int[] GetPositions();

    /// <summary>Lazily decompresses one row at a time (the paper's getNext).</summary>
    IEnumerable<(int Position, int Value)> Iterate();
}
