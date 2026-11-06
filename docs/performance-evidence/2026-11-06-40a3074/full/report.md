# Performance parity: FAILED

256/256 cases: RGB8 YCbCr, Kodak24 plus eight fixed controls, distances1/4, 444/420 and progression0/2, one worker on this CI host. Pinned upstream `031a0077f5799a6041004267fc12b956c1f52a20`.
Tested revision: `40a3074c61fbb052e452a82faf6d74616d060060`.

All cases are measured even when a latency bound fails, so the report identifies every measured regression. A passing verdict requires complete, valid coverage and all fixed bounds.

Fixed latency noninferiority limits: geometric mean managed/native median latency ratio <=1.05 in each corpus/sampling/progression group; every case <=1.10. A faster case cannot excuse a slower case. No universal performance claim is made.

Seven alternating order blocks, five untimed warmups before each block. The native-only three-iteration calibration targets50ms and selects a shared count clamped3..100. Medians use all seven blocks; no outliers are discarded. Complete in-memory encode calls are timed individually and their elapsed times summed; subprocess startup, file I/O, verification and native buffer free are excluded. Managed collections inside encode are included. Tiered compilation is disabled.

Evidence is invalid if a block interval is below1ms, any case has relative MAD >10%, inputs/settings/coverage differ, or an untimed JPEG structure comparison fails. Invalid evidence cannot establish parity.

