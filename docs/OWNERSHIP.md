# Who owns which files

**One task, one owner.** Every GitHub issue has exactly one `role-X` label. If you need a change in a file someone else
owns, comment on their issue or open a new one. Do not edit it yourself.

| Role | Owner | Files (under `src/ColumnStore.Core/` unless noted) |
|---|---|---|
| **A. Storage** | Logan | `Storage/` (writer, reader, byte counter), `Operators/Scan.cs` |
| **B. Bit-vector** | Ryan | `Compression/BitVector*.cs`, `Compression/BitstringPositionList.cs`, `Operators/BitVectorFilter.cs` |
| **C. Dictionary** | Cole | `Compression/Dictionary*.cs`, `Operators/GroupBy.cs` |
| **D. Data and benchmarks** | Josshua | `RowStore/`, `Queries/` (the four configurations), `src/ColumnStore.Bench/`, `python/` |

Tests live in `tests/ColumnStore.Tests/` and belong to whoever owns the code they test.

## Shared files (frozen)

These are agreed by everyone. Change one only through its own PR that all four approve.

- `Compression/ICompressionBlock.cs`, `Compression/IColumnEncoder.cs`
- `Operators/IPositionList.cs`, `Operators/ArrayPositionList.cs`
- `Storage/IColumnWriter.cs`, `Storage/IColumnReader.cs`
- `Queries/IQueryConfiguration.cs`
- `tests/ColumnStore.Tests/EncoderContractTests.cs`, `TestDatasets.cs`

Finished and not owned by anyone: `RleEncoder`, `UncompressedEncoder`, `Aggregates`.

## Who depends on whom

- B's filter and A's scan both return `IPositionList`. Use `ArrayPositionList` until B's bitstring version exists.
- D builds the four configurations from A (files), B and C (encoders, filters, GROUP BY). Until those merge, D codes
  against the interfaces and the finished RLE and uncompressed encoders.
