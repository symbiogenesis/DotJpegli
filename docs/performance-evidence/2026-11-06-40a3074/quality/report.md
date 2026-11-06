# Quality and compression parity: PASS

RGB8 to YCbCr, Kodak24 and controlled corpus; noninferiority within declared tolerances; no universal or visual-quality proof

Pinned upstream: `031a0077f5799a6041004267fc12b956c1f52a20`. Cases: 1280; matched-rate targets: 2850 (640 anchors); matched-quality targets: 4396 (1280 anchors).
Tested revision: `40a3074c61fbb052e452a82faf6d74616d060060`.

Every group and every image/target must satisfy the fixed bounds. Aggregate results never excuse an individual failure. The gate measures PSNR and this explicitly defined local luma SSIM, without a human study or Butteraugli/MS-SSIM claim.

Size limits: <=1.02 geometric mean and <=1.05 each case/target. Matched-rate PSNR delta: >=-0.10dB mean, >=-0.25dB each target. SSIM delta: >=-0.001 mean, >=-0.003 each target. Lower managed bytes and higher managed quality are accepted.

Means use five interior anchors per image. Worst bounds use those anchors, overlap endpoints and the union of both encoders' frontier knots, covering the full common piecewise-linear interpolated range. No extrapolation or unmeasured real encoder behavior between samples is claimed.

| Corpus | Sampling | Progression | Mean size ratio | Worst ratio | Mean PSNR delta dB | Worst PSNR delta dB | Mean SSIM delta | Worst SSIM delta | Verdict |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| natural | 444 | 0 | 0.999856 | 1.000982 | 0.000807 | -0.008772 | 0.000013 | -0.000050 | PASS |
| natural | 444 | 2 | 0.999837 | 1.000342 | 0.001148 | -0.001630 | 0.000010 | -0.000043 | PASS |
| natural | 420 | 0 | 0.999809 | 1.001304 | 0.001062 | -0.007462 | 0.000021 | -0.000083 | PASS |
| natural | 420 | 2 | 0.999838 | 1.000597 | 0.000996 | -0.002690 | 0.000011 | -0.000042 | PASS |
| controlled | 444 | 0 | 0.999833 | 1.001864 | 0.002201 | -0.051802 | 0.000015 | -0.000131 | PASS |
| controlled | 444 | 2 | 1.000010 | 1.002830 | -0.001882 | -0.047166 | -0.000009 | -0.000789 | PASS |
| controlled | 420 | 0 | 0.999872 | 1.001330 | 0.000610 | -0.008942 | 0.000023 | -0.000047 | PASS |
| controlled | 420 | 2 | 0.999928 | 1.003636 | 0.000600 | -0.027752 | -0.000032 | -0.002145 | PASS |

| Corpus | Sampling | Progression | Matched quality metric | Targets | Mean rate ratio | Worst ratio | Verdict |
| --- | --- | --- | --- | ---: | ---: | ---: | --- |
| natural | 444 | 0 | rgb-psnr-db | 429 (120 anchors) | 0.999890 | 1.000982 | PASS |
| natural | 444 | 0 | luma-ssim | 430 (120 anchors) | 0.999777 | 1.000982 | PASS |
| natural | 444 | 2 | rgb-psnr-db | 429 (120 anchors) | 0.999848 | 1.000334 | PASS |
| natural | 444 | 2 | luma-ssim | 430 (120 anchors) | 0.999819 | 1.000343 | PASS |
| natural | 420 | 0 | rgb-psnr-db | 408 (120 anchors) | 0.999831 | 1.001302 | PASS |
| natural | 420 | 0 | luma-ssim | 408 (120 anchors) | 0.999687 | 1.001301 | PASS |
| natural | 420 | 2 | rgb-psnr-db | 408 (120 anchors) | 0.999838 | 1.000597 | PASS |
| natural | 420 | 2 | luma-ssim | 408 (120 anchors) | 0.999833 | 1.000597 | PASS |
| controlled | 444 | 0 | rgb-psnr-db | 133 (40 anchors) | 0.999900 | 1.001864 | PASS |
| controlled | 444 | 0 | luma-ssim | 132 (40 anchors) | 0.999837 | 1.001864 | PASS |
| controlled | 444 | 2 | rgb-psnr-db | 133 (40 anchors) | 0.999953 | 1.002830 | PASS |
| controlled | 444 | 2 | luma-ssim | 132 (40 anchors) | 0.999988 | 1.002830 | PASS |
| controlled | 420 | 0 | rgb-psnr-db | 131 (40 anchors) | 0.999814 | 1.001330 | PASS |
| controlled | 420 | 0 | luma-ssim | 127 (40 anchors) | 0.999853 | 1.001330 | PASS |
| controlled | 420 | 2 | rgb-psnr-db | 131 (40 anchors) | 0.999933 | 1.003636 | PASS |
| controlled | 420 | 2 | luma-ssim | 127 (40 anchors) | 0.999971 | 1.003636 | PASS |

Raw samples, source hashes, decoded hashes, settings, bounds and failures are in the JSON files; JPEGs and exact RGB inputs are retained. Decoder PPM scratch is discarded after its hash and metrics are recorded.
