# Interleaved kernel comparison

Selected managed variants, measured as alternating ABBA/BAAB quartets in one process. Forced pairs change one kernel and hold the other Automatic. With --include-automatic, automatic-policy compares both Original against both Automatic. Exact preference maps identify each baseline and candidate. All variants produce identical JPEG bytes for every completed comparison.

Ratios below1 favor the candidate. Each ratio is the median of quartet geometric-mean ratios; allocations are mean bytes per encode. MAD, within-quartet repeats, p90/p10 drift and measurement-order bias guard against timing-state changes. Invalid rows retain raw samples but cannot establish a speed result. An automatic-policy row with candidate reachable=false selects neither new kernel and still measures the complete baseline/candidate dispatch paths. No native or full-corpus parity verdict is implied.

Evidence preference requires valid samples and every paired ratio beyond the same side of1+/-0.005; mixed or small effects remain inconclusive. raw-comparisons/ retains each completed quartet before analysis, including malformed or nonfinite raw diagnostics.

| Case | Comparison | Baseline preferences | Candidate preferences | Candidate reachable | Iterations/leg | Baseline ms | Candidate ms | Paired ratio | Baseline B/encode | Candidate B/encode | Evidence preference | Valid |
| --- | --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| intermediate-64k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.4366 | 1.4411 | 0.999673 | 125208.0 | 125242.9 | inconclusive | False |
| intermediate-64k/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.4829 | 1.4330 | 0.963087 | 125223.9 | 125208.0 | inconclusive | False |
| intermediate-64k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.4795 | 1.4320 | 0.970704 | 125208.0 | 125220.9 | automatic | True |
| intermediate-64k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0657 | 1.0529 | 0.988668 | 76264.0 | 76268.3 | inconclusive | True |
| intermediate-64k/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.1093 | 1.0654 | 0.958712 | 76272.6 | 76264.0 | optimized | True |
| intermediate-64k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.1116 | 1.0671 | 0.960994 | 76272.6 | 76264.0 | automatic | True |
| intermediate-64k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.9377 | 2.9218 | 0.994844 | 206220.9 | 206212.3 | inconclusive | True |
| intermediate-64k/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 2.9793 | 2.9343 | 0.988212 | 206212.3 | 206225.2 | optimized | True |
| intermediate-64k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 2.9933 | 2.9324 | 0.980765 | 206225.2 | 206208.0 | automatic | True |
| intermediate-64k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.9734 | 1.9535 | 0.994454 | 156341.2 | 156308.3 | inconclusive | True |
| intermediate-64k/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 2.0078 | 1.9608 | 0.979311 | 156304.0 | 156316.9 | optimized | True |
| intermediate-64k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 2.0056 | 1.9623 | 0.977849 | 156308.3 | 156308.3 | automatic | True |
| intermediate-64k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9378 | 0.9238 | 0.986279 | 87836.3 | 87836.3 | inconclusive | True |
| intermediate-64k/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.9607 | 0.9345 | 0.971388 | 87840.6 | 87832.0 | optimized | True |
| intermediate-64k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.9592 | 0.9339 | 0.973412 | 87840.6 | 87832.0 | automatic | True |
| intermediate-64k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7030 | 0.6904 | 0.980876 | 62072.0 | 62072.0 | optimized | True |
| intermediate-64k/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.7267 | 0.7029 | 0.966665 | 62072.0 | 62072.0 | optimized | True |
| intermediate-64k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.7258 | 0.7042 | 0.970640 | 62072.0 | 62076.3 | automatic | True |
| intermediate-64k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.8288 | 1.8224 | 0.990833 | 164408.6 | 164408.6 | inconclusive | True |
| intermediate-64k/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.8561 | 1.8261 | 0.984608 | 164408.6 | 164408.6 | inconclusive | True |
| intermediate-64k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.8527 | 1.8300 | 0.986052 | 164404.3 | 164404.3 | automatic | True |
| intermediate-64k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.2177 | 1.2062 | 0.989509 | 132152.0 | 132160.6 | inconclusive | True |
| intermediate-64k/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.2386 | 1.2159 | 0.981689 | 132152.0 | 132167.7 | inconclusive | True |
| intermediate-64k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.2394 | 1.2176 | 0.981149 | 132156.3 | 132160.6 | automatic | True |
| intermediate-128k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.8655 | 2.8477 | 0.990146 | 202945.8 | 202908.3 | inconclusive | True |
| intermediate-128k/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 2.9568 | 2.8501 | 0.964362 | 202951.6 | 202945.4 | optimized | True |
| intermediate-128k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 2.9364 | 2.8050 | 0.962910 | 202974.8 | 202904.0 | automatic | True |
| intermediate-128k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.0706 | 2.0583 | 0.993825 | 110596.3 | 110592.0 | inconclusive | True |
| intermediate-128k/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 2.1606 | 2.0743 | 0.959468 | 110592.0 | 110592.0 | optimized | True |
| intermediate-128k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 2.1639 | 2.0756 | 0.960014 | 110596.3 | 110596.3 | inconclusive | False |
| intermediate-128k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 5.5758 | 5.5543 | 0.994856 | 289440.2 | 289446.4 | inconclusive | False |
| intermediate-128k/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 5.6559 | 5.5673 | 0.982868 | 289450.7 | 289431.6 | inconclusive | True |
| intermediate-128k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 5.6551 | 5.5725 | 0.985576 | 289412.3 | 289435.8 | automatic | True |
| intermediate-128k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 3.7239 | 3.7035 | 0.995738 | 189652.9 | 189640.0 | inconclusive | True |
| intermediate-128k/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 3.8066 | 3.7199 | 0.977078 | 189640.0 | 189657.2 | optimized | True |
| intermediate-128k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 3.8182 | 3.7189 | 0.974126 | 189648.6 | 189644.3 | automatic | True |
| intermediate-128k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.8062 | 1.7827 | 0.989481 | 133772.3 | 133813.3 | inconclusive | True |
| intermediate-128k/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.8662 | 1.8140 | 0.970764 | 133809.8 | 133768.0 | optimized | True |
| intermediate-128k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.8646 | 1.8148 | 0.971966 | 133805.5 | 133768.0 | automatic | True |
| intermediate-128k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.3560 | 1.3327 | 0.983521 | 82244.3 | 82240.0 | inconclusive | True |
| intermediate-128k/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.3946 | 1.3514 | 0.973534 | 82240.0 | 82244.3 | inconclusive | False |
| intermediate-128k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.3931 | 1.3432 | 0.962030 | 82244.3 | 82244.3 | automatic | True |
| intermediate-128k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 3.4165 | 3.4040 | 0.991759 | 221945.8 | 221896.6 | inconclusive | True |
| intermediate-128k/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 3.4778 | 3.4206 | 0.983132 | 221926.4 | 221935.4 | inconclusive | True |
| intermediate-128k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 3.4863 | 3.4173 | 0.980413 | 221933.5 | 221892.3 | automatic | True |
| intermediate-128k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.2879 | 2.2654 | 0.991046 | 163300.3 | 163300.3 | inconclusive | False |
| intermediate-128k/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 2.3242 | 2.2769 | 0.978918 | 163304.6 | 163304.6 | optimized | True |
| intermediate-128k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 2.3245 | 2.2791 | 0.981097 | 163296.0 | 163304.6 | automatic | True |

Invalid evidence:
- intermediate-64k/444/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- intermediate-64k/444/p0/d1/fuzzy-erosion: optimized: p90/p10 drift exceeds20%, including minority timing states.
- intermediate-64k/444/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- intermediate-64k/444/p0/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- intermediate-128k/444/p0/d4/automatic-policy: original: repeated legs within a quartet differ by over10%.
- intermediate-128k/444/p0/d4/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- intermediate-128k/444/p2/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- intermediate-128k/420/p0/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- intermediate-128k/420/p2/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
