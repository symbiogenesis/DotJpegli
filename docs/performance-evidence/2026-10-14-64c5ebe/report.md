# Interrupted performance comparison

[Hosted run 38572245330](https://github.com/symbiogenesis/DotJpegli/actions/runs/38572245330)
tested revision `64c5ebeb6e48edb6b6228c8fb97f64224a88c235`. The comparison job
reached its 90-minute limit after retaining **138 of 256 cases**. Complete
matrix coverage and a complete parity verdict are unavailable. The Windows and
Ubuntu full tests, SIMD fallback checks and reproducible builds passed; the
1,280-pair quality/compression comparison also passed.

Every retained case passed its timing-stability and JPEG-structure checks, and
every retained case exceeded the fixed latency ratio limit of 1.10. Ratios
across these cases ranged from **13.356666 to 128.365365**. These counterexamples
rule out passing the declared performance gate for this implementation; the
incomplete corpus cannot establish a whole-matrix aggregate or positive parity.

| Measured subset | Cases / required | Geometric mean latency ratio | Worst ratio |
| --- | ---: | ---: | ---: |
| Photographs, 4:2:0, sequential | 34 / 48 | 23.650992 | 31.732018 |
| Photographs, 4:2:0, progressive level 2 | 34 / 48 | 92.880404 | 128.365365 |
| Photographs, 4:4:4, sequential | 36 / 48 | 24.609954 | 31.504790 |
| Photographs, 4:4:4, progressive level 2 | 34 / 48 | 78.314940 | 97.272654 |

Each ratio is managed/native median encoding latency. The fixed group limit is
1.05. These means describe only each measured subset; no controls were reached.
For example, `kodim01`, 4:4:4, distance 1 took 105.98 ms managed versus 5.72 ms
native sequentially, and 806.19 ms versus 13.68 ms progressively.

The measurements used one CPU, one managed worker, .NET 10, tiered compilation
disabled and the pinned native build. The exact CPU model was not retained
because this runner version wrote environment metadata only after completion.
The follow-up runner records machine and executable identity before measurement
and retains explicit incomplete status and progress when interrupted. Follow-up
runs stop on a stable matched counterexample, since later cases cannot rescue
a failed individual bound. Timing methods and acceptance bounds remain unchanged;
a passing verdict still requires all 256 cases.

`cases.json` and `sources.json` are the original retained files, and
`provenance.json` identifies the native build. `analysis.json` is a separately
derived analysis of this interrupted run, not a runner-produced summary. Full
inputs, JPEGs and native logs remain in the hosted artifact and retained session
download. This directory records a failure to establish parity, not a passing
performance proof.
