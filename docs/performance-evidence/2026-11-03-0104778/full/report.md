# Performance parity: INVALID

256/256 cases: RGB8 YCbCr, Kodak24 plus eight fixed controls, distances1/4, 444/420 and progression0/2, one worker on this CI host. Pinned upstream `031a0077f5799a6041004267fc12b956c1f52a20`.
Tested revision: `0104778b2691cf3e61d7dbac280457f383d9bc0c`.

All cases are measured even when a latency bound fails, so the report identifies every measured regression. A passing verdict requires complete, valid coverage and all fixed bounds.

Fixed latency noninferiority limits: geometric mean managed/native median latency ratio <=1.05 in each corpus/sampling/progression group; every case <=1.10. A faster case cannot excuse a slower case. No universal performance claim is made.

Seven alternating order blocks, five untimed warmups before each block. The native-only three-iteration calibration targets50ms and selects a shared count clamped3..100. Medians use all seven blocks; no outliers are discarded. Complete in-memory encode calls are timed individually and their elapsed times summed; subprocess startup, file I/O, verification and native buffer free are excluded. Managed collections inside encode are included. Tiered compilation is disabled.

Evidence is invalid if a block interval is below1ms, any case has relative MAD >10%, inputs/settings/coverage differ, or an untimed JPEG structure comparison fails. Invalid evidence cannot establish parity.

