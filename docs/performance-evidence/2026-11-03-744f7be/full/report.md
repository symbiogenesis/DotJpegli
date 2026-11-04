# Performance parity: FAILED

256/256 cases: RGB8 YCbCr, Kodak24 plus eight fixed controls, distances1/4, 444/420 and progression0/2, one worker on this CI host. Pinned upstream `031a0077f5799a6041004267fc12b956c1f52a20`.
Tested revision: `744f7bead7b9ba982b2f2127f1d2714fcc82def9`.

All cases are measured even when a latency bound fails, so the report identifies every measured regression. A passing verdict requires complete, valid coverage and all fixed bounds.

Fixed latency noninferiority limits: geometric mean managed/native median latency ratio <=1.05 in each corpus/sampling/progression group; every case <=1.10. A faster case cannot excuse a slower case. No universal performance claim is made.

Seven alternating order blocks, five untimed warmups before each block. The native-only three-iteration calibration targets50ms and selects a shared count clamped3..100. Medians use all seven blocks; no outliers are discarded. Complete in-memory encode calls are timed individually and their elapsed times summed; subprocess startup, file I/O, verification and native buffer free are excluded. Managed collections inside encode are included. Tiered compilation is disabled.

Evidence is invalid if a block interval is below1ms, any case has relative MAD >10%, inputs/settings/coverage differ, or an untimed JPEG structure comparison fails. Invalid evidence cannot establish parity.

| Corpus | Sampling | Progression | Cases | Geometric mean ratio | Worst ratio | Verdict |
| --- | --- | --- | ---: | ---: | ---: | --- |
| natural | 444 | 0 | 48 | 1.083312 | 1.445330 | FAIL |
| natural | 444 | 2 | 48 | 0.838515 | 0.977723 | PASS |
| natural | 420 | 0 | 48 | 1.037253 | 1.290605 | FAIL |
| natural | 420 | 2 | 48 | 0.813784 | 0.965081 | PASS |
| controlled | 444 | 0 | 16 | 1.053910 | 1.404755 | FAIL |
| controlled | 444 | 2 | 16 | 0.966792 | 1.359585 | FAIL |
| controlled | 420 | 0 | 16 | 0.994689 | 1.422184 | FAIL |
| controlled | 420 | 2 | 16 | 0.953942 | 1.392916 | FAIL |

