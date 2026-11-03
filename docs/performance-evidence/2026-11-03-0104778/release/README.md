# Released 1.0.0 source comparison at 0104778

[Automatic CI run 41695349570](https://github.com/symbiogenesis/DotJpegli/actions/runs/41695349570)
measured revision `0104778b2691cf3e61d7dbac280457f383d9bc0c` on November 3,
2026 against `a40868bfdace7d737413085522a88d351aa16782`, the source published
as NuGet 1.0.0. This comparison includes the segmented/partial converted-plane
cache and subsequent hot-path changes.

All six lanes complete all 256 cases with valid selected timing evidence and
matching JPEG bytes. **Two of six release regression gates fail**: 110 of
1,536 selected comparisons consistently favor 1.0.0, 1,310 consistently favor
the candidate and 116 are inconclusive. The largest selected median ratio is
1.146380, or **14.64% more encoding time**, for Kodak20, 4:4:4, sequential,
distance 4 in the .NET 10 x64 default lane. It is a confirmed preference for
the baseline, selected on its first attempt.

| Lane | CPU / Vector512 | Candidate favored | 1.0.0 favored | Inconclusive | Earlier invalid trials | Selected median ratio geomean |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| [ARM64 .NET 10](ARM64-net10.0-default/report.md) | ARM part 0xd49 / false | 225 | 0 | 31 | 2 | 0.941204 |
| [ARM64 .NET 11](ARM64-net11.0-default/report.md) | ARM part 0xd49 / false | 245 | 0 | 11 | 4 | 0.932789 |
| [x64 .NET 10, AVX2](X64-net10.0-avx2/report.md) | Intel Xeon Platinum 8573C / false | 242 | 0 | 14 | 44 | 0.891564 |
| [x64 .NET 10, default](X64-net10.0-default/report.md) | AMD EPYC 9V74 / true | 124 | 99 | 33 | 43 | 0.961849 |
| [x64 .NET 11, AVX2](X64-net11.0-avx2/report.md) | AMD EPYC 7763 / false | 250 | 0 | 6 | 23 | 0.891909 |
| [x64 .NET 11, default](X64-net11.0-default/report.md) | AMD EPYC 7763 / false | 224 | 11 | 21 | 14 | 0.900587 |

Ratios are current/baseline; lower means less encoding time. The four lanes
without confirmed regressions pass the release gate. Their results do not
override the failures in the other lanes. Default SIMD settings do not imply
Vector512 support: the .NET 11 default host has no accelerated Vector512.
The lane geometric means describe selected case medians and do not establish
a no-regression verdict or speed on untested hardware.

## Timing selection and scope

The fixed corpus is all 24 Kodak photographs plus eight deterministic controls,
at distances 1/4, sampling 444/420 and progression 0/2. Each lane compares both
revisions on the same host, runtime and CPU affinity, using identical persistent
workers. Six balanced ABBA/BAAB quartets run serially, with five warmups per leg
and 20 timed encodes per leg (100 for the tiny input). Tiered compilation is
disabled and natural GC remains included. The baseline is rebuilt from released
sources with the candidate SDK; this does not measure the original published
binary on its original runtime.

A consistent preference requires every quartet ratio to differ by more than
0.5% in the same direction, with the fixed timing and identity guards passing.
Inconclusive results do not establish equivalent speed. Allocation measurements
are separate from the latency gate and do not measure retained capacity or peak
memory.

There were 112 initially invalid cases and **130 earlier invalid trials**,
for 1,666 total trials. Timing instability permits at most eight complete
attempts per case. The first valid trial is final, including regressions and
inconclusive results. No valid trial was retried and no samples were removed
or merged within a trial. Selecting for stability can favor quieter intervals;
the selected results do not describe the unconditional latency distribution.

## Retained evidence

Every lane retains `cases.json` with all selected raw quartet legs, `summary.json`,
`report.md`, `run.json`, `sources.json` and baseline build provenance. Every
retried case additionally retains each complete raw trial, including all earlier
invalid trials. Input files and redundant incremental snapshots remain in the
hosted artifacts; paths inside the original JSON refer to those artifact layouts.

[Derived analysis](analysis.json) records lane counts, geometric means and worst
cases. [Provenance](provenance.json) records all six source artifacts and SHA-256
hashes of the copied files. Archive digests are GitHub-reported and were not
independently recomputed. All 36,864 selected quartet-leg JPEG hashes were checked
against the paired output hashes, along with the raw trials of retried cases.
Local work only copied and analyzed existing CI evidence; no benchmark ran
locally.

The same workflow's [native comparison](../README.md) has one invalid timing
case and does not establish native performance parity. Its separate
[workload observations](../../2026-11-03-0104778-workloads/README.md) cover
instrumented application scenarios. The overall workflow failed; passing lanes
and successful workload collection do not imply a successful workflow.
