; Assembly listing for method Program:Baseline(System.Span`1[System.Runtime.Intrinsics.Vector256`1[float]]) (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data
; 0 inlinees with PGO data; 12 single block inlinees; 8 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rsi
       push     rbx
       sub      rsp, 248
       vmovaps  xmmword ptr [rsp+0xE0], xmm6
       vmovaps  xmmword ptr [rsp+0xD0], xmm7
       vmovaps  xmmword ptr [rsp+0xC0], xmm8
       vmovaps  xmmword ptr [rsp+0xB0], xmm9
       vmovaps  xmmword ptr [rsp+0xA0], xmm10
       vmovaps  xmmword ptr [rsp+0x90], xmm11
       vmovaps  xmmword ptr [rsp+0x80], xmm12
       vmovaps  xmmword ptr [rsp+0x70], xmm13
       vmovaps  xmmword ptr [rsp+0x60], xmm14
       vmovaps  xmmword ptr [rsp+0x50], xmm15

G_M000_IG02:                ;; offset=0x005A
       mov      rax, bword ptr [rcx]
       mov      ecx, dword ptr [rcx+0x08]
       cmp      ecx, 7
       jle      G_M000_IG06
       mov      rdx, rax
       vmovups  ymm0, ymmword ptr [rdx]
       lea      r8, bword ptr [rax+0x20]
       vmovups  ymm1, ymmword ptr [r8]
       lea      r10, bword ptr [rax+0x40]
       vmovups  ymm2, ymmword ptr [r10]
       lea      r9, bword ptr [rax+0x60]
       vmovups  ymm3, ymmword ptr [r9]
       lea      r11, bword ptr [rax+0x80]
       vmovups  ymm4, ymmword ptr [r11]
       lea      rbx, bword ptr [rax+0xA0]
       vmovups  ymm5, ymmword ptr [rbx]
       lea      rsi, bword ptr [rax+0xC0]
       vmovups  ymm6, ymmword ptr [rsi]
       add      rax, 224
       vmovups  ymm7, ymmword ptr [rax]

G_M000_IG03:                ;; offset=0x00B7
       vmovaps  xmm8, xmm0
       vmovaps  xmm9, xmm1
       vmovaps  xmm10, xmm2
       vmovaps  xmm11, xmm3
       vextractf128 xmm0, ymm0
       vextractf128 xmm1, ymm1
       vextractf128 xmm2, ymm2
       vextractf128 xmm3, ymm3
       vmovaps  xmm12, xmm4
       vmovaps  xmm13, xmm5
       vmovaps  xmm14, xmm6
       vmovaps  xmm15, xmm7
       vextractf128 xmm4, ymm4
       vextractf128 xmm5, ymm5
       vextractf128 xmm6, ymm6
       vmovaps  xmmword ptr [rsp+0x40], xmm6
       vextractf128 xmm7, ymm7
       vunpcklps xmm6, xmm8, xmm9
       vunpckhps xmm8, xmm8, xmm9
       vunpcklps xmm9, xmm10, xmm11
       vunpckhps xmm11, xmm10, xmm11
       vunpcklpd xmm10, xmm6, xmm9
       vunpckhpd xmm9, xmm6, xmm9
       vunpcklpd xmm6, xmm8, xmm11
       vunpckhpd xmm11, xmm8, xmm11
       vunpcklps xmm8, xmm0, xmm1
       vunpckhps xmm0, xmm0, xmm1
       vunpcklps xmm1, xmm2, xmm3
       vunpckhps xmm3, xmm2, xmm3
       vunpcklpd xmm2, xmm8, xmm1
       vunpckhpd xmm1, xmm8, xmm1
       vunpcklpd xmm8, xmm0, xmm3
       vunpckhpd xmm3, xmm0, xmm3
       vunpcklps xmm0, xmm12, xmm13
       vunpckhps xmm12, xmm12, xmm13
       vunpcklps xmm13, xmm14, xmm15
       vunpckhps xmm15, xmm14, xmm15
       vunpcklpd xmm14, xmm0, xmm13
       vunpckhpd xmm13, xmm0, xmm13
       vunpcklpd xmm0, xmm12, xmm15
       vunpckhpd xmm15, xmm12, xmm15
       vunpcklps xmm12, xmm4, xmm5
       vunpckhps xmm4, xmm4, xmm5
       vmovaps  xmmword ptr [rsp+0x30], xmm4
       vmovaps  xmm5, xmmword ptr [rsp+0x40]
       vunpcklps xmm4, xmm5, xmm7
       vunpckhps xmm7, xmm5, xmm7
       vmovaps  xmmword ptr [rsp+0x20], xmm7
       vunpcklpd xmm5, xmm12, xmm4
       vunpckhpd xmm4, xmm12, xmm4
       vmovaps  xmm12, xmmword ptr [rsp+0x30]
       vunpcklpd xmm12, xmm12, xmm7
       vmovaps  xmm7, xmmword ptr [rsp+0x30]
       vunpckhpd xmm7, xmm7, xmmword ptr [rsp+0x20]
       vinsertf128 ymm10, ymm10, xmm14
       vmovups  ymmword ptr [rdx], ymm10
       vinsertf128 ymm9, ymm9, xmm13
       vmovups  ymmword ptr [r8], ymm9
       vinsertf128 ymm0, ymm6, xmm0
       vmovups  ymmword ptr [r10], ymm0
       vinsertf128 ymm0, ymm11, xmm15
       vmovups  ymmword ptr [r9], ymm0
       vinsertf128 ymm0, ymm2, xmm5
       vmovups  ymmword ptr [r11], ymm0

G_M000_IG04:                ;; offset=0x01F3
       vinsertf128 ymm0, ymm1, xmm4
       vmovups  ymmword ptr [rbx], ymm0
       vinsertf128 ymm0, ymm8, xmm12
       vmovups  ymmword ptr [rsi], ymm0
       vinsertf128 ymm0, ymm3, xmm7
       vmovups  ymmword ptr [rax], ymm0

G_M000_IG05:                ;; offset=0x0211
       vzeroupper
       vmovaps  xmm6, xmmword ptr [rsp+0xE0]
       vmovaps  xmm7, xmmword ptr [rsp+0xD0]
       vmovaps  xmm8, xmmword ptr [rsp+0xC0]
       vmovaps  xmm9, xmmword ptr [rsp+0xB0]
       vmovaps  xmm10, xmmword ptr [rsp+0xA0]
       vmovaps  xmm11, xmmword ptr [rsp+0x90]
       vmovaps  xmm12, xmmword ptr [rsp+0x80]
       vmovaps  xmm13, xmmword ptr [rsp+0x70]
       vmovaps  xmm14, xmmword ptr [rsp+0x60]
       vmovaps  xmm15, xmmword ptr [rsp+0x50]
       add      rsp, 248
       pop      rbx
       pop      rsi
       ret

G_M000_IG06:                ;; offset=0x026F
       test     ecx, ecx
       je       SHORT G_M000_IG07
       mov      rdx, rax
       vmovups  ymm0, ymmword ptr [rdx]
       cmp      ecx, 1
       jbe      SHORT G_M000_IG07
       lea      r8, bword ptr [rax+0x20]
       vmovups  ymm1, ymmword ptr [r8]
       cmp      ecx, 2
       jbe      SHORT G_M000_IG07
       lea      r10, bword ptr [rax+0x40]
       vmovups  ymm2, ymmword ptr [r10]
       cmp      ecx, 3
       jbe      SHORT G_M000_IG07
       lea      r9, bword ptr [rax+0x60]
       vmovups  ymm3, ymmword ptr [r9]
       cmp      ecx, 4
       jbe      SHORT G_M000_IG07
       lea      r11, bword ptr [rax+0x80]
       vmovups  ymm4, ymmword ptr [r11]
       cmp      ecx, 5
       jbe      SHORT G_M000_IG07
       lea      rbx, bword ptr [rax+0xA0]
       vmovups  ymm5, ymmword ptr [rbx]
       cmp      ecx, 6
       jbe      SHORT G_M000_IG07
       lea      rsi, bword ptr [rax+0xC0]
       vmovups  ymm6, ymmword ptr [rsi]
       cmp      ecx, 7
       jbe      SHORT G_M000_IG07
       add      rax, 224
       vmovups  ymm7, ymmword ptr [rax]
       jmp      G_M000_IG03

G_M000_IG07:                ;; offset=0x02E9
       call     CORINFO_HELP_RNGCHKFAIL
       int3

; Total bytes of code 751

; Assembly listing for method Program:Candidate(System.Span`1[System.Runtime.Intrinsics.Vector256`1[float]]) (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data
; 0 inlinees with PGO data; 8 single block inlinees; 8 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rsi
       push     rbx
       sub      rsp, 120
       vmovaps  xmmword ptr [rsp+0x60], xmm6
       vmovaps  xmmword ptr [rsp+0x50], xmm7
       vmovaps  xmmword ptr [rsp+0x40], xmm8
       vmovaps  xmmword ptr [rsp+0x30], xmm9
       vmovaps  xmmword ptr [rsp+0x20], xmm10

G_M000_IG02:                ;; offset=0x0024
       mov      rax, bword ptr [rcx]
       mov      ecx, dword ptr [rcx+0x08]
       cmp      ecx, 7
       jle      G_M000_IG06
       mov      rdx, rax
       vmovups  ymm0, ymmword ptr [rdx]
       lea      r8, bword ptr [rax+0x20]
       vmovups  ymm1, ymmword ptr [r8]
       lea      r10, bword ptr [rax+0x40]
       vmovups  ymm2, ymmword ptr [r10]
       lea      r9, bword ptr [rax+0x60]
       vmovups  ymm3, ymmword ptr [r9]
       lea      r11, bword ptr [rax+0x80]
       vmovups  ymm4, ymmword ptr [r11]
       lea      rbx, bword ptr [rax+0xA0]
       vmovups  ymm5, ymmword ptr [rbx]
       lea      rsi, bword ptr [rax+0xC0]
       vmovups  ymm6, ymmword ptr [rsi]
       add      rax, 224
       vmovups  ymm7, ymmword ptr [rax]

G_M000_IG03:                ;; offset=0x0081
       vmovaps  xmm8, xmm0
       vmovaps  xmm9, xmm1
       vunpcklps xmm10, xmm8, xmm9
       vunpckhps xmm8, xmm8, xmm9
       vinsertf128 ymm8, ymm10, xmm8
       vextractf128 xmm0, ymm0
       vextractf128 xmm1, ymm1
       vunpcklps xmm9, xmm0, xmm1
       vunpckhps xmm0, xmm0, xmm1
       vinsertf128 ymm0, ymm9, xmm0
       vmovaps  xmm1, xmm2
       vmovaps  xmm9, xmm3
       vunpcklps xmm10, xmm1, xmm9
       vunpckhps xmm1, xmm1, xmm9
       vinsertf128 ymm1, ymm10, xmm1
       vextractf128 xmm2, ymm2
       vextractf128 xmm3, ymm3
       vunpcklps xmm9, xmm2, xmm3
       vunpckhps xmm2, xmm2, xmm3
       vinsertf128 ymm2, ymm9, xmm2
       vmovaps  xmm3, xmm4
       vmovaps  xmm9, xmm5
       vunpcklps xmm10, xmm3, xmm9
       vunpckhps xmm3, xmm3, xmm9
       vinsertf128 ymm3, ymm10, xmm3
       vextractf128 xmm4, ymm4
       vextractf128 xmm5, ymm5
       vunpcklps xmm9, xmm4, xmm5
       vunpckhps xmm4, xmm4, xmm5
       vinsertf128 ymm4, ymm9, xmm4
       vmovaps  xmm5, xmm6
       vmovaps  xmm9, xmm7
       vunpcklps xmm10, xmm5, xmm9
       vunpckhps xmm5, xmm5, xmm9
       vinsertf128 ymm5, ymm10, xmm5
       vextractf128 xmm6, ymm6
       vextractf128 xmm7, ymm7
       vunpcklps xmm9, xmm6, xmm7
       vunpckhps xmm6, xmm6, xmm7
       vinsertf128 ymm6, ymm9, xmm6
       vmovaps  xmm7, xmm8
       vmovaps  xmm9, xmm1
       vunpcklpd xmm10, xmm7, xmm9
       vunpckhpd xmm7, xmm7, xmm9
       vinsertf128 ymm7, ymm10, xmm7
       vextractf128 xmm8, ymm8
       vextractf128 xmm1, ymm1
       vunpcklpd xmm9, xmm8, xmm1
       vunpckhpd xmm1, xmm8, xmm1
       vinsertf128 ymm1, ymm9, xmm1
       vmovaps  xmm8, xmm0
       vmovaps  xmm9, xmm2
       vunpcklpd xmm10, xmm8, xmm9
       vunpckhpd xmm8, xmm8, xmm9
       vinsertf128 ymm8, ymm10, xmm8
       vextractf128 xmm0, ymm0
       vextractf128 xmm2, ymm2
       vunpcklpd xmm9, xmm0, xmm2
       vunpckhpd xmm0, xmm0, xmm2
       vinsertf128 ymm0, ymm9, xmm0
       vmovaps  xmm2, xmm3
       vmovaps  xmm9, xmm5
       vunpcklpd xmm10, xmm2, xmm9
       vunpckhpd xmm2, xmm2, xmm9
       vinsertf128 ymm2, ymm10, xmm2
       vextractf128 xmm3, ymm3
       vextractf128 xmm5, ymm5
       vunpcklpd xmm9, xmm3, xmm5

G_M000_IG04:                ;; offset=0x01D6
       vunpckhpd xmm3, xmm3, xmm5
       vinsertf128 ymm3, ymm9, xmm3
       vmovaps  xmm5, xmm4
       vmovaps  xmm9, xmm6
       vunpcklpd xmm10, xmm5, xmm9
       vunpckhpd xmm5, xmm5, xmm9
       vinsertf128 ymm5, ymm10, xmm5
       vextractf128 xmm4, ymm4
       vextractf128 xmm6, ymm6
       vunpcklpd xmm9, xmm4, xmm6
       vunpckhpd xmm4, xmm4, xmm6
       vinsertf128 ymm4, ymm9, xmm4
       vinsertf128 ymm6, ymm7, xmm2
       vmovups  ymmword ptr [rdx], ymm6
       vextractf128 xmm6, ymm7
       vextractf128 xmm2, ymm2
       vinsertf128 ymm2, ymm6, xmm2
       vmovups  ymmword ptr [r8], ymm2
       vinsertf128 ymm2, ymm1, xmm3
       vmovups  ymmword ptr [r10], ymm2
       vextractf128 xmm1, ymm1
       vextractf128 xmm2, ymm3
       vinsertf128 ymm1, ymm1, xmm2
       vmovups  ymmword ptr [r9], ymm1
       vinsertf128 ymm1, ymm8, xmm5
       vmovups  ymmword ptr [r11], ymm1
       vextractf128 xmm1, ymm8
       vextractf128 xmm2, ymm5
       vinsertf128 ymm1, ymm1, xmm2
       vmovups  ymmword ptr [rbx], ymm1
       vinsertf128 ymm1, ymm0, xmm4
       vmovups  ymmword ptr [rsi], ymm1
       vextractf128 xmm0, ymm0
       vextractf128 xmm1, ymm4
       vinsertf128 ymm0, ymm0, xmm1
       vmovups  ymmword ptr [rax], ymm0

G_M000_IG05:                ;; offset=0x0296
       vzeroupper
       vmovaps  xmm6, xmmword ptr [rsp+0x60]
       vmovaps  xmm7, xmmword ptr [rsp+0x50]
       vmovaps  xmm8, xmmword ptr [rsp+0x40]
       vmovaps  xmm9, xmmword ptr [rsp+0x30]
       vmovaps  xmm10, xmmword ptr [rsp+0x20]
       add      rsp, 120
       pop      rbx
       pop      rsi
       ret

G_M000_IG06:                ;; offset=0x02BE
       test     ecx, ecx
       je       SHORT G_M000_IG07
       mov      rdx, rax
       vmovups  ymm0, ymmword ptr [rdx]
       cmp      ecx, 1
       jbe      SHORT G_M000_IG07
       lea      r8, bword ptr [rax+0x20]
       vmovups  ymm1, ymmword ptr [r8]
       cmp      ecx, 2
       jbe      SHORT G_M000_IG07
       lea      r10, bword ptr [rax+0x40]
       vmovups  ymm2, ymmword ptr [r10]
       cmp      ecx, 3
       jbe      SHORT G_M000_IG07
       lea      r9, bword ptr [rax+0x60]
       vmovups  ymm3, ymmword ptr [r9]
       cmp      ecx, 4
       jbe      SHORT G_M000_IG07
       lea      r11, bword ptr [rax+0x80]
       vmovups  ymm4, ymmword ptr [r11]
       cmp      ecx, 5
       jbe      SHORT G_M000_IG07
       lea      rbx, bword ptr [rax+0xA0]
       vmovups  ymm5, ymmword ptr [rbx]
       cmp      ecx, 6
       jbe      SHORT G_M000_IG07
       lea      rsi, bword ptr [rax+0xC0]
       vmovups  ymm6, ymmword ptr [rsi]
       cmp      ecx, 7
       jbe      SHORT G_M000_IG07
       add      rax, 224
       vmovups  ymm7, ymmword ptr [rax]
       jmp      G_M000_IG03

G_M000_IG07:                ;; offset=0x0338
       call     CORINFO_HELP_RNGCHKFAIL
       int3

; Total bytes of code 830
