# Workload observations

Separate from the fixed native parity protocol. Each row uses a fresh process, normally three warmups and seven measured batches; the extended-tiering scenario uses 256 warmups. Cold latency excludes process/input preparation. Neither warmup count proves that tiered compilation has stabilized. No speed or tail-latency acceptance verdict is inferred.

| Scenario | Cold ms | Warm median ms | Encodes/s | Workload alloc KiB/encode | Cache peak MiB | Warm phase RSS MiB | MAD |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: |
| photo-array | 157.991 | 4.051 | 246.76 | 1586.3 | 9.0 | 83.3 | 5.1 % |
| photo-stream | 156.858 | 3.713 | 269.14 | 1368.9 | 9.0 | 81.2 | 0.6 % |
| photo-default-tiering | 118.124 | 36.795 | 27.17 | 2691.2 | 9.0 | 78.7 | 4.7 % |
| photo-default-tiering-extended | 118.864 | 3.324 | 300.75 | 1368.9 | 9.0 | 92.2 | 1.2 % |
| tiny | 157.435 | 0.040 | 24235.08 | 35.5 | 0.0 | 79.0 | 8.1 % |
| photograph-9m-444 | 628.656 | 398.080 | 2.51 | 73751.6 | 134.9 | 479.4 | 0.7 % |
| photograph-9m-420 | 264.198 | 71.152 | 14.05 | 20548.1 | 69.0 | 232.4 | 3.4 % |
| planes-444-below | 340.522 | 118.472 | 8.44 | 12004.8 | 128.3 | 274.0 | 0.4 % |
| planes-444-above | 361.082 | 130.053 | 7.69 | 35161.6 | 128.1 | 321.0 | 4.6 % |
| coefficients-444-below | 541.506 | 303.609 | 3.29 | 35171.2 | 134.2 | 564.0 | 0.1 % |
| coefficients-444-above | 553.136 | 311.617 | 3.21 | 65503.7 | 134.8 | 561.6 | 1.9 % |
| coefficients-64m-below | 734.068 | 489.043 | 2.04 | 81641.1 | 157.8 | 774.2 | 1.2 % |
| coefficients-64m-above | 751.764 | 500.513 | 2.00 | 81857.0 | 158.3 | 774.0 | 0.3 % |
| planes-420-below | 334.101 | 116.697 | 8.57 | 9147.7 | 93.2 | 249.6 | 0.8 % |
| planes-420-above | 340.492 | 119.091 | 8.40 | 20704.1 | 117.6 | 247.0 | 0.8 % |
| noise | 257.518 | 65.783 | 15.20 | 9590.2 | 91.5 | 206.2 | 0.3 % |
| rgb | 193.224 | 40.296 | 24.81 | 2668.1 | 22.0 | 103.4 | 0.2 % |
| xyb | 245.807 | 80.506 | 12.42 | 2742.1 | 20.5 | 102.8 | 0.5 % |
| icc | 414.390 | 231.810 | 4.31 | 2555.1 | 18.5 | 100.0 | 0.2 % |
| scaling-workers-1 | 193.949 | 20.427 | 48.95 | 4908.3 | 36.5 | 128.7 | 0.6 % |
| cache-disabled | 389.812 | 214.125 | 4.67 | 80.4 | 0.0 | 77.0 | 0.1 % |
| cache-16m | 287.531 | 99.928 | 10.01 | 113.3 | 16.0 | 91.3 | 0.2 % |
| scaling-workers-2 | 237.076 | 28.378 | 35.23 | 11480.0 | 48.5 | 179.8 | 1.9 % |
| scaling-workers-4 | 236.115 | 26.861 | 37.22 | 16938.8 | 48.5 | 208.8 | 1.9 % |
| scaling-concurrent-2 | 186.704 | 20.613 | 96.17 | 47.1 | 36.5 | 164.0 | 1.0 % |
| scaling-concurrent-4 | 189.066 | 35.034 | 113.35 | 47.0 | 36.5 | 209.3 | 0.6 % |

Noisy observations (all samples retained): photo-array, photo-stream, photo-default-tiering-extended, tiny, scaling-workers-1, scaling-workers-2, scaling-workers-4.


RSS and managed-heap snapshots include caller input and the runtime; neither is exact retained pool capacity. Parent RSS sampling includes setup and final byte-validation allocations and is only a lower bound on peak RSS. Raw samples, cold/warm phase snapshots, hashes, runtime identities and runtime configurations are retained. Rows over 10% MAD or 1.20 p90/p10 are noisy observations, not discarded or retried. These simple dispersion bounds are not timing acceptance. The seven-batch sample does not estimate service p95/p99. Concurrent throughput includes task scheduling, while encode latency is recorded separately. Stream timing uses a counting discard sink, without I/O or retained JPEG storage.
Workload allocation traffic includes encoder, result wrappers, diagnostics, options copies and task scheduling; it is not an encoder-only allocation measurement. Cache peak is accepted storage owned by one encode, not the process or concurrent sum. Instrumentation is present in all timed calls; this report does not replace uninstrumented native acceptance.
