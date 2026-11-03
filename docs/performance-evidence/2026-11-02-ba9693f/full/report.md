# Performance parity: FAILED

256/256 cases: RGB8 YCbCr, Kodak24 plus eight fixed controls, distances1/4, 444/420 and progression0/2, one worker on this CI host. Pinned upstream `031a0077f5799a6041004267fc12b956c1f52a20`.
Tested revision: `ba9693f455b82c2ffd0326b3125a6892373ee64a`.

All cases are measured even when a latency bound fails, so the report identifies every measured regression. A passing verdict requires complete, valid coverage and all fixed bounds.

Fixed latency noninferiority limits: geometric mean managed/native median latency ratio <=1.05 in each corpus/sampling/progression group; every case <=1.10. A faster case cannot excuse a slower case. No universal performance claim is made.

Seven alternating order blocks, five untimed warmups before each block. The native-only three-iteration calibration targets50ms and selects a shared count clamped3..100. Medians use all seven blocks; no outliers are discarded. Complete in-memory encode calls are timed individually and their elapsed times summed; subprocess startup, file I/O, verification and native buffer free are excluded. Managed collections inside encode are included. Tiered compilation is disabled.

Evidence is invalid if a block interval is below1ms, any case has relative MAD >10%, inputs/settings/coverage differ, or an untimed JPEG structure comparison fails. Invalid evidence cannot establish parity.

