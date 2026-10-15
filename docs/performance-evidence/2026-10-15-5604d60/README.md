# Targeted performance evidence

Tested revision: `5604d601ceda2b9600aeff153a1ee65a4e9774cf`.
[Hosted run 38688881338](https://github.com/symbiogenesis/DotJpegli/actions/runs/38688881338).
All four selected comparisons are valid and meet the unchanged 1.10 individual
latency bound. All correctness and deployment jobs passed. Full performance
parity was not evaluated.

The [targeted report](targeted/report.md) retains the managed/native result;
[raw samples](targeted/cases.json), [summary](targeted/summary.json),
[host and settings](targeted/run.json), [input hashes](targeted/sources.json)
and [native provenance](native-provenance.json) support it.
The host is an AMD EPYC 9V74 x64 running .NET 11 RC2
(`11.0.0-rc.2.26504.105`), with tiered compilation disabled.

| Sampling/progression | Managed/native median latency |
| --- | ---: |
| 4:4:4 sequential | 0.848× |
| 4:4:4 progressive level 2 | 0.838× |
| 4:2:0 sequential | 0.811× |
| 4:2:0 progressive level 2 | 0.757× |

The batch combines exact eight-block AQ modulation, fused eight-lane plane DCT
and quantization, direct SIMD ties-to-even rounding, and small RGB conversion,
zigzag, nonzero-mask, inlining and bounded-span improvements. All paths remain
safe, managed and portable; unsupported vector widths retain their fallbacks.
Managed JPEG hashes match baseline `60835e6` in all four measured cases.

The same job then built and measured that baseline using the same native
executable, corpus, pinned CPU, runtime and fixed evidence policy. Three cases
measured 29–33% lower managed latency, with native times changing by less than 1%.
For 4:4:4 sequential, managed latency fell 55%, but native latency also fell 19%:
that case cannot isolate the encoder change from host variation.
See the [serial comparison](baseline-comparison.md),
[machine-readable comparison](baseline-comparison.json),
[baseline samples](baseline/cases.json) and [baseline host/settings](baseline/run.json).
The source revisions run serially, rather than interleaved. This diagnostic does
not replace the current revision's managed/native bounds.

The comparison uses Kodak01 RGB8 to YCbCr at distance 1, adaptive quantization,
optimized Huffman coding and one worker. Seven alternating blocks use five
untimed warmups each and native-calibrated shared iteration counts.
[Artifact provenance](provenance.json) records the hosted artifact identities and
SHA-256 hashes of the retained files. These four cases do not establish
whole-corpus performance parity.
