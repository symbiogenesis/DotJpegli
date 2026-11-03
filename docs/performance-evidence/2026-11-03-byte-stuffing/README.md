# Bounded portable SIMD byte-stuffing experiment

Keep Original as the Automatic byte-stuffing policy. This completes the bounded
experiment: the direct uniform-payload helper comparison favors SIMD on this
host, but the seven valid complete-JPEG comparisons remain inconclusive. The
other six comparisons are invalid. These results do not establish a portable
deployment guard, an end-to-end speed benefit or a reason to add larger staging
batches. Both variants remain available as immutable internal diagnostic choices.

This was a user-authorized exception to the repository's CI-only benchmark rule,
performed locally on November 3, 2026. It is not CI evidence and has no hosted run
identity. The regular performance command retains its CI guard. The measured
source was baseline `59f0da074d3c44cf455365bb17a4dc410b627d8b` plus the session's
working-tree AC reciprocal and byte-stuffing experiments, before their commits.
The encoder and runner SHA-256 values in [run.json](run.json) identify the actual
measured binaries; a baseline commit alone does not identify those binaries.

## Profiling and implementation

The earlier, separately instrumented CI profiles at revision `eb86172` attribute
10.9% of encoder-associated sampled weight to `WriteEntropyTokens` for
[low contrast](../2026-11-02-eb86172/profiles/full-performance-profile-low-contrast.speedscope.json)
and 5.0% for
[low noise](../2026-11-02-eb86172/profiles/full-performance-profile-low-noise.speedscope.json).
These are exclusive frame weights after selecting `BenchmarkManaged` stacks
containing encoder frames and removing CPU/unmanaged bookkeeping frames. There
is no isolated `WriteWord` frame; inlining can hide that attribution. These
profiles motivate an experiment but do not isolate stuffing cost, establish
acceptance, or support comparing speeds across hosts.

The candidate expands one eight-byte big-endian word with portable
`Vector128<byte>` operations. An equality mask identifies `FF` bytes; one of 256
shuffle masks copies the eight payload bytes and inserts zero after each `FF`.
A sixteen-byte store writes into space already reserved in the writer's existing
buffer, and only the returned `8 + popcount(mask)` prefix is committed. The
shuffle table owns 4,096 bytes of vector payload plus its managed array header.
Only the vector schedule references it. There is no per-call table allocation or
new staging array.
Software-only and .NET Standard execution retain scalar expansion.

Production still bypasses both expansion helpers when a word contains no `FF`,
using its existing eight-byte store. With fewer than sixteen free bytes, the
existing scalar byte/pair drain path preserves stuffing boundaries and stream
write prefixes. The candidate therefore does not change output-buffer capacity
or introduce larger drain batches. The direct `word-no-ff` experiment deliberately
calls the expansion helpers for every word; it does not model that production
fast path and cannot justify changing it.

## Host, method and retained evidence

The local ARM64 host has a Qualcomm Snapdragon X2 Elite Extreme X2E94100 Oryon CPU,
Windows build 26300 and .NET `11.0.0-rc.3.26522.148`. The process used CPU 17
(affinity mask 131072), AboveNormal priority, one encoding worker and disabled
tiered compilation. Portable 128-bit SIMD was accelerated; 256-bit and 512-bit
SIMD were not. The runtime's environment report supplies the generic ARM CPU
identifier; the model name above comes from the local machine identification.

Every comparison uses six balanced ABBA/BAAB quartets. Each variant receives
32 interleaved untimed operations before a comparison and three warmups before
each leg. Fixed operations per leg are 256 for direct expansion, 200 for token
replay, 100 for complete JPEGs and 2,000 for tiny JPEGs. A direct operation
expands 32,768 words; a replay operation consumes 32,768 tokens. Complete-encode
operations call the public byte-array encoder once. Timed operation counts are
identical between variants, with no calibration or timing-based dispatch.

Direct and replay output buffers are prepared outside timing. Hashing, output
snapshots and exact equality comparisons run before and after each timed leg.
Complete encodes include their normal result allocation. The candidate's table
initializes during untimed work. Original and optimized outputs match in every
completed comparison, including all eight complete JPEG controls.

The paired ratio is the median of six quartet ratios, each dividing candidate
geometric-mean latency by baseline geometric-mean latency. Existing guards are
unchanged: at least 1 ms per leg, relative MAD at most 10%, p90/p10 drift at most
20%, repeated-leg drift at most 10% and order bias at most 10%. A strong
preference also requires every quartet ratio to clear the same side of
`1 +/- 0.005`. Inconclusive does not establish equivalence. Invalid rows support
no speed claim; their unrounded values and failure reasons remain retained.

The whole [summary](summary.json) is **invalid**, with 14/14 comparisons
completed, eight valid and six invalid. There is no valid aggregate verdict.
[cases.json](cases.json) retains complete analysis, allocations, input/output
hashes and all legs. [raw-comparisons/](raw-comparisons/) retains each final
incremental comparison snapshot, written before analysis. The exact local
[startup source](report-harness/Program.cs) and
[project](report-harness/Research.csproj) preserve affinity, priority, tiering
and command dispatch. Their original relative paths assume placement under
`.artifacts/plan-research`; relocate the snapshot there to reproduce the build.

