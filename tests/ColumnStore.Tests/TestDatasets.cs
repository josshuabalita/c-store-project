namespace ColumnStore.Tests;

/// <summary>Named columns that every encoder is tested against. Fixed seeds keep failures reproducible.</summary>
public static class TestDatasets
{
    private static readonly string[] _names =
    [
        "empty", "single", "all-same", "alternating", "sorted-runs", "descending-runs",
        "random-low-cardinality", "random-high-cardinality", "extremes", "one-long-run",
    ];

    public static IEnumerable<object[]> All() => _names.Select(n => new object[] { n });

    public static int[] Get(string name)
    {
        var rng = new Random(12345);
        return name switch
        {
            "empty" => [],
            "single" => [42],
            "all-same" => Enumerable.Repeat(7, 1000).ToArray(),
            "alternating" => Enumerable.Range(0, 500).Select(i => i % 2).ToArray(),
            "sorted-runs" => Enumerable.Range(0, 5000).Select(i => i / 50).ToArray(),
            "descending-runs" => Enumerable.Range(0, 5000).Select(i => 100 - (i / 50)).ToArray(),
            "random-low-cardinality" => Enumerable.Range(0, 5000).Select(_ => rng.Next(0, 8)).ToArray(),
            "random-high-cardinality" => Enumerable.Range(0, 5000).Select(_ => rng.Next()).ToArray(),
            "extremes" => [int.MinValue, int.MaxValue, 0, -1, int.MaxValue, int.MaxValue, int.MinValue],
            "one-long-run" => Enumerable.Repeat(3, 100_000).ToArray(),
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown dataset."),
        };
    }
}
