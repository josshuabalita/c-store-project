using System.Buffers.Binary;

namespace ColumnStore.Core.Compression;

/// <summary>
/// Run-length encoding. Layout (little endian): int32 runCount, then runCount x (int32 value, int32 start, int32 length)
/// where start is relative to the chunk. Each run becomes one block.
/// </summary>
public sealed class RleEncoder : IColumnEncoder
{
    private const int HeaderBytes = 4;
    private const int RunBytes = 12;

    public EncodingKind Kind => EncodingKind.Rle;

    public byte[] Encode(ReadOnlySpan<int> values)
    {
        var runs = 0;
        for (var i = 0; i < values.Length; i++)
        {
            if (i == 0 || values[i] != values[i - 1])
            {
                runs++;
            }
        }

        var buffer = new byte[HeaderBytes + (runs * RunBytes)];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, runs);

        var offset = HeaderBytes;
        var runStart = 0;
        for (var i = 1; i <= values.Length; i++)
        {
            if (i == values.Length || values[i] != values[runStart])
            {
                BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset), values[runStart]);
                BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset + 4), runStart);
                BinaryPrimitives.WriteInt32LittleEndian(buffer.AsSpan(offset + 8), i - runStart);
                offset += RunBytes;
                runStart = i;
            }
        }

        return buffer;
    }

    public IReadOnlyList<ICompressionBlock> ReadBlocks(ReadOnlyMemory<byte> data, int startPosition)
    {
        var span = data.Span;
        if (span.Length < HeaderBytes)
        {
            throw new InvalidDataException("RLE chunk is shorter than its header.");
        }

        var runs = BinaryPrimitives.ReadInt32LittleEndian(span);
        if (runs < 0 || span.Length != HeaderBytes + ((long)runs * RunBytes))
        {
            throw new InvalidDataException("RLE chunk length does not match its run count.");
        }

        var blocks = new List<ICompressionBlock>(runs);
        var expectedStart = 0;
        for (var r = 0; r < runs; r++)
        {
            var offset = HeaderBytes + (r * RunBytes);
            var value = BinaryPrimitives.ReadInt32LittleEndian(span[offset..]);
            var start = BinaryPrimitives.ReadInt32LittleEndian(span[(offset + 4)..]);
            var length = BinaryPrimitives.ReadInt32LittleEndian(span[(offset + 8)..]);
            if (length <= 0 || start != expectedStart)
            {
                throw new InvalidDataException("RLE runs are not contiguous and positive.");
            }

            blocks.Add(new RleBlock(value, startPosition + start, length));
            expectedStart = checked(start + length);
        }

        return blocks;
    }
}

/// <summary>One RLE triple (value, start, length): one value, sorted, contiguous.</summary>
public sealed class RleBlock(int value, int startPosition, int length) : ICompressionBlock
{
    public bool IsOneValue => true;

    public bool IsValueSorted => true;

    public bool IsPosContiguous => true;

    public int Size => length;

    public int StartPosition => startPosition;

    public int EndPosition => startPosition + length - 1;

    public int StartValue => value;

    public int[] AsArray()
    {
        var result = new int[length];
        Array.Fill(result, value);
        return result;
    }

    public int[] GetPositions()
    {
        var result = new int[length];
        for (var i = 0; i < length; i++)
        {
            result[i] = startPosition + i;
        }

        return result;
    }

    public IEnumerable<(int Position, int Value)> Iterate()
    {
        for (var i = 0; i < length; i++)
        {
            yield return (startPosition + i, value);
        }
    }
}
