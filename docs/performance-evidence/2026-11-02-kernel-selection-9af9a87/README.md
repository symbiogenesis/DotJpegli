# Interleaved kernel selection evidence

[Automatic run 41581024412](https://github.com/symbiogenesis/DotJpegli/actions/runs/41581024412)
tests `9af9a87d68d59f755a517c7e717435f7b21a98a8`. It superseded the cancelled
`1287dc7` automatic run while preserving the requested comparison marker.

Each pair changes one immutable per-encode preference (Original/Optimized), with
the other Automatic. Six balanced ABBA/BAAB quartets run in one process on CPU 0,
tiered compilation disabled, five warmups per leg, 20 encodes per leg (100 below
1,024 pixels). Original, optimized and automatic JPEG hashes match in every case.
The inputs are Kodak01, low contrast, low noise and tiny odd edges; distances
1/4, sampling 444/420 and progression 0/2. ARM64 skips unreachable narrow-ordering
pairs instead of claiming to exercise them.

| Lane | CPU | AQ valid / measured | Narrow valid / measured | Strong narrow Tiled preferences |
| --- | --- | ---: | ---: | ---: |
| [ARM64 .NET 10](ARM64-net10/report.md) | ARM implementer 0x41, part 0xd49 | 26 / 32 | skipped | 0 |
| [ARM64 .NET 11](ARM64-net11/report.md) | ARM implementer 0x41, part 0xd49 | 27 / 32 | skipped | 0 |
| [x64 .NET 10](X64-net10/report.md) | AMD EPYC 9V74 | 25 / 32 | 26 / 32 | 10 |
| [x64 .NET 11](X64-net11/report.md) | Intel Xeon Platinum 8573C | 27 / 32 | 23 / 32 | 7 |

Each complete timing summary is **invalid** because at least one row fails its
predeclared stability guards. Only individual valid rows support the observations
below. All failures remain in the raw data and reports; no bound was relaxed.
Correctness, fallback, deployment, reproducibility and package jobs passed.

## Dispatch decision

Every one of the 42 valid non-tiny narrow-ordering comparisons favors Tiled by
case median. Seventeen also meet the stronger criterion: all six paired ratios
must clear a 0.5% margin in the same direction, with all stability guards passing.
The medians across all valid narrow rows are about 0.9833 for both x64 lanes.
These runs use actual accelerated 256-bit SIMD, eight float lanes and disabled
512-bit SIMD. Framework and CPU differ between lanes; runtime effects are not
isolated from processor effects.

Tiny 19x17 inputs are inconclusive and contain every valid narrow-ordering
regression: .NET 10/420/p2/d4 is 1.023019; .NET 11/444/p2/d1 is 1.000729;
.NET 11/420/p2/d1 is 1.008734. Those samples are retained, not dismissed.

The deployment policy therefore uses Tiled on x64 with the measured vector
configuration, both dimensions at least eight and at least 32,768 source pixels.
The 32K-pixel floor is a **provisional minimum-work guard**, not an observed
crossover: measured source areas were 323, 49,601 and 393,216 pixels. More data
are needed around the floor and for different aspect ratios. Other configurations
keep Original, and automatic 512-bit dispatch remains unchanged. The choice is
captured in each prepared quantizer; there is no per-block CPU query or startup
benchmark. The shared DC-only shortcut remains available in both kernels.

AQ stays on Original automatically. The network is promising on eight-lane
.NET 11 (all 27 valid medians favor it; three strong individual preferences),
but sampling and width do not establish a defensible portable selection rule.
ARM64 and x64/.NET 10 comparisons remain inconclusive. Original is a fallback
under uncertainty; these results do not prove it faster.

## Retained scope

Each lane contains run identity, summary, complete raw cases, source hashes,
skips and its report. Per-quartet legs include times, allocation totals and GC
counts. Guards reject subminimum intervals, MAD, repeated-leg drift, minority
timing states and order bias. Whole-case output equality is checked outside the
timed interval. Allocations are not pool capacity or peak memory measurements.

[Provenance](provenance.json) records the artifact IDs and normalized file hashes.
Exact inputs/JPEGs and incremental raw snapshots remain in hosted artifacts,
subject to artifact retention. A subsequent `--include-automatic` run compares
the deployed policy against both Original, including cases where the fallback
selects no new kernel. These are selected-case diagnostics, not full JPEGli
performance parity or a guarantee of the fastest choice on every call.
The snapshot date uses America/Chicago; UTC is November 3.
