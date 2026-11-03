# Performance parity: FAILED

256/256 cases: RGB8 YCbCr, Kodak24 plus eight fixed controls, distances1/4, 444/420 and progression0/2, one worker on this CI host. Pinned upstream `031a0077f5799a6041004267fc12b956c1f52a20`.
Tested revision: `f52961be35384adfe99b078a47ec0f16b21633e4`.

All cases are measured even when a latency bound fails, so the report identifies every measured regression. A passing verdict requires complete, valid coverage and all fixed bounds.

Fixed latency noninferiority limits: geometric mean managed/native median latency ratio <=1.05 in each corpus/sampling/progression group; every case <=1.10. A faster case cannot excuse a slower case. No universal performance claim is made.

Seven alternating order blocks, five untimed warmups before each block. The native-only three-iteration calibration targets50ms and selects a shared count clamped3..100. Medians use all seven blocks; no outliers are discarded. Complete in-memory encode calls are timed individually and their elapsed times summed; subprocess startup, file I/O, verification and native buffer free are excluded. Managed collections inside encode are included. Tiered compilation is disabled.

Evidence is invalid if a block interval is below1ms, any case has relative MAD >10%, inputs/settings/coverage differ, or an untimed JPEG structure comparison fails. Invalid evidence cannot establish parity.

