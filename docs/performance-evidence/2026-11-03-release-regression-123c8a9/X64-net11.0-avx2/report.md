# Interleaved revision comparison

Persistent baseline and current workers run serially as balanced ABBA/BAAB quartets with identical fixed iterations, warmups, runtime, CPU affinity, encoding policy, inputs and JPEG hashes. Startup and IPC are outside encode timing. Ratios below1 favor current. Allocations are mean bytes per encode; retained rental capacity and peak working memory are separate measurements.

MAD, repeated-leg drift, p90/p10 minority-state drift and order bias guard the timing evidence. Invalid rows retain raw measurements and cannot support a speed claim. Valid rows remain inconclusive unless every quartet differs by more than 0.5% in the same direction. This comparison has no native parity verdict.

Release comparison: 256/256 cases against published 1.0.0 source `a40868bfdace7d737413085522a88d351aa16782`, rebuilt with the candidate SDK. 13 valid cases consistently favor 1.0.0 and fail the regression gate. Invalid or missing evidence fails validation separately. Inconclusive differences are retained and do not prove equal performance. The gate checks latency; allocation counts are reported separately.

| Case | Current/baseline | Order bias | Maximum repeat drift | Baseline p90/p10 | Current p90/p10 | Baseline B/encode | Current B/encode | Valid | Preference |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| kodim01/444/p0/d1 | 0.997580 | 1.102168 | 1.209943 | 1.055424 | 1.169191 | 262893.433333 | 195613.700000 | False | inconclusive |
| kodim01/444/p0/d4 | 0.989072 | 1.020704 | 1.385791 | 1.164376 | 1.050190 | 177256.600000 | 110052.200000 | False | inconclusive |
| kodim01/444/p2/d1 | 0.915833 | 1.008767 | 1.051262 | 1.019580 | 1.048619 | 385613.333333 | 254951.666667 | True | current |
| kodim01/444/p2/d4 | 0.824729 | 1.000979 | 1.037988 | 1.019717 | 1.027769 | 296577.800000 | 165892.900000 | True | current |
| kodim01/420/p0/d1 | 1.035660 | 1.011364 | 1.119402 | 1.034634 | 1.039684 | 271822.666667 | 177963.433333 | False | inconclusive |
| kodim01/420/p0/d4 | 1.019696 | 1.015156 | 1.040774 | 1.015393 | 1.035116 | 192492.900000 | 98704.600000 | True | baseline |
| kodim01/420/p2/d1 | 1.001217 | 1.019466 | 1.126882 | 1.066258 | 1.024412 | 404423.766667 | 228067.100000 | False | inconclusive |
| kodim01/420/p2/d4 | 0.961561 | 1.007366 | 1.062114 | 1.036606 | 1.020963 | 327396.300000 | 151020.900000 | True | current |
| kodim02/444/p0/d1 | 1.071390 | 1.118770 | 1.607207 | 1.071073 | 1.315152 | 210447.900000 | 143167.466667 | False | inconclusive |
| kodim02/444/p0/d4 | 0.979910 | 1.004759 | 1.672642 | 1.018456 | 1.016270 | 148220.900000 | 80992.600000 | False | inconclusive |
| kodim02/444/p2/d1 | 0.939277 | 1.010368 | 1.074941 | 1.026779 | 1.070945 | 353104.200000 | 222532.366667 | True | current |
| kodim02/444/p2/d4 | 0.795071 | 1.000646 | 1.020424 | 1.249945 | 1.017583 | 269714.266667 | 138988.300000 | False | inconclusive |
| kodim02/420/p0/d1 | 1.054218 | 1.000291 | 1.486499 | 1.038199 | 1.020029 | 216113.200000 | 122304.600000 | False | inconclusive |
| kodim02/420/p0/d4 | 1.012469 | 1.001001 | 1.008815 | 1.008589 | 1.008769 | 162224.000000 | 68428.300000 | True | baseline |
| kodim02/420/p2/d1 | 1.024497 | 1.000693 | 1.175425 | 1.041412 | 1.086556 | 362049.800000 | 185716.900000 | False | inconclusive |
| kodim02/420/p2/d4 | 0.946167 | 1.001401 | 1.019606 | 1.012924 | 1.019580 | 292961.200000 | 116536.600000 | True | current |
| kodim03/444/p0/d1 | 0.985559 | 1.007339 | 1.139628 | 1.028079 | 1.017686 | 185214.300000 | 117984.600000 | False | inconclusive |
| kodim03/444/p0/d4 | 0.957112 | 1.005526 | 1.016981 | 1.021401 | 1.016677 | 145676.300000 | 78452.300000 | True | current |
| kodim03/444/p2/d1 | 0.927472 | 1.002074 | 1.065927 | 1.021078 | 1.014445 | 294049.200000 | 163308.900000 | True | current |
| kodim03/444/p2/d4 | 0.810455 | 1.012715 | 1.294952 | 1.025196 | 1.019457 | 249073.200000 | 118292.300000 | False | inconclusive |
| kodim03/420/p0/d1 | 1.007858 | 1.014338 | 1.064292 | 1.024988 | 1.016070 | 202121.100000 | 108300.300000 | True | inconclusive |
| kodim03/420/p0/d4 | 0.977759 | 1.006752 | 1.079376 | 1.021435 | 1.004630 | 166372.900000 | 72580.300000 | True | current |
| kodim03/420/p2/d1 | 0.985931 | 1.001432 | 1.298144 | 1.013148 | 1.031017 | 331644.900000 | 155244.900000 | False | inconclusive |
| kodim03/420/p2/d4 | 0.940581 | 1.006712 | 1.014697 | 1.014787 | 1.019597 | 290981.500000 | 114536.600000 | True | current |
| kodim04/444/p0/d1 | 1.009146 | 1.000169 | 1.057740 | 1.038523 | 1.037565 | 189913.900000 | 143807.566667 | True | inconclusive |
| kodim04/444/p0/d4 | 0.959539 | 1.011682 | 1.061557 | 1.025131 | 1.010465 | 130988.900000 | 84880.600000 | True | current |
| kodim04/444/p2/d1 | 0.909714 | 1.001563 | 1.185873 | 1.059186 | 1.022549 | 312193.633333 | 204266.300000 | False | inconclusive |
| kodim04/444/p2/d4 | 0.798461 | 1.023926 | 1.060062 | 1.033286 | 1.025841 | 244420.900000 | 136492.300000 | True | current |
| kodim04/420/p0/d1 | 1.034219 | 1.005683 | 1.065130 | 1.018845 | 1.019870 | 193065.200000 | 129220.300000 | True | inconclusive |
| kodim04/420/p0/d4 | 1.001129 | 1.007884 | 1.708564 | 1.050287 | 1.175865 | 141651.700000 | 77772.300000 | False | inconclusive |
| kodim04/420/p2/d1 | 0.991947 | 1.007285 | 1.445540 | 1.054673 | 1.022933 | 323057.200000 | 180472.600000 | False | inconclusive |
| kodim04/420/p2/d4 | 0.938543 | 1.000944 | 1.022440 | 1.022799 | 1.031320 | 268197.500000 | 125572.300000 | True | current |
| kodim05/444/p0/d1 | 1.028857 | 1.007748 | 1.037158 | 1.033810 | 1.437409 | 269353.300000 | 202092.833333 | False | inconclusive |
| kodim05/444/p0/d4 | 0.993499 | 1.010348 | 1.726376 | 1.010287 | 1.022317 | 186400.600000 | 119176.600000 | False | inconclusive |
| kodim05/444/p2/d1 | 0.994490 | 1.000198 | 1.109331 | 1.033367 | 1.021157 | 386920.633333 | 256243.166667 | False | inconclusive |
| kodim05/444/p2/d4 | 0.889506 | 1.016552 | 1.093532 | 1.030378 | 1.026937 | 294501.500000 | 163744.000000 | True | current |
| kodim05/420/p0/d1 | 1.031508 | 1.038168 | 1.427609 | 1.059955 | 1.067171 | 279042.733333 | 185258.400000 | False | inconclusive |
| kodim05/420/p0/d4 | 1.025513 | 1.001773 | 1.044069 | 1.027968 | 1.028758 | 202025.200000 | 108254.666667 | True | baseline |
| kodim05/420/p2/d1 | 0.989178 | 1.006062 | 1.219568 | 1.055794 | 1.017235 | 422950.666667 | 246622.400000 | False | inconclusive |
| kodim05/420/p2/d4 | 1.004049 | 1.002269 | 1.018577 | 1.027488 | 1.015652 | 331193.200000 | 154784.600000 | True | inconclusive |
| kodim06/444/p0/d1 | 1.001445 | 1.019768 | 1.051701 | 1.031957 | 1.035677 | 231282.833333 | 164050.000000 | True | inconclusive |
| kodim06/444/p0/d4 | 0.956112 | 1.003535 | 1.030663 | 1.026136 | 1.023226 | 164492.900000 | 97260.300000 | True | current |
| kodim06/444/p2/d1 | 0.887080 | 1.008253 | 1.132959 | 1.070480 | 1.037107 | 343790.033333 | 213074.666667 | False | inconclusive |
| kodim06/444/p2/d4 | 0.802196 | 1.000870 | 1.029445 | 1.027989 | 1.029068 | 266485.500000 | 135708.900000 | True | current |
| kodim06/420/p0/d1 | 1.022138 | 1.006396 | 1.467352 | 1.053330 | 1.087348 | 243812.066667 | 150020.933333 | False | inconclusive |
| kodim06/420/p0/d4 | 0.990768 | 1.007645 | 1.025818 | 1.011007 | 1.026531 | 182128.600000 | 88340.300000 | True | inconclusive |
| kodim06/420/p2/d1 | 1.002330 | 1.029469 | 1.071092 | 1.021906 | 1.058424 | 377258.633333 | 200883.833333 | True | inconclusive |
| kodim06/420/p2/d4 | 0.918478 | 1.001859 | 1.055473 | 1.019705 | 1.012779 | 305513.200000 | 129068.300000 | True | current |
| kodim07/444/p0/d1 | 1.015024 | 1.005411 | 1.463242 | 1.008003 | 1.016550 | 197228.900000 | 130004.900000 | False | inconclusive |
| kodim07/444/p0/d4 | 0.968526 | 1.010610 | 1.437431 | 1.015806 | 1.018658 | 153736.600000 | 86504.000000 | False | inconclusive |
| kodim07/444/p2/d1 | 0.965949 | 1.007134 | 1.205920 | 1.028189 | 1.029506 | 323112.600000 | 192479.466667 | False | inconclusive |
| kodim07/444/p2/d4 | 0.794417 | 1.001852 | 1.213096 | 1.062813 | 1.030740 | 263065.200000 | 132320.600000 | False | inconclusive |
| kodim07/420/p0/d1 | 1.020129 | 1.001733 | 1.041347 | 1.032500 | 1.025171 | 213369.200000 | 119576.600000 | True | baseline |
| kodim07/420/p0/d4 | 0.993089 | 1.012904 | 1.033632 | 1.023134 | 1.010937 | 174052.300000 | 80268.300000 | True | inconclusive |
| kodim07/420/p2/d1 | 0.997155 | 1.010533 | 1.122770 | 1.022851 | 1.023974 | 342753.800000 | 166340.900000 | False | inconclusive |
| kodim07/420/p2/d4 | 0.964104 | 1.014745 | 1.198701 | 1.035384 | 1.019199 | 298601.800000 | 122148.300000 | False | inconclusive |
| kodim08/444/p0/d1 | 1.018804 | 1.008486 | 1.280518 | 1.100026 | 1.044063 | 272516.100000 | 205269.633333 | False | inconclusive |
| kodim08/444/p0/d4 | 0.982193 | 1.068015 | 1.285251 | 1.084051 | 1.037167 | 185676.900000 | 118448.600000 | False | inconclusive |
| kodim08/444/p2/d1 | 0.981611 | 1.017084 | 1.027253 | 1.024865 | 1.032854 | 389615.000000 | 258949.366667 | True | inconclusive |
| kodim08/444/p2/d4 | 0.838434 | 1.002924 | 1.030757 | 1.020602 | 1.013105 | 294037.500000 | 163292.900000 | True | current |
| kodim08/420/p0/d1 | 1.036445 | 1.005787 | 1.342724 | 1.024698 | 1.081231 | 280672.600000 | 186880.000000 | False | inconclusive |
| kodim08/420/p0/d4 | 1.035128 | 1.009563 | 1.405016 | 1.023313 | 1.025083 | 201201.200000 | 107408.600000 | False | inconclusive |
| kodim08/420/p2/d1 | 0.998970 | 1.008412 | 1.144179 | 1.043116 | 1.039143 | 418658.633333 | 242280.600000 | False | inconclusive |
| kodim08/420/p2/d4 | 0.975999 | 1.008220 | 1.035474 | 1.015637 | 1.008097 | 336601.800000 | 160216.600000 | True | current |
| kodim09/444/p0/d1 | 0.986678 | 1.008052 | 1.018175 | 1.017254 | 1.020508 | 168028.900000 | 121916.300000 | True | current |
| kodim09/444/p0/d4 | 0.938373 | 1.001993 | 1.116637 | 1.024022 | 1.022208 | 125388.300000 | 79288.600000 | False | inconclusive |
| kodim09/444/p2/d1 | 0.914699 | 1.001441 | 1.030592 | 1.024324 | 1.029108 | 279497.200000 | 171572.900000 | True | current |
| kodim09/444/p2/d4 | 0.783857 | 1.007814 | 1.213967 | 1.018649 | 1.083378 | 226728.600000 | 118740.300000 | False | inconclusive |
| kodim09/420/p0/d1 | 1.026330 | 1.006551 | 1.093057 | 1.063997 | 1.011887 | 173702.066667 | 109832.600000 | True | inconclusive |
| kodim09/420/p0/d4 | 0.969065 | 1.000854 | 1.275628 | 1.014643 | 1.019256 | 137220.300000 | 73388.300000 | False | inconclusive |
| kodim09/420/p2/d1 | 0.978732 | 1.001265 | 1.275285 | 1.059968 | 1.029018 | 304665.200000 | 162084.900000 | False | inconclusive |
| kodim09/420/p2/d4 | 0.931416 | 1.024270 | 1.406861 | 1.065394 | 1.041370 | 252129.200000 | 109464.600000 | False | inconclusive |
| kodim10/444/p0/d1 | 0.997533 | 1.005519 | 1.046437 | 1.037731 | 1.031981 | 176644.900000 | 130540.900000 | True | inconclusive |
| kodim10/444/p0/d4 | 0.951616 | 1.000276 | 1.044957 | 1.011929 | 1.012403 | 128588.300000 | 82484.300000 | True | current |
| kodim10/444/p2/d1 | 0.913263 | 1.003166 | 1.293930 | 1.038766 | 1.041250 | 288869.500000 | 180936.600000 | False | inconclusive |
| kodim10/444/p2/d4 | 0.804314 | 1.016270 | 1.350572 | 1.017792 | 1.051117 | 230476.900000 | 122484.300000 | False | inconclusive |
| kodim10/420/p0/d1 | 1.040362 | 1.000335 | 1.472281 | 1.025141 | 1.022099 | 181768.600000 | 117936.600000 | False | inconclusive |
| kodim10/420/p0/d4 | 0.993592 | 1.006085 | 1.030767 | 1.025184 | 1.019783 | 139948.300000 | 76116.300000 | True | inconclusive |
| kodim10/420/p2/d1 | 1.003383 | 1.011105 | 1.058187 | 1.038703 | 1.031954 | 313229.500000 | 170644.900000 | True | inconclusive |
| kodim10/420/p2/d4 | 0.914790 | 1.000879 | 1.441698 | 1.021539 | 1.061828 | 261073.200000 | 118420.300000 | False | inconclusive |
| kodim11/444/p0/d1 | 0.994546 | 1.027207 | 1.317955 | 1.046856 | 1.041150 | 226206.166667 | 158940.300000 | False | inconclusive |
| kodim11/444/p0/d4 | 0.959371 | 1.007701 | 1.066157 | 1.025592 | 1.015398 | 160216.100000 | 92952.600000 | True | current |
| kodim11/444/p2/d1 | 0.950979 | 1.039778 | 1.061569 | 1.023820 | 1.063003 | 339373.500000 | 208728.900000 | True | current |
| kodim11/444/p2/d4 | 0.802080 | 1.002365 | 1.090109 | 1.027949 | 1.050940 | 268749.500000 | 138000.600000 | True | current |
| kodim11/420/p0/d1 | 1.009360 | 1.007488 | 1.429660 | 1.088538 | 1.069257 | 238761.833333 | 144991.233333 | False | inconclusive |
| kodim11/420/p0/d4 | 0.997823 | 1.003095 | 1.214296 | 1.111773 | 1.027396 | 178536.600000 | 84744.000000 | False | inconclusive |
| kodim11/420/p2/d1 | 1.012464 | 1.023130 | 1.051641 | 1.039540 | 1.036737 | 372537.166667 | 196151.833333 | True | inconclusive |
| kodim11/420/p2/d4 | 0.928262 | 1.003142 | 1.019716 | 1.048222 | 1.049714 | 308373.500000 | 131964.900000 | True | current |
| kodim12/444/p0/d1 | 0.989247 | 1.017083 | 1.132647 | 1.030634 | 1.042068 | 193848.600000 | 126620.300000 | False | inconclusive |
| kodim12/444/p0/d4 | 0.961707 | 1.011711 | 1.042315 | 1.047311 | 1.068258 | 146180.900000 | 78948.300000 | True | current |
| kodim12/444/p2/d1 | 0.910429 | 1.031230 | 1.172103 | 1.016168 | 1.040078 | 301620.900000 | 170884.900000 | False | inconclusive |
| kodim12/444/p2/d4 | 0.775525 | 1.013346 | 1.063984 | 1.035873 | 1.050132 | 260861.500000 | 130144.600000 | True | current |
| kodim12/420/p0/d1 | 1.018506 | 1.010921 | 1.040134 | 1.027170 | 1.037243 | 208801.200000 | 115045.866667 | True | baseline |
| kodim12/420/p0/d4 | 0.973860 | 1.008974 | 1.017515 | 1.017589 | 1.017830 | 166580.900000 | 72788.300000 | True | current |
| kodim12/420/p2/d1 | 0.993613 | 1.000695 | 1.035144 | 1.027942 | 1.028792 | 337665.800000 | 161248.600000 | True | inconclusive |
| kodim12/420/p2/d4 | 0.922893 | 1.001486 | 1.264493 | 1.058895 | 1.088312 | 290753.200000 | 114304.000000 | False | inconclusive |
| kodim13/444/p0/d1 | 1.017955 | 1.005871 | 1.097530 | 1.090997 | 1.076676 | 295479.333333 | 228279.633333 | True | inconclusive |
| kodim13/444/p0/d4 | 0.977044 | 1.006643 | 1.094450 | 1.041286 | 1.050706 | 191888.600000 | 124664.600000 | True | inconclusive |
| kodim13/444/p2/d1 | 0.908306 | 1.032716 | 1.047455 | 1.057139 | 1.044241 | 410323.133333 | 279626.833333 | True | current |
| kodim13/444/p2/d4 | 0.834986 | 1.003750 | 1.014136 | 1.021255 | 1.019223 | 298057.200000 | 167316.900000 | True | current |
| kodim13/420/p0/d1 | 1.008359 | 1.027475 | 1.322477 | 1.143350 | 1.066162 | 302725.966667 | 208871.133333 | False | inconclusive |
| kodim13/420/p0/d4 | 1.021798 | 1.014836 | 1.047119 | 1.054524 | 1.063729 | 204356.900000 | 110568.600000 | True | inconclusive |
| kodim13/420/p2/d1 | 1.009120 | 1.006582 | 1.068518 | 1.018144 | 1.044572 | 438718.666667 | 262326.300000 | True | inconclusive |
| kodim13/420/p2/d4 | 0.939480 | 1.034003 | 1.085453 | 1.088667 | 1.040682 | 338473.800000 | 162084.300000 | True | current |
| kodim14/444/p0/d1 | 1.033610 | 1.195371 | 1.413182 | 1.043184 | 1.403997 | 253705.500000 | 186424.633333 | False | inconclusive |
| kodim14/444/p0/d4 | 0.982809 | 1.003932 | 1.152666 | 1.023979 | 1.034030 | 172452.300000 | 105228.300000 | False | inconclusive |
| kodim14/444/p2/d1 | 0.952747 | 1.020646 | 1.250978 | 1.080571 | 1.028118 | 358717.700000 | 227959.833333 | False | inconclusive |
| kodim14/444/p2/d4 | 0.878293 | 1.009077 | 1.093706 | 1.026432 | 1.082093 | 286005.500000 | 155288.600000 | True | current |
| kodim14/420/p0/d1 | 1.030005 | 1.013492 | 1.149799 | 1.082246 | 1.088776 | 262090.600000 | 168272.600000 | False | inconclusive |
| kodim14/420/p0/d4 | 1.011543 | 1.009006 | 1.054146 | 1.034796 | 1.054666 | 188768.600000 | 94984.600000 | True | inconclusive |
| kodim14/420/p2/d1 | 0.990654 | 1.015872 | 1.529031 | 1.023903 | 1.039283 | 388098.766667 | 211659.433333 | False | inconclusive |
| kodim14/420/p2/d4 | 0.968640 | 1.015581 | 1.104800 | 1.023126 | 1.036920 | 317725.500000 | 141312.600000 | False | inconclusive |
| kodim15/444/p0/d1 | 1.028527 | 1.050726 | 1.110774 | 1.062338 | 1.126764 | 201190.533333 | 133978.166667 | False | inconclusive |
| kodim15/444/p0/d4 | 0.961706 | 1.004714 | 1.151743 | 1.031152 | 1.032995 | 149664.733333 | 82400.000000 | False | inconclusive |
| kodim15/444/p2/d1 | 0.967525 | 1.001660 | 1.189166 | 1.014713 | 1.019061 | 326137.800000 | 195484.900000 | False | inconclusive |
| kodim15/444/p2/d4 | 0.800022 | 1.006191 | 1.054490 | 1.024055 | 1.023527 | 264772.900000 | 134064.600000 | True | current |
| kodim15/420/p0/d1 | 1.027061 | 1.006092 | 1.142931 | 1.020547 | 1.023820 | 214416.600000 | 120628.300000 | False | inconclusive |
| kodim15/420/p0/d4 | 0.995160 | 1.002795 | 1.025752 | 1.020714 | 1.025052 | 169792.600000 | 76008.600000 | True | inconclusive |
| kodim15/420/p2/d1 | 0.995261 | 1.008615 | 1.081841 | 1.027055 | 1.021877 | 343253.500000 | 166844.900000 | True | inconclusive |
| kodim15/420/p2/d4 | 0.954492 | 1.029452 | 1.349865 | 1.025885 | 1.024647 | 294409.200000 | 118011.200000 | False | inconclusive |
| kodim16/444/p0/d1 | 0.983632 | 1.012813 | 1.059801 | 1.029755 | 1.031635 | 204038.533333 | 136810.366667 | True | inconclusive |
| kodim16/444/p0/d4 | 0.948630 | 1.001175 | 1.021203 | 1.022800 | 1.016351 | 151204.900000 | 83972.300000 | True | current |
| kodim16/444/p2/d1 | 0.853871 | 1.001878 | 1.183633 | 1.039159 | 1.045464 | 317370.500000 | 186638.866667 | False | inconclusive |
| kodim16/444/p2/d4 | 0.772715 | 1.001561 | 1.049339 | 1.038328 | 1.046798 | 253589.500000 | 122808.600000 | True | current |
| kodim16/420/p0/d1 | 1.009531 | 1.009592 | 1.078226 | 1.043031 | 1.071290 | 218489.200000 | 124696.600000 | True | inconclusive |
| kodim16/420/p0/d4 | 0.975315 | 1.001369 | 1.072401 | 1.061152 | 1.046583 | 170572.900000 | 76784.600000 | True | current |
| kodim16/420/p2/d1 | 1.005083 | 1.011097 | 1.031055 | 1.026423 | 1.015298 | 347485.500000 | 171068.300000 | True | inconclusive |
| kodim16/420/p2/d4 | 0.888748 | 1.018446 | 1.053739 | 1.046987 | 1.033279 | 300169.200000 | 123756.300000 | True | current |
| kodim17/444/p0/d1 | 0.975647 | 1.016382 | 1.143975 | 1.056658 | 1.039321 | 181515.266667 | 135415.566667 | False | inconclusive |
| kodim17/444/p0/d4 | 0.939693 | 1.011082 | 1.364067 | 1.039445 | 1.032779 | 131368.600000 | 85256.000000 | False | inconclusive |
| kodim17/444/p2/d1 | 0.846021 | 1.041002 | 1.261627 | 1.072750 | 1.133340 | 292493.500000 | 184564.900000 | False | inconclusive |
| kodim17/444/p2/d4 | 0.792791 | 1.011510 | 1.037918 | 1.018515 | 1.015380 | 232308.900000 | 124320.600000 | True | current |
| kodim17/420/p0/d1 | 1.025036 | 1.007064 | 1.352966 | 1.034265 | 1.051716 | 186656.600000 | 122820.300000 | False | inconclusive |
| kodim17/420/p0/d4 | 0.991715 | 1.001190 | 1.152426 | 1.058599 | 1.043497 | 142056.600000 | 78220.300000 | False | inconclusive |
| kodim17/420/p2/d1 | 0.992017 | 1.006088 | 1.189321 | 1.020931 | 1.091606 | 317185.200000 | 174604.900000 | False | inconclusive |
| kodim17/420/p2/d4 | 0.902461 | 1.018676 | 1.022194 | 1.022699 | 1.024164 | 268416.600000 | 125808.600000 | True | current |
| kodim18/444/p0/d1 | 1.030809 | 1.003583 | 1.043073 | 1.050508 | 1.072564 | 226839.200000 | 180778.033333 | True | baseline |
| kodim18/444/p0/d4 | 0.964629 | 1.003343 | 1.411553 | 1.048025 | 1.028782 | 148400.600000 | 102292.300000 | False | inconclusive |
| kodim18/444/p2/d1 | 0.990613 | 1.005739 | 1.033167 | 1.032945 | 1.016721 | 342416.366667 | 234530.300000 | True | inconclusive |
| kodim18/444/p2/d4 | 0.821461 | 1.014327 | 1.034844 | 1.027006 | 1.019759 | 261161.200000 | 153232.600000 | True | current |
| kodim18/420/p0/d1 | 1.029163 | 1.000193 | 1.109053 | 1.042198 | 1.023821 | 227475.300000 | 163548.200000 | False | inconclusive |
| kodim18/420/p0/d4 | 1.016145 | 1.009976 | 1.027696 | 1.028648 | 1.025400 | 156344.600000 | 92512.600000 | True | baseline |
| kodim18/420/p2/d1 | 1.009494 | 1.015156 | 1.042012 | 1.029366 | 1.033855 | 361716.233333 | 219181.933333 | True | inconclusive |
| kodim18/420/p2/d4 | 0.968817 | 1.008584 | 1.017864 | 1.015960 | 1.014393 | 276833.200000 | 134184.600000 | True | current |
| kodim19/444/p0/d1 | 0.983737 | 1.000032 | 1.058973 | 1.024844 | 1.025652 | 196676.900000 | 150606.800000 | True | inconclusive |
| kodim19/444/p0/d4 | 0.942979 | 1.001190 | 1.014476 | 1.020375 | 1.017834 | 135440.600000 | 89332.300000 | True | current |
| kodim19/444/p2/d1 | 0.887199 | 1.026291 | 1.032603 | 1.021620 | 1.026155 | 307687.033333 | 199754.666667 | True | current |
| kodim19/444/p2/d4 | 0.797005 | 1.008660 | 1.027060 | 1.013307 | 1.025787 | 236297.200000 | 128308.900000 | True | current |
| kodim19/420/p0/d1 | 1.021957 | 1.011382 | 1.069115 | 1.050793 | 1.023569 | 199806.100000 | 135948.100000 | True | inconclusive |
| kodim19/420/p0/d4 | 0.993931 | 1.006544 | 1.028898 | 1.021817 | 1.010528 | 145532.300000 | 81700.300000 | True | inconclusive |
| kodim19/420/p2/d1 | 0.995117 | 1.008239 | 1.263061 | 1.033453 | 1.013588 | 329988.600000 | 187360.600000 | False | inconclusive |
| kodim19/420/p2/d4 | 0.913189 | 1.007410 | 1.058872 | 1.014681 | 1.057172 | 265652.900000 | 123008.600000 | True | current |
| kodim20/444/p0/d1 | 0.968722 | 1.009417 | 1.040911 | 1.014359 | 1.023430 | 187837.633333 | 120560.600000 | True | current |
| kodim20/444/p0/d4 | 0.929661 | 1.001458 | 1.022365 | 1.017060 | 1.014563 | 145496.600000 | 78268.300000 | True | current |
| kodim20/444/p2/d1 | 0.932455 | 1.008452 | 1.018546 | 1.009822 | 1.017817 | 300141.500000 | 169442.833333 | True | current |
| kodim20/444/p2/d4 | 0.769201 | 1.008322 | 1.041629 | 1.038072 | 1.012980 | 247353.200000 | 116576.600000 | True | current |
| kodim20/420/p0/d1 | 0.991659 | 1.004858 | 1.023455 | 1.018644 | 1.018512 | 204580.900000 | 110792.600000 | True | inconclusive |
| kodim20/420/p0/d4 | 0.952656 | 1.005248 | 1.054829 | 1.020621 | 1.012375 | 166236.900000 | 72444.300000 | True | current |
| kodim20/420/p2/d1 | 0.966055 | 1.002776 | 1.016984 | 1.016261 | 1.011402 | 337672.266667 | 161272.600000 | True | current |
| kodim20/420/p2/d4 | 0.920008 | 1.005914 | 1.046464 | 1.027750 | 1.023150 | 289513.200000 | 113068.300000 | True | current |
| kodim21/444/p0/d1 | 1.025622 | 1.033442 | 1.075825 | 1.020876 | 1.037153 | 220438.166667 | 153234.033333 | True | inconclusive |
| kodim21/444/p0/d4 | 0.954202 | 1.005566 | 1.013847 | 1.011790 | 1.013760 | 158760.600000 | 91536.600000 | True | current |
| kodim21/444/p2/d1 | 0.884323 | 1.006062 | 1.046665 | 1.021476 | 1.024689 | 331925.700000 | 201190.266667 | True | current |
| kodim21/444/p2/d4 | 0.782916 | 1.022685 | 1.059853 | 1.044445 | 1.044122 | 260673.200000 | 129892.300000 | True | current |
| kodim21/420/p0/d1 | 1.026210 | 1.017618 | 1.052202 | 1.018203 | 1.047910 | 231982.100000 | 138243.566667 | True | baseline |
| kodim21/420/p0/d4 | 0.975178 | 1.012113 | 1.013856 | 1.014530 | 1.013816 | 177476.900000 | 83684.300000 | True | current |
| kodim21/420/p2/d1 | 0.982460 | 1.006929 | 1.045243 | 1.033872 | 1.040155 | 365047.400000 | 188670.300000 | True | inconclusive |
| kodim21/420/p2/d4 | 0.899257 | 1.001957 | 1.033838 | 1.017922 | 1.007475 | 300665.200000 | 124224.600000 | True | current |
| kodim22/444/p0/d1 | 1.031830 | 1.003072 | 1.045319 | 1.043982 | 1.023722 | 228540.466667 | 161274.000000 | True | baseline |
| kodim22/444/p0/d4 | 0.976652 | 1.003654 | 1.063944 | 1.016691 | 1.009990 | 158776.600000 | 91548.300000 | True | current |
| kodim22/444/p2/d1 | 0.980087 | 1.011044 | 1.063765 | 1.018565 | 1.030675 | 340531.300000 | 209783.333333 | True | inconclusive |
| kodim22/444/p2/d4 | 0.808110 | 1.006346 | 1.013507 | 1.022716 | 1.025144 | 267581.500000 | 136836.900000 | True | current |
| kodim22/420/p0/d1 | 1.038622 | 1.006518 | 1.066669 | 1.051117 | 1.021648 | 238694.866667 | 144884.400000 | True | baseline |
| kodim22/420/p0/d4 | 1.011517 | 1.007814 | 1.048577 | 1.014745 | 1.024892 | 176964.900000 | 83172.300000 | True | baseline |
| kodim22/420/p2/d1 | 1.016860 | 1.000009 | 1.046169 | 1.010123 | 1.035590 | 371821.500000 | 195507.466667 | True | baseline |
| kodim22/420/p2/d4 | 0.950068 | 1.005246 | 1.044415 | 1.033867 | 1.019329 | 306969.200000 | 130560.600000 | True | current |
| kodim23/444/p0/d1 | 1.029903 | 1.026716 | 1.086295 | 1.021178 | 1.043733 | 183024.600000 | 115800.600000 | True | inconclusive |
| kodim23/444/p0/d4 | 0.975830 | 1.009559 | 1.033841 | 1.014691 | 1.019378 | 144904.600000 | 77676.300000 | True | current |
| kodim23/444/p2/d1 | 0.930619 | 1.004084 | 1.041864 | 1.018600 | 1.021128 | 297241.200000 | 166562.600000 | True | current |
| kodim23/444/p2/d4 | 0.772653 | 1.003836 | 1.045853 | 1.033349 | 1.020041 | 254521.200000 | 123776.600000 | True | current |
| kodim23/420/p0/d1 | 1.024613 | 1.005199 | 1.106946 | 1.049750 | 1.020678 | 197616.600000 | 103832.600000 | False | inconclusive |
| kodim23/420/p0/d4 | 0.991615 | 1.021684 | 1.017335 | 1.015006 | 1.018553 | 165924.900000 | 72132.300000 | True | inconclusive |
| kodim23/420/p2/d1 | 0.990369 | 1.013136 | 1.027384 | 1.022973 | 1.029999 | 315345.800000 | 138880.600000 | True | inconclusive |
| kodim23/420/p2/d4 | 0.914152 | 1.002868 | 1.032560 | 1.025814 | 1.022052 | 279081.200000 | 102760.600000 | True | current |
| kodim24/444/p0/d1 | 1.018635 | 1.017768 | 1.106815 | 1.035318 | 1.036290 | 245498.966667 | 178206.700000 | False | inconclusive |
| kodim24/444/p0/d4 | 0.979787 | 1.000390 | 1.026066 | 1.009309 | 1.025718 | 171100.900000 | 103872.600000 | True | current |
| kodim24/444/p2/d1 | 0.970242 | 1.005872 | 1.050174 | 1.023873 | 1.033806 | 357397.333333 | 226633.466667 | True | current |
| kodim24/444/p2/d4 | 0.819139 | 1.005751 | 1.032430 | 1.004632 | 1.010236 | 279289.200000 | 148548.900000 | True | current |
| kodim24/420/p0/d1 | 1.026740 | 1.024237 | 1.066943 | 1.017687 | 1.028526 | 255616.600000 | 161874.500000 | True | baseline |
| kodim24/420/p0/d4 | 1.012916 | 1.002774 | 1.026442 | 1.014131 | 1.004403 | 187656.600000 | 93864.000000 | True | inconclusive |
| kodim24/420/p2/d1 | 1.016702 | 1.009044 | 1.059715 | 1.033816 | 1.030231 | 382844.066667 | 206460.566667 | True | inconclusive |
| kodim24/420/p2/d4 | 0.949644 | 1.021123 | 1.018827 | 1.023781 | 1.025719 | 317521.200000 | 141112.600000 | True | current |
| smooth-gradient/444/p0/d1 | 0.874112 | 1.010177 | 1.118843 | 1.011720 | 1.012147 | 76421.900000 | 50736.000000 | False | inconclusive |
| smooth-gradient/444/p0/d4 | 0.822334 | 1.006607 | 1.040815 | 1.028150 | 1.011256 | 69516.300000 | 43848.000000 | True | current |
| smooth-gradient/444/p2/d1 | 0.662585 | 1.018961 | 1.046270 | 1.030260 | 1.033952 | 171316.900000 | 85328.600000 | True | current |
| smooth-gradient/444/p2/d4 | 0.643530 | 1.005740 | 1.016114 | 1.012205 | 1.011387 | 158852.300000 | 72984.000000 | True | current |
| smooth-gradient/420/p0/d1 | 0.904671 | 1.032106 | 1.149305 | 1.063265 | 1.018571 | 84792.600000 | 49972.300000 | False | inconclusive |
| smooth-gradient/420/p0/d4 | 0.872506 | 1.008536 | 1.054798 | 1.023497 | 1.012314 | 84060.300000 | 49240.000000 | True | current |
| smooth-gradient/420/p2/d1 | 0.695479 | 1.014503 | 1.047616 | 1.029411 | 1.019751 | 190232.600000 | 80444.300000 | True | current |
| smooth-gradient/420/p2/d4 | 0.687260 | 1.004409 | 1.050747 | 1.056058 | 1.066063 | 183916.900000 | 74104.000000 | True | current |
| low-contrast/444/p0/d1 | 0.938410 | 1.002923 | 1.049396 | 1.036920 | 1.018788 | 87232.600000 | 61580.300000 | True | current |
| low-contrast/444/p0/d4 | 0.891590 | 1.011902 | 1.050702 | 1.009841 | 1.021804 | 77212.300000 | 51564.300000 | True | current |
| low-contrast/444/p2/d1 | 0.821634 | 1.007405 | 1.033910 | 1.021890 | 1.029867 | 216696.600000 | 130872.600000 | True | current |
| low-contrast/444/p2/d4 | 0.736304 | 1.014860 | 1.034632 | 1.020727 | 1.033245 | 190112.600000 | 104208.600000 | True | current |
| low-contrast/420/p0/d1 | 0.937813 | 1.007466 | 1.050452 | 1.044832 | 1.022039 | 93092.300000 | 58276.300000 | True | current |
| low-contrast/420/p0/d4 | 0.891934 | 1.032662 | 1.063475 | 1.055893 | 1.012652 | 85312.600000 | 50488.000000 | True | current |
| low-contrast/420/p2/d1 | 0.892434 | 1.005390 | 1.040894 | 1.032580 | 1.024991 | 221641.200000 | 111792.600000 | True | current |
| low-contrast/420/p2/d4 | 0.715650 | 1.008658 | 1.036339 | 1.036078 | 1.017274 | 202636.900000 | 92724.300000 | True | current |
| color-edges/444/p0/d1 | 0.887526 | 1.000581 | 1.036806 | 1.006859 | 1.018760 | 94260.300000 | 68596.300000 | True | current |
| color-edges/444/p0/d4 | 0.869510 | 1.015576 | 1.039575 | 1.037236 | 1.025744 | 84088.600000 | 58420.300000 | True | current |
| color-edges/444/p2/d1 | 0.871916 | 1.008010 | 1.028360 | 1.019639 | 1.020533 | 206608.600000 | 120688.600000 | True | current |
| color-edges/444/p2/d4 | 0.872191 | 1.005595 | 1.050485 | 1.038098 | 1.024879 | 191121.200000 | 105160.600000 | True | current |
| color-edges/420/p0/d1 | 0.907562 | 1.007744 | 1.506668 | 1.017240 | 1.043842 | 104172.300000 | 69356.300000 | False | inconclusive |
| color-edges/420/p0/d4 | 0.890301 | 1.001973 | 1.042632 | 1.007661 | 1.011714 | 90044.300000 | 55212.300000 | True | current |
| color-edges/420/p2/d1 | 0.859960 | 1.009090 | 1.039002 | 1.051138 | 1.043426 | 220889.200000 | 110992.600000 | True | current |
| color-edges/420/p2/d4 | 0.868682 | 1.006239 | 1.029958 | 1.014617 | 1.025157 | 218476.300000 | 108624.600000 | True | current |
| line-art/444/p0/d1 | 0.836249 | 1.004810 | 1.043945 | 1.031488 | 1.016227 | 94816.600000 | 69160.000000 | True | current |
| line-art/444/p0/d4 | 0.813635 | 1.007474 | 1.229193 | 1.013995 | 1.037103 | 87752.000000 | 62112.600000 | False | inconclusive |
| line-art/444/p2/d1 | 0.744214 | 1.008230 | 1.028516 | 1.021471 | 1.018862 | 195340.900000 | 109368.600000 | True | current |
| line-art/444/p2/d4 | 0.733036 | 1.000251 | 1.042287 | 1.012453 | 1.010473 | 182528.600000 | 96540.300000 | True | current |
| line-art/420/p0/d1 | 0.891655 | 1.032290 | 1.046724 | 1.036277 | 1.027649 | 102672.600000 | 67852.300000 | True | current |
| line-art/420/p0/d4 | 0.874623 | 1.015854 | 1.038350 | 1.008495 | 1.039156 | 95312.600000 | 60496.600000 | True | current |
| line-art/420/p2/d1 | 0.829768 | 1.007651 | 1.018971 | 1.018290 | 1.019094 | 213761.200000 | 103824.000000 | True | current |
| line-art/420/p2/d4 | 0.817624 | 1.029617 | 1.027224 | 1.018476 | 1.029912 | 206432.600000 | 96512.600000 | True | current |
| structured-texture/444/p0/d1 | 0.958804 | 1.004149 | 1.035073 | 1.021332 | 1.028367 | 117972.300000 | 92312.600000 | True | current |
| structured-texture/444/p0/d4 | 0.937857 | 1.001804 | 1.033969 | 1.028576 | 1.008394 | 88132.300000 | 62464.000000 | True | current |
| structured-texture/444/p2/d1 | 0.956091 | 1.002695 | 1.061931 | 1.037041 | 1.022989 | 248297.200000 | 162468.900000 | True | current |
| structured-texture/444/p2/d4 | 0.932809 | 1.015137 | 1.035753 | 1.021620 | 1.019435 | 195052.900000 | 109096.600000 | True | current |
| structured-texture/420/p0/d1 | 0.941012 | 1.010626 | 1.039841 | 1.020679 | 1.015438 | 108744.000000 | 73916.300000 | True | current |
| structured-texture/420/p0/d4 | 0.910172 | 1.001968 | 1.044774 | 1.033772 | 1.010002 | 89840.600000 | 55004.300000 | True | current |
| structured-texture/420/p2/d1 | 0.936374 | 1.003563 | 1.014754 | 1.017066 | 1.016820 | 232193.200000 | 122296.600000 | True | current |
| structured-texture/420/p2/d4 | 0.917752 | 1.021441 | 1.027320 | 1.020936 | 1.028126 | 201272.600000 | 91488.600000 | True | current |
| low-noise/444/p0/d1 | 0.920186 | 1.020935 | 1.061346 | 1.042726 | 1.010236 | 89968.600000 | 64312.000000 | True | current |
| low-noise/444/p0/d4 | 0.839988 | 1.012486 | 1.034356 | 1.014838 | 1.014346 | 75316.300000 | 49668.300000 | True | current |
| low-noise/444/p2/d1 | 0.825586 | 1.009433 | 1.043283 | 1.018407 | 1.024276 | 196969.200000 | 111024.600000 | True | current |
| low-noise/444/p2/d4 | 0.655791 | 1.014013 | 1.023788 | 1.024105 | 1.011491 | 165052.900000 | 79196.300000 | True | current |
| low-noise/420/p0/d1 | 0.931611 | 1.009027 | 1.055601 | 1.019269 | 1.020068 | 88668.300000 | 53832.000000 | True | current |
| low-noise/420/p0/d4 | 0.873235 | 1.007214 | 1.056352 | 1.022748 | 1.021843 | 78056.600000 | 43220.300000 | True | current |
| low-noise/420/p2/d1 | 0.886152 | 1.001300 | 1.041528 | 1.021395 | 1.026930 | 211452.900000 | 101544.600000 | True | current |
| low-noise/420/p2/d4 | 0.693250 | 1.004118 | 1.038758 | 1.027574 | 1.041640 | 189644.900000 | 79856.600000 | True | current |
| full-noise/444/p0/d1 | 0.952456 | 1.006644 | 1.052497 | 1.052288 | 1.040169 | 135792.600000 | 110136.000000 | True | current |
| full-noise/444/p0/d4 | 0.938630 | 1.010607 | 1.085244 | 1.032221 | 1.030404 | 103660.300000 | 78012.300000 | True | current |
| full-noise/444/p2/d1 | 0.972329 | 1.003530 | 1.028814 | 1.017754 | 1.028106 | 277689.200000 | 191932.300000 | True | current |
| full-noise/444/p2/d4 | 0.930404 | 1.022970 | 1.045160 | 1.026797 | 1.032506 | 216233.200000 | 130320.600000 | True | current |
| full-noise/420/p0/d1 | 0.947952 | 1.000539 | 1.052332 | 1.016113 | 1.031187 | 116400.600000 | 81564.300000 | True | current |
| full-noise/420/p0/d4 | 0.925905 | 1.000418 | 1.065174 | 1.037429 | 1.017655 | 96360.600000 | 61524.300000 | True | current |
| full-noise/420/p2/d1 | 0.943246 | 1.002229 | 1.018871 | 1.012314 | 1.009854 | 245657.200000 | 135784.000000 | True | current |
| full-noise/420/p2/d4 | 0.904317 | 1.002862 | 1.030838 | 1.022536 | 1.024976 | 219220.300000 | 109336.600000 | True | current |
| tiny-odd-edges/444/p0/d1 | 0.603509 | 1.026941 | 1.212668 | 1.098937 | 1.098295 | 42715.440000 | 36833.720000 | False | inconclusive |
| tiny-odd-edges/444/p0/d4 | 0.525235 | 1.077932 | 1.105598 | 1.053541 | 1.104714 | 42418.580000 | 36538.580000 | False | inconclusive |
| tiny-odd-edges/444/p2/d1 | 0.515339 | 1.017984 | 1.338644 | 1.042785 | 1.058805 | 131007.740000 | 64412.300000 | False | inconclusive |
| tiny-odd-edges/444/p2/d4 | 0.481412 | 1.000006 | 1.069792 | 1.027432 | 1.072732 | 130680.600000 | 64082.580000 | True | current |
| tiny-odd-edges/420/p0/d1 | 0.628720 | 1.020425 | 1.130480 | 1.061625 | 1.119637 | 43410.580000 | 36642.580000 | False | inconclusive |
| tiny-odd-edges/420/p0/d4 | 0.548743 | 1.051564 | 1.105621 | 1.061036 | 1.074321 | 43234.580000 | 36464.860000 | False | inconclusive |
| tiny-odd-edges/420/p2/d1 | 0.499430 | 1.023418 | 1.075618 | 1.030073 | 1.057195 | 145712.600000 | 65500.300000 | True | current |
| tiny-odd-edges/420/p2/d4 | 0.481741 | 1.005162 | 1.065988 | 1.038496 | 1.066252 | 145480.600000 | 65268.300000 | True | current |

