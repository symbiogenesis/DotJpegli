# Interleaved revision comparison

Persistent baseline and current workers run serially as balanced ABBA/BAAB quartets with identical fixed iterations, warmups, runtime, CPU affinity, encoding policy, inputs and JPEG hashes. Startup and IPC are outside encode timing. Ratios below1 favor current. Allocations are mean bytes per encode; retained rental capacity and peak working memory are separate measurements.

MAD, repeated-leg drift, p90/p10 minority-state drift and order bias guard the timing evidence. Invalid rows retain raw measurements and cannot support a speed claim. Valid rows remain inconclusive unless every quartet differs by more than 0.5% in the same direction. This comparison has no native parity verdict.

| Case | Current/baseline | Order bias | Maximum repeat drift | Baseline p90/p10 | Current p90/p10 | Baseline B/encode | Current B/encode | Valid | Preference | Selected attempt |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- | ---: |
| kodim01/444/p0/d1 | 1.003652 | 1.004042 | 1.143891 | 1.088305 | 1.016029 | 195556.200000 | 195554.100000 | False | inconclusive | 1 |
| kodim01/444/p0/d4 | 1.002774 | 1.009732 | 1.036742 | 1.034241 | 1.028332 | 109977.400000 | 109977.400000 | True | inconclusive | 1 |
| kodim01/444/p2/d1 | 1.005293 | 1.003242 | 1.031589 | 1.023026 | 1.025596 | 254923.700000 | 254923.700000 | True | inconclusive | 1 |
| kodim01/444/p2/d4 | 1.006294 | 1.002494 | 1.008577 | 1.005904 | 1.008830 | 165892.900000 | 165892.900000 | True | inconclusive | 1 |
| kodim01/420/p0/d1 | 1.005040 | 1.005151 | 1.034092 | 1.020465 | 1.019935 | 177901.566667 | 177907.633333 | True | inconclusive | 1 |
| kodim01/420/p0/d4 | 1.007445 | 1.007427 | 1.042740 | 1.007540 | 1.014480 | 98679.833333 | 98632.600000 | True | inconclusive | 1 |
| kodim01/420/p2/d1 | 0.994339 | 1.012232 | 1.030230 | 1.026771 | 1.002542 | 228100.200000 | 228049.533333 | True | inconclusive | 1 |
| kodim01/420/p2/d4 | 1.000130 | 1.003045 | 1.009397 | 1.007290 | 1.007073 | 151020.900000 | 151020.900000 | True | inconclusive | 1 |
| kodim02/444/p0/d1 | 1.002222 | 1.008353 | 1.045074 | 1.019364 | 1.020376 | 143110.366667 | 143137.333333 | True | inconclusive | 1 |
| kodim02/444/p0/d4 | 1.005748 | 1.002082 | 1.017824 | 1.009587 | 1.005043 | 80920.600000 | 80920.600000 | True | inconclusive | 1 |
| kodim02/444/p2/d1 | 1.007535 | 1.007441 | 1.215700 | 1.009718 | 1.013216 | 222504.600000 | 222523.833333 | False | inconclusive | 1 |
| kodim02/444/p2/d4 | 1.009020 | 1.007142 | 1.117073 | 1.004609 | 1.011457 | 138992.600000 | 138992.600000 | False | inconclusive | 1 |
| kodim02/420/p0/d1 | 1.009316 | 1.000012 | 1.022909 | 1.007104 | 1.024007 | 122232.600000 | 122244.233333 | True | inconclusive | 1 |
| kodim02/420/p0/d4 | 1.010434 | 1.003696 | 1.042596 | 1.005263 | 1.008451 | 68356.300000 | 68356.300000 | True | baseline | 1 |
| kodim02/420/p2/d1 | 1.004204 | 1.001000 | 1.012364 | 1.005182 | 1.006664 | 185716.900000 | 185716.900000 | True | inconclusive | 1 |
| kodim02/420/p2/d4 | 1.000682 | 1.000834 | 1.010035 | 1.009604 | 1.007237 | 116536.600000 | 116536.600000 | True | inconclusive | 1 |
| kodim04/444/p0/d1 | 1.011867 | 1.015231 | 1.048500 | 1.019488 | 1.031191 | 143746.066667 | 143790.966667 | True | inconclusive | 1 |
| kodim04/444/p0/d4 | 1.008200 | 1.003148 | 1.027314 | 1.006412 | 1.006589 | 84808.600000 | 84808.600000 | True | baseline | 1 |
| kodim04/444/p2/d1 | 1.008195 | 1.014557 | 1.024302 | 1.022255 | 1.030073 | 204323.166667 | 204299.000000 | True | inconclusive | 1 |
| kodim04/444/p2/d4 | 1.013073 | 1.000682 | 1.012968 | 1.008847 | 1.014639 | 136496.600000 | 136496.600000 | True | baseline | 1 |
| kodim04/420/p0/d1 | 1.004783 | 1.001090 | 1.232378 | 1.010482 | 1.013576 | 129156.900000 | 129156.900000 | False | inconclusive | 1 |
| kodim04/420/p0/d4 | 1.006922 | 1.002274 | 1.008996 | 1.006845 | 1.005918 | 77700.300000 | 77700.300000 | True | inconclusive | 1 |
| kodim04/420/p2/d1 | 1.003886 | 1.002009 | 1.027129 | 1.009157 | 1.019172 | 180472.600000 | 180472.600000 | True | inconclusive | 1 |
| kodim04/420/p2/d4 | 0.995860 | 1.005917 | 1.082313 | 1.081715 | 1.008772 | 125578.000000 | 125576.600000 | True | inconclusive | 1 |
| kodim22/444/p0/d1 | 1.007825 | 1.015629 | 1.040634 | 1.025040 | 1.030449 | 161236.366667 | 161236.733333 | True | inconclusive | 1 |
| kodim22/444/p0/d4 | 1.009926 | 1.002172 | 1.007548 | 1.003348 | 1.005901 | 91476.300000 | 91476.300000 | True | baseline | 1 |
| kodim22/444/p2/d1 | 1.007876 | 1.018483 | 1.017890 | 1.014168 | 1.021541 | 209802.466667 | 209790.033333 | True | inconclusive | 1 |
| kodim22/444/p2/d4 | 1.009627 | 1.004722 | 1.010415 | 1.007005 | 1.012218 | 136832.600000 | 136832.600000 | True | baseline | 1 |
| kodim22/420/p0/d1 | 1.008233 | 1.013388 | 1.051356 | 1.018200 | 1.040216 | 144818.300000 | 144834.166667 | True | inconclusive | 1 |
| kodim22/420/p0/d4 | 1.007426 | 1.005943 | 1.012624 | 1.006208 | 1.006002 | 83096.000000 | 83096.000000 | True | baseline | 1 |
| kodim22/420/p2/d1 | 0.998340 | 1.008904 | 1.069727 | 1.016704 | 1.039249 | 195470.300000 | 195499.100000 | True | inconclusive | 1 |
| kodim22/420/p2/d4 | 1.001325 | 1.001133 | 1.008505 | 1.005109 | 1.008074 | 130560.600000 | 130560.600000 | True | inconclusive | 1 |
| low-contrast/444/p0/d1 | 0.999364 | 1.004164 | 1.050594 | 1.047382 | 1.033199 | 61508.300000 | 61508.300000 | True | inconclusive | 1 |
| low-contrast/444/p0/d4 | 0.998632 | 1.009864 | 1.051879 | 1.022733 | 1.026285 | 51488.000000 | 51488.000000 | True | inconclusive | 1 |
| low-contrast/444/p2/d1 | 0.997703 | 1.001802 | 1.024499 | 1.011600 | 1.022241 | 130872.600000 | 130872.600000 | True | inconclusive | 1 |
| low-contrast/444/p2/d4 | 1.004452 | 1.006372 | 1.025955 | 1.013549 | 1.021676 | 104208.600000 | 104208.600000 | True | inconclusive | 1 |
| low-contrast/420/p0/d1 | 1.006522 | 1.007470 | 1.031639 | 1.018692 | 1.025092 | 58204.300000 | 58204.300000 | True | inconclusive | 1 |
| low-contrast/420/p0/d4 | 1.006087 | 1.011096 | 1.039296 | 1.011394 | 1.018789 | 50420.300000 | 50420.300000 | True | inconclusive | 1 |
| low-contrast/420/p2/d1 | 0.991668 | 1.011871 | 1.038118 | 1.012367 | 1.014515 | 111784.000000 | 111784.000000 | True | inconclusive | 1 |
| low-contrast/420/p2/d4 | 0.997276 | 1.005599 | 1.062840 | 1.033432 | 1.021972 | 92728.600000 | 92728.600000 | True | inconclusive | 1 |
| low-noise/444/p0/d1 | 1.005287 | 1.007039 | 1.031709 | 1.021717 | 1.015642 | 64244.300000 | 64244.300000 | True | inconclusive | 1 |
| low-noise/444/p0/d4 | 1.001433 | 1.006988 | 1.028743 | 1.009963 | 1.004941 | 49596.300000 | 49596.300000 | True | inconclusive | 1 |
| low-noise/444/p2/d1 | 1.002241 | 1.006541 | 1.026305 | 1.018795 | 1.017682 | 111024.600000 | 111024.600000 | True | inconclusive | 1 |
| low-noise/444/p2/d4 | 1.005107 | 1.005913 | 1.015777 | 1.013869 | 1.013039 | 79196.300000 | 79196.300000 | True | inconclusive | 1 |
| low-noise/420/p0/d1 | 1.004993 | 1.012721 | 1.041892 | 1.012567 | 1.020881 | 53764.300000 | 53764.300000 | True | inconclusive | 1 |
| low-noise/420/p0/d4 | 1.008054 | 1.009458 | 1.030035 | 1.025042 | 1.022017 | 43148.300000 | 43148.300000 | True | inconclusive | 1 |
| low-noise/420/p2/d1 | 0.997266 | 1.002394 | 1.025657 | 1.021278 | 1.018248 | 101544.600000 | 101544.600000 | True | inconclusive | 1 |
| low-noise/420/p2/d4 | 1.001050 | 1.009318 | 1.028456 | 1.012357 | 1.011182 | 79848.000000 | 79848.000000 | True | inconclusive | 1 |
| full-noise/444/p0/d1 | 1.000242 | 1.000360 | 1.014346 | 1.006216 | 1.007598 | 110064.000000 | 110064.000000 | True | inconclusive | 1 |
| full-noise/444/p0/d4 | 1.000147 | 1.001573 | 1.029499 | 1.018800 | 1.026287 | 77944.600000 | 77944.600000 | True | inconclusive | 1 |
| full-noise/444/p2/d1 | 0.996637 | 1.000494 | 1.032778 | 1.014030 | 1.011188 | 191940.900000 | 191942.300000 | True | inconclusive | 1 |
| full-noise/444/p2/d4 | 1.001036 | 1.004105 | 1.012508 | 1.012128 | 1.010114 | 130320.600000 | 130320.600000 | True | inconclusive | 1 |
| full-noise/420/p0/d1 | 1.005946 | 1.006434 | 1.015496 | 1.011475 | 1.005764 | 81492.300000 | 81488.000000 | True | inconclusive | 1 |
| full-noise/420/p0/d4 | 1.005167 | 1.019586 | 1.044952 | 1.028630 | 1.025679 | 61452.300000 | 61452.300000 | True | inconclusive | 1 |
| full-noise/420/p2/d1 | 1.000524 | 1.004531 | 1.139176 | 1.009852 | 1.016536 | 135788.300000 | 135788.300000 | False | inconclusive | 1 |
| full-noise/420/p2/d4 | 0.996008 | 1.008920 | 1.018430 | 1.019221 | 1.012305 | 109328.000000 | 109332.300000 | True | inconclusive | 1 |
| tiny-odd-edges/444/p0/d1 | 1.004205 | 1.024776 | 1.107697 | 1.099375 | 1.082561 | 36762.580000 | 36761.720000 | False | inconclusive | 1 |
| tiny-odd-edges/444/p0/d4 | 0.997946 | 1.008264 | 1.087897 | 1.089596 | 1.071458 | 36465.720000 | 36465.720000 | True | inconclusive | 1 |
| tiny-odd-edges/444/p2/d1 | 1.000936 | 1.011340 | 1.070484 | 1.050614 | 1.046812 | 64412.300000 | 64412.300000 | True | inconclusive | 1 |
| tiny-odd-edges/444/p2/d4 | 1.001605 | 1.009286 | 1.056950 | 1.050885 | 1.051534 | 64083.440000 | 64082.580000 | True | inconclusive | 1 |
| tiny-odd-edges/420/p0/d1 | 0.991425 | 1.001797 | 1.099954 | 1.102238 | 1.070446 | 36569.720000 | 36569.720000 | True | inconclusive | 1 |
| tiny-odd-edges/420/p0/d4 | 0.999366 | 1.008167 | 1.132387 | 1.108559 | 1.112017 | 36394.580000 | 36394.580000 | False | inconclusive | 1 |
| tiny-odd-edges/420/p2/d1 | 0.993553 | 1.017605 | 1.062965 | 1.048984 | 1.061596 | 65499.440000 | 65500.300000 | True | inconclusive | 1 |
| tiny-odd-edges/420/p2/d4 | 0.995021 | 1.003686 | 1.085014 | 1.087953 | 1.064407 | 65268.300000 | 65268.300000 | True | inconclusive | 1 |

Invalid evidence:

- kodim01/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim02/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim02/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim04/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- full-noise/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4: current: repeated legs within a quartet differ by over10%.
