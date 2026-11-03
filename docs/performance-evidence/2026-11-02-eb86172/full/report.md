# Performance parity: FAILED

256/256 cases: RGB8 YCbCr, Kodak24 plus eight fixed controls, distances1/4, 444/420 and progression0/2, one worker on this CI host. Pinned upstream `031a0077f5799a6041004267fc12b956c1f52a20`.
Tested revision: `eb86172fb197b7614ca9e82ffdbd6422cfc4d74f`.

All cases are measured even when a latency bound fails, so the report identifies every measured regression. A passing verdict requires complete, valid coverage and all fixed bounds.

Fixed latency noninferiority limits: geometric mean managed/native median latency ratio <=1.05 in each corpus/sampling/progression group; every case <=1.10. A faster case cannot excuse a slower case. No universal performance claim is made.

Seven alternating order blocks, five untimed warmups before each block. The native-only three-iteration calibration targets50ms and selects a shared count clamped3..100. Medians use all seven blocks; no outliers are discarded. Complete in-memory encode calls are timed individually and their elapsed times summed; subprocess startup, file I/O, verification and native buffer free are excluded. Managed collections inside encode are included. Tiered compilation is disabled.

Evidence is invalid if a block interval is below1ms, any case has relative MAD >10%, inputs/settings/coverage differ, or an untimed JPEG structure comparison fails. Invalid evidence cannot establish parity.

