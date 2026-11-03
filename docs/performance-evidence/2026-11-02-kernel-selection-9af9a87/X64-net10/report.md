# Interleaved kernel comparison

Selected managed variants, measured as alternating ABBA/BAAB quartets in one process. Each pair changes one kernel and holds the other Automatic. Automatic output is checked untimed. Original, optimized and automatic JPEG bytes match for every completed comparison.

Ratios below1 favor Optimized. Each ratio is the median of quartet geometric-mean ratios; allocations are mean bytes per encode. MAD, within-quartet repeats, p90/p10 drift and measurement-order bias guard against timing-state changes. Invalid rows retain raw samples but cannot establish a speed result. Unreachable candidates do not measure the changed kernel. No native or full-corpus parity verdict is implied.

Evidence preference requires valid samples and every paired ratio beyond the same side of1+/-0.005; mixed or small effects remain inconclusive. raw-comparisons/ retains each completed quartet before analysis, including malformed or nonfinite raw diagnostics.

| Case | Kernel | Candidate reachable | Iterations/leg | Original ms | Optimized ms | Paired ratio | Original B/encode | Optimized B/encode | Evidence preference | Valid |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| kodim01/444/p0/d1 | fuzzy-erosion | True | 20 | 4.8350 | 4.8060 | 0.996036 | 195752.0 | 195752.0 | inconclusive | False |
| kodim01/444/p0/d1 | narrow-zigzag | True | 20 | 4.8296 | 4.6924 | 0.972696 | 195812.0 | 195756.3 | optimized | True |
| kodim01/444/p0/d4 | fuzzy-erosion | True | 20 | 4.0210 | 4.0032 | 0.990371 | 110208.6 | 110204.3 | inconclusive | True |
| kodim01/444/p0/d4 | narrow-zigzag | True | 20 | 4.0565 | 3.9685 | 0.978655 | 110204.3 | 110204.3 | optimized | True |
| kodim01/444/p2/d1 | fuzzy-erosion | True | 20 | 9.5003 | 9.4661 | 0.996869 | 254525.5 | 254525.5 | inconclusive | True |
| kodim01/444/p2/d1 | narrow-zigzag | True | 20 | 9.4309 | 9.3699 | 0.992713 | 254525.5 | 254504.6 | inconclusive | True |
| kodim01/444/p2/d4 | fuzzy-erosion | True | 20 | 7.0752 | 7.0333 | 0.994700 | 165488.6 | 165488.6 | inconclusive | False |
| kodim01/444/p2/d4 | narrow-zigzag | True | 20 | 7.0828 | 7.0681 | 0.993580 | 165480.0 | 165480.0 | inconclusive | True |
| kodim01/420/p0/d1 | fuzzy-erosion | True | 20 | 3.6377 | 3.5943 | 0.987754 | 178108.3 | 178108.3 | inconclusive | True |
| kodim01/420/p0/d1 | narrow-zigzag | True | 20 | 3.6790 | 3.6064 | 0.980968 | 178104.0 | 178104.0 | optimized | True |
| kodim01/420/p0/d4 | fuzzy-erosion | True | 20 | 3.0061 | 2.9625 | 0.986275 | 98880.6 | 98880.6 | inconclusive | True |
| kodim01/420/p0/d4 | narrow-zigzag | True | 20 | 3.0356 | 2.9536 | 0.978728 | 98876.3 | 98880.6 | inconclusive | True |
| kodim01/420/p2/d1 | fuzzy-erosion | True | 20 | 7.1053 | 7.0819 | 0.997558 | 227540.3 | 227547.3 | inconclusive | True |
| kodim01/420/p2/d1 | narrow-zigzag | True | 20 | 7.0573 | 6.9691 | 0.988465 | 227564.8 | 227544.6 | inconclusive | True |
| kodim01/420/p2/d4 | fuzzy-erosion | True | 20 | 4.8776 | 4.8545 | 0.996268 | 150552.6 | 150556.9 | inconclusive | True |
| kodim01/420/p2/d4 | narrow-zigzag | True | 20 | 4.8801 | 4.8236 | 0.990146 | 150556.9 | 150552.6 | optimized | True |
| low-contrast/444/p0/d1 | fuzzy-erosion | True | 20 | 0.6536 | 0.6452 | 0.991028 | 61760.6 | 61752.0 | inconclusive | True |
| low-contrast/444/p0/d1 | narrow-zigzag | True | 20 | 0.6443 | 0.6297 | 0.980600 | 61752.0 | 61756.3 | inconclusive | True |
| low-contrast/444/p0/d4 | fuzzy-erosion | True | 20 | 0.5444 | 0.5395 | 0.990021 | 51736.0 | 51744.6 | inconclusive | True |
| low-contrast/444/p0/d4 | narrow-zigzag | True | 20 | 0.5439 | 0.5277 | 0.968345 | 51740.3 | 51736.0 | optimized | True |
| low-contrast/444/p2/d1 | fuzzy-erosion | True | 20 | 1.1479 | 1.1469 | 0.999355 | 130468.3 | 130472.6 | inconclusive | True |
| low-contrast/444/p2/d1 | narrow-zigzag | True | 20 | 1.1547 | 1.1309 | 0.973831 | 130472.6 | 130472.6 | inconclusive | False |
| low-contrast/444/p2/d4 | fuzzy-erosion | True | 20 | 0.8861 | 0.8845 | 0.997953 | 103804.3 | 103808.6 | inconclusive | True |
| low-contrast/444/p2/d4 | narrow-zigzag | True | 20 | 0.8873 | 0.8702 | 0.978952 | 103808.6 | 103800.0 | optimized | True |
| low-contrast/420/p0/d1 | fuzzy-erosion | True | 20 | 0.4421 | 0.4399 | 0.994753 | 58448.0 | 58452.3 | inconclusive | True |
| low-contrast/420/p0/d1 | narrow-zigzag | True | 20 | 0.4488 | 0.4393 | 0.982761 | 58452.3 | 58452.3 | inconclusive | True |
| low-contrast/420/p0/d4 | fuzzy-erosion | True | 20 | 0.3870 | 0.3831 | 0.991803 | 50668.3 | 50664.0 | inconclusive | True |
| low-contrast/420/p0/d4 | narrow-zigzag | True | 20 | 0.3890 | 0.3793 | 0.978260 | 50664.0 | 50664.0 | optimized | True |
| low-contrast/420/p2/d1 | fuzzy-erosion | True | 20 | 0.7675 | 0.7628 | 0.993762 | 111328.6 | 111328.6 | inconclusive | True |
| low-contrast/420/p2/d1 | narrow-zigzag | True | 20 | 0.7630 | 0.7525 | 0.988436 | 111328.6 | 111328.6 | inconclusive | True |
| low-contrast/420/p2/d4 | fuzzy-erosion | True | 20 | 0.5808 | 0.5778 | 0.997852 | 92264.6 | 92260.3 | inconclusive | True |
| low-contrast/420/p2/d4 | narrow-zigzag | True | 20 | 0.5773 | 0.5674 | 0.978009 | 92264.6 | 92256.0 | optimized | True |
| low-noise/444/p0/d1 | fuzzy-erosion | True | 20 | 0.6570 | 0.6525 | 0.992266 | 64496.6 | 64488.0 | inconclusive | True |
| low-noise/444/p0/d1 | narrow-zigzag | True | 20 | 0.6645 | 0.6441 | 0.962873 | 64496.6 | 64488.0 | optimized | True |
| low-noise/444/p0/d4 | fuzzy-erosion | True | 20 | 0.4917 | 0.4894 | 0.992783 | 49844.3 | 49844.3 | inconclusive | True |
| low-noise/444/p0/d4 | narrow-zigzag | True | 20 | 0.4907 | 0.4842 | 0.988665 | 49840.0 | 49844.3 | inconclusive | True |
| low-noise/444/p2/d1 | fuzzy-erosion | True | 20 | 1.2095 | 1.2048 | 0.995220 | 110624.6 | 110624.6 | inconclusive | True |
| low-noise/444/p2/d1 | narrow-zigzag | True | 20 | 1.2007 | 1.1891 | 0.991783 | 110624.6 | 110624.6 | inconclusive | True |
| low-noise/444/p2/d4 | fuzzy-erosion | True | 20 | 0.7864 | 0.7829 | 0.994554 | 78796.3 | 78800.6 | inconclusive | True |
| low-noise/444/p2/d4 | narrow-zigzag | True | 20 | 0.7835 | 0.7781 | 0.990918 | 78792.0 | 78796.3 | inconclusive | True |
| low-noise/420/p0/d1 | fuzzy-erosion | True | 20 | 0.4562 | 0.4524 | 0.995132 | 54012.3 | 54012.3 | inconclusive | True |
| low-noise/420/p0/d1 | narrow-zigzag | True | 20 | 0.4522 | 0.4436 | 0.978834 | 54012.3 | 54012.3 | inconclusive | True |
| low-noise/420/p0/d4 | fuzzy-erosion | True | 20 | 0.3582 | 0.3535 | 0.983531 | 43392.0 | 43396.3 | inconclusive | False |
| low-noise/420/p0/d4 | narrow-zigzag | True | 20 | 0.3557 | 0.3498 | 0.985989 | 43396.3 | 43392.0 | optimized | True |
| low-noise/420/p2/d1 | fuzzy-erosion | True | 20 | 0.7645 | 0.7629 | 0.989956 | 101084.9 | 101072.0 | inconclusive | True |
| low-noise/420/p2/d1 | narrow-zigzag | True | 20 | 0.7730 | 0.7622 | 0.983891 | 101084.9 | 101076.3 | inconclusive | True |
| low-noise/420/p2/d4 | fuzzy-erosion | True | 20 | 0.5155 | 0.5101 | 0.977575 | 79392.6 | 79384.0 | inconclusive | True |
| low-noise/420/p2/d4 | narrow-zigzag | True | 20 | 0.5139 | 0.5069 | 0.986935 | 79392.6 | 79384.0 | inconclusive | True |
| tiny-odd-edges/444/p0/d1 | fuzzy-erosion | True | 100 | 0.0305 | 0.0304 | 0.991750 | 37010.6 | 37010.6 | inconclusive | False |
| tiny-odd-edges/444/p0/d1 | narrow-zigzag | True | 100 | 0.0305 | 0.0304 | 0.995624 | 37010.6 | 37010.6 | inconclusive | False |
| tiny-odd-edges/444/p0/d4 | fuzzy-erosion | True | 100 | 0.0303 | 0.0299 | 1.011303 | 36713.7 | 36714.6 | inconclusive | False |
| tiny-odd-edges/444/p0/d4 | narrow-zigzag | True | 100 | 0.0303 | 0.0303 | 0.998288 | 36714.6 | 36714.6 | inconclusive | False |
| tiny-odd-edges/444/p2/d1 | fuzzy-erosion | True | 100 | 0.0590 | 0.0570 | 0.982508 | 64013.2 | 64010.6 | inconclusive | True |
| tiny-odd-edges/444/p2/d1 | narrow-zigzag | True | 100 | 0.0599 | 0.0570 | 0.979807 | 64014.0 | 64010.6 | inconclusive | True |
| tiny-odd-edges/444/p2/d4 | fuzzy-erosion | True | 100 | 0.0535 | 0.0531 | 0.978976 | 63684.3 | 63682.6 | inconclusive | True |
| tiny-odd-edges/444/p2/d4 | narrow-zigzag | True | 100 | 0.0528 | 0.0527 | 0.998373 | 63681.7 | 63681.7 | inconclusive | True |
| tiny-odd-edges/420/p0/d1 | fuzzy-erosion | True | 100 | 0.0291 | 0.0288 | 0.941997 | 36819.4 | 36817.7 | inconclusive | False |
| tiny-odd-edges/420/p0/d1 | narrow-zigzag | True | 100 | 0.0292 | 0.0290 | 0.977586 | 36818.6 | 36816.9 | inconclusive | False |
| tiny-odd-edges/420/p0/d4 | fuzzy-erosion | True | 100 | 0.0290 | 0.0286 | 0.967677 | 36642.6 | 36641.7 | inconclusive | False |
| tiny-odd-edges/420/p0/d4 | narrow-zigzag | True | 100 | 0.0290 | 0.0289 | 0.992792 | 36641.7 | 36641.7 | inconclusive | False |
| tiny-odd-edges/420/p2/d1 | fuzzy-erosion | True | 100 | 0.0527 | 0.0540 | 1.007934 | 65033.7 | 65037.2 | inconclusive | True |
| tiny-odd-edges/420/p2/d1 | narrow-zigzag | True | 100 | 0.0545 | 0.0528 | 0.982209 | 65037.2 | 65034.6 | inconclusive | False |
| tiny-odd-edges/420/p2/d4 | fuzzy-erosion | True | 100 | 0.0523 | 0.0500 | 0.994360 | 64805.2 | 64803.4 | inconclusive | True |
| tiny-odd-edges/420/p2/d4 | narrow-zigzag | True | 100 | 0.0502 | 0.0534 | 1.023019 | 64802.6 | 64806.0 | inconclusive | True |

Invalid evidence:
- kodim01/444/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- kodim01/444/p2/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- low-contrast/444/p2/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- low-noise/420/p0/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/narrow-zigzag: ABBA/BAAB order bias exceeds10% or one order is absent.
- tiny-odd-edges/444/p0/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/fuzzy-erosion: ABBA/BAAB order bias exceeds10% or one order is absent.
- tiny-odd-edges/444/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/narrow-zigzag: ABBA/BAAB order bias exceeds10% or one order is absent.
- tiny-odd-edges/420/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/fuzzy-erosion: ABBA/BAAB order bias exceeds10% or one order is absent.
- tiny-odd-edges/420/p0/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p2/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
