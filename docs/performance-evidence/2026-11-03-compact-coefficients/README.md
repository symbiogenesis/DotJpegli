# Bounded compact coefficient-cache comparison

The representation investigation is complete for two executable immutable
prototypes: nonzero bitmaps with packed shorts, and per-frequency bit-packed
tiles. Both retain exact full-block, direct-DC and individual progressive
coefficient access with dense tile fallback. Actual payload and index rentals
demonstrate substantial capacity gains on the measured real coefficients, and
metadata counterexamples prevent a universal storage claim. Production
`QuantizedImage` retains its existing dense storage and 64 MiB limit.

This is an untimed storage/work study. It makes no latency, allocated-byte,
construction-overlap, peak-memory, native-parity or complete-encode performance
claim. Compact storage is confined to the performance tool and focused tests;
encoder code, public APIs and default JPEG bytes are unchanged.

## Layouts and cost model

The [codecs](../../../tools/DotJpegli.Performance/CoefficientCacheRepresentations.cs)
use tiles of at most 32 blocks. All layouts own one pooled byte payload.
The two compact layouts also own a pooled `int[]` tile index; a complemented
offset denotes a dense tile. Every accepted rental's actual length counts
against the fixed 64 MiB cap, including pool tails and index capacity. Managed
object/array headers and stack scratch are outside this payload budget, as are
the current encoder's array headers. Arrays are returned on rejection, failure,
cancellation and disposal; the source coefficients are not retained.

- Dense stores all 64 signed shorts per block: 128 useful bytes.
- Bitmap tiles store direct DC shorts, one 63-bit AC presence mask and one
  16-bit packed-value offset per block, followed by the nonzero signed shorts.
  With `b` blocks and `n` nonzero AC values, payload size is `12*b + 2*n` bytes.
  Individual nonzero lookup uses one population count of preceding mask bits.
  Full-block expansion traverses the mask without population counts.
- Bit-packed tiles store direct DC shorts, 63 band widths, 63 16-bit band bit
  offsets, then the packed unsigned codes. The signed mapping is
  `(value << 1) ^ (value >> 15)`; all signed-short values fit in 0..16 bits.
  Each band uses its tile's maximum code width. Payload size is
  `2*b + 189 + ceil(b*sum(widths)/8)` bytes. Individual lookup needs no rank
  traversal. The largest band's end is at most 32,256 bits, within the stored
  offsets' range.

Compact layouts choose dense payload for a tile when compact useful bytes are
at least its dense useful bytes. The shared compact tile index remains charged,
including when every tile falls back. This local choice does not optimize the
whole image's pool bucket boundary. Both forced-compact and fallback choices
are retained in the component/adversarial reports.

Planning scans every AC value before rental; packing recomputes tile plans.
The [work counters](results/cases.json) retain planning/packing reads, writes,
index resolutions, population counts and explicitly charged bit operations,
plus separate full-block, direct-DC and individual-coefficient passes.
Reads/writes count source/destination short elements, payload bytes and index
elements, without assigning a common time cost. Scratch planning, loop control
and counter updates are excluded. They are logical operation counts, not
machine instructions or throughput predictions.

The common offline Dense codec packs an existing source into bytes and copies
blocks out. Production dense storage is filled directly by DCT and returns a
span without these copy passes. Therefore even the exact prototype counter
ratios do not quantify production overhead or establish deployment speed.

## Real coefficient measurements

[Run metadata](results/run.json), [all component results](results/cases.json)
and the [summary](results/summary.json) retain executable, source-pixel and
little-endian coefficient SHA-256 hashes and all settings. The selection uses
Kodak `kodim01` at 768 by 512, smooth-gradient, low-contrast, low-noise and
full-noise controls at 257 by 193, and tiny odd edges at 19 by 17. Generated
inputs use the existing deterministic `ControlledCorpus` definitions.

Every source uses distances 1 and 4, 4:4:4 and 4:2:0, AQ, baseline-bounded
perceptual tables, smoothing zero and zigzag-ordered coefficients from the
existing completed `QuantizedImage`. Original narrow zigzag preference is
explicit. This produces 24 image configurations, 72 component arrays and 360
layout comparisons. Every block, direct DC value and all 64 individual lookup
values compare exactly with the original cache. The host was Windows ARM64,
.NET `11.0.0-rc.3.26522.148`.

Selected rows below sum all three components' **actual rented bytes**, with
dense fallback enabled. Every Dense byte count also matches an actual rental
from the production `ArrayPool<short>`; compact counts include pooled indexes.

| Source | Distance | Sampling | Dense | Bitmap | Bit-packed |
| --- | ---: | --- | ---: | ---: | ---: |
| Photograph | 1 | 4:4:4 | 3,145,728 | 789,504 | 396,288 |
| Photograph | 4 | 4:2:0 | 1,572,864 | 329,216 | 165,376 |
| Smooth gradient | 1 | 4:4:4 | 393,216 | 49,536 | 24,960 |
| Full noise | 1 | 4:4:4 | 393,216 | 295,296 | 164,224 |
| Full noise | 1 | 4:2:0 | 196,608 | 180,480 | 90,368 |
| Tiny odd edges | 1 | 4:2:0 | 3,072 | 1,728 | 2,240 |

