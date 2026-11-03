# Interleaved revision comparison

Persistent baseline and current workers run serially as balanced ABBA/BAAB quartets with identical fixed iterations, warmups, runtime, CPU affinity, encoding policy, inputs and JPEG hashes. Startup and IPC are outside encode timing. Ratios below1 favor current. Allocations are mean bytes per encode; retained rental capacity and peak working memory are separate measurements.

MAD, repeated-leg drift, p90/p10 minority-state drift and order bias guard the timing evidence. Invalid rows retain raw measurements and cannot support a speed claim. Valid rows remain inconclusive unless every quartet differs by more than 0.5% in the same direction. This comparison has no native parity verdict.

| Case | Current/baseline | Order bias | Maximum repeat drift | Baseline p90/p10 | Current p90/p10 | Baseline B/encode | Current B/encode | Valid | Preference | Selected attempt |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- | ---: |
| kodim01/444/p0/d1 | 1.052164 | 1.000039 | 1.082201 | 1.014502 | 1.012512 | 195614.300000 | 195623.133333 | True | inconclusive | 1 |
| kodim01/444/p0/d4 | 1.058449 | 1.004282 | 1.450238 | 1.012405 | 1.179356 | 110024.000000 | 110024.000000 | False | inconclusive | 1 |
| kodim01/444/p2/d1 | 1.023936 | 1.005964 | 1.025346 | 1.010243 | 1.015487 | 254909.400000 | 254932.200000 | True | baseline | 1 |
| kodim01/444/p2/d4 | 1.039334 | 1.003747 | 1.026504 | 1.007123 | 1.021771 | 165894.100000 | 165894.100000 | True | baseline | 1 |
| kodim01/420/p0/d1 | 1.045643 | 1.002781 | 1.125747 | 1.026146 | 1.015878 | 178003.766667 | 177989.266667 | False | inconclusive | 1 |
| kodim01/420/p0/d4 | 1.044167 | 1.000977 | 1.019113 | 1.028505 | 1.017306 | 98715.266667 | 98700.300000 | True | baseline | 1 |
| kodim01/420/p2/d1 | 1.006458 | 1.007862 | 1.168826 | 1.035456 | 1.031191 | 228053.866667 | 228059.933333 | False | inconclusive | 1 |
| kodim01/420/p2/d4 | 1.033978 | 1.002549 | 1.014449 | 1.016363 | 1.015919 | 151012.300000 | 151012.300000 | True | baseline | 1 |
| kodim02/444/p0/d1 | 1.059851 | 1.002038 | 1.116959 | 1.167065 | 1.160171 | 143191.233333 | 143144.000000 | False | inconclusive | 1 |
| kodim02/444/p0/d4 | 1.038106 | 1.002887 | 1.011383 | 1.008252 | 1.009786 | 80984.000000 | 80984.000000 | True | baseline | 1 |
| kodim02/444/p2/d1 | 1.039259 | 1.000651 | 1.038638 | 1.009582 | 1.007422 | 222513.700000 | 222513.700000 | True | baseline | 1 |
| kodim02/444/p2/d4 | 1.026522 | 1.000104 | 1.015960 | 1.010105 | 1.007284 | 138988.300000 | 138988.300000 | True | baseline | 1 |
| kodim02/420/p0/d1 | 1.078758 | 1.012495 | 1.019200 | 1.012074 | 1.019669 | 122296.000000 | 122296.000000 | True | baseline | 1 |
| kodim02/420/p0/d4 | 1.048546 | 1.019058 | 1.016210 | 1.018681 | 1.029455 | 68428.300000 | 68471.900000 | True | baseline | 1 |
| kodim02/420/p2/d1 | 1.017187 | 1.000663 | 1.013380 | 1.009295 | 1.013092 | 185704.000000 | 185704.000000 | True | baseline | 1 |
| kodim02/420/p2/d4 | 1.025913 | 1.003482 | 1.013950 | 1.014185 | 1.009146 | 116532.300000 | 116532.300000 | True | baseline | 1 |
| kodim04/444/p0/d1 | 1.064941 | 1.008950 | 1.071833 | 1.014348 | 1.019904 | 143807.566667 | 143854.166667 | True | baseline | 1 |
| kodim04/444/p0/d4 | 1.043792 | 1.005649 | 1.008208 | 1.008979 | 1.006263 | 84872.000000 | 84872.000000 | True | baseline | 1 |
| kodim04/444/p2/d1 | 1.039384 | 1.000248 | 1.037512 | 1.009614 | 1.015043 | 204240.000000 | 204275.400000 | True | baseline | 1 |
| kodim04/444/p2/d4 | 1.029629 | 1.000481 | 1.017061 | 1.014035 | 1.012252 | 136488.000000 | 136488.000000 | True | baseline | 1 |
| kodim04/420/p0/d1 | 1.064202 | 1.003919 | 1.159983 | 1.158944 | 1.181010 | 129220.300000 | 129220.300000 | False | inconclusive | 1 |
| kodim04/420/p0/d4 | 1.033634 | 1.000063 | 1.145352 | 1.154980 | 1.153117 | 77768.000000 | 77768.000000 | False | inconclusive | 1 |
| kodim04/420/p2/d1 | 1.003803 | 1.005182 | 1.148066 | 1.006269 | 1.015949 | 180464.000000 | 180464.000000 | False | inconclusive | 1 |
| kodim04/420/p2/d4 | 1.025956 | 1.005931 | 1.275833 | 1.036610 | 1.010510 | 125572.300000 | 125572.300000 | False | inconclusive | 1 |
| kodim22/444/p0/d1 | 1.062561 | 1.010247 | 1.079907 | 1.029852 | 1.031290 | 161302.700000 | 161302.700000 | True | baseline | 1 |
| kodim22/444/p0/d4 | 1.044829 | 1.002731 | 1.009891 | 1.012945 | 1.010507 | 91544.000000 | 91544.000000 | True | baseline | 1 |
| kodim22/444/p2/d1 | 1.035625 | 1.001271 | 1.027279 | 1.022993 | 1.018459 | 209787.900000 | 209791.700000 | True | baseline | 1 |
| kodim22/444/p2/d4 | 1.032161 | 1.006850 | 1.008954 | 1.007844 | 1.009032 | 136824.000000 | 136824.000000 | True | baseline | 1 |
| kodim22/420/p0/d1 | 1.070721 | 1.034402 | 1.107045 | 1.010254 | 1.038067 | 144847.800000 | 144898.500000 | False | inconclusive | 1 |
| kodim22/420/p0/d4 | 1.056304 | 1.005131 | 1.019852 | 1.007033 | 1.010751 | 83172.300000 | 83172.300000 | True | baseline | 1 |
| kodim22/420/p2/d1 | 1.009881 | 1.006172 | 1.027959 | 1.019579 | 1.015822 | 195494.800000 | 195468.866667 | True | inconclusive | 1 |
| kodim22/420/p2/d4 | 1.032020 | 1.005063 | 1.010925 | 1.003604 | 1.010165 | 130556.300000 | 130556.300000 | True | baseline | 1 |
| low-contrast/444/p0/d1 | 1.052378 | 1.011441 | 1.024204 | 1.024380 | 1.019575 | 61576.000000 | 61576.000000 | True | baseline | 1 |
| low-contrast/444/p0/d4 | 1.010780 | 1.012409 | 1.028260 | 1.016013 | 1.027656 | 51560.000000 | 51560.000000 | True | inconclusive | 1 |
| low-contrast/444/p2/d1 | 1.032704 | 1.004977 | 1.044534 | 1.048770 | 1.012622 | 130864.000000 | 130864.000000 | True | inconclusive | 1 |
| low-contrast/444/p2/d4 | 1.014182 | 1.012912 | 1.049178 | 1.009976 | 1.013890 | 104204.300000 | 104200.000000 | True | inconclusive | 1 |
| low-contrast/420/p0/d1 | 1.054762 | 1.002133 | 1.165213 | 1.019615 | 1.017577 | 58272.000000 | 58272.000000 | False | inconclusive | 1 |
| low-contrast/420/p0/d4 | 1.023621 | 1.008198 | 1.023036 | 1.020851 | 1.015818 | 50488.000000 | 50488.000000 | True | baseline | 1 |
| low-contrast/420/p2/d1 | 1.033744 | 1.003316 | 1.032468 | 1.036733 | 1.059561 | 111788.300000 | 111788.300000 | True | baseline | 1 |
| low-contrast/420/p2/d4 | 1.015791 | 1.005929 | 1.034085 | 1.023546 | 1.022817 | 92720.000000 | 92720.000000 | True | inconclusive | 1 |
| low-noise/444/p0/d1 | 1.082904 | 1.007057 | 1.022981 | 1.014843 | 1.018144 | 64312.000000 | 64312.000000 | True | baseline | 1 |
| low-noise/444/p0/d4 | 1.020404 | 1.001196 | 1.014707 | 1.014742 | 1.011856 | 49664.000000 | 49664.000000 | True | baseline | 1 |
| low-noise/444/p2/d1 | 1.061679 | 1.000059 | 1.022068 | 1.012837 | 1.011851 | 111020.300000 | 111020.300000 | True | baseline | 1 |
| low-noise/444/p2/d4 | 1.014285 | 1.004252 | 1.021702 | 1.011056 | 1.005108 | 79192.000000 | 79192.000000 | True | baseline | 1 |
| low-noise/420/p0/d1 | 1.062015 | 1.015087 | 1.024527 | 1.026376 | 1.020447 | 53832.000000 | 53832.000000 | True | baseline | 1 |
| low-noise/420/p0/d4 | 1.017636 | 1.006465 | 1.034660 | 1.025967 | 1.024057 | 43216.000000 | 43216.000000 | True | baseline | 1 |
| low-noise/420/p2/d1 | 1.045555 | 1.002311 | 1.045026 | 1.021855 | 1.032952 | 101540.300000 | 101540.300000 | True | baseline | 1 |
| low-noise/420/p2/d4 | 1.022682 | 1.002413 | 1.025401 | 1.021151 | 1.019091 | 79848.000000 | 79848.000000 | True | baseline | 1 |
| full-noise/444/p0/d1 | 1.054504 | 1.008618 | 1.015014 | 1.009874 | 1.014474 | 110136.000000 | 110136.000000 | True | baseline | 1 |
| full-noise/444/p0/d4 | 1.047184 | 1.004109 | 1.032415 | 1.019696 | 1.024139 | 78008.000000 | 78012.300000 | True | baseline | 1 |
| full-noise/444/p2/d1 | 1.012904 | 1.004141 | 1.017725 | 1.023049 | 1.012091 | 191928.000000 | 191928.000000 | True | baseline | 1 |
| full-noise/444/p2/d4 | 1.015794 | 1.012921 | 1.024955 | 1.023919 | 1.021449 | 130316.300000 | 130316.300000 | True | baseline | 1 |
| full-noise/420/p0/d1 | 1.039840 | 1.013936 | 1.029626 | 1.024283 | 1.018334 | 81560.000000 | 81560.000000 | True | baseline | 1 |
| full-noise/420/p0/d4 | 1.038023 | 1.015160 | 1.031294 | 1.016195 | 1.030957 | 61520.000000 | 61520.000000 | True | baseline | 1 |
| full-noise/420/p2/d1 | 1.007877 | 1.006804 | 1.017801 | 1.018128 | 1.018537 | 135788.300000 | 135788.300000 | True | inconclusive | 1 |
| full-noise/420/p2/d4 | 1.021669 | 1.000775 | 1.017179 | 1.012958 | 1.012901 | 109328.000000 | 109328.000000 | True | baseline | 1 |
| tiny-odd-edges/444/p0/d1 | 1.012334 | 1.018269 | 1.301174 | 1.070016 | 1.020696 | 36832.860000 | 36832.860000 | False | inconclusive | 1 |
| tiny-odd-edges/444/p0/d4 | 1.017861 | 1.006882 | 1.026065 | 1.023486 | 1.015644 | 36536.000000 | 36536.000000 | True | baseline | 1 |
| tiny-odd-edges/444/p2/d1 | 1.021805 | 1.016816 | 1.052344 | 1.022181 | 1.017462 | 64408.860000 | 64408.860000 | True | baseline | 1 |
| tiny-odd-edges/444/p2/d4 | 1.009615 | 1.016340 | 1.045051 | 1.013486 | 1.011516 | 64080.860000 | 64080.860000 | True | inconclusive | 1 |
| tiny-odd-edges/420/p0/d1 | 1.021552 | 1.008794 | 1.075950 | 1.023424 | 1.033387 | 36640.000000 | 36640.860000 | True | baseline | 1 |
| tiny-odd-edges/420/p0/d4 | 1.014362 | 1.017044 | 1.023699 | 1.022756 | 1.019636 | 36464.000000 | 36464.000000 | True | inconclusive | 1 |
| tiny-odd-edges/420/p2/d1 | 1.014596 | 1.010968 | 1.204035 | 1.041708 | 1.016979 | 65496.860000 | 65497.700000 | False | inconclusive | 1 |
| tiny-odd-edges/420/p2/d4 | 1.014331 | 1.008632 | 1.064597 | 1.020258 | 1.022472 | 65264.860000 | 65264.860000 | True | inconclusive | 1 |

Invalid evidence:

- kodim01/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim01/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim01/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim01/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim02/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim04/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim04/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim04/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim04/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim04/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim22/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- low-contrast/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p2/d1: current: repeated legs within a quartet differ by over10%.
