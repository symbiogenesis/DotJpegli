# Interleaved revision comparison

Persistent baseline and current workers run serially as balanced ABBA/BAAB quartets with identical fixed iterations, warmups, runtime, CPU affinity, encoding policy, inputs and JPEG hashes. Startup and IPC are outside encode timing. Ratios below1 favor current. Allocations are mean bytes per encode; retained rental capacity and peak working memory are separate measurements.

MAD, repeated-leg drift, p90/p10 minority-state drift and order bias guard the timing evidence. Invalid rows retain raw measurements and cannot support a speed claim. Valid rows remain inconclusive unless every quartet differs by more than 0.5% in the same direction. This comparison has no native parity verdict.

| Case | Current/baseline | Order bias | Maximum repeat drift | Baseline p90/p10 | Current p90/p10 | Baseline B/encode | Current B/encode | Valid | Preference | Selected attempt |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- | ---: |
| kodim01/444/p0/d1 | 1.067557 | 1.002710 | 1.082142 | 1.010314 | 1.008029 | 195599.566667 | 195614.066667 | True | baseline | 1 |
| kodim01/444/p0/d4 | 1.098170 | 1.000125 | 1.040275 | 1.027229 | 1.026749 | 110052.200000 | 110052.200000 | True | baseline | 1 |
| kodim01/444/p2/d1 | 1.039446 | 1.002182 | 1.012356 | 1.010936 | 1.012346 | 254922.200000 | 254922.300000 | True | baseline | 1 |
| kodim01/444/p2/d4 | 1.056215 | 1.001249 | 1.005010 | 1.005010 | 1.005436 | 165892.900000 | 165892.900000 | True | baseline | 1 |
| kodim01/420/p0/d1 | 1.089164 | 1.003853 | 1.031085 | 1.018327 | 1.036842 | 177989.633333 | 177993.933333 | True | baseline | 1 |
| kodim01/420/p0/d4 | 1.110740 | 1.000525 | 1.008732 | 1.008525 | 1.008870 | 98704.600000 | 98704.600000 | True | baseline | 1 |
| kodim01/420/p2/d1 | 1.042531 | 1.010782 | 1.020967 | 1.010373 | 1.017136 | 228042.600000 | 228021.200000 | True | baseline | 1 |
| kodim01/420/p2/d4 | 1.070757 | 1.000617 | 1.237848 | 1.015529 | 1.003550 | 151020.900000 | 151020.900000 | False | inconclusive | 1 |
| kodim02/444/p0/d1 | 1.116995 | 1.010026 | 1.038158 | 1.004464 | 1.015929 | 143171.866667 | 143167.466667 | True | baseline | 1 |
| kodim02/444/p0/d4 | 1.096173 | 1.001036 | 1.011179 | 1.006152 | 1.006171 | 80992.600000 | 80992.600000 | True | baseline | 1 |
| kodim02/444/p2/d1 | 1.070504 | 1.000006 | 1.021701 | 1.006286 | 1.010907 | 222518.000000 | 222517.900000 | True | baseline | 1 |
| kodim02/444/p2/d4 | 1.060166 | 1.001574 | 1.009956 | 1.006368 | 1.005503 | 138992.600000 | 138988.300000 | True | baseline | 1 |
| kodim02/420/p0/d1 | 1.122693 | 1.000383 | 1.015236 | 1.009097 | 1.008575 | 122340.966667 | 122304.600000 | True | baseline | 1 |
| kodim02/420/p0/d4 | 1.106481 | 1.003057 | 1.011008 | 1.005737 | 1.007362 | 68428.300000 | 68428.300000 | True | baseline | 1 |
| kodim02/420/p2/d1 | 1.075746 | 1.001412 | 1.005814 | 1.002858 | 1.003966 | 185716.900000 | 185716.900000 | True | baseline | 1 |
| kodim02/420/p2/d4 | 1.069444 | 1.000590 | 1.045108 | 1.020470 | 1.018906 | 116536.600000 | 116536.600000 | True | baseline | 1 |
| kodim04/444/p0/d1 | 1.119561 | 1.011216 | 1.024057 | 1.022495 | 1.006263 | 143831.133333 | 143784.000000 | True | baseline | 1 |
| kodim04/444/p0/d4 | 1.110744 | 1.000121 | 1.278888 | 1.013739 | 1.010666 | 84880.600000 | 84880.600000 | False | inconclusive | 1 |
| kodim04/444/p2/d1 | 1.069650 | 1.009428 | 1.014268 | 1.009568 | 1.016724 | 204275.400000 | 204266.300000 | True | baseline | 1 |
| kodim04/444/p2/d4 | 1.062965 | 1.002645 | 1.008032 | 1.007982 | 1.005889 | 136496.600000 | 136496.600000 | True | baseline | 1 |
| kodim04/420/p0/d1 | 1.101027 | 1.002759 | 1.352291 | 1.013329 | 1.009692 | 129228.900000 | 129228.900000 | False | inconclusive | 1 |
| kodim04/420/p0/d4 | 1.093077 | 1.011739 | 1.110500 | 1.023823 | 1.007210 | 77772.300000 | 77772.300000 | False | inconclusive | 1 |
| kodim04/420/p2/d1 | 1.061841 | 1.000265 | 1.006640 | 1.003814 | 1.006059 | 180472.600000 | 180472.600000 | True | baseline | 1 |
| kodim04/420/p2/d4 | 1.067515 | 1.000500 | 1.008765 | 1.003736 | 1.006105 | 125580.900000 | 125580.900000 | True | baseline | 1 |
| kodim22/444/p0/d1 | 1.096228 | 1.008655 | 1.022973 | 1.017062 | 1.024873 | 161283.433333 | 161232.000000 | True | baseline | 1 |
| kodim22/444/p0/d4 | 1.100489 | 1.000413 | 1.009527 | 1.004135 | 1.005345 | 91548.300000 | 91548.300000 | True | baseline | 1 |
| kodim22/444/p2/d1 | 1.056451 | 1.001630 | 1.022621 | 1.009448 | 1.004947 | 209791.700000 | 209773.900000 | True | baseline | 1 |
| kodim22/444/p2/d4 | 1.057656 | 1.002776 | 1.007223 | 1.005851 | 1.006336 | 136832.600000 | 136832.600000 | True | baseline | 1 |
| kodim22/420/p0/d1 | 1.104306 | 1.001591 | 1.018851 | 1.013141 | 1.006654 | 144890.266667 | 144852.300000 | True | baseline | 1 |
| kodim22/420/p0/d4 | 1.107485 | 1.002384 | 1.076529 | 1.006036 | 1.005190 | 83176.600000 | 83176.600000 | True | baseline | 1 |
| kodim22/420/p2/d1 | 1.068522 | 1.000940 | 1.060219 | 1.012256 | 1.007568 | 195474.700000 | 195457.600000 | True | baseline | 1 |
| kodim22/420/p2/d4 | 1.073054 | 1.001926 | 1.006763 | 1.004917 | 1.006065 | 130560.600000 | 130560.600000 | True | baseline | 1 |
| low-contrast/444/p0/d1 | 1.115455 | 1.007403 | 1.026890 | 1.018759 | 1.026869 | 61576.000000 | 61580.300000 | True | baseline | 1 |
| low-contrast/444/p0/d4 | 1.080641 | 1.010952 | 1.027884 | 1.021118 | 1.015922 | 51564.300000 | 51564.300000 | True | baseline | 1 |
| low-contrast/444/p2/d1 | 1.079119 | 1.000173 | 1.016483 | 1.009951 | 1.013441 | 130872.600000 | 130872.600000 | True | baseline | 1 |
| low-contrast/444/p2/d4 | 1.053510 | 1.003030 | 1.029896 | 1.025366 | 1.009198 | 104204.300000 | 104204.300000 | True | baseline | 1 |
| low-contrast/420/p0/d1 | 1.073737 | 1.011532 | 1.625700 | 1.032942 | 1.021237 | 58276.300000 | 58276.300000 | False | inconclusive | 1 |
| low-contrast/420/p0/d4 | 1.067146 | 1.016072 | 1.048216 | 1.010775 | 1.017234 | 50488.000000 | 50492.300000 | True | baseline | 1 |
| low-contrast/420/p2/d1 | 1.069119 | 1.005612 | 1.037835 | 1.014995 | 1.036453 | 111792.600000 | 111792.600000 | True | baseline | 1 |
| low-contrast/420/p2/d4 | 1.048490 | 1.004138 | 1.029868 | 1.023286 | 1.032546 | 92728.600000 | 92728.600000 | True | baseline | 1 |
| low-noise/444/p0/d1 | 1.084107 | 1.006995 | 1.033203 | 1.010144 | 1.025940 | 64316.300000 | 64316.300000 | True | baseline | 1 |
| low-noise/444/p0/d4 | 1.064059 | 1.000883 | 1.059647 | 1.012254 | 1.010283 | 49668.300000 | 49668.300000 | True | baseline | 1 |
| low-noise/444/p2/d1 | 1.082364 | 1.006748 | 1.022245 | 1.014228 | 1.017879 | 111024.600000 | 111024.600000 | True | baseline | 1 |
| low-noise/444/p2/d4 | 1.042458 | 1.001344 | 1.016436 | 1.013445 | 1.009769 | 79196.300000 | 79192.000000 | True | baseline | 1 |
| low-noise/420/p0/d1 | 1.061812 | 1.012748 | 1.024863 | 1.024353 | 1.013964 | 53832.000000 | 53832.000000 | True | baseline | 1 |
| low-noise/420/p0/d4 | 1.047865 | 1.001600 | 1.043273 | 1.016774 | 1.010664 | 43220.300000 | 43216.000000 | True | baseline | 1 |
| low-noise/420/p2/d1 | 1.056188 | 1.001512 | 1.028595 | 1.018564 | 1.026839 | 101544.600000 | 101544.600000 | True | baseline | 1 |
| low-noise/420/p2/d4 | 1.037686 | 1.005809 | 1.033956 | 1.010482 | 1.017302 | 79852.300000 | 79852.300000 | True | baseline | 1 |
| full-noise/444/p0/d1 | 1.062934 | 1.002110 | 1.021641 | 1.012021 | 1.015971 | 110144.600000 | 110140.300000 | True | baseline | 1 |
| full-noise/444/p0/d4 | 1.067943 | 1.004887 | 1.024482 | 1.015350 | 1.010062 | 78016.600000 | 78012.300000 | True | baseline | 1 |
| full-noise/444/p2/d1 | 1.026263 | 1.001977 | 1.127571 | 1.023120 | 1.063311 | 191936.600000 | 191936.600000 | False | inconclusive | 1 |
| full-noise/444/p2/d4 | 1.039211 | 1.006169 | 1.015758 | 1.013691 | 1.012780 | 130324.900000 | 130324.900000 | True | baseline | 1 |
| full-noise/420/p0/d1 | 1.042473 | 1.002806 | 1.020002 | 1.014916 | 1.019474 | 81564.300000 | 81564.300000 | True | baseline | 1 |
| full-noise/420/p0/d4 | 1.047563 | 1.020351 | 1.077609 | 1.030405 | 1.047909 | 61524.300000 | 61524.300000 | True | inconclusive | 1 |
| full-noise/420/p2/d1 | 1.019682 | 1.002268 | 1.014823 | 1.009019 | 1.014617 | 135796.900000 | 135796.900000 | True | baseline | 1 |
| full-noise/420/p2/d4 | 1.035905 | 1.009068 | 1.020892 | 1.013511 | 1.018542 | 109336.600000 | 109336.600000 | True | baseline | 1 |
| tiny-odd-edges/444/p0/d1 | 1.028244 | 1.019174 | 1.088766 | 1.081451 | 1.074935 | 36833.720000 | 36833.720000 | True | inconclusive | 1 |
| tiny-odd-edges/444/p0/d4 | 1.029643 | 1.001728 | 1.082975 | 1.077977 | 1.075796 | 36538.580000 | 36538.580000 | True | baseline | 1 |
| tiny-odd-edges/444/p2/d1 | 1.030344 | 1.007073 | 1.059761 | 1.044394 | 1.041362 | 64411.440000 | 64412.300000 | True | baseline | 1 |
| tiny-odd-edges/444/p2/d4 | 1.020907 | 1.017612 | 1.068609 | 1.067553 | 1.053043 | 64084.300000 | 64084.300000 | True | baseline | 1 |
| tiny-odd-edges/420/p0/d1 | 1.024030 | 1.004944 | 1.073316 | 1.072490 | 1.078897 | 36642.580000 | 36642.580000 | True | baseline | 1 |
| tiny-odd-edges/420/p0/d4 | 1.032827 | 1.019886 | 1.103113 | 1.063106 | 1.076872 | 36465.720000 | 36465.720000 | False | inconclusive | 1 |
| tiny-odd-edges/420/p2/d1 | 1.008315 | 1.008301 | 1.064637 | 1.061643 | 1.051863 | 65500.300000 | 65499.440000 | True | inconclusive | 1 |
| tiny-odd-edges/420/p2/d4 | 1.024683 | 1.022766 | 1.071432 | 1.055204 | 1.061556 | 65267.440000 | 65268.300000 | True | baseline | 1 |

Invalid evidence:

- kodim01/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim04/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim04/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim04/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- low-contrast/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- full-noise/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- full-noise/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4: current: repeated legs within a quartet differ by over10%.