Bit packing uses fewer rented bytes than bitmap on most selected cases; bitmap
wins on the small distance-1 4:2:0 edge case. Pool bucket effects matter beyond
useful payload size. No measured real tile requires dense fallback; the
adversarial measurements below exercise it.

The photograph's distance-1 4:4:4 luma component illustrates access tradeoffs:

| Layout | Packing reads | Full-block pass reads | Individual pass reads | Individual rank calls | Individual bit operations |
| --- | ---: | ---: | ---: | ---: | ---: |
| Dense | 393,216 | 786,432 | 786,432 | 0 | 0 |
| Bitmap | 1,167,360 | 420,436 | 4,183,208 | 170,282 | 944,426 |
| Bit-packed | 1,620,192 | 1,632,480 | 2,019,552 | 0 | 3,425,376 |

Both compact DC passes read one tile-index element and two payload bytes per
block, without decoding AC. Bitmap block expansion reduces payload reads here,
while individual AC lookups repeatedly read masks. Bit packing trades smaller
storage for more bit extraction. Workload scan counts and runtime costs would
be needed to turn these observations into latency estimates.

## Cap admission and counterexamples

[Capacity results](results/capacity.json) repeat all three real component arrays
by integral factors, preserving their original coefficient distributions and
component ratios. Each pair uses the last repetition whose production rental
buckets fit 64 MiB and the next repetition. Actual per-component production
rental lengths are checked against the bucket plan. Prototype components share
one 64 MiB remaining budget. Admitted components check 129 evenly spaced blocks,
their DC values and all individual coefficients. These are **replicated
coefficient sets**, rather than fresh large-image encodes or an extrapolation of
their quality, latency or DCT/AQ edge behavior.

At the first repetition rejected by dense storage, all four real replicas are
admitted by both compact layouts:

| Repeated source | Distance / sampling | Repetitions | Required dense MiB | Bitmap accepted MiB | Bit-packed accepted MiB |
| --- | --- | ---: | ---: | ---: | ---: |
| Photograph | 1 / 4:4:4 | 22 | 96 | 20.09375 | 12.09375 |
| Smooth gradient | 1 / 4:4:4 | 159 | 96 | 6.09375 | 5.09375 |
| Full noise | 1 / 4:4:4 | 159 | 96 | 36.09375 | 20.09375 |
| Full noise | 4 / 4:2:0 | 318 | 96 | 22.09375 | 21.09375 |

The three-component 4:4:4 dense bucket totals jump from 48 to 96 MiB;
the last 4:2:0 noise replica fits exactly 64 MiB. These storage admissions show
that compaction can keep additional coefficient sets below the fixed cap after
charging metadata and actual pool tails. They do not show that packing and
lookup time is repaid by avoided DCT passes.

[Adversarial results](results/adversarial.json) compare 75 additional layouts on
1, 31, 32, 33 and 67 blocks with zero AC, only coefficient 63, and full-width
signed AC. A 32-block full-width tile rents 4,096 bytes dense, 8,256 bytes in
either forced compact layout, and 4,160 bytes with either compact fallback.
The remaining 64 bytes are the index pool's minimum bucket. One zero-AC block
requires 128 bytes dense, 80 bytes bitmap, 320 bytes forced bit-packed and 192
bytes with bit-packed fallback. Tiny and incompressible inputs defeat a
uniform storage advantage.

A separate full-width synthetic grayscale capacity pair makes the cap cost
explicit: 524,288 blocks fit dense at exactly 64 MiB, but both compact fallback
layouts reject the set because their tile indexes exceed the same budget.
The next 32 blocks cause all three layouts to reject. Distance `0` and sampling
`400` in these two rows label synthetic coefficients, not an encoder setting.
All 30 layout admission attempts, successes and failures remain in the report.
Rejected rows retain accepted-prefix costs and every attempted rental length;
they do not present partial costs as complete rejected-image costs.

## Validation and decision

Release builds of the tool and focused tests pass on .NET 10 and .NET 11 with
zero warnings. All 24 focused codec cases pass on each target:
[.NET 10 results](tests/net10.xml), [.NET 11 results](tests/net11.xml).
They independently check exact small-case counters, signed extrema, coefficient
63, partial tiles, shuffled progressive queries, input mutation after packing,
dirty pool tails, oversized rentals, rejection, injected rental failure,
cancellation, invalid access and idempotent disposal. Changed-file whitespace
verification passes; malformed command arguments and stale output directories
are rejected. No full suite or benchmark was run locally.

The bounded comparison resolves representation feasibility and storage costs.
Both candidates preserve values and improve cap admission for the measured real
replicas, while dense fallback still incurs index costs and neither candidate
dominates every input. **Retain dense production storage.** Deployment needs
evidence of end-to-end time and construction memory costs; these untimed counts
provide neither. The completed study does not alter native acceptance bounds.

To reproduce with the pinned SDK and a prepared Kodak `kodim01.ppm`:

```sh
dotnet run --project tools/DotJpegli.Performance -c Release -f net11.0 -- \
  --coefficient-cache-analysis --photograph /absolute/kodim01.ppm \
  --output /absolute/empty-evidence-directory
```

The analysis has no clocks, calibration, warmup or benchmark calls. Photographs
are bounded to 1024 by 1024; output must be empty. All timed benchmark commands
retain their CI guard.
