using ColumnStore.Core.Compression;

namespace ColumnStore.Core.Storage;

/// <summary>Writes one column to one file.</summary>
public interface IColumnWriter
{
    /// <summary>Splits the values into chunks of chunkSize rows, encodes each chunk, and writes them to path.</summary>
    void Write(string path, ReadOnlySpan<int> values, IColumnEncoder encoder, int chunkSize);
}
