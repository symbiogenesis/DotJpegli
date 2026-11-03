# Performance parity: FAILED

256/256 cases: RGB8 YCbCr, Kodak24 plus eight fixed controls, distances1/4, 444/420 and progression0/2, one worker on this CI host. Pinned upstream `031a0077f5799a6041004267fc12b956c1f52a20`.
Tested revision: `6738b09178417877c00c92ec9497a6cfbdc5ab7c`.

All cases are measured even when a latency bound fails, so the report identifies every measured regression. A passing verdict requires complete, valid coverage and all fixed bounds.

Fixed latency noninferiority limits: geometric mean managed/native median latency ratio <=1.05 in each corpus/sampling/progression group; every case <=1.10. A faster case cannot excuse a slower case. No universal performance claim is made.

Seven alternating order blocks, five untimed warmups before each block. The native-only three-iteration calibration targets50ms and selects a shared count clamped3..100. Medians use all seven blocks; no outliers are discarded. Complete in-memory encode calls are timed individually and their elapsed times summed; subprocess startup, file I/O, verification and native buffer free are excluded. Managed collections inside encode are included. Tiered compilation is disabled.

Evidence is invalid if a block interval is below1ms, any case has relative MAD >10%, inputs/settings/coverage differ, or an untimed JPEG structure comparison fails. Invalid evidence cannot establish parity.

