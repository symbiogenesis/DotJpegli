# Interleaved revision comparison

Persistent baseline and current workers run serially as balanced ABBA/BAAB quartets with identical fixed iterations, warmups, runtime, CPU affinity, encoding policy, inputs and JPEG hashes. Startup and IPC are outside encode timing. Ratios below1 favor current. Allocations are mean bytes per encode; retained rental capacity and peak working memory are separate measurements.

MAD, repeated-leg drift, p90/p10 minority-state drift and order bias guard the timing evidence. Invalid rows retain raw measurements and cannot support a speed claim. Valid rows remain inconclusive unless every quartet differs by more than 0.5% in the same direction. This comparison has no native parity verdict.

Release comparison: 256/256 cases against published 1.0.0 source `a40868bfdace7d737413085522a88d351aa16782`, rebuilt with the candidate SDK. 55 valid cases consistently favor 1.0.0 and fail the regression gate. Invalid or missing evidence fails validation separately. Inconclusive differences are retained and do not prove equal performance. The gate checks latency; allocation counts are reported separately.

| Case | Current/baseline | Order bias | Maximum repeat drift | Baseline p90/p10 | Current p90/p10 | Baseline B/encode | Current B/encode | Valid | Preference |
| --- | ---: | ---: | ---: | ---: | ---: | ---: | ---: | --- | --- |
| kodim01/444/p0/d1 | 1.030076 | 1.013149 | 1.329040 | 1.094324 | 1.044001 | 262889.333333 | 195573.100000 | False | inconclusive |
| kodim01/444/p0/d4 | 1.071839 | 1.017697 | 1.072428 | 1.065774 | 1.050086 | 177256.600000 | 109977.400000 | True | baseline |
| kodim01/444/p2/d1 | 0.943491 | 1.017253 | 1.095017 | 1.043201 | 1.051327 | 385592.200000 | 254957.800000 | True | current |
| kodim01/444/p2/d4 | 0.860026 | 1.004665 | 1.180487 | 1.040096 | 1.083942 | 296569.200000 | 165892.900000 | False | inconclusive |
| kodim01/420/p0/d1 | 1.076686 | 1.003231 | 1.034673 | 1.029801 | 1.043436 | 271720.600000 | 177872.833333 | True | baseline |
| kodim01/420/p0/d4 | 1.075383 | 1.002496 | 1.405994 | 1.102185 | 1.039217 | 192488.600000 | 98632.600000 | False | inconclusive |
| kodim01/420/p2/d1 | 1.009328 | 1.006626 | 1.196092 | 1.026689 | 1.035800 | 404404.066667 | 228032.666667 | False | inconclusive |
| kodim01/420/p2/d4 | 0.975808 | 1.002298 | 1.049891 | 1.087354 | 1.021478 | 327404.900000 | 151020.900000 | True | current |
| kodim02/444/p0/d1 | 1.101667 | 1.004975 | 1.210905 | 1.034354 | 1.048995 | 210376.600000 | 143128.966667 | False | inconclusive |
| kodim02/444/p0/d4 | 1.063078 | 1.016695 | 1.215591 | 1.031304 | 1.164796 | 148220.900000 | 80920.600000 | False | inconclusive |
| kodim02/444/p2/d1 | 0.946257 | 1.006144 | 1.108434 | 1.039134 | 1.039957 | 353112.033333 | 222528.700000 | False | inconclusive |
| kodim02/444/p2/d4 | 0.825412 | 1.019094 | 1.351087 | 1.224620 | 1.038065 | 269677.500000 | 138992.600000 | False | inconclusive |
| kodim02/420/p0/d1 | 1.092412 | 1.003985 | 1.139654 | 1.049743 | 1.024333 | 216104.600000 | 122253.766667 | False | inconclusive |
| kodim02/420/p0/d4 | 1.019651 | 1.082632 | 1.748501 | 1.314309 | 1.036130 | 162232.600000 | 68356.300000 | False | inconclusive |
| kodim02/420/p2/d1 | 1.033033 | 1.000787 | 1.037310 | 1.028364 | 1.042418 | 362049.800000 | 185716.900000 | True | baseline |
| kodim02/420/p2/d4 | 0.959844 | 1.002476 | 1.043026 | 1.041917 | 1.033430 | 292965.500000 | 116536.600000 | True | current |
| kodim03/444/p0/d1 | 1.053768 | 1.009553 | 1.044389 | 1.051733 | 1.047415 | 185217.100000 | 117908.300000 | True | baseline |
| kodim03/444/p0/d4 | 1.039234 | 1.013935 | 1.219817 | 1.022861 | 1.028846 | 145680.600000 | 78380.300000 | False | inconclusive |
| kodim03/444/p2/d1 | 0.954705 | 1.025962 | 1.076923 | 1.023832 | 1.050802 | 294049.200000 | 163310.300000 | True | current |
| kodim03/444/p2/d4 | 0.836221 | 1.011663 | 1.289462 | 1.077802 | 1.048657 | 249073.200000 | 118292.300000 | False | inconclusive |
| kodim03/420/p0/d1 | 1.059837 | 1.067725 | 1.462021 | 1.148105 | 1.123616 | 202088.600000 | 108228.300000 | False | inconclusive |
| kodim03/420/p0/d4 | 1.031805 | 1.005506 | 1.129909 | 1.210860 | 1.048093 | 166364.300000 | 72508.300000 | False | inconclusive |
| kodim03/420/p2/d1 | 0.999709 | 1.008600 | 1.060378 | 1.046654 | 1.039807 | 331632.000000 | 155244.900000 | True | inconclusive |
| kodim03/420/p2/d4 | 0.949774 | 1.014758 | 1.043867 | 1.043973 | 1.073429 | 290981.500000 | 114536.600000 | True | current |
| kodim04/444/p0/d1 | 1.068726 | 1.002443 | 1.054266 | 1.039979 | 1.037852 | 189913.900000 | 143738.833333 | True | baseline |
| kodim04/444/p0/d4 | 1.032299 | 1.003943 | 1.175483 | 1.061131 | 1.033680 | 131016.933333 | 84808.600000 | False | inconclusive |
| kodim04/444/p2/d1 | 0.930888 | 1.147040 | 1.322430 | 1.290947 | 1.057428 | 312165.200000 | 204290.400000 | False | inconclusive |
| kodim04/444/p2/d4 | 0.830609 | 1.003914 | 1.041393 | 1.034762 | 1.045265 | 244420.900000 | 136492.300000 | True | current |
| kodim04/420/p0/d1 | 1.086562 | 1.008080 | 1.036406 | 1.051514 | 1.021486 | 193112.466667 | 129148.300000 | True | baseline |
| kodim04/420/p0/d4 | 1.044634 | 1.009950 | 1.045678 | 1.037433 | 1.049519 | 141608.600000 | 77700.300000 | True | baseline |
| kodim04/420/p2/d1 | 1.021718 | 1.004452 | 1.146947 | 1.035745 | 1.124326 | 323057.200000 | 180472.600000 | False | inconclusive |
| kodim04/420/p2/d4 | 0.948588 | 1.005866 | 1.219085 | 1.017747 | 1.096821 | 268188.900000 | 125576.600000 | False | inconclusive |
| kodim05/444/p0/d1 | 1.071796 | 1.015923 | 1.059847 | 1.025060 | 1.054427 | 269314.066667 | 202040.000000 | True | baseline |
| kodim05/444/p0/d4 | 1.067573 | 1.002411 | 1.025814 | 1.038029 | 1.024574 | 186404.900000 | 119121.533333 | True | baseline |
| kodim05/444/p2/d1 | 0.999993 | 1.005453 | 1.398015 | 1.040916 | 1.035079 | 386927.800000 | 256201.300000 | False | inconclusive |
| kodim05/444/p2/d4 | 0.916345 | 1.003009 | 1.175830 | 1.020632 | 1.044872 | 294501.500000 | 163748.300000 | False | inconclusive |
| kodim05/420/p0/d1 | 1.078493 | 1.012788 | 1.050466 | 1.026398 | 1.023478 | 279039.766667 | 185186.200000 | True | baseline |
| kodim05/420/p0/d4 | 1.063858 | 1.011683 | 1.036580 | 1.055629 | 1.049458 | 202016.600000 | 108156.300000 | True | baseline |
| kodim05/420/p2/d1 | 1.013260 | 1.031101 | 1.084456 | 1.053552 | 1.058462 | 422926.366667 | 246634.066667 | True | inconclusive |
| kodim05/420/p2/d4 | 1.028549 | 1.010556 | 1.130393 | 1.057154 | 1.085368 | 331176.000000 | 154784.600000 | False | inconclusive |
| kodim06/444/p0/d1 | 1.055718 | 1.025058 | 1.516818 | 1.050199 | 1.055320 | 231358.333333 | 164016.666667 | False | inconclusive |
| kodim06/444/p0/d4 | 1.042576 | 1.008702 | 1.035336 | 1.021737 | 1.014405 | 164484.300000 | 97188.300000 | True | baseline |
| kodim06/444/p2/d1 | 0.890895 | 1.011837 | 1.186371 | 1.045120 | 1.041690 | 343813.833333 | 213083.366667 | False | inconclusive |
| kodim06/444/p2/d4 | 0.823771 | 1.014071 | 1.181190 | 1.045195 | 1.185040 | 266481.200000 | 135708.900000 | False | inconclusive |
| kodim06/420/p0/d1 | 1.060439 | 1.029960 | 1.066628 | 1.027711 | 1.060840 | 243787.600000 | 149923.966667 | True | baseline |
| kodim06/420/p0/d4 | 1.038788 | 1.000882 | 1.017090 | 1.062919 | 1.050742 | 182137.200000 | 88268.300000 | True | baseline |
| kodim06/420/p2/d1 | 0.994923 | 1.005937 | 1.075829 | 1.034797 | 1.036322 | 377244.066667 | 200885.466667 | True | inconclusive |
| kodim06/420/p2/d4 | 0.932419 | 1.010901 | 1.048808 | 1.047994 | 1.048121 | 305513.200000 | 129068.300000 | True | current |
| kodim07/444/p0/d1 | 1.080581 | 1.053034 | 1.383167 | 1.014241 | 1.086084 | 197228.900000 | 129932.900000 | False | inconclusive |
| kodim07/444/p0/d4 | 1.044999 | 1.002890 | 1.396909 | 1.223375 | 1.048354 | 153736.600000 | 86436.300000 | False | inconclusive |
| kodim07/444/p2/d1 | 0.984093 | 1.008978 | 1.077562 | 1.037160 | 1.018855 | 323116.900000 | 192465.200000 | True | inconclusive |
| kodim07/444/p2/d4 | 0.820705 | 1.009103 | 1.030330 | 1.021173 | 1.024842 | 263060.900000 | 132320.600000 | True | current |
| kodim07/420/p0/d1 | 1.061966 | 1.012340 | 1.027246 | 1.022672 | 1.036016 | 213369.200000 | 119500.300000 | True | baseline |
| kodim07/420/p0/d4 | 1.040670 | 1.006379 | 1.191705 | 1.042991 | 1.046912 | 174056.600000 | 80200.600000 | False | inconclusive |
| kodim07/420/p2/d1 | 1.003524 | 1.063379 | 1.291110 | 1.035840 | 1.092149 | 342749.500000 | 166340.900000 | False | inconclusive |
| kodim07/420/p2/d4 | 0.984083 | 1.017053 | 1.021708 | 1.025334 | 1.017216 | 298593.200000 | 122152.600000 | True | current |
| kodim08/444/p0/d1 | 1.064991 | 1.024586 | 1.046403 | 1.034366 | 1.063807 | 272518.866667 | 205206.500000 | True | baseline |
| kodim08/444/p0/d4 | 1.066227 | 1.035326 | 1.062660 | 1.050969 | 1.035830 | 185672.600000 | 118372.300000 | True | baseline |
| kodim08/444/p2/d1 | 1.001208 | 1.017712 | 1.172615 | 1.045125 | 1.048308 | 389607.766667 | 258939.866667 | False | inconclusive |
| kodim08/444/p2/d4 | 0.857860 | 1.017447 | 1.315691 | 1.049801 | 1.041740 | 294033.200000 | 163292.900000 | False | inconclusive |
| kodim08/420/p0/d1 | 1.084662 | 1.016502 | 1.277846 | 1.061731 | 1.064332 | 280717.000000 | 186841.933333 | False | inconclusive |
| kodim08/420/p0/d4 | 1.073384 | 1.003493 | 1.045674 | 1.030864 | 1.044978 | 201196.900000 | 107336.600000 | True | baseline |
| kodim08/420/p2/d1 | 1.003762 | 1.022427 | 1.146928 | 1.058872 | 1.043363 | 418672.633333 | 242289.000000 | False | inconclusive |
| kodim08/420/p2/d4 | 0.987411 | 1.007582 | 1.032882 | 1.060012 | 1.040459 | 336601.800000 | 160232.600000 | True | inconclusive |
| kodim09/444/p0/d1 | 1.072519 | 1.008795 | 1.052006 | 1.039661 | 1.040791 | 168016.000000 | 121848.600000 | True | baseline |
| kodim09/444/p0/d4 | 1.039611 | 1.006134 | 1.572782 | 1.065672 | 1.055759 | 125388.300000 | 79216.600000 | False | inconclusive |
| kodim09/444/p2/d1 | 0.934010 | 1.008285 | 1.349939 | 1.128705 | 1.034107 | 279497.200000 | 171572.900000 | False | inconclusive |
| kodim09/444/p2/d4 | 0.806750 | 1.000467 | 1.049503 | 1.021125 | 1.039232 | 226737.200000 | 118744.600000 | True | current |
| kodim09/420/p0/d1 | 1.071526 | 1.001857 | 1.076939 | 1.038546 | 1.053310 | 173668.900000 | 109752.000000 | True | baseline |
| kodim09/420/p0/d4 | 1.029159 | 1.007541 | 1.038211 | 1.032313 | 1.026029 | 137242.100000 | 73316.300000 | True | baseline |
| kodim09/420/p2/d1 | 0.980340 | 1.037831 | 1.413807 | 1.096015 | 1.025125 | 304665.200000 | 162120.933333 | False | inconclusive |
| kodim09/420/p2/d4 | 0.951574 | 1.025253 | 1.273721 | 1.024229 | 1.111566 | 252129.200000 | 109464.600000 | False | inconclusive |
| kodim10/444/p0/d1 | 1.067073 | 1.066126 | 1.499774 | 1.054388 | 1.080665 | 176640.600000 | 130468.900000 | False | inconclusive |
| kodim10/444/p0/d4 | 1.041482 | 1.014877 | 1.025355 | 1.018955 | 1.013984 | 128592.600000 | 82408.000000 | True | baseline |
| kodim10/444/p2/d1 | 0.941092 | 1.007476 | 1.059476 | 1.026803 | 1.023272 | 288860.900000 | 180940.900000 | True | current |
| kodim10/444/p2/d4 | 0.819506 | 1.018837 | 1.086176 | 1.016818 | 1.022070 | 230476.900000 | 122488.600000 | True | current |
| kodim10/420/p0/d1 | 1.072913 | 1.001425 | 1.170519 | 1.040950 | 1.038235 | 181772.900000 | 117868.900000 | False | inconclusive |
| kodim10/420/p0/d4 | 1.031705 | 1.027148 | 1.206663 | 1.055987 | 1.040934 | 139948.300000 | 76040.000000 | False | inconclusive |
| kodim10/420/p2/d1 | 1.007777 | 1.020257 | 1.037490 | 1.034810 | 1.035107 | 313229.500000 | 170632.000000 | True | inconclusive |
| kodim10/420/p2/d4 | 0.929282 | 1.003812 | 1.025241 | 1.051042 | 1.027056 | 261068.900000 | 118424.600000 | True | current |
| kodim11/444/p0/d1 | 1.083375 | 1.009058 | 1.671006 | 1.037797 | 1.052641 | 226241.033333 | 158944.300000 | False | inconclusive |
| kodim11/444/p0/d4 | 1.051065 | 1.011975 | 1.048305 | 1.048272 | 1.039210 | 160180.900000 | 92880.600000 | True | baseline |
| kodim11/444/p2/d1 | 0.984525 | 1.011243 | 1.081846 | 1.035765 | 1.066965 | 339387.866667 | 208691.766667 | True | inconclusive |
| kodim11/444/p2/d4 | 0.830557 | 1.009678 | 1.297463 | 1.050429 | 1.197853 | 268749.500000 | 137996.300000 | False | inconclusive |
| kodim11/420/p0/d1 | 1.069872 | 1.008526 | 1.151589 | 1.070754 | 1.105792 | 238788.266667 | 144926.366667 | False | inconclusive |
| kodim11/420/p0/d4 | 1.051409 | 1.010386 | 1.031430 | 1.056591 | 1.057024 | 178536.600000 | 84676.300000 | True | baseline |
| kodim11/420/p2/d1 | 1.004772 | 1.024117 | 1.052465 | 1.045820 | 1.055011 | 372534.033333 | 196139.833333 | True | inconclusive |
| kodim11/420/p2/d4 | 0.939162 | 1.020005 | 1.072384 | 1.054169 | 1.037435 | 308373.500000 | 131964.900000 | True | current |
| kodim12/444/p0/d1 | 1.067103 | 1.003163 | 1.026183 | 1.016821 | 1.013016 | 193852.900000 | 126548.300000 | True | baseline |
| kodim12/444/p0/d4 | 1.040972 | 1.002642 | 1.068342 | 1.051541 | 1.021994 | 146176.600000 | 78876.300000 | True | baseline |
| kodim12/444/p2/d1 | 0.941597 | 1.005058 | 1.044423 | 1.065407 | 1.022770 | 301625.200000 | 170876.300000 | True | current |
| kodim12/444/p2/d4 | 0.802723 | 1.009898 | 1.333744 | 1.024491 | 1.046486 | 260857.200000 | 130140.300000 | False | inconclusive |
| kodim12/420/p0/d1 | 1.069797 | 1.003761 | 1.373433 | 1.039295 | 1.089408 | 208796.900000 | 114932.300000 | False | inconclusive |
| kodim12/420/p0/d4 | 1.039266 | 1.005971 | 1.041028 | 1.057309 | 1.046735 | 166580.900000 | 72716.300000 | True | baseline |
| kodim12/420/p2/d1 | 0.998391 | 1.005435 | 1.414489 | 1.016549 | 1.022153 | 337665.800000 | 161244.300000 | False | inconclusive |
| kodim12/420/p2/d4 | 0.945280 | 1.002328 | 1.062799 | 1.052125 | 1.071456 | 290753.200000 | 114312.600000 | True | inconclusive |
| kodim13/444/p0/d1 | 1.056459 | 1.009081 | 1.266580 | 1.070340 | 1.046577 | 295529.166667 | 228173.333333 | False | inconclusive |
| kodim13/444/p0/d4 | 1.063399 | 1.018695 | 1.042926 | 1.039410 | 1.051485 | 191897.200000 | 124624.600000 | True | baseline |
| kodim13/444/p2/d1 | 0.913898 | 1.047060 | 1.071680 | 1.048907 | 1.047573 | 410347.900000 | 279626.333333 | True | current |
| kodim13/444/p2/d4 | 0.850858 | 1.016658 | 1.169882 | 1.052723 | 1.123834 | 298061.500000 | 167304.000000 | False | inconclusive |
| kodim13/420/p0/d1 | 1.076496 | 1.055421 | 1.070908 | 1.059520 | 1.073270 | 302668.166667 | 208820.600000 | True | baseline |
| kodim13/420/p0/d4 | 1.061502 | 1.013394 | 1.110854 | 1.044573 | 1.038916 | 204352.600000 | 110488.000000 | False | inconclusive |
| kodim13/420/p2/d1 | 0.982317 | 1.019082 | 1.259767 | 1.127634 | 1.050856 | 438699.800000 | 262367.400000 | False | inconclusive |
| kodim13/420/p2/d4 | 0.953035 | 1.003487 | 1.033315 | 1.037328 | 1.033264 | 338473.800000 | 162088.600000 | True | current |
| kodim14/444/p0/d1 | 1.069824 | 1.049389 | 1.106978 | 1.094887 | 1.088279 | 253718.500000 | 186364.866667 | False | inconclusive |
| kodim14/444/p0/d4 | 1.063573 | 1.027313 | 1.067870 | 1.042601 | 1.038638 | 172452.300000 | 105156.300000 | True | baseline |
| kodim14/444/p2/d1 | 0.989523 | 1.000124 | 1.153509 | 1.035806 | 1.056386 | 358735.833333 | 228020.400000 | False | inconclusive |
| kodim14/444/p2/d4 | 0.895028 | 1.026558 | 1.102036 | 1.027009 | 1.066466 | 286001.200000 | 155288.600000 | False | inconclusive |
| kodim14/420/p0/d1 | 1.064964 | 1.000128 | 1.067025 | 1.027066 | 1.022631 | 262046.633333 | 168208.066667 | True | baseline |
| kodim14/420/p0/d4 | 1.049350 | 1.009134 | 1.025906 | 1.026779 | 1.018073 | 188768.600000 | 94908.300000 | True | baseline |
| kodim14/420/p2/d1 | 1.014598 | 1.011125 | 1.186428 | 1.051659 | 1.106103 | 388088.733333 | 211657.066667 | False | inconclusive |
| kodim14/420/p2/d4 | 0.977405 | 1.005532 | 1.087180 | 1.020267 | 1.019924 | 317742.266667 | 141312.600000 | True | inconclusive |
| kodim15/444/p0/d1 | 1.069122 | 1.011222 | 1.102829 | 1.032890 | 1.033085 | 201186.700000 | 133889.400000 | False | inconclusive |
| kodim15/444/p0/d4 | 1.052940 | 1.003600 | 1.046236 | 1.016451 | 1.041298 | 149632.600000 | 82332.300000 | True | baseline |
| kodim15/444/p2/d1 | 0.988287 | 1.005651 | 1.175227 | 1.020954 | 1.023910 | 326166.133333 | 195528.066667 | False | inconclusive |
| kodim15/444/p2/d4 | 0.807966 | 1.006175 | 1.024852 | 1.029751 | 1.021417 | 264768.600000 | 134064.600000 | True | current |
| kodim15/420/p0/d1 | 1.072440 | 1.003421 | 1.023211 | 1.015423 | 1.018445 | 214416.600000 | 120560.600000 | True | baseline |
| kodim15/420/p0/d4 | 1.042264 | 1.009049 | 1.080969 | 1.038630 | 1.025818 | 169784.000000 | 75932.300000 | True | inconclusive |
| kodim15/420/p2/d1 | 0.988267 | 1.033150 | 1.426221 | 1.286255 | 1.030495 | 343257.800000 | 166840.600000 | False | inconclusive |
| kodim15/420/p2/d4 | 0.991352 | 1.014623 | 1.100067 | 1.089804 | 1.103185 | 294409.200000 | 117968.600000 | False | inconclusive |
| kodim16/444/p0/d1 | 1.060690 | 1.002906 | 1.056099 | 1.038808 | 1.060359 | 204083.266667 | 136761.033333 | True | baseline |
| kodim16/444/p0/d4 | 1.033721 | 1.007848 | 1.020899 | 1.018854 | 1.030846 | 151200.600000 | 83896.000000 | True | baseline |
| kodim16/444/p2/d1 | 0.864352 | 1.005604 | 1.039245 | 1.050763 | 1.039949 | 317392.200000 | 186643.833333 | True | current |
| kodim16/444/p2/d4 | 0.808707 | 1.014389 | 1.155659 | 1.040174 | 1.050097 | 253585.200000 | 122808.600000 | False | inconclusive |
| kodim16/420/p0/d1 | 1.053204 | 1.013507 | 1.058459 | 1.045613 | 1.040939 | 218489.200000 | 124620.300000 | True | baseline |
| kodim16/420/p0/d4 | 1.030816 | 1.007951 | 1.276125 | 1.040754 | 1.051585 | 170572.900000 | 76708.300000 | False | inconclusive |
| kodim16/420/p2/d1 | 0.989929 | 1.003911 | 1.077703 | 1.052646 | 1.046581 | 347504.533333 | 171076.900000 | True | inconclusive |
| kodim16/420/p2/d4 | 0.895650 | 1.007602 | 1.081819 | 1.041762 | 1.019295 | 300169.200000 | 123760.600000 | True | current |
| kodim17/444/p0/d1 | 1.051597 | 1.009073 | 1.130589 | 1.082352 | 1.063841 | 181523.700000 | 135388.400000 | False | inconclusive |
| kodim17/444/p0/d4 | 1.041255 | 1.011439 | 1.025658 | 1.025726 | 1.026529 | 131368.600000 | 85188.300000 | True | baseline |
| kodim17/444/p2/d1 | 0.862801 | 1.003088 | 1.025122 | 1.023764 | 1.030396 | 292497.800000 | 184569.200000 | True | current |
| kodim17/444/p2/d4 | 0.808802 | 1.007729 | 1.392911 | 1.066425 | 1.031573 | 232313.200000 | 124316.300000 | False | inconclusive |
| kodim17/420/p0/d1 | 1.065069 | 1.006381 | 1.254717 | 1.146500 | 1.320933 | 186656.600000 | 122752.600000 | False | inconclusive |
| kodim17/420/p0/d4 | 1.046021 | 1.001792 | 1.022638 | 1.022739 | 1.024963 | 142056.600000 | 78148.300000 | True | baseline |
| kodim17/420/p2/d1 | 1.007986 | 1.046696 | 1.208071 | 1.031970 | 1.053780 | 317180.900000 | 174604.900000 | False | inconclusive |
| kodim17/420/p2/d4 | 0.911742 | 1.016592 | 1.090646 | 1.079839 | 1.035520 | 268429.500000 | 125804.300000 | True | current |
| kodim18/444/p0/d1 | 1.070432 | 1.021159 | 1.324451 | 1.104788 | 1.068589 | 226874.300000 | 180709.400000 | False | inconclusive |
| kodim18/444/p0/d4 | 1.052949 | 1.007801 | 1.035933 | 1.033318 | 1.034932 | 148404.900000 | 102220.300000 | True | baseline |
| kodim18/444/p2/d1 | 0.994969 | 1.021271 | 1.102585 | 1.097357 | 1.036547 | 342483.033333 | 234545.266667 | False | inconclusive |
| kodim18/444/p2/d4 | 0.839788 | 1.014436 | 1.157425 | 1.078183 | 1.024548 | 261165.500000 | 153232.600000 | False | inconclusive |
| kodim18/420/p0/d1 | 1.083464 | 1.026324 | 1.113565 | 1.019820 | 1.047372 | 227426.433333 | 163507.600000 | False | inconclusive |
| kodim18/420/p0/d4 | 1.063644 | 1.005243 | 1.032842 | 1.023865 | 1.021148 | 156376.633333 | 92436.300000 | True | baseline |
| kodim18/420/p2/d1 | 1.013767 | 1.016355 | 1.058151 | 1.022987 | 1.049077 | 361700.600000 | 219121.766667 | True | inconclusive |
| kodim18/420/p2/d4 | 0.976389 | 1.009125 | 1.015470 | 1.009388 | 1.019974 | 276833.200000 | 134176.000000 | True | current |
| kodim19/444/p0/d1 | 1.056915 | 1.035591 | 1.109697 | 1.039957 | 1.025265 | 196672.600000 | 150557.233333 | False | inconclusive |
| kodim19/444/p0/d4 | 1.050401 | 1.002628 | 1.034638 | 1.023039 | 1.028350 | 135432.000000 | 89264.600000 | True | baseline |
| kodim19/444/p2/d1 | 0.904654 | 1.011948 | 1.109936 | 1.039932 | 1.023917 | 307727.100000 | 199760.033333 | False | inconclusive |
| kodim19/444/p2/d4 | 0.816687 | 1.003731 | 1.022881 | 1.017474 | 1.017880 | 236297.200000 | 128304.600000 | True | current |
| kodim19/420/p0/d1 | 1.082285 | 1.012659 | 1.087561 | 1.032454 | 1.073621 | 199776.000000 | 135907.866667 | True | baseline |
| kodim19/420/p0/d4 | 1.056273 | 1.015976 | 1.249631 | 1.027642 | 1.134381 | 145536.600000 | 81628.300000 | False | inconclusive |
| kodim19/420/p2/d1 | 0.988232 | 1.028409 | 1.050566 | 1.043624 | 1.060502 | 329967.333333 | 187393.066667 | True | inconclusive |
| kodim19/420/p2/d4 | 0.925389 | 1.013618 | 1.078814 | 1.037445 | 1.028739 | 265648.600000 | 123008.600000 | True | current |
| kodim20/444/p0/d1 | 1.040582 | 1.004168 | 1.046511 | 1.037834 | 1.039083 | 187793.200000 | 120488.600000 | True | baseline |
| kodim20/444/p0/d4 | 1.025875 | 1.005932 | 1.013982 | 1.026423 | 1.012498 | 145496.600000 | 78200.600000 | True | baseline |
| kodim20/444/p2/d1 | 0.963857 | 1.049873 | 1.164990 | 1.052655 | 1.070080 | 300141.500000 | 169428.900000 | False | inconclusive |
| kodim20/444/p2/d4 | 0.799875 | 1.001819 | 1.126022 | 1.056761 | 1.036749 | 247348.900000 | 116572.300000 | False | inconclusive |
| kodim20/420/p0/d1 | 1.050631 | 1.021507 | 1.401631 | 1.041285 | 1.043986 | 204580.900000 | 110720.600000 | False | inconclusive |
| kodim20/420/p0/d4 | 1.019479 | 1.006060 | 1.045606 | 1.053619 | 1.034097 | 166232.600000 | 72372.300000 | True | inconclusive |
| kodim20/420/p2/d1 | 0.969536 | 1.004540 | 1.038477 | 1.053049 | 1.041105 | 337657.800000 | 161272.600000 | True | current |
| kodim20/420/p2/d4 | 0.942753 | 1.006806 | 1.077715 | 1.133314 | 1.010032 | 289517.500000 | 113072.600000 | True | current |
| kodim21/444/p0/d1 | 1.088235 | 1.038056 | 1.394754 | 1.089196 | 1.139782 | 220404.900000 | 153151.066667 | False | inconclusive |
| kodim21/444/p0/d4 | 1.044825 | 1.013428 | 1.207454 | 1.032905 | 1.114800 | 158760.600000 | 91460.300000 | False | inconclusive |
| kodim21/444/p2/d1 | 0.918752 | 1.029578 | 1.140025 | 1.056894 | 1.117663 | 331886.800000 | 201191.666667 | False | inconclusive |
| kodim21/444/p2/d4 | 0.816408 | 1.000170 | 1.031168 | 1.021141 | 1.020917 | 260673.200000 | 129900.900000 | True | current |
| kodim21/420/p0/d1 | 1.071893 | 1.008025 | 1.110827 | 1.030521 | 1.033452 | 231968.600000 | 138121.000000 | False | inconclusive |
| kodim21/420/p0/d4 | 1.044802 | 1.009922 | 1.377682 | 1.129401 | 1.051629 | 177476.900000 | 83616.600000 | False | inconclusive |
| kodim21/420/p2/d1 | 0.977865 | 1.002897 | 1.070552 | 1.023196 | 1.020753 | 365021.500000 | 188640.500000 | True | inconclusive |
| kodim21/420/p2/d4 | 0.916722 | 1.035528 | 1.123030 | 1.026733 | 1.109842 | 300669.500000 | 124220.300000 | False | inconclusive |
| kodim22/444/p0/d1 | 1.092932 | 1.045437 | 1.188978 | 1.041777 | 1.044885 | 228539.800000 | 161229.133333 | False | inconclusive |
| kodim22/444/p0/d4 | 1.058582 | 1.002273 | 1.103181 | 1.034743 | 1.045604 | 158780.900000 | 91476.300000 | False | inconclusive |
| kodim22/444/p2/d1 | 0.990384 | 1.001714 | 1.033164 | 1.030124 | 1.022260 | 340534.300000 | 209800.566667 | True | inconclusive |
| kodim22/444/p2/d4 | 0.830382 | 1.014166 | 1.028295 | 1.043122 | 1.037245 | 267581.500000 | 136832.600000 | True | current |
| kodim22/420/p0/d1 | 1.091065 | 1.015554 | 1.054974 | 1.045199 | 1.068417 | 238696.866667 | 144814.466667 | True | baseline |
| kodim22/420/p0/d4 | 1.057603 | 1.080632 | 1.298336 | 1.278444 | 1.023043 | 176964.900000 | 83100.300000 | False | inconclusive |
| kodim22/420/p2/d1 | 1.019518 | 1.021493 | 1.203794 | 1.028877 | 1.051309 | 371872.900000 | 195457.600000 | False | inconclusive |
| kodim22/420/p2/d4 | 0.952862 | 1.011146 | 1.049401 | 1.039735 | 1.045304 | 306973.500000 | 130552.000000 | True | current |
| kodim23/444/p0/d1 | 1.066661 | 1.009033 | 1.042109 | 1.037986 | 1.031616 | 183028.900000 | 115724.300000 | True | baseline |
| kodim23/444/p0/d4 | 1.037750 | 1.006775 | 1.040852 | 1.041244 | 1.043562 | 144904.600000 | 77604.300000 | True | baseline |
| kodim23/444/p2/d1 | 0.935950 | 1.007541 | 1.074146 | 1.040507 | 1.039229 | 297241.200000 | 166520.000000 | True | current |
| kodim23/444/p2/d4 | 0.792634 | 1.009158 | 1.113563 | 1.038993 | 1.037660 | 254521.200000 | 123772.300000 | False | inconclusive |
| kodim23/420/p0/d1 | 1.067060 | 1.011153 | 1.027079 | 1.035945 | 1.021223 | 197616.600000 | 103760.600000 | True | baseline |
| kodim23/420/p0/d4 | 1.023033 | 1.011894 | 1.026380 | 1.036566 | 1.040178 | 165924.900000 | 72060.300000 | True | baseline |
| kodim23/420/p2/d1 | 0.976710 | 1.011090 | 1.049842 | 1.040630 | 1.030602 | 315345.800000 | 138884.900000 | True | inconclusive |
| kodim23/420/p2/d4 | 0.918720 | 1.010382 | 1.184665 | 1.017030 | 1.017618 | 279081.200000 | 102756.300000 | False | inconclusive |
| kodim24/444/p0/d1 | 1.064505 | 1.008231 | 1.035431 | 1.031393 | 1.030385 | 245493.266667 | 178112.166667 | True | baseline |
| kodim24/444/p0/d4 | 1.050733 | 1.009048 | 1.030445 | 1.055680 | 1.014771 | 171100.900000 | 103800.600000 | True | baseline |
| kodim24/444/p2/d1 | 0.989176 | 1.003418 | 1.022865 | 1.009584 | 1.012225 | 357401.700000 | 226646.400000 | True | inconclusive |
| kodim24/444/p2/d4 | 0.849818 | 1.024504 | 1.239876 | 1.025806 | 1.036319 | 279289.200000 | 148548.900000 | False | inconclusive |
| kodim24/420/p0/d1 | 1.079910 | 1.007253 | 1.057456 | 1.015532 | 1.025520 | 255620.900000 | 161825.800000 | True | baseline |
| kodim24/420/p0/d4 | 1.064865 | 1.001588 | 1.016589 | 1.014275 | 1.007910 | 187660.900000 | 93792.000000 | True | baseline |
| kodim24/420/p2/d1 | 1.006080 | 1.000989 | 1.023162 | 1.012415 | 1.010710 | 382839.766667 | 206412.300000 | True | inconclusive |
| kodim24/420/p2/d4 | 0.963367 | 1.002916 | 1.128206 | 1.007873 | 1.020858 | 317521.200000 | 141112.600000 | False | inconclusive |
| smooth-gradient/444/p0/d1 | 0.920848 | 1.000932 | 1.060382 | 1.030942 | 1.051375 | 76388.300000 | 50668.300000 | True | current |
| smooth-gradient/444/p0/d4 | 0.900379 | 1.013881 | 1.052009 | 1.008062 | 1.019398 | 69516.300000 | 43780.300000 | True | current |
| smooth-gradient/444/p2/d1 | 0.674360 | 1.001686 | 1.029019 | 1.012036 | 1.022679 | 171312.600000 | 85328.600000 | True | current |
| smooth-gradient/444/p2/d4 | 0.668327 | 1.012217 | 1.115084 | 1.020647 | 1.033472 | 158852.300000 | 72988.300000 | False | inconclusive |
| smooth-gradient/420/p0/d1 | 0.929168 | 1.067578 | 1.325132 | 1.283024 | 1.439388 | 84792.600000 | 49900.300000 | False | inconclusive |
| smooth-gradient/420/p0/d4 | 0.904665 | 1.003100 | 1.052888 | 1.007826 | 1.014208 | 84060.300000 | 49172.300000 | True | current |
| smooth-gradient/420/p2/d1 | 0.681638 | 1.014541 | 1.041849 | 1.025815 | 1.013185 | 190241.200000 | 80444.300000 | True | current |
| smooth-gradient/420/p2/d4 | 0.683219 | 1.011399 | 1.035574 | 1.025851 | 1.023180 | 183916.900000 | 74112.600000 | True | current |
| low-contrast/444/p0/d1 | 0.976635 | 1.009484 | 1.032688 | 1.025869 | 1.011684 | 87232.600000 | 61508.300000 | True | current |
| low-contrast/444/p0/d4 | 0.935531 | 1.010467 | 1.046430 | 1.012398 | 1.017999 | 77212.300000 | 51492.300000 | True | current |
| low-contrast/444/p2/d1 | 0.817195 | 1.002797 | 1.021301 | 1.023431 | 1.023973 | 216700.900000 | 130872.600000 | True | current |
| low-contrast/444/p2/d4 | 0.743656 | 1.005190 | 1.027210 | 1.026239 | 1.029711 | 190112.600000 | 104208.600000 | True | current |
| low-contrast/420/p0/d1 | 0.985528 | 1.004503 | 1.051542 | 1.038685 | 1.022844 | 93096.600000 | 58204.300000 | True | current |
| low-contrast/420/p0/d4 | 0.930924 | 1.014405 | 1.053281 | 1.043002 | 1.035665 | 85312.600000 | 50420.300000 | True | current |
| low-contrast/420/p2/d1 | 0.890115 | 1.008508 | 1.048777 | 1.035414 | 1.018222 | 221641.200000 | 111784.000000 | True | current |
| low-contrast/420/p2/d4 | 0.706892 | 1.008593 | 1.072091 | 1.053999 | 1.053133 | 202632.600000 | 92724.300000 | True | current |
| color-edges/444/p0/d1 | 0.927532 | 1.010870 | 1.055961 | 1.045436 | 1.022109 | 94264.600000 | 68524.300000 | True | current |
| color-edges/444/p0/d4 | 0.913434 | 1.012402 | 1.029733 | 1.020445 | 1.007020 | 84080.000000 | 58348.300000 | True | current |
| color-edges/444/p2/d1 | 0.881879 | 1.021459 | 1.027895 | 1.022925 | 1.031293 | 206612.900000 | 120688.600000 | True | current |
| color-edges/444/p2/d4 | 0.882233 | 1.008049 | 1.025855 | 1.024471 | 1.016024 | 191121.200000 | 105160.600000 | True | current |
| color-edges/420/p0/d1 | 0.951800 | 1.011084 | 1.047231 | 1.025038 | 1.016186 | 104176.600000 | 69284.300000 | True | current |
| color-edges/420/p0/d4 | 0.937954 | 1.006704 | 1.054931 | 1.008163 | 1.037321 | 90040.000000 | 55140.300000 | True | current |
| color-edges/420/p2/d1 | 0.870319 | 1.002137 | 1.272479 | 1.019639 | 1.031100 | 220872.000000 | 110992.600000 | False | inconclusive |
| color-edges/420/p2/d4 | 0.867028 | 1.023390 | 1.041768 | 1.024591 | 1.035204 | 218480.600000 | 108624.600000 | True | current |
| line-art/444/p0/d1 | 0.900897 | 1.000041 | 1.042016 | 1.010552 | 1.030368 | 94808.000000 | 69092.300000 | True | current |
| line-art/444/p0/d4 | 0.878571 | 1.015970 | 1.038587 | 1.021357 | 1.021439 | 87756.300000 | 62036.300000 | True | current |
| line-art/444/p2/d1 | 0.761543 | 1.006039 | 1.048129 | 1.011717 | 1.033582 | 195345.200000 | 109368.600000 | True | current |
| line-art/444/p2/d4 | 0.751474 | 1.028894 | 1.110051 | 1.018435 | 1.023372 | 182532.900000 | 96544.600000 | False | inconclusive |
| line-art/420/p0/d1 | 0.927539 | 1.027115 | 1.437745 | 1.319495 | 1.329683 | 102672.600000 | 67780.300000 | False | inconclusive |
| line-art/420/p0/d4 | 0.918679 | 1.002347 | 1.065098 | 1.030916 | 1.046468 | 95312.600000 | 60420.300000 | True | current |
| line-art/420/p2/d1 | 0.835125 | 1.008722 | 1.051305 | 1.041833 | 1.038191 | 213761.200000 | 103832.600000 | True | current |
| line-art/420/p2/d4 | 0.827491 | 1.034851 | 1.045833 | 1.031545 | 1.039019 | 206432.600000 | 96512.600000 | True | current |
| structured-texture/444/p0/d1 | 1.011996 | 1.006351 | 1.027170 | 1.027481 | 1.023508 | 117976.600000 | 92240.600000 | True | inconclusive |
| structured-texture/444/p0/d4 | 1.006009 | 1.004151 | 1.037008 | 1.027621 | 1.009013 | 88136.600000 | 62396.300000 | True | inconclusive |
| structured-texture/444/p2/d1 | 0.963265 | 1.005592 | 1.012128 | 1.006302 | 1.012296 | 248292.900000 | 162464.600000 | True | current |
| structured-texture/444/p2/d4 | 0.944910 | 1.010353 | 1.077949 | 1.056387 | 1.028044 | 195090.466667 | 109096.600000 | True | current |
| structured-texture/420/p0/d1 | 0.995892 | 1.007389 | 1.063162 | 1.022652 | 1.034670 | 108752.600000 | 73844.300000 | True | inconclusive |
| structured-texture/420/p0/d4 | 0.975987 | 1.009642 | 1.047327 | 1.027980 | 1.031376 | 89840.600000 | 54932.300000 | True | current |
| structured-texture/420/p2/d1 | 0.936560 | 1.008090 | 1.279726 | 1.085072 | 1.033720 | 232193.200000 | 122292.300000 | False | inconclusive |
| structured-texture/420/p2/d4 | 0.928651 | 1.006589 | 1.025884 | 1.009443 | 1.013143 | 201276.900000 | 91484.300000 | True | current |
| low-noise/444/p0/d1 | 1.036541 | 1.007366 | 1.039033 | 1.028790 | 1.022664 | 89968.600000 | 64244.300000 | True | baseline |
| low-noise/444/p0/d4 | 0.926537 | 1.006578 | 1.050640 | 1.038056 | 1.029978 | 75316.300000 | 49596.300000 | True | current |
| low-noise/444/p2/d1 | 0.827660 | 1.004982 | 1.018469 | 1.016537 | 1.016940 | 196964.900000 | 111024.600000 | True | current |
| low-noise/444/p2/d4 | 0.672665 | 1.013125 | 1.034397 | 1.019924 | 1.012683 | 165052.900000 | 79196.300000 | True | current |
| low-noise/420/p0/d1 | 1.000307 | 1.012445 | 1.125698 | 1.011022 | 1.398462 | 88664.000000 | 53764.300000 | False | inconclusive |
| low-noise/420/p0/d4 | 0.923344 | 1.024393 | 1.063466 | 1.058797 | 1.014156 | 78052.300000 | 43148.300000 | True | current |
| low-noise/420/p2/d1 | 0.880197 | 1.016987 | 1.031528 | 1.021565 | 1.023054 | 211452.900000 | 101544.600000 | True | current |
| low-noise/420/p2/d4 | 0.697169 | 1.020198 | 1.056592 | 1.028849 | 1.019525 | 189636.300000 | 79852.300000 | True | current |
| full-noise/444/p0/d1 | 1.032639 | 1.004490 | 1.111940 | 1.013723 | 1.024430 | 135788.300000 | 110072.600000 | False | inconclusive |
| full-noise/444/p0/d4 | 0.998861 | 1.018292 | 1.045881 | 1.009871 | 1.033707 | 103660.300000 | 77940.300000 | True | inconclusive |
| full-noise/444/p2/d1 | 0.957564 | 1.004394 | 1.284676 | 1.012724 | 1.015586 | 277689.200000 | 191936.600000 | False | inconclusive |
| full-noise/444/p2/d4 | 0.928551 | 1.001720 | 1.019749 | 1.013499 | 1.014060 | 216233.200000 | 130324.900000 | True | current |
| full-noise/420/p0/d1 | 0.998476 | 1.005162 | 1.029587 | 1.007967 | 1.054149 | 116400.600000 | 81492.300000 | True | inconclusive |
| full-noise/420/p0/d4 | 0.977343 | 1.000960 | 1.062560 | 1.036898 | 1.032315 | 96352.000000 | 61452.300000 | True | inconclusive |
| full-noise/420/p2/d1 | 0.931925 | 1.006285 | 1.036350 | 1.020088 | 1.016425 | 245652.900000 | 135796.900000 | True | current |
| full-noise/420/p2/d4 | 0.905167 | 1.000101 | 1.433498 | 1.014794 | 1.038782 | 219216.000000 | 109336.600000 | False | inconclusive |
| tiny-odd-edges/444/p0/d1 | 0.607358 | 1.058714 | 1.115022 | 1.056155 | 1.102329 | 42715.440000 | 36761.720000 | False | inconclusive |
| tiny-odd-edges/444/p0/d4 | 0.541385 | 1.067761 | 1.116776 | 1.048164 | 1.103589 | 42418.580000 | 36466.580000 | False | inconclusive |
| tiny-odd-edges/444/p2/d1 | 0.517355 | 1.008337 | 1.067427 | 1.041229 | 1.072590 | 131008.600000 | 64411.440000 | True | current |
| tiny-odd-edges/444/p2/d4 | 0.480957 | 1.012565 | 1.087179 | 1.047088 | 1.078481 | 130679.740000 | 64084.300000 | True | current |
| tiny-odd-edges/420/p0/d1 | 0.616901 | 1.026500 | 1.092547 | 1.077349 | 1.099643 | 43410.580000 | 36569.720000 | True | current |
| tiny-odd-edges/420/p0/d4 | 0.545096 | 1.011002 | 1.099420 | 1.055764 | 1.092267 | 43234.580000 | 36393.720000 | True | current |
| tiny-odd-edges/420/p2/d1 | 0.506653 | 1.035887 | 1.103188 | 1.060098 | 1.088609 | 145713.460000 | 65500.300000 | False | inconclusive |
| tiny-odd-edges/420/p2/d4 | 0.475941 | 1.012675 | 1.094088 | 1.043689 | 1.075030 | 145481.460000 | 65268.300000 | True | current |