| Corpus | Sampling | Progression | Cases | Geometric mean ratio | Worst ratio | Verdict |
| --- | --- | --- | ---: | ---: | ---: | --- |
| natural | 444 | 0 | 48 | 1.043987 | 1.215737 | FAIL |
| natural | 444 | 2 | 48 | 0.929395 | 1.124929 | FAIL |
| natural | 420 | 0 | 48 | 1.034205 | 1.256969 | FAIL |
| natural | 420 | 2 | 48 | 0.834666 | 0.995268 | PASS |
| controlled | 444 | 0 | 16 | 1.278679 | 2.260598 | FAIL |
| controlled | 444 | 2 | 16 | 1.289391 | 2.527178 | FAIL |
| controlled | 420 | 0 | 16 | 1.215573 | 2.229427 | FAIL |
| controlled | 420 | 2 | 16 | 1.264848 | 2.614697 | FAIL |

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 3.717 | 4.163 | 0.893 | 1.4 % | 0.3 % |
| kodim01/444/p0/d4 | 3.053 | 2.852 | 1.071 | 0.7 % | 0.2 % |
| kodim01/444/p2/d1 | 9.313 | 11.346 | 0.821 | 0.2 % | 1.2 % |
| kodim01/444/p2/d4 | 7.386 | 7.653 | 0.965 | 0.4 % | 0.8 % |
| kodim01/420/p0/d1 | 2.833 | 3.306 | 0.857 | 0.5 % | 0.7 % |
| kodim01/420/p0/d4 | 2.194 | 2.043 | 1.074 | 0.3 % | 0.4 % |
| kodim01/420/p2/d1 | 6.462 | 8.880 | 0.728 | 1.0 % | 2.7 % |
| kodim01/420/p2/d4 | 4.387 | 5.211 | 0.842 | 1.3 % | 1.2 % |
| kodim02/444/p0/d1 | 3.371 | 3.508 | 0.961 | 1.0 % | 0.4 % |
| kodim02/444/p0/d4 | 2.824 | 2.423 | 1.165 | 0.1 % | 0.2 % |
| kodim02/444/p2/d1 | 7.588 | 9.026 | 0.841 | 0.7 % | 0.4 % |
| kodim02/444/p2/d4 | 6.582 | 6.218 | 1.059 | 0.6 % | 0.4 % |
| kodim02/420/p0/d1 | 2.474 | 2.633 | 0.939 | 0.9 % | 0.3 % |
| kodim02/420/p0/d4 | 2.039 | 1.697 | 1.202 | 0.4 % | 1.1 % |
| kodim02/420/p2/d1 | 4.848 | 6.744 | 0.719 | 0.4 % | 0.6 % |
| kodim02/420/p2/d4 | 3.619 | 3.991 | 0.907 | 0.6 % | 0.4 % |
| kodim03/444/p0/d1 | 3.103 | 2.952 | 1.051 | 0.4 % | 0.3 % |
| kodim03/444/p0/d4 | 2.828 | 2.348 | 1.204 | 0.4 % | 0.5 % |
| kodim03/444/p2/d1 | 6.712 | 7.716 | 0.870 | 0.6 % | 0.3 % |
| kodim03/444/p2/d4 | 6.235 | 6.053 | 1.030 | 0.4 % | 0.2 % |
| kodim03/420/p0/d1 | 2.269 | 2.173 | 1.044 | 0.5 % | 0.1 % |
| kodim03/420/p0/d4 | 2.062 | 1.657 | 1.244 | 1.0 % | 1.3 % |
| kodim03/420/p2/d1 | 4.375 | 5.512 | 0.794 | 0.6 % | 0.4 % |
| kodim03/420/p2/d4 | 3.518 | 3.823 | 0.920 | 0.2 % | 0.3 % |
| kodim04/444/p0/d1 | 3.354 | 3.413 | 0.983 | 0.5 % | 0.4 % |
| kodim04/444/p0/d4 | 2.871 | 2.458 | 1.168 | 0.5 % | 0.1 % |
| kodim04/444/p2/d1 | 7.590 | 8.791 | 0.863 | 0.1 % | 0.1 % |
| kodim04/444/p2/d4 | 6.637 | 6.264 | 1.060 | 0.4 % | 1.0 % |
| kodim04/420/p0/d1 | 2.449 | 2.564 | 0.955 | 0.3 % | 0.2 % |
| kodim04/420/p0/d4 | 2.080 | 1.723 | 1.207 | 0.4 % | 0.3 % |
| kodim04/420/p2/d1 | 4.951 | 6.379 | 0.776 | 0.8 % | 1.2 % |
| kodim04/420/p2/d4 | 3.820 | 3.982 | 0.959 | 0.6 % | 2.0 % |
| kodim05/444/p0/d1 | 3.647 | 4.119 | 0.885 | 0.7 % | 0.3 % |
| kodim05/444/p0/d4 | 3.069 | 2.950 | 1.040 | 0.5 % | 0.1 % |
| kodim05/444/p2/d1 | 8.618 | 10.926 | 0.789 | 0.6 % | 0.5 % |
| kodim05/444/p2/d4 | 7.057 | 7.698 | 0.917 | 0.5 % | 0.4 % |
| kodim05/420/p0/d1 | 2.797 | 3.736 | 0.749 | 0.8 % | 0.2 % |
| kodim05/420/p0/d4 | 2.262 | 2.187 | 1.035 | 0.8 % | 0.8 % |
| kodim05/420/p2/d1 | 6.534 | 8.731 | 0.748 | 0.7 % | 0.8 % |
| kodim05/420/p2/d4 | 4.543 | 5.533 | 0.821 | 0.7 % | 0.7 % |
| kodim06/444/p0/d1 | 3.454 | 3.659 | 0.944 | 0.7 % | 0.6 % |
| kodim06/444/p0/d4 | 2.912 | 2.625 | 1.109 | 0.3 % | 0.1 % |
| kodim06/444/p2/d1 | 8.481 | 9.619 | 0.882 | 0.4 % | 0.2 % |
| kodim06/444/p2/d4 | 6.930 | 6.887 | 1.006 | 0.2 % | 0.4 % |
| kodim06/420/p0/d1 | 2.580 | 2.817 | 0.916 | 0.3 % | 0.3 % |
| kodim06/420/p0/d4 | 2.099 | 1.887 | 1.113 | 0.3 % | 0.2 % |
| kodim06/420/p2/d1 | 5.556 | 7.302 | 0.761 | 0.1 % | 0.4 % |
| kodim06/420/p2/d4 | 4.117 | 4.565 | 0.902 | 1.3 % | 0.9 % |
| kodim07/444/p0/d1 | 3.276 | 3.090 | 1.060 | 2.1 % | 0.3 % |
| kodim07/444/p0/d4 | 2.844 | 2.435 | 1.168 | 0.1 % | 0.1 % |
| kodim07/444/p2/d1 | 6.811 | 7.942 | 0.858 | 0.4 % | 0.2 % |
| kodim07/444/p2/d4 | 6.677 | 6.295 | 1.061 | 0.2 % | 0.4 % |
| kodim07/420/p0/d1 | 2.335 | 2.279 | 1.024 | 0.4 % | 0.1 % |
| kodim07/420/p0/d4 | 2.059 | 1.725 | 1.193 | 0.4 % | 0.2 % |
| kodim07/420/p2/d1 | 4.726 | 5.778 | 0.818 | 0.5 % | 0.4 % |
| kodim07/420/p2/d4 | 3.682 | 4.174 | 0.882 | 0.1 % | 0.4 % |
| kodim08/444/p0/d1 | 3.664 | 4.181 | 0.876 | 0.1 % | 0.0 % |
| kodim08/444/p0/d4 | 3.046 | 2.935 | 1.038 | 0.3 % | 0.4 % |
| kodim08/444/p2/d1 | 8.586 | 11.037 | 0.778 | 0.4 % | 0.7 % |
| kodim08/444/p2/d4 | 7.482 | 7.655 | 0.977 | 0.4 % | 0.5 % |
| kodim08/420/p0/d1 | 2.773 | 3.767 | 0.736 | 0.3 % | 0.2 % |
| kodim08/420/p0/d4 | 2.232 | 2.139 | 1.044 | 0.2 % | 0.1 % |
| kodim08/420/p2/d1 | 6.380 | 8.658 | 0.737 | 0.3 % | 1.0 % |
| kodim08/420/p2/d4 | 4.519 | 5.389 | 0.839 | 0.1 % | 0.6 % |
| kodim09/444/p0/d1 | 3.146 | 2.976 | 1.057 | 0.2 % | 0.2 % |
| kodim09/444/p0/d4 | 2.849 | 2.348 | 1.213 | 0.2 % | 0.3 % |
| kodim09/444/p2/d1 | 6.787 | 7.720 | 0.879 | 0.2 % | 0.4 % |
| kodim09/444/p2/d4 | 6.478 | 6.031 | 1.074 | 0.3 % | 0.3 % |
| kodim09/420/p0/d1 | 2.296 | 2.181 | 1.053 | 0.5 % | 0.2 % |
| kodim09/420/p0/d4 | 2.029 | 1.622 | 1.251 | 0.4 % | 0.3 % |
| kodim09/420/p2/d1 | 4.506 | 5.262 | 0.856 | 0.5 % | 0.8 % |
| kodim09/420/p2/d4 | 3.576 | 3.687 | 0.970 | 0.5 % | 0.1 % |
| kodim10/444/p0/d1 | 3.211 | 3.133 | 1.025 | 0.3 % | 0.3 % |
| kodim10/444/p0/d4 | 2.884 | 2.411 | 1.196 | 0.4 % | 0.1 % |
| kodim10/444/p2/d1 | 7.094 | 8.084 | 0.878 | 0.3 % | 0.2 % |
| kodim10/444/p2/d4 | 6.599 | 6.194 | 1.065 | 0.2 % | 0.5 % |
| kodim10/420/p0/d1 | 2.361 | 2.322 | 1.017 | 0.4 % | 0.4 % |
| kodim10/420/p0/d4 | 2.059 | 1.687 | 1.220 | 0.3 % | 0.3 % |
| kodim10/420/p2/d1 | 4.642 | 5.596 | 0.830 | 0.1 % | 0.3 % |
| kodim10/420/p2/d4 | 3.764 | 3.798 | 0.991 | 0.1 % | 0.1 % |
| kodim11/444/p0/d1 | 3.372 | 3.630 | 0.929 | 0.4 % | 0.1 % |
| kodim11/444/p0/d4 | 2.885 | 2.554 | 1.130 | 0.2 % | 0.2 % |
| kodim11/444/p2/d1 | 7.592 | 9.464 | 0.802 | 0.2 % | 0.4 % |
| kodim11/444/p2/d4 | 6.804 | 6.704 | 1.015 | 0.1 % | 0.4 % |
| kodim11/420/p0/d1 | 2.514 | 2.777 | 0.905 | 0.5 % | 0.4 % |
| kodim11/420/p0/d4 | 2.109 | 1.821 | 1.158 | 1.2 % | 0.1 % |
| kodim11/420/p2/d1 | 5.349 | 7.150 | 0.748 | 0.2 % | 0.4 % |
| kodim11/420/p2/d4 | 3.957 | 4.448 | 0.890 | 0.6 % | 0.4 % |
| kodim12/444/p0/d1 | 3.154 | 3.138 | 1.005 | 0.7 % | 0.2 % |
| kodim12/444/p0/d4 | 2.760 | 2.339 | 1.180 | 0.6 % | 0.2 % |
| kodim12/444/p2/d1 | 6.948 | 8.121 | 0.856 | 0.1 % | 0.4 % |
| kodim12/444/p2/d4 | 6.448 | 5.961 | 1.082 | 0.3 % | 0.3 % |
| kodim12/420/p0/d1 | 2.305 | 2.345 | 0.983 | 0.4 % | 0.7 % |
| kodim12/420/p0/d4 | 1.987 | 1.629 | 1.220 | 0.4 % | 0.2 % |
| kodim12/420/p2/d1 | 4.526 | 5.747 | 0.788 | 0.4 % | 0.2 % |
| kodim12/420/p2/d4 | 3.541 | 3.863 | 0.917 | 0.2 % | 0.6 % |
| kodim13/444/p0/d1 | 3.835 | 4.592 | 0.835 | 0.4 % | 0.2 % |
| kodim13/444/p0/d4 | 3.099 | 3.078 | 1.007 | 0.2 % | 0.1 % |
| kodim13/444/p2/d1 | 10.141 | 12.088 | 0.839 | 0.2 % | 0.5 % |
| kodim13/444/p2/d4 | 7.599 | 8.181 | 0.929 | 0.2 % | 0.2 % |
| kodim13/420/p0/d1 | 2.949 | 4.240 | 0.695 | 0.7 % | 0.5 % |
| kodim13/420/p0/d4 | 2.283 | 2.255 | 1.013 | 1.5 % | 0.4 % |
| kodim13/420/p2/d1 | 7.048 | 9.797 | 0.719 | 0.4 % | 0.2 % |
| kodim13/420/p2/d4 | 4.700 | 5.815 | 0.808 | 0.1 % | 0.5 % |
| kodim14/444/p0/d1 | 3.543 | 4.011 | 0.883 | 0.2 % | 0.2 % |
| kodim14/444/p0/d4 | 2.983 | 2.781 | 1.073 | 0.3 % | 0.2 % |
| kodim14/444/p2/d1 | 8.361 | 10.654 | 0.785 | 0.1 % | 0.4 % |
| kodim14/444/p2/d4 | 6.736 | 7.196 | 0.936 | 0.4 % | 0.6 % |
| kodim14/420/p0/d1 | 2.714 | 3.137 | 0.865 | 0.5 % | 0.1 % |
| kodim14/420/p0/d4 | 2.167 | 1.987 | 1.090 | 0.4 % | 0.2 % |
| kodim14/420/p2/d1 | 5.984 | 8.152 | 0.734 | 0.2 % | 0.2 % |
| kodim14/420/p2/d4 | 4.132 | 4.934 | 0.837 | 0.1 % | 0.3 % |
| kodim15/444/p0/d1 | 3.168 | 3.199 | 0.990 | 0.2 % | 0.1 % |
| kodim15/444/p0/d4 | 2.807 | 2.400 | 1.170 | 0.2 % | 0.2 % |
| kodim15/444/p2/d1 | 6.659 | 8.217 | 0.810 | 0.1 % | 0.3 % |
| kodim15/444/p2/d4 | 6.497 | 6.084 | 1.068 | 0.2 % | 0.4 % |
| kodim15/420/p0/d1 | 2.329 | 2.378 | 0.979 | 0.3 % | 0.4 % |
| kodim15/420/p0/d4 | 2.017 | 1.671 | 1.207 | 0.4 % | 0.4 % |
| kodim15/420/p2/d1 | 4.643 | 5.971 | 0.777 | 0.2 % | 0.7 % |
| kodim15/420/p2/d4 | 3.527 | 3.961 | 0.891 | 0.4 % | 0.2 % |
| kodim16/444/p0/d1 | 3.206 | 3.288 | 0.975 | 0.4 % | 0.2 % |
| kodim16/444/p0/d4 | 2.818 | 2.425 | 1.162 | 0.2 % | 0.1 % |
| kodim16/444/p2/d1 | 7.918 | 8.644 | 0.916 | 0.2 % | 0.4 % |
| kodim16/444/p2/d4 | 6.698 | 6.364 | 1.053 | 0.4 % | 0.7 % |
| kodim16/420/p0/d1 | 2.388 | 2.517 | 0.949 | 0.6 % | 0.5 % |
| kodim16/420/p0/d4 | 2.048 | 1.713 | 1.196 | 0.5 % | 0.5 % |
| kodim16/420/p2/d1 | 4.896 | 6.413 | 0.763 | 0.2 % | 0.3 % |
| kodim16/420/p2/d4 | 3.904 | 4.106 | 0.951 | 1.7 % | 0.6 % |
| kodim17/444/p0/d1 | 3.223 | 3.200 | 1.007 | 0.7 % | 0.2 % |
| kodim17/444/p0/d4 | 2.847 | 2.426 | 1.173 | 0.3 % | 0.1 % |
| kodim17/444/p2/d1 | 7.837 | 8.381 | 0.935 | 0.3 % | 0.3 % |
| kodim17/444/p2/d4 | 6.652 | 6.288 | 1.058 | 0.4 % | 0.1 % |
| kodim17/420/p0/d1 | 2.378 | 2.408 | 0.988 | 0.4 % | 0.1 % |
| kodim17/420/p0/d4 | 2.075 | 1.709 | 1.214 | 0.4 % | 0.1 % |
| kodim17/420/p2/d1 | 4.750 | 5.837 | 0.814 | 0.1 % | 0.4 % |
| kodim17/420/p2/d4 | 3.888 | 3.907 | 0.995 | 0.1 % | 0.3 % |
| kodim18/444/p0/d1 | 3.606 | 3.934 | 0.917 | 0.2 % | 0.2 % |
| kodim18/444/p0/d4 | 2.976 | 2.730 | 1.090 | 0.1 % | 0.2 % |
| kodim18/444/p2/d1 | 8.072 | 10.177 | 0.793 | 0.1 % | 0.5 % |
| kodim18/444/p2/d4 | 7.082 | 7.186 | 0.985 | 0.1 % | 0.5 % |
| kodim18/420/p0/d1 | 2.678 | 3.029 | 0.884 | 0.9 % | 0.2 % |
| kodim18/420/p0/d4 | 2.175 | 1.978 | 1.100 | 0.4 % | 0.1 % |
| kodim18/420/p2/d1 | 5.865 | 7.739 | 0.758 | 0.1 % | 0.1 % |
| kodim18/420/p2/d4 | 4.177 | 4.699 | 0.889 | 0.3 % | 0.6 % |
| kodim19/444/p0/d1 | 3.377 | 3.471 | 0.973 | 1.5 % | 0.3 % |
| kodim19/444/p0/d4 | 2.918 | 2.496 | 1.169 | 0.5 % | 0.0 % |
| kodim19/444/p2/d1 | 7.846 | 9.102 | 0.862 | 0.2 % | 0.3 % |
| kodim19/444/p2/d4 | 6.778 | 6.550 | 1.035 | 0.5 % | 1.4 % |
| kodim19/420/p0/d1 | 2.500 | 2.633 | 0.950 | 0.9 % | 0.1 % |
| kodim19/420/p0/d4 | 2.068 | 1.770 | 1.169 | 0.2 % | 0.2 % |
| kodim19/420/p2/d1 | 5.181 | 6.565 | 0.789 | 0.1 % | 0.2 % |
| kodim19/420/p2/d4 | 3.988 | 4.043 | 0.987 | 0.1 % | 0.7 % |
| kodim20/444/p0/d1 | 3.056 | 2.963 | 1.032 | 0.2 % | 0.3 % |
| kodim20/444/p0/d4 | 2.775 | 2.283 | 1.216 | 0.6 % | 0.1 % |
| kodim20/444/p2/d1 | 6.560 | 7.728 | 0.849 | 0.9 % | 0.2 % |
| kodim20/444/p2/d4 | 6.413 | 5.996 | 1.069 | 0.6 % | 0.9 % |
| kodim20/420/p0/d1 | 2.263 | 2.181 | 1.037 | 1.0 % | 0.1 % |
| kodim20/420/p0/d4 | 1.963 | 1.563 | 1.256 | 0.3 % | 0.2 % |
| kodim20/420/p2/d1 | 4.405 | 5.561 | 0.792 | 0.2 % | 0.3 % |
| kodim20/420/p2/d4 | 3.468 | 3.836 | 0.904 | 0.0 % | 0.4 % |
| kodim21/444/p0/d1 | 3.316 | 3.466 | 0.957 | 0.5 % | 0.2 % |
| kodim21/444/p0/d4 | 2.860 | 2.538 | 1.127 | 0.2 % | 0.1 % |
| kodim21/444/p2/d1 | 8.085 | 9.000 | 0.898 | 0.6 % | 0.5 % |
| kodim21/444/p2/d4 | 6.837 | 6.654 | 1.028 | 0.5 % | 0.6 % |
| kodim21/420/p0/d1 | 2.445 | 2.629 | 0.930 | 0.3 % | 0.4 % |
| kodim21/420/p0/d4 | 2.060 | 1.787 | 1.152 | 0.4 % | 0.1 % |
| kodim21/420/p2/d1 | 5.253 | 6.698 | 0.784 | 0.3 % | 0.8 % |
| kodim21/420/p2/d4 | 4.005 | 4.423 | 0.905 | 0.6 % | 0.6 % |
| kodim22/444/p0/d1 | 3.430 | 3.746 | 0.916 | 0.1 % | 0.4 % |
| kodim22/444/p0/d4 | 2.890 | 2.585 | 1.118 | 0.1 % | 0.3 % |
| kodim22/444/p2/d1 | 7.630 | 9.715 | 0.785 | 0.3 % | 0.7 % |
| kodim22/444/p2/d4 | 6.836 | 6.742 | 1.014 | 0.3 % | 0.2 % |
| kodim22/420/p0/d1 | 2.508 | 2.826 | 0.887 | 0.5 % | 0.6 % |
| kodim22/420/p0/d4 | 2.095 | 1.835 | 1.141 | 0.3 % | 0.2 % |
| kodim22/420/p2/d1 | 5.410 | 7.342 | 0.737 | 0.4 % | 0.5 % |
| kodim22/420/p2/d4 | 3.936 | 4.474 | 0.880 | 0.7 % | 0.4 % |
| kodim23/444/p0/d1 | 3.069 | 2.903 | 1.057 | 0.3 % | 0.1 % |
| kodim23/444/p0/d4 | 2.788 | 2.303 | 1.211 | 0.3 % | 0.1 % |
| kodim23/444/p2/d1 | 6.552 | 7.190 | 0.911 | 0.2 % | 0.4 % |
| kodim23/444/p2/d4 | 6.461 | 5.744 | 1.125 | 0.1 % | 0.3 % |
| kodim23/420/p0/d1 | 2.235 | 2.066 | 1.082 | 0.5 % | 0.1 % |
| kodim23/420/p0/d4 | 1.997 | 1.589 | 1.257 | 0.1 % | 0.2 % |
| kodim23/420/p2/d1 | 4.208 | 5.075 | 0.829 | 0.1 % | 0.4 % |
| kodim23/420/p2/d4 | 3.567 | 3.693 | 0.966 | 0.4 % | 0.3 % |
| kodim24/444/p0/d1 | 3.497 | 3.885 | 0.900 | 0.7 % | 0.1 % |
| kodim24/444/p0/d4 | 2.949 | 2.764 | 1.067 | 0.4 % | 0.2 % |
| kodim24/444/p2/d1 | 8.071 | 10.110 | 0.798 | 0.1 % | 0.1 % |
| kodim24/444/p2/d4 | 7.133 | 7.352 | 0.970 | 0.4 % | 0.4 % |
| kodim24/420/p0/d1 | 2.651 | 3.001 | 0.883 | 0.4 % | 0.1 % |
| kodim24/420/p0/d4 | 2.158 | 1.994 | 1.082 | 0.8 % | 0.1 % |
| kodim24/420/p2/d1 | 5.833 | 7.778 | 0.750 | 0.5 % | 0.3 % |
| kodim24/420/p2/d4 | 4.230 | 5.009 | 0.844 | 0.4 % | 0.3 % |
| smooth-gradient/444/p0/d1 | 0.434 | 0.285 | 1.519 | 0.4 % | 0.4 % |
| smooth-gradient/444/p0/d4 | 0.420 | 0.280 | 1.502 | 0.3 % | 0.2 % |
| smooth-gradient/444/p2/d1 | 0.982 | 0.561 | 1.751 | 0.2 % | 0.2 % |
| smooth-gradient/444/p2/d4 | 0.959 | 0.556 | 1.723 | 0.7 % | 0.5 % |
| smooth-gradient/420/p0/d1 | 0.309 | 0.216 | 1.430 | 1.1 % | 0.2 % |
| smooth-gradient/420/p0/d4 | 0.304 | 0.216 | 1.408 | 0.3 % | 0.2 % |
| smooth-gradient/420/p2/d1 | 0.612 | 0.348 | 1.759 | 0.2 % | 0.3 % |
| smooth-gradient/420/p2/d4 | 0.594 | 0.345 | 1.724 | 0.3 % | 0.2 % |
| low-contrast/444/p0/d1 | 0.514 | 0.367 | 1.402 | 0.2 % | 0.0 % |
| low-contrast/444/p0/d4 | 0.442 | 0.295 | 1.497 | 0.5 % | 0.2 % |
| low-contrast/444/p2/d1 | 1.221 | 1.001 | 1.220 | 0.6 % | 0.6 % |
| low-contrast/444/p2/d4 | 1.004 | 0.642 | 1.565 | 0.3 % | 0.4 % |
| low-contrast/420/p0/d1 | 0.369 | 0.302 | 1.222 | 0.8 % | 0.3 % |
| low-contrast/420/p0/d4 | 0.321 | 0.230 | 1.396 | 0.5 % | 0.5 % |
| low-contrast/420/p2/d1 | 0.754 | 0.700 | 1.077 | 0.6 % | 0.7 % |
| low-contrast/420/p2/d4 | 0.666 | 0.408 | 1.634 | 0.5 % | 0.3 % |
| color-edges/444/p0/d1 | 0.555 | 0.407 | 1.364 | 1.1 % | 0.9 % |
| color-edges/444/p0/d4 | 0.505 | 0.362 | 1.395 | 1.1 % | 0.1 % |
| color-edges/444/p2/d1 | 1.316 | 1.130 | 1.164 | 0.4 % | 0.5 % |
| color-edges/444/p2/d4 | 1.098 | 0.944 | 1.164 | 0.7 % | 0.4 % |
| color-edges/420/p0/d1 | 0.428 | 0.364 | 1.176 | 0.6 % | 1.5 % |
| color-edges/420/p0/d4 | 0.377 | 0.310 | 1.217 | 1.1 % | 0.2 % |
| color-edges/420/p2/d1 | 1.045 | 0.953 | 1.097 | 0.4 % | 0.3 % |
| color-edges/420/p2/d4 | 0.823 | 0.782 | 1.053 | 0.5 % | 0.3 % |
| line-art/444/p0/d1 | 0.523 | 0.362 | 1.445 | 0.9 % | 0.3 % |
| line-art/444/p0/d4 | 0.492 | 0.344 | 1.428 | 0.7 % | 0.0 % |
| line-art/444/p2/d1 | 1.319 | 0.928 | 1.421 | 0.4 % | 0.4 % |
| line-art/444/p2/d4 | 1.189 | 0.935 | 1.271 | 0.4 % | 0.9 % |
| line-art/420/p0/d1 | 0.400 | 0.331 | 1.210 | 0.7 % | 0.5 % |
| line-art/420/p0/d4 | 0.371 | 0.297 | 1.248 | 0.5 % | 0.8 % |
| line-art/420/p2/d1 | 0.919 | 0.709 | 1.296 | 0.6 % | 0.1 % |
| line-art/420/p2/d4 | 0.794 | 0.690 | 1.150 | 0.6 % | 0.3 % |
| structured-texture/444/p0/d1 | 0.758 | 1.033 | 0.734 | 0.4 % | 0.1 % |
| structured-texture/444/p0/d4 | 0.547 | 0.514 | 1.064 | 1.1 % | 0.3 % |
| structured-texture/444/p2/d1 | 2.171 | 2.678 | 0.811 | 0.1 % | 0.2 % |
| structured-texture/444/p2/d4 | 1.316 | 1.477 | 0.891 | 0.2 % | 0.2 % |
| structured-texture/420/p0/d1 | 0.513 | 0.747 | 0.686 | 1.3 % | 0.3 % |
| structured-texture/420/p0/d4 | 0.380 | 0.323 | 1.176 | 0.6 % | 0.9 % |
| structured-texture/420/p2/d1 | 1.390 | 1.725 | 0.806 | 0.2 % | 0.1 % |
| structured-texture/420/p2/d4 | 0.826 | 0.940 | 0.880 | 0.4 % | 0.4 % |
| low-noise/444/p0/d1 | 0.538 | 0.563 | 0.955 | 0.6 % | 0.1 % |
| low-noise/444/p0/d4 | 0.431 | 0.282 | 1.531 | 0.2 % | 0.1 % |
| low-noise/444/p2/d1 | 1.268 | 1.465 | 0.866 | 0.6 % | 0.8 % |
| low-noise/444/p2/d4 | 0.990 | 0.588 | 1.684 | 0.3 % | 0.4 % |
| low-noise/420/p0/d1 | 0.377 | 0.325 | 1.159 | 0.7 % | 0.6 % |
| low-noise/420/p0/d4 | 0.307 | 0.212 | 1.447 | 0.8 % | 0.5 % |
| low-noise/420/p2/d1 | 0.753 | 0.962 | 0.782 | 0.9 % | 0.3 % |
| low-noise/420/p2/d4 | 0.611 | 0.354 | 1.726 | 0.4 % | 0.4 % |
| full-noise/444/p0/d1 | 0.854 | 1.126 | 0.758 | 0.8 % | 0.2 % |
| full-noise/444/p0/d4 | 0.648 | 0.853 | 0.759 | 0.7 % | 0.2 % |
| full-noise/444/p2/d1 | 2.637 | 2.949 | 0.894 | 0.3 % | 0.1 % |
| full-noise/444/p2/d4 | 1.587 | 2.109 | 0.752 | 0.3 % | 0.0 % |
| full-noise/420/p0/d1 | 0.575 | 0.797 | 0.722 | 1.1 % | 0.3 % |
| full-noise/420/p0/d4 | 0.435 | 0.578 | 0.754 | 0.8 % | 0.1 % |
| full-noise/420/p2/d1 | 1.710 | 1.746 | 0.980 | 0.3 % | 0.4 % |
| full-noise/420/p2/d4 | 1.051 | 1.325 | 0.793 | 0.5 % | 0.1 % |
| tiny-odd-edges/444/p0/d1 | 0.044 | 0.019 | 2.261 | 0.4 % | 1.1 % |
| tiny-odd-edges/444/p0/d4 | 0.042 | 0.023 | 1.828 | 0.8 % | 0.2 % |
| tiny-odd-edges/444/p2/d1 | 0.098 | 0.039 | 2.527 | 1.1 % | 0.3 % |
| tiny-odd-edges/444/p2/d4 | 0.092 | 0.040 | 2.312 | 0.4 % | 0.5 % |
| tiny-odd-edges/420/p0/d1 | 0.040 | 0.018 | 2.229 | 1.2 % | 0.7 % |
| tiny-odd-edges/420/p0/d4 | 0.040 | 0.022 | 1.869 | 0.4 % | 0.2 % |
| tiny-odd-edges/420/p2/d1 | 0.092 | 0.035 | 2.615 | 0.9 % | 0.4 % |
| tiny-odd-edges/420/p2/d4 | 0.086 | 0.036 | 2.401 | 0.6 % | 0.5 % |

