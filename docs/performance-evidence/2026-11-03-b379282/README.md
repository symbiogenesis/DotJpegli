# Latest targeted native diagnostic at b379282

[Automatic CI run 41698674503](https://github.com/symbiogenesis/DotJpegli/actions/runs/41698674503)
measured revision `b379282ae58dce4243a988c91d48d4d3f037b555` on November 3,
2026 (America/Chicago). All four selected Kodak01 distance-1 cases completed,
but the diagnostic is **invalid**: sequential 4:2:0 has managed relative MAD
**11.93%**, above the fixed 10% bound. Full corpus coverage and performance
parity were not evaluated.

| Kodak01 mode, distance 1 | Managed median ms | Native median ms | Managed / native | Timing validity |
| --- | ---: | ---: | ---: | --- |
| Sequential 4:4:4 | 5.268 | 5.588 | 0.942777 | Valid |
| Progressive level 2, 4:4:4 | 11.402 | 14.343 | 0.794980 | Valid |
| Sequential 4:2:0 | 4.701 | 4.391 | 1.070604 | **Invalid** |
| Progressive level 2, 4:2:0 | 8.459 | 11.159 | 0.758004 | Valid |

The three individually valid cases meet the selected-case 1.10 bound. That
does not establish whole-corpus acceptance or a passing overall diagnostic.
The invalid case's median is retained descriptively, not as a speed conclusion.
Every selected JPEG structure matches the pinned native encoder.

The host is AMD EPYC 9V74 x64, running .NET `11.0.0-rc.3.26522.148`, with
eight float SIMD lanes, accelerated Vector256 and no Vector512. One worker is
pinned to one CPU, and tiered compilation is disabled. Seven alternating-order
blocks use five untimed warmups per block and 3–100 shared timed iterations.
These native-relative measurements do not isolate revision changes from the
separate [0104778 run](../2026-11-03-0104778/README.md).

## Retained evidence

- [Diagnostic report](targeted/report.md), [summary](targeted/summary.json), [raw cases](targeted/cases.json), [run identity](targeted/run.json) and [input hashes](targeted/sources.json).
- [Native build provenance](targeted/native-provenance.json), [artifact metadata](targeted/artifact.json) and [retained-file hashes](targeted/provenance.json).

Files were copied from existing [artifact 13262473031](https://github.com/symbiogenesis/DotJpegli/actions/runs/41698674503/artifacts/13262473031).
Exact inputs, encoded images, executable files and native logs remain in the
hosted artifact; raw JSON paths refer to that layout. GitHub-reported archive
digests and locally calculated retained-file hashes are distinguished in the
provenance. No benchmark ran locally and no additional workflow was launched.
