# Compression-Aware Column Store

CPSC 4660 · Group 5 · Logan Buye, Ryan Thibault, Cole Mcritchie, Josshua Balita

A column store written from scratch in C# (.NET 8), based on Abadi, Madden and Ferreira (SIGMOD 2006). We test it on
NYC taxi data and check one claim: **queries run faster directly on compressed data than after decompressing it, and
faster than a row store.**

## Dates

| Date | What |
|---|---|
| Nov 19 | Open decisions settled ([docs/DECISIONS.md](docs/DECISIONS.md)) |
| Nov 23 | Parts built |
| Nov 25 | Q1 to Q4 give the same answers on all four configurations |
| **Nov 27** | Benchmarks finished |
| **Dec 4** | Report and code due |
| Dec 8 | Demo |

## Who does what

Each task is a GitHub issue with one owner. Find yours under [Issues](../../issues) by its `role` label.
The names are a proposal, so swap them if you want.

| Role | Owner | Builds | Report section |
|---|---|---|---|
| **A. Storage** | Logan | Column files on disk, bytes-read counter, scan and filter | Implementation (storage) |
| **B. Bit-vector** | Ryan | Bit-vector encoding, equality filter, combining filters | Implementation (bit-vector), results for Q2 and Q3 |
| **C. Dictionary** | Cole | Dictionary encoding, COUNT/SUM with GROUP BY | Implementation (dictionary), results for Q1 and Q4 |
| **D. Data and benchmarks** | Josshua | Data prep, row store, four configurations, benchmark driver, plots | Evaluation, results, conclusions |

Everyone also writes their own **summary** of the paper (report section 1) and reviews teammates' pull requests.

**Already done:** RLE and uncompressed encoders, sum and count, shared tests. Copy their style.

**Which files you may edit:** [docs/OWNERSHIP.md](docs/OWNERSHIP.md). If you need a change in someone else's file,
ask them in their issue.

## How to work

1. Pick your issue and make a branch: `git switch -c yourname/topic`
2. Build it in small commits.
3. Run `.\scripts\check.ps1` before you push. If the format step fails, run `dotnet format`.
4. Open a pull request that says `Closes #issue-number`. Tests must pass and Josshua reviews and merges.
5. Never commit data, CSVs or results.

## Run it

```powershell
dotnet build
dotnet test
dotnet run --project src/ColumnStore.Bench -- 100    # smoke run
```

Needs the .NET 8 SDK or newer.

## More

- [docs/DEVELOPING.md](docs/DEVELOPING.md): how to add an encoder
- [docs/REPORT_GUIDE.md](docs/REPORT_GUIDE.md): what goes in the report
- [docs/DECISIONS.md](docs/DECISIONS.md): choices to settle this week
- [docs/OWNERSHIP.md](docs/OWNERSHIP.md): who owns which files