| Corpus | Sampling | Progression | Cases | Geometric mean ratio | Worst ratio | Verdict |
| --- | --- | --- | ---: | ---: | ---: | --- |
| natural | 444 | 0 | 48 | 1.166935 | 1.354531 | FAIL |
| natural | 444 | 2 | 48 | 0.896545 | 0.972143 | PASS |
| natural | 420 | 0 | 48 | 1.158260 | 1.414206 | INVALID |
| natural | 420 | 2 | 48 | 0.888509 | 1.001192 | PASS |
| controlled | 444 | 0 | 16 | 1.204068 | 1.544175 | FAIL |
| controlled | 444 | 2 | 16 | 1.092364 | 1.380272 | FAIL |
| controlled | 420 | 0 | 16 | 1.104980 | 1.518174 | FAIL |
| controlled | 420 | 2 | 16 | 1.098814 | 1.375819 | FAIL |

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 5.143 | 5.130 | 1.002 | 0.7 % | 0.3 % |
| kodim01/444/p0/d4 | 4.230 | 3.594 | 1.177 | 1.0 % | 0.3 % |
| kodim01/444/p2/d1 | 10.729 | 12.818 | 0.837 | 0.4 % | 0.4 % |
| kodim01/444/p2/d4 | 7.898 | 8.842 | 0.893 | 1.0 % | 0.8 % |
| kodim01/420/p0/d1 | 3.970 | 5.209 | 0.762 | 0.8 % | 1.6 % |
| kodim01/420/p0/d4 | 3.095 | 2.623 | 1.180 | 0.3 % | 0.2 % |
| kodim01/420/p2/d1 | 7.980 | 9.939 | 0.803 | 0.4 % | 0.3 % |
| kodim01/420/p2/d4 | 5.341 | 6.019 | 0.887 | 0.2 % | 0.2 % |
| kodim02/444/p0/d1 | 4.759 | 4.275 | 1.113 | 0.2 % | 0.1 % |
| kodim02/444/p0/d4 | 3.987 | 3.068 | 1.299 | 0.1 % | 0.1 % |
| kodim02/444/p2/d1 | 8.856 | 10.322 | 0.858 | 0.2 % | 0.4 % |
| kodim02/444/p2/d4 | 6.897 | 7.250 | 0.951 | 0.3 % | 0.5 % |
| kodim02/420/p0/d1 | 3.432 | 3.270 | 1.050 | 0.1 % | 0.3 % |
| kodim02/420/p0/d4 | 2.902 | 2.170 | 1.338 | 0.3 % | 0.3 % |
| kodim02/420/p2/d1 | 6.236 | 7.628 | 0.817 | 0.2 % | 0.2 % |
| kodim02/420/p2/d4 | 4.569 | 4.785 | 0.955 | 0.2 % | 0.6 % |
| kodim03/444/p0/d1 | 4.307 | 3.682 | 1.170 | 0.4 % | 0.2 % |
| kodim03/444/p0/d4 | 3.894 | 2.974 | 1.309 | 0.1 % | 0.1 % |
| kodim03/444/p2/d1 | 8.021 | 8.862 | 0.905 | 0.3 % | 0.5 % |
| kodim03/444/p2/d4 | 6.774 | 7.093 | 0.955 | 0.2 % | 0.2 % |
| kodim03/420/p0/d1 | 3.199 | 2.738 | 1.168 | 0.2 % | 0.1 % |
| kodim03/420/p0/d4 | 2.850 | 2.116 | 1.347 | 0.3 % | 0.1 % |
| kodim03/420/p2/d1 | 5.634 | 6.476 | 0.870 | 0.3 % | 0.3 % |
| kodim03/420/p2/d4 | 4.470 | 4.711 | 0.949 | 0.1 % | 0.8 % |
| kodim04/444/p0/d1 | 4.667 | 4.204 | 1.110 | 0.6 % | 0.0 % |
| kodim04/444/p0/d4 | 4.038 | 3.098 | 1.304 | 0.3 % | 0.1 % |
| kodim04/444/p2/d1 | 8.833 | 10.243 | 0.862 | 0.3 % | 0.5 % |
| kodim04/444/p2/d4 | 7.029 | 7.431 | 0.946 | 0.2 % | 0.2 % |
| kodim04/420/p0/d1 | 3.449 | 3.203 | 1.077 | 0.2 % | 0.1 % |
| kodim04/420/p0/d4 | 2.967 | 2.222 | 1.335 | 0.3 % | 0.2 % |
| kodim04/420/p2/d1 | 6.276 | 7.298 | 0.860 | 0.2 % | 0.2 % |
| kodim04/420/p2/d4 | 4.702 | 4.731 | 0.994 | 0.1 % | 0.3 % |
| kodim05/444/p0/d1 | 5.215 | 5.026 | 1.038 | 1.2 % | 0.1 % |
| kodim05/444/p0/d4 | 4.294 | 3.712 | 1.157 | 0.4 % | 0.1 % |
| kodim05/444/p2/d1 | 10.969 | 12.686 | 0.865 | 0.1 % | 0.3 % |
| kodim05/444/p2/d4 | 8.094 | 9.017 | 0.898 | 0.3 % | 0.0 % |
| kodim05/420/p0/d1 | 3.908 | 4.593 | 0.851 | 0.3 % | 0.3 % |
| kodim05/420/p0/d4 | 3.193 | 2.745 | 1.163 | 0.2 % | 0.1 % |
| kodim05/420/p2/d1 | 8.193 | 9.865 | 0.831 | 0.2 % | 0.1 % |
| kodim05/420/p2/d4 | 5.713 | 6.465 | 0.884 | 0.1 % | 0.7 % |
| kodim06/444/p0/d1 | 4.782 | 4.526 | 1.057 | 0.1 % | 0.2 % |
| kodim06/444/p0/d4 | 4.000 | 3.348 | 1.195 | 0.1 % | 0.5 % |
| kodim06/444/p2/d1 | 9.538 | 11.108 | 0.859 | 0.1 % | 0.1 % |
| kodim06/444/p2/d4 | 7.317 | 8.161 | 0.896 | 0.1 % | 0.1 % |
| kodim06/420/p0/d1 | 3.621 | 3.547 | 1.021 | 0.1 % | 0.1 % |
| kodim06/420/p0/d4 | 2.986 | 2.433 | 1.228 | 0.5 % | 0.3 % |
| kodim06/420/p2/d1 | 7.063 | 8.454 | 0.836 | 0.3 % | 0.4 % |
| kodim06/420/p2/d4 | 5.000 | 5.527 | 0.905 | 0.1 % | 0.2 % |
| kodim07/444/p0/d1 | 4.489 | 3.831 | 1.172 | 0.2 % | 0.2 % |
| kodim07/444/p0/d4 | 4.014 | 3.107 | 1.292 | 0.5 % | 0.3 % |
| kodim07/444/p2/d1 | 8.500 | 9.359 | 0.908 | 0.3 % | 0.6 % |
| kodim07/444/p2/d4 | 7.071 | 7.500 | 0.943 | 0.2 % | 0.3 % |
| kodim07/420/p0/d1 | 5.117 | 3.619 | 1.414 | 1.2 % | 2.0 % |
| kodim07/420/p0/d4 | 2.962 | 2.612 | 1.134 | 1.2 % | 8.4 % |
| kodim07/420/p2/d1 | 6.077 | 6.837 | 0.889 | 0.4 % | 0.3 % |
| kodim07/420/p2/d4 | 4.798 | 5.073 | 0.946 | 0.3 % | 0.3 % |
| kodim08/444/p0/d1 | 5.211 | 5.109 | 1.020 | 0.4 % | 0.1 % |
| kodim08/444/p0/d4 | 4.239 | 3.685 | 1.150 | 0.2 % | 0.1 % |
| kodim08/444/p2/d1 | 10.811 | 12.675 | 0.853 | 0.2 % | 0.1 % |
| kodim08/444/p2/d4 | 8.174 | 9.013 | 0.907 | 0.3 % | 0.1 % |
| kodim08/420/p0/d1 | 3.974 | 4.720 | 0.842 | 0.5 % | 0.5 % |
| kodim08/420/p0/d4 | 3.195 | 2.746 | 1.164 | 0.1 % | 0.0 % |
| kodim08/420/p2/d1 | 8.217 | 10.090 | 0.814 | 0.3 % | 0.1 % |
| kodim08/420/p2/d4 | 5.686 | 6.479 | 0.878 | 0.3 % | 0.2 % |
| kodim09/444/p0/d1 | 4.388 | 3.730 | 1.176 | 0.7 % | 0.2 % |
| kodim09/444/p0/d4 | 3.949 | 3.005 | 1.314 | 0.3 % | 0.3 % |
| kodim09/444/p2/d1 | 8.265 | 9.062 | 0.912 | 0.5 % | 0.2 % |
| kodim09/444/p2/d4 | 6.836 | 7.209 | 0.948 | 0.5 % | 0.4 % |
| kodim09/420/p0/d1 | 3.255 | 2.751 | 1.183 | 0.4 % | 0.1 % |
| kodim09/420/p0/d4 | 2.876 | 2.131 | 1.350 | 0.5 % | 0.2 % |
| kodim09/420/p2/d1 | 5.712 | 6.195 | 0.922 | 0.6 % | 0.4 % |
| kodim09/420/p2/d4 | 4.510 | 4.510 | 1.000 | 0.2 % | 0.5 % |
| kodim10/444/p0/d1 | 4.495 | 3.896 | 1.154 | 0.4 % | 0.1 % |
| kodim10/444/p0/d4 | 3.989 | 3.061 | 1.303 | 0.3 % | 0.0 % |
| kodim10/444/p2/d1 | 8.491 | 9.443 | 0.899 | 1.2 % | 0.5 % |
| kodim10/444/p2/d4 | 6.969 | 7.455 | 0.935 | 0.8 % | 0.3 % |
| kodim10/420/p0/d1 | 3.350 | 2.930 | 1.143 | 0.1 % | 0.2 % |
| kodim10/420/p0/d4 | 2.972 | 2.199 | 1.351 | 0.2 % | 0.3 % |
| kodim10/420/p2/d1 | 6.030 | 6.652 | 0.907 | 0.4 % | 0.1 % |
| kodim10/420/p2/d4 | 4.680 | 4.675 | 1.001 | 0.3 % | 0.2 % |
| kodim11/444/p0/d1 | 4.809 | 4.477 | 1.074 | 0.3 % | 0.2 % |
| kodim11/444/p0/d4 | 4.054 | 3.252 | 1.247 | 0.6 % | 0.1 % |
| kodim11/444/p2/d1 | 9.663 | 11.078 | 0.872 | 0.5 % | 0.5 % |
| kodim11/444/p2/d4 | 7.340 | 8.020 | 0.915 | 0.3 % | 0.6 % |
| kodim11/420/p0/d1 | 3.606 | 3.495 | 1.032 | 0.1 % | 0.2 % |
| kodim11/420/p0/d4 | 2.987 | 2.362 | 1.265 | 0.4 % | 0.3 % |
| kodim11/420/p2/d1 | 6.923 | 8.335 | 0.831 | 0.5 % | 0.1 % |
| kodim11/420/p2/d4 | 4.921 | 5.374 | 0.916 | 0.6 % | 0.3 % |
| kodim12/444/p0/d1 | 4.396 | 3.927 | 1.120 | 0.2 % | 0.1 % |
| kodim12/444/p0/d4 | 3.900 | 3.007 | 1.297 | 0.5 % | 0.3 % |
| kodim12/444/p2/d1 | 8.301 | 9.485 | 0.875 | 0.5 % | 0.1 % |
| kodim12/444/p2/d4 | 6.808 | 7.185 | 0.947 | 0.2 % | 0.1 % |
| kodim12/420/p0/d1 | 3.322 | 2.963 | 1.121 | 0.3 % | 0.4 % |
| kodim12/420/p0/d4 | 2.878 | 2.151 | 1.338 | 0.4 % | 0.1 % |
| kodim12/420/p2/d1 | 5.807 | 6.769 | 0.858 | 0.1 % | 0.3 % |
| kodim12/420/p2/d4 | 4.471 | 4.673 | 0.957 | 0.3 % | 0.4 % |
| kodim13/444/p0/d1 | 5.448 | 5.587 | 0.975 | 0.5 % | 0.1 % |
| kodim13/444/p0/d4 | 4.276 | 3.881 | 1.102 | 0.3 % | 0.2 % |
| kodim13/444/p2/d1 | 11.894 | 14.111 | 0.843 | 0.5 % | 0.4 % |
| kodim13/444/p2/d4 | 8.168 | 9.486 | 0.861 | 0.1 % | 0.4 % |
| kodim13/420/p0/d1 | 5.136 | 5.256 | 0.977 | 20.5 % | 0.4 % |
| kodim13/420/p0/d4 | 5.073 | 3.650 | 1.390 | 1.8 % | 1.3 % |
| kodim13/420/p2/d1 | 8.965 | 11.390 | 0.787 | 0.3 % | 0.3 % |
| kodim13/420/p2/d4 | 5.710 | 6.732 | 0.848 | 0.2 % | 0.3 % |
| kodim14/444/p0/d1 | 5.039 | 4.947 | 1.019 | 0.6 % | 0.2 % |
| kodim14/444/p0/d4 | 4.277 | 3.508 | 1.219 | 0.5 % | 0.1 % |
| kodim14/444/p2/d1 | 10.374 | 12.439 | 0.834 | 0.1 % | 0.2 % |
| kodim14/444/p2/d4 | 7.699 | 8.495 | 0.906 | 0.2 % | 0.3 % |
| kodim14/420/p0/d1 | 3.776 | 3.906 | 0.967 | 0.7 % | 0.3 % |
| kodim14/420/p0/d4 | 3.078 | 2.541 | 1.211 | 0.2 % | 0.2 % |
| kodim14/420/p2/d1 | 7.636 | 9.504 | 0.803 | 0.1 % | 0.2 % |
| kodim14/420/p2/d4 | 5.258 | 5.890 | 0.893 | 0.1 % | 0.2 % |
| kodim15/444/p0/d1 | 4.502 | 3.975 | 1.133 | 0.8 % | 0.1 % |
| kodim15/444/p0/d4 | 3.946 | 3.054 | 1.292 | 0.4 % | 0.1 % |
| kodim15/444/p2/d1 | 8.507 | 9.661 | 0.881 | 0.6 % | 0.4 % |
| kodim15/444/p2/d4 | 6.886 | 7.311 | 0.942 | 0.2 % | 0.6 % |
| kodim15/420/p0/d1 | 3.317 | 3.001 | 1.105 | 0.1 % | 0.1 % |
| kodim15/420/p0/d4 | 2.900 | 2.163 | 1.341 | 0.4 % | 0.2 % |
| kodim15/420/p2/d1 | 5.988 | 6.997 | 0.856 | 0.4 % | 0.5 % |
| kodim15/420/p2/d4 | 4.574 | 4.830 | 0.947 | 0.5 % | 0.5 % |
| kodim16/444/p0/d1 | 4.449 | 4.089 | 1.088 | 0.2 % | 0.2 % |
| kodim16/444/p0/d4 | 3.926 | 3.100 | 1.266 | 0.4 % | 0.3 % |
| kodim16/444/p2/d1 | 8.638 | 10.081 | 0.857 | 0.2 % | 0.4 % |
| kodim16/444/p2/d4 | 6.981 | 7.465 | 0.935 | 0.4 % | 0.3 % |
| kodim16/420/p0/d1 | 3.343 | 3.117 | 1.072 | 0.1 % | 0.1 % |
| kodim16/420/p0/d4 | 2.895 | 2.224 | 1.302 | 0.3 % | 0.3 % |
| kodim16/420/p2/d1 | 6.228 | 7.427 | 0.839 | 0.2 % | 0.6 % |
| kodim16/420/p2/d4 | 4.620 | 4.966 | 0.930 | 0.1 % | 0.2 % |
| kodim17/444/p0/d1 | 4.483 | 3.996 | 1.122 | 0.5 % | 0.1 % |
| kodim17/444/p0/d4 | 3.999 | 3.107 | 1.287 | 0.1 % | 0.3 % |
| kodim17/444/p2/d1 | 8.557 | 9.758 | 0.877 | 0.9 % | 0.6 % |
| kodim17/444/p2/d4 | 6.981 | 7.507 | 0.930 | 0.2 % | 0.5 % |
| kodim17/420/p0/d1 | 3.363 | 3.017 | 1.115 | 0.5 % | 0.1 % |
| kodim17/420/p0/d4 | 2.939 | 2.206 | 1.332 | 0.4 % | 0.3 % |
| kodim17/420/p2/d1 | 6.134 | 6.849 | 0.896 | 0.5 % | 0.2 % |
| kodim17/420/p2/d4 | 4.736 | 4.771 | 0.993 | 0.1 % | 0.5 % |
| kodim18/444/p0/d1 | 5.045 | 4.823 | 1.046 | 0.4 % | 0.1 % |
| kodim18/444/p0/d4 | 4.197 | 3.449 | 1.217 | 0.5 % | 0.2 % |
| kodim18/444/p2/d1 | 10.265 | 11.907 | 0.862 | 0.2 % | 1.1 % |
| kodim18/444/p2/d4 | 7.676 | 8.497 | 0.903 | 0.5 % | 0.8 % |
| kodim18/420/p0/d1 | 3.761 | 3.787 | 0.993 | 0.5 % | 0.0 % |
| kodim18/420/p0/d4 | 3.124 | 2.535 | 1.232 | 0.2 % | 0.2 % |
| kodim18/420/p2/d1 | 7.547 | 9.054 | 0.834 | 0.3 % | 0.1 % |
| kodim18/420/p2/d4 | 5.296 | 5.644 | 0.938 | 0.2 % | 0.2 % |
| kodim19/444/p0/d1 | 4.699 | 4.299 | 1.093 | 0.2 % | 0.2 % |
| kodim19/444/p0/d4 | 4.002 | 3.177 | 1.260 | 0.5 % | 0.2 % |
| kodim19/444/p2/d1 | 9.132 | 10.592 | 0.862 | 0.8 % | 0.4 % |
| kodim19/444/p2/d4 | 7.169 | 7.763 | 0.923 | 0.8 % | 0.5 % |
| kodim19/420/p0/d1 | 3.536 | 3.329 | 1.062 | 0.4 % | 1.2 % |
| kodim19/420/p0/d4 | 3.143 | 2.351 | 1.337 | 5.6 % | 2.9 % |
| kodim19/420/p2/d1 | 6.583 | 7.624 | 0.864 | 0.4 % | 0.2 % |
| kodim19/420/p2/d4 | 4.835 | 4.945 | 0.978 | 0.2 % | 0.4 % |
| kodim20/444/p0/d1 | 4.233 | 3.717 | 1.139 | 0.1 % | 0.2 % |
| kodim20/444/p0/d4 | 3.802 | 2.957 | 1.286 | 0.4 % | 0.2 % |
| kodim20/444/p2/d1 | 7.972 | 9.017 | 0.884 | 0.5 % | 0.0 % |
| kodim20/444/p2/d4 | 6.661 | 7.142 | 0.933 | 0.3 % | 0.5 % |
| kodim20/420/p0/d1 | 3.139 | 2.812 | 1.116 | 0.4 % | 0.2 % |
| kodim20/420/p0/d4 | 2.785 | 2.100 | 1.326 | 0.3 % | 0.2 % |
| kodim20/420/p2/d1 | 5.587 | 6.558 | 0.852 | 0.3 % | 0.1 % |
| kodim20/420/p2/d4 | 4.394 | 4.693 | 0.936 | 0.1 % | 0.4 % |
| kodim21/444/p0/d1 | 4.666 | 4.287 | 1.088 | 0.2 % | 0.2 % |
| kodim21/444/p0/d4 | 3.982 | 3.241 | 1.229 | 0.4 % | 0.1 % |
| kodim21/444/p2/d1 | 9.192 | 10.476 | 0.877 | 0.3 % | 0.5 % |
| kodim21/444/p2/d4 | 7.147 | 7.862 | 0.909 | 0.5 % | 0.1 % |
| kodim21/420/p0/d1 | 3.493 | 3.308 | 1.056 | 0.2 % | 0.2 % |
| kodim21/420/p0/d4 | 2.933 | 2.340 | 1.254 | 0.1 % | 0.1 % |
| kodim21/420/p2/d1 | 6.604 | 7.826 | 0.844 | 0.2 % | 0.6 % |
| kodim21/420/p2/d4 | 4.850 | 5.350 | 0.907 | 0.1 % | 0.4 % |
| kodim22/444/p0/d1 | 4.871 | 4.576 | 1.064 | 0.2 % | 0.1 % |
| kodim22/444/p0/d4 | 4.084 | 3.271 | 1.248 | 0.3 % | 0.1 % |
| kodim22/444/p2/d1 | 9.600 | 11.315 | 0.848 | 0.4 % | 0.7 % |
| kodim22/444/p2/d4 | 7.255 | 7.921 | 0.916 | 0.5 % | 0.4 % |
| kodim22/420/p0/d1 | 3.536 | 3.518 | 1.005 | 0.2 % | 0.3 % |
| kodim22/420/p0/d4 | 3.006 | 2.362 | 1.273 | 0.3 % | 0.4 % |
| kodim22/420/p2/d1 | 6.868 | 8.479 | 0.810 | 0.4 % | 0.6 % |
| kodim22/420/p2/d4 | 4.879 | 5.346 | 0.913 | 0.2 % | 0.3 % |
| kodim23/444/p0/d1 | 4.406 | 3.597 | 1.225 | 0.3 % | 0.1 % |
| kodim23/444/p0/d4 | 3.984 | 2.942 | 1.355 | 0.8 % | 0.1 % |
| kodim23/444/p2/d1 | 7.943 | 8.538 | 0.930 | 0.4 % | 0.3 % |
| kodim23/444/p2/d4 | 6.791 | 6.985 | 0.972 | 0.1 % | 0.6 % |
| kodim23/420/p0/d1 | 3.182 | 2.631 | 1.209 | 0.3 % | 0.1 % |
| kodim23/420/p0/d4 | 2.905 | 2.093 | 1.388 | 0.7 % | 0.3 % |
| kodim23/420/p2/d1 | 5.428 | 6.107 | 0.889 | 0.1 % | 0.6 % |
| kodim23/420/p2/d4 | 4.447 | 4.592 | 0.968 | 0.1 % | 0.6 % |
| kodim24/444/p0/d1 | 4.942 | 4.779 | 1.034 | 0.2 % | 0.2 % |
| kodim24/444/p0/d4 | 4.120 | 3.485 | 1.182 | 0.1 % | 0.2 % |
| kodim24/444/p2/d1 | 10.090 | 11.817 | 0.854 | 0.1 % | 0.2 % |
| kodim24/444/p2/d4 | 7.610 | 8.577 | 0.887 | 0.2 % | 0.5 % |
| kodim24/420/p0/d1 | 3.731 | 3.745 | 0.996 | 0.3 % | 0.1 % |
| kodim24/420/p0/d4 | 3.087 | 2.556 | 1.207 | 0.3 % | 0.2 % |
| kodim24/420/p2/d1 | 7.449 | 9.086 | 0.820 | 0.4 % | 0.3 % |
| kodim24/420/p2/d4 | 5.283 | 5.949 | 0.888 | 0.3 % | 0.6 % |
| smooth-gradient/444/p0/d1 | 0.569 | 0.369 | 1.544 | 0.4 % | 0.1 % |
| smooth-gradient/444/p0/d4 | 0.536 | 0.369 | 1.452 | 0.2 % | 0.2 % |
| smooth-gradient/444/p2/d1 | 0.917 | 0.691 | 1.328 | 1.1 % | 0.3 % |
| smooth-gradient/444/p2/d4 | 0.865 | 0.681 | 1.271 | 0.2 % | 0.2 % |
| smooth-gradient/420/p0/d1 | 0.423 | 0.295 | 1.432 | 0.6 % | 0.1 % |
| smooth-gradient/420/p0/d4 | 0.416 | 0.299 | 1.390 | 1.0 % | 0.4 % |
| smooth-gradient/420/p2/d1 | 0.613 | 0.445 | 1.376 | 0.8 % | 0.3 % |
| smooth-gradient/420/p2/d4 | 0.594 | 0.442 | 1.343 | 0.5 % | 0.4 % |
| low-contrast/444/p0/d1 | 0.708 | 0.511 | 1.384 | 1.6 % | 0.8 % |
| low-contrast/444/p0/d4 | 0.608 | 0.401 | 1.515 | 1.2 % | 0.1 % |
| low-contrast/444/p2/d1 | 1.310 | 1.171 | 1.119 | 0.7 % | 0.3 % |
| low-contrast/444/p2/d4 | 1.022 | 0.800 | 1.277 | 1.0 % | 0.3 % |
| low-contrast/420/p0/d1 | 0.508 | 0.436 | 1.165 | 0.9 % | 0.2 % |
| low-contrast/420/p0/d4 | 0.435 | 0.318 | 1.368 | 0.8 % | 0.3 % |
| low-contrast/420/p2/d1 | 0.906 | 0.839 | 1.080 | 0.6 % | 0.3 % |
| low-contrast/420/p2/d4 | 0.681 | 0.529 | 1.288 | 0.3 % | 0.1 % |
| color-edges/444/p0/d1 | 0.740 | 0.564 | 1.312 | 0.5 % | 0.6 % |
| color-edges/444/p0/d4 | 0.673 | 0.491 | 1.373 | 0.5 % | 0.1 % |
| color-edges/444/p2/d1 | 1.563 | 1.352 | 1.155 | 0.2 % | 0.3 % |
| color-edges/444/p2/d4 | 1.338 | 1.153 | 1.160 | 0.6 % | 0.3 % |
| color-edges/420/p0/d1 | 0.575 | 0.559 | 1.028 | 0.1 % | 0.4 % |
| color-edges/420/p0/d4 | 0.514 | 0.452 | 1.138 | 0.6 % | 0.4 % |
| color-edges/420/p2/d1 | 1.199 | 1.130 | 1.061 | 0.4 % | 0.2 % |
| color-edges/420/p2/d4 | 0.982 | 0.931 | 1.055 | 0.1 % | 0.0 % |
| line-art/444/p0/d1 | 0.678 | 0.538 | 1.260 | 0.4 % | 0.2 % |
| line-art/444/p0/d4 | 0.632 | 0.486 | 1.302 | 0.1 % | 0.5 % |
| line-art/444/p2/d1 | 1.359 | 1.142 | 1.190 | 0.2 % | 0.4 % |
| line-art/444/p2/d4 | 1.232 | 1.130 | 1.091 | 0.2 % | 0.1 % |
| line-art/420/p0/d1 | 0.541 | 0.504 | 1.075 | 0.6 % | 0.2 % |
| line-art/420/p0/d4 | 0.493 | 0.422 | 1.168 | 0.1 % | 0.7 % |
| line-art/420/p2/d1 | 1.058 | 0.885 | 1.195 | 0.3 % | 0.2 % |
| line-art/420/p2/d4 | 0.910 | 0.838 | 1.085 | 0.8 % | 0.5 % |
| structured-texture/444/p0/d1 | 0.989 | 1.235 | 0.801 | 0.3 % | 0.1 % |
| structured-texture/444/p0/d4 | 0.738 | 0.706 | 1.046 | 0.4 % | 0.2 % |
| structured-texture/444/p2/d1 | 2.563 | 3.092 | 0.829 | 0.2 % | 0.2 % |
| structured-texture/444/p2/d4 | 1.583 | 1.656 | 0.956 | 0.8 % | 0.3 % |
| structured-texture/420/p0/d1 | 0.683 | 0.918 | 0.744 | 0.2 % | 0.4 % |
| structured-texture/420/p0/d4 | 0.517 | 0.530 | 0.976 | 0.5 % | 0.2 % |
| structured-texture/420/p2/d1 | 1.640 | 2.007 | 0.817 | 0.3 % | 0.1 % |
| structured-texture/420/p2/d4 | 1.010 | 1.075 | 0.939 | 0.4 % | 0.5 % |
| low-noise/444/p0/d1 | 0.714 | 0.725 | 0.985 | 0.2 % | 0.1 % |
| low-noise/444/p0/d4 | 0.549 | 0.376 | 1.462 | 0.6 % | 0.5 % |
| low-noise/444/p2/d1 | 1.330 | 1.629 | 0.816 | 0.4 % | 0.0 % |
| low-noise/444/p2/d4 | 0.911 | 0.728 | 1.252 | 0.6 % | 0.1 % |
| low-noise/420/p0/d1 | 0.519 | 0.563 | 0.921 | 0.5 % | 0.2 % |
| low-noise/420/p0/d4 | 0.409 | 0.295 | 1.384 | 0.4 % | 0.3 % |
| low-noise/420/p2/d1 | 0.892 | 1.076 | 0.829 | 0.5 % | 0.2 % |
| low-noise/420/p2/d4 | 0.601 | 0.460 | 1.304 | 0.8 % | 0.5 % |
| full-noise/444/p0/d1 | 1.101 | 1.365 | 0.807 | 0.4 % | 0.1 % |
| full-noise/444/p0/d4 | 0.844 | 1.025 | 0.823 | 0.5 % | 0.1 % |
| full-noise/444/p2/d1 | 3.145 | 3.426 | 0.918 | 0.8 % | 0.1 % |
| full-noise/444/p2/d4 | 1.873 | 2.410 | 0.777 | 0.4 % | 0.2 % |
| full-noise/420/p0/d1 | 0.758 | 0.994 | 0.762 | 0.9 % | 0.2 % |
| full-noise/420/p0/d4 | 0.585 | 0.742 | 0.788 | 0.8 % | 0.4 % |
| full-noise/420/p2/d1 | 2.002 | 2.042 | 0.980 | 0.3 % | 0.1 % |
| full-noise/420/p2/d4 | 1.216 | 1.528 | 0.796 | 0.4 % | 0.1 % |
| tiny-odd-edges/444/p0/d1 | 0.038 | 0.026 | 1.451 | 0.9 % | 1.6 % |
| tiny-odd-edges/444/p0/d4 | 0.038 | 0.031 | 1.208 | 0.6 % | 0.8 % |
| tiny-odd-edges/444/p2/d1 | 0.074 | 0.053 | 1.380 | 4.3 % | 0.5 % |
| tiny-odd-edges/444/p2/d4 | 0.067 | 0.055 | 1.228 | 0.5 % | 0.5 % |
| tiny-odd-edges/420/p0/d1 | 0.036 | 0.024 | 1.518 | 0.9 % | 0.3 % |
| tiny-odd-edges/420/p0/d4 | 0.037 | 0.029 | 1.256 | 1.3 % | 0.6 % |
| tiny-odd-edges/420/p2/d1 | 0.067 | 0.049 | 1.367 | 1.3 % | 0.3 % |
| tiny-odd-edges/420/p2/d4 | 0.068 | 0.050 | 1.365 | 2.3 % | 0.4 % |

