# Targeted performance diagnostic

Status: selected-case-bound-exceeded. 4/4 selected cases measured. **Full performance parity is not evaluated.**
Tested revision: `b8c8058cee59e21c636a7aa0e340c8df6e131c77`.

7 alternating blocks, 5 untimed warmups per block, 3..100 shared iterations. The selected-case latency bound is managed/native <=1.10. No group or whole-corpus acceptance verdict is inferred.

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 5.278 | 3.484 | 1.515 | 1.2 % | 0.4 % |
| kodim01/444/p2/d1 | 10.330 | 8.771 | 1.178 | 1.9 % | 0.5 % |
| kodim01/420/p0/d1 | 3.843 | 2.841 | 1.352 | 0.9 % | 1.7 % |
| kodim01/420/p2/d1 | 7.519 | 7.093 | 1.060 | 0.5 % | 0.5 % |

- kodim01/444/p0/d1: selected-case median latency ratio 1.514662 exceeds 1.10.

- kodim01/444/p2/d1: selected-case median latency ratio 1.177780 exceeds 1.10.

- kodim01/420/p0/d1: selected-case median latency ratio 1.352414 exceeds 1.10.

Raw samples, allocations, JPEG structure matching, input hashes, settings and host identity are retained. Exit 0 means selected cases were valid and within their individual bound; it does not mean full parity passed. Exit 1 identifies a stable selected-case bound failure; exit 2 identifies invalid evidence.
