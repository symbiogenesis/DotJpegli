# Interleaved kernel comparison

Selected managed variants, measured as alternating ABBA/BAAB quartets in one process. Each pair changes one kernel and holds the other Automatic. Automatic output is checked untimed. Original, optimized and automatic JPEG bytes match for every completed comparison.

Ratios below1 favor Optimized. Each ratio is the median of quartet geometric-mean ratios; allocations are mean bytes per encode. MAD, within-quartet repeats, p90/p10 drift and measurement-order bias guard against timing-state changes. Invalid rows retain raw samples but cannot establish a speed result. Unreachable candidates do not measure the changed kernel. No native or full-corpus parity verdict is implied.

Evidence preference requires valid samples and every paired ratio beyond the same side of1+/-0.005; mixed or small effects remain inconclusive. raw-comparisons/ retains each completed quartet before analysis, including malformed or nonfinite raw diagnostics.

| Case | Kernel | Candidate reachable | Iterations/leg | Original ms | Optimized ms | Paired ratio | Original B/encode | Optimized B/encode | Evidence preference | Valid |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| kodim01/444/p0/d1 | fuzzy-erosion | True | 20 | 8.0129 | 8.0025 | 0.998698 | 194020.2 | 193976.0 | inconclusive | True |
| kodim01/444/p0/d4 | fuzzy-erosion | True | 20 | 7.3870 | 7.3767 | 0.999298 | 108424.0 | 108440.9 | inconclusive | True |
| kodim01/444/p2/d1 | fuzzy-erosion | True | 20 | 13.0800 | 13.0059 | 0.994997 | 252853.6 | 252792.0 | inconclusive | True |
| kodim01/444/p2/d4 | fuzzy-erosion | True | 20 | 10.8757 | 10.8568 | 0.997179 | 163780.3 | 163776.0 | inconclusive | True |
| kodim01/420/p0/d1 | fuzzy-erosion | True | 20 | 5.6869 | 5.6633 | 0.995416 | 176380.7 | 176328.0 | inconclusive | False |
| kodim01/420/p0/d4 | fuzzy-erosion | True | 20 | 5.0767 | 5.0604 | 0.997102 | 97100.3 | 97096.0 | inconclusive | True |
| kodim01/420/p2/d1 | fuzzy-erosion | True | 20 | 9.1555 | 9.1655 | 1.000266 | 225836.3 | 225859.2 | inconclusive | True |
| kodim01/420/p2/d4 | fuzzy-erosion | True | 20 | 7.1286 | 7.1368 | 0.999194 | 148840.0 | 148844.3 | inconclusive | True |
| low-contrast/444/p0/d1 | fuzzy-erosion | True | 20 | 1.0922 | 1.0913 | 0.998289 | 59976.0 | 59976.0 | inconclusive | True |
| low-contrast/444/p0/d4 | fuzzy-erosion | True | 20 | 1.0220 | 1.0218 | 0.997604 | 49971.3 | 49960.0 | inconclusive | False |
| low-contrast/444/p2/d1 | fuzzy-erosion | True | 20 | 1.7006 | 1.6964 | 1.000170 | 128764.3 | 128760.0 | inconclusive | True |
| low-contrast/444/p2/d4 | fuzzy-erosion | True | 20 | 1.4846 | 1.4798 | 0.997466 | 102100.3 | 102096.0 | inconclusive | True |
| low-contrast/420/p0/d1 | fuzzy-erosion | True | 20 | 0.7716 | 0.7703 | 0.998098 | 56676.3 | 56672.0 | inconclusive | True |
| low-contrast/420/p0/d4 | fuzzy-erosion | True | 20 | 0.7191 | 0.7185 | 0.999298 | 48888.0 | 48888.0 | inconclusive | True |
| low-contrast/420/p2/d1 | fuzzy-erosion | True | 20 | 1.1539 | 1.1527 | 1.000105 | 109616.0 | 109620.3 | inconclusive | True |
| low-contrast/420/p2/d4 | fuzzy-erosion | True | 20 | 0.9705 | 0.9676 | 0.999225 | 90556.3 | 90552.0 | inconclusive | True |
| low-noise/444/p0/d1 | fuzzy-erosion | True | 20 | 1.1162 | 1.1182 | 1.000709 | 62712.0 | 62712.0 | inconclusive | True |
| low-noise/444/p0/d4 | fuzzy-erosion | True | 20 | 0.9941 | 0.9942 | 0.999358 | 48064.0 | 48068.3 | inconclusive | True |
| low-noise/444/p2/d1 | fuzzy-erosion | True | 20 | 1.7206 | 1.7205 | 0.999217 | 108912.0 | 108916.3 | inconclusive | True |
| low-noise/444/p2/d4 | fuzzy-erosion | True | 20 | 1.4042 | 1.4025 | 0.999089 | 77088.0 | 77088.0 | inconclusive | True |
| low-noise/420/p0/d1 | fuzzy-erosion | True | 20 | 0.7808 | 0.7808 | 0.999763 | 52232.0 | 52236.3 | inconclusive | True |
| low-noise/420/p0/d4 | fuzzy-erosion | True | 20 | 0.6985 | 0.6970 | 0.999833 | 41616.0 | 41616.0 | inconclusive | True |
| low-noise/420/p2/d1 | fuzzy-erosion | True | 20 | 1.1417 | 1.1407 | 1.000278 | 99368.0 | 99372.3 | inconclusive | True |
| low-noise/420/p2/d4 | fuzzy-erosion | True | 20 | 0.9152 | 0.9135 | 0.999260 | 77684.3 | 77680.0 | inconclusive | True |
| tiny-odd-edges/444/p0/d1 | fuzzy-erosion | True | 100 | 0.0377 | 0.0377 | 0.998515 | 35232.0 | 35232.9 | inconclusive | False |
| tiny-odd-edges/444/p0/d4 | fuzzy-erosion | True | 100 | 0.0381 | 0.0381 | 0.999679 | 34940.6 | 34936.9 | inconclusive | False |
| tiny-odd-edges/444/p2/d1 | fuzzy-erosion | True | 100 | 0.0773 | 0.0774 | 1.003595 | 62304.9 | 62304.9 | inconclusive | True |
| tiny-odd-edges/444/p2/d4 | fuzzy-erosion | True | 100 | 0.0716 | 0.0717 | 1.005523 | 61976.0 | 61977.7 | inconclusive | True |
| tiny-odd-edges/420/p0/d1 | fuzzy-erosion | True | 100 | 0.0351 | 0.0349 | 0.997499 | 35040.9 | 35040.9 | inconclusive | False |
| tiny-odd-edges/420/p0/d4 | fuzzy-erosion | True | 100 | 0.0359 | 0.0357 | 0.999606 | 34864.0 | 34864.9 | inconclusive | False |
| tiny-odd-edges/420/p2/d1 | fuzzy-erosion | True | 100 | 0.0693 | 0.0690 | 0.997512 | 63328.9 | 63328.0 | inconclusive | True |
| tiny-odd-edges/420/p2/d4 | fuzzy-erosion | True | 100 | 0.0666 | 0.0665 | 1.002798 | 63096.9 | 63096.9 | inconclusive | True |

32 narrow-zigzag comparisons were not applicable to this pipeline's vector width; skipped.json retains their case identities and reason.

Invalid evidence:
- kodim01/420/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- low-contrast/444/p0/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
