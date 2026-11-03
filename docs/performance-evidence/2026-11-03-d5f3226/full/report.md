# Performance parity: FAILED

256/256 cases: RGB8 YCbCr, Kodak24 plus eight fixed controls, distances1/4, 444/420 and progression0/2, one worker on this CI host. Pinned upstream `031a0077f5799a6041004267fc12b956c1f52a20`.
Tested revision: `d5f3226d9501a1b636f13eb15ff9b7113a767920`.

All cases are measured even when a latency bound fails, so the report identifies every measured regression. A passing verdict requires complete, valid coverage and all fixed bounds.

Fixed latency noninferiority limits: geometric mean managed/native median latency ratio <=1.05 in each corpus/sampling/progression group; every case <=1.10. A faster case cannot excuse a slower case. No universal performance claim is made.

Seven alternating order blocks, five untimed warmups before each block. The native-only three-iteration calibration targets50ms and selects a shared count clamped3..100. Medians use all seven blocks; no outliers are discarded. Complete in-memory encode calls are timed individually and their elapsed times summed; subprocess startup, file I/O, verification and native buffer free are excluded. Managed collections inside encode are included. Tiered compilation is disabled.

Evidence is invalid if a block interval is below1ms, any case has relative MAD >10%, inputs/settings/coverage differ, or an untimed JPEG structure comparison fails. Invalid evidence cannot establish parity.

