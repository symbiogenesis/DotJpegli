# Complete performance matrix on EPYC 9V45

Revision `eb86172fb197b7614ca9e82ffdbd6422cfc4d74f` was measured in
[hosted run 41573245587](https://github.com/symbiogenesis/DotJpegli/actions/runs/41573245587).
All 256 latency cases are valid, stable and matched. Full parity **failed**.
The complete quality/compression matrix, test suites, deployment and
reproducibility checks passed.

On this AMD EPYC 9V45 x64 host, photograph group means show 8–9% lower latency
for sequential and 28–29% lower latency for progressive encoding. Both
progressive groups pass every fixed bound; sequential 4:4:4 passes too.
Sequential 4:2:0 has a worst ratio of 1.147908, and deterministic controls
still fail, with tiny inputs reaching 2.097201. Group means cannot override
those failures. The runtime is .NET `11.0.0-rc.3.26522.148`, one worker, tiered
compilation disabled. Earlier full runs used different CPU models; their
differences cannot establish optimization gains.

- [Report](full/report.md), [summary](full/summary.json), [raw samples](full/cases.json), [run identity](full/run.json) and [input hashes](full/sources.json).
- [Native provenance](native-provenance.json) and [both artifact identities and retained-file hashes](provenance.json).
- [Profile selection](profiles/full-performance-profile-selection.txt) and separate instrumented Speedscope traces for [tiny input](profiles/full-performance-profile-tiny-odd-edges.speedscope.json), [low contrast](profiles/full-performance-profile-low-contrast.speedscope.json) and [low noise](profiles/full-performance-profile-low-noise.speedscope.json).

Profile timings do not establish acceptance. Traces include startup and warmup;
filter to `BenchmarkManaged` for steady encoding and avoid counting overlapping
parent/child methods twice. Instrumented raw measurements and command logs are
retained under `profiles/`. Exact inputs, JPEGs and binary nettraces remain in
the hosted artifacts. The snapshot date uses America/Chicago; the UTC date is
November 3.
