# Targeted performance evidence

Tested revision: `2c78be9060b64e537373d31d13300ee6cb4716b3`.
[Hosted run 38683739549](https://github.com/symbiogenesis/DotJpegli/actions/runs/38683739549).
All four selected comparisons are valid, but exceed the unchanged 1.10 individual
latency bound. Full performance parity was not evaluated. The same run passed
the four hosted test jobs and the four trimming/NativeAOT deployment jobs.

The batch retains DCT intermediates in portable vector locals, batches buffered
Huffman token replay, removes signed-amplitude branches, inlines vector color
conversion and reuses padded luma neighborhoods during adaptive analysis.
Managed JPEG hashes match the preceding retained `b8c8058` four-case evidence.

See the [report](targeted/report.md), [samples](targeted/cases.json),
[summary](targeted/summary.json), [host and settings](targeted/run.json),
[input hashes](targeted/sources.json) and [native provenance](native-provenance.json).
[Artifact provenance](provenance.json) records the hosted artifact identity and
SHA-256 hashes of the retained files.

The host is an AMD EPYC 7763 x64 running Ubuntu 24.04.5 and .NET 11 RC2
(`11.0.0-rc.2.26504.105`), with tiered compilation disabled. The preceding retained
run used an EPYC 9V45, so their elapsed times do not form a same-host before/after
speed comparison. Each ratio below compares matched encoders on this run's CPU.

| Sampling/progression | Managed/native median latency |
| --- | ---: |
| 4:4:4 sequential | 1.462× |
| 4:4:4 progressive level 2 | 1.262× |
| 4:2:0 sequential | 1.293× |
| 4:2:0 progressive level 2 | 1.206× |

The comparison uses Kodak01 RGB8 to YCbCr at distance 1, adaptive quantization,
optimized Huffman coding and one worker. Seven alternating blocks use five
untimed warmups each and native-calibrated shared iteration counts. Separate
instrumented profiles from the workflow do not supply these latency measurements.
