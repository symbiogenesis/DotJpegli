# Interleaved revision comparison

Persistent baseline and current workers run serially as balanced ABBA/BAAB quartets with identical fixed iterations, warmups, runtime, CPU affinity, encoding policy, inputs and JPEG hashes. Startup and IPC are outside encode timing. Ratios below1 favor current. Allocations are mean bytes per encode; retained rental capacity and peak working memory are separate measurements.

MAD, repeated-leg drift, p90/p10 minority-state drift and order bias guard the timing evidence. Invalid rows retain raw measurements and cannot support a speed claim. Valid rows remain inconclusive unless every quartet differs by more than 0.5% in the same direction. This comparison has no native parity verdict.

Release comparison: 256/256 cases against published 1.0.0 source `a40868bfdace7d737413085522a88d351aa16782`, rebuilt with the candidate SDK. 8 valid cases consistently favor 1.0.0 and fail the regression gate. Invalid or missing evidence fails validation separately. Inconclusive differences are retained and do not prove equal performance. The gate checks latency; allocation counts are reported separately.

| Case | Current/baseline | Order bias | Maximum repeat drift | Baseline p90/p10 | Current p90/p10 | Baseline B/encode | Current B/encode | Valid | Preference |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| kodim01/444/p0/d1 | 0.956114 | 1.001324 | 1.126506 | 1.020359 | 1.019376 | 262862.600000 | 195560.433333 | False | inconclusive |
| kodim01/444/p0/d4 | 0.947491 | 1.006549 | 1.047403 | 1.009527 | 1.028185 | 177260.700000 | 109952.000000 | True | current |
| kodim01/444/p2/d1 | 0.942361 | 1.002602 | 1.056852 | 1.026389 | 1.014444 | 385607.866667 | 254928.300000 | True | current |
| kodim01/444/p2/d4 | 0.847963 | 1.001397 | 1.044898 | 1.036889 | 1.010326 | 296599.400000 | 165894.100000 | True | current |
| kodim01/420/p0/d1 | 0.969813 | 1.006744 | 1.097687 | 1.127142 | 1.130954 | 271716.300000 | 177933.533333 | True | inconclusive |
| kodim01/420/p0/d4 | 0.958415 | 1.003164 | 1.028169 | 1.109857 | 1.136017 | 192484.300000 | 98638.100000 | True | inconclusive |
| kodim01/420/p2/d1 | 1.030420 | 1.002860 | 1.047935 | 1.039557 | 1.031010 | 404427.766667 | 228108.600000 | True | baseline |
| kodim01/420/p2/d4 | 0.978773 | 1.025762 | 1.118759 | 1.145833 | 1.124856 | 327392.000000 | 151012.300000 | False | inconclusive |
| kodim02/444/p0/d1 | 0.959887 | 1.009774 | 1.036972 | 1.015587 | 1.029921 | 210368.000000 | 143148.366667 | True | current |
| kodim02/444/p0/d4 | 0.918270 | 1.028608 | 1.093586 | 1.132346 | 1.127323 | 148212.300000 | 80912.000000 | True | current |
| kodim02/444/p2/d1 | 0.948011 | 1.001832 | 1.125145 | 1.062378 | 1.109755 | 353116.166667 | 222537.966667 | False | inconclusive |
| kodim02/444/p2/d4 | 0.829752 | 1.000715 | 1.021241 | 1.012296 | 1.013328 | 269656.000000 | 138988.300000 | True | current |
| kodim02/420/p0/d1 | 0.991107 | 1.000918 | 1.008322 | 1.008451 | 1.007909 | 216100.300000 | 122224.000000 | True | inconclusive |
| kodim02/420/p0/d4 | 0.952831 | 1.018828 | 1.037242 | 1.007085 | 1.025334 | 162224.000000 | 68356.300000 | True | current |
| kodim02/420/p2/d1 | 1.033106 | 1.003540 | 1.025816 | 1.008328 | 1.016350 | 362028.300000 | 185704.000000 | True | baseline |
| kodim02/420/p2/d4 | 0.944682 | 1.001197 | 1.013518 | 1.010929 | 1.011189 | 292944.000000 | 116532.300000 | True | current |
| kodim03/444/p0/d1 | 0.939916 | 1.003964 | 1.027742 | 1.007617 | 1.015024 | 185204.300000 | 117904.000000 | True | current |
| kodim03/444/p0/d4 | 0.913565 | 1.000569 | 1.008712 | 1.008124 | 1.004675 | 145676.300000 | 78376.000000 | True | current |
| kodim03/444/p2/d1 | 0.959507 | 1.010459 | 1.023769 | 1.016571 | 1.013940 | 294036.300000 | 163300.300000 | True | current |
| kodim03/444/p2/d4 | 0.842208 | 1.007353 | 1.016459 | 1.011489 | 1.010262 | 249060.300000 | 118288.000000 | True | current |
| kodim03/420/p0/d1 | 0.951800 | 1.006178 | 1.011298 | 1.006589 | 1.010377 | 202080.000000 | 108228.300000 | True | current |
| kodim03/420/p0/d4 | 0.921097 | 1.005757 | 1.011593 | 1.004640 | 1.008057 | 166364.300000 | 72504.000000 | True | current |
| kodim03/420/p2/d1 | 1.021227 | 1.013642 | 1.010913 | 1.014119 | 1.013178 | 331636.300000 | 155236.300000 | True | inconclusive |
| kodim03/420/p2/d4 | 0.952664 | 1.001111 | 1.102261 | 1.028983 | 1.014869 | 290964.300000 | 114528.000000 | False | inconclusive |
| kodim04/444/p0/d1 | 0.955690 | 1.007712 | 1.117708 | 1.035475 | 1.051638 | 189917.566667 | 143754.666667 | False | inconclusive |
| kodim04/444/p0/d4 | 0.923666 | 1.000133 | 1.008838 | 1.005638 | 1.007530 | 130976.000000 | 84804.300000 | True | current |
| kodim04/444/p2/d1 | 0.933987 | 1.025196 | 1.096648 | 1.137376 | 1.120066 | 312112.000000 | 204301.266667 | True | current |
| kodim04/444/p2/d4 | 0.819502 | 1.003604 | 1.067787 | 1.124884 | 1.109540 | 244408.000000 | 136488.000000 | True | current |
| kodim04/420/p0/d1 | 0.972898 | 1.023635 | 1.126152 | 1.176129 | 1.137941 | 193048.000000 | 129148.300000 | False | inconclusive |
| kodim04/420/p0/d4 | 0.934344 | 1.008899 | 1.083790 | 1.136339 | 1.138264 | 141600.000000 | 77696.000000 | True | current |
| kodim04/420/p2/d1 | 1.039865 | 1.011318 | 1.021435 | 1.026493 | 1.012046 | 323044.300000 | 180464.000000 | True | baseline |
| kodim04/420/p2/d4 | 0.958066 | 1.001147 | 1.023274 | 1.014009 | 1.012781 | 268181.700000 | 125568.000000 | True | current |
| kodim05/444/p0/d1 | 0.957291 | 1.002062 | 1.168861 | 1.027789 | 1.035619 | 269307.500000 | 202050.733333 | False | inconclusive |
| kodim05/444/p0/d4 | 0.942946 | 1.032706 | 1.137207 | 1.132895 | 1.138251 | 186392.000000 | 119100.300000 | False | inconclusive |
| kodim05/444/p2/d1 | 1.012427 | 1.003059 | 1.180013 | 1.124995 | 1.135238 | 386884.533333 | 256209.566667 | False | inconclusive |
| kodim05/444/p2/d4 | 0.914568 | 1.001376 | 1.150479 | 1.150200 | 1.125030 | 294484.300000 | 163744.000000 | False | inconclusive |
| kodim05/420/p0/d1 | 0.982221 | 1.003894 | 1.136258 | 1.159849 | 1.141441 | 279055.200000 | 185175.133333 | False | inconclusive |
| kodim05/420/p0/d4 | 0.953583 | 1.003839 | 1.022664 | 1.019032 | 1.020243 | 202012.300000 | 108152.000000 | True | current |
| kodim05/420/p2/d1 | 0.994658 | 1.003840 | 1.042706 | 1.044531 | 1.032614 | 422912.666667 | 246606.300000 | True | inconclusive |
| kodim05/420/p2/d4 | 1.027261 | 1.003210 | 1.023922 | 1.026051 | 1.020594 | 331180.300000 | 154776.000000 | True | baseline |
| kodim06/444/p0/d1 | 0.952141 | 1.008108 | 1.112987 | 1.025294 | 1.058713 | 231307.500000 | 164043.733333 | False | inconclusive |
| kodim06/444/p0/d4 | 0.925894 | 1.000073 | 1.014095 | 1.008041 | 1.010650 | 164480.000000 | 97184.000000 | True | current |
| kodim06/444/p2/d1 | 0.899749 | 1.065628 | 1.184023 | 1.165496 | 1.159475 | 343794.466667 | 213103.433333 | False | inconclusive |
| kodim06/444/p2/d4 | 0.844015 | 1.012314 | 1.150024 | 1.016008 | 1.135842 | 266464.000000 | 135696.000000 | False | inconclusive |
| kodim06/420/p0/d1 | 0.960135 | 1.010463 | 1.162681 | 1.071477 | 1.044341 | 243792.733333 | 149914.066667 | False | inconclusive |
| kodim06/420/p0/d4 | 0.923271 | 1.003580 | 1.024787 | 1.024546 | 1.015770 | 182120.000000 | 88264.000000 | True | current |
| kodim06/420/p2/d1 | 1.028412 | 1.017120 | 1.040916 | 1.031163 | 1.044773 | 377254.900000 | 200874.000000 | True | baseline |
| kodim06/420/p2/d4 | 0.933681 | 1.011769 | 1.163335 | 1.161863 | 1.156334 | 305500.300000 | 129064.000000 | False | inconclusive |
| kodim07/444/p0/d1 | 0.953055 | 1.025636 | 1.108891 | 1.147789 | 1.122732 | 197220.300000 | 129924.300000 | False | inconclusive |
| kodim07/444/p0/d4 | 0.918659 | 1.001895 | 1.026247 | 1.023623 | 1.018709 | 153732.300000 | 86432.000000 | True | current |
| kodim07/444/p2/d1 | 0.989023 | 1.013948 | 1.042667 | 1.034105 | 1.012893 | 323108.300000 | 192452.300000 | True | inconclusive |
| kodim07/444/p2/d4 | 0.830911 | 1.005691 | 1.019732 | 1.010154 | 1.016917 | 263052.300000 | 132312.000000 | True | current |
| kodim07/420/p0/d1 | 0.947024 | 1.012632 | 1.154578 | 1.150498 | 1.154401 | 213356.300000 | 119496.000000 | False | inconclusive |
| kodim07/420/p0/d4 | 0.925497 | 1.004339 | 1.016790 | 1.006586 | 1.012359 | 174048.000000 | 80196.300000 | True | current |
| kodim07/420/p2/d1 | 0.987292 | 1.005232 | 1.034410 | 1.013100 | 1.019908 | 342757.866667 | 166328.000000 | True | inconclusive |
| kodim07/420/p2/d4 | 0.975195 | 1.001089 | 1.063336 | 1.012130 | 1.006728 | 298584.600000 | 122148.300000 | True | inconclusive |
| kodim08/444/p0/d1 | 0.959953 | 1.014708 | 1.079828 | 1.031598 | 1.057497 | 272515.866667 | 205250.100000 | True | current |
| kodim08/444/p0/d4 | 0.945864 | 1.005588 | 1.131985 | 1.017008 | 1.014711 | 185668.300000 | 118368.000000 | False | inconclusive |
| kodim08/444/p2/d1 | 0.993488 | 1.026620 | 1.155609 | 1.075771 | 1.050580 | 389589.166667 | 258926.966667 | False | inconclusive |
| kodim08/444/p2/d4 | 0.862216 | 1.076225 | 1.180723 | 1.138381 | 1.027671 | 294024.600000 | 163280.000000 | False | inconclusive |
| kodim08/420/p0/d1 | 0.983178 | 1.000444 | 1.145081 | 1.174927 | 1.078411 | 280723.400000 | 186908.733333 | False | inconclusive |
| kodim08/420/p0/d4 | 0.966039 | 1.002272 | 1.010132 | 1.003883 | 1.008374 | 201188.300000 | 107328.000000 | True | current |
| kodim08/420/p2/d1 | 0.992896 | 1.009870 | 1.177237 | 1.114719 | 1.042644 | 418667.266667 | 242306.000000 | False | inconclusive |
| kodim08/420/p2/d4 | 1.008815 | 1.002317 | 1.015410 | 1.011605 | 1.020162 | 336576.000000 | 160212.300000 | True | inconclusive |
| kodim09/444/p0/d1 | 0.944488 | 1.009098 | 1.154712 | 1.146147 | 1.013775 | 168020.300000 | 121840.000000 | False | inconclusive |
| kodim09/444/p0/d4 | 0.913609 | 1.003237 | 1.011912 | 1.003603 | 1.015567 | 125384.000000 | 79212.300000 | True | current |
| kodim09/444/p2/d1 | 0.945419 | 1.002000 | 1.039367 | 1.014380 | 1.005483 | 279484.300000 | 171560.000000 | True | current |
| kodim09/444/p2/d4 | 0.816802 | 1.001407 | 1.149560 | 1.175693 | 1.148576 | 226724.300000 | 118740.300000 | False | inconclusive |
| kodim09/420/p0/d1 | 0.959508 | 1.001762 | 1.177795 | 1.054906 | 1.036518 | 173656.000000 | 109752.000000 | False | inconclusive |
| kodim09/420/p0/d4 | 0.926090 | 1.009658 | 1.106930 | 1.158089 | 1.168843 | 137216.000000 | 73312.000000 | False | inconclusive |
| kodim09/420/p2/d1 | 0.997021 | 1.017354 | 1.136491 | 1.165117 | 1.146460 | 304652.300000 | 162076.300000 | False | inconclusive |
| kodim09/420/p2/d4 | 0.939333 | 1.001815 | 1.046056 | 1.013015 | 1.007363 | 252116.300000 | 109456.000000 | True | current |
| kodim10/444/p0/d1 | 0.947716 | 1.015361 | 1.158977 | 1.031596 | 1.017223 | 176636.300000 | 130460.300000 | False | inconclusive |
| kodim10/444/p0/d4 | 0.917086 | 1.000900 | 1.142599 | 1.159573 | 1.153271 | 128584.000000 | 82408.000000 | False | inconclusive |
| kodim10/444/p2/d1 | 0.940004 | 1.005480 | 1.057654 | 1.080753 | 1.017598 | 288852.300000 | 180932.300000 | True | current |
| kodim10/444/p2/d4 | 0.824743 | 1.008891 | 1.048037 | 1.107100 | 1.013575 | 230468.300000 | 122480.000000 | True | current |
| kodim10/420/p0/d1 | 0.973270 | 1.002707 | 1.009023 | 1.008035 | 1.005664 | 181764.300000 | 117856.000000 | True | current |
| kodim10/420/p0/d4 | 0.930121 | 1.000628 | 1.081675 | 1.140215 | 1.145678 | 139944.000000 | 76040.000000 | True | current |
| kodim10/420/p2/d1 | 1.031993 | 1.006179 | 1.157019 | 1.008218 | 1.010845 | 313208.000000 | 170632.000000 | False | inconclusive |
| kodim10/420/p2/d4 | 0.933787 | 1.007501 | 1.029476 | 1.016259 | 1.016963 | 261060.300000 | 118420.300000 | True | current |
| kodim11/444/p0/d1 | 0.953790 | 1.009607 | 1.133799 | 1.023501 | 1.042244 | 226235.866667 | 158940.000000 | False | inconclusive |
| kodim11/444/p0/d4 | 0.928658 | 1.000976 | 1.154540 | 1.151352 | 1.150861 | 160168.000000 | 92872.000000 | False | inconclusive |
| kodim11/444/p2/d1 | 0.983368 | 1.014693 | 1.138840 | 1.047167 | 1.148719 | 339390.766667 | 208722.566667 | False | inconclusive |
| kodim11/444/p2/d4 | 0.824397 | 1.000640 | 1.059287 | 1.037639 | 1.012085 | 268728.000000 | 137992.000000 | True | current |
| kodim11/420/p0/d1 | 0.963506 | 1.024140 | 1.142503 | 1.076412 | 1.088162 | 238773.933333 | 144919.266667 | False | inconclusive |
| kodim11/420/p0/d4 | 0.934613 | 1.012098 | 1.156730 | 1.013421 | 1.012194 | 178532.300000 | 84676.300000 | False | inconclusive |
| kodim11/420/p2/d1 | 1.041634 | 1.012009 | 1.061706 | 1.018902 | 1.038308 | 372487.166667 | 196141.800000 | True | inconclusive |
| kodim11/420/p2/d4 | 0.952158 | 1.021732 | 1.080325 | 1.019685 | 1.019345 | 308352.000000 | 131971.400000 | True | current |
| kodim12/444/p0/d1 | 0.952122 | 1.034347 | 1.147290 | 1.040178 | 1.026612 | 193886.300000 | 126544.000000 | False | inconclusive |
| kodim12/444/p0/d4 | 0.916426 | 1.002072 | 1.151383 | 1.152596 | 1.153875 | 146168.000000 | 78872.000000 | False | inconclusive |
| kodim12/444/p2/d1 | 0.932503 | 1.015494 | 1.143304 | 1.053606 | 1.033407 | 301616.600000 | 170876.300000 | False | inconclusive |
| kodim12/444/p2/d4 | 0.816836 | 1.009066 | 1.034242 | 1.013588 | 1.013223 | 260840.000000 | 130136.000000 | True | current |
| kodim12/420/p0/d1 | 0.956667 | 1.002575 | 1.022476 | 1.014095 | 1.018814 | 208788.300000 | 114932.300000 | True | current |
| kodim12/420/p0/d4 | 0.911614 | 1.002447 | 1.138453 | 1.154323 | 1.145910 | 166572.300000 | 72712.000000 | False | inconclusive |
| kodim12/420/p2/d1 | 1.023967 | 1.002053 | 1.074559 | 1.014503 | 1.009796 | 337644.300000 | 161244.300000 | True | inconclusive |
| kodim12/420/p2/d4 | 0.943221 | 1.003459 | 1.170820 | 1.167883 | 1.130990 | 290740.300000 | 114304.000000 | False | inconclusive |
| kodim13/444/p0/d1 | 0.941892 | 1.051195 | 1.142054 | 1.146919 | 1.091486 | 295484.100000 | 228179.900000 | False | inconclusive |
| kodim13/444/p0/d4 | 0.951058 | 1.002496 | 1.106712 | 1.020481 | 1.124478 | 191880.000000 | 124588.300000 | False | inconclusive |
| kodim13/444/p2/d1 | 0.916012 | 1.020700 | 1.176493 | 1.059279 | 1.041339 | 410337.400000 | 279597.166667 | False | inconclusive |
| kodim13/444/p2/d4 | 0.856246 | 1.002681 | 1.132525 | 1.146430 | 1.042412 | 298040.000000 | 167308.300000 | False | inconclusive |
| kodim13/420/p0/d1 | 0.977508 | 1.011897 | 1.098927 | 1.058517 | 1.043089 | 302726.000000 | 208833.033333 | True | current |
| kodim13/420/p0/d4 | 0.954019 | 1.018522 | 1.146631 | 1.133752 | 1.129864 | 204344.000000 | 110488.000000 | False | inconclusive |
| kodim13/420/p2/d1 | 1.038371 | 1.011135 | 1.120352 | 1.060715 | 1.095370 | 438703.766667 | 262374.433333 | False | inconclusive |
| kodim13/420/p2/d4 | 0.961751 | 1.010899 | 1.129623 | 1.031142 | 1.027458 | 338452.300000 | 162080.000000 | False | inconclusive |
| kodim14/444/p0/d1 | 0.957042 | 1.001976 | 1.044850 | 1.032051 | 1.033365 | 253659.133333 | 186360.333333 | True | current |
| kodim14/444/p0/d4 | 0.936163 | 1.009838 | 1.046765 | 1.031441 | 1.036447 | 172452.300000 | 105156.300000 | True | current |
| kodim14/444/p2/d1 | 0.979723 | 1.010670 | 1.104041 | 1.147590 | 1.042383 | 358685.166667 | 227920.000000 | False | inconclusive |
| kodim14/444/p2/d4 | 0.895236 | 1.016769 | 1.164009 | 1.027360 | 1.012169 | 285988.300000 | 155284.300000 | False | inconclusive |
| kodim14/420/p0/d1 | 0.976604 | 1.032379 | 1.146724 | 1.159146 | 1.137925 | 262049.866667 | 168225.200000 | False | inconclusive |
| kodim14/420/p0/d4 | 0.944833 | 1.007849 | 1.049755 | 1.023000 | 1.014126 | 188764.300000 | 94904.000000 | True | current |
| kodim14/420/p2/d1 | 0.995340 | 1.018431 | 1.062751 | 1.016914 | 1.055062 | 388039.266667 | 211707.600000 | True | inconclusive |
| kodim14/420/p2/d4 | 0.972888 | 1.003319 | 1.029395 | 1.021792 | 1.008641 | 317704.000000 | 141304.000000 | True | current |
| kodim15/444/p0/d1 | 0.957405 | 1.015244 | 1.092754 | 1.134983 | 1.125639 | 201181.833333 | 133913.300000 | True | current |
| kodim15/444/p0/d4 | 0.923533 | 1.028237 | 1.128954 | 1.131617 | 1.137337 | 149628.300000 | 82328.000000 | False | inconclusive |
| kodim15/444/p2/d1 | 0.998586 | 1.014658 | 1.018359 | 1.147957 | 1.136282 | 326116.300000 | 195472.000000 | True | inconclusive |
| kodim15/444/p2/d4 | 0.821266 | 1.008046 | 1.021806 | 1.026492 | 1.014012 | 264764.300000 | 134056.000000 | True | current |
| kodim15/420/p0/d1 | 0.968779 | 1.042615 | 1.122803 | 1.016452 | 1.110841 | 214408.000000 | 120552.000000 | False | inconclusive |
| kodim15/420/p0/d4 | 0.926092 | 1.010900 | 1.019527 | 1.016273 | 1.021897 | 169784.000000 | 75928.000000 | True | current |
| kodim15/420/p2/d1 | 1.004082 | 1.002860 | 1.069411 | 1.018499 | 1.037306 | 343236.300000 | 166836.300000 | True | inconclusive |
| kodim15/420/p2/d4 | 0.962517 | 1.027653 | 1.152601 | 1.148538 | 1.135228 | 294400.600000 | 117960.000000 | False | inconclusive |
| kodim16/444/p0/d1 | 0.949204 | 1.019515 | 1.120188 | 1.137555 | 1.154799 | 203992.000000 | 136704.600000 | False | inconclusive |
| kodim16/444/p0/d4 | 0.917060 | 1.003407 | 1.013972 | 1.011164 | 1.014540 | 151192.000000 | 83896.000000 | True | current |
| kodim16/444/p2/d1 | 0.858903 | 1.007703 | 1.069141 | 1.021807 | 1.025360 | 317340.300000 | 186630.366667 | True | current |
| kodim16/444/p2/d4 | 0.821205 | 1.000287 | 1.039642 | 1.106345 | 1.120963 | 253572.300000 | 122804.300000 | True | current |
| kodim16/420/p0/d1 | 0.950348 | 1.006104 | 1.097206 | 1.141921 | 1.145202 | 218472.000000 | 124616.000000 | True | current |
| kodim16/420/p0/d4 | 0.913333 | 1.006235 | 1.014644 | 1.027051 | 1.020034 | 170564.300000 | 76704.000000 | True | current |
| kodim16/420/p2/d1 | 1.036405 | 1.003601 | 1.031228 | 1.026573 | 1.041020 | 347468.300000 | 171108.300000 | True | baseline |
| kodim16/420/p2/d4 | 0.904095 | 1.006904 | 1.041000 | 1.012148 | 1.050085 | 300156.300000 | 123752.000000 | True | current |
| kodim17/444/p0/d1 | 0.943607 | 1.037172 | 1.154495 | 1.157675 | 1.168745 | 181515.266667 | 135324.300000 | False | inconclusive |
| kodim17/444/p0/d4 | 0.921540 | 1.003765 | 1.007181 | 1.003303 | 1.006232 | 131360.000000 | 85184.000000 | True | current |
| kodim17/444/p2/d1 | 0.866640 | 1.006216 | 1.094570 | 1.084715 | 1.069456 | 292476.300000 | 184552.000000 | True | current |
| kodim17/444/p2/d4 | 0.834573 | 1.011600 | 1.008534 | 1.011341 | 1.012766 | 232300.300000 | 124312.000000 | True | current |
| kodim17/420/p0/d1 | 0.962836 | 1.003403 | 1.103225 | 1.010660 | 1.005561 | 186648.000000 | 122744.000000 | False | inconclusive |
| kodim17/420/p0/d4 | 0.929293 | 1.009470 | 1.047350 | 1.022332 | 1.012390 | 142048.000000 | 78148.300000 | True | current |
| kodim17/420/p2/d1 | 1.007693 | 1.022496 | 1.163456 | 1.158356 | 1.125416 | 317172.300000 | 174592.000000 | False | inconclusive |
| kodim17/420/p2/d4 | 0.923399 | 1.009257 | 1.135570 | 1.013482 | 1.055189 | 268412.300000 | 125804.300000 | False | inconclusive |
| kodim18/444/p0/d1 | 0.956606 | 1.038153 | 1.143157 | 1.071366 | 1.144352 | 226860.633333 | 180754.733333 | False | inconclusive |
| kodim18/444/p0/d4 | 0.933323 | 1.002678 | 1.021719 | 1.018312 | 1.013516 | 148396.300000 | 102216.000000 | True | current |
| kodim18/444/p2/d1 | 1.005313 | 1.004875 | 1.039115 | 1.031984 | 1.030165 | 342424.733333 | 234559.733333 | True | inconclusive |
| kodim18/444/p2/d4 | 0.851361 | 1.022378 | 1.151000 | 1.137841 | 1.130073 | 261148.300000 | 153224.000000 | False | inconclusive |
| kodim18/420/p0/d1 | 0.989363 | 1.023198 | 1.130176 | 1.039384 | 1.048053 | 227509.766667 | 163529.666667 | False | inconclusive |
| kodim18/420/p0/d4 | 0.945361 | 1.010086 | 1.017668 | 1.018060 | 1.014707 | 156336.000000 | 92436.300000 | True | current |
| kodim18/420/p2/d1 | 1.009949 | 1.004551 | 1.128708 | 1.136992 | 1.134507 | 361683.300000 | 219130.000000 | False | inconclusive |
| kodim18/420/p2/d4 | 0.984952 | 1.006349 | 1.014529 | 1.015311 | 1.013569 | 276820.300000 | 134180.300000 | True | current |
| kodim19/444/p0/d1 | 0.942933 | 1.009347 | 1.116953 | 1.034052 | 1.010197 | 196697.866667 | 150522.066667 | False | inconclusive |
| kodim19/444/p0/d4 | 0.925351 | 1.001833 | 1.032889 | 1.019694 | 1.009259 | 135432.000000 | 89256.000000 | True | current |
| kodim19/444/p2/d1 | 0.915305 | 1.007722 | 1.031104 | 1.019688 | 1.013532 | 307696.900000 | 199735.900000 | True | current |
| kodim19/444/p2/d4 | 0.833395 | 1.004453 | 1.037433 | 1.019232 | 1.017561 | 236284.300000 | 128296.000000 | True | current |
| kodim19/420/p0/d1 | 0.975735 | 1.011254 | 1.188582 | 1.143352 | 1.132444 | 199797.400000 | 135925.666667 | False | inconclusive |
| kodim19/420/p0/d4 | 0.941029 | 1.001616 | 1.055986 | 1.127242 | 1.110753 | 145532.300000 | 81624.000000 | True | current |
| kodim19/420/p2/d1 | 1.021012 | 1.009314 | 1.054880 | 1.045755 | 1.029685 | 329951.366667 | 187373.300000 | True | baseline |
| kodim19/420/p2/d4 | 0.913516 | 1.026597 | 1.041876 | 1.144184 | 1.120552 | 265644.300000 | 123000.000000 | True | current |
| kodim20/444/p0/d1 | 0.930745 | 1.000874 | 1.141787 | 1.142208 | 1.130939 | 187776.000000 | 120484.300000 | False | inconclusive |
| kodim20/444/p0/d4 | 0.904472 | 1.028218 | 1.061595 | 1.054871 | 1.027985 | 145488.000000 | 78192.000000 | True | current |
| kodim20/444/p2/d1 | 0.975080 | 1.001706 | 1.059977 | 1.123056 | 1.036959 | 300124.300000 | 169420.300000 | True | current |
| kodim20/444/p2/d4 | 0.800309 | 1.006047 | 1.052200 | 1.136623 | 1.108596 | 247340.300000 | 116568.000000 | True | current |
| kodim20/420/p0/d1 | 0.942002 | 1.012823 | 1.141312 | 1.140932 | 1.152973 | 204568.000000 | 110712.000000 | False | inconclusive |
| kodim20/420/p0/d4 | 0.915181 | 1.000070 | 1.018794 | 1.010649 | 1.026166 | 166228.300000 | 72372.300000 | True | current |
| kodim20/420/p2/d1 | 0.989748 | 1.010047 | 1.155164 | 1.150792 | 1.139515 | 337636.300000 | 161264.000000 | False | inconclusive |
| kodim20/420/p2/d4 | 0.946001 | 1.030468 | 1.153840 | 1.145255 | 1.161699 | 289500.300000 | 113085.600000 | False | inconclusive |
| kodim21/444/p0/d1 | 0.942043 | 1.004227 | 1.150369 | 1.143563 | 1.133134 | 220434.333333 | 153096.000000 | False | inconclusive |
| kodim21/444/p0/d4 | 0.919420 | 1.007934 | 1.186813 | 1.147347 | 1.144592 | 158752.000000 | 91456.000000 | False | inconclusive |
| kodim21/444/p2/d1 | 0.894604 | 1.018441 | 1.121025 | 1.158625 | 1.175499 | 331894.500000 | 201179.400000 | False | inconclusive |
| kodim21/444/p2/d4 | 0.820832 | 1.002722 | 1.051358 | 1.138721 | 1.111384 | 260660.300000 | 129892.300000 | True | current |
| kodim21/420/p0/d1 | 0.970436 | 1.018798 | 1.131019 | 1.026489 | 1.082756 | 231977.800000 | 138149.800000 | False | inconclusive |
| kodim21/420/p0/d4 | 0.933761 | 1.000641 | 1.032773 | 1.021839 | 1.018934 | 177464.000000 | 83608.000000 | True | current |
| kodim21/420/p2/d1 | 1.019549 | 1.009123 | 1.157671 | 1.026750 | 1.092131 | 365004.300000 | 188666.266667 | False | inconclusive |
| kodim21/420/p2/d4 | 0.925074 | 1.005762 | 1.128909 | 1.125314 | 1.065723 | 300648.000000 | 124216.000000 | False | inconclusive |
| kodim22/444/p0/d1 | 0.953098 | 1.011525 | 1.172368 | 1.149548 | 1.161076 | 228464.600000 | 161236.000000 | False | inconclusive |
| kodim22/444/p0/d4 | 0.923899 | 1.036395 | 1.147820 | 1.152053 | 1.149751 | 158768.000000 | 91476.300000 | False | inconclusive |
| kodim22/444/p2/d1 | 0.994393 | 1.004149 | 1.040006 | 1.042736 | 1.076225 | 340520.233333 | 209769.700000 | True | inconclusive |
| kodim22/444/p2/d4 | 0.845082 | 1.000439 | 1.144597 | 1.011142 | 1.038015 | 267564.300000 | 136873.300000 | False | inconclusive |
| kodim22/420/p0/d1 | 0.990607 | 1.005646 | 1.186081 | 1.154422 | 1.132263 | 238697.733333 | 144814.000000 | False | inconclusive |
| kodim22/420/p0/d4 | 0.941617 | 1.020881 | 1.147829 | 1.089243 | 1.037206 | 176952.000000 | 83096.000000 | False | inconclusive |
| kodim22/420/p2/d1 | 1.026247 | 1.004451 | 1.048917 | 1.040870 | 1.026304 | 371889.500000 | 195432.000000 | True | baseline |
| kodim22/420/p2/d4 | 0.962440 | 1.007595 | 1.147003 | 1.043469 | 1.066997 | 306952.000000 | 130552.000000 | False | inconclusive |
| kodim23/444/p0/d1 | 0.945664 | 1.000196 | 1.046904 | 1.018253 | 1.021100 | 183020.300000 | 115720.000000 | True | current |
| kodim23/444/p0/d4 | 0.912165 | 1.016999 | 1.039114 | 1.122661 | 1.128473 | 144896.000000 | 77604.300000 | True | current |
| kodim23/444/p2/d1 | 0.943105 | 1.021488 | 1.115354 | 1.151702 | 1.127018 | 297228.300000 | 166520.000000 | False | inconclusive |
| kodim23/444/p2/d4 | 0.795754 | 1.001522 | 1.120385 | 1.163776 | 1.138995 | 254504.000000 | 123772.300000 | False | inconclusive |
| kodim23/420/p0/d1 | 0.960923 | 1.036677 | 1.154211 | 1.060165 | 1.030804 | 197612.300000 | 103752.000000 | False | inconclusive |
| kodim23/420/p0/d4 | 0.917862 | 1.001607 | 1.058017 | 1.017408 | 1.023690 | 165912.000000 | 72056.000000 | True | current |
| kodim23/420/p2/d1 | 1.000080 | 1.028265 | 1.105807 | 1.017219 | 1.145019 | 315324.300000 | 138876.300000 | False | inconclusive |
| kodim23/420/p2/d4 | 0.926681 | 1.000527 | 1.024536 | 1.014667 | 1.020036 | 279068.300000 | 102752.000000 | True | current |
| kodim24/444/p0/d1 | 0.957624 | 1.025097 | 1.110357 | 1.037254 | 1.047668 | 245397.566667 | 178140.100000 | False | inconclusive |
| kodim24/444/p0/d4 | 0.943896 | 1.007731 | 1.033231 | 1.128126 | 1.115139 | 171088.000000 | 103796.300000 | True | current |
| kodim24/444/p2/d1 | 0.995123 | 1.006541 | 1.105359 | 1.037277 | 1.032349 | 357350.766667 | 226643.400000 | False | inconclusive |
| kodim24/444/p2/d4 | 0.847432 | 1.003726 | 1.020903 | 1.012337 | 1.012330 | 279276.300000 | 148540.300000 | True | current |
| kodim24/420/p0/d1 | 0.982876 | 1.003974 | 1.084899 | 1.010394 | 1.008237 | 255637.466667 | 161790.366667 | True | inconclusive |
| kodim24/420/p0/d4 | 0.951146 | 1.003640 | 1.023237 | 1.009397 | 1.007013 | 187692.966667 | 93792.000000 | True | current |
| kodim24/420/p2/d1 | 1.031782 | 1.005435 | 1.062672 | 1.127326 | 1.095624 | 382868.433333 | 206442.000000 | True | inconclusive |
| kodim24/420/p2/d4 | 0.966303 | 1.003499 | 1.133613 | 1.120519 | 1.115886 | 317508.300000 | 141108.300000 | False | inconclusive |
| smooth-gradient/444/p0/d1 | 0.825341 | 1.003965 | 1.036164 | 1.009275 | 1.007096 | 76384.000000 | 50664.000000 | True | current |
| smooth-gradient/444/p0/d4 | 0.795566 | 1.003390 | 1.072661 | 1.209807 | 1.149295 | 69512.000000 | 43776.000000 | False | inconclusive |
| smooth-gradient/444/p2/d1 | 0.679645 | 1.006573 | 1.071189 | 1.076897 | 1.062557 | 171304.000000 | 85324.300000 | True | current |
| smooth-gradient/444/p2/d4 | 0.629970 | 1.020352 | 1.107155 | 1.140255 | 1.118343 | 158852.300000 | 72984.000000 | False | inconclusive |
| smooth-gradient/420/p0/d1 | 0.833091 | 1.006105 | 1.016109 | 1.011940 | 1.005942 | 84784.000000 | 49896.000000 | True | current |
| smooth-gradient/420/p0/d4 | 0.825150 | 1.025889 | 1.048025 | 1.009988 | 1.041919 | 84056.000000 | 49168.000000 | True | current |
| smooth-gradient/420/p2/d1 | 0.691219 | 1.014452 | 1.045600 | 1.032880 | 1.010517 | 190228.300000 | 80440.000000 | True | current |
| smooth-gradient/420/p2/d4 | 0.692994 | 1.025252 | 1.073188 | 1.037907 | 1.018215 | 183904.000000 | 74108.300000 | True | current |
| low-contrast/444/p0/d1 | 0.868028 | 1.004103 | 1.029955 | 1.009480 | 1.008719 | 87224.000000 | 61504.000000 | True | current |
| low-contrast/444/p0/d4 | 0.813846 | 1.026473 | 1.171797 | 1.080723 | 1.084200 | 77208.000000 | 51488.000000 | False | inconclusive |
| low-contrast/444/p2/d1 | 0.828443 | 1.011735 | 1.095748 | 1.120331 | 1.124714 | 216688.000000 | 130868.300000 | True | current |
| low-contrast/444/p2/d4 | 0.734232 | 1.013165 | 1.014861 | 1.021890 | 1.007793 | 190108.300000 | 104200.000000 | True | current |
| low-contrast/420/p0/d1 | 0.884778 | 1.013748 | 1.052570 | 1.010917 | 1.018008 | 93088.000000 | 58200.000000 | True | current |
| low-contrast/420/p0/d4 | 0.826934 | 1.006942 | 1.010825 | 1.008467 | 1.014707 | 85304.000000 | 50416.000000 | True | current |
| low-contrast/420/p2/d1 | 0.859395 | 1.002946 | 1.046039 | 1.035643 | 1.006009 | 221628.300000 | 111788.300000 | True | current |
| low-contrast/420/p2/d4 | 0.740146 | 1.007471 | 1.077241 | 1.029920 | 1.051256 | 202628.300000 | 92720.000000 | True | current |
| color-edges/444/p0/d1 | 0.836213 | 1.011995 | 1.009525 | 1.009252 | 1.011798 | 94256.000000 | 68520.000000 | True | current |
| color-edges/444/p0/d4 | 0.815213 | 1.002807 | 1.037739 | 1.018398 | 1.010899 | 84084.300000 | 58348.300000 | True | current |
| color-edges/444/p2/d1 | 0.873560 | 1.002822 | 1.040954 | 1.019432 | 1.016601 | 206600.000000 | 120680.000000 | True | current |
| color-edges/444/p2/d4 | 0.866058 | 1.017453 | 1.033913 | 1.031915 | 1.017260 | 191104.000000 | 105152.000000 | True | current |
| color-edges/420/p0/d1 | 0.871144 | 1.008760 | 1.013147 | 1.010609 | 1.015667 | 104168.000000 | 69280.000000 | True | current |
| color-edges/420/p0/d4 | 0.859852 | 1.008779 | 1.012987 | 1.008793 | 1.015708 | 90040.000000 | 55136.000000 | True | current |
| color-edges/420/p2/d1 | 0.872880 | 1.003900 | 1.011408 | 1.013952 | 1.002636 | 220872.000000 | 110984.000000 | True | current |
| color-edges/420/p2/d4 | 0.852266 | 1.003605 | 1.053179 | 1.007505 | 1.032685 | 218472.000000 | 108620.300000 | True | current |
| line-art/444/p0/d1 | 0.804821 | 1.001587 | 1.044881 | 1.013765 | 1.028370 | 94812.300000 | 69088.000000 | True | current |
| line-art/444/p0/d4 | 0.783475 | 1.004102 | 1.012565 | 1.006546 | 1.009581 | 87752.000000 | 62032.000000 | True | current |
| line-art/444/p2/d1 | 0.803031 | 1.004891 | 1.022754 | 1.025393 | 1.005610 | 195332.300000 | 109364.300000 | True | current |
| line-art/444/p2/d4 | 0.784315 | 1.000338 | 1.025538 | 1.022692 | 1.004738 | 182520.000000 | 96536.000000 | True | current |
| line-art/420/p0/d1 | 0.852120 | 1.007193 | 1.884852 | 1.012887 | 1.017657 | 102668.300000 | 67817.800000 | False | inconclusive |
| line-art/420/p0/d4 | 0.830492 | 1.000245 | 1.085947 | 1.014318 | 1.023497 | 95304.000000 | 60416.000000 | True | current |
| line-art/420/p2/d1 | 0.846671 | 1.016992 | 1.072852 | 1.047579 | 1.027496 | 213748.300000 | 103828.300000 | True | current |
| line-art/420/p2/d4 | 0.823170 | 1.011053 | 1.023669 | 1.011081 | 1.012451 | 206428.300000 | 96504.000000 | True | current |
| structured-texture/444/p0/d1 | 0.931347 | 1.004212 | 1.009621 | 1.008703 | 1.009343 | 117968.000000 | 92232.000000 | True | current |
| structured-texture/444/p0/d4 | 0.909836 | 1.003275 | 1.031368 | 1.003899 | 1.010553 | 88128.000000 | 62396.300000 | True | current |
| structured-texture/444/p2/d1 | 0.951432 | 1.008783 | 1.051571 | 1.021485 | 1.011402 | 248284.300000 | 162456.000000 | True | current |
| structured-texture/444/p2/d4 | 0.920920 | 1.011783 | 1.131721 | 1.130062 | 1.132989 | 195044.300000 | 109092.300000 | False | inconclusive |
| structured-texture/420/p0/d1 | 0.914669 | 1.000563 | 1.056040 | 1.014536 | 1.044908 | 108744.000000 | 73840.000000 | True | current |
| structured-texture/420/p0/d4 | 0.901131 | 1.017482 | 1.052203 | 1.025275 | 1.033768 | 89836.300000 | 54928.000000 | True | current |
| structured-texture/420/p2/d1 | 0.940471 | 1.001179 | 1.011207 | 1.005466 | 1.007547 | 232176.000000 | 122288.000000 | True | current |
| structured-texture/420/p2/d4 | 0.905319 | 1.004938 | 1.043272 | 1.018382 | 1.011386 | 201268.300000 | 91484.300000 | True | current |
| low-noise/444/p0/d1 | 0.929760 | 1.002918 | 1.083711 | 1.039373 | 1.018431 | 89964.300000 | 64240.000000 | True | current |
| low-noise/444/p0/d4 | 0.807098 | 1.000005 | 1.075577 | 1.126388 | 1.133864 | 75312.000000 | 49592.000000 | True | current |
| low-noise/444/p2/d1 | 0.843097 | 1.002491 | 1.037490 | 1.039990 | 1.014390 | 196956.300000 | 111016.000000 | True | current |
| low-noise/444/p2/d4 | 0.663195 | 1.007534 | 1.029776 | 1.034865 | 1.011687 | 165040.000000 | 79192.000000 | True | current |
| low-noise/420/p0/d1 | 0.909373 | 1.027875 | 1.026428 | 1.033276 | 1.016512 | 88664.000000 | 53760.000000 | True | current |
| low-noise/420/p0/d4 | 0.810432 | 1.005893 | 1.056133 | 1.010210 | 1.017782 | 78052.300000 | 43144.000000 | True | current |
| low-noise/420/p2/d1 | 0.864990 | 1.005793 | 1.027472 | 1.011614 | 1.029035 | 211440.000000 | 101540.300000 | True | current |
| low-noise/420/p2/d4 | 0.720670 | 1.000527 | 1.041665 | 1.014367 | 1.020282 | 189636.300000 | 79848.000000 | True | current |
| full-noise/444/p0/d1 | 0.949217 | 1.000534 | 1.028599 | 1.032110 | 1.043894 | 135788.300000 | 110064.000000 | True | current |
| full-noise/444/p0/d4 | 0.906711 | 1.001898 | 1.023926 | 1.010156 | 1.009015 | 103656.000000 | 77940.300000 | True | current |
| full-noise/444/p2/d1 | 0.962501 | 1.007581 | 1.085967 | 1.062046 | 1.049022 | 277676.300000 | 191928.000000 | True | current |
| full-noise/444/p2/d4 | 0.943923 | 1.004931 | 1.028140 | 1.021219 | 1.017936 | 216220.300000 | 130316.300000 | True | current |
| full-noise/420/p0/d1 | 0.917246 | 1.006786 | 1.030586 | 1.016782 | 1.014230 | 116392.000000 | 81488.000000 | True | current |
| full-noise/420/p0/d4 | 0.894379 | 1.009662 | 1.042992 | 1.085495 | 1.055922 | 96352.000000 | 61448.000000 | True | current |
| full-noise/420/p2/d1 | 0.944533 | 1.008392 | 1.036769 | 1.008646 | 1.052266 | 245640.000000 | 135788.300000 | True | inconclusive |
| full-noise/420/p2/d4 | 0.892487 | 1.009675 | 1.111621 | 1.141315 | 1.146349 | 219220.300000 | 109328.000000 | False | inconclusive |
| tiny-odd-edges/444/p0/d1 | 0.607719 | 1.044526 | 1.119730 | 1.116112 | 1.132650 | 42712.860000 | 36760.860000 | False | inconclusive |
| tiny-odd-edges/444/p0/d4 | 0.541097 | 1.002948 | 1.061355 | 1.030960 | 1.039105 | 42416.860000 | 36464.000000 | True | current |
| tiny-odd-edges/444/p2/d1 | 0.521355 | 1.038120 | 1.052341 | 1.061839 | 1.107678 | 131000.000000 | 64408.000000 | True | current |
| tiny-odd-edges/444/p2/d4 | 0.481159 | 1.020519 | 1.054800 | 1.040607 | 1.034252 | 130673.720000 | 64080.860000 | True | current |
| tiny-odd-edges/420/p0/d1 | 0.632218 | 1.048743 | 1.086328 | 1.077029 | 1.042549 | 43408.860000 | 36568.860000 | True | current |
| tiny-odd-edges/420/p0/d4 | 0.553458 | 1.005838 | 1.061662 | 1.018174 | 1.028629 | 43232.860000 | 36392.000000 | True | current |
| tiny-odd-edges/420/p2/d1 | 0.497930 | 1.031907 | 1.055403 | 1.040433 | 1.027355 | 145704.860000 | 65496.860000 | True | current |
| tiny-odd-edges/420/p2/d4 | 0.474381 | 1.011567 | 1.066954 | 1.038703 | 1.057820 | 145473.720000 | 65264.860000 | True | current |