Invalid evidence:

- kodim01/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim01/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim01/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim01/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim01/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim01/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim02/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim02/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim02/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim02/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim02/444/p2/d4: baseline: p90/p10 drift exceeds20%, including minority timing states.
- kodim02/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim02/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim02/420/p0/d4: baseline: p90/p10 drift exceeds20%, including minority timing states.
- kodim02/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim02/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim03/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim03/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim03/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim03/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim03/420/p0/d4: baseline: p90/p10 drift exceeds20%, including minority timing states.
- kodim03/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim04/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim04/444/p2/d1: baseline: p90/p10 drift exceeds20%, including minority timing states.
- kodim04/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim04/444/p2/d1: ABBA/BAAB order bias exceeds10% or one order is absent.
- kodim04/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim04/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim05/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim05/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim05/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim06/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim06/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim06/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim07/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim07/444/p0/d4: baseline: p90/p10 drift exceeds20%, including minority timing states.
- kodim07/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim07/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim07/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim08/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim08/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim08/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim08/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim09/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim09/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim09/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim09/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim09/420/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim09/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim10/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim10/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim10/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim10/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim11/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim11/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim11/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim12/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim12/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim12/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim13/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim13/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim13/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim13/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim13/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim13/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim14/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim14/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim14/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim14/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim15/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim15/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim15/420/p2/d1: baseline: p90/p10 drift exceeds20%, including minority timing states.
- kodim15/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim15/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim15/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim16/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim16/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim17/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim17/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim17/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim17/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim17/420/p0/d1: current: p90/p10 drift exceeds20%, including minority timing states.
- kodim17/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim18/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim18/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim18/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim18/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim18/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim19/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim19/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim19/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim20/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim20/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim20/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim21/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim21/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim21/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim21/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim21/444/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- kodim21/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim21/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim21/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim21/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim21/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim22/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- kodim22/444/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim22/420/p0/d4: baseline: p90/p10 drift exceeds20%, including minority timing states.
- kodim22/420/p0/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim22/420/p0/d4: current: repeated legs within a quartet differ by over10%.
- kodim22/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- kodim23/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim23/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim23/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim24/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- kodim24/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- kodim24/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- smooth-gradient/444/p2/d4: baseline: repeated legs within a quartet differ by over10%.
- smooth-gradient/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- smooth-gradient/420/p0/d1: baseline: p90/p10 drift exceeds20%, including minority timing states.
- smooth-gradient/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- smooth-gradient/420/p0/d1: current: p90/p10 drift exceeds20%, including minority timing states.
- smooth-gradient/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- color-edges/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- line-art/444/p2/d4: current: repeated legs within a quartet differ by over10%.
- line-art/420/p0/d1: baseline: p90/p10 drift exceeds20%, including minority timing states.
- line-art/420/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- line-art/420/p0/d1: current: p90/p10 drift exceeds20%, including minority timing states.
- line-art/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- structured-texture/420/p2/d1: baseline: repeated legs within a quartet differ by over10%.
- structured-texture/420/p2/d1: current: repeated legs within a quartet differ by over10%.
- low-noise/420/p0/d1: current: p90/p10 drift exceeds20%, including minority timing states.
- low-noise/420/p0/d1: current: repeated legs within a quartet differ by over10%.
- full-noise/444/p0/d1: baseline: repeated legs within a quartet differ by over10%.
- full-noise/444/p2/d1: current: repeated legs within a quartet differ by over10%.
- full-noise/420/p2/d4: current: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d1: current: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/444/p0/d4: current: repeated legs within a quartet differ by over10%.
- tiny-odd-edges/420/p2/d1: current: repeated legs within a quartet differ by over10%.
