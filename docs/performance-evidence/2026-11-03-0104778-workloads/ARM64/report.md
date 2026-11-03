# Workload observations

Separate from the fixed native parity protocol. Each row uses a fresh process, normally three warmups and seven measured batches; the extended-tiering scenario uses 256 warmups. Cold latency excludes process/input preparation. Neither warmup count proves that tiered compilation has stabilized. No speed or tail-latency acceptance verdict is inferred.

| Scenario | Cold ms | Warm median ms | Encodes/s | Workload alloc KiB/encode | Cache peak MiB | Warm phase RSS MiB | MAD |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| photo-array | 179.189 | 8.100 | 123.34 | 2977.2 | 9.0 | 87.9 | 1.5 % |
| photo-stream | 177.339 | 7.956 | 125.62 | 2689.6 | 9.0 | 85.5 | 1.1 % |
| photo-default-tiering | 169.269 | 73.562 | 13.59 | 2689.6 | 9.0 | 79.9 | 13.5 % |
| photo-default-tiering-extended | 165.018 | 7.106 | 140.67 | 1367.3 | 9.0 | 90.9 | 0.5 % |
| tiny | 162.874 | 0.049 | 18496.04 | 34.2 | 0.0 | 77.7 | 7.8 % |
| photograph-9m-444 | 755.216 | 531.207 | 1.88 | 74024.1 | 134.9 | 454.0 | 3.1 % |
| photograph-9m-420 | 321.432 | 128.636 | 7.77 | 20546.9 | 69.0 | 266.1 | 1.5 % |
| planes-444-below | 405.975 | 175.074 | 5.71 | 12003.1 | 128.3 | 272.7 | 0.1 % |
| planes-444-above | 415.099 | 186.945 | 5.35 | 37680.7 | 128.1 | 354.7 | 2.7 % |
| coefficients-444-below | 645.816 | 418.801 | 2.39 | 35169.5 | 134.2 | 562.7 | 0.1 % |
| coefficients-444-above | 653.881 | 424.517 | 2.36 | 65503.5 | 134.8 | 612.9 | 2.3 % |
| coefficients-64m-below | 900.575 | 671.138 | 1.49 | 81846.8 | 157.8 | 798.9 | 0.4 % |
| coefficients-64m-above | 900.613 | 680.093 | 1.47 | 81855.8 | 158.3 | 793.5 | 0.1 % |
| planes-420-below | 401.371 | 181.802 | 5.50 | 18153.8 | 93.2 | 309.2 | 0.2 % |
| planes-420-above | 412.092 | 186.790 | 5.35 | 20702.1 | 117.6 | 278.8 | 1.7 % |
| noise | 279.335 | 104.896 | 9.53 | 9588.6 | 91.5 | 205.1 | 0.1 % |
| rgb | 182.939 | 44.154 | 22.65 | 5320.6 | 22.0 | 115.8 | 0.2 % |
| xyb | 275.032 | 134.323 | 7.44 | 2742.1 | 20.5 | 101.0 | 0.0 % |
| icc | 445.072 | 261.794 | 3.82 | 2553.5 | 18.5 | 98.8 | 0.1 % |
| scaling-workers-1 | 205.847 | 40.979 | 24.40 | 9768.1 | 36.5 | 152.9 | 0.6 % |
| cache-disabled | 453.881 | 304.556 | 3.28 | 77.1 | 0.0 | 75.4 | 0.1 % |
| cache-16m | 323.220 | 156.486 | 6.39 | 180.6 | 16.0 | 90.7 | 0.1 % |
| scaling-workers-2 | 241.142 | 40.423 | 24.74 | 11509.8 | 48.5 | 178.8 | 0.2 % |
| scaling-workers-4 | 243.634 | 40.219 | 24.86 | 16969.0 | 48.5 | 208.2 | 2.6 % |
| scaling-concurrent-2 | 207.891 | 41.087 | 48.56 | 45.4 | 36.5 | 163.2 | 0.2 % |
| scaling-concurrent-4 | 214.645 | 41.307 | 94.62 | 45.3 | 36.5 | 208.2 | 0.6 % |

Noisy observations (all samples retained): photo-array, photo-default-tiering, tiny, scaling-workers-2, scaling-workers-4.


RSS and managed-heap snapshots include caller input and the runtime; neither is exact retained pool capacity. Parent RSS sampling includes setup and final byte-validation allocations and is only a lower bound on peak RSS. Raw samples, cold/warm phase snapshots, hashes, runtime identities and runtime configurations are retained. Rows over 10% MAD or 1.20 p90/p10 are noisy observations, not discarded or retried. These simple dispersion bounds are not timing acceptance. The seven-batch sample does not estimate service p95/p99. Concurrent throughput includes task scheduling, while encode latency is recorded separately. Stream timing uses a counting discard sink, without I/O or retained JPEG storage.
Workload allocation traffic includes encoder, result wrappers, diagnostics, options copies and task scheduling; it is not an encoder-only allocation measurement. Cache peak is accepted storage owned by one encode, not the process or concurrent sum. Instrumentation is present in all timed calls; this report does not replace uninstrumented native acceptance.
