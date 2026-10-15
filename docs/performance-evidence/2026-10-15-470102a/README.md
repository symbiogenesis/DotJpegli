# Targeted performance evidence

Tested revision: `470102aa461fe019fc0bda19d48df07dde94be13`.
[Hosted run 38686751109](https://github.com/symbiogenesis/DotJpegli/actions/runs/38686751109).
All four selected comparisons are valid. The 4:2:0 progressive level 2 case meets
the unchanged 1.10 individual latency bound; the other three exceed it.
All correctness and deployment jobs passed. Full performance parity was not evaluated.

The [targeted report](targeted/report.md) retains the managed/native result;
[raw samples](targeted/cases.json), [summary](targeted/summary.json),
[host and settings](targeted/run.json), [input hashes](targeted/sources.json)
and [native provenance](native-provenance.json) support it.
The host is an AMD EPYC 9V74 x64 running .NET 11 RC2
(`11.0.0-rc.2.26504.105`), with tiered compilation disabled.

| Sampling/progression | Managed/native median latency |
| --- | ---: |
| 4:4:4 sequential | 1.515× |
| 4:4:4 progressive level 2 | 1.165× |
| 4:2:0 sequential | 1.358× |
| 4:2:0 progressive level 2 | 1.020× |

The same job then built and measured baseline `60835e6` on the same host, using
the same native executable, corpus, pinned CPU, runtime and fixed evidence policy.
This serial diagnostic measured 13% lower 4:4:4 progressive latency and 20% lower
4:2:0 progressive latency, with 1–2% lower sequential latency. Native latencies
changed by less than 1%. JPEG hashes matched for all four cases.
See the [comparison](baseline-comparison.md), [machine-readable comparison](baseline-comparison.json),
[baseline samples](baseline/cases.json) and [baseline host/settings](baseline/run.json).
The source revisions run serially, rather than interleaved. This diagnostic does
not replace the current revision's managed/native bounds.

The comparison uses Kodak01 RGB8 to YCbCr at distance 1, adaptive quantization,
optimized Huffman coding and one worker. Seven alternating blocks use five
untimed warmups each and native-calibrated shared iteration counts. Separate
instrumented profiles do not supply these latency measurements.
[Artifact provenance](provenance.json) records the hosted artifact identities and
SHA-256 hashes of the retained files. These four cases do not establish
whole-corpus performance parity.
