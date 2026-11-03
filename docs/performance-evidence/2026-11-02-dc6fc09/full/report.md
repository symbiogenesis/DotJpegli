# Performance parity: FAILED

256/256 cases: RGB8 YCbCr, Kodak24 plus eight fixed controls, distances1/4, 444/420 and progression0/2, one worker on this CI host. Pinned upstream `031a0077f5799a6041004267fc12b956c1f52a20`.
Tested revision: `dc6fc09595dc3a6296abe99575b38e8fae449d82`.

All cases are measured even when a latency bound fails, so the report identifies every measured regression. A passing verdict requires complete, valid coverage and all fixed bounds.

Fixed latency noninferiority limits: geometric mean managed/native median latency ratio <=1.05 in each corpus/sampling/progression group; every case <=1.10. A faster case cannot excuse a slower case. No universal performance claim is made.

Seven alternating order blocks, five untimed warmups before each block. The native-only three-iteration calibration targets50ms and selects a shared count clamped3..100. Medians use all seven blocks; no outliers are discarded. Complete in-memory encode calls are timed individually and their elapsed times summed; subprocess startup, file I/O, verification and native buffer free are excluded. Managed collections inside encode are included. Tiered compilation is disabled.

Evidence is invalid if a block interval is below1ms, any case has relative MAD >10%, inputs/settings/coverage differ, or an untimed JPEG structure comparison fails. Invalid evidence cannot establish parity.

