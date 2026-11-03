# Performance parity: FAILED

256/256 cases: RGB8 YCbCr, Kodak24 plus eight fixed controls, distances1/4, 444/420 and progression0/2, one worker on this CI host. Pinned upstream `031a0077f5799a6041004267fc12b956c1f52a20`.
Tested revision: `b07eb5d240c2be1f6b9aebac13d081fb50d740cd`.

All cases are measured even when a latency bound fails, so the report identifies every measured regression. A passing verdict requires complete, valid coverage and all fixed bounds.

Fixed latency noninferiority limits: geometric mean managed/native median latency ratio <=1.05 in each corpus/sampling/progression group; every case <=1.10. A faster case cannot excuse a slower case. No universal performance claim is made.

Seven alternating order blocks, five untimed warmups before each block. The native-only three-iteration calibration targets50ms and selects a shared count clamped3..100. Medians use all seven blocks; no outliers are discarded. Complete in-memory encode calls are timed individually and their elapsed times summed; subprocess startup, file I/O, verification and native buffer free are excluded. Managed collections inside encode are included. Tiered compilation is disabled.

Evidence is invalid if a block interval is below1ms, any case has relative MAD >10%, inputs/settings/coverage differ, or an untimed JPEG structure comparison fails. Invalid evidence cannot establish parity.

