# Targeted performance development and full acceptance evidence

A normal push to `main` runs four targeted cases: `kodim01`, distance 1,
444/420 and progression 0/2. It does not first run the 1,280-pair quality matrix
or the 256-case performance matrix. The native executable and Kodak inputs are
cached by pinned source/wrapper/toolchain and corpus hashes; native identity,
provenance and input hashes remain checked when reused. The selected-case table
appears in the CI job summary and the `performance-targeted-<revision>` artifact.

If the selected comparison fails, CI also captures one CPU profile using pinned
`dotnet-trace 10.0.750501` on the selected case with the highest managed/native
ratio, using three blocks, 20 iterations and five warmups. This follow-up has a
five-minute limit and retains its trace, Speedscope JSON, logs and marked instrumented evidence in the
separate `performance-profile-<revision>` artifact. Its instrumented timings do
not establish latency acceptance and do not replace the original unprofiled
result. Set the workflow-dispatch `profile` input to `false` to skip this capture.

To focus a later run, use workflow dispatch with `mode=targeted`. For example:

```sh
gh workflow run ci.yml --ref main -f mode=targeted \
  -f images=kodim03 -f distances=4 -f samplings=420 -f progressions=2 \
  -f blocks=3 -f iterations=3 -f warmups=5
```

Inspect existing runs for the intended commit first and reuse its automatic run
when it covers the desired comparison. Do not dispatch a duplicate for the same
revision unless an additional measurement was requested. A deliberately smaller
selection uses three blocks of three encodes, plus five warmups per encoder per
block. The default targeted run retains the full protocol's seven blocks, five
warmups and calibrated iteration count. All timed benchmarks remain CI-only.

The equivalent runner options are `--diagnostic --images kodim03 --distances 4
--samplings 420 --progressions 2 --blocks 3 --iterations 3 --warmups 5`, in
addition to the required absolute `--native`, `--corpus` and `--output` paths.
Images may be `kodim01` through `kodim24` or one of the eight fixed control IDs.
The four selection flags accept unique comma-separated lists; their Cartesian
product is limited to 64 cases. Distances select 1/4, samplings 444/420 and
progressions 0/2. Blocks are bounded to 3..7, warmups to 5..10 and optional fixed
iterations to 3..100. The pinned native wrapper requires at least five warmups.
Omit `--iterations` to retain native calibration.

Diagnostics measure every selected case and return 1 for a stable matched case
above the 1.10 managed/native latency bound, 2 for invalid or unstable evidence,
and 0 when the selected cases are valid and within that individual bound. They
always record `parityVerdict: not-evaluated` and `passed: null`: selected-case
success never establishes full parity or the full protocol's group bounds.
Count/selection overrides require `--diagnostic`; they cannot change acceptance.

An automatic push whose final commit message includes `[compare-baseline]` also
builds revision `2ef3d5fcc12a804c7cd707446091eb74fa9c6476` in a temporary lane.
This selects `kodim01`, `low-noise`, `low-contrast` and `tiny-odd-edges`, both
distances, both sampling modes and both progressions: 32 cases. Other ordinary
pushes retain the four-case default. The same performance job measures the
baseline with identical selections after the current comparison, serially on
the same CPU, runtime, corpus and native executable.
The `performance-baseline-<revision>` artifact retains both the baseline evidence
and a report of current/baseline managed and native latency ratios plus mean
allocated bytes per encode. Both sets of measurements must be valid and JPEG
hashes must match. Allocated bytes exclude retained pool capacity and are not a
peak-memory measurement. Code revisions run serially rather than interleaved, so the retained
native ratios expose possible drift; these diagnostics do not change the
current revision's acceptance bounds or establish full parity. This uses the
single planned automatic run and requires no additional workflow dispatch.

To compare retained evidence without running encodes, use PowerShell 7:

```powershell
./tools/compare-performance-revisions.ps1 -Baseline /absolute/baseline-evidence -Current /absolute/current-evidence -BaselineRevision <full-baseline-sha> -CurrentRevision <full-current-sha> -Output /absolute/comparison
```

`-Output` is a prefix for the Markdown and JSON reports; `-Summary` optionally
appends the Markdown to a CI summary file. `-SelfTest` checks synthetic evidence
and mismatch rejection locally.

The same job repeats these selections serially with `DOTNET_EnableAVX512=0`
for both managed revisions. This exercises narrow zigzag ordering even when the
runner supports AVX-512. Separate `baseline-avx2-comparison` reports and raw
`avx2-current-evidence` / `avx2-baseline-evidence` directories record this
diagnostic; it does not replace the normal hardware-dispatch acceptance result.

## Interleaved kernel selection diagnostics

A main push ending in `[compare-kernels]` compares the original and optimized
fuzzy-erosion and narrow-zigzag kernels in the same automatic workflow, on x64
and ARM64 with both .NET targets. It holds the other kernel Automatic and checks
all three preferences for identical JPEG bytes. Four-lane ARM64 skips unreachable
narrow-zigzag comparisons explicitly. These runs do not compare against native
Jpegli or establish full parity.

The CI-only command is `--compare-kernels`, with absolute `--corpus` and
`--output` paths and the existing image/distance/sampling/progression selection
arguments. The mode uses six balanced ABBA/BAAB quartets, five warmups per leg
and 20 encodes per leg by default; images below 1,024 pixels use 100 encodes.
The same count applies to both variants. Raw samples and actual counts are kept.