| Corpus | Sampling | Progression | Cases | Geometric mean ratio | Worst ratio | Verdict |
| --- | --- | --- | ---: | ---: | ---: | --- |
| natural | 444 | 0 | 48 | 1.371131 | 1.591347 | FAIL |
| natural | 444 | 2 | 48 | 0.906043 | 0.971202 | PASS |
| natural | 420 | 0 | 48 | 1.318178 | 1.600221 | FAIL |
| natural | 420 | 2 | 48 | 0.893823 | 1.012114 | PASS |
| controlled | 444 | 0 | 16 | 1.326253 | 1.802404 | FAIL |
| controlled | 444 | 2 | 16 | 1.112651 | 1.412825 | FAIL |
| controlled | 420 | 0 | 16 | 1.185855 | 1.584124 | FAIL |
| controlled | 420 | 2 | 16 | 1.108397 | 1.432138 | FAIL |

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 5.722 | 5.096 | 1.123 | 0.7 % | 0.0 % |
| kodim01/444/p0/d4 | 5.064 | 3.599 | 1.407 | 0.9 % | 0.1 % |
| kodim01/444/p2/d1 | 10.894 | 12.841 | 0.848 | 0.3 % | 0.7 % |
| kodim01/444/p2/d4 | 8.049 | 8.805 | 0.914 | 0.5 % | 0.3 % |
| kodim01/420/p0/d1 | 4.351 | 4.092 | 1.063 | 0.5 % | 0.2 % |
| kodim01/420/p0/d4 | 3.608 | 2.616 | 1.379 | 0.3 % | 0.2 % |
| kodim01/420/p2/d1 | 8.001 | 9.902 | 0.808 | 0.1 % | 0.1 % |
| kodim01/420/p2/d4 | 5.439 | 6.056 | 0.898 | 0.1 % | 0.1 % |
| kodim02/444/p0/d1 | 5.546 | 4.260 | 1.302 | 0.4 % | 0.1 % |
| kodim02/444/p0/d4 | 4.798 | 3.066 | 1.565 | 0.2 % | 0.4 % |
| kodim02/444/p2/d1 | 9.048 | 10.344 | 0.875 | 1.0 % | 0.1 % |
| kodim02/444/p2/d4 | 7.066 | 7.308 | 0.967 | 0.4 % | 0.2 % |
| kodim02/420/p0/d1 | 4.012 | 3.267 | 1.228 | 0.2 % | 0.1 % |
| kodim02/420/p0/d4 | 3.397 | 2.177 | 1.561 | 0.2 % | 0.1 % |
| kodim02/420/p2/d1 | 6.350 | 7.633 | 0.832 | 0.6 % | 0.2 % |
| kodim02/420/p2/d4 | 4.626 | 4.782 | 0.968 | 0.2 % | 0.4 % |
| kodim03/444/p0/d1 | 5.101 | 3.668 | 1.391 | 0.4 % | 0.3 % |
| kodim03/444/p0/d4 | 4.614 | 2.976 | 1.550 | 0.2 % | 0.1 % |
| kodim03/444/p2/d1 | 8.182 | 8.922 | 0.917 | 0.2 % | 0.4 % |
| kodim03/444/p2/d4 | 6.846 | 7.179 | 0.954 | 0.2 % | 0.1 % |
| kodim03/420/p0/d1 | 3.678 | 2.762 | 1.332 | 0.3 % | 0.2 % |
| kodim03/420/p0/d4 | 3.292 | 2.130 | 1.545 | 0.4 % | 0.3 % |
| kodim03/420/p2/d1 | 5.675 | 6.501 | 0.873 | 0.2 % | 0.2 % |
| kodim03/420/p2/d4 | 4.525 | 4.740 | 0.955 | 1.2 % | 0.4 % |
| kodim04/444/p0/d1 | 5.540 | 4.198 | 1.320 | 0.1 % | 0.1 % |
| kodim04/444/p0/d4 | 4.913 | 3.091 | 1.589 | 0.2 % | 0.2 % |
| kodim04/444/p2/d1 | 8.956 | 10.282 | 0.871 | 1.2 % | 0.1 % |
| kodim04/444/p2/d4 | 7.187 | 7.406 | 0.970 | 0.3 % | 0.2 % |
| kodim04/420/p0/d1 | 3.970 | 3.213 | 1.236 | 0.3 % | 0.1 % |
| kodim04/420/p0/d4 | 3.586 | 2.280 | 1.572 | 1.3 % | 0.7 % |
| kodim04/420/p2/d1 | 6.364 | 7.335 | 0.868 | 0.3 % | 0.1 % |
| kodim04/420/p2/d4 | 4.789 | 4.731 | 1.012 | 0.4 % | 0.4 % |
| kodim05/444/p0/d1 | 5.918 | 5.023 | 1.178 | 0.2 % | 0.1 % |
| kodim05/444/p0/d4 | 5.086 | 3.706 | 1.372 | 0.7 % | 0.1 % |
| kodim05/444/p2/d1 | 11.063 | 12.749 | 0.868 | 0.3 % | 0.4 % |
| kodim05/444/p2/d4 | 8.313 | 9.094 | 0.914 | 0.8 % | 0.8 % |
| kodim05/420/p0/d1 | 4.381 | 4.659 | 0.940 | 0.4 % | 0.2 % |
| kodim05/420/p0/d4 | 3.741 | 2.782 | 1.345 | 0.3 % | 0.2 % |
| kodim05/420/p2/d1 | 8.279 | 10.038 | 0.825 | 0.3 % | 0.0 % |
| kodim05/420/p2/d4 | 5.809 | 6.535 | 0.889 | 1.7 % | 0.2 % |
| kodim06/444/p0/d1 | 5.532 | 4.523 | 1.223 | 0.4 % | 0.1 % |
| kodim06/444/p0/d4 | 4.774 | 3.342 | 1.429 | 0.4 % | 0.1 % |
| kodim06/444/p2/d1 | 9.593 | 11.093 | 0.865 | 0.5 % | 0.2 % |
| kodim06/444/p2/d4 | 7.463 | 8.176 | 0.913 | 0.3 % | 0.2 % |
| kodim06/420/p0/d1 | 4.027 | 3.562 | 1.131 | 0.3 % | 0.3 % |
| kodim06/420/p0/d4 | 3.429 | 2.449 | 1.401 | 0.4 % | 0.5 % |
| kodim06/420/p2/d1 | 7.013 | 8.482 | 0.827 | 0.5 % | 0.3 % |
| kodim06/420/p2/d4 | 5.055 | 5.549 | 0.911 | 0.4 % | 0.2 % |
| kodim07/444/p0/d1 | 5.212 | 3.808 | 1.369 | 0.1 % | 0.2 % |
| kodim07/444/p0/d4 | 4.709 | 3.096 | 1.521 | 0.2 % | 0.1 % |
| kodim07/444/p2/d1 | 8.570 | 9.295 | 0.922 | 0.6 % | 0.3 % |
| kodim07/444/p2/d4 | 7.181 | 7.506 | 0.957 | 0.1 % | 0.5 % |
| kodim07/420/p0/d1 | 3.767 | 2.878 | 1.309 | 0.3 % | 0.2 % |
| kodim07/420/p0/d4 | 3.394 | 2.234 | 1.519 | 0.4 % | 0.2 % |
| kodim07/420/p2/d1 | 6.083 | 6.805 | 0.894 | 0.4 % | 0.6 % |
| kodim07/420/p2/d4 | 4.829 | 5.047 | 0.957 | 0.4 % | 0.4 % |
| kodim08/444/p0/d1 | 5.859 | 5.097 | 1.149 | 0.6 % | 0.1 % |
| kodim08/444/p0/d4 | 5.111 | 3.678 | 1.389 | 0.4 % | 0.1 % |
| kodim08/444/p2/d1 | 10.832 | 12.715 | 0.852 | 0.3 % | 0.4 % |
| kodim08/444/p2/d4 | 8.204 | 8.877 | 0.924 | 1.4 % | 0.3 % |
| kodim08/420/p0/d1 | 4.371 | 4.717 | 0.927 | 0.7 % | 1.0 % |
| kodim08/420/p0/d4 | 3.699 | 2.741 | 1.349 | 0.3 % | 0.3 % |
| kodim08/420/p2/d1 | 8.101 | 9.991 | 0.811 | 0.9 % | 0.2 % |
| kodim08/420/p2/d4 | 5.732 | 6.395 | 0.896 | 0.3 % | 0.1 % |
| kodim09/444/p0/d1 | 5.170 | 3.721 | 1.389 | 0.2 % | 0.1 % |
| kodim09/444/p0/d4 | 4.696 | 2.992 | 1.570 | 0.5 % | 0.0 % |
| kodim09/444/p2/d1 | 8.199 | 8.928 | 0.918 | 0.1 % | 0.2 % |
| kodim09/444/p2/d4 | 6.930 | 7.136 | 0.971 | 0.6 % | 0.2 % |
| kodim09/420/p0/d1 | 3.752 | 2.747 | 1.366 | 0.2 % | 0.1 % |
| kodim09/420/p0/d4 | 3.293 | 2.126 | 1.549 | 0.5 % | 0.3 % |
| kodim09/420/p2/d1 | 5.736 | 6.183 | 0.928 | 0.4 % | 0.3 % |
| kodim09/420/p2/d4 | 4.518 | 4.494 | 1.005 | 0.2 % | 0.2 % |
| kodim10/444/p0/d1 | 5.256 | 3.891 | 1.351 | 0.1 % | 0.3 % |
| kodim10/444/p0/d4 | 4.753 | 3.060 | 1.553 | 0.2 % | 0.2 % |
| kodim10/444/p2/d1 | 8.546 | 9.375 | 0.912 | 0.3 % | 0.5 % |
| kodim10/444/p2/d4 | 7.193 | 7.491 | 0.960 | 0.5 % | 0.7 % |
| kodim10/420/p0/d1 | 3.867 | 2.921 | 1.324 | 0.4 % | 0.1 % |
| kodim10/420/p0/d4 | 3.411 | 2.187 | 1.560 | 0.2 % | 0.1 % |
| kodim10/420/p2/d1 | 6.080 | 6.618 | 0.919 | 0.2 % | 0.3 % |
| kodim10/420/p2/d4 | 4.720 | 4.710 | 1.002 | 0.1 % | 0.5 % |
| kodim11/444/p0/d1 | 5.442 | 4.477 | 1.215 | 0.6 % | 0.3 % |
| kodim11/444/p0/d4 | 4.803 | 3.244 | 1.481 | 0.4 % | 0.1 % |
| kodim11/444/p2/d1 | 9.483 | 10.990 | 0.863 | 0.8 % | 0.2 % |
| kodim11/444/p2/d4 | 7.351 | 7.877 | 0.933 | 0.5 % | 0.2 % |
| kodim11/420/p0/d1 | 3.998 | 3.485 | 1.147 | 0.2 % | 0.2 % |
| kodim11/420/p0/d4 | 3.431 | 2.351 | 1.459 | 0.3 % | 0.1 % |
| kodim11/420/p2/d1 | 6.844 | 8.320 | 0.823 | 0.1 % | 0.1 % |
| kodim11/420/p2/d4 | 4.958 | 5.328 | 0.931 | 0.2 % | 0.4 % |
| kodim12/444/p0/d1 | 5.199 | 3.914 | 1.328 | 0.3 % | 0.1 % |
| kodim12/444/p0/d4 | 4.666 | 2.999 | 1.556 | 0.1 % | 0.1 % |
| kodim12/444/p2/d1 | 8.327 | 9.518 | 0.875 | 0.9 % | 0.1 % |
| kodim12/444/p2/d4 | 6.910 | 7.185 | 0.962 | 0.3 % | 0.5 % |
| kodim12/420/p0/d1 | 3.807 | 2.946 | 1.292 | 1.1 % | 0.1 % |
| kodim12/420/p0/d4 | 3.282 | 2.146 | 1.529 | 0.5 % | 0.3 % |
| kodim12/420/p2/d1 | 5.842 | 6.826 | 0.856 | 0.3 % | 0.1 % |
| kodim12/420/p2/d4 | 4.483 | 4.736 | 0.947 | 0.2 % | 0.2 % |
| kodim13/444/p0/d1 | 5.951 | 5.613 | 1.060 | 0.5 % | 0.5 % |
| kodim13/444/p0/d4 | 5.057 | 3.879 | 1.304 | 0.1 % | 0.2 % |
| kodim13/444/p2/d1 | 11.727 | 14.136 | 0.830 | 0.4 % | 0.3 % |
| kodim13/444/p2/d4 | 8.365 | 9.512 | 0.879 | 0.3 % | 0.5 % |
| kodim13/420/p0/d1 | 4.494 | 5.318 | 0.845 | 0.3 % | 0.2 % |
| kodim13/420/p0/d4 | 3.694 | 2.887 | 1.279 | 0.5 % | 0.4 % |
| kodim13/420/p2/d1 | 9.012 | 11.447 | 0.787 | 0.5 % | 0.4 % |
| kodim13/420/p2/d4 | 5.792 | 6.809 | 0.851 | 0.6 % | 0.3 % |
| kodim14/444/p0/d1 | 5.766 | 4.960 | 1.163 | 0.5 % | 0.1 % |
| kodim14/444/p0/d4 | 4.953 | 3.495 | 1.417 | 0.0 % | 0.2 % |
| kodim14/444/p2/d1 | 10.442 | 12.491 | 0.836 | 0.3 % | 0.5 % |
| kodim14/444/p2/d4 | 7.888 | 8.564 | 0.921 | 0.4 % | 0.4 % |
| kodim14/420/p0/d1 | 4.275 | 3.922 | 1.090 | 0.9 % | 0.2 % |
| kodim14/420/p0/d4 | 3.606 | 2.570 | 1.403 | 0.7 % | 0.3 % |
| kodim14/420/p2/d1 | 7.871 | 9.680 | 0.813 | 0.5 % | 0.4 % |
| kodim14/420/p2/d4 | 5.409 | 6.027 | 0.897 | 1.7 % | 0.6 % |
| kodim15/444/p0/d1 | 5.402 | 4.024 | 1.342 | 0.9 % | 0.9 % |
| kodim15/444/p0/d4 | 4.866 | 3.108 | 1.566 | 0.3 % | 0.4 % |
| kodim15/444/p2/d1 | 8.713 | 9.726 | 0.896 | 2.3 % | 1.3 % |
| kodim15/444/p2/d4 | 7.128 | 7.466 | 0.955 | 1.4 % | 0.7 % |
| kodim15/420/p0/d1 | 3.806 | 2.999 | 1.269 | 0.5 % | 0.1 % |
| kodim15/420/p0/d4 | 3.406 | 2.207 | 1.543 | 0.6 % | 1.3 % |
| kodim15/420/p2/d1 | 6.009 | 6.964 | 0.863 | 0.1 % | 0.2 % |
| kodim15/420/p2/d4 | 4.637 | 4.844 | 0.957 | 0.4 % | 0.5 % |
| kodim16/444/p0/d1 | 5.156 | 4.085 | 1.262 | 0.3 % | 0.2 % |
| kodim16/444/p0/d4 | 4.632 | 3.101 | 1.494 | 0.2 % | 0.3 % |
| kodim16/444/p2/d1 | 8.685 | 10.244 | 0.848 | 1.9 % | 0.6 % |
| kodim16/444/p2/d4 | 7.038 | 7.494 | 0.939 | 0.1 % | 0.5 % |
| kodim16/420/p0/d1 | 3.818 | 3.129 | 1.220 | 0.6 % | 0.7 % |
| kodim16/420/p0/d4 | 3.313 | 2.213 | 1.497 | 0.3 % | 0.2 % |
| kodim16/420/p2/d1 | 6.198 | 7.374 | 0.840 | 0.3 % | 0.1 % |
| kodim16/420/p2/d4 | 4.639 | 4.987 | 0.930 | 0.4 % | 0.5 % |
| kodim17/444/p0/d1 | 5.257 | 3.974 | 1.323 | 0.1 % | 0.1 % |
| kodim17/444/p0/d4 | 4.732 | 3.086 | 1.533 | 0.2 % | 0.1 % |
| kodim17/444/p2/d1 | 8.611 | 9.668 | 0.891 | 0.2 % | 0.3 % |
| kodim17/444/p2/d4 | 7.139 | 7.473 | 0.955 | 0.7 % | 0.5 % |
| kodim17/420/p0/d1 | 3.832 | 3.018 | 1.270 | 0.1 % | 0.2 % |
| kodim17/420/p0/d4 | 3.416 | 2.202 | 1.551 | 0.4 % | 0.2 % |
| kodim17/420/p2/d1 | 6.168 | 6.811 | 0.906 | 0.2 % | 0.3 % |
| kodim17/420/p2/d4 | 4.751 | 4.748 | 1.001 | 1.1 % | 0.6 % |
| kodim18/444/p0/d1 | 5.838 | 4.803 | 1.216 | 0.3 % | 0.1 % |
| kodim18/444/p0/d4 | 4.969 | 3.436 | 1.446 | 0.2 % | 0.1 % |
| kodim18/444/p2/d1 | 10.290 | 11.835 | 0.869 | 1.2 % | 0.5 % |
| kodim18/444/p2/d4 | 7.590 | 8.436 | 0.900 | 0.1 % | 0.2 % |
| kodim18/420/p0/d1 | 4.294 | 3.771 | 1.138 | 0.6 % | 0.3 % |
| kodim18/420/p0/d4 | 3.617 | 2.512 | 1.440 | 0.0 % | 0.2 % |
| kodim18/420/p2/d1 | 7.591 | 8.943 | 0.849 | 0.5 % | 0.3 % |
| kodim18/420/p2/d4 | 5.304 | 5.590 | 0.949 | 0.1 % | 0.2 % |
| kodim19/444/p0/d1 | 5.430 | 4.283 | 1.268 | 0.7 % | 0.2 % |
| kodim19/444/p0/d4 | 4.770 | 3.164 | 1.507 | 0.3 % | 0.1 % |
| kodim19/444/p2/d1 | 9.074 | 10.464 | 0.867 | 0.5 % | 0.4 % |
| kodim19/444/p2/d4 | 7.248 | 7.624 | 0.951 | 0.6 % | 0.1 % |
| kodim19/420/p0/d1 | 4.003 | 3.292 | 1.216 | 0.2 % | 0.1 % |
| kodim19/420/p0/d4 | 3.444 | 2.275 | 1.514 | 0.2 % | 0.2 % |
| kodim19/420/p2/d1 | 6.569 | 7.591 | 0.865 | 0.4 % | 0.2 % |
| kodim19/420/p2/d4 | 4.883 | 4.931 | 0.990 | 0.4 % | 0.3 % |
| kodim20/444/p0/d1 | 4.933 | 3.712 | 1.329 | 0.6 % | 0.1 % |
| kodim20/444/p0/d4 | 4.515 | 2.956 | 1.528 | 0.5 % | 0.1 % |
| kodim20/444/p2/d1 | 8.075 | 9.106 | 0.887 | 1.3 % | 0.5 % |
| kodim20/444/p2/d4 | 6.751 | 7.166 | 0.942 | 0.5 % | 0.1 % |
| kodim20/420/p0/d1 | 3.557 | 2.789 | 1.275 | 0.3 % | 0.3 % |
| kodim20/420/p0/d4 | 3.146 | 2.087 | 1.507 | 0.2 % | 0.2 % |
| kodim20/420/p2/d1 | 5.597 | 6.571 | 0.852 | 0.7 % | 0.2 % |
| kodim20/420/p2/d4 | 4.430 | 4.725 | 0.938 | 0.3 % | 0.7 % |
| kodim21/444/p0/d1 | 5.357 | 4.278 | 1.252 | 0.2 % | 0.1 % |
| kodim21/444/p0/d4 | 4.681 | 3.235 | 1.447 | 0.6 % | 0.1 % |
| kodim21/444/p2/d1 | 9.163 | 10.494 | 0.873 | 0.5 % | 0.4 % |
| kodim21/444/p2/d4 | 7.140 | 7.854 | 0.909 | 0.8 % | 0.3 % |
| kodim21/420/p0/d1 | 3.949 | 3.311 | 1.193 | 0.2 % | 0.4 % |
| kodim21/420/p0/d4 | 3.355 | 2.342 | 1.432 | 0.2 % | 0.4 % |
| kodim21/420/p2/d1 | 6.584 | 7.806 | 0.844 | 0.4 % | 0.3 % |
| kodim21/420/p2/d4 | 4.859 | 5.314 | 0.914 | 0.4 % | 0.4 % |
| kodim22/444/p0/d1 | 5.704 | 4.562 | 1.250 | 0.6 % | 0.1 % |
| kodim22/444/p0/d4 | 4.950 | 3.314 | 1.494 | 0.6 % | 0.6 % |
| kodim22/444/p2/d1 | 9.688 | 11.261 | 0.860 | 0.3 % | 0.2 % |
| kodim22/444/p2/d4 | 7.393 | 7.933 | 0.932 | 2.2 % | 0.2 % |
| kodim22/420/p0/d1 | 4.114 | 3.522 | 1.168 | 0.6 % | 0.3 % |
| kodim22/420/p0/d4 | 3.528 | 2.367 | 1.491 | 0.5 % | 0.2 % |
| kodim22/420/p2/d1 | 6.945 | 8.519 | 0.815 | 0.2 % | 0.1 % |
| kodim22/420/p2/d4 | 4.888 | 5.348 | 0.914 | 0.3 % | 0.3 % |
| kodim23/444/p0/d1 | 5.268 | 3.584 | 1.470 | 0.3 % | 0.1 % |
| kodim23/444/p0/d4 | 4.678 | 2.939 | 1.591 | 0.2 % | 0.1 % |
| kodim23/444/p2/d1 | 8.059 | 8.552 | 0.942 | 0.2 % | 0.6 % |
| kodim23/444/p2/d4 | 6.731 | 7.020 | 0.959 | 0.5 % | 0.5 % |
| kodim23/420/p0/d1 | 3.713 | 2.638 | 1.408 | 0.3 % | 0.2 % |
| kodim23/420/p0/d4 | 3.344 | 2.090 | 1.600 | 0.6 % | 0.4 % |
| kodim23/420/p2/d1 | 5.481 | 6.115 | 0.896 | 0.2 % | 1.3 % |
| kodim23/420/p2/d4 | 4.476 | 4.598 | 0.973 | 1.2 % | 0.1 % |
| kodim24/444/p0/d1 | 5.596 | 4.760 | 1.176 | 0.2 % | 0.1 % |
| kodim24/444/p0/d4 | 4.897 | 3.485 | 1.405 | 0.6 % | 0.2 % |
| kodim24/444/p2/d1 | 10.124 | 11.793 | 0.858 | 0.4 % | 0.2 % |
| kodim24/444/p2/d4 | 7.750 | 8.527 | 0.909 | 0.4 % | 0.3 % |
| kodim24/420/p0/d1 | 4.188 | 3.749 | 1.117 | 0.1 % | 0.2 % |
| kodim24/420/p0/d4 | 3.590 | 2.558 | 1.404 | 0.2 % | 0.1 % |
| kodim24/420/p2/d1 | 7.530 | 9.129 | 0.825 | 0.9 % | 0.1 % |
| kodim24/420/p2/d4 | 5.351 | 5.957 | 0.898 | 0.6 % | 0.3 % |
| smooth-gradient/444/p0/d1 | 0.663 | 0.368 | 1.802 | 0.8 % | 0.1 % |
| smooth-gradient/444/p0/d4 | 0.630 | 0.369 | 1.705 | 0.2 % | 0.2 % |
| smooth-gradient/444/p2/d1 | 0.929 | 0.696 | 1.334 | 0.5 % | 1.4 % |
| smooth-gradient/444/p2/d4 | 0.894 | 0.682 | 1.312 | 0.4 % | 0.3 % |
| smooth-gradient/420/p0/d1 | 0.470 | 0.296 | 1.584 | 0.4 % | 0.1 % |
| smooth-gradient/420/p0/d4 | 0.464 | 0.300 | 1.547 | 0.6 % | 0.5 % |
| smooth-gradient/420/p2/d1 | 0.603 | 0.446 | 1.353 | 0.9 % | 0.3 % |
| smooth-gradient/420/p2/d4 | 0.591 | 0.442 | 1.337 | 0.8 % | 0.2 % |
| low-contrast/444/p0/d1 | 0.808 | 0.503 | 1.606 | 0.8 % | 0.3 % |
| low-contrast/444/p0/d4 | 0.700 | 0.391 | 1.792 | 0.3 % | 0.3 % |
| low-contrast/444/p2/d1 | 1.342 | 1.165 | 1.152 | 0.6 % | 0.1 % |
| low-contrast/444/p2/d4 | 1.047 | 0.798 | 1.311 | 0.6 % | 0.1 % |
| low-contrast/420/p0/d1 | 0.569 | 0.437 | 1.301 | 0.7 % | 0.4 % |
| low-contrast/420/p0/d4 | 0.491 | 0.318 | 1.545 | 0.7 % | 0.6 % |
| low-contrast/420/p2/d1 | 0.928 | 0.837 | 1.108 | 0.7 % | 0.2 % |
| low-contrast/420/p2/d4 | 0.686 | 0.527 | 1.301 | 1.0 % | 0.3 % |
| color-edges/444/p0/d1 | 0.753 | 0.563 | 1.338 | 0.1 % | 0.1 % |
| color-edges/444/p0/d4 | 0.707 | 0.490 | 1.444 | 0.2 % | 0.3 % |
| color-edges/444/p2/d1 | 1.564 | 1.359 | 1.151 | 0.5 % | 0.5 % |
| color-edges/444/p2/d4 | 1.338 | 1.147 | 1.167 | 0.2 % | 0.3 % |
| color-edges/420/p0/d1 | 0.597 | 0.563 | 1.059 | 1.2 % | 0.2 % |
| color-edges/420/p0/d4 | 0.541 | 0.454 | 1.191 | 0.7 % | 0.6 % |
| color-edges/420/p2/d1 | 1.203 | 1.138 | 1.057 | 0.5 % | 0.2 % |
| color-edges/420/p2/d4 | 0.984 | 0.932 | 1.056 | 0.5 % | 0.4 % |
| line-art/444/p0/d1 | 0.673 | 0.538 | 1.251 | 0.5 % | 0.1 % |
| line-art/444/p0/d4 | 0.631 | 0.483 | 1.308 | 0.3 % | 0.3 % |
| line-art/444/p2/d1 | 1.372 | 1.141 | 1.202 | 0.9 % | 0.4 % |
| line-art/444/p2/d4 | 1.223 | 1.130 | 1.082 | 0.2 % | 0.3 % |
| line-art/420/p0/d1 | 0.551 | 0.506 | 1.090 | 0.3 % | 0.2 % |
| line-art/420/p0/d4 | 0.507 | 0.425 | 1.194 | 0.9 % | 1.4 % |
| line-art/420/p2/d1 | 1.060 | 0.887 | 1.195 | 0.5 % | 0.1 % |
| line-art/420/p2/d4 | 0.915 | 0.837 | 1.093 | 0.3 % | 0.3 % |
| structured-texture/444/p0/d1 | 1.087 | 1.234 | 0.881 | 0.2 % | 0.1 % |
| structured-texture/444/p0/d4 | 0.872 | 0.704 | 1.238 | 0.6 % | 0.1 % |
| structured-texture/444/p2/d1 | 2.599 | 3.085 | 0.843 | 0.5 % | 0.1 % |
| structured-texture/444/p2/d4 | 1.604 | 1.651 | 0.971 | 0.1 % | 0.2 % |
| structured-texture/420/p0/d1 | 0.726 | 0.913 | 0.796 | 0.6 % | 0.2 % |
| structured-texture/420/p0/d4 | 0.608 | 0.545 | 1.115 | 0.5 % | 0.8 % |
| structured-texture/420/p2/d1 | 1.653 | 2.021 | 0.818 | 0.6 % | 0.8 % |
| structured-texture/420/p2/d4 | 1.029 | 1.075 | 0.957 | 0.5 % | 0.2 % |
| low-noise/444/p0/d1 | 0.852 | 0.722 | 1.181 | 0.2 % | 0.3 % |
| low-noise/444/p0/d4 | 0.657 | 0.375 | 1.754 | 0.4 % | 0.1 % |
| low-noise/444/p2/d1 | 1.386 | 1.629 | 0.851 | 0.4 % | 0.1 % |
| low-noise/444/p2/d4 | 0.939 | 0.730 | 1.286 | 1.0 % | 0.3 % |
| low-noise/420/p0/d1 | 0.577 | 0.561 | 1.030 | 0.4 % | 0.2 % |
| low-noise/420/p0/d4 | 0.460 | 0.295 | 1.562 | 0.4 % | 0.2 % |
| low-noise/420/p2/d1 | 0.913 | 1.075 | 0.849 | 0.7 % | 0.2 % |
| low-noise/420/p2/d4 | 0.614 | 0.460 | 1.335 | 0.8 % | 0.2 % |
| full-noise/444/p0/d1 | 1.199 | 1.367 | 0.877 | 0.4 % | 0.1 % |
| full-noise/444/p0/d4 | 0.937 | 1.024 | 0.915 | 0.1 % | 0.1 % |
| full-noise/444/p2/d1 | 3.145 | 3.424 | 0.918 | 0.6 % | 0.2 % |
| full-noise/444/p2/d4 | 1.908 | 2.413 | 0.791 | 1.0 % | 0.2 % |
| full-noise/420/p0/d1 | 0.782 | 0.997 | 0.784 | 0.6 % | 0.3 % |
| full-noise/420/p0/d4 | 0.634 | 0.739 | 0.857 | 0.5 % | 0.2 % |
| full-noise/420/p2/d1 | 2.006 | 2.039 | 0.983 | 0.4 % | 0.2 % |
| full-noise/420/p2/d4 | 1.236 | 1.529 | 0.808 | 0.7 % | 0.2 % |
| tiny-odd-edges/444/p0/d1 | 0.039 | 0.026 | 1.471 | 0.9 % | 0.9 % |
| tiny-odd-edges/444/p0/d4 | 0.039 | 0.031 | 1.230 | 0.9 % | 0.3 % |
| tiny-odd-edges/444/p2/d1 | 0.075 | 0.053 | 1.413 | 1.7 % | 0.2 % |
| tiny-odd-edges/444/p2/d4 | 0.071 | 0.054 | 1.300 | 1.1 % | 0.3 % |
| tiny-odd-edges/420/p0/d1 | 0.037 | 0.024 | 1.530 | 0.8 % | 0.7 % |
| tiny-odd-edges/420/p0/d4 | 0.038 | 0.029 | 1.288 | 3.7 % | 0.4 % |
| tiny-odd-edges/420/p2/d1 | 0.070 | 0.049 | 1.432 | 1.0 % | 0.9 % |
| tiny-odd-edges/420/p2/d4 | 0.067 | 0.050 | 1.350 | 2.1 % | 0.3 % |

