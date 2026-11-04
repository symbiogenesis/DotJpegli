# Performance parity: FAILED

256/256 cases: RGB8 YCbCr, Kodak24 plus eight fixed controls, distances1/4, 444/420 and progression0/2, one worker on this CI host. Pinned upstream `031a0077f5799a6041004267fc12b956c1f52a20`.
Tested revision: `75230a072dd31bda896e3ed2706aeb52ac968975`.

All cases are measured even when a latency bound fails, so the report identifies every measured regression. A passing verdict requires complete, valid coverage and all fixed bounds.

Fixed latency noninferiority limits: geometric mean managed/native median latency ratio <=1.05 in each corpus/sampling/progression group; every case <=1.10. A faster case cannot excuse a slower case. No universal performance claim is made.

Seven alternating order blocks, five untimed warmups before each block. The native-only three-iteration calibration targets50ms and selects a shared count clamped3..100. Medians use all seven blocks; no outliers are discarded. Complete in-memory encode calls are timed individually and their elapsed times summed; subprocess startup, file I/O, verification and native buffer free are excluded. Managed collections inside encode are included. Tiered compilation is disabled.

Evidence is invalid if a block interval is below1ms, any case has relative MAD >10%, inputs/settings/coverage differ, or an untimed JPEG structure comparison fails. Invalid evidence cannot establish parity.

