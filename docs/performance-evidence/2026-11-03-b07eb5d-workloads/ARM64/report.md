# Workload observations

Separate from the fixed native parity protocol. Each row uses a fresh process, three warmups and seven warmed batches. Cold latency excludes process/input preparation. No speed or tail-latency acceptance verdict is inferred.

| Scenario | Cold ms | Warm median ms | Encodes/s | Workload alloc KiB/encode | Cache peak MiB | Warm phase RSS MiB | MAD |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| photo-array | 164.120 | 7.720 | 129.49 | 1584.6 | 9.0 | 82.3 | 0.8 % |
| photo-stream | 160.412 | 7.570 | 132.07 | 1367.2 | 9.0 | 80.5 | 0.3 % |
| photo-default-tiering | 164.412 | 73.640 | 13.58 | 2689.5 | 9.0 | 78.3 | 1.4 % |
| tiny | 151.374 | 0.054 | 17376.19 | 33.8 | 0.0 | 77.2 | 10.2 % |
| photograph-9m-444 | 986.725 | 804.433 | 1.24 | 41689.3 | 71.2 | 255.2 | 0.3 % |
| photograph-9m-420 | 736.562 | 581.852 | 1.72 | 488.0 | 9.0 | 108.2 | 0.0 % |
| planes-444-below | 388.866 | 178.495 | 5.60 | 23886.4 | 128.3 | 348.6 | 0.1 % |
| planes-444-above | 659.317 | 472.644 | 2.12 | 5161.7 | 64.3 | 156.6 | 0.0 % |
| coefficients-444-below | 902.306 | 707.555 | 1.41 | 2652.8 | 70.5 | 180.1 | 0.0 % |
| coefficients-444-above | 911.878 | 722.408 | 1.38 | 32985.1 | 71.1 | 378.5 | 0.0 % |
| coefficients-64m-below | 1146.917 | 953.918 | 1.05 | 49121.4 | 94.0 | 412.5 | 0.7 % |
| coefficients-64m-above | 1162.802 | 965.071 | 1.04 | 49338.4 | 94.5 | 386.1 | 0.6 % |
| planes-420-below | 386.177 | 182.365 | 5.48 | 18153.7 | 93.2 | 309.4 | 0.2 % |
| planes-420-above | 768.656 | 579.281 | 1.73 | 6212.0 | 69.2 | 159.9 | 0.0 % |
| noise | 278.867 | 104.221 | 9.59 | 9588.5 | 91.5 | 205.2 | 0.5 % |
| rgb | 168.608 | 45.042 | 22.20 | 2668.0 | 22.0 | 101.8 | 0.4 % |
| xyb | 261.174 | 134.559 | 7.43 | 2742.1 | 20.5 | 100.9 | 0.0 % |
| icc | 433.976 | 261.516 | 3.82 | 2553.4 | 18.5 | 98.8 | 0.0 % |
| scaling-workers-1 | 197.278 | 40.550 | 24.66 | 9768.1 | 36.5 | 152.7 | 0.2 % |
| cache-disabled | 437.807 | 300.670 | 3.33 | 77.1 | 0.0 | 75.5 | 0.1 % |
| cache-16m | 298.508 | 151.293 | 6.61 | 405.7 | 4.5 | 80.6 | 0.1 % |
| scaling-workers-2 | 227.845 | 40.367 | 24.77 | 11509.8 | 48.5 | 179.0 | 0.3 % |
| scaling-workers-4 | 229.322 | 39.354 | 25.41 | 16968.6 | 48.5 | 208.3 | 1.0 % |
| scaling-concurrent-2 | 200.939 | 40.720 | 49.01 | 45.4 | 36.5 | 163.2 | 0.4 % |
| scaling-concurrent-4 | 200.978 | 40.871 | 95.63 | 1260.6 | 36.5 | 233.6 | 0.4 % |

RSS and managed-heap snapshots include caller input and the runtime; neither is exact retained pool capacity. Parent RSS sampling includes setup and final byte-validation allocations and is only a lower bound on peak RSS. Raw samples, cold/warm phase snapshots, hashes, runtime identities and runtime configurations are retained. Rows over 10% MAD are noisy observations, not discarded or retried. The seven-batch sample does not estimate service p95/p99. Concurrent throughput includes task scheduling, while encode latency is recorded separately. Stream timing uses a counting discard sink, without I/O or retained JPEG storage.
Workload allocation traffic includes encoder, result wrappers, diagnostics, options copies and task scheduling; it is not an encoder-only allocation measurement. Cache peak is accepted storage owned by one encode, not the process or concurrent sum. Instrumentation is present in all timed calls; this report does not replace uninstrumented native acceptance.