Exact input/JPEG files, SHA256 identities, per-block raw samples, p10/p50/p90, relative MAD, allocations/collections, calibration counts and native command outputs are retained alongside this report. summary.json records CPU, affinity, OS, runtime, vector widths, policy, revision and group verdicts.

Failed bounds:

- kodim02/444/p0/d4: median latency ratio 1.165149 exceeds 1.10.
- kodim02/420/p0/d4: median latency ratio 1.201696 exceeds 1.10.
- kodim03/444/p0/d4: median latency ratio 1.204302 exceeds 1.10.
- kodim03/420/p0/d4: median latency ratio 1.244280 exceeds 1.10.
- kodim04/444/p0/d4: median latency ratio 1.168151 exceeds 1.10.
- kodim04/420/p0/d4: median latency ratio 1.207030 exceeds 1.10.
- kodim06/444/p0/d4: median latency ratio 1.109288 exceeds 1.10.
- kodim06/420/p0/d4: median latency ratio 1.112714 exceeds 1.10.
- kodim07/444/p0/d4: median latency ratio 1.167949 exceeds 1.10.
- kodim07/420/p0/d4: median latency ratio 1.193241 exceeds 1.10.
- kodim09/444/p0/d4: median latency ratio 1.213306 exceeds 1.10.
- kodim09/420/p0/d4: median latency ratio 1.251436 exceeds 1.10.
- kodim10/444/p0/d4: median latency ratio 1.196272 exceeds 1.10.
- kodim10/420/p0/d4: median latency ratio 1.220314 exceeds 1.10.
- kodim11/444/p0/d4: median latency ratio 1.129722 exceeds 1.10.
- kodim11/420/p0/d4: median latency ratio 1.158401 exceeds 1.10.
- kodim12/444/p0/d4: median latency ratio 1.180103 exceeds 1.10.
- kodim12/420/p0/d4: median latency ratio 1.219855 exceeds 1.10.
- kodim15/444/p0/d4: median latency ratio 1.169559 exceeds 1.10.
- kodim15/420/p0/d4: median latency ratio 1.206819 exceeds 1.10.
- kodim16/444/p0/d4: median latency ratio 1.162215 exceeds 1.10.
- kodim16/420/p0/d4: median latency ratio 1.195518 exceeds 1.10.
- kodim17/444/p0/d4: median latency ratio 1.173475 exceeds 1.10.
- kodim17/420/p0/d4: median latency ratio 1.214086 exceeds 1.10.
- kodim19/444/p0/d4: median latency ratio 1.169053 exceeds 1.10.
- kodim19/420/p0/d4: median latency ratio 1.168902 exceeds 1.10.
- kodim20/444/p0/d4: median latency ratio 1.215737 exceeds 1.10.
- kodim20/420/p0/d4: median latency ratio 1.256277 exceeds 1.10.
- kodim21/444/p0/d4: median latency ratio 1.126709 exceeds 1.10.
- kodim21/420/p0/d4: median latency ratio 1.152278 exceeds 1.10.
- kodim22/444/p0/d4: median latency ratio 1.118169 exceeds 1.10.
- kodim22/420/p0/d4: median latency ratio 1.141316 exceeds 1.10.
- kodim23/444/p0/d4: median latency ratio 1.210509 exceeds 1.10.
- kodim23/444/p2/d4: median latency ratio 1.124929 exceeds 1.10.
- kodim23/420/p0/d4: median latency ratio 1.256969 exceeds 1.10.
- smooth-gradient/444/p0/d1: median latency ratio 1.518804 exceeds 1.10.
- smooth-gradient/444/p0/d4: median latency ratio 1.501524 exceeds 1.10.
- smooth-gradient/444/p2/d1: median latency ratio 1.751071 exceeds 1.10.
- smooth-gradient/444/p2/d4: median latency ratio 1.722949 exceeds 1.10.
- smooth-gradient/420/p0/d1: median latency ratio 1.430030 exceeds 1.10.
- smooth-gradient/420/p0/d4: median latency ratio 1.408092 exceeds 1.10.
- smooth-gradient/420/p2/d1: median latency ratio 1.759229 exceeds 1.10.
- smooth-gradient/420/p2/d4: median latency ratio 1.724221 exceeds 1.10.
- low-contrast/444/p0/d1: median latency ratio 1.401988 exceeds 1.10.
- low-contrast/444/p0/d4: median latency ratio 1.496540 exceeds 1.10.
- low-contrast/444/p2/d1: median latency ratio 1.220277 exceeds 1.10.
- low-contrast/444/p2/d4: median latency ratio 1.564831 exceeds 1.10.
- low-contrast/420/p0/d1: median latency ratio 1.221688 exceeds 1.10.
- low-contrast/420/p0/d4: median latency ratio 1.395823 exceeds 1.10.
- low-contrast/420/p2/d4: median latency ratio 1.634097 exceeds 1.10.
- color-edges/444/p0/d1: median latency ratio 1.364463 exceeds 1.10.
- color-edges/444/p0/d4: median latency ratio 1.395497 exceeds 1.10.
- color-edges/444/p2/d1: median latency ratio 1.164477 exceeds 1.10.
- color-edges/444/p2/d4: median latency ratio 1.163565 exceeds 1.10.
- color-edges/420/p0/d1: median latency ratio 1.175775 exceeds 1.10.
- color-edges/420/p0/d4: median latency ratio 1.217271 exceeds 1.10.
- line-art/444/p0/d1: median latency ratio 1.444951 exceeds 1.10.
- line-art/444/p0/d4: median latency ratio 1.427688 exceeds 1.10.
- line-art/444/p2/d1: median latency ratio 1.420678 exceeds 1.10.
- line-art/444/p2/d4: median latency ratio 1.271391 exceeds 1.10.
- line-art/420/p0/d1: median latency ratio 1.209671 exceeds 1.10.
- line-art/420/p0/d4: median latency ratio 1.247587 exceeds 1.10.
- line-art/420/p2/d1: median latency ratio 1.295790 exceeds 1.10.
- line-art/420/p2/d4: median latency ratio 1.150369 exceeds 1.10.
- structured-texture/420/p0/d4: median latency ratio 1.175537 exceeds 1.10.
- low-noise/444/p0/d4: median latency ratio 1.530702 exceeds 1.10.
- low-noise/444/p2/d4: median latency ratio 1.684397 exceeds 1.10.
- low-noise/420/p0/d1: median latency ratio 1.158542 exceeds 1.10.
- low-noise/420/p0/d4: median latency ratio 1.446525 exceeds 1.10.
- low-noise/420/p2/d4: median latency ratio 1.725951 exceeds 1.10.
- tiny-odd-edges/444/p0/d1: median latency ratio 2.260598 exceeds 1.10.
- tiny-odd-edges/444/p0/d4: median latency ratio 1.827999 exceeds 1.10.
- tiny-odd-edges/444/p2/d1: median latency ratio 2.527178 exceeds 1.10.
- tiny-odd-edges/444/p2/d4: median latency ratio 2.312027 exceeds 1.10.
- tiny-odd-edges/420/p0/d1: median latency ratio 2.229427 exceeds 1.10.
- tiny-odd-edges/420/p0/d4: median latency ratio 1.869196 exceeds 1.10.
- tiny-odd-edges/420/p2/d1: median latency ratio 2.614697 exceeds 1.10.
- tiny-odd-edges/420/p2/d4: median latency ratio 2.401472 exceeds 1.10.
- controlled/444/p0: geometric mean latency ratio 1.278679 exceeds 1.05.
- controlled/444/p2: geometric mean latency ratio 1.289391 exceeds 1.05.
- controlled/420/p0: geometric mean latency ratio 1.215573 exceeds 1.05.
- controlled/420/p2: geometric mean latency ratio 1.264848 exceeds 1.05.
