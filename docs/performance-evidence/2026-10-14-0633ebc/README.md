# Targeted performance evidence

Tested revision: `0633ebc68a4334ef300c93a61accc849e731694e`.
[Hosted run 38681199007](https://github.com/symbiogenesis/DotJpegli/actions/runs/38681199007).
Full correctness and software-fallback jobs passed on Windows and Linux for both
net10.0 and net11.0, including reproducibility checks of both assemblies and the
NuGet package. The selected performance bound failed. Full performance acceptance
was not run and parity is not established.

## Four selected cases on .NET 11

See the [targeted report](targeted/report.md), raw [samples](targeted/cases.json)
and [host/settings metadata](targeted/run.json). Kodak01, distance 1, RGB8 to YCbCr,
AQ, optimized Huffman coding, one worker, seven alternating blocks and five
warmups; common iteration counts are calibrated against native latency.

| Sampling/progression | Managed/native latency |
| --- | ---: |
| 4:4:4 sequential | 1.719 |
| 4:4:4 progressive level 2 | 1.284 |
| 4:2:0 sequential | 1.579 |
| 4:2:0 progressive level 2 | 1.224 |

All four cases exceeded the unchanged 1.10 individual bound. No whole-corpus or
group verdict is inferred from these selected cases.

## Separate runtime and portable API comparisons

Each architecture ran one Kodak01 4:2:0 sequential distance-1 case, seven blocks,
twenty iterations and five warmups. The first pair uses identical net10.0 binaries
on .NET 10.0.13 and .NET 11 RC2. The second pair uses the same .NET 11 SDK/runtime,
comparing portable legacy lane assembly against Zip/Unzip/Concat in DCT and
downsampling. All four managed JPEG hashes match within each host.

| Host | Runtime-only latency change | Extra API latency change |
| --- | ---: | ---: |
| Intel Xeon Platinum 8573C x64 | −0.8% | −0.1%, within timing variation |
| ARM64 implementer 0x41, part 0xd49 | −2.9% | −2.9% |

Reports: [x64](x64/runtime-comparison.md), [ARM64](arm64/runtime-comparison.md).
Each variant retains run metadata, raw cases, summary and source hashes in its
subdirectory. Both implementations use portable APIs. These are one-case results,
not architecture-wide speed guarantees. Native reference medians also vary across
variant runs; the raw samples and per-encoder variability remain available.

The earlier 32.4% x64 API result at `f7f1e10` used AMD hardware and preceded the
legacy transpose improvement. It cannot be generalized to this revision or to
every x64 CPU. The complete uninstrumented output and native build provenance
remain in the linked run's artifacts; instrumented CPU profiles are separate and
do not supply acceptance timings.
