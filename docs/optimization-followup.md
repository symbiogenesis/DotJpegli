# Follow-up optimization batch

This batch follows the source audit at `51126a7`. Earlier experiments have
already been completed or retired; this document tracks the newly identified
work. No operation-count reduction is a measured encoding speedup.

| Lead | Intended integration | Status |
| --- | --- | --- |
| Progressive AC collection across scans | Exact per-component traversal with independent scan states and bounded token storage | Implemented |
| Joint sample/coefficient cache planning | Reserve complete coefficient capacity before admitting converted samples under constrained budgets | Implemented |
| Parallel histogram/token production | Complete-cache baseline batches with private counters and exact predecessor DC values | Implemented |
| Sampled ICC inverse index | Bounded immutable exact predecessor index, including plateau ties | Implemented |
| Smoothing row reuse | Fixed-size three-row halo with original floating-point operation order | Implemented |
| Byte-input XYB transfer values | Lazy exact 256-value table for untransformed byte RGB/grayscale | Implemented |
| Quantized parallel row pipeline | Bounded quantized rows with ordered worker completion and failure handling | Implemented |
| Huffman symbol ordering | Offline complete-JPEG search within equal-length code groups; retain control on ties | Implemented |
| Image-specific RDOQ costs | One additional bounded pass using actual image Huffman lengths | Implemented |
| Cross-block DC trellis | Bounded opt-in search with restart-aware predictors and explicit quality validation | Implemented |
| Improved chroma preparation/advice | Opt-in reconstruction-aware candidates and sampling assessment | Implemented offline |
| Clipping-aware deringing | Opt-in saturated-edge preprocessing with decoded quality floors | Implemented offline |

Ordinary encoding must preserve its existing output unless an explicitly
selected compression experiment changes coefficients or sampling. Experiments
retain controls and rank complete encoded output; approximate rate models never
establish final byte savings. Internal diagnostic choices belong to immutable
encode options and never change another concurrent encode.

Local validation consists of Release builds and focused correctness tests.
Full suites and every benchmark run on CI. The settled batch is pushed once to
`main`, reusing that push's automatic run for correctness, full native comparison
and bounded same-host diagnostics. Logical changes are committed as they become
ready. Raw invalid timing cases remain retained and no thresholds are relaxed.

## Exact implementation choices

Progressive AC histogram/token collection now visits each component block once
when coefficients are uncached or partially cached and a component participates
in multiple AC scans, feeding its independent spectral/refinement states.
Complete coefficient caches and simple one-AC-scan components retain the
established scan-major traversal. DC traversal keeps
its original ordering. Fused AC token rentals interleave in component, block and scan
order, replacing the previous whole-scan admission priority; the one shared cap
still charges actual rentals. Abandoned token streams are recomputed during
emission. This changes which scans fit, without changing their encoded symbols.

The cache planner reserves a complete dense/segmented coefficient cache only
when it fits the existing 64 MiB cap and shared available capacity is below
128 MiB. Normal 192 MiB admission remains unchanged. Reservation is planning,
not allocation, and is released before coefficient allocation. Source-ICC
preflight may already own converted samples; its earlier validation is retained.

