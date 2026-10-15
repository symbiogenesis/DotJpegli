# Same-host encoder development comparison

Baseline `60835e6`, current `5604d601ceda2b9600aeff153a1ee65a4e9774cf`; CPU: AMD EPYC 9V74 80-Core Processor.

Both uninstrumented comparisons use the same pinned native executable, corpus, CPU affinity, runtime and fixed evidence policy. The current comparison runs first, then the baseline; source revisions are not interleaved. Native-calibrated iteration counts and all samples are retained. This diagnostic cannot replace the current revision's managed/native bounds or establish whole-corpus parity.

| Case | Baseline managed (ms) | Current managed (ms) | Current/baseline managed | Current/baseline native |
| --- | ---: | ---: | ---: | ---: |
| kodim01/420/p0/d1 | 4.022 | 2.852 | 0.709174 | 0.998874 |
| kodim01/420/p2/d1 | 9.421 | 6.290 | 0.667663 | 0.996465 |
| kodim01/444/p0/d1 | 8.554 | 3.823 | 0.446898 | 0.811957 |
| kodim01/444/p2/d1 | 13.568 | 9.109 | 0.671383 | 0.991689 |
