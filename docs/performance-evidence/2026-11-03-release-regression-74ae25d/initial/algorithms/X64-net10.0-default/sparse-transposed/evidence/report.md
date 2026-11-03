# Interleaved revision comparison

Persistent baseline and current workers run serially as balanced ABBA/BAAB quartets with identical fixed iterations, warmups, runtime, CPU affinity, encoding policy, inputs and JPEG hashes. Startup and IPC are outside encode timing. Ratios below1 favor current. Allocations are mean bytes per encode; retained rental capacity and peak working memory are separate measurements.

MAD, repeated-leg drift, p90/p10 minority-state drift and order bias guard the timing evidence. Invalid rows retain raw measurements and cannot support a speed claim. Valid rows remain inconclusive unless every quartet differs by more than 0.5% in the same direction. This comparison has no native parity verdict.

| Case | Current/baseline | Order bias | Maximum repeat drift | Baseline p90/p10 | Current p90/p10 | Baseline B/encode | Current B/encode | Valid | Preference | Selected attempt |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- | ---: |
| kodim01/444/p0/d1 | 1.006914 | 1.008342 | 1.222949 | 1.018156 | 1.081393 | 195577.933333 | 195577.566667 | False | inconclusive | 1 |
| kodim01/444/p0/d4 | 1.002807 | 1.010961 | 1.017156 | 1.017036 | 1.011463 | 109952.000000 | 109952.000000 | True | inconclusive | 1 |
| kodim01/444/p2/d1 | 1.113628 | 1.004805 | 1.045278 | 1.038095 | 1.025635 | 254949.100000 | 254949.100000 | True | baseline | 1 |
| kodim01/444/p2/d4 | 1.139708 | 1.013902 | 1.037129 | 1.017398 | 1.017029 | 165926.400000 | 165913.933333 | True | baseline | 1 |
| kodim01/420/p0/d1 | 1.033255 | 1.009209 | 1.117492 | 1.023658 | 1.025348 | 177894.366667 | 177894.366667 | False | inconclusive | 1 |
| kodim01/420/p0/d4 | 1.029310 | 1.012307 | 1.035007 | 1.029254 | 1.027873 | 98628.300000 | 98628.300000 | True | baseline | 1 |
| kodim01/420/p2/d1 | 1.103265 | 1.008506 | 1.027089 | 1.007009 | 1.011526 | 228017.000000 | 228017.000000 | True | baseline | 1 |
| kodim01/420/p2/d4 | 1.117699 | 1.000583 | 1.281302 | 1.010787 | 1.017809 | 151012.300000 | 151012.300000 | False | inconclusive | 1 |
| kodim02/444/p0/d1 | 1.024384 | 1.000567 | 1.039252 | 1.030852 | 1.027213 | 143148.733333 | 143148.733333 | True | baseline | 1 |
| kodim02/444/p0/d4 | 0.991345 | 1.006418 | 1.015473 | 1.015598 | 1.017202 | 80912.000000 | 80912.000000 | True | inconclusive | 1 |
| kodim02/444/p2/d1 | 1.136147 | 1.005553 | 1.029349 | 1.017860 | 1.019056 | 222513.800000 | 222531.400000 | True | baseline | 1 |
| kodim02/444/p2/d4 | 1.140845 | 1.003701 | 1.009088 | 1.011008 | 1.009078 | 138988.300000 | 138988.300000 | True | baseline | 1 |
| kodim02/420/p0/d1 | 1.061141 | 1.006101 | 1.014941 | 1.007945 | 1.010292 | 122224.000000 | 122224.000000 | True | baseline | 1 |
| kodim02/420/p0/d4 | 1.022590 | 1.005223 | 1.215962 | 1.037584 | 1.024200 | 68352.000000 | 68352.000000 | False | inconclusive | 1 |
| kodim02/420/p2/d1 | 1.108531 | 1.001206 | 1.007331 | 1.006329 | 1.004827 | 185704.000000 | 185704.000000 | True | baseline | 1 |
| kodim02/420/p2/d4 | 1.120032 | 1.002594 | 1.012942 | 1.009973 | 1.011950 | 116532.300000 | 116532.300000 | True | baseline | 1 |
| kodim04/444/p0/d1 | 1.023951 | 1.007411 | 1.076595 | 1.028786 | 1.065466 | 143784.433333 | 143750.466667 | True | inconclusive | 1 |
| kodim04/444/p0/d4 | 0.991615 | 1.002297 | 1.010037 | 1.006696 | 1.010866 | 84800.000000 | 84800.000000 | True | current | 1 |
| kodim04/444/p2/d1 | 1.124126 | 1.000525 | 1.046975 | 1.014827 | 1.017321 | 204262.000000 | 204279.700000 | True | baseline | 1 |
| kodim04/444/p2/d4 | 1.136083 | 1.001928 | 1.013467 | 1.011031 | 1.007271 | 136488.000000 | 136488.000000 | True | baseline | 1 |
| kodim04/420/p0/d1 | 1.048247 | 1.000992 | 1.354349 | 1.034363 | 1.038763 | 129148.300000 | 129148.300000 | False | inconclusive | 1 |
| kodim04/420/p0/d4 | 1.019062 | 1.003656 | 1.014762 | 1.012319 | 1.010659 | 77696.000000 | 77696.000000 | True | baseline | 1 |
| kodim04/420/p2/d1 | 1.106968 | 1.001267 | 1.017399 | 1.008810 | 1.004590 | 180464.000000 | 180464.000000 | True | baseline | 1 |
| kodim04/420/p2/d4 | 1.115825 | 1.001944 | 1.021796 | 1.009364 | 1.009503 | 125572.300000 | 125572.300000 | True | baseline | 1 |
| kodim22/444/p0/d1 | 1.011864 | 1.006045 | 1.057860 | 1.034675 | 1.033149 | 161236.733333 | 161236.733333 | True | inconclusive | 1 |
| kodim22/444/p0/d4 | 0.996908 | 1.001276 | 1.070742 | 1.017769 | 1.014708 | 91472.000000 | 91472.000000 | True | inconclusive | 1 |
| kodim22/444/p2/d1 | 1.128877 | 1.000742 | 1.034574 | 1.018160 | 1.026903 | 209791.700000 | 209791.700000 | True | baseline | 1 |
| kodim22/444/p2/d4 | 1.138478 | 1.000914 | 1.016067 | 1.015470 | 1.010395 | 136824.000000 | 136824.000000 | True | baseline | 1 |
| kodim22/420/p0/d1 | 1.037287 | 1.001199 | 1.104066 | 1.038051 | 1.022483 | 144852.733333 | 144814.266667 | False | inconclusive | 1 |
| kodim22/420/p0/d4 | 1.024556 | 1.000692 | 1.011622 | 1.009229 | 1.009059 | 83096.000000 | 83096.000000 | True | baseline | 1 |
| kodim22/420/p2/d1 | 1.101397 | 1.003012 | 1.027363 | 1.016594 | 1.013649 | 195436.300000 | 195449.000000 | True | baseline | 1 |
| kodim22/420/p2/d4 | 1.115711 | 1.001342 | 1.009280 | 1.007440 | 1.012993 | 130556.300000 | 130556.300000 | True | baseline | 1 |
| low-contrast/444/p0/d1 | 1.014615 | 1.003545 | 1.019561 | 1.018657 | 1.008250 | 61504.000000 | 61504.000000 | True | baseline | 1 |
| low-contrast/444/p0/d4 | 0.970346 | 1.022700 | 1.015067 | 1.019445 | 1.018978 | 51488.000000 | 51488.000000 | True | current | 1 |
| low-contrast/444/p2/d1 | 1.116851 | 1.007480 | 1.163501 | 1.065149 | 1.054254 | 130864.000000 | 130864.000000 | False | inconclusive | 1 |
| low-contrast/444/p2/d4 | 1.113987 | 1.004202 | 1.027861 | 1.043395 | 1.025083 | 104204.300000 | 104204.300000 | True | baseline | 1 |
| low-contrast/420/p0/d1 | 1.023146 | 1.002869 | 1.023247 | 1.013758 | 1.019820 | 58200.000000 | 58200.000000 | True | baseline | 1 |
| low-contrast/420/p0/d4 | 0.987943 | 1.008778 | 1.025531 | 1.024442 | 1.010624 | 50416.000000 | 50416.000000 | True | inconclusive | 1 |
| low-contrast/420/p2/d1 | 1.092266 | 1.004588 | 1.028024 | 1.047900 | 1.023675 | 111784.000000 | 111784.000000 | True | baseline | 1 |
| low-contrast/420/p2/d4 | 1.093929 | 1.004201 | 1.093701 | 1.022501 | 1.016856 | 92720.000000 | 92720.000000 | True | baseline | 1 |
| low-noise/444/p0/d1 | 1.063904 | 1.011898 | 1.035801 | 1.022666 | 1.022512 | 64240.000000 | 64240.000000 | True | baseline | 1 |
| low-noise/444/p0/d4 | 0.972788 | 1.003262 | 1.020704 | 1.013142 | 1.013015 | 49592.000000 | 49592.000000 | True | current | 1 |
| low-noise/444/p2/d1 | 1.141045 | 1.000243 | 1.024487 | 1.021091 | 1.011665 | 111020.300000 | 111020.300000 | True | baseline | 1 |
| low-noise/444/p2/d4 | 1.112675 | 1.000517 | 1.015973 | 1.012512 | 1.016140 | 79192.000000 | 79192.000000 | True | baseline | 1 |
| low-noise/420/p0/d1 | 1.044230 | 1.007917 | 1.020807 | 1.018400 | 1.017073 | 53760.000000 | 53760.000000 | True | baseline | 1 |
| low-noise/420/p0/d4 | 0.987874 | 1.009164 | 1.023050 | 1.021342 | 1.026438 | 43144.000000 | 43144.000000 | True | inconclusive | 1 |
| low-noise/420/p2/d1 | 1.115431 | 1.018252 | 1.038862 | 1.026861 | 1.036876 | 101540.300000 | 101540.300000 | True | baseline | 1 |
| low-noise/420/p2/d4 | 1.096254 | 1.008375 | 1.073654 | 1.012778 | 1.015037 | 79848.000000 | 79848.000000 | True | baseline | 1 |
| full-noise/444/p0/d1 | 1.041834 | 1.002726 | 1.034819 | 1.014088 | 1.013310 | 110064.000000 | 110064.000000 | True | baseline | 1 |
| full-noise/444/p0/d4 | 1.033685 | 1.013534 | 1.042754 | 1.022225 | 1.017118 | 77940.300000 | 77940.300000 | True | baseline | 1 |
| full-noise/444/p2/d1 | 1.065257 | 1.003454 | 1.006810 | 1.008804 | 1.006271 | 191928.000000 | 191928.000000 | True | baseline | 1 |
| full-noise/444/p2/d4 | 1.097024 | 1.007803 | 1.019120 | 1.013702 | 1.015395 | 130316.300000 | 130316.300000 | True | baseline | 1 |
| full-noise/420/p0/d1 | 1.017916 | 1.001325 | 1.022449 | 1.017573 | 1.019365 | 81488.000000 | 81488.000000 | True | baseline | 1 |
| full-noise/420/p0/d4 | 1.028480 | 1.002085 | 1.036077 | 1.012907 | 1.016865 | 61448.000000 | 61448.000000 | True | baseline | 1 |
| full-noise/420/p2/d1 | 1.052750 | 1.001025 | 1.009639 | 1.007541 | 1.005644 | 135784.000000 | 135784.000000 | True | baseline | 1 |
| full-noise/420/p2/d4 | 1.080403 | 1.011415 | 1.021534 | 1.012920 | 1.022344 | 109328.000000 | 109328.000000 | True | baseline | 1 |
| tiny-odd-edges/444/p0/d1 | 1.003451 | 1.002561 | 1.104885 | 1.047970 | 1.042198 | 36760.860000 | 36760.860000 | False | inconclusive | 1 |
| tiny-odd-edges/444/p0/d4 | 1.024704 | 1.006388 | 1.972405 | 1.029452 | 1.055876 | 36464.000000 | 36464.000000 | False | inconclusive | 1 |
| tiny-odd-edges/444/p2/d1 | 1.019786 | 1.001443 | 1.074940 | 1.015572 | 1.021596 | 64408.860000 | 64408.860000 | True | baseline | 1 |
| tiny-odd-edges/444/p2/d4 | 1.013045 | 1.007442 | 1.224881 | 1.023071 | 1.017844 | 64080.860000 | 64080.860000 | False | inconclusive | 1 |
| tiny-odd-edges/420/p0/d1 | 1.015558 | 1.007487 | 1.273790 | 1.029862 | 1.039641 | 36568.860000 | 36568.860000 | False | inconclusive | 1 |
| tiny-odd-edges/420/p0/d4 | 1.007496 | 1.013918 | 1.116775 | 1.030635 | 1.042370 | 36392.000000 | 36392.000000 | False | inconclusive | 1 |
| tiny-odd-edges/420/p2/d1 | 1.025128 | 1.012447 | 1.092960 | 1.029213 | 1.044678 | 65496.860000 | 65496.860000 | True | baseline | 1 |
| tiny-odd-edges/420/p2/d4 | 1.020070 | 1.012740 | 1.047881 | 1.024779 | 1.026229 | 65264.860000 | 65264.860000 | True | baseline | 1 |

Invalid evidence:

- kodim01/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim01/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim01/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim01/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim02/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim02/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim04/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim22/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- low-contrast/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4: current: repeated legs within a quartet differ by over10%.
