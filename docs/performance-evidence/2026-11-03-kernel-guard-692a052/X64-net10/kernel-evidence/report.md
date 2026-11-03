# Interleaved kernel comparison

Selected managed variants, measured as alternating ABBA/BAAB quartets in one process. Forced pairs change one kernel and hold the other Automatic. With --include-automatic, automatic-policy compares both Original against both Automatic. Exact preference maps identify each baseline and candidate. All variants produce identical JPEG bytes for every completed comparison.

Ratios below1 favor the candidate. Each ratio is the median of quartet geometric-mean ratios; allocations are mean bytes per encode. MAD, within-quartet repeats, p90/p10 drift and measurement-order bias guard against timing-state changes. Invalid rows retain raw samples but cannot establish a speed result. An automatic-policy row with candidate reachable=false selects neither new kernel and still measures the complete baseline/candidate dispatch paths. No native or full-corpus parity verdict is implied.

Evidence preference requires valid samples and every paired ratio beyond the same side of1+/-0.005; mixed or small effects remain inconclusive. raw-comparisons/ retains each completed quartet before analysis, including malformed or nonfinite raw diagnostics.

| Case | Comparison | Baseline preferences | Candidate preferences | Candidate reachable | Iterations/leg | Baseline ms | Candidate ms | Paired ratio | Baseline B/encode | Candidate B/encode | Evidence preference | Valid |
| --- | --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| kodim01/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 3.2655 | 3.2319 | 0.993453 | 195607.5 | 195598.6 | inconclusive | False |
| kodim01/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 3.4642 | 3.3235 | 0.964183 | 195609.1 | 195606.1 | inconclusive | False |
| kodim01/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 3.5410 | 3.3867 | 0.963972 | 195564.3 | 195564.3 | automatic | True |
| kodim01/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.8699 | 2.8199 | 0.990700 | 110016.6 | 110016.6 | inconclusive | True |
| kodim01/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 2.8773 | 2.7668 | 0.968348 | 110016.6 | 110016.6 | inconclusive | False |
| kodim01/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 2.8345 | 2.7234 | 0.960603 | 110016.6 | 110008.0 | automatic | True |
| kodim01/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 7.2635 | 7.1491 | 0.985108 | 254012.0 | 253991.6 | inconclusive | False |
| kodim01/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 7.3155 | 7.2214 | 0.988418 | 253984.6 | 253976.0 | inconclusive | True |
| kodim01/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 7.3179 | 7.1280 | 0.971152 | 253976.0 | 253998.6 | inconclusive | True |
| kodim01/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 5.0148 | 5.0188 | 0.997980 | 164972.9 | 164972.9 | inconclusive | False |
| kodim01/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 5.0625 | 4.9476 | 0.978801 | 164972.9 | 164972.9 | inconclusive | False |
| kodim01/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 5.1646 | 5.0518 | 0.974301 | 164964.3 | 164964.3 | automatic | True |
| kodim01/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.5697 | 2.5164 | 0.981186 | 177951.8 | 177912.0 | inconclusive | True |
| kodim01/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 2.5489 | 2.4632 | 0.960825 | 177920.6 | 177912.0 | inconclusive | False |
| kodim01/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 2.5851 | 2.4641 | 0.955690 | 177961.0 | 177934.9 | inconclusive | False |
| kodim01/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.0110 | 2.0161 | 0.994421 | 98684.3 | 98688.6 | inconclusive | True |
| kodim01/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 2.1510 | 2.0649 | 0.940945 | 98688.6 | 98684.3 | inconclusive | False |
| kodim01/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 2.1049 | 1.9936 | 0.949459 | 98684.3 | 98684.3 | automatic | True |
| kodim01/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 5.5191 | 5.4983 | 1.002873 | 226944.6 | 226944.6 | inconclusive | False |
| kodim01/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 5.4021 | 5.3097 | 0.976697 | 226948.9 | 226940.3 | inconclusive | True |
| kodim01/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 5.3791 | 5.2844 | 0.974207 | 226944.5 | 226956.9 | inconclusive | False |
| kodim01/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 3.3722 | 3.3579 | 0.993891 | 149956.9 | 149952.6 | inconclusive | True |
| kodim01/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 3.4792 | 3.3835 | 0.962548 | 149948.3 | 149952.6 | inconclusive | True |
| kodim01/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 3.5882 | 3.4895 | 0.963564 | 149956.9 | 149952.6 | automatic | True |
| low-contrast/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4526 | 0.4522 | 1.003804 | 61560.0 | 61572.9 | inconclusive | True |
| low-contrast/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.4863 | 0.4535 | 0.930432 | 61564.3 | 61560.0 | optimized | True |
| low-contrast/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.4855 | 0.4525 | 0.932830 | 61560.0 | 61568.6 | automatic | True |
| low-contrast/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.3747 | 0.3738 | 0.994676 | 51544.0 | 51544.0 | inconclusive | False |
| low-contrast/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.3985 | 0.3771 | 0.943195 | 51544.0 | 51552.6 | optimized | True |
| low-contrast/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.4037 | 0.3795 | 0.940187 | 51548.3 | 51548.3 | automatic | True |
| low-contrast/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7719 | 0.7761 | 1.008166 | 129952.6 | 129952.6 | inconclusive | False |
| low-contrast/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.7935 | 0.7492 | 0.949257 | 129944.0 | 129956.9 | inconclusive | False |
| low-contrast/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.7791 | 0.7588 | 0.963045 | 129944.0 | 129961.2 | automatic | True |
| low-contrast/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5791 | 0.5764 | 0.996701 | 103280.0 | 103284.3 | inconclusive | True |
| low-contrast/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.6138 | 0.5853 | 0.955652 | 103297.2 | 103280.0 | optimized | True |
| low-contrast/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.6314 | 0.6094 | 0.966359 | 103284.3 | 103288.6 | automatic | True |
| low-contrast/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.3178 | 0.3149 | 0.992810 | 58256.0 | 58260.3 | inconclusive | True |
| low-contrast/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.3361 | 0.3187 | 0.948458 | 58260.3 | 58260.3 | optimized | True |
| low-contrast/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.3333 | 0.3190 | 0.958157 | 58256.0 | 58260.3 | automatic | True |
| low-contrast/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.2701 | 0.2661 | 0.994207 | 50472.0 | 50476.3 | inconclusive | True |
| low-contrast/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.2837 | 0.2670 | 0.942735 | 50476.3 | 50472.0 | optimized | True |
| low-contrast/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.2790 | 0.2651 | 0.949411 | 50472.0 | 50480.6 | automatic | True |
| low-contrast/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5323 | 0.5340 | 0.999751 | 110728.6 | 110728.6 | inconclusive | True |
| low-contrast/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5446 | 0.5279 | 0.965080 | 110728.6 | 110728.6 | inconclusive | True |
| low-contrast/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.5450 | 0.5239 | 0.970359 | 110728.6 | 110728.6 | automatic | True |
| low-contrast/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.3913 | 0.3898 | 0.997217 | 91660.3 | 91660.3 | inconclusive | True |
| low-contrast/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.4063 | 0.3920 | 0.963483 | 91660.3 | 91664.6 | optimized | True |
| low-contrast/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.4122 | 0.3946 | 0.948908 | 91668.9 | 91660.3 | automatic | True |
| low-noise/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4604 | 0.4569 | 0.990664 | 64300.3 | 64296.0 | inconclusive | True |
| low-noise/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.4883 | 0.4679 | 0.954669 | 64296.0 | 64300.3 | optimized | True |
| low-noise/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.4820 | 0.4603 | 0.955918 | 64296.0 | 64308.9 | automatic | True |
| low-noise/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.3399 | 0.3360 | 0.989880 | 49652.3 | 49648.0 | inconclusive | True |
| low-noise/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.3484 | 0.3366 | 0.966285 | 49652.3 | 49652.3 | optimized | True |
| low-noise/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.3493 | 0.3380 | 0.967182 | 49656.6 | 49648.0 | automatic | True |
| low-noise/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.8186 | 0.8152 | 0.995049 | 110100.3 | 110100.3 | inconclusive | True |
| low-noise/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.8327 | 0.8124 | 0.975399 | 110104.6 | 110104.6 | inconclusive | True |
| low-noise/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.8384 | 0.8213 | 0.978892 | 110104.6 | 110104.6 | inconclusive | True |
| low-noise/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5170 | 0.5138 | 0.994740 | 78272.0 | 78276.3 | inconclusive | True |
| low-noise/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5307 | 0.5191 | 0.980212 | 78276.3 | 78276.3 | inconclusive | True |
| low-noise/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.5306 | 0.5188 | 0.980358 | 78276.3 | 78276.3 | automatic | True |
| low-noise/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.3206 | 0.3175 | 0.993285 | 53820.3 | 53820.3 | inconclusive | True |
| low-noise/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.3342 | 0.3213 | 0.960366 | 53820.3 | 53820.3 | optimized | True |
| low-noise/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.3320 | 0.3184 | 0.958718 | 53816.0 | 53820.3 | automatic | True |
| low-noise/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.2468 | 0.2462 | 0.990665 | 43200.0 | 43204.3 | inconclusive | True |
| low-noise/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.2642 | 0.2550 | 0.966325 | 43200.0 | 43204.3 | inconclusive | True |
| low-noise/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.2591 | 0.2496 | 0.962259 | 43204.3 | 43200.0 | automatic | True |
| low-noise/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5362 | 0.5264 | 0.994092 | 100476.3 | 100480.6 | inconclusive | True |
| low-noise/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5319 | 0.5233 | 0.983253 | 100476.3 | 100480.6 | inconclusive | True |
| low-noise/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.5266 | 0.5120 | 0.970525 | 100476.3 | 100476.3 | automatic | True |
| low-noise/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.3402 | 0.3363 | 1.001130 | 78784.0 | 78796.9 | inconclusive | True |
| low-noise/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.3489 | 0.3417 | 0.976721 | 78784.0 | 78788.3 | inconclusive | True |
| low-noise/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.3575 | 0.3480 | 0.975885 | 78784.0 | 78792.6 | inconclusive | True |
| tiny-odd-edges/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0217 | 0.0214 | 0.988853 | 36818.6 | 36818.6 | inconclusive | False |
| tiny-odd-edges/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0216 | 0.0214 | 1.011135 | 36817.7 | 36818.6 | inconclusive | False |
| tiny-odd-edges/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0214 | 0.0214 | 0.993753 | 36819.4 | 36817.7 | inconclusive | False |
| tiny-odd-edges/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0219 | 0.0215 | 0.940044 | 36523.4 | 36521.7 | inconclusive | False |
| tiny-odd-edges/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0216 | 0.0212 | 0.985562 | 36521.7 | 36522.6 | inconclusive | False |
| tiny-odd-edges/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0215 | 0.0215 | 0.999287 | 36522.6 | 36522.6 | inconclusive | False |
| tiny-odd-edges/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0399 | 0.0409 | 1.015726 | 63490.6 | 63493.2 | inconclusive | False |
| tiny-odd-edges/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0454 | 0.0606 | 1.221493 | 63492.3 | 63492.3 | inconclusive | False |
| tiny-odd-edges/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0436 | 0.0419 | 0.978566 | 63493.2 | 63490.6 | inconclusive | False |
| tiny-odd-edges/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0401 | 0.0378 | 0.982021 | 63166.0 | 63161.7 | inconclusive | False |
| tiny-odd-edges/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0382 | 0.0371 | 0.976653 | 63165.2 | 63162.6 | inconclusive | True |
| tiny-odd-edges/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0397 | 0.0373 | 0.979116 | 63166.0 | 63162.6 | inconclusive | False |
| tiny-odd-edges/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0206 | 0.0202 | 0.960356 | 36626.6 | 36625.7 | inconclusive | False |
| tiny-odd-edges/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0203 | 0.0204 | 1.027403 | 36625.7 | 36627.4 | inconclusive | False |
| tiny-odd-edges/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0208 | 0.0210 | 1.060740 | 36625.7 | 36627.4 | inconclusive | False |
| tiny-odd-edges/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0204 | 0.0200 | 0.967356 | 36450.6 | 36449.7 | inconclusive | False |
| tiny-odd-edges/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0203 | 0.0202 | 1.018821 | 36449.7 | 36450.6 | inconclusive | False |
| tiny-odd-edges/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0201 | 0.0201 | 0.979012 | 36450.6 | 36449.7 | inconclusive | False |
| tiny-odd-edges/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0360 | 0.0357 | 0.978514 | 64435.4 | 64436.3 | inconclusive | True |
| tiny-odd-edges/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0372 | 0.0368 | 1.000547 | 64435.4 | 64436.3 | inconclusive | False |
| tiny-odd-edges/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0369 | 0.0383 | 1.020617 | 64434.6 | 64437.2 | inconclusive | False |
| tiny-odd-edges/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 100 | 0.0373 | 0.0360 | 0.978563 | 64204.3 | 64203.4 | inconclusive | False |
| tiny-odd-edges/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 100 | 0.0385 | 0.0358 | 0.982638 | 64206.0 | 64202.6 | inconclusive | False |
| tiny-odd-edges/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 100 | 0.0352 | 0.0352 | 0.996285 | 64207.1 | 64202.6 | inconclusive | False |
| guard-below-32k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4601 | 0.4621 | 0.999754 | 80808.0 | 80820.9 | inconclusive | True |
| guard-below-32k/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.4615 | 0.4412 | 0.956349 | 80820.9 | 80808.0 | optimized | True |
| guard-below-32k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.4488 | 0.4487 | 0.997699 | 80812.3 | 80816.6 | inconclusive | True |
| guard-below-32k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.3436 | 0.3389 | 0.991581 | 59120.6 | 59112.0 | optimized | True |
| guard-below-32k/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.3513 | 0.3320 | 0.945368 | 59116.3 | 59112.0 | inconclusive | False |
| guard-below-32k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.3662 | 0.3605 | 0.990260 | 59120.6 | 59112.0 | inconclusive | False |
| guard-below-32k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9247 | 0.9271 | 1.000019 | 156112.6 | 156108.3 | inconclusive | True |
| guard-below-32k/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.9392 | 0.9167 | 0.978484 | 156130.8 | 156116.9 | inconclusive | False |
| guard-below-32k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.9350 | 0.9450 | 1.013888 | 156108.3 | 156116.9 | inconclusive | False |
| guard-below-32k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5690 | 0.5670 | 1.009982 | 122064.0 | 122081.2 | inconclusive | True |
| guard-below-32k/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5623 | 0.5404 | 0.963605 | 122072.6 | 122072.6 | optimized | True |
| guard-below-32k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.5719 | 0.5581 | 0.989124 | 122076.9 | 122064.0 | inconclusive | False |
| guard-below-32k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.2993 | 0.2936 | 0.986975 | 64916.3 | 64912.0 | inconclusive | True |
| guard-below-32k/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.2929 | 0.2841 | 0.970741 | 64916.3 | 64916.3 | inconclusive | False |
| guard-below-32k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.3029 | 0.3032 | 1.004011 | 64916.3 | 64916.3 | inconclusive | True |
| guard-below-32k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.2297 | 0.2273 | 0.985331 | 52048.6 | 52040.0 | inconclusive | True |
| guard-below-32k/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.2311 | 0.2223 | 0.961906 | 52040.0 | 52044.3 | inconclusive | False |
| guard-below-32k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.2316 | 0.2332 | 1.010891 | 52040.0 | 52044.3 | inconclusive | False |
| guard-below-32k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5490 | 0.5334 | 0.973581 | 123820.9 | 123812.3 | inconclusive | True |
| guard-below-32k/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5388 | 0.5247 | 0.977220 | 123812.3 | 123816.6 | inconclusive | True |
| guard-below-32k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.5422 | 0.5475 | 1.008396 | 123816.6 | 123816.6 | inconclusive | True |
| guard-below-32k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.3592 | 0.3552 | 0.976869 | 116392.6 | 116388.3 | inconclusive | True |
| guard-below-32k/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.3684 | 0.3550 | 0.955033 | 116392.6 | 116388.3 | inconclusive | True |
| guard-below-32k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.3653 | 0.3644 | 0.998975 | 116392.6 | 116392.6 | inconclusive | False |
| guard-at-32k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4406 | 0.4374 | 0.995359 | 80776.0 | 80784.6 | inconclusive | False |
| guard-at-32k/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.4631 | 0.4416 | 0.943039 | 80788.9 | 80776.0 | optimized | True |
| guard-at-32k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.4639 | 0.4363 | 0.925562 | 80784.6 | 80776.0 | inconclusive | False |
| guard-at-32k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.3203 | 0.3181 | 0.990190 | 59096.0 | 59100.3 | inconclusive | True |
| guard-at-32k/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.3537 | 0.3264 | 0.933042 | 59104.6 | 59096.0 | optimized | True |
| guard-at-32k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.3467 | 0.3237 | 0.934461 | 59100.3 | 59100.3 | automatic | True |
| guard-at-32k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9092 | 0.8848 | 0.977854 | 156060.9 | 156060.9 | inconclusive | False |
| guard-at-32k/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.9292 | 0.9036 | 0.970419 | 156060.9 | 156056.6 | inconclusive | True |
| guard-at-32k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.9204 | 0.8982 | 0.984548 | 156056.6 | 156056.6 | inconclusive | False |
| guard-at-32k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5385 | 0.5567 | 1.041052 | 122028.3 | 122041.2 | inconclusive | False |
| guard-at-32k/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5604 | 0.5406 | 0.965798 | 122024.0 | 122032.6 | optimized | True |
| guard-at-32k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.5605 | 0.5346 | 0.940790 | 122045.5 | 122024.0 | automatic | True |
| guard-at-32k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.2853 | 0.2839 | 0.991094 | 64892.3 | 64892.3 | inconclusive | True |
| guard-at-32k/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.2903 | 0.2822 | 0.973786 | 64888.0 | 64892.3 | inconclusive | True |
| guard-at-32k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.2935 | 0.2863 | 0.973814 | 64888.0 | 64896.6 | inconclusive | True |
| guard-at-32k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.2114 | 0.2093 | 0.991970 | 52008.0 | 52012.3 | inconclusive | False |
| guard-at-32k/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.2229 | 0.2117 | 0.951139 | 52012.3 | 52008.0 | inconclusive | False |
| guard-at-32k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.2219 | 0.2118 | 0.944730 | 52012.3 | 52012.3 | inconclusive | False |
| guard-at-32k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4895 | 0.4912 | 0.998628 | 123784.6 | 123784.6 | inconclusive | False |
| guard-at-32k/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.4970 | 0.4836 | 0.977956 | 123788.9 | 123776.0 | inconclusive | True |
| guard-at-32k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.5032 | 0.5066 | 0.983938 | 123776.0 | 123793.2 | inconclusive | False |
| guard-at-32k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.3380 | 0.3378 | 0.974216 | 116352.6 | 116348.3 | inconclusive | False |
| guard-at-32k/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.3525 | 0.3637 | 1.017334 | 116348.3 | 116356.9 | inconclusive | False |
| guard-at-32k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.3507 | 0.3430 | 0.974717 | 116348.3 | 116352.6 | inconclusive | True |
| guard-above-32k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4420 | 0.4402 | 1.000969 | 81424.0 | 81432.6 | inconclusive | True |
| guard-above-32k/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.4639 | 0.4431 | 0.951495 | 81432.6 | 81424.0 | optimized | True |
| guard-above-32k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.4612 | 0.4390 | 0.952301 | 81432.6 | 81424.0 | automatic | True |
| guard-above-32k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.3288 | 0.3260 | 0.991358 | 59516.3 | 59512.0 | inconclusive | True |
| guard-above-32k/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.3506 | 0.3281 | 0.921797 | 59520.6 | 59512.0 | inconclusive | False |
| guard-above-32k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.3503 | 0.3267 | 0.922994 | 59520.6 | 59512.0 | automatic | True |
| guard-above-32k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9321 | 0.9275 | 0.994877 | 156732.9 | 156724.3 | inconclusive | True |
| guard-above-32k/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.9628 | 0.9348 | 0.975937 | 156732.9 | 156732.9 | optimized | True |
| guard-above-32k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.9731 | 0.9423 | 0.961636 | 156728.6 | 156728.6 | inconclusive | False |
| guard-above-32k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5599 | 0.5604 | 1.014999 | 122496.0 | 122508.9 | inconclusive | True |
| guard-above-32k/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5938 | 0.5650 | 0.951527 | 122513.2 | 122496.0 | inconclusive | False |
| guard-above-32k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.5812 | 0.5599 | 0.986890 | 122500.3 | 122508.9 | automatic | True |
| guard-above-32k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.2899 | 0.2876 | 0.991566 | 65352.0 | 65356.3 | inconclusive | False |
| guard-above-32k/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.2997 | 0.2913 | 0.969367 | 65352.0 | 65356.3 | inconclusive | False |
| guard-above-32k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.3101 | 0.2896 | 0.943939 | 65356.3 | 65360.6 | inconclusive | False |
| guard-above-32k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.2162 | 0.2136 | 0.994252 | 52344.0 | 52352.6 | inconclusive | False |
| guard-above-32k/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.2256 | 0.2157 | 0.950582 | 52348.3 | 52344.0 | inconclusive | False |
| guard-above-32k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.2246 | 0.2150 | 0.956465 | 52344.0 | 52348.3 | inconclusive | False |
| guard-above-32k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5201 | 0.5077 | 0.977152 | 124264.6 | 124260.3 | inconclusive | False |
| guard-above-32k/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5211 | 0.5093 | 0.971518 | 124264.6 | 124260.3 | inconclusive | True |
| guard-above-32k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.5327 | 0.5174 | 0.958642 | 124268.9 | 124260.3 | automatic | True |
| guard-above-32k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.3412 | 0.3425 | 0.996048 | 116748.3 | 116752.6 | inconclusive | True |
| guard-above-32k/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.3606 | 0.3535 | 0.980207 | 116748.3 | 116752.6 | inconclusive | True |
| guard-above-32k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.3659 | 0.3557 | 0.975739 | 116752.6 | 116752.6 | inconclusive | False |
| narrow-tall/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.6147 | 0.6013 | 0.989752 | 80848.0 | 80856.6 | inconclusive | True |
| narrow-tall/444/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.6434 | 0.6173 | 0.957836 | 80848.0 | 80852.3 | optimized | True |
| narrow-tall/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.6481 | 0.6139 | 0.946317 | 80856.6 | 80848.0 | automatic | True |
| narrow-tall/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5092 | 0.4914 | 0.967771 | 59200.6 | 59192.0 | optimized | True |
| narrow-tall/444/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5314 | 0.4994 | 0.933889 | 59200.6 | 59192.0 | optimized | True |
| narrow-tall/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.5478 | 0.5206 | 0.947780 | 59196.3 | 59192.0 | automatic | True |
| narrow-tall/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0823 | 1.0689 | 0.990974 | 156100.3 | 156104.6 | inconclusive | True |
| narrow-tall/444/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 1.1133 | 1.0848 | 0.979003 | 156104.6 | 156113.2 | inconclusive | False |
| narrow-tall/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 1.1367 | 1.1126 | 0.971245 | 156104.6 | 156108.9 | inconclusive | True |
| narrow-tall/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7661 | 0.7454 | 0.963958 | 122121.2 | 122104.0 | optimized | True |
| narrow-tall/444/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.7736 | 0.7490 | 0.974106 | 122108.3 | 122116.9 | optimized | True |
| narrow-tall/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.7580 | 0.7351 | 0.973585 | 122104.0 | 122116.9 | inconclusive | True |
| narrow-tall/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.5415 | 0.5302 | 0.974924 | 78604.9 | 78592.0 | optimized | True |
| narrow-tall/420/p0/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.5565 | 0.5425 | 0.972217 | 78596.3 | 78596.3 | inconclusive | True |
| narrow-tall/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.5548 | 0.5375 | 0.965333 | 78596.3 | 78592.0 | inconclusive | True |
| narrow-tall/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.4511 | 0.4383 | 0.974166 | 61640.0 | 61648.6 | inconclusive | True |
| narrow-tall/420/p0/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.4657 | 0.4497 | 0.967354 | 61640.0 | 61648.6 | inconclusive | True |
| narrow-tall/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.4614 | 0.4487 | 0.966787 | 61644.3 | 61644.3 | inconclusive | True |
| narrow-tall/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.7966 | 0.7818 | 0.987166 | 148720.6 | 148724.9 | inconclusive | True |
| narrow-tall/420/p2/d1 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.8221 | 0.8031 | 0.981904 | 148720.6 | 148724.9 | inconclusive | True |
| narrow-tall/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.8143 | 0.8010 | 0.976540 | 148716.3 | 148716.3 | inconclusive | True |
| narrow-tall/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.6106 | 0.6005 | 0.980636 | 137388.3 | 137392.6 | inconclusive | False |
| narrow-tall/420/p2/d4 | narrow-zigzag | fuzzy=automatic,narrow=original | fuzzy=automatic,narrow=optimized | True | 20 | 0.6260 | 0.6049 | 0.964825 | 137401.2 | 137388.3 | inconclusive | True |
| narrow-tall/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | True | 20 | 0.6336 | 0.6265 | 1.002606 | 137384.0 | 137401.2 | inconclusive | False |

