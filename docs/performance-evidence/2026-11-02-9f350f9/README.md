# Performance reference evidence

This snapshot tests revision `9f350f9d7f1d8ed3d527d7efad08364a1645f420` in
[hosted run 41567970649](https://github.com/symbiogenesis/DotJpegli/actions/runs/41567970649).
All four selected cases meet the fixed individual latency bound. Progressive
encoding uses 7–17% less time than native; sequential encoding takes 2–5% more.
The host is AMD EPYC 7763 x64, .NET `11.0.0-rc.3.26522.148`, tiered compilation
disabled. Full performance parity is not evaluated.

- [Report](targeted/report.md) and [summary](targeted/summary.json).
- [Raw samples](targeted/cases.json), [host and settings](targeted/run.json) and [input hashes](targeted/sources.json).
- [Native provenance](native-provenance.json) and [artifact identity and retained-file hashes](provenance.json).

Exact inputs and JPEGs are in the hosted artifact; paths in raw JSON refer to
that artifact layout. The snapshot date uses America/Chicago; the run's UTC date
is November 3. The provenance manifest lists the files retained here.