| Case | Managed median (ms) | Native median (ms) | Ratio | Managed MAD | Native MAD |
| --- | ---: | ---: | ---: | ---: | ---: |
| kodim01/444/p0/d1 | 6.905 | 6.499 | 1.062 | 0.3 % | 0.7 % |
| kodim01/444/p0/d4 | 5.656 | 4.409 | 1.283 | 0.4 % | 0.8 % |
| kodim01/444/p2/d1 | 15.649 | 17.173 | 0.911 | 1.8 % | 1.0 % |
| kodim01/444/p2/d4 | 11.261 | 11.517 | 0.978 | 0.6 % | 0.7 % |
| kodim01/420/p0/d1 | 5.300 | 5.173 | 1.025 | 0.8 % | 0.5 % |
| kodim01/420/p0/d4 | 4.117 | 3.190 | 1.291 | 0.9 % | 0.6 % |
| kodim01/420/p2/d1 | 11.425 | 13.123 | 0.871 | 1.0 % | 0.7 % |
| kodim01/420/p2/d4 | 7.544 | 7.817 | 0.965 | 0.7 % | 1.0 % |
| kodim02/444/p0/d1 | 6.283 | 5.401 | 1.163 | 0.5 % | 0.5 % |
| kodim02/444/p0/d4 | 5.289 | 3.659 | 1.445 | 0.9 % | 0.7 % |
| kodim02/444/p2/d1 | 7.892 | 10.086 | 0.782 | 0.4 % | 0.5 % |
| kodim02/444/p2/d4 | 5.856 | 6.581 | 0.890 | 0.5 % | 0.8 % |
| kodim02/420/p0/d1 | 2.920 | 3.199 | 0.913 | 0.5 % | 0.6 % |
| kodim02/420/p0/d4 | 2.461 | 2.052 | 1.200 | 0.8 % | 0.4 % |
| kodim02/420/p2/d1 | 5.530 | 7.675 | 0.720 | 0.9 % | 0.2 % |
| kodim02/420/p2/d4 | 3.833 | 4.398 | 0.872 | 1.5 % | 0.3 % |
| kodim03/444/p0/d1 | 3.777 | 3.536 | 1.068 | 0.7 % | 0.3 % |
| kodim03/444/p0/d4 | 3.445 | 2.769 | 1.244 | 0.2 % | 0.6 % |
| kodim03/444/p2/d1 | 7.105 | 8.528 | 0.833 | 0.9 % | 0.7 % |
| kodim03/444/p2/d4 | 5.816 | 6.501 | 0.895 | 0.4 % | 0.4 % |
| kodim03/420/p0/d1 | 2.764 | 2.660 | 1.039 | 0.5 % | 0.3 % |
| kodim03/420/p0/d4 | 2.442 | 1.988 | 1.229 | 0.8 % | 0.4 % |
| kodim03/420/p2/d1 | 4.946 | 6.215 | 0.796 | 0.4 % | 0.6 % |
| kodim03/420/p2/d4 | 3.772 | 4.221 | 0.894 | 0.3 % | 0.2 % |
| kodim04/444/p0/d1 | 4.141 | 4.165 | 0.994 | 1.0 % | 0.6 % |
| kodim04/444/p0/d4 | 3.595 | 2.966 | 1.212 | 0.7 % | 0.6 % |
| kodim04/444/p2/d1 | 7.924 | 10.064 | 0.787 | 0.6 % | 0.3 % |
| kodim04/444/p2/d4 | 6.096 | 6.786 | 0.898 | 1.0 % | 0.5 % |
| kodim04/420/p0/d1 | 2.946 | 3.107 | 0.948 | 0.8 % | 1.2 % |
| kodim04/420/p0/d4 | 2.471 | 2.106 | 1.174 | 0.3 % | 0.3 % |
| kodim04/420/p2/d1 | 5.634 | 7.424 | 0.759 | 1.1 % | 0.4 % |
| kodim04/420/p2/d4 | 4.037 | 4.437 | 0.910 | 1.0 % | 0.4 % |
| kodim05/444/p0/d1 | 4.541 | 5.060 | 0.898 | 0.3 % | 0.2 % |
| kodim05/444/p0/d4 | 3.802 | 3.609 | 1.054 | 0.3 % | 1.0 % |
| kodim05/444/p2/d1 | 9.862 | 12.476 | 0.790 | 0.4 % | 0.9 % |
| kodim05/444/p2/d4 | 7.259 | 8.717 | 0.833 | 1.1 % | 0.7 % |
| kodim05/420/p0/d1 | 3.368 | 4.370 | 0.771 | 1.0 % | 0.3 % |
| kodim05/420/p0/d4 | 2.733 | 2.683 | 1.019 | 0.8 % | 0.4 % |
| kodim05/420/p2/d1 | 7.632 | 10.159 | 0.751 | 0.5 % | 0.7 % |
| kodim05/420/p2/d4 | 5.035 | 6.325 | 0.796 | 0.3 % | 0.2 % |
| kodim06/444/p0/d1 | 4.220 | 4.488 | 0.940 | 0.3 % | 0.5 % |
| kodim06/444/p0/d4 | 3.599 | 3.173 | 1.134 | 0.7 % | 0.2 % |
| kodim06/444/p2/d1 | 8.671 | 10.869 | 0.798 | 1.3 % | 0.9 % |
| kodim06/444/p2/d4 | 6.470 | 7.656 | 0.845 | 0.2 % | 0.4 % |
| kodim06/420/p0/d1 | 3.124 | 3.520 | 0.887 | 0.8 % | 0.5 % |
| kodim06/420/p0/d4 | 2.563 | 2.317 | 1.106 | 1.5 % | 0.7 % |
| kodim06/420/p2/d1 | 6.343 | 8.493 | 0.747 | 0.5 % | 1.1 % |
| kodim06/420/p2/d4 | 4.270 | 5.202 | 0.821 | 0.4 % | 0.5 % |
| kodim07/444/p0/d1 | 3.899 | 3.746 | 1.041 | 0.6 % | 1.9 % |
| kodim07/444/p0/d4 | 3.491 | 2.922 | 1.195 | 0.5 % | 0.4 % |
| kodim07/444/p2/d1 | 7.471 | 8.888 | 0.841 | 0.3 % | 1.0 % |
| kodim07/444/p2/d4 | 6.182 | 6.847 | 0.903 | 0.9 % | 0.8 % |
| kodim07/420/p0/d1 | 2.898 | 2.808 | 1.032 | 1.0 % | 0.9 % |
| kodim07/420/p0/d4 | 2.528 | 2.132 | 1.186 | 0.2 % | 1.0 % |
| kodim07/420/p2/d1 | 5.420 | 6.676 | 0.812 | 0.7 % | 0.4 % |
| kodim07/420/p2/d4 | 4.046 | 4.586 | 0.882 | 1.1 % | 0.3 % |
| kodim08/444/p0/d1 | 4.528 | 5.077 | 0.892 | 0.6 % | 0.8 % |
| kodim08/444/p0/d4 | 3.744 | 3.553 | 1.054 | 0.7 % | 1.3 % |
| kodim08/444/p2/d1 | 9.908 | 12.712 | 0.779 | 0.3 % | 0.4 % |
| kodim08/444/p2/d4 | 7.157 | 8.593 | 0.833 | 0.3 % | 0.5 % |
| kodim08/420/p0/d1 | 3.380 | 4.472 | 0.756 | 0.6 % | 0.2 % |
| kodim08/420/p0/d4 | 2.720 | 2.647 | 1.028 | 1.0 % | 0.4 % |
| kodim08/420/p2/d1 | 7.523 | 10.181 | 0.739 | 0.2 % | 0.6 % |
| kodim08/420/p2/d4 | 4.991 | 6.225 | 0.802 | 0.6 % | 0.4 % |
| kodim09/444/p0/d1 | 3.881 | 3.616 | 1.073 | 0.2 % | 0.5 % |
| kodim09/444/p0/d4 | 3.503 | 2.802 | 1.250 | 0.6 % | 0.6 % |
| kodim09/444/p2/d1 | 7.081 | 8.622 | 0.821 | 0.3 % | 0.5 % |
| kodim09/444/p2/d4 | 5.864 | 6.434 | 0.911 | 1.1 % | 1.0 % |
| kodim09/420/p0/d1 | 2.825 | 2.674 | 1.056 | 1.2 % | 0.8 % |
| kodim09/420/p0/d4 | 2.435 | 2.001 | 1.217 | 1.0 % | 0.8 % |
| kodim09/420/p2/d1 | 4.997 | 6.114 | 0.817 | 0.3 % | 0.3 % |
| kodim09/420/p2/d4 | 3.812 | 4.152 | 0.918 | 0.4 % | 0.6 % |
| kodim10/444/p0/d1 | 3.946 | 3.786 | 1.042 | 0.7 % | 0.4 % |
| kodim10/444/p0/d4 | 3.554 | 2.906 | 1.223 | 0.3 % | 0.5 % |
| kodim10/444/p2/d1 | 7.356 | 9.188 | 0.801 | 0.7 % | 0.4 % |
| kodim10/444/p2/d4 | 6.054 | 6.697 | 0.904 | 0.3 % | 0.4 % |
| kodim10/420/p0/d1 | 2.824 | 2.850 | 0.991 | 0.6 % | 0.4 % |
| kodim10/420/p0/d4 | 2.489 | 2.056 | 1.211 | 0.9 % | 0.3 % |
| kodim10/420/p2/d1 | 5.271 | 6.641 | 0.794 | 0.8 % | 0.6 % |
| kodim10/420/p2/d4 | 3.935 | 4.302 | 0.915 | 0.4 % | 0.4 % |
| kodim11/444/p0/d1 | 4.121 | 4.399 | 0.937 | 1.2 % | 0.3 % |
| kodim11/444/p0/d4 | 3.495 | 3.055 | 1.144 | 0.5 % | 1.0 % |
| kodim11/444/p2/d1 | 8.454 | 10.598 | 0.798 | 0.5 % | 0.8 % |
| kodim11/444/p2/d4 | 6.213 | 7.256 | 0.856 | 0.4 % | 0.6 % |
| kodim11/420/p0/d1 | 2.986 | 3.406 | 0.877 | 0.3 % | 0.7 % |
| kodim11/420/p0/d4 | 2.520 | 2.206 | 1.142 | 2.3 % | 3.0 % |
| kodim11/420/p2/d1 | 6.069 | 8.135 | 0.746 | 1.6 % | 0.6 % |
| kodim11/420/p2/d4 | 4.054 | 4.893 | 0.829 | 0.3 % | 0.8 % |
| kodim12/444/p0/d1 | 3.794 | 3.734 | 1.016 | 1.8 % | 0.9 % |
| kodim12/444/p0/d4 | 3.438 | 2.765 | 1.243 | 0.6 % | 0.7 % |
| kodim12/444/p2/d1 | 7.201 | 9.035 | 0.797 | 0.7 % | 0.7 % |
| kodim12/444/p2/d4 | 5.812 | 6.471 | 0.898 | 0.5 % | 0.3 % |
| kodim12/420/p0/d1 | 2.812 | 2.863 | 0.982 | 1.3 % | 0.2 % |
| kodim12/420/p0/d4 | 2.449 | 1.998 | 1.226 | 0.5 % | 0.5 % |
| kodim12/420/p2/d1 | 5.118 | 6.707 | 0.763 | 0.6 % | 1.0 % |
| kodim12/420/p2/d4 | 3.739 | 4.227 | 0.885 | 0.2 % | 0.3 % |
| kodim13/444/p0/d1 | 4.751 | 5.422 | 0.876 | 1.1 % | 0.8 % |
| kodim13/444/p0/d4 | 3.816 | 3.687 | 1.035 | 0.8 % | 0.9 % |
| kodim13/444/p2/d1 | 10.880 | 13.979 | 0.778 | 0.6 % | 0.3 % |
| kodim13/444/p2/d4 | 7.255 | 9.171 | 0.791 | 0.6 % | 0.5 % |
| kodim13/420/p0/d1 | 3.502 | 4.947 | 0.708 | 0.4 % | 0.2 % |
| kodim13/420/p0/d4 | 2.749 | 2.742 | 1.003 | 1.2 % | 0.2 % |
| kodim13/420/p2/d1 | 8.354 | 11.478 | 0.728 | 0.4 % | 0.6 % |
| kodim13/420/p2/d4 | 5.023 | 6.542 | 0.768 | 1.2 % | 0.6 % |
| kodim14/444/p0/d1 | 4.372 | 4.899 | 0.893 | 0.6 % | 2.2 % |
| kodim14/444/p0/d4 | 3.712 | 3.343 | 1.110 | 1.1 % | 0.5 % |
| kodim14/444/p2/d1 | 9.423 | 12.364 | 0.762 | 1.3 % | 0.9 % |
| kodim14/444/p2/d4 | 6.419 | 7.769 | 0.826 | 1.8 % | 0.6 % |
| kodim14/420/p0/d1 | 3.166 | 3.702 | 0.855 | 0.4 % | 1.1 % |
| kodim14/420/p0/d4 | 2.629 | 2.398 | 1.096 | 1.4 % | 1.0 % |
| kodim14/420/p2/d1 | 6.831 | 9.633 | 0.709 | 0.4 % | 0.2 % |
| kodim14/420/p2/d4 | 4.523 | 5.585 | 0.810 | 0.5 % | 1.3 % |
| kodim15/444/p0/d1 | 3.922 | 3.912 | 1.003 | 0.8 % | 0.6 % |
| kodim15/444/p0/d4 | 3.516 | 2.903 | 1.211 | 0.4 % | 0.4 % |
| kodim15/444/p2/d1 | 7.446 | 9.289 | 0.802 | 2.0 % | 0.4 % |
| kodim15/444/p2/d4 | 5.901 | 6.611 | 0.893 | 0.4 % | 0.5 % |
| kodim15/420/p0/d1 | 2.847 | 2.889 | 0.986 | 0.6 % | 0.7 % |
| kodim15/420/p0/d4 | 2.463 | 2.032 | 1.212 | 0.6 % | 0.9 % |
| kodim15/420/p2/d1 | 5.317 | 6.893 | 0.771 | 1.4 % | 0.2 % |
| kodim15/420/p2/d4 | 3.843 | 4.389 | 0.875 | 1.3 % | 0.5 % |
| kodim16/444/p0/d1 | 4.021 | 4.029 | 0.998 | 1.1 % | 1.1 % |
| kodim16/444/p0/d4 | 3.475 | 2.888 | 1.203 | 0.4 % | 0.1 % |
| kodim16/444/p2/d1 | 7.831 | 9.747 | 0.803 | 0.9 % | 0.8 % |
| kodim16/444/p2/d4 | 6.015 | 6.797 | 0.885 | 0.6 % | 0.8 % |
| kodim16/420/p0/d1 | 2.896 | 3.010 | 0.962 | 0.7 % | 0.8 % |
| kodim16/420/p0/d4 | 2.468 | 2.073 | 1.190 | 1.2 % | 0.5 % |
| kodim16/420/p2/d1 | 5.516 | 7.343 | 0.751 | 1.2 % | 0.1 % |
| kodim16/420/p2/d4 | 3.892 | 4.469 | 0.871 | 0.2 % | 0.5 % |
| kodim17/444/p0/d1 | 3.975 | 3.814 | 1.042 | 0.8 % | 0.6 % |
| kodim17/444/p0/d4 | 3.534 | 2.887 | 1.224 | 0.6 % | 1.0 % |
| kodim17/444/p2/d1 | 7.526 | 9.391 | 0.801 | 0.7 % | 0.6 % |
| kodim17/444/p2/d4 | 6.013 | 6.813 | 0.883 | 0.6 % | 0.2 % |
| kodim17/420/p0/d1 | 2.903 | 2.943 | 0.986 | 0.7 % | 1.1 % |
| kodim17/420/p0/d4 | 2.480 | 2.052 | 1.209 | 1.0 % | 1.3 % |
| kodim17/420/p2/d1 | 5.392 | 6.777 | 0.796 | 0.6 % | 0.8 % |
| kodim17/420/p2/d4 | 4.002 | 4.393 | 0.911 | 0.7 % | 0.2 % |
| kodim18/444/p0/d1 | 4.438 | 4.801 | 0.924 | 0.3 % | 0.8 % |
| kodim18/444/p0/d4 | 3.710 | 3.290 | 1.128 | 0.6 % | 1.2 % |
| kodim18/444/p2/d1 | 9.172 | 11.631 | 0.789 | 0.6 % | 1.0 % |
| kodim18/444/p2/d4 | 6.664 | 7.999 | 0.833 | 0.7 % | 0.8 % |
| kodim18/420/p0/d1 | 3.217 | 3.730 | 0.863 | 0.4 % | 0.3 % |
| kodim18/420/p0/d4 | 2.657 | 2.414 | 1.101 | 0.4 % | 0.7 % |
| kodim18/420/p2/d1 | 6.858 | 9.256 | 0.741 | 0.3 % | 0.6 % |
| kodim18/420/p2/d4 | 4.542 | 5.400 | 0.841 | 0.6 % | 0.9 % |
| kodim19/444/p0/d1 | 4.170 | 4.221 | 0.988 | 0.8 % | 0.5 % |
| kodim19/444/p0/d4 | 3.634 | 2.996 | 1.213 | 0.5 % | 0.4 % |
| kodim19/444/p2/d1 | 8.110 | 10.269 | 0.790 | 1.4 % | 0.5 % |
| kodim19/444/p2/d4 | 6.215 | 7.064 | 0.880 | 0.7 % | 1.2 % |
| kodim19/420/p0/d1 | 3.023 | 3.239 | 0.933 | 0.2 % | 0.4 % |
| kodim19/420/p0/d4 | 2.521 | 2.172 | 1.160 | 0.5 % | 0.7 % |
| kodim19/420/p2/d1 | 6.119 | 7.660 | 0.799 | 3.2 % | 1.3 % |
| kodim19/420/p2/d4 | 3.972 | 4.587 | 0.866 | 1.0 % | 1.0 % |
| kodim20/444/p0/d1 | 3.726 | 3.487 | 1.069 | 0.6 % | 1.0 % |
| kodim20/444/p0/d4 | 3.392 | 2.673 | 1.269 | 0.2 % | 1.6 % |
| kodim20/444/p2/d1 | 6.921 | 8.432 | 0.821 | 1.8 % | 0.2 % |
| kodim20/444/p2/d4 | 5.681 | 6.260 | 0.908 | 1.1 % | 0.9 % |
| kodim20/420/p0/d1 | 2.666 | 2.590 | 1.029 | 1.1 % | 0.9 % |
| kodim20/420/p0/d4 | 2.386 | 1.856 | 1.286 | 1.1 % | 1.0 % |
| kodim20/420/p2/d1 | 4.878 | 6.229 | 0.783 | 1.4 % | 1.0 % |
| kodim20/420/p2/d4 | 3.635 | 4.115 | 0.883 | 1.4 % | 0.9 % |
| kodim21/444/p0/d1 | 3.980 | 4.137 | 0.962 | 0.8 % | 0.5 % |
| kodim21/444/p0/d4 | 3.475 | 2.993 | 1.161 | 0.8 % | 1.3 % |
| kodim21/444/p2/d1 | 8.105 | 10.024 | 0.809 | 1.6 % | 0.4 % |
| kodim21/444/p2/d4 | 6.170 | 7.098 | 0.869 | 0.7 % | 0.7 % |
| kodim21/420/p0/d1 | 2.976 | 3.133 | 0.950 | 1.0 % | 1.2 % |
| kodim21/420/p0/d4 | 2.462 | 2.137 | 1.152 | 0.8 % | 0.4 % |
| kodim21/420/p2/d1 | 5.840 | 7.586 | 0.770 | 0.9 % | 0.5 % |
| kodim21/420/p2/d4 | 4.069 | 4.868 | 0.836 | 0.8 % | 0.7 % |
| kodim22/444/p0/d1 | 4.146 | 4.479 | 0.926 | 0.4 % | 0.7 % |
| kodim22/444/p0/d4 | 3.550 | 3.048 | 1.165 | 0.8 % | 1.7 % |
| kodim22/444/p2/d1 | 8.225 | 10.796 | 0.762 | 0.9 % | 0.6 % |
| kodim22/444/p2/d4 | 6.170 | 7.210 | 0.856 | 1.3 % | 0.3 % |
| kodim22/420/p0/d1 | 3.071 | 3.397 | 0.904 | 0.3 % | 0.1 % |
| kodim22/420/p0/d4 | 2.532 | 2.240 | 1.130 | 1.2 % | 1.0 % |
| kodim22/420/p2/d1 | 6.185 | 8.489 | 0.729 | 0.7 % | 0.1 % |
| kodim22/420/p2/d4 | 4.112 | 4.889 | 0.841 | 0.5 % | 0.9 % |
| kodim23/444/p0/d1 | 3.814 | 3.509 | 1.087 | 0.3 % | 0.5 % |
| kodim23/444/p0/d4 | 3.443 | 2.762 | 1.247 | 0.4 % | 0.7 % |
| kodim23/444/p2/d1 | 6.799 | 8.009 | 0.849 | 1.3 % | 0.4 % |
| kodim23/444/p2/d4 | 5.664 | 6.109 | 0.927 | 1.4 % | 0.7 % |
| kodim23/420/p0/d1 | 2.701 | 2.501 | 1.080 | 0.8 % | 1.0 % |
| kodim23/420/p0/d4 | 2.428 | 1.946 | 1.248 | 0.4 % | 0.3 % |
| kodim23/420/p2/d1 | 4.702 | 5.743 | 0.819 | 0.8 % | 1.4 % |
| kodim23/420/p2/d4 | 3.678 | 3.991 | 0.921 | 1.4 % | 0.8 % |
| kodim24/444/p0/d1 | 4.310 | 4.649 | 0.927 | 0.3 % | 0.6 % |
| kodim24/444/p0/d4 | 3.567 | 3.286 | 1.085 | 0.8 % | 0.5 % |
| kodim24/444/p2/d1 | 9.047 | 11.521 | 0.785 | 0.6 % | 0.7 % |
| kodim24/444/p2/d4 | 6.643 | 7.931 | 0.838 | 0.2 % | 1.0 % |
| kodim24/420/p0/d1 | 3.122 | 3.648 | 0.856 | 1.1 % | 0.9 % |
| kodim24/420/p0/d4 | 2.573 | 2.373 | 1.084 | 2.0 % | 0.4 % |
| kodim24/420/p2/d1 | 6.730 | 9.072 | 0.742 | 1.1 % | 0.4 % |
| kodim24/420/p2/d4 | 4.515 | 5.524 | 0.817 | 0.1 % | 0.5 % |
| smooth-gradient/444/p0/d1 | 0.450 | 0.350 | 1.287 | 1.0 % | 0.6 % |
| smooth-gradient/444/p0/d4 | 0.440 | 0.332 | 1.326 | 0.8 % | 2.4 % |
| smooth-gradient/444/p2/d1 | 0.717 | 0.576 | 1.244 | 1.0 % | 0.6 % |
| smooth-gradient/444/p2/d4 | 0.701 | 0.580 | 1.209 | 2.3 % | 0.2 % |
| smooth-gradient/420/p0/d1 | 0.318 | 0.261 | 1.219 | 1.5 % | 0.4 % |
| smooth-gradient/420/p0/d4 | 0.323 | 0.259 | 1.247 | 0.4 % | 1.3 % |
| smooth-gradient/420/p2/d1 | 0.471 | 0.369 | 1.277 | 0.5 % | 0.6 % |
| smooth-gradient/420/p2/d4 | 0.453 | 0.375 | 1.208 | 0.6 % | 0.5 % |
| low-contrast/444/p0/d1 | 0.571 | 0.523 | 1.092 | 0.7 % | 1.5 % |
| low-contrast/444/p0/d4 | 0.485 | 0.362 | 1.340 | 0.8 % | 1.4 % |
| low-contrast/444/p2/d1 | 1.090 | 1.234 | 0.883 | 1.2 % | 1.7 % |
| low-contrast/444/p2/d4 | 0.799 | 0.748 | 1.067 | 1.5 % | 0.6 % |
| low-contrast/420/p0/d1 | 0.410 | 0.403 | 1.017 | 0.4 % | 0.5 % |
| low-contrast/420/p0/d4 | 0.345 | 0.270 | 1.277 | 0.7 % | 2.2 % |
| low-contrast/420/p2/d1 | 0.759 | 0.916 | 0.828 | 0.9 % | 0.4 % |
| low-contrast/420/p2/d4 | 0.537 | 0.503 | 1.068 | 1.8 % | 1.0 % |
| color-edges/444/p0/d1 | 0.611 | 0.550 | 1.111 | 0.9 % | 1.1 % |
| color-edges/444/p0/d4 | 0.552 | 0.467 | 1.182 | 1.6 % | 1.2 % |
| color-edges/444/p2/d1 | 1.394 | 1.407 | 0.991 | 0.7 % | 0.9 % |
| color-edges/444/p2/d4 | 1.146 | 1.155 | 0.992 | 0.9 % | 1.5 % |
| color-edges/420/p0/d1 | 0.465 | 0.512 | 0.909 | 0.9 % | 1.2 % |
| color-edges/420/p0/d4 | 0.417 | 0.412 | 1.012 | 1.1 % | 1.7 % |
| color-edges/420/p2/d1 | 1.129 | 1.205 | 0.937 | 0.8 % | 0.5 % |
| color-edges/420/p2/d4 | 0.879 | 0.994 | 0.884 | 0.2 % | 0.8 % |
| line-art/444/p0/d1 | 0.564 | 0.476 | 1.185 | 1.2 % | 0.5 % |
| line-art/444/p0/d4 | 0.531 | 0.449 | 1.181 | 1.5 % | 0.6 % |
| line-art/444/p2/d1 | 1.234 | 1.123 | 1.099 | 0.7 % | 0.9 % |
| line-art/444/p2/d4 | 1.094 | 1.138 | 0.961 | 2.1 % | 1.3 % |
| line-art/420/p0/d1 | 0.449 | 0.433 | 1.037 | 0.7 % | 0.8 % |
| line-art/420/p0/d4 | 0.402 | 0.386 | 1.043 | 1.0 % | 0.8 % |
| line-art/420/p2/d1 | 0.960 | 0.927 | 1.036 | 0.7 % | 0.8 % |
| line-art/420/p2/d4 | 0.802 | 0.890 | 0.901 | 2.3 % | 1.2 % |
| structured-texture/444/p0/d1 | 0.849 | 1.252 | 0.678 | 0.6 % | 0.4 % |
| structured-texture/444/p0/d4 | 0.613 | 0.709 | 0.865 | 0.7 % | 0.6 % |
| structured-texture/444/p2/d1 | 2.474 | 3.351 | 0.738 | 0.4 % | 1.1 % |
| structured-texture/444/p2/d4 | 1.397 | 1.824 | 0.766 | 0.7 % | 0.5 % |
| structured-texture/420/p0/d1 | 0.563 | 0.859 | 0.655 | 0.9 % | 1.3 % |
| structured-texture/420/p0/d4 | 0.423 | 0.495 | 0.855 | 0.9 % | 0.8 % |
| structured-texture/420/p2/d1 | 1.574 | 2.151 | 0.732 | 0.7 % | 0.9 % |
| structured-texture/420/p2/d4 | 0.907 | 1.178 | 0.769 | 0.6 % | 1.4 % |
| low-noise/444/p0/d1 | 0.603 | 0.717 | 0.840 | 0.8 % | 1.0 % |
| low-noise/444/p0/d4 | 0.455 | 0.337 | 1.349 | 1.0 % | 0.9 % |
| low-noise/444/p2/d1 | 1.133 | 1.751 | 0.647 | 1.1 % | 0.6 % |
| low-noise/444/p2/d4 | 0.751 | 0.656 | 1.144 | 0.6 % | 0.5 % |
| low-noise/420/p0/d1 | 0.426 | 0.532 | 0.801 | 0.5 % | 0.2 % |
| low-noise/420/p0/d4 | 0.335 | 0.250 | 1.341 | 0.2 % | 0.6 % |
| low-noise/420/p2/d1 | 0.760 | 1.215 | 0.625 | 0.9 % | 0.3 % |
| low-noise/420/p2/d4 | 0.488 | 0.413 | 1.181 | 0.6 % | 0.5 % |
| full-noise/444/p0/d1 | 0.956 | 1.367 | 0.699 | 1.3 % | 0.4 % |
| full-noise/444/p0/d4 | 0.712 | 1.043 | 0.682 | 0.2 % | 0.5 % |
| full-noise/444/p2/d1 | 3.057 | 3.569 | 0.857 | 0.7 % | 0.3 % |
| full-noise/444/p2/d4 | 1.732 | 2.658 | 0.652 | 1.4 % | 1.5 % |
| full-noise/420/p0/d1 | 0.629 | 0.931 | 0.676 | 0.4 % | 0.9 % |
| full-noise/420/p0/d4 | 0.485 | 0.706 | 0.687 | 0.6 % | 0.6 % |
| full-noise/420/p2/d1 | 1.972 | 2.162 | 0.912 | 0.5 % | 0.1 % |
| full-noise/420/p2/d4 | 1.130 | 1.693 | 0.667 | 0.6 % | 0.3 % |
| tiny-odd-edges/444/p0/d1 | 0.033 | 0.024 | 1.405 | 1.8 % | 1.6 % |
| tiny-odd-edges/444/p0/d4 | 0.032 | 0.028 | 1.134 | 1.5 % | 2.4 % |
| tiny-odd-edges/444/p2/d1 | 0.063 | 0.047 | 1.360 | 0.3 % | 0.2 % |
| tiny-odd-edges/444/p2/d4 | 0.060 | 0.048 | 1.245 | 0.9 % | 1.3 % |
| tiny-odd-edges/420/p0/d1 | 0.031 | 0.022 | 1.422 | 0.8 % | 2.6 % |
| tiny-odd-edges/420/p0/d4 | 0.031 | 0.026 | 1.174 | 0.8 % | 0.6 % |
| tiny-odd-edges/420/p2/d1 | 0.060 | 0.043 | 1.393 | 0.7 % | 0.9 % |
| tiny-odd-edges/420/p2/d4 | 0.056 | 0.044 | 1.264 | 1.5 % | 0.7 % |

