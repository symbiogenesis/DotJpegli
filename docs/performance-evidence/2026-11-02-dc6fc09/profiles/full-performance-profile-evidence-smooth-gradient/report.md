# Targeted performance diagnostic

**Instrumented CPU diagnostic only. Timings do not establish latency acceptance.** dotnet-trace 10.0.750501; dotnet-sampled-thread-time; profiling timings are not acceptance evidence

Status: selected-case-bound-exceeded. 1/1 selected cases measured. **Full performance parity is not evaluated.**
Tested revision: `dc6fc09595dc3a6296abe99575b38e8fae449d82`.

7 alternating blocks, 5 untimed warmups per block, 100..100 shared iterations. The selected-case latency bound is managed/native <=1.10. No group or whole-corpus acceptance verdict is inferred.

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| smooth-gradient/444/p0/d1 | 0.785 | 0.383 | 2.050 | 1.6 % | 0.1 % |

- smooth-gradient/444/p0/d1: selected-case median latency ratio 2.050163 exceeds 1.10.

Raw samples, allocations, JPEG structure matching, input hashes, settings and host identity are retained. Exit 0 means selected cases were valid and within their individual bound; it does not mean full parity passed. Exit 1 identifies a stable selected-case bound failure; exit 2 identifies invalid evidence.
