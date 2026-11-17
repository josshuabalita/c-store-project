# Compression-Aware Column Store

CPSC 4660 · Group 5 · Logan Buye, Ryan Thibault, Cole Mcritchie, Josshua Balita

We build a column store from scratch in C# (.NET 8) that follows Abadi, Madden and Ferreira (SIGMOD 2006), then measure
it on NYC taxi data. The point to prove: **a column store that compresses its data and runs queries directly on the
compressed form beats decompressing first, and beats a row store.**

| Date | What |
|---|---|
| **Nov 27** | All benchmarks finished |
| **Dec 4** | Final report and code due |
| **Dec 8** | Class demo |

## Who does what

> These roles are a proposal. Swap names freely, but every role needs exactly one owner.

| Role | Owner | Builds | Done when |
|---|---|---|---|
| **A. Storage** | Logan | Column file format, chunked column files, bytes-read counter, position lists, scan and filter operator | A column can be written to disk, read back block by block, and the bytes read are counted |
| **B. Bit-vector and filters** | Ryan | `BitVectorEncoder` and its block, equality filter that returns a bitstring, position-list AND (for Q3) | Passes `EncoderContractTests`; `WHERE x = v` on a bit-vector column never touches rows |
| **C. Dictionary and GROUP BY** | Cole | `DictionaryEncoder` and its block, COUNT/SUM with GROUP BY in eager and direct modes | Passes `EncoderContractTests`; Q1 and Q4 answers match plain LINQ in both modes |
| **D. Data, baseline and benchmarks** | Josshua | Python data prep, synthetic generator, row store, benchmark driver, plots | `results/*.csv` for all four configurations, plus the charts |

Already done and available as a template: `RleEncoder`, `UncompressedEncoder`, `Aggregates` (sum and count in both
modes), and the shared test suite. Copy their style.

## What each person hands in

Each role has **code** (merged to `main` with tests), and **material for the final report**. The report needs six
things: summary, implementation details, evaluation strategy, results and analysis, conclusions, and the code.
See [docs/REPORT_GUIDE.md](docs/REPORT_GUIDE.md) for the page budget and the full outline.

### A. Storage (Logan)
- [ ] `ColumnWriter` / `ColumnReader`: one file per column, fixed-size chunks, a small header per chunk (encoding, value count, start position)
- [ ] Bytes-read counter around the file stream, used by every query
- [ ] Position-list type, and a scan/filter operator that works on any encoding
- [ ] Tests: write then read gives the same column; counter matches file size on a full scan; corrupt header is rejected
- [ ] **Report input:** the file layout diagram, why chunks, how bytes read is measured, how the cold-cache run is done on Windows (see [docs/DECISIONS.md](docs/DECISIONS.md))
- [ ] **Report section owned:** 2. Implementation details (storage part)

### B. Bit-vector and filters (Ryan)
- [ ] `BitVectorEncoder`: one bitstring per distinct value, one non-contiguous block per value (as in the paper)
- [ ] Equality filter that returns that value's bitstring without decompressing; `BAnd` for combining filters
- [ ] Tests: add `BitVectorEncoderContractTests`; shortcut test that proves the filter never calls `AsArray`
- [ ] **Report input:** a worked example of the encoding, when it wins and loses (the paper: great for WHERE, slow when the column must be read in position order), your measured numbers for Q2 and Q3
- [ ] **Report section owned:** 2. Implementation details (bit-vector and filters), part of 4. Results (Q2, Q3)

### C. Dictionary and GROUP BY (Cole)
- [ ] `DictionaryEncoder`: pick bits per code from the distinct count, bit-pack codes (decide packing rule in [docs/DECISIONS.md](docs/DECISIONS.md))
- [ ] COUNT and SUM with GROUP BY, eager mode and direct mode, hash table or array keyed by group value
- [ ] Tests: add `DictionaryEncoderContractTests`; GROUP BY results equal a LINQ `GroupBy` on all ten datasets
- [ ] **Report input:** the dictionary layout and packing rule, how the direct GROUP BY avoids decompression, measured results for Q1 and Q4
- [ ] **Report section owned:** 2. Implementation details (dictionary and operators), part of 4. Results (Q1, Q4)

### D. Data, baseline and benchmarks (Josshua)
- [ ] `python/prep_taxi.py`: read 10 to 12 months of Parquet, drop invalid rows, money to integer cents, pickup datetime to day number, write CSV
- [ ] `python/gen_synthetic.py`: integer columns with chosen distinct count (2 to 100,000) and average run length (1 to 1,000)
- [ ] Row store (fixed-width records, sequential scan) and the benchmark driver: 4 configurations x Q1 to Q4, median of 5 runs, cold and warm, CSV out
- [ ] Equivalence test: all four configurations return identical answers for every query
- [ ] `python/plot.py` (matplotlib) for the report figures
- [ ] Add a Python job to CI once `python/` has tests
- [ ] **Report input:** dataset description (rows, columns, cleaning rules), the evaluation setup and metrics, all result figures
- [ ] **Report sections owned:** 3. Evaluation strategy, 4. Results (figures and sensitivity tests), 5. Conclusions (with everyone)

### Everyone
- [ ] Summary (report section 1) in your own words, from your reading of the paper
- [ ] Review at least one teammate's pull request each week
- [ ] A short README note for anything you built that others must run

## Quick start

```powershell
dotnet build
dotnet test
dotnet run --project src/ColumnStore.Bench -- 100    # smoke run, argument = average run length
.\scripts\check.ps1                                   # every CI gate, run before each push
```

Needs the .NET 8 SDK or newer.

## Working together

1. `git switch -c yourname/short-topic`, make small commits.
2. Run `.\scripts\check.ps1`. If the format step fails, run `dotnet format` and commit.
3. Push and open a pull request. CI must be green and one teammate must approve before you merge.
4. Never commit data, CSVs or results. They are in `.gitignore` on purpose.

More: [docs/DEVELOPING.md](docs/DEVELOPING.md) (how to add an encoder), [docs/DECISIONS.md](docs/DECISIONS.md)
(open choices to settle this week), [docs/REPORT_GUIDE.md](docs/REPORT_GUIDE.md) (report outline).

## Layout

| Path | Purpose |
|---|---|
| `src/ColumnStore.Core` | Encoders, compression blocks, operators, storage |
| `src/ColumnStore.Bench` | Benchmark driver (smoke run for now) |
| `tests/ColumnStore.Tests` | Unit tests and the shared encoder contract |
| `python/` | Data prep and plotting (to be added by role D) |
| `docs/` | Developer notes, decisions, report guide |
| `scripts/check.ps1` | Runs every CI gate locally |
