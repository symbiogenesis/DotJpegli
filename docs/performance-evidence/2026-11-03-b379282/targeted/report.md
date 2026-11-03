# Targeted performance diagnostic

Status: invalid. 4/4 selected cases measured. **Full performance parity is not evaluated.**
Tested revision: `b379282ae58dce4243a988c91d48d4d3f037b555`.

7 alternating blocks, 5 untimed warmups per block, 3..100 shared iterations. The selected-case latency bound is managed/native <=1.10. No group or whole-corpus acceptance verdict is inferred.

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 5.268 | 5.588 | 0.943 | 0.9 % | 0.2 % |
| kodim01/444/p2/d1 | 11.402 | 14.343 | 0.795 | 0.1 % | 1.1 % |
| kodim01/420/p0/d1 | 4.701 | 4.391 | 1.071 | 11.9 % | 0.5 % |
| kodim01/420/p2/d1 | 8.459 | 11.159 | 0.758 | 0.3 % | 0.4 % |

- kodim01/420/p0/d1: managed relative MAD exceeds the fixed 10% noise bound.

Raw samples, allocations, JPEG structure matching, input hashes, settings and host identity are retained. Exit 0 means selected cases were valid and within their individual bound; it does not mean full parity passed. Exit 1 identifies a stable selected-case bound failure; exit 2 identifies invalid evidence.