The ICC index is a conservative single-level value grid inspired by
[Recursive Grid Indexing, July 2026](https://onlinelibrary.wiley.com/doi/10.1002/spe.70091),
not its full recursive structure. Eligible destination sampled curves have
512–8192 knots and at most 32 knots per bucket. Each index has 1028 bytes of
integer payload plus object overhead. Small, strongly clustered or otherwise
ineligible curves retain binary search. Exact comparisons, interpolation and
terminal/nonterminal plateau rules remain unchanged.
Indexed sampled inversion is isolated in a non-inlined helper so its search
locals do not enter the parametric inverse's generated frame. Parametric power,
clipping and piecewise arithmetic retain their original order.

Raw byte-input XYB conversion uses a lazy 256-float table generated using the
same runtime normalization and transfer operations. First use computes the
whole 1 KiB payload, so a cold tiny encode can pay extra initialization work.
ICC-transformed and higher-precision sources retain original conversion.

Smoothing retains the original eight-neighbor addition order. Three halo rows
and one fixed tile use 4504 bytes of stack scratch. Raw sample reads fall from
576 to 100 for a 1x1 sampling area and from 9216 to 1156 for a 4x4 area. These
counts do not measure complete encoding speed, allocations or peak memory.

Eligible three-component baseline 4:4:4/4:2:0 parallel encodes produce quantized
short rows directly. AQ reads the worker-owned snapshot and its strengths are
copied before workers start. The ordered consumer retains original MCU, dummy-DC,
restart and buffered-output behavior. No whole-image coefficient cache is needed
on this path. Float row storage is allocated only for the legacy DCT path;
eligible fused quantizer layouts are prepared before parallel work to avoid
losing allocations from concurrent lazy initialization. Custom sampling,
grayscale, progressive and explicit coefficient-search cases retain their
existing preparation paths.

Complete-cache baseline histogram/token construction also supports private
worker counters, direct predecessor DC lookup and deterministic ordered merge.
Actual token staging rentals are capped at 4 MiB across at most 32 slots;
oversized or abandoned chunks are recollected before histogram merge. This is
separate from existing parallel token replay.

Default canonical Huffman table planning retains a noncapturing projection.
Optional symbol reordering has a separate non-inlined helper, avoiding the
captured ordering state introduced into ordinary planning by the initial batch.

Single-worker encodes also reject optional parallel histogram collection at the
call site. Release IL inspection found the collector's captured cancellation
state was constructed before its own worker-count rejection. The call-site
guard avoids entering that method; the parallel body is unchanged. This final
cleanup (`1898828`) follows measured revision `744f7be`, so the paired timings
below do not measure its effect. All three Release library builds, 122 focused
parallel correctness cases and the formatter passed locally; no local benchmark
was run.

## Output-changing research

The [offline tool guide](../tools/DotJpegli.Parity/README.md#follow-up-compression-experiments)
describes commands, bounds and reports. Ordinary public encode options do not
invoke these searches. Huffman ordering preserves every symbol's code length
and all coefficients; complete output decides whether stuffing savings exist.
DC trellis and image-specific AC feedback retain their decoded-quality controls.
Search metadata counts extra passes, including discarded candidates; these
counts do not establish a speedup.

Color preprocessing implements a bounded experimental reconstruction model,
not an exact libsharpyuv port. Its centered bilinear model excludes quantization,
rounding and decoder variations. Model improvement only admits a real encoding
trial. Saturated-run overshoot is also experimental. The integer chroma-edge
advisor reports its own uncalibrated score and never changes explicit sampling;
it does not reproduce Google's classifier or inherit the speedup reported by
its [September 2026 implementation](https://chromium.googlesource.com/webm/libwebp/+/4bd9edc46fbaf09d71952f411042c7c32c34572b).

## Hosted comparison and corrective validation

The automatic push run compares ordinary, smoothing, byte-XYB, constrained-cache
and source-ICC encodes against pre-batch revision
`b2fa25da8fdbe601daf145b93f297fcf01a5caa1` on the same host, separately for
.NET 10/11 and x64/ARM64. An additional sampled-ICC scenario uses a shared
1025-knot destination curve so the new index is actually exercised. Source and
target profile hashes and scenario names are checked. The profiles and initial
output checks are outside timed intervals; this measures warm encoding and
does not establish cold-start benefits.

The two-worker revision scenario uses at least two available CPUs and records
the shared affinity. Single-worker scenarios remain pinned to one CPU. This
avoids treating oversubscribed one-CPU execution as a parallel speed comparison.

The first [automatic comparison run, 41769154898](https://github.com/symbiogenesis/DotJpegli/actions/runs/41769154898)
tested `4c2b2abe` against `b2fa25da`. Each cell below gives timing-valid cases
over measured cases, followed by the geometric mean of those cases' primary
paired-median current/baseline latency ratios. Each case used six balanced
ABBA/BAAB quartets. Ratios below one favor the batch; invalid rows remain in the
artifacts and do not support latency claims.

| Scenario | x64 .NET 10 | x64 .NET 11 | ARM64 .NET 10 | ARM64 .NET 11 |
| --- | ---: | ---: | ---: | ---: |
| Ordinary | 47/64; 0.9960 | 55/64; 0.9897 | 60/64; 1.0084 | 59/64; 1.0027 |
| Source ICC | 3/4; 0.9951 | 4/4; 0.9964 | 4/4; 1.0058 | 4/4; 1.0129 |
| Smoothing | 2/8; 0.1426 | 8/8; 0.1436 | 8/8; 0.2084 | 8/8; 0.2012 |
| Constrained cache | 6/8; 0.3913 | 7/8; 0.4519 | 8/8; 0.4799 | 8/8; 0.4827 |
| Sampled ICC | 7/8; 0.8285 | 8/8; 0.8709 | 8/8; 0.7541 | 8/8; 0.7208 |

This produced 368 of 432 requested rows: 322 timing-valid and 46 timing-invalid.
All 368 baseline/current JPEG hash pairs and all 8832 measured leg hashes
matched. XYB produced no rows because its worker omitted the required output
ICC profile. The two-worker mode produced no rows because the identity guard
still required one CPU. Those 64 missing rows are harness failures, separate
from timing invalidity. Neither mode has CI1 performance evidence.

The x64 .NET 10 host was an AMD EPYC 9V45; x64 .NET 11 used an AMD EPYC 7763.
Both used eight float lanes with 256-bit acceleration and 512-bit acceleration
disabled. ARM64 reported implementer `0x41`, part `0xd49`, four float lanes and
128-bit acceleration. Runtimes were .NET 10.0.14 and .NET 11.0.0-rc.3.26522.148.
Hosts were matched within each revision pair; differences between cells do not
isolate framework effects.

All 26 valid smoothing cases and all 31 valid sampled-ICC cases consistently
favored the batch. All 16 constrained-cache progressive cases were valid and
consistently faster, with ratios 0.1713-0.3813 and 15,994-16,983 fewer allocated
bytes per encode. Ordinary allocation medians across measured rows increased
by 625-679 bytes per encode; sampled-ICC medians increased by 1758-1805 bytes.
These warm allocation counters include timing-invalid rows and exclude initial
static-table preparation, retained pool capacity and peak working memory.

Regressions remain part of the evidence: ARM64 ordinary progressive cases
consistently favored the baseline in 18 .NET 10 cases and 12 .NET 11 cases.
All four ARM64 .NET 11 source-ICC cases favored the baseline, with ratios
1.0123-1.0143. One x64 .NET 11 constrained-cache sequential case also favored
the baseline at 1.0106. This is selected managed-revision evidence; it establishes
no global or Google-native speed superiority.

The corrective batch restores scan-major collection for complete coefficient
caches, isolates indexed sampled inversion from parametric curves and removes
the default Huffman planning closure. It also supplies the XYB profile, checks
actual frame sampling and permits multiple CPUs only for the two-worker mode.
XYB uses explicit numeric 4:4:4/4:2:0 component factors; conventional 4:2:0
subsamples XYB's Y component and therefore disables AQ under the existing policy.
Tiny actual-encode self-tests now check every scenario's front door, frame and
sampling without clocks. The following paired matrix measures revision
`744f7be`; individual corrective changes were not isolated.

Local Release builds passed for all three library targets and both tool targets.
Focused checks passed on Windows ARM64: 117 new-feature cases on .NET 10,
163 parallel-path cases and 18 final fusion cases on .NET 11, and 60 portable
library cases on .NET 10. Additional earlier focused checks covered the exact
lookup and search fixtures. Synthetic revision/provenance tests passed on both
runtimes, and the parity harness self-test and workflow syntax checks passed.
No benchmark or full suite ran locally.

The same workflow runs the full fixed native quality/performance matrix and
bounded native-decoder compression searches. Full suites and benchmarks are
CI-only. Performance acceptance and timing validity are separate outcomes;
neither exact output tests nor synthetic statistics prove a latency gain.

## Corrective CI paired comparison

The [corrective run, 41772797925](https://github.com/symbiogenesis/DotJpegli/actions/runs/41772797925)
compared `744f7be` with the same pre-batch source `b2fa25da`, rebuilt with the
same worker and SDK. It produced all 432 requested rows: 385 timing-valid and
47 timing-invalid. Every one of the 432 baseline/current JPEG hash pairs
matched, and all 10,368 measured legs matched their expected case hashes.
Each case retained its single complete attempt, including every invalid row.

Cells give valid/total rows and the geometric mean of valid cases' primary
paired-median current/baseline ratios. They use the same six-quartet method as
CI1; below one favors `744f7be`.

| Scenario | x64 .NET 10 | x64 .NET 11 | ARM64 .NET 10 | ARM64 .NET 11 |
| --- | ---: | ---: | ---: | ---: |
| Ordinary | 61/64; 0.9985 | 54/64; 1.0060 | 61/64; 0.9981 | 57/64; 0.9998 |
| Source ICC | 3/4; 1.0009 | 4/4; 1.0079 | 4/4; 1.0012 | 4/4; 1.0002 |
| Smoothing | 8/8; 0.1495 | 4/8; 0.1483 | 8/8; 0.2168 | 8/8; 0.2006 |
| Byte XYB | 8/8; 0.6386 | 6/8; 0.6162 | 8/8; 0.6769 | 8/8; 0.6726 |
| Constrained cache | 8/8; 0.5024 | 6/8; 0.6176 | 8/8; 0.4808 | 8/8; 0.4828 |
| Sampled ICC | 3/8; 0.8785 | 7/8; 0.8646 | 8/8; 0.7686 | 8/8; 0.7309 |
| Two workers | 1/8; 0.9978 | 6/8; 0.9250 | 8/8; 0.9196 | 8/8; 0.9436 |

All 28 valid smoothing cases, 30 valid byte-XYB cases and 26 valid sampled-ICC
cases consistently favored the batch. All 14 valid constrained-cache
progressive cases also favored it, with ratios 0.1709-0.3764 and 17,116-18,139
fewer allocated bytes per encode. Constrained sequential cases were all
inconclusive. The aggregate constrained-cache ratios combine those different
progressive and sequential outcomes.

Parallel sequential output consistently favored the batch in all 11 valid
cases, with ratios 0.7457-0.9234 and 32,500-34,868 fewer allocated bytes per
encode. Coverage is narrower on x64: .NET 10 had only one valid parallel case,
which was progressive and inconclusive; all four sequential cases were invalid.
x64 .NET 11 had three valid sequential and three valid progressive cases, with
the latter all inconclusive. Invalid cases supply no parallel speed claim.

Residual regressions are retained. Ordinary x64 .NET 11 had seven consistent
baseline preferences at ratios 1.0077-1.0124; ARM64 .NET 11 had two at
1.0089-1.0094. Source ICC had one x64 .NET 11 baseline preference at 1.0084;
all other valid source-ICC cases were inconclusive. Parallel progressive output
favored the baseline in one ARM64 .NET 10 case at 1.0123 and all four ARM64
.NET 11 cases at 1.0156-1.0246. The ARM64 parallel-progressive geometric means
were 1.0061 and 1.0202. An aggregate gain must not hide these costs.

Across all measured rows, ordinary allocation medians increased by 18-39 bytes
per encode and sampled-ICC medians by 1138-1164 bytes. Allocation medians include
timing-invalid rows; the counters measure warm allocation, not initial table
preparation, retained pool capacity or peak working memory.

Both x64 hosts reported AMD EPYC 7763 processors, eight float lanes and
256-bit acceleration, with 512-bit acceleration disabled. ARM64 retained the
recorded `0x41`/`0xd49` identity and four float lanes. Single-worker scenarios
used affinity `0`; two-worker scenarios used `0-3` with four available CPUs.
Baseline and current runtime/affinity identities matched within each pair.
CI1 and CI2 used separate hosts: changes between their ratios do not isolate
the corrective patches or framework effects. These results describe selected
managed-revision pairs and establish no global native-encoder superiority.
The later single-worker guard `1898828` remains outside these measurements.

## Corrective CI compression results

The [corrective automatic run, 41772797925](https://github.com/symbiogenesis/DotJpegli/actions/runs/41772797925)
tested revision `744f7bead7b9ba982b2f2127f1d2714fcc82def9`. All four bounded
searches completed on `kodim01`, a 768x512 RGB8 image, with explicit 4:2:0
sampling and a distance-1 control. Color candidates also tested distances 0.8
and 1.25. Huffman ordering used progressive level 2; the other searches used
sequential output, with separate component scans for DC and feedback.
Each search compares with its own unchanged managed control. The pinned native
decoder `031a0077f5799a6041004267fc12b956c1f52a20` supplies the common RGB8
decode with fancy upsampling and `JDCT_ISLOW`; it is not a native-encoder size
comparison.

| Search | Complete candidates | Control / winner bytes | Result on this image |
| --- | ---: | ---: | --- |
| Equal-length Huffman ordering | 4 | 126953 / 126948 | Frequency-descending order saved 5 bytes (0.00394%); all decoded pixels matched the control |
| DC chain trellis | 5 | 130250 / 130250 | Control retained; all four modified candidates failed all three quality floors |
| Image-specific AC Huffman feedback | 7 | 130250 / 130250 | Control retained; all six standard/feedback candidates failed all three quality floors |
| Color preprocessing | 15 | 130524 / 130500 | `overshoot-d1-w1` saved 24 bytes (0.01839%) and met all three measured quality floors |

The DC candidates saved 75-519 bytes before quality filtering. Even the smallest
DC change lost 0.001158 dB RGB PSNR and reduced local luma SSIM and the multiscale
structural score. Feedback reduced complete size further than its paired
standard-RDOQ candidates, but did not preserve the control quality: at lambda 16,
feedback saved 14830 bytes (11.39%) while losing 1.216 dB RGB PSNR and both
structural scores. These smaller files were rejected, not accepted compression
gains.

All 15 color trials completed; no hypothesis was rejected before encoding or
by coefficient preflight. The selected overshoot increased RGB PSNR by
0.001070 dB, local luma SSIM by approximately 2.03e-8 and the multiscale score
by 1.70e-7. Half-weight overshoot saved 20 bytes but failed the local SSIM floor.
Distance-1 reconstruction candidates improved RGB PSNR by 0.077-0.100 dB but
added 802-1182 bytes, so neither was selected. All distance-1.25 variants failed
the three control floors.

The artifact retains every trial's sizes, hashes and quality decisions; all
four downloaded winner files matched their reported lengths and SHA-256 hashes.
These 31 complete encodes/decodes establish only the stated single-image
results. No timings were collected, and the tiny accepted savings do not
establish a general compression improvement or human-quality equivalence.
Promotion of an output-changing search needs a broader photographic, line-art
and chroma-edge corpus under the same decoder and control quality floors.

## Corrective CI correctness and native comparison

Revision `744f7be` passed all six full-suite jobs across Windows/Linux and
.NET 10, .NET 11 and the portable library target. All four deployment jobs,
both Windows NativeAOT CLI jobs, reproducibility checks and package publishing
also passed. These checks and the following measurements used the same
[automatic run](https://github.com/symbiogenesis/DotJpegli/actions/runs/41772797925).

The full native quality/compression gate passed all 1280 cases, 2057 matched-rate
targets and 4396 matched-quality targets against the pinned upstream reference.
This establishes the declared size, RGB PSNR and local luma SSIM bounds on that
matrix, rather than visual equivalence.

The native latency matrix had complete, valid coverage of all 256 cases, but
failed its fixed noninferiority limits. Sixty-six cases exceeded the 1.10
per-case ratio and two group means exceeded 1.05. Natural progressive 4:4:4 and
4:2:0 passed their complete gates, with managed/native geometric mean ratios
0.8385 and 0.8138. Natural sequential ratios were 1.0833 and 1.0373; both groups
failed individual bounds. All four control groups also had individual failures.
The worst case was `kodim02/444/p0/d4` at 1.4453.

That native comparison used one CPU on an Intel Xeon Platinum 8573C with
.NET 11.0.0-rc.3.26522.148, eight float lanes and accelerated 256/512-bit vectors.
Its host differs from CI1. Cross-run ratio changes do not measure the corrective
patches; only each same-host managed revision pair isolates the batch. The
results do not establish global speed superiority over Google's encoder.

## Remaining investigation

The legacy floating-point parallel row workspace and its lazy preparation or
object layout are candidates for investigating the progressive parallel costs.
Their causal role is unproven. The current evidence does not justify a new
hardware-specific threshold or a claim that those costs have been removed.

Subsequent commit `f3ffe8f` implemented the Priority 1–3 speed, allocation, and
compression batch (32-row rolling converted planes, dimension-aware
`PooledOutputStream` initial capacity, cached quantization configurations and
scan presets, pooled histograms, per-block `ulong` nonzero AC masks for
progressive scans, and in-register `PruneTailZeroRunTransposed`). See
[`PLAN.md`](../PLAN.md) for the complete status table and remaining validation,
tiny-control, rolling-plane, block-mask segmentation, parallel-progressive, and
opt-in compression items.