| Corpus | Sampling | Progression | Cases | Geometric mean ratio | Worst ratio | Verdict |
| --- | --- | --- | ---: | ---: | ---: | --- |
| natural | 444 | 0 | 48 | 1.060785 | 1.423404 | FAIL |
| natural | 444 | 2 | 48 | 0.889437 | 1.054350 | PASS |
| natural | 420 | 0 | 48 | 1.060078 | 1.309513 | FAIL |
| natural | 420 | 2 | 48 | 0.882312 | 1.152166 | FAIL |
| controlled | 444 | 0 | 16 | 1.115609 | 1.419479 | FAIL |
| controlled | 444 | 2 | 16 | 1.080177 | 1.306306 | FAIL |
| controlled | 420 | 0 | 16 | 1.021440 | 1.381448 | FAIL |
| controlled | 420 | 2 | 16 | 1.077625 | 1.344009 | FAIL |

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 4.608 | 5.084 | 0.906 | 0.1 % | 0.2 % |
| kodim01/444/p0/d4 | 3.841 | 3.593 | 1.069 | 0.6 % | 0.2 % |
| kodim01/444/p2/d1 | 10.554 | 12.749 | 0.828 | 0.1 % | 0.6 % |
| kodim01/444/p2/d4 | 12.091 | 11.468 | 1.054 | 4.6 % | 0.3 % |
| kodim01/420/p0/d1 | 3.585 | 4.071 | 0.880 | 0.2 % | 0.6 % |
| kodim01/420/p0/d4 | 2.815 | 2.602 | 1.082 | 0.1 % | 0.1 % |
| kodim01/420/p2/d1 | 7.819 | 9.845 | 0.794 | 0.3 % | 0.2 % |
| kodim01/420/p2/d4 | 5.233 | 5.999 | 0.872 | 0.3 % | 0.1 % |
| kodim02/444/p0/d1 | 4.294 | 4.254 | 1.009 | 0.2 % | 0.1 % |
| kodim02/444/p0/d4 | 3.616 | 3.046 | 1.187 | 0.3 % | 0.2 % |
| kodim02/444/p2/d1 | 8.759 | 10.251 | 0.854 | 0.2 % | 0.2 % |
| kodim02/444/p2/d4 | 6.813 | 7.219 | 0.944 | 0.2 % | 0.6 % |
| kodim02/420/p0/d1 | 3.183 | 3.248 | 0.980 | 0.2 % | 0.1 % |
| kodim02/420/p0/d4 | 2.666 | 2.166 | 1.231 | 0.3 % | 0.3 % |
| kodim02/420/p2/d1 | 6.125 | 7.605 | 0.805 | 0.1 % | 0.0 % |
| kodim02/420/p2/d4 | 4.481 | 4.751 | 0.943 | 0.5 % | 0.6 % |
| kodim03/444/p0/d1 | 3.888 | 3.654 | 1.064 | 0.3 % | 0.1 % |
| kodim03/444/p0/d4 | 3.517 | 2.966 | 1.186 | 0.4 % | 0.1 % |
| kodim03/444/p2/d1 | 7.849 | 8.871 | 0.885 | 0.2 % | 0.2 % |
| kodim03/444/p2/d4 | 6.640 | 7.079 | 0.938 | 0.1 % | 0.2 % |
| kodim03/420/p0/d1 | 2.929 | 2.731 | 1.073 | 0.1 % | 0.2 % |
| kodim03/420/p0/d4 | 2.618 | 2.111 | 1.240 | 0.1 % | 0.1 % |
| kodim03/420/p2/d1 | 5.505 | 6.451 | 0.853 | 0.2 % | 0.3 % |
| kodim03/420/p2/d4 | 4.406 | 4.688 | 0.940 | 0.2 % | 0.3 % |
| kodim04/444/p0/d1 | 4.215 | 4.194 | 1.005 | 0.4 % | 0.1 % |
| kodim04/444/p0/d4 | 3.654 | 3.081 | 1.186 | 0.2 % | 0.0 % |
| kodim04/444/p2/d1 | 8.671 | 10.158 | 0.854 | 0.2 % | 0.2 % |
| kodim04/444/p2/d4 | 6.958 | 7.392 | 0.941 | 0.1 % | 0.0 % |
| kodim04/420/p0/d1 | 3.197 | 3.200 | 0.999 | 0.3 % | 0.2 % |
| kodim04/420/p0/d4 | 2.717 | 2.210 | 1.229 | 0.1 % | 0.1 % |
| kodim04/420/p2/d1 | 6.143 | 7.267 | 0.845 | 0.3 % | 0.2 % |
| kodim04/420/p2/d4 | 4.620 | 4.709 | 0.981 | 0.1 % | 0.4 % |
| kodim05/444/p0/d1 | 4.687 | 5.000 | 0.937 | 0.2 % | 0.1 % |
| kodim05/444/p0/d4 | 3.882 | 3.691 | 1.052 | 0.7 % | 0.3 % |
| kodim05/444/p2/d1 | 10.639 | 12.517 | 0.850 | 0.3 % | 0.1 % |
| kodim05/444/p2/d4 | 7.962 | 8.982 | 0.886 | 0.1 % | 0.6 % |
| kodim05/420/p0/d1 | 3.640 | 4.621 | 0.788 | 0.1 % | 0.1 % |
| kodim05/420/p0/d4 | 2.947 | 2.748 | 1.072 | 0.1 % | 0.1 % |
| kodim05/420/p2/d1 | 8.035 | 9.910 | 0.811 | 0.2 % | 0.2 % |
| kodim05/420/p2/d4 | 5.578 | 6.466 | 0.863 | 0.2 % | 0.1 % |
| kodim06/444/p0/d1 | 4.341 | 4.507 | 0.963 | 0.3 % | 0.1 % |
| kodim06/444/p0/d4 | 3.635 | 3.325 | 1.093 | 0.3 % | 0.1 % |
| kodim06/444/p2/d1 | 9.333 | 11.039 | 0.845 | 0.1 % | 0.1 % |
| kodim06/444/p2/d4 | 7.216 | 8.124 | 0.888 | 0.1 % | 0.6 % |
| kodim06/420/p0/d1 | 3.299 | 3.540 | 0.932 | 0.1 % | 0.2 % |
| kodim06/420/p0/d4 | 2.748 | 2.422 | 1.135 | 0.2 % | 0.1 % |
| kodim06/420/p2/d1 | 6.863 | 8.445 | 0.813 | 0.2 % | 0.1 % |
| kodim06/420/p2/d4 | 4.901 | 5.592 | 0.877 | 0.3 % | 1.3 % |
| kodim07/444/p0/d1 | 4.061 | 3.798 | 1.069 | 0.7 % | 0.1 % |
| kodim07/444/p0/d4 | 3.599 | 3.087 | 1.166 | 0.3 % | 0.2 % |
| kodim07/444/p2/d1 | 8.360 | 9.223 | 0.906 | 0.2 % | 0.1 % |
| kodim07/444/p2/d4 | 6.994 | 7.460 | 0.938 | 0.5 % | 0.4 % |
| kodim07/420/p0/d1 | 3.006 | 2.872 | 1.047 | 0.4 % | 0.1 % |
| kodim07/420/p0/d4 | 2.938 | 2.243 | 1.310 | 8.9 % | 1.0 % |
| kodim07/420/p2/d1 | 9.041 | 7.847 | 1.152 | 5.8 % | 9.2 % |
| kodim07/420/p2/d4 | 4.686 | 5.011 | 0.935 | 0.3 % | 0.1 % |
| kodim08/444/p0/d1 | 4.678 | 5.091 | 0.919 | 0.2 % | 0.1 % |
| kodim08/444/p0/d4 | 3.824 | 3.670 | 1.042 | 0.1 % | 0.1 % |
| kodim08/444/p2/d1 | 10.574 | 12.691 | 0.833 | 0.5 % | 0.4 % |
| kodim08/444/p2/d4 | 7.946 | 8.879 | 0.895 | 0.1 % | 0.2 % |
| kodim08/420/p0/d1 | 3.570 | 4.675 | 0.764 | 0.2 % | 0.1 % |
| kodim08/420/p0/d4 | 2.913 | 2.722 | 1.070 | 0.2 % | 0.2 % |
| kodim08/420/p2/d1 | 7.917 | 9.924 | 0.798 | 0.2 % | 0.1 % |
| kodim08/420/p2/d4 | 5.540 | 6.363 | 0.871 | 0.5 % | 0.3 % |
| kodim09/444/p0/d1 | 3.916 | 3.706 | 1.056 | 0.2 % | 0.2 % |
| kodim09/444/p0/d4 | 3.522 | 2.992 | 1.177 | 0.3 % | 0.2 % |
| kodim09/444/p2/d1 | 7.916 | 8.808 | 0.899 | 0.2 % | 0.1 % |
| kodim09/444/p2/d4 | 6.708 | 7.124 | 0.942 | 0.1 % | 0.2 % |
| kodim09/420/p0/d1 | 2.970 | 2.745 | 1.082 | 0.1 % | 0.1 % |
| kodim09/420/p0/d4 | 2.615 | 2.124 | 1.231 | 0.3 % | 0.2 % |
| kodim09/420/p2/d1 | 5.554 | 6.185 | 0.898 | 0.3 % | 0.4 % |
| kodim09/420/p2/d4 | 4.419 | 4.504 | 0.981 | 0.1 % | 0.3 % |
| kodim10/444/p0/d1 | 4.029 | 3.875 | 1.040 | 0.1 % | 0.1 % |
| kodim10/444/p0/d4 | 3.561 | 3.054 | 1.166 | 0.1 % | 0.2 % |
| kodim10/444/p2/d1 | 8.244 | 9.362 | 0.881 | 0.1 % | 0.2 % |
| kodim10/444/p2/d4 | 6.870 | 7.332 | 0.937 | 0.2 % | 0.3 % |
| kodim10/420/p0/d1 | 3.098 | 2.910 | 1.064 | 0.4 % | 0.1 % |
| kodim10/420/p0/d4 | 2.696 | 2.175 | 1.240 | 0.3 % | 0.1 % |
| kodim10/420/p2/d1 | 5.855 | 6.578 | 0.890 | 0.1 % | 0.3 % |
| kodim10/420/p2/d4 | 4.600 | 4.636 | 0.992 | 0.2 % | 0.3 % |
| kodim11/444/p0/d1 | 4.305 | 4.447 | 0.968 | 0.4 % | 0.1 % |
| kodim11/444/p0/d4 | 3.606 | 3.240 | 1.113 | 0.1 % | 0.1 % |
| kodim11/444/p2/d1 | 9.238 | 10.920 | 0.846 | 0.1 % | 0.1 % |
| kodim11/444/p2/d4 | 7.112 | 7.855 | 0.905 | 0.1 % | 0.7 % |
| kodim11/420/p0/d1 | 3.229 | 3.483 | 0.927 | 0.2 % | 0.2 % |
| kodim11/420/p0/d4 | 2.733 | 2.346 | 1.165 | 0.2 % | 0.1 % |
| kodim11/420/p2/d1 | 6.694 | 8.275 | 0.809 | 0.2 % | 0.2 % |
| kodim11/420/p2/d4 | 4.808 | 5.328 | 0.902 | 0.4 % | 0.5 % |
| kodim12/444/p0/d1 | 3.976 | 3.910 | 1.017 | 0.3 % | 0.1 % |
| kodim12/444/p0/d4 | 3.487 | 2.990 | 1.166 | 0.2 % | 0.1 % |
| kodim12/444/p2/d1 | 8.110 | 9.454 | 0.858 | 0.6 % | 0.3 % |
| kodim12/444/p2/d4 | 6.631 | 7.121 | 0.931 | 0.2 % | 0.2 % |
| kodim12/420/p0/d1 | 3.009 | 2.937 | 1.025 | 0.5 % | 0.1 % |
| kodim12/420/p0/d4 | 2.623 | 2.128 | 1.233 | 0.1 % | 0.1 % |
| kodim12/420/p2/d1 | 5.671 | 6.768 | 0.838 | 0.2 % | 0.2 % |
| kodim12/420/p2/d4 | 4.374 | 4.705 | 0.930 | 0.1 % | 0.4 % |
| kodim13/444/p0/d1 | 4.854 | 5.574 | 0.871 | 0.2 % | 0.2 % |
| kodim13/444/p0/d4 | 3.850 | 3.872 | 0.994 | 0.0 % | 0.1 % |
| kodim13/444/p2/d1 | 11.467 | 13.963 | 0.821 | 0.2 % | 0.2 % |
| kodim13/444/p2/d4 | 8.024 | 9.384 | 0.855 | 0.1 % | 0.3 % |
| kodim13/420/p0/d1 | 3.777 | 5.246 | 0.720 | 0.2 % | 0.1 % |
| kodim13/420/p0/d4 | 2.935 | 2.862 | 1.025 | 0.3 % | 0.1 % |
| kodim13/420/p2/d1 | 8.765 | 11.308 | 0.775 | 0.6 % | 0.1 % |
| kodim13/420/p2/d4 | 8.876 | 8.512 | 1.043 | 0.5 % | 0.5 % |
| kodim14/444/p0/d1 | 4.501 | 4.932 | 0.913 | 0.1 % | 0.2 % |
| kodim14/444/p0/d4 | 3.765 | 3.476 | 1.083 | 0.1 % | 0.1 % |
| kodim14/444/p2/d1 | 10.112 | 12.382 | 0.817 | 0.1 % | 0.3 % |
| kodim14/444/p2/d4 | 7.545 | 8.443 | 0.894 | 0.2 % | 0.2 % |
| kodim14/420/p0/d1 | 3.448 | 3.905 | 0.883 | 0.1 % | 0.3 % |
| kodim14/420/p0/d4 | 2.813 | 2.528 | 1.113 | 0.2 % | 0.2 % |
| kodim14/420/p2/d1 | 7.464 | 9.408 | 0.793 | 0.1 % | 0.1 % |
| kodim14/420/p2/d4 | 5.108 | 5.838 | 0.875 | 0.2 % | 0.3 % |
| kodim15/444/p0/d1 | 4.072 | 3.946 | 1.032 | 0.6 % | 0.0 % |
| kodim15/444/p0/d4 | 3.540 | 3.033 | 1.167 | 0.1 % | 0.2 % |
| kodim15/444/p2/d1 | 8.353 | 9.647 | 0.866 | 0.3 % | 0.5 % |
| kodim15/444/p2/d4 | 6.782 | 7.237 | 0.937 | 0.1 % | 0.3 % |
| kodim15/420/p0/d1 | 3.041 | 2.983 | 1.019 | 0.5 % | 0.2 % |
| kodim15/420/p0/d4 | 2.628 | 2.161 | 1.216 | 0.4 % | 0.1 % |
| kodim15/420/p2/d1 | 5.829 | 6.945 | 0.839 | 0.4 % | 0.4 % |
| kodim15/420/p2/d4 | 4.477 | 4.800 | 0.933 | 0.2 % | 0.2 % |
| kodim16/444/p0/d1 | 4.015 | 4.077 | 0.985 | 0.2 % | 0.1 % |
| kodim16/444/p0/d4 | 3.529 | 3.092 | 1.141 | 0.4 % | 0.1 % |
| kodim16/444/p2/d1 | 8.532 | 9.985 | 0.854 | 0.3 % | 0.3 % |
| kodim16/444/p2/d4 | 6.861 | 7.433 | 0.923 | 0.4 % | 0.2 % |
| kodim16/420/p0/d1 | 3.097 | 3.125 | 0.991 | 0.3 % | 0.2 % |
| kodim16/420/p0/d4 | 2.643 | 2.221 | 1.190 | 0.2 % | 0.2 % |
| kodim16/420/p2/d1 | 6.121 | 7.372 | 0.830 | 0.4 % | 0.3 % |
| kodim16/420/p2/d4 | 4.524 | 4.963 | 0.911 | 0.3 % | 0.3 % |
| kodim17/444/p0/d1 | 4.068 | 3.993 | 1.019 | 0.1 % | 0.4 % |
| kodim17/444/p0/d4 | 3.554 | 3.097 | 1.148 | 0.3 % | 0.2 % |
| kodim17/444/p2/d1 | 8.430 | 9.691 | 0.870 | 0.4 % | 0.3 % |
| kodim17/444/p2/d4 | 6.904 | 7.477 | 0.923 | 0.1 % | 0.2 % |
| kodim17/420/p0/d1 | 3.109 | 3.017 | 1.031 | 0.3 % | 0.1 % |
| kodim17/420/p0/d4 | 2.703 | 2.214 | 1.221 | 0.3 % | 0.3 % |
| kodim17/420/p2/d1 | 6.016 | 6.897 | 0.872 | 0.3 % | 0.3 % |
| kodim17/420/p2/d4 | 4.638 | 4.785 | 0.969 | 0.1 % | 0.7 % |
| kodim18/444/p0/d1 | 4.586 | 4.802 | 0.955 | 0.4 % | 0.3 % |
| kodim18/444/p0/d4 | 3.752 | 3.431 | 1.094 | 0.2 % | 0.1 % |
| kodim18/444/p2/d1 | 10.005 | 11.754 | 0.851 | 0.1 % | 0.3 % |
| kodim18/444/p2/d4 | 7.549 | 8.502 | 0.888 | 0.1 % | 0.3 % |
| kodim18/420/p0/d1 | 3.458 | 3.764 | 0.919 | 0.1 % | 0.1 % |
| kodim18/420/p0/d4 | 2.863 | 2.514 | 1.139 | 0.1 % | 0.1 % |
| kodim18/420/p2/d1 | 7.461 | 8.992 | 0.830 | 0.1 % | 0.2 % |
| kodim18/420/p2/d4 | 5.171 | 5.600 | 0.923 | 0.2 % | 0.3 % |
| kodim19/444/p0/d1 | 4.293 | 4.276 | 1.004 | 0.4 % | 0.0 % |
| kodim19/444/p0/d4 | 3.598 | 3.169 | 1.136 | 0.1 % | 0.0 % |
| kodim19/444/p2/d1 | 8.954 | 10.509 | 0.852 | 0.1 % | 0.2 % |
| kodim19/444/p2/d4 | 7.054 | 7.640 | 0.923 | 0.1 % | 0.2 % |
| kodim19/420/p0/d1 | 3.257 | 3.290 | 0.990 | 0.3 % | 0.1 % |
| kodim19/420/p0/d4 | 2.741 | 2.274 | 1.205 | 0.2 % | 0.2 % |
| kodim19/420/p2/d1 | 6.453 | 7.623 | 0.847 | 0.6 % | 0.2 % |
| kodim19/420/p2/d4 | 4.751 | 4.938 | 0.962 | 0.4 % | 0.5 % |
| kodim20/444/p0/d1 | 3.898 | 3.770 | 1.034 | 1.8 % | 1.9 % |
| kodim20/444/p0/d4 | 5.121 | 3.597 | 1.423 | 6.7 % | 2.1 % |
| kodim20/444/p2/d1 | 7.879 | 8.951 | 0.880 | 0.1 % | 0.3 % |
| kodim20/444/p2/d4 | 6.636 | 7.117 | 0.932 | 0.5 % | 0.3 % |
| kodim20/420/p0/d1 | 2.892 | 2.801 | 1.033 | 0.4 % | 0.2 % |
| kodim20/420/p0/d4 | 2.530 | 2.096 | 1.207 | 0.1 % | 0.3 % |
| kodim20/420/p2/d1 | 5.508 | 6.656 | 0.827 | 0.2 % | 0.2 % |
| kodim20/420/p2/d4 | 4.330 | 4.690 | 0.923 | 0.3 % | 0.2 % |
| kodim21/444/p0/d1 | 4.248 | 4.282 | 0.992 | 0.2 % | 0.1 % |
| kodim21/444/p0/d4 | 3.601 | 3.228 | 1.115 | 0.4 % | 0.1 % |
| kodim21/444/p2/d1 | 8.940 | 10.418 | 0.858 | 0.3 % | 0.4 % |
| kodim21/444/p2/d4 | 7.047 | 7.787 | 0.905 | 0.2 % | 0.4 % |
| kodim21/420/p0/d1 | 3.219 | 3.291 | 0.978 | 0.2 % | 0.1 % |
| kodim21/420/p0/d4 | 2.674 | 2.325 | 1.150 | 0.3 % | 0.2 % |
| kodim21/420/p2/d1 | 6.482 | 7.753 | 0.836 | 0.4 % | 0.3 % |
| kodim21/420/p2/d4 | 4.731 | 5.320 | 0.889 | 0.1 % | 0.2 % |
| kodim22/444/p0/d1 | 4.469 | 4.556 | 0.981 | 1.1 % | 0.2 % |
| kodim22/444/p0/d4 | 3.673 | 3.259 | 1.127 | 0.3 % | 0.2 % |
| kodim22/444/p2/d1 | 9.475 | 11.175 | 0.848 | 0.7 % | 0.2 % |
| kodim22/444/p2/d4 | 7.092 | 7.896 | 0.898 | 0.1 % | 0.5 % |
| kodim22/420/p0/d1 | 3.241 | 3.495 | 0.927 | 0.2 % | 0.1 % |
| kodim22/420/p0/d4 | 2.750 | 2.342 | 1.175 | 0.1 % | 0.1 % |
| kodim22/420/p2/d1 | 6.694 | 8.384 | 0.798 | 0.2 % | 0.2 % |
| kodim22/420/p2/d4 | 4.756 | 5.312 | 0.895 | 0.3 % | 0.8 % |
| kodim23/444/p0/d1 | 3.995 | 3.570 | 1.119 | 0.9 % | 0.2 % |
| kodim23/444/p0/d4 | 3.551 | 2.929 | 1.212 | 0.2 % | 0.2 % |
| kodim23/444/p2/d1 | 7.750 | 8.435 | 0.919 | 0.1 % | 0.1 % |
| kodim23/444/p2/d4 | 6.651 | 6.934 | 0.959 | 0.8 % | 0.1 % |
| kodim23/420/p0/d1 | 2.922 | 2.616 | 1.117 | 0.4 % | 0.0 % |
| kodim23/420/p0/d4 | 2.642 | 2.077 | 1.272 | 0.2 % | 0.3 % |
| kodim23/420/p2/d1 | 5.290 | 5.973 | 0.886 | 0.1 % | 0.2 % |
| kodim23/420/p2/d4 | 4.337 | 4.510 | 0.962 | 0.2 % | 0.5 % |
| kodim24/444/p0/d1 | 4.543 | 4.756 | 0.955 | 0.3 % | 0.1 % |
| kodim24/444/p0/d4 | 3.748 | 3.463 | 1.082 | 0.2 % | 0.0 % |
| kodim24/444/p2/d1 | 9.960 | 11.737 | 0.849 | 0.3 % | 0.1 % |
| kodim24/444/p2/d4 | 7.548 | 8.549 | 0.883 | 0.1 % | 0.1 % |
| kodim24/420/p0/d1 | 3.410 | 3.740 | 0.912 | 0.1 % | 0.1 % |
| kodim24/420/p0/d4 | 2.829 | 2.549 | 1.110 | 0.3 % | 0.2 % |
| kodim24/420/p2/d1 | 7.287 | 9.027 | 0.807 | 0.1 % | 0.2 % |
| kodim24/420/p2/d4 | 5.140 | 5.921 | 0.868 | 0.2 % | 0.3 % |
| smooth-gradient/444/p0/d1 | 0.521 | 0.367 | 1.419 | 0.4 % | 0.1 % |
| smooth-gradient/444/p0/d4 | 0.489 | 0.368 | 1.330 | 0.1 % | 0.1 % |
| smooth-gradient/444/p2/d1 | 0.899 | 0.688 | 1.306 | 0.8 % | 0.2 % |
| smooth-gradient/444/p2/d4 | 0.863 | 0.684 | 1.262 | 0.8 % | 0.3 % |
| smooth-gradient/420/p0/d1 | 0.391 | 0.295 | 1.325 | 0.4 % | 0.2 % |
| smooth-gradient/420/p0/d4 | 0.384 | 0.295 | 1.301 | 0.6 % | 0.4 % |
| smooth-gradient/420/p2/d1 | 0.594 | 0.442 | 1.344 | 0.5 % | 0.2 % |
| smooth-gradient/420/p2/d4 | 0.578 | 0.440 | 1.315 | 0.5 % | 0.2 % |
| low-contrast/444/p0/d1 | 0.650 | 0.506 | 1.284 | 0.5 % | 0.3 % |
| low-contrast/444/p0/d4 | 0.546 | 0.390 | 1.400 | 0.3 % | 0.4 % |
| low-contrast/444/p2/d1 | 1.296 | 1.162 | 1.115 | 0.1 % | 0.1 % |
| low-contrast/444/p2/d4 | 1.020 | 0.796 | 1.282 | 0.4 % | 0.4 % |
| low-contrast/420/p0/d1 | 0.469 | 0.439 | 1.068 | 0.4 % | 0.8 % |
| low-contrast/420/p0/d4 | 0.405 | 0.317 | 1.276 | 0.2 % | 0.3 % |
| low-contrast/420/p2/d1 | 0.900 | 0.832 | 1.081 | 0.5 % | 0.3 % |
| low-contrast/420/p2/d4 | 0.671 | 0.528 | 1.270 | 0.8 % | 0.7 % |
| color-edges/444/p0/d1 | 0.730 | 0.575 | 1.268 | 6.3 % | 0.6 % |
| color-edges/444/p0/d4 | 0.619 | 0.487 | 1.271 | 0.7 % | 0.4 % |
| color-edges/444/p2/d1 | 1.546 | 1.348 | 1.147 | 0.4 % | 0.3 % |
| color-edges/444/p2/d4 | 1.320 | 1.148 | 1.150 | 0.4 % | 0.3 % |
| color-edges/420/p0/d1 | 0.525 | 0.554 | 0.948 | 0.8 % | 0.2 % |
| color-edges/420/p0/d4 | 0.473 | 0.453 | 1.043 | 0.6 % | 0.5 % |
| color-edges/420/p2/d1 | 1.176 | 1.128 | 1.042 | 0.1 % | 0.3 % |
| color-edges/420/p2/d4 | 0.968 | 0.925 | 1.046 | 0.5 % | 0.3 % |
| line-art/444/p0/d1 | 0.615 | 0.535 | 1.149 | 0.3 % | 0.1 % |
| line-art/444/p0/d4 | 0.577 | 0.483 | 1.194 | 0.4 % | 0.1 % |
| line-art/444/p2/d1 | 1.350 | 1.131 | 1.194 | 0.1 % | 0.1 % |
| line-art/444/p2/d4 | 1.223 | 1.120 | 1.092 | 0.5 % | 0.1 % |
| line-art/420/p0/d1 | 0.494 | 0.499 | 0.990 | 0.3 % | 0.6 % |
| line-art/420/p0/d4 | 0.455 | 0.424 | 1.074 | 0.3 % | 0.4 % |
| line-art/420/p2/d1 | 1.043 | 0.884 | 1.179 | 0.4 % | 0.2 % |
| line-art/420/p2/d4 | 0.897 | 0.836 | 1.073 | 0.1 % | 0.3 % |
| structured-texture/444/p0/d1 | 0.917 | 1.239 | 0.740 | 0.5 % | 0.2 % |
| structured-texture/444/p0/d4 | 0.685 | 0.704 | 0.973 | 0.4 % | 0.2 % |
| structured-texture/444/p2/d1 | 2.563 | 3.083 | 0.831 | 0.2 % | 0.2 % |
| structured-texture/444/p2/d4 | 1.534 | 1.650 | 0.930 | 0.1 % | 0.1 % |
| structured-texture/420/p0/d1 | 0.635 | 0.915 | 0.694 | 0.4 % | 0.1 % |
| structured-texture/420/p0/d4 | 0.480 | 0.527 | 0.910 | 0.5 % | 0.4 % |
| structured-texture/420/p2/d1 | 1.607 | 2.007 | 0.801 | 0.3 % | 0.1 % |
| structured-texture/420/p2/d4 | 0.984 | 1.068 | 0.921 | 0.2 % | 0.1 % |
| low-noise/444/p0/d1 | 0.662 | 0.720 | 0.919 | 0.3 % | 0.2 % |
| low-noise/444/p0/d4 | 0.499 | 0.373 | 1.336 | 0.1 % | 0.1 % |
| low-noise/444/p2/d1 | 1.305 | 1.626 | 0.803 | 0.2 % | 0.2 % |
| low-noise/444/p2/d4 | 0.909 | 0.723 | 1.257 | 1.0 % | 0.2 % |
| low-noise/420/p0/d1 | 0.480 | 0.561 | 0.855 | 0.3 % | 0.1 % |
| low-noise/420/p0/d4 | 0.372 | 0.293 | 1.270 | 0.2 % | 0.2 % |
| low-noise/420/p2/d1 | 0.886 | 1.071 | 0.828 | 0.7 % | 0.3 % |
| low-noise/420/p2/d4 | 0.595 | 0.458 | 1.300 | 0.5 % | 0.2 % |
| full-noise/444/p0/d1 | 1.019 | 1.363 | 0.747 | 0.5 % | 0.1 % |
| full-noise/444/p0/d4 | 0.779 | 1.019 | 0.764 | 0.2 % | 0.1 % |
| full-noise/444/p2/d1 | 3.121 | 3.409 | 0.915 | 0.1 % | 0.4 % |
| full-noise/444/p2/d4 | 1.859 | 2.412 | 0.771 | 0.6 % | 0.7 % |
| full-noise/420/p0/d1 | 0.695 | 0.986 | 0.705 | 0.3 % | 0.4 % |
| full-noise/420/p0/d4 | 0.540 | 0.732 | 0.737 | 0.3 % | 0.2 % |
| full-noise/420/p2/d1 | 1.986 | 2.035 | 0.976 | 0.3 % | 0.2 % |
| full-noise/420/p2/d4 | 1.202 | 1.523 | 0.789 | 0.2 % | 0.1 % |
| tiny-odd-edges/444/p0/d1 | 0.035 | 0.026 | 1.356 | 1.5 % | 1.1 % |
| tiny-odd-edges/444/p0/d4 | 0.035 | 0.031 | 1.121 | 1.1 % | 1.2 % |
| tiny-odd-edges/444/p2/d1 | 0.068 | 0.053 | 1.286 | 1.2 % | 0.6 % |
| tiny-odd-edges/444/p2/d4 | 0.065 | 0.054 | 1.200 | 1.8 % | 0.3 % |
| tiny-odd-edges/420/p0/d1 | 0.033 | 0.024 | 1.381 | 0.4 % | 0.2 % |
| tiny-odd-edges/420/p0/d4 | 0.034 | 0.029 | 1.154 | 0.7 % | 0.4 % |
| tiny-odd-edges/420/p2/d1 | 0.064 | 0.048 | 1.318 | 1.3 % | 0.2 % |
| tiny-odd-edges/420/p2/d4 | 0.061 | 0.050 | 1.227 | 2.2 % | 0.3 % |