| Corpus | Sampling | Progression | Cases | Geometric mean ratio | Worst ratio | Verdict |
| --- | --- | --- | ---: | ---: | ---: | --- |
| natural | 444 | 0 | 48 | 1.180759 | 1.383027 | FAIL |
| natural | 444 | 2 | 48 | 0.898726 | 0.976977 | PASS |
| natural | 420 | 0 | 48 | 1.157399 | 1.405034 | FAIL |
| natural | 420 | 2 | 48 | 0.893689 | 1.009467 | PASS |
| controlled | 444 | 0 | 16 | 1.223892 | 1.814946 | FAIL |
| controlled | 444 | 2 | 16 | 1.104170 | 1.531142 | FAIL |
| controlled | 420 | 0 | 16 | 1.117442 | 1.508274 | FAIL |
| controlled | 420 | 2 | 16 | 1.098329 | 1.395222 | FAIL |

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 5.209 | 5.118 | 1.018 | 0.2 % | 0.2 % |
| kodim01/444/p0/d4 | 4.296 | 3.604 | 1.192 | 0.6 % | 0.2 % |
| kodim01/444/p2/d1 | 10.843 | 12.925 | 0.839 | 0.9 % | 1.2 % |
| kodim01/444/p2/d4 | 7.946 | 8.991 | 0.884 | 0.2 % | 0.8 % |
| kodim01/420/p0/d1 | 3.986 | 4.118 | 0.968 | 1.4 % | 0.3 % |
| kodim01/420/p0/d4 | 3.157 | 2.645 | 1.194 | 0.7 % | 0.2 % |
| kodim01/420/p2/d1 | 8.034 | 10.034 | 0.801 | 0.6 % | 0.8 % |
| kodim01/420/p2/d4 | 5.433 | 6.116 | 0.888 | 0.5 % | 0.2 % |
| kodim02/444/p0/d1 | 4.709 | 4.259 | 1.106 | 0.1 % | 0.0 % |
| kodim02/444/p0/d4 | 4.049 | 3.067 | 1.320 | 0.2 % | 0.2 % |
| kodim02/444/p2/d1 | 9.032 | 10.525 | 0.858 | 0.7 % | 0.3 % |
| kodim02/444/p2/d4 | 6.963 | 7.389 | 0.942 | 0.5 % | 0.6 % |
| kodim02/420/p0/d1 | 3.457 | 3.277 | 1.055 | 0.3 % | 0.2 % |
| kodim02/420/p0/d4 | 2.944 | 2.183 | 1.349 | 0.6 % | 0.5 % |
| kodim02/420/p2/d1 | 6.348 | 7.732 | 0.821 | 0.6 % | 0.3 % |
| kodim02/420/p2/d4 | 4.604 | 4.819 | 0.955 | 0.5 % | 0.4 % |
| kodim03/444/p0/d1 | 4.371 | 3.674 | 1.190 | 0.5 % | 0.2 % |
| kodim03/444/p0/d4 | 3.930 | 2.980 | 1.319 | 0.3 % | 0.3 % |
| kodim03/444/p2/d1 | 8.137 | 9.047 | 0.899 | 0.3 % | 0.3 % |
| kodim03/444/p2/d4 | 6.957 | 7.135 | 0.975 | 0.1 % | 0.3 % |
| kodim03/420/p0/d1 | 3.222 | 2.758 | 1.168 | 0.4 % | 0.2 % |
| kodim03/420/p0/d4 | 2.893 | 2.129 | 1.359 | 0.3 % | 0.3 % |
| kodim03/420/p2/d1 | 5.750 | 6.569 | 0.875 | 0.4 % | 0.3 % |
| kodim03/420/p2/d4 | 4.575 | 4.739 | 0.966 | 0.2 % | 0.5 % |
| kodim04/444/p0/d1 | 4.766 | 4.189 | 1.138 | 0.7 % | 0.2 % |
| kodim04/444/p0/d4 | 4.101 | 3.098 | 1.324 | 0.2 % | 0.4 % |
| kodim04/444/p2/d1 | 9.030 | 10.345 | 0.873 | 0.5 % | 0.2 % |
| kodim04/444/p2/d4 | 7.164 | 7.444 | 0.962 | 0.7 % | 0.3 % |
| kodim04/420/p0/d1 | 3.481 | 3.210 | 1.084 | 0.5 % | 0.1 % |
| kodim04/420/p0/d4 | 2.976 | 2.219 | 1.341 | 0.3 % | 0.2 % |
| kodim04/420/p2/d1 | 6.351 | 7.356 | 0.863 | 1.2 % | 0.3 % |
| kodim04/420/p2/d4 | 4.716 | 4.715 | 1.000 | 0.3 % | 0.6 % |
| kodim05/444/p0/d1 | 5.308 | 5.020 | 1.057 | 0.7 % | 0.3 % |
| kodim05/444/p0/d4 | 4.332 | 3.698 | 1.171 | 0.3 % | 0.3 % |
| kodim05/444/p2/d1 | 11.005 | 12.573 | 0.875 | 0.7 % | 0.2 % |
| kodim05/444/p2/d4 | 8.331 | 9.051 | 0.920 | 0.1 % | 0.7 % |
| kodim05/420/p0/d1 | 3.864 | 4.642 | 0.833 | 0.4 % | 0.2 % |
| kodim05/420/p0/d4 | 3.213 | 2.763 | 1.163 | 0.3 % | 0.3 % |
| kodim05/420/p2/d1 | 8.307 | 10.040 | 0.827 | 0.4 % | 0.1 % |
| kodim05/420/p2/d4 | 5.815 | 6.473 | 0.898 | 0.4 % | 0.2 % |
| kodim06/444/p0/d1 | 4.735 | 4.518 | 1.048 | 0.2 % | 0.2 % |
| kodim06/444/p0/d4 | 3.999 | 3.327 | 1.202 | 0.1 % | 0.0 % |
| kodim06/444/p2/d1 | 9.502 | 11.109 | 0.855 | 0.1 % | 0.1 % |
| kodim06/444/p2/d4 | 7.314 | 8.152 | 0.897 | 0.1 % | 0.4 % |
| kodim06/420/p0/d1 | 3.589 | 3.552 | 1.011 | 0.2 % | 0.1 % |
| kodim06/420/p0/d4 | 3.059 | 2.435 | 1.256 | 1.8 % | 0.5 % |
| kodim06/420/p2/d1 | 7.027 | 8.513 | 0.825 | 0.2 % | 0.2 % |
| kodim06/420/p2/d4 | 5.102 | 5.670 | 0.900 | 0.7 % | 0.8 % |
| kodim07/444/p0/d1 | 4.499 | 3.802 | 1.183 | 0.3 % | 0.1 % |
| kodim07/444/p0/d4 | 3.965 | 3.088 | 1.284 | 0.4 % | 0.1 % |
| kodim07/444/p2/d1 | 8.482 | 9.238 | 0.918 | 0.5 % | 0.4 % |
| kodim07/444/p2/d4 | 7.056 | 7.422 | 0.951 | 0.4 % | 0.4 % |
| kodim07/420/p0/d1 | 3.297 | 2.878 | 1.146 | 0.2 % | 0.2 % |
| kodim07/420/p0/d4 | 2.965 | 2.233 | 1.328 | 0.4 % | 0.1 % |
| kodim07/420/p2/d1 | 6.052 | 6.771 | 0.894 | 0.3 % | 0.2 % |
| kodim07/420/p2/d4 | 4.808 | 5.034 | 0.955 | 0.3 % | 0.4 % |
| kodim08/444/p0/d1 | 5.175 | 5.100 | 1.015 | 0.2 % | 0.1 % |
| kodim08/444/p0/d4 | 4.253 | 3.672 | 1.158 | 0.1 % | 0.1 % |
| kodim08/444/p2/d1 | 11.079 | 12.708 | 0.872 | 0.7 % | 0.3 % |
| kodim08/444/p2/d4 | 8.072 | 8.993 | 0.898 | 0.6 % | 0.2 % |
| kodim08/420/p0/d1 | 3.945 | 4.694 | 0.841 | 0.6 % | 0.2 % |
| kodim08/420/p0/d4 | 3.199 | 2.726 | 1.174 | 0.2 % | 0.3 % |
| kodim08/420/p2/d1 | 8.204 | 9.993 | 0.821 | 0.3 % | 0.6 % |
| kodim08/420/p2/d4 | 5.751 | 6.367 | 0.903 | 0.1 % | 0.0 % |
| kodim09/444/p0/d1 | 4.382 | 3.715 | 1.180 | 0.4 % | 0.1 % |
| kodim09/444/p0/d4 | 3.933 | 2.993 | 1.314 | 0.7 % | 0.1 % |
| kodim09/444/p2/d1 | 8.086 | 9.016 | 0.897 | 0.5 % | 0.4 % |
| kodim09/444/p2/d4 | 6.984 | 7.163 | 0.975 | 0.5 % | 0.1 % |
| kodim09/420/p0/d1 | 3.255 | 2.747 | 1.185 | 0.2 % | 0.2 % |
| kodim09/420/p0/d4 | 2.897 | 2.128 | 1.362 | 0.4 % | 0.1 % |
| kodim09/420/p2/d1 | 5.687 | 6.191 | 0.919 | 0.1 % | 0.3 % |
| kodim09/420/p2/d4 | 4.518 | 4.508 | 1.002 | 0.4 % | 0.2 % |
| kodim10/444/p0/d1 | 4.469 | 3.872 | 1.154 | 0.2 % | 0.0 % |
| kodim10/444/p0/d4 | 3.962 | 3.057 | 1.296 | 0.1 % | 0.2 % |
| kodim10/444/p2/d1 | 8.346 | 9.411 | 0.887 | 0.4 % | 0.2 % |
| kodim10/444/p2/d4 | 6.937 | 7.372 | 0.941 | 0.1 % | 0.2 % |
| kodim10/420/p0/d1 | 3.343 | 2.915 | 1.147 | 0.2 % | 0.1 % |
| kodim10/420/p0/d4 | 2.975 | 2.182 | 1.363 | 0.2 % | 0.2 % |
| kodim10/420/p2/d1 | 5.983 | 6.609 | 0.905 | 0.2 % | 0.4 % |
| kodim10/420/p2/d4 | 4.695 | 4.666 | 1.006 | 0.1 % | 0.1 % |
| kodim11/444/p0/d1 | 4.803 | 4.452 | 1.079 | 0.1 % | 0.0 % |
| kodim11/444/p0/d4 | 4.016 | 3.243 | 1.238 | 0.1 % | 0.2 % |
| kodim11/444/p2/d1 | 9.497 | 11.012 | 0.862 | 0.4 % | 0.4 % |
| kodim11/444/p2/d4 | 7.306 | 7.879 | 0.927 | 0.7 % | 0.5 % |
| kodim11/420/p0/d1 | 3.574 | 3.477 | 1.028 | 0.5 % | 0.1 % |
| kodim11/420/p0/d4 | 2.988 | 2.344 | 1.275 | 0.3 % | 0.0 % |
| kodim11/420/p2/d1 | 6.870 | 8.349 | 0.823 | 0.3 % | 0.2 % |
| kodim11/420/p2/d4 | 4.989 | 5.331 | 0.936 | 0.3 % | 0.0 % |
| kodim12/444/p0/d1 | 4.405 | 3.912 | 1.126 | 0.1 % | 0.2 % |
| kodim12/444/p0/d4 | 3.880 | 2.994 | 1.296 | 0.2 % | 0.2 % |
| kodim12/444/p2/d1 | 8.190 | 9.499 | 0.862 | 0.2 % | 0.1 % |
| kodim12/444/p2/d4 | 6.742 | 7.188 | 0.938 | 0.2 % | 0.2 % |
| kodim12/420/p0/d1 | 3.297 | 2.939 | 1.122 | 0.3 % | 0.2 % |
| kodim12/420/p0/d4 | 2.867 | 2.137 | 1.342 | 0.3 % | 0.3 % |
| kodim12/420/p2/d1 | 5.813 | 6.758 | 0.860 | 0.2 % | 0.1 % |
| kodim12/420/p2/d4 | 4.479 | 4.730 | 0.947 | 0.2 % | 0.4 % |
| kodim13/444/p0/d1 | 8.601 | 7.095 | 1.212 | 1.2 % | 0.9 % |
| kodim13/444/p0/d4 | 4.282 | 3.877 | 1.104 | 0.4 % | 0.1 % |
| kodim13/444/p2/d1 | 11.659 | 14.095 | 0.827 | 0.2 % | 0.2 % |
| kodim13/444/p2/d4 | 8.167 | 9.480 | 0.861 | 0.3 % | 0.4 % |
| kodim13/420/p0/d1 | 4.093 | 5.264 | 0.778 | 0.3 % | 0.3 % |
| kodim13/420/p0/d4 | 3.213 | 2.864 | 1.122 | 0.4 % | 0.1 % |
| kodim13/420/p2/d1 | 8.931 | 11.372 | 0.785 | 0.2 % | 0.2 % |
| kodim13/420/p2/d4 | 5.811 | 6.734 | 0.863 | 0.2 % | 0.2 % |
| kodim14/444/p0/d1 | 4.974 | 4.928 | 1.010 | 0.1 % | 0.0 % |
| kodim14/444/p0/d4 | 4.184 | 3.484 | 1.201 | 0.1 % | 0.1 % |
| kodim14/444/p2/d1 | 10.289 | 12.415 | 0.829 | 0.3 % | 0.3 % |
| kodim14/444/p2/d4 | 7.650 | 8.547 | 0.895 | 0.2 % | 0.2 % |
| kodim14/420/p0/d1 | 3.758 | 3.894 | 0.965 | 0.2 % | 0.2 % |
| kodim14/420/p0/d4 | 3.088 | 2.532 | 1.219 | 0.3 % | 0.1 % |
| kodim14/420/p2/d1 | 7.690 | 9.453 | 0.814 | 0.6 % | 0.5 % |
| kodim14/420/p2/d4 | 5.320 | 5.838 | 0.911 | 0.2 % | 0.3 % |
| kodim15/444/p0/d1 | 4.543 | 3.955 | 1.149 | 0.3 % | 0.1 % |
| kodim15/444/p0/d4 | 3.959 | 3.042 | 1.301 | 0.1 % | 0.2 % |
| kodim15/444/p2/d1 | 8.460 | 9.615 | 0.880 | 0.2 % | 0.4 % |
| kodim15/444/p2/d4 | 6.848 | 7.294 | 0.939 | 0.5 % | 0.1 % |
| kodim15/420/p0/d1 | 3.338 | 2.995 | 1.115 | 0.4 % | 0.3 % |
| kodim15/420/p0/d4 | 2.903 | 2.163 | 1.342 | 0.1 % | 0.2 % |
| kodim15/420/p2/d1 | 5.967 | 6.964 | 0.857 | 0.2 % | 0.2 % |
| kodim15/420/p2/d4 | 4.559 | 4.831 | 0.944 | 0.2 % | 0.2 % |
| kodim16/444/p0/d1 | 4.471 | 4.079 | 1.096 | 0.2 % | 0.1 % |
| kodim16/444/p0/d4 | 3.876 | 3.094 | 1.253 | 0.2 % | 0.1 % |
| kodim16/444/p2/d1 | 8.687 | 10.051 | 0.864 | 0.7 % | 0.2 % |
| kodim16/444/p2/d4 | 6.912 | 7.471 | 0.925 | 0.3 % | 0.6 % |
| kodim16/420/p0/d1 | 3.380 | 3.115 | 1.085 | 0.3 % | 0.1 % |
| kodim16/420/p0/d4 | 2.910 | 2.210 | 1.316 | 0.3 % | 0.1 % |
| kodim16/420/p2/d1 | 6.193 | 7.347 | 0.843 | 0.3 % | 0.2 % |
| kodim16/420/p2/d4 | 4.705 | 4.929 | 0.955 | 0.2 % | 0.3 % |
| kodim17/444/p0/d1 | 4.438 | 3.970 | 1.118 | 0.2 % | 0.1 % |
| kodim17/444/p0/d4 | 3.914 | 3.099 | 1.263 | 0.5 % | 0.4 % |
| kodim17/444/p2/d1 | 8.703 | 9.722 | 0.895 | 0.1 % | 0.2 % |
| kodim17/444/p2/d4 | 6.984 | 7.504 | 0.931 | 0.3 % | 0.2 % |
| kodim17/420/p0/d1 | 3.396 | 3.015 | 1.126 | 0.6 % | 0.0 % |
| kodim17/420/p0/d4 | 2.952 | 2.212 | 1.335 | 0.1 % | 0.3 % |
| kodim17/420/p2/d1 | 6.223 | 6.887 | 0.904 | 0.3 % | 0.3 % |
| kodim17/420/p2/d4 | 4.818 | 4.773 | 1.009 | 0.4 % | 0.1 % |
| kodim18/444/p0/d1 | 5.038 | 4.807 | 1.048 | 0.4 % | 0.2 % |
| kodim18/444/p0/d4 | 4.178 | 3.438 | 1.215 | 0.2 % | 0.3 % |
| kodim18/444/p2/d1 | 10.210 | 11.773 | 0.867 | 0.3 % | 0.1 % |
| kodim18/444/p2/d4 | 7.599 | 8.437 | 0.901 | 0.2 % | 0.6 % |
| kodim18/420/p0/d1 | 3.740 | 3.769 | 0.992 | 0.4 % | 0.1 % |
| kodim18/420/p0/d4 | 3.129 | 2.515 | 1.244 | 0.2 % | 0.1 % |
| kodim18/420/p2/d1 | 7.488 | 8.997 | 0.832 | 0.1 % | 0.2 % |
| kodim18/420/p2/d4 | 5.233 | 5.606 | 0.934 | 0.1 % | 0.7 % |
| kodim19/444/p0/d1 | 7.519 | 5.437 | 1.383 | 0.3 % | 0.2 % |
| kodim19/444/p0/d4 | 3.984 | 3.166 | 1.258 | 0.1 % | 0.1 % |
| kodim19/444/p2/d1 | 9.023 | 10.526 | 0.857 | 0.2 % | 0.2 % |
| kodim19/444/p2/d4 | 7.093 | 7.710 | 0.920 | 0.2 % | 0.2 % |
| kodim19/420/p0/d1 | 3.512 | 3.290 | 1.067 | 0.3 % | 0.1 % |
| kodim19/420/p0/d4 | 2.995 | 2.279 | 1.314 | 0.2 % | 0.2 % |
| kodim19/420/p2/d1 | 6.559 | 7.620 | 0.861 | 0.5 % | 0.2 % |
| kodim19/420/p2/d4 | 4.923 | 4.924 | 1.000 | 0.2 % | 0.3 % |
| kodim20/444/p0/d1 | 4.254 | 3.708 | 1.147 | 0.5 % | 0.2 % |
| kodim20/444/p0/d4 | 3.792 | 2.946 | 1.287 | 0.1 % | 0.2 % |
| kodim20/444/p2/d1 | 7.965 | 9.068 | 0.878 | 0.2 % | 0.3 % |
| kodim20/444/p2/d4 | 6.801 | 7.159 | 0.950 | 0.1 % | 0.3 % |
| kodim20/420/p0/d1 | 3.154 | 2.788 | 1.131 | 0.3 % | 0.1 % |
| kodim20/420/p0/d4 | 2.823 | 2.098 | 1.346 | 0.3 % | 0.3 % |
| kodim20/420/p2/d1 | 5.572 | 6.606 | 0.843 | 0.2 % | 0.1 % |
| kodim20/420/p2/d4 | 4.392 | 4.704 | 0.934 | 0.4 % | 0.5 % |
| kodim21/444/p0/d1 | 4.727 | 4.281 | 1.104 | 0.2 % | 0.1 % |
| kodim21/444/p0/d4 | 3.950 | 3.230 | 1.223 | 0.2 % | 0.2 % |
| kodim21/444/p2/d1 | 9.104 | 10.421 | 0.874 | 0.3 % | 0.2 % |
| kodim21/444/p2/d4 | 7.270 | 7.816 | 0.930 | 0.1 % | 0.2 % |
| kodim21/420/p0/d1 | 3.525 | 3.286 | 1.073 | 0.3 % | 0.1 % |
| kodim21/420/p0/d4 | 2.935 | 2.317 | 1.267 | 0.4 % | 0.2 % |
| kodim21/420/p2/d1 | 6.590 | 7.765 | 0.849 | 0.2 % | 0.3 % |
| kodim21/420/p2/d4 | 4.823 | 5.306 | 0.909 | 0.3 % | 0.6 % |
| kodim22/444/p0/d1 | 4.859 | 4.562 | 1.065 | 0.4 % | 0.1 % |
| kodim22/444/p0/d4 | 4.029 | 3.256 | 1.237 | 0.1 % | 0.1 % |
| kodim22/444/p2/d1 | 9.454 | 11.123 | 0.850 | 0.2 % | 0.2 % |
| kodim22/444/p2/d4 | 7.308 | 7.855 | 0.930 | 1.7 % | 0.3 % |
| kodim22/420/p0/d1 | 3.541 | 3.500 | 1.011 | 0.3 % | 0.2 % |
| kodim22/420/p0/d4 | 3.014 | 2.344 | 1.286 | 0.2 % | 0.0 % |
| kodim22/420/p2/d1 | 6.840 | 8.414 | 0.813 | 0.2 % | 0.1 % |
| kodim22/420/p2/d4 | 4.957 | 5.296 | 0.936 | 0.4 % | 0.2 % |
| kodim23/444/p0/d1 | 4.354 | 3.578 | 1.217 | 0.2 % | 0.0 % |
| kodim23/444/p0/d4 | 3.906 | 2.930 | 1.333 | 0.2 % | 0.2 % |
| kodim23/444/p2/d1 | 8.120 | 8.494 | 0.956 | 0.2 % | 0.4 % |
| kodim23/444/p2/d4 | 6.783 | 6.943 | 0.977 | 0.5 % | 0.1 % |
| kodim23/420/p0/d1 | 3.208 | 2.624 | 1.223 | 0.4 % | 0.2 % |
| kodim23/420/p0/d4 | 2.940 | 2.093 | 1.405 | 0.5 % | 0.2 % |
| kodim23/420/p2/d1 | 5.495 | 6.085 | 0.903 | 0.4 % | 0.9 % |
| kodim23/420/p2/d4 | 4.532 | 4.578 | 0.990 | 0.5 % | 0.3 % |
| kodim24/444/p0/d1 | 4.958 | 4.760 | 1.042 | 0.1 % | 0.1 % |
| kodim24/444/p0/d4 | 4.145 | 3.480 | 1.191 | 0.3 % | 0.1 % |
| kodim24/444/p2/d1 | 10.098 | 11.886 | 0.850 | 0.2 % | 0.5 % |
| kodim24/444/p2/d4 | 7.620 | 8.602 | 0.886 | 0.5 % | 0.8 % |
| kodim24/420/p0/d1 | 3.706 | 3.724 | 0.995 | 0.1 % | 0.2 % |
| kodim24/420/p0/d4 | 3.096 | 2.541 | 1.219 | 0.3 % | 0.2 % |
| kodim24/420/p2/d1 | 7.384 | 8.963 | 0.824 | 0.3 % | 0.2 % |
| kodim24/420/p2/d4 | 5.226 | 5.853 | 0.893 | 0.2 % | 0.6 % |
| smooth-gradient/444/p0/d1 | 0.568 | 0.368 | 1.544 | 0.5 % | 0.0 % |
| smooth-gradient/444/p0/d4 | 0.859 | 0.473 | 1.815 | 0.4 % | 0.8 % |
| smooth-gradient/444/p2/d1 | 1.538 | 1.004 | 1.531 | 1.3 % | 0.4 % |
| smooth-gradient/444/p2/d4 | 0.894 | 0.679 | 1.315 | 0.6 % | 0.2 % |
| smooth-gradient/420/p0/d1 | 0.422 | 0.292 | 1.444 | 0.4 % | 0.4 % |
| smooth-gradient/420/p0/d4 | 0.415 | 0.296 | 1.401 | 0.7 % | 0.5 % |
| smooth-gradient/420/p2/d1 | 0.618 | 0.443 | 1.395 | 0.4 % | 0.3 % |
| smooth-gradient/420/p2/d4 | 0.592 | 0.439 | 1.348 | 0.5 % | 0.3 % |
| low-contrast/444/p0/d1 | 0.690 | 0.503 | 1.370 | 0.2 % | 0.1 % |
| low-contrast/444/p0/d4 | 0.593 | 0.388 | 1.526 | 0.3 % | 0.2 % |
| low-contrast/444/p2/d1 | 1.298 | 1.160 | 1.119 | 0.3 % | 0.1 % |
| low-contrast/444/p2/d4 | 1.021 | 0.791 | 1.291 | 0.4 % | 0.4 % |
| low-contrast/420/p0/d1 | 0.507 | 0.431 | 1.175 | 0.7 % | 0.9 % |
| low-contrast/420/p0/d4 | 0.434 | 0.316 | 1.372 | 0.6 % | 0.3 % |
| low-contrast/420/p2/d1 | 0.902 | 0.833 | 1.082 | 0.5 % | 0.1 % |
| low-contrast/420/p2/d4 | 0.688 | 0.524 | 1.314 | 0.5 % | 0.3 % |
| color-edges/444/p0/d1 | 0.736 | 0.559 | 1.317 | 0.6 % | 0.2 % |
| color-edges/444/p0/d4 | 0.672 | 0.487 | 1.378 | 0.4 % | 0.2 % |
| color-edges/444/p2/d1 | 1.569 | 1.358 | 1.156 | 0.3 % | 0.3 % |
| color-edges/444/p2/d4 | 1.354 | 1.148 | 1.180 | 0.3 % | 0.4 % |
| color-edges/420/p0/d1 | 0.606 | 0.556 | 1.090 | 0.3 % | 0.7 % |
| color-edges/420/p0/d4 | 0.533 | 0.448 | 1.188 | 0.7 % | 0.6 % |
| color-edges/420/p2/d1 | 1.189 | 1.129 | 1.053 | 0.1 % | 0.1 % |
| color-edges/420/p2/d4 | 0.988 | 0.926 | 1.068 | 0.9 % | 0.2 % |
| line-art/444/p0/d1 | 0.671 | 0.536 | 1.253 | 0.4 % | 0.1 % |
| line-art/444/p0/d4 | 0.629 | 0.484 | 1.301 | 0.2 % | 0.3 % |
| line-art/444/p2/d1 | 1.365 | 1.136 | 1.202 | 0.5 % | 0.2 % |
| line-art/444/p2/d4 | 1.226 | 1.122 | 1.092 | 0.3 % | 0.3 % |
| line-art/420/p0/d1 | 0.555 | 0.502 | 1.106 | 0.6 % | 0.4 % |
| line-art/420/p0/d4 | 0.504 | 0.426 | 1.183 | 0.3 % | 0.2 % |
| line-art/420/p2/d1 | 1.052 | 0.883 | 1.192 | 0.6 % | 0.2 % |
| line-art/420/p2/d4 | 0.907 | 0.837 | 1.084 | 0.5 % | 0.2 % |
| structured-texture/444/p0/d1 | 0.993 | 1.236 | 0.803 | 0.6 % | 0.0 % |
| structured-texture/444/p0/d4 | 0.736 | 0.704 | 1.045 | 0.2 % | 0.2 % |
| structured-texture/444/p2/d1 | 2.557 | 3.088 | 0.828 | 0.0 % | 0.2 % |
| structured-texture/444/p2/d4 | 1.558 | 1.648 | 0.945 | 0.1 % | 0.1 % |
| structured-texture/420/p0/d1 | 0.684 | 0.914 | 0.748 | 1.0 % | 0.2 % |
| structured-texture/420/p0/d4 | 0.522 | 0.529 | 0.987 | 0.5 % | 0.1 % |
| structured-texture/420/p2/d1 | 1.644 | 2.005 | 0.820 | 0.2 % | 0.1 % |
| structured-texture/420/p2/d4 | 1.005 | 1.070 | 0.939 | 0.6 % | 0.1 % |
| low-noise/444/p0/d1 | 0.717 | 0.723 | 0.992 | 0.4 % | 0.2 % |
| low-noise/444/p0/d4 | 0.546 | 0.374 | 1.461 | 0.3 % | 0.1 % |
| low-noise/444/p2/d1 | 1.333 | 1.625 | 0.820 | 0.7 % | 0.4 % |
| low-noise/444/p2/d4 | 0.903 | 0.727 | 1.243 | 0.5 % | 0.5 % |
| low-noise/420/p0/d1 | 0.519 | 0.562 | 0.922 | 0.2 % | 0.3 % |
| low-noise/420/p0/d4 | 0.408 | 0.294 | 1.391 | 0.8 % | 0.4 % |
| low-noise/420/p2/d1 | 0.903 | 1.069 | 0.845 | 0.4 % | 0.2 % |
| low-noise/420/p2/d4 | 0.613 | 0.458 | 1.339 | 0.4 % | 0.2 % |
| full-noise/444/p0/d1 | 1.105 | 1.367 | 0.808 | 0.5 % | 0.1 % |
| full-noise/444/p0/d4 | 0.842 | 1.021 | 0.824 | 0.3 % | 0.1 % |
| full-noise/444/p2/d1 | 3.130 | 3.419 | 0.916 | 0.7 % | 0.1 % |
| full-noise/444/p2/d4 | 1.864 | 2.405 | 0.775 | 0.2 % | 0.2 % |
| full-noise/420/p0/d1 | 0.752 | 0.993 | 0.757 | 0.8 % | 0.2 % |
| full-noise/420/p0/d4 | 0.585 | 0.735 | 0.795 | 0.3 % | 0.1 % |
| full-noise/420/p2/d1 | 1.993 | 2.033 | 0.980 | 0.1 % | 0.0 % |
| full-noise/420/p2/d4 | 1.225 | 1.523 | 0.804 | 0.8 % | 0.2 % |
| tiny-odd-edges/444/p0/d1 | 0.038 | 0.026 | 1.479 | 0.8 % | 0.2 % |
| tiny-odd-edges/444/p0/d4 | 0.038 | 0.031 | 1.218 | 0.7 % | 0.4 % |
| tiny-odd-edges/444/p2/d1 | 0.072 | 0.053 | 1.354 | 2.8 % | 0.5 % |
| tiny-odd-edges/444/p2/d4 | 0.067 | 0.055 | 1.223 | 1.0 % | 0.6 % |
| tiny-odd-edges/420/p0/d1 | 0.036 | 0.024 | 1.508 | 1.3 % | 0.3 % |
| tiny-odd-edges/420/p0/d4 | 0.036 | 0.029 | 1.243 | 1.0 % | 0.3 % |
| tiny-odd-edges/420/p2/d1 | 0.065 | 0.049 | 1.338 | 1.2 % | 1.1 % |
| tiny-odd-edges/420/p2/d4 | 0.063 | 0.050 | 1.257 | 1.1 % | 0.5 % |

