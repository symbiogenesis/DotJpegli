# Interleaved kernel comparison

Selected managed variants, measured as alternating ABBA/BAAB quartets in one process. Each pair changes one kernel and holds the other Automatic. Automatic output is checked untimed. Original, optimized and automatic JPEG bytes match for every completed comparison.

Ratios below1 favor Optimized. Each ratio is the median of quartet geometric-mean ratios; allocations are mean bytes per encode. MAD, within-quartet repeats, p90/p10 drift and measurement-order bias guard against timing-state changes. Invalid rows retain raw samples but cannot establish a speed result. Unreachable candidates do not measure the changed kernel. No native or full-corpus parity verdict is implied.

Evidence preference requires valid samples and every paired ratio beyond the same side of1+/-0.005; mixed or small effects remain inconclusive. raw-comparisons/ retains each completed quartet before analysis, including malformed or nonfinite raw diagnostics.

| Case | Kernel | Candidate reachable | Iterations/leg | Original ms | Optimized ms | Paired ratio | Original B/encode | Optimized B/encode | Evidence preference | Valid |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| kodim01/444/p0/d1 | fuzzy-erosion | True | 20 | 7.9198 | 7.9337 | 0.999910 | 194064.4 | 193976.0 | inconclusive | True |
| kodim01/444/p0/d4 | fuzzy-erosion | True | 20 | 7.3329 | 7.3223 | 1.003192 | 108424.0 | 108440.9 | inconclusive | True |
| kodim01/444/p2/d1 | fuzzy-erosion | True | 20 | 13.0661 | 13.0083 | 0.993531 | 252825.7 | 252820.0 | inconclusive | True |
| kodim01/444/p2/d4 | fuzzy-erosion | True | 20 | 10.8859 | 10.8505 | 0.998083 | 163780.3 | 163776.0 | inconclusive | True |
| kodim01/420/p0/d1 | fuzzy-erosion | True | 20 | 5.5882 | 5.5917 | 0.992158 | 176332.3 | 176328.0 | inconclusive | False |
| kodim01/420/p0/d4 | fuzzy-erosion | True | 20 | 4.9354 | 4.9183 | 0.996338 | 97100.3 | 97096.0 | inconclusive | True |
| kodim01/420/p2/d1 | fuzzy-erosion | True | 20 | 9.0959 | 9.0473 | 0.995793 | 225890.6 | 225832.0 | inconclusive | True |
| kodim01/420/p2/d4 | fuzzy-erosion | True | 20 | 7.0695 | 7.0682 | 0.998432 | 148840.0 | 148851.3 | inconclusive | True |
| low-contrast/444/p0/d1 | fuzzy-erosion | True | 20 | 1.0311 | 1.0281 | 0.997343 | 59976.0 | 59976.0 | inconclusive | True |
| low-contrast/444/p0/d4 | fuzzy-erosion | True | 20 | 0.9694 | 0.9673 | 0.997254 | 49964.3 | 49960.0 | inconclusive | True |
| low-contrast/444/p2/d1 | fuzzy-erosion | True | 20 | 1.6501 | 1.6499 | 0.998803 | 128764.3 | 128760.0 | inconclusive | True |
| low-contrast/444/p2/d4 | fuzzy-erosion | True | 20 | 1.4398 | 1.4374 | 0.999761 | 102100.3 | 102096.0 | inconclusive | True |
| low-contrast/420/p0/d1 | fuzzy-erosion | True | 20 | 0.7349 | 0.7346 | 0.999106 | 56676.3 | 56672.0 | inconclusive | True |
| low-contrast/420/p0/d4 | fuzzy-erosion | True | 20 | 0.6855 | 0.6854 | 0.998986 | 48888.0 | 48888.0 | inconclusive | True |
| low-contrast/420/p2/d1 | fuzzy-erosion | True | 20 | 1.1236 | 1.1238 | 0.999837 | 109616.0 | 109620.3 | inconclusive | True |
| low-contrast/420/p2/d4 | fuzzy-erosion | True | 20 | 0.9456 | 0.9428 | 0.995794 | 90556.3 | 90552.0 | inconclusive | True |
| low-noise/444/p0/d1 | fuzzy-erosion | True | 20 | 1.0605 | 1.0580 | 0.997659 | 62712.0 | 62712.0 | inconclusive | True |
| low-noise/444/p0/d4 | fuzzy-erosion | True | 20 | 0.9476 | 0.9467 | 0.999176 | 48064.0 | 48068.3 | inconclusive | True |
| low-noise/444/p2/d1 | fuzzy-erosion | True | 20 | 1.6713 | 1.6692 | 0.999933 | 108912.0 | 108916.3 | inconclusive | True |
| low-noise/444/p2/d4 | fuzzy-erosion | True | 20 | 1.3625 | 1.3606 | 0.999116 | 77088.0 | 77088.0 | inconclusive | True |
| low-noise/420/p0/d1 | fuzzy-erosion | True | 20 | 0.7442 | 0.7428 | 0.997262 | 52232.0 | 52236.3 | inconclusive | True |
| low-noise/420/p0/d4 | fuzzy-erosion | True | 20 | 0.6713 | 0.6700 | 0.999496 | 41616.0 | 41616.0 | inconclusive | True |
| low-noise/420/p2/d1 | fuzzy-erosion | True | 20 | 1.1077 | 1.1067 | 1.000670 | 99368.0 | 99372.3 | inconclusive | True |
| low-noise/420/p2/d4 | fuzzy-erosion | True | 20 | 0.8883 | 0.8874 | 0.998549 | 77684.3 | 77680.0 | inconclusive | True |
| tiny-odd-edges/444/p0/d1 | fuzzy-erosion | True | 100 | 0.0366 | 0.0366 | 0.997932 | 35232.0 | 35239.0 | inconclusive | False |
| tiny-odd-edges/444/p0/d4 | fuzzy-erosion | True | 100 | 0.0374 | 0.0374 | 0.999986 | 34936.9 | 34936.9 | inconclusive | False |
| tiny-odd-edges/444/p2/d1 | fuzzy-erosion | True | 100 | 0.0756 | 0.0757 | 0.999152 | 62304.9 | 62304.9 | inconclusive | True |
| tiny-odd-edges/444/p2/d4 | fuzzy-erosion | True | 100 | 0.0710 | 0.0711 | 1.001999 | 61976.0 | 61977.7 | inconclusive | True |
| tiny-odd-edges/420/p0/d1 | fuzzy-erosion | True | 100 | 0.0340 | 0.0340 | 0.995998 | 35040.9 | 35040.9 | inconclusive | False |
| tiny-odd-edges/420/p0/d4 | fuzzy-erosion | True | 100 | 0.0350 | 0.0348 | 1.001527 | 34864.0 | 34864.9 | inconclusive | False |
| tiny-odd-edges/420/p2/d1 | fuzzy-erosion | True | 100 | 0.0682 | 0.0681 | 0.998320 | 63328.9 | 63328.0 | inconclusive | True |
| tiny-odd-edges/420/p2/d4 | fuzzy-erosion | True | 100 | 0.0657 | 0.0655 | 0.999571 | 63096.9 | 63096.9 | inconclusive | True |

32 narrow-zigzag comparisons were not applicable to this pipeline's vector width; skipped.json retains their case identities and reason.

Invalid evidence:
- kodim01/420/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
