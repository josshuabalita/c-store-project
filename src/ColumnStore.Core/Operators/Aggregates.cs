using ColumnStore.Core.Compression;

namespace ColumnStore.Core.Operators;

/// <summary>
/// Whole-column aggregates in the two execution modes the project compares:
/// eager (decompress every block, then loop) and direct (use block properties, decompress only if forced).
/// Both must return identical answers.
/// </summary>
public static class Aggregates
{
    public static long CountEager(IEnumerable<ICompressionBlock> blocks)
    {
        long count = 0;
        foreach (var block in blocks)
        {
            count += block.AsArray().Length;
        }

        return count;
    }

    public static long CountDirect(IEnumerable<ICompressionBlock> blocks)
    {
        long count = 0;
        foreach (var block in blocks)
        {
            count += block.Size;
        }

        return count;
    }

    public static long SumEager(IEnumerable<ICompressionBlock> blocks)
    {
        long sum = 0;
        foreach (var block in blocks)
        {
            foreach (var value in block.AsArray())
            {
                sum += value;
            }
        }

        return sum;
    }

    public static long SumDirect(IEnumerable<ICompressionBlock> blocks)
    {
        long sum = 0;
        foreach (var block in blocks)
        {
            if (block.IsOneValue)
            {
                sum += (long)block.Size * block.StartValue;
                continue;
            }

            foreach (var value in block.AsArray())
            {
                sum += value;
            }
        }

        return sum;
    }
}