| Corpus | Sampling | Progression | Cases | Geometric mean ratio | Worst ratio | Verdict |
| --- | --- | --- | ---: | ---: | ---: | --- |
| natural | 444 | 0 | 48 | 1.240736 | 1.581874 | FAIL |
| natural | 444 | 2 | 48 | 0.929983 | 1.017882 | PASS |
| natural | 420 | 0 | 48 | 1.180139 | 1.425277 | FAIL |
| natural | 420 | 2 | 48 | 0.902195 | 1.026717 | PASS |
| controlled | 444 | 0 | 16 | 1.255787 | 1.612267 | FAIL |
| controlled | 444 | 2 | 16 | 1.118154 | 1.388623 | FAIL |
| controlled | 420 | 0 | 16 | 1.129530 | 1.523228 | FAIL |
| controlled | 420 | 2 | 16 | 1.103792 | 1.386591 | FAIL |

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 8.939 | 6.661 | 1.342 | 0.7 % | 0.5 % |
| kodim01/444/p0/d4 | 7.346 | 4.644 | 1.582 | 0.9 % | 0.6 % |
| kodim01/444/p2/d1 | 11.080 | 12.821 | 0.864 | 0.4 % | 0.4 % |
| kodim01/444/p2/d4 | 8.150 | 8.798 | 0.926 | 0.1 % | 0.3 % |
| kodim01/420/p0/d1 | 4.014 | 4.090 | 0.981 | 0.1 % | 0.1 % |
| kodim01/420/p0/d4 | 3.195 | 2.630 | 1.215 | 0.4 % | 0.2 % |
| kodim01/420/p2/d1 | 8.127 | 9.957 | 0.816 | 0.2 % | 0.2 % |
| kodim01/420/p2/d4 | 5.452 | 6.026 | 0.905 | 0.0 % | 0.3 % |
| kodim02/444/p0/d1 | 4.911 | 4.262 | 1.152 | 0.2 % | 0.1 % |
| kodim02/444/p0/d4 | 4.208 | 3.054 | 1.378 | 0.1 % | 0.1 % |
| kodim02/444/p2/d1 | 9.218 | 10.344 | 0.891 | 0.3 % | 0.6 % |
| kodim02/444/p2/d4 | 7.214 | 7.243 | 0.996 | 0.2 % | 0.4 % |
| kodim02/420/p0/d1 | 3.506 | 3.261 | 1.075 | 0.2 % | 0.2 % |
| kodim02/420/p0/d4 | 3.005 | 2.175 | 1.381 | 0.1 % | 0.2 % |
| kodim02/420/p2/d1 | 6.309 | 7.636 | 0.826 | 0.1 % | 0.2 % |
| kodim02/420/p2/d4 | 4.650 | 4.799 | 0.969 | 0.2 % | 0.2 % |
| kodim03/444/p0/d1 | 4.534 | 3.665 | 1.237 | 0.2 % | 0.4 % |
| kodim03/444/p0/d4 | 4.124 | 2.974 | 1.387 | 0.3 % | 0.1 % |
| kodim03/444/p2/d1 | 8.302 | 8.905 | 0.932 | 0.1 % | 0.3 % |
| kodim03/444/p2/d4 | 7.050 | 7.153 | 0.986 | 0.3 % | 0.1 % |
| kodim03/420/p0/d1 | 3.267 | 2.737 | 1.193 | 0.1 % | 0.1 % |
| kodim03/420/p0/d4 | 2.946 | 2.111 | 1.396 | 0.2 % | 0.3 % |
| kodim03/420/p2/d1 | 5.714 | 6.472 | 0.883 | 0.3 % | 0.2 % |
| kodim03/420/p2/d4 | 4.564 | 4.709 | 0.969 | 0.2 % | 0.2 % |
| kodim04/444/p0/d1 | 4.844 | 4.194 | 1.155 | 0.2 % | 0.1 % |
| kodim04/444/p0/d4 | 4.268 | 3.090 | 1.381 | 0.3 % | 0.2 % |
| kodim04/444/p2/d1 | 9.184 | 10.235 | 0.897 | 0.2 % | 0.2 % |
| kodim04/444/p2/d4 | 7.331 | 7.429 | 0.987 | 0.1 % | 0.4 % |
| kodim04/420/p0/d1 | 3.547 | 3.200 | 1.108 | 0.1 % | 0.0 % |
| kodim04/420/p0/d4 | 3.043 | 2.218 | 1.372 | 0.2 % | 0.2 % |
| kodim04/420/p2/d1 | 6.351 | 7.289 | 0.871 | 0.1 % | 0.3 % |
| kodim04/420/p2/d4 | 4.791 | 4.719 | 1.015 | 0.3 % | 0.3 % |
| kodim05/444/p0/d1 | 5.359 | 5.008 | 1.070 | 0.2 % | 0.1 % |
| kodim05/444/p0/d4 | 4.511 | 3.689 | 1.223 | 0.3 % | 0.1 % |
| kodim05/444/p2/d1 | 11.173 | 12.554 | 0.890 | 0.3 % | 0.1 % |
| kodim05/444/p2/d4 | 8.402 | 9.014 | 0.932 | 0.2 % | 0.4 % |
| kodim05/420/p0/d1 | 4.010 | 4.644 | 0.863 | 0.1 % | 0.1 % |
| kodim05/420/p0/d4 | 3.268 | 2.754 | 1.187 | 0.1 % | 0.3 % |
| kodim05/420/p2/d1 | 8.216 | 9.929 | 0.827 | 0.2 % | 0.1 % |
| kodim05/420/p2/d4 | 5.798 | 6.478 | 0.895 | 0.1 % | 0.1 % |
| kodim06/444/p0/d1 | 4.960 | 4.517 | 1.098 | 0.3 % | 0.3 % |
| kodim06/444/p0/d4 | 4.244 | 3.332 | 1.274 | 0.2 % | 0.1 % |
| kodim06/444/p2/d1 | 9.792 | 11.104 | 0.882 | 0.3 % | 0.1 % |
| kodim06/444/p2/d4 | 7.627 | 8.195 | 0.931 | 0.1 % | 0.4 % |
| kodim06/420/p0/d1 | 3.648 | 3.545 | 1.029 | 0.3 % | 0.0 % |
| kodim06/420/p0/d4 | 3.079 | 2.428 | 1.268 | 0.3 % | 0.1 % |
| kodim06/420/p2/d1 | 7.081 | 8.497 | 0.833 | 0.1 % | 0.1 % |
| kodim06/420/p2/d4 | 5.102 | 5.546 | 0.920 | 0.2 % | 0.3 % |
| kodim07/444/p0/d1 | 4.689 | 3.794 | 1.236 | 0.2 % | 0.1 % |
| kodim07/444/p0/d4 | 4.328 | 3.159 | 1.370 | 0.6 % | 0.9 % |
| kodim07/444/p2/d1 | 8.833 | 9.286 | 0.951 | 0.9 % | 0.3 % |
| kodim07/444/p2/d4 | 7.379 | 7.494 | 0.985 | 0.1 % | 0.4 % |
| kodim07/420/p0/d1 | 3.366 | 2.874 | 1.171 | 0.1 % | 0.1 % |
| kodim07/420/p0/d4 | 3.016 | 2.238 | 1.347 | 0.3 % | 0.5 % |
| kodim07/420/p2/d1 | 6.147 | 6.828 | 0.900 | 0.2 % | 0.5 % |
| kodim07/420/p2/d4 | 4.850 | 5.046 | 0.961 | 0.1 % | 0.4 % |
| kodim08/444/p0/d1 | 5.405 | 5.091 | 1.062 | 0.1 % | 0.0 % |
| kodim08/444/p0/d4 | 4.462 | 3.670 | 1.216 | 0.1 % | 0.2 % |
| kodim08/444/p2/d1 | 11.022 | 12.614 | 0.874 | 0.4 % | 0.3 % |
| kodim08/444/p2/d4 | 8.341 | 8.849 | 0.943 | 0.1 % | 0.1 % |
| kodim08/420/p0/d1 | 3.962 | 4.645 | 0.853 | 0.4 % | 0.4 % |
| kodim08/420/p0/d4 | 3.240 | 2.721 | 1.190 | 0.2 % | 0.1 % |
| kodim08/420/p2/d1 | 8.132 | 9.967 | 0.816 | 0.3 % | 0.1 % |
| kodim08/420/p2/d4 | 5.740 | 6.419 | 0.894 | 0.1 % | 0.2 % |
| kodim09/444/p0/d1 | 4.566 | 3.717 | 1.229 | 0.4 % | 0.1 % |
| kodim09/444/p0/d4 | 4.142 | 3.000 | 1.380 | 0.0 % | 0.3 % |
| kodim09/444/p2/d1 | 8.335 | 8.958 | 0.930 | 0.3 % | 0.2 % |
| kodim09/444/p2/d4 | 7.121 | 7.166 | 0.994 | 0.1 % | 0.4 % |
| kodim09/420/p0/d1 | 3.317 | 2.748 | 1.207 | 0.3 % | 0.1 % |
| kodim09/420/p0/d4 | 2.953 | 2.127 | 1.388 | 0.1 % | 0.2 % |
| kodim09/420/p2/d1 | 5.793 | 6.210 | 0.933 | 0.2 % | 0.2 % |
| kodim09/420/p2/d4 | 4.593 | 4.505 | 1.020 | 0.2 % | 0.2 % |
| kodim10/444/p0/d1 | 4.674 | 3.874 | 1.206 | 0.2 % | 0.1 % |
| kodim10/444/p0/d4 | 4.199 | 3.054 | 1.375 | 0.2 % | 0.1 % |
| kodim10/444/p2/d1 | 8.685 | 9.434 | 0.921 | 0.2 % | 0.2 % |
| kodim10/444/p2/d4 | 7.253 | 7.359 | 0.986 | 0.2 % | 0.1 % |
| kodim10/420/p0/d1 | 3.435 | 2.912 | 1.180 | 0.2 % | 0.1 % |
| kodim10/420/p0/d4 | 3.024 | 2.176 | 1.390 | 0.1 % | 0.2 % |
| kodim10/420/p2/d1 | 6.056 | 6.603 | 0.917 | 0.2 % | 0.3 % |
| kodim10/420/p2/d4 | 4.791 | 4.667 | 1.027 | 0.9 % | 0.3 % |
| kodim11/444/p0/d1 | 4.947 | 4.459 | 1.110 | 0.1 % | 0.1 % |
| kodim11/444/p0/d4 | 4.265 | 3.250 | 1.312 | 0.2 % | 0.0 % |
| kodim11/444/p2/d1 | 9.748 | 11.064 | 0.881 | 0.2 % | 0.6 % |
| kodim11/444/p2/d4 | 7.569 | 7.926 | 0.955 | 0.2 % | 0.4 % |
| kodim11/420/p0/d1 | 3.613 | 3.484 | 1.037 | 0.2 % | 0.1 % |
| kodim11/420/p0/d4 | 3.073 | 2.360 | 1.302 | 0.2 % | 0.4 % |
| kodim11/420/p2/d1 | 6.898 | 8.333 | 0.828 | 0.1 % | 0.2 % |
| kodim11/420/p2/d4 | 4.981 | 5.329 | 0.935 | 0.2 % | 0.6 % |
| kodim12/444/p0/d1 | 4.606 | 3.915 | 1.177 | 0.1 % | 0.1 % |
| kodim12/444/p0/d4 | 4.129 | 2.991 | 1.380 | 0.1 % | 0.1 % |
| kodim12/444/p2/d1 | 8.546 | 9.464 | 0.903 | 0.3 % | 0.2 % |
| kodim12/444/p2/d4 | 7.106 | 7.241 | 0.981 | 0.1 % | 1.1 % |
| kodim12/420/p0/d1 | 3.371 | 2.936 | 1.148 | 0.5 % | 0.2 % |
| kodim12/420/p0/d4 | 2.948 | 2.139 | 1.378 | 0.2 % | 0.6 % |
| kodim12/420/p2/d1 | 5.917 | 6.806 | 0.869 | 0.5 % | 0.3 % |
| kodim12/420/p2/d4 | 4.556 | 4.724 | 0.964 | 0.3 % | 0.5 % |
| kodim13/444/p0/d1 | 5.576 | 5.579 | 0.999 | 0.2 % | 0.2 % |
| kodim13/444/p0/d4 | 4.605 | 3.946 | 1.167 | 0.2 % | 0.3 % |
| kodim13/444/p2/d1 | 11.949 | 14.019 | 0.852 | 0.2 % | 0.2 % |
| kodim13/444/p2/d4 | 8.452 | 9.439 | 0.895 | 0.1 % | 0.3 % |
| kodim13/420/p0/d1 | 4.185 | 5.236 | 0.799 | 0.1 % | 0.1 % |
| kodim13/420/p0/d4 | 3.281 | 2.861 | 1.147 | 0.3 % | 0.2 % |
| kodim13/420/p2/d1 | 9.047 | 11.327 | 0.799 | 0.6 % | 0.2 % |
| kodim13/420/p2/d4 | 5.808 | 6.678 | 0.870 | 0.5 % | 0.1 % |
| kodim14/444/p0/d1 | 5.179 | 4.929 | 1.051 | 0.0 % | 0.1 % |
| kodim14/444/p0/d4 | 4.392 | 3.483 | 1.261 | 0.1 % | 0.1 % |
| kodim14/444/p2/d1 | 10.575 | 12.317 | 0.859 | 0.1 % | 0.1 % |
| kodim14/444/p2/d4 | 7.960 | 8.444 | 0.943 | 0.2 % | 0.4 % |
| kodim14/420/p0/d1 | 3.827 | 3.892 | 0.983 | 0.2 % | 0.2 % |
| kodim14/420/p0/d4 | 3.164 | 2.545 | 1.243 | 0.1 % | 0.2 % |
| kodim14/420/p2/d1 | 7.633 | 9.487 | 0.805 | 0.1 % | 0.2 % |
| kodim14/420/p2/d4 | 5.351 | 5.857 | 0.914 | 0.4 % | 0.2 % |
| kodim15/444/p0/d1 | 4.731 | 3.972 | 1.191 | 0.2 % | 0.1 % |
| kodim15/444/p0/d4 | 4.196 | 3.045 | 1.378 | 0.3 % | 0.0 % |
| kodim15/444/p2/d1 | 8.861 | 9.705 | 0.913 | 0.3 % | 0.3 % |
| kodim15/444/p2/d4 | 7.254 | 7.302 | 0.993 | 0.6 % | 0.0 % |
| kodim15/420/p0/d1 | 3.388 | 3.003 | 1.128 | 0.2 % | 0.1 % |
| kodim15/420/p0/d4 | 2.971 | 2.170 | 1.369 | 0.1 % | 0.1 % |
| kodim15/420/p2/d1 | 6.065 | 7.024 | 0.863 | 0.3 % | 0.2 % |
| kodim15/420/p2/d4 | 4.667 | 4.819 | 0.968 | 0.3 % | 0.3 % |
| kodim16/444/p0/d1 | 4.704 | 4.092 | 1.150 | 0.5 % | 0.1 % |
| kodim16/444/p0/d4 | 4.171 | 3.112 | 1.340 | 0.2 % | 0.6 % |
| kodim16/444/p2/d1 | 8.970 | 10.074 | 0.890 | 0.2 % | 0.2 % |
| kodim16/444/p2/d4 | 7.288 | 7.468 | 0.976 | 0.1 % | 0.5 % |
| kodim16/420/p0/d1 | 3.441 | 3.129 | 1.100 | 0.2 % | 0.1 % |
| kodim16/420/p0/d4 | 2.987 | 2.227 | 1.341 | 0.1 % | 0.3 % |
| kodim16/420/p2/d1 | 6.290 | 7.379 | 0.852 | 0.1 % | 0.1 % |
| kodim16/420/p2/d4 | 4.713 | 4.960 | 0.950 | 0.3 % | 0.2 % |
| kodim17/444/p0/d1 | 4.705 | 3.984 | 1.181 | 0.1 % | 0.1 % |
| kodim17/444/p0/d4 | 4.193 | 3.094 | 1.355 | 0.3 % | 0.2 % |
| kodim17/444/p2/d1 | 8.823 | 9.744 | 0.905 | 0.2 % | 0.4 % |
| kodim17/444/p2/d4 | 7.305 | 7.512 | 0.972 | 0.2 % | 0.5 % |
| kodim17/420/p0/d1 | 3.451 | 3.014 | 1.145 | 0.1 % | 0.2 % |
| kodim17/420/p0/d4 | 3.040 | 2.213 | 1.374 | 0.3 % | 0.1 % |
| kodim17/420/p2/d1 | 6.219 | 6.851 | 0.908 | 0.3 % | 0.2 % |
| kodim17/420/p2/d4 | 4.813 | 4.766 | 1.010 | 0.4 % | 0.3 % |
| kodim18/444/p0/d1 | 5.241 | 4.809 | 1.090 | 0.2 % | 0.1 % |
| kodim18/444/p0/d4 | 4.391 | 3.439 | 1.277 | 0.2 % | 0.1 % |
| kodim18/444/p2/d1 | 10.468 | 11.837 | 0.884 | 0.5 % | 0.2 % |
| kodim18/444/p2/d4 | 7.931 | 8.482 | 0.935 | 0.5 % | 0.1 % |
| kodim18/420/p0/d1 | 3.809 | 3.784 | 1.007 | 0.2 % | 0.3 % |
| kodim18/420/p0/d4 | 3.218 | 2.525 | 1.275 | 0.5 % | 0.2 % |
| kodim18/420/p2/d1 | 7.595 | 8.982 | 0.846 | 0.3 % | 0.1 % |
| kodim18/420/p2/d4 | 5.362 | 5.564 | 0.964 | 0.0 % | 0.4 % |
| kodim19/444/p0/d1 | 4.936 | 4.303 | 1.147 | 0.8 % | 0.7 % |
| kodim19/444/p0/d4 | 4.251 | 3.175 | 1.339 | 0.8 % | 0.3 % |
| kodim19/444/p2/d1 | 9.376 | 10.438 | 0.898 | 0.3 % | 0.1 % |
| kodim19/444/p2/d4 | 7.434 | 7.623 | 0.975 | 0.1 % | 0.3 % |
| kodim19/420/p0/d1 | 3.591 | 3.301 | 1.088 | 0.2 % | 0.2 % |
| kodim19/420/p0/d4 | 3.077 | 2.287 | 1.345 | 0.2 % | 0.5 % |
| kodim19/420/p2/d1 | 6.609 | 7.612 | 0.868 | 0.1 % | 0.3 % |
| kodim19/420/p2/d4 | 4.929 | 4.952 | 0.995 | 0.2 % | 0.3 % |
| kodim20/444/p0/d1 | 4.499 | 3.709 | 1.213 | 0.1 % | 0.2 % |
| kodim20/444/p0/d4 | 4.064 | 2.950 | 1.377 | 0.2 % | 0.1 % |
| kodim20/444/p2/d1 | 8.300 | 9.023 | 0.920 | 0.5 % | 0.3 % |
| kodim20/444/p2/d4 | 6.971 | 7.160 | 0.974 | 0.1 % | 0.2 % |
| kodim20/420/p0/d1 | 3.253 | 2.823 | 1.152 | 0.3 % | 0.8 % |
| kodim20/420/p0/d4 | 2.887 | 2.112 | 1.367 | 0.2 % | 0.2 % |
| kodim20/420/p2/d1 | 5.673 | 6.650 | 0.853 | 0.1 % | 0.3 % |
| kodim20/420/p2/d4 | 4.493 | 4.720 | 0.952 | 0.3 % | 0.5 % |
| kodim21/444/p0/d1 | 4.920 | 4.287 | 1.148 | 0.5 % | 0.1 % |
| kodim21/444/p0/d4 | 4.225 | 3.233 | 1.307 | 0.4 % | 0.2 % |
| kodim21/444/p2/d1 | 9.451 | 10.502 | 0.900 | 0.2 % | 0.2 % |
| kodim21/444/p2/d4 | 7.487 | 7.865 | 0.952 | 0.5 % | 0.3 % |
| kodim21/420/p0/d1 | 3.577 | 3.309 | 1.081 | 0.4 % | 0.4 % |
| kodim21/420/p0/d4 | 3.044 | 2.340 | 1.300 | 0.9 % | 0.2 % |
| kodim21/420/p2/d1 | 6.710 | 7.844 | 0.855 | 0.1 % | 0.2 % |
| kodim21/420/p2/d4 | 4.911 | 5.300 | 0.927 | 0.2 % | 0.3 % |
| kodim22/444/p0/d1 | 5.111 | 4.562 | 1.121 | 0.3 % | 0.1 % |
| kodim22/444/p0/d4 | 4.312 | 3.259 | 1.323 | 0.1 % | 0.1 % |
| kodim22/444/p2/d1 | 9.845 | 11.274 | 0.873 | 0.1 % | 0.2 % |
| kodim22/444/p2/d4 | 7.572 | 7.889 | 0.960 | 0.3 % | 0.3 % |
| kodim22/420/p0/d1 | 3.669 | 3.522 | 1.042 | 1.3 % | 0.1 % |
| kodim22/420/p0/d4 | 3.093 | 2.369 | 1.306 | 0.6 % | 0.1 % |
| kodim22/420/p2/d1 | 6.984 | 8.489 | 0.823 | 0.2 % | 0.4 % |
| kodim22/420/p2/d4 | 4.987 | 5.260 | 0.948 | 0.8 % | 0.4 % |
| kodim23/444/p0/d1 | 4.615 | 3.576 | 1.290 | 0.2 % | 0.1 % |
| kodim23/444/p0/d4 | 4.185 | 2.929 | 1.428 | 0.1 % | 0.0 % |
| kodim23/444/p2/d1 | 8.205 | 8.503 | 0.965 | 0.3 % | 0.4 % |
| kodim23/444/p2/d4 | 7.065 | 6.941 | 1.018 | 0.3 % | 0.8 % |
| kodim23/420/p0/d1 | 3.253 | 2.640 | 1.232 | 0.4 % | 0.3 % |
| kodim23/420/p0/d4 | 2.991 | 2.098 | 1.425 | 0.5 % | 0.2 % |
| kodim23/420/p2/d1 | 5.503 | 6.083 | 0.905 | 0.1 % | 0.1 % |
| kodim23/420/p2/d4 | 4.543 | 4.543 | 1.000 | 0.3 % | 0.5 % |
| kodim24/444/p0/d1 | 5.274 | 4.764 | 1.107 | 0.7 % | 0.1 % |
| kodim24/444/p0/d4 | 4.379 | 3.474 | 1.261 | 0.2 % | 0.1 % |
| kodim24/444/p2/d1 | 10.380 | 11.733 | 0.885 | 0.3 % | 0.3 % |
| kodim24/444/p2/d4 | 7.890 | 8.476 | 0.931 | 0.1 % | 0.3 % |
| kodim24/420/p0/d1 | 3.779 | 3.728 | 1.014 | 0.3 % | 0.2 % |
| kodim24/420/p0/d4 | 3.176 | 2.554 | 1.244 | 0.3 % | 0.4 % |
| kodim24/420/p2/d1 | 7.491 | 9.011 | 0.831 | 0.1 % | 0.4 % |
| kodim24/420/p2/d4 | 5.341 | 5.864 | 0.911 | 0.3 % | 0.2 % |
| smooth-gradient/444/p0/d1 | 0.592 | 0.367 | 1.612 | 0.6 % | 0.2 % |
| smooth-gradient/444/p0/d4 | 0.584 | 0.375 | 1.557 | 0.3 % | 0.5 % |
| smooth-gradient/444/p2/d1 | 0.961 | 0.692 | 1.389 | 0.9 % | 0.6 % |
| smooth-gradient/444/p2/d4 | 0.912 | 0.688 | 1.326 | 0.3 % | 0.4 % |
| smooth-gradient/420/p0/d1 | 0.433 | 0.294 | 1.475 | 1.0 % | 0.2 % |
| smooth-gradient/420/p0/d4 | 0.425 | 0.298 | 1.426 | 0.4 % | 0.5 % |
| smooth-gradient/420/p2/d1 | 0.615 | 0.444 | 1.387 | 0.3 % | 0.0 % |
| smooth-gradient/420/p2/d4 | 0.606 | 0.440 | 1.375 | 0.4 % | 0.2 % |
| low-contrast/444/p0/d1 | 0.719 | 0.506 | 1.419 | 0.3 % | 0.4 % |
| low-contrast/444/p0/d4 | 0.624 | 0.390 | 1.601 | 1.0 % | 0.2 % |
| low-contrast/444/p2/d1 | 1.346 | 1.169 | 1.151 | 0.4 % | 0.1 % |
| low-contrast/444/p2/d4 | 1.064 | 0.797 | 1.335 | 0.4 % | 0.4 % |
| low-contrast/420/p0/d1 | 0.527 | 0.438 | 1.203 | 0.2 % | 1.1 % |
| low-contrast/420/p0/d4 | 0.447 | 0.318 | 1.404 | 0.6 % | 0.2 % |
| low-contrast/420/p2/d1 | 0.921 | 0.839 | 1.098 | 0.4 % | 0.3 % |
| low-contrast/420/p2/d4 | 0.689 | 0.531 | 1.298 | 0.5 % | 0.4 % |
| color-edges/444/p0/d1 | 0.764 | 0.562 | 1.359 | 0.5 % | 0.2 % |
| color-edges/444/p0/d4 | 0.700 | 0.487 | 1.436 | 0.1 % | 0.1 % |
| color-edges/444/p2/d1 | 1.604 | 1.365 | 1.175 | 0.6 % | 0.6 % |
| color-edges/444/p2/d4 | 1.388 | 1.150 | 1.207 | 0.5 % | 0.1 % |
| color-edges/420/p0/d1 | 0.585 | 0.563 | 1.039 | 0.1 % | 0.3 % |
| color-edges/420/p0/d4 | 0.528 | 0.455 | 1.159 | 0.5 % | 0.4 % |
| color-edges/420/p2/d1 | 1.208 | 1.135 | 1.065 | 0.4 % | 0.2 % |
| color-edges/420/p2/d4 | 0.996 | 0.935 | 1.065 | 0.8 % | 0.2 % |
| line-art/444/p0/d1 | 0.711 | 0.536 | 1.325 | 0.5 % | 0.3 % |
| line-art/444/p0/d4 | 0.669 | 0.482 | 1.388 | 0.1 % | 0.2 % |
| line-art/444/p2/d1 | 1.403 | 1.142 | 1.229 | 0.0 % | 0.3 % |
| line-art/444/p2/d4 | 1.275 | 1.129 | 1.129 | 0.4 % | 0.1 % |
| line-art/420/p0/d1 | 0.554 | 0.502 | 1.104 | 0.5 % | 0.2 % |
| line-art/420/p0/d4 | 0.510 | 0.423 | 1.206 | 0.4 % | 0.1 % |
| line-art/420/p2/d1 | 1.073 | 0.891 | 1.205 | 0.6 % | 0.3 % |
| line-art/420/p2/d4 | 0.926 | 0.842 | 1.099 | 0.6 % | 0.2 % |
| structured-texture/444/p0/d1 | 1.023 | 1.234 | 0.829 | 0.3 % | 0.1 % |
| structured-texture/444/p0/d4 | 0.766 | 0.704 | 1.088 | 0.8 % | 0.1 % |
| structured-texture/444/p2/d1 | 2.614 | 3.118 | 0.838 | 0.2 % | 0.2 % |
| structured-texture/444/p2/d4 | 1.615 | 1.653 | 0.977 | 0.5 % | 0.1 % |
| structured-texture/420/p0/d1 | 0.698 | 0.917 | 0.761 | 0.1 % | 0.1 % |
| structured-texture/420/p0/d4 | 0.533 | 0.535 | 0.997 | 0.5 % | 0.5 % |
| structured-texture/420/p2/d1 | 1.661 | 2.028 | 0.819 | 0.3 % | 0.2 % |
| structured-texture/420/p2/d4 | 1.023 | 1.082 | 0.945 | 0.2 % | 0.4 % |
| low-noise/444/p0/d1 | 0.743 | 0.723 | 1.029 | 0.2 % | 0.1 % |
| low-noise/444/p0/d4 | 0.579 | 0.374 | 1.547 | 0.1 % | 0.2 % |
| low-noise/444/p2/d1 | 1.381 | 1.635 | 0.844 | 0.3 % | 0.2 % |
| low-noise/444/p2/d4 | 0.950 | 0.730 | 1.303 | 0.5 % | 0.4 % |
| low-noise/420/p0/d1 | 0.534 | 0.564 | 0.946 | 0.6 % | 0.3 % |
| low-noise/420/p0/d4 | 0.423 | 0.297 | 1.427 | 0.4 % | 0.2 % |
| low-noise/420/p2/d1 | 0.911 | 1.073 | 0.849 | 0.7 % | 0.2 % |
| low-noise/420/p2/d4 | 0.615 | 0.459 | 1.339 | 0.2 % | 0.2 % |
| full-noise/444/p0/d1 | 1.140 | 1.367 | 0.834 | 0.5 % | 0.1 % |
| full-noise/444/p0/d4 | 0.870 | 1.024 | 0.849 | 0.1 % | 0.2 % |
| full-noise/444/p2/d1 | 3.160 | 3.473 | 0.910 | 0.1 % | 0.6 % |
| full-noise/444/p2/d4 | 1.915 | 2.438 | 0.786 | 0.2 % | 0.0 % |
| full-noise/420/p0/d1 | 0.768 | 0.994 | 0.773 | 0.4 % | 0.2 % |
| full-noise/420/p0/d4 | 0.598 | 0.739 | 0.810 | 0.5 % | 0.1 % |
| full-noise/420/p2/d1 | 2.011 | 2.067 | 0.973 | 0.2 % | 0.2 % |
| full-noise/420/p2/d4 | 1.236 | 1.545 | 0.800 | 0.4 % | 0.3 % |
| tiny-odd-edges/444/p0/d1 | 0.039 | 0.026 | 1.489 | 0.9 % | 1.7 % |
| tiny-odd-edges/444/p0/d4 | 0.038 | 0.031 | 1.235 | 1.4 % | 0.4 % |
| tiny-odd-edges/444/p2/d1 | 0.072 | 0.053 | 1.353 | 2.0 % | 0.2 % |
| tiny-odd-edges/444/p2/d4 | 0.067 | 0.054 | 1.231 | 0.6 % | 0.5 % |
| tiny-odd-edges/420/p0/d1 | 0.037 | 0.024 | 1.523 | 0.5 % | 0.7 % |
| tiny-odd-edges/420/p0/d4 | 0.036 | 0.029 | 1.259 | 1.3 % | 0.4 % |
| tiny-odd-edges/420/p2/d1 | 0.067 | 0.049 | 1.356 | 0.7 % | 0.5 % |
| tiny-odd-edges/420/p2/d4 | 0.064 | 0.050 | 1.282 | 3.6 % | 0.6 % |

