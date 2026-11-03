# Complete native reference at b07eb5d

Revision `b07eb5d240c2be1f6b9aebac13d081fb50d740cd` was measured on November 3,
2026 in [automatic CI run 41690962313](https://github.com/symbiogenesis/DotJpegli/actions/runs/41690962313).
All 256 performance cases are complete, matched and valid under the fixed
protocol, but full performance parity **fails**. The same run's complete
1,280-pair quality/compression comparison passes.

These measurements precede the later segmented/partial converted-plane change.
They do not establish the performance of that subsequent implementation.
The same revision also **fails three of six released-version regression gates**,
with 73 of 1,536 selected cases consistently favoring released 1.0.0. The
[release snapshot](release/README.md) retains every selected case and all 254
earlier invalid trials. The overall workflow also had a formatting failure;
these are the retained measurement jobs' results, not a claim that the whole
workflow passed.

| Corpus | Sampling | Sequential mean / worst | Progressive level 2 mean / worst |
| --- | --- | ---: | ---: |
| Kodak photographs | 4:4:4 | 1.240736 / 1.581874 | 0.929983 / 1.017882 |
| Kodak photographs | 4:2:0 | 1.180139 / 1.425277 | 0.902195 / 1.026717 |
| Deterministic controls | 4:4:4 | 1.255787 / 1.612267 | 1.118154 / 1.388623 |
| Deterministic controls | 4:2:0 | 1.129530 / 1.523228 | 1.103792 / 1.386591 |

Ratios are managed/native median encoding latency; means are geometric means
over each group. Lower is better. Photograph sequential groups use 18.0–24.1%
more time than native, while progressive groups use 7.0–9.8% less. Both
progressive photograph groups pass the fixed 1.05 group and 1.10 individual
bounds. The other six groups fail. The worst individual ratio is 1.612267 for
smooth-gradient, 4:4:4, sequential, distance 1. No aggregate result overrides
an individual failure.

The host is AMD EPYC 7763 x64, running .NET `11.0.0-rc.3.26522.148`, with eight
float SIMD lanes, accelerated Vector256 and no Vector512. The protocol pins
one worker to one CPU and disables tiered compilation. It compares RGB8 to
YCbCr at distances 1/4, 444/420 and progression 0/2, using AQ and optimized
Huffman tables. Every median includes all seven alternating-order blocks;
natural GC remains included and no outliers are discarded.

The [previous complete reference](../2026-11-03-d5f3226/README.md) also reports
an EPYC 7763, but the runs were collected separately. These native-relative
results do not isolate the effect of source changes or replace the same-host
interleaved released-version regression protocol.

## Retained evidence

- [Performance report](full/report.md), [summary](full/summary.json), [raw samples](full/cases.json), [run identity](full/run.json) and [source hashes](full/sources.json).
- [Quality/compression report](../../parity-evidence/2026-11-03-b07eb5d/report.md), [summary](../../parity-evidence/2026-11-03-b07eb5d/summary.json), [matched-quality points](../../parity-evidence/2026-11-03-b07eb5d/matched-quality.json), [artifact identity](../../parity-evidence/2026-11-03-b07eb5d/snapshot.json) and [retained-file hashes](../../parity-evidence/2026-11-03-b07eb5d/hashes.json).
- [Native provenance](native-provenance.json) and [artifact identities/file hashes](provenance.json).
- [Six-lane released-version comparison](release/README.md), [derived case analysis](release/analysis.json) and [release artifact identities/file hashes](release/provenance.json).
- Exact locally regenerated [chart values](charts/metrics.json), [compression](charts/compression.svg), [wall time](charts/wall-time.svg) and [allocations](charts/allocations.svg).
- Separate [application workload observations](../2026-11-03-b07eb5d-workloads/README.md), which are instrumented and do not contribute to native acceptance.

Charts were regenerated from this run's matching performance and quality data,
and all four files match the hosted chart artifact byte for byte. The current
README copies match these retained files. Local work only copied and analyzed
existing CI evidence; no benchmark ran locally.

Compression summarizes photograph group means at matched PSNR and SSIM. The
largest absolute group difference, rounded up to the next 0.001%, is ≤0.002%.
The largest individual matched-quality size overhead, including controls and
all retained targets, is 0.087051%. The full comparison retains 1,280 encoding
pairs, 2,057 matched-rate targets and 4,396 matched-quality targets.

Allocation bars show mean managed allocation traffic per complete byte-array
encode, including the returned JPEG. Native allocation, retained pool capacity
and peak memory were not measured by this protocol. Input/JPEG files and the
complete native logs remain in the hosted artifacts; paths within the retained
JSON refer to those artifact layouts. GitHub-reported archive digests and
locally calculated retained-file hashes are distinguished in the provenance.
