# Interleaved kernel comparison

Selected managed variants, measured as alternating ABBA/BAAB quartets in one process. Forced pairs change one kernel and hold the other Automatic. With --include-automatic, automatic-policy compares both Original against both Automatic. Exact preference maps identify each baseline and candidate. All variants produce identical JPEG bytes for every completed comparison.

Ratios below1 favor the candidate. Each ratio is the median of quartet geometric-mean ratios; allocations are mean bytes per encode. MAD, within-quartet repeats, p90/p10 drift and measurement-order bias guard against timing-state changes. Invalid rows retain raw samples but cannot establish a speed result. An automatic-policy row with candidate reachable=false selects neither new kernel and still measures the complete baseline/candidate dispatch paths. No native or full-corpus parity verdict is implied.

Evidence preference requires valid samples and every paired ratio beyond the same side of1+/-0.005; mixed or small effects remain inconclusive. raw-comparisons/ retains each completed quartet before analysis, including malformed or nonfinite raw diagnostics.

| Case | Comparison | Baseline preferences | Candidate preferences | Candidate reachable | Iterations/leg | Baseline ms | Candidate ms | Paired ratio | Baseline B/encode | Candidate B/encode | Evidence preference | Valid |
| --- | --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| intermediate-64k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.8565 | 0.8572 | 0.998057 | 125208.0 | 125216.6 | inconclusive | False |
| intermediate-64k/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.8991 | 0.8489 | 0.937977 | 125232.5 | 125208.0 | optimized | True |
| intermediate-64k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.8949 | 0.8552 | 0.956879 | 125208.0 | 125225.2 | automatic | True |
| intermediate-64k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.6223 | 0.6149 | 0.995718 | 76268.3 | 76268.3 | inconclusive | True |
| intermediate-64k/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.6776 | 0.6274 | 0.926053 | 76272.6 | 76268.3 | optimized | True |
| intermediate-64k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.6646 | 0.6206 | 0.932038 | 76268.3 | 76268.3 | automatic | True |
| intermediate-64k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.7467 | 1.7344 | 0.992829 | 206233.8 | 206212.3 | inconclusive | True |
| intermediate-64k/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.8088 | 1.7864 | 0.976126 | 206212.3 | 206225.2 | inconclusive | False |
| intermediate-64k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.8044 | 1.7490 | 0.973664 | 206225.2 | 206216.6 | automatic | True |
| intermediate-64k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.1040 | 1.1031 | 0.991706 | 156312.6 | 156312.6 | inconclusive | True |
| intermediate-64k/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.1439 | 1.0952 | 0.959266 | 156312.6 | 156312.6 | optimized | True |
| intermediate-64k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.1468 | 1.1062 | 0.957728 | 156316.9 | 156312.6 | automatic | True |
| intermediate-64k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5381 | 0.5282 | 0.980600 | 87844.9 | 87832.0 | optimized | True |
| intermediate-64k/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5718 | 0.5500 | 0.952003 | 87836.3 | 87836.3 | inconclusive | False |
| intermediate-64k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.5671 | 0.5436 | 0.951064 | 87840.6 | 87832.0 | automatic | True |
| intermediate-64k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4067 | 0.4023 | 0.988662 | 62072.0 | 62076.3 | inconclusive | True |
| intermediate-64k/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.4212 | 0.4002 | 0.949961 | 62076.3 | 62076.3 | inconclusive | False |
| intermediate-64k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.4282 | 0.3994 | 0.941462 | 62080.6 | 62072.0 | automatic | True |
| intermediate-64k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0524 | 1.0511 | 0.989475 | 164412.9 | 164412.9 | inconclusive | False |
| intermediate-64k/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.0977 | 1.0809 | 0.986017 | 164408.6 | 164404.3 | inconclusive | True |
| intermediate-64k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.0897 | 1.0758 | 0.976727 | 164408.6 | 164412.9 | inconclusive | True |
| intermediate-64k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.6529 | 0.6487 | 0.982244 | 132160.6 | 132160.6 | inconclusive | True |
| intermediate-64k/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.6774 | 0.6552 | 0.970335 | 132160.6 | 132171.9 | inconclusive | False |
| intermediate-64k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.6890 | 0.6688 | 0.972908 | 132156.3 | 132169.2 | automatic | True |
| intermediate-128k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.6933 | 1.7117 | 1.005344 | 202929.0 | 202949.1 | inconclusive | True |
| intermediate-128k/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.8193 | 1.7111 | 0.945020 | 202934.2 | 202908.3 | inconclusive | False |
| intermediate-128k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.7751 | 1.7067 | 0.950097 | 202944.7 | 202953.3 | inconclusive | False |
| intermediate-128k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.2158 | 1.2098 | 0.997076 | 110600.6 | 110600.6 | inconclusive | True |
| intermediate-128k/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.3406 | 1.2285 | 0.926266 | 110600.6 | 110600.6 | optimized | True |
| intermediate-128k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.3187 | 1.2276 | 0.933220 | 110600.6 | 110600.6 | automatic | True |
| intermediate-128k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 3.3006 | 3.3322 | 1.000308 | 289408.0 | 289451.1 | inconclusive | True |
| intermediate-128k/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 3.4633 | 3.3353 | 0.967200 | 289463.9 | 289412.3 | inconclusive | False |
| intermediate-128k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 3.3943 | 3.2941 | 0.971071 | 289510.6 | 289416.6 | automatic | True |
| intermediate-128k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.1027 | 2.1237 | 1.004302 | 189652.9 | 189652.9 | inconclusive | True |
| intermediate-128k/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 2.1697 | 2.0943 | 0.964376 | 189657.2 | 189644.3 | inconclusive | False |
| intermediate-128k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 2.1825 | 2.0991 | 0.961702 | 189648.6 | 189648.6 | automatic | True |
| intermediate-128k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0679 | 1.0656 | 1.000777 | 133768.0 | 133807.2 | inconclusive | False |
| intermediate-128k/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.1018 | 1.0559 | 0.961866 | 133772.3 | 133768.0 | optimized | True |
| intermediate-128k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.0945 | 1.0279 | 0.950234 | 133843.9 | 133768.0 | inconclusive | False |
| intermediate-128k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7575 | 0.7364 | 0.979039 | 82252.9 | 82240.0 | optimized | True |
| intermediate-128k/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.7915 | 0.7444 | 0.943681 | 82240.0 | 82248.6 | inconclusive | False |
| intermediate-128k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.7926 | 0.7495 | 0.945624 | 82240.0 | 82240.0 | automatic | True |
| intermediate-128k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.9537 | 1.9490 | 0.995329 | 221916.0 | 221900.9 | inconclusive | True |
| intermediate-128k/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 2.0045 | 1.9592 | 0.987122 | 221896.6 | 221932.4 | inconclusive | False |
| intermediate-128k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 2.0440 | 1.9924 | 0.969556 | 221924.6 | 221939.1 | inconclusive | False |
| intermediate-128k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.2717 | 1.2546 | 0.997166 | 163304.6 | 163304.6 | inconclusive | True |
| intermediate-128k/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.3366 | 1.2926 | 0.970411 | 163308.9 | 163304.6 | optimized | True |
| intermediate-128k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.3361 | 1.2888 | 0.967680 | 163304.6 | 163304.6 | automatic | True |

