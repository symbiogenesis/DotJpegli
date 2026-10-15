# Targeted performance diagnostic

Status: selected-case-bound-exceeded. 4/4 selected cases measured. **Full performance parity is not evaluated.**
Tested revision: `470102aa461fe019fc0bda19d48df07dde94be13`.

7 alternating blocks, 5 untimed warmups per block, 3..100 shared iterations. The selected-case latency bound is managed/native <=1.10. No group or whole-corpus acceptance verdict is inferred.

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 8.399 | 5.542 | 1.515 | 0.2 % | 0.3 % |
| kodim01/444/p2/d1 | 16.835 | 14.455 | 1.165 | 3.6 % | 0.2 % |
| kodim01/420/p0/d1 | 6.012 | 4.428 | 1.358 | 0.1 % | 0.2 % |
| kodim01/420/p2/d1 | 11.002 | 10.788 | 1.020 | 1.4 % | 0.2 % |

- kodim01/444/p0/d1: selected-case median latency ratio 1.515339 exceeds 1.10.

- kodim01/444/p2/d1: selected-case median latency ratio 1.164655 exceeds 1.10.

- kodim01/420/p0/d1: selected-case median latency ratio 1.357663 exceeds 1.10.

Raw samples, allocations, JPEG structure matching, input hashes, settings and host identity are retained. Exit 0 means selected cases were valid and within their individual bound; it does not mean full parity passed. Exit 1 identifies a stable selected-case bound failure; exit 2 identifies invalid evidence.
