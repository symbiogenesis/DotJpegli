# Same-host encoder development comparison

Baseline `2ef3d5fcc12a804c7cd707446091eb74fa9c6476`, current `2fa9afe53325be785bd638c7c4448598d308cb9f`; CPU: AMD EPYC 9V45 96-Core Processor; affinity: 0; Vector128/256/512: True/True/True.

Both uninstrumented diagnostics use identical input and JPEG hashes, the same pinned native executable, CPU affinity, runtime, SIMD capabilities and evidence policy. Revisions are measured separately; their samples are not interleaved. Native-calibrated iteration counts and raw samples are retained. This diagnostic cannot replace the current revision's managed/native bounds or establish whole-corpus parity.

Allocated bytes are the mean per-block managed allocation total divided by encodes per block; pool capacity and peak working set are separate quantities.

| Case | Baseline managed (ms) | Current managed (ms) | Current/baseline managed | Current/baseline native | Baseline allocated B/encode | Current allocated B/encode |
| --- | ---: | ---: | ---: | ---: | ---: | ---: |
| kodim01/420/p0/d1 | 3.500 | 3.445 | 0.984311 | 1.007246 | 268260.7 | 178011.3 |
| kodim01/420/p0/d4 | 2.726 | 2.705 | 0.992103 | 1.000296 | 188949.4 | 98768.0 |
| kodim01/420/p2/d1 | 5.412 | 7.693 | 1.421537 | 1.271254 | 329552.0 | 227504.0 |
| kodim01/420/p2/d4 | 3.366 | 4.964 | 1.474541 | 1.330237 | 252586.8 | 150512.0 |
| kodim01/444/p0/d1 | 4.530 | 4.366 | 0.963776 | 0.998140 | 259371.6 | 195648.0 |
| kodim01/444/p0/d4 | 3.780 | 3.713 | 0.982401 | 0.997185 | 173750.0 | 110111.3 |
| kodim01/444/p2/d1 | 10.272 | 10.240 | 0.996881 | 1.004685 | 322968.0 | 254563.7 |
| kodim01/444/p2/d4 | 7.439 | 7.378 | 0.991761 | 0.985566 | 233976.6 | 165448.0 |
| low-contrast/420/p0/d1 | 0.294 | 0.285 | 0.970882 | 0.999001 | 89549.9 | 58348.4 |
| low-contrast/420/p0/d4 | 0.239 | 0.232 | 0.971995 | 0.994104 | 81765.9 | 50562.9 |
| low-contrast/420/p2/d1 | 0.521 | 0.520 | 0.999455 | 0.997017 | 146817.0 | 111296.8 |
| low-contrast/420/p2/d4 | 0.383 | 0.377 | 0.983964 | 0.966220 | 127752.8 | 92230.4 |
| low-contrast/444/p0/d1 | 0.405 | 0.400 | 0.986489 | 1.007743 | 83685.9 | 61652.4 |
| low-contrast/444/p0/d4 | 0.330 | 0.325 | 0.986366 | 0.995781 | 73666.9 | 51634.9 |
| low-contrast/444/p2/d1 | 0.769 | 0.762 | 0.991052 | 0.993349 | 154391.8 | 130439.6 |
| low-contrast/444/p2/d4 | 0.584 | 0.582 | 0.996318 | 0.998603 | 127727.8 | 103774.1 |
| low-noise/420/p0/d1 | 0.308 | 0.297 | 0.963256 | 0.992504 | 85108.4 | 53906.9 |
| low-noise/420/p0/d4 | 0.224 | 0.220 | 0.979871 | 0.987097 | 74492.4 | 43290.9 |
| low-noise/420/p2/d1 | 0.531 | 0.529 | 0.996066 | 0.993821 | 136567.9 | 101047.6 |
| low-noise/420/p2/d4 | 0.340 | 0.338 | 0.992937 | 0.991160 | 114879.4 | 79357.9 |
| low-noise/444/p0/d1 | 0.453 | 0.441 | 0.972482 | 1.001220 | 86422.6 | 64387.2 |
| low-noise/444/p0/d4 | 0.307 | 0.306 | 0.994932 | 0.998768 | 71772.4 | 49738.9 |
| low-noise/444/p2/d1 | 0.840 | 0.836 | 0.995186 | 0.999328 | 134545.3 | 110591.8 |
| low-noise/444/p2/d4 | 0.522 | 0.520 | 0.996548 | 0.994601 | 102719.4 | 78764.4 |
| tiny-odd-edges/420/p0/d1 | 0.021 | 0.021 | 0.979529 | 0.971426 | 39834.9 | 36713.5 |
| tiny-odd-edges/420/p0/d4 | 0.021 | 0.021 | 0.985669 | 1.005633 | 39657.5 | 36538.9 |
| tiny-odd-edges/420/p2/d1 | 0.040 | 0.040 | 0.987372 | 0.993992 | 68844.4 | 65004.4 |
| tiny-odd-edges/420/p2/d4 | 0.038 | 0.038 | 0.996184 | 0.995435 | 68612.4 | 64772.4 |
| tiny-odd-edges/444/p0/d1 | 0.022 | 0.022 | 0.978700 | 0.999981 | 39138.9 | 36905.5 |
| tiny-odd-edges/444/p0/d4 | 0.022 | 0.022 | 0.987908 | 0.996615 | 38842.9 | 36610.9 |
| tiny-odd-edges/444/p2/d1 | 0.043 | 0.043 | 1.003625 | 0.987883 | 66692.4 | 63980.4 |
| tiny-odd-edges/444/p2/d4 | 0.040 | 0.040 | 0.997251 | 0.992504 | 66364.4 | 63652.4 |