`--include-automatic` adds a pair comparing both Original preferences with both
Automatic preferences. The automatic workflow enables it to measure the actual
deployment policy. Exact preference maps identify each pair, and schema 2 uses
neutral baseline/candidate statistics and hash names. The default 32 selected
configurations therefore produce 96 comparisons on eight-lane x64, or 64 on
ARM64 where the 32 narrow-ordering pairs are explicitly skipped. A policy pair
is measured even if it selects no new kernel; it must not be described as a
forced optimized-kernel result.

Use `--self-test-kernel-comparison` locally to check synthetic statistics without
timing any encoding. The actual mode requires Release and GitHub Actions.
Guards retain the 1 ms minimum and 10% MAD limit, and additionally reject
within-quartet drift, minority timing states and order bias. Both orders must
be balanced. Evidence preferences remain inconclusive unless every paired ratio
clears a 0.5% margin in the same direction and all guards pass. Kernel selection
rules remain conservative when evidence is small, inconsistent or unavailable.

The automatic run retains `kernel-comparison-<architecture>-<framework>-<sha>`
artifacts with the machine/runtime/vector identities, source and JPEG hashes,
raw quartet legs, allocation/GC diagnostics, skipped cases and the report.

## Full acceptance protocol

Include `[full-parity]` in the final commit message of a push to `main` when
full quality/compression and performance evidence is needed. Its automatic run
replaces the targeted comparison (and optional baseline comparison) with the
complete quality matrix followed by the fixed performance acceptance protocol.
Both reports appear in the job summary and their existing evidence artifacts.
The thresholds and per-case timing method remain unchanged.

After a full latency failure, the same job profiles the worst case from each of
the three highest-ratio distinct images with pinned `dotnet-trace`, seven blocks,
100 iterations and five warmups. Installation has a three-minute limit and
collection a five-minute limit. A failing smallest input is also selected if
absent from those three, giving at most four profiles and retaining setup
diagnostics when larger images have higher ratios. The separate `performance-profile-<revision>`
artifact retains the marked instrumented evidence, traces and logs. Profiling
cannot replace the unprofiled verdict; manual full dispatch can disable it with
`profile=false`.

Inspect existing runs for the intended commit and reuse its automatic run.
Workflow dispatch with `mode=full` remains available when no automatic run
covers the intended comparison; reuse queued, running or successful runs and
prefer rerunning an existing failed run. Do not dispatch another workflow for
the same revision unless an additional measurement was requested.

This CI-only tool compares complete in-memory encode latency with the pinned
native jpegli encoder. It uses the shared Kodak24 hash manifest, eight shared
deterministic controls, distances 1/4, sampling 444/420 and progression 0/2:
256 cases. Both encoders use one worker, RGB8 to YCbCr, optimized Huffman tables,
adaptive quantization and baseline-compatible quantizers. Its results apply to
that corpus, configuration and CI host; they do not prove universal speed parity.

The checked-in policy requires each corpus/sampling/progression group to have
geometric mean managed/native median latency ratio <=1.05, and every case <=1.10.
Parallel managed results cannot satisfy this single-worker gate.
Full runs measure all 256 cases even after a stable matched case exceeds an
individual limit, so failed evidence still identifies every optimization target.
Such a counterexample cannot be reversed by later cases. A passing verdict
always requires all 256 cases, valid measurements and both fixed latency bounds.

Seven independent blocks alternate native/managed measurement order. Each block
warms both encoders with five untimed encodes. Tiered JIT compilation is disabled
in the runner runtime configuration. Native three-iteration calibration targets
50ms, clamped to 3..100 encodes; the same count applies to both encoders. Each
encode is timed individually and intervals are summed. The scope includes
encoder construction, configuration, memory output and finalization; it excludes
process startup, files, warmups, byte verification and native output-buffer free.
Managed collections during an encode remain in its measured latency; none are
forced or suppressed. Medians include every block; no outliers are removed.

Any block below 1ms total timed interval, a relative MAD above 10% in either
encoder, missing coverage, nonfinite timings, hash mismatch or unmatched JPEG
structure invalidates the result. Exit 0 means valid passing evidence, 1 means
valid failed latency bounds, and 2 means invalid evidence. Raw blocks, percentiles,
MAD, GC diagnostics, hashes, exact inputs/JPEGs, settings, native command metadata,
revision and CPU/OS/runtime/vector/affinity information are retained in the output.
Run identity and an incomplete summary are written before measurement; progress
is saved after every completed case so interrupted runs retain useful evidence.

The measured run requires a Release build and GitHub Actions:

```sh
dotnet tools/DotJpegli.Performance/bin/Release/net11.0/DotJpegli.Performance.dll \
  --native /absolute/path/jpegli-parity \
  --corpus /absolute/path/kodak-ppm \
  --output /absolute/path/empty-evidence-directory
```

Local validation is deliberately limited to build and synthetic self-test:

```sh
dotnet restore tools/DotJpegli.Performance --locked-mode
dotnet build tools/DotJpegli.Performance -c Release --no-restore
dotnet tools/DotJpegli.Performance/bin/Release/net11.0/DotJpegli.Performance.dll --self-test
```

The self-test exercises only policy, coverage and statistics using fabricated
samples. It does not encode images or read clocks and is not a benchmark.

Both net10.0 and net11.0 are built. The workflow defaults to net11.0; its
`framework=net10.0` input selects the compatible build for a targeted diagnostic.
The net11.0 build uses portable Zip/Unzip/Concat APIs automatically.
