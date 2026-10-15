# Targeted performance diagnostic

Status: selected-case-bound-exceeded. 4/4 selected cases measured. **Full performance parity is not evaluated.**
Tested revision: `60835e6ad1613627fe852bd7678a5ef165ec3b43`.

7 alternating blocks, 5 untimed warmups per block, 3..100 shared iterations. The selected-case latency bound is managed/native <=1.10. No group or whole-corpus acceptance verdict is inferred.

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 8.564 | 5.566 | 1.539 | 0.3 % | 0.2 % |
| kodim01/444/p2/d1 | 19.333 | 14.448 | 1.338 | 0.0 % | 0.5 % |
| kodim01/420/p0/d1 | 6.077 | 4.443 | 1.368 | 0.1 % | 0.3 % |
| kodim01/420/p2/d1 | 13.733 | 10.851 | 1.266 | 0.7 % | 0.3 % |

- kodim01/444/p0/d1: selected-case median latency ratio 1.538623 exceeds 1.10.

- kodim01/444/p2/d1: selected-case median latency ratio 1.338128 exceeds 1.10.

- kodim01/420/p0/d1: selected-case median latency ratio 1.367625 exceeds 1.10.

- kodim01/420/p2/d1: selected-case median latency ratio 1.265671 exceeds 1.10.

Raw samples, allocations, JPEG structure matching, input hashes, settings and host identity are retained. Exit 0 means selected cases were valid and within their individual bound; it does not mean full parity passed. Exit 1 identifies a stable selected-case bound failure; exit 2 identifies invalid evidence.
