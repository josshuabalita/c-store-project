# Open decisions

Settle these by **Nov 19** and record the answer under each one, because code and benchmarks depend on them.

## 1. Sort order of the data
One file can have one sort order, and RLE needs runs. Q4 wants pickup date sorted; Q1, Q2 and Q3 want vendor and
payment type sorted.
- Option 1: one projection sorted by (vendor, payment_type, pickup_date).
- Option 2: two copies of the data, one per sort order.
- Whatever we pick, **all four configurations use the same order**, or the comparison is unfair.

Decision: _not yet made_

## 2. Dictionary packing
The paper packs several codes into 1 to 4 whole bytes (byte-aligned). The proposal says "as few bits as possible".
Byte-aligned decodes faster; bit-packed is smaller.

Decision: _not yet made_ (note the choice in the report either way)

## 3. Cold cache on Windows
Windows has no simple command to drop the OS file cache. Options: open files with `FileOptions.NoBuffering`, use
RAMMap to clear the standby list, or reboot between runs.

Decision: _not yet made_

## 4. Chunk size
Rows per chunk in a column file. Smaller chunks mean more headers; larger chunks mean more memory per read.
Starting suggestion: 65,536 rows.

Decision: _not yet made_