| Corpus | Sampling | Progression | Cases | Geometric mean ratio | Worst ratio | Verdict |
| --- | --- | --- | ---: | ---: | ---: | --- |
| natural | 444 | 0 | 48 | 1.193851 | 1.365634 | FAIL |
| natural | 444 | 2 | 48 | 0.922406 | 1.011400 | PASS |
| natural | 420 | 0 | 48 | 1.200947 | 1.522253 | FAIL |
| natural | 420 | 2 | 48 | 0.908697 | 1.031378 | PASS |
| controlled | 444 | 0 | 16 | 1.345375 | 1.971837 | FAIL |
| controlled | 444 | 2 | 16 | 1.228840 | 1.985063 | FAIL |
| controlled | 420 | 0 | 16 | 1.198971 | 1.919805 | FAIL |
| controlled | 420 | 2 | 16 | 1.226836 | 2.050586 | FAIL |

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 5.132 | 5.073 | 1.012 | 0.2 % | 0.1 % |
| kodim01/444/p0/d4 | 4.343 | 3.580 | 1.213 | 0.4 % | 0.1 % |
| kodim01/444/p2/d1 | 10.879 | 12.913 | 0.842 | 0.6 % | 1.5 % |
| kodim01/444/p2/d4 | 8.111 | 8.742 | 0.928 | 0.3 % | 0.4 % |
| kodim01/420/p0/d1 | 3.975 | 4.061 | 0.979 | 0.4 % | 0.3 % |
| kodim01/420/p0/d4 | 3.245 | 2.610 | 1.243 | 0.3 % | 0.1 % |
| kodim01/420/p2/d1 | 8.014 | 9.888 | 0.811 | 0.2 % | 0.1 % |
| kodim01/420/p2/d4 | 5.533 | 6.022 | 0.919 | 0.3 % | 0.1 % |
| kodim02/444/p0/d1 | 4.818 | 4.269 | 1.129 | 0.2 % | 0.3 % |
| kodim02/444/p0/d4 | 4.095 | 3.052 | 1.342 | 0.1 % | 0.1 % |
| kodim02/444/p2/d1 | 9.250 | 10.342 | 0.894 | 0.6 % | 0.3 % |
| kodim02/444/p2/d4 | 7.206 | 7.294 | 0.988 | 0.2 % | 0.2 % |
| kodim02/420/p0/d1 | 3.725 | 3.340 | 1.115 | 0.7 % | 1.6 % |
| kodim02/420/p0/d4 | 3.031 | 2.179 | 1.391 | 0.2 % | 0.2 % |
| kodim02/420/p2/d1 | 6.396 | 7.617 | 0.840 | 0.3 % | 0.2 % |
| kodim02/420/p2/d4 | 4.730 | 4.793 | 0.987 | 0.3 % | 0.4 % |
| kodim03/444/p0/d1 | 4.377 | 3.660 | 1.196 | 0.1 % | 0.1 % |
| kodim03/444/p0/d4 | 3.985 | 2.973 | 1.340 | 0.3 % | 0.1 % |
| kodim03/444/p2/d1 | 8.273 | 8.910 | 0.928 | 0.3 % | 0.5 % |
| kodim03/444/p2/d4 | 7.049 | 7.147 | 0.986 | 0.3 % | 0.3 % |
| kodim03/420/p0/d1 | 3.296 | 2.743 | 1.201 | 0.4 % | 0.1 % |
| kodim03/420/p0/d4 | 2.942 | 2.114 | 1.392 | 0.1 % | 0.2 % |
| kodim03/420/p2/d1 | 5.729 | 6.457 | 0.887 | 0.2 % | 0.2 % |
| kodim03/420/p2/d4 | 4.603 | 4.715 | 0.976 | 0.2 % | 0.3 % |
| kodim04/444/p0/d1 | 4.775 | 4.195 | 1.138 | 0.2 % | 0.2 % |
| kodim04/444/p0/d4 | 4.178 | 3.087 | 1.353 | 0.2 % | 0.1 % |
| kodim04/444/p2/d1 | 9.064 | 10.271 | 0.882 | 0.0 % | 0.1 % |
| kodim04/444/p2/d4 | 7.310 | 7.399 | 0.988 | 0.3 % | 0.3 % |
| kodim04/420/p0/d1 | 3.608 | 3.206 | 1.126 | 0.3 % | 0.1 % |
| kodim04/420/p0/d4 | 3.087 | 2.211 | 1.396 | 0.6 % | 0.2 % |
| kodim04/420/p2/d1 | 6.399 | 7.299 | 0.877 | 0.1 % | 0.2 % |
| kodim04/420/p2/d4 | 4.843 | 4.713 | 1.028 | 0.3 % | 0.2 % |
| kodim05/444/p0/d1 | 5.165 | 5.016 | 1.030 | 0.7 % | 0.1 % |
| kodim05/444/p0/d4 | 4.383 | 3.695 | 1.186 | 0.2 % | 0.2 % |
| kodim05/444/p2/d1 | 11.049 | 12.635 | 0.874 | 0.3 % | 0.1 % |
| kodim05/444/p2/d4 | 8.386 | 9.050 | 0.927 | 0.1 % | 0.3 % |
| kodim05/420/p0/d1 | 3.943 | 4.622 | 0.853 | 0.3 % | 0.2 % |
| kodim05/420/p0/d4 | 3.333 | 2.744 | 1.215 | 0.6 % | 0.1 % |
| kodim05/420/p2/d1 | 8.196 | 9.948 | 0.824 | 0.1 % | 0.1 % |
| kodim05/420/p2/d4 | 5.847 | 6.454 | 0.906 | 0.4 % | 0.3 % |
| kodim06/444/p0/d1 | 4.731 | 4.510 | 1.049 | 0.0 % | 0.2 % |
| kodim06/444/p0/d4 | 4.128 | 3.340 | 1.236 | 0.2 % | 0.3 % |
| kodim06/444/p2/d1 | 9.658 | 11.100 | 0.870 | 0.1 % | 0.4 % |
| kodim06/444/p2/d4 | 7.593 | 8.136 | 0.933 | 0.1 % | 0.3 % |
| kodim06/420/p0/d1 | 3.639 | 3.556 | 1.024 | 0.3 % | 0.2 % |
| kodim06/420/p0/d4 | 3.071 | 2.427 | 1.266 | 0.2 % | 0.1 % |
| kodim06/420/p2/d1 | 7.057 | 8.487 | 0.831 | 0.4 % | 0.5 % |
| kodim06/420/p2/d4 | 5.074 | 5.525 | 0.918 | 0.1 % | 0.1 % |
| kodim07/444/p0/d1 | 4.516 | 3.800 | 1.188 | 0.2 % | 0.1 % |
| kodim07/444/p0/d4 | 4.060 | 3.094 | 1.312 | 0.2 % | 0.2 % |
| kodim07/444/p2/d1 | 8.707 | 9.369 | 0.929 | 0.1 % | 0.5 % |
| kodim07/444/p2/d4 | 7.335 | 7.523 | 0.975 | 0.2 % | 0.2 % |
| kodim07/420/p0/d1 | 3.402 | 2.875 | 1.183 | 0.1 % | 0.2 % |
| kodim07/420/p0/d4 | 3.039 | 2.230 | 1.363 | 0.6 % | 0.1 % |
| kodim07/420/p2/d1 | 6.166 | 6.857 | 0.899 | 0.2 % | 0.6 % |
| kodim07/420/p2/d4 | 4.917 | 5.067 | 0.970 | 0.4 % | 0.3 % |
| kodim08/444/p0/d1 | 5.127 | 5.091 | 1.007 | 0.2 % | 0.1 % |
| kodim08/444/p0/d4 | 4.387 | 3.676 | 1.193 | 0.2 % | 0.2 % |
| kodim08/444/p2/d1 | 10.901 | 12.697 | 0.859 | 0.4 % | 0.1 % |
| kodim08/444/p2/d4 | 8.338 | 8.922 | 0.934 | 0.1 % | 0.3 % |
| kodim08/420/p0/d1 | 4.059 | 4.783 | 0.849 | 0.8 % | 0.7 % |
| kodim08/420/p0/d4 | 3.370 | 2.736 | 1.232 | 1.2 % | 0.8 % |
| kodim08/420/p2/d1 | 8.216 | 9.960 | 0.825 | 0.5 % | 0.1 % |
| kodim08/420/p2/d4 | 5.812 | 6.377 | 0.911 | 0.3 % | 0.1 % |
| kodim09/444/p0/d1 | 4.561 | 3.715 | 1.228 | 0.4 % | 0.2 % |
| kodim09/444/p0/d4 | 4.065 | 2.995 | 1.357 | 0.2 % | 0.1 % |
| kodim09/444/p2/d1 | 8.358 | 8.922 | 0.937 | 0.5 % | 0.1 % |
| kodim09/444/p2/d4 | 7.079 | 7.132 | 0.993 | 0.3 % | 0.4 % |
| kodim09/420/p0/d1 | 3.373 | 2.747 | 1.228 | 0.2 % | 0.1 % |
| kodim09/420/p0/d4 | 2.967 | 2.121 | 1.399 | 0.4 % | 0.1 % |
| kodim09/420/p2/d1 | 5.836 | 6.208 | 0.940 | 0.1 % | 0.2 % |
| kodim09/420/p2/d4 | 4.635 | 4.494 | 1.031 | 0.1 % | 0.2 % |
| kodim10/444/p0/d1 | 4.648 | 3.880 | 1.198 | 0.3 % | 0.1 % |
| kodim10/444/p0/d4 | 4.154 | 3.064 | 1.356 | 0.4 % | 0.2 % |
| kodim10/444/p2/d1 | 8.765 | 9.470 | 0.926 | 1.0 % | 0.3 % |
| kodim10/444/p2/d4 | 7.288 | 7.397 | 0.985 | 0.3 % | 0.7 % |
| kodim10/420/p0/d1 | 3.480 | 2.917 | 1.193 | 0.1 % | 0.4 % |
| kodim10/420/p0/d4 | 3.043 | 2.174 | 1.399 | 0.1 % | 0.1 % |
| kodim10/420/p2/d1 | 6.120 | 6.637 | 0.922 | 0.1 % | 0.5 % |
| kodim10/420/p2/d4 | 4.806 | 4.671 | 1.029 | 0.3 % | 0.4 % |
| kodim11/444/p0/d1 | 4.823 | 4.466 | 1.080 | 0.1 % | 0.1 % |
| kodim11/444/p0/d4 | 4.135 | 3.244 | 1.275 | 0.3 % | 0.1 % |
| kodim11/444/p2/d1 | 9.637 | 11.045 | 0.873 | 0.2 % | 0.2 % |
| kodim11/444/p2/d4 | 7.489 | 7.908 | 0.947 | 0.2 % | 0.2 % |
| kodim11/420/p0/d1 | 3.675 | 3.472 | 1.058 | 0.2 % | 0.0 % |
| kodim11/420/p0/d4 | 3.080 | 2.346 | 1.313 | 0.1 % | 0.4 % |
| kodim11/420/p2/d1 | 7.003 | 8.367 | 0.837 | 0.3 % | 0.1 % |
| kodim11/420/p2/d4 | 5.105 | 5.335 | 0.957 | 0.9 % | 0.3 % |
| kodim12/444/p0/d1 | 4.497 | 3.934 | 1.143 | 0.3 % | 0.7 % |
| kodim12/444/p0/d4 | 4.098 | 3.043 | 1.347 | 0.8 % | 1.2 % |
| kodim12/444/p2/d1 | 8.636 | 9.575 | 0.902 | 1.5 % | 1.1 % |
| kodim12/444/p2/d4 | 7.103 | 7.193 | 0.988 | 0.8 % | 0.2 % |
| kodim12/420/p0/d1 | 3.525 | 2.948 | 1.195 | 0.5 % | 0.4 % |
| kodim12/420/p0/d4 | 3.043 | 2.149 | 1.416 | 0.4 % | 0.8 % |
| kodim12/420/p2/d1 | 6.150 | 6.974 | 0.882 | 1.1 % | 0.5 % |
| kodim12/420/p2/d4 | 4.747 | 4.741 | 1.001 | 0.7 % | 0.8 % |
| kodim13/444/p0/d1 | 5.356 | 5.701 | 0.939 | 0.1 % | 0.6 % |
| kodim13/444/p0/d4 | 4.577 | 3.874 | 1.182 | 1.0 % | 0.2 % |
| kodim13/444/p2/d1 | 11.778 | 14.117 | 0.834 | 0.3 % | 0.3 % |
| kodim13/444/p2/d4 | 8.475 | 9.508 | 0.891 | 0.5 % | 0.3 % |
| kodim13/420/p0/d1 | 4.205 | 5.216 | 0.806 | 0.3 % | 0.1 % |
| kodim13/420/p0/d4 | 3.333 | 2.859 | 1.166 | 0.3 % | 0.1 % |
| kodim13/420/p2/d1 | 9.033 | 11.299 | 0.799 | 0.2 % | 0.2 % |
| kodim13/420/p2/d4 | 5.835 | 6.733 | 0.867 | 0.2 % | 0.2 % |
| kodim14/444/p0/d1 | 5.077 | 4.920 | 1.032 | 0.2 % | 0.1 % |
| kodim14/444/p0/d4 | 4.281 | 3.483 | 1.229 | 0.2 % | 0.2 % |
| kodim14/444/p2/d1 | 10.554 | 12.418 | 0.850 | 0.1 % | 0.5 % |
| kodim14/444/p2/d4 | 7.945 | 8.549 | 0.929 | 0.4 % | 1.4 % |
| kodim14/420/p0/d1 | 4.000 | 3.977 | 1.006 | 2.5 % | 0.9 % |
| kodim14/420/p0/d4 | 3.209 | 2.539 | 1.264 | 0.4 % | 0.2 % |
| kodim14/420/p2/d1 | 7.671 | 9.383 | 0.818 | 0.1 % | 0.3 % |
| kodim14/420/p2/d4 | 5.362 | 5.841 | 0.918 | 0.3 % | 0.2 % |
| kodim15/444/p0/d1 | 4.612 | 3.957 | 1.165 | 0.3 % | 0.1 % |
| kodim15/444/p0/d4 | 4.050 | 3.039 | 1.333 | 0.4 % | 0.1 % |
| kodim15/444/p2/d1 | 8.792 | 9.603 | 0.915 | 0.8 % | 0.2 % |
| kodim15/444/p2/d4 | 7.147 | 7.348 | 0.973 | 0.3 % | 0.2 % |
| kodim15/420/p0/d1 | 3.430 | 2.986 | 1.149 | 0.3 % | 0.1 % |
| kodim15/420/p0/d4 | 3.006 | 2.159 | 1.392 | 0.5 % | 0.1 % |
| kodim15/420/p2/d1 | 6.070 | 6.979 | 0.870 | 0.1 % | 0.2 % |
| kodim15/420/p2/d4 | 4.719 | 4.826 | 0.978 | 0.1 % | 0.2 % |
| kodim16/444/p0/d1 | 4.509 | 4.084 | 1.104 | 0.1 % | 0.1 % |
| kodim16/444/p0/d4 | 4.004 | 3.096 | 1.293 | 0.2 % | 0.2 % |
| kodim16/444/p2/d1 | 8.868 | 10.110 | 0.877 | 0.6 % | 0.3 % |
| kodim16/444/p2/d4 | 7.191 | 7.450 | 0.965 | 0.4 % | 0.2 % |
| kodim16/420/p0/d1 | 3.430 | 3.115 | 1.101 | 0.1 % | 0.2 % |
| kodim16/420/p0/d4 | 2.964 | 2.206 | 1.343 | 0.2 % | 0.1 % |
| kodim16/420/p2/d1 | 6.288 | 7.347 | 0.856 | 0.1 % | 0.1 % |
| kodim16/420/p2/d4 | 4.714 | 4.943 | 0.954 | 0.1 % | 0.4 % |
| kodim17/444/p0/d1 | 4.609 | 3.975 | 1.160 | 0.2 % | 0.1 % |
| kodim17/444/p0/d4 | 4.137 | 3.095 | 1.337 | 0.2 % | 0.2 % |
| kodim17/444/p2/d1 | 8.766 | 9.726 | 0.901 | 0.2 % | 0.2 % |
| kodim17/444/p2/d4 | 7.298 | 7.574 | 0.964 | 0.3 % | 0.6 % |
| kodim17/420/p0/d1 | 3.471 | 3.011 | 1.153 | 0.1 % | 0.2 % |
| kodim17/420/p0/d4 | 3.042 | 2.205 | 1.379 | 0.1 % | 0.1 % |
| kodim17/420/p2/d1 | 6.195 | 6.836 | 0.906 | 0.1 % | 0.2 % |
| kodim17/420/p2/d4 | 4.827 | 4.783 | 1.009 | 0.1 % | 0.4 % |
| kodim18/444/p0/d1 | 5.093 | 4.796 | 1.062 | 0.3 % | 0.0 % |
| kodim18/444/p0/d4 | 4.326 | 3.440 | 1.258 | 0.1 % | 0.2 % |
| kodim18/444/p2/d1 | 10.278 | 11.760 | 0.874 | 0.4 % | 0.3 % |
| kodim18/444/p2/d4 | 7.850 | 8.464 | 0.928 | 0.1 % | 0.3 % |
| kodim18/420/p0/d1 | 3.854 | 3.762 | 1.025 | 0.5 % | 0.1 % |
| kodim18/420/p0/d4 | 3.230 | 2.515 | 1.284 | 0.3 % | 0.2 % |
| kodim18/420/p2/d1 | 7.586 | 8.971 | 0.846 | 0.1 % | 0.0 % |
| kodim18/420/p2/d4 | 5.382 | 5.593 | 0.962 | 0.1 % | 0.2 % |
| kodim19/444/p0/d1 | 4.809 | 4.277 | 1.124 | 0.3 % | 0.1 % |
| kodim19/444/p0/d4 | 4.156 | 3.169 | 1.312 | 0.2 % | 0.1 % |
| kodim19/444/p2/d1 | 9.240 | 10.528 | 0.878 | 0.2 % | 0.3 % |
| kodim19/444/p2/d4 | 7.385 | 7.679 | 0.962 | 0.4 % | 0.1 % |
| kodim19/420/p0/d1 | 3.645 | 3.286 | 1.109 | 0.1 % | 0.2 % |
| kodim19/420/p0/d4 | 3.086 | 2.268 | 1.361 | 0.3 % | 0.1 % |
| kodim19/420/p2/d1 | 6.677 | 7.632 | 0.875 | 0.0 % | 0.1 % |
| kodim19/420/p2/d4 | 4.941 | 4.916 | 1.005 | 0.1 % | 0.2 % |
| kodim20/444/p0/d1 | 4.345 | 3.705 | 1.173 | 0.6 % | 0.1 % |
| kodim20/444/p0/d4 | 3.937 | 2.945 | 1.337 | 0.4 % | 0.1 % |
| kodim20/444/p2/d1 | 8.263 | 9.030 | 0.915 | 0.5 % | 0.4 % |
| kodim20/444/p2/d4 | 6.941 | 7.077 | 0.981 | 0.3 % | 0.1 % |
| kodim20/420/p0/d1 | 5.112 | 3.358 | 1.522 | 1.4 % | 5.6 % |
| kodim20/420/p0/d4 | 2.902 | 2.086 | 1.391 | 0.5 % | 0.2 % |
| kodim20/420/p2/d1 | 5.687 | 6.545 | 0.869 | 0.2 % | 0.2 % |
| kodim20/420/p2/d4 | 4.527 | 4.698 | 0.964 | 0.3 % | 0.2 % |
| kodim21/444/p0/d1 | 4.733 | 4.268 | 1.109 | 0.1 % | 0.2 % |
| kodim21/444/p0/d4 | 4.066 | 3.227 | 1.260 | 0.2 % | 0.1 % |
| kodim21/444/p2/d1 | 9.244 | 10.441 | 0.885 | 0.2 % | 0.3 % |
| kodim21/444/p2/d4 | 7.389 | 7.858 | 0.940 | 0.4 % | 0.2 % |
| kodim21/420/p0/d1 | 3.553 | 3.294 | 1.079 | 0.2 % | 0.2 % |
| kodim21/420/p0/d4 | 3.029 | 2.318 | 1.307 | 0.4 % | 0.1 % |
| kodim21/420/p2/d1 | 6.655 | 7.723 | 0.862 | 0.2 % | 0.1 % |
| kodim21/420/p2/d4 | 4.929 | 5.288 | 0.932 | 0.2 % | 0.2 % |
| kodim22/444/p0/d1 | 4.882 | 4.553 | 1.072 | 0.1 % | 0.1 % |
| kodim22/444/p0/d4 | 4.173 | 3.255 | 1.282 | 0.2 % | 0.1 % |
| kodim22/444/p2/d1 | 9.659 | 11.129 | 0.868 | 0.3 % | 0.2 % |
| kodim22/444/p2/d4 | 7.459 | 7.887 | 0.946 | 0.1 % | 0.0 % |
| kodim22/420/p0/d1 | 3.672 | 3.497 | 1.050 | 0.1 % | 0.3 % |
| kodim22/420/p0/d4 | 3.130 | 2.340 | 1.338 | 0.2 % | 0.3 % |
| kodim22/420/p2/d1 | 6.967 | 8.437 | 0.826 | 0.3 % | 0.1 % |
| kodim22/420/p2/d4 | 5.003 | 5.308 | 0.943 | 0.2 % | 0.3 % |
| kodim23/444/p0/d1 | 4.449 | 3.574 | 1.245 | 0.3 % | 0.1 % |
| kodim23/444/p0/d4 | 3.997 | 2.927 | 1.366 | 0.5 % | 0.0 % |
| kodim23/444/p2/d1 | 8.107 | 8.464 | 0.958 | 0.1 % | 0.2 % |
| kodim23/444/p2/d4 | 7.014 | 6.934 | 1.011 | 0.8 % | 0.2 % |
| kodim23/420/p0/d1 | 3.292 | 2.612 | 1.260 | 0.1 % | 0.1 % |
| kodim23/420/p0/d4 | 2.947 | 2.073 | 1.422 | 0.3 % | 0.1 % |
| kodim23/420/p2/d1 | 5.552 | 6.015 | 0.923 | 0.6 % | 0.4 % |
| kodim23/420/p2/d4 | 4.548 | 4.558 | 0.998 | 0.1 % | 0.2 % |
| kodim24/444/p0/d1 | 5.011 | 4.746 | 1.056 | 0.6 % | 0.2 % |
| kodim24/444/p0/d4 | 4.267 | 3.477 | 1.227 | 0.4 % | 0.2 % |
| kodim24/444/p2/d1 | 10.187 | 11.730 | 0.868 | 0.1 % | 0.1 % |
| kodim24/444/p2/d4 | 7.875 | 8.499 | 0.927 | 0.3 % | 0.1 % |
| kodim24/420/p0/d1 | 3.815 | 3.735 | 1.021 | 0.3 % | 0.1 % |
| kodim24/420/p0/d4 | 3.218 | 2.542 | 1.266 | 0.3 % | 0.2 % |
| kodim24/420/p2/d1 | 7.483 | 9.038 | 0.828 | 0.2 % | 0.1 % |
| kodim24/420/p2/d4 | 5.377 | 5.888 | 0.913 | 0.1 % | 0.3 % |
| smooth-gradient/444/p0/d1 | 0.609 | 0.369 | 1.653 | 0.3 % | 0.1 % |
| smooth-gradient/444/p0/d4 | 0.591 | 0.369 | 1.602 | 0.3 % | 0.2 % |
| smooth-gradient/444/p2/d1 | 0.992 | 0.688 | 1.443 | 0.7 % | 0.1 % |
| smooth-gradient/444/p2/d4 | 0.960 | 0.679 | 1.413 | 0.6 % | 0.2 % |
| smooth-gradient/420/p0/d1 | 0.438 | 0.294 | 1.490 | 0.9 % | 0.2 % |
| smooth-gradient/420/p0/d4 | 0.432 | 0.299 | 1.446 | 0.9 % | 0.1 % |
| smooth-gradient/420/p2/d1 | 0.652 | 0.444 | 1.467 | 0.3 % | 0.1 % |
| smooth-gradient/420/p2/d4 | 0.630 | 0.440 | 1.431 | 0.3 % | 0.2 % |
| low-contrast/444/p0/d1 | 0.733 | 0.504 | 1.456 | 0.2 % | 0.6 % |
| low-contrast/444/p0/d4 | 0.641 | 0.390 | 1.645 | 0.5 % | 0.4 % |
| low-contrast/444/p2/d1 | 1.401 | 1.168 | 1.200 | 0.2 % | 0.2 % |
| low-contrast/444/p2/d4 | 1.122 | 0.801 | 1.401 | 1.0 % | 0.4 % |
| low-contrast/420/p0/d1 | 0.536 | 0.433 | 1.238 | 0.8 % | 0.1 % |
| low-contrast/420/p0/d4 | 0.461 | 0.316 | 1.457 | 0.2 % | 0.3 % |
| low-contrast/420/p2/d1 | 0.984 | 0.835 | 1.179 | 0.5 % | 0.3 % |
| low-contrast/420/p2/d4 | 0.733 | 0.525 | 1.395 | 0.6 % | 0.3 % |
| color-edges/444/p0/d1 | 0.792 | 0.559 | 1.415 | 0.5 % | 0.6 % |
| color-edges/444/p0/d4 | 0.727 | 0.487 | 1.491 | 0.5 % | 0.2 % |
| color-edges/444/p2/d1 | 1.692 | 1.361 | 1.243 | 1.1 % | 0.2 % |
| color-edges/444/p2/d4 | 1.474 | 1.176 | 1.253 | 0.9 % | 0.3 % |
| color-edges/420/p0/d1 | 0.605 | 0.558 | 1.085 | 0.5 % | 0.3 % |
| color-edges/420/p0/d4 | 0.546 | 0.453 | 1.205 | 0.4 % | 0.2 % |
| color-edges/420/p2/d1 | 1.294 | 1.131 | 1.144 | 0.5 % | 0.1 % |
| color-edges/420/p2/d4 | 1.058 | 0.929 | 1.138 | 0.5 % | 0.3 % |
| line-art/444/p0/d1 | 0.747 | 0.537 | 1.392 | 0.6 % | 0.3 % |
| line-art/444/p0/d4 | 0.710 | 0.485 | 1.464 | 0.6 % | 0.5 % |
| line-art/444/p2/d1 | 1.487 | 1.135 | 1.310 | 0.4 % | 0.3 % |
| line-art/444/p2/d4 | 1.353 | 1.127 | 1.200 | 0.5 % | 0.2 % |
| line-art/420/p0/d1 | 0.575 | 0.501 | 1.149 | 0.3 % | 0.3 % |
| line-art/420/p0/d4 | 0.533 | 0.421 | 1.266 | 0.6 % | 0.6 % |
| line-art/420/p2/d1 | 1.138 | 0.886 | 1.285 | 0.4 % | 0.2 % |
| line-art/420/p2/d4 | 0.980 | 0.836 | 1.172 | 0.4 % | 0.2 % |
| structured-texture/444/p0/d1 | 1.040 | 1.234 | 0.843 | 0.6 % | 0.2 % |
| structured-texture/444/p0/d4 | 0.817 | 0.705 | 1.158 | 0.7 % | 0.1 % |
| structured-texture/444/p2/d1 | 2.653 | 3.086 | 0.860 | 0.3 % | 0.1 % |
| structured-texture/444/p2/d4 | 1.672 | 1.653 | 1.012 | 0.1 % | 0.1 % |
| structured-texture/420/p0/d1 | 0.717 | 0.912 | 0.787 | 1.0 % | 0.1 % |
| structured-texture/420/p0/d4 | 0.562 | 0.529 | 1.062 | 0.8 % | 0.2 % |
| structured-texture/420/p2/d1 | 1.700 | 2.012 | 0.845 | 0.5 % | 0.2 % |
| structured-texture/420/p2/d4 | 1.086 | 1.069 | 1.016 | 0.8 % | 0.2 % |
| low-noise/444/p0/d1 | 0.808 | 0.721 | 1.121 | 1.0 % | 0.1 % |
| low-noise/444/p0/d4 | 0.615 | 0.374 | 1.646 | 0.5 % | 0.1 % |
| low-noise/444/p2/d1 | 1.462 | 1.628 | 0.898 | 0.4 % | 0.3 % |
| low-noise/444/p2/d4 | 1.014 | 0.728 | 1.394 | 0.3 % | 0.5 % |
| low-noise/420/p0/d1 | 0.558 | 0.559 | 0.998 | 0.8 % | 0.4 % |
| low-noise/420/p0/d4 | 0.440 | 0.295 | 1.492 | 0.6 % | 0.3 % |
| low-noise/420/p2/d1 | 0.970 | 1.072 | 0.905 | 0.4 % | 0.2 % |
| low-noise/420/p2/d4 | 0.658 | 0.461 | 1.428 | 0.2 % | 0.2 % |
| full-noise/444/p0/d1 | 1.152 | 1.363 | 0.845 | 0.6 % | 0.1 % |
| full-noise/444/p0/d4 | 0.897 | 1.019 | 0.880 | 0.6 % | 0.0 % |
| full-noise/444/p2/d1 | 3.216 | 3.422 | 0.940 | 0.2 % | 0.2 % |
| full-noise/444/p2/d4 | 1.985 | 2.409 | 0.824 | 1.1 % | 0.1 % |
| full-noise/420/p0/d1 | 0.764 | 0.994 | 0.769 | 0.2 % | 0.3 % |
| full-noise/420/p0/d4 | 0.616 | 0.738 | 0.834 | 0.7 % | 0.2 % |
| full-noise/420/p2/d1 | 2.063 | 2.036 | 1.014 | 0.2 % | 0.1 % |
| full-noise/420/p2/d4 | 1.289 | 1.527 | 0.844 | 0.5 % | 0.2 % |
| tiny-odd-edges/444/p0/d1 | 0.052 | 0.026 | 1.972 | 1.2 % | 0.8 % |
| tiny-odd-edges/444/p0/d4 | 0.050 | 0.031 | 1.598 | 0.5 % | 0.3 % |
| tiny-odd-edges/444/p2/d1 | 0.106 | 0.054 | 1.985 | 0.8 % | 0.7 % |
| tiny-odd-edges/444/p2/d4 | 0.105 | 0.055 | 1.925 | 1.4 % | 0.3 % |
| tiny-odd-edges/420/p0/d1 | 0.046 | 0.024 | 1.920 | 0.6 % | 0.9 % |
| tiny-odd-edges/420/p0/d4 | 0.047 | 0.029 | 1.608 | 1.4 % | 0.6 % |
| tiny-odd-edges/420/p2/d1 | 0.100 | 0.049 | 2.051 | 0.9 % | 0.3 % |
| tiny-odd-edges/420/p2/d4 | 0.100 | 0.050 | 1.999 | 0.5 % | 0.3 % |

