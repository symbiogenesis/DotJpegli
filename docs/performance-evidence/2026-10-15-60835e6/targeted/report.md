# Targeted performance diagnostic

Status: selected-case-bound-exceeded. 4/4 selected cases measured. **Full performance parity is not evaluated.**
Tested revision: `60835e6ad1613627fe852bd7678a5ef165ec3b43`.

7 alternating blocks, 5 untimed warmups per block, 3..100 shared iterations. The selected-case latency bound is managed/native <=1.10. No group or whole-corpus acceptance verdict is inferred.

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 6.613 | 5.747 | 1.151 | 1.2 % | 0.3 % |
| kodim01/444/p2/d1 | 16.422 | 13.664 | 1.202 | 2.3 % | 0.8 % |
| kodim01/420/p0/d1 | 4.782 | 4.594 | 1.041 | 0.6 % | 0.4 % |
| kodim01/420/p2/d1 | 11.982 | 10.834 | 1.106 | 0.2 % | 0.2 % |

- kodim01/444/p0/d1: selected-case median latency ratio 1.150606 exceeds 1.10.

- kodim01/444/p2/d1: selected-case median latency ratio 1.201874 exceeds 1.10.

- kodim01/420/p2/d1: selected-case median latency ratio 1.105901 exceeds 1.10.

Raw samples, allocations, JPEG structure matching, input hashes, settings and host identity are retained. Exit 0 means selected cases were valid and within their individual bound; it does not mean full parity passed. Exit 1 identifies a stable selected-case bound failure; exit 2 identifies invalid evidence.
