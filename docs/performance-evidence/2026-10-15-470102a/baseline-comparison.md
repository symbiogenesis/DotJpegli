# Same-host encoder development comparison

Baseline `60835e6`, current `470102aa461fe019fc0bda19d48df07dde94be13`; CPU: AMD EPYC 9V74 80-Core Processor.

Both uninstrumented comparisons use the same pinned native executable, corpus, CPU affinity, runtime and fixed evidence policy. The current comparison runs first, then the baseline; source revisions are not interleaved. Native-calibrated iteration counts and all samples are retained. This diagnostic cannot replace the current revision's managed/native bounds or establish whole-corpus parity.

| Case | Baseline managed (ms) | Current managed (ms) | Current/baseline managed | Current/baseline native |
| --- | ---: | ---: | ---: | ---: |
| kodim01/420/p0/d1 | 6.077 | 6.012 | 0.989257 | 0.996516 |
| kodim01/420/p2/d1 | 13.733 | 11.002 | 0.801079 | 0.994215 |
| kodim01/444/p0/d1 | 8.564 | 8.399 | 0.980735 | 0.995805 |
| kodim01/444/p2/d1 | 19.333 | 16.835 | 0.870784 | 1.000486 |