Exact input/JPEG files, SHA256 identities, per-block raw samples, p10/p50/p90, relative MAD, allocations/collections, calibration counts and native command outputs are retained alongside this report. summary.json records CPU, affinity, OS, runtime, vector widths, policy, revision and group verdicts.

Failed bounds:

- kodim01/444/p0/d4: median latency ratio 1.191898 exceeds 1.10.
- kodim01/420/p0/d4: median latency ratio 1.193576 exceeds 1.10.
- kodim02/444/p0/d1: median latency ratio 1.105778 exceeds 1.10.
- kodim02/444/p0/d4: median latency ratio 1.320223 exceeds 1.10.
- kodim02/420/p0/d4: median latency ratio 1.348675 exceeds 1.10.
- kodim03/444/p0/d1: median latency ratio 1.189925 exceeds 1.10.
- kodim03/444/p0/d4: median latency ratio 1.318819 exceeds 1.10.
- kodim03/420/p0/d1: median latency ratio 1.168036 exceeds 1.10.
- kodim03/420/p0/d4: median latency ratio 1.358590 exceeds 1.10.
- kodim04/444/p0/d1: median latency ratio 1.137888 exceeds 1.10.
- kodim04/444/p0/d4: median latency ratio 1.323666 exceeds 1.10.
- kodim04/420/p0/d4: median latency ratio 1.340958 exceeds 1.10.
- kodim05/444/p0/d4: median latency ratio 1.171450 exceeds 1.10.
- kodim05/420/p0/d4: median latency ratio 1.162821 exceeds 1.10.
- kodim06/444/p0/d4: median latency ratio 1.201958 exceeds 1.10.
- kodim06/420/p0/d4: median latency ratio 1.256418 exceeds 1.10.
- kodim07/444/p0/d1: median latency ratio 1.183207 exceeds 1.10.
- kodim07/444/p0/d4: median latency ratio 1.283947 exceeds 1.10.
- kodim07/420/p0/d1: median latency ratio 1.145569 exceeds 1.10.
- kodim07/420/p0/d4: median latency ratio 1.327819 exceeds 1.10.
- kodim08/444/p0/d4: median latency ratio 1.158399 exceeds 1.10.
- kodim08/420/p0/d4: median latency ratio 1.173553 exceeds 1.10.
- kodim09/444/p0/d1: median latency ratio 1.179592 exceeds 1.10.
- kodim09/444/p0/d4: median latency ratio 1.314095 exceeds 1.10.
- kodim09/420/p0/d1: median latency ratio 1.184941 exceeds 1.10.
- kodim09/420/p0/d4: median latency ratio 1.361602 exceeds 1.10.
- kodim10/444/p0/d1: median latency ratio 1.154332 exceeds 1.10.
- kodim10/444/p0/d4: median latency ratio 1.296079 exceeds 1.10.
- kodim10/420/p0/d1: median latency ratio 1.147012 exceeds 1.10.
- kodim10/420/p0/d4: median latency ratio 1.363389 exceeds 1.10.
- kodim11/444/p0/d4: median latency ratio 1.238443 exceeds 1.10.
- kodim11/420/p0/d4: median latency ratio 1.274770 exceeds 1.10.
- kodim12/444/p0/d1: median latency ratio 1.126062 exceeds 1.10.
- kodim12/444/p0/d4: median latency ratio 1.296008 exceeds 1.10.
- kodim12/420/p0/d1: median latency ratio 1.121901 exceeds 1.10.
- kodim12/420/p0/d4: median latency ratio 1.341883 exceeds 1.10.
- kodim13/444/p0/d1: median latency ratio 1.212242 exceeds 1.10.
- kodim13/444/p0/d4: median latency ratio 1.104260 exceeds 1.10.
- kodim13/420/p0/d4: median latency ratio 1.121999 exceeds 1.10.
- kodim14/444/p0/d4: median latency ratio 1.200975 exceeds 1.10.
- kodim14/420/p0/d4: median latency ratio 1.219395 exceeds 1.10.
- kodim15/444/p0/d1: median latency ratio 1.148508 exceeds 1.10.
- kodim15/444/p0/d4: median latency ratio 1.301366 exceeds 1.10.
- kodim15/420/p0/d1: median latency ratio 1.114568 exceeds 1.10.
- kodim15/420/p0/d4: median latency ratio 1.342140 exceeds 1.10.
- kodim16/444/p0/d4: median latency ratio 1.252600 exceeds 1.10.
- kodim16/420/p0/d4: median latency ratio 1.316366 exceeds 1.10.
- kodim17/444/p0/d1: median latency ratio 1.117865 exceeds 1.10.
- kodim17/444/p0/d4: median latency ratio 1.262833 exceeds 1.10.
- kodim17/420/p0/d1: median latency ratio 1.126361 exceeds 1.10.
- kodim17/420/p0/d4: median latency ratio 1.334645 exceeds 1.10.
- kodim18/444/p0/d4: median latency ratio 1.215197 exceeds 1.10.
- kodim18/420/p0/d4: median latency ratio 1.243867 exceeds 1.10.
- kodim19/444/p0/d1: median latency ratio 1.383027 exceeds 1.10.
- kodim19/444/p0/d4: median latency ratio 1.258242 exceeds 1.10.
- kodim19/420/p0/d4: median latency ratio 1.314252 exceeds 1.10.
- kodim20/444/p0/d1: median latency ratio 1.147461 exceeds 1.10.
- kodim20/444/p0/d4: median latency ratio 1.287157 exceeds 1.10.
- kodim20/420/p0/d1: median latency ratio 1.131331 exceeds 1.10.
- kodim20/420/p0/d4: median latency ratio 1.345947 exceeds 1.10.
- kodim21/444/p0/d1: median latency ratio 1.104191 exceeds 1.10.
- kodim21/444/p0/d4: median latency ratio 1.222964 exceeds 1.10.
- kodim21/420/p0/d4: median latency ratio 1.266865 exceeds 1.10.
- kodim22/444/p0/d4: median latency ratio 1.237257 exceeds 1.10.
- kodim22/420/p0/d4: median latency ratio 1.286122 exceeds 1.10.
- kodim23/444/p0/d1: median latency ratio 1.216778 exceeds 1.10.
- kodim23/444/p0/d4: median latency ratio 1.333448 exceeds 1.10.
- kodim23/420/p0/d1: median latency ratio 1.222863 exceeds 1.10.
- kodim23/420/p0/d4: median latency ratio 1.405034 exceeds 1.10.
- kodim24/444/p0/d4: median latency ratio 1.191009 exceeds 1.10.
- kodim24/420/p0/d4: median latency ratio 1.218668 exceeds 1.10.
- smooth-gradient/444/p0/d1: median latency ratio 1.544237 exceeds 1.10.
- smooth-gradient/444/p0/d4: median latency ratio 1.814946 exceeds 1.10.
- smooth-gradient/444/p2/d1: median latency ratio 1.531142 exceeds 1.10.
- smooth-gradient/444/p2/d4: median latency ratio 1.315320 exceeds 1.10.
- smooth-gradient/420/p0/d1: median latency ratio 1.444307 exceeds 1.10.
- smooth-gradient/420/p0/d4: median latency ratio 1.400579 exceeds 1.10.
- smooth-gradient/420/p2/d1: median latency ratio 1.395222 exceeds 1.10.
- smooth-gradient/420/p2/d4: median latency ratio 1.348076 exceeds 1.10.
- low-contrast/444/p0/d1: median latency ratio 1.370409 exceeds 1.10.
- low-contrast/444/p0/d4: median latency ratio 1.526181 exceeds 1.10.
- low-contrast/444/p2/d1: median latency ratio 1.118608 exceeds 1.10.
- low-contrast/444/p2/d4: median latency ratio 1.290808 exceeds 1.10.
- low-contrast/420/p0/d1: median latency ratio 1.174943 exceeds 1.10.
- low-contrast/420/p0/d4: median latency ratio 1.372481 exceeds 1.10.
- low-contrast/420/p2/d4: median latency ratio 1.314031 exceeds 1.10.
- color-edges/444/p0/d1: median latency ratio 1.317194 exceeds 1.10.
- color-edges/444/p0/d4: median latency ratio 1.378462 exceeds 1.10.
- color-edges/444/p2/d1: median latency ratio 1.155933 exceeds 1.10.
- color-edges/444/p2/d4: median latency ratio 1.179641 exceeds 1.10.
- color-edges/420/p0/d4: median latency ratio 1.188105 exceeds 1.10.
- line-art/444/p0/d1: median latency ratio 1.253354 exceeds 1.10.
- line-art/444/p0/d4: median latency ratio 1.300910 exceeds 1.10.
- line-art/444/p2/d1: median latency ratio 1.201907 exceeds 1.10.
- line-art/420/p0/d1: median latency ratio 1.105718 exceeds 1.10.
- line-art/420/p0/d4: median latency ratio 1.182524 exceeds 1.10.
- line-art/420/p2/d1: median latency ratio 1.192128 exceeds 1.10.
- low-noise/444/p0/d4: median latency ratio 1.460732 exceeds 1.10.
- low-noise/444/p2/d4: median latency ratio 1.242601 exceeds 1.10.
- low-noise/420/p0/d4: median latency ratio 1.390644 exceeds 1.10.
- low-noise/420/p2/d4: median latency ratio 1.339314 exceeds 1.10.
- tiny-odd-edges/444/p0/d1: median latency ratio 1.478689 exceeds 1.10.
- tiny-odd-edges/444/p0/d4: median latency ratio 1.217580 exceeds 1.10.
- tiny-odd-edges/444/p2/d1: median latency ratio 1.354061 exceeds 1.10.
- tiny-odd-edges/444/p2/d4: median latency ratio 1.223132 exceeds 1.10.
- tiny-odd-edges/420/p0/d1: median latency ratio 1.508274 exceeds 1.10.
- tiny-odd-edges/420/p0/d4: median latency ratio 1.243442 exceeds 1.10.
- tiny-odd-edges/420/p2/d1: median latency ratio 1.338177 exceeds 1.10.
- tiny-odd-edges/420/p2/d4: median latency ratio 1.256529 exceeds 1.10.
- natural/444/p0: geometric mean latency ratio 1.180759 exceeds 1.05.
- natural/420/p0: geometric mean latency ratio 1.157399 exceeds 1.05.
- controlled/444/p0: geometric mean latency ratio 1.223892 exceeds 1.05.
- controlled/444/p2: geometric mean latency ratio 1.104170 exceeds 1.05.
- controlled/420/p0: geometric mean latency ratio 1.117442 exceeds 1.05.
- controlled/420/p2: geometric mean latency ratio 1.098329 exceeds 1.05.
