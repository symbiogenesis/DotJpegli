# Complete performance matrix on EPYC 7763

Revision `dc6fc09595dc3a6296abe99575b38e8fae449d82` was measured in
[hosted run 41575177658](https://github.com/symbiogenesis/DotJpegli/actions/runs/41575177658).
All 256 cases have valid, stable, matched measurements. Full parity **failed**.
The complete quality/compression matrix, tests, deployment and reproducibility
checks passed.

On AMD EPYC 7763 x64 with .NET `11.0.0-rc.3.26522.148`, one worker and tiered
compilation disabled, both progressive photograph groups pass the fixed bounds
with geometric mean ratios 0.906043 (4:4:4) and 0.893823 (4:2:0).
Sequential photograph group ratios are 1.371131 and 1.318178; their worst cases
reach 1.591347 and 1.600221. Deterministic controls also fail, reaching 1.802404.
The earlier EPYC 9V45 run cannot establish gains on this different machine.

- [Report](full/report.md), [summary](full/summary.json), [raw samples](full/cases.json), [run identity](full/run.json) and [input hashes](full/sources.json).
- [Native provenance](native-provenance.json) and [artifact identities and retained-file hashes](provenance.json).
- [Profile selection](profiles/full-performance-profile-selection.txt) and separate instrumented traces for [smooth gradient](profiles/full-performance-profile-smooth-gradient.speedscope.json), [low contrast](profiles/full-performance-profile-low-contrast.speedscope.json) and [low noise](profiles/full-performance-profile-low-noise.speedscope.json).

Profiles are diagnostics, not acceptance timings. They include startup and
warmup; filter to `BenchmarkManaged` and avoid double-counting parent/child
methods. The dominant sampled sequential frame is `ProcessSequentialInterleaved`,
including its inlined transform/quantization work. Instrumented per-case evidence
and logs are retained under `profiles/`. Exact inputs, JPEGs and binary nettraces
remain in the hosted artifacts. The snapshot date uses America/Chicago; the UTC
date is November 3.
