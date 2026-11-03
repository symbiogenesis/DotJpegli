# Targeted performance development and full acceptance evidence

## Regression comparison against released 1.0.0

Include `[compare-release]` in the final commit message of a push to `main` to
compare all 256 photograph/control configurations against the source published
as NuGet 1.0.0, `a40868bfdace7d737413085522a88d351aa16782` (publication run
[38698571455](https://github.com/symbiogenesis/DotJpegli/actions/runs/38698571455)).
This runs in the same automatic workflow, independently of `[full-parity]`.
Reuse that run; do not dispatch a duplicate.

When previous evidence lacks .NET 11 on accelerated Vector512 hardware, use
`[compare-release-wide]` to select only the x64/.NET 11/default lane in the same
automatic workflow. This marker takes precedence over `[compare-release]` if
both are present. `DOTJPEGLI_REQUIRE_WIDE_SIMD=1` makes the worker identity check
reject a runner without accelerated Vector512 before any encoding is timed.
It still compares all 256 cases and the same retained algorithm scenarios.
Reuse or retry that specific failed job when hardware coverage is missing;
there is no additional concurrent workflow or substitute narrow measurement.
Ordinary `[compare-release]` runs retain all six lanes.

The release comparison uses .NET 10 and .NET 11 on x64 and ARM64, with additional
x64 lanes disabling AVX-512 to cover the narrow SIMD path. Each lane rebuilds
the release source with the current pinned SDK, refreshes only its isolated
restore for that SDK, and normalizes its assembly binding version so identical
measurement workers can load both versions. Source revision, SDK, dependency
lock and binary identities are retained. This isolates encoder changes on each
host/runtime; it does not measure the effect of upgrading the runtime itself.

The CI-only `--compare-revisions --release-regression` command requires that
baseline and the complete fixed matrix; it rejects selection/count overrides.
Two persistent workers run serially on one CPU with six balanced ABBA/BAAB
quartets, five warmups and 20 encodes per leg (100 for tiny inputs). JPEG bytes
must match across both versions and every leg. Existing timing validity guards
remain unchanged. Exit 1 means at least one valid case consistently favors the
release by more than 0.5% in every quartet; exit 2 means incomplete or invalid
evidence. Exit 0 means no detected latency regression within this matrix.
Inconclusive rows remain in the report and do not prove equal speed. Managed
allocations are reported separately. This comparison does not establish native
Jpegli performance parity or cover every public encoder option.

Release comparisons allow at most eight complete attempts for each case when
only the existing MAD, repeated-leg drift, minority timing-state or order-bias
guards fail. Every attempt uses the same six quartets, counts and warmups. The
first valid attempt is final, including a regression or an inconclusive result;
selection never uses speed or chooses the fastest trial. Identity, JPEG,
coverage, leg-layout, nonfinite and too-short-interval failures are not retried.
Cases still invalid at the cap fail validation. Other comparison modes retain
one attempt. A confirmed regression does not stop the remaining matrix.

Schema 2 writes each trial to a separate raw file after every completed quartet;
an interrupted trial retains its completed prefix. It also retains per-case
attempt analyses, the selected attempt, and initial-invalid and retry counts.
The first raw filename remains unchanged; retries add
`_attemptNN`, and `_attempts.json` records completed-trial analyses. No samples
are removed or merged within a trial. Earlier invalid attempts remain visible
in the report, even when a later valid attempt supplies the case verdict.
Conditioning on stability can favor quieter intervals, including intervals
without GC. Selected trials therefore do not describe an initially stable
matrix or the full latency distribution of all attempts.

The `release-regression-<architecture>-<framework>-<SIMD>-<revision>-attempt<run-attempt>`
artifacts retain raw legs, identities, input hashes, reports and build provenance.
The workflow-attempt suffix also applies to `algorithm-scenarios-*` artifacts,
so rerunning a specific failed job preserves every earlier job attempt. This
suffix is separate from the per-case timing attempts inside each artifact.
`--self-test-revision-comparison` validates the gate and fixed selection using
synthetic data locally, without encoding or reading clocks.

After retaining the release comparison, each x64 lane also compares the default
encoder with isolated builds enabling `DotJpegliSparseTransposedQuantization`
and, on .NET 11, `DotJpegliFullWidthTranspose`. These diagnostics use Kodak01/02/04/22,
low noise, low contrast, full noise and tiny odd edges at both distances,
samplings and progressions: 64 configurations per alternative. They use the
same CPU and identical workers, retain flags and binary hashes, and do not retry
unstable timings. The `algorithm-scenarios-*` artifacts are separate from the
release gate. They inform deterministic selection rules; they do not change
production policy. ARM64 records an explicit skip because its four-lane encoder
does not enter these eight-lane alternatives.

The `--aq-reciprocal-analysis --photograph <PPM> --output <empty-directory>`
command performs untimed numerical analysis and can run locally. It compares
Original and forced certified-reciprocal AQ encodes on six images, distances
1/4, 444/420 and baseline/progressive level 2, retaining all JPEG hashes and
per-site fallback counts. Photographs are bounded to 1024 by 1024 pixels.
Affected vector-batch counts distinguish lane certificates from batches that
execute original division. A deterministic full-bit operand sample also checks
exact division result bits. The command has no timers, benchmark invocation,
warmup/calibration, allocation measurement or performance verdict. All benchmark
commands keep their existing CI guard. The
[retained numerical study](../../docs/performance-evidence/2026-11-03-aq-reciprocal/README.md)
contains its rounding-cell proof, independent regressions, actual fallback
counts and scope; Automatic remains on original AQ division.

The `--coefficient-cache-analysis --photograph <PPM> --output <empty-directory>`
command performs a separate untimed storage study and can run locally. It
compares immutable 32-block bitmap/packed-short and per-band bit-packed tiles
with dense storage, retaining direct DC access, exact full-block and individual
coefficient reads, dense fallback, and actual pooled payload/index lengths.
The fixed 64 MiB accepted-rental cap is shared across all components in capacity
trials. Photographs are bounded to 1024 by 1024 pixels; output must be empty.

The [retained representation study](../../docs/performance-evidence/2026-11-03-compact-coefficients/README.md)
contains 360 layout comparisons on 24 real AQ image configurations, 75
adversarial comparisons and 30 shared-budget capacity attempts. Source,
coefficient and binary hashes accompany the storage and abstract work counts.
Replicated coefficients establish capacity only. The counters describe offline
codecs; production dense storage writes DCT output directly and returns spans
without these pack/copy passes. That study did not deploy a compact layout;
the later segmented dense cache is a separate implementation. This command
runs before the CI benchmark guard and has no timers, warmups, calibration,
allocation measurement or performance verdict; latency and peak memory remain
unmeasured. All benchmark commands retain their CI guard.

The bounded `--ac-reciprocal-experiment --photograph <absolute-PPM> --output
<empty-directory>` command researches AC quantization with prepared reciprocal
intervals. It runs after the normal CI guard, separately from native acceptance.
It retains machine and executable identities, exact input hashes, scalar and
portable SIMD fallback counts, six balanced ABBA/BAAB timing quartets and the
existing dispersion/drift/order validity checks. The
[retained investigation](../../docs/performance-evidence/2026-11-03-ac-reciprocal/README.md)
contains the interval proof, measurements and scope of the decision. This command
does not change encoder dispatch or establish complete-encode performance.

The separate `--byte-stuffing-experiment --corpus <Kodak-PPM-directory> --output
<empty-directory>` command compares scalar and portable SIMD stuffing inside
the existing word reservation. It measures four direct payload distributions,
two actual token-replay workloads and eight representative complete JPEG cases,
with fixed work counts and six balanced quartets. The same CI and timing-validity
guards apply. Its [retained decision](../../docs/performance-evidence/2026-11-03-byte-stuffing/README.md)
keeps Automatic on Original; valid helper gains do not establish encode gains,
and every invalid comparison remains in the report.

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
pushes retain the four-case default. After the managed/native diagnostic, the
same job starts two persistent workers with identical runner binaries, replacing
only the baseline worker's dependency-free encoder assembly. Each worker owns
its JIT, heap and pools. Only one receives measurement work at a time; both
inherit the parent's single-CPU affinity, runtime and environment.

The revision comparison interleaves six balanced ABBA/BAAB quartets per case,
with five untimed warmups and 20 encodes per leg (100 for inputs below 1,024
pixels). Both revisions use exactly the same counts, source pixels and encoding
policy. Worker startup, IPC, input loading, settings and hash checks are outside
the timer. Each complete public byte-array Encode call is timed separately;
natural GC remains included, and every output must match the case's JPEG hash.

The `performance-baseline-<revision>` artifact retains worker and source
identities, raw quartets, hashes, allocations and row-level validity and
preference reports. Validation checks MAD, within-quartet repeated-leg drift,
p90/p10 minority timing states and ABBA/BAAB order bias with the same limits as
the kernel comparison below. Invalid rows retain reasons; valid rows favor a
revision only when every paired ratio exceeds the 0.5% preference margin in
the same direction. Other rows are inconclusive. Native drift is not used to
correct mixed managed timing regimes. Allocated bytes exclude retained pool
capacity and peak working memory. This diagnostic does not change native
acceptance bounds or establish full parity.

Invoke this mode directly with `dotnet /absolute/DotJpegli.Performance.dll`;
the workers reuse that dotnet host. Its CI-only arguments are
`--compare-revisions --baseline-runner
/absolute/baseline-worker/DotJpegli.Performance.dll --baseline-revision
<full-baseline-sha> --current-revision <full-current-sha> --corpus
/absolute/corpus --output /absolute/empty-evidence`, plus the existing selection
flags. Blocks must be four or six; defaults are six blocks, five warmups and
20 iterations. The baseline directory must contain the identical runner build
with only DotJpegli.dll replaced by the named revision's same-target assembly.
The harness verifies runner hashes, encoder revision metadata, runtime, single
CPU affinity and measurement environment before starting. Synthetic protocol,
identity and statistics checks run locally with
`--self-test-revision-comparison`, without encoding or reading clocks.

To inspect older, separately measured evidence without running encodes, use
PowerShell 7.6 or newer:

```powershell
./tools/compare-performance-revisions.ps1 -Baseline /absolute/baseline-evidence -Current /absolute/current-evidence -BaselineRevision <full-baseline-sha> -CurrentRevision <full-current-sha> -Output /absolute/comparison
```

`-Output` is a prefix for the Markdown and JSON reports; `-Summary` optionally
appends the Markdown to a CI summary file. `-SelfTest` checks synthetic evidence
and mismatch rejection locally.

The same job then repeats the interleaved selection with
`DOTNET_EnableAVX512=0` for both workers, retaining a separate
`baseline-avx2-comparison` directory. The harness verifies that both workers
report eight float lanes, accelerated 256-bit SIMD and disabled 512-bit SIMD.
This exercises narrow ordering even when the host supports AVX-512. Both
selections belong to the single planned automatic run.

## Bounded workload diagnostics

`[performance-workloads]` in a main push requests additional x64 and ARM64
observations in that push's automatic workflow. Add `[full-parity]` to refresh
the complete native reference in the same workflow. The optional dispatch input
`performance_workloads=true` provides the same job when no suitable automatic
run exists; inspect and reuse existing runs as usual.

The CI-only command is `--workload-diagnostics --corpus <absolute-Kodak-directory>
--output <empty-absolute-directory>`. It verifies the existing pinned corpus,
then runs each fixed scenario in a fresh child process, sequentially, with a
ten-minute total measurement deadline. The job has an eighteen-minute timeout
including setup. A timed-out or failed worker preserves its logs, raw completed
samples and memory observations; incomplete coverage returns exit 2. There are
no retries, fastest-sample selection or changes to native acceptance.

The 25 scenarios include the 4:4:4 converted-plane bucket boundary
at 2048 by 2728/2736, the former dense coefficient bucket at 2048 by 4096/4104,
and the 64 MiB segmented coefficient capacity boundary at 2048 by 5456/5464.
4:2:0 uses 4096 by 2048/2064. These are real complete encodes of tiled Kodak01
pixels, not replicated coefficient capacity trials. Two additional 3840-by-2560
cases enlarge the whole Kodak01 photograph fivefold with deterministic
nearest-neighbor sampling, preserving its composition in 4:4:4 progressive and
4:2:0 sequential output. Source and derived dimensions and hashes are retained.
Tiling and enlargement exercise large encodes but do not substitute for a diverse
camera-native high-resolution photo corpus. A deterministic xorshift-noise input adds a dense case. Input hashes
and the generator source are retained through the revision and runner identities.
Per-encode cache observations report actual admitted peak capacity for converted
samples, coefficients and entropy tokens, plus their combined peak; returned
ownership must be zero. These observations establish the paths exercised without
assuming that a nominal dimension always selects the same cache layout.
Converted-plane boundary cases additionally require the expected admission or
fallback, and coefficient cases require actual retained coefficients. The
untimed self-test verifies that segmented capacity plans straddle 64 MiB.

The remaining cases cover RGB, XYB and Display-P3 source-ICC conversion,
byte-array and counting-discard-stream output, disabled/16 MiB/default cache
budgets, and one/two/four encoder workers or two/four concurrent encodes. Concurrent
calls own separate caller pixel arrays. The runner retains CPU affinity and
processor count; it does not fabricate four available cores on a smaller runner.
Only one scenario runs at a time. Parallel and concurrent scenarios use the
runner's available cores, while the fixed native protocol remains single-core.

Each process records its first encode before validation or warmup, then three
warmup batches and seven measured batches. Tiny images use 32 operations per
caller per batch; other scenarios use one. Every encode has a separate timer;
batch throughput also includes task scheduling. Input construction, profile
creation, hash checks and report writes are outside encode timers. Cold latency
excludes process startup and input/settings preparation. It is one observation,
not a startup distribution. One child uses a runtime configuration with normal
tiered compilation enabled; others retain the existing untiered configuration.
The actual runtime configuration is saved beside each request. Seven samples
cannot establish service p95/p99, and cross-process scenario differences are not
interleaved causal estimates. MAD over 10% marks noisy observations without
discarding them or producing a speed preference.

Streaming timings count and discard bytes, avoiding output retention and disk or
network I/O. Array outputs are checked by hash after timing; all timed stream
outputs must match the expected length. Separate untimed array/stream encodes
must match every byte, and parallel cases must also match serial encoding.
Concurrent scenarios additionally validate a separate concurrent batch of real
memory-stream encodes against the serial reference after the measurement snapshots.
The diagnostic sink and per-encode cache accounting add small instrumentation
costs; these observations never replace the uninstrumented parity measurements.

`GC.GetTotalAllocatedBytes` records process-wide workload allocation traffic,
including encoder output, result wrappers, diagnostic sinks, options copies
and task scheduling. It is reported per encode but is not an encoder-only
allocation count. Before/cold/warm
phase snapshots separately record process working set, process lifetime peak,
uncollected managed bytes and last-GC heap/committed/fragmented sizes. No GC is
forced. The parent polls RSS every 25 ms, retaining every observation. A sampled
peak is a lower bound and includes input generation, runtime and final untimed
output validation; the warm-phase snapshot precedes that validation. Neither
RSS nor heap snapshots measure exact retained array-pool capacity, and cache
ownership peaks exclude caller pixels, output and scratch. No native-memory
comparison or blanket low-memory claim is made.

The artifact is `performance-workloads-<architecture>-<revision>`. It contains
scenario requests, raw timing samples, output and input hashes, child runtime
settings, memory observations, logs and a readable report. Locally, only
`--self-test-workloads` is allowed: it checks scenario bounds, coverage, deterministic
input tiling/enlargement and stream byte accounting without encoding or reading clocks.

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
neutral baseline/candidate statistics and hash names. The initial 64 selected
configurations therefore produce 192 comparisons on eight-lane x64, or 128 on
ARM64 where the 64 narrow-ordering pairs are explicitly skipped. The sequential
intermediate-size selection adds 16 configurations: 48 comparisons on x64 or 32
on ARM64, with 16 narrow-ordering pairs skipped there. A policy pair is measured
even if it selects no new kernel; it must not be described as a forced
optimized-kernel result.

The automatic kernel comparison retains `kodim01`, `low-noise`, `low-contrast`
and `tiny-odd-edges`, and adds four controls accepted only by `--compare-kernels`:
`guard-below-32k` (255 x 128), `guard-at-32k` (256 x 128), `guard-above-32k`
(257 x 128) and `narrow-tall` (8 x 4096). Their coordinate-based texture preserves
common pixels across the three guard sizes. The first three bracket Automatic's
32,768-pixel minimum; the tall control exercises its eight-pixel dimension floor.
The same job then measures `intermediate-64k` (256 x 256) and `intermediate-128k`
(512 x 256) on the same CPU, retaining their overlapping coordinate-based
pixels. Both selections use both distances, sampling modes and progressions.
The commands run sequentially with separate evidence directories, preserving
the 64-configuration limit per invocation. The intermediate selection still runs
if the initial timings are invalid, and each command retains its own verdict.
Only selected controls are generated; the eight fixed acceptance controls and
native parity matrix are unchanged. The
[retained assessment](../../docs/performance-evidence/2026-11-03-kernel-guard-692a052/README.md)
supports keeping the conservative guard on the measured x64 hosts. It retains
invalid and inconclusive rows and does not establish a measured crossover.

Use `--self-test-kernel-comparison` locally to check synthetic statistics without
timing any encoding. It also checks control dimensions, deterministic content,
overlapping pixels, guard coverage, selection isolation and both selection
bounds. The actual mode requires Release and GitHub Actions.
Guards retain the 1 ms minimum and 10% MAD limit, and additionally reject
within-quartet drift, minority timing states and order bias. Both orders must
be balanced. Evidence preferences remain inconclusive unless every paired ratio
clears a 0.5% margin in the same direction and all guards pass. Kernel selection
rules remain conservative when evidence is small, inconsistent or unavailable.

The automatic run retains `kernel-comparison-<architecture>-<framework>-<sha>`
artifacts with `kernel-evidence/` and `kernel-intermediate-evidence/` directories.
Each retains machine/runtime/vector identities, source and JPEG hashes, raw
quartet legs, allocation/GC diagnostics, skipped cases and its report. Both
reports are also published in the same job summary.

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
