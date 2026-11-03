# Same-host encoder development comparison

Baseline `2ef3d5fcc12a804c7cd707446091eb74fa9c6476`, current `67d5724b11de0c249577f5d7c11ed861a37eee38`; CPU: AMD EPYC 7763 64-Core Processor; affinity: 0; Vector128/256/512: True/True/False.

Both uninstrumented diagnostics use identical input and JPEG hashes, the same pinned native executable, CPU affinity, runtime, SIMD capabilities and evidence policy. Revisions are measured separately; their samples are not interleaved. Native-calibrated iteration counts and raw samples are retained. This diagnostic cannot replace the current revision's managed/native bounds or establish whole-corpus parity.

Allocated bytes are the mean per-block managed allocation total divided by encodes per block; pool capacity and peak working set are separate quantities.

| Case | Baseline managed (ms) | Current managed (ms) | Current/baseline managed | Current/baseline native | Baseline allocated B/encode | Current allocated B/encode |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| kodim01/420/p0/d1 | 4.301 | 4.074 | 0.947248 | 0.996314 | 268240.0 | 178072.0 |
| kodim01/420/p0/d4 | 3.414 | 3.360 | 0.984325 | 0.998621 | 189015.8 | 98840.0 |
| kodim01/420/p2/d1 | 8.308 | 8.189 | 0.985712 | 0.992674 | 329581.5 | 227504.0 |
| kodim01/420/p2/d4 | 5.703 | 5.811 | 1.018957 | 1.029448 | 252576.4 | 150528.4 |
| kodim01/444/p0/d1 | 5.504 | 5.360 | 0.973958 | 1.001676 | 259328.0 | 195720.0 |
| kodim01/444/p0/d4 | 4.620 | 4.638 | 1.003804 | 1.001575 | 173835.4 | 110168.0 |
| kodim01/444/p2/d1 | 11.005 | 10.995 | 0.999054 | 1.000554 | 323004.9 | 254464.0 |
| kodim01/444/p2/d4 | 8.344 | 8.276 | 0.991903 | 0.986408 | 233976.6 | 165552.9 |
| low-contrast/420/p0/d1 | 0.557 | 0.539 | 0.967903 | 0.996057 | 89621.9 | 58425.0 |
| low-contrast/420/p0/d4 | 0.471 | 0.463 | 0.982940 | 0.990865 | 81836.4 | 50636.4 |
| low-contrast/420/p2/d1 | 0.955 | 0.949 | 0.993158 | 0.994066 | 146815.4 | 111292.9 |
| low-contrast/420/p2/d4 | 0.714 | 0.706 | 0.988849 | 1.004092 | 127752.2 | 92228.7 |
| low-contrast/444/p0/d1 | 0.783 | 0.769 | 0.982076 | 1.007538 | 83758.0 | 61724.5 |
| low-contrast/444/p0/d4 | 0.654 | 0.653 | 0.997665 | 1.002004 | 73740.4 | 51704.0 |
| low-contrast/444/p2/d1 | 1.396 | 1.400 | 1.003099 | 1.001866 | 154407.4 | 130442.3 |
| low-contrast/444/p2/d4 | 1.087 | 1.086 | 0.998782 | 1.004673 | 127729.5 | 103775.0 |
| low-noise/420/p0/d1 | 0.571 | 0.546 | 0.955146 | 0.997680 | 85181.0 | 53979.3 |
| low-noise/420/p0/d4 | 0.442 | 0.435 | 0.983849 | 1.002705 | 74565.9 | 43362.9 |
| low-noise/420/p2/d1 | 0.944 | 0.939 | 0.994405 | 1.000631 | 136569.6 | 101049.4 |
| low-noise/420/p2/d4 | 0.627 | 0.625 | 0.996490 | 1.001009 | 114879.4 | 79356.4 |
| low-noise/444/p0/d1 | 0.800 | 0.780 | 0.975567 | 0.995747 | 86494.3 | 64458.1 |
| low-noise/444/p0/d4 | 0.593 | 0.597 | 1.005768 | 1.003738 | 71844.4 | 49810.9 |
| low-noise/444/p2/d1 | 1.442 | 1.439 | 0.997778 | 1.000001 | 134545.5 | 110584.0 |
| low-noise/444/p2/d4 | 0.964 | 0.960 | 0.995786 | 0.996472 | 102718.4 | 78762.1 |
| tiny-odd-edges/420/p0/d1 | 0.036 | 0.036 | 0.994197 | 1.000844 | 39906.9 | 36786.9 |
| tiny-odd-edges/420/p0/d4 | 0.038 | 0.036 | 0.945705 | 1.003978 | 39730.9 | 36609.5 |
| tiny-odd-edges/420/p2/d1 | 0.068 | 0.068 | 1.004745 | 1.001191 | 68844.4 | 65004.4 |
| tiny-odd-edges/420/p2/d4 | 0.064 | 0.064 | 1.003283 | 0.994705 | 68612.4 | 64772.4 |
| tiny-odd-edges/444/p0/d1 | 0.039 | 0.038 | 0.986185 | 0.999517 | 39210.9 | 36978.9 |
| tiny-odd-edges/444/p0/d4 | 0.039 | 0.038 | 0.989468 | 0.995082 | 38913.5 | 36681.5 |
| tiny-odd-edges/444/p2/d1 | 0.074 | 0.076 | 1.036375 | 0.997748 | 66692.4 | 63980.4 |
| tiny-odd-edges/444/p2/d4 | 0.069 | 0.070 | 1.002149 | 0.993991 | 66364.4 | 63650.9 |
