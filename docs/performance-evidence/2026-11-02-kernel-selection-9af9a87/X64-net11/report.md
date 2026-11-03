# Interleaved kernel comparison

Selected managed variants, measured as alternating ABBA/BAAB quartets in one process. Each pair changes one kernel and holds the other Automatic. Automatic output is checked untimed. Original, optimized and automatic JPEG bytes match for every completed comparison.

Ratios below1 favor Optimized. Each ratio is the median of quartet geometric-mean ratios; allocations are mean bytes per encode. MAD, within-quartet repeats, p90/p10 drift and measurement-order bias guard against timing-state changes. Invalid rows retain raw samples but cannot establish a speed result. Unreachable candidates do not measure the changed kernel. No native or full-corpus parity verdict is implied.

Evidence preference requires valid samples and every paired ratio beyond the same side of1+/-0.005; mixed or small effects remain inconclusive. raw-comparisons/ retains each completed quartet before analysis, including malformed or nonfinite raw diagnostics.

| Case | Kernel | Candidate reachable | Iterations/leg | Original ms | Optimized ms | Paired ratio | Original B/encode | Optimized B/encode | Evidence preference | Valid |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| kodim01/444/p0/d1 | fuzzy-erosion | True | 20 | 5.5019 | 5.4364 | 0.990814 | 195752.0 | 195752.0 | inconclusive | False |
| kodim01/444/p0/d1 | narrow-zigzag | True | 20 | 5.5087 | 5.3132 | 0.965556 | 195804.9 | 195752.0 | inconclusive | False |
| kodim01/444/p0/d4 | fuzzy-erosion | True | 20 | 4.5621 | 4.4837 | 0.984905 | 110218.1 | 110200.0 | inconclusive | True |
| kodim01/444/p0/d4 | narrow-zigzag | True | 20 | 4.5999 | 4.5511 | 0.989433 | 110200.0 | 110200.0 | inconclusive | False |
| kodim01/444/p2/d1 | fuzzy-erosion | True | 20 | 10.8934 | 10.8724 | 0.996208 | 254495.9 | 254521.2 | inconclusive | True |
| kodim01/444/p2/d1 | narrow-zigzag | True | 20 | 10.8979 | 10.8110 | 0.992424 | 254496.0 | 254516.9 | inconclusive | True |
| kodim01/444/p2/d4 | fuzzy-erosion | True | 20 | 7.8495 | 7.7788 | 0.991708 | 165484.3 | 165484.3 | inconclusive | False |
| kodim01/444/p2/d4 | narrow-zigzag | True | 20 | 7.9187 | 7.8643 | 0.991995 | 165480.0 | 165480.0 | inconclusive | True |
| kodim01/420/p0/d1 | fuzzy-erosion | True | 20 | 4.2228 | 4.1814 | 0.991706 | 178104.0 | 178104.0 | inconclusive | True |
| kodim01/420/p0/d1 | narrow-zigzag | True | 20 | 4.2610 | 4.1495 | 0.974090 | 178104.0 | 178104.0 | optimized | True |
| kodim01/420/p0/d4 | fuzzy-erosion | True | 20 | 3.4417 | 3.3694 | 0.981794 | 98883.3 | 98872.0 | inconclusive | True |
| kodim01/420/p0/d4 | narrow-zigzag | True | 20 | 3.4193 | 3.3476 | 0.973840 | 98872.0 | 98872.0 | inconclusive | False |
| kodim01/420/p2/d1 | fuzzy-erosion | True | 20 | 8.4027 | 8.3464 | 0.991066 | 227540.3 | 227536.0 | inconclusive | True |
| kodim01/420/p2/d1 | narrow-zigzag | True | 20 | 8.3766 | 8.2881 | 0.989409 | 227536.0 | 227556.1 | inconclusive | True |
| kodim01/420/p2/d4 | fuzzy-erosion | True | 20 | 5.5490 | 5.5058 | 0.991657 | 150544.0 | 150548.3 | optimized | True |
| kodim01/420/p2/d4 | narrow-zigzag | True | 20 | 5.5501 | 5.4749 | 0.984815 | 150544.0 | 150548.3 | optimized | True |
| low-contrast/444/p0/d1 | fuzzy-erosion | True | 20 | 0.7536 | 0.7414 | 0.984269 | 61752.0 | 61752.0 | inconclusive | True |
| low-contrast/444/p0/d1 | narrow-zigzag | True | 20 | 0.7497 | 0.7172 | 0.961298 | 61752.0 | 61752.0 | optimized | True |
| low-contrast/444/p0/d4 | fuzzy-erosion | True | 20 | 0.6238 | 0.6037 | 0.984021 | 51736.0 | 51736.0 | inconclusive | False |
| low-contrast/444/p0/d4 | narrow-zigzag | True | 20 | 0.6132 | 0.5933 | 0.968237 | 51736.0 | 51736.0 | inconclusive | False |
| low-contrast/444/p2/d1 | fuzzy-erosion | True | 20 | 1.2928 | 1.2801 | 0.993638 | 130464.0 | 130468.3 | inconclusive | True |
| low-contrast/444/p2/d1 | narrow-zigzag | True | 20 | 1.2998 | 1.2652 | 0.974474 | 130468.3 | 130464.0 | optimized | True |
| low-contrast/444/p2/d4 | fuzzy-erosion | True | 20 | 0.9567 | 0.9495 | 0.987665 | 103804.3 | 103800.0 | inconclusive | True |
| low-contrast/444/p2/d4 | narrow-zigzag | True | 20 | 0.9660 | 0.9500 | 0.977818 | 103800.0 | 103804.3 | optimized | True |
| low-contrast/420/p0/d1 | fuzzy-erosion | True | 20 | 0.5292 | 0.5292 | 0.991981 | 58448.0 | 58448.0 | inconclusive | True |
| low-contrast/420/p0/d1 | narrow-zigzag | True | 20 | 0.5372 | 0.5215 | 0.975495 | 58448.0 | 58448.0 | optimized | True |
| low-contrast/420/p0/d4 | fuzzy-erosion | True | 20 | 0.4482 | 0.4342 | 0.972305 | 50668.3 | 50664.0 | inconclusive | False |
| low-contrast/420/p0/d4 | narrow-zigzag | True | 20 | 0.4471 | 0.4337 | 0.978847 | 50664.0 | 50664.0 | inconclusive | True |
| low-contrast/420/p2/d1 | fuzzy-erosion | True | 20 | 0.9061 | 0.9039 | 0.995195 | 111320.0 | 111324.3 | inconclusive | True |
| low-contrast/420/p2/d1 | narrow-zigzag | True | 20 | 0.9027 | 0.8984 | 0.990635 | 111320.0 | 111324.3 | inconclusive | True |
| low-contrast/420/p2/d4 | fuzzy-erosion | True | 20 | 0.6529 | 0.6375 | 0.978737 | 92256.0 | 92256.0 | inconclusive | True |
| low-contrast/420/p2/d4 | narrow-zigzag | True | 20 | 0.6486 | 0.6420 | 0.979102 | 92260.3 | 92256.0 | inconclusive | False |
| low-noise/444/p0/d1 | fuzzy-erosion | True | 20 | 0.7724 | 0.7603 | 0.990000 | 64488.0 | 64488.0 | inconclusive | True |
| low-noise/444/p0/d1 | narrow-zigzag | True | 20 | 0.7715 | 0.7563 | 0.983311 | 64488.0 | 64488.0 | inconclusive | True |
| low-noise/444/p0/d4 | fuzzy-erosion | True | 20 | 0.5604 | 0.5504 | 0.983440 | 49840.0 | 49840.0 | inconclusive | True |
| low-noise/444/p0/d4 | narrow-zigzag | True | 20 | 0.5552 | 0.5521 | 0.993960 | 49844.3 | 49840.0 | inconclusive | True |
| low-noise/444/p2/d1 | fuzzy-erosion | True | 20 | 1.3478 | 1.3357 | 0.988809 | 110620.3 | 110616.0 | inconclusive | True |
| low-noise/444/p2/d1 | narrow-zigzag | True | 20 | 1.3524 | 1.3337 | 0.983340 | 110616.0 | 110616.0 | inconclusive | True |
| low-noise/444/p2/d4 | fuzzy-erosion | True | 20 | 0.8408 | 0.8381 | 0.999089 | 78796.3 | 78792.0 | inconclusive | True |
| low-noise/444/p2/d4 | narrow-zigzag | True | 20 | 0.8488 | 0.8389 | 0.993878 | 78792.0 | 78792.0 | inconclusive | True |
| low-noise/420/p0/d1 | fuzzy-erosion | True | 20 | 0.5455 | 0.5401 | 0.992488 | 54008.0 | 54008.0 | inconclusive | True |
| low-noise/420/p0/d1 | narrow-zigzag | True | 20 | 0.5494 | 0.5324 | 0.977362 | 54008.0 | 54008.0 | optimized | True |
| low-noise/420/p0/d4 | fuzzy-erosion | True | 20 | 0.4201 | 0.4109 | 0.989636 | 43392.0 | 43392.0 | inconclusive | True |
| low-noise/420/p0/d4 | narrow-zigzag | True | 20 | 0.4155 | 0.4055 | 0.971614 | 43392.0 | 43392.0 | inconclusive | True |
| low-noise/420/p2/d1 | fuzzy-erosion | True | 20 | 0.9050 | 0.8872 | 0.978113 | 101072.0 | 101072.0 | optimized | True |
| low-noise/420/p2/d1 | narrow-zigzag | True | 20 | 0.8987 | 0.8871 | 0.991475 | 101072.0 | 101072.0 | inconclusive | True |
| low-noise/420/p2/d4 | fuzzy-erosion | True | 20 | 0.5746 | 0.5699 | 0.987722 | 79384.0 | 79384.0 | inconclusive | True |
| low-noise/420/p2/d4 | narrow-zigzag | True | 20 | 0.5808 | 0.5702 | 0.979307 | 79384.0 | 79384.0 | inconclusive | True |
| tiny-odd-edges/444/p0/d1 | fuzzy-erosion | True | 100 | 0.0386 | 0.0373 | 0.978444 | 37008.9 | 37008.0 | inconclusive | False |
| tiny-odd-edges/444/p0/d1 | narrow-zigzag | True | 100 | 0.0381 | 0.0385 | 1.013886 | 37008.9 | 37008.0 | inconclusive | False |
| tiny-odd-edges/444/p0/d4 | fuzzy-erosion | True | 100 | 0.0382 | 0.0373 | 0.975395 | 36712.9 | 36712.0 | inconclusive | True |
| tiny-odd-edges/444/p0/d4 | narrow-zigzag | True | 100 | 0.0376 | 0.0373 | 0.993963 | 36712.0 | 36712.9 | inconclusive | False |
| tiny-odd-edges/444/p2/d1 | fuzzy-erosion | True | 100 | 0.0734 | 0.0724 | 0.984196 | 64009.7 | 64008.0 | inconclusive | True |
| tiny-odd-edges/444/p2/d1 | narrow-zigzag | True | 100 | 0.0722 | 0.0720 | 1.000729 | 64008.9 | 64008.0 | inconclusive | True |
| tiny-odd-edges/444/p2/d4 | fuzzy-erosion | True | 100 | 0.0670 | 0.0667 | 0.992424 | 63680.9 | 63680.9 | inconclusive | True |
| tiny-odd-edges/444/p2/d4 | narrow-zigzag | True | 100 | 0.0679 | 0.0678 | 0.998896 | 63680.9 | 63680.9 | inconclusive | False |
| tiny-odd-edges/420/p0/d1 | fuzzy-erosion | True | 100 | 0.0359 | 0.0351 | 0.974331 | 36816.0 | 36816.0 | optimized | True |
| tiny-odd-edges/420/p0/d1 | narrow-zigzag | True | 100 | 0.0367 | 0.0362 | 0.979225 | 36816.9 | 36816.0 | inconclusive | True |
| tiny-odd-edges/420/p0/d4 | fuzzy-erosion | True | 100 | 0.0372 | 0.0362 | 0.960570 | 36640.9 | 36640.0 | inconclusive | True |
| tiny-odd-edges/420/p0/d4 | narrow-zigzag | True | 100 | 0.0366 | 0.0366 | 1.000588 | 36640.9 | 36640.0 | inconclusive | False |
| tiny-odd-edges/420/p2/d1 | fuzzy-erosion | True | 100 | 0.0681 | 0.0675 | 0.995206 | 65032.0 | 65033.7 | inconclusive | True |
| tiny-odd-edges/420/p2/d1 | narrow-zigzag | True | 100 | 0.0674 | 0.0682 | 1.008734 | 65032.9 | 65032.9 | inconclusive | True |
| tiny-odd-edges/420/p2/d4 | fuzzy-erosion | True | 100 | 0.0647 | 0.0636 | 0.974277 | 64800.9 | 64800.0 | inconclusive | True |
| tiny-odd-edges/420/p2/d4 | narrow-zigzag | True | 100 | 0.0652 | 0.0649 | 0.996995 | 64800.9 | 64800.9 | inconclusive | True |

Invalid evidence:
- kodim01/444/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- kodim01/444/p0/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- kodim01/444/p0/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- kodim01/444/p2/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- kodim01/420/p0/d4/narrow-zigzag: original: p90/p10 drift exceeds20%, including minority timing states.
- kodim01/420/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- kodim01/420/p0/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- low-contrast/444/p0/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- low-contrast/444/p0/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- low-contrast/444/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- low-contrast/420/p0/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- low-contrast/420/p2/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p2/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
