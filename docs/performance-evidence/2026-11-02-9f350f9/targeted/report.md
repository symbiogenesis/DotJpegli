# Targeted performance diagnostic

Status: selected-cases-within-bound. 4/4 selected cases measured. **Full performance parity is not evaluated.**
Tested revision: `9f350f9d7f1d8ed3d527d7efad08364a1645f420`.

7 alternating blocks, 5 untimed warmups per block, 3..100 shared iterations. The selected-case latency bound is managed/native <=1.10. No group or whole-corpus acceptance verdict is inferred.

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 5.383 | 5.108 | 1.054 | 1.2 % | 0.2 % |
| kodim01/444/p2/d1 | 11.814 | 12.752 | 0.926 | 0.1 % | 0.3 % |
| kodim01/420/p0/d1 | 4.185 | 4.093 | 1.022 | 4.7 % | 0.5 % |
| kodim01/420/p2/d1 | 8.275 | 9.951 | 0.832 | 0.2 % | 0.3 % |

Raw samples, allocations, JPEG structure matching, input hashes, settings and host identity are retained. Exit 0 means selected cases were valid and within their individual bound; it does not mean full parity passed. Exit 1 identifies a stable selected-case bound failure; exit 2 identifies invalid evidence.