| Corpus | Sampling | Progression | Cases | Geometric mean ratio | Worst ratio | Verdict |
| --- | --- | --- | ---: | ---: | ---: | --- |
| natural | 444 | 0 | 48 | 1.255241 | 1.859315 | FAIL |
| natural | 444 | 2 | 48 | 0.963052 | 1.137222 | FAIL |
| natural | 420 | 0 | 48 | 1.249793 | 1.678655 | FAIL |
| natural | 420 | 2 | 48 | 0.956783 | 1.103669 | FAIL |
| controlled | 444 | 0 | 16 | 1.251534 | 1.623458 | FAIL |
| controlled | 444 | 2 | 16 | 1.137176 | 1.382245 | FAIL |
| controlled | 420 | 0 | 16 | 1.179013 | 1.557864 | FAIL |
| controlled | 420 | 2 | 16 | 1.135566 | 1.428382 | FAIL |

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 5.593 | 5.262 | 1.063 | 0.7 % | 0.5 % |
| kodim01/444/p0/d4 | 4.896 | 3.659 | 1.338 | 3.5 % | 1.6 % |
| kodim01/444/p2/d1 | 17.913 | 17.625 | 1.016 | 1.3 % | 0.2 % |
| kodim01/444/p2/d4 | 13.714 | 12.685 | 1.081 | 0.1 % | 0.4 % |
| kodim01/420/p0/d1 | 7.186 | 5.401 | 1.330 | 0.5 % | 0.2 % |
| kodim01/420/p0/d4 | 5.751 | 3.426 | 1.679 | 0.4 % | 0.2 % |
| kodim01/420/p2/d1 | 13.286 | 13.318 | 0.998 | 0.3 % | 0.1 % |
| kodim01/420/p2/d4 | 9.385 | 8.504 | 1.104 | 0.3 % | 0.3 % |
| kodim02/444/p0/d1 | 8.657 | 5.549 | 1.560 | 0.2 % | 0.0 % |
| kodim02/444/p0/d4 | 7.218 | 3.882 | 1.859 | 0.2 % | 0.2 % |
| kodim02/444/p2/d1 | 15.521 | 14.562 | 1.066 | 0.1 % | 0.5 % |
| kodim02/444/p2/d4 | 12.237 | 10.760 | 1.137 | 0.2 % | 0.9 % |
| kodim02/420/p0/d1 | 4.056 | 3.526 | 1.150 | 1.8 % | 1.0 % |
| kodim02/420/p0/d4 | 3.330 | 2.275 | 1.464 | 0.1 % | 0.6 % |
| kodim02/420/p2/d1 | 6.766 | 7.625 | 0.887 | 0.1 % | 0.2 % |
| kodim02/420/p2/d4 | 4.944 | 4.792 | 1.032 | 0.1 % | 0.3 % |
| kodim03/444/p0/d1 | 4.817 | 3.899 | 1.235 | 0.1 % | 0.3 % |
| kodim03/444/p0/d4 | 4.359 | 3.102 | 1.405 | 0.2 % | 0.4 % |
| kodim03/444/p2/d1 | 8.651 | 9.025 | 0.959 | 0.7 % | 0.7 % |
| kodim03/444/p2/d4 | 7.256 | 7.196 | 1.008 | 0.1 % | 0.5 % |
| kodim03/420/p0/d1 | 3.633 | 2.951 | 1.231 | 0.3 % | 0.7 % |
| kodim03/420/p0/d4 | 3.225 | 2.206 | 1.462 | 0.1 % | 0.3 % |
| kodim03/420/p2/d1 | 6.132 | 6.542 | 0.937 | 0.8 % | 0.7 % |
| kodim03/420/p2/d4 | 4.809 | 4.697 | 1.024 | 0.1 % | 0.6 % |
| kodim04/444/p0/d1 | 5.214 | 4.485 | 1.162 | 0.4 % | 0.5 % |
| kodim04/444/p0/d4 | 4.539 | 3.247 | 1.398 | 0.5 % | 0.5 % |
| kodim04/444/p2/d1 | 9.521 | 10.336 | 0.921 | 0.4 % | 0.3 % |
| kodim04/444/p2/d4 | 7.667 | 7.583 | 1.011 | 0.7 % | 1.0 % |
| kodim04/420/p0/d1 | 3.961 | 3.455 | 1.146 | 0.3 % | 0.1 % |
| kodim04/420/p0/d4 | 3.411 | 2.312 | 1.475 | 0.3 % | 0.9 % |
| kodim04/420/p2/d1 | 6.813 | 7.386 | 0.922 | 0.5 % | 0.4 % |
| kodim04/420/p2/d4 | 5.105 | 4.720 | 1.082 | 0.4 % | 0.4 % |
| kodim05/444/p0/d1 | 5.695 | 5.475 | 1.040 | 0.2 % | 0.8 % |
| kodim05/444/p0/d4 | 4.830 | 3.924 | 1.231 | 0.3 % | 0.4 % |
| kodim05/444/p2/d1 | 11.660 | 12.825 | 0.909 | 0.4 % | 0.7 % |
| kodim05/444/p2/d4 | 8.685 | 9.064 | 0.958 | 0.4 % | 0.6 % |
| kodim05/420/p0/d1 | 4.410 | 4.830 | 0.913 | 0.3 % | 0.6 % |
| kodim05/420/p0/d4 | 3.663 | 2.921 | 1.254 | 0.1 % | 1.0 % |
| kodim05/420/p2/d1 | 8.694 | 10.035 | 0.866 | 0.2 % | 0.4 % |
| kodim05/420/p2/d4 | 6.154 | 6.507 | 0.946 | 0.2 % | 0.5 % |
| kodim06/444/p0/d1 | 5.283 | 4.856 | 1.088 | 0.5 % | 1.2 % |
| kodim06/444/p0/d4 | 4.472 | 3.484 | 1.284 | 0.2 % | 0.4 % |
| kodim06/444/p2/d1 | 10.115 | 11.183 | 0.905 | 0.1 % | 0.2 % |
| kodim06/444/p2/d4 | 7.899 | 8.169 | 0.967 | 0.4 % | 0.7 % |
| kodim06/420/p0/d1 | 4.121 | 3.845 | 1.072 | 0.6 % | 0.2 % |
| kodim06/420/p0/d4 | 3.382 | 2.548 | 1.327 | 0.2 % | 0.7 % |
| kodim06/420/p2/d1 | 7.515 | 8.521 | 0.882 | 0.3 % | 0.3 % |
| kodim06/420/p2/d4 | 5.401 | 5.566 | 0.970 | 0.7 % | 0.5 % |
| kodim07/444/p0/d1 | 5.029 | 4.063 | 1.238 | 0.6 % | 0.5 % |
| kodim07/444/p0/d4 | 4.491 | 3.250 | 1.382 | 0.5 % | 0.8 % |
| kodim07/444/p2/d1 | 9.149 | 9.369 | 0.977 | 0.6 % | 0.3 % |
| kodim07/444/p2/d4 | 7.560 | 7.543 | 1.002 | 0.2 % | 0.2 % |
| kodim07/420/p0/d1 | 3.736 | 3.091 | 1.209 | 0.2 % | 0.2 % |
| kodim07/420/p0/d4 | 3.335 | 2.325 | 1.434 | 0.2 % | 1.1 % |
| kodim07/420/p2/d1 | 6.508 | 6.809 | 0.956 | 0.5 % | 0.4 % |
| kodim07/420/p2/d4 | 5.277 | 5.137 | 1.027 | 0.5 % | 0.6 % |
| kodim08/444/p0/d1 | 5.728 | 5.525 | 1.037 | 0.3 % | 0.2 % |
| kodim08/444/p0/d4 | 4.802 | 3.853 | 1.246 | 0.4 % | 0.3 % |
| kodim08/444/p2/d1 | 11.671 | 12.939 | 0.902 | 1.4 % | 0.6 % |
| kodim08/444/p2/d4 | 8.671 | 9.034 | 0.960 | 0.5 % | 0.4 % |
| kodim08/420/p0/d1 | 4.358 | 4.945 | 0.881 | 0.4 % | 0.6 % |
| kodim08/420/p0/d4 | 3.664 | 2.916 | 1.257 | 0.3 % | 0.3 % |
| kodim08/420/p2/d1 | 8.591 | 10.040 | 0.856 | 0.7 % | 0.5 % |
| kodim08/420/p2/d4 | 6.183 | 6.449 | 0.959 | 0.6 % | 0.2 % |
| kodim09/444/p0/d1 | 4.906 | 3.912 | 1.254 | 0.2 % | 0.5 % |
| kodim09/444/p0/d4 | 4.363 | 3.080 | 1.416 | 0.2 % | 0.6 % |
| kodim09/444/p2/d1 | 8.658 | 9.043 | 0.957 | 0.3 % | 0.3 % |
| kodim09/444/p2/d4 | 7.262 | 7.212 | 1.007 | 0.3 % | 0.7 % |
| kodim09/420/p0/d1 | 3.717 | 2.932 | 1.267 | 0.2 % | 0.8 % |
| kodim09/420/p0/d4 | 3.327 | 2.220 | 1.499 | 0.8 % | 0.3 % |
| kodim09/420/p2/d1 | 6.137 | 6.238 | 0.984 | 0.5 % | 0.6 % |
| kodim09/420/p2/d4 | 4.865 | 4.521 | 1.076 | 0.5 % | 0.3 % |
| kodim10/444/p0/d1 | 5.096 | 4.124 | 1.236 | 0.9 % | 1.2 % |
| kodim10/444/p0/d4 | 4.530 | 3.171 | 1.429 | 0.3 % | 0.1 % |
| kodim10/444/p2/d1 | 9.057 | 9.549 | 0.948 | 0.6 % | 0.1 % |
| kodim10/444/p2/d4 | 7.466 | 7.408 | 1.008 | 0.2 % | 0.5 % |
| kodim10/420/p0/d1 | 3.835 | 3.118 | 1.230 | 0.2 % | 0.1 % |
| kodim10/420/p0/d4 | 3.366 | 2.277 | 1.478 | 0.1 % | 0.8 % |
| kodim10/420/p2/d1 | 6.496 | 6.656 | 0.976 | 0.4 % | 0.3 % |
| kodim10/420/p2/d4 | 5.061 | 4.689 | 1.079 | 0.3 % | 0.4 % |
| kodim11/444/p0/d1 | 5.377 | 4.822 | 1.115 | 0.6 % | 0.3 % |
| kodim11/444/p0/d4 | 4.589 | 3.378 | 1.358 | 0.3 % | 0.3 % |
| kodim11/444/p2/d1 | 10.047 | 11.051 | 0.909 | 0.5 % | 0.1 % |
| kodim11/444/p2/d4 | 7.814 | 7.926 | 0.986 | 0.8 % | 0.7 % |
| kodim11/420/p0/d1 | 4.093 | 3.759 | 1.089 | 0.8 % | 0.7 % |
| kodim11/420/p0/d4 | 3.393 | 2.479 | 1.369 | 0.3 % | 0.9 % |
| kodim11/420/p2/d1 | 7.400 | 8.373 | 0.884 | 0.8 % | 0.2 % |
| kodim11/420/p2/d4 | 5.303 | 5.362 | 0.989 | 0.2 % | 0.2 % |
| kodim12/444/p0/d1 | 5.010 | 4.167 | 1.202 | 1.0 % | 0.3 % |
| kodim12/444/p0/d4 | 4.422 | 3.138 | 1.409 | 0.3 % | 0.4 % |
| kodim12/444/p2/d1 | 8.902 | 9.616 | 0.926 | 0.6 % | 0.2 % |
| kodim12/444/p2/d4 | 7.285 | 7.269 | 1.002 | 0.3 % | 0.6 % |
| kodim12/420/p0/d1 | 3.760 | 3.151 | 1.193 | 0.3 % | 0.4 % |
| kodim12/420/p0/d4 | 3.232 | 2.235 | 1.446 | 0.6 % | 0.7 % |
| kodim12/420/p2/d1 | 6.264 | 6.854 | 0.914 | 0.1 % | 0.4 % |
| kodim12/420/p2/d4 | 4.821 | 4.739 | 1.017 | 0.4 % | 0.4 % |
| kodim13/444/p0/d1 | 5.829 | 6.225 | 0.936 | 0.5 % | 0.2 % |
| kodim13/444/p0/d4 | 4.862 | 4.131 | 1.177 | 0.3 % | 0.9 % |
| kodim13/444/p2/d1 | 12.117 | 14.097 | 0.860 | 0.3 % | 0.2 % |
| kodim13/444/p2/d4 | 8.787 | 9.506 | 0.924 | 0.7 % | 0.4 % |
| kodim13/420/p0/d1 | 4.573 | 5.718 | 0.800 | 0.4 % | 1.0 % |
| kodim13/420/p0/d4 | 3.681 | 3.073 | 1.198 | 0.3 % | 0.8 % |
| kodim13/420/p2/d1 | 9.402 | 11.519 | 0.816 | 0.4 % | 0.2 % |
| kodim13/420/p2/d4 | 6.202 | 6.895 | 0.899 | 0.3 % | 1.2 % |
| kodim14/444/p0/d1 | 5.638 | 5.293 | 1.065 | 0.7 % | 0.4 % |
| kodim14/444/p0/d4 | 4.726 | 3.638 | 1.299 | 0.5 % | 0.6 % |
| kodim14/444/p2/d1 | 11.154 | 12.582 | 0.886 | 0.7 % | 0.2 % |
| kodim14/444/p2/d4 | 8.317 | 8.687 | 0.957 | 0.0 % | 0.5 % |
| kodim14/420/p0/d1 | 4.251 | 4.253 | 1.000 | 0.2 % | 0.6 % |
| kodim14/420/p0/d4 | 3.555 | 2.681 | 1.326 | 1.0 % | 1.1 % |
| kodim14/420/p2/d1 | 8.149 | 9.565 | 0.852 | 0.5 % | 0.4 % |
| kodim14/420/p2/d4 | 5.666 | 5.918 | 0.957 | 0.4 % | 0.4 % |
| kodim15/444/p0/d1 | 5.161 | 4.249 | 1.215 | 0.8 % | 0.2 % |
| kodim15/444/p0/d4 | 4.430 | 3.144 | 1.409 | 0.2 % | 0.4 % |
| kodim15/444/p2/d1 | 9.137 | 9.683 | 0.944 | 0.5 % | 0.2 % |
| kodim15/444/p2/d4 | 7.589 | 7.489 | 1.013 | 1.3 % | 1.5 % |
| kodim15/420/p0/d1 | 3.785 | 3.195 | 1.185 | 0.1 % | 0.4 % |
| kodim15/420/p0/d4 | 3.296 | 2.263 | 1.456 | 0.2 % | 0.5 % |
| kodim15/420/p2/d1 | 6.419 | 7.013 | 0.915 | 0.2 % | 0.3 % |
| kodim15/420/p2/d4 | 4.922 | 4.848 | 1.015 | 0.3 % | 0.2 % |
| kodim16/444/p0/d1 | 5.071 | 4.384 | 1.157 | 1.0 % | 0.5 % |
| kodim16/444/p0/d4 | 4.419 | 3.211 | 1.376 | 0.2 % | 0.3 % |
| kodim16/444/p2/d1 | 9.162 | 10.141 | 0.903 | 0.3 % | 0.4 % |
| kodim16/444/p2/d4 | 7.443 | 7.471 | 0.996 | 0.9 % | 0.3 % |
| kodim16/420/p0/d1 | 3.784 | 3.358 | 1.127 | 0.3 % | 0.3 % |
| kodim16/420/p0/d4 | 3.254 | 2.324 | 1.400 | 0.2 % | 0.9 % |
| kodim16/420/p2/d1 | 6.663 | 7.451 | 0.894 | 0.5 % | 0.3 % |
| kodim16/420/p2/d4 | 4.949 | 4.990 | 0.992 | 0.3 % | 0.9 % |
| kodim17/444/p0/d1 | 5.130 | 4.235 | 1.211 | 1.6 % | 1.1 % |
| kodim17/444/p0/d4 | 4.566 | 3.255 | 1.402 | 0.2 % | 0.9 % |
| kodim17/444/p2/d1 | 9.300 | 9.902 | 0.939 | 1.4 % | 0.7 % |
| kodim17/444/p2/d4 | 7.621 | 7.605 | 1.002 | 1.8 % | 0.7 % |
| kodim17/420/p0/d1 | 3.921 | 3.262 | 1.202 | 1.7 % | 0.9 % |
| kodim17/420/p0/d4 | 3.428 | 2.355 | 1.455 | 0.2 % | 1.3 % |
| kodim17/420/p2/d1 | 6.696 | 7.031 | 0.952 | 1.9 % | 0.9 % |
| kodim17/420/p2/d4 | 5.228 | 4.920 | 1.063 | 1.3 % | 1.0 % |
| kodim18/444/p0/d1 | 5.764 | 5.277 | 1.092 | 0.5 % | 0.7 % |
| kodim18/444/p0/d4 | 4.763 | 3.627 | 1.313 | 1.1 % | 0.7 % |
| kodim18/444/p2/d1 | 11.206 | 12.083 | 0.927 | 1.5 % | 0.7 % |
| kodim18/444/p2/d4 | 8.303 | 8.669 | 0.958 | 0.9 % | 1.0 % |
| kodim18/420/p0/d1 | 4.249 | 4.159 | 1.022 | 0.3 % | 0.4 % |
| kodim18/420/p0/d4 | 3.557 | 2.660 | 1.337 | 0.2 % | 0.7 % |
| kodim18/420/p2/d1 | 8.029 | 9.099 | 0.882 | 0.2 % | 1.2 % |
| kodim18/420/p2/d4 | 5.686 | 5.625 | 1.011 | 0.2 % | 0.4 % |
| kodim19/444/p0/d1 | 5.202 | 4.610 | 1.128 | 0.3 % | 0.4 % |
| kodim19/444/p0/d4 | 4.462 | 3.336 | 1.338 | 0.4 % | 0.6 % |
| kodim19/444/p2/d1 | 9.715 | 10.596 | 0.917 | 1.0 % | 0.9 % |
| kodim19/444/p2/d4 | 7.699 | 7.771 | 0.991 | 0.8 % | 0.6 % |
| kodim19/420/p0/d1 | 4.029 | 3.551 | 1.135 | 0.2 % | 1.0 % |
| kodim19/420/p0/d4 | 3.391 | 2.399 | 1.413 | 0.1 % | 0.3 % |
| kodim19/420/p2/d1 | 7.123 | 7.748 | 0.919 | 1.1 % | 0.5 % |
| kodim19/420/p2/d4 | 5.223 | 4.986 | 1.047 | 0.4 % | 0.5 % |
| kodim20/444/p0/d1 | 4.705 | 3.957 | 1.189 | 0.3 % | 0.8 % |
| kodim20/444/p0/d4 | 4.313 | 3.087 | 1.397 | 0.8 % | 0.4 % |
| kodim20/444/p2/d1 | 8.506 | 9.162 | 0.928 | 0.4 % | 0.5 % |
| kodim20/444/p2/d4 | 7.108 | 7.259 | 0.979 | 0.6 % | 0.9 % |
| kodim20/420/p0/d1 | 3.546 | 2.976 | 1.191 | 0.2 % | 0.9 % |
| kodim20/420/p0/d4 | 3.148 | 2.158 | 1.459 | 0.3 % | 0.8 % |
| kodim20/420/p2/d1 | 5.952 | 6.643 | 0.896 | 0.4 % | 0.3 % |
| kodim20/420/p2/d4 | 4.739 | 4.742 | 0.999 | 0.5 % | 0.6 % |
| kodim21/444/p0/d1 | 5.265 | 4.581 | 1.149 | 1.2 % | 0.4 % |
| kodim21/444/p0/d4 | 4.433 | 3.366 | 1.317 | 0.2 % | 0.5 % |
| kodim21/444/p2/d1 | 9.781 | 10.523 | 0.930 | 1.6 % | 0.2 % |
| kodim21/444/p2/d4 | 7.814 | 8.096 | 0.965 | 0.8 % | 1.1 % |
| kodim21/420/p0/d1 | 3.951 | 3.597 | 1.099 | 0.4 % | 0.5 % |
| kodim21/420/p0/d4 | 3.305 | 2.440 | 1.355 | 0.2 % | 1.3 % |
| kodim21/420/p2/d1 | 6.991 | 7.802 | 0.896 | 0.2 % | 0.2 % |
| kodim21/420/p2/d4 | 5.163 | 5.328 | 0.969 | 0.2 % | 0.2 % |
| kodim22/444/p0/d1 | 5.443 | 4.910 | 1.109 | 0.3 % | 0.3 % |
| kodim22/444/p0/d4 | 4.589 | 3.389 | 1.354 | 0.4 % | 0.8 % |
| kodim22/444/p2/d1 | 10.110 | 11.158 | 0.906 | 0.2 % | 0.5 % |
| kodim22/444/p2/d4 | 7.766 | 7.861 | 0.988 | 0.3 % | 0.4 % |
| kodim22/420/p0/d1 | 4.101 | 3.802 | 1.079 | 0.2 % | 0.7 % |
| kodim22/420/p0/d4 | 3.434 | 2.442 | 1.406 | 0.1 % | 0.2 % |
| kodim22/420/p2/d1 | 7.429 | 8.462 | 0.878 | 0.5 % | 1.0 % |
| kodim22/420/p2/d4 | 5.310 | 5.294 | 1.003 | 0.4 % | 0.3 % |
| kodim23/444/p0/d1 | 4.939 | 3.805 | 1.298 | 0.2 % | 0.6 % |
| kodim23/444/p0/d4 | 4.402 | 3.050 | 1.443 | 0.1 % | 0.3 % |
| kodim23/444/p2/d1 | 8.494 | 8.561 | 0.992 | 0.3 % | 0.8 % |
| kodim23/444/p2/d4 | 7.178 | 6.990 | 1.027 | 0.3 % | 0.2 % |
| kodim23/420/p0/d1 | 3.647 | 2.770 | 1.316 | 0.4 % | 0.8 % |
| kodim23/420/p0/d4 | 3.258 | 2.173 | 1.500 | 0.2 % | 0.6 % |
| kodim23/420/p2/d1 | 5.849 | 6.089 | 0.961 | 0.2 % | 0.0 % |
| kodim23/420/p2/d4 | 4.814 | 4.625 | 1.041 | 0.9 % | 0.8 % |
| kodim24/444/p0/d1 | 5.578 | 5.186 | 1.076 | 0.9 % | 0.7 % |
| kodim24/444/p0/d4 | 4.661 | 3.657 | 1.275 | 0.1 % | 0.6 % |
| kodim24/444/p2/d1 | 10.635 | 11.838 | 0.898 | 0.4 % | 0.9 % |
| kodim24/444/p2/d4 | 8.308 | 8.840 | 0.940 | 0.8 % | 1.0 % |
| kodim24/420/p0/d1 | 4.218 | 4.078 | 1.034 | 1.1 % | 0.4 % |
| kodim24/420/p0/d4 | 3.546 | 2.692 | 1.317 | 0.3 % | 0.7 % |
| kodim24/420/p2/d1 | 7.961 | 9.147 | 0.870 | 0.6 % | 0.3 % |
| kodim24/420/p2/d4 | 5.682 | 5.976 | 0.951 | 0.2 % | 0.8 % |
| smooth-gradient/444/p0/d1 | 0.617 | 0.381 | 1.619 | 0.3 % | 0.8 % |
| smooth-gradient/444/p0/d4 | 0.591 | 0.380 | 1.557 | 0.2 % | 0.6 % |
| smooth-gradient/444/p2/d1 | 0.962 | 0.696 | 1.382 | 0.8 % | 0.4 % |
| smooth-gradient/444/p2/d4 | 0.919 | 0.684 | 1.342 | 0.1 % | 0.2 % |
| smooth-gradient/420/p0/d1 | 0.460 | 0.295 | 1.558 | 0.1 % | 0.1 % |
| smooth-gradient/420/p0/d4 | 0.454 | 0.299 | 1.518 | 0.2 % | 0.4 % |
| smooth-gradient/420/p2/d1 | 0.642 | 0.449 | 1.428 | 0.6 % | 0.2 % |
| smooth-gradient/420/p2/d4 | 0.620 | 0.443 | 1.398 | 0.5 % | 0.1 % |
| low-contrast/444/p0/d1 | 0.774 | 0.509 | 1.523 | 0.6 % | 0.2 % |
| low-contrast/444/p0/d4 | 0.658 | 0.405 | 1.623 | 0.6 % | 1.1 % |
| low-contrast/444/p2/d1 | 1.415 | 1.174 | 1.205 | 0.6 % | 0.4 % |
| low-contrast/444/p2/d4 | 1.084 | 0.799 | 1.356 | 0.6 % | 0.2 % |
| low-contrast/420/p0/d1 | 0.568 | 0.444 | 1.280 | 0.6 % | 0.9 % |
| low-contrast/420/p0/d4 | 0.477 | 0.320 | 1.493 | 0.4 % | 0.8 % |
| low-contrast/420/p2/d1 | 0.978 | 0.837 | 1.167 | 0.3 % | 0.3 % |
| low-contrast/420/p2/d4 | 0.723 | 0.529 | 1.366 | 0.6 % | 0.1 % |
| color-edges/444/p0/d1 | 0.783 | 0.589 | 1.329 | 0.3 % | 0.7 % |
| color-edges/444/p0/d4 | 0.725 | 0.522 | 1.389 | 0.4 % | 1.9 % |
| color-edges/444/p2/d1 | 1.611 | 1.369 | 1.176 | 0.4 % | 0.4 % |
| color-edges/444/p2/d4 | 1.399 | 1.145 | 1.223 | 0.2 % | 0.2 % |
| color-edges/420/p0/d1 | 0.616 | 0.568 | 1.086 | 1.0 % | 0.2 % |
| color-edges/420/p0/d4 | 0.554 | 0.457 | 1.211 | 0.7 % | 1.2 % |
| color-edges/420/p2/d1 | 1.234 | 1.139 | 1.084 | 0.4 % | 0.7 % |
| color-edges/420/p2/d4 | 1.024 | 0.930 | 1.101 | 0.2 % | 0.3 % |
| line-art/444/p0/d1 | 0.711 | 0.546 | 1.303 | 0.5 % | 0.8 % |
| line-art/444/p0/d4 | 0.669 | 0.488 | 1.371 | 0.6 % | 0.4 % |
| line-art/444/p2/d1 | 1.429 | 1.165 | 1.226 | 0.7 % | 0.9 % |
| line-art/444/p2/d4 | 1.264 | 1.134 | 1.114 | 0.3 % | 0.1 % |
| line-art/420/p0/d1 | 0.575 | 0.507 | 1.133 | 0.2 % | 0.6 % |
| line-art/420/p0/d4 | 0.531 | 0.421 | 1.259 | 0.6 % | 0.5 % |
| line-art/420/p2/d1 | 1.079 | 0.893 | 1.208 | 0.2 % | 0.5 % |
| line-art/420/p2/d4 | 0.936 | 0.842 | 1.111 | 0.2 % | 0.2 % |
| structured-texture/444/p0/d1 | 1.076 | 1.368 | 0.787 | 0.2 % | 1.2 % |
| structured-texture/444/p0/d4 | 0.838 | 0.754 | 1.112 | 0.5 % | 0.9 % |
| structured-texture/444/p2/d1 | 2.669 | 3.147 | 0.848 | 0.2 % | 0.2 % |
| structured-texture/444/p2/d4 | 1.688 | 1.659 | 1.018 | 0.6 % | 0.3 % |
| structured-texture/420/p0/d1 | 0.733 | 0.970 | 0.756 | 0.4 % | 0.8 % |
| structured-texture/420/p0/d4 | 0.582 | 0.533 | 1.092 | 0.2 % | 0.9 % |
| structured-texture/420/p2/d1 | 1.694 | 2.030 | 0.834 | 0.5 % | 0.2 % |
| structured-texture/420/p2/d4 | 1.085 | 1.077 | 1.008 | 0.5 % | 0.2 % |
| low-noise/444/p0/d1 | 0.802 | 0.774 | 1.035 | 0.5 % | 0.1 % |
| low-noise/444/p0/d4 | 0.605 | 0.382 | 1.583 | 0.1 % | 1.2 % |
| low-noise/444/p2/d1 | 1.455 | 1.635 | 0.890 | 0.3 % | 0.2 % |
| low-noise/444/p2/d4 | 0.969 | 0.733 | 1.322 | 0.8 % | 0.2 % |
| low-noise/420/p0/d1 | 0.584 | 0.579 | 1.008 | 1.1 % | 1.2 % |
| low-noise/420/p0/d4 | 0.455 | 0.301 | 1.514 | 0.8 % | 1.0 % |
| low-noise/420/p2/d1 | 0.958 | 1.076 | 0.891 | 0.0 % | 0.2 % |
| low-noise/420/p2/d4 | 0.642 | 0.461 | 1.393 | 0.8 % | 0.4 % |
| full-noise/444/p0/d1 | 1.193 | 1.519 | 0.786 | 1.0 % | 0.8 % |
| full-noise/444/p0/d4 | 0.924 | 1.109 | 0.834 | 0.5 % | 1.4 % |
| full-noise/444/p2/d1 | 3.239 | 3.521 | 0.920 | 0.3 % | 0.3 % |
| full-noise/444/p2/d4 | 1.964 | 2.430 | 0.808 | 0.3 % | 0.0 % |
| full-noise/420/p0/d1 | 0.802 | 1.018 | 0.788 | 0.3 % | 0.3 % |
| full-noise/420/p0/d4 | 0.640 | 0.751 | 0.852 | 0.5 % | 0.9 % |
| full-noise/420/p2/d1 | 2.051 | 2.089 | 0.982 | 0.2 % | 0.7 % |
| full-noise/420/p2/d4 | 1.272 | 1.535 | 0.829 | 0.3 % | 0.2 % |
| tiny-odd-edges/444/p0/d1 | 0.039 | 0.026 | 1.495 | 1.1 % | 0.3 % |
| tiny-odd-edges/444/p0/d4 | 0.039 | 0.031 | 1.257 | 1.1 % | 0.4 % |
| tiny-odd-edges/444/p2/d1 | 0.073 | 0.053 | 1.377 | 1.0 % | 0.3 % |
| tiny-odd-edges/444/p2/d4 | 0.069 | 0.055 | 1.263 | 1.2 % | 0.3 % |
| tiny-odd-edges/420/p0/d1 | 0.037 | 0.024 | 1.529 | 0.7 % | 0.8 % |
| tiny-odd-edges/420/p0/d4 | 0.037 | 0.029 | 1.272 | 0.9 % | 0.5 % |
| tiny-odd-edges/420/p2/d1 | 0.067 | 0.049 | 1.385 | 1.3 % | 0.5 % |
| tiny-odd-edges/420/p2/d4 | 0.064 | 0.050 | 1.276 | 1.5 % | 0.8 % |

