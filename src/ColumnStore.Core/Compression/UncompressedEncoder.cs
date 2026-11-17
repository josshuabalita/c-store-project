using System.Buffers.Binary;

namespace ColumnStore.Core.Compression;

/// <summary>Plain fixed-width baseline. Layout (little endian): int32 count, then count x int32.</summary>
public sealed class UncompressedEncoder : IColumnEncoder
{
    private const int HeaderBytes = 4;

    public EncodingKind Kind => EncodingKind.Uncompressed;

    public byte[] Encode(ReadOnlySpan<int> values)
    {
        var buffer = new byte[HeaderBytes + (values.Length * sizeof(int))];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, values.Length);
        for (var i = 0; i < values.Length; i++)
        {
            BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(HeaderBytes + (i * sizeof(int))), values[i]);
        }

        return buffer;
    }

    public IReadOnlyList<ICompressionBlock> ReadBlocks(ReadOnlyMemory<byte> data, int startPosition)
    {
        var span = data.Span;
        if (span.Length < HeaderBytes)
        {
            throw new InvalidDataException("Uncompressed chunk is shorter than its header.");
        }

        var count = BinaryPrimitives.ReadInt32LittleEndian(span);
        if (count < 0 || span.Length != HeaderBytes + ((long)count * sizeof(int)))
        {
            throw new InvalidDataException("Uncompressed chunk length does not match its value count.");
        }

        if (count == 0)
        {
            return [];
        }

        var values = new int[count];
        for (var i = 0; i < count; i++)
        {
            values[i] = BinaryPrimitives.ReadInt32LittleEndian(span[(HeaderBytes + (i * sizeof(int)))..]);
        }

        return [new UncompressedBlock(values, startPosition)];
    }
}

/// <summary>All values of a chunk, plain. Contiguous; sortedness depends on the data.</summary>
public sealed class UncompressedBlock : ICompressionBlock
{
    private readonly int[] _values;

    public UncompressedBlock(int[] values, int startPosition)
    {
        _values = values;
        StartPosition = startPosition;
        IsValueSorted = true;
        for (var i = 1; i < values.Length; i++)
        {
            if (values[i] < values[i - 1])
            {
                IsValueSorted = false;
                break;
            }
        }
    }

    public bool IsOneValue => false;

    public bool IsValueSorted { get; }

    public bool IsPosContiguous => true;

    public int Size => _values.Length;

    public int StartPosition { get; }

    public int EndPosition => StartPosition + _values.Length - 1;

    public int StartValue => _values[0];

    public int[] AsArray() => (int[])_values.Clone();

    public int[] GetPositions()
    {
        var result = new int[_values.Length];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = StartPosition + i;
        }

        return result;
    }

    public IEnumerable<(int Position, int Value)> Iterate()
    {
        for (var i = 0; i < _values.Length; i++)
        {
            yield return (StartPosition + i, _values[i]);
        }
    }
}
