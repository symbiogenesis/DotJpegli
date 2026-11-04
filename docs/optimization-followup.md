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
when a component participates in multiple AC scans, feeding its independent
spectral/refinement states. Simple one-AC-scan components retain the established
scan traversal. DC traversal keeps
its original ordering. AC token rentals interleave in component, block and scan
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

## Planned hosted measurement

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
