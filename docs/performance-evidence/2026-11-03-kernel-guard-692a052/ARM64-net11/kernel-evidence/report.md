# Interleaved kernel comparison

Selected managed variants, measured as alternating ABBA/BAAB quartets in one process. Forced pairs change one kernel and hold the other Automatic. With --include-automatic, automatic-policy compares both Original against both Automatic. Exact preference maps identify each baseline and candidate. All variants produce identical JPEG bytes for every completed comparison.

Ratios below1 favor the candidate. Each ratio is the median of quartet geometric-mean ratios; allocations are mean bytes per encode. MAD, within-quartet repeats, p90/p10 drift and measurement-order bias guard against timing-state changes. Invalid rows retain raw samples but cannot establish a speed result. An automatic-policy row with candidate reachable=false selects neither new kernel and still measures the complete baseline/candidate dispatch paths. No native or full-corpus parity verdict is implied.

Evidence preference requires valid samples and every paired ratio beyond the same side of1+/-0.005; mixed or small effects remain inconclusive. raw-comparisons/ retains each completed quartet before analysis, including malformed or nonfinite raw diagnostics.

| Case | Comparison | Baseline preferences | Candidate preferences | Candidate reachable | Iterations/leg | Baseline ms | Candidate ms | Paired ratio | Baseline B/encode | Candidate B/encode | Evidence preference | Valid |
| --- | --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| kodim01/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 7.6846 | 7.6750 | 0.998049 | 193784.0 | 193784.0 | inconclusive | True |
| kodim01/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 7.5497 | 7.5499 | 1.000401 | 193784.0 | 193784.0 | inconclusive | True |
| kodim01/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 6.9966 | 7.0167 | 0.998650 | 108248.7 | 108232.0 | inconclusive | True |
| kodim01/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 7.0230 | 7.0190 | 0.999646 | 108232.0 | 108248.7 | inconclusive | True |
| kodim01/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 12.6425 | 12.6378 | 0.999798 | 252300.7 | 252300.7 | inconclusive | True |
| kodim01/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 12.6913 | 12.6768 | 1.001621 | 252305.0 | 252300.7 | inconclusive | True |
| kodim01/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 10.5111 | 10.5060 | 0.999321 | 163264.6 | 163256.0 | inconclusive | True |
| kodim01/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 10.5036 | 10.5070 | 1.000149 | 163256.0 | 163256.0 | inconclusive | True |
| kodim01/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 5.3929 | 5.3602 | 0.998896 | 176185.2 | 176140.3 | inconclusive | True |
| kodim01/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 5.3709 | 5.3503 | 0.999044 | 176185.2 | 176136.0 | inconclusive | True |
| kodim01/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 4.8092 | 4.8026 | 0.999113 | 96904.0 | 96904.0 | inconclusive | True |
| kodim01/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 4.8221 | 4.8212 | 1.000029 | 96908.3 | 96904.0 | inconclusive | True |
| kodim01/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 8.8497 | 8.8381 | 0.999610 | 225260.0 | 225264.3 | inconclusive | True |
| kodim01/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 8.8599 | 8.8701 | 0.999399 | 225255.7 | 225236.3 | inconclusive | True |
| kodim01/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 6.8750 | 6.8688 | 0.999731 | 148244.3 | 148240.0 | inconclusive | True |
| kodim01/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 6.8734 | 6.8733 | 1.000167 | 148244.3 | 148240.0 | inconclusive | True |
| low-contrast/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0353 | 1.0357 | 1.000815 | 59784.0 | 59788.3 | inconclusive | False |
| low-contrast/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.0345 | 1.0348 | 1.000482 | 59784.0 | 59784.0 | inconclusive | True |
| low-contrast/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9705 | 0.9696 | 0.999345 | 49768.0 | 49768.0 | inconclusive | True |
| low-contrast/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.9712 | 0.9707 | 0.998719 | 49768.0 | 49768.0 | inconclusive | True |
| low-contrast/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.6341 | 1.6335 | 0.998980 | 128240.0 | 128240.0 | inconclusive | True |
| low-contrast/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.6344 | 1.6342 | 1.000356 | 128244.3 | 128244.3 | inconclusive | True |
| low-contrast/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.4247 | 1.4236 | 0.998315 | 101576.0 | 101576.0 | inconclusive | True |
| low-contrast/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.4248 | 1.4252 | 0.999877 | 101576.0 | 101580.3 | inconclusive | True |
| low-contrast/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7343 | 0.7332 | 0.999732 | 56480.0 | 56484.3 | inconclusive | True |
| low-contrast/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7342 | 0.7350 | 0.999776 | 56480.0 | 56480.0 | inconclusive | True |
| low-contrast/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.6810 | 0.6793 | 0.997475 | 48696.0 | 48700.3 | inconclusive | True |
| low-contrast/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.6830 | 0.6833 | 1.000460 | 48696.0 | 48696.0 | inconclusive | True |
| low-contrast/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.1120 | 1.1131 | 0.999482 | 109016.0 | 109020.3 | inconclusive | True |
| low-contrast/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.1119 | 1.1114 | 0.998586 | 109016.0 | 109016.0 | inconclusive | True |
| low-contrast/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9308 | 0.9298 | 0.998328 | 89956.3 | 89952.0 | inconclusive | True |
| low-contrast/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.9350 | 0.9336 | 0.999169 | 89957.7 | 89952.0 | inconclusive | True |
| low-noise/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0601 | 1.0589 | 0.999368 | 62520.0 | 62520.0 | inconclusive | False |
| low-noise/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.0585 | 1.0579 | 0.999590 | 62524.3 | 62520.0 | inconclusive | True |
| low-noise/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9416 | 0.9412 | 0.998950 | 47872.0 | 47872.0 | inconclusive | True |
| low-noise/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.9449 | 0.9446 | 0.999713 | 47872.0 | 47872.0 | inconclusive | True |
| low-noise/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.6570 | 1.6565 | 0.998488 | 108396.3 | 108392.0 | inconclusive | True |
| low-noise/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.6592 | 1.6594 | 0.999249 | 108396.3 | 108392.0 | inconclusive | True |
| low-noise/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.3518 | 1.3492 | 0.998118 | 76572.3 | 76568.0 | inconclusive | True |
| low-noise/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.3500 | 1.3478 | 0.999023 | 76568.0 | 76568.0 | inconclusive | True |
| low-noise/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7409 | 0.7391 | 0.997344 | 52044.3 | 52040.0 | inconclusive | True |
| low-noise/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7418 | 0.7407 | 0.999141 | 52040.0 | 52040.0 | inconclusive | True |
| low-noise/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.6656 | 0.6648 | 0.998685 | 41424.0 | 41428.3 | inconclusive | True |
| low-noise/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.6663 | 0.6656 | 0.999314 | 41424.0 | 41424.0 | inconclusive | True |
| low-noise/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0942 | 1.0937 | 1.000014 | 98768.0 | 98772.3 | inconclusive | True |
| low-noise/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.0946 | 1.0942 | 0.999905 | 98768.0 | 98768.0 | inconclusive | True |
| low-noise/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.8770 | 0.8760 | 0.998824 | 77080.0 | 77080.0 | inconclusive | True |
| low-noise/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.8763 | 0.8763 | 1.000109 | 77084.3 | 77080.0 | inconclusive | True |
| tiny-odd-edges/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0360 | 0.0360 | 1.000702 | 35040.0 | 35040.9 | inconclusive | True |
| tiny-odd-edges/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0357 | 0.0358 | 1.005229 | 35045.0 | 35040.9 | inconclusive | False |
| tiny-odd-edges/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0364 | 0.0365 | 1.000358 | 34744.9 | 34744.0 | inconclusive | True |
| tiny-odd-edges/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0366 | 0.0364 | 0.997734 | 34744.9 | 34744.0 | inconclusive | True |
| tiny-odd-edges/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0755 | 0.0756 | 0.999926 | 61785.7 | 61784.9 | inconclusive | True |
| tiny-odd-edges/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0756 | 0.0756 | 1.001636 | 61784.9 | 61784.9 | inconclusive | True |
| tiny-odd-edges/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0699 | 0.0699 | 0.999647 | 61456.0 | 61456.9 | inconclusive | True |
| tiny-odd-edges/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0697 | 0.0697 | 0.999300 | 61456.9 | 61457.7 | inconclusive | True |
| tiny-odd-edges/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0331 | 0.0330 | 0.997575 | 34848.9 | 34848.0 | inconclusive | True |
| tiny-odd-edges/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0331 | 0.0331 | 0.997845 | 34848.0 | 34848.0 | inconclusive | True |
| tiny-odd-edges/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0339 | 0.0337 | 0.996212 | 34673.7 | 34672.0 | inconclusive | True |
| tiny-odd-edges/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0339 | 0.0341 | 1.001138 | 34672.9 | 34672.0 | inconclusive | True |
| tiny-odd-edges/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0675 | 0.0675 | 0.998974 | 62728.9 | 62728.9 | inconclusive | True |
| tiny-odd-edges/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0675 | 0.0675 | 0.999234 | 62729.7 | 62728.9 | inconclusive | True |
| tiny-odd-edges/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0649 | 0.0649 | 0.999145 | 62496.9 | 62496.9 | inconclusive | True |
| tiny-odd-edges/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0648 | 0.0649 | 1.001902 | 62496.9 | 62496.9 | inconclusive | True |
| guard-below-32k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.8329 | 0.8332 | 0.999415 | 79036.3 | 79032.0 | inconclusive | True |
| guard-below-32k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.8327 | 0.8324 | 0.999851 | 79032.0 | 79032.0 | inconclusive | True |
| guard-below-32k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7034 | 0.7008 | 0.997849 | 57336.0 | 57336.0 | inconclusive | True |
| guard-below-32k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7020 | 0.7015 | 0.999330 | 57340.3 | 57336.0 | inconclusive | True |
| guard-below-32k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.7504 | 1.7502 | 1.000047 | 154400.0 | 154404.3 | inconclusive | True |
| guard-below-32k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.7496 | 1.7516 | 1.000679 | 154400.0 | 154404.3 | inconclusive | True |
| guard-below-32k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.2053 | 1.2046 | 1.000357 | 120360.0 | 120364.3 | inconclusive | True |
| guard-below-32k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.2068 | 1.2061 | 0.999726 | 120360.0 | 120364.3 | inconclusive | True |
| guard-below-32k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5558 | 0.5554 | 0.998740 | 63136.0 | 63136.0 | inconclusive | True |
| guard-below-32k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.5550 | 0.5550 | 1.000438 | 63140.3 | 63136.0 | inconclusive | True |
| guard-below-32k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4718 | 0.4716 | 0.998835 | 50264.0 | 50264.0 | inconclusive | True |
| guard-below-32k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.4717 | 0.4715 | 0.999492 | 50264.0 | 50264.0 | inconclusive | True |
| guard-below-32k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.1074 | 1.1067 | 0.999948 | 122108.3 | 122104.0 | inconclusive | True |
| guard-below-32k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.1058 | 1.1047 | 0.999596 | 122104.0 | 122108.3 | inconclusive | True |
| guard-below-32k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7686 | 0.7685 | 0.999191 | 114680.0 | 114684.3 | inconclusive | True |
| guard-below-32k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7651 | 0.7656 | 0.999787 | 114680.0 | 114684.3 | inconclusive | True |
| guard-at-32k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.8429 | 0.8416 | 0.998896 | 79004.3 | 79000.0 | inconclusive | True |
| guard-at-32k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.8387 | 0.8403 | 0.999666 | 79000.0 | 79004.3 | inconclusive | True |
| guard-at-32k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7069 | 0.7061 | 0.998407 | 57320.0 | 57320.0 | inconclusive | True |
| guard-at-32k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7081 | 0.7074 | 0.999885 | 57324.3 | 57320.0 | inconclusive | True |
| guard-at-32k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.7442 | 1.7449 | 1.000372 | 154344.0 | 154344.0 | inconclusive | True |
| guard-at-32k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.7444 | 1.7416 | 0.998552 | 154344.0 | 154348.3 | inconclusive | True |
| guard-at-32k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.2010 | 1.2007 | 1.001222 | 120320.0 | 120320.0 | inconclusive | True |
| guard-at-32k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.2004 | 1.2022 | 1.000776 | 120320.0 | 120324.3 | inconclusive | True |
| guard-at-32k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5547 | 0.5540 | 0.999083 | 63112.0 | 63112.0 | inconclusive | True |
| guard-at-32k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.5553 | 0.5549 | 0.998546 | 63112.0 | 63116.3 | inconclusive | True |
| guard-at-32k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4705 | 0.4699 | 0.998653 | 50232.0 | 50232.0 | inconclusive | True |
| guard-at-32k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.4711 | 0.4713 | 1.000658 | 50232.0 | 50232.0 | inconclusive | True |
| guard-at-32k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0982 | 1.0977 | 0.999552 | 122072.0 | 122076.3 | inconclusive | True |
| guard-at-32k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.0972 | 1.0974 | 1.000229 | 122072.0 | 122076.3 | inconclusive | True |
| guard-at-32k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7639 | 0.7631 | 0.998406 | 114640.0 | 114644.3 | inconclusive | True |
| guard-at-32k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7635 | 0.7645 | 1.000607 | 114640.0 | 114644.3 | inconclusive | True |
| guard-above-32k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.8578 | 0.8577 | 0.999524 | 79652.3 | 79648.0 | inconclusive | True |
| guard-above-32k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.8601 | 0.8600 | 0.999903 | 79648.0 | 79648.0 | inconclusive | True |
| guard-above-32k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7251 | 0.7235 | 0.998532 | 57736.0 | 57736.0 | inconclusive | True |
| guard-above-32k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7235 | 0.7232 | 1.000130 | 57736.0 | 57736.0 | inconclusive | True |
| guard-above-32k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.7905 | 1.7911 | 1.000522 | 155016.0 | 155020.3 | inconclusive | True |
| guard-above-32k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.7927 | 1.7925 | 0.998920 | 155016.0 | 155016.0 | inconclusive | True |
| guard-above-32k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.2408 | 1.2405 | 0.999095 | 120796.3 | 120792.0 | inconclusive | True |
| guard-above-32k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.2423 | 1.2420 | 0.999738 | 120837.8 | 120792.0 | inconclusive | True |
| guard-above-32k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5734 | 0.5732 | 0.998611 | 63580.3 | 63576.0 | inconclusive | True |
| guard-above-32k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.5742 | 0.5741 | 0.999850 | 63580.3 | 63576.0 | inconclusive | True |
| guard-above-32k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4902 | 0.4900 | 1.000413 | 50568.0 | 50568.0 | inconclusive | True |
| guard-above-32k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.4901 | 0.4906 | 1.001265 | 50568.0 | 50568.0 | inconclusive | True |
| guard-above-32k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.1328 | 1.1318 | 0.999153 | 122552.0 | 122556.3 | inconclusive | True |
| guard-above-32k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.1360 | 1.1337 | 0.998182 | 122556.3 | 122552.0 | inconclusive | True |
| guard-above-32k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7951 | 0.7941 | 0.999041 | 115044.3 | 115040.0 | inconclusive | True |
| guard-above-32k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7940 | 0.7945 | 1.000581 | 115044.3 | 115040.0 | inconclusive | True |
| narrow-tall/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.1667 | 1.1644 | 0.997329 | 79072.0 | 79072.0 | inconclusive | True |
| narrow-tall/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.1664 | 1.1663 | 0.999788 | 79076.3 | 79072.0 | inconclusive | True |
| narrow-tall/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0351 | 1.0328 | 0.998867 | 57416.0 | 57420.3 | inconclusive | True |
| narrow-tall/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.0291 | 1.0283 | 0.999784 | 57416.0 | 57416.0 | inconclusive | True |
| narrow-tall/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.1213 | 2.1185 | 0.998820 | 154400.6 | 154392.0 | inconclusive | True |
| narrow-tall/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 2.1245 | 2.1228 | 0.999718 | 154396.3 | 154392.0 | inconclusive | True |
| narrow-tall/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.5757 | 1.5724 | 0.998568 | 120404.3 | 120400.0 | inconclusive | True |
| narrow-tall/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.5777 | 1.5779 | 0.999768 | 120404.3 | 120400.5 | inconclusive | True |
| narrow-tall/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0075 | 1.0056 | 0.997152 | 76820.3 | 76816.0 | inconclusive | True |
| narrow-tall/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.0079 | 1.0073 | 1.000993 | 76816.0 | 76844.3 | inconclusive | False |
| narrow-tall/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9048 | 0.9028 | 0.997623 | 59864.0 | 59864.0 | inconclusive | True |
| narrow-tall/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.9048 | 0.9036 | 0.998695 | 59868.3 | 59864.0 | inconclusive | True |
| narrow-tall/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.7143 | 1.7108 | 0.997916 | 147008.0 | 147012.3 | inconclusive | True |
| narrow-tall/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.7152 | 1.7157 | 0.998691 | 147012.3 | 147008.0 | inconclusive | True |
| narrow-tall/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.2944 | 1.2927 | 0.998342 | 135680.0 | 135688.6 | inconclusive | True |
| narrow-tall/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.2963 | 1.2954 | 0.999068 | 135680.0 | 135684.3 | inconclusive | True |

64 narrow-zigzag comparisons were not applicable to this pipeline's vector width; skipped.json retains their case identities and reason.

Invalid evidence:
- low-contrast/444/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- low-noise/444/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- narrow-tall/420/p0/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
