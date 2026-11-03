# Targeted performance diagnostic

**Instrumented CPU diagnostic only. Timings do not establish latency acceptance.** dotnet-trace 10.0.750501; dotnet-sampled-thread-time; profiling timings are not acceptance evidence

Status: selected-case-bound-exceeded. 1/1 selected cases measured. **Full performance parity is not evaluated.**
Tested revision: `eb86172fb197b7614ca9e82ffdbd6422cfc4d74f`.

7 alternating blocks, 5 untimed warmups per block, 100..100 shared iterations. The selected-case latency bound is managed/native <=1.10. No group or whole-corpus acceptance verdict is inferred.

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| low-contrast/444/p0/d4 | 0.318 | 0.215 | 1.480 | 3.0 % | 0.6 % |

- low-contrast/444/p0/d4: selected-case median latency ratio 1.480033 exceeds 1.10.

Raw samples, allocations, JPEG structure matching, input hashes, settings and host identity are retained. Exit 0 means selected cases were valid and within their individual bound; it does not mean full parity passed. Exit 1 identifies a stable selected-case bound failure; exit 2 identifies invalid evidence.