Invalid evidence:

- kodim01/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim01/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim01/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim02/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim03/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim04/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim04/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim04/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim04/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim05/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim05/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim05/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim05/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim05/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim05/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim06/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim06/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim06/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim06/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim06/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim06/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim07/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim07/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim07/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim08/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim08/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim08/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim08/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim08/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim08/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim08/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim08/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim09/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim09/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim09/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim09/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim09/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim09/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim09/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim09/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim10/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim10/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim10/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim11/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim11/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim11/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim11/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim11/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim11/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim11/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim11/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim11/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim12/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim12/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim12/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim12/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim12/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim12/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim13/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim13/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim13/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim13/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim13/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim13/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim13/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim13/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim13/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim13/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim13/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim14/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim14/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim14/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim15/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim15/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim15/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim15/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim16/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim17/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim17/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim17/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim17/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim17/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim18/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim18/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim18/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim18/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim18/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim19/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim19/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim19/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim20/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim20/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim20/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim20/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim20/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim20/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim20/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim21/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim21/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim21/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim21/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim21/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim21/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim21/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim21/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim21/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim21/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim22/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim22/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim22/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim22/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim22/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim22/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim22/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim22/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim22/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim23/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim23/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim23/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim23/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim23/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim24/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim24/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim24/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim24/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- smooth-gradient/444/p0/d4: baseline: p90/p10 drift exceeds20%, including minority timing states.
- smooth-gradient/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- low-contrast/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- low-contrast/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- line-art/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- structured-texture/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- full-noise/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1: current: repeated legs within a quartet differ by over10%.
