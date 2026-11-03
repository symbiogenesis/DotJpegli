# Performance parity validation

[Documentation](README.md)

Performance parity requires a passing hosted comparison against unchanged Google
Jpegli at `031a0077f5799a6041004267fc12b956c1f52a20`. The presence of the runner
does not establish parity. Results must name the tested revision, hosted run,
machine and measured bounds. The timing method and acceptance bounds were fixed
before the first measurement.

## Released-version regression checks

The latest retained comparison at `0104778`, after the segmented/partial
converted-plane changes, **fails two of six release regression gates**. All
1,536 selected comparisons against published 1.0.0 source pass the timing guards
and produce matching JPEG hashes. Of these, 1,310 consistently favor the current
encoder, 116 are inconclusive and 110 consistently favor 1.0.0: 99 on .NET 10 x64
default (EPYC 9V74, Vector512 true) and 11 on .NET 11 x64 default (EPYC 7763,
Vector512 false). The other four lanes pass. The largest selected median ratio
is 1.146380, or 14.64% more time. The
[complete release snapshot](performance-evidence/2026-11-03-0104778/release/README.md)
retains all selected cases and 130 earlier invalid trials. For each case, the
first valid complete trial is final, including regressions and inconclusive results;
conditioning on timing stability does not characterize all attempts' latency.

## Native comparison reference

The [later complete native run at `0104778`](performance-evidence/2026-11-03-0104778/README.md)
has an invalid timing verdict: one of 256 cases, `kodim13/420/p0/d1`, fails the
managed relative-MAD guard. Its aggregate timings cannot replace the complete
valid `b07eb5d` reference below or establish native performance parity.

The fixed acceptance matrix is complemented by an opt-in workload diagnostic.
Include `[performance-workloads]` alongside `[full-parity]` in the settled main
commit to collect both in its single automatic workflow. The workload jobs run
on x64 and ARM64, each with a ten-minute measurement deadline. They cover actual
large encodes around converted-plane and coefficient-cache boundaries, whole
photograph enlargements to 9.8 megapixels, bounded
cache settings, RGB/XYB/source-ICC conversion, stream and array output, one/two/four
workers, and two/four concurrent calls with separate caller input buffers.
Their fresh-process first encode and subsequent warmed measurements include
separate default-tiering cases with three and 256 fixed warmup batches. These
counts do not prove mature tiered execution. No local benchmark or additional
workflow is needed.

