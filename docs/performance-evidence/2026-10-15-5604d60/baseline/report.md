# Targeted performance diagnostic

Status: selected-case-bound-exceeded. 4/4 selected cases measured. **Full performance parity is not evaluated.**
Tested revision: `60835e6ad1613627fe852bd7678a5ef165ec3b43`.

7 alternating blocks, 5 untimed warmups per block, 3..100 shared iterations. The selected-case latency bound is managed/native <=1.10. No group or whole-corpus acceptance verdict is inferred.

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 8.554 | 5.552 | 1.541 | 0.3 % | 0.1 % |
| kodim01/444/p2/d1 | 13.568 | 10.956 | 1.238 | 3.9 % | 0.7 % |
| kodim01/420/p0/d1 | 4.022 | 3.522 | 1.142 | 0.6 % | 0.2 % |
| kodim01/420/p2/d1 | 9.421 | 8.337 | 1.130 | 0.3 % | 0.1 % |

- kodim01/444/p0/d1: selected-case median latency ratio 1.540751 exceeds 1.10.

- kodim01/444/p2/d1: selected-case median latency ratio 1.238362 exceeds 1.10.

- kodim01/420/p0/d1: selected-case median latency ratio 1.142069 exceeds 1.10.

- kodim01/420/p2/d1: selected-case median latency ratio 1.129939 exceeds 1.10.

Raw samples, allocations, JPEG structure matching, input hashes, settings and host identity are retained. Exit 0 means selected cases were valid and within their individual bound; it does not mean full parity passed. Exit 1 identifies a stable selected-case bound failure; exit 2 identifies invalid evidence.
