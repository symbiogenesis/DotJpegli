# First research batch: selection, SIMD ordering and workspace reuse

Baseline: `2ef3d5fcc12a804c7cd707446091eb74fa9c6476`. Implementation:
`2fa9afe53325be785bd638c7c4448598d308cb9f`. Corrected validation:
`67d5724b11de0c249577f5d7c11ed861a37eee38`.

The [plan](../../../PLAN.md) describes the three changes, pending research and
correctness checks. This is selected-case evidence for the combined batch.
It does not isolate individual kernel gains or establish full performance parity.

## Verified narrow SIMD comparison

[Run 41579107572](https://github.com/symbiogenesis/DotJpegli/actions/runs/41579107572)
measured 32 cases: Kodak01, low contrast, low noise and tiny odd edges; distances
1/4, sampling 444/420 and progression 0/2. Both managed revisions used
`DOTNET_EnableAVX512=0`; actual capabilities show Vector256 accelerated,
Vector512 disabled and eight float lanes. The host was AMD EPYC 7763 x64,
.NET `11.0.0-rc.3.26522.148`, one worker, affinity 0, tiered compilation disabled.

- Managed latency geometric-mean current/baseline ratio: **0.99003**.
- Native geometric-mean drift: **0.99977**.
- 24 cases faster, eight slower; the largest slowdown is about **3.64%**.
- Allocated bytes per encode decrease in every case, by **4.07-47.71%**.
- Both evidence sets are valid under the fixed policy and all JPEG hashes match.

[Comparison](narrow-comparison.md), [machine-readable comparison](narrow-comparison.json),
[baseline raw cases](narrow-baseline/cases.json), [current raw cases](narrow-current/cases.json),
[baseline run identity](narrow-baseline/run.json) and [current run identity](narrow-current/run.json)
retain the settings, samples, allocation totals and CPU/runtime identities.
The sibling summaries and sources retain verdicts and input hashes.

Revisions ran serially rather than interleaved. Allocations are warmed per-block
managed totals divided by encodes, not pool capacity or peak working memory.
Individual slowdowns are retained; this is not a universal speed improvement.
The corrected run's normal-dispatch result is invalid because Kodak01/444/p0/d1
exceeds the fixed 10% managed relative-MAD limit. Full test, fallback, deployment,
reproducibility and package checks pass; native latency bounds still fail.

## Earlier normal-dispatch measurements and timing drift

[Run 41578440598](https://github.com/symbiogenesis/DotJpegli/actions/runs/41578440598)
used EPYC 9V45 with the same runtime and settings. The
[initial normal comparison](initial-normal-comparison.md) includes two large
Kodak01/420/p2 slowdowns, with substantial native drift. Its intended narrow pair
used the obsolete `DOTNET_EnableAVX512F` switch and retained Vector512. The
capability guard correctly rejected a narrow-path claim.

Those measurements can still be analyzed as a
[repeat of normal dispatch](repeat-normal-comparison.md): encoder/runner/native
and corpus hashes match the initial pair, and Vector512 is enabled in both.
The two large slowdowns do not reproduce (managed ratios 0.99594 and 0.99922).
The repeat's 32-case geometric-mean managed ratio is 0.98664, with native drift
1.00030. Initial raw blocks contain slow/fast timing-state transitions without
matching managed GC. A tight majority cluster can pass MAD despite such a
transition, so a valid policy verdict does not prove comparable host conditions.

The repeat report was regenerated from retained raw JSON with
`tools/compare-performance-revisions.py`; no local measurements were executed.
Its folder names in the hosted artifact say `avx2`, but actual Vector512 flags
identify the execution path. Do not treat that attempted pair as narrow evidence.

[Provenance](provenance.json) records source runs, artifact IDs and SHA256 hashes
of the retained comparison and raw JSON files. Exact images, JPEG outputs and
other original artifacts remain in the hosted runs, subject to artifact retention.
The snapshot date uses America/Chicago; the UTC date is November 3.
