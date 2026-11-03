# Released 1.0.0 comparison on .NET 11 with Vector512

[Automatic CI run 41668548392, attempt 1](https://github.com/symbiogenesis/DotJpegli/actions/runs/41668548392/attempts/1)
compares `a0450526af276d2f686a820a523c270aa30f09f1` with released 1.0.0 source
`a40868bfdace7d737413085522a88d351aa16782`. The candidate's encoder sources and
build settings are unchanged from the
[preceding six-lane comparison](../2026-11-03-release-regression-74ae25d/README.md).
This targeted job requires accelerated Vector512 in both workers before any
case is measured. It ran on an **AMD EPYC 9V74**, with accelerated Vector256 and
Vector512 and eight `Vector<float>` lanes, covering the .NET 11 SIMD capability
missing from the preceding matrix's actual hosts.

The [release comparison](release/X64-net11.0-default/report.md) passes with
**256 valid selected cases, matching JPEG hashes and no strong regressions**.
The geometric mean of all selected candidate/baseline paired medians is
**0.903449**. There are 219 strong candidate preferences and 37 inconclusive
results. Of the 256 selected medians, 252 are faster and four are slightly
slower; all four slower results are inconclusive:

| Case | Candidate / released-source baseline median | Preference |
| --- | ---: | --- |
| Kodak05 / 420 / progression 2 / distance 1 | 1.003342 | Inconclusive |
| Kodak08 / 420 / progression 2 / distance 1 | 1.001452 | Inconclusive |
| Kodak13 / 420 / progression 2 / distance 1 | 1.005702 | Inconclusive |
| Kodak24 / 420 / progression 0 / distance 1 | 1.000701 | Inconclusive |

The largest selected median ratio is 1.005702. A median above 1.005 alone is
insufficient for a strong regression: every one of the six quartet ratios must
exceed 1.005, with all guards passing. Inconclusive results do not prove equal
speed or rule out small regressions. All eight configurations that strongly
favored 1.0.0 in the earlier
[123c8a9 .NET 11 Vector512 lane](../2026-11-03-release-regression-123c8a9/README.md)
now have medians below 1; five strongly favor the candidate and three are
inconclusive. That earlier host was Intel and its protocol permitted one trial,
so this is not a controlled estimate of individual fixes' effects.

Together with the preceding six lanes, the evidence contains **1,792 valid
selected comparisons**, 1,640 strong candidate preferences, 152 inconclusive
results and zero strong regressions. There are 1,788 faster selected medians
and the four slower medians listed above. These counts describe measured
configurations on these hosts and runtimes, not an unrestricted performance
guarantee.

## Sampling and retained attempts

The fixed 256-case matrix covers Kodak24 plus eight deterministic controls,
sampling 444/420, progression 0/2 and distances 1/4. Same-host workers execute
serially under identical CPU affinity, runtime, inputs and options. Each trial
uses six balanced ABBA/BAAB quartets, five untimed warmups per leg and 20 timed
encodes per leg (100 for the tiny input). Tiered compilation is disabled;
natural collections remain included. The baseline is rebuilt with the current
SDK and matching assembly binding version, preserving its released source
identity. Original and rebuilt dependency locks and build provenance are
retained. This compares released sources on the current runtime, not the
original package binary on its original runtime.

The release protocol allows at most eight complete attempts solely for
existing timing stability failures, and selects the **first valid trial**,
including a regression or inconclusive result. Identity, hash, structure,
nonfinite and too-short failures cannot be retried. It does not select the
fastest result, discard individual samples, merge trials or relax guards.

Here, 23 cases were initially invalid. The run collected 281 complete attempts:
256 selected trials and **25 earlier invalid trials**; the maximum selected
attempt was three. All earlier trials and their invalidity reasons are
retained. This first-valid result is conditioned on timing stability, which
can favor quieter intervals, including intervals without GC. It does not
describe an initially stable matrix or the unconditional latency distribution
of all attempts.

The release `cases.json` contains every selected raw quartet, every attempt's
analysis and the selection index. Each earlier invalid trial's original raw
file is retained under `raw-comparisons/`, preserving every report link.
Selected raw files are not duplicated because their quartets are already in
`cases.json`; the hosted artifact retains their original `rawFile` paths.

## Retained algorithm alternatives

Both alternatives were compared with the current default on the same wide
host, using four Kodak images and four deterministic controls across the eight
settings, for 64 cases each. These diagnostics use one trial per case with no
timing retries. All 128 paired JPEG hashes match. Their incomplete valid subsets
do not establish aggregate performance guarantees:

| Alternative | Valid / 64 | Strong default preference | Strong alternative preference | Inconclusive valid | Valid-subset geometric mean ratio |
| --- | ---: | ---: | ---: | ---: | ---: |
| [Sparse transposed quantization](algorithms/X64-net11.0-default/sparse-transposed/evidence/report.md) | 56 | 49 | 0 | 7 | 1.079333 |
| [Full-width transpose](algorithms/X64-net11.0-default/full-width-transpose/evidence/report.md) | 57 | 6 | 0 | 51 | 1.002887 |

Ratios here are alternative / current default. Sparse transposed valid medians
range from 0.995434 to 1.156499, with only one below 1. Full-width transpose
valid medians range from 0.991425 to 1.013073, with 15 below 1, all inconclusive.
There are 15 invalid scenario cases, retained in full. Neither alternative has
a strong win in this wide-host comparison. Combined with the preceding hosts'
conflicting results, these measurements do not support enabling either
alternative through a framework or vector-width rule alone.

## Provenance

[Provenance](provenance.json) records both artifact IDs, archive digests and
exact hashes for every retained artifact file. [Analysis](analysis.json)
contains all case medians and sampling/progression/distance groups. All release
and scenario cases, summaries, reports, run identities, source hashes and build
provenance are retained. The retention check verified matching output hashes
for **9,816 raw legs**, including every earlier invalid release trial. Original
inputs, full build outputs and duplicate selected-trial files remain in the
hosted artifacts.

The earlier six-lane snapshot remains unchanged. This targeted run does not
repeat the separate native comparison, whose retained
[overall INVALID result](../2026-11-03-release-regression-74ae25d/native/report.md)
does not establish native parity. All benchmarks ran on CI; local work only
copied and analyzed retained evidence.