Invalid evidence:
- intermediate-64k/444/p0/d1/fuzzy-erosion: original: p90/p10 drift exceeds20%, including minority timing states.
- intermediate-64k/444/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- intermediate-64k/444/p0/d1/fuzzy-erosion: optimized: p90/p10 drift exceeds20%, including minority timing states.
- intermediate-64k/444/p2/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- intermediate-64k/444/p2/d1/narrow-zigzag: optimized: p90/p10 drift exceeds20%, including minority timing states.
- intermediate-64k/444/p2/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- intermediate-64k/420/p0/d1/narrow-zigzag: original: p90/p10 drift exceeds20%, including minority timing states.
- intermediate-64k/420/p0/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- intermediate-64k/420/p0/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- intermediate-64k/420/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- intermediate-64k/420/p0/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- intermediate-64k/420/p2/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- intermediate-64k/420/p2/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- intermediate-64k/420/p2/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- intermediate-128k/444/p0/d1/narrow-zigzag: optimized: p90/p10 drift exceeds20%, including minority timing states.
- intermediate-128k/444/p0/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- intermediate-128k/444/p0/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- intermediate-128k/444/p2/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- intermediate-128k/444/p2/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- intermediate-128k/420/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- intermediate-128k/420/p0/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- intermediate-128k/420/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- intermediate-128k/420/p0/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- intermediate-128k/420/p2/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- intermediate-128k/420/p2/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
