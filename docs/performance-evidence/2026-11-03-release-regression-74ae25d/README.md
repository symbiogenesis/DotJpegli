# Released 1.0.0 source comparison after fixes

[Automatic CI run 41664306969, initial attempt](https://github.com/symbiogenesis/DotJpegli/actions/runs/41664306969/attempts/1)
compares candidate `74ae25d8a9d8ed5718466a3905131c37845706d3` with
`a40868bfdace7d737413085522a88d351aa16782`, the source revision published as
NuGet 1.0.0 by [publication run 38698571455](https://github.com/symbiogenesis/DotJpegli/actions/runs/38698571455).

All six release lanes pass the fixed regression gate with 256 valid selected
trials each. All **1,536 selected case medians favor the candidate**, and all
paired JPEG hashes match. Of these, 1,421 consistently favor the candidate by
more than 0.5% in every quartet; 115 are inconclusive under that stricter rule.
There are no strong regressions in the selected trials. This conclusion is
limited to the measured corpus, settings, runtimes, hosts and stability
conditioning described below. Inconclusive results do not prove equal speed or
rule out smaller regressions.

| Lane | Actual CPU | Vector512 | Initially invalid cases | Total attempts | Inconclusive selected cases | Selected geometric mean ratio | Largest selected median ratio |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: |
| [ARM64 .NET 10](initial/release/ARM64-net10.0-default/report.md) | ARM implementer 0x41, part 0xd49 | false | 3 | 259 | 50 | 0.941338 | 0.998739 |
| [ARM64 .NET 11](initial/release/ARM64-net11.0-default/report.md) | ARM implementer 0x41, part 0xd49 | false | 4 | 260 | 25 | 0.937387 | 0.999650 |
| [x64 .NET 10, AVX2](initial/release/X64-net10.0-avx2/report.md) | Intel Xeon 6973P-C | false | 45 | 334 | 14 | 0.887010 | 0.996414 |
| [x64 .NET 10, default](initial/release/X64-net10.0-default/report.md) | Intel Xeon Platinum 8573C | true | 21 | 278 | 6 | 0.893353 | 0.970311 |
| [x64 .NET 11, AVX2](initial/release/X64-net11.0-avx2/report.md) | AMD EPYC 9V74 | false | 9 | 265 | 3 | 0.883101 | 0.982634 |
| [x64 .NET 11, default](initial/release/X64-net11.0-default/report.md) | AMD EPYC 7763 | false | 16 | 273 | 17 | 0.899692 | 0.996801 |

Ratios are candidate / released-source baseline. The geometric means summarize
all 256 selected paired medians in a lane. ARM64 has four `Vector<float>` lanes
and no accelerated Vector256; every x64 lane has eight float lanes and
accelerated Vector256. Vector512 capability is independent of that float width.
The initial .NET 11 default job ran on a host without Vector512. It therefore
does not cover the .NET 11 Vector512 path implicated by the earlier
[123c8a9 comparison](../2026-11-03-release-regression-123c8a9/README.md).
Any additional hardware attempt is retained separately from this initial
matrix, preserving all original artifact identities and results.

## Sampling and retained invalid attempts

The fixed matrix is Kodak24 plus eight deterministic controls, sampling
444/420, progression 0/2 and distances 1/4. Each case compares persistent
workers on the same host and CPU affinity with six balanced ABBA/BAAB quartets,
five untimed warmups per leg and 20 timed encodes per leg (100 for the tiny
input). Tiered compilation is disabled; natural collections remain included.

The release protocol permits at most eight complete attempts only when existing
timing stability guards fail. The **first valid trial is final**, including a
regression or inconclusive result. No fastest-trial selection, sample merging,
outlier removal or relaxed guard is used. Identity, hash, structure, nonfinite
and too-short failures cannot be retried. A strong regression still requires
every quartet ratio to exceed 1.005 with all guards passing.

Across the six lanes, 98 cases were initially invalid. The protocol collected
1,669 complete attempts: 1,536 selected trials and **133 earlier invalid trials**.
The maximum selected attempt was six. All earlier trials and their invalidity
reasons remain available. This is a first-valid, stability-conditioned result;
it is not an initially stable matrix or an estimate of the unconditional
latency distribution. Conditioning on stability can favor quieter intervals,
including intervals without GC. The previous comparison used one attempt per
case, so its invalid-case rate cannot be compared directly with this run's
selected-case rate.

Every release `cases.json` retains the selected trial's complete raw quartets,
all attempt analyses and the selected attempt number. Each earlier invalid
trial also has its complete original file under `raw-comparisons/`, so every
report link to a rejected trial resolves locally. Selected-trial raw files are
not duplicated: their quartet data is already in `cases.json`; their original
`rawFile` references remain available in the hosted artifact.

## Retained algorithm alternatives

The same run compares compile-time alternatives with the candidate's default
algorithm, using four Kodak images and four deterministic controls across the
same eight settings, for 64 cases per comparison. These comparisons use one
trial per case and do not retry timing instability. All 384 paired JPEG hashes
match. There are 341 valid and 43 invalid cases; invalid cases cannot establish
a speed preference.

| Alternative and lane | Valid / 64 | Strong default preference | Strong alternative preference | Inconclusive valid |
| --- | ---: | ---: | ---: | ---: |
| [Sparse transposed, .NET 10 AVX2](initial/algorithms/X64-net10.0-avx2/sparse-transposed/evidence/report.md) | 52 | 42 | 0 | 10 |
| [Sparse transposed, .NET 10 default](initial/algorithms/X64-net10.0-default/sparse-transposed/evidence/report.md) | 52 | 42 | 3 | 7 |
| [Sparse transposed, .NET 11 AVX2](initial/algorithms/X64-net11.0-avx2/sparse-transposed/evidence/report.md) | 57 | 54 | 0 | 3 |
| [Sparse transposed, .NET 11 default](initial/algorithms/X64-net11.0-default/sparse-transposed/evidence/report.md) | 62 | 57 | 0 | 5 |
| [Full-width transpose, .NET 11 AVX2](initial/algorithms/X64-net11.0-avx2/full-width-transpose/evidence/report.md) | 58 | 0 | 50 | 8 |
| [Full-width transpose, .NET 11 default](initial/algorithms/X64-net11.0-default/full-width-transpose/evidence/report.md) | 60 | 16 | 0 | 44 |

Sparse transposed quantization's three strong wins occur only on the .NET 10
Vector512 host, at 444/sequential/distance 4: Kodak04 (ratio 0.991615),
low-contrast (0.970346) and low-noise (0.972788). The full-noise control at the
same settings instead strongly favors the default (1.033685). All valid sparse
medians on the other three hosts favor the default. These results do not
support a rule based only on framework, sampling, progression and distance.

Full-width transpose has 50 strong wins on the .NET 11 EPYC 9V74 host, but
16 strong losses on the .NET 11 EPYC 7763 host. Both hosts report the same
accelerated Vector256 capability, eight float lanes and no Vector512.
Framework and vector width alone therefore do not explain a safe selection
rule. The alternatives remain available for controlled experiments; these
incomplete, host-dependent comparisons do not establish universal defaults.

## Separate native comparison

The [native comparison report](native/report.md), from
[artifact 13246905910](https://github.com/symbiogenesis/DotJpegli/actions/runs/41664306969/artifacts/13246905910),
measured all 256 cases but has only 255 valid timings. Kodak05/420/progressive
2/distance 4 exceeds the fixed 10% managed relative-MAD bound. Its overall
verdict is **INVALID**, and several valid cases also exceed the fixed native
latency bounds. This remains a separate native-performance gap; passing the
released-source regression comparison does not establish native parity. No
README performance chart was refreshed from invalid native evidence.

## Provenance and scope

The baseline is rebuilt from released sources with the candidate SDK. Its
assembly binding version is set to 2.0.0.0 so the same worker can load it,
while its informational version preserves 1.0.0 and the released source SHA.
Only the current `global.json` is copied into the archived baseline source.
SDK-owned dependencies are reevaluated, original and rebuilt lock files are
retained, and historical source analyzers are disabled. This isolates encoder
source changes on each current runtime; it does not compare the original
published binary on its original runtime.

[Provenance](provenance-initial.json) records all eleven initial artifact IDs,
archive digests and exact hashes of every retained artifact file.
[Machine-readable analysis](analysis-initial.json) records every case median
and groups by sampling, progression and distance. Each lane retains its cases,
summary, report, executable/runtime identity, source hashes and build provenance.
The retention check verified matching hashes for 49,272 raw release/scenario
legs, including all earlier invalid trials. Original inputs, full build outputs
and duplicate selected-trial snapshots remain in the hosted artifacts. All
measurements ran on CI; local work only copied and analyzed retained samples.
