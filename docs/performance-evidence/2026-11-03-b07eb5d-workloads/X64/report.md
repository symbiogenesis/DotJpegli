# Workload observations

Separate from the fixed native parity protocol. Each row uses a fresh process, three warmups and seven warmed batches. Cold latency excludes process/input preparation. No speed or tail-latency acceptance verdict is inferred.

| Scenario | Cold ms | Warm median ms | Encodes/s | Workload alloc KiB/encode | Cache peak MiB | Warm phase RSS MiB | MAD |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| photo-array | 224.310 | 5.557 | 179.89 | 1732.8 | 12.0 | 86.6 | 1.6 % |
| photo-stream | 209.636 | 5.316 | 188.02 | 1515.4 | 12.0 | 84.1 | 1.1 % |
| photo-default-tiering | 165.837 | 33.767 | 29.61 | 1515.4 | 12.0 | 82.4 | 0.7 % |
| tiny | 208.749 | 0.051 | 18545.35 | 35.6 | 0.0 | 78.4 | 6.3 % |
| photograph-9m-444 | 1024.132 | 769.149 | 1.30 | 41452.9 | 71.2 | 357.1 | 0.6 % |
| photograph-9m-420 | 791.217 | 548.062 | 1.82 | 6120.0 | 57.0 | 159.2 | 0.5 % |
| planes-444-below | 423.685 | 150.479 | 6.65 | 12004.8 | 128.3 | 273.8 | 0.4 % |
| planes-444-above | 705.325 | 450.618 | 2.22 | 2642.4 | 64.3 | 146.1 | 0.4 % |
| coefficients-444-below | 942.999 | 670.728 | 1.49 | 2654.5 | 70.5 | 181.0 | 0.1 % |
| coefficients-444-above | 950.698 | 689.789 | 1.45 | 32986.3 | 71.1 | 323.2 | 0.4 % |
| coefficients-64m-below | 1182.097 | 911.833 | 1.10 | 49122.9 | 94.0 | 435.5 | 1.2 % |
| coefficients-64m-above | 1200.134 | 920.046 | 1.09 | 49132.0 | 94.5 | 436.3 | 0.9 % |
| planes-420-below | 405.884 | 149.812 | 6.67 | 9147.7 | 93.2 | 249.3 | 0.5 % |
| planes-420-above | 790.115 | 544.822 | 1.84 | 6213.7 | 69.2 | 160.8 | 0.2 % |
| noise | 330.133 | 90.039 | 11.11 | 23814.3 | 139.5 | 295.3 | 3.1 % |
| rgb | 240.324 | 53.804 | 18.58 | 2668.0 | 22.0 | 103.0 | 0.8 % |
| xyb | 298.223 | 103.057 | 9.70 | 5468.4 | 20.5 | 117.2 | 0.2 % |
| icc | 556.199 | 324.333 | 3.08 | 2847.9 | 24.5 | 107.4 | 0.2 % |
| scaling-workers-1 | 242.813 | 27.854 | 35.90 | 5493.6 | 48.5 | 144.5 | 0.4 % |
| cache-disabled | 490.591 | 282.828 | 3.54 | 80.5 | 0.0 | 76.6 | 0.1 % |
| cache-16m | 362.009 | 147.987 | 6.76 | 804.7 | 16.0 | 95.6 | 0.1 % |
| scaling-workers-2 | 287.426 | 38.917 | 25.69 | 6025.3 | 48.5 | 150.4 | 1.8 % |
| scaling-workers-4 | 285.159 | 38.363 | 26.06 | 6036.3 | 48.5 | 150.6 | 0.5 % |
| scaling-concurrent-2 | 243.726 | 28.714 | 69.43 | 47.2 | 48.5 | 192.0 | 0.4 % |
| scaling-concurrent-4 | 243.154 | 49.405 | 79.82 | 47.1 | 48.5 | 257.4 | 1.0 % |

RSS and managed-heap snapshots include caller input and the runtime; neither is exact retained pool capacity. Parent RSS sampling includes setup and final byte-validation allocations and is only a lower bound on peak RSS. Raw samples, cold/warm phase snapshots, hashes, runtime identities and runtime configurations are retained. Rows over 10% MAD are noisy observations, not discarded or retried. The seven-batch sample does not estimate service p95/p99. Concurrent throughput includes task scheduling, while encode latency is recorded separately. Stream timing uses a counting discard sink, without I/O or retained JPEG storage.
Workload allocation traffic includes encoder, result wrappers, diagnostics, options copies and task scheduling; it is not an encoder-only allocation measurement. Cache peak is accepted storage owned by one encode, not the process or concurrent sum. Instrumentation is present in all timed calls; this report does not replace uninstrumented native acceptance.