Exact input/JPEG files, SHA256 identities, per-block raw samples, p10/p50/p90, relative MAD, allocations/collections, calibration counts and native command outputs are retained alongside this report. summary.json records CPU, affinity, OS, runtime, vector widths, policy, revision and group verdicts.

Failed bounds:

- kodim01/444/p0/d4: median latency ratio 1.213098 exceeds 1.10.
- kodim01/420/p0/d4: median latency ratio 1.243426 exceeds 1.10.
- kodim02/444/p0/d1: median latency ratio 1.128582 exceeds 1.10.
- kodim02/444/p0/d4: median latency ratio 1.342036 exceeds 1.10.
- kodim02/420/p0/d1: median latency ratio 1.115068 exceeds 1.10.
- kodim02/420/p0/d4: median latency ratio 1.390694 exceeds 1.10.
- kodim03/444/p0/d1: median latency ratio 1.195966 exceeds 1.10.
- kodim03/444/p0/d4: median latency ratio 1.340201 exceeds 1.10.
- kodim03/420/p0/d1: median latency ratio 1.201404 exceeds 1.10.
- kodim03/420/p0/d4: median latency ratio 1.391884 exceeds 1.10.
- kodim04/444/p0/d1: median latency ratio 1.138395 exceeds 1.10.
- kodim04/444/p0/d4: median latency ratio 1.353417 exceeds 1.10.
- kodim04/420/p0/d1: median latency ratio 1.125657 exceeds 1.10.
- kodim04/420/p0/d4: median latency ratio 1.396450 exceeds 1.10.
- kodim05/444/p0/d4: median latency ratio 1.185973 exceeds 1.10.
- kodim05/420/p0/d4: median latency ratio 1.214721 exceeds 1.10.
- kodim06/444/p0/d4: median latency ratio 1.236237 exceeds 1.10.
- kodim06/420/p0/d4: median latency ratio 1.265618 exceeds 1.10.
- kodim07/444/p0/d1: median latency ratio 1.188471 exceeds 1.10.
- kodim07/444/p0/d4: median latency ratio 1.312067 exceeds 1.10.
- kodim07/420/p0/d1: median latency ratio 1.182980 exceeds 1.10.
- kodim07/420/p0/d4: median latency ratio 1.362624 exceeds 1.10.
- kodim08/444/p0/d4: median latency ratio 1.193334 exceeds 1.10.
- kodim08/420/p0/d4: median latency ratio 1.231936 exceeds 1.10.
- kodim09/444/p0/d1: median latency ratio 1.227592 exceeds 1.10.
- kodim09/444/p0/d4: median latency ratio 1.357409 exceeds 1.10.
- kodim09/420/p0/d1: median latency ratio 1.227878 exceeds 1.10.
- kodim09/420/p0/d4: median latency ratio 1.399266 exceeds 1.10.
- kodim10/444/p0/d1: median latency ratio 1.197823 exceeds 1.10.
- kodim10/444/p0/d4: median latency ratio 1.355739 exceeds 1.10.
- kodim10/420/p0/d1: median latency ratio 1.193106 exceeds 1.10.
- kodim10/420/p0/d4: median latency ratio 1.399215 exceeds 1.10.
- kodim11/444/p0/d4: median latency ratio 1.274699 exceeds 1.10.
- kodim11/420/p0/d4: median latency ratio 1.312765 exceeds 1.10.
- kodim12/444/p0/d1: median latency ratio 1.143167 exceeds 1.10.
- kodim12/444/p0/d4: median latency ratio 1.346888 exceeds 1.10.
- kodim12/420/p0/d1: median latency ratio 1.195464 exceeds 1.10.
- kodim12/420/p0/d4: median latency ratio 1.415772 exceeds 1.10.
- kodim13/444/p0/d4: median latency ratio 1.181615 exceeds 1.10.
- kodim13/420/p0/d4: median latency ratio 1.165809 exceeds 1.10.
- kodim14/444/p0/d4: median latency ratio 1.229217 exceeds 1.10.
- kodim14/420/p0/d4: median latency ratio 1.263682 exceeds 1.10.
- kodim15/444/p0/d1: median latency ratio 1.165318 exceeds 1.10.
- kodim15/444/p0/d4: median latency ratio 1.332504 exceeds 1.10.
- kodim15/420/p0/d1: median latency ratio 1.148719 exceeds 1.10.
- kodim15/420/p0/d4: median latency ratio 1.392391 exceeds 1.10.
- kodim16/444/p0/d1: median latency ratio 1.103862 exceeds 1.10.
- kodim16/444/p0/d4: median latency ratio 1.293322 exceeds 1.10.
- kodim16/420/p0/d1: median latency ratio 1.101164 exceeds 1.10.
- kodim16/420/p0/d4: median latency ratio 1.343427 exceeds 1.10.
- kodim17/444/p0/d1: median latency ratio 1.159646 exceeds 1.10.
- kodim17/444/p0/d4: median latency ratio 1.336901 exceeds 1.10.
- kodim17/420/p0/d1: median latency ratio 1.153021 exceeds 1.10.
- kodim17/420/p0/d4: median latency ratio 1.379296 exceeds 1.10.
- kodim18/444/p0/d4: median latency ratio 1.257864 exceeds 1.10.
- kodim18/420/p0/d4: median latency ratio 1.284366 exceeds 1.10.
- kodim19/444/p0/d1: median latency ratio 1.124416 exceeds 1.10.
- kodim19/444/p0/d4: median latency ratio 1.311682 exceeds 1.10.
- kodim19/420/p0/d1: median latency ratio 1.109108 exceeds 1.10.
- kodim19/420/p0/d4: median latency ratio 1.360569 exceeds 1.10.
- kodim20/444/p0/d1: median latency ratio 1.172856 exceeds 1.10.
- kodim20/444/p0/d4: median latency ratio 1.336740 exceeds 1.10.
- kodim20/420/p0/d1: median latency ratio 1.522253 exceeds 1.10.
- kodim20/420/p0/d4: median latency ratio 1.390998 exceeds 1.10.
- kodim21/444/p0/d1: median latency ratio 1.108866 exceeds 1.10.
- kodim21/444/p0/d4: median latency ratio 1.260163 exceeds 1.10.
- kodim21/420/p0/d4: median latency ratio 1.307081 exceeds 1.10.
- kodim22/444/p0/d4: median latency ratio 1.281938 exceeds 1.10.
- kodim22/420/p0/d4: median latency ratio 1.337995 exceeds 1.10.
- kodim23/444/p0/d1: median latency ratio 1.244908 exceeds 1.10.
- kodim23/444/p0/d4: median latency ratio 1.365634 exceeds 1.10.
- kodim23/420/p0/d1: median latency ratio 1.260306 exceeds 1.10.
- kodim23/420/p0/d4: median latency ratio 1.421891 exceeds 1.10.
- kodim24/444/p0/d4: median latency ratio 1.227059 exceeds 1.10.
- kodim24/420/p0/d4: median latency ratio 1.265807 exceeds 1.10.
- smooth-gradient/444/p0/d1: median latency ratio 1.652862 exceeds 1.10.
- smooth-gradient/444/p0/d4: median latency ratio 1.602442 exceeds 1.10.
- smooth-gradient/444/p2/d1: median latency ratio 1.442750 exceeds 1.10.
- smooth-gradient/444/p2/d4: median latency ratio 1.413207 exceeds 1.10.
- smooth-gradient/420/p0/d1: median latency ratio 1.490485 exceeds 1.10.
- smooth-gradient/420/p0/d4: median latency ratio 1.445861 exceeds 1.10.
- smooth-gradient/420/p2/d1: median latency ratio 1.466774 exceeds 1.10.
- smooth-gradient/420/p2/d4: median latency ratio 1.430937 exceeds 1.10.
- low-contrast/444/p0/d1: median latency ratio 1.456179 exceeds 1.10.
- low-contrast/444/p0/d4: median latency ratio 1.645427 exceeds 1.10.
- low-contrast/444/p2/d1: median latency ratio 1.200280 exceeds 1.10.
- low-contrast/444/p2/d4: median latency ratio 1.401271 exceeds 1.10.
- low-contrast/420/p0/d1: median latency ratio 1.238381 exceeds 1.10.
- low-contrast/420/p0/d4: median latency ratio 1.456886 exceeds 1.10.
- low-contrast/420/p2/d1: median latency ratio 1.179371 exceeds 1.10.
- low-contrast/420/p2/d4: median latency ratio 1.394747 exceeds 1.10.
- color-edges/444/p0/d1: median latency ratio 1.415332 exceeds 1.10.
- color-edges/444/p0/d4: median latency ratio 1.491477 exceeds 1.10.
- color-edges/444/p2/d1: median latency ratio 1.242854 exceeds 1.10.
- color-edges/444/p2/d4: median latency ratio 1.252746 exceeds 1.10.
- color-edges/420/p0/d4: median latency ratio 1.204864 exceeds 1.10.
- color-edges/420/p2/d1: median latency ratio 1.143951 exceeds 1.10.
- color-edges/420/p2/d4: median latency ratio 1.138082 exceeds 1.10.
- line-art/444/p0/d1: median latency ratio 1.392438 exceeds 1.10.
- line-art/444/p0/d4: median latency ratio 1.464015 exceeds 1.10.
- line-art/444/p2/d1: median latency ratio 1.310266 exceeds 1.10.
- line-art/444/p2/d4: median latency ratio 1.200115 exceeds 1.10.
- line-art/420/p0/d1: median latency ratio 1.148899 exceeds 1.10.
- line-art/420/p0/d4: median latency ratio 1.266220 exceeds 1.10.
- line-art/420/p2/d1: median latency ratio 1.285059 exceeds 1.10.
- line-art/420/p2/d4: median latency ratio 1.172418 exceeds 1.10.
- structured-texture/444/p0/d4: median latency ratio 1.157893 exceeds 1.10.
- low-noise/444/p0/d1: median latency ratio 1.120948 exceeds 1.10.
- low-noise/444/p0/d4: median latency ratio 1.645832 exceeds 1.10.
- low-noise/444/p2/d4: median latency ratio 1.393677 exceeds 1.10.
- low-noise/420/p0/d4: median latency ratio 1.491731 exceeds 1.10.
- low-noise/420/p2/d4: median latency ratio 1.427721 exceeds 1.10.
- tiny-odd-edges/444/p0/d1: median latency ratio 1.971837 exceeds 1.10.
- tiny-odd-edges/444/p0/d4: median latency ratio 1.598012 exceeds 1.10.
- tiny-odd-edges/444/p2/d1: median latency ratio 1.985063 exceeds 1.10.
- tiny-odd-edges/444/p2/d4: median latency ratio 1.925328 exceeds 1.10.
- tiny-odd-edges/420/p0/d1: median latency ratio 1.919805 exceeds 1.10.
- tiny-odd-edges/420/p0/d4: median latency ratio 1.607697 exceeds 1.10.
- tiny-odd-edges/420/p2/d1: median latency ratio 2.050586 exceeds 1.10.
- tiny-odd-edges/420/p2/d4: median latency ratio 1.999082 exceeds 1.10.
- natural/444/p0: geometric mean latency ratio 1.193851 exceeds 1.05.
- natural/420/p0: geometric mean latency ratio 1.200947 exceeds 1.05.
- controlled/444/p0: geometric mean latency ratio 1.345375 exceeds 1.05.
- controlled/444/p2: geometric mean latency ratio 1.228840 exceeds 1.05.
- controlled/420/p0: geometric mean latency ratio 1.198971 exceeds 1.05.
- controlled/420/p2: geometric mean latency ratio 1.226836 exceeds 1.05.