Invalid evidence:

- kodim01/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim01/444/p0/d1: ABBA/BAAB order bias exceeds10% or one order is absent.
- kodim01/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim01/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim01/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim01/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim02/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim02/444/p0/d1: current: p90/p10 drift exceeds20%, including minority timing states.
- kodim02/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim02/444/p0/d1: ABBA/BAAB order bias exceeds10% or one order is absent.
- kodim02/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim02/444/p2/d4: baseline: p90/p10 drift exceeds20%, including minority timing states.
- kodim02/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim02/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim03/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim03/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim03/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim04/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim04/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim04/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim04/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim04/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim05/444/p0/d1: current: p90/p10 drift exceeds20%, including minority timing states.
- kodim05/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim05/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim05/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim05/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim05/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim06/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim06/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim07/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim07/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim07/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim07/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim07/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim07/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim08/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim08/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim08/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim08/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim08/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim08/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim08/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim08/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim09/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim09/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim09/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim09/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim09/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim09/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim09/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim10/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim10/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim10/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim10/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim10/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim10/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim11/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim11/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim11/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim12/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim12/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim12/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim13/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim13/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim14/444/p0/d1: current: p90/p10 drift exceeds20%, including minority timing states.
- kodim14/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim14/444/p0/d1: ABBA/BAAB order bias exceeds10% or one order is absent.
- kodim14/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim14/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim14/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim14/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim14/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim14/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim15/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim15/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim15/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim15/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim15/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim15/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim16/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim17/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim17/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim17/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim17/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim17/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim17/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim17/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim18/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim18/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim18/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim19/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim19/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim23/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim24/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- smooth-gradient/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- smooth-gradient/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- color-edges/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- line-art/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p0/d4: current: repeated legs within a quartet differ by over10%.