Exact input/JPEG files, SHA256 identities, per-block raw samples, p10/p50/p90, relative MAD, allocations/collections, calibration counts and native command outputs are retained alongside this report. summary.json records CPU, affinity, OS, runtime, vector widths, policy, revision and group verdicts.

Failed bounds:

- kodim01/444/p0/d1: median latency ratio 1.342020 exceeds 1.10.
- kodim01/444/p0/d4: median latency ratio 1.581874 exceeds 1.10.
- kodim01/420/p0/d4: median latency ratio 1.214969 exceeds 1.10.
- kodim02/444/p0/d1: median latency ratio 1.152334 exceeds 1.10.
- kodim02/444/p0/d4: median latency ratio 1.377836 exceeds 1.10.
- kodim02/420/p0/d4: median latency ratio 1.381436 exceeds 1.10.
- kodim03/444/p0/d1: median latency ratio 1.237222 exceeds 1.10.
- kodim03/444/p0/d4: median latency ratio 1.386816 exceeds 1.10.
- kodim03/420/p0/d1: median latency ratio 1.193488 exceeds 1.10.
- kodim03/420/p0/d4: median latency ratio 1.395679 exceeds 1.10.
- kodim04/444/p0/d1: median latency ratio 1.154963 exceeds 1.10.
- kodim04/444/p0/d4: median latency ratio 1.381453 exceeds 1.10.
- kodim04/420/p0/d1: median latency ratio 1.108229 exceeds 1.10.
- kodim04/420/p0/d4: median latency ratio 1.371994 exceeds 1.10.
- kodim05/444/p0/d4: median latency ratio 1.222607 exceeds 1.10.
- kodim05/420/p0/d4: median latency ratio 1.186511 exceeds 1.10.
- kodim06/444/p0/d4: median latency ratio 1.273660 exceeds 1.10.
- kodim06/420/p0/d4: median latency ratio 1.268267 exceeds 1.10.
- kodim07/444/p0/d1: median latency ratio 1.235892 exceeds 1.10.
- kodim07/444/p0/d4: median latency ratio 1.370297 exceeds 1.10.
- kodim07/420/p0/d1: median latency ratio 1.171226 exceeds 1.10.
- kodim07/420/p0/d4: median latency ratio 1.347386 exceeds 1.10.
- kodim08/444/p0/d4: median latency ratio 1.215764 exceeds 1.10.
- kodim08/420/p0/d4: median latency ratio 1.190416 exceeds 1.10.
- kodim09/444/p0/d1: median latency ratio 1.228555 exceeds 1.10.
- kodim09/444/p0/d4: median latency ratio 1.380417 exceeds 1.10.
- kodim09/420/p0/d1: median latency ratio 1.206827 exceeds 1.10.
- kodim09/420/p0/d4: median latency ratio 1.388426 exceeds 1.10.
- kodim10/444/p0/d1: median latency ratio 1.206487 exceeds 1.10.
- kodim10/444/p0/d4: median latency ratio 1.374900 exceeds 1.10.
- kodim10/420/p0/d1: median latency ratio 1.179724 exceeds 1.10.
- kodim10/420/p0/d4: median latency ratio 1.389791 exceeds 1.10.
- kodim11/444/p0/d1: median latency ratio 1.109604 exceeds 1.10.
- kodim11/444/p0/d4: median latency ratio 1.312086 exceeds 1.10.
- kodim11/420/p0/d4: median latency ratio 1.301923 exceeds 1.10.
- kodim12/444/p0/d1: median latency ratio 1.176669 exceeds 1.10.
- kodim12/444/p0/d4: median latency ratio 1.380497 exceeds 1.10.
- kodim12/420/p0/d1: median latency ratio 1.148057 exceeds 1.10.
- kodim12/420/p0/d4: median latency ratio 1.378479 exceeds 1.10.
- kodim13/444/p0/d4: median latency ratio 1.167217 exceeds 1.10.
- kodim13/420/p0/d4: median latency ratio 1.146916 exceeds 1.10.
- kodim14/444/p0/d4: median latency ratio 1.260923 exceeds 1.10.
- kodim14/420/p0/d4: median latency ratio 1.243461 exceeds 1.10.
- kodim15/444/p0/d1: median latency ratio 1.190922 exceeds 1.10.
- kodim15/444/p0/d4: median latency ratio 1.378074 exceeds 1.10.
- kodim15/420/p0/d1: median latency ratio 1.128331 exceeds 1.10.
- kodim15/420/p0/d4: median latency ratio 1.369017 exceeds 1.10.
- kodim16/444/p0/d1: median latency ratio 1.149769 exceeds 1.10.
- kodim16/444/p0/d4: median latency ratio 1.340021 exceeds 1.10.
- kodim16/420/p0/d4: median latency ratio 1.341020 exceeds 1.10.
- kodim17/444/p0/d1: median latency ratio 1.181022 exceeds 1.10.
- kodim17/444/p0/d4: median latency ratio 1.355202 exceeds 1.10.
- kodim17/420/p0/d1: median latency ratio 1.144947 exceeds 1.10.
- kodim17/420/p0/d4: median latency ratio 1.373904 exceeds 1.10.
- kodim18/444/p0/d4: median latency ratio 1.276732 exceeds 1.10.
- kodim18/420/p0/d4: median latency ratio 1.274589 exceeds 1.10.
- kodim19/444/p0/d1: median latency ratio 1.147203 exceeds 1.10.
- kodim19/444/p0/d4: median latency ratio 1.338897 exceeds 1.10.
- kodim19/420/p0/d4: median latency ratio 1.345158 exceeds 1.10.
- kodim20/444/p0/d1: median latency ratio 1.213156 exceeds 1.10.
- kodim20/444/p0/d4: median latency ratio 1.377457 exceeds 1.10.
- kodim20/420/p0/d1: median latency ratio 1.152378 exceeds 1.10.
- kodim20/420/p0/d4: median latency ratio 1.367400 exceeds 1.10.
- kodim21/444/p0/d1: median latency ratio 1.147631 exceeds 1.10.
- kodim21/444/p0/d4: median latency ratio 1.306951 exceeds 1.10.
- kodim21/420/p0/d4: median latency ratio 1.300496 exceeds 1.10.
- kodim22/444/p0/d1: median latency ratio 1.120521 exceeds 1.10.
- kodim22/444/p0/d4: median latency ratio 1.322981 exceeds 1.10.
- kodim22/420/p0/d4: median latency ratio 1.305841 exceeds 1.10.
- kodim23/444/p0/d1: median latency ratio 1.290457 exceeds 1.10.
- kodim23/444/p0/d4: median latency ratio 1.428489 exceeds 1.10.
- kodim23/420/p0/d1: median latency ratio 1.231998 exceeds 1.10.
- kodim23/420/p0/d4: median latency ratio 1.425277 exceeds 1.10.
- kodim24/444/p0/d1: median latency ratio 1.106998 exceeds 1.10.
- kodim24/444/p0/d4: median latency ratio 1.260555 exceeds 1.10.
- kodim24/420/p0/d4: median latency ratio 1.243688 exceeds 1.10.
- smooth-gradient/444/p0/d1: median latency ratio 1.612267 exceeds 1.10.
- smooth-gradient/444/p0/d4: median latency ratio 1.556538 exceeds 1.10.
- smooth-gradient/444/p2/d1: median latency ratio 1.388623 exceeds 1.10.
- smooth-gradient/444/p2/d4: median latency ratio 1.325789 exceeds 1.10.
- smooth-gradient/420/p0/d1: median latency ratio 1.474866 exceeds 1.10.
- smooth-gradient/420/p0/d4: median latency ratio 1.426203 exceeds 1.10.
- smooth-gradient/420/p2/d1: median latency ratio 1.386591 exceeds 1.10.
- smooth-gradient/420/p2/d4: median latency ratio 1.374662 exceeds 1.10.
- low-contrast/444/p0/d1: median latency ratio 1.418700 exceeds 1.10.
- low-contrast/444/p0/d4: median latency ratio 1.600628 exceeds 1.10.
- low-contrast/444/p2/d1: median latency ratio 1.151375 exceeds 1.10.
- low-contrast/444/p2/d4: median latency ratio 1.334659 exceeds 1.10.
- low-contrast/420/p0/d1: median latency ratio 1.203401 exceeds 1.10.
- low-contrast/420/p0/d4: median latency ratio 1.403671 exceeds 1.10.
- low-contrast/420/p2/d4: median latency ratio 1.297703 exceeds 1.10.
- color-edges/444/p0/d1: median latency ratio 1.359090 exceeds 1.10.
- color-edges/444/p0/d4: median latency ratio 1.435780 exceeds 1.10.
- color-edges/444/p2/d1: median latency ratio 1.175048 exceeds 1.10.
- color-edges/444/p2/d4: median latency ratio 1.206909 exceeds 1.10.
- color-edges/420/p0/d4: median latency ratio 1.159415 exceeds 1.10.
- line-art/444/p0/d1: median latency ratio 1.325404 exceeds 1.10.
- line-art/444/p0/d4: median latency ratio 1.388073 exceeds 1.10.
- line-art/444/p2/d1: median latency ratio 1.228597 exceeds 1.10.
- line-art/444/p2/d4: median latency ratio 1.129470 exceeds 1.10.
- line-art/420/p0/d1: median latency ratio 1.104225 exceeds 1.10.
- line-art/420/p0/d4: median latency ratio 1.206440 exceeds 1.10.
- line-art/420/p2/d1: median latency ratio 1.204526 exceeds 1.10.
- low-noise/444/p0/d4: median latency ratio 1.547389 exceeds 1.10.
- low-noise/444/p2/d4: median latency ratio 1.302675 exceeds 1.10.
- low-noise/420/p0/d4: median latency ratio 1.426598 exceeds 1.10.
- low-noise/420/p2/d4: median latency ratio 1.338988 exceeds 1.10.
- tiny-odd-edges/444/p0/d1: median latency ratio 1.489358 exceeds 1.10.
- tiny-odd-edges/444/p0/d4: median latency ratio 1.234697 exceeds 1.10.
- tiny-odd-edges/444/p2/d1: median latency ratio 1.352582 exceeds 1.10.
- tiny-odd-edges/444/p2/d4: median latency ratio 1.231409 exceeds 1.10.
- tiny-odd-edges/420/p0/d1: median latency ratio 1.523228 exceeds 1.10.
- tiny-odd-edges/420/p0/d4: median latency ratio 1.259403 exceeds 1.10.
- tiny-odd-edges/420/p2/d1: median latency ratio 1.356446 exceeds 1.10.
- tiny-odd-edges/420/p2/d4: median latency ratio 1.282378 exceeds 1.10.
- natural/444/p0: geometric mean latency ratio 1.240736 exceeds 1.05.
- natural/420/p0: geometric mean latency ratio 1.180139 exceeds 1.05.
- controlled/444/p0: geometric mean latency ratio 1.255787 exceeds 1.05.
- controlled/444/p2: geometric mean latency ratio 1.118154 exceeds 1.05.
- controlled/420/p0: geometric mean latency ratio 1.129530 exceeds 1.05.
- controlled/420/p2: geometric mean latency ratio 1.103792 exceeds 1.05.
