# Targeted performance diagnostic

**Instrumented CPU diagnostic only. Timings do not establish latency acceptance.** dotnet-trace 10.0.750501; dotnet-sampled-thread-time; profiling timings are not acceptance evidence

Status: invalid. 1/1 selected cases measured. **Full performance parity is not evaluated.**
Tested revision: `eb86172fb197b7614ca9e82ffdbd6422cfc4d74f`.

7 alternating blocks, 5 untimed warmups per block, 100..100 shared iterations. The selected-case latency bound is managed/native <=1.10. No group or whole-corpus acceptance verdict is inferred.

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| tiny-odd-edges/444/p2/d1 | 0.075 | 0.028 | 2.636 | 16.5 % | 1.0 % |

- tiny-odd-edges/444/p2/d1: managed relative MAD exceeds the fixed 10% noise bound.

Raw samples, allocations, JPEG structure matching, input hashes, settings and host identity are retained. Exit 0 means selected cases were valid and within their individual bound; it does not mean full parity passed. Exit 1 identifies a stable selected-case bound failure; exit 2 identifies invalid evidence.
