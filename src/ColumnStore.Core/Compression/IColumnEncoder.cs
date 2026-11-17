namespace ColumnStore.Core.Compression;

public enum EncodingKind : byte
{
    Uncompressed = 0,
    Rle = 1,
    Dictionary = 2,
    BitVector = 3,
}

/// <summary>
/// Encodes one chunk of a column (a run of consecutive int values) into bytes, and reads it back as blocks.
/// All columns are 32-bit integers: money is stored as cents, dates as days since the epoch.
/// </summary>
public interface IColumnEncoder
{
    EncodingKind Kind { get; }

    /// <summary>Encodes the chunk. Must be deterministic: the same input gives the same bytes.</summary>
    byte[] Encode(ReadOnlySpan<int> values);

    /// <summary>
    /// Parses an encoded chunk into blocks that together cover positions
    /// <paramref name="startPosition"/> .. <paramref name="startPosition"/> + n - 1 exactly once.
    /// Throws <see cref="InvalidDataException"/> on truncated or corrupt input. An empty chunk yields no blocks.
    /// </summary>
    IReadOnlyList<ICompressionBlock> ReadBlocks(ReadOnlyMemory<byte> data, int startPosition);
}