Exact input/JPEG files, SHA256 identities, per-block raw samples, p10/p50/p90, relative MAD, allocations/collections, calibration counts and native command outputs are retained alongside this report. summary.json records CPU, affinity, OS, runtime, vector widths, policy, revision and group verdicts.

Invalid evidence:

- kodim13/420/p0/d1: managed relative MAD exceeds the fixed 10% noise bound.

Failed bounds:

- kodim01/444/p0/d4: median latency ratio 1.176808 exceeds 1.10.
- kodim01/420/p0/d4: median latency ratio 1.180313 exceeds 1.10.
- kodim02/444/p0/d1: median latency ratio 1.113179 exceeds 1.10.
- kodim02/444/p0/d4: median latency ratio 1.299471 exceeds 1.10.
- kodim02/420/p0/d4: median latency ratio 1.337588 exceeds 1.10.
- kodim03/444/p0/d1: median latency ratio 1.169830 exceeds 1.10.
- kodim03/444/p0/d4: median latency ratio 1.309334 exceeds 1.10.
- kodim03/420/p0/d1: median latency ratio 1.168225 exceeds 1.10.
- kodim03/420/p0/d4: median latency ratio 1.346862 exceeds 1.10.
- kodim04/444/p0/d1: median latency ratio 1.110144 exceeds 1.10.
- kodim04/444/p0/d4: median latency ratio 1.303555 exceeds 1.10.
- kodim04/420/p0/d4: median latency ratio 1.334916 exceeds 1.10.
- kodim05/444/p0/d4: median latency ratio 1.156644 exceeds 1.10.
- kodim05/420/p0/d4: median latency ratio 1.163213 exceeds 1.10.
- kodim06/444/p0/d4: median latency ratio 1.195046 exceeds 1.10.
- kodim06/420/p0/d4: median latency ratio 1.227576 exceeds 1.10.
- kodim07/444/p0/d1: median latency ratio 1.171701 exceeds 1.10.
- kodim07/444/p0/d4: median latency ratio 1.291623 exceeds 1.10.
- kodim07/420/p0/d1: median latency ratio 1.414206 exceeds 1.10.
- kodim07/420/p0/d4: median latency ratio 1.133785 exceeds 1.10.
- kodim08/444/p0/d4: median latency ratio 1.150375 exceeds 1.10.
- kodim08/420/p0/d4: median latency ratio 1.163559 exceeds 1.10.
- kodim09/444/p0/d1: median latency ratio 1.176276 exceeds 1.10.
- kodim09/444/p0/d4: median latency ratio 1.314344 exceeds 1.10.
- kodim09/420/p0/d1: median latency ratio 1.183059 exceeds 1.10.
- kodim09/420/p0/d4: median latency ratio 1.349755 exceeds 1.10.
- kodim10/444/p0/d1: median latency ratio 1.153740 exceeds 1.10.
- kodim10/444/p0/d4: median latency ratio 1.302988 exceeds 1.10.
- kodim10/420/p0/d1: median latency ratio 1.143327 exceeds 1.10.
- kodim10/420/p0/d4: median latency ratio 1.351439 exceeds 1.10.
- kodim11/444/p0/d4: median latency ratio 1.246739 exceeds 1.10.
- kodim11/420/p0/d4: median latency ratio 1.264777 exceeds 1.10.
- kodim12/444/p0/d1: median latency ratio 1.119516 exceeds 1.10.
- kodim12/444/p0/d4: median latency ratio 1.296688 exceeds 1.10.
- kodim12/420/p0/d1: median latency ratio 1.121305 exceeds 1.10.
- kodim12/420/p0/d4: median latency ratio 1.338028 exceeds 1.10.
- kodim13/444/p0/d4: median latency ratio 1.101955 exceeds 1.10.
- kodim13/420/p0/d4: median latency ratio 1.389994 exceeds 1.10.
- kodim14/444/p0/d4: median latency ratio 1.219031 exceeds 1.10.
- kodim14/420/p0/d4: median latency ratio 1.211386 exceeds 1.10.
- kodim15/444/p0/d1: median latency ratio 1.132521 exceeds 1.10.
- kodim15/444/p0/d4: median latency ratio 1.292038 exceeds 1.10.
- kodim15/420/p0/d1: median latency ratio 1.105028 exceeds 1.10.
- kodim15/420/p0/d4: median latency ratio 1.340672 exceeds 1.10.
- kodim16/444/p0/d4: median latency ratio 1.266265 exceeds 1.10.
- kodim16/420/p0/d4: median latency ratio 1.301650 exceeds 1.10.
- kodim17/444/p0/d1: median latency ratio 1.121765 exceeds 1.10.
- kodim17/444/p0/d4: median latency ratio 1.287079 exceeds 1.10.
- kodim17/420/p0/d1: median latency ratio 1.114678 exceeds 1.10.
- kodim17/420/p0/d4: median latency ratio 1.332422 exceeds 1.10.
- kodim18/444/p0/d4: median latency ratio 1.216930 exceeds 1.10.
- kodim18/420/p0/d4: median latency ratio 1.232282 exceeds 1.10.
- kodim19/444/p0/d4: median latency ratio 1.259882 exceeds 1.10.
- kodim19/420/p0/d4: median latency ratio 1.337128 exceeds 1.10.
- kodim20/444/p0/d1: median latency ratio 1.138831 exceeds 1.10.
- kodim20/444/p0/d4: median latency ratio 1.285726 exceeds 1.10.
- kodim20/420/p0/d1: median latency ratio 1.116036 exceeds 1.10.
- kodim20/420/p0/d4: median latency ratio 1.325690 exceeds 1.10.
- kodim21/444/p0/d4: median latency ratio 1.228613 exceeds 1.10.
- kodim21/420/p0/d4: median latency ratio 1.253527 exceeds 1.10.
- kodim22/444/p0/d4: median latency ratio 1.248392 exceeds 1.10.
- kodim22/420/p0/d4: median latency ratio 1.273046 exceeds 1.10.
- kodim23/444/p0/d1: median latency ratio 1.224896 exceeds 1.10.
- kodim23/444/p0/d4: median latency ratio 1.354531 exceeds 1.10.
- kodim23/420/p0/d1: median latency ratio 1.209064 exceeds 1.10.
- kodim23/420/p0/d4: median latency ratio 1.387959 exceeds 1.10.
- kodim24/444/p0/d4: median latency ratio 1.182141 exceeds 1.10.
- kodim24/420/p0/d4: median latency ratio 1.207491 exceeds 1.10.
- smooth-gradient/444/p0/d1: median latency ratio 1.544175 exceeds 1.10.
- smooth-gradient/444/p0/d4: median latency ratio 1.452111 exceeds 1.10.
- smooth-gradient/444/p2/d1: median latency ratio 1.327929 exceeds 1.10.
- smooth-gradient/444/p2/d4: median latency ratio 1.270521 exceeds 1.10.
- smooth-gradient/420/p0/d1: median latency ratio 1.431700 exceeds 1.10.
- smooth-gradient/420/p0/d4: median latency ratio 1.390460 exceeds 1.10.
- smooth-gradient/420/p2/d1: median latency ratio 1.375819 exceeds 1.10.
- smooth-gradient/420/p2/d4: median latency ratio 1.343152 exceeds 1.10.
- low-contrast/444/p0/d1: median latency ratio 1.383797 exceeds 1.10.
- low-contrast/444/p0/d4: median latency ratio 1.515334 exceeds 1.10.
- low-contrast/444/p2/d1: median latency ratio 1.119146 exceeds 1.10.
- low-contrast/444/p2/d4: median latency ratio 1.277169 exceeds 1.10.
- low-contrast/420/p0/d1: median latency ratio 1.165335 exceeds 1.10.
- low-contrast/420/p0/d4: median latency ratio 1.368246 exceeds 1.10.
- low-contrast/420/p2/d4: median latency ratio 1.288311 exceeds 1.10.
- color-edges/444/p0/d1: median latency ratio 1.311670 exceeds 1.10.
- color-edges/444/p0/d4: median latency ratio 1.372654 exceeds 1.10.
- color-edges/444/p2/d1: median latency ratio 1.155434 exceeds 1.10.
- color-edges/444/p2/d4: median latency ratio 1.160305 exceeds 1.10.
- color-edges/420/p0/d4: median latency ratio 1.137817 exceeds 1.10.
- line-art/444/p0/d1: median latency ratio 1.259840 exceeds 1.10.
- line-art/444/p0/d4: median latency ratio 1.302423 exceeds 1.10.
- line-art/444/p2/d1: median latency ratio 1.190120 exceeds 1.10.
- line-art/420/p0/d4: median latency ratio 1.168028 exceeds 1.10.
- line-art/420/p2/d1: median latency ratio 1.195456 exceeds 1.10.
- low-noise/444/p0/d4: median latency ratio 1.461709 exceeds 1.10.
- low-noise/444/p2/d4: median latency ratio 1.252255 exceeds 1.10.
- low-noise/420/p0/d4: median latency ratio 1.384498 exceeds 1.10.
- low-noise/420/p2/d4: median latency ratio 1.304496 exceeds 1.10.
- tiny-odd-edges/444/p0/d1: median latency ratio 1.451461 exceeds 1.10.
- tiny-odd-edges/444/p0/d4: median latency ratio 1.207876 exceeds 1.10.
- tiny-odd-edges/444/p2/d1: median latency ratio 1.380272 exceeds 1.10.
- tiny-odd-edges/444/p2/d4: median latency ratio 1.228251 exceeds 1.10.
- tiny-odd-edges/420/p0/d1: median latency ratio 1.518174 exceeds 1.10.
- tiny-odd-edges/420/p0/d4: median latency ratio 1.255651 exceeds 1.10.
- tiny-odd-edges/420/p2/d1: median latency ratio 1.366876 exceeds 1.10.
- tiny-odd-edges/420/p2/d4: median latency ratio 1.364539 exceeds 1.10.
- natural/444/p0: geometric mean latency ratio 1.166935 exceeds 1.05.
- natural/420/p0: geometric mean latency ratio 1.158260 exceeds 1.05.
- controlled/444/p0: geometric mean latency ratio 1.204068 exceeds 1.05.
- controlled/444/p2: geometric mean latency ratio 1.092364 exceeds 1.05.
- controlled/420/p0: geometric mean latency ratio 1.104980 exceeds 1.05.
- controlled/420/p2: geometric mean latency ratio 1.098814 exceeds 1.05.
