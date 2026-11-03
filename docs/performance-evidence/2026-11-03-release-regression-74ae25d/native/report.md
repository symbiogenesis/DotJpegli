# Performance parity: INVALID

256/256 cases: RGB8 YCbCr, Kodak24 plus eight fixed controls, distances1/4, 444/420 and progression0/2, one worker on this CI host. Pinned upstream `031a0077f5799a6041004267fc12b956c1f52a20`.
Tested revision: `74ae25d8a9d8ed5718466a3905131c37845706d3`.

All cases are measured even when a latency bound fails, so the report identifies every measured regression. A passing verdict requires complete, valid coverage and all fixed bounds.

Fixed latency noninferiority limits: geometric mean managed/native median latency ratio <=1.05 in each corpus/sampling/progression group; every case <=1.10. A faster case cannot excuse a slower case. No universal performance claim is made.

Seven alternating order blocks, five untimed warmups before each block. The native-only three-iteration calibration targets50ms and selects a shared count clamped3..100. Medians use all seven blocks; no outliers are discarded. Complete in-memory encode calls are timed individually and their elapsed times summed; subprocess startup, file I/O, verification and native buffer free are excluded. Managed collections inside encode are included. Tiered compilation is disabled.

Evidence is invalid if a block interval is below1ms, any case has relative MAD >10%, inputs/settings/coverage differ, or an untimed JPEG structure comparison fails. Invalid evidence cannot establish parity.

