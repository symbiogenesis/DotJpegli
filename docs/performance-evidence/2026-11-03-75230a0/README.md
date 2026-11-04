# Complete native measurements at 75230a0

Revision `75230a072dd31bda896e3ed2706aeb52ac968975` was measured in
[CI run 41752676723](https://github.com/symbiogenesis/DotJpegli/actions/runs/41752676723)
on November 3, 2026 (America/Chicago; November 4 UTC). This full comparison was
explicitly requested after the revision's automatic run measured only four
targeted cases. All 256 performance cases are complete, matched and valid under
the fixed protocol, but full performance parity **fails**. The same run's
complete 1,280-pair quality/compression comparison passes.

The latency step fails because valid measurements exceed the fixed acceptance
bounds. Chart generation succeeds. The overall workflow therefore fails; the
retained [job outcomes](job.json) distinguish measurement validity, performance
acceptance and successful chart generation. No timing retry was needed.

| Corpus | Sampling | Sequential mean / worst | Progressive level 2 mean / worst |
| --- | --- | ---: | ---: |
| Kodak photographs | 4:4:4 | 1.180759 / 1.383027 | 0.898726 / 0.976977 |
| Kodak photographs | 4:2:0 | 1.157399 / 1.405034 | 0.893689 / 1.009467 |
| Deterministic controls | 4:4:4 | 1.223892 / 1.814946 | 1.104170 / 1.531142 |
| Deterministic controls | 4:2:0 | 1.117442 / 1.508274 | 1.098329 / 1.395222 |

Ratios are managed/native median encoding latency; means are geometric means
over every case in each group. Lower is better. Photograph sequential groups
use **15.7–18.1% more time** than native, while progressive groups use
**10.1–10.6% less**. Both progressive photograph groups pass the fixed 1.05
group and 1.10 individual bounds. The other six groups fail. There are 109
cases above the individual bound; the worst ratio is 1.814946 for
smooth-gradient, 4:4:4, sequential, distance 4. No aggregate result overrides
an individual failure.

The host is AMD EPYC 7763 x64, running .NET `11.0.0-rc.3.26522.148`, with eight
float SIMD lanes, accelerated Vector256 and no Vector512. One worker is pinned
to one CPU, and tiered compilation is disabled. The scope is RGB8 to YCbCr,
Kodak24 plus eight controls, distances 1/4, 444/420 and progression 0/2, using AQ
and optimized Huffman tables. Every median includes all seven alternating-order
blocks; natural GC remains included and no outliers are discarded. Separate
workflow runs do not isolate revision speedups or replace the interleaved
released-version regression protocol.

## Retained evidence

- [Performance report](full/report.md), [summary](full/summary.json), [raw samples](full/cases.json), [run identity](full/run.json) and [source hashes](full/sources.json).
- [Quality/compression report](quality/report.md), [summary](quality/summary.json), [raw cases](quality/cases.json), [matched-rate points](quality/matched-rates.json), [matched-quality points](quality/matched-quality.json) and [source hashes](quality/sources.json).
- [Native build provenance](native-provenance.json), [job outcomes](job.json) and [artifact identities/retained-file hashes](provenance.json).
- Exactly regenerated [chart values](charts/metrics.json), [compression](charts/compression.svg), [wall time](charts/wall-time.svg) and [allocations](charts/allocations.svg).

The chart generator validated both complete source matrices and regenerated
all four chart files locally. Each matches the hosted chart artifact byte for
byte, as do the README chart copies. Local work only built the automation tool,
ran its synthetic checks, and copied and analyzed existing CI evidence; no
benchmark ran locally.

Compression summarizes photograph group means at matched PSNR and SSIM. The
largest absolute group difference, rounded up to the next 0.001%, is ≤0.002%.
The largest individual matched-quality size overhead, including controls and
all retained targets, is 0.087051%. The quality comparison retains 1,280 encoding
pairs, 2,057 matched-rate targets and 4,396 matched-quality targets. Raw cases,
matched-rate/quality points and source hashes match the earlier
[`b07eb5d` quality evidence](../../parity-evidence/2026-11-03-b07eb5d/summary.json)
byte for byte; the latest summary records the new revision and assembly identities.

Allocation bars show mean managed allocation traffic per complete byte-array
encode, including the returned JPEG. Native allocations, retained pool capacity
and peak memory were not measured. Exact inputs, JPEGs, executable files and
native logs remain in the hosted artifacts; retained JSON paths refer to those
artifact layouts. GitHub-reported archive digests and locally calculated
retained-file hashes are distinguished in the provenance.
