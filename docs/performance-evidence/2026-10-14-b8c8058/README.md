# Targeted performance evidence

Tested revision: `b8c8058cee59e21c636a7aa0e340c8df6e131c77`.
[Hosted run 38682272963](https://github.com/symbiogenesis/DotJpegli/actions/runs/38682272963).
The uninstrumented four-case diagnostic is valid and complete for its selected
cases. Three cases exceed the unchanged 1.10 individual latency bound; full
performance parity was not evaluated.

See the [report](targeted/report.md), raw [samples](targeted/cases.json),
[summary](targeted/summary.json), [host and settings](targeted/run.json),
[input hashes](targeted/sources.json) and [native build provenance](native-provenance.json).
[Artifact provenance](provenance.json) records the hosted artifact identity and
SHA-256 hashes of the retained files.

The comparison uses Kodak01 RGB8 to YCbCr at distance 1, adaptive quantization,
optimized Huffman coding and one worker. Seven alternating blocks use five
untimed warmups each and native-calibrated shared iteration counts. The host is
an AMD EPYC 9V45 x64 running Ubuntu 24.04.5 and .NET 11 RC2
(`11.0.0-rc.2.26504.105`), with tiered compilation disabled.

| Sampling/progression | Managed/native median latency |
| --- | ---: |
| 4:4:4 sequential | 1.515× |
| 4:4:4 progressive level 2 | 1.178× |
| 4:2:0 sequential | 1.352× |
| 4:2:0 progressive level 2 | 1.060× |

All four cases passed timing stability and JPEG settings matching. Only the
4:2:0 progressive level-2 case meets the selected-case latency bound. No group
or whole-corpus acceptance verdict follows from this diagnostic. Separate
instrumented profiles from the same workflow do not supply these timings.
