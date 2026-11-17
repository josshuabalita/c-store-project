using ColumnStore.Core.Compression;

// Smoke run: encode a column and print sizes. The real benchmark driver replaces this.
var runLength = args.Length > 0 && int.TryParse(args[0], out var parsed) && parsed > 0 ? parsed : 100;
const int Rows = 1_000_000;

var values = new int[Rows];
for (var i = 0; i < Rows; i++)
{
    values[i] = i / runLength;
}

IColumnEncoder[] encoders = [new UncompressedEncoder(), new RleEncoder()];
Console.WriteLine($"rows={Rows:N0} avgRunLength={runLength}");
foreach (var encoder in encoders)
{
    var bytes = encoder.Encode(values);
    Console.WriteLine($"{encoder.Kind,-13} {bytes.Length,12:N0} bytes");
}
