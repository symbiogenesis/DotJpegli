# Same-host encoder development comparison

Baseline `2ef3d5fcc12a804c7cd707446091eb74fa9c6476`, current `2fa9afe53325be785bd638c7c4448598d308cb9f`; CPU: AMD EPYC 9V45 96-Core Processor; affinity: 0; Vector128/256/512: True/True/True.

Both uninstrumented diagnostics use identical input and JPEG hashes, the same pinned native executable, CPU affinity, runtime, SIMD capabilities and evidence policy. Revisions are measured separately; their samples are not interleaved. Native-calibrated iteration counts and raw samples are retained. This diagnostic cannot replace the current revision's managed/native bounds or establish whole-corpus parity.

Allocated bytes are the mean per-block managed allocation total divided by encodes per block; pool capacity and peak working set are separate quantities.

| Case | Baseline managed (ms) | Current managed (ms) | Current/baseline managed | Current/baseline native | Baseline allocated B/encode | Current allocated B/encode |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| kodim01/420/p0/d1 | 2.317 | 2.299 | 0.992549 | 1.005667 | 268177.2 | 178041.1 |
| kodim01/420/p0/d4 | 1.835 | 1.825 | 0.994353 | 1.002236 | 188946.9 | 98768.0 |
| kodim01/420/p2/d1 | 5.431 | 5.409 | 0.995943 | 0.999408 | 329552.0 | 227504.0 |
| kodim01/420/p2/d4 | 3.374 | 3.371 | 0.999216 | 1.004407 | 252560.0 | 150525.4 |
| kodim01/444/p0/d1 | 3.001 | 2.920 | 0.973074 | 1.004665 | 259334.5 | 195844.3 |
| kodim01/444/p0/d4 | 2.511 | 2.426 | 0.966246 | 0.995589 | 173726.7 | 110108.4 |
| kodim01/444/p2/d1 | 7.095 | 7.122 | 1.003855 | 0.983709 | 322968.0 | 254464.0 |
| kodim01/444/p2/d4 | 4.838 | 4.858 | 1.004190 | 0.999035 | 233952.0 | 165466.4 |
| low-contrast/420/p0/d1 | 0.294 | 0.285 | 0.969335 | 0.992942 | 89548.4 | 58348.4 |
| low-contrast/420/p0/d4 | 0.240 | 0.233 | 0.969392 | 0.996325 | 81764.4 | 50561.5 |
| low-contrast/420/p2/d1 | 0.536 | 0.522 | 0.973298 | 0.968047 | 146817.7 | 111295.1 |
| low-contrast/420/p2/d4 | 0.381 | 0.379 | 0.992923 | 1.000324 | 127751.4 | 92229.9 |
| low-contrast/444/p0/d1 | 0.407 | 0.400 | 0.982834 | 1.003610 | 83685.9 | 61652.4 |
| low-contrast/444/p0/d4 | 0.329 | 0.327 | 0.993873 | 0.999505 | 73668.4 | 51634.9 |
| low-contrast/444/p2/d1 | 0.765 | 0.774 | 1.012192 | 0.999368 | 154391.6 | 130442.3 |
| low-contrast/444/p2/d4 | 0.581 | 0.582 | 1.000594 | 0.999469 | 127729.1 | 103774.2 |
| low-noise/420/p0/d1 | 0.304 | 0.296 | 0.973953 | 0.995167 | 85109.9 | 53905.5 |
| low-noise/420/p0/d4 | 0.222 | 0.220 | 0.990719 | 1.002389 | 74492.4 | 43289.5 |
| low-noise/420/p2/d1 | 0.539 | 0.530 | 0.981902 | 0.985500 | 136567.6 | 101047.8 |
| low-noise/420/p2/d4 | 0.341 | 0.333 | 0.976817 | 1.007108 | 114877.9 | 79356.4 |
| low-noise/444/p0/d1 | 0.448 | 0.441 | 0.984181 | 0.999389 | 86422.5 | 64388.9 |
| low-noise/444/p0/d4 | 0.306 | 0.304 | 0.994083 | 0.998457 | 71770.9 | 49738.9 |
| low-noise/444/p2/d1 | 0.837 | 0.836 | 0.998010 | 0.998052 | 134544.0 | 110593.0 |
| low-noise/444/p2/d4 | 0.519 | 0.530 | 1.021820 | 1.006799 | 102719.5 | 78765.9 |
| tiny-odd-edges/420/p0/d1 | 0.021 | 0.021 | 0.982381 | 1.010350 | 39834.9 | 36714.9 |
| tiny-odd-edges/420/p0/d4 | 0.021 | 0.021 | 0.995297 | 1.013712 | 39658.9 | 36538.9 |
| tiny-odd-edges/420/p2/d1 | 0.041 | 0.040 | 0.979627 | 1.008451 | 68844.4 | 65002.9 |
| tiny-odd-edges/420/p2/d4 | 0.038 | 0.038 | 0.990070 | 1.012145 | 68610.9 | 64770.9 |
| tiny-odd-edges/444/p0/d1 | 0.022 | 0.022 | 0.999238 | 1.016992 | 39137.5 | 36906.9 |
| tiny-odd-edges/444/p0/d4 | 0.022 | 0.021 | 0.986782 | 1.001542 | 38842.9 | 36609.5 |
| tiny-odd-edges/444/p2/d1 | 0.044 | 0.043 | 0.974103 | 1.003872 | 66692.4 | 63978.9 |
| tiny-odd-edges/444/p2/d4 | 0.043 | 0.040 | 0.924524 | 0.996572 | 66364.4 | 63650.9 |