| Corpus | Sampling | Progression | Cases | Geometric mean ratio | Worst ratio | Verdict |
| --- | --- | --- | ---: | ---: | ---: | --- |
| natural | 444 | 0 | 48 | 1.197829 | 1.385433 | FAIL |
| natural | 444 | 2 | 48 | 0.911793 | 0.982446 | PASS |
| natural | 420 | 0 | 48 | 1.167912 | 1.401399 | FAIL |
| natural | 420 | 2 | 48 | 0.896439 | 1.010751 | INVALID |
| controlled | 444 | 0 | 16 | 1.232300 | 1.584098 | FAIL |
| controlled | 444 | 2 | 16 | 1.105614 | 1.339336 | FAIL |
| controlled | 420 | 0 | 16 | 1.108180 | 1.509739 | FAIL |
| controlled | 420 | 2 | 16 | 1.095641 | 1.376900 | FAIL |

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 5.214 | 5.106 | 1.021 | 0.6 % | 0.2 % |
| kodim01/444/p0/d4 | 4.351 | 3.599 | 1.209 | 0.7 % | 0.2 % |
| kodim01/444/p2/d1 | 10.995 | 12.875 | 0.854 | 0.8 % | 0.7 % |
| kodim01/444/p2/d4 | 8.084 | 8.839 | 0.915 | 1.1 % | 0.1 % |
| kodim01/420/p0/d1 | 3.968 | 4.105 | 0.967 | 0.4 % | 0.1 % |
| kodim01/420/p0/d4 | 3.156 | 2.655 | 1.189 | 0.4 % | 0.6 % |
| kodim01/420/p2/d1 | 8.122 | 10.022 | 0.810 | 0.1 % | 0.1 % |
| kodim01/420/p2/d4 | 5.455 | 6.103 | 0.894 | 0.2 % | 0.2 % |
| kodim02/444/p0/d1 | 4.893 | 4.303 | 1.137 | 0.2 % | 0.5 % |
| kodim02/444/p0/d4 | 4.168 | 3.074 | 1.356 | 0.4 % | 0.1 % |
| kodim02/444/p2/d1 | 9.188 | 10.379 | 0.885 | 0.2 % | 0.2 % |
| kodim02/444/p2/d4 | 7.129 | 7.314 | 0.975 | 0.3 % | 0.2 % |
| kodim02/420/p0/d1 | 3.506 | 3.297 | 1.063 | 0.2 % | 0.2 % |
| kodim02/420/p0/d4 | 2.955 | 2.178 | 1.357 | 0.2 % | 0.1 % |
| kodim02/420/p2/d1 | 6.320 | 7.665 | 0.825 | 0.6 % | 0.1 % |
| kodim02/420/p2/d4 | 4.611 | 4.825 | 0.956 | 0.1 % | 0.2 % |
| kodim03/444/p0/d1 | 4.413 | 3.666 | 1.204 | 0.2 % | 0.3 % |
| kodim03/444/p0/d4 | 4.009 | 2.977 | 1.346 | 0.1 % | 0.1 % |
| kodim03/444/p2/d1 | 8.147 | 8.930 | 0.912 | 0.3 % | 0.3 % |
| kodim03/444/p2/d4 | 6.884 | 7.159 | 0.962 | 0.2 % | 0.4 % |
| kodim03/420/p0/d1 | 3.236 | 2.746 | 1.179 | 0.2 % | 0.1 % |
| kodim03/420/p0/d4 | 2.916 | 2.126 | 1.371 | 0.1 % | 0.5 % |
| kodim03/420/p2/d1 | 5.692 | 6.507 | 0.875 | 0.2 % | 0.1 % |
| kodim03/420/p2/d4 | 4.517 | 4.715 | 0.958 | 0.2 % | 0.3 % |
| kodim04/444/p0/d1 | 4.748 | 4.198 | 1.131 | 0.1 % | 0.2 % |
| kodim04/444/p0/d4 | 4.162 | 3.094 | 1.345 | 0.1 % | 0.1 % |
| kodim04/444/p2/d1 | 8.997 | 10.231 | 0.879 | 0.1 % | 0.2 % |
| kodim04/444/p2/d4 | 7.201 | 7.416 | 0.971 | 0.1 % | 0.6 % |
| kodim04/420/p0/d1 | 3.499 | 3.206 | 1.092 | 0.2 % | 0.2 % |
| kodim04/420/p0/d4 | 3.008 | 2.217 | 1.357 | 0.1 % | 0.2 % |
| kodim04/420/p2/d1 | 6.331 | 7.293 | 0.868 | 0.3 % | 0.1 % |
| kodim04/420/p2/d4 | 4.743 | 4.717 | 1.005 | 0.1 % | 0.2 % |
| kodim05/444/p0/d1 | 5.276 | 5.026 | 1.050 | 0.3 % | 0.1 % |
| kodim05/444/p0/d4 | 4.405 | 3.703 | 1.190 | 0.2 % | 0.2 % |
| kodim05/444/p2/d1 | 11.050 | 12.585 | 0.878 | 0.1 % | 0.3 % |
| kodim05/444/p2/d4 | 8.250 | 9.000 | 0.917 | 0.1 % | 0.3 % |
| kodim05/420/p0/d1 | 3.911 | 4.627 | 0.845 | 0.4 % | 0.3 % |
| kodim05/420/p0/d4 | 3.274 | 2.763 | 1.185 | 0.5 % | 0.6 % |
| kodim05/420/p2/d1 | 8.276 | 10.026 | 0.825 | 0.8 % | 0.4 % |
| kodim05/420/p2/d4 | 8.041 | 8.246 | 0.975 | 11.6 % | 2.3 % |
| kodim06/444/p0/d1 | 4.935 | 4.510 | 1.094 | 0.8 % | 0.1 % |
| kodim06/444/p0/d4 | 4.149 | 3.334 | 1.244 | 0.3 % | 0.2 % |
| kodim06/444/p2/d1 | 9.724 | 11.134 | 0.873 | 0.3 % | 0.5 % |
| kodim06/444/p2/d4 | 7.510 | 8.188 | 0.917 | 0.1 % | 0.2 % |
| kodim06/420/p0/d1 | 3.635 | 3.547 | 1.025 | 0.2 % | 0.1 % |
| kodim06/420/p0/d4 | 3.045 | 2.437 | 1.249 | 0.2 % | 0.3 % |
| kodim06/420/p2/d1 | 7.071 | 8.485 | 0.833 | 0.2 % | 0.3 % |
| kodim06/420/p2/d4 | 5.043 | 5.539 | 0.911 | 0.2 % | 0.2 % |
| kodim07/444/p0/d1 | 4.583 | 3.810 | 1.203 | 0.1 % | 0.2 % |
| kodim07/444/p0/d4 | 4.116 | 3.094 | 1.330 | 0.2 % | 0.1 % |
| kodim07/444/p2/d1 | 8.653 | 9.290 | 0.931 | 0.2 % | 0.3 % |
| kodim07/444/p2/d4 | 7.219 | 7.496 | 0.963 | 0.2 % | 0.4 % |
| kodim07/420/p0/d1 | 3.356 | 2.881 | 1.165 | 0.2 % | 0.1 % |
| kodim07/420/p0/d4 | 2.985 | 2.240 | 1.332 | 0.3 % | 0.2 % |
| kodim07/420/p2/d1 | 6.140 | 6.810 | 0.902 | 0.7 % | 0.4 % |
| kodim07/420/p2/d4 | 4.828 | 5.065 | 0.953 | 0.2 % | 0.3 % |
| kodim08/444/p0/d1 | 5.251 | 5.101 | 1.029 | 0.5 % | 0.0 % |
| kodim08/444/p0/d4 | 4.374 | 3.686 | 1.187 | 0.4 % | 0.2 % |
| kodim08/444/p2/d1 | 10.905 | 12.666 | 0.861 | 0.1 % | 0.1 % |
| kodim08/444/p2/d4 | 8.191 | 8.955 | 0.915 | 0.3 % | 0.3 % |
| kodim08/420/p0/d1 | 3.923 | 4.686 | 0.837 | 0.2 % | 0.4 % |
| kodim08/420/p0/d4 | 3.236 | 2.725 | 1.187 | 0.1 % | 0.2 % |
| kodim08/420/p2/d1 | 8.218 | 9.963 | 0.825 | 0.2 % | 0.2 % |
| kodim08/420/p2/d4 | 5.730 | 6.375 | 0.899 | 0.3 % | 0.2 % |
| kodim09/444/p0/d1 | 4.441 | 3.719 | 1.194 | 0.1 % | 0.2 % |
| kodim09/444/p0/d4 | 4.062 | 2.994 | 1.357 | 0.3 % | 0.0 % |
| kodim09/444/p2/d1 | 8.171 | 8.937 | 0.914 | 0.5 % | 0.3 % |
| kodim09/444/p2/d4 | 6.953 | 7.172 | 0.969 | 0.3 % | 0.6 % |
| kodim09/420/p0/d1 | 3.303 | 2.753 | 1.200 | 1.0 % | 0.1 % |
| kodim09/420/p0/d4 | 2.936 | 2.126 | 1.380 | 0.2 % | 0.2 % |
| kodim09/420/p2/d1 | 5.752 | 6.197 | 0.928 | 0.6 % | 0.3 % |
| kodim09/420/p2/d4 | 4.538 | 4.529 | 1.002 | 0.1 % | 0.5 % |
| kodim10/444/p0/d1 | 4.575 | 3.888 | 1.177 | 0.0 % | 0.1 % |
| kodim10/444/p0/d4 | 4.113 | 3.065 | 1.342 | 0.3 % | 0.1 % |
| kodim10/444/p2/d1 | 8.555 | 9.456 | 0.905 | 0.1 % | 0.2 % |
| kodim10/444/p2/d4 | 7.088 | 7.352 | 0.964 | 0.4 % | 0.1 % |
| kodim10/420/p0/d1 | 3.397 | 2.923 | 1.162 | 0.1 % | 0.0 % |
| kodim10/420/p0/d4 | 2.985 | 2.187 | 1.365 | 0.0 % | 0.2 % |
| kodim10/420/p2/d1 | 6.033 | 6.626 | 0.910 | 0.1 % | 0.5 % |
| kodim10/420/p2/d4 | 4.729 | 4.679 | 1.011 | 0.7 % | 0.2 % |
| kodim11/444/p0/d1 | 4.839 | 4.477 | 1.081 | 0.1 % | 0.1 % |
| kodim11/444/p0/d4 | 4.143 | 3.246 | 1.276 | 0.3 % | 0.1 % |
| kodim11/444/p2/d1 | 9.556 | 10.999 | 0.869 | 0.3 % | 0.2 % |
| kodim11/444/p2/d4 | 7.391 | 7.887 | 0.937 | 0.2 % | 0.4 % |
| kodim11/420/p0/d1 | 3.560 | 3.475 | 1.025 | 0.1 % | 0.2 % |
| kodim11/420/p0/d4 | 3.051 | 2.351 | 1.298 | 0.6 % | 0.1 % |
| kodim11/420/p2/d1 | 6.958 | 8.414 | 0.827 | 1.3 % | 1.3 % |
| kodim11/420/p2/d4 | 4.983 | 5.417 | 0.920 | 0.7 % | 0.8 % |
| kodim12/444/p0/d1 | 4.510 | 3.924 | 1.150 | 0.1 % | 0.2 % |
| kodim12/444/p0/d4 | 4.040 | 3.005 | 1.344 | 0.6 % | 0.1 % |
| kodim12/444/p2/d1 | 8.423 | 9.528 | 0.884 | 0.3 % | 0.2 % |
| kodim12/444/p2/d4 | 6.902 | 7.233 | 0.954 | 0.3 % | 0.3 % |
| kodim12/420/p0/d1 | 3.344 | 2.950 | 1.134 | 0.2 % | 0.3 % |
| kodim12/420/p0/d4 | 2.904 | 2.140 | 1.357 | 0.2 % | 0.3 % |
| kodim12/420/p2/d1 | 5.885 | 6.831 | 0.861 | 0.0 % | 0.4 % |
| kodim12/420/p2/d4 | 4.519 | 4.769 | 0.948 | 0.1 % | 0.2 % |
| kodim13/444/p0/d1 | 5.494 | 5.595 | 0.982 | 0.1 % | 0.3 % |
| kodim13/444/p0/d4 | 4.399 | 3.887 | 1.132 | 0.3 % | 0.2 % |
| kodim13/444/p2/d1 | 11.838 | 14.023 | 0.844 | 0.2 % | 0.2 % |
| kodim13/444/p2/d4 | 8.344 | 9.475 | 0.881 | 0.3 % | 0.2 % |
| kodim13/420/p0/d1 | 4.142 | 5.296 | 0.782 | 0.1 % | 0.2 % |
| kodim13/420/p0/d4 | 3.267 | 2.880 | 1.135 | 0.2 % | 0.1 % |
| kodim13/420/p2/d1 | 9.056 | 11.333 | 0.799 | 0.7 % | 0.2 % |
| kodim13/420/p2/d4 | 5.775 | 6.769 | 0.853 | 0.2 % | 0.3 % |
| kodim14/444/p0/d1 | 5.156 | 4.936 | 1.045 | 0.2 % | 0.1 % |
| kodim14/444/p0/d4 | 4.302 | 3.489 | 1.233 | 0.3 % | 0.1 % |
| kodim14/444/p2/d1 | 10.470 | 12.466 | 0.840 | 0.4 % | 0.2 % |
| kodim14/444/p2/d4 | 7.853 | 8.535 | 0.920 | 0.2 % | 0.2 % |
| kodim14/420/p0/d1 | 3.796 | 3.918 | 0.969 | 0.3 % | 0.3 % |
| kodim14/420/p0/d4 | 3.142 | 2.545 | 1.235 | 0.2 % | 0.3 % |
| kodim14/420/p2/d1 | 7.648 | 9.481 | 0.807 | 0.3 % | 0.2 % |
| kodim14/420/p2/d4 | 5.311 | 5.880 | 0.903 | 0.3 % | 0.1 % |
| kodim15/444/p0/d1 | 4.586 | 3.966 | 1.156 | 0.1 % | 0.1 % |
| kodim15/444/p0/d4 | 4.058 | 3.040 | 1.335 | 0.1 % | 0.1 % |
| kodim15/444/p2/d1 | 8.614 | 9.633 | 0.894 | 0.3 % | 0.5 % |
| kodim15/444/p2/d4 | 7.030 | 7.360 | 0.955 | 0.1 % | 0.6 % |
| kodim15/420/p0/d1 | 3.364 | 2.992 | 1.124 | 0.2 % | 0.3 % |
| kodim15/420/p0/d4 | 2.942 | 2.166 | 1.358 | 0.2 % | 0.1 % |
| kodim15/420/p2/d1 | 6.030 | 7.023 | 0.859 | 0.3 % | 0.4 % |
| kodim15/420/p2/d4 | 4.607 | 4.831 | 0.954 | 0.2 % | 0.1 % |
| kodim16/444/p0/d1 | 4.604 | 4.079 | 1.129 | 0.3 % | 0.0 % |
| kodim16/444/p0/d4 | 4.027 | 3.100 | 1.299 | 0.1 % | 0.1 % |
| kodim16/444/p2/d1 | 8.811 | 10.055 | 0.876 | 0.4 % | 0.1 % |
| kodim16/444/p2/d4 | 7.088 | 7.495 | 0.946 | 0.6 % | 0.2 % |
| kodim16/420/p0/d1 | 3.402 | 3.122 | 1.090 | 0.1 % | 0.0 % |
| kodim16/420/p0/d4 | 2.956 | 2.211 | 1.337 | 0.3 % | 0.1 % |
| kodim16/420/p2/d1 | 6.274 | 7.366 | 0.852 | 0.4 % | 0.1 % |
| kodim16/420/p2/d4 | 4.672 | 4.952 | 0.943 | 0.2 % | 0.3 % |
| kodim17/444/p0/d1 | 4.648 | 3.994 | 1.164 | 0.2 % | 0.4 % |
| kodim17/444/p0/d4 | 4.074 | 3.102 | 1.313 | 0.2 % | 0.1 % |
| kodim17/444/p2/d1 | 8.688 | 9.672 | 0.898 | 0.3 % | 0.2 % |
| kodim17/444/p2/d4 | 7.139 | 7.542 | 0.947 | 0.3 % | 0.4 % |
| kodim17/420/p0/d1 | 3.473 | 3.032 | 1.145 | 0.9 % | 0.5 % |
| kodim17/420/p0/d4 | 2.999 | 2.210 | 1.357 | 0.3 % | 0.1 % |
| kodim17/420/p2/d1 | 6.343 | 7.046 | 0.900 | 0.5 % | 0.5 % |
| kodim17/420/p2/d4 | 4.769 | 4.804 | 0.993 | 0.4 % | 0.2 % |
| kodim18/444/p0/d1 | 5.241 | 4.802 | 1.091 | 0.1 % | 0.1 % |
| kodim18/444/p0/d4 | 4.284 | 3.447 | 1.243 | 0.4 % | 0.4 % |
| kodim18/444/p2/d1 | 10.398 | 11.804 | 0.881 | 0.2 % | 0.7 % |
| kodim18/444/p2/d4 | 7.802 | 8.511 | 0.917 | 0.6 % | 0.7 % |
| kodim18/420/p0/d1 | 3.776 | 3.782 | 0.998 | 0.5 % | 0.2 % |
| kodim18/420/p0/d4 | 3.225 | 2.521 | 1.279 | 1.2 % | 0.3 % |
| kodim18/420/p2/d1 | 7.574 | 8.979 | 0.843 | 0.3 % | 0.1 % |
| kodim18/420/p2/d4 | 5.278 | 5.590 | 0.944 | 0.1 % | 0.1 % |
| kodim19/444/p0/d1 | 4.792 | 4.287 | 1.118 | 0.2 % | 0.1 % |
| kodim19/444/p0/d4 | 4.112 | 3.177 | 1.294 | 0.3 % | 0.2 % |
| kodim19/444/p2/d1 | 9.214 | 10.504 | 0.877 | 0.4 % | 0.4 % |
| kodim19/444/p2/d4 | 7.267 | 7.699 | 0.944 | 0.5 % | 0.5 % |
| kodim19/420/p0/d1 | 3.561 | 3.303 | 1.078 | 0.3 % | 0.4 % |
| kodim19/420/p0/d4 | 3.027 | 2.286 | 1.324 | 0.2 % | 0.2 % |
| kodim19/420/p2/d1 | 6.611 | 7.641 | 0.865 | 0.1 % | 0.4 % |
| kodim19/420/p2/d4 | 4.872 | 4.949 | 0.984 | 0.3 % | 0.4 % |
| kodim20/444/p0/d1 | 4.375 | 3.716 | 1.178 | 0.5 % | 0.3 % |
| kodim20/444/p0/d4 | 3.924 | 2.953 | 1.329 | 0.3 % | 0.2 % |
| kodim20/444/p2/d1 | 8.104 | 9.046 | 0.896 | 0.2 % | 0.7 % |
| kodim20/444/p2/d4 | 6.763 | 7.121 | 0.950 | 0.0 % | 0.3 % |
| kodim20/420/p0/d1 | 3.199 | 2.783 | 1.150 | 0.3 % | 0.1 % |
| kodim20/420/p0/d4 | 2.834 | 2.098 | 1.351 | 0.3 % | 0.2 % |
| kodim20/420/p2/d1 | 5.628 | 6.589 | 0.854 | 0.3 % | 0.3 % |
| kodim20/420/p2/d4 | 4.419 | 4.697 | 0.941 | 0.1 % | 0.1 % |
| kodim21/444/p0/d1 | 4.773 | 4.284 | 1.114 | 0.2 % | 0.2 % |
| kodim21/444/p0/d4 | 4.077 | 3.239 | 1.259 | 0.1 % | 0.3 % |
| kodim21/444/p2/d1 | 9.226 | 10.401 | 0.887 | 0.2 % | 0.2 % |
| kodim21/444/p2/d4 | 7.270 | 7.856 | 0.925 | 0.4 % | 0.5 % |
| kodim21/420/p0/d1 | 3.528 | 3.304 | 1.068 | 0.0 % | 0.1 % |
| kodim21/420/p0/d4 | 2.982 | 2.337 | 1.276 | 0.2 % | 0.0 % |
| kodim21/420/p2/d1 | 6.645 | 7.832 | 0.848 | 0.1 % | 0.3 % |
| kodim21/420/p2/d4 | 4.863 | 5.340 | 0.911 | 0.2 % | 0.4 % |
| kodim22/444/p0/d1 | 4.978 | 4.559 | 1.092 | 0.1 % | 0.2 % |
| kodim22/444/p0/d4 | 4.173 | 3.259 | 1.281 | 0.2 % | 0.1 % |
| kodim22/444/p2/d1 | 9.672 | 11.194 | 0.864 | 0.3 % | 0.4 % |
| kodim22/444/p2/d4 | 7.429 | 7.942 | 0.935 | 0.6 % | 0.2 % |
| kodim22/420/p0/d1 | 3.620 | 3.532 | 1.025 | 0.3 % | 0.1 % |
| kodim22/420/p0/d4 | 3.073 | 2.369 | 1.297 | 0.1 % | 0.1 % |
| kodim22/420/p2/d1 | 6.987 | 8.547 | 0.817 | 0.2 % | 0.2 % |
| kodim22/420/p2/d4 | 4.956 | 5.377 | 0.922 | 0.1 % | 0.2 % |
| kodim23/444/p0/d1 | 4.499 | 3.583 | 1.256 | 0.2 % | 0.1 % |
| kodim23/444/p0/d4 | 4.067 | 2.936 | 1.385 | 0.2 % | 0.2 % |
| kodim23/444/p2/d1 | 8.034 | 8.472 | 0.948 | 0.2 % | 0.2 % |
| kodim23/444/p2/d4 | 6.843 | 6.966 | 0.982 | 0.2 % | 0.3 % |
| kodim23/420/p0/d1 | 3.236 | 2.625 | 1.233 | 0.5 % | 0.0 % |
| kodim23/420/p0/d4 | 2.953 | 2.107 | 1.401 | 0.2 % | 1.1 % |
| kodim23/420/p2/d1 | 5.583 | 6.180 | 0.903 | 1.1 % | 0.7 % |
| kodim23/420/p2/d4 | 4.481 | 4.574 | 0.979 | 0.4 % | 0.5 % |
| kodim24/444/p0/d1 | 5.100 | 4.756 | 1.072 | 0.2 % | 0.0 % |
| kodim24/444/p0/d4 | 4.261 | 3.479 | 1.225 | 0.7 % | 0.0 % |
| kodim24/444/p2/d1 | 10.321 | 11.736 | 0.879 | 0.1 % | 0.2 % |
| kodim24/444/p2/d4 | 7.772 | 8.539 | 0.910 | 0.2 % | 0.2 % |
| kodim24/420/p0/d1 | 3.804 | 3.731 | 1.020 | 0.5 % | 0.1 % |
| kodim24/420/p0/d4 | 3.135 | 2.551 | 1.229 | 0.2 % | 0.2 % |
| kodim24/420/p2/d1 | 7.537 | 9.058 | 0.832 | 0.1 % | 0.2 % |
| kodim24/420/p2/d4 | 5.285 | 5.917 | 0.893 | 0.3 % | 0.2 % |
| smooth-gradient/444/p0/d1 | 0.582 | 0.368 | 1.584 | 0.4 % | 0.1 % |
| smooth-gradient/444/p0/d4 | 0.553 | 0.369 | 1.499 | 0.3 % | 0.1 % |
| smooth-gradient/444/p2/d1 | 0.920 | 0.687 | 1.339 | 0.3 % | 0.3 % |
| smooth-gradient/444/p2/d4 | 0.885 | 0.681 | 1.301 | 0.6 % | 0.1 % |
| smooth-gradient/420/p0/d1 | 0.428 | 0.296 | 1.444 | 0.5 % | 0.4 % |
| smooth-gradient/420/p0/d4 | 0.418 | 0.298 | 1.399 | 0.2 % | 0.4 % |
| smooth-gradient/420/p2/d1 | 0.611 | 0.444 | 1.377 | 0.6 % | 0.1 % |
| smooth-gradient/420/p2/d4 | 0.589 | 0.441 | 1.334 | 0.6 % | 0.1 % |
| low-contrast/444/p0/d1 | 0.711 | 0.505 | 1.409 | 0.4 % | 0.2 % |
| low-contrast/444/p0/d4 | 0.606 | 0.390 | 1.555 | 0.2 % | 0.2 % |
| low-contrast/444/p2/d1 | 1.342 | 1.171 | 1.146 | 0.8 % | 0.3 % |
| low-contrast/444/p2/d4 | 1.049 | 0.798 | 1.314 | 0.9 % | 0.2 % |
| low-contrast/420/p0/d1 | 0.509 | 0.441 | 1.155 | 0.4 % | 0.4 % |
| low-contrast/420/p0/d4 | 0.439 | 0.318 | 1.383 | 0.3 % | 0.5 % |
| low-contrast/420/p2/d1 | 0.924 | 0.837 | 1.105 | 1.5 % | 0.3 % |
| low-contrast/420/p2/d4 | 0.684 | 0.529 | 1.292 | 0.4 % | 0.2 % |
| color-edges/444/p0/d1 | 0.747 | 0.560 | 1.334 | 0.4 % | 0.3 % |
| color-edges/444/p0/d4 | 0.686 | 0.487 | 1.411 | 0.2 % | 0.2 % |
| color-edges/444/p2/d1 | 1.589 | 1.353 | 1.174 | 1.0 % | 0.5 % |
| color-edges/444/p2/d4 | 1.365 | 1.154 | 1.183 | 0.3 % | 0.7 % |
| color-edges/420/p0/d1 | 0.577 | 0.558 | 1.033 | 0.5 % | 0.2 % |
| color-edges/420/p0/d4 | 0.512 | 0.452 | 1.132 | 0.1 % | 0.4 % |
| color-edges/420/p2/d1 | 1.204 | 1.134 | 1.062 | 0.7 % | 0.1 % |
| color-edges/420/p2/d4 | 0.983 | 0.928 | 1.059 | 0.4 % | 0.2 % |
| line-art/444/p0/d1 | 0.692 | 0.537 | 1.289 | 0.2 % | 0.2 % |
| line-art/444/p0/d4 | 0.649 | 0.487 | 1.333 | 0.7 % | 0.9 % |
| line-art/444/p2/d1 | 1.383 | 1.139 | 1.214 | 0.2 % | 0.4 % |
| line-art/444/p2/d4 | 1.250 | 1.131 | 1.105 | 0.1 % | 0.4 % |
| line-art/420/p0/d1 | 0.541 | 0.509 | 1.062 | 0.6 % | 0.5 % |
| line-art/420/p0/d4 | 0.500 | 0.423 | 1.181 | 0.9 % | 0.2 % |
| line-art/420/p2/d1 | 1.062 | 0.885 | 1.200 | 0.5 % | 0.1 % |
| line-art/420/p2/d4 | 0.907 | 0.839 | 1.081 | 0.1 % | 0.3 % |
| structured-texture/444/p0/d1 | 1.003 | 1.235 | 0.812 | 0.3 % | 0.0 % |
| structured-texture/444/p0/d4 | 0.751 | 0.704 | 1.067 | 0.4 % | 0.3 % |
| structured-texture/444/p2/d1 | 2.595 | 3.088 | 0.840 | 0.3 % | 0.0 % |
| structured-texture/444/p2/d4 | 1.608 | 1.656 | 0.971 | 0.8 % | 0.2 % |
| structured-texture/420/p0/d1 | 0.688 | 0.915 | 0.751 | 0.4 % | 0.3 % |
| structured-texture/420/p0/d4 | 0.520 | 0.534 | 0.974 | 0.5 % | 0.1 % |
| structured-texture/420/p2/d1 | 1.655 | 2.009 | 0.824 | 0.5 % | 0.1 % |
| structured-texture/420/p2/d4 | 1.016 | 1.078 | 0.943 | 0.4 % | 0.4 % |
| low-noise/444/p0/d1 | 0.737 | 0.722 | 1.021 | 0.3 % | 0.1 % |
| low-noise/444/p0/d4 | 0.567 | 0.376 | 1.510 | 0.4 % | 0.2 % |
| low-noise/444/p2/d1 | 1.363 | 1.633 | 0.835 | 0.5 % | 0.2 % |
| low-noise/444/p2/d4 | 0.926 | 0.733 | 1.263 | 0.5 % | 0.4 % |
| low-noise/420/p0/d1 | 0.526 | 0.565 | 0.932 | 0.3 % | 0.2 % |
| low-noise/420/p0/d4 | 0.413 | 0.296 | 1.392 | 0.6 % | 0.5 % |
| low-noise/420/p2/d1 | 0.907 | 1.076 | 0.843 | 1.0 % | 0.4 % |
| low-noise/420/p2/d4 | 0.609 | 0.462 | 1.317 | 0.5 % | 0.3 % |
| full-noise/444/p0/d1 | 1.126 | 1.370 | 0.822 | 0.3 % | 0.2 % |
| full-noise/444/p0/d4 | 0.861 | 1.023 | 0.842 | 0.3 % | 0.1 % |
| full-noise/444/p2/d1 | 3.155 | 3.430 | 0.920 | 0.4 % | 0.2 % |
| full-noise/444/p2/d4 | 1.897 | 2.412 | 0.787 | 0.2 % | 0.1 % |
| full-noise/420/p0/d1 | 0.754 | 0.997 | 0.756 | 0.5 % | 0.4 % |
| full-noise/420/p0/d4 | 0.591 | 0.739 | 0.799 | 0.5 % | 0.3 % |
| full-noise/420/p2/d1 | 2.008 | 2.041 | 0.983 | 0.1 % | 0.1 % |
| full-noise/420/p2/d4 | 1.227 | 1.528 | 0.803 | 0.2 % | 0.2 % |
| tiny-odd-edges/444/p0/d1 | 0.039 | 0.026 | 1.486 | 0.9 % | 0.4 % |
| tiny-odd-edges/444/p0/d4 | 0.039 | 0.032 | 1.225 | 2.1 % | 0.9 % |
| tiny-odd-edges/444/p2/d1 | 0.071 | 0.054 | 1.331 | 1.1 % | 0.1 % |
| tiny-odd-edges/444/p2/d4 | 0.068 | 0.055 | 1.231 | 3.3 % | 0.2 % |
| tiny-odd-edges/420/p0/d1 | 0.036 | 0.024 | 1.510 | 1.2 % | 0.6 % |
| tiny-odd-edges/420/p0/d4 | 0.037 | 0.029 | 1.262 | 1.6 % | 0.5 % |
| tiny-odd-edges/420/p2/d1 | 0.065 | 0.049 | 1.329 | 2.6 % | 0.5 % |
| tiny-odd-edges/420/p2/d4 | 0.062 | 0.050 | 1.244 | 0.3 % | 0.3 % |

