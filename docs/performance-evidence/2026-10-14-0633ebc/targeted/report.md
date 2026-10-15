# Targeted performance diagnostic

Status: selected-case-bound-exceeded. 4/4 selected cases measured. **Full performance parity is not evaluated.**
Tested revision: `0633ebc68a4334ef300c93a61accc849e731694e`.

7 alternating blocks, 5 untimed warmups per block, 3..100 shared iterations. The selected-case latency bound is managed/native <=1.10. No group or whole-corpus acceptance verdict is inferred.

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 7.701 | 4.481 | 1.719 | 0.8 % | 0.2 % |
| kodim01/444/p2/d1 | 14.199 | 11.056 | 1.284 | 1.0 % | 0.7 % |
| kodim01/420/p0/d1 | 5.546 | 3.513 | 1.579 | 1.1 % | 0.2 % |
| kodim01/420/p2/d1 | 10.152 | 8.295 | 1.224 | 0.5 % | 0.3 % |

- kodim01/444/p0/d1: selected-case median latency ratio 1.718675 exceeds 1.10.

- kodim01/444/p2/d1: selected-case median latency ratio 1.284271 exceeds 1.10.

- kodim01/420/p0/d1: selected-case median latency ratio 1.578621 exceeds 1.10.

- kodim01/420/p2/d1: selected-case median latency ratio 1.223859 exceeds 1.10.

Raw samples, allocations, JPEG structure matching, input hashes, settings and host identity are retained. Exit 0 means selected cases were valid and within their individual bound; it does not mean full parity passed. Exit 1 identifies a stable selected-case bound failure; exit 2 identifies invalid evidence.
