# Interleaved kernel comparison

Selected managed variants, measured as alternating ABBA/BAAB quartets in one process. Forced pairs change one kernel and hold the other Automatic. With --include-automatic, automatic-policy compares both Original against both Automatic. Exact preference maps identify each baseline and candidate. All variants produce identical JPEG bytes for every completed comparison.

Ratios below1 favor the candidate. Each ratio is the median of quartet geometric-mean ratios; allocations are mean bytes per encode. MAD, within-quartet repeats, p90/p10 drift and measurement-order bias guard against timing-state changes. Invalid rows retain raw samples but cannot establish a speed result. An automatic-policy row with candidate reachable=false selects neither new kernel and still measures the complete baseline/candidate dispatch paths. No native or full-corpus parity verdict is implied.

Evidence preference requires valid samples and every paired ratio beyond the same side of1+/-0.005; mixed or small effects remain inconclusive. raw-comparisons/ retains each completed quartet before analysis, including malformed or nonfinite raw diagnostics.

| Case | Comparison | Baseline preferences | Candidate preferences | Candidate reachable | Iterations/leg | Baseline ms | Candidate ms | Paired ratio | Baseline B/encode | Candidate B/encode | Evidence preference | Valid |
| --- | --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| intermediate-64k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.7185 | 1.7161 | 0.999196 | 123434.6 | 123432.0 | inconclusive | True |
| intermediate-64k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.6418 | 1.6383 | 1.000314 | 123448.9 | 123432.0 | inconclusive | False |
| intermediate-64k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.3714 | 1.3720 | 0.999014 | 74488.0 | 74488.0 | inconclusive | True |
| intermediate-64k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.3743 | 1.3735 | 0.999580 | 74492.3 | 74488.0 | inconclusive | True |
| intermediate-64k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 3.2070 | 3.2044 | 0.999434 | 204504.0 | 204514.0 | inconclusive | True |
| intermediate-64k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 3.2051 | 3.2011 | 0.999459 | 204504.0 | 204508.3 | inconclusive | True |
| intermediate-64k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.3124 | 2.3087 | 0.998661 | 154604.3 | 154600.0 | inconclusive | True |
| intermediate-64k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 2.3120 | 2.3116 | 1.000131 | 154600.0 | 154604.3 | inconclusive | True |
| intermediate-64k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.0788 | 1.0781 | 0.999555 | 86056.0 | 86056.0 | inconclusive | True |
| intermediate-64k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.0769 | 1.0772 | 0.999805 | 86056.0 | 86060.3 | inconclusive | True |
| intermediate-64k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9122 | 0.9108 | 0.999276 | 60296.0 | 60300.3 | inconclusive | True |
| intermediate-64k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.9122 | 0.9121 | 0.999569 | 60296.0 | 60296.0 | inconclusive | True |
| intermediate-64k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.0057 | 2.0013 | 0.998759 | 162696.0 | 162748.1 | inconclusive | True |
| intermediate-64k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 2.0057 | 2.0036 | 0.999866 | 162700.3 | 162696.0 | inconclusive | True |
| intermediate-64k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.4327 | 1.4306 | 0.998680 | 130452.3 | 130448.0 | inconclusive | True |
| intermediate-64k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.4387 | 1.4373 | 0.999105 | 130456.6 | 130448.0 | inconclusive | True |
| intermediate-128k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 3.2664 | 3.2846 | 1.000254 | 201128.0 | 201170.0 | inconclusive | True |
| intermediate-128k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 3.2546 | 3.2381 | 0.998162 | 201160.8 | 201128.0 | inconclusive | True |
| intermediate-128k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.7067 | 2.7096 | 0.999448 | 108820.3 | 108816.0 | inconclusive | True |
| intermediate-128k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 2.6987 | 2.7007 | 1.000345 | 108816.0 | 108820.3 | inconclusive | True |
| intermediate-128k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 6.1292 | 6.1365 | 1.002541 | 287708.3 | 287762.4 | inconclusive | True |
| intermediate-128k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 6.1200 | 6.1225 | 1.001117 | 287703.9 | 287708.3 | inconclusive | True |
| intermediate-128k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 4.4525 | 4.4526 | 1.000218 | 187936.0 | 187936.0 | inconclusive | True |
| intermediate-128k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 4.4602 | 4.4589 | 0.998762 | 187944.6 | 187936.0 | inconclusive | True |
| intermediate-128k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.1140 | 2.1103 | 1.000135 | 132011.7 | 132011.7 | inconclusive | False |
| intermediate-128k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 2.1114 | 2.1120 | 1.000597 | 131996.3 | 131992.0 | inconclusive | True |
| intermediate-128k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.7859 | 1.7837 | 0.998128 | 80468.3 | 80464.0 | inconclusive | True |
| intermediate-128k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.7854 | 1.7842 | 0.999386 | 80464.0 | 80464.0 | inconclusive | True |
| intermediate-128k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 3.8030 | 3.8045 | 0.999299 | 220212.0 | 220231.6 | inconclusive | True |
| intermediate-128k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 3.8111 | 3.8072 | 0.998790 | 220216.3 | 220188.3 | inconclusive | True |
| intermediate-128k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.7526 | 2.7534 | 1.000323 | 161592.0 | 161592.0 | inconclusive | True |
| intermediate-128k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 2.7561 | 2.7520 | 0.999741 | 161596.3 | 161592.0 | inconclusive | True |

16 narrow-zigzag comparisons were not applicable to this pipeline's vector width; skipped.json retains their case identities and reason.

Invalid evidence:
- intermediate-64k/444/p0/d1/automatic-policy: original: repeated legs within a quartet differ by over10%.
- intermediate-128k/420/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