Exact input/JPEG files, SHA256 identities, per-block raw samples, p10/p50/p90, relative MAD, allocations/collections, calibration counts and native command outputs are retained alongside this report. summary.json records CPU, affinity, OS, runtime, vector widths, policy, revision and group verdicts.

Invalid evidence:

- kodim05/420/p2/d4: managed relative MAD exceeds the fixed 10% noise bound.

Failed bounds:

- kodim01/444/p0/d4: median latency ratio 1.208971 exceeds 1.10.
- kodim01/420/p0/d4: median latency ratio 1.189055 exceeds 1.10.
- kodim02/444/p0/d1: median latency ratio 1.137303 exceeds 1.10.
- kodim02/444/p0/d4: median latency ratio 1.355807 exceeds 1.10.
- kodim02/420/p0/d4: median latency ratio 1.357010 exceeds 1.10.
- kodim03/444/p0/d1: median latency ratio 1.203891 exceeds 1.10.
- kodim03/444/p0/d4: median latency ratio 1.346495 exceeds 1.10.
- kodim03/420/p0/d1: median latency ratio 1.178614 exceeds 1.10.
- kodim03/420/p0/d4: median latency ratio 1.371240 exceeds 1.10.
- kodim04/444/p0/d1: median latency ratio 1.131125 exceeds 1.10.
- kodim04/444/p0/d4: median latency ratio 1.345067 exceeds 1.10.
- kodim04/420/p0/d4: median latency ratio 1.356995 exceeds 1.10.
- kodim05/444/p0/d4: median latency ratio 1.189623 exceeds 1.10.
- kodim05/420/p0/d4: median latency ratio 1.184847 exceeds 1.10.
- kodim06/444/p0/d4: median latency ratio 1.244489 exceeds 1.10.
- kodim06/420/p0/d4: median latency ratio 1.249393 exceeds 1.10.
- kodim07/444/p0/d1: median latency ratio 1.202757 exceeds 1.10.
- kodim07/444/p0/d4: median latency ratio 1.330426 exceeds 1.10.
- kodim07/420/p0/d1: median latency ratio 1.164803 exceeds 1.10.
- kodim07/420/p0/d4: median latency ratio 1.332323 exceeds 1.10.
- kodim08/444/p0/d4: median latency ratio 1.186693 exceeds 1.10.
- kodim08/420/p0/d4: median latency ratio 1.187389 exceeds 1.10.
- kodim09/444/p0/d1: median latency ratio 1.194140 exceeds 1.10.
- kodim09/444/p0/d4: median latency ratio 1.356761 exceeds 1.10.
- kodim09/420/p0/d1: median latency ratio 1.199902 exceeds 1.10.
- kodim09/420/p0/d4: median latency ratio 1.380495 exceeds 1.10.
- kodim10/444/p0/d1: median latency ratio 1.176629 exceeds 1.10.
- kodim10/444/p0/d4: median latency ratio 1.341931 exceeds 1.10.
- kodim10/420/p0/d1: median latency ratio 1.162173 exceeds 1.10.
- kodim10/420/p0/d4: median latency ratio 1.365136 exceeds 1.10.
- kodim11/444/p0/d4: median latency ratio 1.276318 exceeds 1.10.
- kodim11/420/p0/d4: median latency ratio 1.297673 exceeds 1.10.
- kodim12/444/p0/d1: median latency ratio 1.149575 exceeds 1.10.
- kodim12/444/p0/d4: median latency ratio 1.344228 exceeds 1.10.
- kodim12/420/p0/d1: median latency ratio 1.133685 exceeds 1.10.
- kodim12/420/p0/d4: median latency ratio 1.356765 exceeds 1.10.
- kodim13/444/p0/d4: median latency ratio 1.131768 exceeds 1.10.
- kodim13/420/p0/d4: median latency ratio 1.134537 exceeds 1.10.
- kodim14/444/p0/d4: median latency ratio 1.232969 exceeds 1.10.
- kodim14/420/p0/d4: median latency ratio 1.234586 exceeds 1.10.
- kodim15/444/p0/d1: median latency ratio 1.156383 exceeds 1.10.
- kodim15/444/p0/d4: median latency ratio 1.334747 exceeds 1.10.
- kodim15/420/p0/d1: median latency ratio 1.124248 exceeds 1.10.
- kodim15/420/p0/d4: median latency ratio 1.358365 exceeds 1.10.
- kodim16/444/p0/d1: median latency ratio 1.128633 exceeds 1.10.
- kodim16/444/p0/d4: median latency ratio 1.298724 exceeds 1.10.
- kodim16/420/p0/d4: median latency ratio 1.336908 exceeds 1.10.
- kodim17/444/p0/d1: median latency ratio 1.163709 exceeds 1.10.
- kodim17/444/p0/d4: median latency ratio 1.313286 exceeds 1.10.
- kodim17/420/p0/d1: median latency ratio 1.145159 exceeds 1.10.
- kodim17/420/p0/d4: median latency ratio 1.357160 exceeds 1.10.
- kodim18/444/p0/d4: median latency ratio 1.242956 exceeds 1.10.
- kodim18/420/p0/d4: median latency ratio 1.279074 exceeds 1.10.
- kodim19/444/p0/d1: median latency ratio 1.117746 exceeds 1.10.
- kodim19/444/p0/d4: median latency ratio 1.294047 exceeds 1.10.
- kodim19/420/p0/d4: median latency ratio 1.324044 exceeds 1.10.
- kodim20/444/p0/d1: median latency ratio 1.177551 exceeds 1.10.
- kodim20/444/p0/d4: median latency ratio 1.328826 exceeds 1.10.
- kodim20/420/p0/d1: median latency ratio 1.149571 exceeds 1.10.
- kodim20/420/p0/d4: median latency ratio 1.350724 exceeds 1.10.
- kodim21/444/p0/d1: median latency ratio 1.114156 exceeds 1.10.
- kodim21/444/p0/d4: median latency ratio 1.258763 exceeds 1.10.
- kodim21/420/p0/d4: median latency ratio 1.276390 exceeds 1.10.
- kodim22/444/p0/d4: median latency ratio 1.280521 exceeds 1.10.
- kodim22/420/p0/d4: median latency ratio 1.297381 exceeds 1.10.
- kodim23/444/p0/d1: median latency ratio 1.255769 exceeds 1.10.
- kodim23/444/p0/d4: median latency ratio 1.385433 exceeds 1.10.
- kodim23/420/p0/d1: median latency ratio 1.232878 exceeds 1.10.
- kodim23/420/p0/d4: median latency ratio 1.401399 exceeds 1.10.
- kodim24/444/p0/d4: median latency ratio 1.224883 exceeds 1.10.
- kodim24/420/p0/d4: median latency ratio 1.228667 exceeds 1.10.
- smooth-gradient/444/p0/d1: median latency ratio 1.584098 exceeds 1.10.
- smooth-gradient/444/p0/d4: median latency ratio 1.498529 exceeds 1.10.
- smooth-gradient/444/p2/d1: median latency ratio 1.339336 exceeds 1.10.
- smooth-gradient/444/p2/d4: median latency ratio 1.300793 exceeds 1.10.
- smooth-gradient/420/p0/d1: median latency ratio 1.444082 exceeds 1.10.
- smooth-gradient/420/p0/d4: median latency ratio 1.399000 exceeds 1.10.
- smooth-gradient/420/p2/d1: median latency ratio 1.376900 exceeds 1.10.
- smooth-gradient/420/p2/d4: median latency ratio 1.334268 exceeds 1.10.
- low-contrast/444/p0/d1: median latency ratio 1.408754 exceeds 1.10.
- low-contrast/444/p0/d4: median latency ratio 1.555305 exceeds 1.10.
- low-contrast/444/p2/d1: median latency ratio 1.146172 exceeds 1.10.
- low-contrast/444/p2/d4: median latency ratio 1.313842 exceeds 1.10.
- low-contrast/420/p0/d1: median latency ratio 1.154740 exceeds 1.10.
- low-contrast/420/p0/d4: median latency ratio 1.383132 exceeds 1.10.
- low-contrast/420/p2/d1: median latency ratio 1.104626 exceeds 1.10.
- low-contrast/420/p2/d4: median latency ratio 1.292479 exceeds 1.10.
- color-edges/444/p0/d1: median latency ratio 1.333775 exceeds 1.10.
- color-edges/444/p0/d4: median latency ratio 1.410651 exceeds 1.10.
- color-edges/444/p2/d1: median latency ratio 1.174449 exceeds 1.10.
- color-edges/444/p2/d4: median latency ratio 1.183019 exceeds 1.10.
- color-edges/420/p0/d4: median latency ratio 1.132044 exceeds 1.10.
- line-art/444/p0/d1: median latency ratio 1.288602 exceeds 1.10.
- line-art/444/p0/d4: median latency ratio 1.332768 exceeds 1.10.
- line-art/444/p2/d1: median latency ratio 1.213687 exceeds 1.10.
- line-art/444/p2/d4: median latency ratio 1.104930 exceeds 1.10.
- line-art/420/p0/d4: median latency ratio 1.181308 exceeds 1.10.
- line-art/420/p2/d1: median latency ratio 1.200295 exceeds 1.10.
- low-noise/444/p0/d4: median latency ratio 1.509565 exceeds 1.10.
- low-noise/444/p2/d4: median latency ratio 1.262786 exceeds 1.10.
- low-noise/420/p0/d4: median latency ratio 1.392285 exceeds 1.10.
- low-noise/420/p2/d4: median latency ratio 1.317129 exceeds 1.10.
- tiny-odd-edges/444/p0/d1: median latency ratio 1.486480 exceeds 1.10.
- tiny-odd-edges/444/p0/d4: median latency ratio 1.224656 exceeds 1.10.
- tiny-odd-edges/444/p2/d1: median latency ratio 1.330712 exceeds 1.10.
- tiny-odd-edges/444/p2/d4: median latency ratio 1.231224 exceeds 1.10.
- tiny-odd-edges/420/p0/d1: median latency ratio 1.509739 exceeds 1.10.
- tiny-odd-edges/420/p0/d4: median latency ratio 1.262144 exceeds 1.10.
- tiny-odd-edges/420/p2/d1: median latency ratio 1.328838 exceeds 1.10.
- tiny-odd-edges/420/p2/d4: median latency ratio 1.244135 exceeds 1.10.
- natural/444/p0: geometric mean latency ratio 1.197829 exceeds 1.05.
- natural/420/p0: geometric mean latency ratio 1.167912 exceeds 1.05.
- controlled/444/p0: geometric mean latency ratio 1.232300 exceeds 1.05.
- controlled/444/p2: geometric mean latency ratio 1.105614 exceeds 1.05.
- controlled/420/p0: geometric mean latency ratio 1.108180 exceeds 1.05.
- controlled/420/p2: geometric mean latency ratio 1.095641 exceeds 1.05.
