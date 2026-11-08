# Complete native measurements at 373851b

Revision `373851b426633754ca4137a8b0960c4f41113447` was measured in
[CI run 42183646421](https://github.com/symbiogenesis/DotJpegli/actions/runs/42183646421)
on November 6, 2026 (America/Chicago and UTC). All 256 performance cases are
complete, matched and valid under the fixed protocol, but full performance
parity **fails**. The same run's complete 1,280-pair quality/compression
comparison and 192-case untimed allocation observation both pass.

The latency step fails because valid control measurements exceed the fixed
individual acceptance bound. Chart generation succeeds. The overall workflow
therefore fails; the retained [job outcomes](job.json) distinguish measurement
validity, performance acceptance and successful chart generation.

| Corpus | Sampling | Sequential mean / worst | Progressive level 2 mean / worst |
| --- | --- | ---: | ---: |
| Kodak photographs | 4:4:4 | 0.839553 / 1.017118 | 0.704168 / 0.756536 |
| Kodak photographs | 4:2:0 | 0.827390 / 1.084435 | 0.696093 / 0.787409 |
| Deterministic controls | 4:4:4 | 0.964347 / 1.321672 | 0.916086 / 1.348560 |
| Deterministic controls | 4:2:0 | 0.899082 / 1.371160 | 0.931425 / 1.370675 |

Ratios are managed/native median encoding latency; means are geometric means
over every case in each group. Lower is better. Photograph sequential groups
use **16.0–17.3% less time** than native, while progressive groups use
**29.6–30.4% less**. All four photograph groups pass both the fixed 1.05 group
and 1.10 individual bounds. All four deterministic control groups meet the
1.05 group bound (using 3.6–10.1% less time on average) but exceed the 1.10
individual bound. Overall, four of eight groups fail and 25 cases exceed the
1.10 individual bound; the worst ratio is 1.371160 for `tiny-odd-edges`, 4:2:0,
sequential, distance 1. No aggregate result overrides an individual failure.

The latency host is AMD EPYC 9V45 x64, running .NET `11.0.0-rc.3.26522.148`,
with eight float SIMD lanes and accelerated Vector512 (`Vector512` true). One
worker is pinned to one CPU, and tiered compilation is disabled. The scope is
RGB8 to YCbCr, Kodak24 plus eight controls, distances 1/4, 444/420 and
progression 0/2, using AQ and optimized Huffman tables. Every median includes
all seven alternating-order blocks; natural GC remains included and no outliers
are discarded.

## Allocations per encode

The same CI run collected all **192 photograph configurations** in the untimed
allocation job on an Intel Xeon Platinum 8370C x64 runner:

| Encoding | DotJpegli managed GC bytes, KiB/encode | Google C++ jpegli requested heap bytes, KiB/encode |
| --- | ---: | ---: |
| Sequential, 4:4:4 | 110.2 | 1924.8 |
| Sequential, 4:2:0 | 98.8 | 2190.1 |
| Progressive level 2, 4:4:4 | 146.3 | 4288.3 |
| Progressive level 2, 4:2:0 | 134.8 | 3209.3 |

Each value is an arithmetic mean across all 24 Kodak photographs and distances
1/4, with three complete encode samples per case following five warmups.
DotJpegli reports precise `GC.GetTotalAllocatedBytes` deltas; Google C++ jpegli
reports requested heap bytes through the GNU linker-wrapped allocation probe.
Both include the returned JPEG and exclude prepared input, native allocator
metadata, retained pool capacity and peak memory.

## Retained evidence

- [Performance report](full/report.md), [summary](full/summary.json), [raw samples](full/cases.json), [run identity](full/run.json) and [source hashes](full/sources.json).
- [Quality/compression report](quality/report.md), [summary](quality/summary.json), [raw cases](quality/cases.json), [matched-rate points](quality/matched-rates.json), [matched-quality points](quality/matched-quality.json) and [source hashes](quality/sources.json).
- [Allocation summary](allocations/summary.json), [raw allocation samples](allocations/cases.json), [corpus manifest](allocations/corpus.json) and [allocation native provenance](allocations/native-provenance.json).
- [Native build provenance](native-provenance.json), [job outcomes](job.json) and [artifact identities/retained-file hashes](provenance.json).
- Regenerated [chart values](charts/metrics.json), [compression](charts/compression.svg), [wall time](charts/wall-time.svg) and [allocations](charts/allocations.svg).

Compression summarizes photograph group means at matched PSNR and SSIM. Every
photograph group mean is slightly smaller than native; the largest absolute
group difference, rounded up to the next 0.001%, is ≤0.032%. The largest
individual matched-quality size overhead, including controls and all retained
targets, is 0.363636%. The quality comparison retains 1,280 encoding pairs,
2,850 matched-rate targets and 4,396 matched-quality targets.

Regenerate all charts from the retained evidence without running a benchmark:

```powershell
dotnet run --project tools/DotJpegli.Automation -c Release -- performance-charts `
  --performance docs/performance-evidence/2026-11-06-373851b/full `
  --quality docs/performance-evidence/2026-11-06-373851b/quality `
  --allocations docs/performance-evidence/2026-11-06-373851b/allocations `
  --output .artifacts/regenerated-readme-charts
```
