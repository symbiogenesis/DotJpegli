# Released 1.0.0 comparison at b07eb5d

[Automatic CI run 41690962313, attempt 1](https://github.com/symbiogenesis/DotJpegli/actions/runs/41690962313/attempts/1)
compares `b07eb5d240c2be1f6b9aebac13d081fb50d740cd` with released 1.0.0 source
`a40868bfdace7d737413085522a88d351aa16782`, rebuilt with the candidate SDK.
**Three of six release regression gates fail.** All 1,536 selected comparisons
are complete and valid, and all paired JPEG hashes match. There are 73
confirmed regressions, 1,146 consistent improvements and 317 inconclusive
differences under the fixed 0.5% preference margin.

This snapshot precedes the subsequent segmented/partial converted-plane and
hot-path changes. It does not establish their performance. Earlier passing
[74ae25d](../../2026-11-03-release-regression-74ae25d/README.md) and
[a045052](../../2026-11-03-release-regression-a045052/README.md) results remain
historical evidence for those revisions. Separate hosted runs, including runs
on different CPU models, do not isolate the effect of source changes.

| Lane | Actual CPU | Vector512 | Regressions | Inconclusive | Selected geometric mean ratio | Largest selected median ratio |
| --- | --- | --- | ---: | ---: | ---: | ---: |
| [ARM64 .NET 10](ARM64-net10.0-default/report.md) | ARM implementer 0x41, part 0xd49 | false | **5** | 53 | 0.951604 | 1.013253 |
| [ARM64 .NET 11](ARM64-net11.0-default/report.md) | ARM implementer 0x41, part 0xd49 | false | 0 | 66 | 0.946727 | 1.005004 |
| [x64 .NET 10, AVX2](X64-net10.0-avx2/report.md) | Intel Xeon 6973P-C | false | 0 | 23 | 0.897711 | 1.003270 |
| [x64 .NET 10, default](X64-net10.0-default/report.md) | AMD EPYC 7763 | false | **59** | 68 | 0.946960 | 1.050793 |
| [x64 .NET 11, AVX2](X64-net11.0-avx2/report.md) | AMD EPYC 9V45 | false | 0 | 17 | 0.885725 | 1.002311 |
| [x64 .NET 11, default](X64-net11.0-default/report.md) | AMD EPYC 9V74 | true | **9** | 90 | 0.919787 | 1.013482 |

Ratios are candidate/released-source baseline; lower is better. Each geometric
mean summarizes all 256 selected paired medians in that lane. Group means
below one do not override individual failures. The largest selected median
ratio, 1.050793, is Kodak02, 4:4:4, sequential, distance 1 on .NET 10 x64
default. A slightly slower median alone is insufficient to establish a
regression: every quartet must exceed 1.005 with all timing guards passing.
Inconclusive results do not prove equal speed or rule out smaller differences.

## Sampling and invalid attempts

The fixed matrix uses Kodak24 plus eight deterministic controls, 444/420,
progression 0/2 and distances 1/4. Persistent baseline and candidate workers
run serially on the same host and CPU affinity in six balanced ABBA/BAAB
quartets per case, with five untimed warmups and 20 timed encodes per leg
(100 for the tiny input). Tiering is disabled and natural GC remains included.
Runtime, SIMD configuration and build provenance are recorded separately for
each lane. The label "default" does not guarantee Vector512 hardware.

| Lane | Initially invalid cases | Earlier invalid trials | Total attempts | Maximum selected attempt |
| --- | ---: | ---: | ---: | ---: |
| ARM64 .NET 10 | 2 | 2 | 258 | 2 |
| ARM64 .NET 11 | 5 | 5 | 261 | 2 |
| x64 .NET 10, AVX2 | 82 | 138 | 394 | 7 |
| x64 .NET 10, default | 24 | 27 | 283 | 3 |
| x64 .NET 11, AVX2 | 45 | 55 | 311 | 4 |
| x64 .NET 11, default | 23 | 27 | 283 | 4 |

The existing timing guards permit at most eight complete attempts. The first
valid trial is final, including regressions and inconclusive differences;
valid slower results are never retried. Identity, JPEG, structure, nonfinite
and too-short failures cannot be retried. No fastest-trial selection, sample
merging, outlier removal or relaxed acceptance threshold is used.

Across the six lanes, 181 cases were initially invalid. All 1,790 attempts are
retained: 1,536 selected trials and **254 earlier invalid trials**. The result
is conditioned on finding a stable trial; it does not describe an initially
stable matrix or the unconditional latency distribution. Conditioning on
stability can favor quieter intervals, including intervals without GC. The
especially high retry count on .NET 10 AVX2 limits what its selected summary
says about routine latency variation.

## Retained evidence and provenance

Every lane retains `cases.json`, `summary.json`, `report.md`, `run.json` and
`sources.json`. The selected cases include all raw quartets, all attempt
analyses and selected attempt numbers. Every earlier invalid trial also has
its original file under `raw-comparisons/`, so rejected-trial links resolve
locally. Selected-trial raw files are not duplicated because their quartet
data is already in `cases.json`; their original `rawFile` references are
available in the hosted artifacts. Each lane's `build-provenance/` retains
the SDK/build record and both restored package lock files.

[Derived analysis](analysis.json) lists every confirmed regression and lane
summary. [Provenance](provenance.json) records all six artifact identities,
GitHub-reported archive digests, and local SHA-256 hashes for all retained
artifact files. The 42,960 selected and earlier-invalid raw measurement-leg
hashes were checked against the paired case JPEG hash. Inputs and duplicate
selected raw files remain in the hosted archives. Artifact archive digests
were obtained from GitHub and were not independently recomputed.

- [ARM64 .NET 10 artifact 13259250606](https://github.com/symbiogenesis/DotJpegli/actions/runs/41690962313/artifacts/13259250606)
- [ARM64 .NET 11 artifact 13260615194](https://github.com/symbiogenesis/DotJpegli/actions/runs/41690962313/artifacts/13260615194)
- [x64 .NET 10 AVX2 artifact 13260380604](https://github.com/symbiogenesis/DotJpegli/actions/runs/41690962313/artifacts/13260380604)
- [x64 .NET 10 default artifact 13256924986](https://github.com/symbiogenesis/DotJpegli/actions/runs/41690962313/artifacts/13256924986)
- [x64 .NET 11 AVX2 artifact 13256259644](https://github.com/symbiogenesis/DotJpegli/actions/runs/41690962313/artifacts/13256259644)
- [x64 .NET 11 default artifact 13257384757](https://github.com/symbiogenesis/DotJpegli/actions/runs/41690962313/artifacts/13257384757)

Local work copied and analyzed existing CI artifacts. No benchmark ran locally.
