using ColumnStore.Core.Compression;

namespace ColumnStore.Core.Operators;

/// <summary>Sum and count in two modes: eager (decompress first) and direct (use block properties). Same answer.</summary>
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
