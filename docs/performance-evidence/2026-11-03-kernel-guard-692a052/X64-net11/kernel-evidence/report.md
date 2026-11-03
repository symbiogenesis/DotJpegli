# Interleaved kernel comparison

Selected managed variants, measured as alternating ABBA/BAAB quartets in one process. Forced pairs change one kernel and hold the other Automatic. With --include-automatic, automatic-policy compares both Original against both Automatic. Exact preference maps identify each baseline and candidate. All variants produce identical JPEG bytes for every completed comparison.

Ratios below1 favor the candidate. Each ratio is the median of quartet geometric-mean ratios; allocations are mean bytes per encode. MAD, within-quartet repeats, p90/p10 drift and measurement-order bias guard against timing-state changes. Invalid rows retain raw samples but cannot establish a speed result. An automatic-policy row with candidate reachable=false selects neither new kernel and still measures the complete baseline/candidate dispatch paths. No native or full-corpus parity verdict is implied.

Evidence preference requires valid samples and every paired ratio beyond the same side of1+/-0.005; mixed or small effects remain inconclusive. raw-comparisons/ retains each completed quartet before analysis, including malformed or nonfinite raw diagnostics.

| Case | Comparison | Baseline preferences | Candidate preferences | Candidate reachable | Iterations/leg | Baseline ms | Candidate ms | Paired ratio | Baseline B/encode | Candidate B/encode | Evidence preference | Valid |
| --- | --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| kodim01/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 5.6346 | 5.5525 | 0.985795 | 195575.3 | 195599.0 | inconclusive | True |
| kodim01/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 5.7247 | 5.6011 | 0.977091 | 195560.0 | 195587.3 | optimized | True |
| kodim01/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 5.7260 | 5.5883 | 0.980221 | 195560.0 | 195564.3 | automatic | True |
| kodim01/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 4.6508 | 4.5980 | 0.987851 | 110012.3 | 110016.6 | inconclusive | True |
| kodim01/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 4.7344 | 4.6264 | 0.976687 | 110012.3 | 110012.3 | optimized | True |
| kodim01/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 4.7310 | 4.6384 | 0.980886 | 110016.6 | 110012.3 | automatic | True |
| kodim01/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 11.2518 | 11.2184 | 0.997914 | 253984.6 | 254001.9 | inconclusive | True |
| kodim01/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 11.5056 | 11.3830 | 0.983747 | 253980.3 | 253980.3 | inconclusive | False |
| kodim01/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 11.4517 | 11.2597 | 0.983834 | 254023.5 | 253984.6 | automatic | True |
| kodim01/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 8.2610 | 8.2503 | 0.997678 | 164968.6 | 164964.3 | inconclusive | False |
| kodim01/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 8.3613 | 8.2926 | 0.990961 | 164968.6 | 164968.6 | inconclusive | True |
| kodim01/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 8.3917 | 8.2856 | 0.987508 | 164971.4 | 164968.6 | automatic | True |
| kodim01/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 4.3460 | 4.2969 | 0.986220 | 177916.3 | 177912.0 | inconclusive | True |
| kodim01/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 4.4626 | 4.3377 | 0.975292 | 177916.3 | 177953.7 | inconclusive | True |
| kodim01/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 4.3882 | 4.3079 | 0.975671 | 177912.0 | 177970.9 | inconclusive | True |
| kodim01/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 3.4586 | 3.4135 | 0.987773 | 98684.3 | 98684.3 | optimized | True |
| kodim01/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 3.5602 | 3.4596 | 0.969998 | 98684.3 | 98684.3 | optimized | True |
| kodim01/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 3.5272 | 3.4282 | 0.974006 | 98684.3 | 98684.3 | automatic | True |
| kodim01/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 8.4062 | 8.3692 | 0.996506 | 226944.6 | 226944.6 | inconclusive | True |
| kodim01/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 8.5600 | 8.4510 | 0.988061 | 226936.0 | 226965.5 | optimized | True |
| kodim01/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 8.5655 | 8.4302 | 0.985490 | 226940.3 | 226983.3 | inconclusive | True |
| kodim01/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 5.6466 | 5.5866 | 0.989674 | 149948.3 | 149948.3 | optimized | True |
| kodim01/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 5.7289 | 5.6338 | 0.982838 | 149952.6 | 149948.3 | inconclusive | False |
| kodim01/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 5.7466 | 5.6247 | 0.981292 | 149952.6 | 149944.0 | automatic | True |
| low-contrast/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7706 | 0.7638 | 0.989668 | 61568.6 | 61560.0 | optimized | True |
| low-contrast/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.8014 | 0.7704 | 0.960904 | 61560.0 | 61560.0 | optimized | True |
| low-contrast/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.8032 | 0.7716 | 0.961901 | 61560.0 | 61568.6 | automatic | True |
| low-contrast/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.6374 | 0.6315 | 0.990806 | 51544.0 | 51548.3 | inconclusive | True |
| low-contrast/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.6585 | 0.6375 | 0.966462 | 51548.3 | 51544.0 | optimized | True |
| low-contrast/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.6585 | 0.6365 | 0.966554 | 51548.3 | 51548.3 | automatic | True |
| low-contrast/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.3577 | 1.3468 | 0.989271 | 129956.9 | 129944.0 | inconclusive | True |
| low-contrast/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.3869 | 1.3550 | 0.977562 | 129944.0 | 129952.6 | optimized | True |
| low-contrast/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.3879 | 1.3581 | 0.982221 | 129944.0 | 129952.6 | automatic | True |
| low-contrast/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0448 | 1.0358 | 0.991499 | 103288.6 | 103284.3 | inconclusive | True |
| low-contrast/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.0659 | 1.0442 | 0.980814 | 103284.3 | 103284.3 | optimized | True |
| low-contrast/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.0668 | 1.0466 | 0.980659 | 103284.3 | 103284.3 | inconclusive | True |
| low-contrast/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5475 | 0.5405 | 0.985211 | 58260.3 | 58260.3 | inconclusive | True |
| low-contrast/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5647 | 0.5479 | 0.971393 | 58260.3 | 58256.0 | optimized | True |
| low-contrast/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.5618 | 0.5447 | 0.968858 | 58260.3 | 58260.3 | automatic | True |
| low-contrast/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4544 | 0.4479 | 0.982827 | 50476.3 | 50472.0 | optimized | True |
| low-contrast/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.4705 | 0.4559 | 0.967800 | 50476.3 | 50472.0 | optimized | True |
| low-contrast/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.4707 | 0.4552 | 0.967927 | 50476.3 | 50472.0 | automatic | True |
| low-contrast/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9402 | 0.9349 | 0.995605 | 110724.3 | 110728.6 | inconclusive | True |
| low-contrast/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.9562 | 0.9393 | 0.987946 | 110724.3 | 110728.6 | inconclusive | True |
| low-contrast/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.9534 | 0.9387 | 0.981508 | 110724.3 | 110724.3 | automatic | True |
| low-contrast/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.6782 | 0.6697 | 0.982640 | 91668.9 | 91656.0 | optimized | True |
| low-contrast/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.6928 | 0.6810 | 0.983153 | 91660.3 | 91660.3 | inconclusive | True |
| low-contrast/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.6961 | 0.6805 | 0.975656 | 91660.3 | 91656.0 | automatic | True |
| low-noise/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.8051 | 0.7995 | 0.993926 | 64300.3 | 64300.3 | inconclusive | True |
| low-noise/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.8323 | 0.8051 | 0.962718 | 64300.3 | 64296.0 | optimized | True |
| low-noise/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.8307 | 0.8068 | 0.970489 | 64304.6 | 64296.0 | automatic | True |
| low-noise/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5812 | 0.5759 | 0.991594 | 49652.3 | 49648.0 | inconclusive | True |
| low-noise/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5931 | 0.5819 | 0.980865 | 49652.3 | 49648.0 | optimized | True |
| low-noise/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.5924 | 0.5820 | 0.979771 | 49652.3 | 49648.0 | automatic | True |
| low-noise/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.4241 | 1.4178 | 0.996237 | 110100.3 | 110104.6 | inconclusive | True |
| low-noise/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.4480 | 1.4267 | 0.982918 | 110100.3 | 110104.6 | optimized | True |
| low-noise/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.4478 | 1.4249 | 0.984547 | 110100.3 | 110096.0 | automatic | True |
| low-noise/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9276 | 0.9238 | 0.993699 | 78276.3 | 78272.0 | inconclusive | True |
| low-noise/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.9419 | 0.9302 | 0.987122 | 78276.3 | 78276.3 | inconclusive | True |
| low-noise/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.9397 | 0.9298 | 0.989774 | 78276.3 | 78276.3 | inconclusive | True |
| low-noise/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5667 | 0.5577 | 0.987155 | 53816.0 | 53820.3 | optimized | True |
| low-noise/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5829 | 0.5650 | 0.967666 | 53816.0 | 53816.0 | optimized | True |
| low-noise/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.5812 | 0.5664 | 0.972621 | 53816.0 | 53820.3 | inconclusive | True |
| low-noise/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4237 | 0.4150 | 0.977857 | 43200.0 | 43200.0 | optimized | True |
| low-noise/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.4300 | 0.4235 | 0.985098 | 43200.0 | 43200.0 | optimized | True |
| low-noise/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.4305 | 0.4221 | 0.980895 | 43200.0 | 43200.0 | automatic | True |
| low-noise/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9341 | 0.9235 | 0.985161 | 100480.6 | 100472.0 | optimized | True |
| low-noise/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.9504 | 0.9320 | 0.980721 | 100525.7 | 100476.3 | inconclusive | False |
| low-noise/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.9476 | 0.9311 | 0.982588 | 100472.0 | 100472.0 | automatic | True |
| low-noise/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.6138 | 0.6069 | 0.985441 | 78792.6 | 78784.0 | optimized | True |
| low-noise/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.6192 | 0.6143 | 0.993238 | 78788.3 | 78788.3 | inconclusive | True |
| low-noise/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.6228 | 0.6117 | 0.974878 | 78792.6 | 78784.0 | inconclusive | False |
| tiny-odd-edges/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0393 | 0.0392 | 1.019819 | 36816.0 | 36818.6 | inconclusive | True |
| tiny-odd-edges/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0394 | 0.0392 | 1.007668 | 36816.9 | 36817.7 | inconclusive | False |
| tiny-odd-edges/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0393 | 0.0393 | 0.980152 | 36818.6 | 36816.9 | inconclusive | False |
| tiny-odd-edges/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0391 | 0.0393 | 0.991240 | 36521.7 | 36521.7 | inconclusive | False |
| tiny-odd-edges/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0391 | 0.0389 | 0.996165 | 36520.9 | 36521.7 | inconclusive | False |
| tiny-odd-edges/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0392 | 0.0392 | 0.998614 | 36521.7 | 36520.9 | inconclusive | False |
| tiny-odd-edges/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0744 | 0.0740 | 1.023933 | 63489.7 | 63495.3 | inconclusive | False |
| tiny-odd-edges/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0753 | 0.0745 | 0.965254 | 63490.6 | 63491.4 | inconclusive | False |
| tiny-odd-edges/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0745 | 0.0747 | 0.995864 | 63490.6 | 63490.6 | inconclusive | True |
| tiny-odd-edges/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0691 | 0.0687 | 0.993746 | 63162.6 | 63162.6 | inconclusive | True |
| tiny-odd-edges/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0696 | 0.0694 | 1.003818 | 63163.4 | 63162.6 | inconclusive | True |
| tiny-odd-edges/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0693 | 0.0693 | 1.014541 | 63161.7 | 63162.6 | inconclusive | True |
| tiny-odd-edges/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0376 | 0.0371 | 0.965644 | 36626.6 | 36624.9 | inconclusive | False |
| tiny-odd-edges/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0377 | 0.0374 | 0.986716 | 36625.7 | 36624.9 | inconclusive | False |
| tiny-odd-edges/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0375 | 0.0376 | 1.001855 | 36624.9 | 36624.9 | inconclusive | False |
| tiny-odd-edges/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0376 | 0.0370 | 1.006655 | 36448.9 | 36450.6 | inconclusive | False |
| tiny-odd-edges/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0376 | 0.0375 | 0.992790 | 36449.7 | 36449.7 | inconclusive | False |
| tiny-odd-edges/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0377 | 0.0377 | 1.003609 | 36449.7 | 36448.9 | inconclusive | False |
| tiny-odd-edges/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0709 | 0.0684 | 0.960552 | 64435.4 | 64433.7 | inconclusive | True |
| tiny-odd-edges/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0688 | 0.0692 | 0.998451 | 64434.6 | 64434.6 | inconclusive | True |
| tiny-odd-edges/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0688 | 0.0692 | 1.002330 | 64434.6 | 64434.6 | inconclusive | False |
| tiny-odd-edges/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0663 | 0.0655 | 0.989190 | 64202.6 | 64201.7 | inconclusive | True |
| tiny-odd-edges/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0659 | 0.0661 | 0.998404 | 64201.7 | 64203.4 | inconclusive | False |
| tiny-odd-edges/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0657 | 0.0657 | 1.030156 | 64200.9 | 64203.4 | inconclusive | True |
| guard-below-32k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7788 | 0.7720 | 0.990667 | 80812.3 | 80812.3 | inconclusive | True |
| guard-below-32k/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.7788 | 0.7533 | 0.965909 | 80812.3 | 80812.3 | optimized | True |
| guard-below-32k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.7782 | 0.7772 | 0.996711 | 80812.3 | 80812.3 | inconclusive | True |
| guard-below-32k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5832 | 0.5773 | 0.990867 | 59112.0 | 59112.0 | inconclusive | True |
| guard-below-32k/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5834 | 0.5641 | 0.965375 | 59112.0 | 59120.6 | inconclusive | True |
| guard-below-32k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.5839 | 0.5833 | 0.998517 | 59112.0 | 59161.8 | inconclusive | False |
| guard-below-32k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.6402 | 1.6308 | 0.999976 | 156108.3 | 156112.6 | inconclusive | True |
| guard-below-32k/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.6367 | 1.6222 | 0.991068 | 156108.3 | 156112.6 | inconclusive | True |
| guard-below-32k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.6406 | 1.6378 | 0.999966 | 156108.3 | 156112.6 | inconclusive | True |
| guard-below-32k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0730 | 1.0673 | 0.994970 | 122064.0 | 122068.3 | inconclusive | True |
| guard-below-32k/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.0772 | 1.0531 | 0.975705 | 122076.9 | 122064.0 | optimized | True |
| guard-below-32k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.0717 | 1.0731 | 1.003508 | 122072.6 | 122064.0 | inconclusive | True |
| guard-below-32k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5097 | 0.5045 | 0.994160 | 64912.0 | 64920.6 | inconclusive | True |
| guard-below-32k/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5118 | 0.4989 | 0.972434 | 64916.3 | 64912.0 | optimized | True |
| guard-below-32k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.5112 | 0.5108 | 0.997051 | 64920.6 | 64912.0 | inconclusive | True |
| guard-below-32k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.3888 | 0.3827 | 0.984026 | 52044.3 | 52040.0 | inconclusive | True |
| guard-below-32k/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.3892 | 0.3772 | 0.974284 | 52040.0 | 52048.6 | inconclusive | True |
| guard-below-32k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.3886 | 0.3885 | 0.999446 | 52044.3 | 52040.0 | inconclusive | True |
| guard-below-32k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0477 | 1.0376 | 0.985955 | 123816.6 | 123808.0 | inconclusive | True |
| guard-below-32k/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.0434 | 1.0345 | 0.987612 | 123808.0 | 123812.3 | inconclusive | True |
| guard-below-32k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.0451 | 1.0443 | 0.996102 | 123820.9 | 123808.0 | inconclusive | True |
| guard-below-32k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.6782 | 0.6715 | 0.990928 | 116392.6 | 116388.3 | inconclusive | True |
| guard-below-32k/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.6792 | 0.6671 | 0.982723 | 116388.3 | 116384.0 | optimized | True |
| guard-below-32k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.6835 | 0.6820 | 1.001160 | 116396.9 | 116384.0 | inconclusive | True |
| guard-at-32k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7388 | 0.7314 | 0.990494 | 80776.0 | 80780.3 | inconclusive | True |
| guard-at-32k/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.7587 | 0.7373 | 0.970782 | 80780.3 | 80780.3 | optimized | True |
| guard-at-32k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.7590 | 0.7376 | 0.968521 | 80776.0 | 80780.3 | automatic | True |
| guard-at-32k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5454 | 0.5397 | 0.989749 | 59100.3 | 59096.0 | optimized | True |
| guard-at-32k/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5663 | 0.5438 | 0.961117 | 59096.0 | 59096.0 | optimized | True |
| guard-at-32k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.5651 | 0.5439 | 0.962631 | 59096.0 | 59100.3 | automatic | True |
| guard-at-32k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.5869 | 1.5841 | 1.003229 | 156052.3 | 156052.3 | inconclusive | True |
| guard-at-32k/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.6106 | 1.5938 | 0.990863 | 156052.3 | 156056.6 | inconclusive | True |
| guard-at-32k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.6086 | 1.5896 | 0.986520 | 156052.3 | 156074.8 | inconclusive | True |
| guard-at-32k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0228 | 1.0201 | 0.998848 | 122024.0 | 122032.6 | inconclusive | True |
| guard-at-32k/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.0454 | 1.0264 | 0.981857 | 122032.6 | 122024.0 | optimized | True |
| guard-at-32k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.0482 | 1.0241 | 0.977001 | 122036.9 | 122024.0 | automatic | True |
| guard-at-32k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4830 | 0.4777 | 0.991496 | 64892.3 | 64892.3 | inconclusive | True |
| guard-at-32k/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.4953 | 0.4817 | 0.973286 | 64888.0 | 64888.0 | inconclusive | True |
| guard-at-32k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.4942 | 0.4823 | 0.976459 | 64892.3 | 64892.3 | inconclusive | True |
| guard-at-32k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.3621 | 0.3570 | 0.983880 | 52008.0 | 52008.0 | optimized | True |
| guard-at-32k/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.3736 | 0.3623 | 0.968758 | 52008.0 | 52008.0 | optimized | True |
| guard-at-32k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.3743 | 0.3634 | 0.973143 | 52008.0 | 52016.6 | inconclusive | True |
| guard-at-32k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9866 | 0.9814 | 0.994437 | 123780.3 | 123776.0 | inconclusive | True |
| guard-at-32k/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.9997 | 0.9898 | 0.995173 | 123776.0 | 123788.9 | inconclusive | True |
| guard-at-32k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.0017 | 0.9878 | 0.981013 | 123788.9 | 123776.0 | automatic | True |
| guard-at-32k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.6425 | 0.6358 | 0.989238 | 116348.3 | 116344.0 | optimized | True |
| guard-at-32k/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.6526 | 0.6411 | 0.980506 | 116348.3 | 116348.3 | inconclusive | True |
| guard-at-32k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.6530 | 0.6419 | 0.967667 | 116352.6 | 116344.0 | inconclusive | True |
| guard-above-32k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7521 | 0.7518 | 1.000753 | 81428.3 | 81428.3 | inconclusive | True |
| guard-above-32k/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.7749 | 0.7529 | 0.970753 | 81424.0 | 81428.3 | inconclusive | True |
| guard-above-32k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.7757 | 0.7534 | 0.972174 | 81428.3 | 81428.3 | automatic | True |
| guard-above-32k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5583 | 0.5539 | 0.992006 | 59516.3 | 59512.0 | inconclusive | True |
| guard-above-32k/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5810 | 0.5584 | 0.962095 | 59512.5 | 59520.6 | inconclusive | False |
| guard-above-32k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.5799 | 0.5595 | 0.964892 | 59512.0 | 59516.3 | automatic | True |
| guard-above-32k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.6509 | 1.6238 | 0.993829 | 156724.3 | 156732.9 | inconclusive | False |
| guard-above-32k/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.6465 | 1.6219 | 0.980142 | 156732.9 | 156720.0 | optimized | True |
| guard-above-32k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.6428 | 1.6247 | 0.993310 | 156720.0 | 156737.2 | inconclusive | True |
| guard-above-32k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0684 | 1.0610 | 0.984309 | 122508.9 | 122496.0 | inconclusive | True |
| guard-above-32k/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.0870 | 1.0672 | 0.983570 | 122500.3 | 122496.0 | optimized | True |
| guard-above-32k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.0891 | 1.0685 | 0.988508 | 122496.0 | 122508.9 | inconclusive | True |
| guard-above-32k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4901 | 0.4831 | 0.985885 | 65356.3 | 65356.3 | inconclusive | True |
| guard-above-32k/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5036 | 0.4905 | 0.975707 | 65360.6 | 65352.0 | optimized | True |
| guard-above-32k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.4991 | 0.4886 | 0.978708 | 65356.3 | 65352.0 | automatic | True |
| guard-above-32k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.3686 | 0.3644 | 0.986339 | 52344.0 | 52344.0 | inconclusive | True |
| guard-above-32k/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.3803 | 0.3687 | 0.970110 | 52344.0 | 52344.0 | inconclusive | True |
| guard-above-32k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.3812 | 0.3711 | 0.971431 | 52344.0 | 52344.0 | automatic | True |
| guard-above-32k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0276 | 1.0256 | 1.006536 | 124256.0 | 124268.9 | inconclusive | True |
| guard-above-32k/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.0380 | 1.0244 | 0.983292 | 124268.9 | 124256.0 | optimized | True |
| guard-above-32k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.0353 | 1.0221 | 0.993885 | 124256.0 | 124268.9 | inconclusive | True |
| guard-above-32k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.6678 | 0.6668 | 1.002630 | 116744.0 | 116752.6 | inconclusive | True |
| guard-above-32k/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.6819 | 0.6722 | 0.985941 | 116748.3 | 116752.6 | inconclusive | False |
| guard-above-32k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.6830 | 0.6716 | 0.983311 | 116744.0 | 116748.3 | inconclusive | True |
| narrow-tall/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.1219 | 1.1001 | 0.979756 | 80852.3 | 80852.3 | optimized | True |
| narrow-tall/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.1469 | 1.1205 | 0.975708 | 80856.6 | 80848.0 | optimized | True |
| narrow-tall/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.1463 | 1.1204 | 0.976309 | 80852.3 | 80852.3 | automatic | True |
| narrow-tall/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9336 | 0.9132 | 0.980873 | 59192.0 | 59228.0 | inconclusive | False |
| narrow-tall/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.9576 | 0.9282 | 0.969825 | 59192.0 | 59196.3 | optimized | True |
| narrow-tall/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.9574 | 0.9264 | 0.968921 | 59192.0 | 59196.3 | automatic | True |
| narrow-tall/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.9395 | 1.9217 | 0.984364 | 156147.1 | 156096.0 | optimized | True |
| narrow-tall/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.9731 | 1.9456 | 0.987957 | 156100.3 | 156108.9 | inconclusive | True |
| narrow-tall/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.9723 | 1.9456 | 0.984626 | 156108.9 | 156100.3 | inconclusive | True |
| narrow-tall/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.3993 | 1.3807 | 0.986903 | 122104.0 | 122116.9 | inconclusive | True |
| narrow-tall/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.4298 | 1.4025 | 0.986260 | 122104.0 | 122112.6 | optimized | True |
| narrow-tall/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.4341 | 1.4031 | 0.974567 | 122112.6 | 122104.0 | automatic | True |
| narrow-tall/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9839 | 0.9619 | 0.978018 | 78592.0 | 78596.3 | inconclusive | True |
| narrow-tall/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.0034 | 0.9854 | 0.981858 | 78596.3 | 78596.3 | inconclusive | True |
| narrow-tall/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.0042 | 0.9872 | 0.985610 | 78592.0 | 78600.6 | inconclusive | True |
| narrow-tall/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.8352 | 0.8129 | 0.973257 | 61640.0 | 61644.3 | inconclusive | True |
| narrow-tall/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.8596 | 0.8395 | 0.976623 | 61644.3 | 61640.0 | optimized | True |
| narrow-tall/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.8575 | 0.8405 | 0.979367 | 61640.0 | 61648.6 | inconclusive | True |
| narrow-tall/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.5897 | 1.5676 | 0.985229 | 148716.3 | 148716.3 | inconclusive | True |
| narrow-tall/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.6064 | 1.5904 | 0.988843 | 148720.6 | 148716.3 | inconclusive | True |
| narrow-tall/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.6040 | 1.5949 | 0.990092 | 148716.3 | 148720.6 | inconclusive | True |
| narrow-tall/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.1906 | 1.1674 | 0.980708 | 137388.3 | 137392.6 | inconclusive | True |
| narrow-tall/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.2049 | 1.1871 | 0.984201 | 137392.6 | 137388.3 | inconclusive | True |
| narrow-tall/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.2045 | 1.1900 | 0.987168 | 137388.3 | 137388.3 | inconclusive | True |

Invalid evidence:
- kodim01/444/p2/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- kodim01/444/p2/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- kodim01/444/p2/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- kodim01/420/p2/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- kodim01/420/p2/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- low-noise/420/p2/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- low-noise/420/p2/d4/automatic-policy: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/automatic-policy: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p2/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p2/d1/narrow-zigzag: original: p90/p10 drift exceeds20%, including minority timing states.
- tiny-odd-edges/420/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/automatic-policy: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p2/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p2/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p2/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- guard-below-32k/444/p0/d4/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- guard-above-32k/444/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- guard-above-32k/444/p2/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- guard-above-32k/444/p2/d1/fuzzy-erosion: optimized: p90/p10 drift exceeds20%, including minority timing states.
- guard-above-32k/444/p2/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- guard-above-32k/420/p2/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- narrow-tall/444/p0/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