| Corpus | Sampling | Progression | Cases | Geometric mean ratio | Worst ratio | Verdict |
| --- | --- | --- | ---: | ---: | ---: | --- |
| natural | 444 | 0 | 48 | 1.161249 | 1.366639 | FAIL |
| natural | 444 | 2 | 48 | 0.802359 | 0.869357 | PASS |
| natural | 420 | 0 | 48 | 1.134877 | 1.360784 | FAIL |
| natural | 420 | 2 | 48 | 0.798386 | 0.900315 | PASS |
| controlled | 444 | 0 | 16 | 1.195753 | 1.502862 | FAIL |
| controlled | 444 | 2 | 16 | 0.990728 | 1.394145 | FAIL |
| controlled | 420 | 0 | 16 | 1.122608 | 1.502642 | FAIL |
| controlled | 420 | 2 | 16 | 0.988898 | 1.426379 | FAIL |

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 6.301 | 5.256 | 1.199 | 2.2 % | 0.0 % |
| kodim01/444/p0/d4 | 3.426 | 2.848 | 1.203 | 2.2 % | 0.2 % |
| kodim01/444/p2/d1 | 8.580 | 11.518 | 0.745 | 1.4 % | 2.3 % |
| kodim01/444/p2/d4 | 6.095 | 7.656 | 0.796 | 0.5 % | 0.4 % |
| kodim01/420/p0/d1 | 3.079 | 3.291 | 0.936 | 0.2 % | 0.2 % |
| kodim01/420/p0/d4 | 2.427 | 2.038 | 1.191 | 0.3 % | 0.1 % |
| kodim01/420/p2/d1 | 6.367 | 8.701 | 0.732 | 1.0 % | 0.2 % |
| kodim01/420/p2/d4 | 4.124 | 5.111 | 0.807 | 0.4 % | 0.6 % |
| kodim02/444/p0/d1 | 3.785 | 3.513 | 1.077 | 0.3 % | 0.5 % |
| kodim02/444/p0/d4 | 3.152 | 2.428 | 1.298 | 0.2 % | 0.1 % |
| kodim02/444/p2/d1 | 7.081 | 8.984 | 0.788 | 0.5 % | 0.4 % |
| kodim02/444/p2/d4 | 5.260 | 6.173 | 0.852 | 0.9 % | 0.5 % |
| kodim02/420/p0/d1 | 2.753 | 2.606 | 1.056 | 0.4 % | 0.3 % |
| kodim02/420/p0/d4 | 2.220 | 1.687 | 1.315 | 0.3 % | 0.2 % |
| kodim02/420/p2/d1 | 4.891 | 6.625 | 0.738 | 0.3 % | 0.7 % |
| kodim02/420/p2/d4 | 3.395 | 3.931 | 0.864 | 0.3 % | 0.4 % |
| kodim03/444/p0/d1 | 3.365 | 2.947 | 1.142 | 0.2 % | 0.5 % |
| kodim03/444/p0/d4 | 3.037 | 2.331 | 1.303 | 0.5 % | 0.1 % |
| kodim03/444/p2/d1 | 6.164 | 7.579 | 0.813 | 0.8 % | 0.4 % |
| kodim03/444/p2/d4 | 5.027 | 5.983 | 0.840 | 0.2 % | 0.8 % |
| kodim03/420/p0/d1 | 2.454 | 2.166 | 1.133 | 0.4 % | 0.2 % |
| kodim03/420/p0/d4 | 2.155 | 1.622 | 1.328 | 0.1 % | 0.4 % |
| kodim03/420/p2/d1 | 4.293 | 5.500 | 0.781 | 0.2 % | 0.6 % |
| kodim03/420/p2/d4 | 3.282 | 3.835 | 0.856 | 0.3 % | 0.5 % |
| kodim04/444/p0/d1 | 3.739 | 3.408 | 1.097 | 0.7 % | 0.3 % |
| kodim04/444/p0/d4 | 3.190 | 2.447 | 1.303 | 0.3 % | 0.1 % |
| kodim04/444/p2/d1 | 6.883 | 8.849 | 0.778 | 0.8 % | 0.7 % |
| kodim04/444/p2/d4 | 5.300 | 6.236 | 0.850 | 0.5 % | 0.5 % |
| kodim04/420/p0/d1 | 2.715 | 2.556 | 1.062 | 0.7 % | 0.0 % |
| kodim04/420/p0/d4 | 2.249 | 1.720 | 1.307 | 0.4 % | 0.3 % |
| kodim04/420/p2/d1 | 4.856 | 6.285 | 0.773 | 0.3 % | 0.2 % |
| kodim04/420/p2/d4 | 3.491 | 3.903 | 0.894 | 0.6 % | 0.5 % |
| kodim05/444/p0/d1 | 4.039 | 4.117 | 0.981 | 0.2 % | 0.2 % |
| kodim05/444/p0/d4 | 3.391 | 2.958 | 1.146 | 0.3 % | 0.4 % |
| kodim05/444/p2/d1 | 8.610 | 10.960 | 0.786 | 0.4 % | 0.5 % |
| kodim05/444/p2/d4 | 6.251 | 7.764 | 0.805 | 0.3 % | 0.1 % |
| kodim05/420/p0/d1 | 3.056 | 3.814 | 0.801 | 0.9 % | 0.3 % |
| kodim05/420/p0/d4 | 2.509 | 2.181 | 1.150 | 0.5 % | 0.1 % |
| kodim05/420/p2/d1 | 6.461 | 8.709 | 0.742 | 0.5 % | 0.2 % |
| kodim05/420/p2/d4 | 4.386 | 5.532 | 0.793 | 0.5 % | 0.2 % |
| kodim06/444/p0/d1 | 3.748 | 3.647 | 1.027 | 1.3 % | 0.2 % |
| kodim06/444/p0/d4 | 3.150 | 2.627 | 1.199 | 0.5 % | 0.1 % |
| kodim06/444/p2/d1 | 7.381 | 9.711 | 0.760 | 0.8 % | 0.6 % |
| kodim06/444/p2/d4 | 5.593 | 6.973 | 0.802 | 0.4 % | 0.5 % |
| kodim06/420/p0/d1 | 2.769 | 2.821 | 0.982 | 0.4 % | 0.3 % |
| kodim06/420/p0/d4 | 2.261 | 1.883 | 1.201 | 0.3 % | 0.2 % |
| kodim06/420/p2/d1 | 5.423 | 7.324 | 0.741 | 0.1 % | 0.2 % |
| kodim06/420/p2/d4 | 3.713 | 4.616 | 0.804 | 0.3 % | 0.5 % |
| kodim07/444/p0/d1 | 3.505 | 3.091 | 1.134 | 0.4 % | 0.3 % |
| kodim07/444/p0/d4 | 3.103 | 2.462 | 1.260 | 0.2 % | 0.6 % |
| kodim07/444/p2/d1 | 6.582 | 8.038 | 0.819 | 0.1 % | 0.4 % |
| kodim07/444/p2/d4 | 5.361 | 6.360 | 0.843 | 0.5 % | 0.8 % |
| kodim07/420/p0/d1 | 2.553 | 2.276 | 1.122 | 0.5 % | 0.1 % |
| kodim07/420/p0/d4 | 2.229 | 1.724 | 1.293 | 0.6 % | 0.2 % |
| kodim07/420/p2/d1 | 4.675 | 5.843 | 0.800 | 0.4 % | 0.2 % |
| kodim07/420/p2/d4 | 3.566 | 4.216 | 0.846 | 0.5 % | 0.5 % |
| kodim08/444/p0/d1 | 4.049 | 4.185 | 0.968 | 0.2 % | 0.2 % |
| kodim08/444/p0/d4 | 3.378 | 2.935 | 1.151 | 0.2 % | 0.1 % |
| kodim08/444/p2/d1 | 8.450 | 11.123 | 0.760 | 0.4 % | 0.2 % |
| kodim08/444/p2/d4 | 6.257 | 7.751 | 0.807 | 0.7 % | 0.4 % |
| kodim08/420/p0/d1 | 3.065 | 3.786 | 0.810 | 0.2 % | 0.4 % |
| kodim08/420/p0/d4 | 2.504 | 2.140 | 1.170 | 0.4 % | 0.2 % |
| kodim08/420/p2/d1 | 6.449 | 8.785 | 0.734 | 0.4 % | 0.7 % |
| kodim08/420/p2/d4 | 4.361 | 5.449 | 0.800 | 0.4 % | 0.4 % |
| kodim09/444/p0/d1 | 3.485 | 2.977 | 1.171 | 0.2 % | 0.2 % |
| kodim09/444/p0/d4 | 3.156 | 2.342 | 1.348 | 1.7 % | 0.3 % |
| kodim09/444/p2/d1 | 6.251 | 7.789 | 0.803 | 0.3 % | 0.7 % |
| kodim09/444/p2/d4 | 5.181 | 6.158 | 0.841 | 0.6 % | 0.4 % |
| kodim09/420/p0/d1 | 2.518 | 2.178 | 1.156 | 0.3 % | 0.3 % |
| kodim09/420/p0/d4 | 2.140 | 1.619 | 1.321 | 0.2 % | 0.2 % |
| kodim09/420/p2/d1 | 4.345 | 5.308 | 0.819 | 0.4 % | 0.2 % |
| kodim09/420/p2/d4 | 3.298 | 3.738 | 0.882 | 0.3 % | 0.8 % |
| kodim10/444/p0/d1 | 3.566 | 3.131 | 1.139 | 0.1 % | 0.2 % |
| kodim10/444/p0/d4 | 3.133 | 2.408 | 1.301 | 0.2 % | 0.1 % |
| kodim10/444/p2/d1 | 6.627 | 8.240 | 0.804 | 1.3 % | 0.7 % |
| kodim10/444/p2/d4 | 5.297 | 6.282 | 0.843 | 0.6 % | 0.5 % |
| kodim10/420/p0/d1 | 2.637 | 2.319 | 1.137 | 0.8 % | 0.2 % |
| kodim10/420/p0/d4 | 2.217 | 1.685 | 1.315 | 0.5 % | 0.3 % |
| kodim10/420/p2/d1 | 4.612 | 5.665 | 0.814 | 0.1 % | 0.5 % |
| kodim10/420/p2/d4 | 3.486 | 3.872 | 0.900 | 0.5 % | 0.3 % |
| kodim11/444/p0/d1 | 3.725 | 3.639 | 1.024 | 0.2 % | 0.1 % |
| kodim11/444/p0/d4 | 3.187 | 2.561 | 1.245 | 1.1 % | 0.0 % |
| kodim11/444/p2/d1 | 7.341 | 9.597 | 0.765 | 0.8 % | 0.6 % |
| kodim11/444/p2/d4 | 5.519 | 6.794 | 0.812 | 0.9 % | 0.9 % |
| kodim11/420/p0/d1 | 2.791 | 2.778 | 1.005 | 0.3 % | 0.2 % |
| kodim11/420/p0/d4 | 2.278 | 1.811 | 1.258 | 0.2 % | 0.2 % |
| kodim11/420/p2/d1 | 5.367 | 7.197 | 0.746 | 0.4 % | 0.2 % |
| kodim11/420/p2/d4 | 3.663 | 4.462 | 0.821 | 0.4 % | 0.6 % |
| kodim12/444/p0/d1 | 3.474 | 3.154 | 1.102 | 0.2 % | 0.1 % |
| kodim12/444/p0/d4 | 3.039 | 2.350 | 1.293 | 0.3 % | 0.3 % |
| kodim12/444/p2/d1 | 6.418 | 8.235 | 0.779 | 0.4 % | 0.7 % |
| kodim12/444/p2/d4 | 5.085 | 6.031 | 0.843 | 0.3 % | 0.6 % |
| kodim12/420/p0/d1 | 2.588 | 2.335 | 1.108 | 0.1 % | 0.5 % |
| kodim12/420/p0/d4 | 2.143 | 1.638 | 1.308 | 0.1 % | 0.6 % |
| kodim12/420/p2/d1 | 4.454 | 5.799 | 0.768 | 0.1 % | 0.2 % |
| kodim12/420/p2/d4 | 3.256 | 3.853 | 0.845 | 0.1 % | 0.9 % |
| kodim13/444/p0/d1 | 4.263 | 4.592 | 0.928 | 0.4 % | 0.1 % |
| kodim13/444/p0/d4 | 3.407 | 3.080 | 1.106 | 0.3 % | 0.1 % |
| kodim13/444/p2/d1 | 9.226 | 12.217 | 0.755 | 0.3 % | 0.9 % |
| kodim13/444/p2/d4 | 6.407 | 8.348 | 0.767 | 1.1 % | 0.8 % |
| kodim13/420/p0/d1 | 3.258 | 4.278 | 0.762 | 0.5 % | 0.3 % |
| kodim13/420/p0/d4 | 2.513 | 2.243 | 1.121 | 1.2 % | 0.1 % |
| kodim13/420/p2/d1 | 7.118 | 9.785 | 0.727 | 0.6 % | 0.5 % |
| kodim13/420/p2/d4 | 4.339 | 5.754 | 0.754 | 0.2 % | 0.4 % |
| kodim14/444/p0/d1 | 3.980 | 4.013 | 0.992 | 0.2 % | 0.2 % |
| kodim14/444/p0/d4 | 3.385 | 2.778 | 1.219 | 0.4 % | 0.1 % |
| kodim14/444/p2/d1 | 8.186 | 10.818 | 0.757 | 0.5 % | 0.3 % |
| kodim14/444/p2/d4 | 5.873 | 7.332 | 0.801 | 0.2 % | 0.8 % |
| kodim14/420/p0/d1 | 3.021 | 3.123 | 0.967 | 0.3 % | 0.0 % |
| kodim14/420/p0/d4 | 2.371 | 1.981 | 1.197 | 0.3 % | 0.2 % |
| kodim14/420/p2/d1 | 5.984 | 8.252 | 0.725 | 0.1 % | 0.4 % |
| kodim14/420/p2/d4 | 3.936 | 4.948 | 0.796 | 0.1 % | 0.3 % |
| kodim15/444/p0/d1 | 3.679 | 3.226 | 1.140 | 1.0 % | 0.2 % |
| kodim15/444/p0/d4 | 3.201 | 2.398 | 1.335 | 0.3 % | 0.2 % |
| kodim15/444/p2/d1 | 6.600 | 8.307 | 0.795 | 1.3 % | 0.5 % |
| kodim15/444/p2/d4 | 5.200 | 6.142 | 0.847 | 0.3 % | 0.3 % |
| kodim15/420/p0/d1 | 2.577 | 2.368 | 1.088 | 0.3 % | 0.2 % |
| kodim15/420/p0/d4 | 2.204 | 1.664 | 1.325 | 0.5 % | 0.2 % |
| kodim15/420/p2/d1 | 4.582 | 5.981 | 0.766 | 0.2 % | 0.5 % |
| kodim15/420/p2/d4 | 3.371 | 3.947 | 0.854 | 0.4 % | 0.2 % |
| kodim16/444/p0/d1 | 3.655 | 3.286 | 1.112 | 0.2 % | 0.1 % |
| kodim16/444/p0/d4 | 3.047 | 2.427 | 1.255 | 0.2 % | 0.1 % |
| kodim16/444/p2/d1 | 6.785 | 8.709 | 0.779 | 1.1 % | 0.8 % |
| kodim16/444/p2/d4 | 5.230 | 6.264 | 0.835 | 0.9 % | 0.5 % |
| kodim16/420/p0/d1 | 2.592 | 2.483 | 1.044 | 0.2 % | 0.4 % |
| kodim16/420/p0/d4 | 2.174 | 1.696 | 1.282 | 0.3 % | 0.1 % |
| kodim16/420/p2/d1 | 4.801 | 6.381 | 0.752 | 0.5 % | 0.7 % |
| kodim16/420/p2/d4 | 3.381 | 4.081 | 0.829 | 0.3 % | 0.7 % |
| kodim17/444/p0/d1 | 3.560 | 3.194 | 1.115 | 0.4 % | 0.1 % |
| kodim17/444/p0/d4 | 3.132 | 2.424 | 1.292 | 0.2 % | 0.1 % |
| kodim17/444/p2/d1 | 6.778 | 8.560 | 0.792 | 0.7 % | 1.1 % |
| kodim17/444/p2/d4 | 5.339 | 6.380 | 0.837 | 0.6 % | 0.8 % |
| kodim17/420/p0/d1 | 2.609 | 2.405 | 1.085 | 0.4 % | 0.1 % |
| kodim17/420/p0/d4 | 2.281 | 1.708 | 1.336 | 0.6 % | 0.3 % |
| kodim17/420/p2/d1 | 4.700 | 5.873 | 0.800 | 0.2 % | 0.7 % |
| kodim17/420/p2/d4 | 3.484 | 3.912 | 0.891 | 0.2 % | 0.3 % |
| kodim18/444/p0/d1 | 3.982 | 3.932 | 1.013 | 0.2 % | 0.3 % |
| kodim18/444/p0/d4 | 3.442 | 2.733 | 1.259 | 1.0 % | 0.2 % |
| kodim18/444/p2/d1 | 7.966 | 10.306 | 0.773 | 0.2 % | 1.0 % |
| kodim18/444/p2/d4 | 5.850 | 7.300 | 0.801 | 0.6 % | 0.7 % |
| kodim18/420/p0/d1 | 3.011 | 3.059 | 0.984 | 0.4 % | 0.1 % |
| kodim18/420/p0/d4 | 2.388 | 1.976 | 1.208 | 0.2 % | 0.1 % |
| kodim18/420/p2/d1 | 5.891 | 7.760 | 0.759 | 0.1 % | 0.5 % |
| kodim18/420/p2/d4 | 3.961 | 4.683 | 0.846 | 0.2 % | 0.2 % |
| kodim19/444/p0/d1 | 3.719 | 3.466 | 1.073 | 0.4 % | 0.1 % |
| kodim19/444/p0/d4 | 3.277 | 2.496 | 1.313 | 0.5 % | 0.2 % |
| kodim19/444/p2/d1 | 7.059 | 9.149 | 0.771 | 0.3 % | 0.8 % |
| kodim19/444/p2/d4 | 5.399 | 6.493 | 0.831 | 0.4 % | 0.2 % |
| kodim19/420/p0/d1 | 2.766 | 2.630 | 1.052 | 0.2 % | 0.2 % |
| kodim19/420/p0/d4 | 2.285 | 1.765 | 1.295 | 0.1 % | 0.1 % |
| kodim19/420/p2/d1 | 5.072 | 6.600 | 0.769 | 0.2 % | 0.1 % |
| kodim19/420/p2/d4 | 3.591 | 4.060 | 0.884 | 0.1 % | 0.5 % |
| kodim20/444/p0/d1 | 3.319 | 2.963 | 1.120 | 0.5 % | 0.1 % |
| kodim20/444/p0/d4 | 2.978 | 2.291 | 1.300 | 0.8 % | 0.1 % |
| kodim20/444/p2/d1 | 6.102 | 7.729 | 0.789 | 0.3 % | 0.3 % |
| kodim20/444/p2/d4 | 5.072 | 6.045 | 0.839 | 0.4 % | 0.4 % |
| kodim20/420/p0/d1 | 2.465 | 2.175 | 1.134 | 0.2 % | 0.2 % |
| kodim20/420/p0/d4 | 2.087 | 1.573 | 1.327 | 0.2 % | 0.5 % |
| kodim20/420/p2/d1 | 4.235 | 5.611 | 0.755 | 0.3 % | 0.3 % |
| kodim20/420/p2/d4 | 3.227 | 3.879 | 0.832 | 0.5 % | 0.5 % |
| kodim21/444/p0/d1 | 3.789 | 3.469 | 1.092 | 0.1 % | 0.2 % |
| kodim21/444/p0/d4 | 3.090 | 2.566 | 1.205 | 0.1 % | 1.2 % |
| kodim21/444/p2/d1 | 6.988 | 8.913 | 0.784 | 0.4 % | 0.4 % |
| kodim21/444/p2/d4 | 5.377 | 6.667 | 0.807 | 0.5 % | 0.1 % |
| kodim21/420/p0/d1 | 2.721 | 2.629 | 1.035 | 0.7 % | 0.4 % |
| kodim21/420/p0/d4 | 2.266 | 1.788 | 1.268 | 0.7 % | 0.2 % |
| kodim21/420/p2/d1 | 5.064 | 6.644 | 0.762 | 0.2 % | 0.2 % |
| kodim21/420/p2/d4 | 3.571 | 4.369 | 0.817 | 0.3 % | 0.2 % |
| kodim22/444/p0/d1 | 3.958 | 3.735 | 1.060 | 0.3 % | 0.1 % |
| kodim22/444/p0/d4 | 3.187 | 2.583 | 1.234 | 0.1 % | 0.5 % |
| kodim22/444/p2/d1 | 7.436 | 9.728 | 0.764 | 0.7 % | 0.6 % |
| kodim22/444/p2/d4 | 5.497 | 6.781 | 0.811 | 0.4 % | 0.6 % |
| kodim22/420/p0/d1 | 2.791 | 2.808 | 0.994 | 0.0 % | 0.2 % |
| kodim22/420/p0/d4 | 2.330 | 1.831 | 1.273 | 0.3 % | 0.4 % |
| kodim22/420/p2/d1 | 5.375 | 7.328 | 0.733 | 0.3 % | 0.7 % |
| kodim22/420/p2/d4 | 3.657 | 4.448 | 0.822 | 0.4 % | 0.9 % |
| kodim23/444/p0/d1 | 3.451 | 2.905 | 1.188 | 0.3 % | 0.1 % |
| kodim23/444/p0/d4 | 3.158 | 2.311 | 1.367 | 0.8 % | 0.4 % |
| kodim23/444/p2/d1 | 6.024 | 7.235 | 0.833 | 0.6 % | 0.6 % |
| kodim23/444/p2/d4 | 5.021 | 5.776 | 0.869 | 0.4 % | 0.3 % |
| kodim23/420/p0/d1 | 2.478 | 2.074 | 1.195 | 0.7 % | 0.6 % |
| kodim23/420/p0/d4 | 2.161 | 1.588 | 1.361 | 0.1 % | 0.3 % |
| kodim23/420/p2/d1 | 4.100 | 5.100 | 0.804 | 0.2 % | 0.5 % |
| kodim23/420/p2/d4 | 3.256 | 3.692 | 0.882 | 0.3 % | 0.6 % |
| kodim24/444/p0/d1 | 3.882 | 3.910 | 0.993 | 0.3 % | 0.4 % |
| kodim24/444/p0/d4 | 3.265 | 2.763 | 1.182 | 0.5 % | 0.1 % |
| kodim24/444/p2/d1 | 7.955 | 10.263 | 0.775 | 0.7 % | 0.4 % |
| kodim24/444/p2/d4 | 5.851 | 7.371 | 0.794 | 0.2 % | 0.9 % |
| kodim24/420/p0/d1 | 2.906 | 2.985 | 0.973 | 0.2 % | 0.4 % |
| kodim24/420/p0/d4 | 2.419 | 1.990 | 1.216 | 0.3 % | 0.2 % |
| kodim24/420/p2/d1 | 5.807 | 7.764 | 0.748 | 0.3 % | 0.3 % |
| kodim24/420/p2/d4 | 3.938 | 4.974 | 0.792 | 0.2 % | 0.2 % |
| smooth-gradient/444/p0/d1 | 0.427 | 0.284 | 1.503 | 0.8 % | 0.2 % |
| smooth-gradient/444/p0/d4 | 0.395 | 0.280 | 1.413 | 0.2 % | 0.1 % |
| smooth-gradient/444/p2/d1 | 0.652 | 0.564 | 1.156 | 0.2 % | 0.2 % |
| smooth-gradient/444/p2/d4 | 0.626 | 0.554 | 1.130 | 0.5 % | 0.2 % |
| smooth-gradient/420/p0/d1 | 0.299 | 0.216 | 1.383 | 0.9 % | 0.1 % |
| smooth-gradient/420/p0/d4 | 0.296 | 0.218 | 1.356 | 0.4 % | 0.7 % |
| smooth-gradient/420/p2/d1 | 0.413 | 0.347 | 1.192 | 0.5 % | 0.4 % |
| smooth-gradient/420/p2/d4 | 0.407 | 0.344 | 1.184 | 0.6 % | 0.3 % |
| low-contrast/444/p0/d1 | 0.508 | 0.367 | 1.386 | 0.9 % | 0.3 % |
| low-contrast/444/p0/d4 | 0.425 | 0.295 | 1.441 | 0.4 % | 0.1 % |
| low-contrast/444/p2/d1 | 0.990 | 1.008 | 0.982 | 0.8 % | 0.3 % |
| low-contrast/444/p2/d4 | 0.736 | 0.646 | 1.140 | 0.9 % | 0.8 % |
| low-contrast/420/p0/d1 | 0.365 | 0.302 | 1.208 | 0.4 % | 0.3 % |
| low-contrast/420/p0/d4 | 0.311 | 0.231 | 1.349 | 2.5 % | 0.3 % |
| low-contrast/420/p2/d1 | 0.667 | 0.701 | 0.951 | 2.5 % | 0.6 % |
| low-contrast/420/p2/d4 | 0.480 | 0.411 | 1.170 | 0.6 % | 0.5 % |
| color-edges/444/p0/d1 | 0.547 | 0.407 | 1.344 | 0.8 % | 0.7 % |
| color-edges/444/p0/d4 | 0.493 | 0.363 | 1.358 | 1.2 % | 0.2 % |
| color-edges/444/p2/d1 | 1.161 | 1.133 | 1.024 | 0.6 % | 0.6 % |
| color-edges/444/p2/d4 | 0.976 | 0.952 | 1.025 | 0.6 % | 0.7 % |
| color-edges/420/p0/d1 | 0.410 | 0.356 | 1.151 | 1.1 % | 0.5 % |
| color-edges/420/p0/d4 | 0.362 | 0.310 | 1.167 | 0.8 % | 0.3 % |
| color-edges/420/p2/d1 | 0.903 | 0.955 | 0.946 | 0.1 % | 0.5 % |
| color-edges/420/p2/d4 | 0.710 | 0.780 | 0.911 | 1.2 % | 0.3 % |
| line-art/444/p0/d1 | 0.493 | 0.362 | 1.362 | 0.7 % | 0.4 % |
| line-art/444/p0/d4 | 0.463 | 0.346 | 1.339 | 1.4 % | 0.1 % |
| line-art/444/p2/d1 | 0.994 | 0.929 | 1.070 | 0.8 % | 0.5 % |
| line-art/444/p2/d4 | 0.896 | 0.938 | 0.955 | 0.7 % | 0.4 % |
| line-art/420/p0/d1 | 0.377 | 0.332 | 1.134 | 0.7 % | 0.8 % |
| line-art/420/p0/d4 | 0.343 | 0.296 | 1.156 | 1.2 % | 0.5 % |
| line-art/420/p2/d1 | 0.767 | 0.711 | 1.079 | 0.7 % | 0.3 % |
| line-art/420/p2/d4 | 0.643 | 0.695 | 0.926 | 0.7 % | 0.3 % |
| structured-texture/444/p0/d1 | 0.785 | 1.037 | 0.757 | 0.5 % | 0.1 % |
| structured-texture/444/p0/d4 | 0.576 | 0.522 | 1.103 | 0.8 % | 1.1 % |
| structured-texture/444/p2/d1 | 2.076 | 2.685 | 0.773 | 0.2 % | 0.2 % |
| structured-texture/444/p2/d4 | 1.234 | 1.475 | 0.837 | 0.4 % | 0.3 % |
| structured-texture/420/p0/d1 | 0.512 | 0.753 | 0.680 | 0.9 % | 0.2 % |
| structured-texture/420/p0/d4 | 0.374 | 0.324 | 1.154 | 0.7 % | 0.3 % |
| structured-texture/420/p2/d1 | 1.311 | 1.725 | 0.760 | 0.2 % | 0.2 % |
| structured-texture/420/p2/d4 | 0.774 | 0.942 | 0.821 | 0.6 % | 0.2 % |
| low-noise/444/p0/d1 | 0.572 | 0.562 | 1.018 | 0.6 % | 0.4 % |
| low-noise/444/p0/d4 | 0.406 | 0.283 | 1.435 | 0.6 % | 0.2 % |
| low-noise/444/p2/d1 | 1.039 | 1.458 | 0.712 | 0.8 % | 0.4 % |
| low-noise/444/p2/d4 | 0.655 | 0.592 | 1.105 | 0.9 % | 0.2 % |
| low-noise/420/p0/d1 | 0.382 | 0.328 | 1.166 | 1.4 % | 0.2 % |
| low-noise/420/p0/d4 | 0.286 | 0.213 | 1.343 | 1.1 % | 0.7 % |
| low-noise/420/p2/d1 | 0.655 | 0.964 | 0.680 | 1.1 % | 0.1 % |
| low-noise/420/p2/d4 | 0.421 | 0.353 | 1.194 | 1.4 % | 0.2 % |
| full-noise/444/p0/d1 | 0.881 | 1.127 | 0.782 | 0.7 % | 0.1 % |
| full-noise/444/p0/d4 | 0.654 | 0.857 | 0.763 | 0.3 % | 0.1 % |
| full-noise/444/p2/d1 | 2.552 | 2.959 | 0.862 | 0.5 % | 0.1 % |
| full-noise/444/p2/d4 | 1.474 | 2.110 | 0.698 | 0.2 % | 0.1 % |
| full-noise/420/p0/d1 | 0.564 | 0.797 | 0.707 | 0.8 % | 0.2 % |
| full-noise/420/p0/d4 | 0.431 | 0.582 | 0.740 | 0.7 % | 0.3 % |
| full-noise/420/p2/d1 | 1.603 | 1.743 | 0.920 | 0.1 % | 0.2 % |
| full-noise/420/p2/d4 | 0.943 | 1.320 | 0.714 | 0.3 % | 0.1 % |
| tiny-odd-edges/444/p0/d1 | 0.028 | 0.019 | 1.432 | 0.7 % | 0.3 % |
| tiny-odd-edges/444/p0/d4 | 0.027 | 0.023 | 1.187 | 0.8 % | 0.3 % |
| tiny-odd-edges/444/p2/d1 | 0.054 | 0.039 | 1.394 | 3.5 % | 0.3 % |
| tiny-odd-edges/444/p2/d4 | 0.051 | 0.040 | 1.284 | 2.2 % | 0.4 % |
| tiny-odd-edges/420/p0/d1 | 0.027 | 0.018 | 1.503 | 2.2 % | 2.1 % |
| tiny-odd-edges/420/p0/d4 | 0.026 | 0.022 | 1.225 | 0.3 % | 0.4 % |
| tiny-odd-edges/420/p2/d1 | 0.051 | 0.036 | 1.426 | 4.4 % | 0.4 % |
| tiny-odd-edges/420/p2/d4 | 0.047 | 0.036 | 1.301 | 1.5 % | 0.3 % |

