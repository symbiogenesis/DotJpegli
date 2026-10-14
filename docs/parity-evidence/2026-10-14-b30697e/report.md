# Quality and compression parity: PASS

RGB8 to YCbCr, Kodak24 and controlled corpus; noninferiority within declared tolerances; no universal or visual-quality proof

Pinned upstream: `031a0077f5799a6041004267fc12b956c1f52a20`. Cases: 1280; matched-rate targets: 2057 (640 anchors); matched-quality targets: 4396 (1280 anchors).
Tested revision: `b30697e833865d1e90af406d6206921e4884c624`.

Every group and every image/target must satisfy the fixed bounds. Aggregate results never excuse an individual failure. The gate measures PSNR and this explicitly defined local luma SSIM, without a human study or Butteraugli/MS-SSIM claim.

Size limits: <=1.02 geometric mean and <=1.05 each case/target. Matched-rate PSNR delta: >=-0.10dB mean, >=-0.25dB each target. SSIM delta: >=-0.001 mean, >=-0.003 each target. Lower managed bytes and higher managed quality are accepted.

Means use five interior anchors per image. Worst bounds use those anchors, overlap endpoints and the union of both encoders' frontier knots, covering the full common piecewise-linear interpolated range. No extrapolation or unmeasured real encoder behavior between samples is claimed.

| Corpus | Sampling | Progression | Mean size ratio | Worst ratio | Mean PSNR delta dB | Worst PSNR delta dB | Mean SSIM delta | Worst SSIM delta | Verdict |
| --- | --- | --- | ---: | ---: | ---: | ---: | ---: | ---: | --- |
| natural | 444 | 0 | 1.000013 | 1.000871 | -0.000121 | -0.005669 | -0.000001 | -0.000136 | PASS |
| natural | 444 | 2 | 1.000001 | 1.000169 | -0.000001 | -0.001557 | 0.000000 | -0.000014 | PASS |
| natural | 420 | 0 | 1.000003 | 1.000697 | -0.000023 | -0.004043 | -0.000000 | -0.000084 | PASS |
| natural | 420 | 2 | 0.999999 | 1.000239 | 0.000004 | -0.002235 | 0.000000 | -0.000011 | PASS |
| controlled | 444 | 0 | 1.000000 | 1.000039 | -0.000272 | -0.013825 | -0.000000 | -0.000003 | PASS |
| controlled | 444 | 2 | 0.999999 | 1.000017 | -0.000184 | -0.011434 | -0.000000 | -0.000003 | PASS |
| controlled | 420 | 0 | 0.999997 | 1.000000 | 0.000241 | -0.002811 | -0.000000 | -0.000002 | PASS |
| controlled | 420 | 2 | 1.000000 | 1.000000 | -0.000060 | -0.002811 | -0.000000 | -0.000002 | PASS |

| Corpus | Sampling | Progression | Matched quality metric | Targets | Mean rate ratio | Worst ratio | Verdict |
| --- | --- | --- | --- | ---: | ---: | ---: | --- |
| natural | 444 | 0 | rgb-psnr-db | 429 (120 anchors) | 1.000018 | 1.000869 | PASS |
| natural | 444 | 0 | luma-ssim | 430 (120 anchors) | 1.000014 | 1.000871 | PASS |
| natural | 444 | 2 | rgb-psnr-db | 429 (120 anchors) | 1.000001 | 1.000173 | PASS |
| natural | 444 | 2 | luma-ssim | 430 (120 anchors) | 1.000000 | 1.000175 | PASS |
| natural | 420 | 0 | rgb-psnr-db | 408 (120 anchors) | 1.000004 | 1.000710 | PASS |
| natural | 420 | 0 | luma-ssim | 408 (120 anchors) | 1.000004 | 1.000701 | PASS |
| natural | 420 | 2 | rgb-psnr-db | 408 (120 anchors) | 0.999998 | 1.000240 | PASS |
| natural | 420 | 2 | luma-ssim | 408 (120 anchors) | 1.000001 | 1.000244 | PASS |
| controlled | 444 | 0 | rgb-psnr-db | 133 (40 anchors) | 1.000006 | 1.000323 | PASS |
| controlled | 444 | 0 | luma-ssim | 132 (40 anchors) | 1.000001 | 1.000518 | PASS |
| controlled | 444 | 2 | rgb-psnr-db | 133 (40 anchors) | 1.000004 | 1.000287 | PASS |
| controlled | 444 | 2 | luma-ssim | 132 (40 anchors) | 1.000001 | 1.000483 | PASS |
| controlled | 420 | 0 | rgb-psnr-db | 131 (40 anchors) | 0.999996 | 1.000268 | PASS |
| controlled | 420 | 0 | luma-ssim | 127 (40 anchors) | 1.000012 | 1.000602 | PASS |
| controlled | 420 | 2 | rgb-psnr-db | 131 (40 anchors) | 1.000002 | 1.000263 | PASS |
| controlled | 420 | 2 | luma-ssim | 127 (40 anchors) | 1.000015 | 1.000693 | PASS |

Raw samples, source hashes, decoded hashes, settings, bounds and failures are in the JSON files; JPEGs and exact RGB inputs are retained. Decoder PPM scratch is discarded after its hash and metrics are recorded.
