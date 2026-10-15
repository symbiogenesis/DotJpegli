# Targeted performance diagnostic

Status: selected-cases-within-bound. 4/4 selected cases measured. **Full performance parity is not evaluated.**
Tested revision: `5604d601ceda2b9600aeff153a1ee65a4e9774cf`.

7 alternating blocks, 5 untimed warmups per block, 3..100 shared iterations. The selected-case latency bound is managed/native <=1.10. No group or whole-corpus acceptance verdict is inferred.

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 3.823 | 4.508 | 0.848 | 1.8 % | 0.1 % |
| kodim01/444/p2/d1 | 9.109 | 10.865 | 0.838 | 0.4 % | 0.2 % |
| kodim01/420/p0/d1 | 2.852 | 3.518 | 0.811 | 0.9 % | 0.3 % |
| kodim01/420/p2/d1 | 6.290 | 8.308 | 0.757 | 0.8 % | 0.2 % |

Raw samples, allocations, JPEG structure matching, input hashes, settings and host identity are retained. Exit 0 means selected cases were valid and within their individual bound; it does not mean full parity passed. Exit 1 identifies a stable selected-case bound failure; exit 2 identifies invalid evidence.