| Corpus | Sampling | Progression | Cases | Geometric mean ratio | Worst ratio | Verdict |
| --- | --- | --- | ---: | ---: | ---: | --- |
| natural | 444 | 0 | 48 | 0.913721 | 1.099491 | PASS |
| natural | 444 | 2 | 48 | 0.719840 | 0.764924 | PASS |
| natural | 420 | 0 | 48 | 0.918892 | 1.147908 | FAIL |
| natural | 420 | 2 | 48 | 0.712583 | 0.801937 | PASS |
| controlled | 444 | 0 | 16 | 1.085156 | 1.762750 | FAIL |
| controlled | 444 | 2 | 16 | 1.040232 | 2.097201 | FAIL |
| controlled | 420 | 0 | 16 | 1.017411 | 1.752780 | FAIL |
| controlled | 420 | 2 | 16 | 1.048196 | 2.053546 | FAIL |

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 3.883 | 4.381 | 0.886 | 2.0 % | 0.8 % |
| kodim01/444/p0/d4 | 2.188 | 2.344 | 0.933 | 0.7 % | 0.6 % |
| kodim01/444/p2/d1 | 6.971 | 9.994 | 0.698 | 1.3 % | 0.6 % |
| kodim01/444/p2/d4 | 4.803 | 6.671 | 0.720 | 2.0 % | 1.4 % |
| kodim01/420/p0/d1 | 2.130 | 2.934 | 0.726 | 0.9 % | 0.8 % |
| kodim01/420/p0/d4 | 1.656 | 1.719 | 0.963 | 0.6 % | 0.4 % |
| kodim01/420/p2/d1 | 5.234 | 7.758 | 0.675 | 0.3 % | 0.8 % |
| kodim01/420/p2/d4 | 3.157 | 4.473 | 0.706 | 0.5 % | 1.1 % |
| kodim02/444/p0/d1 | 2.502 | 3.031 | 0.825 | 0.4 % | 0.7 % |
| kodim02/444/p0/d4 | 2.027 | 1.926 | 1.053 | 0.4 % | 0.8 % |
| kodim02/444/p2/d1 | 5.412 | 7.838 | 0.690 | 0.7 % | 1.0 % |
| kodim02/444/p2/d4 | 3.837 | 5.119 | 0.750 | 0.8 % | 0.1 % |
| kodim02/420/p0/d1 | 1.924 | 2.305 | 0.835 | 1.1 % | 0.5 % |
| kodim02/420/p0/d4 | 1.497 | 1.368 | 1.094 | 1.0 % | 0.7 % |
| kodim02/420/p2/d1 | 3.874 | 5.930 | 0.653 | 0.1 % | 0.6 % |
| kodim02/420/p2/d4 | 2.514 | 3.338 | 0.753 | 0.3 % | 0.3 % |
| kodim03/444/p0/d1 | 2.246 | 2.499 | 0.899 | 0.6 % | 0.7 % |
| kodim03/444/p0/d4 | 2.006 | 1.831 | 1.095 | 1.4 % | 1.1 % |
| kodim03/444/p2/d1 | 4.783 | 6.552 | 0.730 | 1.7 % | 0.5 % |
| kodim03/444/p2/d4 | 3.726 | 5.044 | 0.739 | 0.4 % | 0.8 % |
| kodim03/420/p0/d1 | 1.705 | 1.849 | 0.922 | 0.3 % | 0.3 % |
| kodim03/420/p0/d4 | 1.457 | 1.296 | 1.123 | 0.9 % | 0.5 % |
| kodim03/420/p2/d1 | 3.421 | 4.944 | 0.692 | 0.7 % | 0.8 % |
| kodim03/420/p2/d4 | 2.522 | 3.328 | 0.758 | 1.9 % | 0.9 % |
| kodim04/444/p0/d1 | 2.570 | 3.004 | 0.856 | 1.9 % | 1.7 % |
| kodim04/444/p0/d4 | 2.163 | 1.989 | 1.087 | 0.8 % | 0.2 % |
| kodim04/444/p2/d1 | 5.474 | 7.866 | 0.696 | 0.4 % | 0.8 % |
| kodim04/444/p2/d4 | 4.001 | 5.344 | 0.749 | 0.9 % | 1.0 % |
| kodim04/420/p0/d1 | 1.940 | 2.277 | 0.852 | 1.4 % | 0.2 % |
| kodim04/420/p0/d4 | 1.542 | 1.413 | 1.091 | 0.3 % | 0.3 % |
| kodim04/420/p2/d1 | 3.844 | 5.594 | 0.687 | 0.6 % | 0.5 % |
| kodim04/420/p2/d4 | 2.655 | 3.364 | 0.789 | 1.2 % | 1.4 % |
| kodim05/444/p0/d1 | 2.781 | 3.709 | 0.750 | 1.2 % | 1.1 % |
| kodim05/444/p0/d4 | 2.233 | 2.490 | 0.897 | 0.7 % | 0.2 % |
| kodim05/444/p2/d1 | 6.936 | 9.745 | 0.712 | 0.7 % | 0.4 % |
| kodim05/444/p2/d4 | 4.833 | 6.711 | 0.720 | 0.6 % | 0.5 % |
| kodim05/420/p0/d1 | 2.204 | 3.449 | 0.639 | 1.2 % | 0.2 % |
| kodim05/420/p0/d4 | 1.723 | 1.849 | 0.932 | 0.7 % | 0.4 % |
| kodim05/420/p2/d1 | 5.418 | 7.989 | 0.678 | 0.2 % | 0.6 % |
| kodim05/420/p2/d4 | 3.539 | 5.023 | 0.705 | 1.0 % | 1.0 % |
| kodim06/444/p0/d1 | 2.524 | 3.230 | 0.781 | 1.2 % | 0.9 % |
| kodim06/444/p0/d4 | 2.064 | 2.141 | 0.964 | 0.4 % | 0.6 % |
| kodim06/444/p2/d1 | 6.030 | 8.708 | 0.692 | 1.1 % | 0.3 % |
| kodim06/444/p2/d4 | 4.285 | 6.114 | 0.701 | 0.4 % | 0.4 % |
| kodim06/420/p0/d1 | 1.938 | 2.527 | 0.767 | 1.0 % | 0.2 % |
| kodim06/420/p0/d4 | 1.534 | 1.577 | 0.973 | 1.1 % | 1.3 % |
| kodim06/420/p2/d1 | 4.547 | 6.791 | 0.670 | 0.3 % | 0.3 % |
| kodim06/420/p2/d4 | 2.896 | 4.124 | 0.702 | 1.5 % | 0.7 % |
| kodim07/444/p0/d1 | 2.390 | 2.668 | 0.896 | 1.3 % | 0.6 % |
| kodim07/444/p0/d4 | 2.058 | 1.953 | 1.054 | 0.4 % | 0.9 % |
| kodim07/444/p2/d1 | 5.118 | 6.894 | 0.742 | 0.2 % | 1.0 % |
| kodim07/444/p2/d4 | 3.951 | 5.281 | 0.748 | 1.0 % | 0.5 % |
| kodim07/420/p0/d1 | 1.776 | 1.957 | 0.907 | 0.9 % | 0.5 % |
| kodim07/420/p0/d4 | 1.513 | 1.385 | 1.093 | 0.7 % | 0.7 % |
| kodim07/420/p2/d1 | 3.685 | 5.098 | 0.723 | 0.7 % | 0.5 % |
| kodim07/420/p2/d4 | 2.624 | 3.491 | 0.752 | 0.3 % | 0.4 % |
| kodim08/444/p0/d1 | 2.737 | 3.644 | 0.751 | 0.8 % | 0.4 % |
| kodim08/444/p0/d4 | 2.179 | 2.429 | 0.897 | 0.6 % | 0.5 % |
| kodim08/444/p2/d1 | 6.773 | 9.567 | 0.708 | 0.9 % | 2.0 % |
| kodim08/444/p2/d4 | 4.671 | 6.405 | 0.729 | 0.9 % | 0.6 % |
| kodim08/420/p0/d1 | 2.212 | 3.428 | 0.645 | 0.4 % | 0.6 % |
| kodim08/420/p0/d4 | 1.733 | 1.829 | 0.947 | 1.6 % | 0.9 % |
| kodim08/420/p2/d1 | 5.343 | 7.868 | 0.679 | 0.5 % | 0.6 % |
| kodim08/420/p2/d4 | 3.460 | 4.856 | 0.712 | 1.2 % | 0.6 % |
| kodim09/444/p0/d1 | 2.342 | 2.514 | 0.931 | 0.4 % | 0.9 % |
| kodim09/444/p0/d4 | 1.995 | 1.835 | 1.087 | 0.9 % | 0.2 % |
| kodim09/444/p2/d1 | 4.805 | 6.617 | 0.726 | 1.4 % | 0.8 % |
| kodim09/444/p2/d4 | 3.810 | 5.040 | 0.756 | 2.4 % | 1.3 % |
| kodim09/420/p0/d1 | 1.749 | 1.853 | 0.944 | 0.8 % | 0.6 % |
| kodim09/420/p0/d4 | 1.443 | 1.289 | 1.120 | 0.8 % | 0.8 % |
| kodim09/420/p2/d1 | 3.483 | 4.678 | 0.745 | 1.0 % | 0.6 % |
| kodim09/420/p2/d4 | 2.433 | 3.095 | 0.786 | 0.3 % | 0.7 % |
| kodim10/444/p0/d1 | 2.413 | 2.672 | 0.903 | 1.2 % | 1.4 % |
| kodim10/444/p0/d4 | 2.089 | 1.900 | 1.099 | 1.2 % | 0.5 % |
| kodim10/444/p2/d1 | 4.986 | 6.967 | 0.716 | 0.3 % | 1.0 % |
| kodim10/444/p2/d4 | 3.877 | 5.174 | 0.749 | 0.3 % | 1.3 % |
| kodim10/420/p0/d1 | 1.829 | 1.978 | 0.925 | 0.6 % | 0.3 % |
| kodim10/420/p0/d4 | 1.503 | 1.346 | 1.117 | 0.4 % | 0.4 % |
| kodim10/420/p2/d1 | 3.631 | 5.012 | 0.724 | 0.6 % | 0.4 % |
| kodim10/420/p2/d4 | 2.608 | 3.252 | 0.802 | 1.1 % | 0.7 % |
| kodim11/444/p0/d1 | 2.499 | 3.161 | 0.791 | 1.4 % | 1.2 % |
| kodim11/444/p0/d4 | 2.007 | 2.045 | 0.981 | 0.2 % | 0.6 % |
| kodim11/444/p2/d1 | 5.793 | 8.384 | 0.691 | 1.4 % | 0.6 % |
| kodim11/444/p2/d4 | 4.139 | 5.716 | 0.724 | 0.3 % | 1.4 % |
| kodim11/420/p0/d1 | 1.908 | 2.418 | 0.789 | 1.2 % | 0.2 % |
| kodim11/420/p0/d4 | 1.526 | 1.472 | 1.037 | 0.5 % | 0.1 % |
| kodim11/420/p2/d1 | 4.289 | 6.413 | 0.669 | 0.2 % | 0.5 % |
| kodim11/420/p2/d4 | 2.782 | 3.849 | 0.723 | 0.8 % | 0.2 % |
| kodim12/444/p0/d1 | 2.293 | 2.640 | 0.869 | 0.7 % | 0.3 % |
| kodim12/444/p0/d4 | 1.943 | 1.814 | 1.071 | 0.7 % | 0.6 % |
| kodim12/444/p2/d1 | 4.830 | 6.951 | 0.695 | 0.3 % | 0.4 % |
| kodim12/444/p2/d4 | 3.692 | 4.953 | 0.745 | 0.3 % | 0.5 % |
| kodim12/420/p0/d1 | 1.722 | 1.985 | 0.867 | 0.5 % | 1.2 % |
| kodim12/420/p0/d4 | 1.407 | 1.278 | 1.101 | 0.4 % | 0.5 % |
| kodim12/420/p2/d1 | 3.479 | 5.086 | 0.684 | 1.0 % | 0.8 % |
| kodim12/420/p2/d4 | 2.387 | 3.212 | 0.743 | 0.6 % | 1.0 % |
| kodim13/444/p0/d1 | 2.818 | 4.043 | 0.697 | 0.7 % | 0.2 % |
| kodim13/444/p0/d4 | 2.232 | 2.612 | 0.855 | 0.4 % | 0.5 % |
| kodim13/444/p2/d1 | 7.474 | 10.700 | 0.698 | 0.5 % | 0.4 % |
| kodim13/444/p2/d4 | 4.906 | 7.083 | 0.693 | 0.7 % | 0.4 % |
| kodim13/420/p0/d1 | 2.281 | 3.954 | 0.577 | 1.9 % | 0.4 % |
| kodim13/420/p0/d4 | 1.700 | 1.923 | 0.884 | 0.4 % | 0.9 % |
| kodim13/420/p2/d1 | 5.900 | 8.927 | 0.661 | 1.1 % | 0.6 % |
| kodim13/420/p2/d4 | 3.511 | 5.239 | 0.670 | 0.6 % | 0.8 % |
| kodim14/444/p0/d1 | 2.634 | 3.516 | 0.749 | 0.3 % | 0.9 % |
| kodim14/444/p0/d4 | 2.126 | 2.274 | 0.935 | 0.9 % | 0.4 % |
| kodim14/444/p2/d1 | 6.471 | 9.574 | 0.676 | 1.3 % | 0.5 % |
| kodim14/444/p2/d4 | 4.384 | 6.228 | 0.704 | 0.5 % | 0.6 % |
| kodim14/420/p0/d1 | 2.068 | 2.778 | 0.745 | 0.4 % | 0.7 % |
| kodim14/420/p0/d4 | 1.605 | 1.640 | 0.979 | 0.5 % | 0.8 % |
| kodim14/420/p2/d1 | 4.893 | 7.400 | 0.661 | 0.6 % | 1.1 % |
| kodim14/420/p2/d4 | 3.006 | 4.349 | 0.691 | 1.4 % | 0.9 % |
| kodim15/444/p0/d1 | 2.326 | 2.734 | 0.851 | 0.6 % | 0.6 % |
| kodim15/444/p0/d4 | 2.018 | 1.932 | 1.045 | 0.5 % | 2.2 % |
| kodim15/444/p2/d1 | 5.003 | 7.044 | 0.710 | 0.3 % | 0.4 % |
| kodim15/444/p2/d4 | 3.856 | 5.046 | 0.764 | 1.2 % | 0.1 % |
| kodim15/420/p0/d1 | 1.784 | 2.026 | 0.881 | 0.9 % | 0.3 % |
| kodim15/420/p0/d4 | 1.452 | 1.309 | 1.109 | 0.8 % | 0.3 % |
| kodim15/420/p2/d1 | 3.650 | 5.297 | 0.689 | 1.4 % | 0.5 % |
| kodim15/420/p2/d4 | 2.530 | 3.327 | 0.761 | 0.1 % | 0.4 % |
| kodim16/444/p0/d1 | 2.326 | 2.765 | 0.841 | 0.5 % | 0.2 % |
| kodim16/444/p0/d4 | 1.915 | 1.882 | 1.018 | 0.2 % | 0.6 % |
| kodim16/444/p2/d1 | 5.112 | 7.303 | 0.700 | 0.4 % | 0.5 % |
| kodim16/444/p2/d4 | 3.784 | 5.106 | 0.741 | 0.7 % | 1.4 % |
| kodim16/420/p0/d1 | 1.777 | 2.130 | 0.834 | 1.3 % | 0.4 % |
| kodim16/420/p0/d4 | 1.418 | 1.329 | 1.067 | 0.8 % | 0.5 % |
| kodim16/420/p2/d1 | 3.760 | 5.582 | 0.674 | 2.3 % | 0.6 % |
| kodim16/420/p2/d4 | 2.449 | 3.379 | 0.725 | 0.7 % | 0.4 % |
| kodim17/444/p0/d1 | 2.369 | 2.671 | 0.887 | 1.5 % | 0.2 % |
| kodim17/444/p0/d4 | 2.037 | 1.880 | 1.083 | 0.6 % | 0.8 % |
| kodim17/444/p2/d1 | 4.942 | 6.977 | 0.708 | 0.2 % | 1.2 % |
| kodim17/444/p2/d4 | 3.787 | 5.102 | 0.742 | 0.4 % | 0.5 % |
| kodim17/420/p0/d1 | 1.756 | 2.018 | 0.870 | 0.5 % | 0.4 % |
| kodim17/420/p0/d4 | 1.444 | 1.327 | 1.088 | 0.2 % | 0.6 % |
| kodim17/420/p2/d1 | 3.622 | 5.045 | 0.718 | 0.4 % | 0.5 % |
| kodim17/420/p2/d4 | 2.554 | 3.223 | 0.792 | 0.6 % | 0.6 % |
| kodim18/444/p0/d1 | 2.655 | 3.401 | 0.781 | 1.3 % | 1.0 % |
| kodim18/444/p0/d4 | 2.129 | 2.208 | 0.964 | 1.2 % | 1.3 % |
| kodim18/444/p2/d1 | 6.176 | 8.768 | 0.704 | 0.2 % | 0.5 % |
| kodim18/444/p2/d4 | 4.280 | 5.939 | 0.721 | 0.5 % | 0.4 % |
| kodim18/420/p0/d1 | 1.997 | 2.627 | 0.760 | 0.7 % | 0.3 % |
| kodim18/420/p0/d4 | 1.593 | 1.606 | 0.991 | 0.5 % | 0.8 % |
| kodim18/420/p2/d1 | 4.759 | 6.925 | 0.687 | 0.7 % | 0.5 % |
| kodim18/420/p2/d4 | 2.981 | 3.976 | 0.750 | 0.7 % | 0.2 % |
| kodim19/444/p0/d1 | 2.481 | 2.974 | 0.834 | 2.9 % | 0.6 % |
| kodim19/444/p0/d4 | 1.999 | 1.934 | 1.034 | 0.4 % | 0.3 % |
| kodim19/444/p2/d1 | 5.342 | 7.652 | 0.698 | 0.7 % | 0.3 % |
| kodim19/444/p2/d4 | 3.919 | 5.309 | 0.738 | 0.4 % | 0.6 % |
| kodim19/420/p0/d1 | 1.875 | 2.254 | 0.832 | 0.6 % | 0.2 % |
| kodim19/420/p0/d4 | 1.480 | 1.389 | 1.066 | 0.1 % | 0.6 % |
| kodim19/420/p2/d1 | 3.959 | 5.677 | 0.697 | 0.8 % | 0.4 % |
| kodim19/420/p2/d4 | 2.658 | 3.396 | 0.783 | 0.4 % | 0.6 % |
| kodim20/444/p0/d1 | 2.087 | 2.373 | 0.879 | 1.0 % | 0.6 % |
| kodim20/444/p0/d4 | 1.852 | 1.699 | 1.090 | 0.6 % | 0.5 % |
| kodim20/444/p2/d1 | 4.583 | 6.452 | 0.710 | 0.2 % | 0.6 % |
| kodim20/444/p2/d4 | 3.560 | 4.778 | 0.745 | 1.0 % | 0.6 % |
| kodim20/420/p0/d1 | 1.600 | 1.788 | 0.895 | 0.8 % | 0.5 % |
| kodim20/420/p0/d4 | 1.344 | 1.189 | 1.130 | 0.5 % | 0.6 % |
| kodim20/420/p2/d1 | 3.327 | 4.812 | 0.691 | 1.0 % | 0.3 % |
| kodim20/420/p2/d4 | 2.373 | 3.187 | 0.745 | 0.7 % | 0.3 % |
| kodim21/444/p0/d1 | 2.442 | 2.955 | 0.826 | 2.5 % | 1.3 % |
| kodim21/444/p0/d4 | 1.964 | 1.976 | 0.994 | 1.1 % | 0.5 % |
| kodim21/444/p2/d1 | 5.474 | 7.695 | 0.711 | 0.3 % | 0.3 % |
| kodim21/444/p2/d4 | 3.943 | 5.496 | 0.717 | 1.0 % | 0.4 % |
| kodim21/420/p0/d1 | 1.866 | 2.307 | 0.809 | 0.6 % | 0.5 % |
| kodim21/420/p0/d4 | 1.439 | 1.408 | 1.022 | 0.5 % | 0.3 % |
| kodim21/420/p2/d1 | 4.019 | 5.853 | 0.687 | 0.5 % | 0.3 % |
| kodim21/420/p2/d4 | 2.651 | 3.704 | 0.716 | 0.6 % | 0.3 % |
| kodim22/444/p0/d1 | 2.493 | 3.204 | 0.778 | 1.1 % | 0.6 % |
| kodim22/444/p0/d4 | 2.028 | 2.046 | 0.991 | 0.6 % | 0.0 % |
| kodim22/444/p2/d1 | 5.794 | 8.311 | 0.697 | 0.5 % | 0.2 % |
| kodim22/444/p2/d4 | 4.027 | 5.627 | 0.716 | 0.8 % | 1.6 % |
| kodim22/420/p0/d1 | 1.947 | 2.426 | 0.803 | 0.4 % | 0.6 % |
| kodim22/420/p0/d4 | 1.529 | 1.458 | 1.048 | 1.4 % | 0.6 % |
| kodim22/420/p2/d1 | 4.257 | 6.465 | 0.659 | 0.4 % | 0.2 % |
| kodim22/420/p2/d4 | 2.699 | 3.711 | 0.727 | 0.3 % | 0.4 % |
| kodim23/444/p0/d1 | 2.205 | 2.406 | 0.916 | 0.4 % | 1.1 % |
| kodim23/444/p0/d4 | 1.894 | 1.757 | 1.078 | 0.7 % | 0.6 % |
| kodim23/444/p2/d1 | 4.523 | 6.051 | 0.748 | 0.9 % | 0.4 % |
| kodim23/444/p2/d4 | 3.625 | 4.739 | 0.765 | 0.6 % | 1.1 % |
| kodim23/420/p0/d1 | 1.643 | 1.697 | 0.968 | 0.6 % | 0.2 % |
| kodim23/420/p0/d4 | 1.399 | 1.218 | 1.148 | 0.1 % | 0.3 % |
| kodim23/420/p2/d1 | 3.112 | 4.417 | 0.705 | 0.5 % | 0.3 % |
| kodim23/420/p2/d4 | 2.373 | 3.109 | 0.763 | 1.9 % | 1.1 % |
| kodim24/444/p0/d1 | 2.598 | 3.366 | 0.772 | 0.3 % | 0.6 % |
| kodim24/444/p0/d4 | 2.125 | 2.277 | 0.933 | 0.7 % | 0.8 % |
| kodim24/444/p2/d1 | 6.295 | 8.802 | 0.715 | 2.2 % | 0.6 % |
| kodim24/444/p2/d4 | 4.356 | 6.057 | 0.719 | 1.4 % | 0.5 % |
| kodim24/420/p0/d1 | 1.988 | 2.608 | 0.762 | 0.5 % | 0.6 % |
| kodim24/420/p0/d4 | 1.627 | 1.624 | 1.001 | 1.9 % | 0.6 % |
| kodim24/420/p2/d1 | 4.686 | 6.894 | 0.680 | 0.5 % | 0.2 % |
| kodim24/420/p2/d4 | 3.036 | 4.241 | 0.716 | 1.2 % | 1.2 % |
| smooth-gradient/444/p0/d1 | 0.265 | 0.198 | 1.338 | 1.0 % | 0.7 % |
| smooth-gradient/444/p0/d4 | 0.256 | 0.198 | 1.292 | 2.8 % | 1.8 % |
| smooth-gradient/444/p2/d1 | 0.489 | 0.412 | 1.187 | 1.1 % | 0.7 % |
| smooth-gradient/444/p2/d4 | 0.477 | 0.407 | 1.173 | 1.6 % | 0.4 % |
| smooth-gradient/420/p0/d1 | 0.198 | 0.155 | 1.280 | 1.8 % | 0.3 % |
| smooth-gradient/420/p0/d4 | 0.196 | 0.157 | 1.250 | 1.0 % | 0.7 % |
| smooth-gradient/420/p2/d1 | 0.315 | 0.256 | 1.229 | 0.5 % | 0.2 % |
| smooth-gradient/420/p2/d4 | 0.307 | 0.253 | 1.216 | 0.8 % | 0.6 % |
| low-contrast/444/p0/d1 | 0.353 | 0.281 | 1.255 | 0.7 % | 0.9 % |
| low-contrast/444/p0/d4 | 0.292 | 0.214 | 1.360 | 1.2 % | 0.3 % |
| low-contrast/444/p2/d1 | 0.746 | 0.797 | 0.936 | 0.9 % | 1.0 % |
| low-contrast/444/p2/d4 | 0.577 | 0.467 | 1.236 | 0.7 % | 0.3 % |
| low-contrast/420/p0/d1 | 0.262 | 0.234 | 1.121 | 0.9 % | 0.3 % |
| low-contrast/420/p0/d4 | 0.213 | 0.172 | 1.241 | 1.5 % | 0.9 % |
| low-contrast/420/p2/d1 | 0.509 | 0.540 | 0.942 | 0.8 % | 0.2 % |
| low-contrast/420/p2/d4 | 0.372 | 0.297 | 1.254 | 0.7 % | 0.5 % |
| color-edges/444/p0/d1 | 0.377 | 0.306 | 1.230 | 0.9 % | 0.2 % |
| color-edges/444/p0/d4 | 0.345 | 0.274 | 1.259 | 1.1 % | 0.6 % |
| color-edges/444/p2/d1 | 0.935 | 0.895 | 1.045 | 1.1 % | 0.7 % |
| color-edges/444/p2/d4 | 0.767 | 0.736 | 1.042 | 1.3 % | 0.1 % |
| color-edges/420/p0/d1 | 0.297 | 0.296 | 1.003 | 1.4 % | 0.6 % |
| color-edges/420/p0/d4 | 0.267 | 0.235 | 1.138 | 2.1 % | 0.2 % |
| color-edges/420/p2/d1 | 0.766 | 0.764 | 1.003 | 1.3 % | 0.4 % |
| color-edges/420/p2/d4 | 0.581 | 0.612 | 0.949 | 0.7 % | 0.5 % |
| line-art/444/p0/d1 | 0.348 | 0.273 | 1.276 | 1.3 % | 0.5 % |
| line-art/444/p0/d4 | 0.326 | 0.251 | 1.297 | 1.1 % | 0.6 % |
| line-art/444/p2/d1 | 0.797 | 0.734 | 1.086 | 0.6 % | 0.4 % |
| line-art/444/p2/d4 | 0.708 | 0.729 | 0.972 | 0.5 % | 0.6 % |
| line-art/420/p0/d1 | 0.274 | 0.252 | 1.086 | 0.5 % | 0.4 % |
| line-art/420/p0/d4 | 0.254 | 0.223 | 1.140 | 1.4 % | 0.7 % |
| line-art/420/p2/d1 | 0.628 | 0.551 | 1.139 | 0.5 % | 0.2 % |
| line-art/420/p2/d4 | 0.509 | 0.518 | 0.983 | 0.8 % | 0.8 % |
| structured-texture/444/p0/d1 | 0.549 | 0.915 | 0.600 | 0.5 % | 0.4 % |
| structured-texture/444/p0/d4 | 0.393 | 0.463 | 0.848 | 0.9 % | 0.2 % |
| structured-texture/444/p2/d1 | 1.757 | 2.453 | 0.716 | 0.4 % | 0.3 % |
| structured-texture/444/p2/d4 | 0.978 | 1.247 | 0.784 | 1.1 % | 0.3 % |
| structured-texture/420/p0/d1 | 0.368 | 0.654 | 0.563 | 1.2 % | 0.2 % |
| structured-texture/420/p0/d4 | 0.266 | 0.316 | 0.840 | 1.1 % | 0.4 % |
| structured-texture/420/p2/d1 | 1.103 | 1.561 | 0.707 | 0.8 % | 0.3 % |
| structured-texture/420/p2/d4 | 0.608 | 0.762 | 0.798 | 1.0 % | 0.2 % |
| low-noise/444/p0/d1 | 0.388 | 0.495 | 0.783 | 0.6 % | 0.1 % |
| low-noise/444/p0/d4 | 0.266 | 0.196 | 1.362 | 1.0 % | 0.6 % |
| low-noise/444/p2/d1 | 0.811 | 1.211 | 0.670 | 0.7 % | 0.4 % |
| low-noise/444/p2/d4 | 0.506 | 0.425 | 1.191 | 0.4 % | 0.8 % |
| low-noise/420/p0/d1 | 0.271 | 0.362 | 0.749 | 0.6 % | 0.2 % |
| low-noise/420/p0/d4 | 0.199 | 0.149 | 1.331 | 1.2 % | 0.7 % |
| low-noise/420/p2/d1 | 0.512 | 0.780 | 0.657 | 0.3 % | 0.1 % |
| low-noise/420/p2/d4 | 0.325 | 0.258 | 1.260 | 0.6 % | 0.6 % |
| full-noise/444/p0/d1 | 0.617 | 1.014 | 0.609 | 1.1 % | 0.2 % |
| full-noise/444/p0/d4 | 0.452 | 0.745 | 0.607 | 1.9 % | 0.6 % |
| full-noise/444/p2/d1 | 2.211 | 2.649 | 0.835 | 1.0 % | 0.1 % |
| full-noise/444/p2/d4 | 1.206 | 1.856 | 0.650 | 0.9 % | 0.2 % |
| full-noise/420/p0/d1 | 0.410 | 0.707 | 0.580 | 2.0 % | 0.6 % |
| full-noise/420/p0/d4 | 0.314 | 0.525 | 0.598 | 1.6 % | 0.4 % |
| full-noise/420/p2/d1 | 1.441 | 1.565 | 0.921 | 1.2 % | 2.4 % |
| full-noise/420/p2/d4 | 0.819 | 1.201 | 0.682 | 2.7 % | 1.7 % |
| tiny-odd-edges/444/p0/d1 | 0.025 | 0.014 | 1.763 | 3.9 % | 0.5 % |
| tiny-odd-edges/444/p0/d4 | 0.025 | 0.018 | 1.384 | 1.2 % | 0.6 % |
| tiny-odd-edges/444/p2/d1 | 0.058 | 0.028 | 2.097 | 3.8 % | 0.6 % |
| tiny-odd-edges/444/p2/d4 | 0.058 | 0.029 | 1.991 | 2.8 % | 1.8 % |
| tiny-odd-edges/420/p0/d1 | 0.024 | 0.013 | 1.753 | 0.7 % | 1.3 % |
| tiny-odd-edges/420/p0/d4 | 0.023 | 0.016 | 1.460 | 1.1 % | 0.6 % |
| tiny-odd-edges/420/p2/d1 | 0.052 | 0.026 | 2.054 | 0.6 % | 0.9 % |
| tiny-odd-edges/420/p2/d4 | 0.049 | 0.026 | 1.877 | 1.8 % | 0.6 % |

