# Interleaved revision comparison

Persistent baseline and current workers run serially as balanced ABBA/BAAB quartets with identical fixed iterations, warmups, runtime, CPU affinity, encoding policy, inputs and JPEG hashes. Startup and IPC are outside encode timing. Ratios below1 favor current. Allocations are mean bytes per encode; retained rental capacity and peak working memory are separate measurements.

MAD, repeated-leg drift, p90/p10 minority-state drift and order bias guard the timing evidence. Invalid rows retain raw measurements and cannot support a speed claim. Valid rows remain inconclusive unless every quartet differs by more than 0.5% in the same direction. This comparison has no native parity verdict.

| Case | Current/baseline | Order bias | Maximum repeat drift | Baseline p90/p10 | Current p90/p10 | Baseline B/encode | Current B/encode | Valid | Preference | Selected attempt |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- | ---: |
| kodim01/444/p0/d1 | 1.093565 | 1.006446 | 1.132124 | 1.093457 | 1.013854 | 195567.300000 | 195567.666667 | False | inconclusive | 1 |
| kodim01/444/p0/d4 | 1.141046 | 1.001461 | 1.036011 | 1.031274 | 1.026239 | 109977.400000 | 109977.400000 | True | baseline | 1 |
| kodim01/444/p2/d1 | 1.058888 | 1.000472 | 1.039435 | 1.026487 | 1.025150 | 254923.700000 | 254923.700000 | True | baseline | 1 |
| kodim01/444/p2/d4 | 1.087562 | 1.003086 | 1.011107 | 1.005941 | 1.006151 | 165892.900000 | 165892.900000 | True | baseline | 1 |
| kodim01/420/p0/d1 | 1.102550 | 1.006198 | 1.039102 | 1.016538 | 1.028055 | 177879.033333 | 177932.000000 | True | baseline | 1 |
| kodim01/420/p0/d4 | 1.127280 | 1.002122 | 1.036976 | 1.004965 | 1.012430 | 98632.600000 | 98655.466667 | True | baseline | 1 |
| kodim01/420/p2/d1 | 1.058902 | 1.013177 | 1.037678 | 1.009803 | 1.024861 | 228029.900000 | 228071.400000 | True | baseline | 1 |
| kodim01/420/p2/d4 | 1.087713 | 1.000574 | 1.004977 | 1.003882 | 1.004607 | 151020.900000 | 151020.900000 | True | baseline | 1 |
| kodim02/444/p0/d1 | 1.140795 | 1.009538 | 1.039545 | 1.023248 | 1.016758 | 143148.366667 | 143110.366667 | True | baseline | 1 |
| kodim02/444/p0/d4 | 1.134823 | 1.000354 | 1.006865 | 1.006593 | 1.005243 | 80920.600000 | 80920.600000 | True | baseline | 1 |
| kodim02/444/p2/d1 | 1.094751 | 1.002613 | 1.164995 | 1.013832 | 1.008233 | 222524.066667 | 222504.600000 | False | inconclusive | 1 |
| kodim02/444/p2/d4 | 1.106356 | 1.001882 | 1.011696 | 1.004784 | 1.011024 | 138992.600000 | 138992.600000 | True | baseline | 1 |
| kodim02/420/p0/d1 | 1.140702 | 1.001182 | 1.023210 | 1.005787 | 1.008708 | 122269.800000 | 122232.600000 | True | baseline | 1 |
| kodim02/420/p0/d4 | 1.119258 | 1.000275 | 1.105090 | 1.008254 | 1.012525 | 68356.300000 | 68356.300000 | False | inconclusive | 1 |
| kodim02/420/p2/d1 | 1.087024 | 1.006823 | 1.010620 | 1.005143 | 1.006828 | 185716.900000 | 185716.900000 | True | baseline | 1 |
| kodim02/420/p2/d4 | 1.101272 | 1.007891 | 1.486483 | 1.019170 | 1.011584 | 116536.600000 | 116536.600000 | False | inconclusive | 1 |
| kodim04/444/p0/d1 | 1.122995 | 1.012663 | 1.045094 | 1.027112 | 1.017451 | 143790.966667 | 143746.066667 | True | baseline | 1 |
| kodim04/444/p0/d4 | 1.121110 | 1.003230 | 1.010000 | 1.005693 | 1.007300 | 84808.600000 | 84808.600000 | True | baseline | 1 |
| kodim04/444/p2/d1 | 1.088699 | 1.008686 | 1.034683 | 1.023413 | 1.019658 | 204299.000000 | 204284.000000 | True | baseline | 1 |
| kodim04/444/p2/d4 | 1.096680 | 1.001283 | 1.015019 | 1.008576 | 1.012174 | 136496.600000 | 136496.600000 | True | baseline | 1 |
| kodim04/420/p0/d1 | 1.122046 | 1.003053 | 1.012178 | 1.010676 | 1.012066 | 129156.900000 | 129156.900000 | True | baseline | 1 |
| kodim04/420/p0/d4 | 1.108611 | 1.006289 | 1.013496 | 1.006399 | 1.014139 | 77700.300000 | 77700.300000 | True | baseline | 1 |
| kodim04/420/p2/d1 | 1.077464 | 1.008418 | 1.021533 | 1.008747 | 1.019759 | 180472.600000 | 180472.600000 | True | baseline | 1 |
| kodim04/420/p2/d4 | 1.090814 | 1.000990 | 1.015304 | 1.008036 | 1.014532 | 125576.600000 | 125578.000000 | True | baseline | 1 |
| kodim22/444/p0/d1 | 1.125720 | 1.018563 | 1.044695 | 1.042512 | 1.024512 | 161236.366667 | 161236.366667 | True | baseline | 1 |
| kodim22/444/p0/d4 | 1.138675 | 1.000734 | 1.008704 | 1.005995 | 1.006063 | 91476.300000 | 91476.300000 | True | baseline | 1 |
| kodim22/444/p2/d1 | 1.079539 | 1.007476 | 1.093480 | 1.029468 | 1.016878 | 209773.166667 | 209778.300000 | True | baseline | 1 |
| kodim22/444/p2/d4 | 1.101979 | 1.009892 | 1.010629 | 1.009033 | 1.017723 | 136832.600000 | 136832.600000 | True | baseline | 1 |
| kodim22/420/p0/d1 | 1.112344 | 1.013817 | 1.067802 | 1.052736 | 1.032303 | 144834.066667 | 144818.300000 | True | baseline | 1 |
| kodim22/420/p0/d4 | 1.123085 | 1.001379 | 1.062253 | 1.010595 | 1.005122 | 83096.000000 | 83096.000000 | True | baseline | 1 |
| kodim22/420/p2/d1 | 1.072251 | 1.001551 | 1.031663 | 1.017657 | 1.011474 | 195499.100000 | 195486.400000 | True | baseline | 1 |
| kodim22/420/p2/d4 | 1.097882 | 1.001055 | 1.210167 | 1.016002 | 1.010187 | 130560.600000 | 130560.600000 | False | inconclusive | 1 |
| low-contrast/444/p0/d1 | 1.086757 | 1.001144 | 1.043457 | 1.020746 | 1.026491 | 61508.300000 | 61508.300000 | True | baseline | 1 |
| low-contrast/444/p0/d4 | 1.083766 | 1.020777 | 1.051052 | 1.012421 | 1.040802 | 51488.000000 | 51488.000000 | True | baseline | 1 |
| low-contrast/444/p2/d1 | 1.086298 | 1.000779 | 1.020388 | 1.018399 | 1.021561 | 130872.600000 | 130872.600000 | True | baseline | 1 |
| low-contrast/444/p2/d4 | 1.085102 | 1.007603 | 1.033681 | 1.018895 | 1.024699 | 104208.600000 | 104208.600000 | True | baseline | 1 |
| low-contrast/420/p0/d1 | 1.059347 | 1.011458 | 1.044236 | 1.032633 | 1.030984 | 58204.300000 | 58204.300000 | True | baseline | 1 |
| low-contrast/420/p0/d4 | 1.055603 | 1.003518 | 1.069597 | 1.015673 | 1.023195 | 50420.300000 | 50420.300000 | True | baseline | 1 |
| low-contrast/420/p2/d1 | 1.081143 | 1.006121 | 1.027302 | 1.015135 | 1.017134 | 111784.000000 | 111784.000000 | True | baseline | 1 |
| low-contrast/420/p2/d4 | 1.049073 | 1.020927 | 1.312306 | 1.295903 | 1.082065 | 92728.600000 | 92728.600000 | False | inconclusive | 1 |
| low-noise/444/p0/d1 | 1.156499 | 1.014028 | 1.032502 | 1.024162 | 1.033298 | 64244.300000 | 64244.300000 | True | baseline | 1 |
| low-noise/444/p0/d4 | 1.106677 | 1.013748 | 1.046481 | 1.029142 | 1.023581 | 49596.300000 | 49596.300000 | True | baseline | 1 |
| low-noise/444/p2/d1 | 1.098170 | 1.003923 | 1.019865 | 1.013043 | 1.014146 | 111024.600000 | 111024.600000 | True | baseline | 1 |
| low-noise/444/p2/d4 | 1.083272 | 1.003303 | 1.028556 | 1.008316 | 1.018380 | 79196.300000 | 79196.300000 | True | baseline | 1 |
| low-noise/420/p0/d1 | 1.079772 | 1.014365 | 1.055500 | 1.020790 | 1.052892 | 53764.300000 | 53764.300000 | True | baseline | 1 |
| low-noise/420/p0/d4 | 1.063497 | 1.021531 | 1.032672 | 1.032862 | 1.029321 | 43148.300000 | 43148.300000 | True | baseline | 1 |
| low-noise/420/p2/d1 | 1.064506 | 1.020612 | 1.028348 | 1.026918 | 1.030270 | 101544.600000 | 101544.600000 | True | baseline | 1 |
| low-noise/420/p2/d4 | 1.063903 | 1.007703 | 1.012006 | 1.010733 | 1.012175 | 79848.000000 | 79848.000000 | True | baseline | 1 |
| full-noise/444/p0/d1 | 1.091784 | 1.007971 | 1.142284 | 1.022778 | 1.008675 | 110064.000000 | 110064.000000 | False | inconclusive | 1 |
| full-noise/444/p0/d4 | 1.101253 | 1.008283 | 1.085457 | 1.021011 | 1.021812 | 77946.000000 | 77944.600000 | True | baseline | 1 |
| full-noise/444/p2/d1 | 1.030039 | 1.000230 | 1.007288 | 1.006131 | 1.007436 | 191940.900000 | 191940.900000 | True | baseline | 1 |
| full-noise/444/p2/d4 | 1.046726 | 1.001617 | 1.018168 | 1.011414 | 1.011433 | 130320.600000 | 130320.600000 | True | baseline | 1 |
| full-noise/420/p0/d1 | 1.047448 | 1.014727 | 1.025207 | 1.023521 | 1.007195 | 81492.300000 | 81492.300000 | True | baseline | 1 |
| full-noise/420/p0/d4 | 1.073314 | 1.006948 | 1.038865 | 1.032358 | 1.024799 | 61452.300000 | 61456.600000 | True | baseline | 1 |
| full-noise/420/p2/d1 | 1.015818 | 1.001276 | 1.013369 | 1.011248 | 1.013114 | 135788.300000 | 135788.300000 | True | baseline | 1 |
| full-noise/420/p2/d4 | 1.041739 | 1.008911 | 1.019621 | 1.010453 | 1.023297 | 109328.000000 | 109332.300000 | True | baseline | 1 |
| tiny-odd-edges/444/p0/d1 | 0.995434 | 1.016732 | 1.089241 | 1.079532 | 1.070727 | 36762.580000 | 36762.580000 | True | inconclusive | 1 |
| tiny-odd-edges/444/p0/d4 | 1.004101 | 1.023978 | 1.067937 | 1.073572 | 1.082490 | 36465.720000 | 36465.720000 | True | inconclusive | 1 |
| tiny-odd-edges/444/p2/d1 | 1.001799 | 1.028332 | 1.076522 | 1.050987 | 1.065255 | 64412.300000 | 64412.300000 | True | inconclusive | 1 |
| tiny-odd-edges/444/p2/d4 | 1.000874 | 1.011767 | 1.092574 | 1.066897 | 1.059357 | 64083.440000 | 64082.580000 | True | inconclusive | 1 |
| tiny-odd-edges/420/p0/d1 | 1.013687 | 1.012879 | 1.094993 | 1.076371 | 1.093410 | 36569.720000 | 36569.720000 | True | inconclusive | 1 |
| tiny-odd-edges/420/p0/d4 | 1.006997 | 1.015201 | 1.115566 | 1.105556 | 1.108346 | 36394.580000 | 36394.580000 | False | inconclusive | 1 |
| tiny-odd-edges/420/p2/d1 | 1.000846 | 1.020509 | 1.076562 | 1.065831 | 1.062212 | 65499.440000 | 65499.440000 | True | inconclusive | 1 |
| tiny-odd-edges/420/p2/d4 | 1.004353 | 1.032055 | 1.081022 | 1.061549 | 1.073111 | 65268.300000 | 65268.300000 | True | inconclusive | 1 |

Invalid evidence:

- kodim01/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim01/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim02/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim02/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim02/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim02/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim22/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- low-contrast/420/p2/d4: baseline: p90/p10 drift exceeds20%, including minority timing states.
- low-contrast/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- low-contrast/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- full-noise/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4: current: repeated legs within a quartet differ by over10%.