Invalid evidence:
- kodim01/444/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- kodim01/444/p0/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- kodim01/444/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- kodim01/444/p0/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- kodim01/444/p2/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- kodim01/444/p2/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- kodim01/444/p2/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- kodim01/420/p0/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- kodim01/420/p0/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- kodim01/420/p0/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- kodim01/420/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- kodim01/420/p2/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- kodim01/420/p2/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- low-contrast/444/p0/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- low-contrast/444/p2/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- low-contrast/444/p2/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/narrow-zigzag: original: p90/p10 drift exceeds20%, including minority timing states.
- tiny-odd-edges/444/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/narrow-zigzag: ABBA/BAAB order bias exceeds10% or one order is absent.
- tiny-odd-edges/444/p0/d4/automatic-policy: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p2/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p2/d1/narrow-zigzag: original: relative MAD exceeds10%.
- tiny-odd-edges/444/p2/d1/narrow-zigzag: original: p90/p10 drift exceeds20%, including minority timing states.
- tiny-odd-edges/444/p2/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p2/d1/narrow-zigzag: optimized: relative MAD exceeds10%.
- tiny-odd-edges/444/p2/d1/narrow-zigzag: optimized: p90/p10 drift exceeds20%, including minority timing states.
- tiny-odd-edges/444/p2/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p2/d1/narrow-zigzag: ABBA/BAAB order bias exceeds10% or one order is absent.
- tiny-odd-edges/444/p2/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p2/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p2/d4/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/automatic-policy: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p2/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p2/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p2/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p2/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p2/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p2/d4/automatic-policy: original: repeated legs within a quartet differ by over10%.
- guard-below-32k/444/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- guard-below-32k/444/p0/d4/narrow-zigzag: optimized: p90/p10 drift exceeds20%, including minority timing states.
- guard-below-32k/444/p0/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- guard-below-32k/444/p0/d4/automatic-policy: original: p90/p10 drift exceeds20%, including minority timing states.
- guard-below-32k/444/p0/d4/automatic-policy: original: repeated legs within a quartet differ by over10%.
- guard-below-32k/444/p2/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- guard-below-32k/444/p2/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- guard-below-32k/444/p2/d4/automatic-policy: original: repeated legs within a quartet differ by over10%.
- guard-below-32k/420/p0/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- guard-below-32k/420/p0/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- guard-below-32k/420/p0/d4/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- guard-below-32k/420/p2/d4/automatic-policy: original: repeated legs within a quartet differ by over10%.
- guard-below-32k/420/p2/d4/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- guard-at-32k/444/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- guard-at-32k/444/p0/d1/fuzzy-erosion: optimized: p90/p10 drift exceeds20%, including minority timing states.
- guard-at-32k/444/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- guard-at-32k/444/p0/d1/automatic-policy: original: p90/p10 drift exceeds20%, including minority timing states.
- guard-at-32k/444/p0/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- guard-at-32k/444/p0/d1/automatic-policy: automatic: p90/p10 drift exceeds20%, including minority timing states.
- guard-at-32k/444/p0/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- guard-at-32k/444/p2/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- guard-at-32k/444/p2/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- guard-at-32k/444/p2/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- guard-at-32k/444/p2/d4/fuzzy-erosion: original: p90/p10 drift exceeds20%, including minority timing states.
- guard-at-32k/444/p2/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- guard-at-32k/444/p2/d4/fuzzy-erosion: optimized: p90/p10 drift exceeds20%, including minority timing states.
- guard-at-32k/444/p2/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- guard-at-32k/420/p0/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- guard-at-32k/420/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- guard-at-32k/420/p0/d4/automatic-policy: original: repeated legs within a quartet differ by over10%.
- guard-at-32k/420/p0/d4/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- guard-at-32k/420/p2/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- guard-at-32k/420/p2/d1/automatic-policy: original: p90/p10 drift exceeds20%, including minority timing states.
- guard-at-32k/420/p2/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- guard-at-32k/420/p2/d1/automatic-policy: automatic: p90/p10 drift exceeds20%, including minority timing states.
- guard-at-32k/420/p2/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- guard-at-32k/420/p2/d4/fuzzy-erosion: original: p90/p10 drift exceeds20%, including minority timing states.
- guard-at-32k/420/p2/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- guard-at-32k/420/p2/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- guard-at-32k/420/p2/d4/narrow-zigzag: optimized: p90/p10 drift exceeds20%, including minority timing states.
- guard-at-32k/420/p2/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- guard-above-32k/444/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- guard-above-32k/444/p2/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- guard-above-32k/444/p2/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- guard-above-32k/444/p2/d4/narrow-zigzag: optimized: p90/p10 drift exceeds20%, including minority timing states.
- guard-above-32k/444/p2/d4/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- guard-above-32k/420/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- guard-above-32k/420/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- guard-above-32k/420/p0/d1/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- guard-above-32k/420/p0/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- guard-above-32k/420/p0/d1/automatic-policy: original: p90/p10 drift exceeds20%, including minority timing states.
- guard-above-32k/420/p0/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- guard-above-32k/420/p0/d1/automatic-policy: automatic: p90/p10 drift exceeds20%, including minority timing states.
- guard-above-32k/420/p0/d1/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- guard-above-32k/420/p0/d4/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
- guard-above-32k/420/p0/d4/narrow-zigzag: original: repeated legs within a quartet differ by over10%.
- guard-above-32k/420/p0/d4/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- guard-above-32k/420/p2/d1/fuzzy-erosion: original: p90/p10 drift exceeds20%, including minority timing states.
- guard-above-32k/420/p2/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- guard-above-32k/420/p2/d4/automatic-policy: automatic: repeated legs within a quartet differ by over10%.
- narrow-tall/444/p2/d1/narrow-zigzag: optimized: repeated legs within a quartet differ by over10%.
- narrow-tall/420/p2/d4/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- narrow-tall/420/p2/d4/automatic-policy: original: repeated legs within a quartet differ by over10%.
