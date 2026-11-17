using ColumnStore.Core.Compression;

namespace ColumnStore.Core.Storage;

/// <summary>Reads one column file, one block at a time.</summary>
public interface IColumnReader : IDisposable
{
    int RowCount { get; }

    /// <summary>Bytes read from disk since the reader was opened or ResetBytesRead was called.</summary>
    long BytesRead { get; }

    void ResetBytesRead();

    /// <summary>All blocks in position order. Reads chunks lazily.</summary>
    IEnumerable<ICompressionBlock> ReadBlocks();
}