The encoder SHA-256 is
`6dfab53fb794e9d68d30cf7fbeca4d8c5ded3806d0984ed6107e1b6a0bec5a11`;
the runner SHA-256 is
`7b514002d23cd99f566dcafe1851eab2085c5234e3951ec0779af4fa6e53010f`.
Kodak input identity is checked against the existing pinned corpus manifest;
control images use the existing deterministic generator. The direct/replay
generator's seed and schedule, input hashes, output hashes and output sizes
remain in the reports. Images and binary outputs are omitted from this retained
snapshot.

## Results

Ratios below one favor SIMD. Timing statistics and analyzed allocation rates
are per operation, rather than per individual word or token. Raw `allocatedBytes`
fields retain per-leg totals. Allocations below are mean **Original / SIMD bytes
per operation**. Invalid timing rows retain their allocation totals for
diagnostics without implying an allocation benefit.

| Case | Operations/leg | Output bytes | Valid paired ratio | Allocated bytes, Original / SIMD | Preference |
| --- | ---: | ---: | ---: | ---: | --- |
| word-no-ff | 256 | 262144 | invalid | 3.1 / 1.7 | inconclusive |
| word-uniform | 256 | 263203 | 0.440206 | 4.8 / 0.0 | SIMD |
| word-ff-one-in-eight | 256 | 294912 | invalid | 1.3 / 0.6 | inconclusive |
| word-all-ff | 256 | 524288 | invalid | 5.7 / 1.3 | inconclusive |
| replay-canonical-mixed | 200 | 93808 | invalid | 4177.1 / 4176.8 | inconclusive |
| replay-raw-all-ff | 200 | 196608 | invalid | 4181.7 / 4177.0 | inconclusive |
| kodim01 / 444 / p0 / d1 | 100 | 148186 | invalid | 193927.3 / 193918.5 | inconclusive |
| kodim01 / 420 / p2 / d4 | 100 | 50167 | 1.000693 | 149362.1 / 149362.1 | inconclusive |
| low-noise / 420 / p0 / d1 | 100 | 12206 | 1.009247 | 52114.6 / 52121.5 | inconclusive |
| low-noise / 444 / p2 / d4 | 100 | 2504 | 1.004465 | 77536.6 / 77538.9 | inconclusive |
| low-contrast / 444 / p2 / d1 | 100 | 13981 | 0.989914 | 129216.3 / 129215.5 | inconclusive |
| low-contrast / 420 / p0 / d4 | 100 | 3275 | 1.002341 | 48775.7 / 48770.6 | inconclusive |
| tiny-odd-edges / 420 / p2 / d1 | 2000 | 877 | 0.985943 | 63839.9 / 63839.9 | inconclusive |
| tiny-odd-edges / 444 / p0 / d4 | 2000 | 603 | 0.994383 | 34820.3 / 34820.3 | inconclusive |

The valid uniform helper row includes 1,059 `FF` bytes in 262,144 input bytes.
Its ratio supports faster direct expansion for that deterministic workload on
this host. Both token replay rows fail stability checks, so their medians cannot
establish replay improvements. Seven JPEG rows pass all guards, but their
quartets do not establish a consistent improvement beyond the preference margin.

All six invalid comparisons exceed the repeated-leg drift guard. Raw-all-FF
replay also exceeds the candidate p90/p10 guard. No unsuccessful row was dropped
and no threshold was relaxed. The complete JPEG selection covers a photograph,
low noise, low contrast and tiny odd edges, both distances, both sampling modes
and baseline/progressive output through eight fixed representative cases; it
does not cover every combination or additional ICC/parallel workloads.

`GC.GetTotalAllocatedBytes` counts the whole process, so small nonzero direct
totals do not demonstrate per-call helper allocations. Replay totals include the
normal writer object and 4 KiB output buffer; its `MemoryStream` storage is
preallocated. Similar JPEG totals do not establish an allocation reduction.
Both schedules use the same extended writer and options layout, including the
new diagnostic preference fields; these allocation pairs do not compare object
sizes with the earlier revision.
Neither actual retained pool capacity nor peak working memory was measured.

The bounded candidate has now been implemented, checked for exact output and
compared through the helper, replay and complete-encode layers. Keep its
explicit diagnostic choice and leave Automatic on Original. Larger staging
batches and any future default deployment require new, independently justified
evidence; neither is a promised follow-up from this experiment.

## Focused validation

The 39 focused stuffing regressions pass on .NET 10, .NET 11 and the .NET
Standard compatibility build, and again on .NET 11 with hardware intrinsics
disabled. They cover every stuffing mask and payload byte position, bounded
dirty-buffer writes, every spill alignment, one through seventeen free buffer
bytes, exact destination calls and partial failures. Complete-encode checks
include baseline, progressive and custom refinement scripts, cached and uncached
scans, restart and unaligned parallel replay, tiny edges, cancellation, recovery
and concurrent immutable choices. Release library and performance-tool builds
pass for all supported targets; both normal experiment commands still reject
local execution through the unchanged CI guard.
