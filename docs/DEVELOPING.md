# Developer notes

## Core design

- Every column is `int32`: money in cents, dates as days since the epoch, categories as ids.
- A column is stored in chunks. `IColumnEncoder.Encode` turns a chunk into bytes and `ReadBlocks` turns bytes back
  into `ICompressionBlock`s.
- A block reports `IsOneValue`, `IsValueSorted` and `IsPosContiguous`. Operators use them to skip decompression
  (a SUM over a one-value block is `Size * StartValue`) and otherwise call `AsArray` or `Iterate`.
- Each operator has an **eager** mode (decompress, then compute) and a **direct** mode (use block properties).
  Both must return the same answer.

## Adding an encoder

1. Implement `IColumnEncoder` and `ICompressionBlock` in `src/ColumnStore.Core/Compression/`. Follow the paper:
   an RLE run is one block, and a bit-vector's bitstring for one value is one non-contiguous block.
2. Add `public sealed class XyzEncoderContractTests : EncoderContractTests` in `tests/ColumnStore.Tests/` and
   return your encoder from `CreateEncoder`. Ten datasets and eight checks then run automatically: round trip,
   every position covered once, truthful block properties, determinism, start offsets, corrupt input, and
   eager-equals-direct aggregates.
3. Add tests for your encoder's own format and shortcuts. Use a spy block to prove a shortcut avoids decompression
   (see `RleEncoderTests.SumDirect_OnOneValueBlock_NeverDecompresses`).
4. Run `.\scripts\check.ps1`.

## CI gates

Format check, build with warnings as errors, tests, and at least 85% line coverage on `ColumnStore.Core`.
The same steps run locally with `.\scripts\check.ps1` and on GitHub for every push and pull request.
