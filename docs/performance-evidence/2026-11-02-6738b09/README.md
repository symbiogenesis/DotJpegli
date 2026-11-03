# Complete performance matrix and CPU diagnostics

Revision `6738b09178417877c00c92ec9497a6cfbdc5ab7c` was measured in
[hosted run 41571363259](https://github.com/symbiogenesis/DotJpegli/actions/runs/41571363259).
All 256 latency cases are valid, stable and matched. Full parity **failed**.
Both progressive photograph groups passed the fixed group and individual
limits, with geometric mean ratios 0.922406 (4:4:4) and 0.908697 (4:2:0).
Sequential encoding and deterministic controls still exceed the limits.

This host is AMD EPYC 7763 x64, .NET `11.0.0-rc.3.26522.148`, one worker,
tiered compilation disabled. The previous complete reference used EPYC 9V74;
cross-host differences cannot establish before/after optimization gains.
The complete quality/compression matrix, tests, deployment and reproducibility
checks passed in this same run.

- [Full report](full/report.md), [summary](full/summary.json), [raw samples](full/cases.json), [run identity](full/run.json) and [input hashes](full/sources.json).
- [Native provenance](native-provenance.json) and [both artifact identities and retained-file hashes](provenance.json).
- [Profile selection](profiles/full-performance-profile-selection.txt) and Speedscope traces for [tiny input](profiles/full-performance-profile-tiny-odd-edges.speedscope.json), [smooth gradient](profiles/full-performance-profile-smooth-gradient.speedscope.json) and [low noise](profiles/full-performance-profile-low-noise.speedscope.json).

Profiles are separate instrumented diagnostics, collected after the unprofiled
verdict using pinned `dotnet-trace 10.0.750501`. Their timings cannot establish
acceptance. Installation/collection logs and instrumented per-case evidence are
retained under `profiles/`. Speedscope stacks include startup and untimed warmup;
filter to `BenchmarkManaged` when investigating steady encoding, and account for
both CPU and unmanaged-time leaves. Do not count overlapping parent/child frames
twice when grouping methods by name.

Exact images, JPEGs, native command logs and binary nettraces remain in the hosted
artifacts. The snapshot date uses America/Chicago; the UTC date is November 3.