Exact input/JPEG files, SHA256 identities, per-block raw samples, p10/p50/p90, relative MAD, allocations/collections, calibration counts and native command outputs are retained alongside this report. summary.json records CPU, affinity, OS, runtime, vector widths, policy, revision and group verdicts.

Failed bounds:

- kodim02/444/p0/d4: median latency ratio 1.186865 exceeds 1.10.
- kodim02/420/p0/d4: median latency ratio 1.230540 exceeds 1.10.
- kodim03/444/p0/d4: median latency ratio 1.185766 exceeds 1.10.
- kodim03/420/p0/d4: median latency ratio 1.240458 exceeds 1.10.
- kodim04/444/p0/d4: median latency ratio 1.186011 exceeds 1.10.
- kodim04/420/p0/d4: median latency ratio 1.229318 exceeds 1.10.
- kodim06/420/p0/d4: median latency ratio 1.134751 exceeds 1.10.
- kodim07/444/p0/d4: median latency ratio 1.165918 exceeds 1.10.
- kodim07/420/p0/d4: median latency ratio 1.309513 exceeds 1.10.
- kodim07/420/p2/d1: median latency ratio 1.152166 exceeds 1.10.
- kodim09/444/p0/d4: median latency ratio 1.177176 exceeds 1.10.
- kodim09/420/p0/d4: median latency ratio 1.230953 exceeds 1.10.
- kodim10/444/p0/d4: median latency ratio 1.165892 exceeds 1.10.
- kodim10/420/p0/d4: median latency ratio 1.239734 exceeds 1.10.
- kodim11/444/p0/d4: median latency ratio 1.112993 exceeds 1.10.
- kodim11/420/p0/d4: median latency ratio 1.165029 exceeds 1.10.
- kodim12/444/p0/d4: median latency ratio 1.166422 exceeds 1.10.
- kodim12/420/p0/d4: median latency ratio 1.232665 exceeds 1.10.
- kodim14/420/p0/d4: median latency ratio 1.112834 exceeds 1.10.
- kodim15/444/p0/d4: median latency ratio 1.167336 exceeds 1.10.
- kodim15/420/p0/d4: median latency ratio 1.215852 exceeds 1.10.
- kodim16/444/p0/d4: median latency ratio 1.141271 exceeds 1.10.
- kodim16/420/p0/d4: median latency ratio 1.190215 exceeds 1.10.
- kodim17/444/p0/d4: median latency ratio 1.147803 exceeds 1.10.
- kodim17/420/p0/d4: median latency ratio 1.220909 exceeds 1.10.
- kodim18/420/p0/d4: median latency ratio 1.138796 exceeds 1.10.
- kodim19/444/p0/d4: median latency ratio 1.135592 exceeds 1.10.
- kodim19/420/p0/d4: median latency ratio 1.205430 exceeds 1.10.
- kodim20/444/p0/d4: median latency ratio 1.423404 exceeds 1.10.
- kodim20/420/p0/d4: median latency ratio 1.207296 exceeds 1.10.
- kodim21/444/p0/d4: median latency ratio 1.115464 exceeds 1.10.
- kodim21/420/p0/d4: median latency ratio 1.149992 exceeds 1.10.
- kodim22/444/p0/d4: median latency ratio 1.127210 exceeds 1.10.
- kodim22/420/p0/d4: median latency ratio 1.174537 exceeds 1.10.
- kodim23/444/p0/d1: median latency ratio 1.119182 exceeds 1.10.
- kodim23/444/p0/d4: median latency ratio 1.212397 exceeds 1.10.
- kodim23/420/p0/d1: median latency ratio 1.116855 exceeds 1.10.
- kodim23/420/p0/d4: median latency ratio 1.271945 exceeds 1.10.
- kodim24/420/p0/d4: median latency ratio 1.109918 exceeds 1.10.
- smooth-gradient/444/p0/d1: median latency ratio 1.419479 exceeds 1.10.
- smooth-gradient/444/p0/d4: median latency ratio 1.329531 exceeds 1.10.
- smooth-gradient/444/p2/d1: median latency ratio 1.306306 exceeds 1.10.
- smooth-gradient/444/p2/d4: median latency ratio 1.261977 exceeds 1.10.
- smooth-gradient/420/p0/d1: median latency ratio 1.324566 exceeds 1.10.
- smooth-gradient/420/p0/d4: median latency ratio 1.301271 exceeds 1.10.
- smooth-gradient/420/p2/d1: median latency ratio 1.344009 exceeds 1.10.
- smooth-gradient/420/p2/d4: median latency ratio 1.314863 exceeds 1.10.
- low-contrast/444/p0/d1: median latency ratio 1.283996 exceeds 1.10.
- low-contrast/444/p0/d4: median latency ratio 1.399733 exceeds 1.10.
- low-contrast/444/p2/d1: median latency ratio 1.114849 exceeds 1.10.
- low-contrast/444/p2/d4: median latency ratio 1.281522 exceeds 1.10.
- low-contrast/420/p0/d4: median latency ratio 1.276077 exceeds 1.10.
- low-contrast/420/p2/d4: median latency ratio 1.270148 exceeds 1.10.
- color-edges/444/p0/d1: median latency ratio 1.268148 exceeds 1.10.
- color-edges/444/p0/d4: median latency ratio 1.270708 exceeds 1.10.
- color-edges/444/p2/d1: median latency ratio 1.146601 exceeds 1.10.
- color-edges/444/p2/d4: median latency ratio 1.149806 exceeds 1.10.
- line-art/444/p0/d1: median latency ratio 1.148659 exceeds 1.10.
- line-art/444/p0/d4: median latency ratio 1.193999 exceeds 1.10.
- line-art/444/p2/d1: median latency ratio 1.193525 exceeds 1.10.
- line-art/420/p2/d1: median latency ratio 1.179139 exceeds 1.10.
- low-noise/444/p0/d4: median latency ratio 1.336135 exceeds 1.10.
- low-noise/444/p2/d4: median latency ratio 1.256517 exceeds 1.10.
- low-noise/420/p0/d4: median latency ratio 1.269630 exceeds 1.10.
- low-noise/420/p2/d4: median latency ratio 1.299986 exceeds 1.10.
- tiny-odd-edges/444/p0/d1: median latency ratio 1.356052 exceeds 1.10.
- tiny-odd-edges/444/p0/d4: median latency ratio 1.121294 exceeds 1.10.
- tiny-odd-edges/444/p2/d1: median latency ratio 1.286006 exceeds 1.10.
- tiny-odd-edges/444/p2/d4: median latency ratio 1.200034 exceeds 1.10.
- tiny-odd-edges/420/p0/d1: median latency ratio 1.381448 exceeds 1.10.
- tiny-odd-edges/420/p0/d4: median latency ratio 1.153668 exceeds 1.10.
- tiny-odd-edges/420/p2/d1: median latency ratio 1.318450 exceeds 1.10.
- tiny-odd-edges/420/p2/d4: median latency ratio 1.226957 exceeds 1.10.
- natural/444/p0: geometric mean latency ratio 1.060785 exceeds 1.05.
- natural/420/p0: geometric mean latency ratio 1.060078 exceeds 1.05.
- controlled/444/p0: geometric mean latency ratio 1.115609 exceeds 1.05.
- controlled/444/p2: geometric mean latency ratio 1.080177 exceeds 1.05.
- controlled/420/p2: geometric mean latency ratio 1.077625 exceeds 1.05.
