# Complete native measurements at 0104778

[Automatic CI run 41695349570](https://github.com/symbiogenesis/DotJpegli/actions/runs/41695349570)
measured revision `0104778b2691cf3e61d7dbac280457f383d9bc0c` on November 3,
2026 (America/Chicago), after segmented/partial converted-plane caching.
All 256 native comparison cases completed and matched JPEG structures, but the
overall native performance verdict is **invalid**. Kodak13, 4:2:0, sequential,
distance 1 has managed relative MAD **20.54%**, above the fixed 10% noise bound.
The remaining 255 cases have no invalidity reasons. The invalid case remains in
the evidence; its group mean cannot establish timing acceptance.

The same job's quality/compression comparison step succeeded. The latency and
chart-generation steps failed; the retained [job outcomes](full/job.json) do
not claim that the overall workflow passed. This run did not replace the
preceding complete valid [b07eb5d snapshot](../2026-11-03-b07eb5d/README.md)
in the README charts. No native chart was regenerated from this invalid run.
The later valid [75230a0 snapshot](../2026-11-03-75230a0/README.md) supplies
the current README charts.

| Corpus | Sampling | Sequential mean / worst | Progressive level 2 mean / worst |
| --- | --- | ---: | ---: |
| Kodak photographs | 4:4:4 | 1.166935 / 1.354531 (fail) | 0.896545 / 0.972143 (pass) |
| Kodak photographs | 4:2:0 | 1.158260 / 1.414206 (**invalid**) | 0.888509 / 1.001192 (pass) |
| Deterministic controls | 4:4:4 | 1.204068 / 1.544175 (fail) | 1.092364 / 1.380272 (fail) |
| Deterministic controls | 4:2:0 | 1.104980 / 1.518174 (fail) | 1.098814 / 1.375819 (fail) |

Ratios are managed/native median encoding time; means are geometric means of
all cases in each group. Lower is better. Both progressive photograph groups
are valid and pass the fixed bounds, using **10.3–11.1% less time** than native
in their group means. The valid 4:4:4 sequential photograph group uses **16.7%
more time** and fails. The 4:2:0 sequential mean includes the invalid row and is
descriptive only. All four control groups are valid and fail. Among valid
cases, 105 exceed the individual 1.10 ratio bound. These results do not establish
full native performance parity or isolate gains from revision changes.

The host is AMD EPYC 7763 x64, running .NET `11.0.0-rc.3.26522.148`, with
eight float SIMD lanes, accelerated Vector256 and no Vector512. One worker is
pinned to one CPU, and tiered compilation is disabled. The fixed scope is
RGB8 to YCbCr, Kodak24 plus eight controls, distances 1/4, sampling 444/420,
progression 0/2, AQ and optimized Huffman tables. Medians include all seven
alternating-order blocks; natural GC remains included and no outliers are
discarded. Separate workflow runs do not provide a controlled revision comparison.

The [released-version comparison](release/README.md) retains the six-lane
same-host comparison against 1.0.0 from this run. Separate
[workload observations](../2026-11-03-0104778-workloads/README.md) retain all
26 scenarios on x64 and ARM64.

## Retained evidence

- [Performance report](full/report.md), [summary](full/summary.json), [raw cases](full/cases.json), [run identity](full/run.json) and [input hashes](full/sources.json).
- [Native build provenance](full/native-provenance.json), [artifact metadata](full/artifact.json), [job outcomes](full/job.json) and [retained-file hashes](full/provenance.json).

Files were copied from existing [artifact 13261376682](https://github.com/symbiogenesis/DotJpegli/actions/runs/41695349570/artifacts/13261376682).
Exact inputs, encoded images, executable files and native logs remain in the
hosted artifact; raw JSON paths refer to that layout. GitHub-reported archive
digests and locally calculated retained-file hashes are distinguished in the
provenance. Local work only copied and analyzed existing CI evidence; no
benchmark ran locally.
