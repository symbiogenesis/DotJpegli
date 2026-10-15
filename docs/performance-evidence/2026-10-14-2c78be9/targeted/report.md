# Targeted performance diagnostic

Status: selected-case-bound-exceeded. 4/4 selected cases measured. **Full performance parity is not evaluated.**
Tested revision: `2c78be9060b64e537373d31d13300ee6cb4716b3`.

7 alternating blocks, 5 untimed warmups per block, 3..100 shared iterations. The selected-case latency bound is managed/native <=1.10. No group or whole-corpus acceptance verdict is inferred.

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 8.380 | 5.733 | 1.462 | 0.8 % | 0.2 % |
| kodim01/444/p2/d1 | 17.236 | 13.654 | 1.262 | 0.9 % | 0.1 % |
| kodim01/420/p0/d1 | 6.017 | 4.654 | 1.293 | 0.7 % | 0.9 % |
| kodim01/420/p2/d1 | 12.447 | 10.321 | 1.206 | 0.2 % | 0.3 % |

- kodim01/444/p0/d1: selected-case median latency ratio 1.461830 exceeds 1.10.

- kodim01/444/p2/d1: selected-case median latency ratio 1.262289 exceeds 1.10.

- kodim01/420/p0/d1: selected-case median latency ratio 1.292829 exceeds 1.10.

- kodim01/420/p2/d1: selected-case median latency ratio 1.205974 exceeds 1.10.

Raw samples, allocations, JPEG structure matching, input hashes, settings and host identity are retained. Exit 0 means selected cases were valid and within their individual bound; it does not mean full parity passed. Exit 1 identifies a stable selected-case bound failure; exit 2 identifies invalid evidence.