Exact input/JPEG files, SHA256 identities, per-block raw samples, p10/p50/p90, relative MAD, allocations/collections, calibration counts and native command outputs are retained alongside this report. summary.json records CPU, affinity, OS, runtime, vector widths, policy, revision and group verdicts.

Failed bounds:

- kodim01/444/p0/d1: median latency ratio 1.122740 exceeds 1.10.
- kodim01/444/p0/d4: median latency ratio 1.407134 exceeds 1.10.
- kodim01/420/p0/d4: median latency ratio 1.379029 exceeds 1.10.
- kodim02/444/p0/d1: median latency ratio 1.301976 exceeds 1.10.
- kodim02/444/p0/d4: median latency ratio 1.564612 exceeds 1.10.
- kodim02/420/p0/d1: median latency ratio 1.228133 exceeds 1.10.
- kodim02/420/p0/d4: median latency ratio 1.560529 exceeds 1.10.
- kodim03/444/p0/d1: median latency ratio 1.390935 exceeds 1.10.
- kodim03/444/p0/d4: median latency ratio 1.550280 exceeds 1.10.
- kodim03/420/p0/d1: median latency ratio 1.331559 exceeds 1.10.
- kodim03/420/p0/d4: median latency ratio 1.545111 exceeds 1.10.
- kodim04/444/p0/d1: median latency ratio 1.319697 exceeds 1.10.
- kodim04/444/p0/d4: median latency ratio 1.589410 exceeds 1.10.
- kodim04/420/p0/d1: median latency ratio 1.235570 exceeds 1.10.
- kodim04/420/p0/d4: median latency ratio 1.572437 exceeds 1.10.
- kodim05/444/p0/d1: median latency ratio 1.178275 exceeds 1.10.
- kodim05/444/p0/d4: median latency ratio 1.372342 exceeds 1.10.
- kodim05/420/p0/d4: median latency ratio 1.344770 exceeds 1.10.
- kodim06/444/p0/d1: median latency ratio 1.223013 exceeds 1.10.
- kodim06/444/p0/d4: median latency ratio 1.428666 exceeds 1.10.
- kodim06/420/p0/d1: median latency ratio 1.130509 exceeds 1.10.
- kodim06/420/p0/d4: median latency ratio 1.400565 exceeds 1.10.
- kodim07/444/p0/d1: median latency ratio 1.368771 exceeds 1.10.
- kodim07/444/p0/d4: median latency ratio 1.520891 exceeds 1.10.
- kodim07/420/p0/d1: median latency ratio 1.308932 exceeds 1.10.
- kodim07/420/p0/d4: median latency ratio 1.519346 exceeds 1.10.
- kodim08/444/p0/d1: median latency ratio 1.149446 exceeds 1.10.
- kodim08/444/p0/d4: median latency ratio 1.389354 exceeds 1.10.
- kodim08/420/p0/d4: median latency ratio 1.349431 exceeds 1.10.
- kodim09/444/p0/d1: median latency ratio 1.389232 exceeds 1.10.
- kodim09/444/p0/d4: median latency ratio 1.569727 exceeds 1.10.
- kodim09/420/p0/d1: median latency ratio 1.365810 exceeds 1.10.
- kodim09/420/p0/d4: median latency ratio 1.548908 exceeds 1.10.
- kodim10/444/p0/d1: median latency ratio 1.350808 exceeds 1.10.
- kodim10/444/p0/d4: median latency ratio 1.552986 exceeds 1.10.
- kodim10/420/p0/d1: median latency ratio 1.324123 exceeds 1.10.
- kodim10/420/p0/d4: median latency ratio 1.559526 exceeds 1.10.
- kodim11/444/p0/d1: median latency ratio 1.215464 exceeds 1.10.
- kodim11/444/p0/d4: median latency ratio 1.480940 exceeds 1.10.
- kodim11/420/p0/d1: median latency ratio 1.147240 exceeds 1.10.
- kodim11/420/p0/d4: median latency ratio 1.459231 exceeds 1.10.
- kodim12/444/p0/d1: median latency ratio 1.328422 exceeds 1.10.
- kodim12/444/p0/d4: median latency ratio 1.555789 exceeds 1.10.
- kodim12/420/p0/d1: median latency ratio 1.292242 exceeds 1.10.
- kodim12/420/p0/d4: median latency ratio 1.529049 exceeds 1.10.
- kodim13/444/p0/d4: median latency ratio 1.303556 exceeds 1.10.
- kodim13/420/p0/d4: median latency ratio 1.279448 exceeds 1.10.
- kodim14/444/p0/d1: median latency ratio 1.162695 exceeds 1.10.
- kodim14/444/p0/d4: median latency ratio 1.417272 exceeds 1.10.
- kodim14/420/p0/d4: median latency ratio 1.403003 exceeds 1.10.
- kodim15/444/p0/d1: median latency ratio 1.342331 exceeds 1.10.
- kodim15/444/p0/d4: median latency ratio 1.565635 exceeds 1.10.
- kodim15/420/p0/d1: median latency ratio 1.269058 exceeds 1.10.
- kodim15/420/p0/d4: median latency ratio 1.543347 exceeds 1.10.
- kodim16/444/p0/d1: median latency ratio 1.262088 exceeds 1.10.
- kodim16/444/p0/d4: median latency ratio 1.493765 exceeds 1.10.
- kodim16/420/p0/d1: median latency ratio 1.220220 exceeds 1.10.
- kodim16/420/p0/d4: median latency ratio 1.496917 exceeds 1.10.
- kodim17/444/p0/d1: median latency ratio 1.323043 exceeds 1.10.
- kodim17/444/p0/d4: median latency ratio 1.533195 exceeds 1.10.
- kodim17/420/p0/d1: median latency ratio 1.269726 exceeds 1.10.
- kodim17/420/p0/d4: median latency ratio 1.551241 exceeds 1.10.
- kodim18/444/p0/d1: median latency ratio 1.215512 exceeds 1.10.
- kodim18/444/p0/d4: median latency ratio 1.446196 exceeds 1.10.
- kodim18/420/p0/d1: median latency ratio 1.138475 exceeds 1.10.
- kodim18/420/p0/d4: median latency ratio 1.439860 exceeds 1.10.
- kodim19/444/p0/d1: median latency ratio 1.267766 exceeds 1.10.
- kodim19/444/p0/d4: median latency ratio 1.507418 exceeds 1.10.
- kodim19/420/p0/d1: median latency ratio 1.215884 exceeds 1.10.
- kodim19/420/p0/d4: median latency ratio 1.513684 exceeds 1.10.
- kodim20/444/p0/d1: median latency ratio 1.329062 exceeds 1.10.
- kodim20/444/p0/d4: median latency ratio 1.527537 exceeds 1.10.
- kodim20/420/p0/d1: median latency ratio 1.275374 exceeds 1.10.
- kodim20/420/p0/d4: median latency ratio 1.506866 exceeds 1.10.
- kodim21/444/p0/d1: median latency ratio 1.252408 exceeds 1.10.
- kodim21/444/p0/d4: median latency ratio 1.447206 exceeds 1.10.
- kodim21/420/p0/d1: median latency ratio 1.192731 exceeds 1.10.
- kodim21/420/p0/d4: median latency ratio 1.432399 exceeds 1.10.
- kodim22/444/p0/d1: median latency ratio 1.250356 exceeds 1.10.
- kodim22/444/p0/d4: median latency ratio 1.493687 exceeds 1.10.
- kodim22/420/p0/d1: median latency ratio 1.167856 exceeds 1.10.
- kodim22/420/p0/d4: median latency ratio 1.490854 exceeds 1.10.
- kodim23/444/p0/d1: median latency ratio 1.469859 exceeds 1.10.
- kodim23/444/p0/d4: median latency ratio 1.591347 exceeds 1.10.
- kodim23/420/p0/d1: median latency ratio 1.407531 exceeds 1.10.
- kodim23/420/p0/d4: median latency ratio 1.600221 exceeds 1.10.
- kodim24/444/p0/d1: median latency ratio 1.175664 exceeds 1.10.
- kodim24/444/p0/d4: median latency ratio 1.405373 exceeds 1.10.
- kodim24/420/p0/d1: median latency ratio 1.117077 exceeds 1.10.
- kodim24/420/p0/d4: median latency ratio 1.403733 exceeds 1.10.
- smooth-gradient/444/p0/d1: median latency ratio 1.802404 exceeds 1.10.
- smooth-gradient/444/p0/d4: median latency ratio 1.705207 exceeds 1.10.
- smooth-gradient/444/p2/d1: median latency ratio 1.334497 exceeds 1.10.
- smooth-gradient/444/p2/d4: median latency ratio 1.311963 exceeds 1.10.
- smooth-gradient/420/p0/d1: median latency ratio 1.584124 exceeds 1.10.
- smooth-gradient/420/p0/d4: median latency ratio 1.547458 exceeds 1.10.
- smooth-gradient/420/p2/d1: median latency ratio 1.352649 exceeds 1.10.
- smooth-gradient/420/p2/d4: median latency ratio 1.336528 exceeds 1.10.
- low-contrast/444/p0/d1: median latency ratio 1.606075 exceeds 1.10.
- low-contrast/444/p0/d4: median latency ratio 1.791585 exceeds 1.10.
- low-contrast/444/p2/d1: median latency ratio 1.151980 exceeds 1.10.
- low-contrast/444/p2/d4: median latency ratio 1.310766 exceeds 1.10.
- low-contrast/420/p0/d1: median latency ratio 1.301024 exceeds 1.10.
- low-contrast/420/p0/d4: median latency ratio 1.544768 exceeds 1.10.
- low-contrast/420/p2/d1: median latency ratio 1.108040 exceeds 1.10.
- low-contrast/420/p2/d4: median latency ratio 1.300573 exceeds 1.10.
- color-edges/444/p0/d1: median latency ratio 1.337653 exceeds 1.10.
- color-edges/444/p0/d4: median latency ratio 1.444183 exceeds 1.10.
- color-edges/444/p2/d1: median latency ratio 1.151059 exceeds 1.10.
- color-edges/444/p2/d4: median latency ratio 1.166627 exceeds 1.10.
- color-edges/420/p0/d4: median latency ratio 1.191215 exceeds 1.10.
- line-art/444/p0/d1: median latency ratio 1.251141 exceeds 1.10.
- line-art/444/p0/d4: median latency ratio 1.308198 exceeds 1.10.
- line-art/444/p2/d1: median latency ratio 1.202273 exceeds 1.10.
- line-art/420/p0/d4: median latency ratio 1.193659 exceeds 1.10.
- line-art/420/p2/d1: median latency ratio 1.195358 exceeds 1.10.
- structured-texture/444/p0/d4: median latency ratio 1.237545 exceeds 1.10.
- structured-texture/420/p0/d4: median latency ratio 1.114851 exceeds 1.10.
- low-noise/444/p0/d1: median latency ratio 1.180668 exceeds 1.10.
- low-noise/444/p0/d4: median latency ratio 1.753753 exceeds 1.10.
- low-noise/444/p2/d4: median latency ratio 1.286130 exceeds 1.10.
- low-noise/420/p0/d4: median latency ratio 1.561862 exceeds 1.10.
- low-noise/420/p2/d4: median latency ratio 1.334860 exceeds 1.10.
- tiny-odd-edges/444/p0/d1: median latency ratio 1.471168 exceeds 1.10.
- tiny-odd-edges/444/p0/d4: median latency ratio 1.230298 exceeds 1.10.
- tiny-odd-edges/444/p2/d1: median latency ratio 1.412825 exceeds 1.10.
- tiny-odd-edges/444/p2/d4: median latency ratio 1.299849 exceeds 1.10.
- tiny-odd-edges/420/p0/d1: median latency ratio 1.530391 exceeds 1.10.
- tiny-odd-edges/420/p0/d4: median latency ratio 1.288288 exceeds 1.10.
- tiny-odd-edges/420/p2/d1: median latency ratio 1.432138 exceeds 1.10.
- tiny-odd-edges/420/p2/d4: median latency ratio 1.350445 exceeds 1.10.
- natural/444/p0: geometric mean latency ratio 1.371131 exceeds 1.05.
- natural/420/p0: geometric mean latency ratio 1.318178 exceeds 1.05.
- controlled/444/p0: geometric mean latency ratio 1.326253 exceeds 1.05.
- controlled/444/p2: geometric mean latency ratio 1.112651 exceeds 1.05.
- controlled/420/p0: geometric mean latency ratio 1.185855 exceeds 1.05.
- controlled/420/p2: geometric mean latency ratio 1.108397 exceeds 1.05.