Exact input/JPEG files, SHA256 identities, per-block raw samples, p10/p50/p90, relative MAD, allocations/collections, calibration counts and native command outputs are retained alongside this report. summary.json records CPU, affinity, OS, runtime, vector widths, policy, revision and group verdicts.

Failed bounds:

- kodim01/444/p0/d4: median latency ratio 1.282993 exceeds 1.10.
- kodim01/420/p0/d4: median latency ratio 1.290605 exceeds 1.10.
- kodim02/444/p0/d1: median latency ratio 1.163387 exceeds 1.10.
- kodim02/444/p0/d4: median latency ratio 1.445330 exceeds 1.10.
- kodim02/420/p0/d4: median latency ratio 1.199677 exceeds 1.10.
- kodim03/444/p0/d4: median latency ratio 1.244312 exceeds 1.10.
- kodim03/420/p0/d4: median latency ratio 1.228561 exceeds 1.10.
- kodim04/444/p0/d4: median latency ratio 1.211886 exceeds 1.10.
- kodim04/420/p0/d4: median latency ratio 1.173558 exceeds 1.10.
- kodim06/444/p0/d4: median latency ratio 1.134115 exceeds 1.10.
- kodim06/420/p0/d4: median latency ratio 1.106376 exceeds 1.10.
- kodim07/444/p0/d4: median latency ratio 1.194731 exceeds 1.10.
- kodim07/420/p0/d4: median latency ratio 1.185704 exceeds 1.10.
- kodim09/444/p0/d4: median latency ratio 1.250312 exceeds 1.10.
- kodim09/420/p0/d4: median latency ratio 1.216938 exceeds 1.10.
- kodim10/444/p0/d4: median latency ratio 1.222668 exceeds 1.10.
- kodim10/420/p0/d4: median latency ratio 1.210588 exceeds 1.10.
- kodim11/444/p0/d4: median latency ratio 1.144213 exceeds 1.10.
- kodim11/420/p0/d4: median latency ratio 1.141946 exceeds 1.10.
- kodim12/444/p0/d4: median latency ratio 1.243310 exceeds 1.10.
- kodim12/420/p0/d4: median latency ratio 1.225682 exceeds 1.10.
- kodim14/444/p0/d4: median latency ratio 1.110402 exceeds 1.10.
- kodim15/444/p0/d4: median latency ratio 1.210995 exceeds 1.10.
- kodim15/420/p0/d4: median latency ratio 1.211631 exceeds 1.10.
- kodim16/444/p0/d4: median latency ratio 1.203094 exceeds 1.10.
- kodim16/420/p0/d4: median latency ratio 1.190443 exceeds 1.10.
- kodim17/444/p0/d4: median latency ratio 1.224060 exceeds 1.10.
- kodim17/420/p0/d4: median latency ratio 1.208903 exceeds 1.10.
- kodim18/444/p0/d4: median latency ratio 1.127569 exceeds 1.10.
- kodim18/420/p0/d4: median latency ratio 1.100685 exceeds 1.10.
- kodim19/444/p0/d4: median latency ratio 1.213057 exceeds 1.10.
- kodim19/420/p0/d4: median latency ratio 1.160233 exceeds 1.10.
- kodim20/444/p0/d4: median latency ratio 1.269132 exceeds 1.10.
- kodim20/420/p0/d4: median latency ratio 1.285762 exceeds 1.10.
- kodim21/444/p0/d4: median latency ratio 1.160872 exceeds 1.10.
- kodim21/420/p0/d4: median latency ratio 1.152058 exceeds 1.10.
- kodim22/444/p0/d4: median latency ratio 1.164807 exceeds 1.10.
- kodim22/420/p0/d4: median latency ratio 1.130414 exceeds 1.10.
- kodim23/444/p0/d4: median latency ratio 1.246623 exceeds 1.10.
- kodim23/420/p0/d4: median latency ratio 1.247936 exceeds 1.10.
- smooth-gradient/444/p0/d1: median latency ratio 1.286710 exceeds 1.10.
- smooth-gradient/444/p0/d4: median latency ratio 1.326247 exceeds 1.10.
- smooth-gradient/444/p2/d1: median latency ratio 1.243612 exceeds 1.10.
- smooth-gradient/444/p2/d4: median latency ratio 1.209384 exceeds 1.10.
- smooth-gradient/420/p0/d1: median latency ratio 1.218720 exceeds 1.10.
- smooth-gradient/420/p0/d4: median latency ratio 1.247111 exceeds 1.10.
- smooth-gradient/420/p2/d1: median latency ratio 1.277231 exceeds 1.10.
- smooth-gradient/420/p2/d4: median latency ratio 1.208345 exceeds 1.10.
- low-contrast/444/p0/d4: median latency ratio 1.339630 exceeds 1.10.
- low-contrast/420/p0/d4: median latency ratio 1.276806 exceeds 1.10.
- color-edges/444/p0/d1: median latency ratio 1.111039 exceeds 1.10.
- color-edges/444/p0/d4: median latency ratio 1.182406 exceeds 1.10.
- line-art/444/p0/d1: median latency ratio 1.184533 exceeds 1.10.
- line-art/444/p0/d4: median latency ratio 1.180727 exceeds 1.10.
- low-noise/444/p0/d4: median latency ratio 1.348588 exceeds 1.10.
- low-noise/444/p2/d4: median latency ratio 1.144235 exceeds 1.10.
- low-noise/420/p0/d4: median latency ratio 1.340901 exceeds 1.10.
- low-noise/420/p2/d4: median latency ratio 1.181458 exceeds 1.10.
- tiny-odd-edges/444/p0/d1: median latency ratio 1.404755 exceeds 1.10.
- tiny-odd-edges/444/p0/d4: median latency ratio 1.134405 exceeds 1.10.
- tiny-odd-edges/444/p2/d1: median latency ratio 1.359585 exceeds 1.10.
- tiny-odd-edges/444/p2/d4: median latency ratio 1.244884 exceeds 1.10.
- tiny-odd-edges/420/p0/d1: median latency ratio 1.422184 exceeds 1.10.
- tiny-odd-edges/420/p0/d4: median latency ratio 1.174436 exceeds 1.10.
- tiny-odd-edges/420/p2/d1: median latency ratio 1.392916 exceeds 1.10.
- tiny-odd-edges/420/p2/d4: median latency ratio 1.263883 exceeds 1.10.
- natural/444/p0: geometric mean latency ratio 1.083312 exceeds 1.05.
- controlled/444/p0: geometric mean latency ratio 1.053910 exceeds 1.05.
