# Targeted performance evidence

Tested revision: `60835e6ad1613627fe852bd7678a5ef165ec3b43`.
[Hosted run 38684730647](https://github.com/symbiogenesis/DotJpegli/actions/runs/38684730647).
All four selected comparisons are valid. The 4:2:0 sequential case meets the
unchanged 1.10 individual latency bound; the other three exceed it.
Full performance parity was not evaluated.

The batch fuses vector HF/gamma adaptive analysis and RGB8/4:2:0 preparation,
keeps reserved entropy-token counts in registers, prepares baseline amplitudes
once, and reuses bounded output staging buffers. Managed JPEG hashes match the
preceding retained four-case runs. The scalar and full-plane paths remain
independent regression references.

See the [report](targeted/report.md), [samples](targeted/cases.json),
[summary](targeted/summary.json), [host and settings](targeted/run.json),
[input hashes](targeted/sources.json) and [native provenance](native-provenance.json).
[Artifact provenance](provenance.json) records the hosted artifact identity and
SHA-256 hashes of the retained files.

The host is an Intel Xeon Platinum 8573C x64 running Ubuntu 24.04.5 and .NET 11 RC2
(`11.0.0-rc.2.26504.105`), with tiered compilation disabled. The preceding retained
run used an AMD EPYC 7763, so their elapsed times do not form a same-host
before/after speed comparison. Each ratio below compares matched encoders on
this run's CPU.

| Sampling/progression | Managed/native median latency |
| --- | ---: |
| 4:4:4 sequential | 1.151× |
| 4:4:4 progressive level 2 | 1.202× |
| 4:2:0 sequential | 1.041× |
| 4:2:0 progressive level 2 | 1.106× |

The comparison uses Kodak01 RGB8 to YCbCr at distance 1, adaptive quantization,
optimized Huffman coding and one worker. Seven alternating blocks use five
untimed warmups each and native-calibrated shared iteration counts. Separate
instrumented profiles from the workflow do not supply these latency measurements.
