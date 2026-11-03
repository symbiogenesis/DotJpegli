# Complete performance matrix

Revision `ba9693f455b82c2ffd0326b3125a6892373ee64a` was measured in
[hosted run 41569147611](https://github.com/symbiogenesis/DotJpegli/actions/runs/41569147611).
All 256 cases have valid, stable, matched measurements. Full performance parity
**failed** the unchanged latency limits. The same run passed the complete
1,280-case quality/compression comparison and all tests and deployment checks.

Progressive 4:2:0 encoding was faster in all 48 Kodak photograph cases, with a
geometric mean managed/native median latency ratio of 0.834666 (16.5% lower
latency). Other groups include slower cases; tiny inputs reach 2.615 times
native latency. This supports a scoped progressive-photograph claim, not full
parity or universal superiority.

The host is AMD EPYC 9V74 x64, .NET `11.0.0-rc.3.26522.148`, one worker, tiered
compilation disabled. Both encoders use RGB8 to YCbCr, distances 1/4, sampling
444/420 and progression 0/2, with matched adaptive quantization and optimized
Huffman coding.

- [Report](full/report.md) and [summary](full/summary.json), including every failed bound.
- [Raw samples](full/cases.json), [run identity](full/run.json) and [input hashes](full/sources.json).
- [Native provenance](native-provenance.json) and [artifact identity and retained-file hashes](provenance.json).

Exact inputs, JPEGs and native command logs remain in the hosted artifact; paths
in raw JSON refer to its layout. The snapshot date uses America/Chicago; the UTC
measurement date is November 3.