Exact input/JPEG files, SHA256 identities, per-block raw samples, p10/p50/p90, relative MAD, allocations/collections, calibration counts and native command outputs are retained alongside this report. summary.json records CPU, affinity, OS, runtime, vector widths, policy, revision and group verdicts.

Failed bounds:

- kodim03/420/p0/d4: median latency ratio 1.123491 exceeds 1.10.
- kodim09/420/p0/d4: median latency ratio 1.119546 exceeds 1.10.
- kodim10/420/p0/d4: median latency ratio 1.116623 exceeds 1.10.
- kodim12/420/p0/d4: median latency ratio 1.101003 exceeds 1.10.
- kodim15/420/p0/d4: median latency ratio 1.109432 exceeds 1.10.
- kodim20/420/p0/d4: median latency ratio 1.130098 exceeds 1.10.
- kodim23/420/p0/d4: median latency ratio 1.147908 exceeds 1.10.
- smooth-gradient/444/p0/d1: median latency ratio 1.338473 exceeds 1.10.
- smooth-gradient/444/p0/d4: median latency ratio 1.291896 exceeds 1.10.
- smooth-gradient/444/p2/d1: median latency ratio 1.187373 exceeds 1.10.
- smooth-gradient/444/p2/d4: median latency ratio 1.173018 exceeds 1.10.
- smooth-gradient/420/p0/d1: median latency ratio 1.280012 exceeds 1.10.
- smooth-gradient/420/p0/d4: median latency ratio 1.249911 exceeds 1.10.
- smooth-gradient/420/p2/d1: median latency ratio 1.228904 exceeds 1.10.
- smooth-gradient/420/p2/d4: median latency ratio 1.215771 exceeds 1.10.
- low-contrast/444/p0/d1: median latency ratio 1.254963 exceeds 1.10.
- low-contrast/444/p0/d4: median latency ratio 1.359971 exceeds 1.10.
- low-contrast/444/p2/d4: median latency ratio 1.235588 exceeds 1.10.
- low-contrast/420/p0/d1: median latency ratio 1.121454 exceeds 1.10.
- low-contrast/420/p0/d4: median latency ratio 1.240759 exceeds 1.10.
- low-contrast/420/p2/d4: median latency ratio 1.253749 exceeds 1.10.
- color-edges/444/p0/d1: median latency ratio 1.230415 exceeds 1.10.
- color-edges/444/p0/d4: median latency ratio 1.258581 exceeds 1.10.
- color-edges/420/p0/d4: median latency ratio 1.138223 exceeds 1.10.
- line-art/444/p0/d1: median latency ratio 1.275873 exceeds 1.10.
- line-art/444/p0/d4: median latency ratio 1.296515 exceeds 1.10.
- line-art/420/p0/d4: median latency ratio 1.140299 exceeds 1.10.
- line-art/420/p2/d1: median latency ratio 1.139072 exceeds 1.10.
- low-noise/444/p0/d4: median latency ratio 1.361837 exceeds 1.10.
- low-noise/444/p2/d4: median latency ratio 1.190746 exceeds 1.10.
- low-noise/420/p0/d4: median latency ratio 1.330913 exceeds 1.10.
- low-noise/420/p2/d4: median latency ratio 1.259873 exceeds 1.10.
- tiny-odd-edges/444/p0/d1: median latency ratio 1.762750 exceeds 1.10.
- tiny-odd-edges/444/p0/d4: median latency ratio 1.384217 exceeds 1.10.
- tiny-odd-edges/444/p2/d1: median latency ratio 2.097201 exceeds 1.10.
- tiny-odd-edges/444/p2/d4: median latency ratio 1.990507 exceeds 1.10.
- tiny-odd-edges/420/p0/d1: median latency ratio 1.752780 exceeds 1.10.
- tiny-odd-edges/420/p0/d4: median latency ratio 1.460493 exceeds 1.10.
- tiny-odd-edges/420/p2/d1: median latency ratio 2.053546 exceeds 1.10.
- tiny-odd-edges/420/p2/d4: median latency ratio 1.876938 exceeds 1.10.
- controlled/444/p0: geometric mean latency ratio 1.085156 exceeds 1.05.
