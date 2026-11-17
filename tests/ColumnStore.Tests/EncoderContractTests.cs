using ColumnStore.Core.Compression;
using ColumnStore.Core.Operators;

namespace ColumnStore.Tests;

/// <summary>Tests every encoder must pass. Subclass this and return your encoder from CreateEncoder.</summary>
public abstract class EncoderContractTests
{
    protected abstract IColumnEncoder CreateEncoder();

    private static (int Position, int Value)[] Flatten(IEnumerable<ICompressionBlock> blocks) =>
        blocks.SelectMany(b => b.Iterate()).OrderBy(p => p.Position).ToArray();

    [Theory]
    [MemberData(nameof(TestDatasets.All), MemberType = typeof(TestDatasets))]
    public void RoundTrip_ReconstructsOriginalColumn(string dataset)
    {
        var values = TestDatasets.Get(dataset);
        var encoder = CreateEncoder();

        var blocks = encoder.ReadBlocks(encoder.Encode(values), 0);

        var rebuilt = new int[values.Length];
        foreach (var (position, value) in Flatten(blocks))
        {
            rebuilt[position] = value;
        }

        Assert.Equal(values, rebuilt);
    }

    [Theory]
    [MemberData(nameof(TestDatasets.All), MemberType = typeof(TestDatasets))]
    public void Blocks_CoverEveryPositionExactlyOnce(string dataset)
    {
        var values = TestDatasets.Get(dataset);
        var encoder = CreateEncoder();

        var positions = encoder.ReadBlocks(encoder.Encode(values), 0)
            .SelectMany(b => b.GetPositions())
            .OrderBy(p => p)
            .ToArray();

        Assert.Equal(Enumerable.Range(0, values.Length), positions);
    }

    [Theory]
    [MemberData(nameof(TestDatasets.All), MemberType = typeof(TestDatasets))]
    public void BlockProperties_AreTruthful(string dataset)
    {
        var values = TestDatasets.Get(dataset);
        var encoder = CreateEncoder();

        foreach (var block in encoder.ReadBlocks(encoder.Encode(values), 0))
        {
            var rows = block.Iterate().ToArray();
            var positions = block.GetPositions();
            var array = block.AsArray();

            Assert.True(block.Size > 0);
            Assert.Equal(block.Size, rows.Length);
            Assert.Equal(block.Size, positions.Length);
            Assert.Equal(positions, rows.Select(r => r.Position));
            Assert.Equal(array, rows.Select(r => r.Value));
            Assert.Equal(positions[0], block.StartPosition);
            Assert.Equal(positions[^1], block.EndPosition);
            Assert.Equal(array[0], block.StartValue);
            Assert.All(positions, p => Assert.Equal(array[Array.IndexOf(positions, p)], values[p]));

            if (block.IsOneValue)
            {
                Assert.All(array, v => Assert.Equal(block.StartValue, v));
            }

            if (block.IsValueSorted)
            {
                for (var i = 1; i < array.Length; i++)
                {
                    Assert.True(array[i] >= array[i - 1], "IsValueSorted is true but values decrease.");
                }
            }

            if (block.IsPosContiguous)
            {
                Assert.Equal(Enumerable.Range(block.StartPosition, block.Size), positions);
            }
        }
    }

    [Theory]
    [MemberData(nameof(TestDatasets.All), MemberType = typeof(TestDatasets))]
    public void Encode_IsDeterministic(string dataset)
    {
        var values = TestDatasets.Get(dataset);
        var encoder = CreateEncoder();

        Assert.Equal(encoder.Encode(values), encoder.Encode(values));
    }

    [Theory]
    [MemberData(nameof(TestDatasets.All), MemberType = typeof(TestDatasets))]
    public void ReadBlocks_ShiftsPositionsByStartPosition(string dataset)
    {
        var values = TestDatasets.Get(dataset);
        var encoder = CreateEncoder();

        var rows = Flatten(encoder.ReadBlocks(encoder.Encode(values), 1000));

        Assert.Equal(values.Length, rows.Length);
        for (var i = 0; i < values.Length; i++)
        {
            Assert.Equal((1000 + i, values[i]), rows[i]);
        }
    }

    [Theory]
    [MemberData(nameof(TestDatasets.All), MemberType = typeof(TestDatasets))]
    public void EagerAndDirectAggregates_Agree(string dataset)
    {
        var values = TestDatasets.Get(dataset);
        var encoder = CreateEncoder();
        var blocks = encoder.ReadBlocks(encoder.Encode(values), 0);

        var expectedSum = values.Sum(v => (long)v);
        Assert.Equal(expectedSum, Aggregates.SumEager(blocks));
        Assert.Equal(expectedSum, Aggregates.SumDirect(blocks));
        Assert.Equal(values.Length, Aggregates.CountEager(blocks));
        Assert.Equal(values.Length, Aggregates.CountDirect(blocks));
    }

    [Fact]
    public void ReadBlocks_RejectsEmptyBuffer()
    {
        Assert.Throws<InvalidDataException>(() => CreateEncoder().ReadBlocks(Array.Empty<byte>(), 0));
    }

    [Theory]
    [InlineData("alternating")]
    [InlineData("sorted-runs")]
    public void ReadBlocks_RejectsTruncatedData(string dataset)
    {
        var encoder = CreateEncoder();
        var bytes = encoder.Encode(TestDatasets.Get(dataset));

        Assert.Throws<InvalidDataException>(() => encoder.ReadBlocks(bytes.AsMemory(0, bytes.Length - 1), 0));
    }
}