Exact input/JPEG files, SHA256 identities, per-block raw samples, p10/p50/p90, relative MAD, allocations/collections, calibration counts and native command outputs are retained alongside this report. summary.json records CPU, affinity, OS, runtime, vector widths, policy, revision and group verdicts.

Failed bounds:

- kodim01/444/p0/d1: median latency ratio 1.198809 exceeds 1.10.
- kodim01/444/p0/d4: median latency ratio 1.202653 exceeds 1.10.
- kodim01/420/p0/d4: median latency ratio 1.190688 exceeds 1.10.
- kodim02/444/p0/d4: median latency ratio 1.298426 exceeds 1.10.
- kodim02/420/p0/d4: median latency ratio 1.315490 exceeds 1.10.
- kodim03/444/p0/d1: median latency ratio 1.141873 exceeds 1.10.
- kodim03/444/p0/d4: median latency ratio 1.302663 exceeds 1.10.
- kodim03/420/p0/d1: median latency ratio 1.133013 exceeds 1.10.
- kodim03/420/p0/d4: median latency ratio 1.328474 exceeds 1.10.
- kodim04/444/p0/d4: median latency ratio 1.303486 exceeds 1.10.
- kodim04/420/p0/d4: median latency ratio 1.307281 exceeds 1.10.
- kodim05/444/p0/d4: median latency ratio 1.146396 exceeds 1.10.
- kodim05/420/p0/d4: median latency ratio 1.150277 exceeds 1.10.
- kodim06/444/p0/d4: median latency ratio 1.199178 exceeds 1.10.
- kodim06/420/p0/d4: median latency ratio 1.201205 exceeds 1.10.
- kodim07/444/p0/d1: median latency ratio 1.133807 exceeds 1.10.
- kodim07/444/p0/d4: median latency ratio 1.260193 exceeds 1.10.
- kodim07/420/p0/d1: median latency ratio 1.121709 exceeds 1.10.
- kodim07/420/p0/d4: median latency ratio 1.293276 exceeds 1.10.
- kodim08/444/p0/d4: median latency ratio 1.150769 exceeds 1.10.
- kodim08/420/p0/d4: median latency ratio 1.169977 exceeds 1.10.
- kodim09/444/p0/d1: median latency ratio 1.170866 exceeds 1.10.
- kodim09/444/p0/d4: median latency ratio 1.347567 exceeds 1.10.
- kodim09/420/p0/d1: median latency ratio 1.156201 exceeds 1.10.
- kodim09/420/p0/d4: median latency ratio 1.321442 exceeds 1.10.
- kodim10/444/p0/d1: median latency ratio 1.138817 exceeds 1.10.
- kodim10/444/p0/d4: median latency ratio 1.300985 exceeds 1.10.
- kodim10/420/p0/d1: median latency ratio 1.136782 exceeds 1.10.
- kodim10/420/p0/d4: median latency ratio 1.315417 exceeds 1.10.
- kodim11/444/p0/d4: median latency ratio 1.244855 exceeds 1.10.
- kodim11/420/p0/d4: median latency ratio 1.257736 exceeds 1.10.
- kodim12/444/p0/d1: median latency ratio 1.101505 exceeds 1.10.
- kodim12/444/p0/d4: median latency ratio 1.293457 exceeds 1.10.
- kodim12/420/p0/d1: median latency ratio 1.108255 exceeds 1.10.
- kodim12/420/p0/d4: median latency ratio 1.307861 exceeds 1.10.
- kodim13/444/p0/d4: median latency ratio 1.106072 exceeds 1.10.
- kodim13/420/p0/d4: median latency ratio 1.120660 exceeds 1.10.
- kodim14/444/p0/d4: median latency ratio 1.218583 exceeds 1.10.
- kodim14/420/p0/d4: median latency ratio 1.196630 exceeds 1.10.
- kodim15/444/p0/d1: median latency ratio 1.140362 exceeds 1.10.
- kodim15/444/p0/d4: median latency ratio 1.335275 exceeds 1.10.
- kodim15/420/p0/d4: median latency ratio 1.324587 exceeds 1.10.
- kodim16/444/p0/d1: median latency ratio 1.112191 exceeds 1.10.
- kodim16/444/p0/d4: median latency ratio 1.255261 exceeds 1.10.
- kodim16/420/p0/d4: median latency ratio 1.282072 exceeds 1.10.
- kodim17/444/p0/d1: median latency ratio 1.114765 exceeds 1.10.
- kodim17/444/p0/d4: median latency ratio 1.292313 exceeds 1.10.
- kodim17/420/p0/d4: median latency ratio 1.335692 exceeds 1.10.
- kodim18/444/p0/d4: median latency ratio 1.259373 exceeds 1.10.
- kodim18/420/p0/d4: median latency ratio 1.208483 exceeds 1.10.
- kodim19/444/p0/d4: median latency ratio 1.312768 exceeds 1.10.
- kodim19/420/p0/d4: median latency ratio 1.294540 exceeds 1.10.
- kodim20/444/p0/d1: median latency ratio 1.120072 exceeds 1.10.
- kodim20/444/p0/d4: median latency ratio 1.299797 exceeds 1.10.
- kodim20/420/p0/d1: median latency ratio 1.133565 exceeds 1.10.
- kodim20/420/p0/d4: median latency ratio 1.326547 exceeds 1.10.
- kodim21/444/p0/d4: median latency ratio 1.204529 exceeds 1.10.
- kodim21/420/p0/d4: median latency ratio 1.267509 exceeds 1.10.
- kodim22/444/p0/d4: median latency ratio 1.233715 exceeds 1.10.
- kodim22/420/p0/d4: median latency ratio 1.272603 exceeds 1.10.
- kodim23/444/p0/d1: median latency ratio 1.188097 exceeds 1.10.
- kodim23/444/p0/d4: median latency ratio 1.366639 exceeds 1.10.
- kodim23/420/p0/d1: median latency ratio 1.194968 exceeds 1.10.
- kodim23/420/p0/d4: median latency ratio 1.360784 exceeds 1.10.
- kodim24/444/p0/d4: median latency ratio 1.181662 exceeds 1.10.
- kodim24/420/p0/d4: median latency ratio 1.215744 exceeds 1.10.
- smooth-gradient/444/p0/d1: median latency ratio 1.502862 exceeds 1.10.
- smooth-gradient/444/p0/d4: median latency ratio 1.413449 exceeds 1.10.
- smooth-gradient/444/p2/d1: median latency ratio 1.155877 exceeds 1.10.
- smooth-gradient/444/p2/d4: median latency ratio 1.130297 exceeds 1.10.
- smooth-gradient/420/p0/d1: median latency ratio 1.382961 exceeds 1.10.
- smooth-gradient/420/p0/d4: median latency ratio 1.355748 exceeds 1.10.
- smooth-gradient/420/p2/d1: median latency ratio 1.191767 exceeds 1.10.
- smooth-gradient/420/p2/d4: median latency ratio 1.184172 exceeds 1.10.
- low-contrast/444/p0/d1: median latency ratio 1.386184 exceeds 1.10.
- low-contrast/444/p0/d4: median latency ratio 1.441080 exceeds 1.10.
- low-contrast/444/p2/d4: median latency ratio 1.140223 exceeds 1.10.
- low-contrast/420/p0/d1: median latency ratio 1.207603 exceeds 1.10.
- low-contrast/420/p0/d4: median latency ratio 1.349327 exceeds 1.10.
- low-contrast/420/p2/d4: median latency ratio 1.169941 exceeds 1.10.
- color-edges/444/p0/d1: median latency ratio 1.344322 exceeds 1.10.
- color-edges/444/p0/d4: median latency ratio 1.357522 exceeds 1.10.
- color-edges/420/p0/d1: median latency ratio 1.150597 exceeds 1.10.
- color-edges/420/p0/d4: median latency ratio 1.167087 exceeds 1.10.
- line-art/444/p0/d1: median latency ratio 1.362373 exceeds 1.10.
- line-art/444/p0/d4: median latency ratio 1.339348 exceeds 1.10.
- line-art/420/p0/d1: median latency ratio 1.134351 exceeds 1.10.
- line-art/420/p0/d4: median latency ratio 1.156060 exceeds 1.10.
- structured-texture/444/p0/d4: median latency ratio 1.103168 exceeds 1.10.
- structured-texture/420/p0/d4: median latency ratio 1.154466 exceeds 1.10.
- low-noise/444/p0/d4: median latency ratio 1.435058 exceeds 1.10.
- low-noise/444/p2/d4: median latency ratio 1.105363 exceeds 1.10.
- low-noise/420/p0/d1: median latency ratio 1.165566 exceeds 1.10.
- low-noise/420/p0/d4: median latency ratio 1.343438 exceeds 1.10.
- low-noise/420/p2/d4: median latency ratio 1.193846 exceeds 1.10.
- tiny-odd-edges/444/p0/d1: median latency ratio 1.432267 exceeds 1.10.
- tiny-odd-edges/444/p0/d4: median latency ratio 1.186963 exceeds 1.10.
- tiny-odd-edges/444/p2/d1: median latency ratio 1.394145 exceeds 1.10.
- tiny-odd-edges/444/p2/d4: median latency ratio 1.283502 exceeds 1.10.
- tiny-odd-edges/420/p0/d1: median latency ratio 1.502642 exceeds 1.10.
- tiny-odd-edges/420/p0/d4: median latency ratio 1.224810 exceeds 1.10.
- tiny-odd-edges/420/p2/d1: median latency ratio 1.426379 exceeds 1.10.
- tiny-odd-edges/420/p2/d4: median latency ratio 1.301326 exceeds 1.10.
- natural/444/p0: geometric mean latency ratio 1.161249 exceeds 1.05.
- natural/420/p0: geometric mean latency ratio 1.134877 exceeds 1.05.
- controlled/444/p0: geometric mean latency ratio 1.195753 exceeds 1.05.
- controlled/420/p0: geometric mean latency ratio 1.122608 exceeds 1.05.
