# Interleaved revision comparison

Persistent baseline and current workers run serially as balanced ABBA/BAAB quartets with identical fixed iterations, warmups, runtime, CPU affinity, encoding policy, inputs and JPEG hashes. Startup and IPC are outside encode timing. Ratios below1 favor current. Allocations are mean bytes per encode; retained rental capacity and peak working memory are separate measurements.

MAD, repeated-leg drift, p90/p10 minority-state drift and order bias guard the timing evidence. Invalid rows retain raw measurements and cannot support a speed claim. Valid rows remain inconclusive unless every quartet differs by more than 0.5% in the same direction. This comparison has no native parity verdict.

| Case | Current/baseline | Order bias | Maximum repeat drift | Baseline p90/p10 | Current p90/p10 | Baseline B/encode | Current B/encode | Valid | Preference | Selected attempt |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- | ---: |
| kodim01/444/p0/d1 | 1.014728 | 1.007444 | 1.057611 | 1.010122 | 1.082585 | 195599.566667 | 195599.566667 | True | inconclusive | 1 |
| kodim01/444/p0/d4 | 1.014618 | 1.009461 | 1.026558 | 1.023277 | 1.022680 | 110052.200000 | 110052.200000 | True | baseline | 1 |
| kodim01/444/p2/d1 | 1.006679 | 1.000408 | 1.012167 | 1.010993 | 1.014428 | 254922.200000 | 254940.000000 | True | inconclusive | 1 |
| kodim01/444/p2/d4 | 1.008925 | 1.002119 | 1.023108 | 1.015325 | 1.011154 | 165892.900000 | 165892.900000 | True | inconclusive | 1 |
| kodim01/420/p0/d1 | 1.006546 | 1.016016 | 1.021472 | 1.024786 | 1.026240 | 177975.133333 | 177979.433333 | True | inconclusive | 1 |
| kodim01/420/p0/d4 | 1.005093 | 1.002851 | 1.007413 | 1.004740 | 1.007136 | 98704.600000 | 98704.600000 | True | inconclusive | 1 |
| kodim01/420/p2/d1 | 1.000923 | 1.001109 | 1.016965 | 1.012788 | 1.010612 | 228064.733333 | 228067.100000 | True | inconclusive | 1 |
| kodim01/420/p2/d4 | 1.000542 | 1.002056 | 1.185863 | 1.017916 | 1.002117 | 151016.600000 | 151020.900000 | False | inconclusive | 1 |
| kodim02/444/p0/d1 | 1.019991 | 1.000790 | 1.019187 | 1.012291 | 1.013998 | 143167.566667 | 143167.566667 | True | baseline | 1 |
| kodim02/444/p0/d4 | 1.017287 | 1.002042 | 1.014886 | 1.005400 | 1.004624 | 80992.600000 | 80992.600000 | True | baseline | 1 |
| kodim02/444/p2/d1 | 1.010153 | 1.005325 | 1.120275 | 1.008114 | 1.018984 | 222518.000000 | 222522.300000 | False | inconclusive | 1 |
| kodim02/444/p2/d4 | 1.006673 | 1.004256 | 1.016910 | 1.011857 | 1.003677 | 138992.600000 | 138992.600000 | True | inconclusive | 1 |
| kodim02/420/p0/d1 | 1.005734 | 1.006282 | 1.007494 | 1.004572 | 1.009017 | 122304.600000 | 122304.600000 | True | inconclusive | 1 |
| kodim02/420/p0/d4 | 1.007120 | 1.003755 | 1.030821 | 1.005334 | 1.010574 | 68428.300000 | 68428.300000 | True | inconclusive | 1 |
| kodim02/420/p2/d1 | 1.002455 | 1.000427 | 1.010247 | 1.002218 | 1.006777 | 185716.900000 | 185716.900000 | True | inconclusive | 1 |
| kodim02/420/p2/d4 | 1.003806 | 1.003027 | 1.022537 | 1.008828 | 1.009237 | 116536.600000 | 116536.600000 | True | inconclusive | 1 |
| kodim04/444/p0/d1 | 1.014796 | 1.009811 | 1.027932 | 1.017688 | 1.022429 | 143826.833333 | 143831.133333 | True | baseline | 1 |
| kodim04/444/p0/d4 | 1.019522 | 1.001580 | 1.008900 | 1.005407 | 1.007774 | 84880.600000 | 84880.600000 | True | baseline | 1 |
| kodim04/444/p2/d1 | 1.008318 | 1.002688 | 1.015414 | 1.009553 | 1.016972 | 204271.100000 | 204240.000000 | True | inconclusive | 1 |
| kodim04/444/p2/d4 | 1.008025 | 1.001848 | 1.007681 | 1.010247 | 1.003785 | 136496.600000 | 136496.600000 | True | inconclusive | 1 |
| kodim04/420/p0/d1 | 1.008562 | 1.002931 | 1.020999 | 1.005564 | 1.009330 | 129228.900000 | 129228.900000 | True | inconclusive | 1 |
| kodim04/420/p0/d4 | 1.007404 | 1.001377 | 1.009732 | 1.003081 | 1.004053 | 77772.300000 | 77772.300000 | True | baseline | 1 |
| kodim04/420/p2/d1 | 1.004147 | 1.001768 | 1.024702 | 1.024389 | 1.020797 | 180472.600000 | 180476.900000 | True | inconclusive | 1 |
| kodim04/420/p2/d4 | 1.006637 | 1.000221 | 1.006344 | 1.004854 | 1.006474 | 125580.900000 | 125580.900000 | True | inconclusive | 1 |
| kodim22/444/p0/d1 | 1.014858 | 1.007677 | 1.019208 | 1.017730 | 1.018664 | 161283.433333 | 161283.700000 | True | baseline | 1 |
| kodim22/444/p0/d4 | 1.018358 | 1.001105 | 1.009058 | 1.006005 | 1.007522 | 91548.300000 | 91548.300000 | True | baseline | 1 |
| kodim22/444/p2/d1 | 1.006677 | 1.015349 | 1.073837 | 1.020198 | 1.012081 | 209805.366667 | 209796.100000 | True | inconclusive | 1 |
| kodim22/444/p2/d4 | 1.010079 | 1.003951 | 1.014899 | 1.004839 | 1.012216 | 136832.600000 | 136875.833333 | True | baseline | 1 |
| kodim22/420/p0/d1 | 1.003641 | 1.002552 | 1.025597 | 1.006591 | 1.012107 | 144871.566667 | 144847.900000 | True | inconclusive | 1 |
| kodim22/420/p0/d4 | 1.010307 | 1.002220 | 1.034242 | 1.010473 | 1.004944 | 83176.600000 | 83176.600000 | True | baseline | 1 |
| kodim22/420/p2/d1 | 1.002421 | 1.000958 | 1.013748 | 1.005812 | 1.013295 | 195440.600000 | 195472.600000 | True | inconclusive | 1 |
| kodim22/420/p2/d4 | 1.003421 | 1.000013 | 1.013515 | 1.006157 | 1.003150 | 130560.600000 | 130560.600000 | True | inconclusive | 1 |
| low-contrast/444/p0/d1 | 1.016681 | 1.000063 | 1.017578 | 1.011612 | 1.010826 | 61580.300000 | 61576.000000 | True | baseline | 1 |
| low-contrast/444/p0/d4 | 1.024507 | 1.003896 | 1.024753 | 1.014946 | 1.006487 | 51564.300000 | 51564.300000 | True | baseline | 1 |
| low-contrast/444/p2/d1 | 1.005291 | 1.002364 | 1.016218 | 1.011703 | 1.012838 | 130872.600000 | 130872.600000 | True | inconclusive | 1 |
| low-contrast/444/p2/d4 | 1.011586 | 1.002980 | 1.020689 | 1.016337 | 1.017181 | 104208.600000 | 104204.300000 | True | baseline | 1 |
| low-contrast/420/p0/d1 | 1.011204 | 1.000568 | 1.026867 | 1.007003 | 1.012775 | 58272.000000 | 58272.000000 | True | inconclusive | 1 |
| low-contrast/420/p0/d4 | 1.010090 | 1.003103 | 1.035695 | 1.016467 | 1.015968 | 50492.300000 | 50488.000000 | True | inconclusive | 1 |
| low-contrast/420/p2/d1 | 1.003034 | 1.002831 | 1.115726 | 1.015713 | 1.023179 | 111792.600000 | 111792.600000 | False | inconclusive | 1 |
| low-contrast/420/p2/d4 | 1.007468 | 1.001878 | 1.030564 | 1.025931 | 1.016279 | 92724.300000 | 92724.300000 | True | inconclusive | 1 |
| low-noise/444/p0/d1 | 1.016472 | 1.001494 | 1.026752 | 1.014632 | 1.018193 | 64316.300000 | 64316.300000 | True | baseline | 1 |
| low-noise/444/p0/d4 | 1.021159 | 1.013468 | 1.054591 | 1.040460 | 1.008423 | 49668.300000 | 49668.300000 | True | inconclusive | 1 |
| low-noise/444/p2/d1 | 1.007078 | 1.001496 | 1.022489 | 1.009822 | 1.018500 | 111024.600000 | 111024.600000 | True | inconclusive | 1 |
| low-noise/444/p2/d4 | 1.007769 | 1.003119 | 1.011696 | 1.013038 | 1.017905 | 79196.300000 | 79196.300000 | True | inconclusive | 1 |
| low-noise/420/p0/d1 | 1.013029 | 1.009938 | 1.043566 | 1.021876 | 1.011748 | 53836.300000 | 53836.300000 | True | inconclusive | 1 |
| low-noise/420/p0/d4 | 1.015353 | 1.006762 | 1.047404 | 1.036990 | 1.023029 | 43220.300000 | 43220.300000 | True | inconclusive | 1 |
| low-noise/420/p2/d1 | 1.002638 | 1.003034 | 1.023756 | 1.012450 | 1.022056 | 101544.600000 | 101544.600000 | True | inconclusive | 1 |
| low-noise/420/p2/d4 | 1.005499 | 1.006725 | 1.024386 | 1.019256 | 1.010365 | 79852.300000 | 79852.300000 | True | inconclusive | 1 |
| full-noise/444/p0/d1 | 1.011041 | 1.003248 | 1.017222 | 1.011490 | 1.013045 | 110144.600000 | 110144.600000 | True | baseline | 1 |
| full-noise/444/p0/d4 | 1.014920 | 1.002850 | 1.018592 | 1.014962 | 1.021918 | 78016.600000 | 78016.600000 | True | baseline | 1 |
| full-noise/444/p2/d1 | 1.001322 | 1.000855 | 1.064495 | 1.007965 | 1.011875 | 191936.600000 | 191936.600000 | True | inconclusive | 1 |
| full-noise/444/p2/d4 | 1.004027 | 1.000731 | 1.014732 | 1.010228 | 1.015089 | 130324.900000 | 130324.900000 | True | inconclusive | 1 |
| full-noise/420/p0/d1 | 1.009254 | 1.000881 | 1.023339 | 1.021565 | 1.018875 | 81564.300000 | 81564.300000 | True | inconclusive | 1 |
| full-noise/420/p0/d4 | 1.006456 | 1.008515 | 1.025723 | 1.009831 | 1.011716 | 61524.300000 | 61524.300000 | True | inconclusive | 1 |
| full-noise/420/p2/d1 | 0.999158 | 1.002574 | 1.010959 | 1.016626 | 1.012446 | 135796.900000 | 135796.900000 | True | inconclusive | 1 |
| full-noise/420/p2/d4 | 1.001934 | 1.000688 | 1.027706 | 1.014587 | 1.015412 | 109336.600000 | 109336.600000 | True | inconclusive | 1 |
| tiny-odd-edges/444/p0/d1 | 0.990801 | 1.005470 | 1.090234 | 1.068702 | 1.070193 | 36833.720000 | 36833.720000 | True | inconclusive | 1 |
| tiny-odd-edges/444/p0/d4 | 0.997581 | 1.025671 | 1.084183 | 1.074641 | 1.073937 | 36538.580000 | 36538.580000 | True | inconclusive | 1 |
| tiny-odd-edges/444/p2/d1 | 1.001476 | 1.019878 | 1.055323 | 1.041079 | 1.040612 | 64411.440000 | 64411.440000 | True | inconclusive | 1 |
| tiny-odd-edges/444/p2/d4 | 0.990531 | 1.002848 | 1.102694 | 1.062906 | 1.052378 | 64084.300000 | 64084.300000 | False | inconclusive | 1 |
| tiny-odd-edges/420/p0/d1 | 0.998247 | 1.007935 | 1.091847 | 1.081247 | 1.088970 | 36642.580000 | 36642.580000 | True | inconclusive | 1 |
| tiny-odd-edges/420/p0/d4 | 1.002305 | 1.022015 | 1.093869 | 1.059113 | 1.083388 | 36465.720000 | 36465.720000 | True | inconclusive | 1 |
| tiny-odd-edges/420/p2/d1 | 1.000163 | 1.010931 | 1.064177 | 1.057884 | 1.053878 | 65500.300000 | 65500.300000 | True | inconclusive | 1 |
| tiny-odd-edges/420/p2/d4 | 0.992806 | 1.024447 | 1.062426 | 1.053117 | 1.058211 | 65267.440000 | 65267.440000 | True | inconclusive | 1 |

Invalid evidence:

- kodim01/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim02/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- low-contrast/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
