# Interleaved kernel comparison

Selected managed variants, measured as alternating ABBA/BAAB quartets in one process. Forced pairs change one kernel and hold the other Automatic. With --include-automatic, automatic-policy compares both Original against both Automatic. Exact preference maps identify each baseline and candidate. All variants produce identical JPEG bytes for every completed comparison.

Ratios below1 favor the candidate. Each ratio is the median of quartet geometric-mean ratios; allocations are mean bytes per encode. MAD, within-quartet repeats, p90/p10 drift and measurement-order bias guard against timing-state changes. Invalid rows retain raw samples but cannot establish a speed result. An automatic-policy row with candidate reachable=false selects neither new kernel and still measures the complete baseline/candidate dispatch paths. No native or full-corpus parity verdict is implied.

Evidence preference requires valid samples and every paired ratio beyond the same side of1+/-0.005; mixed or small effects remain inconclusive. raw-comparisons/ retains each completed quartet before analysis, including malformed or nonfinite raw diagnostics.

| Case | Comparison | Baseline preferences | Candidate preferences | Candidate reachable | Iterations/leg | Baseline ms | Candidate ms | Paired ratio | Baseline B/encode | Candidate B/encode | Evidence preference | Valid |
| --- | --- | --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| intermediate-64k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.8136 | 1.8109 | 0.997377 | 123439.1 | 123432.0 | inconclusive | False |
| intermediate-64k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.7331 | 1.7346 | 0.998095 | 123439.3 | 123432.0 | inconclusive | True |
| intermediate-64k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.4521 | 1.4493 | 0.999298 | 74488.0 | 74488.0 | inconclusive | True |
| intermediate-64k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.4501 | 1.4499 | 0.999634 | 74492.3 | 74488.0 | inconclusive | True |
| intermediate-64k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 3.3341 | 3.3356 | 1.000178 | 204504.0 | 204504.0 | inconclusive | True |
| intermediate-64k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 3.3302 | 3.3314 | 0.998656 | 204508.3 | 204504.0 | inconclusive | True |
| intermediate-64k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.3971 | 2.3961 | 1.000168 | 154600.0 | 154600.0 | inconclusive | True |
| intermediate-64k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 2.4010 | 2.4058 | 1.002467 | 154600.0 | 154608.6 | inconclusive | True |
| intermediate-64k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.1452 | 1.1436 | 0.999609 | 86056.0 | 86056.0 | inconclusive | True |
| intermediate-64k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.1421 | 1.1423 | 0.999372 | 86060.3 | 86056.0 | inconclusive | True |
| intermediate-64k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 0.9647 | 0.9616 | 0.997821 | 60303.0 | 60296.0 | inconclusive | True |
| intermediate-64k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 0.9642 | 0.9629 | 0.997966 | 60296.0 | 60296.0 | inconclusive | True |
| intermediate-64k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.0921 | 2.0865 | 0.998784 | 162696.0 | 162704.6 | inconclusive | True |
| intermediate-64k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 2.0947 | 2.0951 | 0.998281 | 162696.0 | 162700.3 | inconclusive | True |
| intermediate-64k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.4935 | 1.4905 | 0.997487 | 130452.3 | 130448.0 | inconclusive | True |
| intermediate-64k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.4921 | 1.4943 | 0.999537 | 130452.3 | 130452.3 | inconclusive | True |
| intermediate-128k/444/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 3.4193 | 3.4225 | 0.998121 | 201176.8 | 201177.1 | inconclusive | True |
| intermediate-128k/444/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 3.3897 | 3.4112 | 1.007943 | 201128.0 | 201215.7 | inconclusive | True |
| intermediate-128k/444/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.8599 | 2.8469 | 0.997120 | 108820.3 | 108816.0 | inconclusive | True |
| intermediate-128k/444/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 2.8277 | 2.8358 | 1.000310 | 108816.0 | 108816.0 | inconclusive | True |
| intermediate-128k/444/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 6.3948 | 6.3793 | 0.997329 | 287739.0 | 287761.4 | inconclusive | True |
| intermediate-128k/444/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 6.3595 | 6.3811 | 1.002592 | 287729.4 | 287732.7 | inconclusive | True |
| intermediate-128k/444/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 4.6189 | 4.6129 | 0.999266 | 187940.3 | 187936.0 | inconclusive | True |
| intermediate-128k/444/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 4.6112 | 4.6057 | 0.998243 | 187944.6 | 187936.0 | inconclusive | True |
| intermediate-128k/420/p0/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.2318 | 2.2297 | 0.996532 | 131992.0 | 132016.0 | inconclusive | False |
| intermediate-128k/420/p0/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 2.2365 | 2.2450 | 1.003946 | 132011.7 | 132027.4 | inconclusive | True |
| intermediate-128k/420/p0/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 1.8887 | 1.8786 | 0.996251 | 80468.3 | 80464.0 | inconclusive | True |
| intermediate-128k/420/p0/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 1.8807 | 1.8832 | 1.003047 | 80464.0 | 80464.0 | inconclusive | True |
| intermediate-128k/420/p2/d1 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 3.9804 | 3.9852 | 1.001979 | 220184.0 | 220188.3 | inconclusive | True |
| intermediate-128k/420/p2/d1 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 3.9731 | 3.9678 | 0.998428 | 220192.6 | 220188.3 | inconclusive | True |
| intermediate-128k/420/p2/d4 | fuzzy-erosion | fuzzy=original,narrow=automatic | fuzzy=optimized,narrow=automatic | True | 20 | 2.8536 | 2.8539 | 0.999868 | 161592.0 | 161596.3 | inconclusive | True |
| intermediate-128k/420/p2/d4 | automatic-policy | fuzzy=original,narrow=original | fuzzy=automatic,narrow=automatic | False | 20 | 2.8550 | 2.8482 | 0.997385 | 161596.3 | 161592.0 | inconclusive | True |

16 narrow-zigzag comparisons were not applicable to this pipeline's vector width; skipped.json retains their case identities and reason.

Invalid evidence:
- intermediate-64k/444/p0/d1/fuzzy-erosion: original: repeated legs within a quartet differ by over10%.
- intermediate-128k/420/p0/d1/fuzzy-erosion: optimized: repeated legs within a quartet differ by over10%.
