# Automatic narrow-ordering assessment

[Automatic run 41592528894](https://github.com/symbiogenesis/DotJpegli/actions/runs/41592528894)
measures revision `692a052ead6561515e9a8908e2ad619dff46954b`. This assessment
covers the 32K-pixel guard, intermediate sizes and unusually narrow/tall images.
It assesses the deployed Automatic policy
alongside isolated forced kernels, on x64 and ARM64 with .NET 10 and .NET 11.

## Deployment decision

Keep the existing Automatic policy: Tiled only on x64 with accelerated 256-bit
SIMD, eight float lanes, disabled 512-bit SIMD, both dimensions at least eight
and at least 32,768 source pixels. Keep Original below the floor and on other
narrow configurations; normal wide-SIMD dispatch remains available. AQ continues
to use insertion automatically. No runtime dispatch change is warranted.

Across the two x64 lanes, 112 of 128 reachable Automatic rows pass every guard.
All 112 favor Automatic by median (ratios 0.922994 to 0.993885); 83 meet the
stronger all-quartets preference rule. The other 29 valid rows are inconclusive
under that rule, and 16 rows are invalid. Across non-tiny forced narrow pairs,
111 of 144 rows are valid, with all valid medians favoring Tiled (0.926053 to
0.995173); 66 strongly prefer Tiled and 45 remain inconclusive. No x64 row
strongly prefers Original.

The new threshold, tall and intermediate controls support retaining the
conservative deployment floor on these measured hosts. Forced Tiled also wins
some below-floor rows, so the data do not identify a crossover at 32K or justify
lowering the floor. The tiny .NET 11 forced-narrow 444/p2/d4 row has a valid
median ratio of 1.003818, with an inconclusive preference. Automatic's tiny and
below-floor rows compare the same selected kernels; their mixed directions do
not establish an optimization benefit or equivalence.

Each entry below is **valid rows / 8 (strong candidate preferences)**. All other
valid rows are inconclusive; the complement to eight is invalid. Strong
candidate means Tiled for a forced pair and Automatic for a policy pair.

| Input | .NET 10 forced narrow | .NET 10 Automatic | .NET 11 forced narrow | .NET 11 Automatic |
| --- | ---: | ---: | ---: | ---: |
| Below 32K | 4/8 (2) | 3/8 (0; same kernels) | 8/8 (4) | 7/8 (0; same kernels) |
| At 32K | 6/8 (3) | 4/8 (2) | 8/8 (4) | 8/8 (4) |
| Above 32K | 4/8 (2) | 4/8 (4) | 6/8 (4) | 8/8 (4) |
| Narrow/tall | 7/8 (3) | 7/8 (2) | 8/8 (4) | 8/8 (3) |
| 64K | 4/8 (3) | 8/8 (7) | 7/8 (5) | 8/8 (8) |
| 128K | 3/8 (3) | 5/8 (5) | 7/8 (5) | 7/8 (7) |

## Method and coverage

Each case uses six balanced ABBA/BAAB quartets in one process on CPU 0, with
tiered compilation disabled, five untimed warmups per leg and 20 encodes per
leg (100 for tiny odd edges). Each forced pair changes one immutable preference
while holding the other Automatic. The combined policy pair compares both
Original preferences against both Automatic preferences. Only complete public
byte-array encodes are timed; output comparisons run outside the timer.

The initial selection includes Kodak01, low noise, low contrast, 19-by-17 odd
edges, and these coordinate-based textures:

| Input | Dimensions | Pixels | Automatic on measured x64 |
| --- | ---: | ---: | --- |
| guard-below-32k | 255 x 128 | 32,640 | Original |
| guard-at-32k | 256 x 128 | 32,768 | Tiled |
| guard-above-32k | 257 x 128 | 32,896 | Tiled |
| narrow-tall | 8 x 4,096 | 32,768 | Tiled |
| intermediate-64k | 256 x 256 | 65,536 | Tiled |
| intermediate-128k | 512 x 256 | 131,072 | Tiled |

The first four textures run with the photograph and existing controls. The two
intermediate textures run sequentially afterward on the same host and CPU.
Every image uses distances 1/4, sampling 444/420 and progression 0/2. Separate
directories preserve the verdicts of both bounded selections. These diagnostic
inputs do not change the fixed native acceptance corpus or its bounds.

The measured x64 lanes have accelerated 256-bit SIMD, eight float lanes and
disabled 512-bit SIMD. ARM64 has four float lanes and explicitly skips forced
narrow-ordering pairs because that kernel is unreachable. Its combined
Automatic pairs select Original for both kernels, providing same-kernel
controls rather than evidence of a Tiled benefit.

| Lane (initial / intermediate reports) | CPU | All valid / measured | Forced narrow valid / measured | Automatic valid / measured |
| --- | --- | ---: | ---: | ---: |
| ARM64 .NET 10 ([initial](ARM64-net10/kernel-evidence/report.md) / [intermediate](ARM64-net10/kernel-intermediate-evidence/report.md)) | ARM implementer 0x41, part 0xd49 | 147 / 160 | 80 skipped | 75 / 80 |
| ARM64 .NET 11 ([initial](ARM64-net11/kernel-evidence/report.md) / [intermediate](ARM64-net11/kernel-intermediate-evidence/report.md)) | ARM implementer 0x41, part 0xd49 | 154 / 160 | 80 skipped | 77 / 80 |
| x64 .NET 10 ([initial](X64-net10/kernel-evidence/report.md) / [intermediate](X64-net10/kernel-intermediate-evidence/report.md)) | AMD EPYC 9V45 | 154 / 240 | 47 / 80 | 53 / 80 |
| x64 .NET 11 ([initial](X64-net11/kernel-evidence/report.md) / [intermediate](X64-net11/kernel-intermediate-evidence/report.md)) | Intel Xeon Platinum 8370C | 209 / 240 | 67 / 80 | 72 / 80 |

All 800 comparisons completed: 664 valid and 136 invalid. Every one of the
eight complete selection summaries is **invalid**, so there is no valid
aggregate timing verdict. Only individual valid rows support the decision
above. ARM64's 152 valid Automatic rows are all inconclusive, as expected for
same-kernel controls; they do not prove equivalent latency.

Original, optimized and Automatic JPEG hashes match for every completed
comparison. The same input and output hashes also match across all four lanes.
Offline verification independently recomputed the raw quartet statistics,
stability guards, preferences and summary verdicts, and checked actual input,
JPEG and incremental snapshot files before retention. All correctness,
automation, deployment, reproducibility and package jobs passed. The overall
workflow failed its performance diagnostics; no timing bound was relaxed and
no unsuccessful row was discarded.

## Interpretation and retained evidence

A valid row passes every fixed guard: at least 1 ms per leg, at most 10%
relative MAD, 20% p90/p10 drift, 10% repeated-leg drift within a quartet and
10% order bias. A strong preference additionally requires all six paired
ratios to clear the same side of 1 +/- 0.005. Valid rows with mixed or smaller
effects remain inconclusive; they do not establish equivalence. Invalid rows
cannot establish a speed benefit or a regression.

Paired ratios are candidate/baseline: each quartet divides the candidate's
geometric-mean latency by the baseline's, and the reported row statistic is
the median of the six ratios. Different hosted CPUs prevent attributing
differences between framework lanes to the runtime alone.

Each lane retains both selections' run metadata, complete cases with all raw
quartet legs, summaries, source hashes, skipped comparisons and reports.
[Provenance](provenance.json) records the hosted artifact IDs and SHA-256 hashes
of all retained files, normalized to UTF-8 with LF newlines. Exact inputs,
JPEGs and incremental raw snapshots remain in hosted artifacts, subject to
retention. Allocation totals are distinct from pool capacity and peak memory.

This is a selected-case scheduling assessment. It does not establish native
performance parity, a fastest choice for every input, or a universal crossover.
