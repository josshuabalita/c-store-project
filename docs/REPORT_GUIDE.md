# Final report guide

The report is **4 to 5 pages** and must contain these six items. Check with the instructor whether each student writes
their own report or the group submits one; the outline below works either way, because every section lists the
material each role contributes.

| # | Section | Pages | Who supplies what |
|---|---|---|---|
| 1 | **Summary** of the strategy in your own words | 0.5 | Everyone writes their own. Cover: row vs column store, compression, running queries on compressed data, the block properties |
| 2 | **Implementation details** | 1.25 | A: file format and bytes-read. B: bit-vector and filters. C: dictionary and GROUP BY. D: row store, data prep, benchmark driver. One diagram of the block interface |
| 3 | **Evaluation strategy** | 0.5 | D: four configurations, Q1 to Q4, metrics, dataset, sensitivity tests, cold vs warm cache method |
| 4 | **Results and analysis** | 1.5 | D: figures. B and C: analysis of their queries. Compare with the paper's 3x, 4x and 10x claims |
| 5 | **Conclusions** | 0.5 | Everyone. What held up, what did not, and why |
| 6 | **Code** | n/a | Submit the repository (tag a release before Dec 4). README must say how to build and run |

## Figures to produce (role D, by Nov 27)

1. Storage size per column and per scheme (MB and compression ratio).
2. Bytes read per query, four configurations.
3. Query time per query, four configurations, cold and warm.
4. Speedup of "operate on compressed" over "eager decompression", per scheme.
5. Sensitivity: best scheme as run length (1 to 1,000) and distinct values (2 to 100,000) change. Compare with the
   paper's decision tree.

## Facts you must report

- Exact row count and months loaded, and the cleaning rules.
- Machine: CPU, RAM, disk, OS, .NET version.
- Query time is the median of 5 runs. State how the cold cache was produced.
- All four configurations returned identical answers (the equivalence test).
- Anything that disagrees with the paper. Disagreement with an explanation is a good result.

## Paper numbers to compare against

On sorted runs of 1,000 (Abadi et al., section 6.2), speedup of direct execution over eager decompression:
RLE about 3.3x, bit-vector about 10.3x, dictionary about 3.9x for group-by-self and 1.1x otherwise.
Best scheme by data: runs and low cardinality gives RLE; no runs gives dictionary; bit-vector only below roughly
50 distinct values.
