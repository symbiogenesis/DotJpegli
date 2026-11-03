# Performance parity validation

[Documentation](README.md)

Performance parity requires a passing hosted comparison against unchanged Google
Jpegli at `031a0077f5799a6041004267fc12b956c1f52a20`. The presence of the runner
does not establish parity. Results must name the tested revision, hosted run,
machine and measured bounds. The timing method and acceptance bounds were fixed
before the first measurement.

## Current measurement scope

Full performance parity is not established. The retained reference measurement
tests revision `9f350f9d7f1d8ed3d527d7efad08364a1645f420` in
[hosted run 41567970649](https://github.com/symbiogenesis/DotJpegli/actions/runs/41567970649).
All four selected cases pass timing stability, matched JPEG settings and the
fixed 1.10 individual latency bound:

| Sampling | Sequential | Progressive level 2 |
| --- | ---: | ---: |
| 4:4:4 | 1.054× | 0.926× |
| 4:2:0 | 1.022× | 0.832× |

Ratios are managed/native median latency; lower is better. The comparison uses
Kodak01 RGB8 to YCbCr at distance 1, adaptive quantization, optimized Huffman
coding and one worker, on an AMD EPYC 7763 x64 host with
.NET `11.0.0-rc.3.26522.148` and tiered compilation disabled. Progressive
latency is 7–17% lower; sequential latency is 2–5% higher. The
[reference evidence](performance-evidence/2026-11-02-9f350f9/README.md) retains
raw samples, host identity, settings, hashes and native build provenance.
These selected cases do not establish the 256-case acceptance protocol or
predict performance for other images, runtimes, architectures or options.
The [older reference](performance-evidence/2026-10-15-5604d60/README.md) used a
different CPU and runtime; its 15–24% reduction cannot be applied to this host.

## Targeted development comparisons

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