Exact input/JPEG files, SHA256 identities, per-block raw samples, p10/p50/p90, relative MAD, allocations/collections, calibration counts and native command outputs are retained alongside this report. summary.json records CPU, affinity, OS, runtime, vector widths, policy, revision and group verdicts.

Failed bounds:

- kodim01/444/p0/d4: median latency ratio 1.337937 exceeds 1.10.
- kodim01/420/p0/d1: median latency ratio 1.330350 exceeds 1.10.
- kodim01/420/p0/d4: median latency ratio 1.678655 exceeds 1.10.
- kodim01/420/p2/d4: median latency ratio 1.103669 exceeds 1.10.
- kodim02/444/p0/d1: median latency ratio 1.559933 exceeds 1.10.
- kodim02/444/p0/d4: median latency ratio 1.859315 exceeds 1.10.
- kodim02/444/p2/d4: median latency ratio 1.137222 exceeds 1.10.
- kodim02/420/p0/d1: median latency ratio 1.150201 exceeds 1.10.
- kodim02/420/p0/d4: median latency ratio 1.463544 exceeds 1.10.
- kodim03/444/p0/d1: median latency ratio 1.235375 exceeds 1.10.
- kodim03/444/p0/d4: median latency ratio 1.405306 exceeds 1.10.
- kodim03/420/p0/d1: median latency ratio 1.231431 exceeds 1.10.
- kodim03/420/p0/d4: median latency ratio 1.461739 exceeds 1.10.
- kodim04/444/p0/d1: median latency ratio 1.162396 exceeds 1.10.
- kodim04/444/p0/d4: median latency ratio 1.397900 exceeds 1.10.
- kodim04/420/p0/d1: median latency ratio 1.146480 exceeds 1.10.
- kodim04/420/p0/d4: median latency ratio 1.475178 exceeds 1.10.
- kodim05/444/p0/d4: median latency ratio 1.230879 exceeds 1.10.
- kodim05/420/p0/d4: median latency ratio 1.254277 exceeds 1.10.
- kodim06/444/p0/d4: median latency ratio 1.283675 exceeds 1.10.
- kodim06/420/p0/d4: median latency ratio 1.327163 exceeds 1.10.
- kodim07/444/p0/d1: median latency ratio 1.237785 exceeds 1.10.
- kodim07/444/p0/d4: median latency ratio 1.381546 exceeds 1.10.
- kodim07/420/p0/d1: median latency ratio 1.208835 exceeds 1.10.
- kodim07/420/p0/d4: median latency ratio 1.434238 exceeds 1.10.
- kodim08/444/p0/d4: median latency ratio 1.246345 exceeds 1.10.
- kodim08/420/p0/d4: median latency ratio 1.256784 exceeds 1.10.
- kodim09/444/p0/d1: median latency ratio 1.254078 exceeds 1.10.
- kodim09/444/p0/d4: median latency ratio 1.416320 exceeds 1.10.
- kodim09/420/p0/d1: median latency ratio 1.267432 exceeds 1.10.
- kodim09/420/p0/d4: median latency ratio 1.498970 exceeds 1.10.
- kodim10/444/p0/d1: median latency ratio 1.235754 exceeds 1.10.
- kodim10/444/p0/d4: median latency ratio 1.428821 exceeds 1.10.
- kodim10/420/p0/d1: median latency ratio 1.229831 exceeds 1.10.
- kodim10/420/p0/d4: median latency ratio 1.478497 exceeds 1.10.
- kodim11/444/p0/d1: median latency ratio 1.115256 exceeds 1.10.
- kodim11/444/p0/d4: median latency ratio 1.358476 exceeds 1.10.
- kodim11/420/p0/d4: median latency ratio 1.369164 exceeds 1.10.
- kodim12/444/p0/d1: median latency ratio 1.202283 exceeds 1.10.
- kodim12/444/p0/d4: median latency ratio 1.409157 exceeds 1.10.
- kodim12/420/p0/d1: median latency ratio 1.192975 exceeds 1.10.
- kodim12/420/p0/d4: median latency ratio 1.445820 exceeds 1.10.
- kodim13/444/p0/d4: median latency ratio 1.176992 exceeds 1.10.
- kodim13/420/p0/d4: median latency ratio 1.198054 exceeds 1.10.
- kodim14/444/p0/d4: median latency ratio 1.298987 exceeds 1.10.
- kodim14/420/p0/d4: median latency ratio 1.326324 exceeds 1.10.
- kodim15/444/p0/d1: median latency ratio 1.214527 exceeds 1.10.
- kodim15/444/p0/d4: median latency ratio 1.409019 exceeds 1.10.
- kodim15/420/p0/d1: median latency ratio 1.184837 exceeds 1.10.
- kodim15/420/p0/d4: median latency ratio 1.456006 exceeds 1.10.
- kodim16/444/p0/d1: median latency ratio 1.156636 exceeds 1.10.
- kodim16/444/p0/d4: median latency ratio 1.376176 exceeds 1.10.
- kodim16/420/p0/d1: median latency ratio 1.127137 exceeds 1.10.
- kodim16/420/p0/d4: median latency ratio 1.400291 exceeds 1.10.
- kodim17/444/p0/d1: median latency ratio 1.211198 exceeds 1.10.
- kodim17/444/p0/d4: median latency ratio 1.402452 exceeds 1.10.
- kodim17/420/p0/d1: median latency ratio 1.202100 exceeds 1.10.
- kodim17/420/p0/d4: median latency ratio 1.455291 exceeds 1.10.
- kodim18/444/p0/d4: median latency ratio 1.313344 exceeds 1.10.
- kodim18/420/p0/d4: median latency ratio 1.337258 exceeds 1.10.
- kodim19/444/p0/d1: median latency ratio 1.128273 exceeds 1.10.
- kodim19/444/p0/d4: median latency ratio 1.337629 exceeds 1.10.
- kodim19/420/p0/d1: median latency ratio 1.134602 exceeds 1.10.
- kodim19/420/p0/d4: median latency ratio 1.413405 exceeds 1.10.
- kodim20/444/p0/d1: median latency ratio 1.189023 exceeds 1.10.
- kodim20/444/p0/d4: median latency ratio 1.397239 exceeds 1.10.
- kodim20/420/p0/d1: median latency ratio 1.191190 exceeds 1.10.
- kodim20/420/p0/d4: median latency ratio 1.458688 exceeds 1.10.
- kodim21/444/p0/d1: median latency ratio 1.149412 exceeds 1.10.
- kodim21/444/p0/d4: median latency ratio 1.317046 exceeds 1.10.
- kodim21/420/p0/d4: median latency ratio 1.354560 exceeds 1.10.
- kodim22/444/p0/d1: median latency ratio 1.108634 exceeds 1.10.
- kodim22/444/p0/d4: median latency ratio 1.354082 exceeds 1.10.
- kodim22/420/p0/d4: median latency ratio 1.406041 exceeds 1.10.
- kodim23/444/p0/d1: median latency ratio 1.298170 exceeds 1.10.
- kodim23/444/p0/d4: median latency ratio 1.443234 exceeds 1.10.
- kodim23/420/p0/d1: median latency ratio 1.316475 exceeds 1.10.
- kodim23/420/p0/d4: median latency ratio 1.499786 exceeds 1.10.
- kodim24/444/p0/d4: median latency ratio 1.274538 exceeds 1.10.
- kodim24/420/p0/d4: median latency ratio 1.317333 exceeds 1.10.
- smooth-gradient/444/p0/d1: median latency ratio 1.618703 exceeds 1.10.
- smooth-gradient/444/p0/d4: median latency ratio 1.556881 exceeds 1.10.
- smooth-gradient/444/p2/d1: median latency ratio 1.382245 exceeds 1.10.
- smooth-gradient/444/p2/d4: median latency ratio 1.342481 exceeds 1.10.
- smooth-gradient/420/p0/d1: median latency ratio 1.557864 exceeds 1.10.
- smooth-gradient/420/p0/d4: median latency ratio 1.517748 exceeds 1.10.
- smooth-gradient/420/p2/d1: median latency ratio 1.428382 exceeds 1.10.
- smooth-gradient/420/p2/d4: median latency ratio 1.397898 exceeds 1.10.
- low-contrast/444/p0/d1: median latency ratio 1.522645 exceeds 1.10.
- low-contrast/444/p0/d4: median latency ratio 1.623458 exceeds 1.10.
- low-contrast/444/p2/d1: median latency ratio 1.204913 exceeds 1.10.
- low-contrast/444/p2/d4: median latency ratio 1.356059 exceeds 1.10.
- low-contrast/420/p0/d1: median latency ratio 1.279883 exceeds 1.10.
- low-contrast/420/p0/d4: median latency ratio 1.493358 exceeds 1.10.
- low-contrast/420/p2/d1: median latency ratio 1.167401 exceeds 1.10.
- low-contrast/420/p2/d4: median latency ratio 1.366048 exceeds 1.10.
- color-edges/444/p0/d1: median latency ratio 1.328851 exceeds 1.10.
- color-edges/444/p0/d4: median latency ratio 1.388599 exceeds 1.10.
- color-edges/444/p2/d1: median latency ratio 1.176499 exceeds 1.10.
- color-edges/444/p2/d4: median latency ratio 1.222563 exceeds 1.10.
- color-edges/420/p0/d4: median latency ratio 1.211381 exceeds 1.10.
- color-edges/420/p2/d4: median latency ratio 1.100724 exceeds 1.10.
- line-art/444/p0/d1: median latency ratio 1.302522 exceeds 1.10.
- line-art/444/p0/d4: median latency ratio 1.371420 exceeds 1.10.
- line-art/444/p2/d1: median latency ratio 1.225983 exceeds 1.10.
- line-art/444/p2/d4: median latency ratio 1.114081 exceeds 1.10.
- line-art/420/p0/d1: median latency ratio 1.132894 exceeds 1.10.
- line-art/420/p0/d4: median latency ratio 1.259169 exceeds 1.10.
- line-art/420/p2/d1: median latency ratio 1.207900 exceeds 1.10.
- line-art/420/p2/d4: median latency ratio 1.111103 exceeds 1.10.
- structured-texture/444/p0/d4: median latency ratio 1.112281 exceeds 1.10.
- low-noise/444/p0/d4: median latency ratio 1.582757 exceeds 1.10.
- low-noise/444/p2/d4: median latency ratio 1.321985 exceeds 1.10.
- low-noise/420/p0/d4: median latency ratio 1.513584 exceeds 1.10.
- low-noise/420/p2/d4: median latency ratio 1.392741 exceeds 1.10.
- tiny-odd-edges/444/p0/d1: median latency ratio 1.494815 exceeds 1.10.
- tiny-odd-edges/444/p0/d4: median latency ratio 1.256853 exceeds 1.10.
- tiny-odd-edges/444/p2/d1: median latency ratio 1.376945 exceeds 1.10.
- tiny-odd-edges/444/p2/d4: median latency ratio 1.262778 exceeds 1.10.
- tiny-odd-edges/420/p0/d1: median latency ratio 1.529469 exceeds 1.10.
- tiny-odd-edges/420/p0/d4: median latency ratio 1.271775 exceeds 1.10.
- tiny-odd-edges/420/p2/d1: median latency ratio 1.384693 exceeds 1.10.
- tiny-odd-edges/420/p2/d4: median latency ratio 1.276107 exceeds 1.10.
- natural/444/p0: geometric mean latency ratio 1.255241 exceeds 1.05.
- natural/420/p0: geometric mean latency ratio 1.249793 exceeds 1.05.
- controlled/444/p0: geometric mean latency ratio 1.251534 exceeds 1.05.
- controlled/444/p2: geometric mean latency ratio 1.137176 exceeds 1.05.
- controlled/420/p0: geometric mean latency ratio 1.179013 exceeds 1.05.
- controlled/420/p2: geometric mean latency ratio 1.135566 exceeds 1.05.
