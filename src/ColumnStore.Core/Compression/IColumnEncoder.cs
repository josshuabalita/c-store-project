namespace ColumnStore.Core.Compression;

public enum EncodingKind : byte
{
    Uncompressed = 0,
    Rle = 1,
    Dictionary = 2,
    BitVector = 3,
}

/// <summary>Encodes a chunk of int values to bytes and reads it back as blocks.</summary>
public interface IColumnEncoder
{
    EncodingKind Kind { get; }

    /// <summary>The same input must give the same bytes.</summary>
    byte[] Encode(ReadOnlySpan<int> values);

    /// <summary>Throws InvalidDataException on bad input. An empty chunk gives no blocks.</summary>
    IReadOnlyList<ICompressionBlock> ReadBlocks(ReadOnlyMemory<byte> data, int startPosition);
}
