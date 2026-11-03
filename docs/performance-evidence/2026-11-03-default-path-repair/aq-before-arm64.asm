; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:.ctor(int,int,int,System.Buffers.ArrayPool`1[float],int):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 2 inlinees with PGO data; 14 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x50]!
            stp     x19, x20, [sp, #0x20]
            stp     x21, x22, [sp, #0x30]
            stp     x23, x24, [sp, #0x40]
            mov     fp, sp
            str     x0, [fp, #0x18]
            mov     w20, w1
            mov     w21, w2
            mov     w19, w3
            mov     x23, x4
            mov     w22, w5

G_M000_IG02:                ;; offset=0x002C
            movz    x0, #0x5148
            movk    x0, #0x695D LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldapr   w0, [x0]
            tbz     w0, #0, G_M000_IG11

G_M000_IG03:                ;; offset=0x0040
            movz    x14, #0xB58
            movk    x14, #0x3B80 LSL #16
            movk    x14, #524 LSL #32
            ldr     x0, [x14]
            ldr     x24, [fp, #0x18]
            add     x14, x24, #16
            mov     x15, x0
            bl      CORINFO_HELP_ASSIGN_REF
            add     x14, x24, #24
            mov     x15, x0
            bl      CORINFO_HELP_ASSIGN_REF
            add     x14, x24, #32
            mov     x15, x0
            bl      CORINFO_HELP_ASSIGN_REF
            add     x14, x24, #40
            mov     x15, x0
            bl      CORINFO_HELP_ASSIGN_REF
            add     x14, x24, #48
            mov     x15, x0
            bl      CORINFO_HELP_ASSIGN_REF
            cmp     w20, #0
            ble     G_M000_IG12
            cmp     w21, #0
            ble     G_M000_IG13

G_M000_IG04:                ;; offset=0x00A0
            sub     w14, w19, #1
            cmp     w14, #3
            bhi     G_M000_IG14
            stp     w20, w21, [x24, #0x40]
            str     w19, [x24, #0x48]
            cmp     w22, #2
            bhi     G_M000_IG15

G_M000_IG05:                ;; offset=0x00BC
            mov     w14, #3
            lsr     w14, w14, w22
            tbz     w14, #0, G_M000_IG10

G_M000_IG06:                ;; offset=0x00C8
            mov     w14, wzr

G_M000_IG07:                ;; offset=0x00CC
            strb    w14, [x24, #0x68]
            adds    w14, w20, #7
            bvs     G_M000_IG16
            asr     w15, w14, #31
            and     w15, w15, #7
            add     w14, w15, w14
            asr     w14, w14, #3
            str     w14, [x24, #0x64]
            mov     w15, #8
            smull   x15, w14, w15
            lsr     x12, x15, #32
            cmp     w12, w15,  ASR #31
            bne     G_M000_IG16
            str     w15, [x24, #0x4C]
            lsl     w12, w19, #3
            adds    wip0, w21, w12
            bvs     G_M000_IG16
            subs    wip0, wip0, #1
            bvs     G_M000_IG16
            sdiv    wip0, wip0, w12
            smull   xip0, wip0, w12
            lsr     x0, xip0, #32
            cmp     w0, wip0,  ASR #31
            bne     G_M000_IG16
            str     wip0, [x24, #0x50]
            adds    w15, w15, #2
            bvs     G_M000_IG16
            str     w15, [x24, #0x54]
            mov     wip0, #2
            smull   x14, w14, wip0
            lsr     x0, x14, #32
            cmp     w0, w14,  ASR #31
            bne     G_M000_IG16
            str     w14, [x24, #0x58]
            add     w12, w12, #10
            smull   x1, w15, w12
            lsr     xip0, x1, #32
            cmp     wip0, w1,  ASR #31
            bne     G_M000_IG16
            lsl     w15, w19, #1
            adds    w15, w15, #2
            bvs     G_M000_IG16
            smull   x21, w14, w15
            lsr     x12, x21, #32
            cmp     w12, w21,  ASR #31
            bne     G_M000_IG16
            mov     w15, #2
            smull   x20, w14, w15
            lsr     x12, x20, #32
            cmp     w12, w20,  ASR #31
            bne     G_M000_IG16
            cmp     w1, #64, LSL #12
            csel    x15, x23, xzr, le
            add     x14, x24, #8
            bl      CORINFO_HELP_ASSIGN_REF

G_M000_IG08:                ;; offset=0x01A8
            mov     x0, x24
            movz    x2, #0xFF78
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:Allocate(int):float[]:this
            mov     x15, x0
            add     x14, x24, #16
            bl      CORINFO_HELP_ASSIGN_REF
            mov     x0, x24
            mov     w1, w21
            movz    x2, #0xFF78
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:Allocate(int):float[]:this
            mov     x15, x0
            add     x14, x24, #24
            bl      CORINFO_HELP_ASSIGN_REF
            ldr     w1, [x24, #0x4C]
            mov     x0, x24
            movz    x2, #0xFF78
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:Allocate(int):float[]:this
            mov     x15, x0
            add     x14, x24, #32
            bl      CORINFO_HELP_ASSIGN_REF
            mov     x0, x24
            mov     w1, w20
            movz    x2, #0xFF78
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:Allocate(int):float[]:this
            mov     x15, x0
            add     x14, x24, #40
            bl      CORINFO_HELP_ASSIGN_REF

G_M000_IG09:                ;; offset=0x0244
            ldp     x23, x24, [sp, #0x40]
            ldp     x21, x22, [sp, #0x30]
            ldp     x19, x20, [sp, #0x20]
            ldp     fp, lr, [sp], #0x50
            ret     lr

G_M000_IG10:                ;; offset=0x0258
            mov     w14, #1
            b       G_M000_IG07

G_M000_IG11:                ;; offset=0x0260
            movz    x0, #0x50C0
            movk    x0, #0x695D LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_GET_GCSTATIC_BASE
            b       G_M000_IG03

G_M000_IG12:                ;; offset=0x0274
            mov     w0, w20
            movz    x1, #0x6198
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF48
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:ThrowNegativeOrZero[int](int,System.String)
            brk     #0

G_M000_IG13:                ;; offset=0x029C
            mov     w0, w21
            movz    x1, #0x6170
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF48
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:ThrowNegativeOrZero[int](int,System.String)
            brk     #0

G_M000_IG14:                ;; offset=0x02C4
            movz    x0, #0xDCE0
            movk    x0, #0x6947 LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x6138
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF00
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:.ctor(System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG15:                ;; offset=0x0304
            movz    x0, #0xDCE0
            movk    x0, #0x6947 LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x6108
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF00
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:.ctor(System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG16:                ;; offset=0x0344
            bl      CORINFO_HELP_OVERFLOW
            brk     #0

G_M000_IG17:                ;; offset=0x034C
            stp     fp, lr, [sp, #-0x40]!
            stp     x19, x20, [sp, #0x10]
            stp     x21, x22, [sp, #0x20]
            stp     x23, x24, [sp, #0x30]

G_M000_IG18:                ;; offset=0x035C
            ldr     x0, [fp, #0x18]
            movz    x1, #0x9238
            movk    x1, #0x6959 LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:Dispose():this
            bl      CORINFO_HELP_RETHROW
            brk     #0

; Total bytes of code 892

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:Allocate(int):float[]:this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x20]!
            str     x19, [sp, #0x18]
            mov     fp, sp
            mov     w19, w1

G_M000_IG02:                ;; offset=0x0010
            ldr     x0, [x0, #0x08]
            cbz     x0, G_M000_IG04

G_M000_IG03:                ;; offset=0x0018
            mov     w1, w19
            ldr     x2, [x0]
            ldr     x2, [x2, #0x40]
            ldr     x2, [x2, #0x20]
            blr     x2      // code for System.Buffers.ArrayPool`1[float]:Rent(int):float[]:this
            b       G_M000_IG05

G_M000_IG04:                ;; offset=0x0030
            mov     x0, xzr

G_M000_IG05:                ;; offset=0x0034
            cbnz    x0, G_M000_IG07

G_M000_IG06:                ;; offset=0x0038
            sxtw    x1, w19
            movz    x0, #0xA1E8
            movk    x0, #0x6959 LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWARR_1_VC

G_M000_IG07:                ;; offset=0x004C
            ldr     x19, [sp, #0x18]
            ldp     fp, lr, [sp], #0x20
            ret     lr

; Total bytes of code 88

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:.ctor(int,int,int,System.Buffers.ArrayPool`1[float],int):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 2 inlinees with PGO data; 13 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x30]!
            stp     x19, x20, [sp, #0x20]
            mov     fp, sp
            str     x0, [fp, #0x18]

G_M000_IG02:                ;; offset=0x0010
            movz    x14, #0x61B8
            movk    x14, #0xA15 LSL #16
            movk    x14, #548 LSL #32
            stp     x14, x14, [x0, #0x10]
            stp     x14, x14, [x0, #0x20]
            cmp     w1, #0
            ble     G_M000_IG10
            cmp     w2, #0
            ble     G_M000_IG11

G_M000_IG03:                ;; offset=0x0034
            sub     w14, w3, #1
            cmp     w14, #3
            bhi     G_M000_IG12
            stp     w1, w2, [x0, #0x38]
            str     w3, [x0, #0x40]
            cmp     w5, #2
            bhi     G_M000_IG13

G_M000_IG04:                ;; offset=0x0050
            mov     w14, #3
            lsr     w14, w14, w5
            tbz     w14, #0, G_M000_IG09

G_M000_IG05:                ;; offset=0x005C
            mov     w14, wzr

G_M000_IG06:                ;; offset=0x0060
            strb    w14, [x0, #0x60]
            adds    w14, w1, #7
            bvs     G_M000_IG14
            asr     w15, w14, #31
            and     w15, w15, #7
            add     w14, w15, w14
            asr     w14, w14, #3
            str     w14, [x0, #0x5C]
            mov     w15, #8
            smull   x15, w14, w15
            lsr     x12, x15, #32
            cmp     w12, w15,  ASR #31
            bne     G_M000_IG14
            str     w15, [x0, #0x44]
            lsl     w12, w3, #3
            adds    wip0, w2, w12
            bvs     G_M000_IG14
            subs    wip0, wip0, #1
            bvs     G_M000_IG14
            sdiv    wip0, wip0, w12
            smull   xip0, wip0, w12
            lsr     x1, xip0, #32
            cmp     w1, wip0,  ASR #31
            bne     G_M000_IG14
            str     wip0, [x0, #0x48]
            adds    w15, w15, #2
            bvs     G_M000_IG14
            str     w15, [x0, #0x4C]
            mov     wip0, #2
            smull   x14, w14, wip0
            lsr     x1, x14, #32
            cmp     w1, w14,  ASR #31
            bne     G_M000_IG14
            str     w14, [x0, #0x50]
            add     w12, w12, #10
            smull   x1, w15, w12
            lsr     xip0, x1, #32
            cmp     wip0, w1,  ASR #31
            bne     G_M000_IG14
            lsl     w15, w3, #1
            adds    w15, w15, #2
            bvs     G_M000_IG14
            smull   x19, w14, w15
            lsr     x12, x19, #32
            cmp     w12, w19,  ASR #31
            bne     G_M000_IG14
            mov     w15, #2
            smull   x20, w14, w15
            lsr     x12, x20, #32
            cmp     w12, w20,  ASR #31
            bne     G_M000_IG14
            cmp     w1, #64, LSL #12
            csel    x15, x4, xzr, le
            add     x14, x0, #8
            bl      CORINFO_HELP_ASSIGN_REF

G_M000_IG07:                ;; offset=0x013C
            movz    x2, #0xFFC0
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.StencilAdaptiveQuantization:Allocate(int):float[]:this
            mov     x15, x0
            ldr     x0, [fp, #0x18]
            add     x14, x0, #16
            bl      CORINFO_HELP_ASSIGN_REF
            mov     w1, w19
            movz    x2, #0xFFC0
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.StencilAdaptiveQuantization:Allocate(int):float[]:this
            mov     x15, x0
            ldr     x0, [fp, #0x18]
            add     x14, x0, #24
            bl      CORINFO_HELP_ASSIGN_REF
            mov     w1, w20
            movz    x2, #0xFFC0
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.StencilAdaptiveQuantization:Allocate(int):float[]:this
            mov     x15, x0
            ldr     x0, [fp, #0x18]
            add     x14, x0, #32
            bl      CORINFO_HELP_ASSIGN_REF

G_M000_IG08:                ;; offset=0x01B0
            ldp     x19, x20, [sp, #0x20]
            ldp     fp, lr, [sp], #0x30
            ret     lr

G_M000_IG09:                ;; offset=0x01BC
            mov     w14, #1
            b       G_M000_IG06

G_M000_IG10:                ;; offset=0x01C4
            mov     w0, w1
            movz    x1, #0x6198
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF48
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:ThrowNegativeOrZero[int](int,System.String)
            brk     #0

G_M000_IG11:                ;; offset=0x01EC
            mov     w0, w2
            movz    x1, #0x6170
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF48
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:ThrowNegativeOrZero[int](int,System.String)
            brk     #0

G_M000_IG12:                ;; offset=0x0214
            movz    x0, #0xDCE0
            movk    x0, #0x6947 LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x6138
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF00
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:.ctor(System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG13:                ;; offset=0x0254
            movz    x0, #0xDCE0
            movk    x0, #0x6947 LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x6108
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF00
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:.ctor(System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG14:                ;; offset=0x0294
            bl      CORINFO_HELP_OVERFLOW
            brk     #0

G_M000_IG15:                ;; offset=0x029C
            stp     fp, lr, [sp, #-0x20]!
            stp     x19, x20, [sp, #0x10]

G_M000_IG16:                ;; offset=0x02A4
            ldr     x0, [fp, #0x18]
            movz    x1, #0x9970
            movk    x1, #0x6959 LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.StencilAdaptiveQuantization:Dispose():this
            bl      CORINFO_HELP_RETHROW
            brk     #0

; Total bytes of code 708

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:Allocate(int):float[]:this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x20]!
            str     x19, [sp, #0x18]
            mov     fp, sp
            mov     w19, w1

G_M000_IG02:                ;; offset=0x0010
            ldr     x0, [x0, #0x08]
            cbz     x0, G_M000_IG04

G_M000_IG03:                ;; offset=0x0018
            mov     w1, w19
            ldr     x2, [x0]
            ldr     x2, [x2, #0x40]
            ldr     x2, [x2, #0x20]
            blr     x2      // code for System.Buffers.ArrayPool`1[float]:Rent(int):float[]:this
            b       G_M000_IG05

G_M000_IG04:                ;; offset=0x0030
            mov     x0, xzr

G_M000_IG05:                ;; offset=0x0034
            cbnz    x0, G_M000_IG07

G_M000_IG06:                ;; offset=0x0038
            sxtw    x1, w19
            movz    x0, #0xA1E8
            movk    x0, #0x6959 LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWARR_1_VC

G_M000_IG07:                ;; offset=0x004C
            ldr     x19, [sp, #0x18]
            ldp     fp, lr, [sp], #0x20
            ret     lr

; Total bytes of code 88

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:.ctor(int,int,int,System.Buffers.ArrayPool`1[float],int,int,DotJpegli.Internal.AqRationalDiagnostics):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 2 inlinees with PGO data; 13 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x30]!
            stp     x19, x20, [sp, #0x20]
            mov     fp, sp
            str     x0, [fp, #0x18]

G_M000_IG02:                ;; offset=0x0010
            movz    x14, #0x61B8
            movk    x14, #0xA15 LSL #16
            movk    x14, #548 LSL #32
            stp     x14, x14, [x0, #0x18]
            stp     x14, x14, [x0, #0x28]
            cmp     w1, #0
            ble     G_M000_IG13
            cmp     w2, #0
            ble     G_M000_IG14

G_M000_IG03:                ;; offset=0x0034
            sub     w14, w3, #1
            cmp     w14, #3
            bhi     G_M000_IG15
            stp     w1, w2, [x0, #0x40]
            str     w3, [x0, #0x48]
            cmp     w5, #2
            bhi     G_M000_IG16

G_M000_IG04:                ;; offset=0x0050
            mov     w14, #3
            lsr     w14, w14, w5
            tbz     w14, #0, G_M000_IG11

G_M000_IG05:                ;; offset=0x005C
            mov     w14, wzr

G_M000_IG06:                ;; offset=0x0060
            strb    w14, [x0, #0x68]
            cmp     w6, #1
            bls     G_M000_IG12

G_M000_IG07:                ;; offset=0x006C
            cmp     w6, #2
            bne     G_M000_IG17
            mov     w14, #1

G_M000_IG08:                ;; offset=0x0078
            strb    w14, [x0, #0x69]
            add     x14, x0, #16
            mov     x15, x7
            bl      CORINFO_HELP_ASSIGN_REF
            adds    w14, w1, #7
            bvs     G_M000_IG18
            asr     w15, w14, #31
            and     w15, w15, #7
            add     w14, w15, w14
            asr     w14, w14, #3
            str     w14, [x0, #0x64]
            mov     w15, #8
            smull   x15, w14, w15
            lsr     x12, x15, #32
            cmp     w12, w15,  ASR #31
            bne     G_M000_IG18
            str     w15, [x0, #0x4C]
            lsl     w12, w3, #3
            adds    wip0, w2, w12
            bvs     G_M000_IG18
            subs    wip0, wip0, #1
            bvs     G_M000_IG18
            sdiv    wip0, wip0, w12
            smull   xip0, wip0, w12
            lsr     x1, xip0, #32
            cmp     w1, wip0,  ASR #31
            bne     G_M000_IG18
            str     wip0, [x0, #0x50]
            adds    w15, w15, #2
            bvs     G_M000_IG18
            str     w15, [x0, #0x54]
            mov     wip0, #2
            smull   x14, w14, wip0
            lsr     x1, x14, #32
            cmp     w1, w14,  ASR #31
            bne     G_M000_IG18
            str     w14, [x0, #0x58]
            add     w12, w12, #10
            smull   x1, w15, w12
            lsr     xip0, x1, #32
            cmp     wip0, w1,  ASR #31
            bne     G_M000_IG18
            lsl     w15, w3, #1
            adds    w15, w15, #2
            bvs     G_M000_IG18
            smull   x19, w14, w15
            lsr     x12, x19, #32
            cmp     w12, w19,  ASR #31
            bne     G_M000_IG18
            mov     w15, #2
            smull   x20, w14, w15
            lsr     x12, x20, #32
            cmp     w12, w20,  ASR #31
            bne     G_M000_IG18
            cmp     w1, #64, LSL #12
            csel    x15, x4, xzr, le
            add     x14, x0, #8
            bl      CORINFO_HELP_ASSIGN_REF

G_M000_IG09:                ;; offset=0x0160
            movz    x2, #0xFFD8
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.AdaptiveQuantization:Allocate(int):float[]:this
            mov     x15, x0
            ldr     x0, [fp, #0x18]
            add     x14, x0, #24
            bl      CORINFO_HELP_ASSIGN_REF
            mov     w1, w19
            movz    x2, #0xFFD8
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.AdaptiveQuantization:Allocate(int):float[]:this
            mov     x15, x0
            ldr     x0, [fp, #0x18]
            add     x14, x0, #32
            bl      CORINFO_HELP_ASSIGN_REF
            mov     w1, w20
            movz    x2, #0xFFD8
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.AdaptiveQuantization:Allocate(int):float[]:this
            mov     x15, x0
            ldr     x0, [fp, #0x18]
            add     x14, x0, #40
            bl      CORINFO_HELP_ASSIGN_REF

G_M000_IG10:                ;; offset=0x01D4
            ldp     x19, x20, [sp, #0x20]
            ldp     fp, lr, [sp], #0x30
            ret     lr

G_M000_IG11:                ;; offset=0x01E0
            mov     w14, #1
            b       G_M000_IG06

G_M000_IG12:                ;; offset=0x01E8
            mov     w14, wzr
            b       G_M000_IG08

G_M000_IG13:                ;; offset=0x01F0
            mov     w0, w1
            movz    x1, #0x6198
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF48
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:ThrowNegativeOrZero[int](int,System.String)
            brk     #0

G_M000_IG14:                ;; offset=0x0218
            mov     w0, w2
            movz    x1, #0x6170
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF48
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:ThrowNegativeOrZero[int](int,System.String)
            brk     #0

G_M000_IG15:                ;; offset=0x0240
            movz    x0, #0xDCE0
            movk    x0, #0x6947 LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x6138
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF00
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:.ctor(System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG16:                ;; offset=0x0280
            movz    x0, #0xDCE0
            movk    x0, #0x6947 LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x6108
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF00
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:.ctor(System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG17:                ;; offset=0x02C0
            movz    x0, #0xDCE0
            movk    x0, #0x6947 LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x61D0
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF00
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:.ctor(System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG18:                ;; offset=0x0300
            bl      CORINFO_HELP_OVERFLOW
            brk     #0

G_M000_IG19:                ;; offset=0x0308
            stp     fp, lr, [sp, #-0x20]!
            stp     x19, x20, [sp, #0x10]

G_M000_IG20:                ;; offset=0x0310
            ldr     x0, [fp, #0x18]
            movz    x1, #0xA168
            movk    x1, #0x6959 LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AdaptiveQuantization:Dispose():this
            bl      CORINFO_HELP_RETHROW
            brk     #0

; Total bytes of code 816

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:Allocate(int):float[]:this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x20]!
            str     x19, [sp, #0x18]
            mov     fp, sp
            mov     w19, w1

G_M000_IG02:                ;; offset=0x0010
            ldr     x0, [x0, #0x08]
            cbz     x0, G_M000_IG04

G_M000_IG03:                ;; offset=0x0018
            mov     w1, w19
            ldr     x2, [x0]
            ldr     x2, [x2, #0x40]
            ldr     x2, [x2, #0x20]
            blr     x2      // code for System.Buffers.ArrayPool`1[float]:Rent(int):float[]:this
            b       G_M000_IG05

G_M000_IG04:                ;; offset=0x0030
            mov     x0, xzr

G_M000_IG05:                ;; offset=0x0034
            cbnz    x0, G_M000_IG07

G_M000_IG06:                ;; offset=0x0038
            sxtw    x1, w19
            movz    x0, #0xA1E8
            movk    x0, #0x6959 LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWARR_1_VC

G_M000_IG07:                ;; offset=0x004C
            ldr     x19, [sp, #0x18]
            ldp     fp, lr, [sp], #0x20
            ret     lr

; Total bytes of code 88

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeRowCore(DotJpegli.Internal.PixelSource,int,int,int,System.Span`1[float],bool,bool,System.Threading.CancellationToken):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data
; 9 inlinees with PGO data; 35 single block inlinees; 33 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            sub     sp, sp, #288
            stp     d8, d9, [sp, #0x88]
            stp     d10, d11, [sp, #0x98]
            stp     d12, d13, [sp, #0xA8]
            str     d14, [sp, #0xB8]
            stp     x19, x20, [sp, #0xC0]
            stp     x21, x22, [sp, #0xD0]
            stp     x23, x24, [sp, #0xE0]
            stp     x25, x26, [sp, #0xF0]
            stp     x27, x28, [sp, #0x100]
            stp     fp, lr, [sp, #0x110]
            add     fp, sp, #272
            str     xzr, [fp, #-0xE0]
            movz    x8, #0x2D5A
            movk    x8, #0xF7B0 LSL #16
            movk    x8, #0x2883 LSL #32
            str     x8, [fp, #-0x90]
            mov     x19, x0
            mov     x20, x1
            mov     w23, w2
            mov     w21, w3
            mov     w24, w4
            mov     w22, w7

G_M000_IG02:                ;; offset=0x005C
            ldr     x2, [fp, #0x18]
            str     x2, [fp, #-0xE0]
            mov     x25, x5
            mov     w26, w6
            ldrb    w2, [x19, #0x69]
            cbnz    w2, G_M000_IG126
            ldr     w2, [x19, #0x48]
            lsl     w27, w2, #3
            ldr     x28, [x20, #0x08]
            cbz     x28, G_M000_IG04

G_M000_IG03:                ;; offset=0x0084
            ldr     x2, [x19, #0x38]
            cmp     x28, x2
            bne     G_M000_IG04
            ldr     w2, [x19, #0x5C]
            add     w2, w2, w27
            cmp     w2, w21
            bne     G_M000_IG04
            ldr     w2, [x19, #0x60]
            cmp     w23, w2
            beq     G_M000_IG05

G_M000_IG04:                ;; offset=0x00AC
            mov     w3, wzr
            b       G_M000_IG06

G_M000_IG05:                ;; offset=0x00B4
            uxtb    w4, w22
            ldrb    w3, [x19, #0x6A]
            cmp     w4, w3
            cset    x3, eq

G_M000_IG06:                ;; offset=0x00C4
            str     w3, [fp, #-0x94]
            str     xzr, [x19, #0x38]
            ldr     x2, [fp, #-0xE0]
            cbnz    x2, G_M000_IG123

G_M000_IG07:                ;; offset=0x00D4
            tbnz    w21, #31, G_M000_IG128
            ldr     w2, [x19, #0x44]
            cmp     w21, w2
            bge     G_M000_IG128
            cbz     w27, G_M000_IG137
            sdiv    w2, w21, w27
            msub    w2, w2, w27, w21
            cbnz    w2, G_M000_IG128
            ldr     w2, [x19, #0x64]
            ldr     w0, [x19, #0x48]
            mul     w2, w2, w0
            cmp     w2, w26
            bgt     G_M000_IG129
            sub     w5, w21, #5
            str     w5, [fp, #-0x98]
            mov     w6, wzr
            cbz     w3, G_M000_IG124

G_M000_IG08:                ;; offset=0x0118
            ldr     x2, [x19, #0x10]
            mov     x0, x2
            ldr     w1, [x19, #0x54]
            mul     w4, w27, w1
            mov     w6, #10
            mul     w1, w1, w6
            cbnz    x0, G_M000_IG125
            orr     w0, w4, w1
            cbnz    w0, G_M000_IG130
            mov     x4, xzr
            mov     w1, wzr

G_M000_IG09:                ;; offset=0x0144
            cbz     x2, G_M000_IG10
            add     x0, x2, #16
            ldr     w2, [x2, #0x08]
            b       G_M000_IG11

G_M000_IG10:                ;; offset=0x0154
            mov     x0, xzr
            mov     w2, wzr

G_M000_IG11:                ;; offset=0x015C
            cmp     w1, w2
            bgt     G_M000_IG131
            ubfiz   x2, x1, #2, #32
            mov     x1, x4
            movz    x4, #0x56E0
            movk    x4, #0x6938 LSL #16
            movk    x4, #0x7FF8 LSL #32
            ldr     x4, [x4]
            blr     x4      // code for System.SpanHelpers:Memmove(byref,byref,nuint)
            mov     w6, #10
            mov     w3, w6
            ldr     w4, [fp, #-0x98]
            add     w1, w4, #9
            ldr     w0, [x19, #0x44]
            sub     w5, w0, #1
            tbnz    w5, #31, G_M000_IG132
            tbnz    w1, #31, G_M000_IG12
            cmp     w1, w5
            csel    w7, w1, w5, le
            b       G_M000_IG13

G_M000_IG12:                ;; offset=0x01AC
            mov     w7, wzr

G_M000_IG13:                ;; offset=0x01B0
            mov     w6, w3

G_M000_IG14:                ;; offset=0x01B4
            mov     w4, w6
            b       G_M000_IG52

G_M000_IG15:                ;; offset=0x01BC
            ldr     x1, [fp, #-0xE0]
            add     x1, x1, #32
            ldapr   w1, [x1]
            cbz     w1, G_M000_IG54
            b       G_M000_IG127

G_M000_IG16:                ;; offset=0x01D0
            mov     w6, w8
            b       G_M000_IG18

G_M000_IG17:                ;; offset=0x01D8
            mov     w6, wzr

G_M000_IG18:                ;; offset=0x01DC
            str     w6, [fp, #-0xA0]
            ldr     w8, [x19, #0x54]
            str     w4, [fp, #-0x9C]
            mul     w9, w4, w8
            add     w10, w9, #1
            str     w10, [fp, #-0xA4]
            cmp     w6, w7
            beq     G_M000_IG56

G_M000_IG19:                ;; offset=0x01FC
            uxtb    w0, w22
            mov     w11, w0
            cbnz    w11, G_M000_IG40
            ldr     w7, [x19, #0x40]
            mov     w8, w7
            str     w8, [fp, #-0xBC]
            ldr     w0, [x20, #0x1C]
            cbnz    w0, G_M000_IG22
            ldrb    w0, [x20, #0x26]
            cbz     w0, G_M000_IG20
            mov     w0, #4
            b       G_M000_IG22

G_M000_IG20:                ;; offset=0x022C
            ldr     w0, [x20, #0x20]
            cmp     w0, #1
            bne     G_M000_IG21
            mov     w0, #1
            b       G_M000_IG22

G_M000_IG21:                ;; offset=0x0240
            mov     w0, #2
            b       G_M000_IG22

G_M000_IG22:                ;; offset=0x0248
            cmp     w0, #3
            bne     G_M000_IG23
            mov     w9, wzr
            b       G_M000_IG24

G_M000_IG23:                ;; offset=0x0258
            cmp     w0, #4
            cset    x9, ne

G_M000_IG24:                ;; offset=0x0260
            cmp     w9, #0
            cset    x9, eq
            str     w9, [fp, #-0xC0]
            ldr     x0, [x19, #0x10]
            cbz     x0, G_M000_IG25
            mov     w11, w10
            add     x11, x11, w7, UXTW
            ldr     w13, [x0, #0x08]
            cmp     x11, x13
            bhi     G_M000_IG130
            add     x0, x0, #16
            ubfiz   x11, x10, #2, #32
            add     x11, x0, x11
            b       G_M000_IG26

G_M000_IG25:                ;; offset=0x0298
            orr     w11, w10, w7
            cbnz    w11, G_M000_IG130
            mov     x11, xzr
            mov     w7, wzr

G_M000_IG26:                ;; offset=0x02A8
            str     x11, [fp, #-0xF8]
            mov     w14, w7
            str     w14, [fp, #-0xD4]
            ldr     x0, [x20, #0x08]
            cbz     x0, G_M000_IG29
            ldrb    w0, [x0, #0x2C]
            cbnz    w0, G_M000_IG28

G_M000_IG27:                ;; offset=0x02C4
            b       G_M000_IG39

G_M000_IG28:                ;; offset=0x02C8
            cbz     w9, G_M000_IG39

G_M000_IG29:                ;; offset=0x02CC
            cmp     w6, w1
            bgt     G_M000_IG30
            mov     w0, w6
            ldr     w6, [fp, #-0xA0]
            b       G_M000_IG31

G_M000_IG30:                ;; offset=0x02E0
            mov     w0, w1

G_M000_IG31:                ;; offset=0x02E4
            mul     w7, w0, w23
            str     w7, [fp, #-0xC4]
            mov     w11, wzr
            cbnz    w14, G_M000_IG37

G_M000_IG32:                ;; offset=0x02F4
            b       G_M000_IG48

G_M000_IG33:                ;; offset=0x02F8
            mov     w0, w1
            str     w11, [fp, #-0xC8]
            b       G_M000_IG35

G_M000_IG34:                ;; offset=0x0304
            mov     w0, wzr
            str     w11, [fp, #-0xC8]

G_M000_IG35:                ;; offset=0x030C
            ldr     w1, [x20, #0x20]
            madd    w1, w0, w1, w7
            mov     x0, x20
            mov     w2, w9
            movz    x12, #0x44B0
            movk    x12, #0x695F LSL #16
            movk    x12, #0x7FF8 LSL #32
            ldr     x12, [x12]
            blr     x12      // code for DotJpegli.Internal.PixelSource:ReadSample(int,int):float:this
            ldr     x6, [fp, #-0xF8]
            ldr     w7, [fp, #-0xC8]
            str     s0, [x6, w7, UXTW #2]
            add     w7, w7, #1
            ldr     w0, [fp, #-0xD4]
            cmp     w7, w0
            mov     w11, w7
            ldp     w9, w8, [fp, #-0xC0]
            bge     G_M000_IG48

G_M000_IG36:                ;; offset=0x0354
            ldr     w6, [fp, #-0xA0]
            ldr     w7, [fp, #-0xC4]

G_M000_IG37:                ;; offset=0x035C
            sub     w1, w8, #1
            tbnz    w1, #31, G_M000_IG134
            ldr     w8, [fp, #-0xBC]
            tbnz    w11, #31, G_M000_IG34

G_M000_IG38:                ;; offset=0x036C
            cmp     w11, w1
            bgt     G_M000_IG33
            str     w11, [fp, #-0xC8]
            mov     w0, w11
            b       G_M000_IG35

G_M000_IG39:                ;; offset=0x0380
            mov     x6, x11
            mov     x0, x20
            mov     w1, w8
            mov     w3, w9
            ldr     w5, [fp, #-0xA0]
            mov     w4, wzr
            movz    x8, #0x4498
            movk    x8, #0x695F LSL #16
            movk    x8, #0x7FF8 LSL #32
            ldr     x8, [x8]
            blr     x8      // code for DotJpegli.Internal.PixelSource:ReadConvertedRow(int,int,int,int,int,System.Span`1[float]):this
            b       G_M000_IG48

G_M000_IG40:                ;; offset=0x03B0
            ldr     w7, [x19, #0x40]
            mov     w1, w7
            ldr     w0, [x20, #0x1C]
            cbnz    w0, G_M000_IG43
            ldrb    w0, [x20, #0x26]
            cbz     w0, G_M000_IG41
            mov     w0, #4
            b       G_M000_IG43

G_M000_IG41:                ;; offset=0x03D0
            ldr     w0, [x20, #0x20]
            cmp     w0, #1
            bne     G_M000_IG42
            mov     w0, #1
            b       G_M000_IG43

G_M000_IG42:                ;; offset=0x03E4
            mov     w0, #2
            b       G_M000_IG43

G_M000_IG43:                ;; offset=0x03EC
            cmp     w0, #3
            bne     G_M000_IG44
            mov     w8, wzr
            b       G_M000_IG45

G_M000_IG44:                ;; offset=0x03FC
            cmp     w0, #4
            cset    x8, ne

G_M000_IG45:                ;; offset=0x0404
            cmp     w8, #0
            cset    x8, eq
            ldr     x0, [x19, #0x10]
            cbz     x0, G_M000_IG46
            mov     w9, w10
            add     x9, x9, w7, UXTW
            ldr     w11, [x0, #0x08]
            cmp     x9, x11
            bhi     G_M000_IG130
            add     x0, x0, #16
            ubfiz   x9, x10, #2, #32
            add     x0, x0, x9
            b       G_M000_IG47

G_M000_IG46:                ;; offset=0x0438
            orr     w0, w10, w7
            cbnz    w0, G_M000_IG130
            mov     x0, xzr
            mov     w7, wzr

G_M000_IG47:                ;; offset=0x0448
            str     x0, [sp]
            str     w7, [sp, #0x08]
            mov     x0, x20
            mov     w3, w23
            mov     w4, w8
            mov     w5, wzr
            movz    x7, #0x4480
            movk    x7, #0x695F LSL #16
            movk    x7, #0x7FF8 LSL #32
            ldr     x7, [x7]
            blr     x7      // code for DotJpegli.Internal.PixelSource:ReadRowVector(int,int,int,int,int,int,System.Span`1[float]):this

G_M000_IG48:                ;; offset=0x0474
            ldr     x1, [x19, #0x10]
            mov     x0, x1
            ldr     w2, [x0, #0x08]
            ldr     w10, [fp, #-0xA4]
            cmp     w10, w2
            bhs     G_M000_IG138
            add     x0, x0, #16
            ldr     s0, [x0, w10, UXTW #2]
            sub     w3, w10, #1
            cmp     w3, w2
            bhs     G_M000_IG138
            sub     w3, w10, #1
            str     s0, [x0, w3, UXTW #2]
            ldr     w3, [x19, #0x40]
            add     w4, w10, w3
            mov     w5, w4
            ldr     w6, [x19, #0x4C]
            sub     w3, w6, w3
            add     w3, w3, #1
            cbz     x1, G_M000_IG49
            mov     w6, w5
            add     x6, x6, w3, UXTW
            cmp     x6, w2, UXTW
            bhi     G_M000_IG130
            add     x1, x1, #16
            ubfiz   x5, x5, #2, #32
            add     x5, x1, x5
            b       G_M000_IG50

G_M000_IG49:                ;; offset=0x04E4
            orr     w1, w5, w3
            cbnz    w1, G_M000_IG130
            mov     x5, xzr
            mov     w3, wzr

G_M000_IG50:                ;; offset=0x04F4
            sub     w1, w4, #1
            cmp     w1, w2
            bhs     G_M000_IG138
            ldr     s0, [x0, w1, UXTW #2]
            mov     w1, w3
            mov     x0, x5
            movz    x2, #0x42E8
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.SpanHelpers:Fill[float](byref,nuint,float)

G_M000_IG51:                ;; offset=0x0520
            ldp     w7, w1, [fp, #-0xA0]
            add     w1, w1, #1
            mov     w4, w1

G_M000_IG52:                ;; offset=0x052C
            add     w1, w27, #10
            cmp     w4, w1
            bge     G_M000_IG61

G_M000_IG53:                ;; offset=0x0538
            ldr     x1, [fp, #-0xE0]
            cbnz    x1, G_M000_IG15

G_M000_IG54:                ;; offset=0x0540
            ldr     w5, [fp, #-0x98]
            add     w6, w5, w4
            ldr     w2, [x19, #0x44]
            sub     w1, w2, #1
            mov     w8, w1
            tbnz    w8, #31, G_M000_IG133
            tbnz    w6, #31, G_M000_IG17

G_M000_IG55:                ;; offset=0x055C
            cmp     w6, w8
            bgt     G_M000_IG16
            b       G_M000_IG18

G_M000_IG56:                ;; offset=0x0568
            ldr     x2, [x19, #0x10]
            ldr     w4, [fp, #-0x9C]
            sub     w0, w4, #1
            mul     w0, w0, w8
            mov     w1, w8
            cbz     x2, G_M000_IG57
            mov     w7, w0
            add     x7, x7, w1, UXTW
            ldr     w10, [x2, #0x08]
            cmp     x7, x10
            bhi     G_M000_IG130
            add     x2, x2, #16
            ubfiz   x0, x0, #2, #32
            add     x7, x2, x0
            b       G_M000_IG58

G_M000_IG57:                ;; offset=0x05A4
            orr     w7, w0, w1
            cbnz    w7, G_M000_IG130
            mov     x7, xzr
            mov     w1, wzr

G_M000_IG58:                ;; offset=0x05B4
            ldr     x2, [x19, #0x10]
            cbz     x2, G_M000_IG59
            mov     w0, w9
            add     x0, x0, w8, UXTW
            ldr     w10, [x2, #0x08]
            cmp     x0, x10
            bhi     G_M000_IG130
            add     x2, x2, #16
            ubfiz   x9, x9, #2, #32
            add     x0, x2, x9
            b       G_M000_IG60

G_M000_IG59:                ;; offset=0x05E0
            orr     w0, w9, w8
            cbnz    w0, G_M000_IG130
            mov     x0, xzr
            mov     w8, wzr

G_M000_IG60:                ;; offset=0x05F0
            cmp     w1, w8
            bgt     G_M000_IG131
            ubfiz   x2, x1, #2, #32
            mov     x1, x7
            movz    x7, #0x56E0
            movk    x7, #0x6938 LSL #16
            movk    x7, #0x7FF8 LSL #32
            ldr     x7, [x7]
            blr     x7      // code for System.SpanHelpers:Memmove(byref,byref,nuint)
            b       G_M000_IG51

G_M000_IG61:                ;; offset=0x0618
            lsr     w2, w21, #2
            sub     w20, w2, #1
            ldr     w2, [x19, #0x50]
            asr     w0, w2, #31
            and     w0, w0, #3
            add     w2, w0, w2
            asr     w27, w2, #2
            mov     w4, wzr
            ldr     w3, [fp, #-0x94]
            cbnz    w3, G_M000_IG63

G_M000_IG62:                ;; offset=0x0640
            movn    w3, #0
            b       G_M000_IG70

G_M000_IG63:                ;; offset=0x0648
            ldr     x2, [x19, #0x18]
            mov     x0, x2
            ldr     w1, [x19, #0x48]
            lsl     w1, w1, #1
            ldr     w3, [x19, #0x58]
            mul     w1, w1, w3
            lsl     w3, w3, #1
            cbz     x0, G_M000_IG64
            mov     w4, w1
            add     x4, x4, w3, UXTW
            ldr     w6, [x0, #0x08]
            cmp     x4, x6
            bhi     G_M000_IG130
            add     x0, x0, #16
            ubfiz   x1, x1, #2, #32
            add     x1, x0, x1
            b       G_M000_IG65

G_M000_IG64:                ;; offset=0x068C
            orr     w0, w1, w3
            cbnz    w0, G_M000_IG130
            mov     x1, xzr
            mov     w3, wzr

G_M000_IG65:                ;; offset=0x069C
            cbz     x2, G_M000_IG66
            add     x0, x2, #16
            ldr     w2, [x2, #0x08]
            b       G_M000_IG67

G_M000_IG66:                ;; offset=0x06AC
            mov     x0, xzr
            mov     w2, wzr

G_M000_IG67:                ;; offset=0x06B4
            cmp     w3, w2
            bgt     G_M000_IG131
            ubfiz   x2, x3, #2, #32
            movz    x3, #0x56E0
            movk    x3, #0x6938 LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for System.SpanHelpers:Memmove(byref,byref,nuint)
            mov     w4, #2
            add     w1, w20, #1
            sub     w5, w27, #1
            tbnz    w5, #31, G_M000_IG135
            tbnz    w1, #31, G_M000_IG68
            cmp     w1, w5
            csel    w1, w1, w5, le
            b       G_M000_IG69

G_M000_IG68:                ;; offset=0x06F4
            mov     w1, wzr

G_M000_IG69:                ;; offset=0x06F8
            lsl     w3, w1, #2

G_M000_IG70:                ;; offset=0x06FC
            b       G_M000_IG84

G_M000_IG71:                ;; offset=0x0700
            ldr     x1, [fp, #-0xE0]
            cbnz    x1, G_M000_IG122

G_M000_IG72:                ;; offset=0x0708
            add     w1, w20, w4
            sub     w6, w27, #1
            tbnz    w6, #31, G_M000_IG136
            tbnz    w1, #31, G_M000_IG75

G_M000_IG73:                ;; offset=0x0718
            cmp     w1, w6
            bgt     G_M000_IG74
            b       G_M000_IG76

G_M000_IG74:                ;; offset=0x0724
            mov     w1, w6
            b       G_M000_IG76

G_M000_IG75:                ;; offset=0x072C
            mov     w1, wzr

G_M000_IG76:                ;; offset=0x0730
            lsl     w6, w1, #2
            str     w6, [fp, #-0xAC]
            cmp     w6, w3
            beq     G_M000_IG78

G_M000_IG77:                ;; offset=0x0740
            ldr     w5, [fp, #-0x98]
            sub     w1, w6, w5
            uxtb    w11, w22
            mov     w7, w11
            mov     w3, w7
            mov     x0, x19
            str     w4, [fp, #-0xA8]
            mov     w2, w4
            movz    x7, #0x44C8
            movk    x7, #0x695F LSL #16
            movk    x7, #0x7FF8 LSL #32
            ldr     x7, [x7]
            blr     x7      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:ComputePreErosion(int,int,bool):this
            b       G_M000_IG83

G_M000_IG78:                ;; offset=0x0778
            ldr     x2, [x19, #0x18]
            mov     x0, x2
            sub     w1, w4, #1
            ldr     w3, [x19, #0x58]
            mul     w1, w1, w3
            mov     w7, w3
            cbz     x0, G_M000_IG79
            mov     w8, w1
            add     x8, x8, w7, UXTW
            ldr     w9, [x0, #0x08]
            cmp     x8, x9
            bhi     G_M000_IG130
            add     x0, x0, #16
            ubfiz   x1, x1, #2, #32
            add     x1, x0, x1
            b       G_M000_IG80

G_M000_IG79:                ;; offset=0x07B8
            orr     w0, w1, w7
            cbnz    w0, G_M000_IG130
            mov     x1, xzr
            mov     w7, wzr

G_M000_IG80:                ;; offset=0x07C8
            str     w4, [fp, #-0xA8]
            mul     w0, w4, w3
            cbz     x2, G_M000_IG81
            mov     w8, w0
            add     x8, x8, w3, UXTW
            ldr     w9, [x2, #0x08]
            cmp     x8, x9
            bhi     G_M000_IG130
            add     x2, x2, #16
            ubfiz   x0, x0, #2, #32
            add     x0, x2, x0
            b       G_M000_IG82

G_M000_IG81:                ;; offset=0x07F8
            orr     w2, w0, w3
            cbnz    w2, G_M000_IG130
            mov     x0, xzr
            mov     w3, wzr

G_M000_IG82:                ;; offset=0x0808
            cmp     w7, w3
            bgt     G_M000_IG131
            ubfiz   x2, x7, #2, #32
            movz    x3, #0x56E0
            movk    x3, #0x6938 LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for System.SpanHelpers:Memmove(byref,byref,nuint)

G_M000_IG83:                ;; offset=0x0828
            ldp     w3, w4, [fp, #-0xAC]
            add     w4, w4, #1

G_M000_IG84:                ;; offset=0x0830
            ldr     w1, [x19, #0x48]
            lsl     w1, w1, #1
            add     w1, w1, #2
            cmp     w4, w1
            blt     G_M000_IG71

G_M000_IG85:                ;; offset=0x0844
            cmp     w24, #9
            bge     G_M000_IG87

G_M000_IG86:                ;; offset=0x084C
            fmov    s16, #1.0000
            b       G_M000_IG88

G_M000_IG87:                ;; offset=0x0854
            scvtf   s16, w24
            fmov    s17, #9.0000
            fsub    s16, s16, s17
            ldr     s17, [@RWD00]
            fdiv    s16, s16, s17
            fmov    s17, #1.0000
            fsub    s16, s17, s16
            movi    v17.2s, #0
            fmax    s16, s16, s17

G_M000_IG88:                ;; offset=0x0878
            ldr     s17, [@RWD04]
            fmul    s8, s16, s17
            fmov    s17, #1.0000
            fsub    s16, s17, s16
            ldr     s17, [@RWD08]
            fmul    s9, s16, s17
            uxtb    w7, w22
            mov     w20, w7
            cbz     w20, G_M000_IG90

G_M000_IG89:                ;; offset=0x089C
            ldr     w0, [x19, #0x64]
            cmp     w0, #8
            bge     G_M000_IG91

G_M000_IG90:                ;; offset=0x08A8
            mov     w24, wzr
            b       G_M000_IG92

G_M000_IG91:                ;; offset=0x08B0
            ldrb    w24, [fp, #0x10]

G_M000_IG92:                ;; offset=0x08B4
            cbnz    w24, G_M000_IG94

G_M000_IG93:                ;; offset=0x08B8
            mov     x27, xzr
            mov     w2, wzr
            mov     w4, w2
            b       G_M000_IG95

G_M000_IG94:                ;; offset=0x08C8
            add     sp, sp, #16
            ldp     x0, xzr, [sp], #-0x200
            sub     sp, sp, #16
            add     x0, sp, #16
            str     x0, [fp, #-0xE8]
            mov     x1, #512
            bl      CORINFO_HELP_MEMZERO
            ldr     x0, [fp, #-0xE8]
            mov     x27, x0
            mov     w2, #16
            mov     w4, w2

G_M000_IG95:                ;; offset=0x08F4
            str     w4, [fp, #-0xCC]
            cbnz    w24, G_M000_IG97

G_M000_IG96:                ;; offset=0x08FC
            mov     x5, xzr
            mov     w6, wzr
            b       G_M000_IG98

G_M000_IG97:                ;; offset=0x0908
            add     sp, sp, #16
            ldr     wzr, [sp], #-0x40
            sub     sp, sp, #16
            add     x5, sp, #16
            movi    v16.16b, #0
            stp     q16, q16, [x5]
            stp     q16, q16, [x5, #0x20]
            mov     w6, #16

G_M000_IG98:                ;; offset=0x0928
            str     x5, [fp, #-0xF0]
            str     w6, [fp, #-0xD0]
            mov     w7, wzr
            ldr     s10, [@RWD12]
            ldr     s11, [@RWD16]
            ldr     s12, [@RWD20]
            b       G_M000_IG119

G_M000_IG99:                ;; offset=0x0944
            ldr     x1, [fp, #-0xE0]
            cbnz    x1, G_M000_IG108

G_M000_IG100:                ;; offset=0x094C
            str     w7, [fp, #-0xB0]
            lsl     w1, w7, #1
            add     w1, w1, #1
            mov     w3, w20
            mov     x0, x19
            mov     w2, wzr
            movz    x8, #0x44E0
            movk    x8, #0x695F LSL #16
            movk    x8, #0x7FF8 LSL #32
            ldr     x8, [x8]
            blr     x8      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this
            ldr     w4, [fp, #-0xB0]
            lsl     w1, w4, #1
            add     w1, w1, #2
            ldr     w2, [x19, #0x58]
            mov     w3, w20
            mov     x0, x19
            movz    x5, #0x44E0
            movk    x5, #0x695F LSL #16
            movk    x5, #0x7FF8 LSL #32
            ldr     x5, [x5]
            blr     x5      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this
            mov     w7, wzr
            cbz     w24, G_M000_IG116
            b       G_M000_IG115

G_M000_IG101:                ;; offset=0x09B0
            ldr     w3, [fp, #-0xB0]
            lsl     w0, w3, #3
            add     w0, w0, #5
            ldr     w1, [x19, #0x54]
            ldr     w4, [fp, #-0xB4]
            lsl     w2, w4, #3
            madd    w0, w0, w1, w2
            add     w0, w0, #1
            ldr     x1, [x19, #0x10]
            cbz     x1, G_M000_IG102
            ldr     w2, [x1, #0x08]
            cmp     w2, w0
            blo     G_M000_IG130
            add     x2, x1, #16
            ubfiz   x5, x0, #2, #32
            add     x2, x2, x5
            ldr     w1, [x1, #0x08]
            sub     w1, w1, w0
            b       G_M000_IG103

G_M000_IG102:                ;; offset=0x09FC
            cbnz    w0, G_M000_IG130
            mov     x2, xzr
            mov     w1, wzr

G_M000_IG103:                ;; offset=0x0A08
            mov     x0, x2
            ldr     w2, [x19, #0x54]
            movz    x5, #0x4570
            movk    x5, #0x695F LSL #16
            movk    x5, #0x7FF8 LSL #32
            ldr     x5, [x5]
            blr     x5      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:ModulateScalar(System.ReadOnlySpan`1[float],int,float):float

G_M000_IG104:                ;; offset=0x0A24
            fmul    s0, s0, s11
            movz    x0, #0x4540
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:FastPow2(float):float
            fmul    s16, s0, s8
            fadd    s16, s16, s9
            ldr     w0, [x19, #0x64]
            ldp     w4, w3, [fp, #-0xB4]
            madd    w0, w3, w0, w4
            cmp     w0, w26
            bhs     G_M000_IG138
            fdiv    s16, s12, s16
            fmov    s17, #1.0000
            fsub    s16, s16, s17
            movi    v17.2s, #0
            fmax    s16, s16, s17
            str     s16, [x25, w0, UXTW #2]
            add     w4, w4, #1
            ldr     w0, [x19, #0x64]
            cmp     w4, w0
            mov     w7, w4
            bge     G_M000_IG118

G_M000_IG105:                ;; offset=0x0A84
            str     w3, [fp, #-0xB0]

G_M000_IG106:                ;; offset=0x0A88
            str     w7, [fp, #-0xB4]
            lsl     w0, w7, #1
            ldr     x1, [x19, #0x28]
            ldr     w3, [x19, #0x58]
            add     w3, w3, w0
            ldr     w4, [x1, #0x08]
            cmp     w3, w4
            bhs     G_M000_IG138
            add     x1, x1, #16
            ldr     s0, [x1, w3, UXTW #2]
            ldr     x1, [x19, #0x28]
            ldr     w3, [x19, #0x58]
            add     w3, w3, w0
            add     w3, w3, #1
            ldr     w4, [x1, #0x08]
            cmp     w3, w4
            bhs     G_M000_IG138
            add     x1, x1, #16
            ldr     s16, [x1, w3, UXTW #2]
            fadd    s0, s0, s16
            ldr     x1, [x19, #0x28]
            ldr     w3, [x1, #0x08]
            cmp     w0, w3
            bhs     G_M000_IG138
            add     x1, x1, #16
            ldr     s16, [x1, w0, UXTW #2]
            fadd    s0, s0, s16
            ldr     x1, [x19, #0x28]
            add     w3, w0, #1
            ldr     w4, [x1, #0x08]
            cmp     w3, w4
            bhs     G_M000_IG138
            add     x1, x1, #16
            add     w0, w0, #1
            ldr     s16, [x1, w0, UXTW #2]
            fadd    s0, s0, s16
            movz    x0, #0x4510
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeMask(float):float
            cbz     w20, G_M000_IG101

G_M000_IG107:                ;; offset=0x0B30
            ldr     w3, [fp, #-0xB0]
            lsl     w2, w3, #3
            add     w2, w2, #5
            ldr     w4, [fp, #-0xB4]
            lsl     w1, w4, #3
            mov     x0, x19
            movz    x5, #0x4558
            movk    x5, #0x695F LSL #16
            movk    x5, #0x7FF8 LSL #32
            ldr     x5, [x5]
            blr     x5      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:ModulateVector(int,int,float):float:this
            b       G_M000_IG104

G_M000_IG108:                ;; offset=0x0B60
            ldr     x1, [fp, #-0xE0]
            add     x1, x1, #32
            ldapr   w1, [x1]
            cbz     w1, G_M000_IG100
            b       G_M000_IG127

G_M000_IG109:                ;; offset=0x0B74
            ldr     w8, [fp, #-0xB0]
            lsl     w2, w8, #3
            add     w2, w2, #5
            ldr     x5, [fp, #-0xF0]
            ldr     w6, [fp, #-0xD0]
            mov     x3, x27
            ldr     w4, [fp, #-0xCC]
            str     w7, [fp, #-0xB4]
            lsl     w1, w7, #3
            mov     x0, x19
            movz    x9, #0x44F8
            movk    x9, #0x695F LSL #16
            movk    x9, #0x7FF8 LSL #32
            ldr     x9, [x9]
            blr     x9      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:ModulateEightBlocks(int,int,System.Span`1[System.Runtime.Intrinsics.Vector256`1[float]],System.Span`1[float]):this
            mov     w0, wzr
            ldr     w1, [fp, #-0xD0]
            cmp     w1, #8
            blt     G_M000_IG113

G_M000_IG110:                ;; offset=0x0BC0
            ldr     w2, [fp, #-0xB4]
            str     w0, [fp, #-0xB8]
            add     w3, w2, w0
            lsl     w3, w3, #1
            ldr     x4, [x19, #0x28]
            ldr     w5, [x19, #0x58]
            add     w5, w5, w3
            mov     w6, w5
            ldr     w7, [x4, #0x08]
            cmp     w6, w7
            bhs     G_M000_IG138
            add     x4, x4, #16
            ldr     s0, [x4, w6, UXTW #2]
            add     w5, w5, #1
            cmp     w5, w7
            bhs     G_M000_IG138
            ldr     s16, [x4, w5, UXTW #2]
            fadd    s0, s0, s16
            cmp     w3, w7
            bhs     G_M000_IG138
            ldr     s16, [x4, w3, UXTW #2]
            fadd    s0, s0, s16
            add     w3, w3, #1
            cmp     w3, w7
            bhs     G_M000_IG138
            ldr     s16, [x4, w3, UXTW #2]
            fadd    s13, s0, s16
            fmov    s0, s13
            movz    x3, #0x4510
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeMask(float):float
            fmov    s13, s0
            ldr     x0, [fp, #-0xF0]
            ldr     w1, [fp, #-0xB8]
            ldr     s0, [x0, w1, UXTW #2]
            fadd    s13, s0, s13
            str     w1, [fp, #-0xB8]
            add     w2, w1, #8
            ldr     w3, [fp, #-0xD0]
            cmp     w2, w3
            bhs     G_M000_IG138
            ldr     s0, [x0, w2, UXTW #2]
            movz    x2, #0x4528
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:FastLog2(float):float
            fmul    s0, s0, s10
            fadd    s13, s0, s13
            fmul    s0, s13, s11
            movz    x0, #0x4540
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:FastPow2(float):float
            fmul    s0, s0, s8
            fadd    s14, s0, s9
            ldr     w0, [x19, #0x64]
            ldp     w1, w2, [fp, #-0xB4]
            madd    w0, w2, w0, w1
            ldr     w3, [fp, #-0xB8]
            add     w0, w0, w3
            cmp     w0, w26
            bhs     G_M000_IG138
            fdiv    s0, s12, s14
            fmov    s16, #1.0000
            fsub    s0, s0, s16
            movi    v16.2s, #0
            fmax    s0, s0, s16
            str     s0, [x25, w0, UXTW #2]
            add     w3, w3, #1
            cmp     w3, #8
            mov     w0, w3
            bge     G_M000_IG114

G_M000_IG111:                ;; offset=0x0CF0
            str     w1, [fp, #-0xB4]
            b       G_M000_IG110

G_M000_IG112:                ;; offset=0x0CF8
            str     w1, [fp, #-0xB4]

G_M000_IG113:                ;; offset=0x0CFC
            ldr     w3, [fp, #-0xB4]
            str     w0, [fp, #-0xB8]
            add     w4, w3, w0
            lsl     w4, w4, #1
            ldr     x5, [x19, #0x28]
            mov     x6, x5
            ldr     w7, [x19, #0x58]
            add     w7, w7, w4
            mov     w8, w7
            ldr     w9, [x6, #0x08]
            cmp     w8, w9
            bhs     G_M000_IG138
            add     x6, x6, #16
            ldr     s0, [x6, w8, UXTW #2]
            mov     x6, x5
            add     w7, w7, #1
            cmp     w7, w9
            bhs     G_M000_IG138
            add     x6, x6, #16
            ldr     s16, [x6, w7, UXTW #2]
            fadd    s0, s0, s16
            mov     x6, x5
            cmp     w4, w9
            bhs     G_M000_IG138
            add     x6, x6, #16
            ldr     s16, [x6, w4, UXTW #2]
            fadd    s0, s0, s16
            add     w6, w4, #1
            cmp     w6, w9
            bhs     G_M000_IG138
            add     x5, x5, #16
            add     w4, w4, #1
            ldr     s16, [x5, w4, UXTW #2]
            fadd    s13, s0, s16
            fmov    s0, s13
            movz    x4, #0x4510
            movk    x4, #0x695F LSL #16
            movk    x4, #0x7FF8 LSL #32
            ldr     x4, [x4]
            blr     x4      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeMask(float):float
            fmov    s13, s0
            ldr     w0, [fp, #-0xB8]
            ldr     w1, [fp, #-0xD0]
            cmp     w0, w1
            bhs     G_M000_IG138
            ldr     x2, [fp, #-0xF0]
            ldr     s0, [x2, w0, UXTW #2]
            fadd    s13, s0, s13
            add     w3, w0, #8
            cmp     w3, w1
            bhs     G_M000_IG138
            str     w0, [fp, #-0xB8]
            add     w3, w0, #8
            ldr     s0, [x2, w3, UXTW #2]
            movz    x3, #0x4528
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:FastLog2(float):float
            fmul    s0, s0, s10
            fadd    s13, s0, s13
            fmul    s0, s13, s11
            movz    x0, #0x4540
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:FastPow2(float):float
            fmul    s0, s0, s8
            fadd    s14, s0, s9
            ldr     w0, [x19, #0x64]
            ldp     w1, w2, [fp, #-0xB4]
            madd    w0, w2, w0, w1
            ldr     w3, [fp, #-0xB8]
            add     w0, w0, w3
            cmp     w0, w26
            bhs     G_M000_IG138
            fdiv    s0, s12, s14
            fmov    s16, #1.0000
            fsub    s0, s0, s16
            movi    v16.2s, #0
            fmax    s0, s0, s16
            str     s0, [x25, w0, UXTW #2]
            add     w3, w3, #1
            cmp     w3, #8
            mov     w0, w3
            blt     G_M000_IG112

G_M000_IG114:                ;; offset=0x0E54
            add     w1, w1, #8
            mov     w7, w1

G_M000_IG115:                ;; offset=0x0E5C
            ldr     w2, [x19, #0x64]
            sub     w2, w2, #8
            cmp     w7, w2
            ble     G_M000_IG109

G_M000_IG116:                ;; offset=0x0E6C
            ldr     w0, [x19, #0x64]
            cmp     w7, w0
            blt     G_M000_IG106

G_M000_IG117:                ;; offset=0x0E78
            ldr     w3, [fp, #-0xB0]

G_M000_IG118:                ;; offset=0x0E7C
            add     w3, w3, #1
            mov     w7, w3

G_M000_IG119:                ;; offset=0x0E84
            ldr     w1, [x19, #0x48]
            cmp     w7, w1
            blt     G_M000_IG99

G_M000_IG120:                ;; offset=0x0E90
            add     x14, x19, #56
            mov     x15, x28
            bl      CORINFO_HELP_ASSIGN_REF
            stp     w21, w23, [x19, #0x5C]
            strb    w22, [x19, #0x6A]
            movz    xip0, #0x2D5A
            movk    xip0, #0xF7B0 LSL #16
            movk    xip0, #0x2883 LSL #32
            ldr     xip1, [fp, #-0x90]
            cmp     xip0, xip1
            beq     G_M000_IG121
            bl      CORINFO_HELP_FAIL_FAST

G_M000_IG121:                ;; offset=0x0EC0
            sub     sp, fp, #272
            ldp     fp, lr, [sp, #0x110]
            ldp     x27, x28, [sp, #0x100]
            ldp     x25, x26, [sp, #0xF0]
            ldp     x23, x24, [sp, #0xE0]
            ldp     x21, x22, [sp, #0xD0]
            ldp     x19, x20, [sp, #0xC0]
            ldr     d14, [sp, #0xB8]
            ldp     d12, d13, [sp, #0xA8]
            ldp     d10, d11, [sp, #0x98]
            ldp     d8, d9, [sp, #0x88]
            add     sp, sp, #288
            ret     lr

G_M000_IG122:                ;; offset=0x0EF4
            ldr     x1, [fp, #-0xE0]
            add     x1, x1, #32
            ldapr   w1, [x1]
            cbz     w1, G_M000_IG72
            b       G_M000_IG127

G_M000_IG123:                ;; offset=0x0F08
            ldr     x2, [fp, #-0xE0]
            add     x2, x2, #32
            ldapr   w2, [x2]
            cbz     w2, G_M000_IG07
            b       G_M000_IG127

G_M000_IG124:                ;; offset=0x0F1C
            movn    w7, #0
            b       G_M000_IG14

G_M000_IG125:                ;; offset=0x0F24
            mov     w6, w4
            add     x6, x6, w1, UXTW
            ldr     w7, [x0, #0x08]
            cmp     x6, x7
            bhi     G_M000_IG130
            add     x0, x0, #16
            ubfiz   x4, x4, #2, #32
            add     x4, x0, x4
            b       G_M000_IG09

G_M000_IG126:                ;; offset=0x0F48
            mov     x0, x19
            movz    x1, #0x4408
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for System.ThrowHelper:ThrowObjectDisposedException(System.Object)
            brk     #0

G_M000_IG127:                ;; offset=0x0F64
            sub     x0, fp, #224
            movz    x1, #0x4420
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for System.Threading.CancellationToken:ThrowOperationCanceledException():this
            brk     #0

G_M000_IG128:                ;; offset=0x0F80
            movz    x0, #0xDCE0
            movk    x0, #0x6947 LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x62C8
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF00
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:.ctor(System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG129:                ;; offset=0x0FC0
            movz    x0, #0xF360
            movk    x0, #0x693D LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x6208
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0x62A0
            movk    x2, #0xA15 LSL #16
            movk    x2, #548 LSL #32
            movz    x3, #0x4438
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for System.ArgumentException:.ctor(System.String,System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG130:                ;; offset=0x100C
            movz    x0, #0x4450
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentOutOfRangeException()
            brk     #0

G_M000_IG131:                ;; offset=0x1024
            movz    x0, #0x4468
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentException_DestinationTooShort()
            brk     #0

G_M000_IG132:                ;; offset=0x103C
            mov     w1, w5
            mov     w0, wzr
            movz    x2, #0x42D0
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.Math:ThrowMinMaxException[int](int,int)
            brk     #0

G_M000_IG133:                ;; offset=0x105C
            mov     w1, w8
            mov     w0, wzr
            movz    x2, #0x42D0
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.Math:ThrowMinMaxException[int](int,int)
            brk     #0

G_M000_IG134:                ;; offset=0x107C
            mov     w0, wzr
            movz    x2, #0x42D0
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.Math:ThrowMinMaxException[int](int,int)
            brk     #0

G_M000_IG135:                ;; offset=0x1098
            mov     w1, w5
            mov     w0, wzr
            movz    x2, #0x42D0
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.Math:ThrowMinMaxException[int](int,int)
            brk     #0

G_M000_IG136:                ;; offset=0x10B8
            mov     w1, w6
            mov     w0, wzr
            movz    x2, #0x42D0
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.Math:ThrowMinMaxException[int](int,int)
            brk     #0

G_M000_IG137:                ;; offset=0x10D8
            bl      CORINFO_HELP_THROWDIVZERO
            brk     #0

G_M000_IG138:                ;; offset=0x10E0
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dd	42600000h		;        56
RWD04  	dd	3F574BC7h		;     0.841
RWD08  	dd	3ECEAF25h		;   0.40368
RWD12  	dd	BDDC6A08h		; -0.107624
RWD16  	dd	3FB8AA3Bh		;    1.4427
RWD20  	dd	3F19999Ah		;       0.6

; Total bytes of code 4328

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ComputePreErosion(int,int,bool):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 38 single block inlinees; 19 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x90]!
            stp     d8, d9, [sp, #0x28]
            stp     d10, d11, [sp, #0x38]
            stp     d12, d13, [sp, #0x48]
            stp     d14, d15, [sp, #0x58]
            stp     x19, x20, [sp, #0x68]
            stp     x21, x22, [sp, #0x78]
            str     x23, [sp, #0x88]
            mov     fp, sp
            mov     x19, x0
            mov     w20, w2

G_M000_IG02:                ;; offset=0x002C
            ldr     s8, [@RWD00]
            ldr     w0, [x19, #0x54]
            mul     w0, w1, w0
            add     w21, w0, #1
            mov     w22, wzr
            tst     w3, #255
            beq     G_M000_IG06

G_M000_IG03:                ;; offset=0x0048
            ldr     w0, [x19, #0x4C]
            sub     w0, w0, #4
            tbnz    w0, #31, G_M000_IG06

G_M000_IG04:                ;; offset=0x0054
            add     w23, w21, w22
            ldr     x0, [x19, #0x10]
            ldr     w1, [x19, #0x54]
            sub     w2, w23, w1
            tbnz    w2, #31, G_M000_IG15
            ldr     w3, [x0, #0x08]
            sub     w4, w3, w2
            cmp     w4, #4
            blt     G_M000_IG15
            cmp     w2, w3
            bhs     G_M000_IG19
            add     x0, x0, #16
            ubfiz   x2, x2, #2, #32
            ldr     q9, [x0, x2]
            sub     w2, w3, w23
            cmp     w2, #4
            ccmp    w23, #0, nc, ge
            blt     G_M000_IG15
            cmp     w23, w3
            bhs     G_M000_IG19
            ubfiz   x2, x23, #2, #32
            ldr     q10, [x0, x2]
            add     w2, w23, w1
            mov     w4, w2
            sub     w5, w3, w4
            cmp     w5, #4
            ccmp    w4, #0, nc, ge
            blt     G_M000_IG15
            cmp     w4, w3
            bhs     G_M000_IG19
            ubfiz   x4, x4, #2, #32
            ldr     q11, [x0, x4]
            sub     w4, w23, #1
            sub     w5, w3, w4
            cmp     w5, #4
            ccmp    w4, #0, nc, ge
            blt     G_M000_IG15
            cmp     w4, w3
            bhs     G_M000_IG19
            ubfiz   x4, x4, #2, #32
            ldr     q0, [x0, x4]
            add     w4, w23, #1
            sub     w5, w3, w4
            cmp     w5, #4
            blt     G_M000_IG15
            cmp     w4, w3
            bhs     G_M000_IG19
            ubfiz   x4, x4, #2, #32
            ldr     q16, [x0, x4]
            fadd    v0.4s, v16.4s, v0.4s
            fadd    v16.4s, v9.4s, v11.4s
            fadd    v0.4s, v0.4s, v16.4s
            ldr     q16, [@RWD04]
            fmul    v0.4s, v0.4s, v16.4s
            ldr     q16, [@RWD20]
            fadd    v16.4s, v10.4s, v16.4s
            movi    v17.4s, #0
            fmax    v16.4s, v16.4s, v17.4s
            fmul    v17.4s, v16.4s, v16.4s
            ldr     q18, [@RWD36]
            fmul    v16.4s, v16.4s, v18.4s
            fmul    v16.4s, v16.4s, v17.4s
            ldr     q19, [@RWD52]
            fadd    v16.4s, v16.4s, v19.4s
            ldr     q20, [@RWD68]
            fmul    v17.4s, v17.4s, v20.4s
            ldr     q21, [@RWD84]
            fadd    v17.4s, v17.4s, v21.4s
            fdiv    v16.4s, v16.4s, v17.4s
            fsub    v0.4s, v10.4s, v0.4s
            fmul    v0.4s, v16.4s, v0.4s
            fmul    v0.4s, v0.4s, v0.4s
            ldr     q16, [@RWD100]
            fmin    v0.4s, v0.4s, v16.4s
            ldr     q16, [@RWD116]
            fmul    v0.4s, v0.4s, v16.4s
            ldr     q16, [@RWD132]
            fadd    v0.4s, v0.4s, v16.4s
            fsqrt   v0.4s, v0.4s
            ldr     q16, [@RWD148]
            fmul    v12.4s, v0.4s, v16.4s
            mov     v9.16b, v10.16b
            mov     v10.16b, v11.16b
            add     w4, w23, w1,  LSL #1
            mov     w5, w4
            sub     w6, w3, w5
            cmp     w6, #4
            ccmp    w5, #0, nc, ge
            blt     G_M000_IG15
            cmp     w5, w3
            bhs     G_M000_IG19
            ubfiz   x5, x5, #2, #32
            ldr     q11, [x0, x5]
            sub     w5, w2, #1
            sub     w6, w3, w5
            cmp     w6, #4
            ccmp    w5, #0, nc, ge
            blt     G_M000_IG15
            cmp     w5, w3
            bhs     G_M000_IG19
            ubfiz   x5, x5, #2, #32
            ldr     q0, [x0, x5]
            add     w2, w2, #1
            sub     w5, w3, w2
            cmp     w5, #4
            ccmp    w2, #0, nc, ge
            blt     G_M000_IG15
            cmp     w2, w3
            bhs     G_M000_IG19
            ubfiz   x2, x2, #2, #32
            ldr     q16, [x0, x2]
            fadd    v0.4s, v16.4s, v0.4s
            fadd    v16.4s, v9.4s, v11.4s
            fadd    v0.4s, v0.4s, v16.4s
            ldr     q16, [@RWD164]
            fmul    v0.4s, v0.4s, v16.4s
            ldr     q16, [@RWD180]
            fadd    v16.4s, v10.4s, v16.4s
            movi    v17.4s, #0
            fmax    v16.4s, v16.4s, v17.4s
            fmul    v17.4s, v16.4s, v16.4s
            fmul    v16.4s, v16.4s, v18.4s
            fmul    v16.4s, v16.4s, v17.4s
            fadd    v16.4s, v16.4s, v19.4s
            fmul    v17.4s, v17.4s, v20.4s
            fadd    v17.4s, v17.4s, v21.4s
            fdiv    v16.4s, v16.4s, v17.4s
            fsub    v0.4s, v10.4s, v0.4s
            fmul    v0.4s, v16.4s, v0.4s
            fmul    v0.4s, v0.4s, v0.4s
            ldr     q16, [@RWD196]
            fmin    v0.4s, v0.4s, v16.4s
            ldr     q16, [@RWD212]
            fmul    v0.4s, v0.4s, v16.4s
            ldr     q16, [@RWD228]
            fadd    v0.4s, v0.4s, v16.4s
            fsqrt   v0.4s, v0.4s
            ldr     q16, [@RWD244]
            fmul    v0.4s, v0.4s, v16.4s
            fadd    v12.4s, v12.4s, v0.4s
            mov     v9.16b, v10.16b
            mov     v10.16b, v11.16b
            mov     w2, #3
            madd    w1, w1, w2, w23
            sub     w2, w3, w1
            cmp     w2, #4
            ccmp    w1, #0, nc, ge
            blt     G_M000_IG15
            cmp     w1, w3
            bhs     G_M000_IG19
            ubfiz   x1, x1, #2, #32
            ldr     q11, [x0, x1]
            sub     w1, w4, #1
            sub     w2, w3, w1
            cmp     w2, #4
            ccmp    w1, #0, nc, ge
            blt     G_M000_IG15
            cmp     w1, w3

G_M000_IG05:                ;; offset=0x02CC
            bhs     G_M000_IG19
            ubfiz   x1, x1, #2, #32
            ldr     q0, [x0, x1]
            add     w1, w4, #1
            sub     w2, w3, w1
            cmp     w2, #4
            ccmp    w1, #0, nc, ge
            blt     G_M000_IG15
            cmp     w1, w3
            bhs     G_M000_IG19
            ubfiz   x1, x1, #2, #32
            ldr     q16, [x0, x1]
            fadd    v0.4s, v16.4s, v0.4s
            fadd    v16.4s, v9.4s, v11.4s
            fadd    v0.4s, v0.4s, v16.4s
            ldr     q16, [@RWD260]
            fmul    v9.4s, v0.4s, v16.4s
            str     q9, [fp, #0x10]
            ldr     q0, [@RWD276]
            fadd    v0.4s, v10.4s, v0.4s
            mov     w0, wzr
            movz    x1, #0x4738
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            mov     v13.d[0], v10.d[1]
            mov     v14.d[0], v11.d[1]
            mov     v15.d[0], v12.d[1]
            blr     x1      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:DerivativeRatio(System.Numerics.Vector`1[float],bool):System.Numerics.Vector`1[float]
            mov     v10.d[1], v13.d[0]
            ldr     q16, [fp, #0x10]
            fsub    v16.4s, v10.4s, v16.4s
            fmul    v0.4s, v0.4s, v16.4s
            fmul    v0.4s, v0.4s, v0.4s
            ldr     q16, [@RWD292]
            fmin    v0.4s, v0.4s, v16.4s
            ldr     q16, [@RWD308]
            fmul    v0.4s, v0.4s, v16.4s
            ldr     q16, [@RWD324]
            fadd    v0.4s, v0.4s, v16.4s
            fsqrt   v0.4s, v0.4s
            ldr     q16, [@RWD340]
            fmul    v0.4s, v0.4s, v16.4s
            mov     v12.d[1], v15.d[0]
            fadd    v12.4s, v12.4s, v0.4s
            mov     v11.d[1], v14.d[0]
            ldr     x0, [x19, #0x10]
            ldr     w1, [x19, #0x54]
            add     w2, w23, w1,  LSL #2
            tbnz    w2, #31, G_M000_IG15
            ldr     w3, [x0, #0x08]
            sub     w4, w3, w2
            cmp     w4, #4
            blt     G_M000_IG15
            cmp     w2, w3
            bhs     G_M000_IG19
            add     x0, x0, #16
            ubfiz   x2, x2, #2, #32
            ldr     q0, [x0, x2]
            mov     v13.16b, v0.16b
            mov     w2, #3
            madd    w1, w1, w2, w23
            sub     w2, w1, #1
            sub     w4, w3, w2
            cmp     w4, #4
            ccmp    w2, #0, nc, ge
            blt     G_M000_IG15
            cmp     w2, w3
            bhs     G_M000_IG19
            ubfiz   x2, x2, #2, #32
            ldr     q0, [x0, x2]
            add     w1, w1, #1
            sub     w2, w3, w1
            cmp     w2, #4
            ccmp    w1, #0, nc, ge
            blt     G_M000_IG15
            cmp     w1, w3
            bhs     G_M000_IG19
            ubfiz   x1, x1, #2, #32
            ldr     q16, [x0, x1]
            fadd    v0.4s, v16.4s, v0.4s
            fadd    v16.4s, v10.4s, v13.4s
            fadd    v0.4s, v0.4s, v16.4s
            ldr     q16, [@RWD356]
            fmul    v9.4s, v0.4s, v16.4s
            ldr     q0, [@RWD372]
            fadd    v0.4s, v11.4s, v0.4s
            mov     w0, wzr
            movz    x1, #0x4738
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            mov     v13.d[0], v11.d[1]
            mov     v15.d[0], v12.d[1]
            mov     v10.d[0], v9.d[1]
            blr     x1      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:DerivativeRatio(System.Numerics.Vector`1[float],bool):System.Numerics.Vector`1[float]
            mov     v11.d[1], v13.d[0]
            mov     v9.d[1], v10.d[0]
            fsub    v16.4s, v11.4s, v9.4s
            fmul    v16.4s, v0.4s, v16.4s
            fmul    v16.4s, v16.4s, v16.4s
            ldr     q17, [@RWD388]
            fmin    v16.4s, v16.4s, v17.4s
            ldr     q17, [@RWD404]
            fmul    v16.4s, v16.4s, v17.4s
            ldr     q17, [@RWD420]
            fadd    v16.4s, v16.4s, v17.4s
            fsqrt   v16.4s, v16.4s
            ldr     q17, [@RWD436]
            fmul    v16.4s, v16.4s, v17.4s
            mov     v12.d[1], v15.d[0]
            fadd    v12.4s, v12.4s, v16.4s
            ldr     x0, [x19, #0x20]
            ldr     w1, [x0, #0x08]
            cmp     w1, w22
            bls     G_M000_IG16
            sub     w2, w1, w22
            cmp     w2, #4
            blt     G_M000_IG17
            cmp     w22, w1
            bhs     G_M000_IG19
            add     x0, x0, #16
            ubfiz   x1, x22, #2, #32
            str     q12, [x0, x1]
            add     w22, w22, #4
            ldr     w0, [x19, #0x4C]
            sub     w0, w0, #4
            cmp     w22, w0
            ble     G_M000_IG04

G_M000_IG06:                ;; offset=0x04D0
            ldr     w0, [x19, #0x4C]
            ldr     s16, [@RWD20]
            ldr     s17, [@RWD36]
            ldr     s18, [@RWD52]
            ldr     s19, [@RWD68]
            ldr     s20, [@RWD84]
            ldr     d21, [@RWD452]
            b       G_M000_IG10
            align   [0 bytes for IG08]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG07:                ;; offset=0x04F0
            add     w1, w21, w22
            ldr     x2, [x19, #0x10]
            ldr     w3, [x2, #0x08]
            cmp     w1, w3
            bhs     G_M000_IG19
            add     x2, x2, #16
            ldr     s22, [x2, w1, UXTW #2]
            sub     w4, w1, #1
            cmp     w4, w3
            bhs     G_M000_IG19
            ldr     s23, [x2, w4, UXTW #2]
            add     w4, w1, #1
            cmp     w4, w3
            bhs     G_M000_IG19
            ldr     s24, [x2, w4, UXTW #2]
            fadd    s23, s24, s23
            ldr     w4, [x19, #0x54]
            sub     w5, w1, w4
            cmp     w5, w3
            bhs     G_M000_IG19
            ldr     s24, [x2, w5, UXTW #2]
            add     w5, w1, w4
            cmp     w5, w3
            bhs     G_M000_IG19
            ldr     s25, [x2, w5, UXTW #2]
            fadd    s24, s24, s25
            fadd    s23, s23, s24
            fmov    s24, #0.2500
            fmul    s23, s23, s24
            fadd    s24, s22, s16
            movi    v25.2s, #0
            fmax    s24, s24, s25
            fmul    s25, s24, s24
            fmul    s24, s24, s17
            fmul    s24, s24, s25
            fadd    s24, s24, s18
            fmul    s25, s25, s19
            fadd    s25, s25, s20
            fdiv    s24, s24, s25
            fsub    s22, s22, s23
            fmul    s22, s24, s22
            fmul    s22, s22, s22
            fmin    s22, s22, s21
            ldr     s23, [@RWD00]
            fmul    s22, s22, s23
            fmov    s23, #28.0000
            fadd    s22, s22, s23
            fsqrt   s22, s22
            fmov    s23, #0.2500
            fmul    s22, s22, s23
            mov     w5, #1

G_M000_IG08:                ;; offset=0x05BC
            ldr     w6, [x19, #0x54]
            madd    w6, w5, w6, w1
            cmp     w6, w3
            bhs     G_M000_IG19
            ldr     s23, [x2, w6, UXTW #2]
            sub     w7, w6, #1
            cmp     w7, w3
            bhs     G_M000_IG19
            ldr     s24, [x2, w7, UXTW #2]
            add     w7, w6, #1
            cmp     w7, w3
            bhs     G_M000_IG19
            ldr     s25, [x2, w7, UXTW #2]
            fadd    s24, s25, s24
            sub     w7, w6, w4
            cmp     w7, w3
            bhs     G_M000_IG19
            ldr     s25, [x2, w7, UXTW #2]
            add     w6, w6, w4
            cmp     w6, w3
            bhs     G_M000_IG19
            ldr     s26, [x2, w6, UXTW #2]
            fadd    s25, s25, s26
            fadd    s24, s24, s25
            fmov    s25, #0.2500
            fmul    s24, s24, s25
            fadd    s25, s23, s16
            movi    v26.2s, #0
            fmax    s25, s25, s26
            fmul    s26, s25, s25
            fmul    s25, s25, s17
            fmul    s25, s25, s26
            fadd    s25, s25, s18
            fmul    s26, s26, s19
            fadd    s26, s26, s20
            fdiv    s25, s25, s26
            fsub    s23, s23, s24
            fmul    s23, s25, s23
            fmul    s23, s23, s23
            fmin    s23, s23, s21
            fmul    s23, s23, s8
            fmov    s24, #28.0000
            fadd    s23, s23, s24
            fsqrt   s23, s23
            fmov    s24, #0.2500
            fmul    s23, s23, s24
            fadd    s22, s23, s22
            add     w5, w5, #1
            cmp     w5, #4
            blt     G_M000_IG08

G_M000_IG09:                ;; offset=0x0684
            ldr     x1, [x19, #0x20]
            ldr     w2, [x1, #0x08]
            cmp     w22, w2
            bhs     G_M000_IG19
            add     x1, x1, #16
            str     s22, [x1, w22, UXTW #2]
            add     w22, w22, #1

G_M000_IG10:                ;; offset=0x06A0
            cmp     w22, w0
            blt     G_M000_IG07

G_M000_IG11:                ;; offset=0x06A8
            ldr     w2, [x19, #0x58]
            mul     w0, w20, w2
            mov     w22, wzr
            ldr     w2, [x19, #0x58]
            cmp     w2, #0
            ble     G_M000_IG14
            align   [0 bytes for IG12]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG12:                ;; offset=0x06C0
            lsl     w3, w22, #2
            ldp     x4, x1, [x19, #0x18]
            ldr     w5, [x1, #0x08]
            sub     w6, w5, #3
            cmp     w3, w6
            ccmp    w3, #0, nc, lt
            blt     G_M000_IG18
            add     x1, x1, #16
            ldr     s16, [x1, w3, UXTW #2]
            add     w6, w3, #1
            ldr     s17, [x1, w6, UXTW #2]
            fadd    s16, s16, s17
            add     w7, w3, #2
            ldr     s17, [x1, w7, UXTW #2]
            fadd    s16, s16, s17
            add     w8, w3, #3
            ldr     s17, [x1, w8, UXTW #2]
            fadd    s16, s16, s17
            fmov    s17, #0.2500
            fmul    s16, s16, s17

G_M000_IG13:                ;; offset=0x0710
            add     w1, w0, w22
            ldr     w3, [x4, #0x08]
            cmp     w1, w3
            bhs     G_M000_IG19
            add     x3, x4, #16
            str     s16, [x3, w1, UXTW #2]
            add     w22, w22, #1
            cmp     w22, w2
            blt     G_M000_IG12

G_M000_IG14:                ;; offset=0x0734
            ldr     x23, [sp, #0x88]
            ldp     x21, x22, [sp, #0x78]
            ldp     x19, x20, [sp, #0x68]
            ldp     d14, d15, [sp, #0x58]
            ldp     d12, d13, [sp, #0x48]
            ldp     d10, d11, [sp, #0x38]
            ldp     d8, d9, [sp, #0x28]
            ldp     fp, lr, [sp], #0x90
            ret     lr

G_M000_IG15:                ;; offset=0x0758
            movz    x0, #0x4750
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()
            brk     #0

G_M000_IG16:                ;; offset=0x0770
            movz    x0, #0x4768
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLess()
            brk     #0

G_M000_IG17:                ;; offset=0x0788
            movz    x0, #0x4468
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentException_DestinationTooShort()
            brk     #0

G_M000_IG18:                ;; offset=0x07A0
            cmp     w3, w5
            bhs     G_M000_IG19
            add     x1, x1, #16
            ldr     s16, [x1, w3, UXTW #2]
            add     w6, w3, #1
            cmp     w6, w5
            bhs     G_M000_IG19
            ldr     s17, [x1, w6, UXTW #2]
            fadd    s16, s16, s17
            add     w7, w3, #2
            cmp     w7, w5
            bhs     G_M000_IG19
            ldr     s17, [x1, w7, UXTW #2]
            fadd    s16, s16, s17
            add     w8, w3, #3
            cmp     w8, w5
            bhs     G_M000_IG19
            ldr     s17, [x1, w8, UXTW #2]
            fadd    s16, s16, s17
            fmov    s17, #0.2500
            fmul    s16, s16, s17
            b       G_M000_IG13

G_M000_IG19:                ;; offset=0x07F8
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dd	480E0640h		;    145433
RWD04  	dq	3E8000003E800000h, 3E8000003E800000h
RWD20  	dq	409B0A3D409B0A3Dh, 409B0A3D409B0A3Dh
RWD36  	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD52  	dq	449E380D449E380Dh, 449E380D449E380Dh
RWD68  	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD84  	dq	44228FFE44228FFEh, 44228FFE44228FFEh
RWD100 	dq	3E4CCCCD3E4CCCCDh, 3E4CCCCD3E4CCCCDh
RWD116 	dq	480E0640480E0640h, 480E0640480E0640h
RWD132 	dq	41E0000041E00000h, 41E0000041E00000h
RWD148 	dq	3E8000003E800000h, 3E8000003E800000h
RWD164 	dq	3E8000003E800000h, 3E8000003E800000h
RWD180 	dq	409B0A3D409B0A3Dh, 409B0A3D409B0A3Dh
RWD196 	dq	3E4CCCCD3E4CCCCDh, 3E4CCCCD3E4CCCCDh
RWD212 	dq	480E0640480E0640h, 480E0640480E0640h
RWD228 	dq	41E0000041E00000h, 41E0000041E00000h
RWD244 	dq	3E8000003E800000h, 3E8000003E800000h
RWD260 	dq	3E8000003E800000h, 3E8000003E800000h
RWD276 	dq	409B0A3D409B0A3Dh, 409B0A3D409B0A3Dh
RWD292 	dq	3E4CCCCD3E4CCCCDh, 3E4CCCCD3E4CCCCDh
RWD308 	dq	480E0640480E0640h, 480E0640480E0640h
RWD324 	dq	41E0000041E00000h, 41E0000041E00000h
RWD340 	dq	3E8000003E800000h, 3E8000003E800000h
RWD356 	dq	3E8000003E800000h, 3E8000003E800000h
RWD372 	dq	409B0A3D409B0A3Dh, 409B0A3D409B0A3Dh
RWD388 	dq	3E4CCCCD3E4CCCCDh, 3E4CCCCD3E4CCCCDh
RWD404 	dq	480E0640480E0640h, 480E0640480E0640h
RWD420 	dq	41E0000041E00000h, 41E0000041E00000h
RWD436 	dq	3E8000003E800000h, 3E8000003E800000h
RWD452 	dq	3E4CCCCD3E4CCCCDh

; Total bytes of code 2048

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 33 single block inlinees; 11 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x30]!
            stp     x19, x20, [sp, #0x10]
            stp     x21, x22, [sp, #0x20]
            mov     fp, sp
            mov     x19, x0
            mov     w20, w1
            mov     w21, w2

G_M000_IG02:                ;; offset=0x001C
            mov     w22, wzr
            tst     w3, #255
            beq     G_M000_IG08

G_M000_IG03:                ;; offset=0x0028
            ldr     w0, [x19, #0x58]
            cmp     w0, #5
            ble     G_M000_IG08
            mov     w22, #1
            mov     x0, x19
            mov     w1, w20
            mov     w2, w21
            mov     w3, wzr
            movz    x4, #0x4780
            movk    x4, #0x695F LSL #16
            movk    x4, #0x7FF8 LSL #32
            ldr     x4, [x4]
            blr     x4      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this
            ldr     w0, [x19, #0x58]
            sub     w1, w0, #4
            cmp     w1, #1
            ble     G_M000_IG08
            sub     w1, w20, #1
            add     w2, w20, #1
            align   [0 bytes for IG04]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG04:                ;; offset=0x0074
            madd    w3, w1, w0, w22
            madd    w4, w20, w0, w22
            madd    w5, w2, w0, w22
            ldr     x6, [x19, #0x18]
            sub     w7, w3, #1
            tbnz    w7, #31, G_M000_IG11
            ldr     w8, [x6, #0x08]
            sub     w9, w8, w7
            cmp     w9, #4
            blt     G_M000_IG11
            cmp     w7, w8
            bhs     G_M000_IG14
            add     x6, x6, #16
            ubfiz   x7, x7, #2, #32
            ldr     q16, [x6, x7]
            sub     w7, w8, w3
            cmp     w7, #4
            ccmp    w3, #0, nc, ge
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x7, x3, #2, #32
            ldr     q17, [x6, x7]
            add     w3, w3, #1
            sub     w7, w8, w3
            cmp     w7, #4
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x3, x3, #2, #32
            ldr     q18, [x6, x3]
            sub     w3, w4, #1
            sub     w7, w8, w3
            cmp     w7, #4
            ccmp    w3, #0, nc, ge
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x3, x3, #2, #32
            ldr     q19, [x6, x3]
            sub     w3, w8, w4
            cmp     w3, #4
            ccmp    w4, #0, nc, ge
            blt     G_M000_IG11
            cmp     w4, w8
            bhs     G_M000_IG14
            ubfiz   x3, x4, #2, #32
            ldr     q20, [x6, x3]
            add     w3, w4, #1
            sub     w4, w8, w3
            cmp     w4, #4
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x3, x3, #2, #32
            ldr     q21, [x6, x3]
            sub     w3, w5, #1
            sub     w4, w8, w3
            cmp     w4, #4
            ccmp    w3, #0, nc, ge
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x3, x3, #2, #32
            ldr     q22, [x6, x3]
            sub     w3, w8, w5
            cmp     w3, #4
            ccmp    w5, #0, nc, ge
            blt     G_M000_IG11
            cmp     w5, w8
            bhs     G_M000_IG14
            ubfiz   x3, x5, #2, #32
            ldr     q23, [x6, x3]
            add     w3, w5, #1
            sub     w4, w8, w3
            cmp     w4, #4
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x3, x3, #2, #32
            ldr     q24, [x6, x3]
            ldrb    w3, [x19, #0x68]
            cbnz    w3, G_M000_IG06

G_M000_IG05:                ;; offset=0x01C0
            fmax    v25.4s, v16.4s, v17.4s
            fmin    v16.4s, v16.4s, v17.4s
            fmax    v17.4s, v18.4s, v19.4s
            fmin    v18.4s, v18.4s, v19.4s
            fmax    v19.4s, v16.4s, v18.4s
            fmin    v16.4s, v16.4s, v18.4s
            fmax    v26.4s, v25.4s, v17.4s
            fmin    v17.4s, v25.4s, v17.4s
            fmax    v18.4s, v17.4s, v19.4s
            fmin    v17.4s, v17.4s, v19.4s
            fmax    v19.4s, v16.4s, v20.4s
            fmax    v25.4s, v17.4s, v19.4s
            fmax    v27.4s, v18.4s, v25.4s
            fmin    v16.4s, v16.4s, v20.4s
            fmin    v17.4s, v17.4s, v19.4s
            fmin    v18.4s, v18.4s, v25.4s
            fmin    v19.4s, v26.4s, v27.4s
            fmax    v20.4s, v16.4s, v21.4s
            fmax    v25.4s, v17.4s, v20.4s
            fmax    v26.4s, v18.4s, v25.4s
            fmin    v16.4s, v16.4s, v21.4s
            fmin    v17.4s, v17.4s, v20.4s
            fmin    v18.4s, v18.4s, v25.4s
            fmin    v19.4s, v19.4s, v26.4s
            fmax    v21.4s, v16.4s, v22.4s
            fmax    v20.4s, v17.4s, v21.4s
            fmax    v25.4s, v18.4s, v20.4s
            fmin    v16.4s, v16.4s, v22.4s
            fmin    v17.4s, v17.4s, v21.4s
            fmin    v18.4s, v18.4s, v20.4s
            fmin    v19.4s, v19.4s, v25.4s
            fmax    v22.4s, v16.4s, v23.4s
            fmax    v20.4s, v17.4s, v22.4s
            fmax    v21.4s, v18.4s, v20.4s
            fmin    v16.4s, v16.4s, v23.4s
            fmin    v17.4s, v17.4s, v22.4s
            fmin    v18.4s, v18.4s, v20.4s
            fmin    v19.4s, v19.4s, v21.4s
            fmax    v23.4s, v16.4s, v24.4s
            fmax    v20.4s, v17.4s, v23.4s
            fmax    v21.4s, v18.4s, v20.4s
            fmin    v16.4s, v16.4s, v24.4s
            fmin    v17.4s, v17.4s, v23.4s
            fmin    v18.4s, v18.4s, v20.4s
            fmin    v19.4s, v19.4s, v21.4s
            mov     v20.16b, v19.16b
            mov     v19.16b, v17.16b
            mov     v17.16b, v18.16b
            b       G_M000_IG07

G_M000_IG06:                ;; offset=0x0284
            fmax    v25.4s, v16.4s, v19.4s
            fmin    v16.4s, v16.4s, v19.4s
            fmax    v19.4s, v17.4s, v23.4s
            fmin    v17.4s, v17.4s, v23.4s
            fmax    v23.4s, v18.4s, v21.4s
            fmin    v18.4s, v18.4s, v21.4s
            fmax    v21.4s, v20.4s, v24.4s
            fmin    v20.4s, v20.4s, v24.4s
            fmax    v24.4s, v16.4s, v19.4s
            fmin    v16.4s, v16.4s, v19.4s
            fmax    v19.4s, v18.4s, v20.4s
            fmin    v18.4s, v18.4s, v20.4s
            fmax    v26.4s, v25.4s, v21.4s
            fmin    v20.4s, v25.4s, v21.4s
            fmax    v21.4s, v23.4s, v22.4s
            fmin    v22.4s, v23.4s, v22.4s
            fmax    v23.4s, v16.4s, v18.4s
            fmin    v16.4s, v16.4s, v18.4s
            fmax    v18.4s, v17.4s, v20.4s
            fmin    v17.4s, v17.4s, v20.4s
            fmax    v25.4s, v19.4s, v22.4s
            fmin    v20.4s, v19.4s, v22.4s
            fmin    v22.4s, v24.4s, v26.4s
            fmax    v19.4s, v17.4s, v20.4s
            fmin    v17.4s, v17.4s, v20.4s
            fmin    v20.4s, v18.4s, v21.4s
            fmin    v22.4s, v25.4s, v22.4s
            fmax    v18.4s, v16.4s, v17.4s
            fmin    v16.4s, v16.4s, v17.4s
            fmax    v17.4s, v23.4s, v19.4s
            fmin    v19.4s, v23.4s, v19.4s
            fmax    v21.4s, v20.4s, v22.4s
            fmin    v20.4s, v20.4s, v22.4s
            fmax    v22.4s, v19.4s, v20.4s
            fmin    v19.4s, v19.4s, v20.4s
            fmin    v20.4s, v17.4s, v21.4s
            fmax    v17.4s, v18.4s, v19.4s
            fmin    v19.4s, v18.4s, v19.4s
            fmin    v20.4s, v22.4s, v20.4s

G_M000_IG07:                ;; offset=0x0320
            movi    v18.4s, #0x3E,  LSL #24
            fmul    v16.4s, v16.4s, v18.4s
            ldr     q18, [@RWD00]
            fmul    v18.4s, v19.4s, v18.4s
            fadd    v16.4s, v16.4s, v18.4s
            ldr     q18, [@RWD16]
            fmul    v17.4s, v17.4s, v18.4s
            ldr     q18, [@RWD32]
            fmul    v18.4s, v20.4s, v18.4s
            fadd    v17.4s, v17.4s, v18.4s
            fadd    v16.4s, v16.4s, v17.4s
            ldr     x3, [x19, #0x28]
            add     w4, w21, w22
            ldr     w5, [x3, #0x08]
            cmp     w5, w4
            bls     G_M000_IG12
            sub     w5, w5, w4
            cmp     w5, #4
            blt     G_M000_IG13
            add     x3, x3, #16
            ubfiz   x4, x4, #2, #32
            str     q16, [x3, x4]
            add     w22, w22, #4
            ldr     w3, [x19, #0x58]
            sub     w3, w3, #4
            cmp     w22, w3
            blt     G_M000_IG04

G_M000_IG08:                ;; offset=0x038C
            ldr     w0, [x19, #0x58]
            cmp     w22, w0
            bge     G_M000_IG10

G_M000_IG09:                ;; offset=0x0398
            mov     x0, x19
            mov     w1, w20
            mov     w2, w21
            mov     w3, w22
            movz    x4, #0x4780
            movk    x4, #0x695F LSL #16
            movk    x4, #0x7FF8 LSL #32
            ldr     x4, [x4]
            blr     x4      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this
            add     w22, w22, #1
            ldr     w0, [x19, #0x58]
            cmp     w22, w0
            blt     G_M000_IG09

G_M000_IG10:                ;; offset=0x03CC
            ldp     x21, x22, [sp, #0x20]
            ldp     x19, x20, [sp, #0x10]
            ldp     fp, lr, [sp], #0x30
            ret     lr

G_M000_IG11:                ;; offset=0x03DC
            movz    x0, #0x4750
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()
            brk     #0

G_M000_IG12:                ;; offset=0x03F4
            movz    x0, #0x4768
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLess()
            brk     #0

G_M000_IG13:                ;; offset=0x040C
            movz    x0, #0x4468
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentException_DestinationTooShort()
            brk     #0

G_M000_IG14:                ;; offset=0x0424
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dq	3D99999A3D99999Ah, 3D99999A3D99999Ah
RWD16  	dq	3D75C28F3D75C28Fh, 3D75C28F3D75C28Fh
RWD32  	dq	3D4CCCCD3D4CCCCDh, 3D4CCCCD3D4CCCCDh

; Total bytes of code 1068

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data
; 2 inlinees with PGO data; 29 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x10]!
            mov     fp, sp

G_M000_IG02:                ;; offset=0x0008
            sub     w4, w3, #1
            cmp     w4, #0
            csel    w4, w4, wzr, ge
            add     w5, w3, #1
            ldr     w6, [x0, #0x58]
            sub     w7, w6, #1
            cmp     w5, w7
            csel    w5, w5, w7, le
            sub     w7, w1, #1
            mul     w7, w7, w6
            mul     w8, w1, w6
            add     w1, w1, #1
            mul     w1, w1, w6
            ldr     x6, [x0, #0x18]
            add     w9, w7, w4
            ldr     w10, [x6, #0x08]
            cmp     w9, w10
            bhs     G_M000_IG07
            add     x6, x6, #16
            ldr     s16, [x6, w9, UXTW #2]
            add     w9, w7, w3
            cmp     w9, w10
            bhs     G_M000_IG07
            ldr     s17, [x6, w9, UXTW #2]
            add     w7, w7, w5
            cmp     w7, w10
            bhs     G_M000_IG07
            ldr     s18, [x6, w7, UXTW #2]
            add     w7, w8, w4
            cmp     w7, w10
            bhs     G_M000_IG07
            ldr     s19, [x6, w7, UXTW #2]
            add     w7, w8, w3
            cmp     w7, w10
            bhs     G_M000_IG07
            ldr     s20, [x6, w7, UXTW #2]
            add     w7, w8, w5
            cmp     w7, w10
            bhs     G_M000_IG07
            ldr     s21, [x6, w7, UXTW #2]
            add     w4, w1, w4
            cmp     w4, w10
            bhs     G_M000_IG07
            ldr     s22, [x6, w4, UXTW #2]
            add     w4, w1, w3
            cmp     w4, w10
            bhs     G_M000_IG07
            ldr     s23, [x6, w4, UXTW #2]
            add     w1, w1, w5
            cmp     w1, w10
            bhs     G_M000_IG07
            ldr     s24, [x6, w1, UXTW #2]
            ldrb    w1, [x0, #0x68]
            cbnz    w1, G_M000_IG04

G_M000_IG03:                ;; offset=0x00E0
            fmax    s25, s16, s17
            fmin    s16, s16, s17
            fmax    s17, s18, s19
            fmin    s18, s18, s19
            fmax    s19, s16, s18
            fmin    s16, s16, s18
            fmax    s18, s25, s17
            fmin    s17, s25, s17
            fmax    s25, s17, s19
            fmin    s17, s17, s19
            fmax    s19, s16, s20
            fmax    s26, s17, s19
            fmax    s27, s25, s26
            fmin    s16, s16, s20
            fmin    s17, s17, s19
            fmin    s20, s25, s26
            fmin    s19, s18, s27
            fmax    s18, s16, s21
            fmax    s25, s17, s18
            fmax    s26, s20, s25
            fmin    s16, s16, s21
            fmin    s17, s17, s18
            fmin    s20, s20, s25
            fmin    s19, s19, s26
            fmax    s21, s16, s22
            fmax    s18, s17, s21
            fmax    s25, s20, s18
            fmin    s16, s16, s22
            fmin    s17, s17, s21
            fmin    s20, s20, s18
            fmin    s19, s19, s25
            fmax    s22, s16, s23
            fmax    s18, s17, s22
            fmax    s21, s20, s18
            fmin    s16, s16, s23
            fmin    s17, s17, s22
            fmin    s20, s20, s18
            fmin    s19, s19, s21
            fmax    s23, s16, s24
            fmax    s18, s17, s23
            fmax    s21, s20, s18
            fmin    s16, s16, s24
            fmin    s17, s17, s23
            fmin    s20, s20, s18
            fmin    s19, s19, s21
            fmov    s22, s19
            fmov    s0, s17
            fmov    s17, s20
            fmov    s20, s0
            b       G_M000_IG05

G_M000_IG04:                ;; offset=0x01A8
            fmax    s25, s16, s19
            fmin    s16, s16, s19
            fmax    s19, s17, s23
            fmin    s17, s17, s23
            fmax    s23, s18, s21
            fmin    s18, s18, s21
            fmax    s21, s20, s24
            fmin    s20, s20, s24
            fmax    s24, s16, s19
            fmin    s16, s16, s19
            fmax    s19, s18, s20
            fmin    s18, s18, s20
            fmax    s20, s25, s21
            fmin    s21, s25, s21
            fmax    s25, s23, s22
            fmin    s22, s23, s22
            fmax    s23, s16, s18
            fmin    s16, s16, s18
            fmax    s18, s17, s21
            fmin    s17, s17, s21
            fmax    s21, s19, s22
            fmin    s22, s19, s22
            fmin    s19, s24, s20
            fmax    s20, s17, s22
            fmin    s17, s17, s22
            fmin    s22, s18, s25
            fmin    s18, s21, s19
            fmax    s19, s16, s17
            fmin    s16, s16, s17
            fmax    s17, s23, s20
            fmin    s20, s23, s20
            fmax    s21, s22, s18
            fmin    s22, s22, s18
            fmax    s18, s20, s22
            fmin    s20, s20, s22
            fmin    s22, s17, s21
            fmax    s17, s19, s20
            fmin    s20, s19, s20
            fmin    s22, s18, s22

G_M000_IG05:                ;; offset=0x0244
            ldr     x0, [x0, #0x28]
            add     w1, w2, w3
            ldr     w4, [x0, #0x08]
            cmp     w1, w4
            bhs     G_M000_IG07
            add     w1, w2, w3
            add     x0, x0, #16
            fmov    s18, #0.1250
            fmul    s16, s16, s18
            ldr     s18, [@RWD00]
            fmul    s18, s20, s18
            fadd    s16, s16, s18
            ldr     s18, [@RWD04]
            fmul    s17, s17, s18
            ldr     s18, [@RWD08]
            fmul    s18, s22, s18
            fadd    s17, s17, s18
            fadd    s16, s16, s17
            str     s16, [x0, w1, UXTW #2]

G_M000_IG06:                ;; offset=0x0290
            ldp     fp, lr, [sp], #0x10
            ret     lr

G_M000_IG07:                ;; offset=0x0298
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dd	3D99999Ah		;     0.075
RWD04  	dd	3D75C28Fh		;      0.06
RWD08  	dd	3D4CCCCDh		;      0.05

; Total bytes of code 672

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeMask(float):float (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x10]!
            mov     fp, sp

G_M000_IG02:                ;; offset=0x0008
            ldr     s16, [@RWD00]
            fmul    s0, s0, s16
            ldr     d16, [@RWD04]
            fmax    s0, s0, s16
            fmul    s16, s0, s0
            ldr     s17, [@RWD12]
            fadd    s17, s16, s17
            fmov    s18, #1.0000
            fdiv    s17, s18, s17
            ldr     s18, [@RWD16]
            fmul    s17, s17, s18
            ldr     s18, [@RWD20]
            fadd    s16, s16, s18
            fmov    s18, #1.0000
            fdiv    s16, s18, s16
            ldr     s18, [@RWD24]
            fmul    s16, s16, s18
            ldr     s18, [@RWD28]
            fadd    s0, s0, s18
            fmov    s18, #1.0000
            fdiv    s0, s18, s0
            ldr     s18, [@RWD32]
            fmul    s0, s0, s18
            fadd    s0, s16, s0
            fadd    s0, s17, s0
            ldr     s16, [@RWD36]
            fadd    s0, s0, s16

G_M000_IG03:                ;; offset=0x0074
            ldp     fp, lr, [sp], #0x10
            ret     lr

RWD00  	dd	3F3F62FEh		;  0.747604
RWD04  	dq	3A83126F3A83126Fh
RWD12  	dd	3F0C5322h		;  0.548144
RWD16  	dd	404F0F94h		;   3.23533
RWD20  	dd	400C5322h		;   2.19257
RWD24  	dd	40A0B47Bh		;   5.02203
RWD28  	dd	4398852Ah		;    305.04
RWD32  	dd	414E7F18h		;    12.906
RWD36  	dd	BF3DE353h		;  -0.74175

; Total bytes of code 124

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ModulateScalar(System.ReadOnlySpan`1[float],int,float):float (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 0 single block inlinees; 2 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x20]!
            stp     d8, d9, [sp, #0x10]
            mov     fp, sp
            fmov    s8, s0

G_M000_IG02:                ;; offset=0x0010
            movi    v9.16b, #0
            movi    v0.16b, #0
            mov     w3, wzr
            ldr     s16, [@RWD00]
            ldr     s17, [@RWD04]
            ldr     s18, [@RWD08]
            ldr     s19, [@RWD12]
            ldr     s20, [@RWD16]

G_M000_IG03:                ;; offset=0x0030
            mul     w4, w3, w2
            cmp     w3, #7
            bne     G_M000_IG05

G_M000_IG04:                ;; offset=0x003C
            mov     w5, w4
            b       G_M000_IG06
            align   [0 bytes for IG07]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG05:                ;; offset=0x0044
            add     w5, w4, w2

G_M000_IG06:                ;; offset=0x0048
            cmp     w4, w1
            bhs     G_M000_IG11
            ldr     s21, [x0, w4, UXTW #2]
            mov     w6, wzr

G_M000_IG07:                ;; offset=0x0058
            add     w7, w4, w6
            add     w7, w7, #1
            cmp     w7, w1
            bhs     G_M000_IG11
            ldr     s22, [x0, w7, UXTW #2]
            add     w7, w5, w6
            cmp     w7, w1
            bhs     G_M000_IG11
            ldr     s23, [x0, w7, UXTW #2]
            fsub    s23, s21, s23
            fabs    s23, s23
            fsub    s24, s21, s22
            fabs    s24, s24
            fadd    s24, s24, s9
            fadd    s9, s23, s24
            fadd    s21, s21, s16
            movi    v23.2s, #0
            fmax    s21, s21, s23
            fmul    s23, s21, s21
            fmul    s21, s21, s17
            fmul    s21, s21, s23
            fadd    s21, s21, s18
            fmul    s23, s23, s19
            fadd    s23, s23, s20
            fdiv    s21, s23, s21
            fadd    s0, s21, s0
            fmov    s21, s22
            add     w6, w6, #1
            cmp     w6, #7
            blt     G_M000_IG07

G_M000_IG08:                ;; offset=0x00D0
            add     w4, w5, #7
            cmp     w4, w1
            bhs     G_M000_IG11
            ldr     s22, [x0, w4, UXTW #2]
            fsub    s22, s21, s22
            fabs    s22, s22
            fadd    s9, s22, s9
            fadd    s21, s21, s16
            movi    v22.2s, #0
            fmax    s21, s21, s22
            fmul    s22, s21, s21
            fmul    s21, s21, s17
            fmul    s21, s21, s22
            fadd    s21, s21, s18
            fmul    s22, s22, s19
            fadd    s22, s22, s20
            fdiv    s21, s22, s21
            fadd    s0, s21, s0
            add     w3, w3, #1
            cmp     w3, #8
            blt     G_M000_IG03

G_M000_IG09:                ;; offset=0x0124
            ldr     s16, [@RWD20]
            fmul    s0, s0, s16
            movz    x0, #0x4528
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:FastLog2(float):float
            ldr     s16, [@RWD24]
            fmul    s0, s0, s16
            ldr     s16, [@RWD28]
            fmul    s16, s9, s16
            fadd    s16, s16, s8
            fadd    s0, s0, s16

G_M000_IG10:                ;; offset=0x0158
            ldp     d8, d9, [sp, #0x10]
            ldp     fp, lr, [sp], #0x20
            ret     lr

G_M000_IG11:                ;; offset=0x0164
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dd	42233332h		;      40.8
RWD04  	dd	3B1DEA78h		; 0.00240961
RWD08  	dd	449E380Dh		;   1265.75
RWD12  	dd	42EF0C23h		;   119.524
RWD16  	dd	44228FFEh		;    650.25
RWD20  	dd	38808081h		; 6.12745e-05
RWD24  	dd	BDDC6A08h		; -0.107624
RWD28  	dd	B8933E1Eh		; -7.02108e-05

; Total bytes of code 364

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:FastLog2(float):float (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x10]!
            mov     fp, sp

G_M000_IG02:                ;; offset=0x0008
            mov     w0, v0.s[0]
            movz    w1, #0x5555
            movk    w1, #0xC0D5 LSL #16
            add     w1, w0, w1
            asr     w1, w1, #23
            sub     w0, w0, w1,  LSL #23
            fmov    s0, w0
            fmov    s16, #1.0000
            fsub    s0, s0, s16
            ldr     s16, [@RWD00]
            fmul    s16, s0, s16
            ldr     s17, [@RWD04]
            fadd    s16, s16, s17
            fmul    s16, s16, s0
            ldr     s17, [@RWD08]
            fsub    s16, s16, s17
            ldr     s17, [@RWD12]
            fmul    s17, s0, s17
            ldr     s18, [@RWD16]
            fadd    s17, s17, s18
            fmul    s0, s17, s0
            ldr     s17, [@RWD20]
            fadd    s0, s0, s17
            fdiv    s0, s16, s0
            scvtf   s16, w1
            fadd    s0, s0, s16

G_M000_IG03:                ;; offset=0x0070
            ldp     fp, lr, [sp], #0x10
            ret     lr

RWD00  	dd	3F3E11C7h		;  0.742459
RWD04  	dd	3FB6E02Bh		;   1.42872
RWD08  	dd	35F85AB0h		; 1.85038e-06
RWD12  	dd	3E32458Ch		;  0.174093
RWD16  	dd	3F813CEDh		;   1.00967
RWD20  	dd	3F7D8625h		;  0.990328

; Total bytes of code 120

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:FastPow2(float):float (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x10]!
            mov     fp, sp

G_M000_IG02:                ;; offset=0x0008
            frintm  s16, s0
            fcvtzs  w0, s16
            add     w0, w0, #127
            lsl     w0, w0, #23
            fmov    s17, w0
            fsub    s0, s0, s16
            ldr     s16, [@RWD00]
            fadd    s16, s0, s16
            fmul    s16, s16, s0
            ldr     s18, [@RWD04]
            fadd    s16, s16, s18
            fmul    s16, s16, s0
            ldr     s18, [@RWD08]
            fadd    s16, s16, s18
            fmul    s16, s16, s17
            ldr     s17, [@RWD12]
            fmul    s17, s0, s17
            ldr     s18, [@RWD16]
            fsub    s17, s17, s18
            fmul    s17, s17, s0
            ldr     s18, [@RWD20]
            fsub    s17, s17, s18
            fmul    s0, s17, s0
            ldr     s17, [@RWD24]
            fadd    s0, s0, s17
            fdiv    s0, s16, s0

G_M000_IG03:                ;; offset=0x0070
            ldp     fp, lr, [sp], #0x10
            ret     lr

RWD00  	dd	4122CC6Bh		;   10.1749
RWD04  	dd	424379A1h		;   48.8688
RWD08  	dd	42C519F0h		;   98.5507
RWD12  	dd	3E5749EEh		;  0.210243
RWD16  	dd	3CB621BEh		; 0.0222329
RWD20  	dd	419B8831h		;   19.4415
RWD24  	dd	42C519F1h		;   98.5507

; Total bytes of code 120

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ComputeRowCore(DotJpegli.Internal.PixelSource,int,int,int,System.Span`1[float],bool,bool,System.Threading.CancellationToken):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data
; 9 inlinees with PGO data; 35 single block inlinees; 33 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            sub     sp, sp, #288
            stp     d8, d9, [sp, #0x88]
            stp     d10, d11, [sp, #0x98]
            stp     d12, d13, [sp, #0xA8]
            str     d14, [sp, #0xB8]
            stp     x19, x20, [sp, #0xC0]
            stp     x21, x22, [sp, #0xD0]
            stp     x23, x24, [sp, #0xE0]
            stp     x25, x26, [sp, #0xF0]
            stp     x27, x28, [sp, #0x100]
            stp     fp, lr, [sp, #0x110]
            add     fp, sp, #272
            str     xzr, [fp, #-0xE0]
            movz    x8, #0x2D5A
            movk    x8, #0xF7B0 LSL #16
            movk    x8, #0x2883 LSL #32
            str     x8, [fp, #-0x90]
            mov     x19, x0
            mov     x20, x1
            mov     w23, w2
            mov     w21, w3
            mov     w24, w4
            mov     w22, w7

G_M000_IG02:                ;; offset=0x005C
            ldr     x2, [fp, #0x18]
            str     x2, [fp, #-0xE0]
            mov     x25, x5
            mov     w26, w6
            ldrb    w2, [x19, #0x61]
            cbnz    w2, G_M000_IG126
            ldr     w2, [x19, #0x40]
            lsl     w27, w2, #3
            ldr     x28, [x20, #0x08]
            cbz     x28, G_M000_IG04

G_M000_IG03:                ;; offset=0x0084
            ldr     x2, [x19, #0x30]
            cmp     x28, x2
            bne     G_M000_IG04
            ldr     w2, [x19, #0x54]
            add     w2, w2, w27
            cmp     w2, w21
            bne     G_M000_IG04
            ldr     w2, [x19, #0x58]
            cmp     w23, w2
            beq     G_M000_IG05

G_M000_IG04:                ;; offset=0x00AC
            mov     w3, wzr
            b       G_M000_IG06

G_M000_IG05:                ;; offset=0x00B4
            uxtb    w4, w22
            ldrb    w3, [x19, #0x62]
            cmp     w4, w3
            cset    x3, eq

G_M000_IG06:                ;; offset=0x00C4
            str     w3, [fp, #-0x94]
            str     xzr, [x19, #0x30]
            ldr     x2, [fp, #-0xE0]
            cbnz    x2, G_M000_IG123

G_M000_IG07:                ;; offset=0x00D4
            tbnz    w21, #31, G_M000_IG128
            ldr     w2, [x19, #0x3C]
            cmp     w21, w2
            bge     G_M000_IG128
            cbz     w27, G_M000_IG137
            sdiv    w2, w21, w27
            msub    w2, w2, w27, w21
            cbnz    w2, G_M000_IG128
            ldr     w2, [x19, #0x5C]
            ldr     w0, [x19, #0x40]
            mul     w2, w2, w0
            cmp     w2, w26
            bgt     G_M000_IG129
            sub     w5, w21, #5
            str     w5, [fp, #-0x98]
            mov     w6, wzr
            cbz     w3, G_M000_IG124

G_M000_IG08:                ;; offset=0x0118
            ldr     x2, [x19, #0x10]
            mov     x0, x2
            ldr     w1, [x19, #0x4C]
            mul     w4, w27, w1
            mov     w6, #10
            mul     w1, w1, w6
            cbnz    x0, G_M000_IG125
            orr     w0, w4, w1
            cbnz    w0, G_M000_IG130
            mov     x4, xzr
            mov     w1, wzr

G_M000_IG09:                ;; offset=0x0144
            cbz     x2, G_M000_IG10
            add     x0, x2, #16
            ldr     w2, [x2, #0x08]
            b       G_M000_IG11

G_M000_IG10:                ;; offset=0x0154
            mov     x0, xzr
            mov     w2, wzr

G_M000_IG11:                ;; offset=0x015C
            cmp     w1, w2
            bgt     G_M000_IG131
            ubfiz   x2, x1, #2, #32
            mov     x1, x4
            movz    x4, #0x56E0
            movk    x4, #0x6938 LSL #16
            movk    x4, #0x7FF8 LSL #32
            ldr     x4, [x4]
            blr     x4      // code for System.SpanHelpers:Memmove(byref,byref,nuint)
            mov     w6, #10
            mov     w3, w6
            ldr     w4, [fp, #-0x98]
            add     w1, w4, #9
            ldr     w0, [x19, #0x3C]
            sub     w5, w0, #1
            tbnz    w5, #31, G_M000_IG132
            tbnz    w1, #31, G_M000_IG12
            cmp     w1, w5
            csel    w7, w1, w5, le
            b       G_M000_IG13

G_M000_IG12:                ;; offset=0x01AC
            mov     w7, wzr

G_M000_IG13:                ;; offset=0x01B0
            mov     w6, w3

G_M000_IG14:                ;; offset=0x01B4
            mov     w4, w6
            b       G_M000_IG52

G_M000_IG15:                ;; offset=0x01BC
            ldr     x1, [fp, #-0xE0]
            add     x1, x1, #32
            ldapr   w1, [x1]
            cbz     w1, G_M000_IG54
            b       G_M000_IG127

G_M000_IG16:                ;; offset=0x01D0
            mov     w6, w8
            b       G_M000_IG18

G_M000_IG17:                ;; offset=0x01D8
            mov     w6, wzr

G_M000_IG18:                ;; offset=0x01DC
            str     w6, [fp, #-0xA0]
            ldr     w8, [x19, #0x4C]
            str     w4, [fp, #-0x9C]
            mul     w9, w4, w8
            add     w10, w9, #1
            str     w10, [fp, #-0xA4]
            cmp     w6, w7
            beq     G_M000_IG56

G_M000_IG19:                ;; offset=0x01FC
            uxtb    w0, w22
            mov     w11, w0
            cbnz    w11, G_M000_IG40
            ldr     w7, [x19, #0x38]
            mov     w8, w7
            str     w8, [fp, #-0xBC]
            ldr     w0, [x20, #0x1C]
            cbnz    w0, G_M000_IG22
            ldrb    w0, [x20, #0x26]
            cbz     w0, G_M000_IG20
            mov     w0, #4
            b       G_M000_IG22

G_M000_IG20:                ;; offset=0x022C
            ldr     w0, [x20, #0x20]
            cmp     w0, #1
            bne     G_M000_IG21
            mov     w0, #1
            b       G_M000_IG22

G_M000_IG21:                ;; offset=0x0240
            mov     w0, #2
            b       G_M000_IG22

G_M000_IG22:                ;; offset=0x0248
            cmp     w0, #3
            bne     G_M000_IG23
            mov     w9, wzr
            b       G_M000_IG24

G_M000_IG23:                ;; offset=0x0258
            cmp     w0, #4
            cset    x9, ne

G_M000_IG24:                ;; offset=0x0260
            cmp     w9, #0
            cset    x9, eq
            str     w9, [fp, #-0xC0]
            ldr     x0, [x19, #0x10]
            cbz     x0, G_M000_IG25
            mov     w11, w10
            add     x11, x11, w7, UXTW
            ldr     w13, [x0, #0x08]
            cmp     x11, x13
            bhi     G_M000_IG130
            add     x0, x0, #16
            ubfiz   x11, x10, #2, #32
            add     x11, x0, x11
            b       G_M000_IG26

G_M000_IG25:                ;; offset=0x0298
            orr     w11, w10, w7
            cbnz    w11, G_M000_IG130
            mov     x11, xzr
            mov     w7, wzr

G_M000_IG26:                ;; offset=0x02A8
            str     x11, [fp, #-0xF8]
            mov     w14, w7
            str     w14, [fp, #-0xD4]
            ldr     x0, [x20, #0x08]
            cbz     x0, G_M000_IG29
            ldrb    w0, [x0, #0x2C]
            cbnz    w0, G_M000_IG28

G_M000_IG27:                ;; offset=0x02C4
            b       G_M000_IG39

G_M000_IG28:                ;; offset=0x02C8
            cbz     w9, G_M000_IG39

G_M000_IG29:                ;; offset=0x02CC
            cmp     w6, w1
            bgt     G_M000_IG30
            mov     w0, w6
            ldr     w6, [fp, #-0xA0]
            b       G_M000_IG31

G_M000_IG30:                ;; offset=0x02E0
            mov     w0, w1

G_M000_IG31:                ;; offset=0x02E4
            mul     w7, w0, w23
            str     w7, [fp, #-0xC4]
            mov     w11, wzr
            cbnz    w14, G_M000_IG37

G_M000_IG32:                ;; offset=0x02F4
            b       G_M000_IG48

G_M000_IG33:                ;; offset=0x02F8
            mov     w0, w1
            str     w11, [fp, #-0xC8]
            b       G_M000_IG35

G_M000_IG34:                ;; offset=0x0304
            mov     w0, wzr
            str     w11, [fp, #-0xC8]

G_M000_IG35:                ;; offset=0x030C
            ldr     w1, [x20, #0x20]
            madd    w1, w0, w1, w7
            mov     x0, x20
            mov     w2, w9
            movz    x12, #0x44B0
            movk    x12, #0x695F LSL #16
            movk    x12, #0x7FF8 LSL #32
            ldr     x12, [x12]
            blr     x12      // code for DotJpegli.Internal.PixelSource:ReadSample(int,int):float:this
            ldr     x6, [fp, #-0xF8]
            ldr     w7, [fp, #-0xC8]
            str     s0, [x6, w7, UXTW #2]
            add     w7, w7, #1
            ldr     w0, [fp, #-0xD4]
            cmp     w7, w0
            mov     w11, w7
            ldp     w9, w8, [fp, #-0xC0]
            bge     G_M000_IG48

G_M000_IG36:                ;; offset=0x0354
            ldr     w6, [fp, #-0xA0]
            ldr     w7, [fp, #-0xC4]

G_M000_IG37:                ;; offset=0x035C
            sub     w1, w8, #1
            tbnz    w1, #31, G_M000_IG134
            ldr     w8, [fp, #-0xBC]
            tbnz    w11, #31, G_M000_IG34

G_M000_IG38:                ;; offset=0x036C
            cmp     w11, w1
            bgt     G_M000_IG33
            str     w11, [fp, #-0xC8]
            mov     w0, w11
            b       G_M000_IG35

G_M000_IG39:                ;; offset=0x0380
            mov     x6, x11
            mov     x0, x20
            mov     w1, w8
            mov     w3, w9
            ldr     w5, [fp, #-0xA0]
            mov     w4, wzr
            movz    x8, #0x4498
            movk    x8, #0x695F LSL #16
            movk    x8, #0x7FF8 LSL #32
            ldr     x8, [x8]
            blr     x8      // code for DotJpegli.Internal.PixelSource:ReadConvertedRow(int,int,int,int,int,System.Span`1[float]):this
            b       G_M000_IG48

G_M000_IG40:                ;; offset=0x03B0
            ldr     w7, [x19, #0x38]
            mov     w1, w7
            ldr     w0, [x20, #0x1C]
            cbnz    w0, G_M000_IG43
            ldrb    w0, [x20, #0x26]
            cbz     w0, G_M000_IG41
            mov     w0, #4
            b       G_M000_IG43

G_M000_IG41:                ;; offset=0x03D0
            ldr     w0, [x20, #0x20]
            cmp     w0, #1
            bne     G_M000_IG42
            mov     w0, #1
            b       G_M000_IG43

G_M000_IG42:                ;; offset=0x03E4
            mov     w0, #2
            b       G_M000_IG43

G_M000_IG43:                ;; offset=0x03EC
            cmp     w0, #3
            bne     G_M000_IG44
            mov     w8, wzr
            b       G_M000_IG45

G_M000_IG44:                ;; offset=0x03FC
            cmp     w0, #4
            cset    x8, ne

G_M000_IG45:                ;; offset=0x0404
            cmp     w8, #0
            cset    x8, eq
            ldr     x0, [x19, #0x10]
            cbz     x0, G_M000_IG46
            mov     w9, w10
            add     x9, x9, w7, UXTW
            ldr     w11, [x0, #0x08]
            cmp     x9, x11
            bhi     G_M000_IG130
            add     x0, x0, #16
            ubfiz   x9, x10, #2, #32
            add     x0, x0, x9
            b       G_M000_IG47

G_M000_IG46:                ;; offset=0x0438
            orr     w0, w10, w7
            cbnz    w0, G_M000_IG130
            mov     x0, xzr
            mov     w7, wzr

G_M000_IG47:                ;; offset=0x0448
            str     x0, [sp]
            str     w7, [sp, #0x08]
            mov     x0, x20
            mov     w3, w23
            mov     w4, w8
            mov     w5, wzr
            movz    x7, #0x4480
            movk    x7, #0x695F LSL #16
            movk    x7, #0x7FF8 LSL #32
            ldr     x7, [x7]
            blr     x7      // code for DotJpegli.Internal.PixelSource:ReadRowVector(int,int,int,int,int,int,System.Span`1[float]):this

G_M000_IG48:                ;; offset=0x0474
            ldr     x1, [x19, #0x10]
            mov     x0, x1
            ldr     w2, [x0, #0x08]
            ldr     w10, [fp, #-0xA4]
            cmp     w10, w2
            bhs     G_M000_IG138
            add     x0, x0, #16
            ldr     s0, [x0, w10, UXTW #2]
            sub     w3, w10, #1
            cmp     w3, w2
            bhs     G_M000_IG138
            sub     w3, w10, #1
            str     s0, [x0, w3, UXTW #2]
            ldr     w3, [x19, #0x38]
            add     w4, w10, w3
            mov     w5, w4
            ldr     w6, [x19, #0x44]
            sub     w3, w6, w3
            add     w3, w3, #1
            cbz     x1, G_M000_IG49
            mov     w6, w5
            add     x6, x6, w3, UXTW
            cmp     x6, w2, UXTW
            bhi     G_M000_IG130
            add     x1, x1, #16
            ubfiz   x5, x5, #2, #32
            add     x5, x1, x5
            b       G_M000_IG50

G_M000_IG49:                ;; offset=0x04E4
            orr     w1, w5, w3
            cbnz    w1, G_M000_IG130
            mov     x5, xzr
            mov     w3, wzr

G_M000_IG50:                ;; offset=0x04F4
            sub     w1, w4, #1
            cmp     w1, w2
            bhs     G_M000_IG138
            ldr     s0, [x0, w1, UXTW #2]
            mov     w1, w3
            mov     x0, x5
            movz    x2, #0x42E8
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.SpanHelpers:Fill[float](byref,nuint,float)

G_M000_IG51:                ;; offset=0x0520
            ldp     w7, w1, [fp, #-0xA0]
            add     w1, w1, #1
            mov     w4, w1

G_M000_IG52:                ;; offset=0x052C
            add     w1, w27, #10
            cmp     w4, w1
            bge     G_M000_IG61

G_M000_IG53:                ;; offset=0x0538
            ldr     x1, [fp, #-0xE0]
            cbnz    x1, G_M000_IG15

G_M000_IG54:                ;; offset=0x0540
            ldr     w5, [fp, #-0x98]
            add     w6, w5, w4
            ldr     w2, [x19, #0x3C]
            sub     w1, w2, #1
            mov     w8, w1
            tbnz    w8, #31, G_M000_IG133
            tbnz    w6, #31, G_M000_IG17

G_M000_IG55:                ;; offset=0x055C
            cmp     w6, w8
            bgt     G_M000_IG16
            b       G_M000_IG18

G_M000_IG56:                ;; offset=0x0568
            ldr     x2, [x19, #0x10]
            ldr     w4, [fp, #-0x9C]
            sub     w0, w4, #1
            mul     w0, w0, w8
            mov     w1, w8
            cbz     x2, G_M000_IG57
            mov     w7, w0
            add     x7, x7, w1, UXTW
            ldr     w10, [x2, #0x08]
            cmp     x7, x10
            bhi     G_M000_IG130
            add     x2, x2, #16
            ubfiz   x0, x0, #2, #32
            add     x7, x2, x0
            b       G_M000_IG58

G_M000_IG57:                ;; offset=0x05A4
            orr     w7, w0, w1
            cbnz    w7, G_M000_IG130
            mov     x7, xzr
            mov     w1, wzr

G_M000_IG58:                ;; offset=0x05B4
            ldr     x2, [x19, #0x10]
            cbz     x2, G_M000_IG59
            mov     w0, w9
            add     x0, x0, w8, UXTW
            ldr     w10, [x2, #0x08]
            cmp     x0, x10
            bhi     G_M000_IG130
            add     x2, x2, #16
            ubfiz   x9, x9, #2, #32
            add     x0, x2, x9
            b       G_M000_IG60

G_M000_IG59:                ;; offset=0x05E0
            orr     w0, w9, w8
            cbnz    w0, G_M000_IG130
            mov     x0, xzr
            mov     w8, wzr

G_M000_IG60:                ;; offset=0x05F0
            cmp     w1, w8
            bgt     G_M000_IG131
            ubfiz   x2, x1, #2, #32
            mov     x1, x7
            movz    x7, #0x56E0
            movk    x7, #0x6938 LSL #16
            movk    x7, #0x7FF8 LSL #32
            ldr     x7, [x7]
            blr     x7      // code for System.SpanHelpers:Memmove(byref,byref,nuint)
            b       G_M000_IG51

G_M000_IG61:                ;; offset=0x0618
            lsr     w2, w21, #2
            sub     w20, w2, #1
            ldr     w2, [x19, #0x48]
            asr     w0, w2, #31
            and     w0, w0, #3
            add     w2, w0, w2
            asr     w27, w2, #2
            mov     w4, wzr
            ldr     w3, [fp, #-0x94]
            cbnz    w3, G_M000_IG63

G_M000_IG62:                ;; offset=0x0640
            movn    w3, #0
            b       G_M000_IG70

G_M000_IG63:                ;; offset=0x0648
            ldr     x2, [x19, #0x18]
            mov     x0, x2
            ldr     w1, [x19, #0x40]
            lsl     w1, w1, #1
            ldr     w3, [x19, #0x50]
            mul     w1, w1, w3
            lsl     w3, w3, #1
            cbz     x0, G_M000_IG64
            mov     w4, w1
            add     x4, x4, w3, UXTW
            ldr     w6, [x0, #0x08]
            cmp     x4, x6
            bhi     G_M000_IG130
            add     x0, x0, #16
            ubfiz   x1, x1, #2, #32
            add     x1, x0, x1
            b       G_M000_IG65

G_M000_IG64:                ;; offset=0x068C
            orr     w0, w1, w3
            cbnz    w0, G_M000_IG130
            mov     x1, xzr
            mov     w3, wzr

G_M000_IG65:                ;; offset=0x069C
            cbz     x2, G_M000_IG66
            add     x0, x2, #16
            ldr     w2, [x2, #0x08]
            b       G_M000_IG67

G_M000_IG66:                ;; offset=0x06AC
            mov     x0, xzr
            mov     w2, wzr

G_M000_IG67:                ;; offset=0x06B4
            cmp     w3, w2
            bgt     G_M000_IG131
            ubfiz   x2, x3, #2, #32
            movz    x3, #0x56E0
            movk    x3, #0x6938 LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for System.SpanHelpers:Memmove(byref,byref,nuint)
            mov     w4, #2
            add     w1, w20, #1
            sub     w5, w27, #1
            tbnz    w5, #31, G_M000_IG135
            tbnz    w1, #31, G_M000_IG68
            cmp     w1, w5
            csel    w1, w1, w5, le
            b       G_M000_IG69

G_M000_IG68:                ;; offset=0x06F4
            mov     w1, wzr

G_M000_IG69:                ;; offset=0x06F8
            lsl     w3, w1, #2

G_M000_IG70:                ;; offset=0x06FC
            b       G_M000_IG84

G_M000_IG71:                ;; offset=0x0700
            ldr     x1, [fp, #-0xE0]
            cbnz    x1, G_M000_IG122

G_M000_IG72:                ;; offset=0x0708
            add     w1, w20, w4
            sub     w6, w27, #1
            tbnz    w6, #31, G_M000_IG136
            tbnz    w1, #31, G_M000_IG75

G_M000_IG73:                ;; offset=0x0718
            cmp     w1, w6
            bgt     G_M000_IG74
            b       G_M000_IG76

G_M000_IG74:                ;; offset=0x0724
            mov     w1, w6
            b       G_M000_IG76

G_M000_IG75:                ;; offset=0x072C
            mov     w1, wzr

G_M000_IG76:                ;; offset=0x0730
            lsl     w6, w1, #2
            str     w6, [fp, #-0xAC]
            cmp     w6, w3
            beq     G_M000_IG78

G_M000_IG77:                ;; offset=0x0740
            ldr     w5, [fp, #-0x98]
            sub     w1, w6, w5
            uxtb    w11, w22
            mov     w7, w11
            mov     w3, w7
            mov     x0, x19
            str     w4, [fp, #-0xA8]
            mov     w2, w4
            movz    x7, #0x4798
            movk    x7, #0x695F LSL #16
            movk    x7, #0x7FF8 LSL #32
            ldr     x7, [x7]
            blr     x7      // code for DotJpegli.Internal.StencilAdaptiveQuantization:ComputePreErosion(int,int,bool):this
            b       G_M000_IG83

G_M000_IG78:                ;; offset=0x0778
            ldr     x2, [x19, #0x18]
            mov     x0, x2
            sub     w1, w4, #1
            ldr     w3, [x19, #0x50]
            mul     w1, w1, w3
            mov     w7, w3
            cbz     x0, G_M000_IG79
            mov     w8, w1
            add     x8, x8, w7, UXTW
            ldr     w9, [x0, #0x08]
            cmp     x8, x9
            bhi     G_M000_IG130
            add     x0, x0, #16
            ubfiz   x1, x1, #2, #32
            add     x1, x0, x1
            b       G_M000_IG80

G_M000_IG79:                ;; offset=0x07B8
            orr     w0, w1, w7
            cbnz    w0, G_M000_IG130
            mov     x1, xzr
            mov     w7, wzr

G_M000_IG80:                ;; offset=0x07C8
            str     w4, [fp, #-0xA8]
            mul     w0, w4, w3
            cbz     x2, G_M000_IG81
            mov     w8, w0
            add     x8, x8, w3, UXTW
            ldr     w9, [x2, #0x08]
            cmp     x8, x9
            bhi     G_M000_IG130
            add     x2, x2, #16
            ubfiz   x0, x0, #2, #32
            add     x0, x2, x0
            b       G_M000_IG82

G_M000_IG81:                ;; offset=0x07F8
            orr     w2, w0, w3
            cbnz    w2, G_M000_IG130
            mov     x0, xzr
            mov     w3, wzr

G_M000_IG82:                ;; offset=0x0808
            cmp     w7, w3
            bgt     G_M000_IG131
            ubfiz   x2, x7, #2, #32
            movz    x3, #0x56E0
            movk    x3, #0x6938 LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for System.SpanHelpers:Memmove(byref,byref,nuint)

G_M000_IG83:                ;; offset=0x0828
            ldp     w3, w4, [fp, #-0xAC]
            add     w4, w4, #1

G_M000_IG84:                ;; offset=0x0830
            ldr     w1, [x19, #0x40]
            lsl     w1, w1, #1
            add     w1, w1, #2
            cmp     w4, w1
            blt     G_M000_IG71

G_M000_IG85:                ;; offset=0x0844
            cmp     w24, #9
            bge     G_M000_IG87

G_M000_IG86:                ;; offset=0x084C
            fmov    s16, #1.0000
            b       G_M000_IG88

G_M000_IG87:                ;; offset=0x0854
            scvtf   s16, w24
            fmov    s17, #9.0000
            fsub    s16, s16, s17
            ldr     s17, [@RWD00]
            fdiv    s16, s16, s17
            fmov    s17, #1.0000
            fsub    s16, s17, s16
            movi    v17.2s, #0
            fmax    s16, s16, s17

G_M000_IG88:                ;; offset=0x0878
            ldr     s17, [@RWD04]
            fmul    s8, s16, s17
            fmov    s17, #1.0000
            fsub    s16, s17, s16
            ldr     s17, [@RWD08]
            fmul    s9, s16, s17
            uxtb    w7, w22
            mov     w20, w7
            cbz     w20, G_M000_IG90

G_M000_IG89:                ;; offset=0x089C
            ldr     w0, [x19, #0x5C]
            cmp     w0, #8
            bge     G_M000_IG91

G_M000_IG90:                ;; offset=0x08A8
            mov     w24, wzr
            b       G_M000_IG92

G_M000_IG91:                ;; offset=0x08B0
            ldrb    w24, [fp, #0x10]

G_M000_IG92:                ;; offset=0x08B4
            cbnz    w24, G_M000_IG94

G_M000_IG93:                ;; offset=0x08B8
            mov     x27, xzr
            mov     w2, wzr
            mov     w4, w2
            b       G_M000_IG95

G_M000_IG94:                ;; offset=0x08C8
            add     sp, sp, #16
            ldp     x0, xzr, [sp], #-0x200
            sub     sp, sp, #16
            add     x0, sp, #16
            str     x0, [fp, #-0xE8]
            mov     x1, #512
            bl      CORINFO_HELP_MEMZERO
            ldr     x0, [fp, #-0xE8]
            mov     x27, x0
            mov     w2, #16
            mov     w4, w2

G_M000_IG95:                ;; offset=0x08F4
            str     w4, [fp, #-0xCC]
            cbnz    w24, G_M000_IG97

G_M000_IG96:                ;; offset=0x08FC
            mov     x5, xzr
            mov     w6, wzr
            b       G_M000_IG98

G_M000_IG97:                ;; offset=0x0908
            add     sp, sp, #16
            ldr     wzr, [sp], #-0x40
            sub     sp, sp, #16
            add     x5, sp, #16
            movi    v16.16b, #0
            stp     q16, q16, [x5]
            stp     q16, q16, [x5, #0x20]
            mov     w6, #16

G_M000_IG98:                ;; offset=0x0928
            str     x5, [fp, #-0xF0]
            str     w6, [fp, #-0xD0]
            mov     w7, wzr
            ldr     s10, [@RWD12]
            ldr     s11, [@RWD16]
            ldr     s12, [@RWD20]
            b       G_M000_IG119

G_M000_IG99:                ;; offset=0x0944
            ldr     x1, [fp, #-0xE0]
            cbnz    x1, G_M000_IG108

G_M000_IG100:                ;; offset=0x094C
            str     w7, [fp, #-0xB0]
            lsl     w1, w7, #1
            add     w1, w1, #1
            mov     w3, w20
            mov     x0, x19
            mov     w2, wzr
            movz    x8, #0x47B0
            movk    x8, #0x695F LSL #16
            movk    x8, #0x7FF8 LSL #32
            ldr     x8, [x8]
            blr     x8      // code for DotJpegli.Internal.StencilAdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this
            ldr     w4, [fp, #-0xB0]
            lsl     w1, w4, #1
            add     w1, w1, #2
            ldr     w2, [x19, #0x50]
            mov     w3, w20
            mov     x0, x19
            movz    x5, #0x47B0
            movk    x5, #0x695F LSL #16
            movk    x5, #0x7FF8 LSL #32
            ldr     x5, [x5]
            blr     x5      // code for DotJpegli.Internal.StencilAdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this
            mov     w7, wzr
            cbz     w24, G_M000_IG116
            b       G_M000_IG115

G_M000_IG101:                ;; offset=0x09B0
            ldr     w3, [fp, #-0xB0]
            lsl     w0, w3, #3
            add     w0, w0, #5
            ldr     w1, [x19, #0x4C]
            ldr     w4, [fp, #-0xB4]
            lsl     w2, w4, #3
            madd    w0, w0, w1, w2
            add     w0, w0, #1
            ldr     x1, [x19, #0x10]
            cbz     x1, G_M000_IG102
            ldr     w2, [x1, #0x08]
            cmp     w2, w0
            blo     G_M000_IG130
            add     x2, x1, #16
            ubfiz   x5, x0, #2, #32
            add     x2, x2, x5
            ldr     w1, [x1, #0x08]
            sub     w1, w1, w0
            b       G_M000_IG103

G_M000_IG102:                ;; offset=0x09FC
            cbnz    w0, G_M000_IG130
            mov     x2, xzr
            mov     w1, wzr

G_M000_IG103:                ;; offset=0x0A08
            mov     x0, x2
            ldr     w2, [x19, #0x4C]
            movz    x5, #0x4840
            movk    x5, #0x695F LSL #16
            movk    x5, #0x7FF8 LSL #32
            ldr     x5, [x5]
            blr     x5      // code for DotJpegli.Internal.StencilAdaptiveQuantization:ModulateScalar(System.ReadOnlySpan`1[float],int,float):float

G_M000_IG104:                ;; offset=0x0A24
            fmul    s0, s0, s11
            movz    x0, #0x4810
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for DotJpegli.Internal.StencilAdaptiveQuantization:FastPow2(float):float
            fmul    s16, s0, s8
            fadd    s16, s16, s9
            ldr     w0, [x19, #0x5C]
            ldp     w4, w3, [fp, #-0xB4]
            madd    w0, w3, w0, w4
            cmp     w0, w26
            bhs     G_M000_IG138
            fdiv    s16, s12, s16
            fmov    s17, #1.0000
            fsub    s16, s16, s17
            movi    v17.2s, #0
            fmax    s16, s16, s17
            str     s16, [x25, w0, UXTW #2]
            add     w4, w4, #1
            ldr     w0, [x19, #0x5C]
            cmp     w4, w0
            mov     w7, w4
            bge     G_M000_IG118

G_M000_IG105:                ;; offset=0x0A84
            str     w3, [fp, #-0xB0]

G_M000_IG106:                ;; offset=0x0A88
            str     w7, [fp, #-0xB4]
            lsl     w0, w7, #1
            ldr     x1, [x19, #0x20]
            ldr     w3, [x19, #0x50]
            add     w3, w3, w0
            ldr     w4, [x1, #0x08]
            cmp     w3, w4
            bhs     G_M000_IG138
            add     x1, x1, #16
            ldr     s0, [x1, w3, UXTW #2]
            ldr     x1, [x19, #0x20]
            ldr     w3, [x19, #0x50]
            add     w3, w3, w0
            add     w3, w3, #1
            ldr     w4, [x1, #0x08]
            cmp     w3, w4
            bhs     G_M000_IG138
            add     x1, x1, #16
            ldr     s16, [x1, w3, UXTW #2]
            fadd    s0, s0, s16
            ldr     x1, [x19, #0x20]
            ldr     w3, [x1, #0x08]
            cmp     w0, w3
            bhs     G_M000_IG138
            add     x1, x1, #16
            ldr     s16, [x1, w0, UXTW #2]
            fadd    s0, s0, s16
            ldr     x1, [x19, #0x20]
            add     w3, w0, #1
            ldr     w4, [x1, #0x08]
            cmp     w3, w4
            bhs     G_M000_IG138
            add     x1, x1, #16
            add     w0, w0, #1
            ldr     s16, [x1, w0, UXTW #2]
            fadd    s0, s0, s16
            movz    x0, #0x47E0
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for DotJpegli.Internal.StencilAdaptiveQuantization:ComputeMask(float):float
            cbz     w20, G_M000_IG101

G_M000_IG107:                ;; offset=0x0B30
            ldr     w3, [fp, #-0xB0]
            lsl     w2, w3, #3
            add     w2, w2, #5
            ldr     w4, [fp, #-0xB4]
            lsl     w1, w4, #3
            mov     x0, x19
            movz    x5, #0x4828
            movk    x5, #0x695F LSL #16
            movk    x5, #0x7FF8 LSL #32
            ldr     x5, [x5]
            blr     x5      // code for DotJpegli.Internal.StencilAdaptiveQuantization:ModulateVector(int,int,float):float:this
            b       G_M000_IG104

G_M000_IG108:                ;; offset=0x0B60
            ldr     x1, [fp, #-0xE0]
            add     x1, x1, #32
            ldapr   w1, [x1]
            cbz     w1, G_M000_IG100
            b       G_M000_IG127

G_M000_IG109:                ;; offset=0x0B74
            ldr     w8, [fp, #-0xB0]
            lsl     w2, w8, #3
            add     w2, w2, #5
            ldr     x5, [fp, #-0xF0]
            ldr     w6, [fp, #-0xD0]
            mov     x3, x27
            ldr     w4, [fp, #-0xCC]
            str     w7, [fp, #-0xB4]
            lsl     w1, w7, #3
            mov     x0, x19
            movz    x9, #0x47C8
            movk    x9, #0x695F LSL #16
            movk    x9, #0x7FF8 LSL #32
            ldr     x9, [x9]
            blr     x9      // code for DotJpegli.Internal.StencilAdaptiveQuantization:ModulateEightBlocks(int,int,System.Span`1[System.Runtime.Intrinsics.Vector256`1[float]],System.Span`1[float]):this
            mov     w0, wzr
            ldr     w1, [fp, #-0xD0]
            cmp     w1, #8
            blt     G_M000_IG113

G_M000_IG110:                ;; offset=0x0BC0
            ldr     w2, [fp, #-0xB4]
            str     w0, [fp, #-0xB8]
            add     w3, w2, w0
            lsl     w3, w3, #1
            ldr     x4, [x19, #0x20]
            ldr     w5, [x19, #0x50]
            add     w5, w5, w3
            mov     w6, w5
            ldr     w7, [x4, #0x08]
            cmp     w6, w7
            bhs     G_M000_IG138
            add     x4, x4, #16
            ldr     s0, [x4, w6, UXTW #2]
            add     w5, w5, #1
            cmp     w5, w7
            bhs     G_M000_IG138
            ldr     s16, [x4, w5, UXTW #2]
            fadd    s0, s0, s16
            cmp     w3, w7
            bhs     G_M000_IG138
            ldr     s16, [x4, w3, UXTW #2]
            fadd    s0, s0, s16
            add     w3, w3, #1
            cmp     w3, w7
            bhs     G_M000_IG138
            ldr     s16, [x4, w3, UXTW #2]
            fadd    s13, s0, s16
            fmov    s0, s13
            movz    x3, #0x47E0
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.StencilAdaptiveQuantization:ComputeMask(float):float
            fmov    s13, s0
            ldr     x0, [fp, #-0xF0]
            ldr     w1, [fp, #-0xB8]
            ldr     s0, [x0, w1, UXTW #2]
            fadd    s13, s0, s13
            str     w1, [fp, #-0xB8]
            add     w2, w1, #8
            ldr     w3, [fp, #-0xD0]
            cmp     w2, w3
            bhs     G_M000_IG138
            ldr     s0, [x0, w2, UXTW #2]
            movz    x2, #0x47F8
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.StencilAdaptiveQuantization:FastLog2(float):float
            fmul    s0, s0, s10
            fadd    s13, s0, s13
            fmul    s0, s13, s11
            movz    x0, #0x4810
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for DotJpegli.Internal.StencilAdaptiveQuantization:FastPow2(float):float
            fmul    s0, s0, s8
            fadd    s14, s0, s9
            ldr     w0, [x19, #0x5C]
            ldp     w1, w2, [fp, #-0xB4]
            madd    w0, w2, w0, w1
            ldr     w3, [fp, #-0xB8]
            add     w0, w0, w3
            cmp     w0, w26
            bhs     G_M000_IG138
            fdiv    s0, s12, s14
            fmov    s16, #1.0000
            fsub    s0, s0, s16
            movi    v16.2s, #0
            fmax    s0, s0, s16
            str     s0, [x25, w0, UXTW #2]
            add     w3, w3, #1
            cmp     w3, #8
            mov     w0, w3
            bge     G_M000_IG114

G_M000_IG111:                ;; offset=0x0CF0
            str     w1, [fp, #-0xB4]
            b       G_M000_IG110

G_M000_IG112:                ;; offset=0x0CF8
            str     w1, [fp, #-0xB4]

G_M000_IG113:                ;; offset=0x0CFC
            ldr     w3, [fp, #-0xB4]
            str     w0, [fp, #-0xB8]
            add     w4, w3, w0
            lsl     w4, w4, #1
            ldr     x5, [x19, #0x20]
            mov     x6, x5
            ldr     w7, [x19, #0x50]
            add     w7, w7, w4
            mov     w8, w7
            ldr     w9, [x6, #0x08]
            cmp     w8, w9
            bhs     G_M000_IG138
            add     x6, x6, #16
            ldr     s0, [x6, w8, UXTW #2]
            mov     x6, x5
            add     w7, w7, #1
            cmp     w7, w9
            bhs     G_M000_IG138
            add     x6, x6, #16
            ldr     s16, [x6, w7, UXTW #2]
            fadd    s0, s0, s16
            mov     x6, x5
            cmp     w4, w9
            bhs     G_M000_IG138
            add     x6, x6, #16
            ldr     s16, [x6, w4, UXTW #2]
            fadd    s0, s0, s16
            add     w6, w4, #1
            cmp     w6, w9
            bhs     G_M000_IG138
            add     x5, x5, #16
            add     w4, w4, #1
            ldr     s16, [x5, w4, UXTW #2]
            fadd    s13, s0, s16
            fmov    s0, s13
            movz    x4, #0x47E0
            movk    x4, #0x695F LSL #16
            movk    x4, #0x7FF8 LSL #32
            ldr     x4, [x4]
            blr     x4      // code for DotJpegli.Internal.StencilAdaptiveQuantization:ComputeMask(float):float
            fmov    s13, s0
            ldr     w0, [fp, #-0xB8]
            ldr     w1, [fp, #-0xD0]
            cmp     w0, w1
            bhs     G_M000_IG138
            ldr     x2, [fp, #-0xF0]
            ldr     s0, [x2, w0, UXTW #2]
            fadd    s13, s0, s13
            add     w3, w0, #8
            cmp     w3, w1
            bhs     G_M000_IG138
            str     w0, [fp, #-0xB8]
            add     w3, w0, #8
            ldr     s0, [x2, w3, UXTW #2]
            movz    x3, #0x47F8
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.StencilAdaptiveQuantization:FastLog2(float):float
            fmul    s0, s0, s10
            fadd    s13, s0, s13
            fmul    s0, s13, s11
            movz    x0, #0x4810
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for DotJpegli.Internal.StencilAdaptiveQuantization:FastPow2(float):float
            fmul    s0, s0, s8
            fadd    s14, s0, s9
            ldr     w0, [x19, #0x5C]
            ldp     w1, w2, [fp, #-0xB4]
            madd    w0, w2, w0, w1
            ldr     w3, [fp, #-0xB8]
            add     w0, w0, w3
            cmp     w0, w26
            bhs     G_M000_IG138
            fdiv    s0, s12, s14
            fmov    s16, #1.0000
            fsub    s0, s0, s16
            movi    v16.2s, #0
            fmax    s0, s0, s16
            str     s0, [x25, w0, UXTW #2]
            add     w3, w3, #1
            cmp     w3, #8
            mov     w0, w3
            blt     G_M000_IG112

G_M000_IG114:                ;; offset=0x0E54
            add     w1, w1, #8
            mov     w7, w1

G_M000_IG115:                ;; offset=0x0E5C
            ldr     w2, [x19, #0x5C]
            sub     w2, w2, #8
            cmp     w7, w2
            ble     G_M000_IG109

G_M000_IG116:                ;; offset=0x0E6C
            ldr     w0, [x19, #0x5C]
            cmp     w7, w0
            blt     G_M000_IG106

G_M000_IG117:                ;; offset=0x0E78
            ldr     w3, [fp, #-0xB0]

G_M000_IG118:                ;; offset=0x0E7C
            add     w3, w3, #1
            mov     w7, w3

G_M000_IG119:                ;; offset=0x0E84
            ldr     w1, [x19, #0x40]
            cmp     w7, w1
            blt     G_M000_IG99

G_M000_IG120:                ;; offset=0x0E90
            add     x14, x19, #48
            mov     x15, x28
            bl      CORINFO_HELP_ASSIGN_REF
            stp     w21, w23, [x19, #0x54]
            strb    w22, [x19, #0x62]
            movz    xip0, #0x2D5A
            movk    xip0, #0xF7B0 LSL #16
            movk    xip0, #0x2883 LSL #32
            ldr     xip1, [fp, #-0x90]
            cmp     xip0, xip1
            beq     G_M000_IG121
            bl      CORINFO_HELP_FAIL_FAST

G_M000_IG121:                ;; offset=0x0EC0
            sub     sp, fp, #272
            ldp     fp, lr, [sp, #0x110]
            ldp     x27, x28, [sp, #0x100]
            ldp     x25, x26, [sp, #0xF0]
            ldp     x23, x24, [sp, #0xE0]
            ldp     x21, x22, [sp, #0xD0]
            ldp     x19, x20, [sp, #0xC0]
            ldr     d14, [sp, #0xB8]
            ldp     d12, d13, [sp, #0xA8]
            ldp     d10, d11, [sp, #0x98]
            ldp     d8, d9, [sp, #0x88]
            add     sp, sp, #288
            ret     lr

G_M000_IG122:                ;; offset=0x0EF4
            ldr     x1, [fp, #-0xE0]
            add     x1, x1, #32
            ldapr   w1, [x1]
            cbz     w1, G_M000_IG72
            b       G_M000_IG127

G_M000_IG123:                ;; offset=0x0F08
            ldr     x2, [fp, #-0xE0]
            add     x2, x2, #32
            ldapr   w2, [x2]
            cbz     w2, G_M000_IG07
            b       G_M000_IG127

G_M000_IG124:                ;; offset=0x0F1C
            movn    w7, #0
            b       G_M000_IG14

G_M000_IG125:                ;; offset=0x0F24
            mov     w6, w4
            add     x6, x6, w1, UXTW
            ldr     w7, [x0, #0x08]
            cmp     x6, x7
            bhi     G_M000_IG130
            add     x0, x0, #16
            ubfiz   x4, x4, #2, #32
            add     x4, x0, x4
            b       G_M000_IG09

G_M000_IG126:                ;; offset=0x0F48
            mov     x0, x19
            movz    x1, #0x4408
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for System.ThrowHelper:ThrowObjectDisposedException(System.Object)
            brk     #0

G_M000_IG127:                ;; offset=0x0F64
            sub     x0, fp, #224
            movz    x1, #0x4420
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for System.Threading.CancellationToken:ThrowOperationCanceledException():this
            brk     #0

G_M000_IG128:                ;; offset=0x0F80
            movz    x0, #0xDCE0
            movk    x0, #0x6947 LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x62C8
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF00
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:.ctor(System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG129:                ;; offset=0x0FC0
            movz    x0, #0xF360
            movk    x0, #0x693D LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x6208
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0x62A0
            movk    x2, #0xA15 LSL #16
            movk    x2, #548 LSL #32
            movz    x3, #0x4438
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for System.ArgumentException:.ctor(System.String,System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG130:                ;; offset=0x100C
            movz    x0, #0x4450
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentOutOfRangeException()
            brk     #0

G_M000_IG131:                ;; offset=0x1024
            movz    x0, #0x4468
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentException_DestinationTooShort()
            brk     #0

G_M000_IG132:                ;; offset=0x103C
            mov     w1, w5
            mov     w0, wzr
            movz    x2, #0x42D0
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.Math:ThrowMinMaxException[int](int,int)
            brk     #0

G_M000_IG133:                ;; offset=0x105C
            mov     w1, w8
            mov     w0, wzr
            movz    x2, #0x42D0
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.Math:ThrowMinMaxException[int](int,int)
            brk     #0

G_M000_IG134:                ;; offset=0x107C
            mov     w0, wzr
            movz    x2, #0x42D0
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.Math:ThrowMinMaxException[int](int,int)
            brk     #0

G_M000_IG135:                ;; offset=0x1098
            mov     w1, w5
            mov     w0, wzr
            movz    x2, #0x42D0
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.Math:ThrowMinMaxException[int](int,int)
            brk     #0

G_M000_IG136:                ;; offset=0x10B8
            mov     w1, w6
            mov     w0, wzr
            movz    x2, #0x42D0
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.Math:ThrowMinMaxException[int](int,int)
            brk     #0

G_M000_IG137:                ;; offset=0x10D8
            bl      CORINFO_HELP_THROWDIVZERO
            brk     #0

G_M000_IG138:                ;; offset=0x10E0
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dd	42600000h		;        56
RWD04  	dd	3F574BC7h		;     0.841
RWD08  	dd	3ECEAF25h		;   0.40368
RWD12  	dd	BDDC6A08h		; -0.107624
RWD16  	dd	3FB8AA3Bh		;    1.4427
RWD20  	dd	3F19999Ah		;       0.6

; Total bytes of code 4328

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ComputePreErosion(int,int,bool):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 52 single block inlinees; 29 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x70]!
            stp     d8, d9, [sp, #0x18]
            stp     d10, d11, [sp, #0x28]
            stp     d12, d13, [sp, #0x38]
            str     d14, [sp, #0x48]
            stp     x19, x20, [sp, #0x50]
            stp     x21, x22, [sp, #0x60]
            mov     fp, sp
            mov     x19, x0

G_M000_IG02:                ;; offset=0x0024
            ldr     s8, [@RWD00]
            ldp     w0, w4, [x19, #0x4C]
            mul     w0, w1, w0
            add     w20, w0, #1
            mul     w21, w2, w4
            mov     w22, wzr
            tst     w3, #255
            beq     G_M000_IG06

G_M000_IG03:                ;; offset=0x0044
            ldr     w0, [x19, #0x44]
            sub     w0, w0, #4
            tbnz    w0, #31, G_M000_IG06

G_M000_IG04:                ;; offset=0x0050
            add     w0, w20, w22
            ldr     x1, [x19, #0x10]
            ldr     w2, [x19, #0x4C]
            sub     w3, w0, w2
            tbnz    w3, #31, G_M000_IG18
            ldr     w4, [x1, #0x08]
            sub     w5, w4, w3
            cmp     w5, #4
            blt     G_M000_IG18
            cmp     w3, w4
            bhs     G_M000_IG19
            add     x1, x1, #16
            ubfiz   x3, x3, #2, #32
            ldr     q0, [x1, x3]
            sub     w3, w4, w0
            cmp     w3, #4
            ccmp    w0, #0, nc, ge
            blt     G_M000_IG18
            cmp     w0, w4
            bhs     G_M000_IG19
            ubfiz   x3, x0, #2, #32
            ldr     q9, [x1, x3]
            add     w3, w0, w2
            mov     w5, w3
            sub     w6, w4, w5
            cmp     w6, #4
            ccmp    w5, #0, nc, ge
            blt     G_M000_IG18
            cmp     w5, w4
            bhs     G_M000_IG19
            ubfiz   x5, x5, #2, #32
            ldr     q16, [x1, x5]
            sub     w5, w0, #1
            sub     w6, w4, w5
            cmp     w6, #4
            ccmp    w5, #0, nc, ge
            blt     G_M000_IG18
            cmp     w5, w4
            bhs     G_M000_IG19
            ubfiz   x5, x5, #2, #32
            ldr     q17, [x1, x5]
            add     w5, w0, #1
            sub     w6, w4, w5
            cmp     w6, #4
            blt     G_M000_IG18
            cmp     w5, w4
            bhs     G_M000_IG19
            ubfiz   x5, x5, #2, #32
            ldr     q18, [x1, x5]
            fadd    v17.4s, v18.4s, v17.4s
            fadd    v0.4s, v0.4s, v16.4s
            fadd    v0.4s, v17.4s, v0.4s
            ldr     q17, [@RWD04]
            fmul    v0.4s, v0.4s, v17.4s
            ldr     q17, [@RWD20]
            fadd    v18.4s, v9.4s, v17.4s
            movi    v19.4s, #0
            fmax    v18.4s, v18.4s, v19.4s
            fmul    v19.4s, v18.4s, v18.4s
            ldr     q20, [@RWD36]
            fmul    v18.4s, v18.4s, v20.4s
            fmul    v18.4s, v18.4s, v19.4s
            ldr     q21, [@RWD52]
            fadd    v18.4s, v18.4s, v21.4s
            ldr     q22, [@RWD68]
            fmul    v19.4s, v19.4s, v22.4s
            ldr     q23, [@RWD84]
            fadd    v19.4s, v19.4s, v23.4s
            fdiv    v18.4s, v18.4s, v19.4s
            fsub    v0.4s, v9.4s, v0.4s
            fmul    v0.4s, v18.4s, v0.4s
            fmul    v0.4s, v0.4s, v0.4s
            ldr     q18, [@RWD100]
            fmin    v0.4s, v0.4s, v18.4s
            ldr     q18, [@RWD116]
            fmul    v0.4s, v0.4s, v18.4s
            ldr     q18, [@RWD132]
            fadd    v0.4s, v0.4s, v18.4s
            fsqrt   v0.4s, v0.4s
            ldr     q18, [@RWD148]
            fmul    v10.4s, v0.4s, v18.4s
            mov     v0.16b, v9.16b
            mov     v9.16b, v16.16b
            add     w5, w0, w2,  LSL #1
            mov     w6, w5
            sub     w7, w4, w6
            cmp     w7, #4
            ccmp    w6, #0, nc, ge
            blt     G_M000_IG18
            cmp     w6, w4
            bhs     G_M000_IG19
            ubfiz   x6, x6, #2, #32
            ldr     q16, [x1, x6]
            sub     w6, w3, #1
            sub     w7, w4, w6
            cmp     w7, #4
            ccmp    w6, #0, nc, ge
            blt     G_M000_IG18
            cmp     w6, w4
            bhs     G_M000_IG19
            ubfiz   x6, x6, #2, #32
            ldr     q18, [x1, x6]
            add     w3, w3, #1
            sub     w6, w4, w3
            cmp     w6, #4
            ccmp    w3, #0, nc, ge
            blt     G_M000_IG18
            cmp     w3, w4
            bhs     G_M000_IG19
            ubfiz   x3, x3, #2, #32
            ldr     q19, [x1, x3]
            fadd    v18.4s, v19.4s, v18.4s
            fadd    v0.4s, v0.4s, v16.4s
            fadd    v0.4s, v18.4s, v0.4s
            ldr     q18, [@RWD164]
            fmul    v0.4s, v0.4s, v18.4s
            fadd    v18.4s, v9.4s, v17.4s
            movi    v19.4s, #0
            fmax    v18.4s, v18.4s, v19.4s
            fmul    v19.4s, v18.4s, v18.4s
            fmul    v18.4s, v18.4s, v20.4s
            fmul    v18.4s, v18.4s, v19.4s
            fadd    v18.4s, v18.4s, v21.4s
            fmul    v19.4s, v19.4s, v22.4s
            fadd    v19.4s, v19.4s, v23.4s
            fdiv    v18.4s, v18.4s, v19.4s
            fsub    v0.4s, v9.4s, v0.4s
            fmul    v0.4s, v18.4s, v0.4s
            fmul    v0.4s, v0.4s, v0.4s
            ldr     q18, [@RWD180]
            fmin    v0.4s, v0.4s, v18.4s
            ldr     q18, [@RWD196]
            fmul    v0.4s, v0.4s, v18.4s
            ldr     q18, [@RWD212]
            fadd    v0.4s, v0.4s, v18.4s
            fsqrt   v0.4s, v0.4s
            ldr     q18, [@RWD228]
            fmul    v0.4s, v0.4s, v18.4s
            fadd    v10.4s, v10.4s, v0.4s
            mov     v0.16b, v9.16b
            mov     v9.16b, v16.16b
            mov     w3, #3
            madd    w3, w2, w3, w0
            mov     w6, w3
            sub     w7, w4, w6
            cmp     w7, #4
            ccmp    w6, #0, nc, ge
            blt     G_M000_IG18
            cmp     w6, w4
            bhs     G_M000_IG19
            ubfiz   x6, x6, #2, #32
            ldr     q16, [x1, x6]
            sub     w6, w5, #1
            sub     w7, w4, w6
            cmp     w7, #4
            ccmp    w6, #0, nc, ge
            blt     G_M000_IG18
            cmp     w6, w4

G_M000_IG05:                ;; offset=0x02C8
            bhs     G_M000_IG19
            ubfiz   x6, x6, #2, #32
            ldr     q18, [x1, x6]
            add     w5, w5, #1
            sub     w6, w4, w5
            cmp     w6, #4
            ccmp    w5, #0, nc, ge
            blt     G_M000_IG18
            cmp     w5, w4
            bhs     G_M000_IG19
            ubfiz   x5, x5, #2, #32
            ldr     q19, [x1, x5]
            fadd    v18.4s, v19.4s, v18.4s
            fadd    v0.4s, v0.4s, v16.4s
            fadd    v0.4s, v18.4s, v0.4s
            ldr     q18, [@RWD244]
            fmul    v0.4s, v0.4s, v18.4s
            fadd    v18.4s, v9.4s, v17.4s
            movi    v19.4s, #0
            fmax    v18.4s, v18.4s, v19.4s
            fmul    v19.4s, v18.4s, v18.4s
            fmul    v18.4s, v18.4s, v20.4s
            fmul    v18.4s, v18.4s, v19.4s
            fadd    v18.4s, v18.4s, v21.4s
            fmul    v19.4s, v19.4s, v22.4s
            fadd    v19.4s, v19.4s, v23.4s
            fdiv    v18.4s, v18.4s, v19.4s
            fsub    v0.4s, v9.4s, v0.4s
            fmul    v0.4s, v18.4s, v0.4s
            fmul    v0.4s, v0.4s, v0.4s
            ldr     q18, [@RWD260]
            fmin    v0.4s, v0.4s, v18.4s
            ldr     q18, [@RWD276]
            fmul    v0.4s, v0.4s, v18.4s
            ldr     q18, [@RWD292]
            fadd    v0.4s, v0.4s, v18.4s
            fsqrt   v0.4s, v0.4s
            ldr     q18, [@RWD308]
            fmul    v0.4s, v0.4s, v18.4s
            fadd    v10.4s, v10.4s, v0.4s
            mov     v0.16b, v9.16b
            mov     v9.16b, v16.16b
            add     w0, w0, w2,  LSL #2
            sub     w2, w4, w0
            cmp     w2, #4
            ccmp    w0, #0, nc, ge
            blt     G_M000_IG18
            cmp     w0, w4
            bhs     G_M000_IG19
            ubfiz   x0, x0, #2, #32
            ldr     q16, [x1, x0]
            sub     w0, w3, #1
            sub     w2, w4, w0
            cmp     w2, #4
            ccmp    w0, #0, nc, ge
            blt     G_M000_IG18
            cmp     w0, w4
            bhs     G_M000_IG19
            ubfiz   x0, x0, #2, #32
            ldr     q18, [x1, x0]
            add     w0, w3, #1
            sub     w2, w4, w0
            cmp     w2, #4
            ccmp    w0, #0, nc, ge
            blt     G_M000_IG18
            cmp     w0, w4
            bhs     G_M000_IG19
            ubfiz   x0, x0, #2, #32
            ldr     q19, [x1, x0]
            fadd    v18.4s, v19.4s, v18.4s
            fadd    v0.4s, v0.4s, v16.4s
            fadd    v0.4s, v18.4s, v0.4s
            ldr     q16, [@RWD324]
            fmul    v11.4s, v0.4s, v16.4s
            fadd    v0.4s, v9.4s, v17.4s
            mov     w0, wzr
            movz    x1, #0x4888
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            mov     v12.d[0], v9.d[1]
            mov     v13.d[0], v10.d[1]
            mov     v14.d[0], v11.d[1]
            blr     x1      // code for DotJpegli.Internal.StencilAdaptiveQuantization:DerivativeRatio(System.Numerics.Vector`1[float],bool):System.Numerics.Vector`1[float]
            mov     v9.d[1], v12.d[0]
            mov     v11.d[1], v14.d[0]
            fsub    v16.4s, v9.4s, v11.4s
            fmul    v16.4s, v0.4s, v16.4s
            fmul    v16.4s, v16.4s, v16.4s
            ldr     q17, [@RWD340]
            fmin    v16.4s, v16.4s, v17.4s
            ldr     q17, [@RWD356]
            fmul    v16.4s, v16.4s, v17.4s
            ldr     q17, [@RWD372]
            fadd    v16.4s, v16.4s, v17.4s
            fsqrt   v16.4s, v16.4s
            ldr     q17, [@RWD388]
            fmul    v16.4s, v16.4s, v17.4s
            mov     v10.d[1], v13.d[0]
            fadd    v10.4s, v10.4s, v16.4s
            ldr     x0, [x19, #0x18]
            dup     s16, v10.s[0]
            dup     s17, v10.s[1]
            fadd    s16, s17, s16
            dup     s17, v10.s[2]
            fadd    s16, s17, s16
            dup     s17, v10.s[3]
            fadd    s16, s17, s16
            fmov    s17, #0.2500
            fmul    s16, s16, s17
            asr     w1, w22, #31
            and     w1, w1, #3
            add     w1, w1, w22
            add     w1, w21, w1,  ASR #2
            ldr     w2, [x0, #0x08]
            cmp     w1, w2
            bhs     G_M000_IG19
            add     x0, x0, #16
            str     s16, [x0, w1, UXTW #2]
            add     w22, w22, #4
            ldr     w0, [x19, #0x44]
            sub     w0, w0, #4
            cmp     w22, w0
            ble     G_M000_IG04

G_M000_IG06:                ;; offset=0x04B8
            ldr     w0, [x19, #0x44]
            ldr     s16, [@RWD20]
            ldr     s17, [@RWD36]
            ldr     s18, [@RWD52]
            ldr     s19, [@RWD68]
            ldr     s20, [@RWD84]
            ldr     d21, [@RWD404]
            b       G_M000_IG16
            align   [0 bytes for IG08]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG07:                ;; offset=0x04D8
            add     w1, w20, w22
            ldr     x2, [x19, #0x10]
            ldr     w3, [x2, #0x08]
            cmp     w1, w3
            bhs     G_M000_IG19
            add     x2, x2, #16
            ldr     s22, [x2, w1, UXTW #2]
            sub     w4, w1, #1
            cmp     w4, w3
            bhs     G_M000_IG19
            sub     w4, w1, #1
            ldr     s23, [x2, w4, UXTW #2]
            add     w4, w1, #1
            cmp     w4, w3
            bhs     G_M000_IG19
            ldr     s24, [x2, w4, UXTW #2]
            fadd    s23, s24, s23
            ldr     w5, [x19, #0x4C]
            sub     w6, w1, w5
            cmp     w6, w3
            bhs     G_M000_IG19
            ldr     s25, [x2, w6, UXTW #2]
            add     w6, w1, w5
            cmp     w6, w3
            bhs     G_M000_IG19
            ldr     s26, [x2, w6, UXTW #2]
            fadd    s25, s25, s26
            fadd    s23, s23, s25
            fmov    s25, #0.2500
            fmul    s23, s23, s25
            fadd    s25, s22, s16
            movi    v26.2s, #0
            fmax    s25, s25, s26
            fmul    s26, s25, s25
            fmul    s25, s25, s17
            fmul    s25, s25, s26
            fadd    s25, s25, s18
            fmul    s26, s26, s19
            fadd    s26, s26, s20
            fdiv    s25, s25, s26
            fsub    s22, s22, s23
            fmul    s22, s25, s22
            fmul    s22, s22, s22
            fmin    s22, s22, s21
            ldr     s23, [@RWD00]
            fmul    s22, s22, s23
            fmov    s23, #28.0000
            fadd    s22, s22, s23
            fsqrt   s22, s22
            fmov    s23, #0.2500
            fmul    s22, s22, s23
            mov     w6, #1

G_M000_IG08:                ;; offset=0x05A8
            ldr     w7, [x19, #0x4C]
            madd    w7, w6, w7, w1
            cmp     w7, w3
            bhs     G_M000_IG19
            ldr     s23, [x2, w7, UXTW #2]
            sub     w8, w7, #1
            cmp     w8, w3
            bhs     G_M000_IG19
            ldr     s25, [x2, w8, UXTW #2]
            add     w8, w7, #1
            cmp     w8, w3
            bhs     G_M000_IG19
            ldr     s26, [x2, w8, UXTW #2]
            fadd    s25, s26, s25
            sub     w8, w7, w5
            cmp     w8, w3
            bhs     G_M000_IG19
            ldr     s26, [x2, w8, UXTW #2]
            add     w7, w7, w5
            cmp     w7, w3
            bhs     G_M000_IG19
            ldr     s27, [x2, w7, UXTW #2]
            fadd    s26, s26, s27
            fadd    s25, s25, s26
            fmov    s26, #0.2500
            fmul    s25, s25, s26
            fadd    s26, s23, s16
            movi    v27.2s, #0
            fmax    s26, s26, s27
            fmul    s27, s26, s26
            fmul    s26, s26, s17
            fmul    s26, s26, s27
            fadd    s26, s26, s18
            fmul    s27, s27, s19
            fadd    s27, s27, s20
            fdiv    s26, s26, s27
            fsub    s23, s23, s25
            fmul    s23, s26, s23
            fmul    s23, s23, s23
            fmin    s23, s23, s21
            fmul    s23, s23, s8
            fmov    s25, #28.0000
            fadd    s23, s23, s25
            fsqrt   s23, s23
            fmov    s25, #0.2500
            fmul    s23, s23, s25
            fadd    s22, s23, s22
            add     w6, w6, #1
            cmp     w6, #4
            blt     G_M000_IG08

G_M000_IG09:                ;; offset=0x0670
            sub     w6, w4, #1
            cmp     w6, w3
            bhs     G_M000_IG19
            sub     w6, w4, #1
            ldr     s23, [x2, w6, UXTW #2]
            add     w6, w4, #1
            cmp     w6, w3
            bhs     G_M000_IG19
            add     w6, w4, #1
            ldr     s25, [x2, w6, UXTW #2]
            fadd    s23, s25, s23
            sub     w6, w4, w5
            cmp     w6, w3
            bhs     G_M000_IG19
            ldr     s25, [x2, w6, UXTW #2]
            add     w6, w4, w5
            cmp     w6, w3
            bhs     G_M000_IG19
            ldr     s26, [x2, w6, UXTW #2]
            fadd    s25, s25, s26
            fadd    s23, s23, s25
            fmov    s25, #0.2500
            fmul    s23, s23, s25
            fadd    s25, s24, s16
            movi    v26.2s, #0
            fmax    s25, s25, s26
            fmul    s26, s25, s25
            fmul    s25, s25, s17
            fmul    s25, s25, s26
            fadd    s25, s25, s18
            fmul    s26, s26, s19
            fadd    s26, s26, s20
            fdiv    s25, s25, s26
            fsub    s23, s24, s23
            fmul    s23, s25, s23
            fmul    s23, s23, s23
            fmin    s23, s23, s21
            ldr     s24, [@RWD00]
            fmul    s23, s23, s24
            fmov    s24, #28.0000
            fadd    s23, s23, s24
            fsqrt   s23, s23
            fmov    s24, #0.2500
            fmul    s23, s23, s24
            mov     w6, #1
            align   [0 bytes for IG10]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG10:                ;; offset=0x0724
            ldr     w7, [x19, #0x4C]
            madd    w7, w6, w7, w4
            cmp     w7, w3
            bhs     G_M000_IG19
            ldr     s24, [x2, w7, UXTW #2]
            sub     w8, w7, #1
            cmp     w8, w3
            bhs     G_M000_IG19
            ldr     s25, [x2, w8, UXTW #2]
            add     w8, w7, #1
            cmp     w8, w3
            bhs     G_M000_IG19
            ldr     s26, [x2, w8, UXTW #2]
            fadd    s25, s26, s25
            sub     w8, w7, w5
            cmp     w8, w3
            bhs     G_M000_IG19
            ldr     s26, [x2, w8, UXTW #2]
            add     w7, w7, w5
            cmp     w7, w3
            bhs     G_M000_IG19
            ldr     s27, [x2, w7, UXTW #2]
            fadd    s26, s26, s27
            fadd    s25, s25, s26
            fmov    s26, #0.2500
            fmul    s25, s25, s26
            fadd    s26, s24, s16
            movi    v27.2s, #0
            fmax    s26, s26, s27
            fmul    s27, s26, s26
            fmul    s26, s26, s17
            fmul    s26, s26, s27
            fadd    s26, s26, s18
            fmul    s27, s27, s19
            fadd    s27, s27, s20
            fdiv    s26, s26, s27
            fsub    s24, s24, s25
            fmul    s24, s26, s24
            fmul    s24, s24, s24
            fmin    s24, s24, s21
            fmul    s24, s24, s8
            fmov    s25, #28.0000
            fadd    s24, s24, s25
            fsqrt   s24, s24
            fmov    s25, #0.2500
            fmul    s24, s24, s25
            fadd    s23, s24, s23
            add     w6, w6, #1
            cmp     w6, #4
            blt     G_M000_IG10

G_M000_IG11:                ;; offset=0x07EC
            add     w4, w1, #2
            cmp     w4, w3
            bhs     G_M000_IG19
            ldr     s24, [x2, w4, UXTW #2]
            sub     w6, w4, #1
            cmp     w6, w3
            bhs     G_M000_IG19
            sub     w6, w4, #1
            ldr     s25, [x2, w6, UXTW #2]
            add     w6, w4, #1
            cmp     w6, w3
            bhs     G_M000_IG19
            add     w6, w4, #1
            ldr     s26, [x2, w6, UXTW #2]
            fadd    s25, s26, s25
            sub     w6, w4, w5
            cmp     w6, w3
            bhs     G_M000_IG19
            ldr     s26, [x2, w6, UXTW #2]
            add     w6, w4, w5
            cmp     w6, w3
            bhs     G_M000_IG19
            ldr     s27, [x2, w6, UXTW #2]
            fadd    s26, s26, s27
            fadd    s25, s25, s26
            fmov    s26, #0.2500
            fmul    s25, s25, s26
            fadd    s26, s24, s16
            movi    v27.2s, #0
            fmax    s26, s26, s27
            fmul    s27, s26, s26
            fmul    s26, s26, s17
            fmul    s26, s26, s27
            fadd    s26, s26, s18
            fmul    s27, s27, s19
            fadd    s27, s27, s20
            fdiv    s26, s26, s27
            fsub    s24, s24, s25
            fmul    s24, s26, s24
            fmul    s24, s24, s24
            fmin    s24, s24, s21
            ldr     s25, [@RWD00]
            fmul    s24, s24, s25
            fmov    s25, #28.0000
            fadd    s24, s24, s25
            fsqrt   s24, s24
            fmov    s25, #0.2500
            fmul    s24, s24, s25
            mov     w6, #1
            align   [0 bytes for IG12]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG12:                ;; offset=0x08B0
            ldr     w7, [x19, #0x4C]
            madd    w7, w6, w7, w4
            cmp     w7, w3
            bhs     G_M000_IG19
            ldr     s25, [x2, w7, UXTW #2]
            sub     w8, w7, #1
            cmp     w8, w3
            bhs     G_M000_IG19
            ldr     s26, [x2, w8, UXTW #2]
            add     w8, w7, #1
            cmp     w8, w3
            bhs     G_M000_IG19
            ldr     s27, [x2, w8, UXTW #2]
            fadd    s26, s27, s26
            sub     w8, w7, w5
            cmp     w8, w3
            bhs     G_M000_IG19
            ldr     s27, [x2, w8, UXTW #2]
            add     w7, w7, w5
            cmp     w7, w3
            bhs     G_M000_IG19
            ldr     s28, [x2, w7, UXTW #2]
            fadd    s27, s27, s28
            fadd    s26, s26, s27
            fmov    s27, #0.2500
            fmul    s26, s26, s27
            fadd    s27, s25, s16
            movi    v28.2s, #0
            fmax    s27, s27, s28
            fmul    s28, s27, s27
            fmul    s27, s27, s17
            fmul    s27, s27, s28
            fadd    s27, s27, s18
            fmul    s28, s28, s19
            fadd    s28, s28, s20
            fdiv    s27, s27, s28
            fsub    s25, s25, s26
            fmul    s25, s27, s25
            fmul    s25, s25, s25
            fmin    s25, s25, s21
            fmul    s25, s25, s8
            fmov    s26, #28.0000
            fadd    s25, s25, s26
            fsqrt   s25, s25
            fmov    s26, #0.2500
            fmul    s25, s25, s26
            fadd    s24, s25, s24
            add     w6, w6, #1
            cmp     w6, #4
            blt     G_M000_IG12

G_M000_IG13:                ;; offset=0x0978
            add     w1, w1, #3
            cmp     w1, w3
            bhs     G_M000_IG19
            ldr     s25, [x2, w1, UXTW #2]
            sub     w4, w1, #1
            cmp     w4, w3
            bhs     G_M000_IG19
            sub     w4, w1, #1
            ldr     s26, [x2, w4, UXTW #2]
            add     w4, w1, #1
            cmp     w4, w3
            bhs     G_M000_IG19
            add     w4, w1, #1
            ldr     s27, [x2, w4, UXTW #2]
            fadd    s26, s27, s26
            sub     w4, w1, w5
            cmp     w4, w3
            bhs     G_M000_IG19
            ldr     s27, [x2, w4, UXTW #2]
            add     w4, w1, w5
            cmp     w4, w3
            bhs     G_M000_IG19
            ldr     s28, [x2, w4, UXTW #2]
            fadd    s27, s27, s28
            fadd    s26, s26, s27
            fmov    s27, #0.2500
            fmul    s26, s26, s27
            fadd    s27, s25, s16
            movi    v28.2s, #0
            fmax    s27, s27, s28
            fmul    s28, s27, s27
            fmul    s27, s27, s17
            fmul    s27, s27, s28
            fadd    s27, s27, s18
            fmul    s28, s28, s19
            fadd    s28, s28, s20
            fdiv    s27, s27, s28
            fsub    s25, s25, s26
            fmul    s25, s27, s25
            fmul    s25, s25, s25
            fmin    s25, s25, s21
            ldr     s26, [@RWD00]
            fmul    s25, s25, s26
            fmov    s26, #28.0000
            fadd    s25, s25, s26
            fsqrt   s25, s25
            fmov    s26, #0.2500
            fmul    s25, s25, s26
            mov     w4, #1
            align   [0 bytes for IG14]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG14:                ;; offset=0x0A3C
            ldr     w6, [x19, #0x4C]
            madd    w6, w4, w6, w1
            cmp     w6, w3
            bhs     G_M000_IG19
            ldr     s26, [x2, w6, UXTW #2]
            sub     w7, w6, #1
            cmp     w7, w3
            bhs     G_M000_IG19
            ldr     s27, [x2, w7, UXTW #2]
            add     w7, w6, #1
            cmp     w7, w3
            bhs     G_M000_IG19
            ldr     s28, [x2, w7, UXTW #2]
            fadd    s27, s28, s27
            sub     w7, w6, w5
            cmp     w7, w3
            bhs     G_M000_IG19
            ldr     s28, [x2, w7, UXTW #2]
            add     w6, w6, w5
            cmp     w6, w3
            bhs     G_M000_IG19
            ldr     s29, [x2, w6, UXTW #2]
            fadd    s28, s28, s29
            fadd    s27, s27, s28
            fmov    s28, #0.2500
            fmul    s27, s27, s28
            fadd    s28, s26, s16
            movi    v29.2s, #0
            fmax    s28, s28, s29
            fmul    s29, s28, s28
            fmul    s28, s28, s17
            fmul    s28, s28, s29
            fadd    s28, s28, s18
            fmul    s29, s29, s19
            fadd    s29, s29, s20
            fdiv    s28, s28, s29
            fsub    s26, s26, s27
            fmul    s26, s28, s26
            fmul    s26, s26, s26
            fmin    s26, s26, s21
            fmul    s26, s26, s8
            fmov    s27, #28.0000
            fadd    s26, s26, s27
            fsqrt   s26, s26
            fmov    s27, #0.2500
            fmul    s26, s26, s27
            fadd    s25, s26, s25
            add     w4, w4, #1
            cmp     w4, #4
            blt     G_M000_IG14

G_M000_IG15:                ;; offset=0x0B04
            ldr     x1, [x19, #0x18]
            asr     w2, w22, #31
            and     w2, w2, #3
            add     w2, w2, w22
            add     w2, w21, w2,  ASR #2
            ldr     w3, [x1, #0x08]
            cmp     w2, w3
            bhs     G_M000_IG19
            add     x1, x1, #16
            fadd    s22, s22, s23
            fadd    s22, s22, s24
            fadd    s22, s22, s25
            fmov    s23, #0.2500
            fmul    s22, s22, s23
            str     s22, [x1, w2, UXTW #2]
            add     w22, w22, #4

G_M000_IG16:                ;; offset=0x0B44
            cmp     w22, w0
            blt     G_M000_IG07

G_M000_IG17:                ;; offset=0x0B4C
            ldp     x21, x22, [sp, #0x60]
            ldp     x19, x20, [sp, #0x50]
            ldr     d14, [sp, #0x48]
            ldp     d12, d13, [sp, #0x38]
            ldp     d10, d11, [sp, #0x28]
            ldp     d8, d9, [sp, #0x18]
            ldp     fp, lr, [sp], #0x70
            ret     lr

G_M000_IG18:                ;; offset=0x0B6C
            movz    x0, #0x4750
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()
            brk     #0

G_M000_IG19:                ;; offset=0x0B84
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dd	480E0640h		;    145433
RWD04  	dq	3E8000003E800000h, 3E8000003E800000h
RWD20  	dq	409B0A3D409B0A3Dh, 409B0A3D409B0A3Dh
RWD36  	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD52  	dq	449E380D449E380Dh, 449E380D449E380Dh
RWD68  	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD84  	dq	44228FFE44228FFEh, 44228FFE44228FFEh
RWD100 	dq	3E4CCCCD3E4CCCCDh, 3E4CCCCD3E4CCCCDh
RWD116 	dq	480E0640480E0640h, 480E0640480E0640h
RWD132 	dq	41E0000041E00000h, 41E0000041E00000h
RWD148 	dq	3E8000003E800000h, 3E8000003E800000h
RWD164 	dq	3E8000003E800000h, 3E8000003E800000h
RWD180 	dq	3E4CCCCD3E4CCCCDh, 3E4CCCCD3E4CCCCDh
RWD196 	dq	480E0640480E0640h, 480E0640480E0640h
RWD212 	dq	41E0000041E00000h, 41E0000041E00000h
RWD228 	dq	3E8000003E800000h, 3E8000003E800000h
RWD244 	dq	3E8000003E800000h, 3E8000003E800000h
RWD260 	dq	3E4CCCCD3E4CCCCDh, 3E4CCCCD3E4CCCCDh
RWD276 	dq	480E0640480E0640h, 480E0640480E0640h
RWD292 	dq	41E0000041E00000h, 41E0000041E00000h
RWD308 	dq	3E8000003E800000h, 3E8000003E800000h
RWD324 	dq	3E8000003E800000h, 3E8000003E800000h
RWD340 	dq	3E4CCCCD3E4CCCCDh, 3E4CCCCD3E4CCCCDh
RWD356 	dq	480E0640480E0640h, 480E0640480E0640h
RWD372 	dq	41E0000041E00000h, 41E0000041E00000h
RWD388 	dq	3E8000003E800000h, 3E8000003E800000h
RWD404 	dq	3E4CCCCD3E4CCCCDh

; Total bytes of code 2956

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 33 single block inlinees; 11 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x30]!
            stp     x19, x20, [sp, #0x10]
            stp     x21, x22, [sp, #0x20]
            mov     fp, sp
            mov     x19, x0
            mov     w20, w1
            mov     w21, w2

G_M000_IG02:                ;; offset=0x001C
            mov     w22, wzr
            tst     w3, #255
            beq     G_M000_IG08

G_M000_IG03:                ;; offset=0x0028
            ldr     w0, [x19, #0x50]
            cmp     w0, #5
            ble     G_M000_IG08
            mov     w22, #1
            mov     x0, x19
            mov     w1, w20
            mov     w2, w21
            mov     w3, wzr
            movz    x4, #0x48A0
            movk    x4, #0x695F LSL #16
            movk    x4, #0x7FF8 LSL #32
            ldr     x4, [x4]
            blr     x4      // code for DotJpegli.Internal.StencilAdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this
            ldr     w0, [x19, #0x50]
            sub     w1, w0, #4
            cmp     w1, #1
            ble     G_M000_IG08
            sub     w1, w20, #1
            add     w2, w20, #1
            align   [0 bytes for IG04]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG04:                ;; offset=0x0074
            madd    w3, w1, w0, w22
            madd    w4, w20, w0, w22
            madd    w5, w2, w0, w22
            ldr     x6, [x19, #0x18]
            sub     w7, w3, #1
            tbnz    w7, #31, G_M000_IG11
            ldr     w8, [x6, #0x08]
            sub     w9, w8, w7
            cmp     w9, #4
            blt     G_M000_IG11
            cmp     w7, w8
            bhs     G_M000_IG14
            add     x6, x6, #16
            ubfiz   x7, x7, #2, #32
            ldr     q16, [x6, x7]
            sub     w7, w8, w3
            cmp     w7, #4
            ccmp    w3, #0, nc, ge
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x7, x3, #2, #32
            ldr     q17, [x6, x7]
            add     w3, w3, #1
            sub     w7, w8, w3
            cmp     w7, #4
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x3, x3, #2, #32
            ldr     q18, [x6, x3]
            sub     w3, w4, #1
            sub     w7, w8, w3
            cmp     w7, #4
            ccmp    w3, #0, nc, ge
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x3, x3, #2, #32
            ldr     q19, [x6, x3]
            sub     w3, w8, w4
            cmp     w3, #4
            ccmp    w4, #0, nc, ge
            blt     G_M000_IG11
            cmp     w4, w8
            bhs     G_M000_IG14
            ubfiz   x3, x4, #2, #32
            ldr     q20, [x6, x3]
            add     w3, w4, #1
            sub     w4, w8, w3
            cmp     w4, #4
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x3, x3, #2, #32
            ldr     q21, [x6, x3]
            sub     w3, w5, #1
            sub     w4, w8, w3
            cmp     w4, #4
            ccmp    w3, #0, nc, ge
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x3, x3, #2, #32
            ldr     q22, [x6, x3]
            sub     w3, w8, w5
            cmp     w3, #4
            ccmp    w5, #0, nc, ge
            blt     G_M000_IG11
            cmp     w5, w8
            bhs     G_M000_IG14
            ubfiz   x3, x5, #2, #32
            ldr     q23, [x6, x3]
            add     w3, w5, #1
            sub     w4, w8, w3
            cmp     w4, #4
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x3, x3, #2, #32
            ldr     q24, [x6, x3]
            ldrb    w3, [x19, #0x60]
            cbnz    w3, G_M000_IG06

G_M000_IG05:                ;; offset=0x01C0
            fmax    v25.4s, v16.4s, v17.4s
            fmin    v16.4s, v16.4s, v17.4s
            fmax    v17.4s, v18.4s, v19.4s
            fmin    v18.4s, v18.4s, v19.4s
            fmax    v19.4s, v16.4s, v18.4s
            fmin    v16.4s, v16.4s, v18.4s
            fmax    v26.4s, v25.4s, v17.4s
            fmin    v17.4s, v25.4s, v17.4s
            fmax    v18.4s, v17.4s, v19.4s
            fmin    v17.4s, v17.4s, v19.4s
            fmax    v19.4s, v16.4s, v20.4s
            fmax    v25.4s, v17.4s, v19.4s
            fmax    v27.4s, v18.4s, v25.4s
            fmin    v16.4s, v16.4s, v20.4s
            fmin    v17.4s, v17.4s, v19.4s
            fmin    v18.4s, v18.4s, v25.4s
            fmin    v19.4s, v26.4s, v27.4s
            fmax    v20.4s, v16.4s, v21.4s
            fmax    v25.4s, v17.4s, v20.4s
            fmax    v26.4s, v18.4s, v25.4s
            fmin    v16.4s, v16.4s, v21.4s
            fmin    v17.4s, v17.4s, v20.4s
            fmin    v18.4s, v18.4s, v25.4s
            fmin    v19.4s, v19.4s, v26.4s
            fmax    v21.4s, v16.4s, v22.4s
            fmax    v20.4s, v17.4s, v21.4s
            fmax    v25.4s, v18.4s, v20.4s
            fmin    v16.4s, v16.4s, v22.4s
            fmin    v17.4s, v17.4s, v21.4s
            fmin    v18.4s, v18.4s, v20.4s
            fmin    v19.4s, v19.4s, v25.4s
            fmax    v22.4s, v16.4s, v23.4s
            fmax    v20.4s, v17.4s, v22.4s
            fmax    v21.4s, v18.4s, v20.4s
            fmin    v16.4s, v16.4s, v23.4s
            fmin    v17.4s, v17.4s, v22.4s
            fmin    v18.4s, v18.4s, v20.4s
            fmin    v19.4s, v19.4s, v21.4s
            fmax    v23.4s, v16.4s, v24.4s
            fmax    v20.4s, v17.4s, v23.4s
            fmax    v21.4s, v18.4s, v20.4s
            fmin    v16.4s, v16.4s, v24.4s
            fmin    v17.4s, v17.4s, v23.4s
            fmin    v18.4s, v18.4s, v20.4s
            fmin    v19.4s, v19.4s, v21.4s
            mov     v20.16b, v19.16b
            mov     v19.16b, v17.16b
            mov     v17.16b, v18.16b
            b       G_M000_IG07

G_M000_IG06:                ;; offset=0x0284
            fmax    v25.4s, v16.4s, v19.4s
            fmin    v16.4s, v16.4s, v19.4s
            fmax    v19.4s, v17.4s, v23.4s
            fmin    v17.4s, v17.4s, v23.4s
            fmax    v23.4s, v18.4s, v21.4s
            fmin    v18.4s, v18.4s, v21.4s
            fmax    v21.4s, v20.4s, v24.4s
            fmin    v20.4s, v20.4s, v24.4s
            fmax    v24.4s, v16.4s, v19.4s
            fmin    v16.4s, v16.4s, v19.4s
            fmax    v19.4s, v18.4s, v20.4s
            fmin    v18.4s, v18.4s, v20.4s
            fmax    v26.4s, v25.4s, v21.4s
            fmin    v20.4s, v25.4s, v21.4s
            fmax    v21.4s, v23.4s, v22.4s
            fmin    v22.4s, v23.4s, v22.4s
            fmax    v23.4s, v16.4s, v18.4s
            fmin    v16.4s, v16.4s, v18.4s
            fmax    v18.4s, v17.4s, v20.4s
            fmin    v17.4s, v17.4s, v20.4s
            fmax    v25.4s, v19.4s, v22.4s
            fmin    v20.4s, v19.4s, v22.4s
            fmin    v22.4s, v24.4s, v26.4s
            fmax    v19.4s, v17.4s, v20.4s
            fmin    v17.4s, v17.4s, v20.4s
            fmin    v20.4s, v18.4s, v21.4s
            fmin    v22.4s, v25.4s, v22.4s
            fmax    v18.4s, v16.4s, v17.4s
            fmin    v16.4s, v16.4s, v17.4s
            fmax    v17.4s, v23.4s, v19.4s
            fmin    v19.4s, v23.4s, v19.4s
            fmax    v21.4s, v20.4s, v22.4s
            fmin    v20.4s, v20.4s, v22.4s
            fmax    v22.4s, v19.4s, v20.4s
            fmin    v19.4s, v19.4s, v20.4s
            fmin    v20.4s, v17.4s, v21.4s
            fmax    v17.4s, v18.4s, v19.4s
            fmin    v19.4s, v18.4s, v19.4s
            fmin    v20.4s, v22.4s, v20.4s

G_M000_IG07:                ;; offset=0x0320
            movi    v18.4s, #0x3E,  LSL #24
            fmul    v16.4s, v16.4s, v18.4s
            ldr     q18, [@RWD00]
            fmul    v18.4s, v19.4s, v18.4s
            fadd    v16.4s, v16.4s, v18.4s
            ldr     q18, [@RWD16]
            fmul    v17.4s, v17.4s, v18.4s
            ldr     q18, [@RWD32]
            fmul    v18.4s, v20.4s, v18.4s
            fadd    v17.4s, v17.4s, v18.4s
            fadd    v16.4s, v16.4s, v17.4s
            ldr     x3, [x19, #0x20]
            add     w4, w21, w22
            ldr     w5, [x3, #0x08]
            cmp     w5, w4
            bls     G_M000_IG12
            sub     w5, w5, w4
            cmp     w5, #4
            blt     G_M000_IG13
            add     x3, x3, #16
            ubfiz   x4, x4, #2, #32
            str     q16, [x3, x4]
            add     w22, w22, #4
            ldr     w3, [x19, #0x50]
            sub     w3, w3, #4
            cmp     w22, w3
            blt     G_M000_IG04

G_M000_IG08:                ;; offset=0x038C
            ldr     w0, [x19, #0x50]
            cmp     w22, w0
            bge     G_M000_IG10

G_M000_IG09:                ;; offset=0x0398
            mov     x0, x19
            mov     w1, w20
            mov     w2, w21
            mov     w3, w22
            movz    x4, #0x48A0
            movk    x4, #0x695F LSL #16
            movk    x4, #0x7FF8 LSL #32
            ldr     x4, [x4]
            blr     x4      // code for DotJpegli.Internal.StencilAdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this
            add     w22, w22, #1
            ldr     w0, [x19, #0x50]
            cmp     w22, w0
            blt     G_M000_IG09

G_M000_IG10:                ;; offset=0x03CC
            ldp     x21, x22, [sp, #0x20]
            ldp     x19, x20, [sp, #0x10]
            ldp     fp, lr, [sp], #0x30
            ret     lr

G_M000_IG11:                ;; offset=0x03DC
            movz    x0, #0x4750
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()
            brk     #0

G_M000_IG12:                ;; offset=0x03F4
            movz    x0, #0x4768
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLess()
            brk     #0

G_M000_IG13:                ;; offset=0x040C
            movz    x0, #0x4468
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentException_DestinationTooShort()
            brk     #0

G_M000_IG14:                ;; offset=0x0424
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dq	3D99999A3D99999Ah, 3D99999A3D99999Ah
RWD16  	dq	3D75C28F3D75C28Fh, 3D75C28F3D75C28Fh
RWD32  	dq	3D4CCCCD3D4CCCCDh, 3D4CCCCD3D4CCCCDh

; Total bytes of code 1068

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data
; 2 inlinees with PGO data; 29 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x10]!
            mov     fp, sp

G_M000_IG02:                ;; offset=0x0008
            sub     w4, w3, #1
            cmp     w4, #0
            csel    w4, w4, wzr, ge
            add     w5, w3, #1
            ldr     w6, [x0, #0x50]
            sub     w7, w6, #1
            cmp     w5, w7
            csel    w5, w5, w7, le
            sub     w7, w1, #1
            mul     w7, w7, w6
            mul     w8, w1, w6
            add     w1, w1, #1
            mul     w1, w1, w6
            ldr     x6, [x0, #0x18]
            add     w9, w7, w4
            ldr     w10, [x6, #0x08]
            cmp     w9, w10
            bhs     G_M000_IG07
            add     x6, x6, #16
            ldr     s16, [x6, w9, UXTW #2]
            add     w9, w7, w3
            cmp     w9, w10
            bhs     G_M000_IG07
            ldr     s17, [x6, w9, UXTW #2]
            add     w7, w7, w5
            cmp     w7, w10
            bhs     G_M000_IG07
            ldr     s18, [x6, w7, UXTW #2]
            add     w7, w8, w4
            cmp     w7, w10
            bhs     G_M000_IG07
            ldr     s19, [x6, w7, UXTW #2]
            add     w7, w8, w3
            cmp     w7, w10
            bhs     G_M000_IG07
            ldr     s20, [x6, w7, UXTW #2]
            add     w7, w8, w5
            cmp     w7, w10
            bhs     G_M000_IG07
            ldr     s21, [x6, w7, UXTW #2]
            add     w4, w1, w4
            cmp     w4, w10
            bhs     G_M000_IG07
            ldr     s22, [x6, w4, UXTW #2]
            add     w4, w1, w3
            cmp     w4, w10
            bhs     G_M000_IG07
            ldr     s23, [x6, w4, UXTW #2]
            add     w1, w1, w5
            cmp     w1, w10
            bhs     G_M000_IG07
            ldr     s24, [x6, w1, UXTW #2]
            ldrb    w1, [x0, #0x60]
            cbnz    w1, G_M000_IG04

G_M000_IG03:                ;; offset=0x00E0
            fmax    s25, s16, s17
            fmin    s16, s16, s17
            fmax    s17, s18, s19
            fmin    s18, s18, s19
            fmax    s19, s16, s18
            fmin    s16, s16, s18
            fmax    s18, s25, s17
            fmin    s17, s25, s17
            fmax    s25, s17, s19
            fmin    s17, s17, s19
            fmax    s19, s16, s20
            fmax    s26, s17, s19
            fmax    s27, s25, s26
            fmin    s16, s16, s20
            fmin    s17, s17, s19
            fmin    s20, s25, s26
            fmin    s19, s18, s27
            fmax    s18, s16, s21
            fmax    s25, s17, s18
            fmax    s26, s20, s25
            fmin    s16, s16, s21
            fmin    s17, s17, s18
            fmin    s20, s20, s25
            fmin    s19, s19, s26
            fmax    s21, s16, s22
            fmax    s18, s17, s21
            fmax    s25, s20, s18
            fmin    s16, s16, s22
            fmin    s17, s17, s21
            fmin    s20, s20, s18
            fmin    s19, s19, s25
            fmax    s22, s16, s23
            fmax    s18, s17, s22
            fmax    s21, s20, s18
            fmin    s16, s16, s23
            fmin    s17, s17, s22
            fmin    s20, s20, s18
            fmin    s19, s19, s21
            fmax    s23, s16, s24
            fmax    s18, s17, s23
            fmax    s21, s20, s18
            fmin    s16, s16, s24
            fmin    s17, s17, s23
            fmin    s20, s20, s18
            fmin    s19, s19, s21
            fmov    s22, s19
            fmov    s0, s17
            fmov    s17, s20
            fmov    s20, s0
            b       G_M000_IG05

G_M000_IG04:                ;; offset=0x01A8
            fmax    s25, s16, s19
            fmin    s16, s16, s19
            fmax    s19, s17, s23
            fmin    s17, s17, s23
            fmax    s23, s18, s21
            fmin    s18, s18, s21
            fmax    s21, s20, s24
            fmin    s20, s20, s24
            fmax    s24, s16, s19
            fmin    s16, s16, s19
            fmax    s19, s18, s20
            fmin    s18, s18, s20
            fmax    s20, s25, s21
            fmin    s21, s25, s21
            fmax    s25, s23, s22
            fmin    s22, s23, s22
            fmax    s23, s16, s18
            fmin    s16, s16, s18
            fmax    s18, s17, s21
            fmin    s17, s17, s21
            fmax    s21, s19, s22
            fmin    s22, s19, s22
            fmin    s19, s24, s20
            fmax    s20, s17, s22
            fmin    s17, s17, s22
            fmin    s22, s18, s25
            fmin    s18, s21, s19
            fmax    s19, s16, s17
            fmin    s16, s16, s17
            fmax    s17, s23, s20
            fmin    s20, s23, s20
            fmax    s21, s22, s18
            fmin    s22, s22, s18
            fmax    s18, s20, s22
            fmin    s20, s20, s22
            fmin    s22, s17, s21
            fmax    s17, s19, s20
            fmin    s20, s19, s20
            fmin    s22, s18, s22

G_M000_IG05:                ;; offset=0x0244
            ldr     x0, [x0, #0x20]
            add     w1, w2, w3
            ldr     w4, [x0, #0x08]
            cmp     w1, w4
            bhs     G_M000_IG07
            add     w1, w2, w3
            add     x0, x0, #16
            fmov    s18, #0.1250
            fmul    s16, s16, s18
            ldr     s18, [@RWD00]
            fmul    s18, s20, s18
            fadd    s16, s16, s18
            ldr     s18, [@RWD04]
            fmul    s17, s17, s18
            ldr     s18, [@RWD08]
            fmul    s18, s22, s18
            fadd    s17, s17, s18
            fadd    s16, s16, s17
            str     s16, [x0, w1, UXTW #2]

G_M000_IG06:                ;; offset=0x0290
            ldp     fp, lr, [sp], #0x10
            ret     lr

G_M000_IG07:                ;; offset=0x0298
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dd	3D99999Ah		;     0.075
RWD04  	dd	3D75C28Fh		;      0.06
RWD08  	dd	3D4CCCCDh		;      0.05

; Total bytes of code 672

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ComputeMask(float):float (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x10]!
            mov     fp, sp

G_M000_IG02:                ;; offset=0x0008
            ldr     s16, [@RWD00]
            fmul    s0, s0, s16
            ldr     d16, [@RWD04]
            fmax    s0, s0, s16
            fmul    s16, s0, s0
            ldr     s17, [@RWD12]
            fadd    s17, s16, s17
            fmov    s18, #1.0000
            fdiv    s17, s18, s17
            ldr     s18, [@RWD16]
            fmul    s17, s17, s18
            ldr     s18, [@RWD20]
            fadd    s16, s16, s18
            fmov    s18, #1.0000
            fdiv    s16, s18, s16
            ldr     s18, [@RWD24]
            fmul    s16, s16, s18
            ldr     s18, [@RWD28]
            fadd    s0, s0, s18
            fmov    s18, #1.0000
            fdiv    s0, s18, s0
            ldr     s18, [@RWD32]
            fmul    s0, s0, s18
            fadd    s0, s16, s0
            fadd    s0, s17, s0
            ldr     s16, [@RWD36]
            fadd    s0, s0, s16

G_M000_IG03:                ;; offset=0x0074
            ldp     fp, lr, [sp], #0x10
            ret     lr

RWD00  	dd	3F3F62FEh		;  0.747604
RWD04  	dq	3A83126F3A83126Fh
RWD12  	dd	3F0C5322h		;  0.548144
RWD16  	dd	404F0F94h		;   3.23533
RWD20  	dd	400C5322h		;   2.19257
RWD24  	dd	40A0B47Bh		;   5.02203
RWD28  	dd	4398852Ah		;    305.04
RWD32  	dd	414E7F18h		;    12.906
RWD36  	dd	BF3DE353h		;  -0.74175

; Total bytes of code 124

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ModulateScalar(System.ReadOnlySpan`1[float],int,float):float (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 0 single block inlinees; 2 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x20]!
            stp     d8, d9, [sp, #0x10]
            mov     fp, sp
            fmov    s8, s0

G_M000_IG02:                ;; offset=0x0010
            movi    v9.16b, #0
            movi    v0.16b, #0
            mov     w3, wzr
            ldr     s16, [@RWD00]
            ldr     s17, [@RWD04]
            ldr     s18, [@RWD08]
            ldr     s19, [@RWD12]
            ldr     s20, [@RWD16]

G_M000_IG03:                ;; offset=0x0030
            mul     w4, w3, w2
            cmp     w3, #7
            bne     G_M000_IG05

G_M000_IG04:                ;; offset=0x003C
            mov     w5, w4
            b       G_M000_IG06
            align   [0 bytes for IG07]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG05:                ;; offset=0x0044
            add     w5, w4, w2

G_M000_IG06:                ;; offset=0x0048
            cmp     w4, w1
            bhs     G_M000_IG11
            ldr     s21, [x0, w4, UXTW #2]
            mov     w6, wzr

G_M000_IG07:                ;; offset=0x0058
            add     w7, w4, w6
            add     w7, w7, #1
            cmp     w7, w1
            bhs     G_M000_IG11
            ldr     s22, [x0, w7, UXTW #2]
            add     w7, w5, w6
            cmp     w7, w1
            bhs     G_M000_IG11
            ldr     s23, [x0, w7, UXTW #2]
            fsub    s23, s21, s23
            fabs    s23, s23
            fsub    s24, s21, s22
            fabs    s24, s24
            fadd    s24, s24, s9
            fadd    s9, s23, s24
            fadd    s21, s21, s16
            movi    v23.2s, #0
            fmax    s21, s21, s23
            fmul    s23, s21, s21
            fmul    s21, s21, s17
            fmul    s21, s21, s23
            fadd    s21, s21, s18
            fmul    s23, s23, s19
            fadd    s23, s23, s20
            fdiv    s21, s23, s21
            fadd    s0, s21, s0
            fmov    s21, s22
            add     w6, w6, #1
            cmp     w6, #7
            blt     G_M000_IG07

G_M000_IG08:                ;; offset=0x00D0
            add     w4, w5, #7
            cmp     w4, w1
            bhs     G_M000_IG11
            ldr     s22, [x0, w4, UXTW #2]
            fsub    s22, s21, s22
            fabs    s22, s22
            fadd    s9, s22, s9
            fadd    s21, s21, s16
            movi    v22.2s, #0
            fmax    s21, s21, s22
            fmul    s22, s21, s21
            fmul    s21, s21, s17
            fmul    s21, s21, s22
            fadd    s21, s21, s18
            fmul    s22, s22, s19
            fadd    s22, s22, s20
            fdiv    s21, s22, s21
            fadd    s0, s21, s0
            add     w3, w3, #1
            cmp     w3, #8
            blt     G_M000_IG03

G_M000_IG09:                ;; offset=0x0124
            ldr     s16, [@RWD20]
            fmul    s0, s0, s16
            movz    x0, #0x47F8
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for DotJpegli.Internal.StencilAdaptiveQuantization:FastLog2(float):float
            ldr     s16, [@RWD24]
            fmul    s0, s0, s16
            ldr     s16, [@RWD28]
            fmul    s16, s9, s16
            fadd    s16, s16, s8
            fadd    s0, s0, s16

G_M000_IG10:                ;; offset=0x0158
            ldp     d8, d9, [sp, #0x10]
            ldp     fp, lr, [sp], #0x20
            ret     lr

G_M000_IG11:                ;; offset=0x0164
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dd	42233332h		;      40.8
RWD04  	dd	3B1DEA78h		; 0.00240961
RWD08  	dd	449E380Dh		;   1265.75
RWD12  	dd	42EF0C23h		;   119.524
RWD16  	dd	44228FFEh		;    650.25
RWD20  	dd	38808081h		; 6.12745e-05
RWD24  	dd	BDDC6A08h		; -0.107624
RWD28  	dd	B8933E1Eh		; -7.02108e-05

; Total bytes of code 364

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:FastLog2(float):float (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x10]!
            mov     fp, sp

G_M000_IG02:                ;; offset=0x0008
            mov     w0, v0.s[0]
            movz    w1, #0x5555
            movk    w1, #0xC0D5 LSL #16
            add     w1, w0, w1
            asr     w1, w1, #23
            sub     w0, w0, w1,  LSL #23
            fmov    s0, w0
            fmov    s16, #1.0000
            fsub    s0, s0, s16
            ldr     s16, [@RWD00]
            fmul    s16, s0, s16
            ldr     s17, [@RWD04]
            fadd    s16, s16, s17
            fmul    s16, s16, s0
            ldr     s17, [@RWD08]
            fsub    s16, s16, s17
            ldr     s17, [@RWD12]
            fmul    s17, s0, s17
            ldr     s18, [@RWD16]
            fadd    s17, s17, s18
            fmul    s0, s17, s0
            ldr     s17, [@RWD20]
            fadd    s0, s0, s17
            fdiv    s0, s16, s0
            scvtf   s16, w1
            fadd    s0, s0, s16

G_M000_IG03:                ;; offset=0x0070
            ldp     fp, lr, [sp], #0x10
            ret     lr

RWD00  	dd	3F3E11C7h		;  0.742459
RWD04  	dd	3FB6E02Bh		;   1.42872
RWD08  	dd	35F85AB0h		; 1.85038e-06
RWD12  	dd	3E32458Ch		;  0.174093
RWD16  	dd	3F813CEDh		;   1.00967
RWD20  	dd	3F7D8625h		;  0.990328

; Total bytes of code 120

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:FastPow2(float):float (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x10]!
            mov     fp, sp

G_M000_IG02:                ;; offset=0x0008
            frintm  s16, s0
            fcvtzs  w0, s16
            add     w0, w0, #127
            lsl     w0, w0, #23
            fmov    s17, w0
            fsub    s0, s0, s16
            ldr     s16, [@RWD00]
            fadd    s16, s0, s16
            fmul    s16, s16, s0
            ldr     s18, [@RWD04]
            fadd    s16, s16, s18
            fmul    s16, s16, s0
            ldr     s18, [@RWD08]
            fadd    s16, s16, s18
            fmul    s16, s16, s17
            ldr     s17, [@RWD12]
            fmul    s17, s0, s17
            ldr     s18, [@RWD16]
            fsub    s17, s17, s18
            fmul    s17, s17, s0
            ldr     s18, [@RWD20]
            fsub    s17, s17, s18
            fmul    s0, s17, s0
            ldr     s17, [@RWD24]
            fadd    s0, s0, s17
            fdiv    s0, s16, s0

G_M000_IG03:                ;; offset=0x0070
            ldp     fp, lr, [sp], #0x10
            ret     lr

RWD00  	dd	4122CC6Bh		;   10.1749
RWD04  	dd	424379A1h		;   48.8688
RWD08  	dd	42C519F0h		;   98.5507
RWD12  	dd	3E5749EEh		;  0.210243
RWD16  	dd	3CB621BEh		; 0.0222329
RWD20  	dd	419B8831h		;   19.4415
RWD24  	dd	42C519F1h		;   98.5507

; Total bytes of code 120

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ComputeRowCore(DotJpegli.Internal.PixelSource,int,int,int,System.Span`1[float],bool,bool,System.Threading.CancellationToken):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data
; 9 inlinees with PGO data; 35 single block inlinees; 35 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            sub     sp, sp, #320
            stp     d8, d9, [sp, #0xB0]
            stp     d10, d11, [sp, #0xC0]
            stp     d12, d13, [sp, #0xD0]
            stp     x19, x20, [sp, #0xE0]
            stp     x21, x22, [sp, #0xF0]
            stp     x23, x24, [sp, #0x100]
            stp     x25, x26, [sp, #0x110]
            stp     x27, x28, [sp, #0x120]
            stp     fp, lr, [sp, #0x130]
            add     fp, sp, #304
            stp     xzr, xzr, [fp, #-0xD0]
            str     xzr, [fp, #-0xE8]
            movz    x8, #0x2D5A
            movk    x8, #0xF7B0 LSL #16
            movk    x8, #0x2883 LSL #32
            str     x8, [fp, #-0x88]
            mov     x19, x0
            mov     x20, x1
            mov     w23, w2
            mov     w21, w3
            mov     w24, w4
            mov     w22, w7

G_M000_IG02:                ;; offset=0x005C
            ldr     x2, [fp, #0x18]
            str     x2, [fp, #-0xE8]
            mov     x25, x5
            mov     w26, w6
            ldrb    w2, [x19, #0x6A]
            cbnz    w2, G_M000_IG135
            ldr     w2, [x19, #0x48]
            lsl     w27, w2, #3
            ldr     x28, [x20, #0x08]
            cbz     x28, G_M000_IG04

G_M000_IG03:                ;; offset=0x0084
            ldr     x2, [x19, #0x38]
            cmp     x28, x2
            bne     G_M000_IG04
            ldr     w2, [x19, #0x5C]
            add     w2, w2, w27
            cmp     w2, w21
            bne     G_M000_IG04
            ldr     w2, [x19, #0x60]
            cmp     w23, w2
            beq     G_M000_IG05

G_M000_IG04:                ;; offset=0x00AC
            mov     w3, wzr
            b       G_M000_IG06

G_M000_IG05:                ;; offset=0x00B4
            uxtb    w4, w22
            ldrb    w3, [x19, #0x6B]
            cmp     w4, w3
            cset    x3, eq

G_M000_IG06:                ;; offset=0x00C4
            str     w3, [fp, #-0x8C]
            str     xzr, [x19, #0x38]
            ldr     x2, [fp, #-0xE8]
            cbnz    x2, G_M000_IG132

G_M000_IG07:                ;; offset=0x00D4
            tbnz    w21, #31, G_M000_IG137
            ldr     w2, [x19, #0x44]
            cmp     w21, w2
            bge     G_M000_IG137
            cbz     w27, G_M000_IG146
            sdiv    w2, w21, w27
            msub    w2, w2, w27, w21
            cbnz    w2, G_M000_IG137
            ldr     w2, [x19, #0x64]
            ldr     w0, [x19, #0x48]
            mul     w2, w2, w0
            cmp     w2, w26
            bgt     G_M000_IG138
            sub     w5, w21, #5
            str     w5, [fp, #-0x90]
            mov     w6, wzr
            cbz     w3, G_M000_IG133

G_M000_IG08:                ;; offset=0x0118
            ldr     x2, [x19, #0x18]
            mov     x0, x2
            ldr     w1, [x19, #0x54]
            mul     w4, w27, w1
            mov     w6, #10
            mul     w1, w1, w6
            cbnz    x0, G_M000_IG134
            orr     w0, w4, w1
            cbnz    w0, G_M000_IG139
            mov     x4, xzr
            mov     w1, wzr

G_M000_IG09:                ;; offset=0x0144
            cbz     x2, G_M000_IG10
            add     x0, x2, #16
            ldr     w2, [x2, #0x08]
            b       G_M000_IG11

G_M000_IG10:                ;; offset=0x0154
            mov     x0, xzr
            mov     w2, wzr

G_M000_IG11:                ;; offset=0x015C
            cmp     w1, w2
            bgt     G_M000_IG140
            ubfiz   x2, x1, #2, #32
            mov     x1, x4
            movz    x4, #0x56E0
            movk    x4, #0x6938 LSL #16
            movk    x4, #0x7FF8 LSL #32
            ldr     x4, [x4]
            blr     x4      // code for System.SpanHelpers:Memmove(byref,byref,nuint)
            mov     w6, #10
            mov     w3, w6
            ldr     w4, [fp, #-0x90]
            add     w1, w4, #9
            ldr     w0, [x19, #0x44]
            sub     w5, w0, #1
            tbnz    w5, #31, G_M000_IG141
            tbnz    w1, #31, G_M000_IG12
            cmp     w1, w5
            csel    w7, w1, w5, le
            b       G_M000_IG13

G_M000_IG12:                ;; offset=0x01AC
            mov     w7, wzr

G_M000_IG13:                ;; offset=0x01B0
            mov     w6, w3

G_M000_IG14:                ;; offset=0x01B4
            mov     w4, w6
            b       G_M000_IG52

G_M000_IG15:                ;; offset=0x01BC
            ldr     x1, [fp, #-0xE8]
            add     x1, x1, #32
            ldapr   w1, [x1]
            cbz     w1, G_M000_IG54
            b       G_M000_IG136

G_M000_IG16:                ;; offset=0x01D0
            mov     w6, w8
            b       G_M000_IG18

G_M000_IG17:                ;; offset=0x01D8
            mov     w6, wzr

G_M000_IG18:                ;; offset=0x01DC
            str     w6, [fp, #-0x98]
            ldr     w8, [x19, #0x54]
            str     w4, [fp, #-0x94]
            mul     w9, w4, w8
            add     w10, w9, #1
            str     w10, [fp, #-0x9C]
            cmp     w6, w7
            beq     G_M000_IG56

G_M000_IG19:                ;; offset=0x01FC
            uxtb    w0, w22
            mov     w11, w0
            cbnz    w11, G_M000_IG40
            ldr     w7, [x19, #0x40]
            mov     w8, w7
            str     w8, [fp, #-0xB4]
            ldr     w0, [x20, #0x1C]
            cbnz    w0, G_M000_IG22
            ldrb    w0, [x20, #0x26]
            cbz     w0, G_M000_IG20
            mov     w0, #4
            b       G_M000_IG22

G_M000_IG20:                ;; offset=0x022C
            ldr     w0, [x20, #0x20]
            cmp     w0, #1
            bne     G_M000_IG21
            mov     w0, #1
            b       G_M000_IG22

G_M000_IG21:                ;; offset=0x0240
            mov     w0, #2
            b       G_M000_IG22

G_M000_IG22:                ;; offset=0x0248
            cmp     w0, #3
            bne     G_M000_IG23
            mov     w9, wzr
            b       G_M000_IG24

G_M000_IG23:                ;; offset=0x0258
            cmp     w0, #4
            cset    x9, ne

G_M000_IG24:                ;; offset=0x0260
            cmp     w9, #0
            cset    x9, eq
            str     w9, [fp, #-0xB8]
            ldr     x0, [x19, #0x18]
            cbz     x0, G_M000_IG25
            mov     w11, w10
            add     x11, x11, w7, UXTW
            ldr     w13, [x0, #0x08]
            cmp     x11, x13
            bhi     G_M000_IG139
            add     x0, x0, #16
            ubfiz   x11, x10, #2, #32
            add     x11, x0, x11
            b       G_M000_IG26

G_M000_IG25:                ;; offset=0x0298
            orr     w11, w10, w7
            cbnz    w11, G_M000_IG139
            mov     x11, xzr
            mov     w7, wzr

G_M000_IG26:                ;; offset=0x02A8
            movn    xip1, #287
            str     x11, [fp, xip1]
            mov     w14, w7
            str     w14, [fp, #-0xDC]
            ldr     x0, [x20, #0x08]
            cbz     x0, G_M000_IG29
            ldrb    w0, [x0, #0x2C]
            cbnz    w0, G_M000_IG28

G_M000_IG27:                ;; offset=0x02C8
            b       G_M000_IG39

G_M000_IG28:                ;; offset=0x02CC
            cbz     w9, G_M000_IG39

G_M000_IG29:                ;; offset=0x02D0
            cmp     w6, w1
            bgt     G_M000_IG30
            mov     w0, w6
            ldr     w6, [fp, #-0x98]
            b       G_M000_IG31

G_M000_IG30:                ;; offset=0x02E4
            mov     w0, w1

G_M000_IG31:                ;; offset=0x02E8
            mul     w7, w0, w23
            str     w7, [fp, #-0xBC]
            mov     w11, wzr
            cbnz    w14, G_M000_IG37

G_M000_IG32:                ;; offset=0x02F8
            b       G_M000_IG48

G_M000_IG33:                ;; offset=0x02FC
            mov     w0, w1
            str     w11, [fp, #-0xC0]
            b       G_M000_IG35

G_M000_IG34:                ;; offset=0x0308
            mov     w0, wzr
            str     w11, [fp, #-0xC0]

G_M000_IG35:                ;; offset=0x0310
            ldr     w1, [x20, #0x20]
            madd    w1, w0, w1, w7
            mov     x0, x20
            mov     w2, w9
            movz    x12, #0x44B0
            movk    x12, #0x695F LSL #16
            movk    x12, #0x7FF8 LSL #32
            ldr     x12, [x12]
            blr     x12      // code for DotJpegli.Internal.PixelSource:ReadSample(int,int):float:this
            movn    xip1, #287
            ldr     x6, [fp, xip1]
            ldr     w7, [fp, #-0xC0]
            str     s0, [x6, w7, UXTW #2]
            add     w7, w7, #1
            ldr     w0, [fp, #-0xDC]
            cmp     w7, w0
            mov     w11, w7
            ldp     w9, w8, [fp, #-0xB8]
            bge     G_M000_IG48

G_M000_IG36:                ;; offset=0x035C
            ldr     w6, [fp, #-0x98]
            ldr     w7, [fp, #-0xBC]

G_M000_IG37:                ;; offset=0x0364
            sub     w1, w8, #1
            tbnz    w1, #31, G_M000_IG143
            ldr     w8, [fp, #-0xB4]
            tbnz    w11, #31, G_M000_IG34

G_M000_IG38:                ;; offset=0x0374
            cmp     w11, w1
            bgt     G_M000_IG33
            str     w11, [fp, #-0xC0]
            mov     w0, w11
            b       G_M000_IG35

G_M000_IG39:                ;; offset=0x0388
            mov     x6, x11
            mov     x0, x20
            mov     w1, w8
            mov     w3, w9
            ldr     w5, [fp, #-0x98]
            mov     w4, wzr
            movz    x8, #0x4498
            movk    x8, #0x695F LSL #16
            movk    x8, #0x7FF8 LSL #32
            ldr     x8, [x8]
            blr     x8      // code for DotJpegli.Internal.PixelSource:ReadConvertedRow(int,int,int,int,int,System.Span`1[float]):this
            b       G_M000_IG48

G_M000_IG40:                ;; offset=0x03B8
            ldr     w7, [x19, #0x40]
            mov     w1, w7
            ldr     w0, [x20, #0x1C]
            cbnz    w0, G_M000_IG43
            ldrb    w0, [x20, #0x26]
            cbz     w0, G_M000_IG41
            mov     w0, #4
            b       G_M000_IG43

G_M000_IG41:                ;; offset=0x03D8
            ldr     w0, [x20, #0x20]
            cmp     w0, #1
            bne     G_M000_IG42
            mov     w0, #1
            b       G_M000_IG43

G_M000_IG42:                ;; offset=0x03EC
            mov     w0, #2
            b       G_M000_IG43

G_M000_IG43:                ;; offset=0x03F4
            cmp     w0, #3
            bne     G_M000_IG44
            mov     w8, wzr
            b       G_M000_IG45

G_M000_IG44:                ;; offset=0x0404
            cmp     w0, #4
            cset    x8, ne

G_M000_IG45:                ;; offset=0x040C
            cmp     w8, #0
            cset    x8, eq
            ldr     x0, [x19, #0x18]
            cbz     x0, G_M000_IG46
            mov     w9, w10
            add     x9, x9, w7, UXTW
            ldr     w11, [x0, #0x08]
            cmp     x9, x11
            bhi     G_M000_IG139
            add     x0, x0, #16
            ubfiz   x9, x10, #2, #32
            add     x0, x0, x9
            b       G_M000_IG47

G_M000_IG46:                ;; offset=0x0440
            orr     w0, w10, w7
            cbnz    w0, G_M000_IG139
            mov     x0, xzr
            mov     w7, wzr

G_M000_IG47:                ;; offset=0x0450
            str     x0, [sp]
            str     w7, [sp, #0x08]
            mov     x0, x20
            mov     w3, w23
            mov     w4, w8
            mov     w5, wzr
            movz    x7, #0x4480
            movk    x7, #0x695F LSL #16
            movk    x7, #0x7FF8 LSL #32
            ldr     x7, [x7]
            blr     x7      // code for DotJpegli.Internal.PixelSource:ReadRowVector(int,int,int,int,int,int,System.Span`1[float]):this

G_M000_IG48:                ;; offset=0x047C
            ldr     x1, [x19, #0x18]
            mov     x0, x1
            ldr     w2, [x0, #0x08]
            ldr     w10, [fp, #-0x9C]
            cmp     w10, w2
            bhs     G_M000_IG147
            add     x0, x0, #16
            ldr     s0, [x0, w10, UXTW #2]
            sub     w3, w10, #1
            cmp     w3, w2
            bhs     G_M000_IG147
            sub     w3, w10, #1
            str     s0, [x0, w3, UXTW #2]
            ldr     w3, [x19, #0x40]
            add     w4, w10, w3
            mov     w5, w4
            ldr     w6, [x19, #0x4C]
            sub     w3, w6, w3
            add     w3, w3, #1
            cbz     x1, G_M000_IG49
            mov     w6, w5
            add     x6, x6, w3, UXTW
            cmp     x6, w2, UXTW
            bhi     G_M000_IG139
            add     x1, x1, #16
            ubfiz   x5, x5, #2, #32
            add     x5, x1, x5
            b       G_M000_IG50

G_M000_IG49:                ;; offset=0x04EC
            orr     w1, w5, w3
            cbnz    w1, G_M000_IG139
            mov     x5, xzr
            mov     w3, wzr

G_M000_IG50:                ;; offset=0x04FC
            sub     w1, w4, #1
            cmp     w1, w2
            bhs     G_M000_IG147
            ldr     s0, [x0, w1, UXTW #2]
            mov     w1, w3
            mov     x0, x5
            movz    x2, #0x42E8
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.SpanHelpers:Fill[float](byref,nuint,float)

G_M000_IG51:                ;; offset=0x0528
            ldp     w7, w1, [fp, #-0x98]
            add     w1, w1, #1
            mov     w4, w1

G_M000_IG52:                ;; offset=0x0534
            add     w1, w27, #10
            cmp     w4, w1
            bge     G_M000_IG61

G_M000_IG53:                ;; offset=0x0540
            ldr     x1, [fp, #-0xE8]
            cbnz    x1, G_M000_IG15

G_M000_IG54:                ;; offset=0x0548
            ldr     w5, [fp, #-0x90]
            add     w6, w5, w4
            ldr     w2, [x19, #0x44]
            sub     w1, w2, #1
            mov     w8, w1
            tbnz    w8, #31, G_M000_IG142
            tbnz    w6, #31, G_M000_IG17

G_M000_IG55:                ;; offset=0x0564
            cmp     w6, w8
            bgt     G_M000_IG16
            b       G_M000_IG18

G_M000_IG56:                ;; offset=0x0570
            ldr     x2, [x19, #0x18]
            ldr     w4, [fp, #-0x94]
            sub     w0, w4, #1
            mul     w0, w0, w8
            mov     w1, w8
            cbz     x2, G_M000_IG57
            mov     w7, w0
            add     x7, x7, w1, UXTW
            ldr     w10, [x2, #0x08]
            cmp     x7, x10
            bhi     G_M000_IG139
            add     x2, x2, #16
            ubfiz   x0, x0, #2, #32
            add     x7, x2, x0
            b       G_M000_IG58

G_M000_IG57:                ;; offset=0x05AC
            orr     w7, w0, w1
            cbnz    w7, G_M000_IG139
            mov     x7, xzr
            mov     w1, wzr

G_M000_IG58:                ;; offset=0x05BC
            ldr     x2, [x19, #0x18]
            cbz     x2, G_M000_IG59
            mov     w0, w9
            add     x0, x0, w8, UXTW
            ldr     w10, [x2, #0x08]
            cmp     x0, x10
            bhi     G_M000_IG139
            add     x2, x2, #16
            ubfiz   x9, x9, #2, #32
            add     x0, x2, x9
            b       G_M000_IG60

G_M000_IG59:                ;; offset=0x05E8
            orr     w0, w9, w8
            cbnz    w0, G_M000_IG139
            mov     x0, xzr
            mov     w8, wzr

G_M000_IG60:                ;; offset=0x05F8
            cmp     w1, w8
            bgt     G_M000_IG140
            ubfiz   x2, x1, #2, #32
            mov     x1, x7
            movz    x7, #0x56E0
            movk    x7, #0x6938 LSL #16
            movk    x7, #0x7FF8 LSL #32
            ldr     x7, [x7]
            blr     x7      // code for System.SpanHelpers:Memmove(byref,byref,nuint)
            b       G_M000_IG51

G_M000_IG61:                ;; offset=0x0620
            lsr     w2, w21, #2
            sub     w20, w2, #1
            ldr     w2, [x19, #0x50]
            asr     w0, w2, #31
            and     w0, w0, #3
            add     w2, w0, w2
            asr     w27, w2, #2
            mov     w4, wzr
            ldr     w3, [fp, #-0x8C]
            cbnz    w3, G_M000_IG63

G_M000_IG62:                ;; offset=0x0648
            movn    w3, #0
            b       G_M000_IG70

G_M000_IG63:                ;; offset=0x0650
            ldr     x2, [x19, #0x20]
            mov     x0, x2
            ldr     w1, [x19, #0x48]
            lsl     w1, w1, #1
            ldr     w3, [x19, #0x58]
            mul     w1, w1, w3
            lsl     w3, w3, #1
            cbz     x0, G_M000_IG64
            mov     w4, w1
            add     x4, x4, w3, UXTW
            ldr     w6, [x0, #0x08]
            cmp     x4, x6
            bhi     G_M000_IG139
            add     x0, x0, #16
            ubfiz   x1, x1, #2, #32
            add     x1, x0, x1
            b       G_M000_IG65

G_M000_IG64:                ;; offset=0x0694
            orr     w0, w1, w3
            cbnz    w0, G_M000_IG139
            mov     x1, xzr
            mov     w3, wzr

G_M000_IG65:                ;; offset=0x06A4
            cbz     x2, G_M000_IG66
            add     x0, x2, #16
            ldr     w2, [x2, #0x08]
            b       G_M000_IG67

G_M000_IG66:                ;; offset=0x06B4
            mov     x0, xzr
            mov     w2, wzr

G_M000_IG67:                ;; offset=0x06BC
            cmp     w3, w2
            bgt     G_M000_IG140
            ubfiz   x2, x3, #2, #32
            movz    x3, #0x56E0
            movk    x3, #0x6938 LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for System.SpanHelpers:Memmove(byref,byref,nuint)
            mov     w4, #2
            add     w1, w20, #1
            sub     w5, w27, #1
            tbnz    w5, #31, G_M000_IG144
            tbnz    w1, #31, G_M000_IG68
            cmp     w1, w5
            csel    w1, w1, w5, le
            b       G_M000_IG69

G_M000_IG68:                ;; offset=0x06FC
            mov     w1, wzr

G_M000_IG69:                ;; offset=0x0700
            lsl     w3, w1, #2

G_M000_IG70:                ;; offset=0x0704
            b       G_M000_IG84

G_M000_IG71:                ;; offset=0x0708
            ldr     x1, [fp, #-0xE8]
            cbnz    x1, G_M000_IG131

G_M000_IG72:                ;; offset=0x0710
            add     w1, w20, w4
            sub     w6, w27, #1
            tbnz    w6, #31, G_M000_IG145
            tbnz    w1, #31, G_M000_IG75

G_M000_IG73:                ;; offset=0x0720
            cmp     w1, w6
            bgt     G_M000_IG74
            b       G_M000_IG76

G_M000_IG74:                ;; offset=0x072C
            mov     w1, w6
            b       G_M000_IG76

G_M000_IG75:                ;; offset=0x0734
            mov     w1, wzr

G_M000_IG76:                ;; offset=0x0738
            lsl     w6, w1, #2
            str     w6, [fp, #-0xA4]
            cmp     w6, w3
            beq     G_M000_IG78

G_M000_IG77:                ;; offset=0x0748
            ldr     w5, [fp, #-0x90]
            sub     w1, w6, w5
            uxtb    w11, w22
            mov     w7, w11
            mov     w3, w7
            mov     x0, x19
            str     w4, [fp, #-0xA0]
            mov     w2, w4
            movz    x7, #0x48B8
            movk    x7, #0x695F LSL #16
            movk    x7, #0x7FF8 LSL #32
            ldr     x7, [x7]
            blr     x7      // code for DotJpegli.Internal.AdaptiveQuantization:ComputePreErosion(int,int,bool):this
            b       G_M000_IG83

G_M000_IG78:                ;; offset=0x0780
            ldr     x2, [x19, #0x20]
            mov     x0, x2
            sub     w1, w4, #1
            ldr     w3, [x19, #0x58]
            mul     w1, w1, w3
            mov     w7, w3
            cbz     x0, G_M000_IG79
            mov     w8, w1
            add     x8, x8, w7, UXTW
            ldr     w9, [x0, #0x08]
            cmp     x8, x9
            bhi     G_M000_IG139
            add     x0, x0, #16
            ubfiz   x1, x1, #2, #32
            add     x1, x0, x1
            b       G_M000_IG80

G_M000_IG79:                ;; offset=0x07C0
            orr     w0, w1, w7
            cbnz    w0, G_M000_IG139
            mov     x1, xzr
            mov     w7, wzr

G_M000_IG80:                ;; offset=0x07D0
            str     w4, [fp, #-0xA0]
            mul     w0, w4, w3
            cbz     x2, G_M000_IG81
            mov     w8, w0
            add     x8, x8, w3, UXTW
            ldr     w9, [x2, #0x08]
            cmp     x8, x9
            bhi     G_M000_IG139
            add     x2, x2, #16
            ubfiz   x0, x0, #2, #32
            add     x0, x2, x0
            b       G_M000_IG82

G_M000_IG81:                ;; offset=0x0800
            orr     w2, w0, w3
            cbnz    w2, G_M000_IG139
            mov     x0, xzr
            mov     w3, wzr

G_M000_IG82:                ;; offset=0x0810
            cmp     w7, w3
            bgt     G_M000_IG140
            ubfiz   x2, x7, #2, #32
            movz    x3, #0x56E0
            movk    x3, #0x6938 LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for System.SpanHelpers:Memmove(byref,byref,nuint)

G_M000_IG83:                ;; offset=0x0830
            ldp     w3, w4, [fp, #-0xA4]
            add     w4, w4, #1

G_M000_IG84:                ;; offset=0x0838
            ldr     w1, [x19, #0x48]
            lsl     w1, w1, #1
            add     w1, w1, #2
            cmp     w4, w1
            blt     G_M000_IG71

G_M000_IG85:                ;; offset=0x084C
            cmp     w24, #9
            bge     G_M000_IG87

G_M000_IG86:                ;; offset=0x0854
            fmov    s16, #1.0000
            b       G_M000_IG88

G_M000_IG87:                ;; offset=0x085C
            scvtf   s16, w24
            fmov    s17, #9.0000
            fsub    s16, s16, s17
            ldr     s17, [@RWD00]
            fdiv    s16, s16, s17
            fmov    s17, #1.0000
            fsub    s16, s17, s16
            movi    v17.2s, #0
            fmax    s16, s16, s17

G_M000_IG88:                ;; offset=0x0880
            ldr     s17, [@RWD04]
            fmul    s8, s16, s17
            fmov    s17, #1.0000
            fsub    s16, s17, s16
            ldr     s17, [@RWD08]
            fmul    s9, s16, s17
            uxtb    w7, w22
            mov     w20, w7
            cbz     w20, G_M000_IG90

G_M000_IG89:                ;; offset=0x08A4
            ldr     w0, [x19, #0x64]
            cmp     w0, #8
            bge     G_M000_IG91

G_M000_IG90:                ;; offset=0x08B0
            mov     w24, wzr
            b       G_M000_IG92

G_M000_IG91:                ;; offset=0x08B8
            ldrb    w24, [fp, #0x10]

G_M000_IG92:                ;; offset=0x08BC
            cbnz    w24, G_M000_IG94

G_M000_IG93:                ;; offset=0x08C0
            mov     x27, xzr
            mov     w2, wzr
            mov     w4, w2
            b       G_M000_IG95

G_M000_IG94:                ;; offset=0x08D0
            add     sp, sp, #16
            ldp     x0, xzr, [sp], #-0x200
            sub     sp, sp, #16
            add     x0, sp, #16
            str     x0, [fp, #-0xF0]
            mov     x1, #512
            bl      CORINFO_HELP_MEMZERO
            ldr     x0, [fp, #-0xF0]
            mov     x27, x0
            mov     w2, #16
            mov     w4, w2

G_M000_IG95:                ;; offset=0x08FC
            str     w4, [fp, #-0xD4]
            cbnz    w24, G_M000_IG97

G_M000_IG96:                ;; offset=0x0904
            mov     x5, xzr
            mov     w6, wzr
            b       G_M000_IG98

G_M000_IG97:                ;; offset=0x0910
            add     sp, sp, #16
            ldr     wzr, [sp], #-0x40
            sub     sp, sp, #16
            add     x5, sp, #16
            movi    v16.16b, #0
            stp     q16, q16, [x5]
            stp     q16, q16, [x5, #0x20]
            mov     w6, #16

G_M000_IG98:                ;; offset=0x0930
            movn    xip1, #279
            str     x5, [fp, xip1]
            str     w6, [fp, #-0xD8]
            mov     w7, wzr
            b       G_M000_IG114

G_M000_IG99:                ;; offset=0x0944
            ldr     x1, [fp, #-0xE8]
            add     x1, x1, #32
            ldapr   w1, [x1]
            cbz     w1, G_M000_IG116
            b       G_M000_IG136

G_M000_IG100:                ;; offset=0x0958
            ldr     s0, [@RWD12]
            fdiv    s13, s0, s1

G_M000_IG101:                ;; offset=0x0960
            fmov    s0, #1.0000
            fsub    s0, s13, s0
            movi    v16.2s, #0
            fmax    s0, s0, s16
            ldr     x4, [fp, #-0x100]
            str     s0, [x4]
            ldr     w3, [fp, #-0xB0]
            add     w3, w3, #1
            cmp     w3, #8
            mov     w2, w3
            bge     G_M000_IG124

G_M000_IG102:                ;; offset=0x098C
            ldr     w1, [fp, #-0xAC]
            str     w2, [fp, #-0xB0]
            add     w0, w1, w2
            lsl     w5, w0, #1
            ldr     x0, [x19, #0x28]
            mov     x6, x0
            ldr     w4, [x19, #0x58]
            add     w4, w4, w5
            mov     w7, w4
            ldr     w8, [x6, #0x08]
            cmp     w7, w8
            bhs     G_M000_IG147
            add     x6, x6, #16
            ldr     s0, [x6, w7, UXTW #2]
            mov     x6, x0
            add     w4, w4, #1
            cmp     w4, w8
            bhs     G_M000_IG147
            add     x6, x6, #16
            ldr     s16, [x6, w4, UXTW #2]
            fadd    s0, s0, s16
            mov     x4, x0
            cmp     w5, w8
            bhs     G_M000_IG147
            add     x4, x4, #16
            ldr     s16, [x4, w5, UXTW #2]
            fadd    s0, s0, s16
            add     w4, w5, #1
            cmp     w4, w8
            bhs     G_M000_IG147
            add     x0, x0, #16
            add     w4, w5, #1
            ldr     s16, [x0, w4, UXTW #2]
            fadd    s12, s0, s16
            ldrb    w0, [x19, #0x69]
            ldr     x1, [x19, #0x10]
            fmov    s0, s12
            movz    x4, #0x4900
            movk    x4, #0x695F LSL #16
            movk    x4, #0x7FF8 LSL #32
            ldr     x4, [x4]
            blr     x4      // code for DotJpegli.Internal.AdaptiveQuantization:ComputeMask(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float
            fmov    s12, s0
            ldr     w2, [fp, #-0xB0]
            ldr     w3, [fp, #-0xD8]
            cmp     w2, w3
            bhs     G_M000_IG147
            movn    xip1, #279
            ldr     x4, [fp, xip1]
            ldr     s0, [x4, w2, UXTW #2]
            fadd    s12, s0, s12
            add     w0, w2, #8
            cmp     w0, w3
            bhs     G_M000_IG147
            str     w2, [fp, #-0xB0]
            add     w0, w2, #8
            ldr     s0, [x4, w0, UXTW #2]
            ldrb    w0, [x19, #0x69]
            ldr     x1, [x19, #0x10]
            movz    x5, #0x4918
            movk    x5, #0x695F LSL #16
            movk    x5, #0x7FF8 LSL #32
            ldr     x5, [x5]
            blr     x5      // code for DotJpegli.Internal.AdaptiveQuantization:FastLog2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float
            fmul    s0, s0, s10
            fadd    s12, s0, s12
            fmul    s0, s12, s11
            ldrb    w0, [x19, #0x69]
            ldr     x1, [x19, #0x10]
            movz    x2, #0x4930
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.AdaptiveQuantization:FastPow2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float
            fmul    s0, s0, s8
            fadd    s1, s0, s9
            ldr     w0, [x19, #0x64]
            ldp     w1, w2, [fp, #-0xAC]
            madd    w0, w2, w0, w1
            ldr     w3, [fp, #-0xB0]
            add     w4, w0, w3
            cmp     w4, w26
            bhs     G_M000_IG147
            ubfiz   x0, x4, #2, #32
            add     x5, x25, x0
            str     x5, [fp, #-0x100]
            ldrb    w6, [x19, #0x69]
            ldr     x0, [x19, #0x10]
            movn    xip1, #263
            str     x0, [fp, xip1]
            strb    wzr, [fp, #-0xC8]
            cbz     w6, G_M000_IG100

G_M000_IG103:                ;; offset=0x0AFC
            ldr     s0, [@RWD12]
            sub     x0, fp, #200
            movz    x6, #0x4948
            movk    x6, #0x695F LSL #16
            movk    x6, #0x7FF8 LSL #32
            ldr     x6, [x6]
            blr     x6      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            fmov    s13, s0
            movn    xip1, #263
            ldr     x0, [fp, xip1]
            cbz     x0, G_M000_IG104
            ldrb    w2, [fp, #-0xC8]
            mov     w1, #4
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG104:                ;; offset=0x0B44
            b       G_M000_IG101

G_M000_IG105:                ;; offset=0x0B48
            ldr     w5, [fp, #-0xA8]
            lsl     w3, w5, #3
            add     w3, w3, #5
            ldr     w2, [x19, #0x54]
            ldr     w6, [fp, #-0xAC]
            lsl     w4, w6, #3
            madd    w3, w3, w2, w4
            add     w3, w3, #1
            ldr     x2, [x19, #0x18]
            cbz     x2, G_M000_IG106
            ldr     w4, [x2, #0x08]
            cmp     w4, w3
            blo     G_M000_IG139
            add     x4, x2, #16
            ubfiz   x0, x3, #2, #32
            add     x0, x4, x0
            ldr     w2, [x2, #0x08]
            sub     w1, w2, w3
            b       G_M000_IG107

G_M000_IG106:                ;; offset=0x0B94
            cbnz    w3, G_M000_IG139
            mov     x0, xzr
            mov     w1, wzr

G_M000_IG107:                ;; offset=0x0BA0
            ldrb    w3, [x19, #0x69]
            ldr     w2, [x19, #0x54]
            ldr     x4, [x19, #0x10]
            movz    x7, #0x4990
            movk    x7, #0x695F LSL #16
            movk    x7, #0x7FF8 LSL #32
            ldr     x7, [x7]
            blr     x7      // code for DotJpegli.Internal.AdaptiveQuantization:ModulateScalar(System.ReadOnlySpan`1[float],int,float,bool,DotJpegli.Internal.AqRationalDiagnostics):float

G_M000_IG108:                ;; offset=0x0BC0
            ldr     s11, [@RWD16]
            fmul    s0, s0, s11
            ldrb    w0, [x19, #0x69]
            ldr     x1, [x19, #0x10]
            movz    x2, #0x4930
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.AdaptiveQuantization:FastPow2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float
            fmul    s0, s0, s8
            fadd    s1, s0, s9
            ldr     w0, [x19, #0x64]
            ldp     w1, w2, [fp, #-0xAC]
            madd    w0, w2, w0, w1
            cmp     w0, w26
            bhs     G_M000_IG147
            ubfiz   x0, x0, #2, #32
            add     x3, x25, x0
            str     x3, [fp, #-0xF8]
            ldrb    w0, [x19, #0x69]
            ldr     x4, [x19, #0x10]
            movn    xip1, #271
            str     x4, [fp, xip1]
            strb    wzr, [fp, #-0xD0]
            cbz     w0, G_M000_IG111

G_M000_IG109:                ;; offset=0x0C24
            ldr     s0, [@RWD12]
            sub     x0, fp, #208
            movz    x5, #0x4948
            movk    x5, #0x695F LSL #16
            movk    x5, #0x7FF8 LSL #32
            ldr     x5, [x5]
            blr     x5      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            fmov    s10, s0
            movn    xip1, #271
            ldr     x0, [fp, xip1]
            cbz     x0, G_M000_IG110
            ldrb    w2, [fp, #-0xD0]
            mov     w1, #4
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG110:                ;; offset=0x0C6C
            b       G_M000_IG112

G_M000_IG111:                ;; offset=0x0C70
            ldr     s16, [@RWD12]
            fdiv    s10, s16, s1

G_M000_IG112:                ;; offset=0x0C78
            fmov    s16, #1.0000
            fsub    s16, s10, s16
            movi    v17.2s, #0
            fmax    s16, s16, s17
            ldr     x3, [fp, #-0xF8]
            str     s16, [x3]
            ldr     w1, [fp, #-0xAC]
            add     w1, w1, #1
            ldr     w0, [x19, #0x64]
            cmp     w1, w0
            mov     w7, w1
            blt     G_M000_IG127

G_M000_IG113:                ;; offset=0x0CA8
            ldr     w2, [fp, #-0xA8]
            add     w2, w2, #1
            mov     w7, w2

G_M000_IG114:                ;; offset=0x0CB4
            ldr     w1, [x19, #0x48]
            cmp     w7, w1
            bge     G_M000_IG129

G_M000_IG115:                ;; offset=0x0CC0
            ldr     x1, [fp, #-0xE8]
            cbnz    x1, G_M000_IG99

G_M000_IG116:                ;; offset=0x0CC8
            str     w7, [fp, #-0xA8]
            lsl     w1, w7, #1
            add     w1, w1, #1
            mov     w3, w20
            mov     x0, x19
            mov     w2, wzr
            movz    x8, #0x48D0
            movk    x8, #0x695F LSL #16
            movk    x8, #0x7FF8 LSL #32
            ldr     x8, [x8]
            blr     x8      // code for DotJpegli.Internal.AdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this
            ldr     w4, [fp, #-0xA8]
            lsl     w1, w4, #1
            add     w1, w1, #2
            ldr     w2, [x19, #0x58]
            mov     w3, w20
            mov     x0, x19
            movz    x5, #0x48D0
            movk    x5, #0x695F LSL #16
            movk    x5, #0x7FF8 LSL #32
            ldr     x5, [x5]
            blr     x5      // code for DotJpegli.Internal.AdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this
            mov     w7, wzr
            cbz     w24, G_M000_IG126

G_M000_IG117:                ;; offset=0x0D28
            ldr     s10, [@RWD20]
            ldr     s11, [@RWD16]
            b       G_M000_IG125

G_M000_IG118:                ;; offset=0x0D34
            ldr     w8, [fp, #-0xA8]
            lsl     w2, w8, #3
            add     w2, w2, #5
            movn    xip1, #279
            ldr     x5, [fp, xip1]
            ldr     w6, [fp, #-0xD8]
            mov     x3, x27
            ldr     w4, [fp, #-0xD4]
            str     w7, [fp, #-0xAC]
            lsl     w1, w7, #3
            mov     x0, x19
            movz    x9, #0x48E8
            movk    x9, #0x695F LSL #16
            movk    x9, #0x7FF8 LSL #32
            ldr     x9, [x9]
            blr     x9      // code for DotJpegli.Internal.AdaptiveQuantization:ModulateEightBlocks(int,int,System.Span`1[System.Runtime.Intrinsics.Vector256`1[float]],System.Span`1[float]):this
            mov     w2, wzr
            ldr     w3, [fp, #-0xD8]
            cmp     w3, #8
            blt     G_M000_IG102

G_M000_IG119:                ;; offset=0x0D84
            ldr     w4, [fp, #-0xAC]
            str     w2, [fp, #-0xB0]
            add     w0, w4, w2
            lsl     w5, w0, #1
            ldr     x6, [x19, #0x28]
            ldr     w0, [x19, #0x58]
            add     w0, w0, w5
            mov     w7, w0
            ldr     w1, [x6, #0x08]
            cmp     w7, w1
            bhs     G_M000_IG147
            add     x6, x6, #16
            ldr     s0, [x6, w7, UXTW #2]
            add     w8, w0, #1
            cmp     w8, w1
            bhs     G_M000_IG147
            ldr     s16, [x6, w8, UXTW #2]
            fadd    s0, s0, s16
            cmp     w5, w1
            bhs     G_M000_IG147
            ldr     s16, [x6, w5, UXTW #2]
            fadd    s0, s0, s16
            add     w0, w5, #1
            cmp     w0, w1
            bhs     G_M000_IG147
            ldr     s16, [x6, w0, UXTW #2]
            fadd    s12, s0, s16
            ldrb    w0, [x19, #0x69]
            ldr     x1, [x19, #0x10]
            fmov    s0, s12
            movz    x5, #0x4900
            movk    x5, #0x695F LSL #16
            movk    x5, #0x7FF8 LSL #32
            ldr     x5, [x5]
            blr     x5      // code for DotJpegli.Internal.AdaptiveQuantization:ComputeMask(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float
            fmov    s12, s0
            movn    xip1, #279
            ldr     x2, [fp, xip1]
            ldr     w3, [fp, #-0xB0]
            ldr     s0, [x2, w3, UXTW #2]
            fadd    s12, s0, s12
            str     w3, [fp, #-0xB0]
            add     w0, w3, #8
            ldr     w4, [fp, #-0xD8]
            cmp     w0, w4
            bhs     G_M000_IG147
            ldr     s0, [x2, w0, UXTW #2]
            ldrb    w0, [x19, #0x69]
            ldr     x1, [x19, #0x10]
            movz    x5, #0x4918
            movk    x5, #0x695F LSL #16
            movk    x5, #0x7FF8 LSL #32
            ldr     x5, [x5]
            blr     x5      // code for DotJpegli.Internal.AdaptiveQuantization:FastLog2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float
            fmul    s0, s0, s10
            fadd    s12, s0, s12
            fmul    s0, s12, s11
            ldrb    w0, [x19, #0x69]
            ldr     x1, [x19, #0x10]
            movz    x2, #0x4930
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.AdaptiveQuantization:FastPow2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float
            fmul    s0, s0, s8
            fadd    s1, s0, s9
            ldr     w0, [x19, #0x64]
            ldp     w1, w2, [fp, #-0xAC]
            madd    w0, w2, w0, w1
            ldr     w3, [fp, #-0xB0]
            add     w4, w0, w3
            cmp     w4, w26
            bhs     G_M000_IG147
            ubfiz   x0, x4, #2, #32
            add     x5, x25, x0
            str     x5, [fp, #-0x100]
            ldrb    w6, [x19, #0x69]
            ldr     x7, [x19, #0x10]
            movn    xip1, #263
            str     x7, [fp, xip1]
            strb    wzr, [fp, #-0xC8]
            cbz     w6, G_M000_IG122

G_M000_IG120:                ;; offset=0x0ECC
            ldr     s0, [@RWD12]
            sub     x0, fp, #200
            movz    x4, #0x4948
            movk    x4, #0x695F LSL #16
            movk    x4, #0x7FF8 LSL #32
            ldr     x4, [x4]
            blr     x4      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            fmov    s13, s0
            movn    xip1, #263
            ldr     x3, [fp, xip1]
            cbz     x3, G_M000_IG121
            ldrb    w2, [fp, #-0xC8]
            mov     x0, x3
            mov     w1, #4
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG121:                ;; offset=0x0F18
            b       G_M000_IG123

G_M000_IG122:                ;; offset=0x0F1C
            ldr     s0, [@RWD12]
            fdiv    s13, s0, s1

G_M000_IG123:                ;; offset=0x0F24
            fmov    s0, #1.0000
            fsub    s0, s13, s0
            movi    v16.2s, #0
            fmax    s0, s0, s16
            ldr     x5, [fp, #-0x100]
            str     s0, [x5]
            ldr     w3, [fp, #-0xB0]
            add     w3, w3, #1
            cmp     w3, #8
            mov     w2, w3
            blt     G_M000_IG119

G_M000_IG124:                ;; offset=0x0F50
            ldr     w1, [fp, #-0xAC]
            add     w1, w1, #8
            mov     w7, w1

G_M000_IG125:                ;; offset=0x0F5C
            ldr     w2, [x19, #0x64]
            sub     w2, w2, #8
            cmp     w7, w2
            ble     G_M000_IG118

G_M000_IG126:                ;; offset=0x0F6C
            ldr     w0, [x19, #0x64]
            cmp     w7, w0
            bge     G_M000_IG113

G_M000_IG127:                ;; offset=0x0F78
            str     w7, [fp, #-0xAC]
            lsl     w0, w7, #1
            ldr     x1, [x19, #0x28]
            ldr     w3, [x19, #0x58]
            add     w3, w3, w0
            ldr     w4, [x1, #0x08]
            cmp     w3, w4
            bhs     G_M000_IG147
            add     x1, x1, #16
            ldr     s0, [x1, w3, UXTW #2]
            ldr     x1, [x19, #0x28]
            ldr     w3, [x19, #0x58]
            add     w3, w3, w0
            add     w3, w3, #1
            ldr     w4, [x1, #0x08]
            cmp     w3, w4
            bhs     G_M000_IG147
            add     x1, x1, #16
            ldr     s16, [x1, w3, UXTW #2]
            fadd    s0, s0, s16
            ldr     x1, [x19, #0x28]
            ldr     w3, [x1, #0x08]
            cmp     w0, w3
            bhs     G_M000_IG147
            add     x1, x1, #16
            ldr     s16, [x1, w0, UXTW #2]
            fadd    s0, s0, s16
            ldr     x1, [x19, #0x28]
            add     w3, w0, #1
            ldr     w4, [x1, #0x08]
            cmp     w3, w4
            bhs     G_M000_IG147
            add     x1, x1, #16
            add     w0, w0, #1
            ldr     s16, [x1, w0, UXTW #2]
            fadd    s0, s0, s16
            ldrb    w0, [x19, #0x69]
            ldr     x1, [x19, #0x10]
            movz    x3, #0x4900
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AdaptiveQuantization:ComputeMask(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float
            cbz     w20, G_M000_IG105

G_M000_IG128:                ;; offset=0x1028
            ldr     w3, [fp, #-0xA8]
            lsl     w2, w3, #3
            add     w2, w2, #5
            ldr     w4, [fp, #-0xAC]
            lsl     w1, w4, #3
            mov     x0, x19
            movz    x5, #0x4978
            movk    x5, #0x695F LSL #16
            movk    x5, #0x7FF8 LSL #32
            ldr     x5, [x5]
            blr     x5      // code for DotJpegli.Internal.AdaptiveQuantization:ModulateVector(int,int,float):float:this
            b       G_M000_IG108

G_M000_IG129:                ;; offset=0x1058
            add     x14, x19, #56
            mov     x15, x28
            bl      CORINFO_HELP_ASSIGN_REF
            stp     w21, w23, [x19, #0x5C]
            strb    w22, [x19, #0x6B]
            movz    xip0, #0x2D5A
            movk    xip0, #0xF7B0 LSL #16
            movk    xip0, #0x2883 LSL #32
            ldr     xip1, [fp, #-0x88]
            cmp     xip0, xip1
            beq     G_M000_IG130
            bl      CORINFO_HELP_FAIL_FAST

G_M000_IG130:                ;; offset=0x1088
            sub     sp, fp, #304
            ldp     fp, lr, [sp, #0x130]
            ldp     x27, x28, [sp, #0x120]
            ldp     x25, x26, [sp, #0x110]
            ldp     x23, x24, [sp, #0x100]
            ldp     x21, x22, [sp, #0xF0]
            ldp     x19, x20, [sp, #0xE0]
            ldp     d12, d13, [sp, #0xD0]
            ldp     d10, d11, [sp, #0xC0]
            ldp     d8, d9, [sp, #0xB0]
            add     sp, sp, #320
            ret     lr

G_M000_IG131:                ;; offset=0x10B8
            ldr     x1, [fp, #-0xE8]
            add     x1, x1, #32
            ldapr   w1, [x1]
            cbz     w1, G_M000_IG72
            b       G_M000_IG136

G_M000_IG132:                ;; offset=0x10CC
            ldr     x2, [fp, #-0xE8]
            add     x2, x2, #32
            ldapr   w2, [x2]
            cbz     w2, G_M000_IG07
            b       G_M000_IG136

G_M000_IG133:                ;; offset=0x10E0
            movn    w7, #0
            b       G_M000_IG14

G_M000_IG134:                ;; offset=0x10E8
            mov     w6, w4
            add     x6, x6, w1, UXTW
            ldr     w7, [x0, #0x08]
            cmp     x6, x7
            bhi     G_M000_IG139
            add     x0, x0, #16
            ubfiz   x4, x4, #2, #32
            add     x4, x0, x4
            b       G_M000_IG09

G_M000_IG135:                ;; offset=0x110C
            mov     x0, x19
            movz    x1, #0x4408
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for System.ThrowHelper:ThrowObjectDisposedException(System.Object)
            brk     #0

G_M000_IG136:                ;; offset=0x1128
            sub     x0, fp, #232
            movz    x1, #0x4420
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for System.Threading.CancellationToken:ThrowOperationCanceledException():this
            brk     #0

G_M000_IG137:                ;; offset=0x1144
            movz    x0, #0xDCE0
            movk    x0, #0x6947 LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x62C8
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0xFF00
            movk    x2, #0x6950 LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.ArgumentOutOfRangeException:.ctor(System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG138:                ;; offset=0x1184
            movz    x0, #0xF360
            movk    x0, #0x693D LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x6208
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0x62A0
            movk    x2, #0xA15 LSL #16
            movk    x2, #548 LSL #32
            movz    x3, #0x4438
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for System.ArgumentException:.ctor(System.String,System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG139:                ;; offset=0x11D0
            movz    x0, #0x4450
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentOutOfRangeException()
            brk     #0

G_M000_IG140:                ;; offset=0x11E8
            movz    x0, #0x4468
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentException_DestinationTooShort()
            brk     #0

G_M000_IG141:                ;; offset=0x1200
            mov     w1, w5
            mov     w0, wzr
            movz    x2, #0x42D0
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.Math:ThrowMinMaxException[int](int,int)
            brk     #0

G_M000_IG142:                ;; offset=0x1220
            mov     w1, w8
            mov     w0, wzr
            movz    x2, #0x42D0
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.Math:ThrowMinMaxException[int](int,int)
            brk     #0

G_M000_IG143:                ;; offset=0x1240
            mov     w0, wzr
            movz    x2, #0x42D0
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.Math:ThrowMinMaxException[int](int,int)
            brk     #0

G_M000_IG144:                ;; offset=0x125C
            mov     w1, w5
            mov     w0, wzr
            movz    x2, #0x42D0
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.Math:ThrowMinMaxException[int](int,int)
            brk     #0

G_M000_IG145:                ;; offset=0x127C
            mov     w1, w6
            mov     w0, wzr
            movz    x2, #0x42D0
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for System.Math:ThrowMinMaxException[int](int,int)
            brk     #0

G_M000_IG146:                ;; offset=0x129C
            bl      CORINFO_HELP_THROWDIVZERO
            brk     #0

G_M000_IG147:                ;; offset=0x12A4
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dd	42600000h		;        56
RWD04  	dd	3F574BC7h		;     0.841
RWD08  	dd	3ECEAF25h		;   0.40368
RWD12  	dd	3F19999Ah		;       0.6
RWD16  	dd	3FB8AA3Bh		;    1.4427
RWD20  	dd	BDDC6A08h		; -0.107624

; Total bytes of code 4780

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ComputePreErosion(int,int,bool):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 56 single block inlinees; 42 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x180]!
            stp     d8, d9, [sp, #0x100]
            stp     d10, d11, [sp, #0x110]
            stp     d12, d13, [sp, #0x120]
            stp     d14, d15, [sp, #0x130]
            stp     x19, x20, [sp, #0x140]
            stp     x21, x22, [sp, #0x150]
            stp     x23, x24, [sp, #0x160]
            stp     x25, x26, [sp, #0x170]
            mov     fp, sp
            add     x9, fp, #32
            movi    v16.16b, #0
            stp     q16, q16, [x9]
            stp     q16, q16, [x9, #0x20]
            stp     q16, q16, [x9, #0x40]
            stp     q16, q16, [x9, #0x60]
            stp     q16, q16, [x9, #0x80]
            stp     xzr, xzr, [x9, #0xA0]
            mov     x19, x0

G_M000_IG02:                ;; offset=0x004C
            ldr     s8, [@RWD00]
            ldp     w0, w4, [x19, #0x54]
            mul     w1, w1, w0
            add     w20, w1, #1
            mul     w21, w2, w4
            mov     w22, wzr
            tst     w3, #255
            beq     G_M000_IG58

G_M000_IG03:                ;; offset=0x006C
            ldr     w1, [x19, #0x4C]
            sub     w1, w1, #4
            tbnz    w1, #31, G_M000_IG58
            b       G_M000_IG15

G_M000_IG04:                ;; offset=0x007C
            fdiv    v0.4s, v0.4s, v16.4s

G_M000_IG05:                ;; offset=0x0080
            fsub    v1.4s, v10.4s, v9.4s
            fmul    v0.4s, v0.4s, v1.4s
            fmul    v0.4s, v0.4s, v0.4s
            ldr     q1, [@RWD04]
            fmin    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD20]
            fmul    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD36]
            fadd    v0.4s, v0.4s, v1.4s
            fsqrt   v0.4s, v0.4s
            ldr     q1, [@RWD52]
            fmul    v9.4s, v0.4s, v1.4s
            ldr     x1, [x19, #0x18]
            ldr     w0, [x19, #0x54]
            add     w2, w23, w0,  LSL #1
            tbnz    w2, #31, G_M000_IG60
            ldr     w3, [x1, #0x08]
            sub     w4, w3, w2
            cmp     w4, #4
            blt     G_M000_IG60
            cmp     w2, w3
            bhs     G_M000_IG61
            add     x1, x1, #16
            ubfiz   x2, x2, #2, #32
            ldr     q0, [x1, x2]
            mov     v14.16b, v0.16b
            add     w0, w23, w0
            sub     w2, w0, #1
            sub     w4, w3, w2
            cmp     w4, #4
            ccmp    w2, #0, nc, ge
            blt     G_M000_IG60
            cmp     w2, w3
            bhs     G_M000_IG61
            ubfiz   x2, x2, #2, #32
            ldr     q0, [x1, x2]
            add     w0, w0, #1
            sub     w2, w3, w0
            cmp     w2, #4
            ccmp    w0, #0, nc, ge
            blt     G_M000_IG60
            cmp     w0, w3
            bhs     G_M000_IG61
            ubfiz   x0, x0, #2, #32
            ldr     q1, [x1, x0]
            fadd    v0.4s, v1.4s, v0.4s
            fadd    v1.4s, v10.4s, v14.4s
            fadd    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD68]
            fmul    v10.4s, v0.4s, v1.4s
            str     q10, [fp, #0xE0]
            ldr     q0, [@RWD84]
            fadd    v0.4s, v11.4s, v0.4s
            ldrb    w1, [x19, #0x69]
            ldr     x0, [x19, #0x10]
            movi    v1.4s, #0
            fmax    v0.4s, v0.4s, v1.4s
            fmul    v1.4s, v0.4s, v0.4s
            ldr     q16, [@RWD100]
            fmul    v16.4s, v1.4s, v16.4s
            ldr     q17, [@RWD116]
            fadd    v16.4s, v16.4s, v17.4s
            ldr     q17, [@RWD132]
            fmul    v0.4s, v0.4s, v17.4s
            fmul    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD148]
            fadd    v0.4s, v0.4s, v1.4s
            cbz     w1, G_M000_IG07

G_M000_IG06:                ;; offset=0x0190
            mov     v1.16b, v16.16b
            mov     x1, x0
            mov     w0, #1
            movz    x2, #0x49A8
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            mov     v12.d[0], v11.d[1]
            mov     v13.d[0], v14.d[1]
            mov     v15.d[0], v9.d[1]
            blr     x2      // code for DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Numerics.Vector`1[float],System.Numerics.Vector`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Numerics.Vector`1[float]
            mov     v11.d[1], v12.d[0]
            mov     v14.d[1], v13.d[0]
            mov     v9.d[1], v15.d[0]
            b       G_M000_IG08

G_M000_IG07:                ;; offset=0x01CC
            fdiv    v0.4s, v0.4s, v16.4s

G_M000_IG08:                ;; offset=0x01D0
            ldr     q10, [fp, #0xE0]
            fsub    v1.4s, v11.4s, v10.4s
            fmul    v0.4s, v0.4s, v1.4s
            fmul    v0.4s, v0.4s, v0.4s
            ldr     q1, [@RWD164]
            fmin    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD180]
            fmul    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD196]
            fadd    v0.4s, v0.4s, v1.4s
            fsqrt   v0.4s, v0.4s
            ldr     q1, [@RWD212]
            fmul    v0.4s, v0.4s, v1.4s
            fadd    v9.4s, v9.4s, v0.4s
            mov     v10.16b, v11.16b
            mov     v11.16b, v14.16b
            ldr     x1, [x19, #0x18]
            ldr     w0, [x19, #0x54]
            mov     w2, #3
            madd    w2, w0, w2, w23
            tbnz    w2, #31, G_M000_IG60
            ldr     w3, [x1, #0x08]
            sub     w4, w3, w2
            cmp     w4, #4
            blt     G_M000_IG60
            cmp     w2, w3
            bhs     G_M000_IG61
            add     x1, x1, #16
            ubfiz   x2, x2, #2, #32
            ldr     q14, [x1, x2]
            add     w0, w23, w0,  LSL #1
            sub     w2, w0, #1
            sub     w4, w3, w2
            cmp     w4, #4
            ccmp    w2, #0, nc, ge
            blt     G_M000_IG60
            cmp     w2, w3
            bhs     G_M000_IG61
            ubfiz   x2, x2, #2, #32
            ldr     q0, [x1, x2]
            add     w0, w0, #1
            sub     w2, w3, w0
            cmp     w2, #4
            ccmp    w0, #0, nc, ge
            blt     G_M000_IG60
            cmp     w0, w3
            bhs     G_M000_IG61
            ubfiz   x0, x0, #2, #32
            ldr     q1, [x1, x0]
            fadd    v0.4s, v1.4s, v0.4s
            fadd    v1.4s, v10.4s, v14.4s
            fadd    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD228]
            fmul    v10.4s, v0.4s, v1.4s
            str     q10, [fp, #0xD0]
            ldr     q0, [@RWD244]
            fadd    v0.4s, v11.4s, v0.4s
            ldrb    w1, [x19, #0x69]
            ldr     x0, [x19, #0x10]
            movi    v1.4s, #0
            fmax    v0.4s, v0.4s, v1.4s
            fmul    v1.4s, v0.4s, v0.4s
            ldr     q16, [@RWD260]
            fmul    v16.4s, v1.4s, v16.4s
            ldr     q17, [@RWD276]
            fadd    v16.4s, v16.4s, v17.4s
            ldr     q17, [@RWD292]
            fmul    v0.4s, v0.4s, v17.4s
            fmul    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD308]
            fadd    v0.4s, v0.4s, v1.4s
            cbz     w1, G_M000_IG10

G_M000_IG09:                ;; offset=0x02F0
            mov     v1.16b, v16.16b
            mov     x1, x0
            mov     w0, #1
            movz    x2, #0x49A8
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            mov     v12.d[0], v11.d[1]
            mov     v13.d[0], v14.d[1]
            mov     v15.d[0], v9.d[1]
            blr     x2      // code for DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Numerics.Vector`1[float],System.Numerics.Vector`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Numerics.Vector`1[float]
            mov     v11.d[1], v12.d[0]
            mov     v14.d[1], v13.d[0]
            mov     v9.d[1], v15.d[0]
            b       G_M000_IG11

G_M000_IG10:                ;; offset=0x032C
            fdiv    v0.4s, v0.4s, v16.4s

G_M000_IG11:                ;; offset=0x0330
            ldr     q10, [fp, #0xD0]
            fsub    v1.4s, v11.4s, v10.4s
            fmul    v0.4s, v0.4s, v1.4s
            fmul    v0.4s, v0.4s, v0.4s
            ldr     q1, [@RWD324]
            fmin    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD340]
            fmul    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD356]
            fadd    v0.4s, v0.4s, v1.4s
            fsqrt   v0.4s, v0.4s
            ldr     q1, [@RWD372]
            fmul    v0.4s, v0.4s, v1.4s
            fadd    v9.4s, v9.4s, v0.4s
            mov     v10.16b, v11.16b
            mov     v11.16b, v14.16b
            ldr     x1, [x19, #0x18]
            ldr     w0, [x19, #0x54]
            add     w2, w23, w0,  LSL #2
            tbnz    w2, #31, G_M000_IG60
            ldr     w3, [x1, #0x08]
            sub     w4, w3, w2
            cmp     w4, #4
            blt     G_M000_IG60
            cmp     w2, w3
            bhs     G_M000_IG61
            add     x1, x1, #16
            ubfiz   x2, x2, #2, #32
            ldr     q14, [x1, x2]
            mov     w2, #3
            madd    w0, w0, w2, w23
            sub     w2, w0, #1
            sub     w4, w3, w2
            cmp     w4, #4
            ccmp    w2, #0, nc, ge
            blt     G_M000_IG60
            cmp     w2, w3
            bhs     G_M000_IG61
            ubfiz   x2, x2, #2, #32
            ldr     q0, [x1, x2]
            add     w0, w0, #1
            sub     w2, w3, w0
            cmp     w2, #4
            ccmp    w0, #0, nc, ge
            blt     G_M000_IG60
            cmp     w0, w3
            bhs     G_M000_IG61
            ubfiz   x0, x0, #2, #32
            ldr     q1, [x1, x0]
            fadd    v0.4s, v1.4s, v0.4s
            fadd    v1.4s, v10.4s, v14.4s
            fadd    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD388]
            fmul    v10.4s, v0.4s, v1.4s
            ldr     q0, [@RWD404]
            fadd    v0.4s, v11.4s, v0.4s
            ldrb    w1, [x19, #0x69]
            ldr     x0, [x19, #0x10]
            movi    v1.4s, #0
            fmax    v0.4s, v0.4s, v1.4s
            fmul    v1.4s, v0.4s, v0.4s
            ldr     q16, [@RWD420]
            fmul    v16.4s, v1.4s, v16.4s
            ldr     q17, [@RWD436]
            fadd    v16.4s, v16.4s, v17.4s
            ldr     q17, [@RWD452]
            fmul    v0.4s, v0.4s, v17.4s
            fmul    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD468]
            fadd    v0.4s, v0.4s, v1.4s
            cbz     w1, G_M000_IG13

G_M000_IG12:                ;; offset=0x044C
            mov     v1.16b, v16.16b
            mov     x1, x0
            mov     w0, #1
            movz    x2, #0x49A8
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            mov     v12.d[0], v11.d[1]
            mov     v15.d[0], v9.d[1]
            mov     v13.d[0], v10.d[1]
            blr     x2      // code for DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Numerics.Vector`1[float],System.Numerics.Vector`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Numerics.Vector`1[float]
            mov     v11.d[1], v12.d[0]
            mov     v9.d[1], v15.d[0]
            mov     v10.d[1], v13.d[0]
            b       G_M000_IG14

G_M000_IG13:                ;; offset=0x0488
            fdiv    v0.4s, v0.4s, v16.4s

G_M000_IG14:                ;; offset=0x048C
            fsub    v1.4s, v11.4s, v10.4s
            fmul    v0.4s, v0.4s, v1.4s
            fmul    v0.4s, v0.4s, v0.4s
            ldr     q1, [@RWD484]
            fmin    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD500]
            fmul    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD516]
            fadd    v0.4s, v0.4s, v1.4s
            fsqrt   v0.4s, v0.4s
            ldr     q1, [@RWD532]
            fmul    v0.4s, v0.4s, v1.4s
            fadd    v9.4s, v9.4s, v0.4s
            ldr     x0, [x19, #0x20]
            dup     s0, v9.s[0]
            dup     s1, v9.s[1]
            fadd    s0, s1, s0
            dup     s1, v9.s[2]
            fadd    s0, s1, s0
            dup     s1, v9.s[3]
            fadd    s0, s1, s0
            fmov    s1, #0.2500
            fmul    s0, s0, s1
            asr     w1, w22, #31
            and     w1, w1, #3
            add     w1, w1, w22
            add     w1, w21, w1,  ASR #2
            ldr     w2, [x0, #0x08]
            cmp     w1, w2
            bhs     G_M000_IG61
            add     x0, x0, #16
            str     s0, [x0, w1, UXTW #2]
            add     w22, w22, #4
            ldr     w0, [x19, #0x4C]
            sub     w0, w0, #4
            cmp     w22, w0
            bgt     G_M000_IG58

G_M000_IG15:                ;; offset=0x0520
            add     w23, w20, w22
            ldr     x1, [x19, #0x18]
            ldr     w0, [x19, #0x54]
            sub     w2, w23, w0
            tbnz    w2, #31, G_M000_IG60
            ldr     w3, [x1, #0x08]
            sub     w4, w3, w2
            cmp     w4, #4
            blt     G_M000_IG60
            cmp     w2, w3
            bhs     G_M000_IG61
            add     x1, x1, #16
            ubfiz   x2, x2, #2, #32
            ldr     q9, [x1, x2]
            sub     w2, w3, w23
            cmp     w2, #4
            ccmp    w23, #0, nc, ge
            blt     G_M000_IG60
            cmp     w23, w3
            bhs     G_M000_IG61
            ubfiz   x2, x23, #2, #32
            ldr     q10, [x1, x2]
            add     w0, w23, w0
            sub     w2, w3, w0
            cmp     w2, #4
            ccmp    w0, #0, nc, ge
            blt     G_M000_IG60
            cmp     w0, w3
            bhs     G_M000_IG61
            ubfiz   x0, x0, #2, #32
            ldr     q11, [x1, x0]
            sub     w0, w23, #1
            sub     w2, w3, w0
            cmp     w2, #4
            ccmp    w0, #0, nc, ge
            blt     G_M000_IG60
            cmp     w0, w3
            bhs     G_M000_IG61
            ubfiz   x0, x0, #2, #32
            ldr     q0, [x1, x0]
            add     w0, w23, #1
            sub     w2, w3, w0
            cmp     w2, #4
            blt     G_M000_IG60
            cmp     w0, w3
            bhs     G_M000_IG61
            ubfiz   x0, x0, #2, #32
            ldr     q1, [x1, x0]
            fadd    v0.4s, v1.4s, v0.4s
            fadd    v1.4s, v9.4s, v11.4s
            fadd    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD548]
            fmul    v9.4s, v0.4s, v1.4s
            ldr     q0, [@RWD564]
            fadd    v0.4s, v10.4s, v0.4s
            ldrb    w1, [x19, #0x69]
            ldr     x0, [x19, #0x10]
            movi    v1.4s, #0
            fmax    v0.4s, v0.4s, v1.4s
            fmul    v1.4s, v0.4s, v0.4s
            ldr     q16, [@RWD580]
            fmul    v16.4s, v1.4s, v16.4s
            ldr     q17, [@RWD596]
            fadd    v16.4s, v16.4s, v17.4s
            ldr     q17, [@RWD612]
            fmul    v0.4s, v0.4s, v17.4s
            fmul    v0.4s, v0.4s, v1.4s
            ldr     q1, [@RWD628]
            fadd    v0.4s, v0.4s, v1.4s
            cbz     w1, G_M000_IG04

G_M000_IG16:                ;; offset=0x0638
            mov     v1.16b, v16.16b
            mov     x1, x0
            mov     w0, #1
            movz    x2, #0x49A8
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            mov     v12.d[0], v10.d[1]
            mov     v13.d[0], v11.d[1]
            mov     v14.d[0], v9.d[1]
            blr     x2      // code for DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Numerics.Vector`1[float],System.Numerics.Vector`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Numerics.Vector`1[float]
            mov     v10.d[1], v12.d[0]
            mov     v11.d[1], v13.d[0]
            mov     v9.d[1], v14.d[0]
            b       G_M000_IG05

G_M000_IG17:                ;; offset=0x0674
            add     w23, w20, w22
            ldr     x0, [x19, #0x18]
            ldr     w1, [x0, #0x08]
            cmp     w23, w1
            bhs     G_M000_IG61
            add     x0, x0, #16
            ldr     s9, [x0, w23, UXTW #2]
            sub     w2, w23, #1
            cmp     w2, w1
            bhs     G_M000_IG61
            sub     w2, w23, #1
            ldr     s0, [x0, w2, UXTW #2]
            add     w2, w23, #1
            cmp     w2, w1
            bhs     G_M000_IG61
            add     w2, w23, #1
            ldr     s1, [x0, w2, UXTW #2]
            fadd    s0, s1, s0
            ldr     w2, [x19, #0x54]
            sub     w3, w23, w2
            cmp     w3, w1
            bhs     G_M000_IG61
            ldr     s1, [x0, w3, UXTW #2]
            add     w2, w23, w2
            cmp     w2, w1
            bhs     G_M000_IG61
            ldr     s16, [x0, w2, UXTW #2]
            fadd    s1, s1, s16
            fadd    s0, s0, s1
            fmov    s1, #0.2500
            fmul    s10, s0, s1
            ldr     s11, [@RWD84]
            fadd    s0, s9, s11
            ldrb    w0, [x19, #0x69]
            ldr     x24, [x19, #0x10]
            movi    v1.2s, #0
            fmax    s0, s0, s1
            fmul    s1, s0, s0
            ldr     s12, [@RWD100]
            fmul    s16, s1, s12
            ldr     s13, [@RWD116]
            fadd    s16, s16, s13
            ldr     s14, [@RWD132]
            fmul    s0, s0, s14
            fmul    s0, s0, s1
            ldr     s15, [@RWD148]
            fadd    s0, s0, s15
            strb    wzr, [fp, #0xC8]
            cbz     w0, G_M000_IG20

G_M000_IG18:                ;; offset=0x0738
            add     x0, fp, #200
            fmov    s1, s16
            movz    x1, #0x4948
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            str     s0, [fp, #0xC4]
            cbz     x24, G_M000_IG19
            ldrb    w2, [fp, #0xC8]
            mov     x0, x24
            mov     w1, #1
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG19:                ;; offset=0x077C
            ldr     s16, [fp, #0xC4]
            b       G_M000_IG21

G_M000_IG20:                ;; offset=0x0784
            fdiv    s16, s0, s16

G_M000_IG21:                ;; offset=0x0788
            fsub    s0, s9, s10
            fmul    s0, s0, s16
            fmul    s0, s0, s0
            ldr     d9, [@RWD644]
            fmin    s0, s0, s9
            ldr     s1, [@RWD00]
            fmul    s0, s0, s1
            fmov    s1, #28.0000
            fadd    s0, s0, s1
            fsqrt   s0, s0
            fmov    s1, #0.2500
            fmul    s10, s0, s1
            mov     w24, #1

G_M000_IG22:                ;; offset=0x07BC
            ldr     w0, [x19, #0x54]
            madd    w1, w24, w0, w23
            ldr     x2, [x19, #0x18]
            ldr     w3, [x2, #0x08]
            cmp     w1, w3
            bhs     G_M000_IG61
            add     x2, x2, #16
            ldr     s0, [x2, w1, UXTW #2]
            fmov    s16, s0
            str     s16, [fp, #0xBC]
            sub     w4, w1, #1
            cmp     w4, w3
            bhs     G_M000_IG61
            ldr     s0, [x2, w4, UXTW #2]
            add     w4, w1, #1
            cmp     w4, w3
            bhs     G_M000_IG61
            ldr     s1, [x2, w4, UXTW #2]
            fadd    s0, s1, s0
            sub     w4, w1, w0
            cmp     w4, w3
            bhs     G_M000_IG61
            ldr     s1, [x2, w4, UXTW #2]
            add     w0, w1, w0
            cmp     w0, w3
            bhs     G_M000_IG61
            ldr     s17, [x2, w0, UXTW #2]
            fadd    s1, s1, s17
            fadd    s0, s0, s1
            fmov    s1, #0.2500
            fmul    s0, s0, s1
            str     s0, [fp, #0xC0]
            fadd    s0, s16, s11
            ldrb    w0, [x19, #0x69]
            ldr     x25, [x19, #0x10]
            movi    v1.2s, #0
            fmax    s0, s0, s1
            fmul    s1, s0, s0
            fmul    s18, s1, s12
            fadd    s18, s18, s13
            fmul    s0, s0, s14
            fmul    s0, s0, s1
            fadd    s0, s0, s15
            strb    wzr, [fp, #0xB0]
            cbz     w0, G_M000_IG25

G_M000_IG23:                ;; offset=0x0870
            add     x0, fp, #176
            fmov    s1, s18
            movz    x1, #0x4948
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            str     s0, [fp, #0xAC]
            cbz     x25, G_M000_IG24
            ldrb    w2, [fp, #0xB0]
            mov     x0, x25
            mov     w1, #1
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG24:                ;; offset=0x08B4
            ldr     s16, [fp, #0xAC]
            fmov    s0, s16
            b       G_M000_IG26

G_M000_IG25:                ;; offset=0x08C0
            fdiv    s0, s0, s18

G_M000_IG26:                ;; offset=0x08C4
            ldp     s16, s17, [fp, #0xBC]
            fsub    s1, s16, s17
            fmul    s0, s1, s0
            fmul    s0, s0, s0
            fmin    s0, s0, s9
            fmul    s0, s0, s8
            fmov    s1, #28.0000
            fadd    s0, s0, s1
            fsqrt   s0, s0
            fmov    s1, #0.2500
            fmul    s0, s0, s1
            fadd    s10, s0, s10
            add     w24, w24, #1
            cmp     w24, #4
            blt     G_M000_IG22

G_M000_IG27:                ;; offset=0x0900
            add     w24, w23, #1
            ldr     x0, [x19, #0x18]
            ldr     w1, [x0, #0x08]
            cmp     w24, w1
            bhs     G_M000_IG61
            add     x0, x0, #16
            ldr     s0, [x0, w24, UXTW #2]
            fmov    s16, s0
            str     s16, [fp, #0xA0]
            sub     w2, w24, #1
            cmp     w2, w1
            bhs     G_M000_IG61
            sub     w2, w24, #1
            ldr     s0, [x0, w2, UXTW #2]
            add     w2, w24, #1
            cmp     w2, w1
            bhs     G_M000_IG61
            add     w2, w24, #1
            ldr     s1, [x0, w2, UXTW #2]
            fadd    s0, s1, s0
            ldr     w2, [x19, #0x54]
            sub     w3, w24, w2
            cmp     w3, w1
            bhs     G_M000_IG61
            ldr     s1, [x0, w3, UXTW #2]
            add     w2, w24, w2
            cmp     w2, w1
            bhs     G_M000_IG61
            ldr     s17, [x0, w2, UXTW #2]
            fadd    s1, s1, s17
            fadd    s0, s0, s1
            fmov    s1, #0.2500
            fmul    s0, s0, s1
            str     s0, [fp, #0xA4]
            fadd    s0, s16, s11
            ldrb    w0, [x19, #0x69]
            ldr     x25, [x19, #0x10]
            movi    v1.2s, #0
            fmax    s0, s0, s1
            fmul    s1, s0, s0
            fmul    s18, s1, s12
            fadd    s18, s18, s13
            fmul    s0, s0, s14
            fmul    s0, s0, s1
            fadd    s0, s0, s15
            strb    wzr, [fp, #0x98]
            cbz     w0, G_M000_IG30

G_M000_IG28:                ;; offset=0x09BC
            add     x0, fp, #152
            fmov    s1, s18
            movz    x1, #0x4948
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            str     s0, [fp, #0x94]
            cbz     x25, G_M000_IG29
            ldrb    w2, [fp, #0x98]
            mov     x0, x25
            mov     w1, #1
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG29:                ;; offset=0x0A00
            ldr     s16, [fp, #0x94]
            fmov    s0, s16
            b       G_M000_IG31

G_M000_IG30:                ;; offset=0x0A0C
            fdiv    s0, s0, s18

G_M000_IG31:                ;; offset=0x0A10
            ldp     s16, s17, [fp, #0xA0]
            fsub    s1, s16, s17
            fmul    s0, s1, s0
            fmul    s0, s0, s0
            fmin    s0, s0, s9
            ldr     s1, [@RWD00]
            fmul    s0, s0, s1
            fmov    s1, #28.0000
            fadd    s0, s0, s1
            fsqrt   s0, s0
            fmov    s1, #0.2500
            fmul    s0, s0, s1
            str     s0, [fp, #0xA8]
            mov     w25, #1

G_M000_IG32:                ;; offset=0x0A48
            ldr     w0, [x19, #0x54]
            madd    w1, w25, w0, w24
            ldr     x2, [x19, #0x18]
            ldr     w3, [x2, #0x08]
            cmp     w1, w3
            bhs     G_M000_IG61
            add     x2, x2, #16
            ldr     s0, [x2, w1, UXTW #2]
            fmov    s17, s0
            str     s17, [fp, #0x8C]
            sub     w4, w1, #1
            cmp     w4, w3
            bhs     G_M000_IG61
            ldr     s0, [x2, w4, UXTW #2]
            add     w4, w1, #1
            cmp     w4, w3
            bhs     G_M000_IG61
            ldr     s1, [x2, w4, UXTW #2]
            fadd    s0, s1, s0
            sub     w4, w1, w0
            cmp     w4, w3
            bhs     G_M000_IG61
            ldr     s1, [x2, w4, UXTW #2]
            add     w0, w1, w0
            cmp     w0, w3
            bhs     G_M000_IG61
            ldr     s18, [x2, w0, UXTW #2]
            fadd    s1, s1, s18
            fadd    s0, s0, s1
            fmov    s1, #0.2500
            fmul    s0, s0, s1
            str     s0, [fp, #0x90]
            fadd    s0, s17, s11
            ldrb    w0, [x19, #0x69]
            ldr     x26, [x19, #0x10]
            movi    v1.2s, #0
            fmax    s0, s0, s1
            fmul    s1, s0, s0
            fmul    s19, s1, s12
            fadd    s19, s19, s13
            fmul    s0, s0, s14
            fmul    s0, s0, s1
            fadd    s0, s0, s15
            strb    wzr, [fp, #0x80]
            cbz     w0, G_M000_IG35

G_M000_IG33:                ;; offset=0x0AFC
            add     x0, fp, #128
            fmov    s1, s19
            movz    x1, #0x4948
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            str     s0, [fp, #0x7C]
            cbz     x26, G_M000_IG34
            ldrb    w2, [fp, #0x80]
            mov     x0, x26
            mov     w1, #1
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG34:                ;; offset=0x0B40
            ldr     s16, [fp, #0x7C]
            fmov    s0, s16
            b       G_M000_IG36

G_M000_IG35:                ;; offset=0x0B4C
            fdiv    s0, s0, s19

G_M000_IG36:                ;; offset=0x0B50
            ldp     s17, s18, [fp, #0x8C]
            fsub    s1, s17, s18
            fmul    s0, s1, s0
            fmul    s0, s0, s0
            fmin    s0, s0, s9
            fmul    s0, s0, s8
            fmov    s1, #28.0000
            fadd    s0, s0, s1
            fsqrt   s0, s0
            fmov    s1, #0.2500
            fmul    s0, s0, s1
            ldr     s16, [fp, #0xA8]
            fadd    s16, s0, s16
            add     w25, w25, #1
            cmp     w25, #4
            str     s16, [fp, #0xA8]
            blt     G_M000_IG32

G_M000_IG37:                ;; offset=0x0B94
            ldr     s16, [fp, #0xA8]
            str     s16, [fp, #0xFC]
            add     w24, w23, #2
            ldr     x0, [x19, #0x18]
            ldr     w1, [x0, #0x08]
            cmp     w24, w1
            bhs     G_M000_IG61
            add     x0, x0, #16
            ldr     s0, [x0, w24, UXTW #2]
            fmov    s17, s0
            str     s17, [fp, #0x70]
            sub     w2, w24, #1
            cmp     w2, w1
            bhs     G_M000_IG61
            sub     w2, w24, #1
            ldr     s0, [x0, w2, UXTW #2]
            add     w2, w24, #1
            cmp     w2, w1
            bhs     G_M000_IG61
            add     w2, w24, #1
            ldr     s1, [x0, w2, UXTW #2]
            fadd    s0, s1, s0
            ldr     w2, [x19, #0x54]
            sub     w2, w24, w2
            cmp     w2, w1
            bhs     G_M000_IG61
            ldr     s1, [x0, w2, UXTW #2]
            ldr     w2, [x19, #0x54]
            add     w2, w24, w2
            cmp     w2, w1
            bhs     G_M000_IG61
            ldr     s18, [x0, w2, UXTW #2]
            fadd    s1, s1, s18
            fadd    s0, s0, s1
            fmov    s1, #0.2500
            fmul    s0, s0, s1
            str     s0, [fp, #0x74]
            fadd    s0, s17, s11
            ldrb    w0, [x19, #0x69]
            ldr     x25, [x19, #0x10]
            movi    v1.2s, #0
            fmax    s0, s0, s1
            fmul    s1, s0, s0
            fmul    s19, s1, s12
            fadd    s19, s19, s13
            fmul    s0, s0, s14
            fmul    s0, s0, s1
            fadd    s0, s0, s15
            strb    wzr, [fp, #0x68]
            cbz     w0, G_M000_IG40

G_M000_IG38:                ;; offset=0x0C5C
            add     x0, fp, #104
            fmov    s1, s19
            movz    x1, #0x4948
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            str     s0, [fp, #0x64]
            cbz     x25, G_M000_IG39
            ldrb    w2, [fp, #0x68]
            mov     x0, x25
            mov     w1, #1
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG39:                ;; offset=0x0CA0
            ldr     s16, [fp, #0x64]
            fmov    s0, s16
            b       G_M000_IG41

G_M000_IG40:                ;; offset=0x0CAC
            fdiv    s0, s0, s19

G_M000_IG41:                ;; offset=0x0CB0
            ldp     s17, s18, [fp, #0x70]
            fsub    s1, s17, s18
            fmul    s0, s1, s0
            fmul    s0, s0, s0
            fmin    s0, s0, s9
            ldr     s1, [@RWD00]
            fmul    s0, s0, s1
            fmov    s1, #28.0000
            fadd    s0, s0, s1
            fsqrt   s0, s0
            fmov    s1, #0.2500
            fmul    s0, s0, s1
            str     s0, [fp, #0x78]
            mov     w25, #1

G_M000_IG42:                ;; offset=0x0CE8
            ldr     w0, [x19, #0x54]
            madd    w0, w25, w0, w24
            ldr     x1, [x19, #0x18]
            ldr     w2, [x1, #0x08]
            cmp     w0, w2
            bhs     G_M000_IG61
            add     x1, x1, #16
            ldr     s0, [x1, w0, UXTW #2]
            fmov    s18, s0
            str     s18, [fp, #0x5C]
            ldr     x1, [x19, #0x18]
            sub     w2, w0, #1
            ldr     w3, [x1, #0x08]
            cmp     w2, w3
            bhs     G_M000_IG61
            add     x1, x1, #16
            sub     w2, w0, #1
            ldr     s0, [x1, w2, UXTW #2]
            ldr     x1, [x19, #0x18]
            add     w2, w0, #1
            ldr     w3, [x1, #0x08]
            cmp     w2, w3
            bhs     G_M000_IG61
            add     x1, x1, #16
            add     w2, w0, #1
            ldr     s1, [x1, w2, UXTW #2]
            fadd    s0, s1, s0
            ldr     x1, [x19, #0x18]
            ldr     w2, [x19, #0x54]
            sub     w2, w0, w2
            ldr     w3, [x1, #0x08]
            cmp     w2, w3
            bhs     G_M000_IG61
            add     x1, x1, #16
            ldr     s1, [x1, w2, UXTW #2]
            ldr     x1, [x19, #0x18]
            ldr     w2, [x19, #0x54]
            add     w0, w0, w2
            ldr     w2, [x1, #0x08]
            cmp     w0, w2
            bhs     G_M000_IG61
            add     x1, x1, #16
            ldr     s19, [x1, w0, UXTW #2]
            fadd    s1, s1, s19
            fadd    s0, s0, s1
            fmov    s1, #0.2500
            fmul    s0, s0, s1
            str     s0, [fp, #0x60]
            fadd    s0, s18, s11
            ldrb    w0, [x19, #0x69]
            ldr     x26, [x19, #0x10]
            movi    v1.2s, #0
            fmax    s0, s0, s1
            fmul    s1, s0, s0
            fmul    s20, s1, s12
            fadd    s20, s20, s13
            fmul    s0, s0, s14
            fmul    s0, s0, s1
            fadd    s0, s0, s15
            strb    wzr, [fp, #0x50]
            cbz     w0, G_M000_IG45

G_M000_IG43:                ;; offset=0x0DDC
            add     x0, fp, #80
            fmov    s1, s20
            movz    x1, #0x4948
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            str     s0, [fp, #0x4C]
            cbz     x26, G_M000_IG44
            ldrb    w2, [fp, #0x50]
            mov     x0, x26
            mov     w1, #1
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG44:                ;; offset=0x0E20
            ldr     s16, [fp, #0x4C]
            fmov    s0, s16
            b       G_M000_IG46

G_M000_IG45:                ;; offset=0x0E2C
            fdiv    s0, s0, s20

G_M000_IG46:                ;; offset=0x0E30
            ldp     s18, s19, [fp, #0x5C]
            fsub    s1, s18, s19
            fmul    s0, s1, s0
            fmul    s0, s0, s0
            fmin    s0, s0, s9
            fmul    s0, s0, s8
            fmov    s1, #28.0000
            fadd    s0, s0, s1
            fsqrt   s0, s0
            fmov    s1, #0.2500
            fmul    s0, s0, s1
            ldr     s17, [fp, #0x78]
            fadd    s17, s0, s17
            add     w25, w25, #1
            cmp     w25, #4
            str     s17, [fp, #0x78]
            blt     G_M000_IG42

G_M000_IG47:                ;; offset=0x0E74
            ldr     s17, [fp, #0x78]
            str     s17, [fp, #0xF8]
            add     w23, w23, #3
            ldr     x0, [x19, #0x18]
            ldr     w1, [x0, #0x08]
            cmp     w23, w1
            bhs     G_M000_IG61
            add     x0, x0, #16
            ldr     s0, [x0, w23, UXTW #2]
            fmov    s18, s0
            str     s18, [fp, #0x40]
            ldr     x0, [x19, #0x18]
            sub     w1, w23, #1
            ldr     w2, [x0, #0x08]
            cmp     w1, w2
            bhs     G_M000_IG61
            add     x0, x0, #16
            sub     w1, w23, #1
            ldr     s0, [x0, w1, UXTW #2]
            ldr     x0, [x19, #0x18]
            add     w1, w23, #1
            ldr     w2, [x0, #0x08]
            cmp     w1, w2
            bhs     G_M000_IG61
            add     x0, x0, #16
            add     w1, w23, #1
            ldr     s1, [x0, w1, UXTW #2]
            fadd    s0, s1, s0
            ldr     x0, [x19, #0x18]
            ldr     w1, [x19, #0x54]
            sub     w1, w23, w1
            ldr     w2, [x0, #0x08]
            cmp     w1, w2
            bhs     G_M000_IG61
            add     x0, x0, #16
            ldr     s1, [x0, w1, UXTW #2]
            ldr     x0, [x19, #0x18]
            ldr     w1, [x19, #0x54]
            add     w1, w23, w1
            ldr     w2, [x0, #0x08]
            cmp     w1, w2
            bhs     G_M000_IG61
            add     x0, x0, #16
            ldr     s19, [x0, w1, UXTW #2]
            fadd    s1, s1, s19
            fadd    s0, s0, s1
            fmov    s1, #0.2500
            fmul    s0, s0, s1
            str     s0, [fp, #0x44]
            fadd    s0, s18, s11
            ldrb    w0, [x19, #0x69]
            ldr     x24, [x19, #0x10]
            movi    v1.2s, #0
            fmax    s0, s0, s1
            fmul    s1, s0, s0
            fmul    s20, s1, s12
            fadd    s20, s20, s13
            fmul    s0, s0, s14
            fmul    s0, s0, s1
            fadd    s0, s0, s15
            strb    wzr, [fp, #0x38]
            cbz     w0, G_M000_IG50

G_M000_IG48:                ;; offset=0x0F6C
            add     x0, fp, #56
            fmov    s1, s20
            movz    x1, #0x4948
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            str     s0, [fp, #0x34]
            cbz     x24, G_M000_IG49
            ldrb    w2, [fp, #0x38]
            mov     x0, x24
            mov     w1, #1
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG49:                ;; offset=0x0FB0
            ldr     s16, [fp, #0x34]
            fmov    s0, s16
            b       G_M000_IG51

G_M000_IG50:                ;; offset=0x0FBC
            fdiv    s0, s0, s20

G_M000_IG51:                ;; offset=0x0FC0
            ldp     s18, s19, [fp, #0x40]
            fsub    s1, s18, s19
            fmul    s0, s1, s0
            fmul    s0, s0, s0
            fmin    s0, s0, s9
            ldr     s1, [@RWD00]
            fmul    s0, s0, s1
            fmov    s1, #28.0000
            fadd    s0, s0, s1
            fsqrt   s0, s0
            fmov    s1, #0.2500
            fmul    s0, s0, s1
            str     s0, [fp, #0x48]
            mov     w24, #1

G_M000_IG52:                ;; offset=0x0FF8
            ldr     w0, [x19, #0x54]
            madd    w0, w24, w0, w23
            ldr     x1, [x19, #0x18]
            ldr     w2, [x1, #0x08]
            cmp     w0, w2
            bhs     G_M000_IG61
            add     x1, x1, #16
            ldr     s0, [x1, w0, UXTW #2]
            fmov    s19, s0
            str     s19, [fp, #0x2C]
            ldr     x1, [x19, #0x18]
            sub     w2, w0, #1
            ldr     w3, [x1, #0x08]
            cmp     w2, w3
            bhs     G_M000_IG61
            add     x1, x1, #16
            sub     w2, w0, #1
            ldr     s0, [x1, w2, UXTW #2]
            ldr     x1, [x19, #0x18]
            add     w2, w0, #1
            ldr     w3, [x1, #0x08]
            cmp     w2, w3
            bhs     G_M000_IG61
            add     x1, x1, #16
            add     w2, w0, #1
            ldr     s1, [x1, w2, UXTW #2]
            fadd    s0, s1, s0
            ldr     x1, [x19, #0x18]
            ldr     w2, [x19, #0x54]
            sub     w2, w0, w2
            ldr     w3, [x1, #0x08]
            cmp     w2, w3
            bhs     G_M000_IG61
            add     x1, x1, #16
            ldr     s1, [x1, w2, UXTW #2]
            ldr     x1, [x19, #0x18]
            ldr     w2, [x19, #0x54]
            add     w0, w0, w2
            ldr     w2, [x1, #0x08]
            cmp     w0, w2
            bhs     G_M000_IG61
            add     x1, x1, #16
            ldr     s20, [x1, w0, UXTW #2]
            fadd    s1, s1, s20
            fadd    s0, s0, s1
            fmov    s1, #0.2500
            fmul    s0, s0, s1
            str     s0, [fp, #0x30]
            fadd    s0, s19, s11
            ldrb    w0, [x19, #0x69]
            ldr     x25, [x19, #0x10]
            movi    v1.2s, #0
            fmax    s0, s0, s1
            fmul    s1, s0, s0
            fmul    s21, s1, s12
            fadd    s21, s21, s13
            fmul    s0, s0, s14
            fmul    s0, s0, s1
            fadd    s0, s0, s15
            strb    wzr, [fp, #0x20]
            cbz     w0, G_M000_IG55

G_M000_IG53:                ;; offset=0x10EC
            add     x0, fp, #32
            fmov    s1, s21
            movz    x1, #0x4948
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            str     s0, [fp, #0x1C]
            cbz     x25, G_M000_IG54
            ldrb    w2, [fp, #0x20]
            mov     x0, x25
            mov     w1, #1
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG54:                ;; offset=0x1130
            ldr     s16, [fp, #0x1C]
            fmov    s21, s16
            b       G_M000_IG56

G_M000_IG55:                ;; offset=0x113C
            fdiv    s21, s0, s21

G_M000_IG56:                ;; offset=0x1140
            ldp     s19, s20, [fp, #0x2C]
            fsub    s19, s19, s20
            fmul    s19, s19, s21
            fmul    s19, s19, s19
            fmin    s19, s19, s9
            fmul    s19, s19, s8
            fmov    s20, #28.0000
            fadd    s19, s19, s20
            fsqrt   s19, s19
            fmov    s20, #0.2500
            fmul    s19, s19, s20
            ldr     s18, [fp, #0x48]
            fadd    s18, s19, s18
            add     w24, w24, #1
            cmp     w24, #4
            str     s18, [fp, #0x48]
            blt     G_M000_IG52

G_M000_IG57:                ;; offset=0x1184
            ldr     x0, [x19, #0x20]
            asr     w1, w22, #31
            and     w1, w1, #3
            add     w1, w1, w22
            add     w1, w21, w1,  ASR #2
            ldr     w2, [x0, #0x08]
            cmp     w1, w2
            bhs     G_M000_IG61
            add     x0, x0, #16
            ldr     s16, [fp, #0xFC]
            fadd    s16, s10, s16
            ldr     s17, [fp, #0xF8]
            fadd    s16, s16, s17
            ldr     s18, [fp, #0x48]
            fadd    s16, s16, s18
            fmov    s17, #0.2500
            fmul    s16, s16, s17
            str     s16, [x0, w1, UXTW #2]
            add     w22, w22, #4

G_M000_IG58:                ;; offset=0x11D0
            ldr     w0, [x19, #0x4C]
            cmp     w22, w0
            blt     G_M000_IG17

G_M000_IG59:                ;; offset=0x11DC
            ldp     x25, x26, [sp, #0x170]
            ldp     x23, x24, [sp, #0x160]
            ldp     x21, x22, [sp, #0x150]
            ldp     x19, x20, [sp, #0x140]
            ldp     d14, d15, [sp, #0x130]
            ldp     d12, d13, [sp, #0x120]
            ldp     d10, d11, [sp, #0x110]
            ldp     d8, d9, [sp, #0x100]
            ldp     fp, lr, [sp], #0x180
            ret     lr

G_M000_IG60:                ;; offset=0x1204
            movz    x0, #0x4750
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()
            brk     #0

G_M000_IG61:                ;; offset=0x121C
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dd	480E0640h		;    145433
RWD04  	dq	3E4CCCCD3E4CCCCDh, 3E4CCCCD3E4CCCCDh
RWD20  	dq	480E0640480E0640h, 480E0640480E0640h
RWD36  	dq	41E0000041E00000h, 41E0000041E00000h
RWD52  	dq	3E8000003E800000h, 3E8000003E800000h
RWD68  	dq	3E8000003E800000h, 3E8000003E800000h
RWD84  	dq	409B0A3D409B0A3Dh, 409B0A3D409B0A3Dh
RWD100 	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD116 	dq	44228FFE44228FFEh, 44228FFE44228FFEh
RWD132 	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD148 	dq	449E380D449E380Dh, 449E380D449E380Dh
RWD164 	dq	3E4CCCCD3E4CCCCDh, 3E4CCCCD3E4CCCCDh
RWD180 	dq	480E0640480E0640h, 480E0640480E0640h
RWD196 	dq	41E0000041E00000h, 41E0000041E00000h
RWD212 	dq	3E8000003E800000h, 3E8000003E800000h
RWD228 	dq	3E8000003E800000h, 3E8000003E800000h
RWD244 	dq	409B0A3D409B0A3Dh, 409B0A3D409B0A3Dh
RWD260 	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD276 	dq	44228FFE44228FFEh, 44228FFE44228FFEh
RWD292 	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD308 	dq	449E380D449E380Dh, 449E380D449E380Dh
RWD324 	dq	3E4CCCCD3E4CCCCDh, 3E4CCCCD3E4CCCCDh
RWD340 	dq	480E0640480E0640h, 480E0640480E0640h
RWD356 	dq	41E0000041E00000h, 41E0000041E00000h
RWD372 	dq	3E8000003E800000h, 3E8000003E800000h
RWD388 	dq	3E8000003E800000h, 3E8000003E800000h
RWD404 	dq	409B0A3D409B0A3Dh, 409B0A3D409B0A3Dh
RWD420 	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD436 	dq	44228FFE44228FFEh, 44228FFE44228FFEh
RWD452 	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD468 	dq	449E380D449E380Dh, 449E380D449E380Dh
RWD484 	dq	3E4CCCCD3E4CCCCDh, 3E4CCCCD3E4CCCCDh
RWD500 	dq	480E0640480E0640h, 480E0640480E0640h
RWD516 	dq	41E0000041E00000h, 41E0000041E00000h
RWD532 	dq	3E8000003E800000h, 3E8000003E800000h
RWD548 	dq	3E8000003E800000h, 3E8000003E800000h
RWD564 	dq	409B0A3D409B0A3Dh, 409B0A3D409B0A3Dh
RWD580 	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD596 	dq	44228FFE44228FFEh, 44228FFE44228FFEh
RWD612 	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD628 	dq	449E380D449E380Dh, 449E380D449E380Dh
RWD644 	dq	3E4CCCCD3E4CCCCDh

; Total bytes of code 4644

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 33 single block inlinees; 11 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x30]!
            stp     x19, x20, [sp, #0x10]
            stp     x21, x22, [sp, #0x20]
            mov     fp, sp
            mov     x19, x0
            mov     w20, w1
            mov     w21, w2

G_M000_IG02:                ;; offset=0x001C
            mov     w22, wzr
            tst     w3, #255
            beq     G_M000_IG08

G_M000_IG03:                ;; offset=0x0028
            ldr     w0, [x19, #0x58]
            cmp     w0, #5
            ble     G_M000_IG08
            mov     w22, #1
            mov     x0, x19
            mov     w1, w20
            mov     w2, w21
            mov     w3, wzr
            movz    x4, #0x49C0
            movk    x4, #0x695F LSL #16
            movk    x4, #0x7FF8 LSL #32
            ldr     x4, [x4]
            blr     x4      // code for DotJpegli.Internal.AdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this
            ldr     w0, [x19, #0x58]
            sub     w1, w0, #4
            cmp     w1, #1
            ble     G_M000_IG08
            sub     w1, w20, #1
            add     w2, w20, #1
            align   [0 bytes for IG04]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG04:                ;; offset=0x0074
            madd    w3, w1, w0, w22
            madd    w4, w20, w0, w22
            madd    w5, w2, w0, w22
            ldr     x6, [x19, #0x20]
            sub     w7, w3, #1
            tbnz    w7, #31, G_M000_IG11
            ldr     w8, [x6, #0x08]
            sub     w9, w8, w7
            cmp     w9, #4
            blt     G_M000_IG11
            cmp     w7, w8
            bhs     G_M000_IG14
            add     x6, x6, #16
            ubfiz   x7, x7, #2, #32
            ldr     q16, [x6, x7]
            sub     w7, w8, w3
            cmp     w7, #4
            ccmp    w3, #0, nc, ge
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x7, x3, #2, #32
            ldr     q17, [x6, x7]
            add     w3, w3, #1
            sub     w7, w8, w3
            cmp     w7, #4
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x3, x3, #2, #32
            ldr     q18, [x6, x3]
            sub     w3, w4, #1
            sub     w7, w8, w3
            cmp     w7, #4
            ccmp    w3, #0, nc, ge
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x3, x3, #2, #32
            ldr     q19, [x6, x3]
            sub     w3, w8, w4
            cmp     w3, #4
            ccmp    w4, #0, nc, ge
            blt     G_M000_IG11
            cmp     w4, w8
            bhs     G_M000_IG14
            ubfiz   x3, x4, #2, #32
            ldr     q20, [x6, x3]
            add     w3, w4, #1
            sub     w4, w8, w3
            cmp     w4, #4
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x3, x3, #2, #32
            ldr     q21, [x6, x3]
            sub     w3, w5, #1
            sub     w4, w8, w3
            cmp     w4, #4
            ccmp    w3, #0, nc, ge
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x3, x3, #2, #32
            ldr     q22, [x6, x3]
            sub     w3, w8, w5
            cmp     w3, #4
            ccmp    w5, #0, nc, ge
            blt     G_M000_IG11
            cmp     w5, w8
            bhs     G_M000_IG14
            ubfiz   x3, x5, #2, #32
            ldr     q23, [x6, x3]
            add     w3, w5, #1
            sub     w4, w8, w3
            cmp     w4, #4
            blt     G_M000_IG11
            cmp     w3, w8
            bhs     G_M000_IG14
            ubfiz   x3, x3, #2, #32
            ldr     q24, [x6, x3]
            ldrb    w3, [x19, #0x68]
            cbnz    w3, G_M000_IG06

G_M000_IG05:                ;; offset=0x01C0
            fmax    v25.4s, v16.4s, v17.4s
            fmin    v16.4s, v16.4s, v17.4s
            fmax    v17.4s, v18.4s, v19.4s
            fmin    v18.4s, v18.4s, v19.4s
            fmax    v19.4s, v16.4s, v18.4s
            fmin    v16.4s, v16.4s, v18.4s
            fmax    v26.4s, v25.4s, v17.4s
            fmin    v17.4s, v25.4s, v17.4s
            fmax    v18.4s, v17.4s, v19.4s
            fmin    v17.4s, v17.4s, v19.4s
            fmax    v19.4s, v16.4s, v20.4s
            fmax    v25.4s, v17.4s, v19.4s
            fmax    v27.4s, v18.4s, v25.4s
            fmin    v16.4s, v16.4s, v20.4s
            fmin    v17.4s, v17.4s, v19.4s
            fmin    v18.4s, v18.4s, v25.4s
            fmin    v19.4s, v26.4s, v27.4s
            fmax    v20.4s, v16.4s, v21.4s
            fmax    v25.4s, v17.4s, v20.4s
            fmax    v26.4s, v18.4s, v25.4s
            fmin    v16.4s, v16.4s, v21.4s
            fmin    v17.4s, v17.4s, v20.4s
            fmin    v18.4s, v18.4s, v25.4s
            fmin    v19.4s, v19.4s, v26.4s
            fmax    v21.4s, v16.4s, v22.4s
            fmax    v20.4s, v17.4s, v21.4s
            fmax    v25.4s, v18.4s, v20.4s
            fmin    v16.4s, v16.4s, v22.4s
            fmin    v17.4s, v17.4s, v21.4s
            fmin    v18.4s, v18.4s, v20.4s
            fmin    v19.4s, v19.4s, v25.4s
            fmax    v22.4s, v16.4s, v23.4s
            fmax    v20.4s, v17.4s, v22.4s
            fmax    v21.4s, v18.4s, v20.4s
            fmin    v16.4s, v16.4s, v23.4s
            fmin    v17.4s, v17.4s, v22.4s
            fmin    v18.4s, v18.4s, v20.4s
            fmin    v19.4s, v19.4s, v21.4s
            fmax    v23.4s, v16.4s, v24.4s
            fmax    v20.4s, v17.4s, v23.4s
            fmax    v21.4s, v18.4s, v20.4s
            fmin    v16.4s, v16.4s, v24.4s
            fmin    v17.4s, v17.4s, v23.4s
            fmin    v18.4s, v18.4s, v20.4s
            fmin    v19.4s, v19.4s, v21.4s
            mov     v20.16b, v19.16b
            mov     v19.16b, v17.16b
            mov     v17.16b, v18.16b
            b       G_M000_IG07

G_M000_IG06:                ;; offset=0x0284
            fmax    v25.4s, v16.4s, v19.4s
            fmin    v16.4s, v16.4s, v19.4s
            fmax    v19.4s, v17.4s, v23.4s
            fmin    v17.4s, v17.4s, v23.4s
            fmax    v23.4s, v18.4s, v21.4s
            fmin    v18.4s, v18.4s, v21.4s
            fmax    v21.4s, v20.4s, v24.4s
            fmin    v20.4s, v20.4s, v24.4s
            fmax    v24.4s, v16.4s, v19.4s
            fmin    v16.4s, v16.4s, v19.4s
            fmax    v19.4s, v18.4s, v20.4s
            fmin    v18.4s, v18.4s, v20.4s
            fmax    v26.4s, v25.4s, v21.4s
            fmin    v20.4s, v25.4s, v21.4s
            fmax    v21.4s, v23.4s, v22.4s
            fmin    v22.4s, v23.4s, v22.4s
            fmax    v23.4s, v16.4s, v18.4s
            fmin    v16.4s, v16.4s, v18.4s
            fmax    v18.4s, v17.4s, v20.4s
            fmin    v17.4s, v17.4s, v20.4s
            fmax    v25.4s, v19.4s, v22.4s
            fmin    v20.4s, v19.4s, v22.4s
            fmin    v22.4s, v24.4s, v26.4s
            fmax    v19.4s, v17.4s, v20.4s
            fmin    v17.4s, v17.4s, v20.4s
            fmin    v20.4s, v18.4s, v21.4s
            fmin    v22.4s, v25.4s, v22.4s
            fmax    v18.4s, v16.4s, v17.4s
            fmin    v16.4s, v16.4s, v17.4s
            fmax    v17.4s, v23.4s, v19.4s
            fmin    v19.4s, v23.4s, v19.4s
            fmax    v21.4s, v20.4s, v22.4s
            fmin    v20.4s, v20.4s, v22.4s
            fmax    v22.4s, v19.4s, v20.4s
            fmin    v19.4s, v19.4s, v20.4s
            fmin    v20.4s, v17.4s, v21.4s
            fmax    v17.4s, v18.4s, v19.4s
            fmin    v19.4s, v18.4s, v19.4s
            fmin    v20.4s, v22.4s, v20.4s

G_M000_IG07:                ;; offset=0x0320
            movi    v18.4s, #0x3E,  LSL #24
            fmul    v16.4s, v16.4s, v18.4s
            ldr     q18, [@RWD00]
            fmul    v18.4s, v19.4s, v18.4s
            fadd    v16.4s, v16.4s, v18.4s
            ldr     q18, [@RWD16]
            fmul    v17.4s, v17.4s, v18.4s
            ldr     q18, [@RWD32]
            fmul    v18.4s, v20.4s, v18.4s
            fadd    v17.4s, v17.4s, v18.4s
            fadd    v16.4s, v16.4s, v17.4s
            ldr     x3, [x19, #0x28]
            add     w4, w21, w22
            ldr     w5, [x3, #0x08]
            cmp     w5, w4
            bls     G_M000_IG12
            sub     w5, w5, w4
            cmp     w5, #4
            blt     G_M000_IG13
            add     x3, x3, #16
            ubfiz   x4, x4, #2, #32
            str     q16, [x3, x4]
            add     w22, w22, #4
            ldr     w3, [x19, #0x58]
            sub     w3, w3, #4
            cmp     w22, w3
            blt     G_M000_IG04

G_M000_IG08:                ;; offset=0x038C
            ldr     w0, [x19, #0x58]
            cmp     w22, w0
            bge     G_M000_IG10

G_M000_IG09:                ;; offset=0x0398
            mov     x0, x19
            mov     w1, w20
            mov     w2, w21
            mov     w3, w22
            movz    x4, #0x49C0
            movk    x4, #0x695F LSL #16
            movk    x4, #0x7FF8 LSL #32
            ldr     x4, [x4]
            blr     x4      // code for DotJpegli.Internal.AdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this
            add     w22, w22, #1
            ldr     w0, [x19, #0x58]
            cmp     w22, w0
            blt     G_M000_IG09

G_M000_IG10:                ;; offset=0x03CC
            ldp     x21, x22, [sp, #0x20]
            ldp     x19, x20, [sp, #0x10]
            ldp     fp, lr, [sp], #0x30
            ret     lr

G_M000_IG11:                ;; offset=0x03DC
            movz    x0, #0x4750
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()
            brk     #0

G_M000_IG12:                ;; offset=0x03F4
            movz    x0, #0x4768
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLess()
            brk     #0

G_M000_IG13:                ;; offset=0x040C
            movz    x0, #0x4468
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentException_DestinationTooShort()
            brk     #0

G_M000_IG14:                ;; offset=0x0424
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dq	3D99999A3D99999Ah, 3D99999A3D99999Ah
RWD16  	dq	3D75C28F3D75C28Fh, 3D75C28F3D75C28Fh
RWD32  	dq	3D4CCCCD3D4CCCCDh, 3D4CCCCD3D4CCCCDh

; Total bytes of code 1068

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data
; 2 inlinees with PGO data; 29 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x10]!
            mov     fp, sp

G_M000_IG02:                ;; offset=0x0008
            sub     w4, w3, #1
            cmp     w4, #0
            csel    w4, w4, wzr, ge
            add     w5, w3, #1
            ldr     w6, [x0, #0x58]
            sub     w7, w6, #1
            cmp     w5, w7
            csel    w5, w5, w7, le
            sub     w7, w1, #1
            mul     w7, w7, w6
            mul     w8, w1, w6
            add     w1, w1, #1
            mul     w1, w1, w6
            ldr     x6, [x0, #0x20]
            add     w9, w7, w4
            ldr     w10, [x6, #0x08]
            cmp     w9, w10
            bhs     G_M000_IG07
            add     x6, x6, #16
            ldr     s16, [x6, w9, UXTW #2]
            add     w9, w7, w3
            cmp     w9, w10
            bhs     G_M000_IG07
            ldr     s17, [x6, w9, UXTW #2]
            add     w7, w7, w5
            cmp     w7, w10
            bhs     G_M000_IG07
            ldr     s18, [x6, w7, UXTW #2]
            add     w7, w8, w4
            cmp     w7, w10
            bhs     G_M000_IG07
            ldr     s19, [x6, w7, UXTW #2]
            add     w7, w8, w3
            cmp     w7, w10
            bhs     G_M000_IG07
            ldr     s20, [x6, w7, UXTW #2]
            add     w7, w8, w5
            cmp     w7, w10
            bhs     G_M000_IG07
            ldr     s21, [x6, w7, UXTW #2]
            add     w4, w1, w4
            cmp     w4, w10
            bhs     G_M000_IG07
            ldr     s22, [x6, w4, UXTW #2]
            add     w4, w1, w3
            cmp     w4, w10
            bhs     G_M000_IG07
            ldr     s23, [x6, w4, UXTW #2]
            add     w1, w1, w5
            cmp     w1, w10
            bhs     G_M000_IG07
            ldr     s24, [x6, w1, UXTW #2]
            ldrb    w1, [x0, #0x68]
            cbnz    w1, G_M000_IG04

G_M000_IG03:                ;; offset=0x00E0
            fmax    s25, s16, s17
            fmin    s16, s16, s17
            fmax    s17, s18, s19
            fmin    s18, s18, s19
            fmax    s19, s16, s18
            fmin    s16, s16, s18
            fmax    s18, s25, s17
            fmin    s17, s25, s17
            fmax    s25, s17, s19
            fmin    s17, s17, s19
            fmax    s19, s16, s20
            fmax    s26, s17, s19
            fmax    s27, s25, s26
            fmin    s16, s16, s20
            fmin    s17, s17, s19
            fmin    s20, s25, s26
            fmin    s19, s18, s27
            fmax    s18, s16, s21
            fmax    s25, s17, s18
            fmax    s26, s20, s25
            fmin    s16, s16, s21
            fmin    s17, s17, s18
            fmin    s20, s20, s25
            fmin    s19, s19, s26
            fmax    s21, s16, s22
            fmax    s18, s17, s21
            fmax    s25, s20, s18
            fmin    s16, s16, s22
            fmin    s17, s17, s21
            fmin    s20, s20, s18
            fmin    s19, s19, s25
            fmax    s22, s16, s23
            fmax    s18, s17, s22
            fmax    s21, s20, s18
            fmin    s16, s16, s23
            fmin    s17, s17, s22
            fmin    s20, s20, s18
            fmin    s19, s19, s21
            fmax    s23, s16, s24
            fmax    s18, s17, s23
            fmax    s21, s20, s18
            fmin    s16, s16, s24
            fmin    s17, s17, s23
            fmin    s20, s20, s18
            fmin    s19, s19, s21
            fmov    s22, s19
            fmov    s0, s17
            fmov    s17, s20
            fmov    s20, s0
            b       G_M000_IG05

G_M000_IG04:                ;; offset=0x01A8
            fmax    s25, s16, s19
            fmin    s16, s16, s19
            fmax    s19, s17, s23
            fmin    s17, s17, s23
            fmax    s23, s18, s21
            fmin    s18, s18, s21
            fmax    s21, s20, s24
            fmin    s20, s20, s24
            fmax    s24, s16, s19
            fmin    s16, s16, s19
            fmax    s19, s18, s20
            fmin    s18, s18, s20
            fmax    s20, s25, s21
            fmin    s21, s25, s21
            fmax    s25, s23, s22
            fmin    s22, s23, s22
            fmax    s23, s16, s18
            fmin    s16, s16, s18
            fmax    s18, s17, s21
            fmin    s17, s17, s21
            fmax    s21, s19, s22
            fmin    s22, s19, s22
            fmin    s19, s24, s20
            fmax    s20, s17, s22
            fmin    s17, s17, s22
            fmin    s22, s18, s25
            fmin    s18, s21, s19
            fmax    s19, s16, s17
            fmin    s16, s16, s17
            fmax    s17, s23, s20
            fmin    s20, s23, s20
            fmax    s21, s22, s18
            fmin    s22, s22, s18
            fmax    s18, s20, s22
            fmin    s20, s20, s22
            fmin    s22, s17, s21
            fmax    s17, s19, s20
            fmin    s20, s19, s20
            fmin    s22, s18, s22

G_M000_IG05:                ;; offset=0x0244
            ldr     x0, [x0, #0x28]
            add     w1, w2, w3
            ldr     w4, [x0, #0x08]
            cmp     w1, w4
            bhs     G_M000_IG07
            add     w1, w2, w3
            add     x0, x0, #16
            fmov    s18, #0.1250
            fmul    s16, s16, s18
            ldr     s18, [@RWD00]
            fmul    s18, s20, s18
            fadd    s16, s16, s18
            ldr     s18, [@RWD04]
            fmul    s17, s17, s18
            ldr     s18, [@RWD08]
            fmul    s18, s22, s18
            fadd    s17, s17, s18
            fadd    s16, s16, s17
            str     s16, [x0, w1, UXTW #2]

G_M000_IG06:                ;; offset=0x0290
            ldp     fp, lr, [sp], #0x10
            ret     lr

G_M000_IG07:                ;; offset=0x0298
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dd	3D99999Ah		;     0.075
RWD04  	dd	3D75C28Fh		;      0.06
RWD08  	dd	3D4CCCCDh		;      0.05

; Total bytes of code 672

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ComputeMask(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data
; 0 inlinees with PGO data; 0 single block inlinees; 3 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x60]!
            str     d8, [sp, #0x48]
            stp     x19, x20, [sp, #0x50]
            mov     fp, sp
            str     xzr, [fp, #0x38]
            str     xzr, [fp, #0x28]
            str     xzr, [fp, #0x18]
            mov     x19, x1

G_M000_IG02:                ;; offset=0x0020
            ldr     s1, [@RWD00]
            fmul    s0, s0, s1
            ldr     d1, [@RWD04]
            fmax    s8, s0, s1
            ldr     s0, [@RWD12]
            fadd    s1, s8, s0
            uxtb    w20, w0
            cbz     w20, G_M000_IG05

G_M000_IG03:                ;; offset=0x0040
            add     x0, fp, #56
            fmov    s0, #1.0000
            movz    x1, #0x4948
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            str     s0, [fp, #0x34]
            cbz     x19, G_M000_IG04
            ldrb    w2, [fp, #0x38]
            mov     x0, x19
            mov     w1, wzr
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG04:                ;; offset=0x0084
            ldr     s0, [fp, #0x34]
            str     s0, [fp, #0x44]
            b       G_M000_IG06

G_M000_IG05:                ;; offset=0x0090
            fmov    s0, #1.0000
            fdiv    s0, s0, s1
            str     s0, [fp, #0x44]

G_M000_IG06:                ;; offset=0x009C
            fmul    s8, s8, s8
            ldr     s1, [@RWD16]
            fadd    s1, s8, s1
            cbz     w20, G_M000_IG09

G_M000_IG07:                ;; offset=0x00AC
            add     x0, fp, #40
            fmov    s0, #1.0000
            movz    x1, #0x4948
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            str     s0, [fp, #0x24]
            cbz     x19, G_M000_IG08
            ldrb    w2, [fp, #0x28]
            mov     x0, x19
            mov     w1, wzr
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG08:                ;; offset=0x00F0
            ldr     s0, [fp, #0x24]
            str     s0, [fp, #0x30]
            b       G_M000_IG10

G_M000_IG09:                ;; offset=0x00FC
            fmov    s16, #1.0000
            fdiv    s1, s16, s1
            str     s1, [fp, #0x30]

G_M000_IG10:                ;; offset=0x0108
            ldr     s16, [@RWD20]
            fadd    s16, s8, s16
            cbz     w20, G_M000_IG13

G_M000_IG11:                ;; offset=0x0114
            add     x0, fp, #24
            fmov    s0, #1.0000
            fmov    s1, s16
            movz    x1, #0x4948
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            str     s0, [fp, #0x14]
            cbz     x19, G_M000_IG12
            ldrb    w2, [fp, #0x18]
            mov     x0, x19
            mov     w1, wzr
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG12:                ;; offset=0x015C
            ldr     s0, [fp, #0x14]
            fmov    s16, s0
            b       G_M000_IG14

G_M000_IG13:                ;; offset=0x0168
            fmov    s17, #1.0000
            fdiv    s16, s17, s16

G_M000_IG14:                ;; offset=0x0170
            ldr     s17, [@RWD24]
            ldr     s0, [fp, #0x44]
            fmul    s0, s0, s17
            ldr     s17, [@RWD28]
            ldr     s1, [fp, #0x30]
            fmul    s17, s1, s17
            fadd    s0, s0, s17
            ldr     s17, [@RWD32]
            fmul    s16, s16, s17
            fadd    s0, s0, s16
            ldr     s16, [@RWD36]
            fadd    s0, s0, s16

G_M000_IG15:                ;; offset=0x01A0
            ldp     x19, x20, [sp, #0x50]
            ldr     d8, [sp, #0x48]
            ldp     fp, lr, [sp], #0x60
            ret     lr

RWD00  	dd	3F3F62FEh		;  0.747604
RWD04  	dq	3A83126F3A83126Fh
RWD12  	dd	4398852Ah		;    305.04
RWD16  	dd	400C5322h		;   2.19257
RWD20  	dd	3F0C5322h		;  0.548144
RWD24  	dd	414E7F18h		;    12.906
RWD28  	dd	40A0B47Bh		;   5.02203
RWD32  	dd	404F0F94h		;   3.23533
RWD36  	dd	BF3DE353h		;  -0.74175

; Total bytes of code 432

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ModulateScalar(System.ReadOnlySpan`1[float],int,float,bool,DotJpegli.Internal.AqRationalDiagnostics):float (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 0 single block inlinees; 4 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0xA0]!
            stp     d8, d9, [sp, #0x28]
            stp     d10, d11, [sp, #0x38]
            str     d12, [sp, #0x48]
            stp     x19, x20, [sp, #0x50]
            stp     x21, x22, [sp, #0x60]
            stp     x23, x24, [sp, #0x70]
            stp     x25, x26, [sp, #0x80]
            stp     x27, x28, [sp, #0x90]
            mov     fp, sp
            stp     xzr, xzr, [fp, #0x18]
            mov     x19, x0
            mov     w20, w1
            mov     w22, w2
            fmov    s8, s0
            mov     w23, w3
            mov     x21, x4

G_M000_IG02:                ;; offset=0x0044
            movi    v9.16b, #0
            movi    v10.16b, #0
            mov     w24, wzr

G_M000_IG03:                ;; offset=0x0050
            mul     w25, w24, w22
            cmp     w24, #7
            bne     G_M000_IG05

G_M000_IG04:                ;; offset=0x005C
            mov     w26, w25
            b       G_M000_IG06

G_M000_IG05:                ;; offset=0x0064
            add     w26, w25, w22

G_M000_IG06:                ;; offset=0x0068
            cmp     w25, w20
            bhs     G_M000_IG19
            ldr     s11, [x19, w25, UXTW #2]
            mov     w27, wzr
            uxtb    w28, w23

G_M000_IG07:                ;; offset=0x007C
            add     w0, w25, w27
            add     w0, w0, #1
            cmp     w0, w20
            bhs     G_M000_IG19
            ldr     s12, [x19, w0, UXTW #2]
            add     w0, w26, w27
            cmp     w0, w20
            bhs     G_M000_IG19
            ldr     s0, [x19, w0, UXTW #2]
            fsub    s0, s11, s0
            fabs    s0, s0
            fsub    s1, s11, s12
            fabs    s1, s1
            fadd    s1, s1, s9
            fadd    s9, s0, s1
            ldr     s0, [@RWD00]
            fadd    s0, s11, s0
            movi    v1.2s, #0
            fmax    s0, s0, s1
            fmul    s1, s0, s0
            ldr     s16, [@RWD04]
            fmul    s16, s1, s16
            ldr     s17, [@RWD08]
            fadd    s16, s16, s17
            ldr     s17, [@RWD12]
            fmul    s0, s0, s17
            fmul    s0, s0, s1
            ldr     s1, [@RWD16]
            fadd    s1, s0, s1
            strb    wzr, [fp, #0x20]
            cbz     w28, G_M000_IG10

G_M000_IG08:                ;; offset=0x00F8
            add     x0, fp, #32
            fmov    s0, s16
            movz    x1, #0x4948
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            fmov    s11, s0
            cbz     x21, G_M000_IG09
            ldrb    w2, [fp, #0x20]
            mov     x0, x21
            mov     w1, #1
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG09:                ;; offset=0x013C
            b       G_M000_IG11

G_M000_IG10:                ;; offset=0x0140
            fdiv    s11, s16, s1

G_M000_IG11:                ;; offset=0x0144
            fadd    s10, s10, s11
            fmov    s11, s12
            add     w27, w27, #1
            cmp     w27, #7
            blt     G_M000_IG07

G_M000_IG12:                ;; offset=0x0158
            add     w0, w26, #7
            cmp     w0, w20
            bhs     G_M000_IG19
            ldr     s0, [x19, w0, UXTW #2]
            fsub    s0, s11, s0
            fabs    s0, s0
            fadd    s9, s0, s9
            ldr     s0, [@RWD00]
            fadd    s0, s11, s0
            movi    v1.2s, #0
            fmax    s0, s0, s1
            fmul    s1, s0, s0
            ldr     s16, [@RWD04]
            fmul    s16, s1, s16
            ldr     s17, [@RWD08]
            fadd    s16, s16, s17
            ldr     s17, [@RWD12]
            fmul    s0, s0, s17
            fmul    s0, s0, s1
            ldr     s1, [@RWD16]
            fadd    s1, s0, s1
            strb    wzr, [fp, #0x18]
            cbz     w28, G_M000_IG15

G_M000_IG13:                ;; offset=0x01B4
            add     x0, fp, #24
            fmov    s0, s16
            movz    x1, #0x4948
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            fmov    s11, s0
            cbz     x21, G_M000_IG14
            ldrb    w2, [fp, #0x18]
            mov     x0, x21
            mov     w1, #1
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG14:                ;; offset=0x01F8
            b       G_M000_IG16

G_M000_IG15:                ;; offset=0x01FC
            fdiv    s11, s16, s1

G_M000_IG16:                ;; offset=0x0200
            fadd    s10, s10, s11
            add     w24, w24, #1
            cmp     w24, #8
            blt     G_M000_IG03

G_M000_IG17:                ;; offset=0x0210
            ldr     s0, [@RWD20]
            fmul    s0, s10, s0
            mov     w0, w28
            mov     x1, x21
            movz    x2, #0x4918
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.AdaptiveQuantization:FastLog2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float
            ldr     s16, [@RWD24]
            fmul    s0, s0, s16
            ldr     s16, [@RWD28]
            fmul    s16, s9, s16
            fadd    s16, s16, s8
            fadd    s0, s0, s16

G_M000_IG18:                ;; offset=0x024C
            ldp     x27, x28, [sp, #0x90]
            ldp     x25, x26, [sp, #0x80]
            ldp     x23, x24, [sp, #0x70]
            ldp     x21, x22, [sp, #0x60]
            ldp     x19, x20, [sp, #0x50]
            ldr     d12, [sp, #0x48]
            ldp     d10, d11, [sp, #0x38]
            ldp     d8, d9, [sp, #0x28]
            ldp     fp, lr, [sp], #0xA0
            ret     lr

G_M000_IG19:                ;; offset=0x0274
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dd	42233332h		;      40.8
RWD04  	dd	42EF0C23h		;   119.524
RWD08  	dd	44228FFEh		;    650.25
RWD12  	dd	3B1DEA78h		; 0.00240961
RWD16  	dd	449E380Dh		;   1265.75
RWD20  	dd	38808081h		; 6.12745e-05
RWD24  	dd	BDDC6A08h		; -0.107624
RWD28  	dd	B8933E1Eh		; -7.02108e-05

; Total bytes of code 636

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:FastLog2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data
; 0 inlinees with PGO data; 0 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x30]!
            stp     x19, x20, [sp, #0x20]
            mov     fp, sp
            str     xzr, [fp, #0x18]
            mov     x19, x1

G_M000_IG02:                ;; offset=0x0014
            mov     w1, v0.s[0]
            movz    w2, #0x5555
            movk    w2, #0xC0D5 LSL #16
            add     w2, w1, w2
            asr     w20, w2, #23
            sub     w1, w1, w20,  LSL #23
            fmov    s0, w1
            fmov    s1, #1.0000
            fsub    s0, s0, s1
            ldr     s1, [@RWD00]
            fmul    s1, s0, s1
            ldr     s16, [@RWD04]
            fadd    s1, s1, s16
            fmul    s1, s1, s0
            ldr     s16, [@RWD08]
            fadd    s1, s1, s16
            ldr     s16, [@RWD12]
            fmul    s16, s0, s16
            ldr     s17, [@RWD16]
            fadd    s16, s16, s17
            fmul    s0, s16, s0
            ldr     s16, [@RWD20]
            fsub    s0, s0, s16
            tst     w0, #255
            beq     G_M000_IG05

G_M000_IG03:                ;; offset=0x0078
            add     x0, fp, #24
            movz    x1, #0x4948
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            str     s0, [fp, #0x14]
            cbz     x19, G_M000_IG04
            ldrb    w2, [fp, #0x18]
            mov     x0, x19
            mov     w1, #2
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG04:                ;; offset=0x00B8
            ldr     s0, [fp, #0x14]
            b       G_M000_IG06

G_M000_IG05:                ;; offset=0x00C0
            fdiv    s0, s0, s1

G_M000_IG06:                ;; offset=0x00C4
            scvtf   s16, w20
            fadd    s0, s0, s16

G_M000_IG07:                ;; offset=0x00CC
            ldp     x19, x20, [sp, #0x20]
            ldp     fp, lr, [sp], #0x30
            ret     lr

RWD00  	dd	3E32458Ch		;  0.174093
RWD04  	dd	3F813CEDh		;   1.00967
RWD08  	dd	3F7D8625h		;  0.990328
RWD12  	dd	3F3E11C7h		;  0.742459
RWD16  	dd	3FB6E02Bh		;   1.42872
RWD20  	dd	35F85AB0h		; 1.85038e-06

; Total bytes of code 216

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:FastPow2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data
; 0 inlinees with PGO data; 0 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x30]!
            str     x19, [sp, #0x28]
            mov     fp, sp
            str     xzr, [fp, #0x20]
            mov     x19, x1

G_M000_IG02:                ;; offset=0x0014
            frintm  s1, s0
            fcvtzs  w1, s1
            add     w1, w1, #127
            lsl     w1, w1, #23
            fmov    s16, w1
            fsub    s0, s0, s1
            ldr     s1, [@RWD00]
            fmul    s1, s0, s1
            ldr     s17, [@RWD04]
            fsub    s1, s1, s17
            fmul    s1, s1, s0
            ldr     s17, [@RWD08]
            fsub    s1, s1, s17
            fmul    s1, s1, s0
            ldr     s17, [@RWD12]
            fadd    s1, s1, s17
            ldr     s17, [@RWD16]
            fadd    s17, s0, s17
            fmul    s17, s17, s0
            ldr     s18, [@RWD20]
            fadd    s17, s17, s18
            fmul    s0, s17, s0
            ldr     s17, [@RWD24]
            fadd    s0, s0, s17
            fmul    s0, s0, s16
            tst     w0, #255
            beq     G_M000_IG05

G_M000_IG03:                ;; offset=0x0080
            add     x0, fp, #32
            movz    x1, #0x4948
            movk    x1, #0x695F LSL #16
            movk    x1, #0x7FF8 LSL #32
            ldr     x1, [x1]
            blr     x1      // code for DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float
            str     s0, [fp, #0x1C]
            cbz     x19, G_M000_IG04
            ldrb    w2, [fp, #0x20]
            mov     x0, x19
            mov     w1, #3
            movz    x3, #0x4960
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this

G_M000_IG04:                ;; offset=0x00C0
            ldr     s0, [fp, #0x1C]
            b       G_M000_IG06

G_M000_IG05:                ;; offset=0x00C8
            fdiv    s0, s0, s1

G_M000_IG06:                ;; offset=0x00CC
            ldr     x19, [sp, #0x28]
            ldp     fp, lr, [sp], #0x30
            ret     lr

RWD00  	dd	3E5749EEh		;  0.210243
RWD04  	dd	3CB621BEh		; 0.0222329
RWD08  	dd	419B8831h		;   19.4415
RWD12  	dd	42C519F1h		;   98.5507
RWD16  	dd	4122CC6Bh		;   10.1749
RWD20  	dd	424379A1h		;   48.8688
RWD24  	dd	42C519F0h		;   98.5507

; Total bytes of code 216

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:DerivativeRatio(System.Numerics.Vector`1[float],bool):System.Numerics.Vector`1[float] (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data
; 0 inlinees with PGO data; 4 single block inlinees; 0 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x10]!
            mov     fp, sp

G_M000_IG02:                ;; offset=0x0008
            movi    v16.4s, #0
            fmax    v0.4s, v0.4s, v16.4s
            fmul    v16.4s, v0.4s, v0.4s
            ldr     q17, [@RWD00]
            fmul    v17.4s, v16.4s, v17.4s
            ldr     q18, [@RWD16]
            fadd    v17.4s, v17.4s, v18.4s
            ldr     q18, [@RWD32]
            fmul    v0.4s, v0.4s, v18.4s
            fmul    v0.4s, v0.4s, v16.4s
            ldr     q16, [@RWD48]
            fadd    v16.4s, v0.4s, v16.4s
            tst     w0, #255
            beq     G_M000_IG05

G_M000_IG03:                ;; offset=0x0040
            fdiv    v0.4s, v17.4s, v16.4s

G_M000_IG04:                ;; offset=0x0044
            ldp     fp, lr, [sp], #0x10
            ret     lr

G_M000_IG05:                ;; offset=0x004C
            fdiv    v0.4s, v16.4s, v17.4s

G_M000_IG06:                ;; offset=0x0050
            ldp     fp, lr, [sp], #0x10
            ret     lr

RWD00  	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD16  	dq	44228FFE44228FFEh, 44228FFE44228FFEh
RWD32  	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD48  	dq	449E380D449E380Dh, 449E380D449E380Dh

; Total bytes of code 88

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ModulateVector(int,int,float):float:this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 34 single block inlinees; 10 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x20]!
            str     d8, [sp, #0x18]
            mov     fp, sp
            str     s0, [fp, #0x14]

G_M000_IG02:                ;; offset=0x0010
            movi    v8.16b, #0
            movi    v16.16b, #0
            ldr     w3, [x0, #0x54]
            madd    w1, w2, w3, w1
            add     w1, w1, #1
            ldr     q17, [@RWD00]
            ldr     x2, [x0, #0x10]
            mov     x4, x2
            tbnz    w1, #31, G_M000_IG12
            ldr     w5, [x4, #0x08]
            sub     w6, w5, w1
            cmp     w6, #4
            blt     G_M000_IG12
            cmp     w1, w5
            bhs     G_M000_IG13
            add     x4, x4, #16
            ubfiz   x6, x1, #2, #32
            ldr     q18, [x4, x6]
            add     w6, w1, #4
            sub     w7, w5, w6
            cmp     w7, #4
            blt     G_M000_IG12
            cmp     w6, w5
            bhs     G_M000_IG13
            ubfiz   x6, x6, #2, #32
            ldr     q19, [x4, x6]
            mov     w6, wzr
            b       G_M000_IG09
            align   [0 bytes for IG03]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG03:                ;; offset=0x0080
            mov     v20.16b, v18.16b

G_M000_IG04:                ;; offset=0x0084
            cmp     w6, #7
            bne     G_M000_IG06

G_M000_IG05:                ;; offset=0x008C
            mov     v21.16b, v19.16b
            b       G_M000_IG07

G_M000_IG06:                ;; offset=0x0094
            add     w7, w1, w3
            add     w7, w7, #4
            sub     w8, w5, w7
            cmp     w8, #4
            ccmp    w7, #0, nc, ge
            blt     G_M000_IG12
            cmp     w7, w5
            bhs     G_M000_IG13
            ubfiz   x7, x7, #2, #32
            ldr     q21, [x4, x7]

G_M000_IG07:                ;; offset=0x00BC
            add     w7, w1, #1
            sub     w8, w5, w7
            cmp     w8, #4
            ccmp    w7, #0, nc, ge
            blt     G_M000_IG12
            cmp     w7, w5
            bhs     G_M000_IG13
            ubfiz   x7, x7, #2, #32
            ldr     q22, [x4, x7]
            fsub    v22.4s, v18.4s, v22.4s
            fabs    v22.4s, v22.4s
            add     w7, w1, #5
            sub     w8, w5, w7
            cmp     w8, #4
            ccmp    w7, #0, nc, ge
            blt     G_M000_IG12
            cmp     w7, w5
            bhs     G_M000_IG13
            ubfiz   x7, x7, #2, #32
            ldr     q23, [x4, x7]
            fsub    v23.4s, v19.4s, v23.4s
            fabs    v23.4s, v23.4s
            fsub    v24.4s, v18.4s, v20.4s
            fabs    v24.4s, v24.4s
            dup     s25, v22.s[0]
            fadd    s25, s8, s25
            dup     s26, v24.s[0]
            fadd    s25, s25, s26
            dup     s26, v22.s[1]
            fadd    s25, s25, s26
            dup     s26, v24.s[1]
            fadd    s25, s25, s26
            dup     s26, v22.s[2]
            fadd    s25, s25, s26
            dup     s26, v24.s[2]
            fadd    s25, s25, s26
            dup     s22, v22.s[3]
            fadd    s22, s25, s22
            dup     s24, v24.s[3]
            fadd    s22, s22, s24
            fsub    v24.4s, v19.4s, v21.4s
            fabs    v24.4s, v24.4s
            dup     s25, v23.s[0]
            fadd    s22, s22, s25
            dup     s25, v24.s[0]
            fadd    s22, s22, s25
            dup     s25, v23.s[1]
            fadd    s22, s22, s25
            dup     s25, v24.s[1]
            fadd    s22, s22, s25
            dup     s23, v23.s[2]
            fadd    s22, s22, s23
            dup     s23, v24.s[2]
            fadd    s22, s22, s23
            dup     s23, v24.s[3]
            fadd    s8, s22, s23
            fadd    v18.4s, v18.4s, v17.4s
            movi    v22.4s, #0
            fmax    v18.4s, v18.4s, v22.4s
            fmul    v22.4s, v18.4s, v18.4s
            ldr     q23, [@RWD16]
            fmul    v18.4s, v18.4s, v23.4s
            fmul    v18.4s, v18.4s, v22.4s
            ldr     q24, [@RWD32]
            fadd    v18.4s, v18.4s, v24.4s
            ldr     q25, [@RWD48]
            fmul    v22.4s, v22.4s, v25.4s
            ldr     q26, [@RWD64]
            fadd    v22.4s, v22.4s, v26.4s
            fdiv    v18.4s, v22.4s, v18.4s
            dup     s22, v18.s[0]
            fadd    s16, s16, s22
            dup     s22, v18.s[1]
            fadd    s16, s16, s22
            dup     s22, v18.s[2]
            fadd    s16, s16, s22
            dup     s18, v18.s[3]
            fadd    s16, s16, s18
            fadd    v19.4s, v19.4s, v17.4s
            movi    v18.4s, #0
            fmax    v18.4s, v19.4s, v18.4s
            fmul    v19.4s, v18.4s, v18.4s
            fmul    v18.4s, v18.4s, v23.4s
            fmul    v18.4s, v18.4s, v19.4s
            fadd    v18.4s, v18.4s, v24.4s
            fmul    v19.4s, v19.4s, v25.4s
            fadd    v19.4s, v19.4s, v26.4s
            fdiv    v18.4s, v19.4s, v18.4s
            dup     s19, v18.s[0]
            fadd    s16, s16, s19
            dup     s19, v18.s[1]
            fadd    s16, s16, s19
            dup     s19, v18.s[2]
            fadd    s16, s16, s19
            dup     s18, v18.s[3]
            fadd    s16, s16, s18
            mov     v18.16b, v20.16b
            mov     v19.16b, v21.16b
            add     w1, w1, w3
            add     w6, w6, #1
            cmp     w6, #8
            bge     G_M000_IG10

G_M000_IG08:                ;; offset=0x0254
            cmp     w6, #7
            beq     G_M000_IG03

G_M000_IG09:                ;; offset=0x025C
            add     w7, w1, w3
            sub     w8, w5, w7
            cmp     w8, #4
            ccmp    w7, #0, nc, ge
            blt     G_M000_IG12
            cmp     w7, w5
            bhs     G_M000_IG13
            ubfiz   x7, x7, #2, #32
            ldr     q20, [x4, x7]
            b       G_M000_IG04

G_M000_IG10:                ;; offset=0x0284
            ldr     s17, [@RWD80]
            fmul    s0, s16, s17
            movz    x0, #0x4528
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for DotJpegli.Internal.BaselineAdaptiveQuantization:FastLog2(float):float
            ldr     s16, [@RWD84]
            fmul    s0, s0, s16
            ldr     s16, [@RWD88]
            fmul    s16, s8, s16
            ldr     s17, [fp, #0x14]
            fadd    s16, s16, s17
            fadd    s0, s0, s16

G_M000_IG11:                ;; offset=0x02BC
            ldr     d8, [sp, #0x18]
            ldp     fp, lr, [sp], #0x20
            ret     lr

G_M000_IG12:                ;; offset=0x02C8
            movz    x0, #0x4750
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()
            brk     #0

G_M000_IG13:                ;; offset=0x02E0
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dq	4223333242233332h, 4223333242233332h
RWD16  	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD32  	dq	449E380D449E380Dh, 449E380D449E380Dh
RWD48  	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD64  	dq	44228FFE44228FFEh, 44228FFE44228FFEh
RWD80  	dd	38808081h		; 6.12745e-05
RWD84  	dd	BDDC6A08h		; -0.107624
RWD88  	dd	B8933E1Eh		; -7.02108e-05

; Total bytes of code 744

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:DerivativeRatio(System.Numerics.Vector`1[float],bool):System.Numerics.Vector`1[float] (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; partially interruptible
; No PGO data
; 0 inlinees with PGO data; 4 single block inlinees; 0 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x10]!
            mov     fp, sp

G_M000_IG02:                ;; offset=0x0008
            movi    v16.4s, #0
            fmax    v0.4s, v0.4s, v16.4s
            fmul    v16.4s, v0.4s, v0.4s
            ldr     q17, [@RWD00]
            fmul    v17.4s, v16.4s, v17.4s
            ldr     q18, [@RWD16]
            fadd    v17.4s, v17.4s, v18.4s
            ldr     q18, [@RWD32]
            fmul    v0.4s, v0.4s, v18.4s
            fmul    v0.4s, v0.4s, v16.4s
            ldr     q16, [@RWD48]
            fadd    v16.4s, v0.4s, v16.4s
            tst     w0, #255
            beq     G_M000_IG05

G_M000_IG03:                ;; offset=0x0040
            fdiv    v0.4s, v17.4s, v16.4s

G_M000_IG04:                ;; offset=0x0044
            ldp     fp, lr, [sp], #0x10
            ret     lr

G_M000_IG05:                ;; offset=0x004C
            fdiv    v0.4s, v16.4s, v17.4s

G_M000_IG06:                ;; offset=0x0050
            ldp     fp, lr, [sp], #0x10
            ret     lr

RWD00  	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD16  	dq	44228FFE44228FFEh, 44228FFE44228FFEh
RWD32  	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD48  	dq	449E380D449E380Dh, 449E380D449E380Dh

; Total bytes of code 88

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ModulateVector(int,int,float):float:this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 34 single block inlinees; 10 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x20]!
            str     d8, [sp, #0x18]
            mov     fp, sp
            str     s0, [fp, #0x14]

G_M000_IG02:                ;; offset=0x0010
            movi    v8.16b, #0
            movi    v16.16b, #0
            ldr     w3, [x0, #0x4C]
            madd    w1, w2, w3, w1
            add     w1, w1, #1
            ldr     q17, [@RWD00]
            ldr     x2, [x0, #0x10]
            mov     x4, x2
            tbnz    w1, #31, G_M000_IG12
            ldr     w5, [x4, #0x08]
            sub     w6, w5, w1
            cmp     w6, #4
            blt     G_M000_IG12
            cmp     w1, w5
            bhs     G_M000_IG13
            add     x4, x4, #16
            ubfiz   x6, x1, #2, #32
            ldr     q18, [x4, x6]
            add     w6, w1, #4
            sub     w7, w5, w6
            cmp     w7, #4
            blt     G_M000_IG12
            cmp     w6, w5
            bhs     G_M000_IG13
            ubfiz   x6, x6, #2, #32
            ldr     q19, [x4, x6]
            mov     w6, wzr
            b       G_M000_IG09
            align   [0 bytes for IG03]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG03:                ;; offset=0x0080
            mov     v20.16b, v18.16b

G_M000_IG04:                ;; offset=0x0084
            cmp     w6, #7
            bne     G_M000_IG06

G_M000_IG05:                ;; offset=0x008C
            mov     v21.16b, v19.16b
            b       G_M000_IG07

G_M000_IG06:                ;; offset=0x0094
            add     w7, w1, w3
            add     w7, w7, #4
            sub     w8, w5, w7
            cmp     w8, #4
            ccmp    w7, #0, nc, ge
            blt     G_M000_IG12
            cmp     w7, w5
            bhs     G_M000_IG13
            ubfiz   x7, x7, #2, #32
            ldr     q21, [x4, x7]

G_M000_IG07:                ;; offset=0x00BC
            add     w7, w1, #1
            sub     w8, w5, w7
            cmp     w8, #4
            ccmp    w7, #0, nc, ge
            blt     G_M000_IG12
            cmp     w7, w5
            bhs     G_M000_IG13
            ubfiz   x7, x7, #2, #32
            ldr     q22, [x4, x7]
            fsub    v22.4s, v18.4s, v22.4s
            fabs    v22.4s, v22.4s
            add     w7, w1, #5
            sub     w8, w5, w7
            cmp     w8, #4
            ccmp    w7, #0, nc, ge
            blt     G_M000_IG12
            cmp     w7, w5
            bhs     G_M000_IG13
            ubfiz   x7, x7, #2, #32
            ldr     q23, [x4, x7]
            fsub    v23.4s, v19.4s, v23.4s
            fabs    v23.4s, v23.4s
            fsub    v24.4s, v18.4s, v20.4s
            fabs    v24.4s, v24.4s
            dup     s25, v22.s[0]
            fadd    s25, s8, s25
            dup     s26, v24.s[0]
            fadd    s25, s25, s26
            dup     s26, v22.s[1]
            fadd    s25, s25, s26
            dup     s26, v24.s[1]
            fadd    s25, s25, s26
            dup     s26, v22.s[2]
            fadd    s25, s25, s26
            dup     s26, v24.s[2]
            fadd    s25, s25, s26
            dup     s22, v22.s[3]
            fadd    s22, s25, s22
            dup     s24, v24.s[3]
            fadd    s22, s22, s24
            fsub    v24.4s, v19.4s, v21.4s
            fabs    v24.4s, v24.4s
            dup     s25, v23.s[0]
            fadd    s22, s22, s25
            dup     s25, v24.s[0]
            fadd    s22, s22, s25
            dup     s25, v23.s[1]
            fadd    s22, s22, s25
            dup     s25, v24.s[1]
            fadd    s22, s22, s25
            dup     s23, v23.s[2]
            fadd    s22, s22, s23
            dup     s23, v24.s[2]
            fadd    s22, s22, s23
            dup     s23, v24.s[3]
            fadd    s8, s22, s23
            fadd    v18.4s, v18.4s, v17.4s
            movi    v22.4s, #0
            fmax    v18.4s, v18.4s, v22.4s
            fmul    v22.4s, v18.4s, v18.4s
            ldr     q23, [@RWD16]
            fmul    v18.4s, v18.4s, v23.4s
            fmul    v18.4s, v18.4s, v22.4s
            ldr     q24, [@RWD32]
            fadd    v18.4s, v18.4s, v24.4s
            ldr     q25, [@RWD48]
            fmul    v22.4s, v22.4s, v25.4s
            ldr     q26, [@RWD64]
            fadd    v22.4s, v22.4s, v26.4s
            fdiv    v18.4s, v22.4s, v18.4s
            dup     s22, v18.s[0]
            fadd    s16, s16, s22
            dup     s22, v18.s[1]
            fadd    s16, s16, s22
            dup     s22, v18.s[2]
            fadd    s16, s16, s22
            dup     s18, v18.s[3]
            fadd    s16, s16, s18
            fadd    v19.4s, v19.4s, v17.4s
            movi    v18.4s, #0
            fmax    v18.4s, v19.4s, v18.4s
            fmul    v19.4s, v18.4s, v18.4s
            fmul    v18.4s, v18.4s, v23.4s
            fmul    v18.4s, v18.4s, v19.4s
            fadd    v18.4s, v18.4s, v24.4s
            fmul    v19.4s, v19.4s, v25.4s
            fadd    v19.4s, v19.4s, v26.4s
            fdiv    v18.4s, v19.4s, v18.4s
            dup     s19, v18.s[0]
            fadd    s16, s16, s19
            dup     s19, v18.s[1]
            fadd    s16, s16, s19
            dup     s19, v18.s[2]
            fadd    s16, s16, s19
            dup     s18, v18.s[3]
            fadd    s16, s16, s18
            mov     v18.16b, v20.16b
            mov     v19.16b, v21.16b
            add     w1, w1, w3
            add     w6, w6, #1
            cmp     w6, #8
            bge     G_M000_IG10

G_M000_IG08:                ;; offset=0x0254
            cmp     w6, #7
            beq     G_M000_IG03

G_M000_IG09:                ;; offset=0x025C
            add     w7, w1, w3
            sub     w8, w5, w7
            cmp     w8, #4
            ccmp    w7, #0, nc, ge
            blt     G_M000_IG12
            cmp     w7, w5
            bhs     G_M000_IG13
            ubfiz   x7, x7, #2, #32
            ldr     q20, [x4, x7]
            b       G_M000_IG04

G_M000_IG10:                ;; offset=0x0284
            ldr     s17, [@RWD80]
            fmul    s0, s16, s17
            movz    x0, #0x47F8
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for DotJpegli.Internal.StencilAdaptiveQuantization:FastLog2(float):float
            ldr     s16, [@RWD84]
            fmul    s0, s0, s16
            ldr     s16, [@RWD88]
            fmul    s16, s8, s16
            ldr     s17, [fp, #0x14]
            fadd    s16, s16, s17
            fadd    s0, s0, s16

G_M000_IG11:                ;; offset=0x02BC
            ldr     d8, [sp, #0x18]
            ldp     fp, lr, [sp], #0x20
            ret     lr

G_M000_IG12:                ;; offset=0x02C8
            movz    x0, #0x4750
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()
            brk     #0

G_M000_IG13:                ;; offset=0x02E0
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dq	4223333242233332h, 4223333242233332h
RWD16  	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD32  	dq	449E380D449E380Dh, 449E380D449E380Dh
RWD48  	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD64  	dq	44228FFE44228FFEh, 44228FFE44228FFEh
RWD80  	dd	38808081h		; 6.12745e-05
RWD84  	dd	BDDC6A08h		; -0.107624
RWD88  	dd	B8933E1Eh		; -7.02108e-05

; Total bytes of code 744

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ModulateVector(int,int,float):float:this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 34 single block inlinees; 12 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x80]!
            stp     d8, d9, [sp, #0x28]
            stp     d10, d11, [sp, #0x38]
            stp     d12, d13, [sp, #0x48]
            stp     d14, d15, [sp, #0x58]
            stp     x19, x20, [sp, #0x68]
            str     x21, [sp, #0x78]
            mov     fp, sp
            str     s0, [fp, #0x24]
            mov     x19, x0

G_M000_IG02:                ;; offset=0x0028
            movi    v8.16b, #0
            movi    v9.16b, #0
            ldr     w0, [x19, #0x54]
            madd    w1, w2, w0, w1
            add     w20, w1, #1
            ldr     q10, [@RWD00]
            str     q10, [fp, #0x10]
            ldr     x1, [x19, #0x18]
            tbnz    w20, #31, G_M000_IG18
            ldr     w0, [x1, #0x08]
            sub     w2, w0, w20
            cmp     w2, #4
            blt     G_M000_IG18
            cmp     w20, w0
            bhs     G_M000_IG19
            add     x1, x1, #16
            ubfiz   x2, x20, #2, #32
            ldr     q11, [x1, x2]
            add     w2, w20, #4
            sub     w3, w0, w2
            cmp     w3, #4
            blt     G_M000_IG18
            cmp     w2, w0
            bhs     G_M000_IG19
            ubfiz   x0, x2, #2, #32
            ldr     q12, [x1, x0]
            mov     w21, wzr
            b       G_M000_IG15

G_M000_IG03:                ;; offset=0x0098
            mov     v13.16b, v11.16b

G_M000_IG04:                ;; offset=0x009C
            cmp     w21, #7
            bne     G_M000_IG06

G_M000_IG05:                ;; offset=0x00A4
            mov     v14.16b, v12.16b
            b       G_M000_IG07

G_M000_IG06:                ;; offset=0x00AC
            ldr     x1, [x19, #0x18]
            ldr     w0, [x19, #0x54]
            add     w0, w20, w0
            add     w0, w0, #4
            tbnz    w0, #31, G_M000_IG18
            ldr     w2, [x1, #0x08]
            sub     w3, w2, w0
            cmp     w3, #4
            blt     G_M000_IG18
            cmp     w0, w2
            bhs     G_M000_IG19
            add     x1, x1, #16
            ubfiz   x2, x0, #2, #32
            ldr     q14, [x1, x2]

G_M000_IG07:                ;; offset=0x00E4
            ldr     x1, [x19, #0x18]
            add     w0, w20, #1
            tbnz    w0, #31, G_M000_IG18
            ldr     w2, [x1, #0x08]
            sub     w3, w2, w0
            cmp     w3, #4
            blt     G_M000_IG18
            cmp     w0, w2
            bhs     G_M000_IG19
            add     x1, x1, #16
            ubfiz   x0, x0, #2, #32
            ldr     q1, [x1, x0]
            fsub    v1.4s, v11.4s, v1.4s
            fabs    v1.4s, v1.4s
            add     w0, w20, #5
            sub     w3, w2, w0
            cmp     w3, #4
            ccmp    w0, #0, nc, ge
            blt     G_M000_IG18
            cmp     w0, w2
            bhs     G_M000_IG19
            ubfiz   x0, x0, #2, #32
            ldr     q16, [x1, x0]
            fsub    v16.4s, v12.4s, v16.4s
            fabs    v16.4s, v16.4s
            fsub    v17.4s, v11.4s, v13.4s
            fabs    v17.4s, v17.4s
            dup     s18, v1.s[0]
            fadd    s18, s8, s18
            dup     s19, v17.s[0]
            fadd    s18, s18, s19
            dup     s19, v1.s[1]
            fadd    s18, s18, s19
            dup     s19, v17.s[1]
            fadd    s18, s18, s19
            dup     s19, v1.s[2]
            fadd    s18, s18, s19
            dup     s19, v17.s[2]
            fadd    s18, s18, s19
            dup     s1, v1.s[3]
            fadd    s1, s18, s1
            dup     s17, v17.s[3]
            fadd    s1, s1, s17
            fsub    v17.4s, v12.4s, v14.4s
            fabs    v17.4s, v17.4s
            dup     s18, v16.s[0]
            fadd    s1, s1, s18
            dup     s18, v17.s[0]
            fadd    s1, s1, s18
            dup     s18, v16.s[1]
            fadd    s1, s1, s18
            dup     s18, v17.s[1]
            fadd    s1, s1, s18
            dup     s16, v16.s[2]
            fadd    s1, s1, s16
            dup     s16, v17.s[2]
            fadd    s1, s1, s16
            dup     s16, v17.s[3]
            fadd    s8, s1, s16
            fadd    v1.4s, v11.4s, v10.4s
            ldrb    w1, [x19, #0x69]
            ldr     x0, [x19, #0x10]
            movi    v16.4s, #0
            fmax    v1.4s, v1.4s, v16.4s
            fmul    v16.4s, v1.4s, v1.4s
            ldr     q17, [@RWD16]
            fmul    v17.4s, v16.4s, v17.4s
            ldr     q18, [@RWD32]
            fadd    v17.4s, v17.4s, v18.4s
            ldr     q18, [@RWD48]
            fmul    v1.4s, v1.4s, v18.4s
            fmul    v1.4s, v1.4s, v16.4s
            ldr     q16, [@RWD64]
            fadd    v1.4s, v1.4s, v16.4s
            cbz     w1, G_M000_IG09

G_M000_IG08:                ;; offset=0x0210
            mov     v0.16b, v17.16b
            mov     x1, x0
            mov     w0, #1
            movz    x2, #0x49A8
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            mov     v11.d[0], v12.d[1]
            mov     v15.d[0], v13.d[1]
            mov     v10.d[0], v14.d[1]
            blr     x2      // code for DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Numerics.Vector`1[float],System.Numerics.Vector`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Numerics.Vector`1[float]
            mov     v12.d[1], v11.d[0]
            mov     v13.d[1], v15.d[0]
            mov     v14.d[1], v10.d[0]
            mov     v1.16b, v0.16b
            b       G_M000_IG10

G_M000_IG09:                ;; offset=0x0250
            fdiv    v1.4s, v17.4s, v1.4s

G_M000_IG10:                ;; offset=0x0254
            dup     s16, v1.s[0]
            fadd    s16, s9, s16
            dup     s17, v1.s[1]
            fadd    s16, s16, s17
            dup     s17, v1.s[2]
            fadd    s16, s16, s17
            dup     s1, v1.s[3]
            fadd    s9, s16, s1
            ldr     q10, [fp, #0x10]
            fadd    v1.4s, v12.4s, v10.4s
            ldrb    w1, [x19, #0x69]
            ldr     x0, [x19, #0x10]
            movi    v16.4s, #0
            fmax    v1.4s, v1.4s, v16.4s
            fmul    v16.4s, v1.4s, v1.4s
            ldr     q17, [@RWD16]
            fmul    v17.4s, v16.4s, v17.4s
            ldr     q18, [@RWD32]
            fadd    v17.4s, v17.4s, v18.4s
            ldr     q18, [@RWD48]
            fmul    v1.4s, v1.4s, v18.4s
            fmul    v1.4s, v1.4s, v16.4s
            ldr     q16, [@RWD64]
            fadd    v1.4s, v1.4s, v16.4s
            cbz     w1, G_M000_IG12

G_M000_IG11:                ;; offset=0x02B8
            mov     v0.16b, v17.16b
            mov     x1, x0
            mov     w0, #1
            movz    x2, #0x49A8
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            mov     v15.d[0], v13.d[1]
            mov     v12.d[0], v14.d[1]
            mov     v11.d[0], v10.d[1]
            blr     x2      // code for DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Numerics.Vector`1[float],System.Numerics.Vector`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Numerics.Vector`1[float]
            mov     v13.d[1], v15.d[0]
            mov     v14.d[1], v12.d[0]
            mov     v10.d[1], v11.d[0]
            mov     v16.16b, v0.16b
            b       G_M000_IG13

G_M000_IG12:                ;; offset=0x02F8
            fdiv    v16.4s, v17.4s, v1.4s

G_M000_IG13:                ;; offset=0x02FC
            dup     s17, v16.s[0]
            fadd    s17, s9, s17
            dup     s18, v16.s[1]
            fadd    s17, s17, s18
            dup     s18, v16.s[2]
            fadd    s17, s17, s18
            dup     s16, v16.s[3]
            fadd    s9, s17, s16
            mov     v11.16b, v13.16b
            mov     v12.16b, v14.16b
            ldr     w0, [x19, #0x54]
            add     w20, w20, w0
            add     w21, w21, #1
            cmp     w21, #8
            bge     G_M000_IG16

G_M000_IG14:                ;; offset=0x0338
            cmp     w21, #7
            beq     G_M000_IG03

G_M000_IG15:                ;; offset=0x0340
            ldr     x1, [x19, #0x18]
            ldr     w0, [x19, #0x54]
            add     w0, w20, w0
            tbnz    w0, #31, G_M000_IG18
            ldr     w2, [x1, #0x08]
            sub     w3, w2, w0
            cmp     w3, #4
            blt     G_M000_IG18
            cmp     w0, w2
            bhs     G_M000_IG19
            add     x1, x1, #16
            ubfiz   x0, x0, #2, #32
            ldr     q13, [x1, x0]
            b       G_M000_IG04

G_M000_IG16:                ;; offset=0x0378
            ldr     s16, [@RWD80]
            fmul    s0, s9, s16
            ldrb    w0, [x19, #0x69]
            ldr     x1, [x19, #0x10]
            movz    x2, #0x4918
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            blr     x2      // code for DotJpegli.Internal.AdaptiveQuantization:FastLog2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float
            ldr     s16, [@RWD84]
            fmul    s0, s0, s16
            ldr     s16, [@RWD88]
            fmul    s16, s8, s16
            ldr     s17, [fp, #0x24]
            fadd    s16, s16, s17
            fadd    s0, s0, s16

G_M000_IG17:                ;; offset=0x03B8
            ldr     x21, [sp, #0x78]
            ldp     x19, x20, [sp, #0x68]
            ldp     d14, d15, [sp, #0x58]
            ldp     d12, d13, [sp, #0x48]
            ldp     d10, d11, [sp, #0x38]
            ldp     d8, d9, [sp, #0x28]
            ldp     fp, lr, [sp], #0x80
            ret     lr

G_M000_IG18:                ;; offset=0x03D8
            movz    x0, #0x4750
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()
            brk     #0

G_M000_IG19:                ;; offset=0x03F0
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dq	4223333242233332h, 4223333242233332h
RWD16  	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD32  	dq	44228FFE44228FFEh, 44228FFE44228FFEh
RWD48  	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD64  	dq	449E380D449E380Dh, 449E380D449E380Dh
RWD80  	dd	38808081h		; 6.12745e-05
RWD84  	dd	BDDC6A08h		; -0.107624
RWD88  	dd	B8933E1Eh		; -7.02108e-05

; Total bytes of code 1016

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:Dispose():this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 4 single block inlinees; 4 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x20]!
            stp     x19, x20, [sp, #0x10]
            mov     fp, sp
            mov     x19, x0

G_M000_IG02:                ;; offset=0x0010
            ldrb    w0, [x19, #0x6A]
            cbnz    w0, G_M000_IG08

G_M000_IG03:                ;; offset=0x0018
            mov     w0, #1
            strb    w0, [x19, #0x6A]
            str     xzr, [x19, #0x38]
            add     x0, x19, #24
            ldr     x1, [x0]
            movz    x20, #0x61B8
            movk    x20, #0xA15 LSL #16
            movk    x20, #548 LSL #32
            str     x20, [x0]
            ldr     w0, [x1, #0x08]
            cbz     w0, G_M000_IG04
            ldr     x0, [x19, #0x08]
            cbz     x0, G_M000_IG04
            mov     w2, wzr
            ldr     x3, [x0]
            ldr     x3, [x3, #0x40]
            ldr     x3, [x3, #0x28]
            blr     x3      // code for System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG04:                ;; offset=0x0060
            add     x0, x19, #32
            ldr     x1, [x0]
            str     x20, [x0]
            ldr     w0, [x1, #0x08]
            cbz     w0, G_M000_IG05
            ldr     x0, [x19, #0x08]
            cbz     x0, G_M000_IG05
            mov     w2, wzr
            ldr     x3, [x0]
            ldr     x3, [x3, #0x40]
            ldr     x3, [x3, #0x28]
            blr     x3      // code for System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG05:                ;; offset=0x0090
            add     x0, x19, #40
            ldr     x1, [x0]
            str     x20, [x0]
            ldr     w0, [x1, #0x08]
            cbz     w0, G_M000_IG06
            ldr     x0, [x19, #0x08]
            cbz     x0, G_M000_IG06
            mov     w2, wzr
            ldr     x3, [x0]
            ldr     x3, [x3, #0x40]
            ldr     x3, [x3, #0x28]
            blr     x3      // code for System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG06:                ;; offset=0x00C0
            add     x0, x19, #48
            ldr     x1, [x0]
            str     x20, [x0]
            ldr     w0, [x1, #0x08]
            cbz     w0, G_M000_IG08
            ldr     x0, [x19, #0x08]
            cbz     x0, G_M000_IG08
            mov     w2, wzr
            ldr     x3, [x0]
            ldr     x3, [x3, #0x40]
            ldr     x3, [x3, #0x28]

G_M000_IG07:                ;; offset=0x00EC
            ldp     x19, x20, [sp, #0x10]
            ldp     fp, lr, [sp], #0x20
            br      x3      // code for System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG08:                ;; offset=0x00F8
            ldp     x19, x20, [sp, #0x10]
            ldp     fp, lr, [sp], #0x20
            ret     lr

; Total bytes of code 260

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:Dispose():this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 4 single block inlinees; 4 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x20]!
            stp     x19, x20, [sp, #0x10]
            mov     fp, sp
            mov     x19, x0

G_M000_IG02:                ;; offset=0x0010
            ldrb    w0, [x19, #0x61]
            cbnz    w0, G_M000_IG08

G_M000_IG03:                ;; offset=0x0018
            mov     w0, #1
            strb    w0, [x19, #0x61]
            str     xzr, [x19, #0x30]
            add     x0, x19, #16
            ldr     x1, [x0]
            movz    x20, #0x61B8
            movk    x20, #0xA15 LSL #16
            movk    x20, #548 LSL #32
            str     x20, [x0]
            ldr     w0, [x1, #0x08]
            cbz     w0, G_M000_IG04
            ldr     x0, [x19, #0x08]
            cbz     x0, G_M000_IG04
            mov     w2, wzr
            ldr     x3, [x0]
            ldr     x3, [x3, #0x40]
            ldr     x3, [x3, #0x28]
            blr     x3      // code for System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG04:                ;; offset=0x0060
            add     x0, x19, #24
            ldr     x1, [x0]
            str     x20, [x0]
            ldr     w0, [x1, #0x08]
            cbz     w0, G_M000_IG05
            ldr     x0, [x19, #0x08]
            cbz     x0, G_M000_IG05
            mov     w2, wzr
            ldr     x3, [x0]
            ldr     x3, [x3, #0x40]
            ldr     x3, [x3, #0x28]
            blr     x3      // code for System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG05:                ;; offset=0x0090
            add     x0, x19, #32
            ldr     x1, [x0]
            str     x20, [x0]
            ldr     w0, [x1, #0x08]
            cbz     w0, G_M000_IG06
            ldr     x0, [x19, #0x08]
            cbz     x0, G_M000_IG06
            mov     w2, wzr
            ldr     x3, [x0]
            ldr     x3, [x3, #0x40]
            ldr     x3, [x3, #0x28]
            blr     x3      // code for System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG06:                ;; offset=0x00C0
            add     x0, x19, #40
            ldr     x1, [x0]
            str     x20, [x0]
            ldr     w0, [x1, #0x08]
            cbz     w0, G_M000_IG08
            ldr     x0, [x19, #0x08]
            cbz     x0, G_M000_IG08
            mov     w2, wzr
            ldr     x3, [x0]
            ldr     x3, [x3, #0x40]
            ldr     x3, [x3, #0x28]

G_M000_IG07:                ;; offset=0x00EC
            ldp     x19, x20, [sp, #0x10]
            ldp     fp, lr, [sp], #0x20
            br      x3      // code for System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG08:                ;; offset=0x00F8
            ldp     x19, x20, [sp, #0x10]
            ldp     fp, lr, [sp], #0x20
            ret     lr

; Total bytes of code 260

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:Dispose():this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 5 single block inlinees; 5 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x20]!
            stp     x19, x20, [sp, #0x10]
            mov     fp, sp
            mov     x19, x0

G_M000_IG02:                ;; offset=0x0010
            ldrb    w0, [x19, #0x69]
            cbnz    w0, G_M000_IG09

G_M000_IG03:                ;; offset=0x0018
            mov     w0, #1
            strb    w0, [x19, #0x69]
            str     xzr, [x19, #0x38]
            add     x0, x19, #16
            ldr     x1, [x0]
            movz    x20, #0x61B8
            movk    x20, #0xA15 LSL #16
            movk    x20, #548 LSL #32
            str     x20, [x0]
            ldr     w0, [x1, #0x08]
            cbz     w0, G_M000_IG04
            ldr     x0, [x19, #0x08]
            cbz     x0, G_M000_IG04
            mov     w2, wzr
            ldr     x3, [x0]
            ldr     x3, [x3, #0x40]
            ldr     x3, [x3, #0x28]
            blr     x3      // code for System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG04:                ;; offset=0x0060
            add     x0, x19, #24
            ldr     x1, [x0]
            str     x20, [x0]
            ldr     w0, [x1, #0x08]
            cbz     w0, G_M000_IG05
            ldr     x0, [x19, #0x08]
            cbz     x0, G_M000_IG05
            mov     w2, wzr
            ldr     x3, [x0]
            ldr     x3, [x3, #0x40]
            ldr     x3, [x3, #0x28]
            blr     x3      // code for System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG05:                ;; offset=0x0090
            add     x0, x19, #32
            ldr     x1, [x0]
            str     x20, [x0]
            ldr     w0, [x1, #0x08]
            cbz     w0, G_M000_IG06
            ldr     x0, [x19, #0x08]
            cbz     x0, G_M000_IG06
            mov     w2, wzr
            ldr     x3, [x0]
            ldr     x3, [x3, #0x40]
            ldr     x3, [x3, #0x28]
            blr     x3      // code for System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG06:                ;; offset=0x00C0
            add     x0, x19, #40
            ldr     x1, [x0]
            str     x20, [x0]
            ldr     w0, [x1, #0x08]
            cbz     w0, G_M000_IG07
            ldr     x0, [x19, #0x08]
            cbz     x0, G_M000_IG07
            mov     w2, wzr
            ldr     x3, [x0]
            ldr     x3, [x3, #0x40]
            ldr     x3, [x3, #0x28]
            blr     x3      // code for System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG07:                ;; offset=0x00F0
            add     x0, x19, #48
            ldr     x1, [x0]
            str     x20, [x0]
            ldr     w0, [x1, #0x08]
            cbz     w0, G_M000_IG09
            ldr     x0, [x19, #0x08]
            cbz     x0, G_M000_IG09
            mov     w2, wzr
            ldr     x3, [x0]
            ldr     x3, [x3, #0x40]
            ldr     x3, [x3, #0x28]

G_M000_IG08:                ;; offset=0x011C
            ldp     x19, x20, [sp, #0x10]
            ldp     fp, lr, [sp], #0x20
            br      x3      // code for System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG09:                ;; offset=0x0128
            ldp     x19, x20, [sp, #0x10]
            ldp     fp, lr, [sp], #0x20
            ret     lr

; Total bytes of code 308

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ModulateEightBlocks(int,int,System.Span`1[System.Runtime.Intrinsics.Vector256`1[float]],System.Span`1[float]):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 556 single block inlinees; 462 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x60]!
            stp     d8, d9, [sp, #0x18]
            stp     d10, d11, [sp, #0x28]
            stp     d12, d13, [sp, #0x38]
            stp     d14, d15, [sp, #0x48]
            str     x19, [sp, #0x58]
            mov     fp, sp

G_M000_IG02:                ;; offset=0x001C
            cmp     w4, #8
            blt     G_M000_IG19
            mov     w7, #8
            cmp     w4, #16
            blt     G_M000_IG19
            add     x4, x3, #256
            mov     w8, #8
            ldr     w9, [x0, #0x54]
            madd    w1, w2, w9, w1
            add     w1, w1, #1
            mov     w2, wzr
            align   [0 bytes for IG03]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG03:                ;; offset=0x0048
            ubfiz   x9, x2, #5, #32
            add     x9, x3, x9
            ldr     x10, [x0, #0x10]
            add     w11, w1, w2,  LSL #3
            cbz     x10, G_M000_IG19
            ldr     w13, [x10, #0x08]
            mov     w14, w11
            add     x14, x14, #8
            cmp     x13, x14
            blo     G_M000_IG19
            add     x10, x10, #16
            ubfiz   x11, x11, #2, #32
            add     x10, x10, x11
            ldp     q16, q17, [x10]
            stp     q16, q17, [x9]
            add     w2, w2, #1
            cmp     w2, #8
            blt     G_M000_IG03

G_M000_IG04:                ;; offset=0x0090
            add     x2, x3, #32
            add     x9, x3, #64
            add     x10, x3, #96
            add     x11, x3, #128
            add     x13, x3, #160
            add     x14, x3, #192
            add     x12, x3, #224
            ldr     q16, [x3]
            ldr     q17, [x2]
            zip1    v18.4s, v16.4s, v17.4s
            zip2    v16.4s, v16.4s, v17.4s
            ldr     q17, [x3, #0x10]
            ldr     q19, [x2, #0x10]
            zip1    v20.4s, v17.4s, v19.4s
            zip2    v17.4s, v17.4s, v19.4s
            ldr     q19, [x9]
            ldr     q21, [x10]
            zip1    v22.4s, v19.4s, v21.4s
            zip2    v19.4s, v19.4s, v21.4s
            ldr     q21, [x9, #0x10]
            ldr     q23, [x10, #0x10]
            zip1    v24.4s, v21.4s, v23.4s
            zip2    v21.4s, v21.4s, v23.4s
            ldr     q23, [x11]
            ldr     q25, [x13]
            zip1    v26.4s, v23.4s, v25.4s
            zip2    v23.4s, v23.4s, v25.4s
            ldr     q25, [x11, #0x10]
            ldr     q27, [x13, #0x10]
            zip1    v28.4s, v25.4s, v27.4s
            zip2    v25.4s, v25.4s, v27.4s
            ldr     q27, [x14]
            ldr     q29, [x12]
            zip1    v30.4s, v27.4s, v29.4s
            zip2    v27.4s, v27.4s, v29.4s
            ldr     q29, [x14, #0x10]
            ldr     q31, [x12, #0x10]
            zip1    v7.4s, v29.4s, v31.4s
            zip2    v29.4s, v29.4s, v31.4s
            zip1    v31.2d, v18.2d, v22.2d
            zip2    v18.2d, v18.2d, v22.2d
            zip1    v22.2d, v16.2d, v19.2d
            zip2    v16.2d, v16.2d, v19.2d
            zip1    v19.2d, v20.2d, v24.2d
            zip2    v20.2d, v20.2d, v24.2d
            zip1    v24.2d, v17.2d, v21.2d
            zip2    v17.2d, v17.2d, v21.2d
            zip1    v21.2d, v26.2d, v30.2d
            zip2    v26.2d, v26.2d, v30.2d
            zip1    v30.2d, v23.2d, v27.2d
            zip2    v23.2d, v23.2d, v27.2d
            zip1    v27.2d, v28.2d, v7.2d
            zip2    v28.2d, v28.2d, v7.2d
            zip1    v7.2d, v25.2d, v29.2d
            zip2    v25.2d, v25.2d, v29.2d
            stp     q31, q21, [x3]
            stp     q18, q26, [x2]
            stp     q22, q30, [x9]
            stp     q16, q23, [x10]
            stp     q19, q27, [x11]
            stp     q20, q28, [x13]
            stp     q24, q7, [x14]
            stp     q17, q25, [x12]
            movi    v16.4s, #0
            movi    v17.4s, #0
            movi    v18.4s, #0
            movi    v19.4s, #0
            ldr     q20, [@RWD00]
            ldr     q21, [@RWD00]
            mov     w2, wzr
            ldr     q22, [@RWD16]
            ldr     q23, [@RWD32]
            ldr     q24, [@RWD48]
            ldr     q25, [@RWD64]
            b       G_M000_IG07
            align   [0 bytes for IG05]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG05:                ;; offset=0x01BC
            add     w13, w11, #1
            cmp     w13, w7
            bhs     G_M000_IG21
            ubfiz   x14, x13, #5, #32
            add     x14, x3, x14
            ldp     q28, q29, [x14]
            fsub    v30.4s, v26.4s, v28.4s
            fsub    v31.4s, v27.4s, v29.4s
            fabs    v7.4s, v30.4s
            fabs    v6.4s, v31.4s
            fadd    v16.4s, v16.4s, v7.4s
            fadd    v17.4s, v17.4s, v6.4s
            cmp     w11, w10
            bhs     G_M000_IG21
            ubfiz   x11, x11, #5, #32
            add     x11, x9, x11
            ldp     q5, q4, [x11]
            fsub    v3.4s, v26.4s, v5.4s
            fsub    v2.4s, v27.4s, v4.4s
            fabs    v1.4s, v3.4s
            fabs    v0.4s, v2.4s
            fadd    v16.4s, v16.4s, v1.4s
            fadd    v17.4s, v17.4s, v0.4s
            fadd    v8.4s, v26.4s, v21.4s
            fadd    v9.4s, v27.4s, v20.4s
            movi    v26.4s, #0
            fmax    v26.4s, v8.4s, v26.4s
            movi    v27.4s, #0
            fmax    v27.4s, v9.4s, v27.4s
            fmul    v10.4s, v26.4s, v26.4s
            fmul    v11.4s, v27.4s, v27.4s
            fmul    v12.4s, v10.4s, v22.4s
            fmul    v13.4s, v11.4s, v22.4s
            fadd    v14.4s, v12.4s, v23.4s
            fadd    v15.4s, v13.4s, v23.4s
            fmul    v30.4s, v26.4s, v24.4s
            fmul    v31.4s, v27.4s, v24.4s
            fmul    v7.4s, v30.4s, v10.4s
            fmul    v6.4s, v31.4s, v11.4s
            fadd    v5.4s, v7.4s, v25.4s
            fadd    v4.4s, v6.4s, v25.4s
            fdiv    v3.4s, v14.4s, v5.4s
            fdiv    v2.4s, v15.4s, v4.4s
            fadd    v18.4s, v18.4s, v3.4s
            fadd    v19.4s, v19.4s, v2.4s
            mov     v26.16b, v28.16b
            mov     v27.16b, v29.16b
            mov     w11, w13
            cmp     w11, #7
            blt     G_M000_IG05
            b       G_M000_IG16

G_M000_IG06:                ;; offset=0x0288
            cmp     w2, #7
            mov     x9, x3
            mov     x3, x4
            mov     x4, x9
            mov     w9, w7
            mov     w7, w8
            mov     w8, w9
            beq     G_M000_IG10

G_M000_IG07:                ;; offset=0x02A8
            ldr     w9, [x0, #0x54]
            add     w9, w1, w9
            mov     w10, wzr
            cmp     w8, #8
            blt     G_M000_IG12
            align   [4 bytes for IG08]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG08:                ;; offset=0x02C0
            ubfiz   x11, x10, #5, #32
            add     x11, x4, x11
            ldr     x13, [x0, #0x10]
            add     w14, w9, w10,  LSL #3
            cbz     x13, G_M000_IG19
            ldr     w12, [x13, #0x08]
            mov     w15, w14
            add     x15, x15, #8
            cmp     x12, x15
            blo     G_M000_IG19
            add     x13, x13, #16
            ubfiz   x14, x14, #2, #32
            add     x13, x13, x14
            ldp     q26, q27, [x13]
            stp     q26, q27, [x11]
            add     w10, w10, #1
            cmp     w10, #8
            blt     G_M000_IG08

G_M000_IG09:                ;; offset=0x0308
            mov     w9, w8
            cmp     w9, #8
            blt     G_M000_IG20
            add     x9, x4, #32
            add     x10, x4, #64
            add     x11, x4, #96
            add     x13, x4, #128
            add     x14, x4, #160
            add     x12, x4, #192
            add     x15, x4, #224
            ldr     q26, [x4]
            ldr     q27, [x9]
            zip1    v28.4s, v26.4s, v27.4s
            zip2    v26.4s, v26.4s, v27.4s
            ldr     q27, [x4, #0x10]
            ldr     q29, [x9, #0x10]
            zip1    v30.4s, v27.4s, v29.4s
            zip2    v27.4s, v27.4s, v29.4s
            ldr     q29, [x10]
            ldr     q31, [x11]
            zip1    v7.4s, v29.4s, v31.4s
            zip2    v29.4s, v29.4s, v31.4s
            ldr     q31, [x10, #0x10]
            ldr     q6, [x11, #0x10]
            zip1    v5.4s, v31.4s, v6.4s
            zip2    v31.4s, v31.4s, v6.4s
            ldr     q6, [x13]
            ldr     q4, [x14]
            zip1    v3.4s, v6.4s, v4.4s
            zip2    v6.4s, v6.4s, v4.4s
            ldr     q4, [x13, #0x10]
            ldr     q2, [x14, #0x10]
            zip1    v1.4s, v4.4s, v2.4s
            zip2    v4.4s, v4.4s, v2.4s
            ldr     q2, [x12]
            ldr     q0, [x15]
            zip1    v8.4s, v2.4s, v0.4s
            zip2    v2.4s, v2.4s, v0.4s
            ldr     q0, [x12, #0x10]
            ldr     q9, [x15, #0x10]
            zip1    v10.4s, v0.4s, v9.4s
            zip2    v0.4s, v0.4s, v9.4s
            zip1    v9.2d, v28.2d, v7.2d
            zip2    v28.2d, v28.2d, v7.2d
            zip1    v7.2d, v26.2d, v29.2d
            zip2    v26.2d, v26.2d, v29.2d
            zip1    v29.2d, v30.2d, v5.2d
            zip2    v30.2d, v30.2d, v5.2d
            zip1    v5.2d, v27.2d, v31.2d
            zip2    v27.2d, v27.2d, v31.2d
            zip1    v31.2d, v3.2d, v8.2d
            zip2    v3.2d, v3.2d, v8.2d
            zip1    v8.2d, v6.2d, v2.2d
            zip2    v6.2d, v6.2d, v2.2d
            zip1    v2.2d, v1.2d, v10.2d
            zip2    v1.2d, v1.2d, v10.2d
            zip1    v10.2d, v4.2d, v0.2d
            zip2    v4.2d, v4.2d, v0.2d
            stp     q9, q31, [x4]
            stp     q28, q3, [x9]
            stp     q7, q8, [x10]
            stp     q26, q6, [x11]
            stp     q29, q2, [x13]
            stp     q30, q1, [x14]
            stp     q5, q10, [x12]
            stp     q27, q4, [x15]

G_M000_IG10:                ;; offset=0x0410
            cmp     w2, #7
            beq     G_M000_IG13

G_M000_IG11:                ;; offset=0x0418
            mov     x9, x4
            mov     w10, w8
            b       G_M000_IG14
            align   [0 bytes for IG15]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG12:                ;; offset=0x0424
            cmp     w10, w8
            bhs     G_M000_IG21
            ubfiz   x11, x10, #5, #32
            add     x11, x4, x11
            ldr     x13, [x0, #0x10]
            add     w14, w9, w10,  LSL #3
            cbz     x13, G_M000_IG19
            ldr     w12, [x13, #0x08]
            mov     w15, w14
            add     x15, x15, #8
            cmp     x12, x15
            blo     G_M000_IG19
            add     x13, x13, #16
            ubfiz   x14, x14, #2, #32
            add     x13, x13, x14
            ldp     q26, q27, [x13]
            stp     q26, q27, [x11]
            add     w10, w10, #1
            cmp     w10, #8
            blt     G_M000_IG12
            b       G_M000_IG09

G_M000_IG13:                ;; offset=0x0478
            mov     x9, x3
            mov     w10, w7

G_M000_IG14:                ;; offset=0x0480
            cbz     w7, G_M000_IG21
            ldp     q26, q27, [x3]
            mov     w11, wzr
            cmp     w10, #7
            blt     G_M000_IG05

G_M000_IG15:                ;; offset=0x0494
            add     w13, w11, #1
            cmp     w13, w7
            bhs     G_M000_IG21
            ubfiz   x14, x13, #5, #32
            add     x14, x3, x14
            ldp     q28, q29, [x14]
            fsub    v30.4s, v26.4s, v28.4s
            fsub    v31.4s, v27.4s, v29.4s
            fabs    v7.4s, v30.4s
            fabs    v6.4s, v31.4s
            fadd    v16.4s, v16.4s, v7.4s
            fadd    v17.4s, v17.4s, v6.4s
            ubfiz   x11, x11, #5, #32
            add     x11, x9, x11
            ldp     q5, q4, [x11]
            fsub    v3.4s, v26.4s, v5.4s
            fsub    v2.4s, v27.4s, v4.4s
            fabs    v1.4s, v3.4s
            fabs    v0.4s, v2.4s
            fadd    v16.4s, v16.4s, v1.4s
            fadd    v17.4s, v17.4s, v0.4s
            fadd    v8.4s, v26.4s, v21.4s
            fadd    v9.4s, v27.4s, v20.4s
            movi    v26.4s, #0
            fmax    v26.4s, v8.4s, v26.4s
            movi    v27.4s, #0
            fmax    v27.4s, v9.4s, v27.4s
            fmul    v10.4s, v26.4s, v26.4s
            fmul    v11.4s, v27.4s, v27.4s
            fmul    v12.4s, v10.4s, v22.4s
            fmul    v13.4s, v11.4s, v22.4s
            fadd    v14.4s, v12.4s, v23.4s
            fadd    v15.4s, v13.4s, v23.4s
            fmul    v30.4s, v26.4s, v24.4s
            fmul    v31.4s, v27.4s, v24.4s
            fmul    v7.4s, v30.4s, v10.4s
            fmul    v6.4s, v31.4s, v11.4s
            fadd    v5.4s, v7.4s, v25.4s
            fadd    v4.4s, v6.4s, v25.4s
            fdiv    v3.4s, v14.4s, v5.4s
            fdiv    v2.4s, v15.4s, v4.4s
            fadd    v18.4s, v18.4s, v3.4s
            fadd    v19.4s, v19.4s, v2.4s
            mov     v26.16b, v28.16b
            mov     v27.16b, v29.16b
            mov     w11, w13
            cmp     w11, #7
            blt     G_M000_IG15

G_M000_IG16:                ;; offset=0x0554
            cmp     w10, #7
            bls     G_M000_IG21
            add     x9, x9, #224
            ldp     q28, q29, [x9]
            fsub    v28.4s, v26.4s, v28.4s
            fsub    v29.4s, v27.4s, v29.4s
            fabs    v28.4s, v28.4s
            fabs    v29.4s, v29.4s
            fadd    v16.4s, v16.4s, v28.4s
            fadd    v17.4s, v17.4s, v29.4s
            fadd    v26.4s, v26.4s, v21.4s
            fadd    v27.4s, v27.4s, v20.4s
            movi    v28.4s, #0
            fmax    v26.4s, v26.4s, v28.4s
            movi    v28.4s, #0
            fmax    v27.4s, v27.4s, v28.4s
            fmul    v28.4s, v26.4s, v26.4s
            fmul    v29.4s, v27.4s, v27.4s
            fmul    v30.4s, v28.4s, v22.4s
            fmul    v31.4s, v29.4s, v22.4s
            fadd    v30.4s, v30.4s, v23.4s
            fadd    v31.4s, v31.4s, v23.4s
            fmul    v26.4s, v26.4s, v24.4s
            fmul    v27.4s, v27.4s, v24.4s
            fmul    v26.4s, v26.4s, v28.4s
            fmul    v27.4s, v27.4s, v29.4s
            fadd    v26.4s, v26.4s, v25.4s
            fadd    v27.4s, v27.4s, v25.4s
            fdiv    v26.4s, v30.4s, v26.4s
            fdiv    v27.4s, v31.4s, v27.4s
            fadd    v18.4s, v18.4s, v26.4s
            fadd    v19.4s, v19.4s, v27.4s
            ldr     w9, [x0, #0x54]
            add     w1, w1, w9
            add     w2, w2, #1
            cmp     w2, #8
            blt     G_M000_IG06

G_M000_IG17:                ;; offset=0x05E8
            ldr     q20, [@RWD80]
            fmul    v16.4s, v16.4s, v20.4s
            ldr     q20, [@RWD80]
            fmul    v17.4s, v17.4s, v20.4s
            cmp     w6, #8
            blt     G_M000_IG19
            stp     q16, q17, [x5]
            ldr     q16, [@RWD96]
            fmul    v16.4s, v18.4s, v16.4s
            ldr     q17, [@RWD96]
            fmul    v17.4s, v19.4s, v17.4s
            cmp     w6, #16
            blt     G_M000_IG19
            add     x0, x5, #32
            stp     q16, q17, [x0]

G_M000_IG18:                ;; offset=0x0624
            ldr     x19, [sp, #0x58]
            ldp     d14, d15, [sp, #0x48]
            ldp     d12, d13, [sp, #0x38]
            ldp     d10, d11, [sp, #0x28]
            ldp     d8, d9, [sp, #0x18]
            ldp     fp, lr, [sp], #0x60
            ret     lr

G_M000_IG19:                ;; offset=0x0640
            movz    x0, #0x4450
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentOutOfRangeException()
            brk     #0

G_M000_IG20:                ;; offset=0x0658
            movz    x0, #0xF360
            movk    x0, #0x693D LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x6350
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0x63C8
            movk    x2, #0xA15 LSL #16
            movk    x2, #548 LSL #32
            movz    x3, #0x4438
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for System.ArgumentException:.ctor(System.String,System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG21:                ;; offset=0x06A4
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dq	4223333242233332h, 4223333242233332h
RWD16  	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD32  	dq	44228FFE44228FFEh, 44228FFE44228FFEh
RWD48  	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD64  	dq	449E380D449E380Dh, 449E380D449E380Dh
RWD80  	dq	B8933E1EB8933E1Eh, B8933E1EB8933E1Eh
RWD96  	dq	3880808138808081h, 3880808138808081h

; Total bytes of code 1708

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ModulateEightBlocks(int,int,System.Span`1[System.Runtime.Intrinsics.Vector256`1[float]],System.Span`1[float]):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 556 single block inlinees; 462 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x60]!
            stp     d8, d9, [sp, #0x18]
            stp     d10, d11, [sp, #0x28]
            stp     d12, d13, [sp, #0x38]
            stp     d14, d15, [sp, #0x48]
            str     x19, [sp, #0x58]
            mov     fp, sp

G_M000_IG02:                ;; offset=0x001C
            cmp     w4, #8
            blt     G_M000_IG19
            mov     w7, #8
            cmp     w4, #16
            blt     G_M000_IG19
            add     x4, x3, #256
            mov     w8, #8
            ldr     w9, [x0, #0x4C]
            madd    w1, w2, w9, w1
            add     w1, w1, #1
            mov     w2, wzr
            align   [0 bytes for IG03]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG03:                ;; offset=0x0048
            ubfiz   x9, x2, #5, #32
            add     x9, x3, x9
            ldr     x10, [x0, #0x10]
            add     w11, w1, w2,  LSL #3
            cbz     x10, G_M000_IG19
            ldr     w13, [x10, #0x08]
            mov     w14, w11
            add     x14, x14, #8
            cmp     x13, x14
            blo     G_M000_IG19
            add     x10, x10, #16
            ubfiz   x11, x11, #2, #32
            add     x10, x10, x11
            ldp     q16, q17, [x10]
            stp     q16, q17, [x9]
            add     w2, w2, #1
            cmp     w2, #8
            blt     G_M000_IG03

G_M000_IG04:                ;; offset=0x0090
            add     x2, x3, #32
            add     x9, x3, #64
            add     x10, x3, #96
            add     x11, x3, #128
            add     x13, x3, #160
            add     x14, x3, #192
            add     x12, x3, #224
            ldr     q16, [x3]
            ldr     q17, [x2]
            zip1    v18.4s, v16.4s, v17.4s
            zip2    v16.4s, v16.4s, v17.4s
            ldr     q17, [x3, #0x10]
            ldr     q19, [x2, #0x10]
            zip1    v20.4s, v17.4s, v19.4s
            zip2    v17.4s, v17.4s, v19.4s
            ldr     q19, [x9]
            ldr     q21, [x10]
            zip1    v22.4s, v19.4s, v21.4s
            zip2    v19.4s, v19.4s, v21.4s
            ldr     q21, [x9, #0x10]
            ldr     q23, [x10, #0x10]
            zip1    v24.4s, v21.4s, v23.4s
            zip2    v21.4s, v21.4s, v23.4s
            ldr     q23, [x11]
            ldr     q25, [x13]
            zip1    v26.4s, v23.4s, v25.4s
            zip2    v23.4s, v23.4s, v25.4s
            ldr     q25, [x11, #0x10]
            ldr     q27, [x13, #0x10]
            zip1    v28.4s, v25.4s, v27.4s
            zip2    v25.4s, v25.4s, v27.4s
            ldr     q27, [x14]
            ldr     q29, [x12]
            zip1    v30.4s, v27.4s, v29.4s
            zip2    v27.4s, v27.4s, v29.4s
            ldr     q29, [x14, #0x10]
            ldr     q31, [x12, #0x10]
            zip1    v7.4s, v29.4s, v31.4s
            zip2    v29.4s, v29.4s, v31.4s
            zip1    v31.2d, v18.2d, v22.2d
            zip2    v18.2d, v18.2d, v22.2d
            zip1    v22.2d, v16.2d, v19.2d
            zip2    v16.2d, v16.2d, v19.2d
            zip1    v19.2d, v20.2d, v24.2d
            zip2    v20.2d, v20.2d, v24.2d
            zip1    v24.2d, v17.2d, v21.2d
            zip2    v17.2d, v17.2d, v21.2d
            zip1    v21.2d, v26.2d, v30.2d
            zip2    v26.2d, v26.2d, v30.2d
            zip1    v30.2d, v23.2d, v27.2d
            zip2    v23.2d, v23.2d, v27.2d
            zip1    v27.2d, v28.2d, v7.2d
            zip2    v28.2d, v28.2d, v7.2d
            zip1    v7.2d, v25.2d, v29.2d
            zip2    v25.2d, v25.2d, v29.2d
            stp     q31, q21, [x3]
            stp     q18, q26, [x2]
            stp     q22, q30, [x9]
            stp     q16, q23, [x10]
            stp     q19, q27, [x11]
            stp     q20, q28, [x13]
            stp     q24, q7, [x14]
            stp     q17, q25, [x12]
            movi    v16.4s, #0
            movi    v17.4s, #0
            movi    v18.4s, #0
            movi    v19.4s, #0
            ldr     q20, [@RWD00]
            ldr     q21, [@RWD00]
            mov     w2, wzr
            ldr     q22, [@RWD16]
            ldr     q23, [@RWD32]
            ldr     q24, [@RWD48]
            ldr     q25, [@RWD64]
            b       G_M000_IG07
            align   [0 bytes for IG05]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG05:                ;; offset=0x01BC
            add     w13, w11, #1
            cmp     w13, w7
            bhs     G_M000_IG21
            ubfiz   x14, x13, #5, #32
            add     x14, x3, x14
            ldp     q28, q29, [x14]
            fsub    v30.4s, v26.4s, v28.4s
            fsub    v31.4s, v27.4s, v29.4s
            fabs    v7.4s, v30.4s
            fabs    v6.4s, v31.4s
            fadd    v16.4s, v16.4s, v7.4s
            fadd    v17.4s, v17.4s, v6.4s
            cmp     w11, w10
            bhs     G_M000_IG21
            ubfiz   x11, x11, #5, #32
            add     x11, x9, x11
            ldp     q5, q4, [x11]
            fsub    v3.4s, v26.4s, v5.4s
            fsub    v2.4s, v27.4s, v4.4s
            fabs    v1.4s, v3.4s
            fabs    v0.4s, v2.4s
            fadd    v16.4s, v16.4s, v1.4s
            fadd    v17.4s, v17.4s, v0.4s
            fadd    v8.4s, v26.4s, v21.4s
            fadd    v9.4s, v27.4s, v20.4s
            movi    v26.4s, #0
            fmax    v26.4s, v8.4s, v26.4s
            movi    v27.4s, #0
            fmax    v27.4s, v9.4s, v27.4s
            fmul    v10.4s, v26.4s, v26.4s
            fmul    v11.4s, v27.4s, v27.4s
            fmul    v12.4s, v10.4s, v22.4s
            fmul    v13.4s, v11.4s, v22.4s
            fadd    v14.4s, v12.4s, v23.4s
            fadd    v15.4s, v13.4s, v23.4s
            fmul    v30.4s, v26.4s, v24.4s
            fmul    v31.4s, v27.4s, v24.4s
            fmul    v7.4s, v30.4s, v10.4s
            fmul    v6.4s, v31.4s, v11.4s
            fadd    v5.4s, v7.4s, v25.4s
            fadd    v4.4s, v6.4s, v25.4s
            fdiv    v3.4s, v14.4s, v5.4s
            fdiv    v2.4s, v15.4s, v4.4s
            fadd    v18.4s, v18.4s, v3.4s
            fadd    v19.4s, v19.4s, v2.4s
            mov     v26.16b, v28.16b
            mov     v27.16b, v29.16b
            mov     w11, w13
            cmp     w11, #7
            blt     G_M000_IG05
            b       G_M000_IG16

G_M000_IG06:                ;; offset=0x0288
            cmp     w2, #7
            mov     x9, x3
            mov     x3, x4
            mov     x4, x9
            mov     w9, w7
            mov     w7, w8
            mov     w8, w9
            beq     G_M000_IG10

G_M000_IG07:                ;; offset=0x02A8
            ldr     w9, [x0, #0x4C]
            add     w9, w1, w9
            mov     w10, wzr
            cmp     w8, #8
            blt     G_M000_IG12
            align   [4 bytes for IG08]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG08:                ;; offset=0x02C0
            ubfiz   x11, x10, #5, #32
            add     x11, x4, x11
            ldr     x13, [x0, #0x10]
            add     w14, w9, w10,  LSL #3
            cbz     x13, G_M000_IG19
            ldr     w12, [x13, #0x08]
            mov     w15, w14
            add     x15, x15, #8
            cmp     x12, x15
            blo     G_M000_IG19
            add     x13, x13, #16
            ubfiz   x14, x14, #2, #32
            add     x13, x13, x14
            ldp     q26, q27, [x13]
            stp     q26, q27, [x11]
            add     w10, w10, #1
            cmp     w10, #8
            blt     G_M000_IG08

G_M000_IG09:                ;; offset=0x0308
            mov     w9, w8
            cmp     w9, #8
            blt     G_M000_IG20
            add     x9, x4, #32
            add     x10, x4, #64
            add     x11, x4, #96
            add     x13, x4, #128
            add     x14, x4, #160
            add     x12, x4, #192
            add     x15, x4, #224
            ldr     q26, [x4]
            ldr     q27, [x9]
            zip1    v28.4s, v26.4s, v27.4s
            zip2    v26.4s, v26.4s, v27.4s
            ldr     q27, [x4, #0x10]
            ldr     q29, [x9, #0x10]
            zip1    v30.4s, v27.4s, v29.4s
            zip2    v27.4s, v27.4s, v29.4s
            ldr     q29, [x10]
            ldr     q31, [x11]
            zip1    v7.4s, v29.4s, v31.4s
            zip2    v29.4s, v29.4s, v31.4s
            ldr     q31, [x10, #0x10]
            ldr     q6, [x11, #0x10]
            zip1    v5.4s, v31.4s, v6.4s
            zip2    v31.4s, v31.4s, v6.4s
            ldr     q6, [x13]
            ldr     q4, [x14]
            zip1    v3.4s, v6.4s, v4.4s
            zip2    v6.4s, v6.4s, v4.4s
            ldr     q4, [x13, #0x10]
            ldr     q2, [x14, #0x10]
            zip1    v1.4s, v4.4s, v2.4s
            zip2    v4.4s, v4.4s, v2.4s
            ldr     q2, [x12]
            ldr     q0, [x15]
            zip1    v8.4s, v2.4s, v0.4s
            zip2    v2.4s, v2.4s, v0.4s
            ldr     q0, [x12, #0x10]
            ldr     q9, [x15, #0x10]
            zip1    v10.4s, v0.4s, v9.4s
            zip2    v0.4s, v0.4s, v9.4s
            zip1    v9.2d, v28.2d, v7.2d
            zip2    v28.2d, v28.2d, v7.2d
            zip1    v7.2d, v26.2d, v29.2d
            zip2    v26.2d, v26.2d, v29.2d
            zip1    v29.2d, v30.2d, v5.2d
            zip2    v30.2d, v30.2d, v5.2d
            zip1    v5.2d, v27.2d, v31.2d
            zip2    v27.2d, v27.2d, v31.2d
            zip1    v31.2d, v3.2d, v8.2d
            zip2    v3.2d, v3.2d, v8.2d
            zip1    v8.2d, v6.2d, v2.2d
            zip2    v6.2d, v6.2d, v2.2d
            zip1    v2.2d, v1.2d, v10.2d
            zip2    v1.2d, v1.2d, v10.2d
            zip1    v10.2d, v4.2d, v0.2d
            zip2    v4.2d, v4.2d, v0.2d
            stp     q9, q31, [x4]
            stp     q28, q3, [x9]
            stp     q7, q8, [x10]
            stp     q26, q6, [x11]
            stp     q29, q2, [x13]
            stp     q30, q1, [x14]
            stp     q5, q10, [x12]
            stp     q27, q4, [x15]

G_M000_IG10:                ;; offset=0x0410
            cmp     w2, #7
            beq     G_M000_IG13

G_M000_IG11:                ;; offset=0x0418
            mov     x9, x4
            mov     w10, w8
            b       G_M000_IG14
            align   [0 bytes for IG15]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG12:                ;; offset=0x0424
            cmp     w10, w8
            bhs     G_M000_IG21
            ubfiz   x11, x10, #5, #32
            add     x11, x4, x11
            ldr     x13, [x0, #0x10]
            add     w14, w9, w10,  LSL #3
            cbz     x13, G_M000_IG19
            ldr     w12, [x13, #0x08]
            mov     w15, w14
            add     x15, x15, #8
            cmp     x12, x15
            blo     G_M000_IG19
            add     x13, x13, #16
            ubfiz   x14, x14, #2, #32
            add     x13, x13, x14
            ldp     q26, q27, [x13]
            stp     q26, q27, [x11]
            add     w10, w10, #1
            cmp     w10, #8
            blt     G_M000_IG12
            b       G_M000_IG09

G_M000_IG13:                ;; offset=0x0478
            mov     x9, x3
            mov     w10, w7

G_M000_IG14:                ;; offset=0x0480
            cbz     w7, G_M000_IG21
            ldp     q26, q27, [x3]
            mov     w11, wzr
            cmp     w10, #7
            blt     G_M000_IG05

G_M000_IG15:                ;; offset=0x0494
            add     w13, w11, #1
            cmp     w13, w7
            bhs     G_M000_IG21
            ubfiz   x14, x13, #5, #32
            add     x14, x3, x14
            ldp     q28, q29, [x14]
            fsub    v30.4s, v26.4s, v28.4s
            fsub    v31.4s, v27.4s, v29.4s
            fabs    v7.4s, v30.4s
            fabs    v6.4s, v31.4s
            fadd    v16.4s, v16.4s, v7.4s
            fadd    v17.4s, v17.4s, v6.4s
            ubfiz   x11, x11, #5, #32
            add     x11, x9, x11
            ldp     q5, q4, [x11]
            fsub    v3.4s, v26.4s, v5.4s
            fsub    v2.4s, v27.4s, v4.4s
            fabs    v1.4s, v3.4s
            fabs    v0.4s, v2.4s
            fadd    v16.4s, v16.4s, v1.4s
            fadd    v17.4s, v17.4s, v0.4s
            fadd    v8.4s, v26.4s, v21.4s
            fadd    v9.4s, v27.4s, v20.4s
            movi    v26.4s, #0
            fmax    v26.4s, v8.4s, v26.4s
            movi    v27.4s, #0
            fmax    v27.4s, v9.4s, v27.4s
            fmul    v10.4s, v26.4s, v26.4s
            fmul    v11.4s, v27.4s, v27.4s
            fmul    v12.4s, v10.4s, v22.4s
            fmul    v13.4s, v11.4s, v22.4s
            fadd    v14.4s, v12.4s, v23.4s
            fadd    v15.4s, v13.4s, v23.4s
            fmul    v30.4s, v26.4s, v24.4s
            fmul    v31.4s, v27.4s, v24.4s
            fmul    v7.4s, v30.4s, v10.4s
            fmul    v6.4s, v31.4s, v11.4s
            fadd    v5.4s, v7.4s, v25.4s
            fadd    v4.4s, v6.4s, v25.4s
            fdiv    v3.4s, v14.4s, v5.4s
            fdiv    v2.4s, v15.4s, v4.4s
            fadd    v18.4s, v18.4s, v3.4s
            fadd    v19.4s, v19.4s, v2.4s
            mov     v26.16b, v28.16b
            mov     v27.16b, v29.16b
            mov     w11, w13
            cmp     w11, #7
            blt     G_M000_IG15

G_M000_IG16:                ;; offset=0x0554
            cmp     w10, #7
            bls     G_M000_IG21
            add     x9, x9, #224
            ldp     q28, q29, [x9]
            fsub    v28.4s, v26.4s, v28.4s
            fsub    v29.4s, v27.4s, v29.4s
            fabs    v28.4s, v28.4s
            fabs    v29.4s, v29.4s
            fadd    v16.4s, v16.4s, v28.4s
            fadd    v17.4s, v17.4s, v29.4s
            fadd    v26.4s, v26.4s, v21.4s
            fadd    v27.4s, v27.4s, v20.4s
            movi    v28.4s, #0
            fmax    v26.4s, v26.4s, v28.4s
            movi    v28.4s, #0
            fmax    v27.4s, v27.4s, v28.4s
            fmul    v28.4s, v26.4s, v26.4s
            fmul    v29.4s, v27.4s, v27.4s
            fmul    v30.4s, v28.4s, v22.4s
            fmul    v31.4s, v29.4s, v22.4s
            fadd    v30.4s, v30.4s, v23.4s
            fadd    v31.4s, v31.4s, v23.4s
            fmul    v26.4s, v26.4s, v24.4s
            fmul    v27.4s, v27.4s, v24.4s
            fmul    v26.4s, v26.4s, v28.4s
            fmul    v27.4s, v27.4s, v29.4s
            fadd    v26.4s, v26.4s, v25.4s
            fadd    v27.4s, v27.4s, v25.4s
            fdiv    v26.4s, v30.4s, v26.4s
            fdiv    v27.4s, v31.4s, v27.4s
            fadd    v18.4s, v18.4s, v26.4s
            fadd    v19.4s, v19.4s, v27.4s
            ldr     w9, [x0, #0x4C]
            add     w1, w1, w9
            add     w2, w2, #1
            cmp     w2, #8
            blt     G_M000_IG06

G_M000_IG17:                ;; offset=0x05E8
            ldr     q20, [@RWD80]
            fmul    v16.4s, v16.4s, v20.4s
            ldr     q20, [@RWD80]
            fmul    v17.4s, v17.4s, v20.4s
            cmp     w6, #8
            blt     G_M000_IG19
            stp     q16, q17, [x5]
            ldr     q16, [@RWD96]
            fmul    v16.4s, v18.4s, v16.4s
            ldr     q17, [@RWD96]
            fmul    v17.4s, v19.4s, v17.4s
            cmp     w6, #16
            blt     G_M000_IG19
            add     x0, x5, #32
            stp     q16, q17, [x0]

G_M000_IG18:                ;; offset=0x0624
            ldr     x19, [sp, #0x58]
            ldp     d14, d15, [sp, #0x48]
            ldp     d12, d13, [sp, #0x38]
            ldp     d10, d11, [sp, #0x28]
            ldp     d8, d9, [sp, #0x18]
            ldp     fp, lr, [sp], #0x60
            ret     lr

G_M000_IG19:                ;; offset=0x0640
            movz    x0, #0x4450
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentOutOfRangeException()
            brk     #0

G_M000_IG20:                ;; offset=0x0658
            movz    x0, #0xF360
            movk    x0, #0x693D LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x6350
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0x63C8
            movk    x2, #0xA15 LSL #16
            movk    x2, #548 LSL #32
            movz    x3, #0x4438
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for System.ArgumentException:.ctor(System.String,System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG21:                ;; offset=0x06A4
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dq	4223333242233332h, 4223333242233332h
RWD16  	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD32  	dq	44228FFE44228FFEh, 44228FFE44228FFEh
RWD48  	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD64  	dq	449E380D449E380Dh, 449E380D449E380Dh
RWD80  	dq	B8933E1EB8933E1Eh, B8933E1EB8933E1Eh
RWD96  	dq	3880808138808081h, 3880808138808081h

; Total bytes of code 1708

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ModulateEightBlocks(int,int,System.Span`1[System.Runtime.Intrinsics.Vector256`1[float]],System.Span`1[float]):this (FullOpts)
; Emitting BLENDED_CODE for arm64 + SVE on Windows
; FullOpts code
; optimized code
; fp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 556 single block inlinees; 464 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
            stp     fp, lr, [sp, #-0x140]!
            stp     d8, d9, [sp, #0xB0]
            stp     d10, d11, [sp, #0xC0]
            stp     d12, d13, [sp, #0xD0]
            stp     d14, d15, [sp, #0xE0]
            stp     x19, x20, [sp, #0xF0]
            stp     x21, x22, [sp, #0x100]
            stp     x23, x24, [sp, #0x110]
            stp     x25, x26, [sp, #0x120]
            stp     x27, x28, [sp, #0x130]
            mov     fp, sp
            mov     x19, x0
            mov     x20, x5
            mov     w21, w6

G_M000_IG02:                ;; offset=0x0038
            cmp     w4, #8
            blt     G_M000_IG30
            mov     x22, x3
            mov     w23, #8
            cmp     w4, #16
            blt     G_M000_IG30
            add     x24, x22, #256
            mov     w25, #8
            ldr     w0, [x19, #0x54]
            madd    w1, w2, w0, w1
            add     w26, w1, #1
            mov     w1, wzr
            align   [0 bytes for IG03]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG03:                ;; offset=0x0068
            ubfiz   x0, x1, #5, #32
            add     x0, x22, x0
            ldr     x2, [x19, #0x18]
            add     w3, w26, w1,  LSL #3
            cbz     x2, G_M000_IG30
            ldr     w4, [x2, #0x08]
            mov     w5, w3
            add     x5, x5, #8
            cmp     x4, x5
            blo     G_M000_IG30
            add     x2, x2, #16
            ubfiz   x3, x3, #2, #32
            add     x2, x2, x3
            ldp     q0, q1, [x2]
            stp     q0, q1, [x0]
            add     w1, w1, #1
            cmp     w1, #8
            blt     G_M000_IG03

G_M000_IG04:                ;; offset=0x00B0
            add     x1, x22, #32
            add     x0, x22, #64
            add     x2, x22, #96
            add     x3, x22, #128
            add     x4, x22, #160
            add     x5, x22, #192
            add     x6, x22, #224
            ldr     q0, [x22]
            ldr     q1, [x1]
            zip1    v2.4s, v0.4s, v1.4s
            zip2    v0.4s, v0.4s, v1.4s
            ldr     q1, [x22, #0x10]
            ldr     q3, [x1, #0x10]
            zip1    v16.4s, v1.4s, v3.4s
            zip2    v1.4s, v1.4s, v3.4s
            ldr     q3, [x0]
            ldr     q17, [x2]
            zip1    v18.4s, v3.4s, v17.4s
            zip2    v3.4s, v3.4s, v17.4s
            ldr     q17, [x0, #0x10]
            ldr     q19, [x2, #0x10]
            zip1    v20.4s, v17.4s, v19.4s
            zip2    v17.4s, v17.4s, v19.4s
            ldr     q19, [x3]
            ldr     q21, [x4]
            zip1    v22.4s, v19.4s, v21.4s
            zip2    v19.4s, v19.4s, v21.4s
            ldr     q21, [x3, #0x10]
            ldr     q23, [x4, #0x10]
            zip1    v24.4s, v21.4s, v23.4s
            zip2    v21.4s, v21.4s, v23.4s
            ldr     q23, [x5]
            ldr     q25, [x6]
            zip1    v26.4s, v23.4s, v25.4s
            zip2    v23.4s, v23.4s, v25.4s
            ldr     q25, [x5, #0x10]
            ldr     q27, [x6, #0x10]
            zip1    v28.4s, v25.4s, v27.4s
            zip2    v25.4s, v25.4s, v27.4s
            zip1    v27.2d, v2.2d, v18.2d
            zip2    v2.2d, v2.2d, v18.2d
            zip1    v18.2d, v0.2d, v3.2d
            zip2    v0.2d, v0.2d, v3.2d
            zip1    v3.2d, v16.2d, v20.2d
            zip2    v16.2d, v16.2d, v20.2d
            zip1    v20.2d, v1.2d, v17.2d
            zip2    v1.2d, v1.2d, v17.2d
            zip1    v17.2d, v22.2d, v26.2d
            zip2    v22.2d, v22.2d, v26.2d
            zip1    v26.2d, v19.2d, v23.2d
            zip2    v19.2d, v19.2d, v23.2d
            zip1    v23.2d, v24.2d, v28.2d
            zip2    v24.2d, v24.2d, v28.2d
            zip1    v28.2d, v21.2d, v25.2d
            zip2    v21.2d, v21.2d, v25.2d
            stp     q27, q17, [x22]
            stp     q2, q22, [x1]
            stp     q18, q26, [x0]
            stp     q0, q19, [x2]
            stp     q3, q23, [x3]
            stp     q16, q24, [x4]
            stp     q20, q28, [x5]
            stp     q1, q21, [x6]
            movi    v8.4s, #0
            movi    v9.4s, #0
            movi    v10.4s, #0
            movi    v11.4s, #0
            ldr     q12, [@RWD00]
            str     q12, [fp, #0x60]
            ldr     q13, [@RWD00]
            str     q13, [fp, #0x50]
            mov     w27, wzr
            b       G_M000_IG23

G_M000_IG05:                ;; offset=0x01D4
            cmp     w0, w25
            bhs     G_M000_IG32
            ubfiz   x2, x0, #5, #32
            add     x2, x24, x2
            ldr     x3, [x19, #0x18]
            add     w4, w1, w0,  LSL #3
            cbz     x3, G_M000_IG30
            ldr     w5, [x3, #0x08]
            mov     w6, w4
            add     x6, x6, #8
            cmp     x5, x6
            blo     G_M000_IG30
            add     x3, x3, #16
            ubfiz   x4, x4, #2, #32
            add     x3, x3, x4
            ldp     q0, q1, [x3]
            stp     q0, q1, [x2]
            add     w0, w0, #1
            cmp     w0, #8
            blt     G_M000_IG05
            b       G_M000_IG25

G_M000_IG06:                ;; offset=0x0228
            mov     x28, x22
            mov     w2, w23

G_M000_IG07:                ;; offset=0x0230
            str     w2, [fp, #0x4C]
            cbz     w23, G_M000_IG32
            ldp     q14, q15, [x22]
            mov     w3, wzr
            cmp     w2, #7
            blt     G_M000_IG14
            b       G_M000_IG11

G_M000_IG08:                ;; offset=0x024C
            fdiv    v0.4s, v18.4s, v2.4s
            fdiv    v1.4s, v19.4s, v3.4s

G_M000_IG09:                ;; offset=0x0254
            fadd    v10.4s, v10.4s, v0.4s
            fadd    v11.4s, v11.4s, v1.4s
            ldr     q16, [fp, #0x30]
            mov     v14.16b, v16.16b
            ldr     q17, [fp, #0x20]
            mov     v15.16b, v17.16b
            ldr     w4, [fp, #0x1C]
            mov     w3, w4
            cmp     w3, #7
            bge     G_M000_IG18

G_M000_IG10:                ;; offset=0x027C
            ldp     q13, q12, [fp, #0x50]

G_M000_IG11:                ;; offset=0x0280
            add     w4, w3, #1
            str     w4, [fp, #0x1C]
            cmp     w4, w23
            bhs     G_M000_IG32
            ubfiz   x1, x4, #5, #32
            add     x5, x22, x1
            ldp     q0, q1, [x5]
            mov     v16.16b, v0.16b
            mov     v17.16b, v1.16b
            str     q16, [fp, #0x30]
            fsub    v0.4s, v14.4s, v16.4s
            str     q17, [fp, #0x20]
            fsub    v1.4s, v15.4s, v17.4s
            fabs    v0.4s, v0.4s
            fabs    v1.4s, v1.4s
            fadd    v8.4s, v8.4s, v0.4s
            fadd    v9.4s, v9.4s, v1.4s
            ubfiz   x1, x3, #5, #32
            add     x6, x28, x1
            ldp     q0, q1, [x6]
            fsub    v0.4s, v14.4s, v0.4s
            fsub    v1.4s, v15.4s, v1.4s
            fabs    v0.4s, v0.4s
            fabs    v1.4s, v1.4s
            fadd    v8.4s, v8.4s, v0.4s
            fadd    v9.4s, v9.4s, v1.4s
            fadd    v0.4s, v14.4s, v13.4s
            fadd    v1.4s, v15.4s, v12.4s
            ldrb    w7, [x19, #0x69]
            ldr     x1, [x19, #0x10]
            movi    v2.4s, #0
            fmax    v0.4s, v0.4s, v2.4s
            movi    v2.4s, #0
            fmax    v1.4s, v1.4s, v2.4s
            fmul    v2.4s, v0.4s, v0.4s
            fmul    v3.4s, v1.4s, v1.4s
            ldr     q18, [@RWD16]
            fmul    v18.4s, v2.4s, v18.4s
            ldr     q19, [@RWD16]
            fmul    v19.4s, v3.4s, v19.4s
            ldr     q20, [@RWD32]
            fadd    v18.4s, v18.4s, v20.4s
            ldr     q20, [@RWD32]
            fadd    v19.4s, v19.4s, v20.4s
            ldr     q20, [@RWD48]
            fmul    v0.4s, v0.4s, v20.4s
            ldr     q20, [@RWD48]
            fmul    v1.4s, v1.4s, v20.4s
            fmul    v0.4s, v0.4s, v2.4s
            fmul    v1.4s, v1.4s, v3.4s
            ldr     q2, [@RWD64]
            fadd    v2.4s, v0.4s, v2.4s
            ldr     q0, [@RWD64]
            fadd    v3.4s, v1.4s, v0.4s
            cbz     w7, G_M000_IG08

G_M000_IG12:                ;; offset=0x035C
            mov     v0.16b, v18.16b
            mov     v1.16b, v19.16b
            mov     w0, #1
            movz    x3, #0x7840
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            mov     v15.d[0], v8.d[1]
            mov     v14.d[0], v9.d[1]
            mov     v12.d[0], v10.d[1]
            mov     v13.d[0], v11.d[1]
            blr     x3      // code for DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Runtime.Intrinsics.Vector256`1[float],System.Runtime.Intrinsics.Vector256`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Runtime.Intrinsics.Vector256`1[float]
            stp     q0, q1, [fp, #0x90]
            ldp     q0, q1, [fp, #0x90]
            mov     v8.d[1], v15.d[0]
            mov     v9.d[1], v14.d[0]
            mov     v10.d[1], v12.d[0]
            mov     v11.d[1], v13.d[0]
            b       G_M000_IG09

G_M000_IG13:                ;; offset=0x03A8
            ldp     q13, q12, [fp, #0x50]

G_M000_IG14:                ;; offset=0x03AC
            add     w4, w3, #1
            str     w4, [fp, #0x18]
            cmp     w4, w23
            bhs     G_M000_IG32
            ubfiz   x1, x4, #5, #32
            add     x5, x22, x1
            ldp     q16, q17, [x5]
            str     q16, [fp, #0x30]
            fsub    v0.4s, v14.4s, v16.4s
            str     q17, [fp, #0x20]
            fsub    v1.4s, v15.4s, v17.4s
            fabs    v0.4s, v0.4s
            fabs    v1.4s, v1.4s
            fadd    v8.4s, v8.4s, v0.4s
            fadd    v9.4s, v9.4s, v1.4s
            ldr     w2, [fp, #0x4C]
            cmp     w3, w2
            bhs     G_M000_IG32
            ubfiz   x1, x3, #5, #32
            add     x6, x28, x1
            ldp     q0, q1, [x6]
            fsub    v0.4s, v14.4s, v0.4s
            fsub    v1.4s, v15.4s, v1.4s
            fabs    v0.4s, v0.4s
            fabs    v1.4s, v1.4s
            fadd    v8.4s, v8.4s, v0.4s
            fadd    v9.4s, v9.4s, v1.4s
            fadd    v0.4s, v14.4s, v13.4s
            fadd    v1.4s, v15.4s, v12.4s
            ldrb    w7, [x19, #0x69]
            ldr     x1, [x19, #0x10]
            movi    v18.4s, #0
            fmax    v0.4s, v0.4s, v18.4s
            movi    v19.4s, #0
            fmax    v1.4s, v1.4s, v19.4s
            fmul    v2.4s, v0.4s, v0.4s
            fmul    v3.4s, v1.4s, v1.4s
            ldr     q18, [@RWD16]
            fmul    v18.4s, v2.4s, v18.4s
            ldr     q19, [@RWD16]
            fmul    v19.4s, v3.4s, v19.4s
            ldr     q20, [@RWD32]
            fadd    v18.4s, v18.4s, v20.4s
            ldr     q20, [@RWD32]
            fadd    v19.4s, v19.4s, v20.4s
            ldr     q20, [@RWD48]
            fmul    v0.4s, v0.4s, v20.4s
            ldr     q20, [@RWD48]
            fmul    v1.4s, v1.4s, v20.4s
            fmul    v0.4s, v0.4s, v2.4s
            fmul    v1.4s, v1.4s, v3.4s
            ldr     q2, [@RWD64]
            fadd    v2.4s, v0.4s, v2.4s
            ldr     q0, [@RWD64]
            fadd    v3.4s, v1.4s, v0.4s
            cbz     w7, G_M000_IG16

G_M000_IG15:                ;; offset=0x048C
            mov     v0.16b, v18.16b
            mov     v1.16b, v19.16b
            mov     w0, #1
            movz    x3, #0x7840
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            mov     v15.d[0], v8.d[1]
            mov     v14.d[0], v9.d[1]
            mov     v12.d[0], v10.d[1]
            mov     v13.d[0], v11.d[1]
            blr     x3      // code for DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Runtime.Intrinsics.Vector256`1[float],System.Runtime.Intrinsics.Vector256`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Runtime.Intrinsics.Vector256`1[float]
            stp     q0, q1, [fp, #0x90]
            ldp     q0, q1, [fp, #0x90]
            mov     v8.d[1], v15.d[0]
            mov     v9.d[1], v14.d[0]
            mov     v10.d[1], v12.d[0]
            mov     v11.d[1], v13.d[0]
            b       G_M000_IG17
            align   [4 bytes for IG24]
            align   [0 bytes]
            align   [0 bytes]
            align   [0 bytes]

G_M000_IG16:                ;; offset=0x04DC
            fdiv    v0.4s, v18.4s, v2.4s
            fdiv    v1.4s, v19.4s, v3.4s

G_M000_IG17:                ;; offset=0x04E4
            fadd    v10.4s, v10.4s, v0.4s
            fadd    v11.4s, v11.4s, v1.4s
            ldr     q16, [fp, #0x30]
            mov     v14.16b, v16.16b
            ldr     q17, [fp, #0x20]
            mov     v15.16b, v17.16b
            ldr     w4, [fp, #0x18]
            mov     w3, w4
            cmp     w3, #7
            blt     G_M000_IG13

G_M000_IG18:                ;; offset=0x050C
            ldr     w2, [fp, #0x4C]
            cmp     w2, #7
            bls     G_M000_IG32
            add     x1, x28, #224
            ldp     q0, q1, [x1]
            fsub    v0.4s, v14.4s, v0.4s
            fsub    v1.4s, v15.4s, v1.4s
            fabs    v0.4s, v0.4s
            fabs    v1.4s, v1.4s
            fadd    v8.4s, v8.4s, v0.4s
            fadd    v9.4s, v9.4s, v1.4s
            ldr     q13, [fp, #0x50]
            fadd    v0.4s, v14.4s, v13.4s
            ldr     q12, [fp, #0x60]
            fadd    v1.4s, v15.4s, v12.4s
            ldrb    w1, [x19, #0x69]
            ldr     x0, [x19, #0x10]
            movi    v2.4s, #0
            fmax    v0.4s, v0.4s, v2.4s
            movi    v2.4s, #0
            fmax    v1.4s, v1.4s, v2.4s
            fmul    v2.4s, v0.4s, v0.4s
            fmul    v3.4s, v1.4s, v1.4s
            ldr     q16, [@RWD16]
            fmul    v16.4s, v2.4s, v16.4s
            ldr     q17, [@RWD16]
            fmul    v17.4s, v3.4s, v17.4s
            ldr     q18, [@RWD32]
            fadd    v16.4s, v16.4s, v18.4s
            ldr     q18, [@RWD32]
            fadd    v17.4s, v17.4s, v18.4s
            ldr     q18, [@RWD48]
            fmul    v0.4s, v0.4s, v18.4s
            ldr     q18, [@RWD48]
            fmul    v1.4s, v1.4s, v18.4s
            fmul    v0.4s, v0.4s, v2.4s
            fmul    v1.4s, v1.4s, v3.4s
            ldr     q2, [@RWD64]
            fadd    v2.4s, v0.4s, v2.4s
            ldr     q0, [@RWD64]
            fadd    v3.4s, v1.4s, v0.4s
            cbz     w1, G_M000_IG20

G_M000_IG19:                ;; offset=0x05B4
            mov     v0.16b, v16.16b
            mov     v1.16b, v17.16b
            mov     x1, x0
            mov     w0, #1
            movz    x2, #0x7840
            movk    x2, #0x695F LSL #16
            movk    x2, #0x7FF8 LSL #32
            ldr     x2, [x2]
            mov     v15.d[0], v8.d[1]
            mov     v14.d[0], v9.d[1]
            mov     v12.d[0], v10.d[1]
            mov     v13.d[0], v11.d[1]
            blr     x2      // code for DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Runtime.Intrinsics.Vector256`1[float],System.Runtime.Intrinsics.Vector256`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Runtime.Intrinsics.Vector256`1[float]
            stp     q0, q1, [fp, #0x70]
            ldp     q16, q17, [fp, #0x70]
            mov     v8.d[1], v15.d[0]
            mov     v9.d[1], v14.d[0]
            mov     v10.d[1], v12.d[0]
            mov     v11.d[1], v13.d[0]
            b       G_M000_IG21

G_M000_IG20:                ;; offset=0x0604
            fdiv    v16.4s, v16.4s, v2.4s
            fdiv    v17.4s, v17.4s, v3.4s

G_M000_IG21:                ;; offset=0x060C
            fadd    v10.4s, v10.4s, v16.4s
            fadd    v11.4s, v11.4s, v17.4s
            ldr     w0, [x19, #0x54]
            add     w26, w26, w0
            add     w27, w27, #1
            cmp     w27, #8
            bge     G_M000_IG28

G_M000_IG22:                ;; offset=0x0628
            cmp     w27, #7
            mov     x0, x22
            mov     x22, x24
            mov     x24, x0
            mov     w0, w23
            mov     w23, w25
            mov     w25, w0
            ldp     q13, q12, [fp, #0x50]
            beq     G_M000_IG26

G_M000_IG23:                ;; offset=0x064C
            ldr     w1, [x19, #0x54]
            add     w1, w26, w1
            mov     w0, wzr
            cmp     w25, #8
            blt     G_M000_IG05

G_M000_IG24:                ;; offset=0x0660
            ubfiz   x2, x0, #5, #32
            add     x2, x24, x2
            ldr     x3, [x19, #0x18]
            add     w4, w1, w0,  LSL #3
            cbz     x3, G_M000_IG30
            ldr     w5, [x3, #0x08]
            mov     w6, w4
            add     x6, x6, #8
            cmp     x5, x6
            blo     G_M000_IG30
            add     x3, x3, #16
            ubfiz   x4, x4, #2, #32
            add     x3, x3, x4
            ldp     q0, q1, [x3]
            stp     q0, q1, [x2]
            add     w0, w0, #1
            cmp     w0, #8
            blt     G_M000_IG24

G_M000_IG25:                ;; offset=0x06A8
            mov     w1, w25
            cmp     w1, #8
            blt     G_M000_IG31
            add     x1, x24, #32
            add     x0, x24, #64
            add     x2, x24, #96
            add     x3, x24, #128
            add     x4, x24, #160
            add     x5, x24, #192
            add     x6, x24, #224
            ldr     q0, [x24]
            ldr     q1, [x1]
            zip1    v2.4s, v0.4s, v1.4s
            zip2    v0.4s, v0.4s, v1.4s
            ldr     q1, [x24, #0x10]
            ldr     q3, [x1, #0x10]
            zip1    v16.4s, v1.4s, v3.4s
            zip2    v1.4s, v1.4s, v3.4s
            ldr     q3, [x0]
            ldr     q17, [x2]
            zip1    v18.4s, v3.4s, v17.4s
            zip2    v3.4s, v3.4s, v17.4s
            ldr     q17, [x0, #0x10]
            ldr     q19, [x2, #0x10]
            zip1    v20.4s, v17.4s, v19.4s
            zip2    v17.4s, v17.4s, v19.4s
            ldr     q19, [x3]
            ldr     q21, [x4]
            zip1    v22.4s, v19.4s, v21.4s
            zip2    v19.4s, v19.4s, v21.4s
            ldr     q21, [x3, #0x10]
            ldr     q23, [x4, #0x10]
            zip1    v24.4s, v21.4s, v23.4s
            zip2    v21.4s, v21.4s, v23.4s
            ldr     q23, [x5]
            ldr     q25, [x6]
            zip1    v26.4s, v23.4s, v25.4s
            zip2    v23.4s, v23.4s, v25.4s
            ldr     q25, [x5, #0x10]
            ldr     q27, [x6, #0x10]
            zip1    v28.4s, v25.4s, v27.4s
            zip2    v25.4s, v25.4s, v27.4s
            zip1    v27.2d, v2.2d, v18.2d
            zip2    v2.2d, v2.2d, v18.2d
            zip1    v18.2d, v0.2d, v3.2d
            zip2    v0.2d, v0.2d, v3.2d
            zip1    v3.2d, v16.2d, v20.2d
            zip2    v16.2d, v16.2d, v20.2d
            zip1    v20.2d, v1.2d, v17.2d
            zip2    v1.2d, v1.2d, v17.2d
            zip1    v17.2d, v22.2d, v26.2d
            zip2    v22.2d, v22.2d, v26.2d
            zip1    v26.2d, v19.2d, v23.2d
            zip2    v19.2d, v19.2d, v23.2d
            zip1    v23.2d, v24.2d, v28.2d
            zip2    v24.2d, v24.2d, v28.2d
            zip1    v28.2d, v21.2d, v25.2d
            zip2    v21.2d, v21.2d, v25.2d
            stp     q27, q17, [x24]
            stp     q2, q22, [x1]
            stp     q18, q26, [x0]
            stp     q0, q19, [x2]
            stp     q3, q23, [x3]
            stp     q16, q24, [x4]
            stp     q20, q28, [x5]
            stp     q1, q21, [x6]

G_M000_IG26:                ;; offset=0x07B0
            cmp     w27, #7
            beq     G_M000_IG06

G_M000_IG27:                ;; offset=0x07B8
            mov     x28, x24
            mov     w2, w25
            b       G_M000_IG07

G_M000_IG28:                ;; offset=0x07C4
            ldr     q16, [@RWD80]
            fmul    v16.4s, v8.4s, v16.4s
            ldr     q17, [@RWD80]
            fmul    v17.4s, v9.4s, v17.4s
            cmp     w21, #8
            blt     G_M000_IG30
            stp     q16, q17, [x20]
            ldr     q16, [@RWD96]
            fmul    v16.4s, v10.4s, v16.4s
            ldr     q17, [@RWD96]
            fmul    v17.4s, v11.4s, v17.4s
            cmp     w21, #16
            blt     G_M000_IG30
            add     x0, x20, #32
            stp     q16, q17, [x0]

G_M000_IG29:                ;; offset=0x0800
            ldp     x27, x28, [sp, #0x130]
            ldp     x25, x26, [sp, #0x120]
            ldp     x23, x24, [sp, #0x110]
            ldp     x21, x22, [sp, #0x100]
            ldp     x19, x20, [sp, #0xF0]
            ldp     d14, d15, [sp, #0xE0]
            ldp     d12, d13, [sp, #0xD0]
            ldp     d10, d11, [sp, #0xC0]
            ldp     d8, d9, [sp, #0xB0]
            ldp     fp, lr, [sp], #0x140
            ret     lr

G_M000_IG30:                ;; offset=0x082C
            movz    x0, #0x4450
            movk    x0, #0x695F LSL #16
            movk    x0, #0x7FF8 LSL #32
            ldr     x0, [x0]
            blr     x0      // code for System.ThrowHelper:ThrowArgumentOutOfRangeException()
            brk     #0

G_M000_IG31:                ;; offset=0x0844
            movz    x0, #0xF360
            movk    x0, #0x693D LSL #16
            movk    x0, #0x7FF8 LSL #32
            bl      CORINFO_HELP_NEWSFAST
            mov     x19, x0
            movz    x1, #0x6350
            movk    x1, #0xA15 LSL #16
            movk    x1, #548 LSL #32
            movz    x2, #0x63C8
            movk    x2, #0xA15 LSL #16
            movk    x2, #548 LSL #32
            movz    x3, #0x4438
            movk    x3, #0x695F LSL #16
            movk    x3, #0x7FF8 LSL #32
            ldr     x3, [x3]
            blr     x3      // code for System.ArgumentException:.ctor(System.String,System.String):this
            mov     x0, x19
            bl      CORINFO_HELP_THROW
            brk     #0

G_M000_IG32:                ;; offset=0x0890
            bl      CORINFO_HELP_RNGCHKFAIL
            brk     #0

RWD00  	dq	4223333242233332h, 4223333242233332h
RWD16  	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD32  	dq	44228FFE44228FFEh, 44228FFE44228FFEh
RWD48  	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD64  	dq	449E380D449E380Dh, 449E380D449E380Dh
RWD80  	dq	B8933E1EB8933E1Eh, B8933E1EB8933E1Eh
RWD96  	dq	3880808138808081h, 3880808138808081h

; Total bytes of code 2200
