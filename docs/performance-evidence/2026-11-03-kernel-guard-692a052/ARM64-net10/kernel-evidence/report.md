# Interleaved kernel comparison

Selected managed variants, measured as alternating ABBA/BAAB quartets in one process. Forced pairs change one kernel and hold the other Automatic. With --include-automatic, automatic-policy compares both Original against both Automatic. Exact preference maps identify each baseline and candidate. All variants produce identical JPEG bytes for every completed comparison.

Ratios below1 favor the candidate. Each ratio is the median of quartet geometric-mean ratios; allocations are mean bytes per encode. MAD, within-quartet repeats, p90/p10 drift and measurement-order bias guard against timing-state changes. Invalid rows retain raw samples but cannot establish a speed result. An automatic-policy row with candidate reachable=false selects neither new kernel and still measures the complete baseline/candidate dispatch paths. No native or full-corpus parity verdict is implied.

Evidence preference requires valid samples and every paired ratio beyond the same side of1+/-0.005; mixed or small effects remain inconclusive. raw-comparisons/ retains each completed quartet before analysis, including malformed or nonfinite raw diagnostics.

| Case | Comparison | Baseline preferences | Candidate preferences | Candidate reachable | Iterations/leg | Baseline ms | Candidate ms | Paired ratio | Baseline B/encode | Candidate B/encode | Evidence preference | Valid |
| --- | --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| kodim01/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 7.9760 | 7.9735 | 0.998279 | 193869.7 | 193784.0 | inconclusive | True |
| kodim01/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 8.0384 | 7.9991 | 0.999535 | 193832.0 | 193831.8 | inconclusive | True |
| kodim01/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 7.4177 | 7.3925 | 0.999600 | 108248.7 | 108232.0 | inconclusive | True |
| kodim01/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 7.3539 | 7.3526 | 0.999876 | 108232.0 | 108232.0 | inconclusive | True |
| kodim01/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 13.1240 | 13.1251 | 1.001971 | 252300.7 | 252277.6 | inconclusive | True |
| kodim01/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 13.1378 | 13.1230 | 0.998497 | 252305.0 | 252272.0 | inconclusive | False |
| kodim01/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 10.9169 | 10.8970 | 0.998003 | 163264.6 | 163256.0 | inconclusive | True |
| kodim01/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 10.8879 | 10.8944 | 0.999814 | 163256.0 | 163256.0 | inconclusive | True |
| kodim01/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 5.6533 | 5.6464 | 1.000354 | 176136.0 | 176147.3 | inconclusive | True |
| kodim01/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 5.6821 | 5.6750 | 1.000294 | 176136.0 | 176136.0 | inconclusive | True |
| kodim01/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 5.0757 | 5.0624 | 0.998307 | 96904.0 | 96904.0 | inconclusive | True |
| kodim01/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 5.0708 | 5.0694 | 0.999960 | 96908.3 | 96904.0 | inconclusive | True |
| kodim01/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 9.2094 | 9.2131 | 1.000518 | 225232.0 | 225264.3 | inconclusive | True |
| kodim01/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 9.2451 | 9.2412 | 1.000359 | 225232.0 | 225236.3 | inconclusive | True |
| kodim01/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 7.1509 | 7.1368 | 0.997487 | 148244.3 | 148240.0 | inconclusive | True |
| kodim01/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 7.1451 | 7.1397 | 0.998753 | 148244.3 | 148240.0 | inconclusive | True |
| low-contrast/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0863 | 1.0864 | 0.999917 | 59784.0 | 59788.3 | inconclusive | True |
| low-contrast/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.0898 | 1.0909 | 1.001113 | 59784.0 | 59784.0 | inconclusive | True |
| low-contrast/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0194 | 1.0193 | 1.001033 | 49772.3 | 49768.0 | inconclusive | True |
| low-contrast/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.0212 | 1.0230 | 1.000048 | 49768.0 | 49768.0 | inconclusive | True |
| low-contrast/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.6990 | 1.7002 | 0.999706 | 128244.3 | 128240.0 | inconclusive | True |
| low-contrast/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.7032 | 1.7001 | 0.999693 | 128244.3 | 128244.3 | inconclusive | True |
| low-contrast/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.4839 | 1.4841 | 1.000736 | 101576.0 | 101576.0 | inconclusive | True |
| low-contrast/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.4880 | 1.4865 | 0.999325 | 101576.0 | 101580.3 | inconclusive | True |
| low-contrast/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7694 | 0.7688 | 1.000804 | 56480.0 | 56484.3 | inconclusive | True |
| low-contrast/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7698 | 0.7726 | 1.000742 | 56480.0 | 56480.0 | inconclusive | True |
| low-contrast/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7171 | 0.7141 | 0.998277 | 48696.0 | 48700.3 | inconclusive | True |
| low-contrast/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7170 | 0.7171 | 1.000463 | 48696.0 | 48696.0 | inconclusive | True |
| low-contrast/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.1601 | 1.1590 | 0.998908 | 109016.0 | 109020.3 | inconclusive | True |
| low-contrast/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.1549 | 1.1541 | 0.998713 | 109016.0 | 109020.3 | inconclusive | True |
| low-contrast/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9706 | 0.9705 | 0.996882 | 89956.3 | 89952.0 | inconclusive | True |
| low-contrast/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.9700 | 0.9698 | 0.998031 | 89956.3 | 89952.0 | inconclusive | True |
| low-noise/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.1188 | 1.1152 | 0.997485 | 62520.0 | 62520.0 | inconclusive | True |
| low-noise/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.1180 | 1.1158 | 1.000042 | 62524.3 | 62520.0 | inconclusive | True |
| low-noise/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9961 | 0.9962 | 1.000609 | 47872.0 | 47872.0 | inconclusive | True |
| low-noise/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.9986 | 0.9968 | 0.998544 | 47876.3 | 47872.0 | inconclusive | True |
| low-noise/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.7313 | 1.7308 | 0.999360 | 108396.3 | 108392.0 | inconclusive | True |
| low-noise/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.7262 | 1.7226 | 0.998710 | 108396.3 | 108392.0 | inconclusive | True |
| low-noise/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.4045 | 1.4023 | 0.997003 | 76572.3 | 76568.0 | inconclusive | True |
| low-noise/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.4088 | 1.4094 | 1.000540 | 76568.0 | 76568.0 | inconclusive | True |
| low-noise/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7821 | 0.7807 | 0.999062 | 52040.0 | 52040.0 | inconclusive | True |
| low-noise/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7790 | 0.7794 | 0.999565 | 52040.0 | 52040.0 | inconclusive | True |
| low-noise/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7024 | 0.7009 | 0.997673 | 41428.3 | 41424.0 | inconclusive | True |
| low-noise/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7022 | 0.7011 | 0.997970 | 41424.0 | 41424.0 | inconclusive | True |
| low-noise/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.1475 | 1.1451 | 0.998607 | 98768.0 | 98768.0 | inconclusive | True |
| low-noise/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.1472 | 1.1456 | 0.998596 | 98768.0 | 98768.0 | inconclusive | True |
| low-noise/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9161 | 0.9161 | 1.001692 | 77080.0 | 77084.3 | inconclusive | True |
| low-noise/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.9174 | 0.9190 | 1.001423 | 77080.0 | 77080.0 | inconclusive | True |
| tiny-odd-edges/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0378 | 0.0376 | 1.000465 | 35040.0 | 35041.7 | inconclusive | False |
| tiny-odd-edges/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0378 | 0.0377 | 1.000073 | 35040.0 | 35040.9 | inconclusive | False |
| tiny-odd-edges/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0379 | 0.0378 | 0.994886 | 34744.9 | 34744.0 | inconclusive | False |
| tiny-odd-edges/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0379 | 0.0377 | 0.997481 | 34744.0 | 34744.9 | inconclusive | False |
| tiny-odd-edges/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0792 | 0.0791 | 0.998071 | 61785.7 | 61784.0 | inconclusive | True |
| tiny-odd-edges/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0801 | 0.0804 | 1.009133 | 61784.9 | 61784.9 | inconclusive | True |
| tiny-odd-edges/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0728 | 0.0729 | 0.999021 | 61456.9 | 61457.7 | inconclusive | True |
| tiny-odd-edges/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0727 | 0.0732 | 1.010392 | 61456.0 | 61457.7 | inconclusive | True |
| tiny-odd-edges/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0346 | 0.0347 | 1.001663 | 34848.0 | 34848.0 | inconclusive | True |
| tiny-odd-edges/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0347 | 0.0348 | 1.006762 | 34848.9 | 34848.9 | inconclusive | False |
| tiny-odd-edges/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0353 | 0.0353 | 0.992133 | 34672.9 | 34672.0 | inconclusive | False |
| tiny-odd-edges/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0350 | 0.0352 | 1.008642 | 34672.0 | 34672.9 | inconclusive | True |
| tiny-odd-edges/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0701 | 0.0705 | 0.998830 | 62728.9 | 62729.7 | inconclusive | True |
| tiny-odd-edges/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0707 | 0.0709 | 0.992982 | 62728.9 | 62728.9 | inconclusive | True |
| tiny-odd-edges/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0690 | 0.0687 | 0.995624 | 62496.9 | 62496.9 | inconclusive | True |
| tiny-odd-edges/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0680 | 0.0685 | 1.000515 | 62496.9 | 62496.9 | inconclusive | True |
| guard-below-32k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.8824 | 0.8794 | 0.998999 | 79032.0 | 79032.0 | inconclusive | True |
| guard-below-32k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.8846 | 0.8824 | 1.000415 | 79036.3 | 79032.0 | inconclusive | True |
| guard-below-32k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7453 | 0.7442 | 0.999299 | 57340.3 | 57336.0 | inconclusive | True |
| guard-below-32k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7423 | 0.7406 | 1.000240 | 57336.0 | 57336.0 | inconclusive | True |
| guard-below-32k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.7903 | 1.7871 | 0.999191 | 154404.3 | 154404.3 | inconclusive | True |
| guard-below-32k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.7908 | 1.7913 | 1.001135 | 154400.0 | 154404.3 | inconclusive | True |
| guard-below-32k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.2583 | 1.2546 | 0.997861 | 120360.0 | 120364.3 | inconclusive | True |
| guard-below-32k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.2578 | 1.2615 | 1.002031 | 120360.0 | 120360.0 | inconclusive | True |
| guard-below-32k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5890 | 0.5880 | 1.000779 | 63136.0 | 63140.3 | inconclusive | False |
| guard-below-32k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.5833 | 0.5834 | 0.998198 | 63136.0 | 63136.0 | inconclusive | True |
| guard-below-32k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4967 | 0.4956 | 0.997556 | 50268.3 | 50264.0 | inconclusive | True |
| guard-below-32k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.4944 | 0.4944 | 1.001398 | 50264.0 | 50264.0 | inconclusive | True |
| guard-below-32k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.1353 | 1.1366 | 1.000097 | 122104.0 | 122108.3 | inconclusive | True |
| guard-below-32k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.1329 | 1.1313 | 1.000919 | 122104.0 | 122108.3 | inconclusive | True |
| guard-below-32k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.8006 | 0.7975 | 0.995227 | 114684.3 | 114680.0 | inconclusive | True |
| guard-below-32k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7995 | 0.7994 | 0.997592 | 114684.3 | 114680.0 | inconclusive | True |
| guard-at-32k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.8925 | 0.8955 | 1.001644 | 79000.0 | 79004.3 | inconclusive | True |
| guard-at-32k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.8958 | 0.8907 | 0.997036 | 79004.3 | 79000.0 | inconclusive | True |
| guard-at-32k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7529 | 0.7530 | 1.000650 | 57320.0 | 57320.0 | inconclusive | True |
| guard-at-32k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7503 | 0.7505 | 1.000941 | 57320.0 | 57324.3 | inconclusive | True |
| guard-at-32k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.7835 | 1.7789 | 0.999318 | 154344.0 | 154348.3 | inconclusive | True |
| guard-at-32k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.7793 | 1.7812 | 1.003659 | 154344.0 | 154348.3 | inconclusive | False |
| guard-at-32k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.2483 | 1.2506 | 1.002348 | 120320.0 | 120324.3 | inconclusive | True |
| guard-at-32k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.2474 | 1.2456 | 0.998744 | 120320.0 | 120324.3 | inconclusive | True |
| guard-at-32k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5832 | 0.5843 | 1.000752 | 63112.0 | 63112.0 | inconclusive | True |
| guard-at-32k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.5841 | 0.5860 | 1.001702 | 63112.0 | 63116.3 | inconclusive | True |
| guard-at-32k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4984 | 0.4982 | 0.999838 | 50232.0 | 50232.0 | inconclusive | True |
| guard-at-32k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.4960 | 0.4984 | 1.001273 | 50232.0 | 50236.3 | inconclusive | True |
| guard-at-32k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.1182 | 1.1178 | 1.000001 | 122103.1 | 122072.0 | inconclusive | True |
| guard-at-32k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.1219 | 1.1187 | 0.998041 | 122076.3 | 122072.0 | inconclusive | True |
| guard-at-32k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7937 | 0.7958 | 1.002163 | 114640.0 | 114644.3 | inconclusive | True |
| guard-at-32k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7943 | 0.7929 | 0.997452 | 114640.0 | 114644.3 | inconclusive | True |
| guard-above-32k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9101 | 0.9088 | 0.999620 | 79648.0 | 79648.0 | inconclusive | True |
| guard-above-32k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.9106 | 0.9101 | 0.997382 | 79652.3 | 79648.0 | inconclusive | True |
| guard-above-32k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7655 | 0.7637 | 1.000692 | 57736.0 | 57736.0 | inconclusive | True |
| guard-above-32k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7669 | 0.7671 | 1.001158 | 57736.0 | 57740.3 | inconclusive | True |
| guard-above-32k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.8264 | 1.8251 | 0.999708 | 155020.3 | 155016.0 | inconclusive | True |
| guard-above-32k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.8189 | 1.8224 | 1.000790 | 155020.3 | 155016.0 | inconclusive | True |
| guard-above-32k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.2883 | 1.2938 | 1.000894 | 120792.0 | 120792.0 | inconclusive | True |
| guard-above-32k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.2933 | 1.2912 | 0.999586 | 120792.0 | 120792.0 | inconclusive | True |
| guard-above-32k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.6045 | 0.6038 | 1.000236 | 63576.0 | 63576.0 | inconclusive | True |
| guard-above-32k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.6084 | 0.6065 | 0.995370 | 63580.3 | 63576.0 | inconclusive | True |
| guard-above-32k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5125 | 0.5126 | 1.000306 | 50568.0 | 50568.0 | inconclusive | True |
| guard-above-32k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.5146 | 0.5141 | 1.004133 | 50568.0 | 50572.3 | inconclusive | True |
| guard-above-32k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.1589 | 1.1608 | 1.000402 | 122552.0 | 122556.3 | inconclusive | True |
| guard-above-32k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.1598 | 1.1601 | 0.998161 | 122552.0 | 122552.0 | inconclusive | True |
| guard-above-32k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.8304 | 0.8294 | 1.000689 | 115063.3 | 115040.0 | inconclusive | False |
| guard-above-32k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.8274 | 0.8275 | 1.000292 | 115044.3 | 115040.0 | inconclusive | True |
| narrow-tall/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.2199 | 1.2183 | 0.997836 | 79072.0 | 79076.3 | inconclusive | True |
| narrow-tall/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.2190 | 1.2169 | 0.998188 | 79072.0 | 79072.0 | inconclusive | True |
| narrow-tall/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0771 | 1.0734 | 0.994503 | 57420.3 | 57416.0 | inconclusive | True |
| narrow-tall/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.0749 | 1.0750 | 0.998341 | 57416.0 | 57416.0 | inconclusive | True |
| narrow-tall/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.1708 | 2.1650 | 0.997793 | 154396.3 | 154396.3 | inconclusive | True |
| narrow-tall/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 2.1690 | 2.1738 | 1.001513 | 154392.0 | 154392.0 | inconclusive | True |
| narrow-tall/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.6406 | 1.6360 | 0.997165 | 120404.3 | 120400.0 | inconclusive | True |
| narrow-tall/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.6352 | 1.6371 | 1.001372 | 120400.0 | 120404.3 | inconclusive | True |
| narrow-tall/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0592 | 1.0560 | 0.997936 | 76816.0 | 76848.9 | inconclusive | False |
| narrow-tall/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.0592 | 1.0563 | 0.999301 | 76816.0 | 76820.3 | inconclusive | True |
| narrow-tall/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9427 | 0.9385 | 0.995315 | 59864.0 | 59864.0 | inconclusive | True |
| narrow-tall/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.9418 | 0.9414 | 1.001385 | 59864.0 | 59868.3 | inconclusive | True |
| narrow-tall/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.7511 | 1.7478 | 0.998655 | 147008.0 | 147008.0 | inconclusive | True |
| narrow-tall/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.7515 | 1.7484 | 0.998725 | 147012.3 | 147008.0 | inconclusive | True |
| narrow-tall/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.3385 | 1.3364 | 0.998743 | 135680.0 | 135684.3 | inconclusive | True |
| narrow-tall/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.3399 | 1.3397 | 0.997988 | 135680.0 | 135684.3 | inconclusive | True |

64 narrow-zigzag comparisons were not applicable to this pipeline's vector width; skipped.json retains their case identities and reason.

Invalid evidence:
- kodim01/444/p2/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- guard-below-32k/420/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- guard-at-32k/444/p2/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- guard-above-32k/420/p2/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- narrow-tall/420/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
