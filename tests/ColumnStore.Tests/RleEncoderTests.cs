using ColumnStore.Core.Compression;
using ColumnStore.Core.Operators;

namespace ColumnStore.Tests;

public sealed class RleEncoderContractTests : EncoderContractTests
{
    protected override IColumnEncoder CreateEncoder() => new RleEncoder();
}

public sealed class UncompressedEncoderContractTests : EncoderContractTests
{
    protected override IColumnEncoder CreateEncoder() => new UncompressedEncoder();
}

public sealed class RleEncoderTests
{
    private readonly RleEncoder _encoder = new();

    [Fact]
    public void Encode_ProducesOneTriplePerRun()
    {
        var blocks = _encoder.ReadBlocks(_encoder.Encode([7, 7, 7, 7, 9, 9, 9, 3, 3]), 0);

        Assert.Collection(
            blocks,
            b => AssertRun(b, value: 7, start: 0, size: 4),
            b => AssertRun(b, value: 9, start: 4, size: 3),
            b => AssertRun(b, value: 3, start: 7, size: 2));
    }

    [Fact]
    public void Encode_ThousandEqualValues_IsOneBlockOfTwelveBytesPlusHeader()
    {
        var bytes = _encoder.Encode(Enumerable.Repeat(7, 1000).ToArray());

        Assert.Equal(4 + 12, bytes.Length);
        var block = Assert.Single(_encoder.ReadBlocks(bytes, 0));
        Assert.True(block.IsOneValue);
        Assert.Equal(1000, block.Size);
    }

    [Fact]
    public void Encode_ShuffledData_IsLargerThanUncompressed()
    {
        // With no runs RLE costs 12 bytes per value against 4: the reason an encoding chooser exists.
        var values = Enumerable.Range(0, 100).Select(i => i % 2).ToArray();

        Assert.True(_encoder.Encode(values).Length > new UncompressedEncoder().Encode(values).Length);
    }

    [Fact]
    public void SumDirect_OnOneValueBlock_NeverDecompresses()
    {
        var spy = new SpyBlock(value: 5, size: 1000);

        Assert.Equal(5000, Aggregates.SumDirect([spy]));
        Assert.Equal(0, spy.DecompressCalls);
    }

    [Fact]
    public void SumDirect_DoesNotOverflowInt32()
    {
        var blocks = _encoder.ReadBlocks(_encoder.Encode(Enumerable.Repeat(int.MaxValue, 1000).ToArray()), 0);

        Assert.Equal(1000L * int.MaxValue, Aggregates.SumDirect(blocks));
    }

    [Fact]
    public void ReadBlocks_RejectsRunsThatLeaveAGap()
    {
        var bytes = _encoder.Encode([1, 1, 2, 2]);
        bytes[4 + 12 + 4] = 5; // second run now claims to start at position 5

        Assert.Throws<InvalidDataException>(() => _encoder.ReadBlocks(bytes, 0));
    }

    private static void AssertRun(ICompressionBlock block, int value, int start, int size)
    {
        Assert.True(block.IsOneValue);
        Assert.True(block.IsValueSorted);
        Assert.True(block.IsPosContiguous);
        Assert.Equal(value, block.StartValue);
        Assert.Equal(start, block.StartPosition);
        Assert.Equal(size, block.Size);
    }

    private sealed class SpyBlock(int value, int size) : ICompressionBlock
    {
        public int DecompressCalls { get; private set; }

        public bool IsOneValue => true;

        public bool IsValueSorted => true;

        public bool IsPosContiguous => true;

        public int Size => size;

        public int StartPosition => 0;

        public int EndPosition => size - 1;

        public int StartValue => value;

        public int[] AsArray()
        {
            DecompressCalls++;
            return Enumerable.Repeat(value, size).ToArray();
        }

        public int[] GetPositions()
        {
            DecompressCalls++;
            return Enumerable.Range(0, size).ToArray();
        }

        public IEnumerable<(int Position, int Value)> Iterate()
        {
            DecompressCalls++;
            return Enumerable.Range(0, size).Select(i => (i, value));
        }
    }
}
