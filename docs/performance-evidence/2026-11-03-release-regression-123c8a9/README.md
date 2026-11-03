# Released 1.0.0 source comparison

[Automatic CI run 41625779278](https://github.com/symbiogenesis/DotJpegli/actions/runs/41625779278)
compares candidate `123c8a963364484cca1b565f623aafc89fe7cbe0` with
`a40868bfdace7d737413085522a88d351aa16782`, the source revision verified and
published as NuGet 1.0.0 by
[publication run 38698571455](https://github.com/symbiogenesis/DotJpegli/actions/runs/38698571455).
Measurements were collected on November 3, 2026 (America/Chicago).

All six lanes completed all 256 configurations. All **1,536 paired JPEG hashes
match**. Every lane has invalid timing rows, so every complete release comparison
verdict is **invalid**. Among the individually valid rows, **120 lane/case pairs
strongly favor 1.0.0**, identifying remaining regressions. This run does not
establish that the candidate is free of regressions.

| Lane | CPU | Float lanes / Vector512 | Valid / 256 | Invalid | Strong regressions | Valid inconclusive |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| [ARM64 .NET 10](ARM64-net10.0-default/report.md) | ARM implementer 0x41, part 0xd49 | 4 / false | 254 | 2 | 0 | 48 |
| [ARM64 .NET 11](ARM64-net11.0-default/report.md) | ARM implementer 0x41, part 0xd49 | 4 / false | 252 | 4 | 0 | 24 |
| [x64 .NET 10, AVX2](X64-net10.0-avx2/report.md) | AMD EPYC 9V74 | 8 / false | 212 | 44 | 44 | 50 |
| [x64 .NET 10, default](X64-net10.0-default/report.md) | AMD EPYC 9V45 | 8 / true | 156 | 100 | 55 | 26 |
| [x64 .NET 11, AVX2](X64-net11.0-avx2/report.md) | AMD EPYC 9V45 | 8 / false | 175 | 81 | 13 | 39 |
| [x64 .NET 11, default](X64-net11.0-default/report.md) | Intel Xeon 6973P-C | 8 / true | 161 | 95 | 8 | 19 |

Vector512 describes the actual runtime capability, independently of the width
of `Vector<float>`. Both AVX2 lanes have accelerated Vector256 and disabled
Vector512; both default x64 lanes retain Vector512. Runtime, machine, affinity,
GC and executable identities are retained in each lane's `run.json`.

## Individually valid regressions

A strong regression requires every one of the six paired quartet ratios to
exceed 1.005, with all existing timing and identity guards passing. No invalid
row establishes a regression or an improvement. An inconclusive preference does
not establish equivalent speed or absence of a smaller regression.

| Lane | Largest strong current / baseline ratio | Case |
| --- | ---: | --- |
| x64 .NET 10, AVX2 | 1.069047 | Kodak02, 420, sequential, distance 1 |
| x64 .NET 10, default | 1.091065 | Kodak22, 420, sequential, distance 1 |
| x64 .NET 11, AVX2 | 1.038622 | Kodak22, 420, sequential, distance 1 |
| x64 .NET 11, default | 1.039865 | Kodak04, 420, progressive level 2, distance 1 |

Most strong x64 regressions are sequential photograph encodes. All eight strong
regressions in the default .NET 11 lane are progressive 420 photographs.
Neither ARM64 lane has a strong regression; incomplete timing validity prevents
a complete no-regression conclusion there. Allocation reductions also occur in
slower cases and do not establish latency improvements.

[Every strong regression](regressions.json) retains its label, paired statistics,
allocation measurements, matching output hashes and all raw quartet legs. Each
lane's `cases.json` additionally retains every valid, invalid and inconclusive
row. [The detailed breakdown](analysis.md) groups all cases by lane, sampling,
progression and distance. [Machine-readable analysis](analysis.json) includes
every case median, per-guard counts and repeated-leg drift diagnostics. Its
all-row aggregates include invalid cases and are descriptive only; valid-only
aggregates describe incomplete subsets. Neither is a passing aggregate verdict.

## Timing invalidity

All 326 invalid rows remain in the evidence. Repeated-leg drift above 10%
dominates; some rows also fail percentile drift or order bias. No row fails the
relative-MAD, minimum-interval or JPEG-hash guards. These categories overlap.

| Lane | Repeated-leg drift pairs | Slower leg has a GC | Slower leg has a Gen2 GC | Neither leg has a GC |
| --- | ---: | ---: | ---: | ---: |
| ARM64 .NET 10 | 2 | 1 | 0 | 1 |
| ARM64 .NET 11 | 4 | 2 | 0 | 1 |
| x64 .NET 10, AVX2 | 52 | 10 | 1 | 36 |
| x64 .NET 10, default | 160 | 37 | 8 | 94 |
| x64 .NET 11, AVX2 | 105 | 40 | 10 | 49 |
| x64 .NET 11, default | 149 | 28 | 20 | 113 |

A pair compares the two measurements of one revision within a quartet. GC
counts cover the timed encode loop. They cannot identify the cause of a delay,
and GC does not explain most x64 drift pairs: many have no collection in either
leg. Drift affects both revisions, earlier and later repeated legs, and all six
quartet blocks. For example, default .NET 10 has 71 baseline and 89 current
drift pairs, with 79 earlier-leg and 81 later-leg slowdowns; its per-block counts
are 26, 28, 32, 21, 27 and 26. This is consistent with shared-host timing
variability, but scheduler, frequency and host-load diagnostics were not
collected, so the precise cause is unestablished. The samples do not support
attributing all invalidity to initial warmup, or discarding particular legs.

## Scope and provenance

Each lane compares the two revisions on the same host and CPU affinity using
identical persistent worker binaries. Only one worker receives encode work at
a time. The fixed corpus is Kodak24 plus eight deterministic controls, distances
1/4, sampling 444/420 and progression 0/2. Six balanced ABBA/BAAB quartets use
five warmups per leg and 20 timed encodes per leg, or 100 for the tiny input.
Tiered compilation is disabled. Natural GC remains included. No timing guards,
sample counts or acceptance bounds were relaxed.

The baseline is rebuilt from released sources using the same current SDK as
the candidate. Only `global.json` is copied into its source tree. SDK-owned
ILLink dependencies are reevaluated; original and rebuilt lock files are kept.
The baseline's assembly binding version is set to 2.0.0.0 so the identical
worker can load it, while its informational version retains `1.0.0` and the
released source SHA. Baseline static analyzers are disabled for historical
source compatibility. These comparisons isolate encoder-source behavior on
each lane's current runtime; they do not measure the original published binary
on its original runtime.

[Provenance](provenance.json) records all six artifact IDs and exact SHA256 hashes
for retained files. Each lane retains its complete cases, summary, report, run
identity, source hashes, build provenance and dependency locks. Original inputs
and incremental raw snapshots remain in the hosted artifacts. No benchmarks ran
locally; the additional analysis only reads retained CI samples.

The earlier `ac49d11` automatic run failed before measurement because its
baseline assembly-version query returned an empty value. The corrected revision
queries the MSBuild version target and validates the result. That setup failure
contains no performance evidence and is not merged into this comparison.