These observations preserve the fixed native corpus, settings and acceptance
bounds. They retain raw per-encode samples, throughput, allocation counts, actual
accepted cache capacities, cold/warm memory snapshots and sampled process RSS.
Process memory includes runtime and caller data; allocation traffic is not peak
memory or retained pool capacity. Results remain observations, including noisy
rows, rather than a regression or service-tail-latency verdict. See the
[workload method](../tools/DotJpegli.Performance/README.md#bounded-workload-diagnostics)
for input generation, output checks, runtime settings and measurement limits.

The [later workload snapshot at `0104778`](performance-evidence/2026-11-03-0104778-workloads/README.md)
retains all 26 scenarios on x64 and ARM64 after the segmented/partial
converted-plane changes. Within that run, converted-plane boundary
above-to-below median ratios are 1.098/1.068 for 4:4:4 and 1.021/1.027 for
4:2:0 (x64/ARM64). Both sides pass the workload dispersion checks. These
observations cover the later policy, but the separate hosts and runs do not
establish a before/after speedup. The snapshot preserves noisy rows, memory
observations and the added 256-warmup tiering scenario; it does not establish
native parity or a release regression verdict.

The complete 256-case comparison of revision
`b07eb5d240c2be1f6b9aebac13d081fb50d740cd` in
[hosted run 41690962313](https://github.com/symbiogenesis/DotJpegli/actions/runs/41690962313)
has valid measurements under the fixed protocol and matched settings, but **fails full performance
parity**. Measurements were collected on November 3, 2026. The same run passes
the complete quality/compression matrix. This reference precedes the later
segmented/partial converted-plane change.

| Corpus | Sampling | Sequential mean / worst | Progressive level 2 mean / worst |
| --- | --- | ---: | ---: |
| Kodak photographs | 4:4:4 | 1.241 / 1.582 | 0.930 / 1.018 |
| Kodak photographs | 4:2:0 | 1.180 / 1.425 | 0.902 / 1.027 |
| Deterministic controls | 4:4:4 | 1.256 / 1.612 | 1.118 / 1.389 |
| Deterministic controls | 4:2:0 | 1.130 / 1.523 | 1.104 / 1.387 |

Each mean is the geometric mean of managed/native median latency ratios across
the group's images and both distances; worst is the largest case ratio.
Lower is better. Photograph groups use 18.0–24.1% more time for sequential
encoding and 7.0–9.8% less for progressive encoding. Both progressive photograph
groups pass the group and individual bounds. The other six groups fail;
the worst individual case is a 1.612267 ratio for sequential 4:4:4
smooth-gradient at distance 1. Group averages cannot override individual failures.

This RGB8 to YCbCr comparison uses distances 1/4, adaptive quantization, optimized
Huffman coding and one worker, on AMD EPYC 7763 x64 with .NET
`11.0.0-rc.3.26522.148` and tiered compilation disabled.
The [complete reference evidence](performance-evidence/2026-11-03-b07eb5d/README.md)
retains every case, failure, raw sample, host identity, setting, hash and native
provenance. These measurements support lower group mean
progressive photograph latency within this scope; full parity remains
unestablished.

## Untimed compact coefficient storage study

The [completed bounded representation study](performance-evidence/2026-11-03-compact-coefficients/README.md)
compares dense storage, bitmap/packed-short tiles and per-band bit-packed tiles,
including dense fallback and actual pooled payload/index capacity. Its 360
real-coefficient comparisons cover 24 AQ configurations from a photograph and
five sparse, noisy and odd-edge controls at distances 1/4 and 444/420. Another
75 adversarial layout comparisons check signed extremes and partial tiles;
30 capacity attempts test shared component budgets around the then-current 64 MiB
dense admission boundary. Full-block, direct-DC and individual coefficient
values are checked exactly, and retained operation counts have a defined scope.

Compact layouts admit additional replicated coefficients for some source sets;
pooling and index overhead prevent uniform savings. These are synthetic capacity
trials, without large-image encoding or speed evidence. The work counters
compare offline prototypes; production dense storage writes DCT output directly
and returns spans. The study left production unchanged and makes no
latency, allocation, peak-memory or native-acceptance claim. Its local untimed
command is described in the [runner guide](../tools/DotJpegli.Performance/README.md).
The later segmented dense cache and partial-row reuse are separate from this
historical compact-layout study and require their own workload measurements.

## Targeted development comparisons

For a version regression investigation, `[compare-release]` requests a separate
complete 256-case comparison against the source actually published as 1.0.0,
`a40868bfdace7d737413085522a88d351aa16782`. Both versions are rebuilt with the
current SDK and interleaved on the same CPU/runtime, on .NET 10/11 and
x64/ARM64, including forced narrow x64 SIMD. The fixed comparison rejects
invalid evidence and consistently slower cases; inconclusive rows remain
explicit. Timing-instability failures may retry the entire case, at most eight
attempts, with every trial retained. The first valid trial is final regardless
of speed; valid regressions and inconclusive results are never retried. Hard
identity, JPEG, structural, nonfinite and too-short failures cannot retry.
Reports expose initial-invalid and retry counts and every prior invalid trial.
This selection conditions on stability and may favor quieter intervals; it
does not characterize the full latency distribution across all attempts.
See the [release comparison protocol](../tools/DotJpegli.Performance/README.md#regression-comparison-against-released-100).
It can accompany `[full-parity]` in one automatic run. Historical README
native-relative charts from different CPUs cannot establish version regressions.

A normal push to `main` runs four cases directly: `kodim01` at distance 1,
444/420 and progression 0/2 on .NET 11. Select `framework=net10.0` in a targeted
dispatch to inspect the compatible .NET 10 build. It does not wait for the full quality/compression
matrix. The default measurement counts remain seven blocks, five warmups and
native-calibrated shared iteration counts. The native executable and corpus are
cached by their source/toolchain and manifest identities; native provenance,
executable identity and input hashes are checked when reused.

Include `[full-parity]` in the final commit message of a push to `main` to run
the complete quality/compression and performance acceptance protocols in that
automatic CI run. This replaces the targeted comparison for that revision,
including its optional baseline comparison. Reuse the automatic run instead of
dispatching another workflow for the same revision.

Include `[compare-baseline]` instead for the current optimization batch's
same-host development comparison against `2ef3d5f`. That automatic run selects
32 cases: `kodim01`, low noise, low contrast and tiny odd edges, both distances,
sampling modes and progressions. After the current managed/native diagnostic,
the same performance job interleaves six balanced ABBA/BAAB quartets per case in
two persistent workers. The worker runner binaries, inputs, policies, iteration
counts, runtime and CPU affinity match; JPEG hashes must agree. MAD, minority
timing states, within-quartet drift and order bias invalidate affected rows,
which remain in the report alongside inconclusive results. A second sequential
selection interleaves both revisions with managed AVX-512 disabled to exercise
narrow coefficient ordering. These are selected-case diagnostics. The
[runner guide](../tools/DotJpegli.Performance/README.md) describes their method
and retained evidence. Ordinary pushes without either marker retain four cases.

Include `[compare-kernels]` to collect interleaved forced-kernel and combined
Automatic-policy comparisons on x64 and ARM64, for both .NET 10 and .NET 11, in
the same automatic run. The selected photographs and controls include matching
255-by-128, 256-by-128 and 257-by-128 textures around the 32,768-pixel guard, plus
an 8-by-4,096 tall texture. A second sequential selection in the same job adds
256-by-256 and 512-by-256 intermediate textures on the same CPU. Separate evidence
directories retain both verdicts while preserving the 64-case limit per command.
These six inputs are exclusive to kernel diagnostics; the native acceptance
corpus and bounds remain fixed. The comparison retains
JPEG hashes, allocations, balanced ABBA/BAAB quartets, drift and order-bias
checks, including invalid and inconclusive results. The
[retained assessment of run 41592528894](performance-evidence/2026-11-03-kernel-guard-692a052/README.md)
covers both selections on all four lanes. All 112 valid reachable Automatic
rows on x64 favor the deployed policy by median; 83 also meet the stronger
all-quartets preference rule. Sixteen reachable rows are invalid and 29 valid
rows remain inconclusive under that rule. Every complete selection has invalid
rows, so none establishes a stable aggregate timing verdict. The decision is
to retain the conservative 32,768-pixel floor and existing fallbacks, without
claiming a crossover or a benefit on every input. ARM64's Automatic rows select
the same kernels and its forced narrow pairs are explicitly skipped.

For a smaller development question, workflow dispatch accepts comma-separated
`images`, `distances`, `samplings` and `progressions`, plus bounded `blocks`,
`warmups` and optional fixed `iterations`. For example, this selects one image
and configuration with three blocks of three encodes:

```sh
gh workflow run ci.yml --ref main -f mode=targeted \
  -f framework=net11.0 -f images=kodim03 -f distances=4 -f samplings=420 -f progressions=2 \
  -f blocks=3 -f iterations=3 -f warmups=5
```

Reuse the automatic run for the intended revision when it already covers the
desired comparison; inspect existing runs before dispatching another. See the
[runner guide](../tools/DotJpegli.Performance/README.md) for selection limits and
the equivalent CLI options. All measured runs remain CI-only.

The job summary and `performance-targeted-<revision>` artifact show per-case
latencies, ratios, noise and retained raw evidence. Exit 1 identifies a stable
matched selected case above the fixed 1.10 latency bound; exit 2 identifies
invalid evidence. Exit 0 means only that the selected cases were valid and within
their individual bounds. The diagnostic records `passed: null` and
`parityVerdict: not-evaluated`; it cannot establish the full protocol's group
bounds or a passing performance parity verdict.

When a targeted comparison fails, the same CI job collects one bounded CPU
profile of the selected case with the highest managed/native ratio using pinned
`dotnet-trace 10.0.750501`. Its trace, Speedscope file, logs and marked
instrumented evidence are retained separately as `performance-profile-<revision>`.
These profiling timings cannot establish latency acceptance; the original
unprofiled comparison remains the job's result. Manual dispatch can disable this
follow-up with `profile=false`.

A failed full latency comparison collects separate profiles for the worst case
from each of the three highest-ratio distinct images, using seven blocks and
100 iterations to provide more samples for tiny inputs. If the smallest input
also exceeds the individual limit and is not already selected, its worst case
is profiled too, giving at most four distinct images. Installation is bounded
to three minutes and collection to five minutes. These marked instrumented
diagnostics are retained in `performance-profile-<revision>` and cannot change
the original unprofiled acceptance verdict. `profile=false` also disables them
in a manual full run.

## Optional .NET 11 runtime and portable SIMD API comparison

The workflow's `compare_net11=true` input runs one Kodak01 4:2:0 sequential case
on .NET 10.0.14 and .NET 11.0.0-rc.3.26522.148, serially on the same CPU,
separately on x64 and ARM64 runners. A main
commit containing `[compare-net11]` requests the same diagnostic in its automatic
run. Its first pair uses the identical net10.0 assembly and checks matching JPEG
hashes to isolate runtime/JIT changes. The library, tests and comparison tools
target both net10.0 and net11.0, built with the pinned .NET 11 SDK.
The same optional job also builds two isolated net11.0 variants with SDK
`11.0.100-rc.3.26522.148`: portable `Shuffle`/`Create` lane assembly and portable
`Zip`/`Unzip`/`Concat` equivalents in DCT transposes and 2x2 downsampling. Neither
implementation uses CPU-specific intrinsics. The 4:2:0 case exercises both kernels.
Both variants run on the identical .NET 11 runtime and CPU, with matching JPEG
hashes and focused kernel regression tests. This second pair measures the extra
effect of these API rewrites; it cannot predict gains for other operations or CPUs.
Normal net11.0 builds use these APIs automatically; net10.0 uses compatible
portable lane assembly. `DotJpegliNet11Simd=false` disables these APIs for the
comparison baseline only. The default `true` defines `DOTJPEGLI_NET11_SIMD`.
Evidence is retained separately as `runtime-comparison-X64-<revision>` and
`runtime-comparison-ARM64-<revision>`, including exact runtime product builds.
The reference executable is built natively and cached separately per architecture.

## Full acceptance scope and policy

The comparison covers the 24 hash-pinned Kodak photographs and eight deterministic
controls used by the [quality/compression comparison](quality-compression-parity.md).
Distances are **1 and 4**, sampling is **4:4:4 and 4:2:0**, and progression is
**sequential level 0 and progressive level 2**, giving **256 matched cases**.
Both encoders use RGB8 input, YCbCr output, adaptive quantization, optimized
Huffman coding, baseline-range quantizers and equivalent JFIF metadata.
Smoothing, restart intervals and optional metadata are absent. The primary
comparison uses one managed worker and the single-threaded native encoder.

Acceptance requires complete coverage, matching JPEG frame/sampling/quantization/
scan settings, deterministic outputs, valid timing samples and both bounds below.
The ratio is managed latency divided by native latency; lower is better.

| Bound | Fixed limit |
| --- | ---: |
| Geometric mean of case latency ratios, separately for each corpus category, sampling and progression | 1.05 |
| Every case latency ratio | 1.10 |

These bounds express latency noninferiority within 5% per group and 10% per case.
They are never relaxed after measurements. An aggregate speedup cannot conceal
a slower individual case or a failed group. Each image and distance has equal
weight within its group. A failed or unstable measurement cannot support a parity
claim. Raw timings expose variation so reviewers can assess the evidence.
Full runs measure all 256 cases even after a stable matched case exceeds an
individual bound, retaining the complete set of optimization targets. Such a
case still fails acceptance; faster later cases cannot reverse that failure.
A passing verdict always requires complete coverage.
Every block must contain at least 1 ms of summed timed work. The relative median
absolute deviation must be at most 10% separately for each encoder in every case;
exceeding this fixed variability limit makes the evidence invalid.

## Timing method

Inputs are loaded before timing. Each timed operation creates a fresh encoder,
configures the matched options, encodes all pixels and finishes a complete JPEG
in newly allocated memory. Managed timing covers the public byte-array encode
API, including its output copy and allocations. Native timing covers compressor
creation, configuration, memory destination, scanline submission, finish and
destruction. Native destination freeing and output checks occur outside timing;
managed garbage collection during encoding remains included. No forced collections
are used. These are the normal APIs' allocation and output costs.

Reading input files, process startup, JIT warmup, output hashing, JPEG validation,
file writes and report generation are excluded. The native wrapper uses a
monotonic steady clock; the managed runner uses `Stopwatch`. A calibration encode
chooses a common iteration count targeting a 50 ms native block, bounded to
3–100 encodes. Calibration never contributes to reported samples. Each encoder
receives five warmup encodes before every block. Seven paired blocks alternate
which encoder runs first. Each encode has its own timed interval; the intervals
are summed and divided by iterations to form a block sample. Output comparisons
and native buffer release between iterations remain outside these intervals.
The case ratio compares the two encoders' median block samples, retaining all
seven samples without discarding outliers.

CI pins the comparison process and its native children to the same available CPU.
Encoders run serially, with no concurrent benchmark in this runner. The Linux
host remains shared infrastructure; CPU model, OS, runtime, SIMD width, timer
frequency and relevant environment settings are retained. Tiered compilation is
disabled for this comparison so optimized JIT code does not change tiers during
measurement. This measured runtime configuration must accompany any speed claim.

## Reproduction and retained evidence

All benchmarks run only on CI under [agent instructions](../AGENTS.md). Publishing
`main` starts the targeted comparison described above; reuse its automatic run.
The full acceptance protocol is opt-in through `[full-parity]` in the pushed
final commit message, or workflow dispatch with `mode=full` when no automatic
run covers the intended comparison. Inspect existing runs before dispatching;
reuse a queued, running or successful run, and prefer rerunning a failed run.
It follows the complete quality/compression measurement on the same Ubuntu runner,
using the shared pinned native build and prepared corpus, and uploads
`performance-parity-<revision>`, including unsuccessful comparisons. Diagnostic
selection/count overrides cannot alter this fixed acceptance protocol. Both full
reports appear in the CI job summary, including failures and incomplete evidence.

Evidence includes a versioned summary, group verdicts and failures, raw repeated
timings, per-case output and input hashes, native build provenance, runtime and
machine details and a readable report. Run identity is written before measurement,
and each completed case is saved with progress; an interrupted run retains an
explicit incomplete summary and cannot pass. Completed full runs retain every
case, group bounds and individual failures; a positive result requires the full matrix.
For the full acceptance protocol, exit 0 means every bound passed; exit 1
means valid measurements exceeded the latency bounds; exit 2 means invalid
input, settings, coverage or timing evidence, or a runner error.
Retain the report and machine-readable evidence durably when recording a
hosted result.

Local builds and the focused synthetic-statistics check are allowed:

```text
dotnet build tools/DotJpegli.Performance/DotJpegli.Performance.csproj -c Release
dotnet run --project tools/DotJpegli.Performance -c Release -f net11.0 --no-build -- --self-test
```

This scope measures single-image steady-state encoding latency. It excludes
cold startup, decoding, parallel scaling, batch throughput, allocation/memory
parity, other architectures and untested encoder options. A successful result
would establish parity only for the measured corpus, settings, host and bounds.
