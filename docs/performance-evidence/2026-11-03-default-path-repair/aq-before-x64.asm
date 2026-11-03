; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:.ctor(int,int,int,System.Buffers.ArrayPool`1[float],int):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rbp based frame
; fully interruptible
; No PGO data
; 2 inlinees with PGO data; 14 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbp
       push     r15
       push     r14
       push     r13
       push     rdi
       push     rsi
       push     rbx
       sub      rsp, 32
       lea      rbp, [rsp+0x50]
       mov      gword ptr [rbp+0x10], rcx
       mov      esi, edx
       mov      edi, r8d
       mov      ebx, r9d
       mov      r14d, dword ptr [rbp+0x38]

G_M000_IG02:                ;; offset=0x0023
       test     byte  ptr [(reloc 0x7ff882d8b0c8)], 1
       je       G_M000_IG11

G_M000_IG03:                ;; offset=0x0030
       mov      rcx, 0x2199F800B58
       mov      r15, gword ptr [rcx]
       mov      r13, gword ptr [rbp+0x10]
       lea      rcx, bword ptr [r13+0x10]
       mov      rdx, r15
       call     CORINFO_HELP_ASSIGN_REF
       lea      rcx, bword ptr [r13+0x18]
       mov      rdx, r15
       call     CORINFO_HELP_ASSIGN_REF
       lea      rcx, bword ptr [r13+0x20]
       mov      rdx, r15
       call     CORINFO_HELP_ASSIGN_REF
       lea      rcx, bword ptr [r13+0x28]
       mov      rdx, r15
       call     CORINFO_HELP_ASSIGN_REF
       lea      rcx, bword ptr [r13+0x30]
       mov      rdx, r15
       call     CORINFO_HELP_ASSIGN_REF
       test     esi, esi
       jle      G_M000_IG12
       test     edi, edi
       jle      G_M000_IG13

G_M000_IG04:                ;; offset=0x008D
       mov      eax, ebx
       sub      eax, 1
       cmp      eax, 3
       ja       G_M000_IG14
       mov      dword ptr [r13+0x40], esi
       mov      dword ptr [r13+0x44], edi
       mov      dword ptr [r13+0x48], ebx
       cmp      r14d, 2
       ja       G_M000_IG15

G_M000_IG05:                ;; offset=0x00B1
       mov      eax, 3
       bt       eax, r14d
       jae      G_M000_IG10

G_M000_IG06:                ;; offset=0x00C0
       xor      eax, eax

G_M000_IG07:                ;; offset=0x00C2
       mov      byte  ptr [r13+0x68], al
       add      esi, 7
       jo       G_M000_IG16
       mov      ecx, esi
       sar      ecx, 31
       and      ecx, 7
       add      ecx, esi
       sar      ecx, 3
       mov      dword ptr [r13+0x64], ecx
       imul     esi, ecx, 8
       jo       G_M000_IG16
       mov      dword ptr [r13+0x4C], esi
       lea      r14d, [8*rbx]
       mov      eax, edi
       add      eax, r14d
       jo       G_M000_IG16
       sub      eax, 1
       jo       G_M000_IG16
       cdq
       idiv     edx:eax, r14d
       imul     eax, r14d
       jo       G_M000_IG16
       mov      dword ptr [r13+0x50], eax
       add      esi, 2
       jo       G_M000_IG16
       mov      dword ptr [r13+0x54], esi
       imul     ecx, ecx, 2
       jo       G_M000_IG16
       mov      dword ptr [r13+0x58], ecx
       add      r14d, 10
       imul     esi, r14d
       jo       G_M000_IG16
       add      ebx, ebx
       add      ebx, 2
       jo       G_M000_IG16
       imul     ebx, ecx
       jo       G_M000_IG16
       imul     edi, ecx, 2
       jo       G_M000_IG16
       xor      rcx, rcx
       cmp      esi, 0x40000
       mov      rdx, rcx
       cmovle   rdx, gword ptr [rbp+0x30]
       lea      rcx, bword ptr [r13+0x08]
       call     CORINFO_HELP_ASSIGN_REF
       nop

G_M000_IG08:                ;; offset=0x017A
       mov      rcx, r13
       mov      edx, esi
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:Allocate(int):float[]:this]
       mov      rdx, rax
       lea      rcx, bword ptr [r13+0x10]
       call     CORINFO_HELP_ASSIGN_REF
       mov      rcx, r13
       mov      edx, ebx
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:Allocate(int):float[]:this]
       mov      rdx, rax
       lea      rcx, bword ptr [r13+0x18]
       call     CORINFO_HELP_ASSIGN_REF
       mov      edx, dword ptr [r13+0x4C]
       mov      rcx, r13
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:Allocate(int):float[]:this]
       mov      rdx, rax
       lea      rcx, bword ptr [r13+0x20]
       call     CORINFO_HELP_ASSIGN_REF
       mov      rcx, r13
       mov      edx, edi
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:Allocate(int):float[]:this]
       mov      rdx, rax
       lea      rcx, bword ptr [r13+0x28]
       call     CORINFO_HELP_ASSIGN_REF
       nop

G_M000_IG09:                ;; offset=0x01D9
       add      rsp, 32
       pop      rbx
       pop      rsi
       pop      rdi
       pop      r13
       pop      r14
       pop      r15
       pop      rbp
       ret

G_M000_IG10:                ;; offset=0x01E8
       mov      eax, 1
       jmp      G_M000_IG07

G_M000_IG11:                ;; offset=0x01F2
       mov      rcx, 0x7FF882D8B040
       call     CORINFO_HELP_GET_GCSTATIC_BASE
       jmp      G_M000_IG03

G_M000_IG12:                ;; offset=0x0206
       mov      ecx, esi
       mov      rdx, 0x231A58E6198
       call     [System.ArgumentOutOfRangeException:ThrowNegativeOrZero[int](int,System.String)]
       int3

G_M000_IG13:                ;; offset=0x0219
       mov      ecx, edi
       mov      rdx, 0x231A58E6170
       call     [System.ArgumentOutOfRangeException:ThrowNegativeOrZero[int](int,System.String)]
       int3

G_M000_IG14:                ;; offset=0x022C
       mov      rcx, 0x7FF882C3DDB8
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      rcx, rbx
       mov      rdx, 0x231A58E6138
       call     [System.ArgumentOutOfRangeException:.ctor(System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3

G_M000_IG15:                ;; offset=0x025A
       mov      rcx, 0x7FF882C3DDB8
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      rcx, rbx
       mov      rdx, 0x231A58E6108
       call     [System.ArgumentOutOfRangeException:.ctor(System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3

G_M000_IG16:                ;; offset=0x0288
       call     CORINFO_HELP_OVERFLOW
       int3

G_M000_IG17:                ;; offset=0x028E
       sub      rsp, 40

G_M000_IG18:                ;; offset=0x0292
       mov      rcx, gword ptr [rbp+0x10]
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:Dispose():this]
       call     CORINFO_HELP_RETHROW
       int3

; Total bytes of code 674

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:Allocate(int):float[]:this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbx
       sub      rsp, 32
       mov      ebx, edx

G_M000_IG02:                ;; offset=0x0007
       mov      rcx, gword ptr [rcx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG04

G_M000_IG03:                ;; offset=0x0010
       mov      edx, ebx
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]
       call     [rax+0x20]System.Buffers.ArrayPool`1[float]:Rent(int):float[]:this
       jmp      SHORT G_M000_IG05

G_M000_IG04:                ;; offset=0x001E
       xor      rax, rax

G_M000_IG05:                ;; offset=0x0020
       test     rax, rax
       jne      SHORT G_M000_IG07

G_M000_IG06:                ;; offset=0x0025
       movsxd   rdx, ebx
       mov      rcx, 0x7FF882D80700
       call     CORINFO_HELP_NEWARR_1_VC

G_M000_IG07:                ;; offset=0x0037
       nop

G_M000_IG08:                ;; offset=0x0038
       add      rsp, 32
       pop      rbx
       ret

; Total bytes of code 62

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:.ctor(int,int,int,System.Buffers.ArrayPool`1[float],int):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rbp based frame
; fully interruptible
; No PGO data
; 2 inlinees with PGO data; 13 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbp
       push     rdi
       push     rsi
       push     rbx
       sub      rsp, 40
       lea      rbp, [rsp+0x40]
       mov      gword ptr [rbp+0x10], rcx
       mov      eax, dword ptr [rbp+0x38]

G_M000_IG02:                ;; offset=0x0014
       mov      r10, 0x231A58E61B8
       mov      gword ptr [rcx+0x10], r10
       mov      gword ptr [rcx+0x18], r10
       mov      gword ptr [rcx+0x20], r10
       mov      gword ptr [rcx+0x28], r10
       test     edx, edx
       jle      G_M000_IG10
       test     r8d, r8d
       jle      G_M000_IG11

G_M000_IG03:                ;; offset=0x003F
       mov      r10d, r9d
       sub      r10d, 1
       cmp      r10d, 3
       ja       G_M000_IG12
       mov      dword ptr [rcx+0x38], edx
       mov      dword ptr [rcx+0x3C], r8d
       mov      dword ptr [rcx+0x40], r9d
       cmp      eax, 2
       ja       G_M000_IG13

G_M000_IG04:                ;; offset=0x0064
       mov      r10d, 3
       bt       r10d, eax
       jae      G_M000_IG09

G_M000_IG05:                ;; offset=0x0074
       xor      eax, eax

G_M000_IG06:                ;; offset=0x0076
       mov      byte  ptr [rcx+0x60], al
       add      edx, 7
       jo       G_M000_IG14
       mov      r10d, edx
       sar      r10d, 31
       and      r10d, 7
       add      r10d, edx
       sar      r10d, 3
       mov      dword ptr [rcx+0x5C], r10d
       imul     ebx, r10d, 8
       jo       G_M000_IG14
       mov      dword ptr [rcx+0x44], ebx
       lea      esi, [8*r9]
       mov      eax, r8d
       add      eax, esi
       jo       G_M000_IG14
       sub      eax, 1
       jo       G_M000_IG14
       cdq
       idiv     edx:eax, esi
       imul     eax, esi
       jo       G_M000_IG14
       mov      dword ptr [rcx+0x48], eax
       add      ebx, 2
       jo       G_M000_IG14
       mov      dword ptr [rcx+0x4C], ebx
       imul     edx, r10d, 2
       jo       G_M000_IG14
       mov      dword ptr [rcx+0x50], edx
       add      esi, 10
       imul     ebx, esi
       jo       G_M000_IG14
       add      r9d, r9d
       add      r9d, 2
       jo       G_M000_IG14
       mov      esi, edx
       imul     esi, r9d
       jo       G_M000_IG14
       imul     edi, edx, 2
       jo       G_M000_IG14
       xor      rdx, rdx
       cmp      ebx, 0x40000
       cmovle   rdx, gword ptr [rbp+0x30]
       lea      rcx, bword ptr [rcx+0x08]
       call     CORINFO_HELP_ASSIGN_REF
       nop

G_M000_IG07:                ;; offset=0x012E
       mov      rcx, gword ptr [rbp+0x10]
       mov      edx, ebx
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:Allocate(int):float[]:this]
       mov      rdx, rax
       mov      rcx, gword ptr [rbp+0x10]
       lea      rcx, bword ptr [rcx+0x10]
       call     CORINFO_HELP_ASSIGN_REF
       mov      rcx, gword ptr [rbp+0x10]
       mov      edx, esi
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:Allocate(int):float[]:this]
       mov      rdx, rax
       mov      rcx, gword ptr [rbp+0x10]
       lea      rcx, bword ptr [rcx+0x18]
       call     CORINFO_HELP_ASSIGN_REF
       mov      rcx, gword ptr [rbp+0x10]
       mov      edx, edi
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:Allocate(int):float[]:this]
       mov      rdx, rax
       mov      rcx, gword ptr [rbp+0x10]
       lea      rcx, bword ptr [rcx+0x20]
       call     CORINFO_HELP_ASSIGN_REF
       nop

G_M000_IG08:                ;; offset=0x0183
       add      rsp, 40
       pop      rbx
       pop      rsi
       pop      rdi
       pop      rbp
       ret

G_M000_IG09:                ;; offset=0x018C
       mov      eax, 1
       jmp      G_M000_IG06

G_M000_IG10:                ;; offset=0x0196
       mov      ecx, edx
       mov      rdx, 0x231A58E6198
       call     [System.ArgumentOutOfRangeException:ThrowNegativeOrZero[int](int,System.String)]
       int3

G_M000_IG11:                ;; offset=0x01A9
       mov      ecx, r8d
       mov      rdx, 0x231A58E6170
       call     [System.ArgumentOutOfRangeException:ThrowNegativeOrZero[int](int,System.String)]
       int3

G_M000_IG12:                ;; offset=0x01BD
       mov      rcx, 0x7FF882C3DDB8
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      rcx, rbx
       mov      rdx, 0x231A58E6138
       call     [System.ArgumentOutOfRangeException:.ctor(System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3

G_M000_IG13:                ;; offset=0x01EB
       mov      rcx, 0x7FF882C3DDB8
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      rcx, rbx
       mov      rdx, 0x231A58E6108
       call     [System.ArgumentOutOfRangeException:.ctor(System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3

G_M000_IG14:                ;; offset=0x0219
       call     CORINFO_HELP_OVERFLOW
       int3

G_M000_IG15:                ;; offset=0x021F
       sub      rsp, 40

G_M000_IG16:                ;; offset=0x0223
       mov      rcx, gword ptr [rbp+0x10]
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:Dispose():this]
       call     CORINFO_HELP_RETHROW
       int3

; Total bytes of code 563

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:Allocate(int):float[]:this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbx
       sub      rsp, 32
       mov      ebx, edx

G_M000_IG02:                ;; offset=0x0007
       mov      rcx, gword ptr [rcx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG04

G_M000_IG03:                ;; offset=0x0010
       mov      edx, ebx
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]
       call     [rax+0x20]System.Buffers.ArrayPool`1[float]:Rent(int):float[]:this
       jmp      SHORT G_M000_IG05

G_M000_IG04:                ;; offset=0x001E
       xor      rax, rax

G_M000_IG05:                ;; offset=0x0020
       test     rax, rax
       jne      SHORT G_M000_IG07

G_M000_IG06:                ;; offset=0x0025
       movsxd   rdx, ebx
       mov      rcx, 0x7FF882D80700
       call     CORINFO_HELP_NEWARR_1_VC

G_M000_IG07:                ;; offset=0x0037
       nop

G_M000_IG08:                ;; offset=0x0038
       add      rsp, 32
       pop      rbx
       ret

; Total bytes of code 62

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:.ctor(int,int,int,System.Buffers.ArrayPool`1[float],int,int,DotJpegli.Internal.AqRationalDiagnostics):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rbp based frame
; fully interruptible
; No PGO data
; 2 inlinees with PGO data; 13 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbp
       push     r15
       push     r14
       push     rdi
       push     rsi
       push     rbx
       sub      rsp, 40
       lea      rbp, [rsp+0x50]
       mov      gword ptr [rbp+0x10], rcx
       mov      esi, edx
       mov      edi, r8d
       mov      ebx, r9d
       mov      edx, dword ptr [rbp+0x38]
       mov      eax, dword ptr [rbp+0x40]

G_M000_IG02:                ;; offset=0x0023
       mov      r8, 0x231A58E61B8
       mov      gword ptr [rcx+0x18], r8
       mov      gword ptr [rcx+0x20], r8
       mov      gword ptr [rcx+0x28], r8
       mov      gword ptr [rcx+0x30], r8
       test     esi, esi
       jle      G_M000_IG14
       test     edi, edi
       jle      G_M000_IG15

G_M000_IG03:                ;; offset=0x004D
       mov      r8d, ebx
       sub      r8d, 1
       cmp      r8d, 3
       ja       G_M000_IG16
       mov      dword ptr [rcx+0x40], esi
       mov      dword ptr [rcx+0x44], edi
       mov      dword ptr [rcx+0x48], ebx
       cmp      edx, 2
       ja       G_M000_IG17

G_M000_IG04:                ;; offset=0x0070
       mov      r8d, 3
       bt       r8d, edx
       jae      G_M000_IG12

G_M000_IG05:                ;; offset=0x0080
       xor      edx, edx

G_M000_IG06:                ;; offset=0x0082
       mov      byte  ptr [rcx+0x68], dl
       cmp      eax, 1
       jbe      G_M000_IG13

G_M000_IG07:                ;; offset=0x008E
       cmp      eax, 2
       jne      G_M000_IG18
       mov      edx, 1

G_M000_IG08:                ;; offset=0x009C
       mov      byte  ptr [rcx+0x69], dl
       lea      rcx, bword ptr [rcx+0x10]
       mov      rdx, gword ptr [rbp+0x48]
       call     CORINFO_HELP_ASSIGN_REF
       add      esi, 7
       jo       G_M000_IG19
       mov      ecx, esi
       sar      ecx, 31
       and      ecx, 7
       add      ecx, esi
       sar      ecx, 3
       mov      rsi, gword ptr [rbp+0x10]
       mov      dword ptr [rsi+0x64], ecx
       imul     r14d, ecx, 8
       jo       G_M000_IG19
       mov      dword ptr [rsi+0x4C], r14d
       lea      r15d, [8*rbx]
       mov      eax, edi
       add      eax, r15d
       jo       G_M000_IG19
       sub      eax, 1
       jo       G_M000_IG19
       cdq
       idiv     edx:eax, r15d
       imul     eax, r15d
       jo       G_M000_IG19
       mov      dword ptr [rsi+0x50], eax
       add      r14d, 2
       jo       G_M000_IG19
       mov      dword ptr [rsi+0x54], r14d
       imul     ecx, ecx, 2
       jo       G_M000_IG19
       mov      dword ptr [rsi+0x58], ecx
       add      r15d, 10
       imul     r14d, r15d
       jo       G_M000_IG19
       add      ebx, ebx
       add      ebx, 2
       jo       G_M000_IG19
       imul     ebx, ecx
       jo       G_M000_IG19
       imul     edi, ecx, 2
       jo       G_M000_IG19
       xor      rcx, rcx
       cmp      r14d, 0x40000
       mov      rdx, rcx
       cmovle   rdx, gword ptr [rbp+0x30]
       lea      rcx, bword ptr [rsi+0x08]

G_M000_IG09:                ;; offset=0x015E
       call     CORINFO_HELP_ASSIGN_REF
       nop

G_M000_IG10:                ;; offset=0x0164
       mov      rcx, rsi
       mov      edx, r14d
       call     [DotJpegli.Internal.AdaptiveQuantization:Allocate(int):float[]:this]
       mov      rdx, rax
       lea      rcx, bword ptr [rsi+0x18]
       call     CORINFO_HELP_ASSIGN_REF
       mov      rcx, rsi
       mov      edx, ebx
       call     [DotJpegli.Internal.AdaptiveQuantization:Allocate(int):float[]:this]
       mov      rdx, rax
       lea      rcx, bword ptr [rsi+0x20]
       call     CORINFO_HELP_ASSIGN_REF
       mov      rcx, rsi
       mov      edx, edi
       call     [DotJpegli.Internal.AdaptiveQuantization:Allocate(int):float[]:this]
       mov      rdx, rax
       lea      rcx, bword ptr [rsi+0x28]
       call     CORINFO_HELP_ASSIGN_REF
       nop

G_M000_IG11:                ;; offset=0x01AB
       add      rsp, 40
       pop      rbx
       pop      rsi
       pop      rdi
       pop      r14
       pop      r15
       pop      rbp
       ret

G_M000_IG12:                ;; offset=0x01B8
       mov      edx, 1
       jmp      G_M000_IG06

G_M000_IG13:                ;; offset=0x01C2
       xor      edx, edx
       jmp      G_M000_IG08

G_M000_IG14:                ;; offset=0x01C9
       mov      ecx, esi
       mov      rdx, 0x231A58E6198
       call     [System.ArgumentOutOfRangeException:ThrowNegativeOrZero[int](int,System.String)]
       int3

G_M000_IG15:                ;; offset=0x01DC
       mov      ecx, edi
       mov      rdx, 0x231A58E6170
       call     [System.ArgumentOutOfRangeException:ThrowNegativeOrZero[int](int,System.String)]
       int3

G_M000_IG16:                ;; offset=0x01EF
       mov      rcx, 0x7FF882C3DDB8
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      rcx, rbx
       mov      rdx, 0x231A58E6138
       call     [System.ArgumentOutOfRangeException:.ctor(System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3

G_M000_IG17:                ;; offset=0x021D
       mov      rcx, 0x7FF882C3DDB8
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      rcx, rbx
       mov      rdx, 0x231A58E6108
       call     [System.ArgumentOutOfRangeException:.ctor(System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3

G_M000_IG18:                ;; offset=0x024B
       mov      rcx, 0x7FF882C3DDB8
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      rcx, rbx
       mov      rdx, 0x231A58E61D0
       call     [System.ArgumentOutOfRangeException:.ctor(System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3

G_M000_IG19:                ;; offset=0x0279
       call     CORINFO_HELP_OVERFLOW
       int3

G_M000_IG20:                ;; offset=0x027F
       sub      rsp, 40

G_M000_IG21:                ;; offset=0x0283
       mov      rcx, gword ptr [rbp+0x10]
       call     [DotJpegli.Internal.AdaptiveQuantization:Dispose():this]
       call     CORINFO_HELP_RETHROW
       int3

; Total bytes of code 659

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:Allocate(int):float[]:this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbx
       sub      rsp, 32
       mov      ebx, edx

G_M000_IG02:                ;; offset=0x0007
       mov      rcx, gword ptr [rcx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG04

G_M000_IG03:                ;; offset=0x0010
       mov      edx, ebx
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]
       call     [rax+0x20]System.Buffers.ArrayPool`1[float]:Rent(int):float[]:this
       jmp      SHORT G_M000_IG05

G_M000_IG04:                ;; offset=0x001E
       xor      rax, rax

G_M000_IG05:                ;; offset=0x0020
       test     rax, rax
       jne      SHORT G_M000_IG07

G_M000_IG06:                ;; offset=0x0025
       movsxd   rdx, ebx
       mov      rcx, 0x7FF882D80700
       call     CORINFO_HELP_NEWARR_1_VC

G_M000_IG07:                ;; offset=0x0037
       nop

G_M000_IG08:                ;; offset=0x0038
       add      rsp, 32
       pop      rbx
       ret

; Total bytes of code 62

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeRowCore(DotJpegli.Internal.PixelSource,int,int,int,System.Span`1[float],bool,bool,System.Threading.CancellationToken):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rbp based frame
; partially interruptible
; No PGO data
; 9 inlinees with PGO data; 35 single block inlinees; 33 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbp
       push     r15
       push     r14
       push     r13
       push     r12
       push     rdi
       push     rsi
       push     rbx
       sub      rsp, 456
       vzeroupper
       vmovaps  xmmword ptr [rsp+0x1B0], xmm6
       vmovaps  xmmword ptr [rsp+0x1A0], xmm7
       vmovaps  xmmword ptr [rsp+0x190], xmm8
       vmovaps  xmmword ptr [rsp+0x180], xmm9
       vmovaps  xmmword ptr [rsp+0x170], xmm10
       vmovaps  xmmword ptr [rsp+0x160], xmm11
       vmovaps  xmmword ptr [rsp+0x150], xmm12
       vmovaps  xmmword ptr [rsp+0x140], xmm13
       lea      rbp, [rsp+0x40]
       vxorps   xmm4, xmm4, xmm4
       vmovdqu  ymmword ptr [rbp+0x60], ymm4
       vmovdqu  ymmword ptr [rbp+0x80], ymm4
       vmovdqa  xmmword ptr [rbp+0xA0], xmm4
       mov      rax, 0x6911662D01EE
       mov      qword ptr [rbp+0xF8], rax
       mov      rax, bword ptr [rbp+0x1F8]
       mov      ebx, dword ptr [rbp+0x200]

G_M000_IG02:                ;; offset=0x009A
       mov      r10, gword ptr [rbp+0x210]
       mov      gword ptr [rbp+0x60], r10
       mov      edi, r9d
       mov      dword ptr [rbp+0x68], edi
       mov      r14d, r8d
       mov      dword ptr [rbp+0x6C], r14d
       mov      r15, rdx
       mov      r13, rcx
       mov      r12, bword ptr [rax]
       mov      bword ptr [rbp+0x18], r12
       mov      r10d, dword ptr [rax+0x08]
       mov      dword ptr [rbp+0xB4], r10d
       cmp      byte  ptr [r13+0x69], 0
       jne      G_M000_IG132
       mov      r9d, dword ptr [r13+0x48]
       shl      r9d, 3
       mov      dword ptr [rbp+0xF4], r9d
       mov      r11, gword ptr [r15+0x08]
       mov      gword ptr [rbp+0x50], r11
       test     r11, r11
       je       SHORT G_M000_IG08

G_M000_IG03:                ;; offset=0x00F1
       cmp      r11, gword ptr [r13+0x38]
       je       SHORT G_M000_IG05

G_M000_IG04:                ;; offset=0x00F7
       mov      r11, gword ptr [rbp+0x50]
       jmp      SHORT G_M000_IG08

G_M000_IG05:                ;; offset=0x00FD
       mov      eax, r9d
       add      eax, dword ptr [r13+0x5C]
       cmp      eax, edi
       je       SHORT G_M000_IG07

G_M000_IG06:                ;; offset=0x0108
       mov      r11, gword ptr [rbp+0x50]
       jmp      SHORT G_M000_IG08

G_M000_IG07:                ;; offset=0x010E
       cmp      r14d, dword ptr [r13+0x60]
       mov      r11, gword ptr [rbp+0x50]
       mov      r14d, dword ptr [rbp+0x6C]
       je       SHORT G_M000_IG09

G_M000_IG08:                ;; offset=0x011C
       xor      ecx, ecx
       jmp      SHORT G_M000_IG10

G_M000_IG09:                ;; offset=0x0120
       movzx    r8, bl
       movzx    rcx, byte  ptr [r13+0x6A]
       cmp      r8d, ecx
       sete     cl
       movzx    rcx, cl

G_M000_IG10:                ;; offset=0x0132
       mov      dword ptr [rbp+0xF0], ecx
       xor      rax, rax
       mov      gword ptr [r13+0x38], rax
       cmp      gword ptr [rbp+0x60], 0
       jne      G_M000_IG129

G_M000_IG11:                ;; offset=0x0149
       test     edi, edi
       jl       G_M000_IG134
       cmp      edi, dword ptr [r13+0x44]
       jge      G_M000_IG134
       mov      eax, edi
       cdq
       idiv     edx:eax, r9d
       test     edx, edx
       jne      G_M000_IG134
       mov      r8d, dword ptr [r13+0x64]
       imul     r8d, dword ptr [r13+0x48]
       cmp      r8d, r10d
       jg       G_M000_IG135
       lea      eax, [rdi-0x05]
       mov      dword ptr [rbp+0xEC], eax
       xor      edx, edx
       test     ecx, ecx
       je       G_M000_IG130

G_M000_IG12:                ;; offset=0x018E
       mov      r8, gword ptr [r13+0x10]
       mov      gword ptr [rbp+0x10], r8
       mov      rdx, r8
       mov      r8d, dword ptr [r13+0x54]
       mov      dword ptr [rbp+0x78], r8d
       imul     r8d, r9d
       mov      ecx, dword ptr [rbp+0x78]
       lea      ecx, [rcx+4*rcx]
       add      ecx, ecx
       test     rdx, rdx
       jne      G_M000_IG131
       or       r8d, ecx
       jne      G_M000_IG136
       xor      rdx, rdx
       mov      bword ptr [rbp+0x38], rdx
       xor      ecx, ecx

G_M000_IG13:                ;; offset=0x01C7
       mov      r8, gword ptr [rbp+0x10]
       test     r8, r8
       je       SHORT G_M000_IG14
       lea      rdx, bword ptr [r8+0x10]
       mov      r8d, dword ptr [r8+0x08]
       mov      bword ptr [rbp+0x30], rdx
       mov      edx, r8d
       jmp      SHORT G_M000_IG15

G_M000_IG14:                ;; offset=0x01E1
       xor      r8, r8
       mov      bword ptr [rbp+0x30], r8
       xor      edx, edx

G_M000_IG15:                ;; offset=0x01EA
       cmp      ecx, edx
       jg       G_M000_IG137
       mov      ecx, ecx
       lea      r8, [4*rcx]
       mov      rcx, bword ptr [rbp+0x30]
       mov      rdx, bword ptr [rbp+0x38]
       call     [System.SpanHelpers:Memmove(byref,byref,nuint)]
       mov      eax, 10
       mov      r10d, dword ptr [rbp+0xEC]
       lea      r8d, [r10+0x09]
       mov      edx, dword ptr [r13+0x44]
       dec      edx
       js       G_M000_IG138
       test     r8d, r8d
       jl       SHORT G_M000_IG16
       cmp      r8d, edx
       cmovg    r8d, edx
       jmp      SHORT G_M000_IG17

G_M000_IG16:                ;; offset=0x0234
       xor      r8d, r8d

G_M000_IG17:                ;; offset=0x0237
       mov      edx, eax
       mov      r9d, dword ptr [rbp+0xF4]
       mov      r11, gword ptr [rbp+0x50]

G_M000_IG18:                ;; offset=0x0244
       jmp      G_M000_IG39

G_M000_IG19:                ;; offset=0x0249
       mov      rcx, gword ptr [rbp+0x60]
       cmp      dword ptr [rcx+0x20], 0
       je       G_M000_IG41
       jmp      G_M000_IG133

G_M000_IG20:                ;; offset=0x025C
       xor      eax, eax
       mov      qword ptr [rbp+0x70], r9

G_M000_IG21:                ;; offset=0x0262
       imul     eax, dword ptr [r15+0x20]
       lea      edx, [rax+rbx]
       mov      rcx, r15
       mov      r8d, dword ptr [rbp+0xC4]
       call     [DotJpegli.Internal.PixelSource:ReadSample(int,int):float:this]
       mov      rdx, qword ptr [rbp+0x70]
       vmovss   dword ptr [rdi+4*rdx], xmm0
       inc      edx
       mov      r8d, dword ptr [rbp+0xB8]
       cmp      edx, r8d
       mov      r9, rdx
       mov      ecx, dword ptr [rbp+0xC4]
       jge      G_M000_IG35

G_M000_IG22:                ;; offset=0x029E
       mov      r8d, dword ptr [rbp+0xCC]

G_M000_IG23:                ;; offset=0x02A5
       lea      eax, [r8-0x01]
       test     eax, eax
       jl       G_M000_IG140
       mov      r8d, dword ptr [rbp+0xCC]
       test     r9d, r9d
       jl       SHORT G_M000_IG20

G_M000_IG24:                ;; offset=0x02BD
       cmp      r9d, eax
       jg       G_M000_IG57
       mov      qword ptr [rbp+0x70], r9
       mov      eax, r9d
       jmp      SHORT G_M000_IG21

G_M000_IG25:                ;; offset=0x02CF
       xor      eax, eax
       mov      dword ptr [rsp+0x20], eax
       mov      dword ptr [rsp+0x28], esi
       mov      rbx, bword ptr [rbp+0x28]
       mov      bword ptr [rbp+0xA0], rbx
       mov      dword ptr [rbp+0xA8], r9d
       lea      r9, [rbp+0xA0]
       mov      qword ptr [rsp+0x30], r9
       mov      edx, r8d
       mov      r8d, dword ptr [rbp+0xC8]
       mov      r9d, ecx
       mov      rcx, r15
       call     [DotJpegli.Internal.PixelSource:ReadConvertedRow(int,int,int,int,int,System.Span`1[float]):this]
       jmp      G_M000_IG35

G_M000_IG26:                ;; offset=0x0312
       mov      r9d, dword ptr [r13+0x40]
       mov      r8d, r9d
       mov      ecx, r11d
       mov      eax, dword ptr [r15+0x1C]
       test     eax, eax
       jne      SHORT G_M000_IG29
       cmp      byte  ptr [r15+0x26], 0
       je       SHORT G_M000_IG27
       mov      r11d, 4
       jmp      SHORT G_M000_IG30

G_M000_IG27:                ;; offset=0x0333
       cmp      dword ptr [r15+0x20], 1
       jne      SHORT G_M000_IG28
       mov      r11d, 1
       jmp      SHORT G_M000_IG30

G_M000_IG28:                ;; offset=0x0342
       mov      r11d, 2
       jmp      SHORT G_M000_IG30

G_M000_IG29:                ;; offset=0x034A
       mov      r11d, eax

G_M000_IG30:                ;; offset=0x034D
       cmp      r11d, 3
       jne      SHORT G_M000_IG31
       xor      eax, eax
       jmp      SHORT G_M000_IG32

G_M000_IG31:                ;; offset=0x0357
       cmp      r11d, 4
       setne    al
       movzx    rax, al

G_M000_IG32:                ;; offset=0x0361
       test     eax, eax
       sete     al
       movzx    rax, al
       mov      r11, gword ptr [r13+0x10]
       test     r11, r11
       je       SHORT G_M000_IG33
       mov      ebx, r14d
       mov      edi, r9d
       add      rbx, rdi
       mov      edi, dword ptr [r11+0x08]
       cmp      rbx, rdi
       ja       G_M000_IG136
       mov      ebx, r14d
       lea      r11, bword ptr [r11+4*rbx+0x10]
       jmp      SHORT G_M000_IG34

G_M000_IG33:                ;; offset=0x0392
       mov      r11d, r14d
       or       r11d, r9d
       jne      G_M000_IG136
       xor      r11, r11
       xor      r9d, r9d

G_M000_IG34:                ;; offset=0x03A4
       mov      dword ptr [rsp+0x20], eax
       xor      eax, eax
       mov      dword ptr [rsp+0x28], eax
       mov      dword ptr [rsp+0x30], esi
       mov      bword ptr [rbp+0xA0], r11
       mov      dword ptr [rbp+0xA8], r9d
       lea      r9, [rbp+0xA0]
       mov      qword ptr [rsp+0x38], r9
       mov      edx, r8d
       mov      r8d, ecx
       mov      r9d, dword ptr [rbp+0x6C]
       mov      rcx, r15
       call     [DotJpegli.Internal.PixelSource:ReadRowVector(int,int,int,int,int,int,System.Span`1[float]):this]

G_M000_IG35:                ;; offset=0x03DF
       mov      rdx, gword ptr [r13+0x10]
       mov      rcx, rdx
       mov      rax, rdx
       mov      r8d, dword ptr [rax+0x08]
       cmp      r14d, r8d
       jae      G_M000_IG143
       mov      r10d, r14d
       vmovss   xmm2, dword ptr [rax+4*r10+0x10]
       lea      eax, [r14-0x01]
       cmp      eax, r8d
       jae      G_M000_IG143
       lea      eax, [r14-0x01]
       vmovss   dword ptr [rcx+4*rax+0x10], xmm2
       mov      rcx, rdx
       mov      eax, dword ptr [r13+0x40]
       add      r14d, eax
       mov      r10d, r14d
       mov      r9d, dword ptr [r13+0x4C]
       sub      r9d, eax
       inc      r9d
       test     rcx, rcx
       je       SHORT G_M000_IG36
       mov      eax, r10d
       mov      r11d, r9d
       add      rax, r11
       mov      r11d, r8d
       cmp      rax, r11
       ja       G_M000_IG136
       lea      rcx, bword ptr [rcx+4*r10+0x10]
       jmp      SHORT G_M000_IG37

G_M000_IG36:                ;; offset=0x044F
       or       r10d, r9d
       jne      G_M000_IG136
       xor      rcx, rcx
       xor      r9d, r9d

G_M000_IG37:                ;; offset=0x045D
       dec      r14d
       cmp      r14d, r8d
       jae      G_M000_IG143
       mov      eax, r14d
       vmovss   xmm2, dword ptr [rdx+4*rax+0x10]
       mov      edx, r9d
       call     [System.SpanHelpers:Fill[float](byref,nuint,float)]
       movzx    rbx, byte  ptr [rbp+0x200]

G_M000_IG38:                ;; offset=0x0482
       mov      r8d, esi
       mov      esi, dword ptr [rbp+0xE4]
       inc      esi
       mov      edx, esi
       mov      r9d, dword ptr [rbp+0xF4]
       mov      r11, gword ptr [rbp+0x50]
       mov      r14d, dword ptr [rbp+0x6C]

G_M000_IG39:                ;; offset=0x049E
       lea      ecx, [r9+0x0A]
       cmp      edx, ecx
       jge      G_M000_IG63

G_M000_IG40:                ;; offset=0x04AA
       cmp      gword ptr [rbp+0x60], 0
       jne      G_M000_IG19

G_M000_IG41:                ;; offset=0x04B5
       mov      ecx, edx
       add      ecx, dword ptr [rbp+0xEC]
       mov      r11d, dword ptr [r13+0x44]
       lea      eax, [r11-0x01]
       mov      esi, eax
       test     esi, esi
       jl       G_M000_IG139
       test     ecx, ecx
       jl       SHORT G_M000_IG44

G_M000_IG42:                ;; offset=0x04D3
       cmp      ecx, esi
       jg       SHORT G_M000_IG43
       mov      esi, ecx
       jmp      SHORT G_M000_IG45

G_M000_IG43:                ;; offset=0x04DB
       jmp      SHORT G_M000_IG45

G_M000_IG44:                ;; offset=0x04DD
       xor      esi, esi

G_M000_IG45:                ;; offset=0x04DF
       mov      ecx, dword ptr [r13+0x54]
       mov      dword ptr [rbp+0xE4], edx
       mov      r9d, edx
       imul     r9d, ecx
       lea      r14d, [r9+0x01]
       cmp      esi, r8d
       je       G_M000_IG58

G_M000_IG46:                ;; offset=0x04FD
       mov      dword ptr [rbp+0x200], ebx
       movzx    r8, bl
       test     r8d, r8d
       jne      G_M000_IG26
       mov      r9d, dword ptr [r13+0x40]
       mov      r8d, r9d
       mov      dword ptr [rbp+0xCC], r8d
       mov      dword ptr [rbp+0xC8], r11d
       mov      ecx, dword ptr [r15+0x1C]
       test     ecx, ecx
       jne      SHORT G_M000_IG49
       cmp      byte  ptr [r15+0x26], 0
       je       SHORT G_M000_IG47
       mov      ecx, 4
       jmp      SHORT G_M000_IG49

G_M000_IG47:                ;; offset=0x053B
       cmp      dword ptr [r15+0x20], 1
       jne      SHORT G_M000_IG48
       mov      ecx, 1
       jmp      SHORT G_M000_IG49

G_M000_IG48:                ;; offset=0x0549
       mov      ecx, 2

G_M000_IG49:                ;; offset=0x054E
       cmp      ecx, 3
       jne      SHORT G_M000_IG50
       xor      ecx, ecx
       jmp      SHORT G_M000_IG51

G_M000_IG50:                ;; offset=0x0557
       cmp      ecx, 4
       setne    cl
       movzx    rcx, cl

G_M000_IG51:                ;; offset=0x0560
       test     ecx, ecx
       sete     cl
       movzx    rcx, cl
       mov      dword ptr [rbp+0xC4], ecx
       mov      rbx, gword ptr [r13+0x10]
       test     rbx, rbx
       je       SHORT G_M000_IG52
       mov      edi, r14d
       mov      r11d, r9d
       add      r11, rdi
       mov      edi, dword ptr [rbx+0x08]
       cmp      r11, rdi
       ja       G_M000_IG136
       mov      r11d, r14d
       lea      rbx, bword ptr [rbx+4*r11+0x10]
       jmp      SHORT G_M000_IG53

G_M000_IG52:                ;; offset=0x0596
       or       r9d, r14d
       jne      G_M000_IG136
       xor      rbx, rbx
       xor      r9d, r9d

G_M000_IG53:                ;; offset=0x05A4
       mov      bword ptr [rbp+0x28], rbx
       mov      rdi, rbx
       mov      r11d, r9d
       mov      dword ptr [rbp+0xB8], r11d
       mov      rbx, gword ptr [r15+0x08]
       test     rbx, rbx
       je       SHORT G_M000_IG54
       cmp      byte  ptr [rbx+0x2C], 0
       je       G_M000_IG25
       test     ecx, ecx
       mov      ecx, dword ptr [rbp+0xC4]
       je       G_M000_IG25

G_M000_IG54:                ;; offset=0x05D6
       cmp      esi, eax
       jg       SHORT G_M000_IG55
       mov      ebx, esi
       jmp      SHORT G_M000_IG56

G_M000_IG55:                ;; offset=0x05DE
       mov      ebx, eax

G_M000_IG56:                ;; offset=0x05E0
       mov      eax, dword ptr [rbp+0x6C]
       imul     ebx, eax
       xor      r9d, r9d
       test     r11d, r11d
       je       G_M000_IG35
       jmp      G_M000_IG23

G_M000_IG57:                ;; offset=0x05F7
       mov      qword ptr [rbp+0x70], r9
       jmp      G_M000_IG21

G_M000_IG58:                ;; offset=0x0600
       mov      r8, gword ptr [r13+0x10]
       mov      edx, dword ptr [rbp+0xE4]
       lea      eax, [rdx-0x01]
       imul     eax, ecx
       mov      r11d, ecx
       test     r8, r8
       je       SHORT G_M000_IG59
       mov      dword ptr [rbp+0x200], ebx
       mov      r14d, eax
       mov      ebx, r11d
       add      rbx, r14
       mov      r14d, dword ptr [r8+0x08]
       cmp      rbx, r14
       ja       G_M000_IG136
       lea      rax, bword ptr [r8+4*rax+0x10]
       movzx    rbx, byte  ptr [rbp+0x200]
       jmp      SHORT G_M000_IG60

G_M000_IG59:                ;; offset=0x0642
       or       eax, r11d
       jne      G_M000_IG136
       xor      rax, rax
       xor      r11d, r11d

G_M000_IG60:                ;; offset=0x0650
       mov      r8, gword ptr [r13+0x10]
       test     r8, r8
       je       SHORT G_M000_IG61
       mov      dword ptr [rbp+0x200], ebx
       mov      r14d, r9d
       mov      ebx, ecx
       add      rbx, r14
       mov      r14d, dword ptr [r8+0x08]
       cmp      rbx, r14
       ja       G_M000_IG136
       mov      r9d, r9d
       lea      r9, bword ptr [r8+4*r9+0x10]
       movzx    rbx, byte  ptr [rbp+0x200]
       jmp      SHORT G_M000_IG62

G_M000_IG61:                ;; offset=0x0685
       or       r9d, ecx
       jne      G_M000_IG136
       xor      r9, r9
       xor      ecx, ecx

G_M000_IG62:                ;; offset=0x0693
       cmp      r11d, ecx
       jg       G_M000_IG137
       mov      r8d, r11d
       shl      r8, 2
       mov      rcx, r9
       mov      rdx, rax
       call     [System.SpanHelpers:Memmove(byref,byref,nuint)]
       jmp      G_M000_IG38

G_M000_IG63:                ;; offset=0x06B4
       mov      edi, dword ptr [rbp+0x68]
       mov      r15d, edi
       shr      r15d, 2
       dec      r15d
       mov      r9d, dword ptr [r13+0x50]
       mov      edx, r9d
       sar      edx, 31
       and      edx, 3
       add      r9d, edx
       sar      r9d, 2
       mov      dword ptr [rbp+0xE8], r9d
       xor      edx, edx
       cmp      dword ptr [rbp+0xF0], 0
       jne      SHORT G_M000_IG65

G_M000_IG64:                ;; offset=0x06E7
       mov      ecx, -1
       jmp      G_M000_IG72

G_M000_IG65:                ;; offset=0x06F1
       mov      r8, gword ptr [r13+0x18]
       mov      gword ptr [rbp+0x08], r8
       mov      rcx, r8
       mov      edx, dword ptr [r13+0x48]
       add      edx, edx
       mov      r8d, dword ptr [r13+0x58]
       imul     edx, r8d
       add      r8d, r8d
       test     rcx, rcx
       je       SHORT G_M000_IG66
       mov      dword ptr [rbp+0x200], ebx
       mov      ebx, edx
       mov      qword ptr [rbp], rbx
       mov      ebx, r8d
       add      rbx, qword ptr [rbp]
       mov      qword ptr [rbp], rbx
       mov      ebx, dword ptr [rcx+0x08]
       cmp      qword ptr [rbp], rbx
       ja       G_M000_IG136
       lea      rdx, bword ptr [rcx+4*rdx+0x10]
       mov      bword ptr [rbp+0x20], rdx
       movzx    rbx, byte  ptr [rbp+0x200]
       jmp      SHORT G_M000_IG67

G_M000_IG66:                ;; offset=0x0748
       or       edx, r8d
       jne      G_M000_IG136
       xor      rdx, rdx
       mov      bword ptr [rbp+0x20], rdx
       xor      r8d, r8d

G_M000_IG67:                ;; offset=0x075A
       mov      rcx, gword ptr [rbp+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG68
       lea      rdx, bword ptr [rcx+0x10]
       mov      ecx, dword ptr [rcx+0x08]
       mov      eax, ecx
       mov      rcx, rdx
       mov      edx, eax
       jmp      SHORT G_M000_IG69

G_M000_IG68:                ;; offset=0x0773
       xor      rcx, rcx
       xor      edx, edx

G_M000_IG69:                ;; offset=0x0777
       cmp      r8d, edx
       jg       G_M000_IG137
       mov      r8d, r8d
       shl      r8, 2
       mov      rdx, bword ptr [rbp+0x20]
       call     [System.SpanHelpers:Memmove(byref,byref,nuint)]
       mov      eax, 2
       lea      ecx, [r15+0x01]
       mov      r10d, dword ptr [rbp+0xE8]
       lea      edx, [r10-0x01]
       test     edx, edx
       jl       G_M000_IG141
       test     ecx, ecx
       jl       SHORT G_M000_IG70
       cmp      ecx, edx
       cmovg    ecx, edx
       jmp      SHORT G_M000_IG71

G_M000_IG70:                ;; offset=0x07B8
       xor      ecx, ecx

G_M000_IG71:                ;; offset=0x07BA
       shl      ecx, 2
       mov      edx, eax
       mov      r11, gword ptr [rbp+0x50]

G_M000_IG72:                ;; offset=0x07C3
       mov      r8d, edx
       jmp      G_M000_IG86

G_M000_IG73:                ;; offset=0x07CB
       cmp      gword ptr [rbp+0x60], 0
       jne      G_M000_IG128

G_M000_IG74:                ;; offset=0x07D6
       lea      edx, [r15+r8]
       mov      r9d, dword ptr [rbp+0xE8]
       dec      r9d
       js       G_M000_IG142
       test     edx, edx
       jl       SHORT G_M000_IG77

G_M000_IG75:                ;; offset=0x07EE
       cmp      edx, r9d
       jg       SHORT G_M000_IG76
       jmp      SHORT G_M000_IG78

G_M000_IG76:                ;; offset=0x07F5
       mov      edx, r9d
       jmp      SHORT G_M000_IG78

G_M000_IG77:                ;; offset=0x07FA
       xor      edx, edx

G_M000_IG78:                ;; offset=0x07FC
       lea      r9d, [4*rdx]
       mov      dword ptr [rbp+0xDC], r9d
       cmp      r9d, ecx
       je       SHORT G_M000_IG80

G_M000_IG79:                ;; offset=0x0810
       mov      edx, r9d
       sub      edx, dword ptr [rbp+0xEC]
       mov      dword ptr [rbp+0x200], ebx
       movzx    rcx, bl
       mov      r9d, ecx
       mov      rcx, r13
       mov      dword ptr [rbp+0xE0], r8d
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:ComputePreErosion(int,int,bool):this]
       jmp      G_M000_IG85

G_M000_IG80:                ;; offset=0x083A
       mov      dword ptr [rbp+0x200], ebx
       mov      rcx, gword ptr [r13+0x18]
       mov      rdx, rcx
       lea      ebx, [r8-0x01]
       mov      edi, dword ptr [r13+0x58]
       imul     ebx, edi
       mov      eax, edi
       test     rdx, rdx
       je       SHORT G_M000_IG81
       mov      esi, ebx
       mov      r12d, eax
       add      rsi, r12
       mov      r12d, dword ptr [rdx+0x08]
       cmp      rsi, r12
       ja       G_M000_IG136
       lea      rdx, bword ptr [rdx+4*rbx+0x10]
       mov      r12, bword ptr [rbp+0x18]
       jmp      SHORT G_M000_IG82

G_M000_IG81:                ;; offset=0x0879
       or       ebx, eax
       jne      G_M000_IG136
       xor      rdx, rdx
       xor      eax, eax

G_M000_IG82:                ;; offset=0x0885
       mov      dword ptr [rbp+0xE0], r8d
       mov      ebx, r8d
       imul     ebx, edi
       test     rcx, rcx
       je       SHORT G_M000_IG83
       mov      esi, ebx
       mov      r12d, edi
       add      rsi, r12
       mov      r12d, dword ptr [rcx+0x08]
       cmp      rsi, r12
       ja       G_M000_IG136
       lea      rcx, bword ptr [rcx+4*rbx+0x10]
       mov      r12, bword ptr [rbp+0x18]
       jmp      SHORT G_M000_IG84

G_M000_IG83:                ;; offset=0x08B7
       or       ebx, edi
       jne      G_M000_IG136
       xor      rcx, rcx
       xor      edi, edi

G_M000_IG84:                ;; offset=0x08C3
       cmp      eax, edi
       jg       G_M000_IG137
       mov      eax, eax
       lea      r8, [4*rax]
       call     [System.SpanHelpers:Memmove(byref,byref,nuint)]

G_M000_IG85:                ;; offset=0x08DB
       mov      ecx, dword ptr [rbp+0xDC]
       mov      r8d, dword ptr [rbp+0xE0]
       inc      r8d
       movzx    rbx, byte  ptr [rbp+0x200]
       mov      r11, gword ptr [rbp+0x50]

G_M000_IG86:                ;; offset=0x08F6
       mov      edx, dword ptr [r13+0x48]
       lea      edx, [2*rdx+0x02]
       cmp      r8d, edx
       jl       G_M000_IG73

G_M000_IG87:                ;; offset=0x090A
       mov      esi, dword ptr [rbp+0x1F0]
       cmp      esi, 9
       jge      SHORT G_M000_IG89

G_M000_IG88:                ;; offset=0x0915
       vmovss   xmm6, dword ptr [reloc @RWD00]
       vmovaps  xmm0, xmm6
       jmp      SHORT G_M000_IG90

G_M000_IG89:                ;; offset=0x0923
       vxorps   xmm0, xmm0, xmm0
       vcvtsi2ss xmm0, xmm0, esi
       vsubss   xmm0, xmm0, dword ptr [reloc @RWD04]
       vdivss   xmm0, xmm0, dword ptr [reloc @RWD08]
       vmovss   xmm6, dword ptr [reloc @RWD00]
       vsubss   xmm0, xmm6, xmm0
       vxorps   xmm1, xmm1, xmm1
       vcmpeqps xmm1, xmm1, xmm0
       vxorps   xmm2, xmm2, xmm2
       vpcmpgtd xmm2, xmm2, xmm0
       vandps   xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpltps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandnps  xmm0, xmm1, xmm0

G_M000_IG90:                ;; offset=0x096D
       vmulss   xmm7, xmm0, dword ptr [reloc @RWD12]
       vsubss   xmm0, xmm6, xmm0
       vmulss   xmm8, xmm0, dword ptr [reloc @RWD16]
       mov      dword ptr [rbp+0x200], ebx
       movzx    rcx, bl
       mov      esi, ecx
       test     esi, esi
       je       SHORT G_M000_IG92

G_M000_IG91:                ;; offset=0x0990
       cmp      dword ptr [r13+0x64], 8
       jge      SHORT G_M000_IG93

G_M000_IG92:                ;; offset=0x0997
       xor      r15d, r15d
       jmp      SHORT G_M000_IG94

G_M000_IG93:                ;; offset=0x099C
       movzx    r15, byte  ptr [rbp+0x208]
       or       r15d, 1

G_M000_IG94:                ;; offset=0x09A8
       test     r15d, r15d
       jne      SHORT G_M000_IG96

G_M000_IG95:                ;; offset=0x09AD
       xor      rax, rax
       xor      r8d, r8d
       mov      r10d, r8d
       jmp      SHORT G_M000_IG97

G_M000_IG96:                ;; offset=0x09B7
       test     dword ptr [rsp], esp
       sub      rsp, 512
       lea      rcx, [rsp+0x40]
       mov      qword ptr [rbp+0x58], rcx
       mov      edx, 512
       call     CORINFO_HELP_MEMZERO
       mov      rcx, qword ptr [rbp+0x58]
       mov      rax, rcx
       mov      r10d, 16

G_M000_IG97:                ;; offset=0x09E1
       mov      bword ptr [rbp+0x48], rax
       mov      dword ptr [rbp+0xC0], r10d
       test     r15d, r15d
       jne      SHORT G_M000_IG99

G_M000_IG98:                ;; offset=0x09F1
       xor      r11, r11
       xor      r8d, r8d
       jmp      SHORT G_M000_IG100

G_M000_IG99:                ;; offset=0x09F9
       test     dword ptr [rsp], esp
       sub      rsp, 64
       lea      r11, [rsp+0x40]
       vxorps   ymm0, ymm0, ymm0
       vmovdqu  ymmword ptr [r11], ymm0
       vmovdqu  ymmword ptr [r11+0x20], ymm0
       mov      r8d, 16

G_M000_IG100:                ;; offset=0x0A1A
       mov      bword ptr [rbp+0x40], r11
       mov      dword ptr [rbp+0xBC], r8d
       xor      ecx, ecx
       vmovss   xmm9, dword ptr [reloc @RWD20]
       vmovss   xmm10, dword ptr [reloc @RWD24]
       vmovss   xmm11, dword ptr [reloc @RWD28]
       jmp      G_M000_IG114

G_M000_IG101:                ;; offset=0x0A44
       mov      r10, bword ptr [rbp+0x48]
       mov      bword ptr [rbp+0x90], r10
       mov      r11d, dword ptr [rbp+0xC0]
       mov      dword ptr [rbp+0x98], r11d
       mov      rcx, bword ptr [rbp+0x40]
       mov      bword ptr [rbp+0xA0], rcx
       mov      edx, dword ptr [rbp+0xBC]
       mov      dword ptr [rbp+0xA8], edx
       lea      r9, [rbp+0x90]
       lea      r8, [rbp+0xA0]
       mov      qword ptr [rsp+0x20], r8
       mov      r8d, dword ptr [rbp+0xD8]
       lea      r8d, [8*r8+0x05]
       mov      dword ptr [rbp+0xD4], eax
       lea      edx, [8*rax]
       mov      rcx, r13
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:ModulateEightBlocks(int,int,System.Span`1[System.Runtime.Intrinsics.Vector256`1[float]],System.Span`1[float]):this]
       xor      eax, eax
       cmp      dword ptr [rbp+0xBC], 8
       jl       G_M000_IG106

G_M000_IG102:                ;; offset=0x0ABB
       mov      dword ptr [rbp+0xD0], eax
       mov      r8d, eax
       add      r8d, dword ptr [rbp+0xD4]
       add      r8d, r8d
       mov      r10, gword ptr [r13+0x28]
       mov      r9, r10
       mov      r11d, r8d
       add      r11d, dword ptr [r13+0x58]
       mov      ebx, r11d
       mov      edi, dword ptr [r9+0x08]
       cmp      ebx, edi
       jae      G_M000_IG143
       vmovss   xmm0, dword ptr [r9+4*rbx+0x10]
       mov      r9, r10
       inc      r11d
       cmp      r11d, edi
       jae      G_M000_IG143
       vaddss   xmm0, xmm0, dword ptr [r9+4*r11+0x10]
       mov      r11, r10
       cmp      r8d, edi
       jae      G_M000_IG143
       mov      r9d, r8d
       vaddss   xmm0, xmm0, dword ptr [r11+4*r9+0x10]
       inc      r8d
       cmp      r8d, edi
       jae      G_M000_IG143
       vaddss   xmm12, xmm0, dword ptr [r10+4*r8+0x10]
       vmovaps  xmm0, xmm12
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeMask(float):float]
       vmovaps  xmm12, xmm0
       mov      ebx, dword ptr [rbp+0xD0]
       mov      eax, ebx
       mov      rdi, bword ptr [rbp+0x40]
       vaddss   xmm12, xmm12, dword ptr [rdi+4*rax]
       lea      eax, [rbx+0x08]
       cmp      eax, dword ptr [rbp+0xBC]
       jae      G_M000_IG143
       vmovss   xmm0, dword ptr [rdi+4*rax]
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:FastLog2(float):float]
       vmulss   xmm0, xmm9, xmm0
       vaddss   xmm12, xmm12, xmm0
       vmulss   xmm0, xmm12, xmm10
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:FastPow2(float):float]
       vmulss   xmm0, xmm0, xmm7
       vaddss   xmm13, xmm8, xmm0
       mov      eax, dword ptr [rbp+0xD8]
       mov      ecx, eax
       imul     ecx, dword ptr [r13+0x64]
       mov      edx, dword ptr [rbp+0xD4]
       add      ecx, edx
       add      ecx, ebx
       cmp      ecx, dword ptr [rbp+0xB4]

G_M000_IG103:                ;; offset=0x0BA3
       jae      G_M000_IG143
       vdivss   xmm0, xmm11, xmm13
       vsubss   xmm0, xmm0, xmm6
       vxorps   xmm1, xmm1, xmm1
       vcmpeqps xmm1, xmm1, xmm0
       vxorps   xmm2, xmm2, xmm2
       vpcmpgtd xmm2, xmm2, xmm0
       vandps   xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpltps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandnps  xmm0, xmm1, xmm0
       vmovss   dword ptr [r12+4*rcx], xmm0
       inc      ebx
       cmp      ebx, 8
       jge      G_M000_IG109

G_M000_IG104:                ;; offset=0x0BE9
       mov      dword ptr [rbp+0xD4], edx
       mov      dword ptr [rbp+0xD8], eax
       mov      eax, ebx
       jmp      G_M000_IG102

G_M000_IG105:                ;; offset=0x0BFC
       mov      dword ptr [rbp+0xD4], edx
       mov      dword ptr [rbp+0xD8], eax
       mov      eax, ebx

G_M000_IG106:                ;; offset=0x0C0A
       mov      dword ptr [rbp+0xD0], eax
       mov      r10d, eax
       add      r10d, dword ptr [rbp+0xD4]
       add      r10d, r10d
       mov      r9, gword ptr [r13+0x28]
       mov      r11, r9
       mov      ebx, r10d
       add      ebx, dword ptr [r13+0x58]
       mov      dword ptr [rbp+0x7C], ebx
       mov      edi, dword ptr [r11+0x08]
       cmp      ebx, edi
       jae      G_M000_IG143
       vmovss   xmm0, dword ptr [r11+4*rbx+0x10]
       mov      r11, r9
       mov      ebx, dword ptr [rbp+0x7C]
       inc      ebx
       cmp      ebx, edi
       jae      G_M000_IG143
       vaddss   xmm0, xmm0, dword ptr [r11+4*rbx+0x10]
       mov      r11, r9
       cmp      r10d, edi
       jae      G_M000_IG143
       mov      ebx, r10d
       vaddss   xmm0, xmm0, dword ptr [r11+4*rbx+0x10]
       lea      r11d, [r10+0x01]
       cmp      r11d, edi
       jae      G_M000_IG143
       inc      r10d
       vaddss   xmm12, xmm0, dword ptr [r9+4*r10+0x10]
       vmovaps  xmm0, xmm12
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeMask(float):float]
       vmovaps  xmm12, xmm0
       mov      ebx, dword ptr [rbp+0xD0]
       mov      edi, dword ptr [rbp+0xBC]
       cmp      ebx, edi
       jae      G_M000_IG143
       mov      eax, ebx
       mov      rcx, bword ptr [rbp+0x40]
       vaddss   xmm12, xmm12, dword ptr [rcx+4*rax]
       lea      eax, [rbx+0x08]
       cmp      eax, edi
       jae      G_M000_IG143
       lea      eax, [rbx+0x08]
       vmovss   xmm0, dword ptr [rcx+4*rax]
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:FastLog2(float):float]
       vmulss   xmm0, xmm9, xmm0
       vaddss   xmm12, xmm12, xmm0
       vmulss   xmm0, xmm12, xmm10
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:FastPow2(float):float]
       vmulss   xmm0, xmm0, xmm7

G_M000_IG107:                ;; offset=0x0CE3
       vaddss   xmm13, xmm8, xmm0
       mov      eax, dword ptr [rbp+0xD8]
       mov      ecx, eax
       imul     ecx, dword ptr [r13+0x64]
       mov      edx, dword ptr [rbp+0xD4]
       add      ecx, edx
       add      ecx, ebx
       cmp      ecx, dword ptr [rbp+0xB4]
       jae      G_M000_IG143
       vdivss   xmm0, xmm11, xmm13
       vsubss   xmm0, xmm0, xmm6
       vxorps   xmm1, xmm1, xmm1
       vcmpeqps xmm1, xmm1, xmm0
       vxorps   xmm2, xmm2, xmm2
       vpcmpgtd xmm2, xmm2, xmm0
       vandps   xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpltps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandnps  xmm0, xmm1, xmm0
       vmovss   dword ptr [r12+4*rcx], xmm0
       inc      ebx
       cmp      ebx, 8
       jl       G_M000_IG105

G_M000_IG108:                ;; offset=0x0D4A
       mov      rdi, bword ptr [rbp+0x40]

G_M000_IG109:                ;; offset=0x0D4E
       add      edx, 8
       mov      dword ptr [rbp+0xD8], eax
       mov      eax, edx

G_M000_IG110:                ;; offset=0x0D59
       mov      r9d, dword ptr [r13+0x64]
       add      r9d, -8
       cmp      eax, r9d
       jle      G_M000_IG101

G_M000_IG111:                ;; offset=0x0D6A
       cmp      eax, dword ptr [r13+0x64]
       jl       G_M000_IG122

G_M000_IG112:                ;; offset=0x0D74
       mov      eax, dword ptr [rbp+0xD8]
       mov      r8d, dword ptr [rbp+0xB4]

G_M000_IG113:                ;; offset=0x0D81
       inc      eax
       mov      ecx, eax

G_M000_IG114:                ;; offset=0x0D85
       cmp      ecx, dword ptr [r13+0x48]
       jge      G_M000_IG125

G_M000_IG115:                ;; offset=0x0D8F
       cmp      gword ptr [rbp+0x60], 0
       jne      G_M000_IG124

G_M000_IG116:                ;; offset=0x0D9A
       mov      dword ptr [rbp+0xD8], ecx
       lea      edx, [2*rcx+0x01]
       mov      r9d, esi
       mov      rcx, r13
       xor      r8d, r8d
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this]
       mov      eax, dword ptr [rbp+0xD8]
       lea      edx, [2*rax+0x02]
       mov      r8d, dword ptr [r13+0x58]
       mov      r9d, esi
       mov      rcx, r13
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this]
       xor      eax, eax
       test     r15d, r15d
       je       SHORT G_M000_IG111
       jmp      G_M000_IG110

G_M000_IG117:                ;; offset=0x0DDF
       mov      eax, dword ptr [rbp+0xD8]
       lea      ecx, [8*rax+0x05]
       imul     ecx, dword ptr [r13+0x54]
       mov      r8d, dword ptr [rbp+0xD4]
       lea      ecx, [rcx+8*r8+0x01]
       mov      rdx, gword ptr [r13+0x10]
       test     rdx, rdx
       je       SHORT G_M000_IG118
       cmp      dword ptr [rdx+0x08], ecx
       jb       G_M000_IG136
       mov      r10d, ecx
       lea      r10, bword ptr [rdx+4*r10+0x10]
       mov      edx, dword ptr [rdx+0x08]
       sub      edx, ecx
       jmp      SHORT G_M000_IG119

G_M000_IG118:                ;; offset=0x0E1E
       test     ecx, ecx
       jne      G_M000_IG136
       xor      r10, r10
       xor      edx, edx

G_M000_IG119:                ;; offset=0x0E2B
       mov      bword ptr [rbp+0x80], r10
       mov      dword ptr [rbp+0x88], edx
       lea      rcx, [rbp+0x80]
       mov      edx, dword ptr [r13+0x54]
       vmovaps  xmm2, xmm0
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:ModulateScalar(System.ReadOnlySpan`1[float],int,float):float]

G_M000_IG120:                ;; offset=0x0E4D
       vmulss   xmm0, xmm10, xmm0
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:FastPow2(float):float]
       vmulss   xmm0, xmm0, xmm7
       vaddss   xmm0, xmm8, xmm0
       mov      eax, dword ptr [rbp+0xD8]
       mov      ecx, eax
       imul     ecx, dword ptr [r13+0x64]
       mov      r10d, dword ptr [rbp+0xD4]
       add      ecx, r10d
       mov      r8d, dword ptr [rbp+0xB4]
       cmp      ecx, r8d
       jae      G_M000_IG143
       vdivss   xmm0, xmm11, xmm0
       vsubss   xmm0, xmm0, xmm6
       vxorps   xmm1, xmm1, xmm1
       vcmpeqps xmm1, xmm1, xmm0
       vxorps   xmm2, xmm2, xmm2
       vpcmpgtd xmm2, xmm2, xmm0
       vandps   xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpltps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandnps  xmm0, xmm1, xmm0
       vmovss   dword ptr [r12+4*rcx], xmm0
       inc      r10d
       cmp      r10d, dword ptr [r13+0x64]
       jge      G_M000_IG113

G_M000_IG121:                ;; offset=0x0EC7
       mov      dword ptr [rbp+0xD8], eax
       mov      eax, r10d

G_M000_IG122:                ;; offset=0x0ED0
       mov      dword ptr [rbp+0xD4], eax
       lea      ecx, [rax+rax]
       mov      rdx, gword ptr [r13+0x28]
       mov      r10d, ecx
       add      r10d, dword ptr [r13+0x58]
       cmp      r10d, dword ptr [rdx+0x08]
       jae      G_M000_IG143
       vmovss   xmm0, dword ptr [rdx+4*r10+0x10]
       mov      rdx, gword ptr [r13+0x28]
       mov      r10d, dword ptr [r13+0x58]
       lea      r10d, [r10+rcx+0x01]
       cmp      r10d, dword ptr [rdx+0x08]
       jae      G_M000_IG143
       vaddss   xmm0, xmm0, dword ptr [rdx+4*r10+0x10]
       mov      rdx, gword ptr [r13+0x28]
       cmp      ecx, dword ptr [rdx+0x08]
       jae      G_M000_IG143
       mov      r10d, ecx
       vaddss   xmm0, xmm0, dword ptr [rdx+4*r10+0x10]
       mov      rdx, gword ptr [r13+0x28]
       lea      r10d, [rcx+0x01]
       cmp      r10d, dword ptr [rdx+0x08]
       jae      G_M000_IG143
       inc      ecx
       vaddss   xmm0, xmm0, dword ptr [rdx+4*rcx+0x10]
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeMask(float):float]
       test     esi, esi
       je       G_M000_IG117

G_M000_IG123:                ;; offset=0x0F52
       mov      eax, dword ptr [rbp+0xD8]
       lea      r8d, [8*rax+0x05]
       mov      r10d, dword ptr [rbp+0xD4]
       lea      edx, [8*r10]
       mov      rcx, r13
       vmovaps  xmm3, xmm0
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:ModulateVector(int,int,float):float:this]
       jmp      G_M000_IG120

G_M000_IG124:                ;; offset=0x0F81
       mov      rdx, gword ptr [rbp+0x60]
       cmp      dword ptr [rdx+0x20], 0
       je       G_M000_IG116
       jmp      G_M000_IG133

G_M000_IG125:                ;; offset=0x0F94
       lea      rcx, bword ptr [r13+0x38]
       mov      rdx, gword ptr [rbp+0x50]
       call     CORINFO_HELP_ASSIGN_REF
       mov      edi, dword ptr [rbp+0x68]
       mov      dword ptr [r13+0x5C], edi
       mov      dword ptr [r13+0x60], r14d
       movzx    rbx, byte  ptr [rbp+0x200]
       mov      byte  ptr [r13+0x6A], bl
       mov      r9, 0x6911662D01EE
       cmp      qword ptr [rbp+0xF8], r9
       je       SHORT G_M000_IG126
       call     CORINFO_HELP_FAIL_FAST

G_M000_IG126:                ;; offset=0x0FCF
       nop

G_M000_IG127:                ;; offset=0x0FD0
       vmovaps  xmm6, xmmword ptr [rbp+0x170]
       vmovaps  xmm7, xmmword ptr [rbp+0x160]
       vmovaps  xmm8, xmmword ptr [rbp+0x150]
       vmovaps  xmm9, xmmword ptr [rbp+0x140]
       vmovaps  xmm10, xmmword ptr [rbp+0x130]
       vmovaps  xmm11, xmmword ptr [rbp+0x120]
       vmovaps  xmm12, xmmword ptr [rbp+0x110]
       vmovaps  xmm13, xmmword ptr [rbp+0x100]
       lea      rsp, [rbp+0x188]
       pop      rbx
       pop      rsi
       pop      rdi
       pop      r12
       pop      r13
       pop      r14
       pop      r15
       pop      rbp
       ret

G_M000_IG128:                ;; offset=0x1024
       mov      rdx, gword ptr [rbp+0x60]
       cmp      dword ptr [rdx+0x20], 0
       je       G_M000_IG74
       jmp      SHORT G_M000_IG133

G_M000_IG129:                ;; offset=0x1034
       mov      rax, gword ptr [rbp+0x60]
       cmp      dword ptr [rax+0x20], 0
       je       G_M000_IG11
       jmp      SHORT G_M000_IG133

G_M000_IG130:                ;; offset=0x1044
       mov      r8d, -1
       jmp      G_M000_IG18

G_M000_IG131:                ;; offset=0x104F
       mov      r11d, r8d
       mov      qword ptr [rbp], r11
       mov      r11d, ecx
       add      r11, qword ptr [rbp]
       mov      qword ptr [rbp], r11
       mov      r11d, dword ptr [rdx+0x08]
       cmp      qword ptr [rbp], r11
       ja       G_M000_IG136
       mov      r8d, r8d
       lea      rdx, bword ptr [rdx+4*r8+0x10]
       mov      bword ptr [rbp+0x38], rdx
       mov      r11, gword ptr [rbp+0x50]
       jmp      G_M000_IG13

G_M000_IG132:                ;; offset=0x1084
       mov      rcx, r13
       call     [System.ThrowHelper:ThrowObjectDisposedException(System.Object)]
       int3

G_M000_IG133:                ;; offset=0x108E
       lea      rcx, [rbp+0x60]
       call     [System.Threading.CancellationToken:ThrowOperationCanceledException():this]
       int3

G_M000_IG134:                ;; offset=0x1099
       mov      rcx, 0x7FF882C3DDB8
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      rcx, rbx
       mov      rdx, 0x231A58E62C8
       call     [System.ArgumentOutOfRangeException:.ctor(System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3

G_M000_IG135:                ;; offset=0x10C7
       mov      rcx, 0x7FF882B8F4B8
       call     CORINFO_HELP_NEWSFAST
       mov      r13, rax
       mov      rcx, r13
       mov      rdx, 0x231A58E6208
       mov      r8, 0x231A58E62A0
       call     [System.ArgumentException:.ctor(System.String,System.String):this]
       mov      rcx, r13
       call     CORINFO_HELP_THROW
       int3

G_M000_IG136:                ;; offset=0x10FF
       call     [System.ThrowHelper:ThrowArgumentOutOfRangeException()]
       int3

G_M000_IG137:                ;; offset=0x1106
       call     [System.ThrowHelper:ThrowArgumentException_DestinationTooShort()]
       int3

G_M000_IG138:                ;; offset=0x110D
       xor      ecx, ecx
       call     [System.Math:ThrowMinMaxException[int](int,int)]
       int3

G_M000_IG139:                ;; offset=0x1116
       mov      edx, esi
       xor      ecx, ecx
       call     [System.Math:ThrowMinMaxException[int](int,int)]
       int3

G_M000_IG140:                ;; offset=0x1121
       mov      edx, eax
       xor      ecx, ecx
       call     [System.Math:ThrowMinMaxException[int](int,int)]
       int3

G_M000_IG141:                ;; offset=0x112C
       xor      ecx, ecx
       call     [System.Math:ThrowMinMaxException[int](int,int)]
       int3

G_M000_IG142:                ;; offset=0x1135
       mov      edx, r9d
       xor      ecx, ecx
       call     [System.Math:ThrowMinMaxException[int](int,int)]
       int3

G_M000_IG143:                ;; offset=0x1141
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	3F800000h		;         1
RWD04  	dd	41100000h		;         9
RWD08  	dd	42600000h		;        56
RWD12  	dd	3F574BC7h		;     0.841
RWD16  	dd	3ECEAF25h		;   0.40368
RWD20  	dd	BDDC6A08h		; -0.107624
RWD24  	dd	3FB8AA3Bh		;    1.4427
RWD28  	dd	3F19999Ah		;       0.6

; Total bytes of code 4423

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ComputePreErosion(int,int,bool):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 46 single block inlinees; 21 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r15
       push     r14
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 264
       vmovaps  xmmword ptr [rsp+0xF0], xmm6
       vmovaps  xmmword ptr [rsp+0xE0], xmm7
       vmovaps  xmmword ptr [rsp+0xD0], xmm8
       vmovaps  xmmword ptr [rsp+0xC0], xmm9
       vmovaps  xmmword ptr [rsp+0xB0], xmm10
       vmovaps  xmmword ptr [rsp+0xA0], xmm11
       vmovaps  xmmword ptr [rsp+0x90], xmm12
       vmovaps  xmmword ptr [rsp+0x80], xmm13
       vmovaps  xmmword ptr [rsp+0x70], xmm14
       vmovaps  xmmword ptr [rsp+0x60], xmm15

G_M000_IG02:                ;; offset=0x0063
       vmovss   xmm0, dword ptr [reloc @RWD00]
       mov      eax, dword ptr [rcx+0x54]
       imul     edx, eax
       inc      edx
       xor      r10d, r10d
       test     r9b, r9b
       je       G_M000_IG10

G_M000_IG03:                ;; offset=0x007F
       mov      r9d, dword ptr [rcx+0x4C]
       add      r9d, -8
       js       G_M000_IG10
       align    [0 bytes for IG04]

G_M000_IG04:                ;; offset=0x008D
       lea      r9d, [rdx+r10]
       mov      r11, gword ptr [rcx+0x10]
       mov      rbx, r11
       mov      esi, r9d
       sub      esi, eax
       js       G_M000_IG20
       mov      edi, dword ptr [rbx+0x08]
       mov      ebp, edi
       sub      ebp, esi
       cmp      ebp, 8
       jl       G_M000_IG20
       cmp      esi, edi
       jae      G_M000_IG23
       vmovups  ymm1, ymmword ptr [rbx+4*rsi+0x10]
       mov      rbx, r11
       test     r9d, r9d
       jl       G_M000_IG20
       mov      esi, edi
       sub      esi, r9d
       cmp      esi, 8
       jl       G_M000_IG20
       cmp      r9d, edi
       jae      G_M000_IG23
       mov      esi, r9d
       vmovups  ymm2, ymmword ptr [rbx+4*rsi+0x10]
       mov      rbx, r11
       lea      esi, [r9+rax]
       mov      ebp, esi
       test     ebp, ebp
       jl       G_M000_IG20
       mov      r14d, edi
       sub      r14d, ebp
       cmp      r14d, 8
       jl       G_M000_IG20
       cmp      ebp, edi
       jae      G_M000_IG23
       vmovups  ymm3, ymmword ptr [rbx+4*rbp+0x10]
       mov      rbx, r11
       lea      ebp, [r9-0x01]
       test     ebp, ebp
       jl       G_M000_IG20
       mov      r14d, edi
       sub      r14d, ebp
       cmp      r14d, 8
       jl       G_M000_IG20
       cmp      ebp, edi
       jae      G_M000_IG23
       vmovups  ymm4, ymmword ptr [rbx+4*rbp+0x10]
       mov      rbx, r11
       lea      ebp, [r9+0x01]
       mov      r14d, edi
       sub      r14d, ebp
       cmp      r14d, 8

G_M000_IG05:                ;; offset=0x015A
       jl       G_M000_IG20
       cmp      ebp, edi
       jae      G_M000_IG23
       vaddps   ymm4, ymm4, ymmword ptr [rbx+4*rbp+0x10]
       vaddps   ymm1, ymm3, ymm1
       vaddps   ymm1, ymm1, ymm4
       vbroadcastss ymm4, dword ptr [reloc @RWD04]
       vmulps   ymm1, ymm1, ymm4
       vbroadcastss ymm5, dword ptr [reloc @RWD08]
       vaddps   ymm6, ymm5, ymm2
       vcmpneqps ymm7, ymm6, ymm6
       vxorps   ymm8, ymm8, ymm8
       vcmpgtps ymm8, ymm6, ymm8
       vorps    ymm7, ymm8, ymm7
       vandps   ymm6, ymm7, ymm6
       vmulps   ymm7, ymm6, ymm6
       vbroadcastss ymm8, dword ptr [reloc @RWD12]
       vmulps   ymm6, ymm8, ymm6
       vmulps   ymm6, ymm6, ymm7
       vbroadcastss ymm9, dword ptr [reloc @RWD16]
       vaddps   ymm6, ymm9, ymm6
       vbroadcastss ymm10, dword ptr [reloc @RWD20]
       vmulps   ymm7, ymm10, ymm7
       vbroadcastss ymm11, dword ptr [reloc @RWD24]
       vaddps   ymm7, ymm11, ymm7
       vdivps   ymm6, ymm6, ymm7
       vsubps   ymm1, ymm2, ymm1
       vmulps   ymm1, ymm1, ymm6
       vmulps   ymm1, ymm1, ymm1
       vbroadcastss ymm6, dword ptr [reloc @RWD28]
       vcmpeqps ymm7, ymm6, ymm1
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm1
       vandps   ymm7, ymm12, ymm7
       vcmpneqps ymm12, ymm1, ymm1
       vorps    ymm7, ymm12, ymm7
       vcmpltps ymm12, ymm1, ymm6
       vorps    ymm7, ymm12, ymm7
       vblendvps ymm1, ymm6, ymm1, ymm7
       vbroadcastss ymm7, dword ptr [reloc @RWD00]
       vmulps   ymm1, ymm7, ymm1
       vbroadcastss ymm12, dword ptr [reloc @RWD32]
       vaddps   ymm1, ymm12, ymm1
       vsqrtps  ymm1, ymm1
       vmulps   ymm1, ymm1, ymm4
       mov      rbx, r11
       lea      ebp, [r9+2*rax]
       mov      r14d, ebp
       test     r14d, r14d
       jl       G_M000_IG20
       mov      r15d, edi
       sub      r15d, r14d
       cmp      r15d, 8
       jl       G_M000_IG20
       cmp      r14d, edi

G_M000_IG06:                ;; offset=0x026F
       jae      G_M000_IG23
       vmovups  ymm13, ymmword ptr [rbx+4*r14+0x10]
       mov      rbx, r11
       lea      r14d, [rsi-0x01]
       test     r14d, r14d
       jl       G_M000_IG20
       mov      r15d, edi
       sub      r15d, r14d
       cmp      r15d, 8
       jl       G_M000_IG20
       cmp      r14d, edi
       jae      G_M000_IG23
       vmovups  ymm14, ymmword ptr [rbx+4*r14+0x10]
       mov      rbx, r11
       inc      esi
       js       G_M000_IG20
       mov      r14d, edi
       sub      r14d, esi
       cmp      r14d, 8
       jl       G_M000_IG20
       cmp      esi, edi
       jae      G_M000_IG23
       vaddps   ymm14, ymm14, ymmword ptr [rbx+4*rsi+0x10]
       vaddps   ymm2, ymm13, ymm2
       vaddps   ymm2, ymm14, ymm2
       vmulps   ymm2, ymm2, ymm4
       vmovups  ymmword ptr [rsp+0x40], ymm2
       vaddps   ymm14, ymm5, ymm3
       vcmpneqps ymm15, ymm14, ymm14
       vxorps   ymm2, ymm2, ymm2
       vcmpgtps ymm2, ymm14, ymm2
       vorps    ymm2, ymm15, ymm2
       vandps   ymm14, ymm14, ymm2
       vmulps   ymm2, ymm14, ymm14
       vmulps   ymm14, ymm8, ymm14
       vmulps   ymm14, ymm14, ymm2
       vaddps   ymm14, ymm14, ymm9
       vmulps   ymm2, ymm10, ymm2
       vaddps   ymm2, ymm11, ymm2
       vdivps   ymm2, ymm14, ymm2
       vsubps   ymm14, ymm3, ymmword ptr [rsp+0x40]
       vmulps   ymm2, ymm14, ymm2
       vmulps   ymm2, ymm2, ymm2
       vcmpeqps ymm14, ymm6, ymm2
       vxorps   ymm15, ymm15, ymm15
       vpcmpgtd ymm15, ymm15, ymm2
       vandps   ymm14, ymm15, ymm14
       vcmpneqps ymm15, ymm2, ymm2
       vorps    ymm14, ymm15, ymm14
       vcmpltps ymm15, ymm2, ymm6
       vorps    ymm14, ymm15, ymm14
       vblendvps ymm2, ymm6, ymm2, ymm14
       vmulps   ymm2, ymm7, ymm2

G_M000_IG07:                ;; offset=0x0360
       vaddps   ymm2, ymm12, ymm2
       vsqrtps  ymm2, ymm2
       vmulps   ymm2, ymm2, ymm4
       vaddps   ymm1, ymm2, ymm1
       vmovaps  ymm2, ymm3
       vmovaps  ymm3, ymm13
       mov      rbx, r11
       lea      esi, [rax+2*rax]
       add      esi, r9d
       mov      r14d, esi
       test     r14d, r14d
       jl       G_M000_IG20
       mov      r15d, edi
       sub      r15d, r14d
       cmp      r15d, 8
       jl       G_M000_IG20
       cmp      r14d, edi
       jae      G_M000_IG23
       vmovups  ymm13, ymmword ptr [rbx+4*r14+0x10]
       mov      rbx, r11
       lea      r14d, [rbp-0x01]
       test     r14d, r14d
       jl       G_M000_IG20
       mov      r15d, edi
       sub      r15d, r14d
       cmp      r15d, 8
       jl       G_M000_IG20
       cmp      r14d, edi
       jae      G_M000_IG23
       vmovups  ymm14, ymmword ptr [rbx+4*r14+0x10]
       mov      rbx, r11
       inc      ebp
       js       G_M000_IG20
       mov      r14d, edi
       sub      r14d, ebp
       cmp      r14d, 8
       jl       G_M000_IG20
       cmp      ebp, edi
       jae      G_M000_IG23
       vaddps   ymm14, ymm14, ymmword ptr [rbx+4*rbp+0x10]
       vaddps   ymm2, ymm13, ymm2
       vaddps   ymm2, ymm14, ymm2
       vmulps   ymm2, ymm2, ymm4
       vmovups  ymmword ptr [rsp+0x20], ymm2
       vaddps   ymm14, ymm5, ymm3
       vcmpneqps ymm15, ymm14, ymm14
       vxorps   ymm2, ymm2, ymm2
       vcmpgtps ymm2, ymm14, ymm2
       vorps    ymm2, ymm15, ymm2
       vandps   ymm14, ymm14, ymm2
       vmulps   ymm2, ymm14, ymm14
       vmulps   ymm14, ymm8, ymm14
       vmulps   ymm14, ymm14, ymm2
       vaddps   ymm14, ymm14, ymm9

G_M000_IG08:                ;; offset=0x0447
       vmulps   ymm2, ymm10, ymm2
       vaddps   ymm2, ymm11, ymm2
       vdivps   ymm2, ymm14, ymm2
       vsubps   ymm14, ymm3, ymmword ptr [rsp+0x20]
       vmulps   ymm2, ymm14, ymm2
       vmulps   ymm2, ymm2, ymm2
       vcmpeqps ymm14, ymm6, ymm2
       vxorps   ymm15, ymm15, ymm15
       vpcmpgtd ymm15, ymm15, ymm2
       vandps   ymm14, ymm15, ymm14
       vcmpneqps ymm15, ymm2, ymm2
       vorps    ymm14, ymm15, ymm14
       vcmpltps ymm15, ymm2, ymm6
       vorps    ymm14, ymm15, ymm14
       vblendvps ymm2, ymm6, ymm2, ymm14
       vmulps   ymm2, ymm7, ymm2
       vaddps   ymm2, ymm12, ymm2
       vsqrtps  ymm2, ymm2
       vmulps   ymm2, ymm2, ymm4
       vaddps   ymm1, ymm2, ymm1
       vmovaps  ymm2, ymm3
       vmovaps  ymm3, ymm13
       mov      rbx, r11
       lea      r9d, [r9+4*rax]
       test     r9d, r9d
       jl       G_M000_IG20
       mov      ebp, edi
       sub      ebp, r9d
       cmp      ebp, 8
       jl       G_M000_IG20
       cmp      r9d, edi
       jae      G_M000_IG23
       vmovups  ymm13, ymmword ptr [rbx+4*r9+0x10]
       mov      r9, r11
       lea      ebx, [rsi-0x01]
       test     ebx, ebx
       jl       G_M000_IG20
       mov      ebp, edi
       sub      ebp, ebx
       cmp      ebp, 8
       jl       G_M000_IG20
       cmp      ebx, edi
       jae      G_M000_IG23
       vmovups  ymm14, ymmword ptr [r9+4*rbx+0x10]
       inc      esi
       js       G_M000_IG20
       mov      r9d, edi
       sub      r9d, esi
       cmp      r9d, 8
       jl       G_M000_IG20
       cmp      esi, edi
       jae      G_M000_IG23
       mov      r9d, esi
       vaddps   ymm14, ymm14, ymmword ptr [r11+4*r9+0x10]

G_M000_IG09:                ;; offset=0x052D
       vaddps   ymm2, ymm13, ymm2
       vaddps   ymm2, ymm14, ymm2
       vmulps   ymm2, ymm2, ymm4
       vaddps   ymm5, ymm5, ymm3
       vcmpneqps ymm13, ymm5, ymm5
       vxorps   ymm14, ymm14, ymm14
       vcmpgtps ymm14, ymm5, ymm14
       vorps    ymm13, ymm14, ymm13
       vandps   ymm5, ymm13, ymm5
       vmulps   ymm13, ymm5, ymm5
       vmulps   ymm5, ymm8, ymm5
       vmulps   ymm5, ymm13, ymm5
       vaddps   ymm5, ymm9, ymm5
       vmulps   ymm8, ymm10, ymm13
       vaddps   ymm8, ymm8, ymm11
       vdivps   ymm5, ymm5, ymm8
       vsubps   ymm2, ymm3, ymm2
       vmulps   ymm2, ymm2, ymm5
       vmulps   ymm2, ymm2, ymm2
       vcmpeqps ymm3, ymm6, ymm2
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm2
       vandps   ymm3, ymm5, ymm3
       vcmpneqps ymm5, ymm2, ymm2
       vorps    ymm3, ymm5, ymm3
       vcmpltps ymm5, ymm2, ymm6
       vorps    ymm3, ymm5, ymm3
       vblendvps ymm2, ymm6, ymm2, ymm3
       vmulps   ymm2, ymm7, ymm2
       vaddps   ymm2, ymm12, ymm2
       vsqrtps  ymm2, ymm2
       vmulps   ymm2, ymm2, ymm4
       vaddps   ymm1, ymm2, ymm1
       mov      r9, gword ptr [rcx+0x20]
       mov      r11d, dword ptr [r9+0x08]
       cmp      r11d, r10d
       jbe      G_M000_IG21
       sub      r11d, r10d
       cmp      r11d, 8
       jl       G_M000_IG22
       mov      r11d, r10d
       vmovups  ymmword ptr [r9+4*r11+0x10], ymm1
       add      r10d, 8
       mov      r9d, dword ptr [rcx+0x4C]
       add      r9d, -8
       cmp      r10d, r9d
       jle      G_M000_IG04

G_M000_IG10:                ;; offset=0x05FB
       mov      eax, dword ptr [rcx+0x4C]
       vmovss   xmm1, dword ptr [reloc @RWD04]
       vmovss   xmm2, dword ptr [reloc @RWD08]
       vmovss   xmm3, dword ptr [reloc @RWD12]
       vmovss   xmm4, dword ptr [reloc @RWD16]
       vmovss   xmm5, dword ptr [reloc @RWD20]
       vmovss   xmm6, dword ptr [reloc @RWD24]
       vbroadcastss xmm7, dword ptr [reloc @RWD28]
       vmovss   xmm8, dword ptr [reloc @RWD32]
       jmp      G_M000_IG16
       align    [0 bytes for IG13]

G_M000_IG11:                ;; offset=0x0644
       lea      r9d, [rdx+r10]
       mov      r11, gword ptr [rcx+0x10]
       mov      rbx, r11
       mov      esi, dword ptr [rbx+0x08]
       cmp      r9d, esi
       jae      G_M000_IG23
       mov      edi, r9d
       vmovss   xmm9, dword ptr [rbx+4*rdi+0x10]
       mov      rbx, r11
       lea      edi, [r9-0x01]
       cmp      edi, esi
       jae      G_M000_IG23
       lea      edi, [r9-0x01]
       vmovss   xmm10, dword ptr [rbx+4*rdi+0x10]
       mov      rbx, r11
       lea      edi, [r9+0x01]
       cmp      edi, esi
       jae      G_M000_IG23
       lea      edi, [r9+0x01]
       vaddss   xmm10, xmm10, dword ptr [rbx+4*rdi+0x10]
       mov      rbx, r11
       mov      edi, dword ptr [rcx+0x54]
       mov      ebp, r9d
       sub      ebp, edi
       cmp      ebp, esi
       jae      G_M000_IG23
       vmovss   xmm11, dword ptr [rbx+4*rbp+0x10]
       mov      rbx, r11
       lea      ebp, [r9+rdi]
       cmp      ebp, esi
       jae      G_M000_IG23
       vaddss   xmm11, xmm11, dword ptr [rbx+4*rbp+0x10]
       vaddss   xmm10, xmm10, xmm11
       vmulss   xmm10, xmm10, xmm1
       vaddss   xmm11, xmm9, xmm2
       vxorps   xmm12, xmm12, xmm12
       vcmpneqps xmm13, xmm11, xmm11
       vorps    xmm12, xmm13, xmm12
       vxorps   xmm13, xmm13, xmm13
       vcmpgtps xmm13, xmm11, xmm13
       vorps    xmm12, xmm13, xmm12
       vandps   xmm11, xmm12, xmm11
       vmulss   xmm12, xmm11, xmm11
       vmulss   xmm11, xmm11, xmm3
       vmulss   xmm11, xmm11, xmm12
       vaddss   xmm11, xmm11, xmm4
       vmulss   xmm12, xmm12, xmm5
       vaddss   xmm12, xmm12, xmm6
       vdivss   xmm11, xmm11, xmm12
       vsubss   xmm9, xmm9, xmm10
       vmulss   xmm9, xmm11, xmm9
       vmulss   xmm9, xmm9, xmm9
       vminss   xmm9, xmm7, xmm9
       vmulss   xmm9, xmm9, dword ptr [reloc @RWD00]
       vaddss   xmm9, xmm9, xmm8
       vsqrtss  xmm9, xmm9, xmm9

G_M000_IG12:                ;; offset=0x073B
       vmulss   xmm9, xmm9, xmm1
       mov      ebx, 1

G_M000_IG13:                ;; offset=0x0744
       mov      ebp, ebx
       imul     ebp, dword ptr [rcx+0x54]
       add      ebp, r9d
       mov      r14, r11
       cmp      ebp, esi
       jae      G_M000_IG23
       mov      r15d, ebp
       vmovss   xmm10, dword ptr [r14+4*r15+0x10]
       mov      r14, r11
       lea      r15d, [rbp-0x01]
       cmp      r15d, esi
       jae      G_M000_IG23
       vmovss   xmm11, dword ptr [r14+4*r15+0x10]
       mov      r14, r11
       lea      r15d, [rbp+0x01]
       cmp      r15d, esi
       jae      G_M000_IG23
       vaddss   xmm11, xmm11, dword ptr [r14+4*r15+0x10]
       mov      r14, r11
       mov      r15d, ebp
       sub      r15d, edi
       cmp      r15d, esi
       jae      G_M000_IG23
       vmovss   xmm12, dword ptr [r14+4*r15+0x10]
       mov      r14, r11
       add      ebp, edi
       cmp      ebp, esi
       jae      G_M000_IG23
       vaddss   xmm12, xmm12, dword ptr [r14+4*rbp+0x10]
       vaddss   xmm11, xmm11, xmm12
       vmulss   xmm11, xmm11, xmm1
       vaddss   xmm12, xmm10, xmm2
       vxorps   xmm13, xmm13, xmm13
       vcmpneqps xmm14, xmm12, xmm12
       vorps    xmm13, xmm14, xmm13
       vxorps   xmm14, xmm14, xmm14
       vcmpgtps xmm14, xmm12, xmm14
       vorps    xmm13, xmm14, xmm13
       vandps   xmm12, xmm13, xmm12
       vmulss   xmm13, xmm12, xmm12
       vmulss   xmm12, xmm12, xmm3
       vmulss   xmm12, xmm12, xmm13
       vaddss   xmm12, xmm12, xmm4
       vmulss   xmm13, xmm13, xmm5
       vaddss   xmm13, xmm13, xmm6
       vdivss   xmm12, xmm12, xmm13
       vsubss   xmm10, xmm10, xmm11
       vmulss   xmm10, xmm12, xmm10
       vmulss   xmm10, xmm10, xmm10
       vminss   xmm10, xmm7, xmm10
       vmulss   xmm10, xmm10, xmm0
       vaddss   xmm10, xmm10, xmm8
       vsqrtss  xmm10, xmm10, xmm10
       vmulss   xmm10, xmm10, xmm1
       vaddss   xmm9, xmm10, xmm9
       inc      ebx
       cmp      ebx, 4

G_M000_IG14:                ;; offset=0x083E
       jl       G_M000_IG13

G_M000_IG15:                ;; offset=0x0844
       mov      r9, gword ptr [rcx+0x20]
       cmp      r10d, dword ptr [r9+0x08]
       jae      G_M000_IG23
       mov      r11d, r10d
       vmovss   dword ptr [r9+4*r11+0x10], xmm9
       inc      r10d

G_M000_IG16:                ;; offset=0x085F
       cmp      r10d, eax
       jl       G_M000_IG11

G_M000_IG17:                ;; offset=0x0868
       imul     r8d, dword ptr [rcx+0x58]
       xor      r10d, r10d
       mov      eax, dword ptr [rcx+0x58]
       test     eax, eax
       jle      G_M000_IG19
       align    [0 bytes for IG18]

G_M000_IG18:                ;; offset=0x087B
       lea      edx, [4*r10]
       mov      r9, gword ptr [rcx+0x18]
       mov      r11, gword ptr [rcx+0x20]
       mov      rbx, r11
       cmp      edx, dword ptr [rbx+0x08]
       jae      G_M000_IG23
       mov      esi, edx
       vmovss   xmm0, dword ptr [rbx+4*rsi+0x10]
       mov      rbx, r11
       lea      esi, [rdx+0x01]
       cmp      esi, dword ptr [rbx+0x08]
       jae      G_M000_IG23
       lea      esi, [rdx+0x01]
       vaddss   xmm0, xmm0, dword ptr [rbx+4*rsi+0x10]
       mov      rbx, r11
       lea      esi, [rdx+0x02]
       cmp      esi, dword ptr [rbx+0x08]
       jae      G_M000_IG23
       lea      esi, [rdx+0x02]
       vaddss   xmm0, xmm0, dword ptr [rbx+4*rsi+0x10]
       lea      ebx, [rdx+0x03]
       cmp      ebx, dword ptr [r11+0x08]
       jae      G_M000_IG23
       add      edx, 3
       vaddss   xmm0, xmm0, dword ptr [r11+4*rdx+0x10]
       vmulss   xmm0, xmm0, xmm1
       lea      edx, [r8+r10]
       cmp      edx, dword ptr [r9+0x08]
       jae      G_M000_IG23
       lea      edx, [r8+r10]
       vmovss   dword ptr [r9+4*rdx+0x10], xmm0
       inc      r10d
       cmp      r10d, eax
       jl       G_M000_IG18

G_M000_IG19:                ;; offset=0x090F
       vzeroupper
       vmovaps  xmm6, xmmword ptr [rsp+0xF0]
       vmovaps  xmm7, xmmword ptr [rsp+0xE0]
       vmovaps  xmm8, xmmword ptr [rsp+0xD0]
       vmovaps  xmm9, xmmword ptr [rsp+0xC0]
       vmovaps  xmm10, xmmword ptr [rsp+0xB0]
       vmovaps  xmm11, xmmword ptr [rsp+0xA0]
       vmovaps  xmm12, xmmword ptr [rsp+0x90]
       vmovaps  xmm13, xmmword ptr [rsp+0x80]
       vmovaps  xmm14, xmmword ptr [rsp+0x70]
       vmovaps  xmm15, xmmword ptr [rsp+0x60]
       add      rsp, 264
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r14
       pop      r15
       ret

G_M000_IG20:                ;; offset=0x0976
       call     [System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()]
       int3

G_M000_IG21:                ;; offset=0x097D
       call     [System.ThrowHelper:ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLess()]
       int3

G_M000_IG22:                ;; offset=0x0984
       call     [System.ThrowHelper:ThrowArgumentException_DestinationTooShort()]
       int3

G_M000_IG23:                ;; offset=0x098B
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	480E0640h		;    145433
RWD04  	dd	3E800000h		;      0.25
RWD08  	dd	409B0A3Dh		;     4.845
RWD12  	dd	3B1DEA78h		; 0.00240961
RWD16  	dd	449E380Dh		;   1265.75
RWD20  	dd	42EF0C23h		;   119.524
RWD24  	dd	44228FFEh		;    650.25
RWD28  	dd	3E4CCCCDh		;       0.2
RWD32  	dd	41E00000h		;        28

; Total bytes of code 2449

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 33 single block inlinees; 11 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r15
       push     r14
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 360
       vmovaps  xmmword ptr [rsp+0x150], xmm6
       vmovaps  xmmword ptr [rsp+0x140], xmm7
       vmovaps  xmmword ptr [rsp+0x130], xmm8
       vmovaps  xmmword ptr [rsp+0x120], xmm9
       vmovaps  xmmword ptr [rsp+0x110], xmm10
       vmovaps  xmmword ptr [rsp+0x100], xmm11
       vmovaps  xmmword ptr [rsp+0xF0], xmm12
       vmovaps  xmmword ptr [rsp+0xE0], xmm13
       vmovaps  xmmword ptr [rsp+0xD0], xmm14
       vmovaps  xmmword ptr [rsp+0xC0], xmm15
       mov      rbx, rcx
       mov      esi, edx
       mov      edi, r8d

G_M000_IG02:                ;; offset=0x0071
       xor      ebp, ebp
       test     r9b, r9b
       je       G_M000_IG23

G_M000_IG03:                ;; offset=0x007C
       cmp      dword ptr [rbx+0x58], 9
       jle      G_M000_IG23
       mov      ebp, 1
       mov      rcx, rbx
       mov      edx, esi
       mov      r8d, edi
       xor      r9d, r9d
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this]
       mov      ecx, dword ptr [rbx+0x58]
       lea      edx, [rcx-0x08]
       cmp      edx, 1
       jle      G_M000_IG23
       align    [0 bytes for IG04]

G_M000_IG04:                ;; offset=0x00AB
       lea      edx, [rsi-0x01]
       imul     edx, ecx
       add      edx, ebp
       mov      r8d, esi
       imul     r8d, ecx
       add      r8d, ebp
       lea      r9d, [rsi+0x01]
       imul     r9d, ecx
       add      r9d, ebp
       mov      rax, gword ptr [rbx+0x18]
       mov      r10, rax
       lea      r11d, [rdx-0x01]
       test     r11d, r11d
       jl       G_M000_IG26
       mov      r14d, dword ptr [r10+0x08]
       mov      r15d, r14d
       sub      r15d, r11d
       cmp      r15d, 8
       jl       G_M000_IG26
       cmp      r11d, r14d
       jae      G_M000_IG29
       vmovups  ymm0, ymmword ptr [r10+4*r11+0x10]
       mov      r10, rax
       test     edx, edx
       jl       G_M000_IG26
       mov      r11d, r14d
       sub      r11d, edx
       cmp      r11d, 8
       jl       G_M000_IG26
       cmp      edx, r14d
       jae      G_M000_IG29
       mov      r11d, edx
       vmovups  ymm1, ymmword ptr [r10+4*r11+0x10]
       mov      r10, rax
       inc      edx
       mov      r11d, r14d
       sub      r11d, edx
       cmp      r11d, 8
       jl       G_M000_IG26
       cmp      edx, r14d
       jae      G_M000_IG29
       vmovups  ymm2, ymmword ptr [r10+4*rdx+0x10]
       mov      rdx, rax
       lea      r10d, [r8-0x01]
       test     r10d, r10d
       jl       G_M000_IG26
       mov      r11d, r14d
       sub      r11d, r10d
       cmp      r11d, 8
       jl       G_M000_IG26
       cmp      r10d, r14d
       jae      G_M000_IG29
       vmovups  ymm3, ymmword ptr [rdx+4*r10+0x10]
       mov      rdx, rax
       test     r8d, r8d

G_M000_IG05:                ;; offset=0x0189
       jl       G_M000_IG26
       mov      r10d, r14d
       sub      r10d, r8d
       cmp      r10d, 8
       jl       G_M000_IG26
       cmp      r8d, r14d
       jae      G_M000_IG29
       mov      r10d, r8d
       vmovups  ymm4, ymmword ptr [rdx+4*r10+0x10]
       mov      rdx, rax
       inc      r8d
       mov      r10d, r14d
       sub      r10d, r8d
       cmp      r10d, 8
       jl       G_M000_IG26
       cmp      r8d, r14d
       jae      G_M000_IG29
       vmovups  ymm5, ymmword ptr [rdx+4*r8+0x10]
       mov      rdx, rax
       lea      r8d, [r9-0x01]
       test     r8d, r8d
       jl       G_M000_IG26
       mov      r10d, r14d
       sub      r10d, r8d
       cmp      r10d, 8
       jl       G_M000_IG26
       cmp      r8d, r14d
       jae      G_M000_IG29
       vmovups  ymm6, ymmword ptr [rdx+4*r8+0x10]
       mov      rdx, rax
       test     r9d, r9d
       jl       G_M000_IG26
       mov      r8d, r14d
       sub      r8d, r9d
       cmp      r8d, 8
       jl       G_M000_IG26
       cmp      r9d, r14d
       jae      G_M000_IG29
       mov      r8d, r9d
       vmovups  ymm7, ymmword ptr [rdx+4*r8+0x10]
       inc      r9d
       mov      edx, r14d
       sub      edx, r9d
       cmp      edx, 8
       jl       G_M000_IG26
       cmp      r9d, r14d
       jae      G_M000_IG29
       mov      edx, r9d
       vmovups  ymm8, ymmword ptr [rax+4*rdx+0x10]
       cmp      byte  ptr [rbx+0x68], 0

G_M000_IG06:                ;; offset=0x025F
       jne      G_M000_IG15

G_M000_IG07:                ;; offset=0x0265
       vcmpeqps ymm9, ymm0, ymm1
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm1
       vandps   ymm10, ymm10, ymm9
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm10, ymm10, ymm11
       vcmpltps ymm12, ymm1, ymm0
       vorps    ymm10, ymm12, ymm10
       vandps   ymm12, ymm10, ymm0
       vandnps  ymm10, ymm10, ymm1
       vorps    ymm10, ymm10, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm0
       vandps   ymm9, ymm12, ymm9
       vorps    ymm11, ymm9, ymm11
       vcmpltps ymm9, ymm0, ymm1
       vorps    ymm9, ymm9, ymm11
       vandps   ymm0, ymm9, ymm0
       vandnps  ymm1, ymm9, ymm1
       vorps    ymm0, ymm1, ymm0
       vcmpeqps ymm1, ymm2, ymm3
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm3
       vandps   ymm9, ymm9, ymm1
       vcmpneqps ymm11, ymm2, ymm2
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm12, ymm3, ymm2
       vorps    ymm9, ymm12, ymm9
       vandps   ymm12, ymm9, ymm2
       vandnps  ymm9, ymm9, ymm3
       vorps    ymm9, ymm9, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm2
       vandps   ymm1, ymm12, ymm1
       vorps    ymm1, ymm11, ymm1
       vcmpltps ymm11, ymm2, ymm3
       vorps    ymm1, ymm11, ymm1
       vandps   ymm2, ymm2, ymm1
       vandnps  ymm3, ymm1, ymm3
       vorps    ymm2, ymm3, ymm2
       vcmpeqps ymm1, ymm0, ymm2
       vxorps   ymm3, ymm3, ymm3
       vpcmpgtd ymm3, ymm3, ymm2
       vandps   ymm3, ymm3, ymm1
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm12, ymm2, ymm0
       vorps    ymm3, ymm12, ymm3
       vandps   ymm12, ymm0, ymm3
       vandnps  ymm3, ymm3, ymm2
       vorps    ymm3, ymm12, ymm3
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm0
       vandps   ymm1, ymm12, ymm1
       vorps    ymm1, ymm11, ymm1
       vcmpltps ymm11, ymm0, ymm2
       vorps    ymm1, ymm11, ymm1
       vandps   ymm0, ymm0, ymm1
       vandnps  ymm2, ymm1, ymm2

G_M000_IG08:                ;; offset=0x036C
       vorps    ymm0, ymm2, ymm0
       vcmpeqps ymm1, ymm10, ymm9
       vxorps   ymm2, ymm2, ymm2
       vpcmpgtd ymm2, ymm2, ymm9
       vandps   ymm2, ymm2, ymm1
       vcmpneqps ymm11, ymm10, ymm10
       vorps    ymm2, ymm11, ymm2
       vcmpltps ymm12, ymm9, ymm10
       vorps    ymm2, ymm12, ymm2
       vandps   ymm12, ymm10, ymm2
       vandnps  ymm2, ymm2, ymm9
       vorps    ymm2, ymm12, ymm2
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm10
       vandps   ymm1, ymm12, ymm1
       vorps    ymm1, ymm11, ymm1
       vcmpltps ymm11, ymm10, ymm9
       vorps    ymm1, ymm11, ymm1
       vandps   ymm10, ymm10, ymm1
       vandnps  ymm1, ymm1, ymm9
       vorps    ymm1, ymm10, ymm1
       vcmpeqps ymm9, ymm1, ymm3
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm3
       vandps   ymm10, ymm10, ymm9
       vcmpneqps ymm11, ymm1, ymm1
       vorps    ymm10, ymm10, ymm11
       vcmpltps ymm12, ymm3, ymm1
       vorps    ymm10, ymm12, ymm10
       vandps   ymm12, ymm10, ymm1
       vandnps  ymm10, ymm10, ymm3
       vorps    ymm10, ymm10, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm1
       vandps   ymm9, ymm12, ymm9
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm11, ymm1, ymm3
       vorps    ymm9, ymm11, ymm9
       vandps   ymm1, ymm9, ymm1
       vandnps  ymm3, ymm9, ymm3
       vorps    ymm1, ymm3, ymm1
       vcmpeqps ymm3, ymm0, ymm4
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm4
       vandps   ymm9, ymm9, ymm3
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm12, ymm4, ymm0
       vorps    ymm9, ymm12, ymm9
       vandps   ymm12, ymm9, ymm0
       vandnps  ymm9, ymm9, ymm4
       vorps    ymm9, ymm9, ymm12
       vcmpeqps ymm12, ymm1, ymm9
       vxorps   ymm13, ymm13, ymm13
       vpcmpgtd ymm13, ymm13, ymm9
       vandps   ymm13, ymm13, ymm12
       vcmpneqps ymm14, ymm1, ymm1
       vmovups  ymmword ptr [rsp+0xA0], ymm14
       vorps    ymm13, ymm13, ymm14

G_M000_IG09:                ;; offset=0x0485
       vcmpltps ymm15, ymm9, ymm1
       vorps    ymm13, ymm15, ymm13
       vandps   ymm15, ymm13, ymm1
       vandnps  ymm13, ymm13, ymm9
       vorps    ymm13, ymm13, ymm15
       vcmpeqps ymm15, ymm10, ymm13
       vmovups  ymmword ptr [rsp+0x80], ymm15
       vxorps   ymm14, ymm14, ymm14
       vpcmpgtd ymm14, ymm14, ymm13
       vandps   ymm14, ymm14, ymm15
       vcmpneqps ymm15, ymm10, ymm10
       vmovups  ymmword ptr [rsp+0x60], ymm15
       vorps    ymm14, ymm14, ymm15
       vcmpltps ymm15, ymm13, ymm10
       vorps    ymm14, ymm15, ymm14
       vandps   ymm15, ymm10, ymm14
       vandnps  ymm14, ymm14, ymm13
       vorps    ymm14, ymm14, ymm15
       vxorps   ymm15, ymm15, ymm15
       vpcmpgtd ymm15, ymm15, ymm0
       vandps   ymm3, ymm15, ymm3
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm11, ymm0, ymm4
       vorps    ymm3, ymm11, ymm3
       vandps   ymm0, ymm0, ymm3
       vandnps  ymm4, ymm3, ymm4
       vorps    ymm0, ymm4, ymm0
       vxorps   ymm3, ymm3, ymm3
       vpcmpgtd ymm3, ymm3, ymm1
       vandps   ymm3, ymm12, ymm3
       vorps    ymm3, ymm3, ymmword ptr [rsp+0xA0]
       vcmpltps ymm4, ymm1, ymm9
       vorps    ymm3, ymm4, ymm3
       vandps   ymm1, ymm1, ymm3
       vandnps  ymm3, ymm3, ymm9
       vorps    ymm1, ymm3, ymm1
       vxorps   ymm3, ymm3, ymm3
       vpcmpgtd ymm3, ymm3, ymm10
       vandps   ymm3, ymm3, ymmword ptr [rsp+0x80]
       vorps    ymm3, ymm3, ymmword ptr [rsp+0x60]
       vcmpltps ymm4, ymm10, ymm13
       vorps    ymm3, ymm4, ymm3
       vandps   ymm4, ymm10, ymm3
       vandnps  ymm3, ymm3, ymm13
       vorps    ymm3, ymm3, ymm4
       vcmpeqps ymm4, ymm2, ymm14
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm2
       vandps   ymm4, ymm9, ymm4
       vcmpneqps ymm9, ymm2, ymm2
       vorps    ymm4, ymm9, ymm4
       vcmpltps ymm9, ymm2, ymm14
       vorps    ymm4, ymm9, ymm4
       vblendvps ymm2, ymm14, ymm2, ymm4
       vcmpeqps ymm4, ymm0, ymm5
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm5
       vandps   ymm9, ymm9, ymm4
       vcmpneqps ymm10, ymm0, ymm0

G_M000_IG10:                ;; offset=0x05AA
       vorps    ymm9, ymm9, ymm10
       vcmpltps ymm11, ymm5, ymm0
       vorps    ymm9, ymm11, ymm9
       vandps   ymm11, ymm9, ymm0
       vandnps  ymm9, ymm9, ymm5
       vorps    ymm9, ymm9, ymm11
       vcmpeqps ymm11, ymm1, ymm9
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm9
       vandps   ymm12, ymm12, ymm11
       vcmpneqps ymm13, ymm1, ymm1
       vorps    ymm12, ymm12, ymm13
       vcmpltps ymm14, ymm9, ymm1
       vorps    ymm12, ymm14, ymm12
       vandps   ymm14, ymm12, ymm1
       vandnps  ymm12, ymm12, ymm9
       vorps    ymm12, ymm12, ymm14
       vcmpeqps ymm14, ymm3, ymm12
       vmovups  ymmword ptr [rsp+0x40], ymm14
       vxorps   ymm15, ymm15, ymm15
       vpcmpgtd ymm15, ymm15, ymm12
       vandps   ymm15, ymm15, ymm14
       vcmpneqps ymm14, ymm3, ymm3
       vmovups  ymmword ptr [rsp+0x20], ymm14
       vorps    ymm15, ymm15, ymm14
       vcmpltps ymm14, ymm12, ymm3
       vorps    ymm14, ymm14, ymm15
       vandps   ymm15, ymm14, ymm3
       vandnps  ymm14, ymm14, ymm12
       vorps    ymm14, ymm14, ymm15
       vxorps   ymm15, ymm15, ymm15
       vpcmpgtd ymm15, ymm15, ymm0
       vandps   ymm4, ymm15, ymm4
       vorps    ymm4, ymm10, ymm4
       vcmpltps ymm10, ymm0, ymm5
       vorps    ymm4, ymm10, ymm4
       vandps   ymm0, ymm0, ymm4
       vandnps  ymm5, ymm4, ymm5
       vorps    ymm0, ymm5, ymm0
       vxorps   ymm4, ymm4, ymm4
       vpcmpgtd ymm4, ymm4, ymm1
       vandps   ymm4, ymm11, ymm4
       vorps    ymm4, ymm13, ymm4
       vcmpltps ymm5, ymm1, ymm9
       vorps    ymm4, ymm5, ymm4
       vandps   ymm1, ymm1, ymm4
       vandnps  ymm4, ymm4, ymm9
       vorps    ymm1, ymm4, ymm1
       vxorps   ymm4, ymm4, ymm4
       vpcmpgtd ymm4, ymm4, ymm3
       vandps   ymm4, ymm4, ymmword ptr [rsp+0x40]
       vorps    ymm4, ymm4, ymmword ptr [rsp+0x20]
       vcmpltps ymm5, ymm3, ymm12
       vorps    ymm4, ymm5, ymm4
       vandps   ymm3, ymm3, ymm4
       vandnps  ymm4, ymm4, ymm12
       vorps    ymm3, ymm4, ymm3
       vcmpeqps ymm4, ymm2, ymm14
       vxorps   ymm5, ymm5, ymm5

G_M000_IG11:                ;; offset=0x06C2
       vpcmpgtd ymm5, ymm5, ymm2
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm2, ymm2
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm2, ymm14
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm2, ymm14, ymm2, ymm4
       vcmpeqps ymm4, ymm0, ymm6
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm6
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm0, ymm0
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm6, ymm0
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm4, ymm6, ymm0, ymm4
       vcmpeqps ymm5, ymm1, ymm4
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm4
       vandps   ymm5, ymm9, ymm5
       vcmpneqps ymm9, ymm1, ymm1
       vorps    ymm5, ymm9, ymm5
       vcmpltps ymm9, ymm4, ymm1
       vorps    ymm5, ymm9, ymm5
       vblendvps ymm5, ymm4, ymm1, ymm5
       vcmpeqps ymm9, ymm3, ymm5
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm5
       vandps   ymm9, ymm10, ymm9
       vcmpneqps ymm10, ymm3, ymm3
       vorps    ymm9, ymm10, ymm9
       vcmpltps ymm10, ymm5, ymm3
       vorps    ymm9, ymm10, ymm9
       vblendvps ymm9, ymm5, ymm3, ymm9
       vcmpeqps ymm10, ymm0, ymm6
       vxorps   ymm11, ymm11, ymm11
       vpcmpgtd ymm11, ymm11, ymm0
       vandps   ymm10, ymm11, ymm10
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm10, ymm11, ymm10
       vcmpltps ymm11, ymm0, ymm6
       vorps    ymm10, ymm11, ymm10
       vblendvps ymm0, ymm6, ymm0, ymm10
       vcmpeqps ymm6, ymm1, ymm4
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm1
       vandps   ymm6, ymm10, ymm6
       vcmpneqps ymm10, ymm1, ymm1
       vorps    ymm6, ymm10, ymm6
       vcmpltps ymm10, ymm1, ymm4
       vorps    ymm6, ymm10, ymm6
       vblendvps ymm1, ymm4, ymm1, ymm6
       vcmpeqps ymm4, ymm3, ymm5
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm3
       vandps   ymm4, ymm6, ymm4
       vcmpneqps ymm6, ymm3, ymm3
       vorps    ymm4, ymm6, ymm4
       vcmpltps ymm6, ymm3, ymm5

G_M000_IG12:                ;; offset=0x07D9
       vorps    ymm4, ymm6, ymm4
       vblendvps ymm3, ymm5, ymm3, ymm4
       vcmpeqps ymm4, ymm2, ymm9
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm2
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm2, ymm2
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm2, ymm9
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm2, ymm9, ymm2, ymm4
       vcmpeqps ymm4, ymm0, ymm7
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm7
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm0, ymm0
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm7, ymm0
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm4, ymm7, ymm0, ymm4
       vcmpeqps ymm5, ymm1, ymm4
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm4
       vandps   ymm5, ymm6, ymm5
       vcmpneqps ymm6, ymm1, ymm1
       vorps    ymm5, ymm6, ymm5
       vcmpltps ymm6, ymm4, ymm1
       vorps    ymm5, ymm6, ymm5
       vblendvps ymm5, ymm4, ymm1, ymm5
       vcmpeqps ymm6, ymm3, ymm5
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm5
       vandps   ymm6, ymm9, ymm6
       vcmpneqps ymm9, ymm3, ymm3
       vorps    ymm6, ymm9, ymm6
       vcmpltps ymm9, ymm5, ymm3
       vorps    ymm6, ymm9, ymm6
       vblendvps ymm6, ymm5, ymm3, ymm6
       vcmpeqps ymm9, ymm0, ymm7
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm0
       vandps   ymm9, ymm10, ymm9
       vcmpneqps ymm10, ymm0, ymm0
       vorps    ymm9, ymm10, ymm9
       vcmpltps ymm10, ymm0, ymm7
       vorps    ymm9, ymm10, ymm9
       vblendvps ymm0, ymm7, ymm0, ymm9
       vcmpeqps ymm7, ymm1, ymm4
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm1
       vandps   ymm7, ymm9, ymm7
       vcmpneqps ymm9, ymm1, ymm1
       vorps    ymm7, ymm9, ymm7
       vcmpltps ymm9, ymm1, ymm4
       vorps    ymm7, ymm9, ymm7
       vblendvps ymm1, ymm4, ymm1, ymm7
       vcmpeqps ymm4, ymm3, ymm5
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm3

G_M000_IG13:                ;; offset=0x08EE
       vandps   ymm4, ymm7, ymm4
       vcmpneqps ymm7, ymm3, ymm3
       vorps    ymm4, ymm7, ymm4
       vcmpltps ymm7, ymm3, ymm5
       vorps    ymm4, ymm7, ymm4
       vblendvps ymm3, ymm5, ymm3, ymm4
       vcmpeqps ymm4, ymm2, ymm6
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm2
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm2, ymm2
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm2, ymm6
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm2, ymm6, ymm2, ymm4
       vcmpeqps ymm4, ymm0, ymm8
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm8
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm0, ymm0
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm8, ymm0
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm4, ymm8, ymm0, ymm4
       vcmpeqps ymm5, ymm1, ymm4
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm4
       vandps   ymm5, ymm6, ymm5
       vcmpneqps ymm6, ymm1, ymm1
       vorps    ymm5, ymm6, ymm5
       vcmpltps ymm6, ymm4, ymm1
       vorps    ymm5, ymm6, ymm5
       vblendvps ymm5, ymm4, ymm1, ymm5
       vcmpeqps ymm6, ymm3, ymm5
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm5
       vandps   ymm6, ymm7, ymm6
       vcmpneqps ymm7, ymm3, ymm3
       vorps    ymm6, ymm7, ymm6
       vcmpltps ymm7, ymm5, ymm3
       vorps    ymm6, ymm7, ymm6
       vblendvps ymm6, ymm5, ymm3, ymm6
       vcmpeqps ymm7, ymm0, ymm8
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm0
       vandps   ymm7, ymm9, ymm7
       vcmpneqps ymm9, ymm0, ymm0
       vorps    ymm7, ymm9, ymm7
       vcmpltps ymm9, ymm0, ymm8
       vorps    ymm7, ymm9, ymm7
       vblendvps ymm0, ymm8, ymm0, ymm7
       vcmpeqps ymm8, ymm1, ymm4
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm1
       vandps   ymm7, ymm8, ymm7
       vcmpneqps ymm8, ymm1, ymm1
       vorps    ymm7, ymm8, ymm7
       vcmpltps ymm8, ymm1, ymm4
       vorps    ymm7, ymm8, ymm7

G_M000_IG14:                ;; offset=0x09FF
       vblendvps ymm1, ymm4, ymm1, ymm7
       vcmpeqps ymm4, ymm3, ymm5
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm3
       vandps   ymm4, ymm7, ymm4
       vcmpneqps ymm7, ymm3, ymm3
       vorps    ymm4, ymm7, ymm4
       vcmpltps ymm7, ymm3, ymm5
       vorps    ymm4, ymm7, ymm4
       vblendvps ymm3, ymm5, ymm3, ymm4
       vcmpeqps ymm4, ymm2, ymm6
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm2
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm2, ymm2
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm2, ymm6
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm2, ymm6, ymm2, ymm4
       vmovaps  ymm5, ymm3
       jmp      G_M000_IG22

G_M000_IG15:                ;; offset=0x0A60
       vcmpeqps ymm9, ymm0, ymm3
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm3
       vandps   ymm10, ymm10, ymm9
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm10, ymm10, ymm11
       vcmpltps ymm12, ymm3, ymm0
       vorps    ymm10, ymm12, ymm10
       vandps   ymm12, ymm10, ymm0
       vandnps  ymm10, ymm10, ymm3
       vorps    ymm10, ymm10, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm0
       vandps   ymm9, ymm12, ymm9
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm11, ymm0, ymm3
       vorps    ymm9, ymm11, ymm9
       vandps   ymm0, ymm9, ymm0
       vandnps  ymm3, ymm9, ymm3
       vorps    ymm0, ymm3, ymm0
       vcmpeqps ymm3, ymm1, ymm7
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm7
       vandps   ymm9, ymm9, ymm3
       vcmpneqps ymm11, ymm1, ymm1
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm12, ymm7, ymm1
       vorps    ymm9, ymm12, ymm9
       vandps   ymm12, ymm9, ymm1
       vandnps  ymm9, ymm9, ymm7
       vorps    ymm9, ymm9, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm1
       vandps   ymm3, ymm12, ymm3
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm11, ymm1, ymm7
       vorps    ymm3, ymm11, ymm3
       vandps   ymm1, ymm1, ymm3
       vandnps  ymm3, ymm3, ymm7
       vorps    ymm1, ymm3, ymm1
       vcmpeqps ymm3, ymm2, ymm5
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm5
       vandps   ymm7, ymm7, ymm3
       vcmpneqps ymm11, ymm2, ymm2
       vorps    ymm7, ymm11, ymm7
       vcmpltps ymm12, ymm5, ymm2
       vorps    ymm7, ymm12, ymm7
       vandps   ymm12, ymm2, ymm7
       vandnps  ymm7, ymm7, ymm5
       vorps    ymm7, ymm12, ymm7
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm2
       vandps   ymm3, ymm12, ymm3
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm11, ymm2, ymm5
       vorps    ymm3, ymm11, ymm3
       vandps   ymm2, ymm2, ymm3
       vandnps  ymm3, ymm3, ymm5

G_M000_IG16:                ;; offset=0x0B67
       vorps    ymm2, ymm3, ymm2
       vcmpeqps ymm3, ymm4, ymm8
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm8
       vandps   ymm5, ymm5, ymm3
       vcmpneqps ymm11, ymm4, ymm4
       vorps    ymm5, ymm11, ymm5
       vcmpltps ymm12, ymm8, ymm4
       vorps    ymm5, ymm12, ymm5
       vandps   ymm12, ymm4, ymm5
       vandnps  ymm5, ymm5, ymm8
       vorps    ymm5, ymm12, ymm5
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm4
       vandps   ymm3, ymm12, ymm3
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm11, ymm4, ymm8
       vorps    ymm3, ymm11, ymm3
       vandps   ymm4, ymm4, ymm3
       vandnps  ymm3, ymm3, ymm8
       vorps    ymm3, ymm3, ymm4
       vcmpeqps ymm4, ymm0, ymm9
       vxorps   ymm8, ymm8, ymm8
       vpcmpgtd ymm8, ymm8, ymm9
       vandps   ymm8, ymm8, ymm4
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm8, ymm8, ymm11
       vcmpltps ymm12, ymm9, ymm0
       vorps    ymm8, ymm12, ymm8
       vandps   ymm12, ymm8, ymm0
       vandnps  ymm8, ymm8, ymm9
       vorps    ymm8, ymm8, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm0
       vandps   ymm4, ymm12, ymm4
       vorps    ymm4, ymm11, ymm4
       vcmpltps ymm11, ymm0, ymm9
       vorps    ymm4, ymm11, ymm4
       vandps   ymm0, ymm0, ymm4
       vandnps  ymm4, ymm4, ymm9
       vorps    ymm0, ymm4, ymm0
       vcmpeqps ymm4, ymm2, ymm3
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm3
       vandps   ymm9, ymm9, ymm4
       vcmpneqps ymm11, ymm2, ymm2
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm12, ymm3, ymm2
       vorps    ymm9, ymm12, ymm9
       vandps   ymm12, ymm9, ymm2
       vandnps  ymm9, ymm9, ymm3
       vorps    ymm9, ymm9, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm2
       vandps   ymm4, ymm12, ymm4
       vorps    ymm4, ymm11, ymm4
       vcmpltps ymm11, ymm2, ymm3
       vorps    ymm4, ymm11, ymm4
       vandps   ymm2, ymm2, ymm4

G_M000_IG17:                ;; offset=0x0C74
       vandnps  ymm3, ymm4, ymm3
       vorps    ymm2, ymm3, ymm2
       vcmpeqps ymm3, ymm10, ymm5
       vxorps   ymm4, ymm4, ymm4
       vpcmpgtd ymm4, ymm4, ymm5
       vandps   ymm4, ymm4, ymm3
       vcmpneqps ymm11, ymm10, ymm10
       vorps    ymm4, ymm11, ymm4
       vcmpltps ymm12, ymm5, ymm10
       vorps    ymm4, ymm12, ymm4
       vandps   ymm12, ymm10, ymm4
       vandnps  ymm4, ymm4, ymm5
       vorps    ymm4, ymm12, ymm4
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm10
       vandps   ymm3, ymm12, ymm3
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm11, ymm10, ymm5
       vorps    ymm3, ymm11, ymm3
       vandps   ymm10, ymm10, ymm3
       vandnps  ymm3, ymm3, ymm5
       vorps    ymm3, ymm10, ymm3
       vcmpeqps ymm5, ymm6, ymm7
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm6
       vandps   ymm10, ymm10, ymm5
       vcmpneqps ymm11, ymm7, ymm7
       vorps    ymm10, ymm10, ymm11
       vcmpltps ymm12, ymm6, ymm7
       vorps    ymm10, ymm12, ymm10
       vandps   ymm12, ymm10, ymm7
       vandnps  ymm10, ymm10, ymm6
       vorps    ymm10, ymm10, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm7
       vandps   ymm5, ymm12, ymm5
       vorps    ymm5, ymm11, ymm5
       vcmpltps ymm11, ymm7, ymm6
       vorps    ymm5, ymm11, ymm5
       vandps   ymm7, ymm7, ymm5
       vandnps  ymm5, ymm5, ymm6
       vorps    ymm5, ymm5, ymm7
       vcmpeqps ymm6, ymm0, ymm2
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm2
       vandps   ymm7, ymm7, ymm6
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm7, ymm11, ymm7
       vcmpltps ymm12, ymm2, ymm0
       vorps    ymm7, ymm12, ymm7
       vandps   ymm12, ymm0, ymm7
       vandnps  ymm7, ymm7, ymm2
       vorps    ymm7, ymm12, ymm7
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm0
       vandps   ymm6, ymm12, ymm6
       vorps    ymm6, ymm11, ymm6
       vcmpltps ymm11, ymm0, ymm2
       vorps    ymm6, ymm11, ymm6

G_M000_IG18:                ;; offset=0x0D76
       vandps   ymm0, ymm0, ymm6
       vandnps  ymm2, ymm6, ymm2
       vorps    ymm0, ymm2, ymm0
       vcmpeqps ymm2, ymm1, ymm3
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm3
       vandps   ymm6, ymm6, ymm2
       vcmpneqps ymm11, ymm1, ymm1
       vorps    ymm6, ymm11, ymm6
       vcmpltps ymm12, ymm3, ymm1
       vorps    ymm6, ymm12, ymm6
       vandps   ymm12, ymm1, ymm6
       vandnps  ymm6, ymm6, ymm3
       vorps    ymm6, ymm12, ymm6
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm1
       vandps   ymm2, ymm12, ymm2
       vorps    ymm2, ymm11, ymm2
       vcmpltps ymm11, ymm1, ymm3
       vorps    ymm2, ymm11, ymm2
       vandps   ymm1, ymm1, ymm2
       vandnps  ymm3, ymm2, ymm3
       vorps    ymm1, ymm3, ymm1
       vcmpeqps ymm2, ymm5, ymm9
       vxorps   ymm3, ymm3, ymm3
       vpcmpgtd ymm3, ymm3, ymm5
       vandps   ymm3, ymm3, ymm2
       vcmpneqps ymm11, ymm9, ymm9
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm12, ymm5, ymm9
       vorps    ymm3, ymm12, ymm3
       vandps   ymm12, ymm9, ymm3
       vandnps  ymm3, ymm3, ymm5
       vorps    ymm3, ymm12, ymm3
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm9
       vandps   ymm2, ymm12, ymm2
       vorps    ymm2, ymm11, ymm2
       vcmpltps ymm11, ymm9, ymm5
       vorps    ymm2, ymm11, ymm2
       vandps   ymm9, ymm9, ymm2
       vandnps  ymm5, ymm2, ymm5
       vorps    ymm2, ymm9, ymm5
       vcmpeqps ymm5, ymm8, ymm4
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm8
       vandps   ymm5, ymm9, ymm5
       vcmpneqps ymm9, ymm8, ymm8
       vorps    ymm5, ymm9, ymm5
       vcmpltps ymm9, ymm8, ymm4
       vorps    ymm5, ymm9, ymm5
       vblendvps ymm4, ymm4, ymm8, ymm5
       vcmpeqps ymm5, ymm1, ymm2
       vxorps   ymm8, ymm8, ymm8
       vpcmpgtd ymm8, ymm8, ymm2
       vandps   ymm8, ymm8, ymm5
       vcmpneqps ymm9, ymm1, ymm1
       vorps    ymm8, ymm8, ymm9
       vcmpltps ymm11, ymm2, ymm1

G_M000_IG19:                ;; offset=0x0E7D
       vorps    ymm8, ymm11, ymm8
       vandps   ymm11, ymm8, ymm1
       vandnps  ymm8, ymm8, ymm2
       vorps    ymm8, ymm8, ymm11
       vxorps   ymm11, ymm11, ymm11
       vpcmpgtd ymm11, ymm11, ymm1
       vandps   ymm5, ymm11, ymm5
       vorps    ymm5, ymm9, ymm5
       vcmpltps ymm9, ymm1, ymm2
       vorps    ymm5, ymm9, ymm5
       vandps   ymm1, ymm1, ymm5
       vandnps  ymm2, ymm5, ymm2
       vorps    ymm1, ymm2, ymm1
       vcmpeqps ymm2, ymm6, ymm10
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm6
       vandps   ymm2, ymm5, ymm2
       vcmpneqps ymm5, ymm6, ymm6
       vorps    ymm2, ymm5, ymm2
       vcmpltps ymm5, ymm6, ymm10
       vorps    ymm2, ymm5, ymm2
       vblendvps ymm2, ymm10, ymm6, ymm2
       vcmpeqps ymm5, ymm4, ymm3
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm3
       vandps   ymm5, ymm6, ymm5
       vcmpneqps ymm6, ymm3, ymm3
       vorps    ymm5, ymm6, ymm5
       vcmpltps ymm6, ymm3, ymm4
       vorps    ymm5, ymm6, ymm5
       vblendvps ymm5, ymm4, ymm3, ymm5
       vcmpeqps ymm3, ymm0, ymm1
       vxorps   ymm4, ymm4, ymm4
       vpcmpgtd ymm4, ymm4, ymm1
       vandps   ymm4, ymm4, ymm3
       vcmpneqps ymm6, ymm0, ymm0
       vorps    ymm4, ymm4, ymm6
       vcmpltps ymm9, ymm1, ymm0
       vorps    ymm4, ymm9, ymm4
       vandps   ymm9, ymm0, ymm4
       vandnps  ymm4, ymm4, ymm1
       vorps    ymm4, ymm9, ymm4
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm0
       vandps   ymm3, ymm9, ymm3
       vorps    ymm3, ymm3, ymm6
       vcmpltps ymm6, ymm0, ymm1
       vorps    ymm3, ymm6, ymm3
       vandps   ymm0, ymm0, ymm3
       vandnps  ymm1, ymm3, ymm1
       vorps    ymm0, ymm1, ymm0
       vcmpeqps ymm1, ymm7, ymm8
       vxorps   ymm3, ymm3, ymm3
       vpcmpgtd ymm3, ymm3, ymm8
       vandps   ymm3, ymm3, ymm1
       vcmpneqps ymm6, ymm7, ymm7
       vorps    ymm3, ymm3, ymm6
       vcmpltps ymm9, ymm8, ymm7
       vorps    ymm3, ymm9, ymm3

G_M000_IG20:                ;; offset=0x0F83
       vandps   ymm9, ymm7, ymm3
       vandnps  ymm3, ymm3, ymm8
       vorps    ymm3, ymm9, ymm3
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm7
       vandps   ymm1, ymm9, ymm1
       vorps    ymm1, ymm1, ymm6
       vcmpltps ymm6, ymm7, ymm8
       vorps    ymm1, ymm6, ymm1
       vandps   ymm6, ymm7, ymm1
       vandnps  ymm1, ymm1, ymm8
       vorps    ymm1, ymm1, ymm6
       vcmpeqps ymm6, ymm2, ymm5
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm5
       vandps   ymm7, ymm7, ymm6
       vcmpneqps ymm8, ymm2, ymm2
       vorps    ymm7, ymm8, ymm7
       vcmpltps ymm9, ymm5, ymm2
       vorps    ymm7, ymm9, ymm7
       vandps   ymm9, ymm2, ymm7
       vandnps  ymm7, ymm7, ymm5
       vorps    ymm7, ymm9, ymm7
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm2
       vandps   ymm6, ymm9, ymm6
       vorps    ymm6, ymm8, ymm6
       vcmpltps ymm8, ymm2, ymm5
       vorps    ymm6, ymm8, ymm6
       vandps   ymm2, ymm2, ymm6
       vandnps  ymm5, ymm6, ymm5
       vorps    ymm2, ymm5, ymm2
       vcmpeqps ymm5, ymm1, ymm2
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm2
       vandps   ymm6, ymm6, ymm5
       vcmpneqps ymm8, ymm1, ymm1
       vorps    ymm6, ymm8, ymm6
       vcmpltps ymm9, ymm2, ymm1
       vorps    ymm6, ymm9, ymm6
       vandps   ymm9, ymm1, ymm6
       vandnps  ymm6, ymm6, ymm2
       vorps    ymm6, ymm9, ymm6
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm1
       vandps   ymm5, ymm9, ymm5
       vorps    ymm5, ymm8, ymm5
       vcmpltps ymm8, ymm1, ymm2
       vorps    ymm5, ymm8, ymm5
       vandps   ymm1, ymm1, ymm5
       vandnps  ymm2, ymm5, ymm2
       vorps    ymm1, ymm2, ymm1
       vcmpeqps ymm2, ymm3, ymm7
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm3
       vandps   ymm2, ymm5, ymm2
       vcmpneqps ymm5, ymm3, ymm3
       vorps    ymm2, ymm5, ymm2
       vcmpltps ymm5, ymm3, ymm7

G_M000_IG21:                ;; offset=0x1081
       vorps    ymm2, ymm5, ymm2
       vblendvps ymm2, ymm7, ymm3, ymm2
       vcmpeqps ymm3, ymm1, ymm4
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm1
       vandps   ymm5, ymm5, ymm3
       vcmpneqps ymm7, ymm4, ymm4
       vorps    ymm5, ymm5, ymm7
       vcmpltps ymm8, ymm1, ymm4
       vorps    ymm5, ymm8, ymm5
       vandps   ymm8, ymm4, ymm5
       vandnps  ymm5, ymm5, ymm1
       vorps    ymm5, ymm8, ymm5
       vxorps   ymm8, ymm8, ymm8
       vpcmpgtd ymm8, ymm8, ymm4
       vandps   ymm3, ymm8, ymm3
       vorps    ymm3, ymm3, ymm7
       vcmpltps ymm7, ymm4, ymm1
       vorps    ymm3, ymm7, ymm3
       vandps   ymm4, ymm4, ymm3
       vandnps  ymm1, ymm3, ymm1
       vorps    ymm1, ymm1, ymm4
       vcmpeqps ymm3, ymm2, ymm6
       vxorps   ymm4, ymm4, ymm4
       vpcmpgtd ymm4, ymm4, ymm6
       vandps   ymm3, ymm4, ymm3
       vcmpneqps ymm4, ymm6, ymm6
       vorps    ymm3, ymm4, ymm3
       vcmpltps ymm4, ymm6, ymm2
       vorps    ymm3, ymm4, ymm3
       vblendvps ymm2, ymm2, ymm6, ymm3

G_M000_IG22:                ;; offset=0x1109
       vmulps   ymm0, ymm0, ymmword ptr [reloc @RWD00]
       vmulps   ymm1, ymm1, ymmword ptr [reloc @RWD32]
       vaddps   ymm0, ymm1, ymm0
       vmulps   ymm1, ymm5, ymmword ptr [reloc @RWD64]
       vmulps   ymm2, ymm2, ymmword ptr [reloc @RWD96]
       vaddps   ymm1, ymm2, ymm1
       vaddps   ymm0, ymm1, ymm0
       mov      rdx, gword ptr [rbx+0x28]
       lea      r8d, [rdi+rbp]
       cmp      dword ptr [rdx+0x08], r8d
       jbe      G_M000_IG27
       mov      r9d, dword ptr [rdx+0x08]
       sub      r9d, r8d
       cmp      r9d, 8
       jl       G_M000_IG28
       vmovups  ymmword ptr [rdx+4*r8+0x10], ymm0
       add      ebp, 8
       mov      edx, dword ptr [rbx+0x58]
       add      edx, -8
       cmp      ebp, edx
       jl       G_M000_IG04

G_M000_IG23:                ;; offset=0x1170
       cmp      ebp, dword ptr [rbx+0x58]
       jge      SHORT G_M000_IG25

G_M000_IG24:                ;; offset=0x1175
       mov      rcx, rbx
       mov      edx, esi
       mov      r8d, edi
       mov      r9d, ebp
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this]
       inc      ebp
       cmp      ebp, dword ptr [rbx+0x58]
       jl       SHORT G_M000_IG24

G_M000_IG25:                ;; offset=0x118D
       vzeroupper
       vmovaps  xmm6, xmmword ptr [rsp+0x150]
       vmovaps  xmm7, xmmword ptr [rsp+0x140]
       vmovaps  xmm8, xmmword ptr [rsp+0x130]
       vmovaps  xmm9, xmmword ptr [rsp+0x120]
       vmovaps  xmm10, xmmword ptr [rsp+0x110]
       vmovaps  xmm11, xmmword ptr [rsp+0x100]
       vmovaps  xmm12, xmmword ptr [rsp+0xF0]
       vmovaps  xmm13, xmmword ptr [rsp+0xE0]
       vmovaps  xmm14, xmmword ptr [rsp+0xD0]
       vmovaps  xmm15, xmmword ptr [rsp+0xC0]
       add      rsp, 360
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r14
       pop      r15
       ret

G_M000_IG26:                ;; offset=0x11FA
       call     [System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()]
       int3

G_M000_IG27:                ;; offset=0x1201
       call     [System.ThrowHelper:ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLess()]
       int3

G_M000_IG28:                ;; offset=0x1208
       call     [System.ThrowHelper:ThrowArgumentException_DestinationTooShort()]
       int3

G_M000_IG29:                ;; offset=0x120F
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dq	3E0000003E000000h, 3E0000003E000000h, 3E0000003E000000h, 3E0000003E000000h
RWD32  	dq	3D99999A3D99999Ah, 3D99999A3D99999Ah, 3D99999A3D99999Ah, 3D99999A3D99999Ah
RWD64  	dq	3D75C28F3D75C28Fh, 3D75C28F3D75C28Fh, 3D75C28F3D75C28Fh, 3D75C28F3D75C28Fh
RWD96  	dq	3D4CCCCD3D4CCCCDh, 3D4CCCCD3D4CCCCDh, 3D4CCCCD3D4CCCCDh, 3D4CCCCD3D4CCCCDh

; Total bytes of code 4629

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data
; 2 inlinees with PGO data; 29 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r14
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 176
       vmovaps  xmmword ptr [rsp+0xA0], xmm6
       vmovaps  xmmword ptr [rsp+0x90], xmm7
       vmovaps  xmmword ptr [rsp+0x80], xmm8
       vmovaps  xmmword ptr [rsp+0x70], xmm9
       vmovaps  xmmword ptr [rsp+0x60], xmm10
       vmovaps  xmmword ptr [rsp+0x50], xmm11
       vmovaps  xmmword ptr [rsp+0x40], xmm12
       vmovaps  xmmword ptr [rsp+0x30], xmm13
       vmovaps  xmmword ptr [rsp+0x20], xmm14

G_M000_IG02:                ;; offset=0x004C
       lea      eax, [r9-0x01]
       xor      r10d, r10d
       test     eax, eax
       cmovl    eax, r10d
       lea      r10d, [r9+0x01]
       mov      r11d, dword ptr [rcx+0x58]
       lea      ebx, [r11-0x01]
       cmp      r10d, ebx
       cmovg    r10d, ebx
       lea      ebx, [rdx-0x01]
       imul     ebx, r11d
       mov      esi, edx
       imul     esi, r11d
       inc      edx
       imul     edx, r11d
       mov      r11, gword ptr [rcx+0x18]
       mov      rdi, r11
       lea      ebp, [rbx+rax]
       mov      r14d, dword ptr [rdi+0x08]
       cmp      ebp, r14d
       jae      G_M000_IG22
       lea      ebp, [rbx+rax]
       vmovss   xmm0, dword ptr [rdi+4*rbp+0x10]
       mov      rdi, r11
       lea      ebp, [rbx+r9]
       cmp      ebp, r14d
       jae      G_M000_IG22
       lea      ebp, [rbx+r9]
       vmovss   xmm1, dword ptr [rdi+4*rbp+0x10]
       mov      rdi, r11
       lea      ebp, [rbx+r10]
       cmp      ebp, r14d
       jae      G_M000_IG22
       add      ebx, r10d
       vmovss   xmm2, dword ptr [rdi+4*rbx+0x10]
       mov      rbx, r11
       lea      edi, [rsi+rax]
       cmp      edi, r14d
       jae      G_M000_IG22
       lea      edi, [rsi+rax]
       vmovss   xmm3, dword ptr [rbx+4*rdi+0x10]
       mov      rbx, r11
       lea      edi, [rsi+r9]
       cmp      edi, r14d
       jae      G_M000_IG22
       lea      edi, [rsi+r9]
       vmovss   xmm4, dword ptr [rbx+4*rdi+0x10]
       mov      rbx, r11
       lea      edi, [rsi+r10]
       cmp      edi, r14d
       jae      G_M000_IG22
       add      esi, r10d
       vmovss   xmm5, dword ptr [rbx+4*rsi+0x10]
       mov      rbx, r11
       lea      esi, [rdx+rax]
       cmp      esi, r14d
       jae      G_M000_IG22
       add      eax, edx

G_M000_IG03:                ;; offset=0x012E
       vmovss   xmm6, dword ptr [rbx+4*rax+0x10]
       mov      rax, r11
       lea      ebx, [rdx+r9]
       cmp      ebx, r14d
       jae      G_M000_IG22
       lea      ebx, [rdx+r9]
       vmovss   xmm7, dword ptr [rax+4*rbx+0x10]
       lea      eax, [rdx+r10]
       cmp      eax, r14d
       jae      G_M000_IG22
       add      edx, r10d
       mov      eax, edx
       vmovss   xmm8, dword ptr [r11+4*rax+0x10]
       cmp      byte  ptr [rcx+0x68], 0
       jne      G_M000_IG12

G_M000_IG04:                ;; offset=0x0171
       vmovaps  xmm9, xmm0
       vmovaps  xmm10, xmm1
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm9
       vorps    xmm11, xmm13, xmm11
       vandps   xmm9, xmm9, xmm11
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm9, xmm10, xmm9
       vcmpeqps xmm10, xmm0, xmm1
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10
       vorps    xmm12, xmm10, xmm12
       vcmpltps xmm10, xmm0, xmm1
       vorps    xmm10, xmm10, xmm12
       vandps   xmm0, xmm10, xmm0
       vandnps  xmm1, xmm10, xmm1
       vorps    xmm0, xmm1, xmm0
       vmovaps  xmm1, xmm2
       vmovaps  xmm10, xmm3
       vcmpeqps xmm11, xmm1, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm1, xmm1
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm1
       vorps    xmm11, xmm13, xmm11
       vandps   xmm1, xmm11, xmm1
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm1, xmm10, xmm1
       vcmpeqps xmm10, xmm2, xmm3
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm2
       vandps   xmm10, xmm11, xmm10
       vorps    xmm12, xmm10, xmm12
       vcmpltps xmm10, xmm2, xmm3
       vorps    xmm10, xmm10, xmm12
       vandps   xmm2, xmm10, xmm2
       vandnps  xmm3, xmm10, xmm3
       vorps    xmm2, xmm3, xmm2
       vmovaps  xmm3, xmm0
       vmovaps  xmm10, xmm2
       vcmpeqps xmm11, xmm3, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm3, xmm3
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm3
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm3, xmm10, xmm3, xmm11
       vcmpeqps xmm10, xmm0, xmm2
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10

G_M000_IG05:                ;; offset=0x0297
       vcmpneqps xmm11, xmm0, xmm0
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm0, xmm2
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm0, xmm2, xmm0, xmm10
       vmovaps  xmm2, xmm9
       vmovaps  xmm10, xmm1
       vcmpeqps xmm11, xmm2, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm2, xmm2
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm2
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm2, xmm10, xmm2, xmm11
       vcmpeqps xmm10, xmm9, xmm1
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm9
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm9, xmm9
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm9, xmm1
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm1, xmm1, xmm9, xmm10
       vmovaps  xmm9, xmm1
       vmovaps  xmm10, xmm3
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm9
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm9, xmm10, xmm9, xmm11
       vcmpeqps xmm10, xmm1, xmm3
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm1
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm1, xmm1
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm1, xmm3
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm1, xmm3, xmm1, xmm10
       vmovaps  xmm3, xmm0
       vmovaps  xmm10, xmm4
       vcmpeqps xmm11, xmm3, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm3, xmm3
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm3
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm3, xmm10, xmm3, xmm11
       vmovaps  xmm10, xmm1
       vmovaps  xmm11, xmm3
       vcmpeqps xmm12, xmm10, xmm11
       vxorps   xmm13, xmm13, xmm13
       vpcmpgtd xmm13, xmm13, xmm11

G_M000_IG06:                ;; offset=0x03CD
       vandps   xmm12, xmm13, xmm12
       vcmpneqps xmm13, xmm10, xmm10
       vorps    xmm12, xmm13, xmm12
       vcmpltps xmm13, xmm11, xmm10
       vorps    xmm12, xmm13, xmm12
       vblendvps xmm10, xmm11, xmm10, xmm12
       vmovaps  xmm11, xmm9
       vmovaps  xmm12, xmm10
       vcmpeqps xmm13, xmm11, xmm12
       vxorps   xmm14, xmm14, xmm14
       vpcmpgtd xmm14, xmm14, xmm12
       vandps   xmm13, xmm14, xmm13
       vcmpneqps xmm14, xmm11, xmm11
       vorps    xmm13, xmm14, xmm13
       vcmpltps xmm14, xmm12, xmm11
       vorps    xmm13, xmm14, xmm13
       vblendvps xmm11, xmm12, xmm11, xmm13
       vcmpeqps xmm12, xmm0, xmm4
       vxorps   xmm13, xmm13, xmm13
       vpcmpgtd xmm13, xmm13, xmm0
       vandps   xmm12, xmm13, xmm12
       vcmpneqps xmm13, xmm0, xmm0
       vorps    xmm12, xmm13, xmm12
       vcmpltps xmm13, xmm0, xmm4
       vorps    xmm12, xmm13, xmm12
       vblendvps xmm0, xmm4, xmm0, xmm12
       vcmpeqps xmm4, xmm1, xmm3
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm1
       vandps   xmm4, xmm12, xmm4
       vcmpneqps xmm12, xmm1, xmm1
       vorps    xmm4, xmm12, xmm4
       vcmpltps xmm12, xmm1, xmm3
       vorps    xmm4, xmm12, xmm4
       vblendvps xmm1, xmm3, xmm1, xmm4
       vcmpeqps xmm3, xmm9, xmm10
       vxorps   xmm4, xmm4, xmm4
       vpcmpgtd xmm4, xmm4, xmm9
       vandps   xmm3, xmm4, xmm3
       vcmpneqps xmm4, xmm9, xmm9
       vorps    xmm3, xmm4, xmm3
       vcmpltps xmm4, xmm9, xmm10
       vorps    xmm3, xmm4, xmm3
       vblendvps xmm3, xmm10, xmm9, xmm3
       vcmpeqps xmm4, xmm2, xmm11
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm2
       vandps   xmm4, xmm9, xmm4
       vcmpneqps xmm9, xmm2, xmm2
       vorps    xmm4, xmm9, xmm4
       vcmpltps xmm9, xmm2, xmm11
       vorps    xmm4, xmm9, xmm4
       vblendvps xmm2, xmm11, xmm2, xmm4
       vmovaps  xmm4, xmm0
       vmovaps  xmm9, xmm5
       vcmpeqps xmm10, xmm4, xmm9
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm9
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm4, xmm4

G_M000_IG07:                ;; offset=0x04FB
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm9, xmm4
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm4, xmm9, xmm4, xmm10
       vmovaps  xmm9, xmm1
       vmovaps  xmm10, xmm4
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm9
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm9, xmm10, xmm9, xmm11
       vmovaps  xmm10, xmm3
       vmovaps  xmm11, xmm9
       vcmpeqps xmm12, xmm10, xmm11
       vxorps   xmm13, xmm13, xmm13
       vpcmpgtd xmm13, xmm13, xmm11
       vandps   xmm12, xmm13, xmm12
       vcmpneqps xmm13, xmm10, xmm10
       vorps    xmm12, xmm13, xmm12
       vcmpltps xmm13, xmm11, xmm10
       vorps    xmm12, xmm13, xmm12
       vblendvps xmm10, xmm11, xmm10, xmm12
       vcmpeqps xmm11, xmm0, xmm5
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm0
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm0, xmm0
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm0, xmm5
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm0, xmm5, xmm0, xmm11
       vcmpeqps xmm5, xmm1, xmm4
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm1
       vandps   xmm5, xmm11, xmm5
       vcmpneqps xmm11, xmm1, xmm1
       vorps    xmm5, xmm11, xmm5
       vcmpltps xmm11, xmm1, xmm4
       vorps    xmm5, xmm11, xmm5
       vblendvps xmm1, xmm4, xmm1, xmm5
       vcmpeqps xmm4, xmm3, xmm9
       vxorps   xmm5, xmm5, xmm5
       vpcmpgtd xmm5, xmm5, xmm3
       vandps   xmm4, xmm5, xmm4
       vcmpneqps xmm5, xmm3, xmm3
       vorps    xmm4, xmm5, xmm4
       vcmpltps xmm5, xmm3, xmm9
       vorps    xmm4, xmm5, xmm4
       vblendvps xmm3, xmm9, xmm3, xmm4
       vcmpeqps xmm4, xmm2, xmm10
       vxorps   xmm5, xmm5, xmm5
       vpcmpgtd xmm5, xmm5, xmm2
       vandps   xmm4, xmm5, xmm4
       vcmpneqps xmm5, xmm2, xmm2
       vorps    xmm4, xmm5, xmm4
       vcmpltps xmm5, xmm2, xmm10

G_M000_IG08:                ;; offset=0x0626
       vorps    xmm4, xmm5, xmm4
       vblendvps xmm2, xmm10, xmm2, xmm4
       vmovaps  xmm4, xmm0
       vmovaps  xmm5, xmm6
       vcmpeqps xmm9, xmm4, xmm5
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm5
       vandps   xmm9, xmm10, xmm9
       vcmpneqps xmm10, xmm4, xmm4
       vorps    xmm9, xmm10, xmm9
       vcmpltps xmm10, xmm5, xmm4
       vorps    xmm9, xmm10, xmm9
       vblendvps xmm4, xmm5, xmm4, xmm9
       vmovaps  xmm5, xmm1
       vmovaps  xmm9, xmm4
       vcmpeqps xmm10, xmm5, xmm9
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm9
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm5, xmm5
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm9, xmm5
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm5, xmm9, xmm5, xmm10
       vmovaps  xmm9, xmm3
       vmovaps  xmm10, xmm5
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm9
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm9, xmm10, xmm9, xmm11
       vcmpeqps xmm10, xmm0, xmm6
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm0, xmm0
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm0, xmm6
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm0, xmm6, xmm0, xmm10
       vcmpeqps xmm6, xmm1, xmm4
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm1
       vandps   xmm6, xmm10, xmm6
       vcmpneqps xmm10, xmm1, xmm1
       vorps    xmm6, xmm10, xmm6
       vcmpltps xmm10, xmm1, xmm4
       vorps    xmm6, xmm10, xmm6
       vblendvps xmm1, xmm4, xmm1, xmm6
       vcmpeqps xmm4, xmm3, xmm5
       vxorps   xmm6, xmm6, xmm6
       vpcmpgtd xmm6, xmm6, xmm3
       vandps   xmm4, xmm6, xmm4
       vcmpneqps xmm6, xmm3, xmm3
       vorps    xmm4, xmm6, xmm4
       vcmpltps xmm6, xmm3, xmm5
       vorps    xmm4, xmm6, xmm4

G_M000_IG09:                ;; offset=0x074F
       vblendvps xmm3, xmm5, xmm3, xmm4
       vcmpeqps xmm4, xmm2, xmm9
       vxorps   xmm5, xmm5, xmm5
       vpcmpgtd xmm5, xmm5, xmm2
       vandps   xmm4, xmm5, xmm4
       vcmpneqps xmm5, xmm2, xmm2
       vorps    xmm4, xmm5, xmm4
       vcmpltps xmm5, xmm2, xmm9
       vorps    xmm4, xmm5, xmm4
       vblendvps xmm2, xmm9, xmm2, xmm4
       vmovaps  xmm4, xmm0
       vmovaps  xmm5, xmm7
       vcmpeqps xmm6, xmm4, xmm5
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm5
       vandps   xmm6, xmm9, xmm6
       vcmpneqps xmm9, xmm4, xmm4
       vorps    xmm6, xmm9, xmm6
       vcmpltps xmm9, xmm5, xmm4
       vorps    xmm6, xmm9, xmm6
       vblendvps xmm4, xmm5, xmm4, xmm6
       vmovaps  xmm5, xmm1
       vmovaps  xmm6, xmm4
       vcmpeqps xmm9, xmm5, xmm6
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm6
       vandps   xmm9, xmm10, xmm9
       vcmpneqps xmm10, xmm5, xmm5
       vorps    xmm9, xmm10, xmm9
       vcmpltps xmm10, xmm6, xmm5
       vorps    xmm9, xmm10, xmm9
       vblendvps xmm5, xmm6, xmm5, xmm9
       vmovaps  xmm6, xmm3
       vmovaps  xmm9, xmm5
       vcmpeqps xmm10, xmm6, xmm9
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm9
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm6, xmm6
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm9, xmm6
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm6, xmm9, xmm6, xmm10
       vcmpeqps xmm9, xmm0, xmm7
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm0
       vandps   xmm9, xmm10, xmm9
       vcmpneqps xmm10, xmm0, xmm0
       vorps    xmm9, xmm10, xmm9
       vcmpltps xmm10, xmm0, xmm7
       vorps    xmm9, xmm10, xmm9
       vblendvps xmm0, xmm7, xmm0, xmm9
       vcmpeqps xmm7, xmm1, xmm4
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm1
       vandps   xmm7, xmm9, xmm7
       vcmpneqps xmm9, xmm1, xmm1
       vorps    xmm7, xmm9, xmm7
       vcmpltps xmm9, xmm1, xmm4
       vorps    xmm7, xmm9, xmm7
       vblendvps xmm1, xmm4, xmm1, xmm7

G_M000_IG10:                ;; offset=0x0875
       vcmpeqps xmm4, xmm3, xmm5
       vxorps   xmm7, xmm7, xmm7
       vpcmpgtd xmm7, xmm7, xmm3
       vandps   xmm4, xmm7, xmm4
       vcmpneqps xmm7, xmm3, xmm3
       vorps    xmm4, xmm7, xmm4
       vcmpltps xmm7, xmm3, xmm5
       vorps    xmm4, xmm7, xmm4
       vblendvps xmm3, xmm5, xmm3, xmm4
       vcmpeqps xmm4, xmm2, xmm6
       vxorps   xmm5, xmm5, xmm5
       vpcmpgtd xmm5, xmm5, xmm2
       vandps   xmm4, xmm5, xmm4
       vcmpneqps xmm5, xmm2, xmm2
       vorps    xmm4, xmm5, xmm4
       vcmpltps xmm5, xmm2, xmm6
       vorps    xmm4, xmm5, xmm4
       vblendvps xmm2, xmm6, xmm2, xmm4
       vmovaps  xmm4, xmm0
       vmovaps  xmm5, xmm8
       vcmpeqps xmm6, xmm4, xmm5
       vxorps   xmm7, xmm7, xmm7
       vpcmpgtd xmm7, xmm7, xmm5
       vandps   xmm6, xmm7, xmm6
       vcmpneqps xmm7, xmm4, xmm4
       vorps    xmm6, xmm7, xmm6
       vcmpltps xmm7, xmm5, xmm4
       vorps    xmm6, xmm7, xmm6
       vblendvps xmm4, xmm5, xmm4, xmm6
       vmovaps  xmm5, xmm1
       vmovaps  xmm6, xmm4
       vcmpeqps xmm7, xmm5, xmm6
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm6
       vandps   xmm7, xmm9, xmm7
       vcmpneqps xmm9, xmm5, xmm5
       vorps    xmm7, xmm9, xmm7
       vcmpltps xmm9, xmm6, xmm5
       vorps    xmm7, xmm9, xmm7
       vblendvps xmm5, xmm6, xmm5, xmm7
       vmovaps  xmm6, xmm3
       vmovaps  xmm7, xmm5
       vcmpeqps xmm9, xmm6, xmm7
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm7
       vandps   xmm9, xmm10, xmm9
       vcmpneqps xmm10, xmm6, xmm6
       vorps    xmm9, xmm10, xmm9
       vcmpltps xmm10, xmm7, xmm6
       vorps    xmm9, xmm10, xmm9
       vblendvps xmm6, xmm7, xmm6, xmm9
       vcmpeqps xmm7, xmm0, xmm8
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm0
       vandps   xmm7, xmm9, xmm7
       vcmpneqps xmm9, xmm0, xmm0
       vorps    xmm7, xmm9, xmm7
       vcmpltps xmm9, xmm0, xmm8
       vorps    xmm7, xmm9, xmm7
       vblendvps xmm0, xmm8, xmm0, xmm7
       vcmpeqps xmm7, xmm1, xmm4

G_M000_IG11:                ;; offset=0x0991
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm1
       vandps   xmm7, xmm8, xmm7
       vcmpneqps xmm8, xmm1, xmm1
       vorps    xmm7, xmm8, xmm7
       vcmpltps xmm8, xmm1, xmm4
       vorps    xmm7, xmm8, xmm7
       vblendvps xmm1, xmm4, xmm1, xmm7
       vcmpeqps xmm4, xmm3, xmm5
       vxorps   xmm7, xmm7, xmm7
       vpcmpgtd xmm7, xmm7, xmm3
       vandps   xmm4, xmm7, xmm4
       vcmpneqps xmm7, xmm3, xmm3
       vorps    xmm4, xmm7, xmm4
       vcmpltps xmm7, xmm3, xmm5
       vorps    xmm4, xmm7, xmm4
       vblendvps xmm3, xmm5, xmm3, xmm4
       vcmpeqps xmm4, xmm2, xmm6
       vxorps   xmm5, xmm5, xmm5
       vpcmpgtd xmm5, xmm5, xmm2
       vandps   xmm4, xmm5, xmm4
       vcmpneqps xmm5, xmm2, xmm2
       vorps    xmm4, xmm5, xmm4
       vcmpltps xmm5, xmm2, xmm6
       vorps    xmm4, xmm5, xmm4
       vblendvps xmm2, xmm6, xmm2, xmm4
       vmovaps  xmm4, xmm1
       vmovaps  xmm1, xmm2
       vmovaps  xmm2, xmm3
       jmp      G_M000_IG20

G_M000_IG12:                ;; offset=0x0A19
       vmovaps  xmm9, xmm0
       vmovaps  xmm10, xmm3
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm9
       vorps    xmm11, xmm13, xmm11
       vandps   xmm9, xmm9, xmm11
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm9, xmm10, xmm9
       vcmpeqps xmm10, xmm0, xmm3
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm0, xmm3
       vorps    xmm10, xmm11, xmm10
       vandps   xmm0, xmm10, xmm0
       vandnps  xmm3, xmm10, xmm3
       vorps    xmm0, xmm3, xmm0
       vmovaps  xmm3, xmm1
       vmovaps  xmm10, xmm7
       vcmpeqps xmm11, xmm3, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm3, xmm3
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm3
       vorps    xmm11, xmm13, xmm11
       vandps   xmm3, xmm11, xmm3
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm3, xmm10, xmm3
       vcmpeqps xmm10, xmm1, xmm7
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm1
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm1, xmm7
       vorps    xmm10, xmm11, xmm10
       vandps   xmm1, xmm10, xmm1
       vandnps  xmm7, xmm10, xmm7
       vorps    xmm1, xmm7, xmm1
       vmovaps  xmm7, xmm2
       vmovaps  xmm10, xmm5
       vcmpeqps xmm11, xmm7, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm7, xmm7
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm7
       vorps    xmm11, xmm13, xmm11
       vandps   xmm7, xmm11, xmm7
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm7, xmm10, xmm7
       vcmpeqps xmm10, xmm2, xmm5
       vxorps   xmm11, xmm11, xmm11

G_M000_IG13:                ;; offset=0x0B3D
       vpcmpgtd xmm11, xmm11, xmm2
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm2, xmm5
       vorps    xmm10, xmm11, xmm10
       vandps   xmm2, xmm10, xmm2
       vandnps  xmm5, xmm10, xmm5
       vorps    xmm2, xmm5, xmm2
       vmovaps  xmm5, xmm4
       vmovaps  xmm10, xmm8
       vcmpeqps xmm11, xmm5, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm5, xmm5
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm5
       vorps    xmm11, xmm13, xmm11
       vandps   xmm5, xmm11, xmm5
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm5, xmm10, xmm5
       vcmpeqps xmm10, xmm4, xmm8
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm4
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm4, xmm8
       vorps    xmm10, xmm11, xmm10
       vandps   xmm4, xmm10, xmm4
       vandnps  xmm8, xmm10, xmm8
       vorps    xmm4, xmm8, xmm4
       vmovaps  xmm8, xmm0
       vmovaps  xmm10, xmm3
       vcmpeqps xmm11, xmm8, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm8, xmm8
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm8
       vorps    xmm11, xmm13, xmm11
       vandps   xmm8, xmm8, xmm11
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm8, xmm10, xmm8
       vcmpeqps xmm10, xmm0, xmm3
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm0, xmm3
       vorps    xmm10, xmm11, xmm10
       vandps   xmm0, xmm10, xmm0
       vandnps  xmm3, xmm10, xmm3
       vorps    xmm0, xmm3, xmm0
       vmovaps  xmm3, xmm2
       vmovaps  xmm10, xmm4
       vcmpeqps xmm11, xmm3, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm3, xmm3

G_M000_IG14:                ;; offset=0x0C63
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm3
       vorps    xmm11, xmm13, xmm11
       vandps   xmm3, xmm11, xmm3
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm3, xmm10, xmm3
       vcmpeqps xmm10, xmm2, xmm4
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm2
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm2, xmm4
       vorps    xmm10, xmm11, xmm10
       vandps   xmm2, xmm10, xmm2
       vandnps  xmm4, xmm10, xmm4
       vorps    xmm2, xmm4, xmm2
       vmovaps  xmm4, xmm9
       vmovaps  xmm10, xmm5
       vcmpeqps xmm11, xmm4, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm4, xmm4
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm4
       vorps    xmm11, xmm13, xmm11
       vandps   xmm4, xmm11, xmm4
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm4, xmm10, xmm4
       vcmpeqps xmm10, xmm9, xmm5
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm9
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm9, xmm5
       vorps    xmm10, xmm11, xmm10
       vandps   xmm9, xmm9, xmm10
       vandnps  xmm5, xmm10, xmm5
       vorps    xmm5, xmm9, xmm5
       vmovaps  xmm9, xmm7
       vmovaps  xmm10, xmm6
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm9
       vorps    xmm11, xmm13, xmm11
       vandps   xmm9, xmm9, xmm11
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm9, xmm10, xmm9
       vcmpeqps xmm10, xmm7, xmm6
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm7
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm7, xmm6
       vorps    xmm10, xmm11, xmm10
       vandps   xmm7, xmm10, xmm7

G_M000_IG15:                ;; offset=0x0D84
       vandnps  xmm6, xmm10, xmm6
       vorps    xmm6, xmm6, xmm7
       vmovaps  xmm7, xmm0
       vmovaps  xmm10, xmm2
       vcmpeqps xmm11, xmm7, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm7, xmm7
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm7
       vorps    xmm11, xmm13, xmm11
       vandps   xmm7, xmm11, xmm7
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm7, xmm10, xmm7
       vcmpeqps xmm10, xmm0, xmm2
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm0, xmm2
       vorps    xmm10, xmm11, xmm10
       vandps   xmm0, xmm10, xmm0
       vandnps  xmm2, xmm10, xmm2
       vorps    xmm0, xmm2, xmm0
       vmovaps  xmm2, xmm1
       vmovaps  xmm10, xmm5
       vcmpeqps xmm11, xmm2, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm2, xmm2
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm2
       vorps    xmm11, xmm13, xmm11
       vandps   xmm2, xmm11, xmm2
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm2, xmm10, xmm2
       vcmpeqps xmm10, xmm1, xmm5
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm1
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm1, xmm5
       vorps    xmm10, xmm11, xmm10
       vandps   xmm1, xmm10, xmm1
       vandnps  xmm5, xmm10, xmm5
       vorps    xmm1, xmm5, xmm1
       vmovaps  xmm5, xmm3
       vmovaps  xmm10, xmm6
       vcmpeqps xmm11, xmm5, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm5, xmm5
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm5
       vorps    xmm11, xmm13, xmm11
       vandps   xmm5, xmm11, xmm5
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm5, xmm10, xmm5

G_M000_IG16:                ;; offset=0x0EA2
       vcmpeqps xmm10, xmm3, xmm6
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm3
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm3, xmm6
       vorps    xmm10, xmm11, xmm10
       vandps   xmm3, xmm10, xmm3
       vandnps  xmm6, xmm10, xmm6
       vorps    xmm3, xmm6, xmm3
       vcmpeqps xmm6, xmm8, xmm4
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm8
       vandps   xmm6, xmm10, xmm6
       vcmpneqps xmm10, xmm8, xmm8
       vorps    xmm6, xmm10, xmm6
       vcmpltps xmm10, xmm8, xmm4
       vorps    xmm6, xmm10, xmm6
       vblendvps xmm4, xmm4, xmm8, xmm6
       vmovaps  xmm6, xmm1
       vmovaps  xmm8, xmm3
       vcmpeqps xmm10, xmm6, xmm8
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm8
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm6, xmm6
       vorps    xmm10, xmm10, xmm11
       vcmpltps xmm12, xmm8, xmm6
       vorps    xmm10, xmm12, xmm10
       vandps   xmm6, xmm10, xmm6
       vandnps  xmm8, xmm10, xmm8
       vorps    xmm6, xmm8, xmm6
       vcmpeqps xmm8, xmm1, xmm3
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm1
       vandps   xmm8, xmm10, xmm8
       vorps    xmm8, xmm8, xmm11
       vcmpltps xmm10, xmm1, xmm3
       vorps    xmm8, xmm10, xmm8
       vandps   xmm1, xmm8, xmm1
       vandnps  xmm3, xmm8, xmm3
       vorps    xmm1, xmm3, xmm1
       vcmpeqps xmm3, xmm2, xmm9
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm2
       vandps   xmm3, xmm8, xmm3
       vcmpneqps xmm8, xmm2, xmm2
       vorps    xmm3, xmm8, xmm3
       vcmpltps xmm8, xmm2, xmm9
       vorps    xmm3, xmm8, xmm3
       vblendvps xmm2, xmm9, xmm2, xmm3
       vcmpeqps xmm3, xmm5, xmm4
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm5
       vandps   xmm3, xmm8, xmm3
       vcmpneqps xmm8, xmm5, xmm5
       vorps    xmm3, xmm8, xmm3
       vcmpltps xmm8, xmm5, xmm4
       vorps    xmm3, xmm8, xmm3

G_M000_IG17:                ;; offset=0x0FB8
       vblendvps xmm3, xmm4, xmm5, xmm3
       vmovaps  xmm4, xmm0
       vmovaps  xmm5, xmm1
       vcmpeqps xmm8, xmm4, xmm5
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm5
       vandps   xmm8, xmm9, xmm8
       vcmpneqps xmm9, xmm4, xmm4
       vorps    xmm8, xmm8, xmm9
       vcmpltps xmm10, xmm5, xmm4
       vorps    xmm8, xmm10, xmm8
       vandps   xmm4, xmm8, xmm4
       vandnps  xmm5, xmm8, xmm5
       vorps    xmm4, xmm5, xmm4
       vcmpeqps xmm5, xmm0, xmm1
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm0
       vandps   xmm5, xmm8, xmm5
       vorps    xmm5, xmm9, xmm5
       vcmpltps xmm8, xmm0, xmm1
       vorps    xmm5, xmm8, xmm5
       vandps   xmm0, xmm0, xmm5
       vandnps  xmm1, xmm5, xmm1
       vorps    xmm0, xmm1, xmm0
       vmovaps  xmm1, xmm7
       vmovaps  xmm5, xmm6
       vcmpeqps xmm8, xmm1, xmm5
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm5
       vandps   xmm8, xmm9, xmm8
       vcmpneqps xmm9, xmm1, xmm1
       vorps    xmm8, xmm8, xmm9
       vcmpltps xmm10, xmm5, xmm1
       vorps    xmm8, xmm10, xmm8
       vandps   xmm1, xmm8, xmm1
       vandnps  xmm5, xmm8, xmm5
       vorps    xmm1, xmm5, xmm1
       vcmpeqps xmm5, xmm7, xmm6
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm7
       vandps   xmm5, xmm8, xmm5
       vorps    xmm5, xmm9, xmm5
       vcmpltps xmm8, xmm7, xmm6
       vorps    xmm5, xmm8, xmm5
       vandps   xmm7, xmm7, xmm5
       vandnps  xmm5, xmm5, xmm6
       vorps    xmm5, xmm5, xmm7
       vmovaps  xmm6, xmm2
       vmovaps  xmm7, xmm3
       vcmpeqps xmm8, xmm6, xmm7
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm7
       vandps   xmm8, xmm9, xmm8
       vcmpneqps xmm9, xmm6, xmm6
       vorps    xmm8, xmm8, xmm9
       vcmpltps xmm10, xmm7, xmm6
       vorps    xmm8, xmm10, xmm8
       vandps   xmm6, xmm8, xmm6
       vandnps  xmm7, xmm8, xmm7
       vorps    xmm6, xmm7, xmm6
       vcmpeqps xmm7, xmm2, xmm3

G_M000_IG18:                ;; offset=0x10CA
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm2
       vandps   xmm7, xmm8, xmm7
       vorps    xmm7, xmm9, xmm7
       vcmpltps xmm8, xmm2, xmm3
       vorps    xmm7, xmm8, xmm7
       vandps   xmm2, xmm2, xmm7
       vandnps  xmm3, xmm7, xmm3
       vorps    xmm2, xmm3, xmm2
       vmovaps  xmm3, xmm5
       vmovaps  xmm7, xmm2
       vcmpeqps xmm8, xmm3, xmm7
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm7
       vandps   xmm8, xmm9, xmm8
       vcmpneqps xmm9, xmm3, xmm3
       vorps    xmm8, xmm8, xmm9
       vcmpltps xmm10, xmm7, xmm3
       vorps    xmm8, xmm10, xmm8
       vandps   xmm3, xmm8, xmm3
       vandnps  xmm7, xmm8, xmm7
       vorps    xmm3, xmm7, xmm3
       vcmpeqps xmm7, xmm5, xmm2
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm5
       vandps   xmm7, xmm8, xmm7
       vorps    xmm7, xmm9, xmm7
       vcmpltps xmm8, xmm5, xmm2
       vorps    xmm7, xmm8, xmm7
       vandps   xmm5, xmm5, xmm7
       vandnps  xmm2, xmm7, xmm2
       vorps    xmm5, xmm2, xmm5
       vcmpeqps xmm2, xmm1, xmm6
       vxorps   xmm7, xmm7, xmm7
       vpcmpgtd xmm7, xmm7, xmm1
       vandps   xmm2, xmm7, xmm2
       vcmpneqps xmm7, xmm1, xmm1
       vorps    xmm2, xmm7, xmm2
       vcmpltps xmm7, xmm1, xmm6
       vorps    xmm2, xmm7, xmm2
       vblendvps xmm1, xmm6, xmm1, xmm2
       vmovaps  xmm2, xmm4
       vmovaps  xmm6, xmm5
       vcmpeqps xmm7, xmm2, xmm6
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm6
       vandps   xmm7, xmm8, xmm7
       vcmpneqps xmm8, xmm2, xmm2
       vorps    xmm7, xmm8, xmm7
       vcmpltps xmm9, xmm6, xmm2
       vorps    xmm7, xmm9, xmm7
       vandps   xmm2, xmm2, xmm7
       vandnps  xmm6, xmm7, xmm6
       vorps    xmm2, xmm6, xmm2
       vcmpeqps xmm6, xmm4, xmm5
       vxorps   xmm7, xmm7, xmm7
       vpcmpgtd xmm7, xmm7, xmm4
       vandps   xmm6, xmm7, xmm6
       vorps    xmm6, xmm8, xmm6
       vcmpltps xmm7, xmm4, xmm5

G_M000_IG19:                ;; offset=0x11D1
       vorps    xmm6, xmm7, xmm6
       vandps   xmm4, xmm4, xmm6
       vandnps  xmm5, xmm6, xmm5
       vorps    xmm4, xmm5, xmm4
       vcmpeqps xmm5, xmm3, xmm1
       vxorps   xmm6, xmm6, xmm6
       vpcmpgtd xmm6, xmm6, xmm3
       vandps   xmm5, xmm6, xmm5
       vcmpneqps xmm6, xmm3, xmm3
       vorps    xmm5, xmm6, xmm5
       vcmpltps xmm6, xmm3, xmm1
       vorps    xmm5, xmm6, xmm5
       vblendvps xmm1, xmm1, xmm3, xmm5

G_M000_IG20:                ;; offset=0x120A
       mov      rax, gword ptr [rcx+0x28]
       lea      ecx, [r8+r9]
       cmp      ecx, dword ptr [rax+0x08]
       jae      G_M000_IG22
       add      r8d, r9d
       mov      ecx, r8d
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD00]
       vmulss   xmm3, xmm4, dword ptr [reloc @RWD04]
       vaddss   xmm0, xmm0, xmm3
       vmulss   xmm2, xmm2, dword ptr [reloc @RWD08]
       vmulss   xmm1, xmm1, dword ptr [reloc @RWD12]
       vaddss   xmm1, xmm2, xmm1
       vaddss   xmm0, xmm0, xmm1
       vmovss   dword ptr [rax+4*rcx+0x10], xmm0

G_M000_IG21:                ;; offset=0x1253
       vmovaps  xmm6, xmmword ptr [rsp+0xA0]
       vmovaps  xmm7, xmmword ptr [rsp+0x90]
       vmovaps  xmm8, xmmword ptr [rsp+0x80]
       vmovaps  xmm9, xmmword ptr [rsp+0x70]
       vmovaps  xmm10, xmmword ptr [rsp+0x60]
       vmovaps  xmm11, xmmword ptr [rsp+0x50]
       vmovaps  xmm12, xmmword ptr [rsp+0x40]
       vmovaps  xmm13, xmmword ptr [rsp+0x30]
       vmovaps  xmm14, xmmword ptr [rsp+0x20]
       add      rsp, 176
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r14
       ret

G_M000_IG22:                ;; offset=0x12A0
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	3E000000h		;     0.125
RWD04  	dd	3D99999Ah		;     0.075
RWD08  	dd	3D75C28Fh		;      0.06
RWD12  	dd	3D4CCCCDh		;      0.05

; Total bytes of code 4774

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ComputeMask(float):float (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000

G_M000_IG02:                ;; offset=0x0000
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD00]
       vbroadcastss xmm1, dword ptr [reloc @RWD04]
       vmaxss   xmm0, xmm1, xmm0
       vmulss   xmm1, xmm0, xmm0
       vaddss   xmm2, xmm1, dword ptr [reloc @RWD08]
       vmovss   xmm3, dword ptr [reloc @RWD12]
       vdivss   xmm2, xmm3, xmm2
       vmulss   xmm2, xmm2, dword ptr [reloc @RWD16]
       vaddss   xmm1, xmm1, dword ptr [reloc @RWD20]
       vdivss   xmm1, xmm3, xmm1
       vmulss   xmm1, xmm1, dword ptr [reloc @RWD24]
       vaddss   xmm0, xmm0, dword ptr [reloc @RWD28]
       vdivss   xmm0, xmm3, xmm0
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD32]
       vaddss   xmm0, xmm1, xmm0
       vaddss   xmm0, xmm2, xmm0
       vaddss   xmm0, xmm0, dword ptr [reloc @RWD36]

G_M000_IG03:                ;; offset=0x006D
       ret

RWD00  	dd	3F3F62FEh		;  0.747604
RWD04  	dd	3A83126Fh		;     0.001
RWD08  	dd	3F0C5322h		;  0.548144
RWD12  	dd	3F800000h		;         1
RWD16  	dd	404F0F94h		;   3.23533
RWD20  	dd	400C5322h		;   2.19257
RWD24  	dd	40A0B47Bh		;   5.02203
RWD28  	dd	4398852Ah		;    305.04
RWD32  	dd	414E7F18h		;    12.906
RWD36  	dd	BF3DE353h		;  -0.74175

; Total bytes of code 110

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ModulateScalar(System.ReadOnlySpan`1[float],int,float):float (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 0 single block inlinees; 2 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbx
       sub      rsp, 144
       vmovaps  xmmword ptr [rsp+0x80], xmm6
       vmovaps  xmmword ptr [rsp+0x70], xmm7
       vmovaps  xmmword ptr [rsp+0x60], xmm8
       vmovaps  xmmword ptr [rsp+0x50], xmm9
       vmovaps  xmmword ptr [rsp+0x40], xmm10
       vmovaps  xmmword ptr [rsp+0x30], xmm11
       vmovaps  xmmword ptr [rsp+0x20], xmm12
       vmovaps  xmm6, xmm2

G_M000_IG02:                ;; offset=0x0039
       mov      rax, bword ptr [rcx]
       mov      ecx, dword ptr [rcx+0x08]
       vxorps   xmm7, xmm7, xmm7
       vxorps   xmm0, xmm0, xmm0
       xor      r8d, r8d
       vmovss   xmm1, dword ptr [reloc @RWD00]
       vmovss   xmm2, dword ptr [reloc @RWD04]
       vmovss   xmm3, dword ptr [reloc @RWD08]
       vmovss   xmm4, dword ptr [reloc @RWD12]
       vmovss   xmm5, dword ptr [reloc @RWD16]

G_M000_IG03:                ;; offset=0x0072
       mov      r10d, r8d
       imul     r10d, edx
       cmp      r8d, 7
       jne      SHORT G_M000_IG05

G_M000_IG04:                ;; offset=0x007F
       mov      r9d, r10d
       jmp      SHORT G_M000_IG06
       align    [0 bytes for IG07]

G_M000_IG05:                ;; offset=0x0084
       lea      r9d, [r10+rdx]

G_M000_IG06:                ;; offset=0x0088
       cmp      r10d, ecx
       jae      G_M000_IG11
       mov      r11d, r10d
       vmovss   xmm8, dword ptr [rax+4*r11]
       xor      r11d, r11d

G_M000_IG07:                ;; offset=0x009D
       lea      ebx, [r10+r11+0x01]
       cmp      ebx, ecx
       jae      G_M000_IG11
       vmovss   xmm9, dword ptr [rax+4*rbx]
       lea      ebx, [r9+r11]
       cmp      ebx, ecx
       jae      G_M000_IG11
       vsubss   xmm10, xmm8, dword ptr [rax+4*rbx]
       vandps   xmm10, xmm10, xmmword ptr [reloc @RWD20]
       vsubss   xmm11, xmm8, xmm9
       vandps   xmm11, xmm11, xmmword ptr [reloc @RWD36]
       vaddss   xmm7, xmm11, xmm7
       vaddss   xmm7, xmm10, xmm7
       vaddss   xmm8, xmm8, xmm1
       vxorps   xmm10, xmm10, xmm10
       vcmpneqps xmm11, xmm8, xmm8
       vorps    xmm10, xmm11, xmm10
       vxorps   xmm11, xmm11, xmm11
       vcmpgtps xmm11, xmm8, xmm11
       vorps    xmm10, xmm11, xmm10
       vandps   xmm8, xmm10, xmm8
       vmulss   xmm10, xmm8, xmm8
       vmulss   xmm8, xmm8, xmm2
       vmulss   xmm8, xmm8, xmm10
       vaddss   xmm8, xmm8, xmm3
       vmulss   xmm10, xmm10, xmm4
       vaddss   xmm10, xmm10, xmm5
       vdivss   xmm8, xmm10, xmm8
       vaddss   xmm0, xmm8, xmm0
       vmovaps  xmm8, xmm9
       inc      r11d
       cmp      r11d, 7
       jl       G_M000_IG07

G_M000_IG08:                ;; offset=0x013B
       add      r9d, 7
       cmp      r9d, ecx
       jae      G_M000_IG11
       mov      r10d, r9d
       vsubss   xmm9, xmm8, dword ptr [rax+4*r10]
       vandps   xmm9, xmm9, xmmword ptr [reloc @RWD52]
       vaddss   xmm7, xmm9, xmm7
       vaddss   xmm8, xmm8, xmm1
       vxorps   xmm9, xmm9, xmm9
       vcmpneqps xmm10, xmm8, xmm8
       vorps    xmm9, xmm10, xmm9
       vxorps   xmm10, xmm10, xmm10
       vcmpgtps xmm10, xmm8, xmm10
       vorps    xmm9, xmm10, xmm9
       vandps   xmm8, xmm9, xmm8
       vmulss   xmm9, xmm8, xmm8
       vmulss   xmm8, xmm8, xmm2
       vmulss   xmm8, xmm8, xmm9
       vaddss   xmm8, xmm8, xmm3
       vmulss   xmm9, xmm9, xmm4
       vaddss   xmm9, xmm9, xmm5
       vdivss   xmm8, xmm9, xmm8
       vaddss   xmm0, xmm8, xmm0
       inc      r8d
       cmp      r8d, 8
       jl       G_M000_IG03

G_M000_IG09:                ;; offset=0x01B6
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD68]
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:FastLog2(float):float]
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD72]
       vmulss   xmm1, xmm7, dword ptr [reloc @RWD76]
       vaddss   xmm1, xmm1, xmm6
       vaddss   xmm0, xmm0, xmm1

G_M000_IG10:                ;; offset=0x01DC
       vmovaps  xmm6, xmmword ptr [rsp+0x80]
       vmovaps  xmm7, xmmword ptr [rsp+0x70]
       vmovaps  xmm8, xmmword ptr [rsp+0x60]
       vmovaps  xmm9, xmmword ptr [rsp+0x50]
       vmovaps  xmm10, xmmword ptr [rsp+0x40]
       vmovaps  xmm11, xmmword ptr [rsp+0x30]
       vmovaps  xmm12, xmmword ptr [rsp+0x20]
       add      rsp, 144
       pop      rbx
       ret

G_M000_IG11:                ;; offset=0x0212
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	42233332h		;      40.8
RWD04  	dd	3B1DEA78h		; 0.00240961
RWD08  	dd	449E380Dh		;   1265.75
RWD12  	dd	42EF0C23h		;   119.524
RWD16  	dd	44228FFEh		;    650.25
RWD20  	dq	7FFFFFFF7FFFFFFFh, 7FFFFFFF7FFFFFFFh
RWD36  	dq	7FFFFFFF7FFFFFFFh, 7FFFFFFF7FFFFFFFh
RWD52  	dq	7FFFFFFF7FFFFFFFh, 7FFFFFFF7FFFFFFFh
RWD68  	dd	38808081h		; 6.12745e-05
RWD72  	dd	BDDC6A08h		; -0.107624
RWD76  	dd	B8933E1Eh		; -7.02108e-05

; Total bytes of code 536

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:FastLog2(float):float (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000

G_M000_IG02:                ;; offset=0x0000
       vmovd    eax, xmm0
       lea      ecx, [rax+0xC0D55555]
       sar      ecx, 23
       mov      edx, ecx
       shl      edx, 23
       sub      eax, edx
       vmovd    xmm0, eax
       vsubss   xmm0, xmm0, dword ptr [reloc @RWD00]
       vmulss   xmm1, xmm0, dword ptr [reloc @RWD04]
       vaddss   xmm1, xmm1, dword ptr [reloc @RWD08]
       vmulss   xmm1, xmm1, xmm0
       vsubss   xmm1, xmm1, dword ptr [reloc @RWD12]
       vmulss   xmm2, xmm0, dword ptr [reloc @RWD16]
       vaddss   xmm2, xmm2, dword ptr [reloc @RWD20]
       vmulss   xmm0, xmm2, xmm0
       vaddss   xmm0, xmm0, dword ptr [reloc @RWD24]
       vdivss   xmm0, xmm1, xmm0
       vxorps   xmm1, xmm1, xmm1
       vcvtsi2ss xmm1, xmm1, ecx
       vaddss   xmm0, xmm0, xmm1

G_M000_IG03:                ;; offset=0x0068
       ret

RWD00  	dd	3F800000h		;         1
RWD04  	dd	3F3E11C7h		;  0.742459
RWD08  	dd	3FB6E02Bh		;   1.42872
RWD12  	dd	35F85AB0h		; 1.85038e-06
RWD16  	dd	3E32458Ch		;  0.174093
RWD20  	dd	3F813CEDh		;   1.00967
RWD24  	dd	3F7D8625h		;  0.990328

; Total bytes of code 105

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:FastPow2(float):float (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000

G_M000_IG02:                ;; offset=0x0000
       vroundss xmm1, xmm1, xmm0, 9
       vmovaps  xmm2, xmm1
       vcmpordss xmm3, xmm2, xmm2
       vandps   xmm3, xmm3, xmm2
       mov      eax, 0x7FFFFFFF
       vcvttss2si ecx, xmm3
       vucomiss xmm2, dword ptr [reloc @RWD00]
       cmovb    eax, ecx
       add      eax, 127
       shl      eax, 23
       vmovd    xmm2, eax
       vsubss   xmm0, xmm0, xmm1
       vaddss   xmm1, xmm0, dword ptr [reloc @RWD04]
       vmulss   xmm1, xmm1, xmm0
       vaddss   xmm1, xmm1, dword ptr [reloc @RWD08]
       vmulss   xmm1, xmm1, xmm0
       vaddss   xmm1, xmm1, dword ptr [reloc @RWD12]
       vmulss   xmm1, xmm1, xmm2
       vmulss   xmm2, xmm0, dword ptr [reloc @RWD16]
       vsubss   xmm2, xmm2, dword ptr [reloc @RWD20]
       vmulss   xmm2, xmm2, xmm0
       vsubss   xmm2, xmm2, dword ptr [reloc @RWD24]
       vmulss   xmm0, xmm2, xmm0
       vaddss   xmm0, xmm0, dword ptr [reloc @RWD28]
       vdivss   xmm0, xmm1, xmm0

G_M000_IG03:                ;; offset=0x0085
       ret

RWD00  	dd	4F000000h		; 2.14748e+09
RWD04  	dd	4122CC6Bh		;   10.1749
RWD08  	dd	424379A1h		;   48.8688
RWD12  	dd	42C519F0h		;   98.5507
RWD16  	dd	3E5749EEh		;  0.210243
RWD20  	dd	3CB621BEh		; 0.0222329
RWD24  	dd	419B8831h		;   19.4415
RWD28  	dd	42C519F1h		;   98.5507

; Total bytes of code 134

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ComputeRowCore(DotJpegli.Internal.PixelSource,int,int,int,System.Span`1[float],bool,bool,System.Threading.CancellationToken):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rbp based frame
; partially interruptible
; No PGO data
; 9 inlinees with PGO data; 35 single block inlinees; 33 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbp
       push     r15
       push     r14
       push     r13
       push     r12
       push     rdi
       push     rsi
       push     rbx
       sub      rsp, 456
       vzeroupper
       vmovaps  xmmword ptr [rsp+0x1B0], xmm6
       vmovaps  xmmword ptr [rsp+0x1A0], xmm7
       vmovaps  xmmword ptr [rsp+0x190], xmm8
       vmovaps  xmmword ptr [rsp+0x180], xmm9
       vmovaps  xmmword ptr [rsp+0x170], xmm10
       vmovaps  xmmword ptr [rsp+0x160], xmm11
       vmovaps  xmmword ptr [rsp+0x150], xmm12
       vmovaps  xmmword ptr [rsp+0x140], xmm13
       lea      rbp, [rsp+0x40]
       vxorps   xmm4, xmm4, xmm4
       vmovdqu  ymmword ptr [rbp+0x60], ymm4
       vmovdqu  ymmword ptr [rbp+0x80], ymm4
       vmovdqa  xmmword ptr [rbp+0xA0], xmm4
       mov      rax, 0x6911662D01EE
       mov      qword ptr [rbp+0xF8], rax
       mov      rax, bword ptr [rbp+0x1F8]
       mov      ebx, dword ptr [rbp+0x200]

G_M000_IG02:                ;; offset=0x009A
       mov      r10, gword ptr [rbp+0x210]
       mov      gword ptr [rbp+0x60], r10
       mov      edi, r9d
       mov      dword ptr [rbp+0x68], edi
       mov      r14d, r8d
       mov      dword ptr [rbp+0x6C], r14d
       mov      r15, rdx
       mov      r13, rcx
       mov      r12, bword ptr [rax]
       mov      bword ptr [rbp+0x18], r12
       mov      r10d, dword ptr [rax+0x08]
       mov      dword ptr [rbp+0xB4], r10d
       cmp      byte  ptr [r13+0x61], 0
       jne      G_M000_IG132
       mov      r9d, dword ptr [r13+0x40]
       shl      r9d, 3
       mov      dword ptr [rbp+0xF4], r9d
       mov      r11, gword ptr [r15+0x08]
       mov      gword ptr [rbp+0x50], r11
       test     r11, r11
       je       SHORT G_M000_IG08

G_M000_IG03:                ;; offset=0x00F1
       cmp      r11, gword ptr [r13+0x30]
       je       SHORT G_M000_IG05

G_M000_IG04:                ;; offset=0x00F7
       mov      r11, gword ptr [rbp+0x50]
       jmp      SHORT G_M000_IG08

G_M000_IG05:                ;; offset=0x00FD
       mov      eax, r9d
       add      eax, dword ptr [r13+0x54]
       cmp      eax, edi
       je       SHORT G_M000_IG07

G_M000_IG06:                ;; offset=0x0108
       mov      r11, gword ptr [rbp+0x50]
       jmp      SHORT G_M000_IG08

G_M000_IG07:                ;; offset=0x010E
       cmp      r14d, dword ptr [r13+0x58]
       mov      r11, gword ptr [rbp+0x50]
       mov      r14d, dword ptr [rbp+0x6C]
       je       SHORT G_M000_IG09

G_M000_IG08:                ;; offset=0x011C
       xor      ecx, ecx
       jmp      SHORT G_M000_IG10

G_M000_IG09:                ;; offset=0x0120
       movzx    r8, bl
       movzx    rcx, byte  ptr [r13+0x62]
       cmp      r8d, ecx
       sete     cl
       movzx    rcx, cl

G_M000_IG10:                ;; offset=0x0132
       mov      dword ptr [rbp+0xF0], ecx
       xor      rax, rax
       mov      gword ptr [r13+0x30], rax
       cmp      gword ptr [rbp+0x60], 0
       jne      G_M000_IG129

G_M000_IG11:                ;; offset=0x0149
       test     edi, edi
       jl       G_M000_IG134
       cmp      edi, dword ptr [r13+0x3C]
       jge      G_M000_IG134
       mov      eax, edi
       cdq
       idiv     edx:eax, r9d
       test     edx, edx
       jne      G_M000_IG134
       mov      r8d, dword ptr [r13+0x5C]
       imul     r8d, dword ptr [r13+0x40]
       cmp      r8d, r10d
       jg       G_M000_IG135
       lea      eax, [rdi-0x05]
       mov      dword ptr [rbp+0xEC], eax
       xor      edx, edx
       test     ecx, ecx
       je       G_M000_IG130

G_M000_IG12:                ;; offset=0x018E
       mov      r8, gword ptr [r13+0x10]
       mov      gword ptr [rbp+0x10], r8
       mov      rdx, r8
       mov      r8d, dword ptr [r13+0x4C]
       mov      dword ptr [rbp+0x78], r8d
       imul     r8d, r9d
       mov      ecx, dword ptr [rbp+0x78]
       lea      ecx, [rcx+4*rcx]
       add      ecx, ecx
       test     rdx, rdx
       jne      G_M000_IG131
       or       r8d, ecx
       jne      G_M000_IG136
       xor      rdx, rdx
       mov      bword ptr [rbp+0x38], rdx
       xor      ecx, ecx

G_M000_IG13:                ;; offset=0x01C7
       mov      r8, gword ptr [rbp+0x10]
       test     r8, r8
       je       SHORT G_M000_IG14
       lea      rdx, bword ptr [r8+0x10]
       mov      r8d, dword ptr [r8+0x08]
       mov      bword ptr [rbp+0x30], rdx
       mov      edx, r8d
       jmp      SHORT G_M000_IG15

G_M000_IG14:                ;; offset=0x01E1
       xor      r8, r8
       mov      bword ptr [rbp+0x30], r8
       xor      edx, edx

G_M000_IG15:                ;; offset=0x01EA
       cmp      ecx, edx
       jg       G_M000_IG137
       mov      ecx, ecx
       lea      r8, [4*rcx]
       mov      rcx, bword ptr [rbp+0x30]
       mov      rdx, bword ptr [rbp+0x38]
       call     [System.SpanHelpers:Memmove(byref,byref,nuint)]
       mov      eax, 10
       mov      r10d, dword ptr [rbp+0xEC]
       lea      r8d, [r10+0x09]
       mov      edx, dword ptr [r13+0x3C]
       dec      edx
       js       G_M000_IG138
       test     r8d, r8d
       jl       SHORT G_M000_IG16
       cmp      r8d, edx
       cmovg    r8d, edx
       jmp      SHORT G_M000_IG17

G_M000_IG16:                ;; offset=0x0234
       xor      r8d, r8d

G_M000_IG17:                ;; offset=0x0237
       mov      edx, eax
       mov      r9d, dword ptr [rbp+0xF4]
       mov      r11, gword ptr [rbp+0x50]

G_M000_IG18:                ;; offset=0x0244
       jmp      G_M000_IG39

G_M000_IG19:                ;; offset=0x0249
       mov      rcx, gword ptr [rbp+0x60]
       cmp      dword ptr [rcx+0x20], 0
       je       G_M000_IG41
       jmp      G_M000_IG133

G_M000_IG20:                ;; offset=0x025C
       xor      eax, eax
       mov      qword ptr [rbp+0x70], r9

G_M000_IG21:                ;; offset=0x0262
       imul     eax, dword ptr [r15+0x20]
       lea      edx, [rax+rbx]
       mov      rcx, r15
       mov      r8d, dword ptr [rbp+0xC4]
       call     [DotJpegli.Internal.PixelSource:ReadSample(int,int):float:this]
       mov      rdx, qword ptr [rbp+0x70]
       vmovss   dword ptr [rdi+4*rdx], xmm0
       inc      edx
       mov      r8d, dword ptr [rbp+0xB8]
       cmp      edx, r8d
       mov      r9, rdx
       mov      ecx, dword ptr [rbp+0xC4]
       jge      G_M000_IG35

G_M000_IG22:                ;; offset=0x029E
       mov      r8d, dword ptr [rbp+0xCC]

G_M000_IG23:                ;; offset=0x02A5
       lea      eax, [r8-0x01]
       test     eax, eax
       jl       G_M000_IG140
       mov      r8d, dword ptr [rbp+0xCC]
       test     r9d, r9d
       jl       SHORT G_M000_IG20

G_M000_IG24:                ;; offset=0x02BD
       cmp      r9d, eax
       jg       G_M000_IG57
       mov      qword ptr [rbp+0x70], r9
       mov      eax, r9d
       jmp      SHORT G_M000_IG21

G_M000_IG25:                ;; offset=0x02CF
       xor      eax, eax
       mov      dword ptr [rsp+0x20], eax
       mov      dword ptr [rsp+0x28], esi
       mov      rbx, bword ptr [rbp+0x28]
       mov      bword ptr [rbp+0xA0], rbx
       mov      dword ptr [rbp+0xA8], r9d
       lea      r9, [rbp+0xA0]
       mov      qword ptr [rsp+0x30], r9
       mov      edx, r8d
       mov      r8d, dword ptr [rbp+0xC8]
       mov      r9d, ecx
       mov      rcx, r15
       call     [DotJpegli.Internal.PixelSource:ReadConvertedRow(int,int,int,int,int,System.Span`1[float]):this]
       jmp      G_M000_IG35

G_M000_IG26:                ;; offset=0x0312
       mov      r9d, dword ptr [r13+0x38]
       mov      r8d, r9d
       mov      ecx, r11d
       mov      eax, dword ptr [r15+0x1C]
       test     eax, eax
       jne      SHORT G_M000_IG29
       cmp      byte  ptr [r15+0x26], 0
       je       SHORT G_M000_IG27
       mov      r11d, 4
       jmp      SHORT G_M000_IG30

G_M000_IG27:                ;; offset=0x0333
       cmp      dword ptr [r15+0x20], 1
       jne      SHORT G_M000_IG28
       mov      r11d, 1
       jmp      SHORT G_M000_IG30

G_M000_IG28:                ;; offset=0x0342
       mov      r11d, 2
       jmp      SHORT G_M000_IG30

G_M000_IG29:                ;; offset=0x034A
       mov      r11d, eax

G_M000_IG30:                ;; offset=0x034D
       cmp      r11d, 3
       jne      SHORT G_M000_IG31
       xor      eax, eax
       jmp      SHORT G_M000_IG32

G_M000_IG31:                ;; offset=0x0357
       cmp      r11d, 4
       setne    al
       movzx    rax, al

G_M000_IG32:                ;; offset=0x0361
       test     eax, eax
       sete     al
       movzx    rax, al
       mov      r11, gword ptr [r13+0x10]
       test     r11, r11
       je       SHORT G_M000_IG33
       mov      ebx, r14d
       mov      edi, r9d
       add      rbx, rdi
       mov      edi, dword ptr [r11+0x08]
       cmp      rbx, rdi
       ja       G_M000_IG136
       mov      ebx, r14d
       lea      r11, bword ptr [r11+4*rbx+0x10]
       jmp      SHORT G_M000_IG34

G_M000_IG33:                ;; offset=0x0392
       mov      r11d, r14d
       or       r11d, r9d
       jne      G_M000_IG136
       xor      r11, r11
       xor      r9d, r9d

G_M000_IG34:                ;; offset=0x03A4
       mov      dword ptr [rsp+0x20], eax
       xor      eax, eax
       mov      dword ptr [rsp+0x28], eax
       mov      dword ptr [rsp+0x30], esi
       mov      bword ptr [rbp+0xA0], r11
       mov      dword ptr [rbp+0xA8], r9d
       lea      r9, [rbp+0xA0]
       mov      qword ptr [rsp+0x38], r9
       mov      edx, r8d
       mov      r8d, ecx
       mov      r9d, dword ptr [rbp+0x6C]
       mov      rcx, r15
       call     [DotJpegli.Internal.PixelSource:ReadRowVector(int,int,int,int,int,int,System.Span`1[float]):this]

G_M000_IG35:                ;; offset=0x03DF
       mov      rdx, gword ptr [r13+0x10]
       mov      rcx, rdx
       mov      rax, rdx
       mov      r8d, dword ptr [rax+0x08]
       cmp      r14d, r8d
       jae      G_M000_IG143
       mov      r10d, r14d
       vmovss   xmm2, dword ptr [rax+4*r10+0x10]
       lea      eax, [r14-0x01]
       cmp      eax, r8d
       jae      G_M000_IG143
       lea      eax, [r14-0x01]
       vmovss   dword ptr [rcx+4*rax+0x10], xmm2
       mov      rcx, rdx
       mov      eax, dword ptr [r13+0x38]
       add      r14d, eax
       mov      r10d, r14d
       mov      r9d, dword ptr [r13+0x44]
       sub      r9d, eax
       inc      r9d
       test     rcx, rcx
       je       SHORT G_M000_IG36
       mov      eax, r10d
       mov      r11d, r9d
       add      rax, r11
       mov      r11d, r8d
       cmp      rax, r11
       ja       G_M000_IG136
       lea      rcx, bword ptr [rcx+4*r10+0x10]
       jmp      SHORT G_M000_IG37

G_M000_IG36:                ;; offset=0x044F
       or       r10d, r9d
       jne      G_M000_IG136
       xor      rcx, rcx
       xor      r9d, r9d

G_M000_IG37:                ;; offset=0x045D
       dec      r14d
       cmp      r14d, r8d
       jae      G_M000_IG143
       mov      eax, r14d
       vmovss   xmm2, dword ptr [rdx+4*rax+0x10]
       mov      edx, r9d
       call     [System.SpanHelpers:Fill[float](byref,nuint,float)]
       movzx    rbx, byte  ptr [rbp+0x200]

G_M000_IG38:                ;; offset=0x0482
       mov      r8d, esi
       mov      esi, dword ptr [rbp+0xE4]
       inc      esi
       mov      edx, esi
       mov      r9d, dword ptr [rbp+0xF4]
       mov      r11, gword ptr [rbp+0x50]
       mov      r14d, dword ptr [rbp+0x6C]

G_M000_IG39:                ;; offset=0x049E
       lea      ecx, [r9+0x0A]
       cmp      edx, ecx
       jge      G_M000_IG63

G_M000_IG40:                ;; offset=0x04AA
       cmp      gword ptr [rbp+0x60], 0
       jne      G_M000_IG19

G_M000_IG41:                ;; offset=0x04B5
       mov      ecx, edx
       add      ecx, dword ptr [rbp+0xEC]
       mov      r11d, dword ptr [r13+0x3C]
       lea      eax, [r11-0x01]
       mov      esi, eax
       test     esi, esi
       jl       G_M000_IG139
       test     ecx, ecx
       jl       SHORT G_M000_IG44

G_M000_IG42:                ;; offset=0x04D3
       cmp      ecx, esi
       jg       SHORT G_M000_IG43
       mov      esi, ecx
       jmp      SHORT G_M000_IG45

G_M000_IG43:                ;; offset=0x04DB
       jmp      SHORT G_M000_IG45

G_M000_IG44:                ;; offset=0x04DD
       xor      esi, esi

G_M000_IG45:                ;; offset=0x04DF
       mov      ecx, dword ptr [r13+0x4C]
       mov      dword ptr [rbp+0xE4], edx
       mov      r9d, edx
       imul     r9d, ecx
       lea      r14d, [r9+0x01]
       cmp      esi, r8d
       je       G_M000_IG58

G_M000_IG46:                ;; offset=0x04FD
       mov      dword ptr [rbp+0x200], ebx
       movzx    r8, bl
       test     r8d, r8d
       jne      G_M000_IG26
       mov      r9d, dword ptr [r13+0x38]
       mov      r8d, r9d
       mov      dword ptr [rbp+0xCC], r8d
       mov      dword ptr [rbp+0xC8], r11d
       mov      ecx, dword ptr [r15+0x1C]
       test     ecx, ecx
       jne      SHORT G_M000_IG49
       cmp      byte  ptr [r15+0x26], 0
       je       SHORT G_M000_IG47
       mov      ecx, 4
       jmp      SHORT G_M000_IG49

G_M000_IG47:                ;; offset=0x053B
       cmp      dword ptr [r15+0x20], 1
       jne      SHORT G_M000_IG48
       mov      ecx, 1
       jmp      SHORT G_M000_IG49

G_M000_IG48:                ;; offset=0x0549
       mov      ecx, 2

G_M000_IG49:                ;; offset=0x054E
       cmp      ecx, 3
       jne      SHORT G_M000_IG50
       xor      ecx, ecx
       jmp      SHORT G_M000_IG51

G_M000_IG50:                ;; offset=0x0557
       cmp      ecx, 4
       setne    cl
       movzx    rcx, cl

G_M000_IG51:                ;; offset=0x0560
       test     ecx, ecx
       sete     cl
       movzx    rcx, cl
       mov      dword ptr [rbp+0xC4], ecx
       mov      rbx, gword ptr [r13+0x10]
       test     rbx, rbx
       je       SHORT G_M000_IG52
       mov      edi, r14d
       mov      r11d, r9d
       add      r11, rdi
       mov      edi, dword ptr [rbx+0x08]
       cmp      r11, rdi
       ja       G_M000_IG136
       mov      r11d, r14d
       lea      rbx, bword ptr [rbx+4*r11+0x10]
       jmp      SHORT G_M000_IG53

G_M000_IG52:                ;; offset=0x0596
       or       r9d, r14d
       jne      G_M000_IG136
       xor      rbx, rbx
       xor      r9d, r9d

G_M000_IG53:                ;; offset=0x05A4
       mov      bword ptr [rbp+0x28], rbx
       mov      rdi, rbx
       mov      r11d, r9d
       mov      dword ptr [rbp+0xB8], r11d
       mov      rbx, gword ptr [r15+0x08]
       test     rbx, rbx
       je       SHORT G_M000_IG54
       cmp      byte  ptr [rbx+0x2C], 0
       je       G_M000_IG25
       test     ecx, ecx
       mov      ecx, dword ptr [rbp+0xC4]
       je       G_M000_IG25

G_M000_IG54:                ;; offset=0x05D6
       cmp      esi, eax
       jg       SHORT G_M000_IG55
       mov      ebx, esi
       jmp      SHORT G_M000_IG56

G_M000_IG55:                ;; offset=0x05DE
       mov      ebx, eax

G_M000_IG56:                ;; offset=0x05E0
       mov      eax, dword ptr [rbp+0x6C]
       imul     ebx, eax
       xor      r9d, r9d
       test     r11d, r11d
       je       G_M000_IG35
       jmp      G_M000_IG23

G_M000_IG57:                ;; offset=0x05F7
       mov      qword ptr [rbp+0x70], r9
       jmp      G_M000_IG21

G_M000_IG58:                ;; offset=0x0600
       mov      r8, gword ptr [r13+0x10]
       mov      edx, dword ptr [rbp+0xE4]
       lea      eax, [rdx-0x01]
       imul     eax, ecx
       mov      r11d, ecx
       test     r8, r8
       je       SHORT G_M000_IG59
       mov      dword ptr [rbp+0x200], ebx
       mov      r14d, eax
       mov      ebx, r11d
       add      rbx, r14
       mov      r14d, dword ptr [r8+0x08]
       cmp      rbx, r14
       ja       G_M000_IG136
       lea      rax, bword ptr [r8+4*rax+0x10]
       movzx    rbx, byte  ptr [rbp+0x200]
       jmp      SHORT G_M000_IG60

G_M000_IG59:                ;; offset=0x0642
       or       eax, r11d
       jne      G_M000_IG136
       xor      rax, rax
       xor      r11d, r11d

G_M000_IG60:                ;; offset=0x0650
       mov      r8, gword ptr [r13+0x10]
       test     r8, r8
       je       SHORT G_M000_IG61
       mov      dword ptr [rbp+0x200], ebx
       mov      r14d, r9d
       mov      ebx, ecx
       add      rbx, r14
       mov      r14d, dword ptr [r8+0x08]
       cmp      rbx, r14
       ja       G_M000_IG136
       mov      r9d, r9d
       lea      r9, bword ptr [r8+4*r9+0x10]
       movzx    rbx, byte  ptr [rbp+0x200]
       jmp      SHORT G_M000_IG62

G_M000_IG61:                ;; offset=0x0685
       or       r9d, ecx
       jne      G_M000_IG136
       xor      r9, r9
       xor      ecx, ecx

G_M000_IG62:                ;; offset=0x0693
       cmp      r11d, ecx
       jg       G_M000_IG137
       mov      r8d, r11d
       shl      r8, 2
       mov      rcx, r9
       mov      rdx, rax
       call     [System.SpanHelpers:Memmove(byref,byref,nuint)]
       jmp      G_M000_IG38

G_M000_IG63:                ;; offset=0x06B4
       mov      edi, dword ptr [rbp+0x68]
       mov      r15d, edi
       shr      r15d, 2
       dec      r15d
       mov      r9d, dword ptr [r13+0x48]
       mov      edx, r9d
       sar      edx, 31
       and      edx, 3
       add      r9d, edx
       sar      r9d, 2
       mov      dword ptr [rbp+0xE8], r9d
       xor      edx, edx
       cmp      dword ptr [rbp+0xF0], 0
       jne      SHORT G_M000_IG65

G_M000_IG64:                ;; offset=0x06E7
       mov      ecx, -1
       jmp      G_M000_IG72

G_M000_IG65:                ;; offset=0x06F1
       mov      r8, gword ptr [r13+0x18]
       mov      gword ptr [rbp+0x08], r8
       mov      rcx, r8
       mov      edx, dword ptr [r13+0x40]
       add      edx, edx
       mov      r8d, dword ptr [r13+0x50]
       imul     edx, r8d
       add      r8d, r8d
       test     rcx, rcx
       je       SHORT G_M000_IG66
       mov      dword ptr [rbp+0x200], ebx
       mov      ebx, edx
       mov      qword ptr [rbp], rbx
       mov      ebx, r8d
       add      rbx, qword ptr [rbp]
       mov      qword ptr [rbp], rbx
       mov      ebx, dword ptr [rcx+0x08]
       cmp      qword ptr [rbp], rbx
       ja       G_M000_IG136
       lea      rdx, bword ptr [rcx+4*rdx+0x10]
       mov      bword ptr [rbp+0x20], rdx
       movzx    rbx, byte  ptr [rbp+0x200]
       jmp      SHORT G_M000_IG67

G_M000_IG66:                ;; offset=0x0748
       or       edx, r8d
       jne      G_M000_IG136
       xor      rdx, rdx
       mov      bword ptr [rbp+0x20], rdx
       xor      r8d, r8d

G_M000_IG67:                ;; offset=0x075A
       mov      rcx, gword ptr [rbp+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG68
       lea      rdx, bword ptr [rcx+0x10]
       mov      ecx, dword ptr [rcx+0x08]
       mov      eax, ecx
       mov      rcx, rdx
       mov      edx, eax
       jmp      SHORT G_M000_IG69

G_M000_IG68:                ;; offset=0x0773
       xor      rcx, rcx
       xor      edx, edx

G_M000_IG69:                ;; offset=0x0777
       cmp      r8d, edx
       jg       G_M000_IG137
       mov      r8d, r8d
       shl      r8, 2
       mov      rdx, bword ptr [rbp+0x20]
       call     [System.SpanHelpers:Memmove(byref,byref,nuint)]
       mov      eax, 2
       lea      ecx, [r15+0x01]
       mov      r10d, dword ptr [rbp+0xE8]
       lea      edx, [r10-0x01]
       test     edx, edx
       jl       G_M000_IG141
       test     ecx, ecx
       jl       SHORT G_M000_IG70
       cmp      ecx, edx
       cmovg    ecx, edx
       jmp      SHORT G_M000_IG71

G_M000_IG70:                ;; offset=0x07B8
       xor      ecx, ecx

G_M000_IG71:                ;; offset=0x07BA
       shl      ecx, 2
       mov      edx, eax
       mov      r11, gword ptr [rbp+0x50]

G_M000_IG72:                ;; offset=0x07C3
       mov      r8d, edx
       jmp      G_M000_IG86

G_M000_IG73:                ;; offset=0x07CB
       cmp      gword ptr [rbp+0x60], 0
       jne      G_M000_IG128

G_M000_IG74:                ;; offset=0x07D6
       lea      edx, [r15+r8]
       mov      r9d, dword ptr [rbp+0xE8]
       dec      r9d
       js       G_M000_IG142
       test     edx, edx
       jl       SHORT G_M000_IG77

G_M000_IG75:                ;; offset=0x07EE
       cmp      edx, r9d
       jg       SHORT G_M000_IG76
       jmp      SHORT G_M000_IG78

G_M000_IG76:                ;; offset=0x07F5
       mov      edx, r9d
       jmp      SHORT G_M000_IG78

G_M000_IG77:                ;; offset=0x07FA
       xor      edx, edx

G_M000_IG78:                ;; offset=0x07FC
       lea      r9d, [4*rdx]
       mov      dword ptr [rbp+0xDC], r9d
       cmp      r9d, ecx
       je       SHORT G_M000_IG80

G_M000_IG79:                ;; offset=0x0810
       mov      edx, r9d
       sub      edx, dword ptr [rbp+0xEC]
       mov      dword ptr [rbp+0x200], ebx
       movzx    rcx, bl
       mov      r9d, ecx
       mov      rcx, r13
       mov      dword ptr [rbp+0xE0], r8d
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:ComputePreErosion(int,int,bool):this]
       jmp      G_M000_IG85

G_M000_IG80:                ;; offset=0x083A
       mov      dword ptr [rbp+0x200], ebx
       mov      rcx, gword ptr [r13+0x18]
       mov      rdx, rcx
       lea      ebx, [r8-0x01]
       mov      edi, dword ptr [r13+0x50]
       imul     ebx, edi
       mov      eax, edi
       test     rdx, rdx
       je       SHORT G_M000_IG81
       mov      esi, ebx
       mov      r12d, eax
       add      rsi, r12
       mov      r12d, dword ptr [rdx+0x08]
       cmp      rsi, r12
       ja       G_M000_IG136
       lea      rdx, bword ptr [rdx+4*rbx+0x10]
       mov      r12, bword ptr [rbp+0x18]
       jmp      SHORT G_M000_IG82

G_M000_IG81:                ;; offset=0x0879
       or       ebx, eax
       jne      G_M000_IG136
       xor      rdx, rdx
       xor      eax, eax

G_M000_IG82:                ;; offset=0x0885
       mov      dword ptr [rbp+0xE0], r8d
       mov      ebx, r8d
       imul     ebx, edi
       test     rcx, rcx
       je       SHORT G_M000_IG83
       mov      esi, ebx
       mov      r12d, edi
       add      rsi, r12
       mov      r12d, dword ptr [rcx+0x08]
       cmp      rsi, r12
       ja       G_M000_IG136
       lea      rcx, bword ptr [rcx+4*rbx+0x10]
       mov      r12, bword ptr [rbp+0x18]
       jmp      SHORT G_M000_IG84

G_M000_IG83:                ;; offset=0x08B7
       or       ebx, edi
       jne      G_M000_IG136
       xor      rcx, rcx
       xor      edi, edi

G_M000_IG84:                ;; offset=0x08C3
       cmp      eax, edi
       jg       G_M000_IG137
       mov      eax, eax
       lea      r8, [4*rax]
       call     [System.SpanHelpers:Memmove(byref,byref,nuint)]

G_M000_IG85:                ;; offset=0x08DB
       mov      ecx, dword ptr [rbp+0xDC]
       mov      r8d, dword ptr [rbp+0xE0]
       inc      r8d
       movzx    rbx, byte  ptr [rbp+0x200]
       mov      r11, gword ptr [rbp+0x50]

G_M000_IG86:                ;; offset=0x08F6
       mov      edx, dword ptr [r13+0x40]
       lea      edx, [2*rdx+0x02]
       cmp      r8d, edx
       jl       G_M000_IG73

G_M000_IG87:                ;; offset=0x090A
       mov      esi, dword ptr [rbp+0x1F0]
       cmp      esi, 9
       jge      SHORT G_M000_IG89

G_M000_IG88:                ;; offset=0x0915
       vmovss   xmm6, dword ptr [reloc @RWD00]
       vmovaps  xmm0, xmm6
       jmp      SHORT G_M000_IG90

G_M000_IG89:                ;; offset=0x0923
       vxorps   xmm0, xmm0, xmm0
       vcvtsi2ss xmm0, xmm0, esi
       vsubss   xmm0, xmm0, dword ptr [reloc @RWD04]
       vdivss   xmm0, xmm0, dword ptr [reloc @RWD08]
       vmovss   xmm6, dword ptr [reloc @RWD00]
       vsubss   xmm0, xmm6, xmm0
       vxorps   xmm1, xmm1, xmm1
       vcmpeqps xmm1, xmm1, xmm0
       vxorps   xmm2, xmm2, xmm2
       vpcmpgtd xmm2, xmm2, xmm0
       vandps   xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpltps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandnps  xmm0, xmm1, xmm0

G_M000_IG90:                ;; offset=0x096D
       vmulss   xmm7, xmm0, dword ptr [reloc @RWD12]
       vsubss   xmm0, xmm6, xmm0
       vmulss   xmm8, xmm0, dword ptr [reloc @RWD16]
       mov      dword ptr [rbp+0x200], ebx
       movzx    rcx, bl
       mov      esi, ecx
       test     esi, esi
       je       SHORT G_M000_IG92

G_M000_IG91:                ;; offset=0x0990
       cmp      dword ptr [r13+0x5C], 8
       jge      SHORT G_M000_IG93

G_M000_IG92:                ;; offset=0x0997
       xor      r15d, r15d
       jmp      SHORT G_M000_IG94

G_M000_IG93:                ;; offset=0x099C
       movzx    r15, byte  ptr [rbp+0x208]
       or       r15d, 1

G_M000_IG94:                ;; offset=0x09A8
       test     r15d, r15d
       jne      SHORT G_M000_IG96

G_M000_IG95:                ;; offset=0x09AD
       xor      rax, rax
       xor      r8d, r8d
       mov      r10d, r8d
       jmp      SHORT G_M000_IG97

G_M000_IG96:                ;; offset=0x09B7
       test     dword ptr [rsp], esp
       sub      rsp, 512
       lea      rcx, [rsp+0x40]
       mov      qword ptr [rbp+0x58], rcx
       mov      edx, 512
       call     CORINFO_HELP_MEMZERO
       mov      rcx, qword ptr [rbp+0x58]
       mov      rax, rcx
       mov      r10d, 16

G_M000_IG97:                ;; offset=0x09E1
       mov      bword ptr [rbp+0x48], rax
       mov      dword ptr [rbp+0xC0], r10d
       test     r15d, r15d
       jne      SHORT G_M000_IG99

G_M000_IG98:                ;; offset=0x09F1
       xor      r11, r11
       xor      r8d, r8d
       jmp      SHORT G_M000_IG100

G_M000_IG99:                ;; offset=0x09F9
       test     dword ptr [rsp], esp
       sub      rsp, 64
       lea      r11, [rsp+0x40]
       vxorps   ymm0, ymm0, ymm0
       vmovdqu  ymmword ptr [r11], ymm0
       vmovdqu  ymmword ptr [r11+0x20], ymm0
       mov      r8d, 16

G_M000_IG100:                ;; offset=0x0A1A
       mov      bword ptr [rbp+0x40], r11
       mov      dword ptr [rbp+0xBC], r8d
       xor      ecx, ecx
       vmovss   xmm9, dword ptr [reloc @RWD20]
       vmovss   xmm10, dword ptr [reloc @RWD24]
       vmovss   xmm11, dword ptr [reloc @RWD28]
       jmp      G_M000_IG114

G_M000_IG101:                ;; offset=0x0A44
       mov      r10, bword ptr [rbp+0x48]
       mov      bword ptr [rbp+0x90], r10
       mov      r11d, dword ptr [rbp+0xC0]
       mov      dword ptr [rbp+0x98], r11d
       mov      rcx, bword ptr [rbp+0x40]
       mov      bword ptr [rbp+0xA0], rcx
       mov      edx, dword ptr [rbp+0xBC]
       mov      dword ptr [rbp+0xA8], edx
       lea      r9, [rbp+0x90]
       lea      r8, [rbp+0xA0]
       mov      qword ptr [rsp+0x20], r8
       mov      r8d, dword ptr [rbp+0xD8]
       lea      r8d, [8*r8+0x05]
       mov      dword ptr [rbp+0xD4], eax
       lea      edx, [8*rax]
       mov      rcx, r13
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:ModulateEightBlocks(int,int,System.Span`1[System.Runtime.Intrinsics.Vector256`1[float]],System.Span`1[float]):this]
       xor      eax, eax
       cmp      dword ptr [rbp+0xBC], 8
       jl       G_M000_IG106

G_M000_IG102:                ;; offset=0x0ABB
       mov      dword ptr [rbp+0xD0], eax
       mov      r8d, eax
       add      r8d, dword ptr [rbp+0xD4]
       add      r8d, r8d
       mov      r10, gword ptr [r13+0x20]
       mov      r9, r10
       mov      r11d, r8d
       add      r11d, dword ptr [r13+0x50]
       mov      ebx, r11d
       mov      edi, dword ptr [r9+0x08]
       cmp      ebx, edi
       jae      G_M000_IG143
       vmovss   xmm0, dword ptr [r9+4*rbx+0x10]
       mov      r9, r10
       inc      r11d
       cmp      r11d, edi
       jae      G_M000_IG143
       vaddss   xmm0, xmm0, dword ptr [r9+4*r11+0x10]
       mov      r11, r10
       cmp      r8d, edi
       jae      G_M000_IG143
       mov      r9d, r8d
       vaddss   xmm0, xmm0, dword ptr [r11+4*r9+0x10]
       inc      r8d
       cmp      r8d, edi
       jae      G_M000_IG143
       vaddss   xmm12, xmm0, dword ptr [r10+4*r8+0x10]
       vmovaps  xmm0, xmm12
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:ComputeMask(float):float]
       vmovaps  xmm12, xmm0
       mov      ebx, dword ptr [rbp+0xD0]
       mov      eax, ebx
       mov      rdi, bword ptr [rbp+0x40]
       vaddss   xmm12, xmm12, dword ptr [rdi+4*rax]
       lea      eax, [rbx+0x08]
       cmp      eax, dword ptr [rbp+0xBC]
       jae      G_M000_IG143
       vmovss   xmm0, dword ptr [rdi+4*rax]
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:FastLog2(float):float]
       vmulss   xmm0, xmm9, xmm0
       vaddss   xmm12, xmm12, xmm0
       vmulss   xmm0, xmm12, xmm10
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:FastPow2(float):float]
       vmulss   xmm0, xmm0, xmm7
       vaddss   xmm13, xmm8, xmm0
       mov      eax, dword ptr [rbp+0xD8]
       mov      ecx, eax
       imul     ecx, dword ptr [r13+0x5C]
       mov      edx, dword ptr [rbp+0xD4]
       add      ecx, edx
       add      ecx, ebx
       cmp      ecx, dword ptr [rbp+0xB4]

G_M000_IG103:                ;; offset=0x0BA3
       jae      G_M000_IG143
       vdivss   xmm0, xmm11, xmm13
       vsubss   xmm0, xmm0, xmm6
       vxorps   xmm1, xmm1, xmm1
       vcmpeqps xmm1, xmm1, xmm0
       vxorps   xmm2, xmm2, xmm2
       vpcmpgtd xmm2, xmm2, xmm0
       vandps   xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpltps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandnps  xmm0, xmm1, xmm0
       vmovss   dword ptr [r12+4*rcx], xmm0
       inc      ebx
       cmp      ebx, 8
       jge      G_M000_IG109

G_M000_IG104:                ;; offset=0x0BE9
       mov      dword ptr [rbp+0xD4], edx
       mov      dword ptr [rbp+0xD8], eax
       mov      eax, ebx
       jmp      G_M000_IG102

G_M000_IG105:                ;; offset=0x0BFC
       mov      dword ptr [rbp+0xD4], edx
       mov      dword ptr [rbp+0xD8], eax
       mov      eax, ebx

G_M000_IG106:                ;; offset=0x0C0A
       mov      dword ptr [rbp+0xD0], eax
       mov      r10d, eax
       add      r10d, dword ptr [rbp+0xD4]
       add      r10d, r10d
       mov      r9, gword ptr [r13+0x20]
       mov      r11, r9
       mov      ebx, r10d
       add      ebx, dword ptr [r13+0x50]
       mov      dword ptr [rbp+0x7C], ebx
       mov      edi, dword ptr [r11+0x08]
       cmp      ebx, edi
       jae      G_M000_IG143
       vmovss   xmm0, dword ptr [r11+4*rbx+0x10]
       mov      r11, r9
       mov      ebx, dword ptr [rbp+0x7C]
       inc      ebx
       cmp      ebx, edi
       jae      G_M000_IG143
       vaddss   xmm0, xmm0, dword ptr [r11+4*rbx+0x10]
       mov      r11, r9
       cmp      r10d, edi
       jae      G_M000_IG143
       mov      ebx, r10d
       vaddss   xmm0, xmm0, dword ptr [r11+4*rbx+0x10]
       lea      r11d, [r10+0x01]
       cmp      r11d, edi
       jae      G_M000_IG143
       inc      r10d
       vaddss   xmm12, xmm0, dword ptr [r9+4*r10+0x10]
       vmovaps  xmm0, xmm12
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:ComputeMask(float):float]
       vmovaps  xmm12, xmm0
       mov      ebx, dword ptr [rbp+0xD0]
       mov      edi, dword ptr [rbp+0xBC]
       cmp      ebx, edi
       jae      G_M000_IG143
       mov      eax, ebx
       mov      rcx, bword ptr [rbp+0x40]
       vaddss   xmm12, xmm12, dword ptr [rcx+4*rax]
       lea      eax, [rbx+0x08]
       cmp      eax, edi
       jae      G_M000_IG143
       lea      eax, [rbx+0x08]
       vmovss   xmm0, dword ptr [rcx+4*rax]
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:FastLog2(float):float]
       vmulss   xmm0, xmm9, xmm0
       vaddss   xmm12, xmm12, xmm0
       vmulss   xmm0, xmm12, xmm10
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:FastPow2(float):float]
       vmulss   xmm0, xmm0, xmm7

G_M000_IG107:                ;; offset=0x0CE3
       vaddss   xmm13, xmm8, xmm0
       mov      eax, dword ptr [rbp+0xD8]
       mov      ecx, eax
       imul     ecx, dword ptr [r13+0x5C]
       mov      edx, dword ptr [rbp+0xD4]
       add      ecx, edx
       add      ecx, ebx
       cmp      ecx, dword ptr [rbp+0xB4]
       jae      G_M000_IG143
       vdivss   xmm0, xmm11, xmm13
       vsubss   xmm0, xmm0, xmm6
       vxorps   xmm1, xmm1, xmm1
       vcmpeqps xmm1, xmm1, xmm0
       vxorps   xmm2, xmm2, xmm2
       vpcmpgtd xmm2, xmm2, xmm0
       vandps   xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpltps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandnps  xmm0, xmm1, xmm0
       vmovss   dword ptr [r12+4*rcx], xmm0
       inc      ebx
       cmp      ebx, 8
       jl       G_M000_IG105

G_M000_IG108:                ;; offset=0x0D4A
       mov      rdi, bword ptr [rbp+0x40]

G_M000_IG109:                ;; offset=0x0D4E
       add      edx, 8
       mov      dword ptr [rbp+0xD8], eax
       mov      eax, edx

G_M000_IG110:                ;; offset=0x0D59
       mov      r9d, dword ptr [r13+0x5C]
       add      r9d, -8
       cmp      eax, r9d
       jle      G_M000_IG101

G_M000_IG111:                ;; offset=0x0D6A
       cmp      eax, dword ptr [r13+0x5C]
       jl       G_M000_IG122

G_M000_IG112:                ;; offset=0x0D74
       mov      eax, dword ptr [rbp+0xD8]
       mov      r8d, dword ptr [rbp+0xB4]

G_M000_IG113:                ;; offset=0x0D81
       inc      eax
       mov      ecx, eax

G_M000_IG114:                ;; offset=0x0D85
       cmp      ecx, dword ptr [r13+0x40]
       jge      G_M000_IG125

G_M000_IG115:                ;; offset=0x0D8F
       cmp      gword ptr [rbp+0x60], 0
       jne      G_M000_IG124

G_M000_IG116:                ;; offset=0x0D9A
       mov      dword ptr [rbp+0xD8], ecx
       lea      edx, [2*rcx+0x01]
       mov      r9d, esi
       mov      rcx, r13
       xor      r8d, r8d
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this]
       mov      eax, dword ptr [rbp+0xD8]
       lea      edx, [2*rax+0x02]
       mov      r8d, dword ptr [r13+0x50]
       mov      r9d, esi
       mov      rcx, r13
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this]
       xor      eax, eax
       test     r15d, r15d
       je       SHORT G_M000_IG111
       jmp      G_M000_IG110

G_M000_IG117:                ;; offset=0x0DDF
       mov      eax, dword ptr [rbp+0xD8]
       lea      ecx, [8*rax+0x05]
       imul     ecx, dword ptr [r13+0x4C]
       mov      r8d, dword ptr [rbp+0xD4]
       lea      ecx, [rcx+8*r8+0x01]
       mov      rdx, gword ptr [r13+0x10]
       test     rdx, rdx
       je       SHORT G_M000_IG118
       cmp      dword ptr [rdx+0x08], ecx
       jb       G_M000_IG136
       mov      r10d, ecx
       lea      r10, bword ptr [rdx+4*r10+0x10]
       mov      edx, dword ptr [rdx+0x08]
       sub      edx, ecx
       jmp      SHORT G_M000_IG119

G_M000_IG118:                ;; offset=0x0E1E
       test     ecx, ecx
       jne      G_M000_IG136
       xor      r10, r10
       xor      edx, edx

G_M000_IG119:                ;; offset=0x0E2B
       mov      bword ptr [rbp+0x80], r10
       mov      dword ptr [rbp+0x88], edx
       lea      rcx, [rbp+0x80]
       mov      edx, dword ptr [r13+0x4C]
       vmovaps  xmm2, xmm0
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:ModulateScalar(System.ReadOnlySpan`1[float],int,float):float]

G_M000_IG120:                ;; offset=0x0E4D
       vmulss   xmm0, xmm10, xmm0
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:FastPow2(float):float]
       vmulss   xmm0, xmm0, xmm7
       vaddss   xmm0, xmm8, xmm0
       mov      eax, dword ptr [rbp+0xD8]
       mov      ecx, eax
       imul     ecx, dword ptr [r13+0x5C]
       mov      r10d, dword ptr [rbp+0xD4]
       add      ecx, r10d
       mov      r8d, dword ptr [rbp+0xB4]
       cmp      ecx, r8d
       jae      G_M000_IG143
       vdivss   xmm0, xmm11, xmm0
       vsubss   xmm0, xmm0, xmm6
       vxorps   xmm1, xmm1, xmm1
       vcmpeqps xmm1, xmm1, xmm0
       vxorps   xmm2, xmm2, xmm2
       vpcmpgtd xmm2, xmm2, xmm0
       vandps   xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpltps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandnps  xmm0, xmm1, xmm0
       vmovss   dword ptr [r12+4*rcx], xmm0
       inc      r10d
       cmp      r10d, dword ptr [r13+0x5C]
       jge      G_M000_IG113

G_M000_IG121:                ;; offset=0x0EC7
       mov      dword ptr [rbp+0xD8], eax
       mov      eax, r10d

G_M000_IG122:                ;; offset=0x0ED0
       mov      dword ptr [rbp+0xD4], eax
       lea      ecx, [rax+rax]
       mov      rdx, gword ptr [r13+0x20]
       mov      r10d, ecx
       add      r10d, dword ptr [r13+0x50]
       cmp      r10d, dword ptr [rdx+0x08]
       jae      G_M000_IG143
       vmovss   xmm0, dword ptr [rdx+4*r10+0x10]
       mov      rdx, gword ptr [r13+0x20]
       mov      r10d, dword ptr [r13+0x50]
       lea      r10d, [r10+rcx+0x01]
       cmp      r10d, dword ptr [rdx+0x08]
       jae      G_M000_IG143
       vaddss   xmm0, xmm0, dword ptr [rdx+4*r10+0x10]
       mov      rdx, gword ptr [r13+0x20]
       cmp      ecx, dword ptr [rdx+0x08]
       jae      G_M000_IG143
       mov      r10d, ecx
       vaddss   xmm0, xmm0, dword ptr [rdx+4*r10+0x10]
       mov      rdx, gword ptr [r13+0x20]
       lea      r10d, [rcx+0x01]
       cmp      r10d, dword ptr [rdx+0x08]
       jae      G_M000_IG143
       inc      ecx
       vaddss   xmm0, xmm0, dword ptr [rdx+4*rcx+0x10]
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:ComputeMask(float):float]
       test     esi, esi
       je       G_M000_IG117

G_M000_IG123:                ;; offset=0x0F52
       mov      eax, dword ptr [rbp+0xD8]
       lea      r8d, [8*rax+0x05]
       mov      r10d, dword ptr [rbp+0xD4]
       lea      edx, [8*r10]
       mov      rcx, r13
       vmovaps  xmm3, xmm0
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:ModulateVector(int,int,float):float:this]
       jmp      G_M000_IG120

G_M000_IG124:                ;; offset=0x0F81
       mov      rdx, gword ptr [rbp+0x60]
       cmp      dword ptr [rdx+0x20], 0
       je       G_M000_IG116
       jmp      G_M000_IG133

G_M000_IG125:                ;; offset=0x0F94
       lea      rcx, bword ptr [r13+0x30]
       mov      rdx, gword ptr [rbp+0x50]
       call     CORINFO_HELP_ASSIGN_REF
       mov      edi, dword ptr [rbp+0x68]
       mov      dword ptr [r13+0x54], edi
       mov      dword ptr [r13+0x58], r14d
       movzx    rbx, byte  ptr [rbp+0x200]
       mov      byte  ptr [r13+0x62], bl
       mov      r9, 0x6911662D01EE
       cmp      qword ptr [rbp+0xF8], r9
       je       SHORT G_M000_IG126
       call     CORINFO_HELP_FAIL_FAST

G_M000_IG126:                ;; offset=0x0FCF
       nop

G_M000_IG127:                ;; offset=0x0FD0
       vmovaps  xmm6, xmmword ptr [rbp+0x170]
       vmovaps  xmm7, xmmword ptr [rbp+0x160]
       vmovaps  xmm8, xmmword ptr [rbp+0x150]
       vmovaps  xmm9, xmmword ptr [rbp+0x140]
       vmovaps  xmm10, xmmword ptr [rbp+0x130]
       vmovaps  xmm11, xmmword ptr [rbp+0x120]
       vmovaps  xmm12, xmmword ptr [rbp+0x110]
       vmovaps  xmm13, xmmword ptr [rbp+0x100]
       lea      rsp, [rbp+0x188]
       pop      rbx
       pop      rsi
       pop      rdi
       pop      r12
       pop      r13
       pop      r14
       pop      r15
       pop      rbp
       ret

G_M000_IG128:                ;; offset=0x1024
       mov      rdx, gword ptr [rbp+0x60]
       cmp      dword ptr [rdx+0x20], 0
       je       G_M000_IG74
       jmp      SHORT G_M000_IG133

G_M000_IG129:                ;; offset=0x1034
       mov      rax, gword ptr [rbp+0x60]
       cmp      dword ptr [rax+0x20], 0
       je       G_M000_IG11
       jmp      SHORT G_M000_IG133

G_M000_IG130:                ;; offset=0x1044
       mov      r8d, -1
       jmp      G_M000_IG18

G_M000_IG131:                ;; offset=0x104F
       mov      r11d, r8d
       mov      qword ptr [rbp], r11
       mov      r11d, ecx
       add      r11, qword ptr [rbp]
       mov      qword ptr [rbp], r11
       mov      r11d, dword ptr [rdx+0x08]
       cmp      qword ptr [rbp], r11
       ja       G_M000_IG136
       mov      r8d, r8d
       lea      rdx, bword ptr [rdx+4*r8+0x10]
       mov      bword ptr [rbp+0x38], rdx
       mov      r11, gword ptr [rbp+0x50]
       jmp      G_M000_IG13

G_M000_IG132:                ;; offset=0x1084
       mov      rcx, r13
       call     [System.ThrowHelper:ThrowObjectDisposedException(System.Object)]
       int3

G_M000_IG133:                ;; offset=0x108E
       lea      rcx, [rbp+0x60]
       call     [System.Threading.CancellationToken:ThrowOperationCanceledException():this]
       int3

G_M000_IG134:                ;; offset=0x1099
       mov      rcx, 0x7FF882C3DDB8
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      rcx, rbx
       mov      rdx, 0x231A58E62C8
       call     [System.ArgumentOutOfRangeException:.ctor(System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3

G_M000_IG135:                ;; offset=0x10C7
       mov      rcx, 0x7FF882B8F4B8
       call     CORINFO_HELP_NEWSFAST
       mov      r13, rax
       mov      rcx, r13
       mov      rdx, 0x231A58E6208
       mov      r8, 0x231A58E62A0
       call     [System.ArgumentException:.ctor(System.String,System.String):this]
       mov      rcx, r13
       call     CORINFO_HELP_THROW
       int3

G_M000_IG136:                ;; offset=0x10FF
       call     [System.ThrowHelper:ThrowArgumentOutOfRangeException()]
       int3

G_M000_IG137:                ;; offset=0x1106
       call     [System.ThrowHelper:ThrowArgumentException_DestinationTooShort()]
       int3

G_M000_IG138:                ;; offset=0x110D
       xor      ecx, ecx
       call     [System.Math:ThrowMinMaxException[int](int,int)]
       int3

G_M000_IG139:                ;; offset=0x1116
       mov      edx, esi
       xor      ecx, ecx
       call     [System.Math:ThrowMinMaxException[int](int,int)]
       int3

G_M000_IG140:                ;; offset=0x1121
       mov      edx, eax
       xor      ecx, ecx
       call     [System.Math:ThrowMinMaxException[int](int,int)]
       int3

G_M000_IG141:                ;; offset=0x112C
       xor      ecx, ecx
       call     [System.Math:ThrowMinMaxException[int](int,int)]
       int3

G_M000_IG142:                ;; offset=0x1135
       mov      edx, r9d
       xor      ecx, ecx
       call     [System.Math:ThrowMinMaxException[int](int,int)]
       int3

G_M000_IG143:                ;; offset=0x1141
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	3F800000h		;         1
RWD04  	dd	41100000h		;         9
RWD08  	dd	42600000h		;        56
RWD12  	dd	3F574BC7h		;     0.841
RWD16  	dd	3ECEAF25h		;   0.40368
RWD20  	dd	BDDC6A08h		; -0.107624
RWD24  	dd	3FB8AA3Bh		;    1.4427
RWD28  	dd	3F19999Ah		;       0.6

; Total bytes of code 4423

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ComputePreErosion(int,int,bool):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 56 single block inlinees; 30 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r15
       push     r14
       push     r13
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 288
       vmovaps  xmmword ptr [rsp+0x110], xmm6
       vmovaps  xmmword ptr [rsp+0x100], xmm7
       vmovaps  xmmword ptr [rsp+0xF0], xmm8
       vmovaps  xmmword ptr [rsp+0xE0], xmm9
       vmovaps  xmmword ptr [rsp+0xD0], xmm10
       vmovaps  xmmword ptr [rsp+0xC0], xmm11
       vmovaps  xmmword ptr [rsp+0xB0], xmm12
       vmovaps  xmmword ptr [rsp+0xA0], xmm13
       vmovaps  xmmword ptr [rsp+0x90], xmm14
       vmovaps  xmmword ptr [rsp+0x80], xmm15

G_M000_IG02:                ;; offset=0x006B
       vmovss   xmm0, dword ptr [reloc @RWD00]
       mov      eax, dword ptr [rcx+0x4C]
       imul     edx, eax
       inc      edx
       imul     r8d, dword ptr [rcx+0x50]
       xor      r10d, r10d
       test     r9b, r9b
       je       G_M000_IG11

G_M000_IG03:                ;; offset=0x008C
       mov      r9d, dword ptr [rcx+0x44]
       add      r9d, -8
       js       G_M000_IG11
       align    [0 bytes for IG04]

G_M000_IG04:                ;; offset=0x009A
       lea      r11d, [rdx+r10]
       mov      rbx, gword ptr [rcx+0x10]
       mov      rsi, rbx
       mov      edi, r11d
       sub      edi, eax
       js       G_M000_IG29
       mov      ebp, dword ptr [rsi+0x08]
       mov      r14d, ebp
       sub      r14d, edi
       cmp      r14d, 8
       jl       G_M000_IG29
       cmp      edi, ebp
       jae      G_M000_IG30
       vmovups  ymm1, ymmword ptr [rsi+4*rdi+0x10]
       mov      rsi, rbx
       test     r11d, r11d
       jl       G_M000_IG29
       mov      edi, ebp
       sub      edi, r11d
       cmp      edi, 8
       jl       G_M000_IG29
       cmp      r11d, ebp
       jae      G_M000_IG30
       mov      edi, r11d
       vmovups  ymm2, ymmword ptr [rsi+4*rdi+0x10]
       mov      rsi, rbx
       lea      edi, [r11+rax]
       mov      r14d, edi
       test     r14d, r14d
       jl       G_M000_IG29
       mov      r15d, ebp
       sub      r15d, r14d
       cmp      r15d, 8
       jl       G_M000_IG29
       cmp      r14d, ebp
       jae      G_M000_IG30
       vmovups  ymm3, ymmword ptr [rsi+4*r14+0x10]
       mov      rsi, rbx
       lea      r14d, [r11-0x01]
       test     r14d, r14d
       jl       G_M000_IG29
       mov      r15d, ebp
       sub      r15d, r14d
       cmp      r15d, 8
       jl       G_M000_IG29
       cmp      r14d, ebp
       jae      G_M000_IG30
       vmovups  ymm4, ymmword ptr [rsi+4*r14+0x10]
       mov      rsi, rbx
       lea      r14d, [r11+0x01]
       mov      r15d, ebp
       sub      r15d, r14d
       cmp      r15d, 8

G_M000_IG05:                ;; offset=0x0171
       jl       G_M000_IG29
       cmp      r14d, ebp
       jae      G_M000_IG30
       vaddps   ymm4, ymm4, ymmword ptr [rsi+4*r14+0x10]
       vaddps   ymm1, ymm3, ymm1
       vaddps   ymm1, ymm1, ymm4
       vbroadcastss ymm4, dword ptr [reloc @RWD04]
       vmulps   ymm1, ymm1, ymm4
       vbroadcastss ymm5, dword ptr [reloc @RWD08]
       vaddps   ymm6, ymm5, ymm2
       vcmpneqps ymm7, ymm6, ymm6
       vxorps   ymm8, ymm8, ymm8
       vcmpgtps ymm8, ymm6, ymm8
       vorps    ymm7, ymm8, ymm7
       vandps   ymm6, ymm7, ymm6
       vmulps   ymm7, ymm6, ymm6
       vbroadcastss ymm8, dword ptr [reloc @RWD12]
       vmulps   ymm6, ymm8, ymm6
       vmulps   ymm6, ymm6, ymm7
       vbroadcastss ymm9, dword ptr [reloc @RWD16]
       vaddps   ymm6, ymm9, ymm6
       vbroadcastss ymm10, dword ptr [reloc @RWD20]
       vmulps   ymm7, ymm10, ymm7
       vbroadcastss ymm11, dword ptr [reloc @RWD24]
       vaddps   ymm7, ymm11, ymm7
       vdivps   ymm6, ymm6, ymm7
       vsubps   ymm1, ymm2, ymm1
       vmulps   ymm1, ymm1, ymm6
       vmulps   ymm1, ymm1, ymm1
       vbroadcastss ymm6, dword ptr [reloc @RWD28]
       vcmpeqps ymm7, ymm6, ymm1
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm1
       vandps   ymm7, ymm12, ymm7
       vcmpneqps ymm12, ymm1, ymm1
       vorps    ymm7, ymm12, ymm7
       vcmpltps ymm12, ymm1, ymm6
       vorps    ymm7, ymm12, ymm7
       vblendvps ymm1, ymm6, ymm1, ymm7
       vbroadcastss ymm7, dword ptr [reloc @RWD00]
       vmulps   ymm1, ymm7, ymm1
       vbroadcastss ymm12, dword ptr [reloc @RWD32]
       vaddps   ymm1, ymm12, ymm1
       vsqrtps  ymm1, ymm1
       vmulps   ymm1, ymm1, ymm4
       mov      rsi, rbx
       lea      r14d, [r11+2*rax]
       mov      r15d, r14d
       test     r15d, r15d
       jl       G_M000_IG29
       mov      r13d, ebp
       sub      r13d, r15d
       cmp      r13d, 8
       jl       G_M000_IG29
       cmp      r15d, ebp

G_M000_IG06:                ;; offset=0x0288
       jae      G_M000_IG30
       vmovups  ymm13, ymmword ptr [rsi+4*r15+0x10]
       mov      rsi, rbx
       lea      r15d, [rdi-0x01]
       test     r15d, r15d
       jl       G_M000_IG29
       mov      r13d, ebp
       sub      r13d, r15d
       cmp      r13d, 8
       jl       G_M000_IG29
       cmp      r15d, ebp
       jae      G_M000_IG30
       vmovups  ymm14, ymmword ptr [rsi+4*r15+0x10]
       mov      rsi, rbx
       inc      edi
       js       G_M000_IG29
       mov      r15d, ebp
       sub      r15d, edi
       cmp      r15d, 8
       jl       G_M000_IG29
       cmp      edi, ebp
       jae      G_M000_IG30
       vaddps   ymm14, ymm14, ymmword ptr [rsi+4*rdi+0x10]
       vaddps   ymm2, ymm13, ymm2
       vaddps   ymm2, ymm14, ymm2
       vmulps   ymm2, ymm2, ymm4
       vmovups  ymmword ptr [rsp+0x40], ymm2
       vaddps   ymm14, ymm5, ymm3
       vcmpneqps ymm15, ymm14, ymm14
       vxorps   ymm2, ymm2, ymm2
       vcmpgtps ymm2, ymm14, ymm2
       vorps    ymm2, ymm15, ymm2
       vandps   ymm14, ymm14, ymm2
       vmulps   ymm2, ymm14, ymm14
       vmulps   ymm14, ymm8, ymm14
       vmulps   ymm14, ymm14, ymm2
       vaddps   ymm14, ymm14, ymm9
       vmulps   ymm2, ymm10, ymm2
       vaddps   ymm2, ymm11, ymm2
       vdivps   ymm2, ymm14, ymm2
       vsubps   ymm14, ymm3, ymmword ptr [rsp+0x40]
       vmulps   ymm2, ymm14, ymm2
       vmulps   ymm2, ymm2, ymm2
       vcmpeqps ymm14, ymm6, ymm2
       vxorps   ymm15, ymm15, ymm15
       vpcmpgtd ymm15, ymm15, ymm2
       vandps   ymm14, ymm15, ymm14
       vcmpneqps ymm15, ymm2, ymm2
       vorps    ymm14, ymm15, ymm14
       vcmpltps ymm15, ymm2, ymm6
       vorps    ymm14, ymm15, ymm14
       vblendvps ymm2, ymm6, ymm2, ymm14
       vmulps   ymm2, ymm7, ymm2

G_M000_IG07:                ;; offset=0x0379
       vaddps   ymm2, ymm12, ymm2
       vsqrtps  ymm2, ymm2
       vmulps   ymm2, ymm2, ymm4
       vaddps   ymm1, ymm2, ymm1
       vmovaps  ymm2, ymm3
       vmovaps  ymm3, ymm13
       mov      rsi, rbx
       lea      edi, [rax+2*rax]
       add      edi, r11d
       mov      r15d, edi
       test     r15d, r15d
       jl       G_M000_IG29
       mov      r13d, ebp
       sub      r13d, r15d
       cmp      r13d, 8
       jl       G_M000_IG29
       cmp      r15d, ebp
       jae      G_M000_IG30
       vmovups  ymm13, ymmword ptr [rsi+4*r15+0x10]
       mov      rsi, rbx
       lea      r15d, [r14-0x01]
       test     r15d, r15d
       jl       G_M000_IG29
       mov      r13d, ebp
       sub      r13d, r15d
       cmp      r13d, 8
       jl       G_M000_IG29
       cmp      r15d, ebp
       jae      G_M000_IG30
       vmovups  ymm14, ymmword ptr [rsi+4*r15+0x10]
       mov      rsi, rbx
       inc      r14d
       js       G_M000_IG29
       mov      r15d, ebp
       sub      r15d, r14d
       cmp      r15d, 8
       jl       G_M000_IG29
       cmp      r14d, ebp
       jae      G_M000_IG30
       vaddps   ymm14, ymm14, ymmword ptr [rsi+4*r14+0x10]
       vaddps   ymm2, ymm13, ymm2
       vaddps   ymm2, ymm14, ymm2
       vmulps   ymm2, ymm2, ymm4
       vmovups  ymmword ptr [rsp+0x20], ymm2
       vaddps   ymm14, ymm5, ymm3
       vcmpneqps ymm15, ymm14, ymm14
       vxorps   ymm2, ymm2, ymm2
       vcmpgtps ymm2, ymm14, ymm2
       vorps    ymm2, ymm15, ymm2
       vandps   ymm14, ymm14, ymm2
       vmulps   ymm2, ymm14, ymm14
       vmulps   ymm14, ymm8, ymm14
       vmulps   ymm14, ymm14, ymm2
       vaddps   ymm14, ymm14, ymm9

G_M000_IG08:                ;; offset=0x0463
       vmulps   ymm2, ymm10, ymm2
       vaddps   ymm2, ymm11, ymm2
       vdivps   ymm2, ymm14, ymm2
       vsubps   ymm14, ymm3, ymmword ptr [rsp+0x20]
       vmulps   ymm2, ymm14, ymm2
       vmulps   ymm2, ymm2, ymm2
       vcmpeqps ymm14, ymm6, ymm2
       vxorps   ymm15, ymm15, ymm15
       vpcmpgtd ymm15, ymm15, ymm2
       vandps   ymm14, ymm15, ymm14
       vcmpneqps ymm15, ymm2, ymm2
       vorps    ymm14, ymm15, ymm14
       vcmpltps ymm15, ymm2, ymm6
       vorps    ymm14, ymm15, ymm14
       vblendvps ymm2, ymm6, ymm2, ymm14
       vmulps   ymm2, ymm7, ymm2
       vaddps   ymm2, ymm12, ymm2
       vsqrtps  ymm2, ymm2
       vmulps   ymm2, ymm2, ymm4
       vaddps   ymm1, ymm2, ymm1
       vmovaps  ymm2, ymm3
       vmovaps  ymm3, ymm13
       mov      rsi, rbx
       lea      r11d, [r11+4*rax]
       test     r11d, r11d
       jl       G_M000_IG29
       mov      r14d, ebp
       sub      r14d, r11d
       cmp      r14d, 8
       jl       G_M000_IG29
       cmp      r11d, ebp
       jae      G_M000_IG30
       vmovups  ymm13, ymmword ptr [rsi+4*r11+0x10]
       mov      r11, rbx
       lea      esi, [rdi-0x01]
       test     esi, esi
       jl       G_M000_IG29
       mov      r14d, ebp
       sub      r14d, esi
       cmp      r14d, 8
       jl       G_M000_IG29
       cmp      esi, ebp
       jae      G_M000_IG30
       vmovups  ymm14, ymmword ptr [r11+4*rsi+0x10]
       inc      edi
       js       G_M000_IG29
       mov      r11d, ebp
       sub      r11d, edi
       cmp      r11d, 8
       jl       G_M000_IG29
       cmp      edi, ebp
       jae      G_M000_IG30
       mov      r11d, edi
       vaddps   ymm14, ymm14, ymmword ptr [rbx+4*r11+0x10]

G_M000_IG09:                ;; offset=0x054E
       vaddps   ymm2, ymm13, ymm2
       vaddps   ymm2, ymm14, ymm2
       vmulps   ymm2, ymm2, ymm4
       vaddps   ymm5, ymm5, ymm3
       vcmpneqps ymm13, ymm5, ymm5
       vxorps   ymm14, ymm14, ymm14
       vcmpgtps ymm14, ymm5, ymm14
       vorps    ymm13, ymm14, ymm13
       vandps   ymm5, ymm13, ymm5
       vmulps   ymm13, ymm5, ymm5
       vmulps   ymm5, ymm8, ymm5
       vmulps   ymm5, ymm13, ymm5
       vaddps   ymm5, ymm9, ymm5
       vmulps   ymm8, ymm10, ymm13
       vaddps   ymm8, ymm8, ymm11
       vdivps   ymm5, ymm5, ymm8
       vsubps   ymm2, ymm3, ymm2
       vmulps   ymm2, ymm2, ymm5
       vmulps   ymm2, ymm2, ymm2
       vcmpeqps ymm3, ymm6, ymm2
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm2
       vandps   ymm3, ymm5, ymm3
       vcmpneqps ymm5, ymm2, ymm2
       vorps    ymm3, ymm5, ymm3
       vcmpltps ymm5, ymm2, ymm6
       vorps    ymm3, ymm5, ymm3
       vblendvps ymm2, ymm6, ymm2, ymm3
       vmulps   ymm2, ymm7, ymm2
       vaddps   ymm2, ymm12, ymm2
       vsqrtps  ymm2, ymm2
       vmulps   ymm2, ymm2, ymm4
       vaddps   ymm1, ymm2, ymm1
       mov      r11, gword ptr [rcx+0x18]
       vmovaps  ymm2, ymm1
       vmovaps  xmm3, xmm1
       vmovshdup xmm3, xmm3
       vaddss   xmm3, xmm3, xmm2
       vmovaps  xmm2, xmm1
       vunpckhps xmm2, xmm2, xmm2
       vaddss   xmm2, xmm2, xmm3
       vmovaps  xmm3, xmm1
       vshufps  xmm3, xmm3, xmm3, -1
       vaddss   xmm2, xmm3, xmm2
       vmovss   xmm3, dword ptr [reloc @RWD04]
       vmulss   xmm2, xmm2, xmm3
       mov      ebp, r10d
       sar      ebp, 31
       and      ebp, 3
       add      ebp, r10d
       sar      ebp, 2
       add      ebp, r8d
       cmp      ebp, dword ptr [r11+0x08]
       jae      G_M000_IG30
       vmovss   dword ptr [r11+4*rbp+0x10], xmm2
       mov      r11, gword ptr [rcx+0x18]
       vextractf128 xmm2, ymm1
       vextractf128 xmm4, ymm1
       vmovshdup xmm4, xmm4
       vaddss   xmm2, xmm4, xmm2
       vextractf128 xmm4, ymm1
       vunpckhps xmm4, xmm4, xmm4

G_M000_IG10:                ;; offset=0x065D
       vaddss   xmm2, xmm4, xmm2
       vextractf128 xmm1, ymm1
       vshufps  xmm1, xmm1, xmm1, -1
       vaddss   xmm1, xmm1, xmm2
       vmulss   xmm2, xmm1, xmm3
       lea      ebp, [r10+0x04]
       mov      ebx, ebp
       sar      ebx, 31
       and      ebx, 3
       add      ebp, ebx
       sar      ebp, 2
       add      ebp, r8d
       cmp      ebp, dword ptr [r11+0x08]
       jae      G_M000_IG30
       mov      ebx, ebp
       vmovss   dword ptr [r11+4*rbx+0x10], xmm2
       add      r10d, 8
       cmp      r10d, r9d
       jle      G_M000_IG04

G_M000_IG11:                ;; offset=0x06A8
       mov      r9d, dword ptr [rcx+0x44]
       vmovss   xmm3, dword ptr [reloc @RWD04]
       vmovss   xmm1, dword ptr [reloc @RWD08]
       vmovss   xmm2, dword ptr [reloc @RWD12]
       vmovss   xmm4, dword ptr [reloc @RWD16]
       vmovss   xmm5, dword ptr [reloc @RWD20]
       vmovss   xmm6, dword ptr [reloc @RWD24]
       vbroadcastss xmm7, dword ptr [reloc @RWD28]
       jmp      G_M000_IG27
       align    [0 bytes for IG14]

G_M000_IG12:                ;; offset=0x06EA
       lea      r11d, [rdx+r10]
       mov      rbx, gword ptr [rcx+0x10]
       mov      rsi, rbx
       mov      ebp, dword ptr [rsi+0x08]
       cmp      r11d, ebp
       jae      G_M000_IG30
       mov      edi, r11d
       vmovss   xmm8, dword ptr [rsi+4*rdi+0x10]
       mov      rsi, rbx
       lea      edi, [r11-0x01]
       cmp      edi, ebp
       jae      G_M000_IG30
       lea      edi, [r11-0x01]
       vmovss   xmm9, dword ptr [rsi+4*rdi+0x10]
       mov      rsi, rbx
       lea      edi, [r11+0x01]
       cmp      edi, ebp
       jae      G_M000_IG30
       mov      r14d, edi
       vmovss   xmm10, dword ptr [rsi+4*r14+0x10]
       vaddss   xmm9, xmm10, xmm9
       mov      rsi, rbx
       mov      r14d, r11d
       sub      r14d, eax
       cmp      r14d, ebp
       jae      G_M000_IG30
       vmovss   xmm11, dword ptr [rsi+4*r14+0x10]
       mov      rsi, rbx
       lea      r14d, [r11+rax]
       cmp      r14d, ebp
       jae      G_M000_IG30
       vaddss   xmm11, xmm11, dword ptr [rsi+4*r14+0x10]
       vaddss   xmm9, xmm9, xmm11
       vmulss   xmm9, xmm9, xmm3
       vaddss   xmm11, xmm8, xmm1
       vxorps   xmm12, xmm12, xmm12
       vcmpneqps xmm13, xmm11, xmm11
       vorps    xmm12, xmm13, xmm12
       vxorps   xmm13, xmm13, xmm13
       vcmpgtps xmm13, xmm11, xmm13
       vorps    xmm12, xmm13, xmm12
       vandps   xmm11, xmm12, xmm11
       vmulss   xmm12, xmm11, xmm11
       vmulss   xmm11, xmm11, xmm2
       vmulss   xmm11, xmm11, xmm12
       vaddss   xmm11, xmm11, xmm4
       vmulss   xmm12, xmm12, xmm5
       vaddss   xmm12, xmm12, xmm6
       vdivss   xmm11, xmm11, xmm12
       vsubss   xmm8, xmm8, xmm9
       vmulss   xmm8, xmm11, xmm8
       vmulss   xmm8, xmm8, xmm8
       vminss   xmm8, xmm7, xmm8
       vmulss   xmm8, xmm8, dword ptr [reloc @RWD00]
       vmovss   xmm9, dword ptr [reloc @RWD32]
       vaddss   xmm8, xmm8, xmm9
       vsqrtss  xmm8, xmm8, xmm8

G_M000_IG13:                ;; offset=0x07F0
       vmulss   xmm8, xmm8, xmm3
       mov      esi, 1

G_M000_IG14:                ;; offset=0x07F9
       mov      r14d, esi
       imul     r14d, eax
       add      r14d, r11d
       mov      r15, rbx
       cmp      r14d, ebp
       jae      G_M000_IG30
       mov      r13d, r14d
       vmovss   xmm11, dword ptr [r15+4*r13+0x10]
       mov      r15, rbx
       lea      r13d, [r14-0x01]
       cmp      r13d, ebp
       jae      G_M000_IG30
       lea      r13d, [r14-0x01]
       vmovss   xmm12, dword ptr [r15+4*r13+0x10]
       mov      r15, rbx
       lea      r13d, [r14+0x01]
       cmp      r13d, ebp
       jae      G_M000_IG30
       lea      r13d, [r14+0x01]
       vaddss   xmm12, xmm12, dword ptr [r15+4*r13+0x10]
       mov      r15, rbx
       mov      r13d, r14d
       sub      r13d, eax
       cmp      r13d, ebp
       jae      G_M000_IG30
       vmovss   xmm13, dword ptr [r15+4*r13+0x10]
       mov      r15, rbx
       add      r14d, eax
       cmp      r14d, ebp
       jae      G_M000_IG30
       vaddss   xmm13, xmm13, dword ptr [r15+4*r14+0x10]
       vaddss   xmm12, xmm12, xmm13
       vmulss   xmm12, xmm12, xmm3
       vaddss   xmm13, xmm11, xmm1
       vxorps   xmm14, xmm14, xmm14
       vcmpneqps xmm15, xmm13, xmm13
       vorps    xmm14, xmm15, xmm14
       vxorps   xmm15, xmm15, xmm15
       vcmpgtps xmm15, xmm13, xmm15
       vorps    xmm14, xmm15, xmm14
       vandps   xmm13, xmm14, xmm13
       vmulss   xmm14, xmm13, xmm13
       vmulss   xmm13, xmm13, xmm2
       vmulss   xmm13, xmm13, xmm14
       vaddss   xmm13, xmm13, xmm4
       vmulss   xmm14, xmm14, xmm5
       vaddss   xmm14, xmm14, xmm6
       vdivss   xmm13, xmm13, xmm14
       vsubss   xmm11, xmm11, xmm12
       vmulss   xmm11, xmm13, xmm11
       vmulss   xmm11, xmm11, xmm11
       vminss   xmm11, xmm7, xmm11
       vmulss   xmm11, xmm11, xmm0
       vaddss   xmm11, xmm11, xmm9
       vsqrtss  xmm11, xmm11, xmm11
       vmulss   xmm11, xmm11, xmm3
       vaddss   xmm8, xmm11, xmm8
       inc      esi

G_M000_IG15:                ;; offset=0x08FC
       cmp      esi, 4
       jl       G_M000_IG14

G_M000_IG16:                ;; offset=0x0905
       vmovss   dword ptr [rsp+0x7C], xmm8
       mov      rsi, rbx
       lea      r14d, [rdi-0x01]
       cmp      r14d, ebp
       jae      G_M000_IG30
       lea      r14d, [rdi-0x01]
       vmovss   xmm11, dword ptr [rsi+4*r14+0x10]
       mov      rsi, rbx
       lea      r14d, [rdi+0x01]
       cmp      r14d, ebp
       jae      G_M000_IG30
       lea      r14d, [rdi+0x01]
       vaddss   xmm11, xmm11, dword ptr [rsi+4*r14+0x10]
       mov      rsi, rbx
       mov      r14d, edi
       sub      r14d, eax
       cmp      r14d, ebp
       jae      G_M000_IG30
       vmovss   xmm12, dword ptr [rsi+4*r14+0x10]
       mov      rsi, rbx
       lea      r14d, [rdi+rax]
       cmp      r14d, ebp
       jae      G_M000_IG30
       vaddss   xmm12, xmm12, dword ptr [rsi+4*r14+0x10]
       vaddss   xmm11, xmm11, xmm12
       vmulss   xmm11, xmm11, xmm3
       vaddss   xmm12, xmm10, xmm1
       vxorps   xmm13, xmm13, xmm13
       vcmpneqps xmm14, xmm12, xmm12
       vorps    xmm13, xmm14, xmm13
       vxorps   xmm14, xmm14, xmm14
       vcmpgtps xmm14, xmm12, xmm14
       vorps    xmm13, xmm14, xmm13
       vandps   xmm12, xmm13, xmm12
       vmulss   xmm13, xmm12, xmm12
       vmulss   xmm12, xmm12, xmm2
       vmulss   xmm12, xmm12, xmm13
       vaddss   xmm12, xmm12, xmm4
       vmulss   xmm13, xmm13, xmm5
       vaddss   xmm13, xmm13, xmm6
       vdivss   xmm12, xmm12, xmm13
       vsubss   xmm10, xmm10, xmm11
       vmulss   xmm10, xmm12, xmm10
       vmulss   xmm10, xmm10, xmm10
       vminss   xmm10, xmm7, xmm10
       vmulss   xmm10, xmm10, dword ptr [reloc @RWD00]
       vaddss   xmm10, xmm10, xmm9
       vsqrtss  xmm10, xmm10, xmm10
       vmulss   xmm10, xmm10, xmm3
       mov      esi, 1
       align    [0 bytes for IG17]

G_M000_IG17:                ;; offset=0x09F1
       mov      r14d, esi
       imul     r14d, eax
       add      r14d, edi
       mov      r15, rbx
       cmp      r14d, ebp
       jae      G_M000_IG30
       mov      r13d, r14d
       vmovss   xmm11, dword ptr [r15+4*r13+0x10]
       mov      r15, rbx
       lea      r13d, [r14-0x01]
       cmp      r13d, ebp
       jae      G_M000_IG30
       lea      r13d, [r14-0x01]
       vmovss   xmm12, dword ptr [r15+4*r13+0x10]
       mov      r15, rbx
       lea      r13d, [r14+0x01]
       cmp      r13d, ebp
       jae      G_M000_IG30
       lea      r13d, [r14+0x01]
       vaddss   xmm12, xmm12, dword ptr [r15+4*r13+0x10]
       mov      r15, rbx
       mov      r13d, r14d
       sub      r13d, eax
       cmp      r13d, ebp
       jae      G_M000_IG30
       vmovss   xmm13, dword ptr [r15+4*r13+0x10]
       mov      r15, rbx
       add      r14d, eax
       cmp      r14d, ebp
       jae      G_M000_IG30
       vaddss   xmm13, xmm13, dword ptr [r15+4*r14+0x10]
       vaddss   xmm12, xmm12, xmm13
       vmulss   xmm12, xmm12, xmm3
       vaddss   xmm13, xmm11, xmm1
       vxorps   xmm14, xmm14, xmm14
       vcmpneqps xmm15, xmm13, xmm13
       vorps    xmm14, xmm15, xmm14
       vxorps   xmm15, xmm15, xmm15
       vcmpgtps xmm15, xmm13, xmm15
       vorps    xmm14, xmm15, xmm14
       vandps   xmm13, xmm14, xmm13
       vmulss   xmm14, xmm13, xmm13
       vmulss   xmm13, xmm13, xmm2
       vmulss   xmm13, xmm13, xmm14
       vaddss   xmm13, xmm13, xmm4
       vmulss   xmm14, xmm14, xmm5
       vaddss   xmm14, xmm14, xmm6
       vdivss   xmm13, xmm13, xmm14
       vsubss   xmm11, xmm11, xmm12
       vmulss   xmm11, xmm13, xmm11
       vmulss   xmm11, xmm11, xmm11
       vminss   xmm11, xmm7, xmm11
       vmulss   xmm11, xmm11, xmm0
       vaddss   xmm11, xmm11, xmm9
       vsqrtss  xmm11, xmm11, xmm11
       vmulss   xmm11, xmm11, xmm3
       vaddss   xmm10, xmm11, xmm10
       inc      esi

G_M000_IG18:                ;; offset=0x0AF4
       cmp      esi, 4
       jl       G_M000_IG17

G_M000_IG19:                ;; offset=0x0AFD
       lea      esi, [r11+0x02]
       mov      rdi, gword ptr [rcx+0x10]
       cmp      esi, ebp
       jae      G_M000_IG30
       mov      r14d, esi
       vmovss   xmm11, dword ptr [rdi+4*r14+0x10]
       mov      rdi, rbx
       lea      r14d, [rsi-0x01]
       cmp      r14d, ebp
       jae      G_M000_IG30
       lea      r14d, [rsi-0x01]
       vmovss   xmm12, dword ptr [rdi+4*r14+0x10]
       mov      rdi, rbx
       lea      r14d, [rsi+0x01]
       cmp      r14d, ebp
       jae      G_M000_IG30
       lea      r14d, [rsi+0x01]
       vaddss   xmm12, xmm12, dword ptr [rdi+4*r14+0x10]
       mov      rdi, rbx
       mov      r14d, esi
       sub      r14d, eax
       cmp      r14d, ebp
       jae      G_M000_IG30
       vmovss   xmm13, dword ptr [rdi+4*r14+0x10]
       mov      rdi, rbx
       lea      r14d, [rsi+rax]
       cmp      r14d, ebp
       jae      G_M000_IG30
       vaddss   xmm13, xmm13, dword ptr [rdi+4*r14+0x10]
       vaddss   xmm12, xmm12, xmm13
       vmulss   xmm12, xmm12, xmm3
       vaddss   xmm13, xmm11, xmm1
       vxorps   xmm14, xmm14, xmm14
       vcmpneqps xmm15, xmm13, xmm13
       vorps    xmm14, xmm15, xmm14
       vxorps   xmm15, xmm15, xmm15
       vcmpgtps xmm15, xmm13, xmm15
       vorps    xmm14, xmm15, xmm14
       vandps   xmm13, xmm14, xmm13
       vmulss   xmm14, xmm13, xmm13
       vmulss   xmm13, xmm13, xmm2
       vmulss   xmm13, xmm13, xmm14
       vaddss   xmm13, xmm13, xmm4
       vmulss   xmm14, xmm14, xmm5
       vaddss   xmm14, xmm14, xmm6
       vdivss   xmm13, xmm13, xmm14
       vsubss   xmm11, xmm11, xmm12
       vmulss   xmm11, xmm13, xmm11
       vmulss   xmm11, xmm11, xmm11
       vminss   xmm11, xmm7, xmm11
       vmulss   xmm11, xmm11, dword ptr [reloc @RWD00]
       vaddss   xmm11, xmm11, xmm9
       vsqrtss  xmm11, xmm11, xmm11
       vmulss   xmm11, xmm11, xmm3
       mov      edi, 1

G_M000_IG20:                ;; offset=0x0BFD
       align    [0 bytes for IG21]

G_M000_IG21:                ;; offset=0x0BFD
       mov      r14d, edi
       imul     r14d, eax
       add      r14d, esi
       mov      r15, rbx
       cmp      r14d, ebp
       jae      G_M000_IG30
       mov      r13d, r14d
       vmovss   xmm12, dword ptr [r15+4*r13+0x10]
       mov      r15, rbx
       lea      r13d, [r14-0x01]
       cmp      r13d, ebp
       jae      G_M000_IG30
       lea      r13d, [r14-0x01]
       vmovss   xmm13, dword ptr [r15+4*r13+0x10]
       mov      r15, rbx
       lea      r13d, [r14+0x01]
       cmp      r13d, ebp
       jae      G_M000_IG30
       lea      r13d, [r14+0x01]
       vaddss   xmm13, xmm13, dword ptr [r15+4*r13+0x10]
       mov      r15, rbx
       mov      r13d, r14d
       sub      r13d, eax
       cmp      r13d, ebp
       jae      G_M000_IG30
       vmovss   xmm14, dword ptr [r15+4*r13+0x10]
       mov      r15, rbx
       add      r14d, eax
       cmp      r14d, ebp
       jae      G_M000_IG30
       vaddss   xmm14, xmm14, dword ptr [r15+4*r14+0x10]
       vaddss   xmm13, xmm13, xmm14
       vmulss   xmm13, xmm13, xmm3
       vaddss   xmm14, xmm12, xmm1
       vxorps   xmm15, xmm15, xmm15
       vcmpneqps xmm8, xmm14, xmm14
       vorps    xmm8, xmm8, xmm15
       vcmpgtps xmm15, xmm14, xmm15
       vorps    xmm8, xmm15, xmm8
       vandps   xmm8, xmm8, xmm14
       vmulss   xmm14, xmm8, xmm8
       vmulss   xmm8, xmm8, xmm2
       vmulss   xmm8, xmm8, xmm14
       vaddss   xmm8, xmm8, xmm4
       vmulss   xmm14, xmm14, xmm5
       vaddss   xmm14, xmm14, xmm6
       vdivss   xmm8, xmm8, xmm14
       vsubss   xmm12, xmm12, xmm13
       vmulss   xmm8, xmm8, xmm12
       vmulss   xmm8, xmm8, xmm8
       vminss   xmm8, xmm7, xmm8
       vmulss   xmm8, xmm8, xmm0
       vaddss   xmm8, xmm8, xmm9
       vsqrtss  xmm8, xmm8, xmm8
       vmulss   xmm8, xmm8, xmm3
       vaddss   xmm11, xmm8, xmm11
       inc      edi
       cmp      edi, 4

G_M000_IG22:                ;; offset=0x0CFE
       jl       G_M000_IG21

G_M000_IG23:                ;; offset=0x0D04
       vmovss   dword ptr [rsp+0x78], xmm11
       add      r11d, 3
       mov      rsi, gword ptr [rcx+0x10]
       cmp      r11d, ebp
       jae      G_M000_IG30
       mov      edi, r11d
       vmovss   xmm12, dword ptr [rsi+4*rdi+0x10]
       mov      rsi, rbx
       lea      edi, [r11-0x01]
       cmp      edi, ebp
       jae      G_M000_IG30
       lea      edi, [r11-0x01]
       vmovss   xmm13, dword ptr [rsi+4*rdi+0x10]
       mov      rsi, rbx
       lea      edi, [r11+0x01]
       cmp      edi, ebp
       jae      G_M000_IG30
       lea      edi, [r11+0x01]
       vaddss   xmm13, xmm13, dword ptr [rsi+4*rdi+0x10]
       mov      rsi, rbx
       mov      edi, r11d
       sub      edi, eax
       cmp      edi, ebp
       jae      G_M000_IG30
       vmovss   xmm14, dword ptr [rsi+4*rdi+0x10]
       mov      rsi, rbx
       lea      edi, [r11+rax]
       cmp      edi, ebp
       jae      G_M000_IG30
       vaddss   xmm14, xmm14, dword ptr [rsi+4*rdi+0x10]
       vaddss   xmm13, xmm13, xmm14
       vmulss   xmm13, xmm13, xmm3
       vaddss   xmm14, xmm12, xmm1
       vxorps   xmm15, xmm15, xmm15
       vcmpneqps xmm8, xmm14, xmm14
       vorps    xmm8, xmm8, xmm15
       vcmpgtps xmm15, xmm14, xmm15
       vorps    xmm8, xmm15, xmm8
       vandps   xmm8, xmm8, xmm14
       vmulss   xmm14, xmm8, xmm8
       vmulss   xmm8, xmm8, xmm2
       vmulss   xmm8, xmm8, xmm14
       vaddss   xmm8, xmm8, xmm4
       vmulss   xmm14, xmm14, xmm5
       vaddss   xmm14, xmm14, xmm6
       vdivss   xmm8, xmm8, xmm14
       vsubss   xmm12, xmm12, xmm13
       vmulss   xmm8, xmm8, xmm12
       vmulss   xmm8, xmm8, xmm8
       vminss   xmm8, xmm7, xmm8
       vmulss   xmm8, xmm8, dword ptr [reloc @RWD00]
       vaddss   xmm8, xmm8, xmm9
       vsqrtss  xmm8, xmm8, xmm8
       vmulss   xmm8, xmm8, xmm3
       mov      esi, 1
       align    [0 bytes for IG24]

G_M000_IG24:                ;; offset=0x0DFC
       mov      edi, esi
       imul     edi, eax
       add      edi, r11d
       mov      r14, rbx
       cmp      edi, ebp
       jae      G_M000_IG30
       mov      r15d, edi
       vmovss   xmm12, dword ptr [r14+4*r15+0x10]
       mov      r14, rbx
       lea      r15d, [rdi-0x01]
       cmp      r15d, ebp
       jae      G_M000_IG30
       lea      r15d, [rdi-0x01]
       vmovss   xmm13, dword ptr [r14+4*r15+0x10]
       mov      r14, rbx
       lea      r15d, [rdi+0x01]
       cmp      r15d, ebp
       jae      G_M000_IG30
       lea      r15d, [rdi+0x01]
       vaddss   xmm13, xmm13, dword ptr [r14+4*r15+0x10]
       mov      r14, rbx
       mov      r15d, edi
       sub      r15d, eax
       cmp      r15d, ebp
       jae      G_M000_IG30
       vmovss   xmm14, dword ptr [r14+4*r15+0x10]
       mov      r14, rbx
       add      edi, eax
       cmp      edi, ebp
       jae      G_M000_IG30
       vaddss   xmm14, xmm14, dword ptr [r14+4*rdi+0x10]
       vaddss   xmm13, xmm13, xmm14
       vmulss   xmm13, xmm13, xmm3
       vaddss   xmm14, xmm12, xmm1
       vxorps   xmm15, xmm15, xmm15
       vcmpneqps xmm11, xmm14, xmm14
       vorps    xmm11, xmm11, xmm15
       vcmpgtps xmm15, xmm14, xmm15
       vorps    xmm11, xmm15, xmm11
       vandps   xmm11, xmm11, xmm14
       vmulss   xmm14, xmm11, xmm11
       vmulss   xmm11, xmm11, xmm2
       vmulss   xmm11, xmm11, xmm14
       vaddss   xmm11, xmm11, xmm4
       vmulss   xmm14, xmm14, xmm5
       vaddss   xmm14, xmm14, xmm6
       vdivss   xmm11, xmm11, xmm14
       vsubss   xmm12, xmm12, xmm13
       vmulss   xmm11, xmm11, xmm12
       vmulss   xmm11, xmm11, xmm11
       vminss   xmm11, xmm7, xmm11
       vmulss   xmm11, xmm11, xmm0
       vaddss   xmm11, xmm11, xmm9
       vsqrtss  xmm11, xmm11, xmm11
       vmulss   xmm11, xmm11, xmm3
       vaddss   xmm8, xmm11, xmm8
       inc      esi
       cmp      esi, 4

G_M000_IG25:                ;; offset=0x0EF8
       jl       G_M000_IG24

G_M000_IG26:                ;; offset=0x0EFE
       mov      r11, gword ptr [rcx+0x18]
       mov      ebx, r10d
       sar      ebx, 31
       and      ebx, 3
       add      ebx, r10d
       sar      ebx, 2
       add      ebx, r8d
       cmp      ebx, dword ptr [r11+0x08]
       jae      G_M000_IG30
       vmovss   xmm9, dword ptr [rsp+0x7C]
       vaddss   xmm9, xmm9, xmm10
       vaddss   xmm9, xmm9, dword ptr [rsp+0x78]
       vaddss   xmm8, xmm9, xmm8
       vmulss   xmm8, xmm8, xmm3
       vmovss   dword ptr [r11+4*rbx+0x10], xmm8
       add      r10d, 4

G_M000_IG27:                ;; offset=0x0F43
       cmp      r10d, r9d
       jl       G_M000_IG12

G_M000_IG28:                ;; offset=0x0F4C
       vzeroupper
       vmovaps  xmm6, xmmword ptr [rsp+0x110]
       vmovaps  xmm7, xmmword ptr [rsp+0x100]
       vmovaps  xmm8, xmmword ptr [rsp+0xF0]
       vmovaps  xmm9, xmmword ptr [rsp+0xE0]
       vmovaps  xmm10, xmmword ptr [rsp+0xD0]
       vmovaps  xmm11, xmmword ptr [rsp+0xC0]
       vmovaps  xmm12, xmmword ptr [rsp+0xB0]
       vmovaps  xmm13, xmmword ptr [rsp+0xA0]
       vmovaps  xmm14, xmmword ptr [rsp+0x90]
       vmovaps  xmm15, xmmword ptr [rsp+0x80]
       add      rsp, 288
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r13
       pop      r14
       pop      r15
       ret

G_M000_IG29:                ;; offset=0x0FBB
       call     [System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()]
       int3

G_M000_IG30:                ;; offset=0x0FC2
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	480E0640h		;    145433
RWD04  	dd	3E800000h		;      0.25
RWD08  	dd	409B0A3Dh		;     4.845
RWD12  	dd	3B1DEA78h		; 0.00240961
RWD16  	dd	449E380Dh		;   1265.75
RWD20  	dd	42EF0C23h		;   119.524
RWD24  	dd	44228FFEh		;    650.25
RWD28  	dd	3E4CCCCDh		;       0.2
RWD32  	dd	41E00000h		;        28

; Total bytes of code 4040

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 33 single block inlinees; 11 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r15
       push     r14
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 360
       vmovaps  xmmword ptr [rsp+0x150], xmm6
       vmovaps  xmmword ptr [rsp+0x140], xmm7
       vmovaps  xmmword ptr [rsp+0x130], xmm8
       vmovaps  xmmword ptr [rsp+0x120], xmm9
       vmovaps  xmmword ptr [rsp+0x110], xmm10
       vmovaps  xmmword ptr [rsp+0x100], xmm11
       vmovaps  xmmword ptr [rsp+0xF0], xmm12
       vmovaps  xmmword ptr [rsp+0xE0], xmm13
       vmovaps  xmmword ptr [rsp+0xD0], xmm14
       vmovaps  xmmword ptr [rsp+0xC0], xmm15
       mov      rbx, rcx
       mov      esi, edx
       mov      edi, r8d

G_M000_IG02:                ;; offset=0x0071
       xor      ebp, ebp
       test     r9b, r9b
       je       G_M000_IG23

G_M000_IG03:                ;; offset=0x007C
       cmp      dword ptr [rbx+0x50], 9
       jle      G_M000_IG23
       mov      ebp, 1
       mov      rcx, rbx
       mov      edx, esi
       mov      r8d, edi
       xor      r9d, r9d
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this]
       mov      ecx, dword ptr [rbx+0x50]
       lea      edx, [rcx-0x08]
       cmp      edx, 1
       jle      G_M000_IG23
       align    [0 bytes for IG04]

G_M000_IG04:                ;; offset=0x00AB
       lea      edx, [rsi-0x01]
       imul     edx, ecx
       add      edx, ebp
       mov      r8d, esi
       imul     r8d, ecx
       add      r8d, ebp
       lea      r9d, [rsi+0x01]
       imul     r9d, ecx
       add      r9d, ebp
       mov      rax, gword ptr [rbx+0x18]
       mov      r10, rax
       lea      r11d, [rdx-0x01]
       test     r11d, r11d
       jl       G_M000_IG26
       mov      r14d, dword ptr [r10+0x08]
       mov      r15d, r14d
       sub      r15d, r11d
       cmp      r15d, 8
       jl       G_M000_IG26
       cmp      r11d, r14d
       jae      G_M000_IG29
       vmovups  ymm0, ymmword ptr [r10+4*r11+0x10]
       mov      r10, rax
       test     edx, edx
       jl       G_M000_IG26
       mov      r11d, r14d
       sub      r11d, edx
       cmp      r11d, 8
       jl       G_M000_IG26
       cmp      edx, r14d
       jae      G_M000_IG29
       mov      r11d, edx
       vmovups  ymm1, ymmword ptr [r10+4*r11+0x10]
       mov      r10, rax
       inc      edx
       mov      r11d, r14d
       sub      r11d, edx
       cmp      r11d, 8
       jl       G_M000_IG26
       cmp      edx, r14d
       jae      G_M000_IG29
       vmovups  ymm2, ymmword ptr [r10+4*rdx+0x10]
       mov      rdx, rax
       lea      r10d, [r8-0x01]
       test     r10d, r10d
       jl       G_M000_IG26
       mov      r11d, r14d
       sub      r11d, r10d
       cmp      r11d, 8
       jl       G_M000_IG26
       cmp      r10d, r14d
       jae      G_M000_IG29
       vmovups  ymm3, ymmword ptr [rdx+4*r10+0x10]
       mov      rdx, rax
       test     r8d, r8d

G_M000_IG05:                ;; offset=0x0189
       jl       G_M000_IG26
       mov      r10d, r14d
       sub      r10d, r8d
       cmp      r10d, 8
       jl       G_M000_IG26
       cmp      r8d, r14d
       jae      G_M000_IG29
       mov      r10d, r8d
       vmovups  ymm4, ymmword ptr [rdx+4*r10+0x10]
       mov      rdx, rax
       inc      r8d
       mov      r10d, r14d
       sub      r10d, r8d
       cmp      r10d, 8
       jl       G_M000_IG26
       cmp      r8d, r14d
       jae      G_M000_IG29
       vmovups  ymm5, ymmword ptr [rdx+4*r8+0x10]
       mov      rdx, rax
       lea      r8d, [r9-0x01]
       test     r8d, r8d
       jl       G_M000_IG26
       mov      r10d, r14d
       sub      r10d, r8d
       cmp      r10d, 8
       jl       G_M000_IG26
       cmp      r8d, r14d
       jae      G_M000_IG29
       vmovups  ymm6, ymmword ptr [rdx+4*r8+0x10]
       mov      rdx, rax
       test     r9d, r9d
       jl       G_M000_IG26
       mov      r8d, r14d
       sub      r8d, r9d
       cmp      r8d, 8
       jl       G_M000_IG26
       cmp      r9d, r14d
       jae      G_M000_IG29
       mov      r8d, r9d
       vmovups  ymm7, ymmword ptr [rdx+4*r8+0x10]
       inc      r9d
       mov      edx, r14d
       sub      edx, r9d
       cmp      edx, 8
       jl       G_M000_IG26
       cmp      r9d, r14d
       jae      G_M000_IG29
       mov      edx, r9d
       vmovups  ymm8, ymmword ptr [rax+4*rdx+0x10]
       cmp      byte  ptr [rbx+0x60], 0

G_M000_IG06:                ;; offset=0x025F
       jne      G_M000_IG15

G_M000_IG07:                ;; offset=0x0265
       vcmpeqps ymm9, ymm0, ymm1
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm1
       vandps   ymm10, ymm10, ymm9
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm10, ymm10, ymm11
       vcmpltps ymm12, ymm1, ymm0
       vorps    ymm10, ymm12, ymm10
       vandps   ymm12, ymm10, ymm0
       vandnps  ymm10, ymm10, ymm1
       vorps    ymm10, ymm10, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm0
       vandps   ymm9, ymm12, ymm9
       vorps    ymm11, ymm9, ymm11
       vcmpltps ymm9, ymm0, ymm1
       vorps    ymm9, ymm9, ymm11
       vandps   ymm0, ymm9, ymm0
       vandnps  ymm1, ymm9, ymm1
       vorps    ymm0, ymm1, ymm0
       vcmpeqps ymm1, ymm2, ymm3
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm3
       vandps   ymm9, ymm9, ymm1
       vcmpneqps ymm11, ymm2, ymm2
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm12, ymm3, ymm2
       vorps    ymm9, ymm12, ymm9
       vandps   ymm12, ymm9, ymm2
       vandnps  ymm9, ymm9, ymm3
       vorps    ymm9, ymm9, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm2
       vandps   ymm1, ymm12, ymm1
       vorps    ymm1, ymm11, ymm1
       vcmpltps ymm11, ymm2, ymm3
       vorps    ymm1, ymm11, ymm1
       vandps   ymm2, ymm2, ymm1
       vandnps  ymm3, ymm1, ymm3
       vorps    ymm2, ymm3, ymm2
       vcmpeqps ymm1, ymm0, ymm2
       vxorps   ymm3, ymm3, ymm3
       vpcmpgtd ymm3, ymm3, ymm2
       vandps   ymm3, ymm3, ymm1
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm12, ymm2, ymm0
       vorps    ymm3, ymm12, ymm3
       vandps   ymm12, ymm0, ymm3
       vandnps  ymm3, ymm3, ymm2
       vorps    ymm3, ymm12, ymm3
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm0
       vandps   ymm1, ymm12, ymm1
       vorps    ymm1, ymm11, ymm1
       vcmpltps ymm11, ymm0, ymm2
       vorps    ymm1, ymm11, ymm1
       vandps   ymm0, ymm0, ymm1
       vandnps  ymm2, ymm1, ymm2

G_M000_IG08:                ;; offset=0x036C
       vorps    ymm0, ymm2, ymm0
       vcmpeqps ymm1, ymm10, ymm9
       vxorps   ymm2, ymm2, ymm2
       vpcmpgtd ymm2, ymm2, ymm9
       vandps   ymm2, ymm2, ymm1
       vcmpneqps ymm11, ymm10, ymm10
       vorps    ymm2, ymm11, ymm2
       vcmpltps ymm12, ymm9, ymm10
       vorps    ymm2, ymm12, ymm2
       vandps   ymm12, ymm10, ymm2
       vandnps  ymm2, ymm2, ymm9
       vorps    ymm2, ymm12, ymm2
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm10
       vandps   ymm1, ymm12, ymm1
       vorps    ymm1, ymm11, ymm1
       vcmpltps ymm11, ymm10, ymm9
       vorps    ymm1, ymm11, ymm1
       vandps   ymm10, ymm10, ymm1
       vandnps  ymm1, ymm1, ymm9
       vorps    ymm1, ymm10, ymm1
       vcmpeqps ymm9, ymm1, ymm3
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm3
       vandps   ymm10, ymm10, ymm9
       vcmpneqps ymm11, ymm1, ymm1
       vorps    ymm10, ymm10, ymm11
       vcmpltps ymm12, ymm3, ymm1
       vorps    ymm10, ymm12, ymm10
       vandps   ymm12, ymm10, ymm1
       vandnps  ymm10, ymm10, ymm3
       vorps    ymm10, ymm10, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm1
       vandps   ymm9, ymm12, ymm9
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm11, ymm1, ymm3
       vorps    ymm9, ymm11, ymm9
       vandps   ymm1, ymm9, ymm1
       vandnps  ymm3, ymm9, ymm3
       vorps    ymm1, ymm3, ymm1
       vcmpeqps ymm3, ymm0, ymm4
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm4
       vandps   ymm9, ymm9, ymm3
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm12, ymm4, ymm0
       vorps    ymm9, ymm12, ymm9
       vandps   ymm12, ymm9, ymm0
       vandnps  ymm9, ymm9, ymm4
       vorps    ymm9, ymm9, ymm12
       vcmpeqps ymm12, ymm1, ymm9
       vxorps   ymm13, ymm13, ymm13
       vpcmpgtd ymm13, ymm13, ymm9
       vandps   ymm13, ymm13, ymm12
       vcmpneqps ymm14, ymm1, ymm1
       vmovups  ymmword ptr [rsp+0xA0], ymm14
       vorps    ymm13, ymm13, ymm14

G_M000_IG09:                ;; offset=0x0485
       vcmpltps ymm15, ymm9, ymm1
       vorps    ymm13, ymm15, ymm13
       vandps   ymm15, ymm13, ymm1
       vandnps  ymm13, ymm13, ymm9
       vorps    ymm13, ymm13, ymm15
       vcmpeqps ymm15, ymm10, ymm13
       vmovups  ymmword ptr [rsp+0x80], ymm15
       vxorps   ymm14, ymm14, ymm14
       vpcmpgtd ymm14, ymm14, ymm13
       vandps   ymm14, ymm14, ymm15
       vcmpneqps ymm15, ymm10, ymm10
       vmovups  ymmword ptr [rsp+0x60], ymm15
       vorps    ymm14, ymm14, ymm15
       vcmpltps ymm15, ymm13, ymm10
       vorps    ymm14, ymm15, ymm14
       vandps   ymm15, ymm10, ymm14
       vandnps  ymm14, ymm14, ymm13
       vorps    ymm14, ymm14, ymm15
       vxorps   ymm15, ymm15, ymm15
       vpcmpgtd ymm15, ymm15, ymm0
       vandps   ymm3, ymm15, ymm3
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm11, ymm0, ymm4
       vorps    ymm3, ymm11, ymm3
       vandps   ymm0, ymm0, ymm3
       vandnps  ymm4, ymm3, ymm4
       vorps    ymm0, ymm4, ymm0
       vxorps   ymm3, ymm3, ymm3
       vpcmpgtd ymm3, ymm3, ymm1
       vandps   ymm3, ymm12, ymm3
       vorps    ymm3, ymm3, ymmword ptr [rsp+0xA0]
       vcmpltps ymm4, ymm1, ymm9
       vorps    ymm3, ymm4, ymm3
       vandps   ymm1, ymm1, ymm3
       vandnps  ymm3, ymm3, ymm9
       vorps    ymm1, ymm3, ymm1
       vxorps   ymm3, ymm3, ymm3
       vpcmpgtd ymm3, ymm3, ymm10
       vandps   ymm3, ymm3, ymmword ptr [rsp+0x80]
       vorps    ymm3, ymm3, ymmword ptr [rsp+0x60]
       vcmpltps ymm4, ymm10, ymm13
       vorps    ymm3, ymm4, ymm3
       vandps   ymm4, ymm10, ymm3
       vandnps  ymm3, ymm3, ymm13
       vorps    ymm3, ymm3, ymm4
       vcmpeqps ymm4, ymm2, ymm14
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm2
       vandps   ymm4, ymm9, ymm4
       vcmpneqps ymm9, ymm2, ymm2
       vorps    ymm4, ymm9, ymm4
       vcmpltps ymm9, ymm2, ymm14
       vorps    ymm4, ymm9, ymm4
       vblendvps ymm2, ymm14, ymm2, ymm4
       vcmpeqps ymm4, ymm0, ymm5
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm5
       vandps   ymm9, ymm9, ymm4
       vcmpneqps ymm10, ymm0, ymm0

G_M000_IG10:                ;; offset=0x05AA
       vorps    ymm9, ymm9, ymm10
       vcmpltps ymm11, ymm5, ymm0
       vorps    ymm9, ymm11, ymm9
       vandps   ymm11, ymm9, ymm0
       vandnps  ymm9, ymm9, ymm5
       vorps    ymm9, ymm9, ymm11
       vcmpeqps ymm11, ymm1, ymm9
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm9
       vandps   ymm12, ymm12, ymm11
       vcmpneqps ymm13, ymm1, ymm1
       vorps    ymm12, ymm12, ymm13
       vcmpltps ymm14, ymm9, ymm1
       vorps    ymm12, ymm14, ymm12
       vandps   ymm14, ymm12, ymm1
       vandnps  ymm12, ymm12, ymm9
       vorps    ymm12, ymm12, ymm14
       vcmpeqps ymm14, ymm3, ymm12
       vmovups  ymmword ptr [rsp+0x40], ymm14
       vxorps   ymm15, ymm15, ymm15
       vpcmpgtd ymm15, ymm15, ymm12
       vandps   ymm15, ymm15, ymm14
       vcmpneqps ymm14, ymm3, ymm3
       vmovups  ymmword ptr [rsp+0x20], ymm14
       vorps    ymm15, ymm15, ymm14
       vcmpltps ymm14, ymm12, ymm3
       vorps    ymm14, ymm14, ymm15
       vandps   ymm15, ymm14, ymm3
       vandnps  ymm14, ymm14, ymm12
       vorps    ymm14, ymm14, ymm15
       vxorps   ymm15, ymm15, ymm15
       vpcmpgtd ymm15, ymm15, ymm0
       vandps   ymm4, ymm15, ymm4
       vorps    ymm4, ymm10, ymm4
       vcmpltps ymm10, ymm0, ymm5
       vorps    ymm4, ymm10, ymm4
       vandps   ymm0, ymm0, ymm4
       vandnps  ymm5, ymm4, ymm5
       vorps    ymm0, ymm5, ymm0
       vxorps   ymm4, ymm4, ymm4
       vpcmpgtd ymm4, ymm4, ymm1
       vandps   ymm4, ymm11, ymm4
       vorps    ymm4, ymm13, ymm4
       vcmpltps ymm5, ymm1, ymm9
       vorps    ymm4, ymm5, ymm4
       vandps   ymm1, ymm1, ymm4
       vandnps  ymm4, ymm4, ymm9
       vorps    ymm1, ymm4, ymm1
       vxorps   ymm4, ymm4, ymm4
       vpcmpgtd ymm4, ymm4, ymm3
       vandps   ymm4, ymm4, ymmword ptr [rsp+0x40]
       vorps    ymm4, ymm4, ymmword ptr [rsp+0x20]
       vcmpltps ymm5, ymm3, ymm12
       vorps    ymm4, ymm5, ymm4
       vandps   ymm3, ymm3, ymm4
       vandnps  ymm4, ymm4, ymm12
       vorps    ymm3, ymm4, ymm3
       vcmpeqps ymm4, ymm2, ymm14
       vxorps   ymm5, ymm5, ymm5

G_M000_IG11:                ;; offset=0x06C2
       vpcmpgtd ymm5, ymm5, ymm2
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm2, ymm2
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm2, ymm14
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm2, ymm14, ymm2, ymm4
       vcmpeqps ymm4, ymm0, ymm6
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm6
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm0, ymm0
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm6, ymm0
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm4, ymm6, ymm0, ymm4
       vcmpeqps ymm5, ymm1, ymm4
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm4
       vandps   ymm5, ymm9, ymm5
       vcmpneqps ymm9, ymm1, ymm1
       vorps    ymm5, ymm9, ymm5
       vcmpltps ymm9, ymm4, ymm1
       vorps    ymm5, ymm9, ymm5
       vblendvps ymm5, ymm4, ymm1, ymm5
       vcmpeqps ymm9, ymm3, ymm5
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm5
       vandps   ymm9, ymm10, ymm9
       vcmpneqps ymm10, ymm3, ymm3
       vorps    ymm9, ymm10, ymm9
       vcmpltps ymm10, ymm5, ymm3
       vorps    ymm9, ymm10, ymm9
       vblendvps ymm9, ymm5, ymm3, ymm9
       vcmpeqps ymm10, ymm0, ymm6
       vxorps   ymm11, ymm11, ymm11
       vpcmpgtd ymm11, ymm11, ymm0
       vandps   ymm10, ymm11, ymm10
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm10, ymm11, ymm10
       vcmpltps ymm11, ymm0, ymm6
       vorps    ymm10, ymm11, ymm10
       vblendvps ymm0, ymm6, ymm0, ymm10
       vcmpeqps ymm6, ymm1, ymm4
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm1
       vandps   ymm6, ymm10, ymm6
       vcmpneqps ymm10, ymm1, ymm1
       vorps    ymm6, ymm10, ymm6
       vcmpltps ymm10, ymm1, ymm4
       vorps    ymm6, ymm10, ymm6
       vblendvps ymm1, ymm4, ymm1, ymm6
       vcmpeqps ymm4, ymm3, ymm5
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm3
       vandps   ymm4, ymm6, ymm4
       vcmpneqps ymm6, ymm3, ymm3
       vorps    ymm4, ymm6, ymm4
       vcmpltps ymm6, ymm3, ymm5

G_M000_IG12:                ;; offset=0x07D9
       vorps    ymm4, ymm6, ymm4
       vblendvps ymm3, ymm5, ymm3, ymm4
       vcmpeqps ymm4, ymm2, ymm9
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm2
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm2, ymm2
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm2, ymm9
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm2, ymm9, ymm2, ymm4
       vcmpeqps ymm4, ymm0, ymm7
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm7
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm0, ymm0
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm7, ymm0
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm4, ymm7, ymm0, ymm4
       vcmpeqps ymm5, ymm1, ymm4
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm4
       vandps   ymm5, ymm6, ymm5
       vcmpneqps ymm6, ymm1, ymm1
       vorps    ymm5, ymm6, ymm5
       vcmpltps ymm6, ymm4, ymm1
       vorps    ymm5, ymm6, ymm5
       vblendvps ymm5, ymm4, ymm1, ymm5
       vcmpeqps ymm6, ymm3, ymm5
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm5
       vandps   ymm6, ymm9, ymm6
       vcmpneqps ymm9, ymm3, ymm3
       vorps    ymm6, ymm9, ymm6
       vcmpltps ymm9, ymm5, ymm3
       vorps    ymm6, ymm9, ymm6
       vblendvps ymm6, ymm5, ymm3, ymm6
       vcmpeqps ymm9, ymm0, ymm7
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm0
       vandps   ymm9, ymm10, ymm9
       vcmpneqps ymm10, ymm0, ymm0
       vorps    ymm9, ymm10, ymm9
       vcmpltps ymm10, ymm0, ymm7
       vorps    ymm9, ymm10, ymm9
       vblendvps ymm0, ymm7, ymm0, ymm9
       vcmpeqps ymm7, ymm1, ymm4
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm1
       vandps   ymm7, ymm9, ymm7
       vcmpneqps ymm9, ymm1, ymm1
       vorps    ymm7, ymm9, ymm7
       vcmpltps ymm9, ymm1, ymm4
       vorps    ymm7, ymm9, ymm7
       vblendvps ymm1, ymm4, ymm1, ymm7
       vcmpeqps ymm4, ymm3, ymm5
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm3

G_M000_IG13:                ;; offset=0x08EE
       vandps   ymm4, ymm7, ymm4
       vcmpneqps ymm7, ymm3, ymm3
       vorps    ymm4, ymm7, ymm4
       vcmpltps ymm7, ymm3, ymm5
       vorps    ymm4, ymm7, ymm4
       vblendvps ymm3, ymm5, ymm3, ymm4
       vcmpeqps ymm4, ymm2, ymm6
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm2
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm2, ymm2
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm2, ymm6
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm2, ymm6, ymm2, ymm4
       vcmpeqps ymm4, ymm0, ymm8
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm8
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm0, ymm0
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm8, ymm0
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm4, ymm8, ymm0, ymm4
       vcmpeqps ymm5, ymm1, ymm4
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm4
       vandps   ymm5, ymm6, ymm5
       vcmpneqps ymm6, ymm1, ymm1
       vorps    ymm5, ymm6, ymm5
       vcmpltps ymm6, ymm4, ymm1
       vorps    ymm5, ymm6, ymm5
       vblendvps ymm5, ymm4, ymm1, ymm5
       vcmpeqps ymm6, ymm3, ymm5
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm5
       vandps   ymm6, ymm7, ymm6
       vcmpneqps ymm7, ymm3, ymm3
       vorps    ymm6, ymm7, ymm6
       vcmpltps ymm7, ymm5, ymm3
       vorps    ymm6, ymm7, ymm6
       vblendvps ymm6, ymm5, ymm3, ymm6
       vcmpeqps ymm7, ymm0, ymm8
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm0
       vandps   ymm7, ymm9, ymm7
       vcmpneqps ymm9, ymm0, ymm0
       vorps    ymm7, ymm9, ymm7
       vcmpltps ymm9, ymm0, ymm8
       vorps    ymm7, ymm9, ymm7
       vblendvps ymm0, ymm8, ymm0, ymm7
       vcmpeqps ymm8, ymm1, ymm4
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm1
       vandps   ymm7, ymm8, ymm7
       vcmpneqps ymm8, ymm1, ymm1
       vorps    ymm7, ymm8, ymm7
       vcmpltps ymm8, ymm1, ymm4
       vorps    ymm7, ymm8, ymm7

G_M000_IG14:                ;; offset=0x09FF
       vblendvps ymm1, ymm4, ymm1, ymm7
       vcmpeqps ymm4, ymm3, ymm5
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm3
       vandps   ymm4, ymm7, ymm4
       vcmpneqps ymm7, ymm3, ymm3
       vorps    ymm4, ymm7, ymm4
       vcmpltps ymm7, ymm3, ymm5
       vorps    ymm4, ymm7, ymm4
       vblendvps ymm3, ymm5, ymm3, ymm4
       vcmpeqps ymm4, ymm2, ymm6
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm2
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm2, ymm2
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm2, ymm6
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm2, ymm6, ymm2, ymm4
       vmovaps  ymm5, ymm3
       jmp      G_M000_IG22

G_M000_IG15:                ;; offset=0x0A60
       vcmpeqps ymm9, ymm0, ymm3
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm3
       vandps   ymm10, ymm10, ymm9
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm10, ymm10, ymm11
       vcmpltps ymm12, ymm3, ymm0
       vorps    ymm10, ymm12, ymm10
       vandps   ymm12, ymm10, ymm0
       vandnps  ymm10, ymm10, ymm3
       vorps    ymm10, ymm10, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm0
       vandps   ymm9, ymm12, ymm9
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm11, ymm0, ymm3
       vorps    ymm9, ymm11, ymm9
       vandps   ymm0, ymm9, ymm0
       vandnps  ymm3, ymm9, ymm3
       vorps    ymm0, ymm3, ymm0
       vcmpeqps ymm3, ymm1, ymm7
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm7
       vandps   ymm9, ymm9, ymm3
       vcmpneqps ymm11, ymm1, ymm1
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm12, ymm7, ymm1
       vorps    ymm9, ymm12, ymm9
       vandps   ymm12, ymm9, ymm1
       vandnps  ymm9, ymm9, ymm7
       vorps    ymm9, ymm9, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm1
       vandps   ymm3, ymm12, ymm3
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm11, ymm1, ymm7
       vorps    ymm3, ymm11, ymm3
       vandps   ymm1, ymm1, ymm3
       vandnps  ymm3, ymm3, ymm7
       vorps    ymm1, ymm3, ymm1
       vcmpeqps ymm3, ymm2, ymm5
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm5
       vandps   ymm7, ymm7, ymm3
       vcmpneqps ymm11, ymm2, ymm2
       vorps    ymm7, ymm11, ymm7
       vcmpltps ymm12, ymm5, ymm2
       vorps    ymm7, ymm12, ymm7
       vandps   ymm12, ymm2, ymm7
       vandnps  ymm7, ymm7, ymm5
       vorps    ymm7, ymm12, ymm7
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm2
       vandps   ymm3, ymm12, ymm3
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm11, ymm2, ymm5
       vorps    ymm3, ymm11, ymm3
       vandps   ymm2, ymm2, ymm3
       vandnps  ymm3, ymm3, ymm5

G_M000_IG16:                ;; offset=0x0B67
       vorps    ymm2, ymm3, ymm2
       vcmpeqps ymm3, ymm4, ymm8
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm8
       vandps   ymm5, ymm5, ymm3
       vcmpneqps ymm11, ymm4, ymm4
       vorps    ymm5, ymm11, ymm5
       vcmpltps ymm12, ymm8, ymm4
       vorps    ymm5, ymm12, ymm5
       vandps   ymm12, ymm4, ymm5
       vandnps  ymm5, ymm5, ymm8
       vorps    ymm5, ymm12, ymm5
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm4
       vandps   ymm3, ymm12, ymm3
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm11, ymm4, ymm8
       vorps    ymm3, ymm11, ymm3
       vandps   ymm4, ymm4, ymm3
       vandnps  ymm3, ymm3, ymm8
       vorps    ymm3, ymm3, ymm4
       vcmpeqps ymm4, ymm0, ymm9
       vxorps   ymm8, ymm8, ymm8
       vpcmpgtd ymm8, ymm8, ymm9
       vandps   ymm8, ymm8, ymm4
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm8, ymm8, ymm11
       vcmpltps ymm12, ymm9, ymm0
       vorps    ymm8, ymm12, ymm8
       vandps   ymm12, ymm8, ymm0
       vandnps  ymm8, ymm8, ymm9
       vorps    ymm8, ymm8, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm0
       vandps   ymm4, ymm12, ymm4
       vorps    ymm4, ymm11, ymm4
       vcmpltps ymm11, ymm0, ymm9
       vorps    ymm4, ymm11, ymm4
       vandps   ymm0, ymm0, ymm4
       vandnps  ymm4, ymm4, ymm9
       vorps    ymm0, ymm4, ymm0
       vcmpeqps ymm4, ymm2, ymm3
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm3
       vandps   ymm9, ymm9, ymm4
       vcmpneqps ymm11, ymm2, ymm2
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm12, ymm3, ymm2
       vorps    ymm9, ymm12, ymm9
       vandps   ymm12, ymm9, ymm2
       vandnps  ymm9, ymm9, ymm3
       vorps    ymm9, ymm9, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm2
       vandps   ymm4, ymm12, ymm4
       vorps    ymm4, ymm11, ymm4
       vcmpltps ymm11, ymm2, ymm3
       vorps    ymm4, ymm11, ymm4
       vandps   ymm2, ymm2, ymm4

G_M000_IG17:                ;; offset=0x0C74
       vandnps  ymm3, ymm4, ymm3
       vorps    ymm2, ymm3, ymm2
       vcmpeqps ymm3, ymm10, ymm5
       vxorps   ymm4, ymm4, ymm4
       vpcmpgtd ymm4, ymm4, ymm5
       vandps   ymm4, ymm4, ymm3
       vcmpneqps ymm11, ymm10, ymm10
       vorps    ymm4, ymm11, ymm4
       vcmpltps ymm12, ymm5, ymm10
       vorps    ymm4, ymm12, ymm4
       vandps   ymm12, ymm10, ymm4
       vandnps  ymm4, ymm4, ymm5
       vorps    ymm4, ymm12, ymm4
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm10
       vandps   ymm3, ymm12, ymm3
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm11, ymm10, ymm5
       vorps    ymm3, ymm11, ymm3
       vandps   ymm10, ymm10, ymm3
       vandnps  ymm3, ymm3, ymm5
       vorps    ymm3, ymm10, ymm3
       vcmpeqps ymm5, ymm6, ymm7
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm6
       vandps   ymm10, ymm10, ymm5
       vcmpneqps ymm11, ymm7, ymm7
       vorps    ymm10, ymm10, ymm11
       vcmpltps ymm12, ymm6, ymm7
       vorps    ymm10, ymm12, ymm10
       vandps   ymm12, ymm10, ymm7
       vandnps  ymm10, ymm10, ymm6
       vorps    ymm10, ymm10, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm7
       vandps   ymm5, ymm12, ymm5
       vorps    ymm5, ymm11, ymm5
       vcmpltps ymm11, ymm7, ymm6
       vorps    ymm5, ymm11, ymm5
       vandps   ymm7, ymm7, ymm5
       vandnps  ymm5, ymm5, ymm6
       vorps    ymm5, ymm5, ymm7
       vcmpeqps ymm6, ymm0, ymm2
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm2
       vandps   ymm7, ymm7, ymm6
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm7, ymm11, ymm7
       vcmpltps ymm12, ymm2, ymm0
       vorps    ymm7, ymm12, ymm7
       vandps   ymm12, ymm0, ymm7
       vandnps  ymm7, ymm7, ymm2
       vorps    ymm7, ymm12, ymm7
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm0
       vandps   ymm6, ymm12, ymm6
       vorps    ymm6, ymm11, ymm6
       vcmpltps ymm11, ymm0, ymm2
       vorps    ymm6, ymm11, ymm6

G_M000_IG18:                ;; offset=0x0D76
       vandps   ymm0, ymm0, ymm6
       vandnps  ymm2, ymm6, ymm2
       vorps    ymm0, ymm2, ymm0
       vcmpeqps ymm2, ymm1, ymm3
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm3
       vandps   ymm6, ymm6, ymm2
       vcmpneqps ymm11, ymm1, ymm1
       vorps    ymm6, ymm11, ymm6
       vcmpltps ymm12, ymm3, ymm1
       vorps    ymm6, ymm12, ymm6
       vandps   ymm12, ymm1, ymm6
       vandnps  ymm6, ymm6, ymm3
       vorps    ymm6, ymm12, ymm6
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm1
       vandps   ymm2, ymm12, ymm2
       vorps    ymm2, ymm11, ymm2
       vcmpltps ymm11, ymm1, ymm3
       vorps    ymm2, ymm11, ymm2
       vandps   ymm1, ymm1, ymm2
       vandnps  ymm3, ymm2, ymm3
       vorps    ymm1, ymm3, ymm1
       vcmpeqps ymm2, ymm5, ymm9
       vxorps   ymm3, ymm3, ymm3
       vpcmpgtd ymm3, ymm3, ymm5
       vandps   ymm3, ymm3, ymm2
       vcmpneqps ymm11, ymm9, ymm9
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm12, ymm5, ymm9
       vorps    ymm3, ymm12, ymm3
       vandps   ymm12, ymm9, ymm3
       vandnps  ymm3, ymm3, ymm5
       vorps    ymm3, ymm12, ymm3
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm9
       vandps   ymm2, ymm12, ymm2
       vorps    ymm2, ymm11, ymm2
       vcmpltps ymm11, ymm9, ymm5
       vorps    ymm2, ymm11, ymm2
       vandps   ymm9, ymm9, ymm2
       vandnps  ymm5, ymm2, ymm5
       vorps    ymm2, ymm9, ymm5
       vcmpeqps ymm5, ymm8, ymm4
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm8
       vandps   ymm5, ymm9, ymm5
       vcmpneqps ymm9, ymm8, ymm8
       vorps    ymm5, ymm9, ymm5
       vcmpltps ymm9, ymm8, ymm4
       vorps    ymm5, ymm9, ymm5
       vblendvps ymm4, ymm4, ymm8, ymm5
       vcmpeqps ymm5, ymm1, ymm2
       vxorps   ymm8, ymm8, ymm8
       vpcmpgtd ymm8, ymm8, ymm2
       vandps   ymm8, ymm8, ymm5
       vcmpneqps ymm9, ymm1, ymm1
       vorps    ymm8, ymm8, ymm9
       vcmpltps ymm11, ymm2, ymm1

G_M000_IG19:                ;; offset=0x0E7D
       vorps    ymm8, ymm11, ymm8
       vandps   ymm11, ymm8, ymm1
       vandnps  ymm8, ymm8, ymm2
       vorps    ymm8, ymm8, ymm11
       vxorps   ymm11, ymm11, ymm11
       vpcmpgtd ymm11, ymm11, ymm1
       vandps   ymm5, ymm11, ymm5
       vorps    ymm5, ymm9, ymm5
       vcmpltps ymm9, ymm1, ymm2
       vorps    ymm5, ymm9, ymm5
       vandps   ymm1, ymm1, ymm5
       vandnps  ymm2, ymm5, ymm2
       vorps    ymm1, ymm2, ymm1
       vcmpeqps ymm2, ymm6, ymm10
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm6
       vandps   ymm2, ymm5, ymm2
       vcmpneqps ymm5, ymm6, ymm6
       vorps    ymm2, ymm5, ymm2
       vcmpltps ymm5, ymm6, ymm10
       vorps    ymm2, ymm5, ymm2
       vblendvps ymm2, ymm10, ymm6, ymm2
       vcmpeqps ymm5, ymm4, ymm3
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm3
       vandps   ymm5, ymm6, ymm5
       vcmpneqps ymm6, ymm3, ymm3
       vorps    ymm5, ymm6, ymm5
       vcmpltps ymm6, ymm3, ymm4
       vorps    ymm5, ymm6, ymm5
       vblendvps ymm5, ymm4, ymm3, ymm5
       vcmpeqps ymm3, ymm0, ymm1
       vxorps   ymm4, ymm4, ymm4
       vpcmpgtd ymm4, ymm4, ymm1
       vandps   ymm4, ymm4, ymm3
       vcmpneqps ymm6, ymm0, ymm0
       vorps    ymm4, ymm4, ymm6
       vcmpltps ymm9, ymm1, ymm0
       vorps    ymm4, ymm9, ymm4
       vandps   ymm9, ymm0, ymm4
       vandnps  ymm4, ymm4, ymm1
       vorps    ymm4, ymm9, ymm4
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm0
       vandps   ymm3, ymm9, ymm3
       vorps    ymm3, ymm3, ymm6
       vcmpltps ymm6, ymm0, ymm1
       vorps    ymm3, ymm6, ymm3
       vandps   ymm0, ymm0, ymm3
       vandnps  ymm1, ymm3, ymm1
       vorps    ymm0, ymm1, ymm0
       vcmpeqps ymm1, ymm7, ymm8
       vxorps   ymm3, ymm3, ymm3
       vpcmpgtd ymm3, ymm3, ymm8
       vandps   ymm3, ymm3, ymm1
       vcmpneqps ymm6, ymm7, ymm7
       vorps    ymm3, ymm3, ymm6
       vcmpltps ymm9, ymm8, ymm7
       vorps    ymm3, ymm9, ymm3

G_M000_IG20:                ;; offset=0x0F83
       vandps   ymm9, ymm7, ymm3
       vandnps  ymm3, ymm3, ymm8
       vorps    ymm3, ymm9, ymm3
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm7
       vandps   ymm1, ymm9, ymm1
       vorps    ymm1, ymm1, ymm6
       vcmpltps ymm6, ymm7, ymm8
       vorps    ymm1, ymm6, ymm1
       vandps   ymm6, ymm7, ymm1
       vandnps  ymm1, ymm1, ymm8
       vorps    ymm1, ymm1, ymm6
       vcmpeqps ymm6, ymm2, ymm5
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm5
       vandps   ymm7, ymm7, ymm6
       vcmpneqps ymm8, ymm2, ymm2
       vorps    ymm7, ymm8, ymm7
       vcmpltps ymm9, ymm5, ymm2
       vorps    ymm7, ymm9, ymm7
       vandps   ymm9, ymm2, ymm7
       vandnps  ymm7, ymm7, ymm5
       vorps    ymm7, ymm9, ymm7
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm2
       vandps   ymm6, ymm9, ymm6
       vorps    ymm6, ymm8, ymm6
       vcmpltps ymm8, ymm2, ymm5
       vorps    ymm6, ymm8, ymm6
       vandps   ymm2, ymm2, ymm6
       vandnps  ymm5, ymm6, ymm5
       vorps    ymm2, ymm5, ymm2
       vcmpeqps ymm5, ymm1, ymm2
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm2
       vandps   ymm6, ymm6, ymm5
       vcmpneqps ymm8, ymm1, ymm1
       vorps    ymm6, ymm8, ymm6
       vcmpltps ymm9, ymm2, ymm1
       vorps    ymm6, ymm9, ymm6
       vandps   ymm9, ymm1, ymm6
       vandnps  ymm6, ymm6, ymm2
       vorps    ymm6, ymm9, ymm6
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm1
       vandps   ymm5, ymm9, ymm5
       vorps    ymm5, ymm8, ymm5
       vcmpltps ymm8, ymm1, ymm2
       vorps    ymm5, ymm8, ymm5
       vandps   ymm1, ymm1, ymm5
       vandnps  ymm2, ymm5, ymm2
       vorps    ymm1, ymm2, ymm1
       vcmpeqps ymm2, ymm3, ymm7
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm3
       vandps   ymm2, ymm5, ymm2
       vcmpneqps ymm5, ymm3, ymm3
       vorps    ymm2, ymm5, ymm2
       vcmpltps ymm5, ymm3, ymm7

G_M000_IG21:                ;; offset=0x1081
       vorps    ymm2, ymm5, ymm2
       vblendvps ymm2, ymm7, ymm3, ymm2
       vcmpeqps ymm3, ymm1, ymm4
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm1
       vandps   ymm5, ymm5, ymm3
       vcmpneqps ymm7, ymm4, ymm4
       vorps    ymm5, ymm5, ymm7
       vcmpltps ymm8, ymm1, ymm4
       vorps    ymm5, ymm8, ymm5
       vandps   ymm8, ymm4, ymm5
       vandnps  ymm5, ymm5, ymm1
       vorps    ymm5, ymm8, ymm5
       vxorps   ymm8, ymm8, ymm8
       vpcmpgtd ymm8, ymm8, ymm4
       vandps   ymm3, ymm8, ymm3
       vorps    ymm3, ymm3, ymm7
       vcmpltps ymm7, ymm4, ymm1
       vorps    ymm3, ymm7, ymm3
       vandps   ymm4, ymm4, ymm3
       vandnps  ymm1, ymm3, ymm1
       vorps    ymm1, ymm1, ymm4
       vcmpeqps ymm3, ymm2, ymm6
       vxorps   ymm4, ymm4, ymm4
       vpcmpgtd ymm4, ymm4, ymm6
       vandps   ymm3, ymm4, ymm3
       vcmpneqps ymm4, ymm6, ymm6
       vorps    ymm3, ymm4, ymm3
       vcmpltps ymm4, ymm6, ymm2
       vorps    ymm3, ymm4, ymm3
       vblendvps ymm2, ymm2, ymm6, ymm3

G_M000_IG22:                ;; offset=0x1109
       vmulps   ymm0, ymm0, ymmword ptr [reloc @RWD00]
       vmulps   ymm1, ymm1, ymmword ptr [reloc @RWD32]
       vaddps   ymm0, ymm1, ymm0
       vmulps   ymm1, ymm5, ymmword ptr [reloc @RWD64]
       vmulps   ymm2, ymm2, ymmword ptr [reloc @RWD96]
       vaddps   ymm1, ymm2, ymm1
       vaddps   ymm0, ymm1, ymm0
       mov      rdx, gword ptr [rbx+0x20]
       lea      r8d, [rdi+rbp]
       cmp      dword ptr [rdx+0x08], r8d
       jbe      G_M000_IG27
       mov      r9d, dword ptr [rdx+0x08]
       sub      r9d, r8d
       cmp      r9d, 8
       jl       G_M000_IG28
       vmovups  ymmword ptr [rdx+4*r8+0x10], ymm0
       add      ebp, 8
       mov      edx, dword ptr [rbx+0x50]
       add      edx, -8
       cmp      ebp, edx
       jl       G_M000_IG04

G_M000_IG23:                ;; offset=0x1170
       cmp      ebp, dword ptr [rbx+0x50]
       jge      SHORT G_M000_IG25

G_M000_IG24:                ;; offset=0x1175
       mov      rcx, rbx
       mov      edx, esi
       mov      r8d, edi
       mov      r9d, ebp
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this]
       inc      ebp
       cmp      ebp, dword ptr [rbx+0x50]
       jl       SHORT G_M000_IG24

G_M000_IG25:                ;; offset=0x118D
       vzeroupper
       vmovaps  xmm6, xmmword ptr [rsp+0x150]
       vmovaps  xmm7, xmmword ptr [rsp+0x140]
       vmovaps  xmm8, xmmword ptr [rsp+0x130]
       vmovaps  xmm9, xmmword ptr [rsp+0x120]
       vmovaps  xmm10, xmmword ptr [rsp+0x110]
       vmovaps  xmm11, xmmword ptr [rsp+0x100]
       vmovaps  xmm12, xmmword ptr [rsp+0xF0]
       vmovaps  xmm13, xmmword ptr [rsp+0xE0]
       vmovaps  xmm14, xmmword ptr [rsp+0xD0]
       vmovaps  xmm15, xmmword ptr [rsp+0xC0]
       add      rsp, 360
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r14
       pop      r15
       ret

G_M000_IG26:                ;; offset=0x11FA
       call     [System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()]
       int3

G_M000_IG27:                ;; offset=0x1201
       call     [System.ThrowHelper:ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLess()]
       int3

G_M000_IG28:                ;; offset=0x1208
       call     [System.ThrowHelper:ThrowArgumentException_DestinationTooShort()]
       int3

G_M000_IG29:                ;; offset=0x120F
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dq	3E0000003E000000h, 3E0000003E000000h, 3E0000003E000000h, 3E0000003E000000h
RWD32  	dq	3D99999A3D99999Ah, 3D99999A3D99999Ah, 3D99999A3D99999Ah, 3D99999A3D99999Ah
RWD64  	dq	3D75C28F3D75C28Fh, 3D75C28F3D75C28Fh, 3D75C28F3D75C28Fh, 3D75C28F3D75C28Fh
RWD96  	dq	3D4CCCCD3D4CCCCDh, 3D4CCCCD3D4CCCCDh, 3D4CCCCD3D4CCCCDh, 3D4CCCCD3D4CCCCDh

; Total bytes of code 4629

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data
; 2 inlinees with PGO data; 29 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r14
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 176
       vmovaps  xmmword ptr [rsp+0xA0], xmm6
       vmovaps  xmmword ptr [rsp+0x90], xmm7
       vmovaps  xmmword ptr [rsp+0x80], xmm8
       vmovaps  xmmword ptr [rsp+0x70], xmm9
       vmovaps  xmmword ptr [rsp+0x60], xmm10
       vmovaps  xmmword ptr [rsp+0x50], xmm11
       vmovaps  xmmword ptr [rsp+0x40], xmm12
       vmovaps  xmmword ptr [rsp+0x30], xmm13
       vmovaps  xmmword ptr [rsp+0x20], xmm14

G_M000_IG02:                ;; offset=0x004C
       lea      eax, [r9-0x01]
       xor      r10d, r10d
       test     eax, eax
       cmovl    eax, r10d
       lea      r10d, [r9+0x01]
       mov      r11d, dword ptr [rcx+0x50]
       lea      ebx, [r11-0x01]
       cmp      r10d, ebx
       cmovg    r10d, ebx
       lea      ebx, [rdx-0x01]
       imul     ebx, r11d
       mov      esi, edx
       imul     esi, r11d
       inc      edx
       imul     edx, r11d
       mov      r11, gword ptr [rcx+0x18]
       mov      rdi, r11
       lea      ebp, [rbx+rax]
       mov      r14d, dword ptr [rdi+0x08]
       cmp      ebp, r14d
       jae      G_M000_IG22
       lea      ebp, [rbx+rax]
       vmovss   xmm0, dword ptr [rdi+4*rbp+0x10]
       mov      rdi, r11
       lea      ebp, [rbx+r9]
       cmp      ebp, r14d
       jae      G_M000_IG22
       lea      ebp, [rbx+r9]
       vmovss   xmm1, dword ptr [rdi+4*rbp+0x10]
       mov      rdi, r11
       lea      ebp, [rbx+r10]
       cmp      ebp, r14d
       jae      G_M000_IG22
       add      ebx, r10d
       vmovss   xmm2, dword ptr [rdi+4*rbx+0x10]
       mov      rbx, r11
       lea      edi, [rsi+rax]
       cmp      edi, r14d
       jae      G_M000_IG22
       lea      edi, [rsi+rax]
       vmovss   xmm3, dword ptr [rbx+4*rdi+0x10]
       mov      rbx, r11
       lea      edi, [rsi+r9]
       cmp      edi, r14d
       jae      G_M000_IG22
       lea      edi, [rsi+r9]
       vmovss   xmm4, dword ptr [rbx+4*rdi+0x10]
       mov      rbx, r11
       lea      edi, [rsi+r10]
       cmp      edi, r14d
       jae      G_M000_IG22
       add      esi, r10d
       vmovss   xmm5, dword ptr [rbx+4*rsi+0x10]
       mov      rbx, r11
       lea      esi, [rdx+rax]
       cmp      esi, r14d
       jae      G_M000_IG22
       add      eax, edx

G_M000_IG03:                ;; offset=0x012E
       vmovss   xmm6, dword ptr [rbx+4*rax+0x10]
       mov      rax, r11
       lea      ebx, [rdx+r9]
       cmp      ebx, r14d
       jae      G_M000_IG22
       lea      ebx, [rdx+r9]
       vmovss   xmm7, dword ptr [rax+4*rbx+0x10]
       lea      eax, [rdx+r10]
       cmp      eax, r14d
       jae      G_M000_IG22
       add      edx, r10d
       mov      eax, edx
       vmovss   xmm8, dword ptr [r11+4*rax+0x10]
       cmp      byte  ptr [rcx+0x60], 0
       jne      G_M000_IG12

G_M000_IG04:                ;; offset=0x0171
       vmovaps  xmm9, xmm0
       vmovaps  xmm10, xmm1
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm9
       vorps    xmm11, xmm13, xmm11
       vandps   xmm9, xmm9, xmm11
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm9, xmm10, xmm9
       vcmpeqps xmm10, xmm0, xmm1
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10
       vorps    xmm12, xmm10, xmm12
       vcmpltps xmm10, xmm0, xmm1
       vorps    xmm10, xmm10, xmm12
       vandps   xmm0, xmm10, xmm0
       vandnps  xmm1, xmm10, xmm1
       vorps    xmm0, xmm1, xmm0
       vmovaps  xmm1, xmm2
       vmovaps  xmm10, xmm3
       vcmpeqps xmm11, xmm1, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm1, xmm1
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm1
       vorps    xmm11, xmm13, xmm11
       vandps   xmm1, xmm11, xmm1
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm1, xmm10, xmm1
       vcmpeqps xmm10, xmm2, xmm3
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm2
       vandps   xmm10, xmm11, xmm10
       vorps    xmm12, xmm10, xmm12
       vcmpltps xmm10, xmm2, xmm3
       vorps    xmm10, xmm10, xmm12
       vandps   xmm2, xmm10, xmm2
       vandnps  xmm3, xmm10, xmm3
       vorps    xmm2, xmm3, xmm2
       vmovaps  xmm3, xmm0
       vmovaps  xmm10, xmm2
       vcmpeqps xmm11, xmm3, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm3, xmm3
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm3
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm3, xmm10, xmm3, xmm11
       vcmpeqps xmm10, xmm0, xmm2
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10

G_M000_IG05:                ;; offset=0x0297
       vcmpneqps xmm11, xmm0, xmm0
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm0, xmm2
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm0, xmm2, xmm0, xmm10
       vmovaps  xmm2, xmm9
       vmovaps  xmm10, xmm1
       vcmpeqps xmm11, xmm2, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm2, xmm2
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm2
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm2, xmm10, xmm2, xmm11
       vcmpeqps xmm10, xmm9, xmm1
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm9
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm9, xmm9
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm9, xmm1
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm1, xmm1, xmm9, xmm10
       vmovaps  xmm9, xmm1
       vmovaps  xmm10, xmm3
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm9
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm9, xmm10, xmm9, xmm11
       vcmpeqps xmm10, xmm1, xmm3
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm1
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm1, xmm1
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm1, xmm3
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm1, xmm3, xmm1, xmm10
       vmovaps  xmm3, xmm0
       vmovaps  xmm10, xmm4
       vcmpeqps xmm11, xmm3, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm3, xmm3
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm3
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm3, xmm10, xmm3, xmm11
       vmovaps  xmm10, xmm1
       vmovaps  xmm11, xmm3
       vcmpeqps xmm12, xmm10, xmm11
       vxorps   xmm13, xmm13, xmm13
       vpcmpgtd xmm13, xmm13, xmm11

G_M000_IG06:                ;; offset=0x03CD
       vandps   xmm12, xmm13, xmm12
       vcmpneqps xmm13, xmm10, xmm10
       vorps    xmm12, xmm13, xmm12
       vcmpltps xmm13, xmm11, xmm10
       vorps    xmm12, xmm13, xmm12
       vblendvps xmm10, xmm11, xmm10, xmm12
       vmovaps  xmm11, xmm9
       vmovaps  xmm12, xmm10
       vcmpeqps xmm13, xmm11, xmm12
       vxorps   xmm14, xmm14, xmm14
       vpcmpgtd xmm14, xmm14, xmm12
       vandps   xmm13, xmm14, xmm13
       vcmpneqps xmm14, xmm11, xmm11
       vorps    xmm13, xmm14, xmm13
       vcmpltps xmm14, xmm12, xmm11
       vorps    xmm13, xmm14, xmm13
       vblendvps xmm11, xmm12, xmm11, xmm13
       vcmpeqps xmm12, xmm0, xmm4
       vxorps   xmm13, xmm13, xmm13
       vpcmpgtd xmm13, xmm13, xmm0
       vandps   xmm12, xmm13, xmm12
       vcmpneqps xmm13, xmm0, xmm0
       vorps    xmm12, xmm13, xmm12
       vcmpltps xmm13, xmm0, xmm4
       vorps    xmm12, xmm13, xmm12
       vblendvps xmm0, xmm4, xmm0, xmm12
       vcmpeqps xmm4, xmm1, xmm3
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm1
       vandps   xmm4, xmm12, xmm4
       vcmpneqps xmm12, xmm1, xmm1
       vorps    xmm4, xmm12, xmm4
       vcmpltps xmm12, xmm1, xmm3
       vorps    xmm4, xmm12, xmm4
       vblendvps xmm1, xmm3, xmm1, xmm4
       vcmpeqps xmm3, xmm9, xmm10
       vxorps   xmm4, xmm4, xmm4
       vpcmpgtd xmm4, xmm4, xmm9
       vandps   xmm3, xmm4, xmm3
       vcmpneqps xmm4, xmm9, xmm9
       vorps    xmm3, xmm4, xmm3
       vcmpltps xmm4, xmm9, xmm10
       vorps    xmm3, xmm4, xmm3
       vblendvps xmm3, xmm10, xmm9, xmm3
       vcmpeqps xmm4, xmm2, xmm11
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm2
       vandps   xmm4, xmm9, xmm4
       vcmpneqps xmm9, xmm2, xmm2
       vorps    xmm4, xmm9, xmm4
       vcmpltps xmm9, xmm2, xmm11
       vorps    xmm4, xmm9, xmm4
       vblendvps xmm2, xmm11, xmm2, xmm4
       vmovaps  xmm4, xmm0
       vmovaps  xmm9, xmm5
       vcmpeqps xmm10, xmm4, xmm9
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm9
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm4, xmm4

G_M000_IG07:                ;; offset=0x04FB
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm9, xmm4
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm4, xmm9, xmm4, xmm10
       vmovaps  xmm9, xmm1
       vmovaps  xmm10, xmm4
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm9
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm9, xmm10, xmm9, xmm11
       vmovaps  xmm10, xmm3
       vmovaps  xmm11, xmm9
       vcmpeqps xmm12, xmm10, xmm11
       vxorps   xmm13, xmm13, xmm13
       vpcmpgtd xmm13, xmm13, xmm11
       vandps   xmm12, xmm13, xmm12
       vcmpneqps xmm13, xmm10, xmm10
       vorps    xmm12, xmm13, xmm12
       vcmpltps xmm13, xmm11, xmm10
       vorps    xmm12, xmm13, xmm12
       vblendvps xmm10, xmm11, xmm10, xmm12
       vcmpeqps xmm11, xmm0, xmm5
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm0
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm0, xmm0
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm0, xmm5
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm0, xmm5, xmm0, xmm11
       vcmpeqps xmm5, xmm1, xmm4
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm1
       vandps   xmm5, xmm11, xmm5
       vcmpneqps xmm11, xmm1, xmm1
       vorps    xmm5, xmm11, xmm5
       vcmpltps xmm11, xmm1, xmm4
       vorps    xmm5, xmm11, xmm5
       vblendvps xmm1, xmm4, xmm1, xmm5
       vcmpeqps xmm4, xmm3, xmm9
       vxorps   xmm5, xmm5, xmm5
       vpcmpgtd xmm5, xmm5, xmm3
       vandps   xmm4, xmm5, xmm4
       vcmpneqps xmm5, xmm3, xmm3
       vorps    xmm4, xmm5, xmm4
       vcmpltps xmm5, xmm3, xmm9
       vorps    xmm4, xmm5, xmm4
       vblendvps xmm3, xmm9, xmm3, xmm4
       vcmpeqps xmm4, xmm2, xmm10
       vxorps   xmm5, xmm5, xmm5
       vpcmpgtd xmm5, xmm5, xmm2
       vandps   xmm4, xmm5, xmm4
       vcmpneqps xmm5, xmm2, xmm2
       vorps    xmm4, xmm5, xmm4
       vcmpltps xmm5, xmm2, xmm10

G_M000_IG08:                ;; offset=0x0626
       vorps    xmm4, xmm5, xmm4
       vblendvps xmm2, xmm10, xmm2, xmm4
       vmovaps  xmm4, xmm0
       vmovaps  xmm5, xmm6
       vcmpeqps xmm9, xmm4, xmm5
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm5
       vandps   xmm9, xmm10, xmm9
       vcmpneqps xmm10, xmm4, xmm4
       vorps    xmm9, xmm10, xmm9
       vcmpltps xmm10, xmm5, xmm4
       vorps    xmm9, xmm10, xmm9
       vblendvps xmm4, xmm5, xmm4, xmm9
       vmovaps  xmm5, xmm1
       vmovaps  xmm9, xmm4
       vcmpeqps xmm10, xmm5, xmm9
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm9
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm5, xmm5
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm9, xmm5
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm5, xmm9, xmm5, xmm10
       vmovaps  xmm9, xmm3
       vmovaps  xmm10, xmm5
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm9
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm9, xmm10, xmm9, xmm11
       vcmpeqps xmm10, xmm0, xmm6
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm0, xmm0
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm0, xmm6
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm0, xmm6, xmm0, xmm10
       vcmpeqps xmm6, xmm1, xmm4
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm1
       vandps   xmm6, xmm10, xmm6
       vcmpneqps xmm10, xmm1, xmm1
       vorps    xmm6, xmm10, xmm6
       vcmpltps xmm10, xmm1, xmm4
       vorps    xmm6, xmm10, xmm6
       vblendvps xmm1, xmm4, xmm1, xmm6
       vcmpeqps xmm4, xmm3, xmm5
       vxorps   xmm6, xmm6, xmm6
       vpcmpgtd xmm6, xmm6, xmm3
       vandps   xmm4, xmm6, xmm4
       vcmpneqps xmm6, xmm3, xmm3
       vorps    xmm4, xmm6, xmm4
       vcmpltps xmm6, xmm3, xmm5
       vorps    xmm4, xmm6, xmm4

G_M000_IG09:                ;; offset=0x074F
       vblendvps xmm3, xmm5, xmm3, xmm4
       vcmpeqps xmm4, xmm2, xmm9
       vxorps   xmm5, xmm5, xmm5
       vpcmpgtd xmm5, xmm5, xmm2
       vandps   xmm4, xmm5, xmm4
       vcmpneqps xmm5, xmm2, xmm2
       vorps    xmm4, xmm5, xmm4
       vcmpltps xmm5, xmm2, xmm9
       vorps    xmm4, xmm5, xmm4
       vblendvps xmm2, xmm9, xmm2, xmm4
       vmovaps  xmm4, xmm0
       vmovaps  xmm5, xmm7
       vcmpeqps xmm6, xmm4, xmm5
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm5
       vandps   xmm6, xmm9, xmm6
       vcmpneqps xmm9, xmm4, xmm4
       vorps    xmm6, xmm9, xmm6
       vcmpltps xmm9, xmm5, xmm4
       vorps    xmm6, xmm9, xmm6
       vblendvps xmm4, xmm5, xmm4, xmm6
       vmovaps  xmm5, xmm1
       vmovaps  xmm6, xmm4
       vcmpeqps xmm9, xmm5, xmm6
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm6
       vandps   xmm9, xmm10, xmm9
       vcmpneqps xmm10, xmm5, xmm5
       vorps    xmm9, xmm10, xmm9
       vcmpltps xmm10, xmm6, xmm5
       vorps    xmm9, xmm10, xmm9
       vblendvps xmm5, xmm6, xmm5, xmm9
       vmovaps  xmm6, xmm3
       vmovaps  xmm9, xmm5
       vcmpeqps xmm10, xmm6, xmm9
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm9
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm6, xmm6
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm9, xmm6
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm6, xmm9, xmm6, xmm10
       vcmpeqps xmm9, xmm0, xmm7
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm0
       vandps   xmm9, xmm10, xmm9
       vcmpneqps xmm10, xmm0, xmm0
       vorps    xmm9, xmm10, xmm9
       vcmpltps xmm10, xmm0, xmm7
       vorps    xmm9, xmm10, xmm9
       vblendvps xmm0, xmm7, xmm0, xmm9
       vcmpeqps xmm7, xmm1, xmm4
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm1
       vandps   xmm7, xmm9, xmm7
       vcmpneqps xmm9, xmm1, xmm1
       vorps    xmm7, xmm9, xmm7
       vcmpltps xmm9, xmm1, xmm4
       vorps    xmm7, xmm9, xmm7
       vblendvps xmm1, xmm4, xmm1, xmm7

G_M000_IG10:                ;; offset=0x0875
       vcmpeqps xmm4, xmm3, xmm5
       vxorps   xmm7, xmm7, xmm7
       vpcmpgtd xmm7, xmm7, xmm3
       vandps   xmm4, xmm7, xmm4
       vcmpneqps xmm7, xmm3, xmm3
       vorps    xmm4, xmm7, xmm4
       vcmpltps xmm7, xmm3, xmm5
       vorps    xmm4, xmm7, xmm4
       vblendvps xmm3, xmm5, xmm3, xmm4
       vcmpeqps xmm4, xmm2, xmm6
       vxorps   xmm5, xmm5, xmm5
       vpcmpgtd xmm5, xmm5, xmm2
       vandps   xmm4, xmm5, xmm4
       vcmpneqps xmm5, xmm2, xmm2
       vorps    xmm4, xmm5, xmm4
       vcmpltps xmm5, xmm2, xmm6
       vorps    xmm4, xmm5, xmm4
       vblendvps xmm2, xmm6, xmm2, xmm4
       vmovaps  xmm4, xmm0
       vmovaps  xmm5, xmm8
       vcmpeqps xmm6, xmm4, xmm5
       vxorps   xmm7, xmm7, xmm7
       vpcmpgtd xmm7, xmm7, xmm5
       vandps   xmm6, xmm7, xmm6
       vcmpneqps xmm7, xmm4, xmm4
       vorps    xmm6, xmm7, xmm6
       vcmpltps xmm7, xmm5, xmm4
       vorps    xmm6, xmm7, xmm6
       vblendvps xmm4, xmm5, xmm4, xmm6
       vmovaps  xmm5, xmm1
       vmovaps  xmm6, xmm4
       vcmpeqps xmm7, xmm5, xmm6
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm6
       vandps   xmm7, xmm9, xmm7
       vcmpneqps xmm9, xmm5, xmm5
       vorps    xmm7, xmm9, xmm7
       vcmpltps xmm9, xmm6, xmm5
       vorps    xmm7, xmm9, xmm7
       vblendvps xmm5, xmm6, xmm5, xmm7
       vmovaps  xmm6, xmm3
       vmovaps  xmm7, xmm5
       vcmpeqps xmm9, xmm6, xmm7
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm7
       vandps   xmm9, xmm10, xmm9
       vcmpneqps xmm10, xmm6, xmm6
       vorps    xmm9, xmm10, xmm9
       vcmpltps xmm10, xmm7, xmm6
       vorps    xmm9, xmm10, xmm9
       vblendvps xmm6, xmm7, xmm6, xmm9
       vcmpeqps xmm7, xmm0, xmm8
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm0
       vandps   xmm7, xmm9, xmm7
       vcmpneqps xmm9, xmm0, xmm0
       vorps    xmm7, xmm9, xmm7
       vcmpltps xmm9, xmm0, xmm8
       vorps    xmm7, xmm9, xmm7
       vblendvps xmm0, xmm8, xmm0, xmm7
       vcmpeqps xmm7, xmm1, xmm4

G_M000_IG11:                ;; offset=0x0991
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm1
       vandps   xmm7, xmm8, xmm7
       vcmpneqps xmm8, xmm1, xmm1
       vorps    xmm7, xmm8, xmm7
       vcmpltps xmm8, xmm1, xmm4
       vorps    xmm7, xmm8, xmm7
       vblendvps xmm1, xmm4, xmm1, xmm7
       vcmpeqps xmm4, xmm3, xmm5
       vxorps   xmm7, xmm7, xmm7
       vpcmpgtd xmm7, xmm7, xmm3
       vandps   xmm4, xmm7, xmm4
       vcmpneqps xmm7, xmm3, xmm3
       vorps    xmm4, xmm7, xmm4
       vcmpltps xmm7, xmm3, xmm5
       vorps    xmm4, xmm7, xmm4
       vblendvps xmm3, xmm5, xmm3, xmm4
       vcmpeqps xmm4, xmm2, xmm6
       vxorps   xmm5, xmm5, xmm5
       vpcmpgtd xmm5, xmm5, xmm2
       vandps   xmm4, xmm5, xmm4
       vcmpneqps xmm5, xmm2, xmm2
       vorps    xmm4, xmm5, xmm4
       vcmpltps xmm5, xmm2, xmm6
       vorps    xmm4, xmm5, xmm4
       vblendvps xmm2, xmm6, xmm2, xmm4
       vmovaps  xmm4, xmm1
       vmovaps  xmm1, xmm2
       vmovaps  xmm2, xmm3
       jmp      G_M000_IG20

G_M000_IG12:                ;; offset=0x0A19
       vmovaps  xmm9, xmm0
       vmovaps  xmm10, xmm3
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm9
       vorps    xmm11, xmm13, xmm11
       vandps   xmm9, xmm9, xmm11
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm9, xmm10, xmm9
       vcmpeqps xmm10, xmm0, xmm3
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm0, xmm3
       vorps    xmm10, xmm11, xmm10
       vandps   xmm0, xmm10, xmm0
       vandnps  xmm3, xmm10, xmm3
       vorps    xmm0, xmm3, xmm0
       vmovaps  xmm3, xmm1
       vmovaps  xmm10, xmm7
       vcmpeqps xmm11, xmm3, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm3, xmm3
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm3
       vorps    xmm11, xmm13, xmm11
       vandps   xmm3, xmm11, xmm3
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm3, xmm10, xmm3
       vcmpeqps xmm10, xmm1, xmm7
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm1
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm1, xmm7
       vorps    xmm10, xmm11, xmm10
       vandps   xmm1, xmm10, xmm1
       vandnps  xmm7, xmm10, xmm7
       vorps    xmm1, xmm7, xmm1
       vmovaps  xmm7, xmm2
       vmovaps  xmm10, xmm5
       vcmpeqps xmm11, xmm7, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm7, xmm7
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm7
       vorps    xmm11, xmm13, xmm11
       vandps   xmm7, xmm11, xmm7
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm7, xmm10, xmm7
       vcmpeqps xmm10, xmm2, xmm5
       vxorps   xmm11, xmm11, xmm11

G_M000_IG13:                ;; offset=0x0B3D
       vpcmpgtd xmm11, xmm11, xmm2
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm2, xmm5
       vorps    xmm10, xmm11, xmm10
       vandps   xmm2, xmm10, xmm2
       vandnps  xmm5, xmm10, xmm5
       vorps    xmm2, xmm5, xmm2
       vmovaps  xmm5, xmm4
       vmovaps  xmm10, xmm8
       vcmpeqps xmm11, xmm5, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm5, xmm5
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm5
       vorps    xmm11, xmm13, xmm11
       vandps   xmm5, xmm11, xmm5
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm5, xmm10, xmm5
       vcmpeqps xmm10, xmm4, xmm8
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm4
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm4, xmm8
       vorps    xmm10, xmm11, xmm10
       vandps   xmm4, xmm10, xmm4
       vandnps  xmm8, xmm10, xmm8
       vorps    xmm4, xmm8, xmm4
       vmovaps  xmm8, xmm0
       vmovaps  xmm10, xmm3
       vcmpeqps xmm11, xmm8, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm8, xmm8
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm8
       vorps    xmm11, xmm13, xmm11
       vandps   xmm8, xmm8, xmm11
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm8, xmm10, xmm8
       vcmpeqps xmm10, xmm0, xmm3
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm0, xmm3
       vorps    xmm10, xmm11, xmm10
       vandps   xmm0, xmm10, xmm0
       vandnps  xmm3, xmm10, xmm3
       vorps    xmm0, xmm3, xmm0
       vmovaps  xmm3, xmm2
       vmovaps  xmm10, xmm4
       vcmpeqps xmm11, xmm3, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm3, xmm3

G_M000_IG14:                ;; offset=0x0C63
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm3
       vorps    xmm11, xmm13, xmm11
       vandps   xmm3, xmm11, xmm3
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm3, xmm10, xmm3
       vcmpeqps xmm10, xmm2, xmm4
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm2
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm2, xmm4
       vorps    xmm10, xmm11, xmm10
       vandps   xmm2, xmm10, xmm2
       vandnps  xmm4, xmm10, xmm4
       vorps    xmm2, xmm4, xmm2
       vmovaps  xmm4, xmm9
       vmovaps  xmm10, xmm5
       vcmpeqps xmm11, xmm4, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm4, xmm4
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm4
       vorps    xmm11, xmm13, xmm11
       vandps   xmm4, xmm11, xmm4
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm4, xmm10, xmm4
       vcmpeqps xmm10, xmm9, xmm5
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm9
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm9, xmm5
       vorps    xmm10, xmm11, xmm10
       vandps   xmm9, xmm9, xmm10
       vandnps  xmm5, xmm10, xmm5
       vorps    xmm5, xmm9, xmm5
       vmovaps  xmm9, xmm7
       vmovaps  xmm10, xmm6
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm9
       vorps    xmm11, xmm13, xmm11
       vandps   xmm9, xmm9, xmm11
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm9, xmm10, xmm9
       vcmpeqps xmm10, xmm7, xmm6
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm7
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm7, xmm6
       vorps    xmm10, xmm11, xmm10
       vandps   xmm7, xmm10, xmm7

G_M000_IG15:                ;; offset=0x0D84
       vandnps  xmm6, xmm10, xmm6
       vorps    xmm6, xmm6, xmm7
       vmovaps  xmm7, xmm0
       vmovaps  xmm10, xmm2
       vcmpeqps xmm11, xmm7, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm7, xmm7
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm7
       vorps    xmm11, xmm13, xmm11
       vandps   xmm7, xmm11, xmm7
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm7, xmm10, xmm7
       vcmpeqps xmm10, xmm0, xmm2
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm0, xmm2
       vorps    xmm10, xmm11, xmm10
       vandps   xmm0, xmm10, xmm0
       vandnps  xmm2, xmm10, xmm2
       vorps    xmm0, xmm2, xmm0
       vmovaps  xmm2, xmm1
       vmovaps  xmm10, xmm5
       vcmpeqps xmm11, xmm2, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm2, xmm2
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm2
       vorps    xmm11, xmm13, xmm11
       vandps   xmm2, xmm11, xmm2
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm2, xmm10, xmm2
       vcmpeqps xmm10, xmm1, xmm5
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm1
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm1, xmm5
       vorps    xmm10, xmm11, xmm10
       vandps   xmm1, xmm10, xmm1
       vandnps  xmm5, xmm10, xmm5
       vorps    xmm1, xmm5, xmm1
       vmovaps  xmm5, xmm3
       vmovaps  xmm10, xmm6
       vcmpeqps xmm11, xmm5, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm5, xmm5
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm5
       vorps    xmm11, xmm13, xmm11
       vandps   xmm5, xmm11, xmm5
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm5, xmm10, xmm5

G_M000_IG16:                ;; offset=0x0EA2
       vcmpeqps xmm10, xmm3, xmm6
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm3
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm3, xmm6
       vorps    xmm10, xmm11, xmm10
       vandps   xmm3, xmm10, xmm3
       vandnps  xmm6, xmm10, xmm6
       vorps    xmm3, xmm6, xmm3
       vcmpeqps xmm6, xmm8, xmm4
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm8
       vandps   xmm6, xmm10, xmm6
       vcmpneqps xmm10, xmm8, xmm8
       vorps    xmm6, xmm10, xmm6
       vcmpltps xmm10, xmm8, xmm4
       vorps    xmm6, xmm10, xmm6
       vblendvps xmm4, xmm4, xmm8, xmm6
       vmovaps  xmm6, xmm1
       vmovaps  xmm8, xmm3
       vcmpeqps xmm10, xmm6, xmm8
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm8
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm6, xmm6
       vorps    xmm10, xmm10, xmm11
       vcmpltps xmm12, xmm8, xmm6
       vorps    xmm10, xmm12, xmm10
       vandps   xmm6, xmm10, xmm6
       vandnps  xmm8, xmm10, xmm8
       vorps    xmm6, xmm8, xmm6
       vcmpeqps xmm8, xmm1, xmm3
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm1
       vandps   xmm8, xmm10, xmm8
       vorps    xmm8, xmm8, xmm11
       vcmpltps xmm10, xmm1, xmm3
       vorps    xmm8, xmm10, xmm8
       vandps   xmm1, xmm8, xmm1
       vandnps  xmm3, xmm8, xmm3
       vorps    xmm1, xmm3, xmm1
       vcmpeqps xmm3, xmm2, xmm9
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm2
       vandps   xmm3, xmm8, xmm3
       vcmpneqps xmm8, xmm2, xmm2
       vorps    xmm3, xmm8, xmm3
       vcmpltps xmm8, xmm2, xmm9
       vorps    xmm3, xmm8, xmm3
       vblendvps xmm2, xmm9, xmm2, xmm3
       vcmpeqps xmm3, xmm5, xmm4
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm5
       vandps   xmm3, xmm8, xmm3
       vcmpneqps xmm8, xmm5, xmm5
       vorps    xmm3, xmm8, xmm3
       vcmpltps xmm8, xmm5, xmm4
       vorps    xmm3, xmm8, xmm3

G_M000_IG17:                ;; offset=0x0FB8
       vblendvps xmm3, xmm4, xmm5, xmm3
       vmovaps  xmm4, xmm0
       vmovaps  xmm5, xmm1
       vcmpeqps xmm8, xmm4, xmm5
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm5
       vandps   xmm8, xmm9, xmm8
       vcmpneqps xmm9, xmm4, xmm4
       vorps    xmm8, xmm8, xmm9
       vcmpltps xmm10, xmm5, xmm4
       vorps    xmm8, xmm10, xmm8
       vandps   xmm4, xmm8, xmm4
       vandnps  xmm5, xmm8, xmm5
       vorps    xmm4, xmm5, xmm4
       vcmpeqps xmm5, xmm0, xmm1
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm0
       vandps   xmm5, xmm8, xmm5
       vorps    xmm5, xmm9, xmm5
       vcmpltps xmm8, xmm0, xmm1
       vorps    xmm5, xmm8, xmm5
       vandps   xmm0, xmm0, xmm5
       vandnps  xmm1, xmm5, xmm1
       vorps    xmm0, xmm1, xmm0
       vmovaps  xmm1, xmm7
       vmovaps  xmm5, xmm6
       vcmpeqps xmm8, xmm1, xmm5
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm5
       vandps   xmm8, xmm9, xmm8
       vcmpneqps xmm9, xmm1, xmm1
       vorps    xmm8, xmm8, xmm9
       vcmpltps xmm10, xmm5, xmm1
       vorps    xmm8, xmm10, xmm8
       vandps   xmm1, xmm8, xmm1
       vandnps  xmm5, xmm8, xmm5
       vorps    xmm1, xmm5, xmm1
       vcmpeqps xmm5, xmm7, xmm6
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm7
       vandps   xmm5, xmm8, xmm5
       vorps    xmm5, xmm9, xmm5
       vcmpltps xmm8, xmm7, xmm6
       vorps    xmm5, xmm8, xmm5
       vandps   xmm7, xmm7, xmm5
       vandnps  xmm5, xmm5, xmm6
       vorps    xmm5, xmm5, xmm7
       vmovaps  xmm6, xmm2
       vmovaps  xmm7, xmm3
       vcmpeqps xmm8, xmm6, xmm7
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm7
       vandps   xmm8, xmm9, xmm8
       vcmpneqps xmm9, xmm6, xmm6
       vorps    xmm8, xmm8, xmm9
       vcmpltps xmm10, xmm7, xmm6
       vorps    xmm8, xmm10, xmm8
       vandps   xmm6, xmm8, xmm6
       vandnps  xmm7, xmm8, xmm7
       vorps    xmm6, xmm7, xmm6
       vcmpeqps xmm7, xmm2, xmm3

G_M000_IG18:                ;; offset=0x10CA
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm2
       vandps   xmm7, xmm8, xmm7
       vorps    xmm7, xmm9, xmm7
       vcmpltps xmm8, xmm2, xmm3
       vorps    xmm7, xmm8, xmm7
       vandps   xmm2, xmm2, xmm7
       vandnps  xmm3, xmm7, xmm3
       vorps    xmm2, xmm3, xmm2
       vmovaps  xmm3, xmm5
       vmovaps  xmm7, xmm2
       vcmpeqps xmm8, xmm3, xmm7
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm7
       vandps   xmm8, xmm9, xmm8
       vcmpneqps xmm9, xmm3, xmm3
       vorps    xmm8, xmm8, xmm9
       vcmpltps xmm10, xmm7, xmm3
       vorps    xmm8, xmm10, xmm8
       vandps   xmm3, xmm8, xmm3
       vandnps  xmm7, xmm8, xmm7
       vorps    xmm3, xmm7, xmm3
       vcmpeqps xmm7, xmm5, xmm2
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm5
       vandps   xmm7, xmm8, xmm7
       vorps    xmm7, xmm9, xmm7
       vcmpltps xmm8, xmm5, xmm2
       vorps    xmm7, xmm8, xmm7
       vandps   xmm5, xmm5, xmm7
       vandnps  xmm2, xmm7, xmm2
       vorps    xmm5, xmm2, xmm5
       vcmpeqps xmm2, xmm1, xmm6
       vxorps   xmm7, xmm7, xmm7
       vpcmpgtd xmm7, xmm7, xmm1
       vandps   xmm2, xmm7, xmm2
       vcmpneqps xmm7, xmm1, xmm1
       vorps    xmm2, xmm7, xmm2
       vcmpltps xmm7, xmm1, xmm6
       vorps    xmm2, xmm7, xmm2
       vblendvps xmm1, xmm6, xmm1, xmm2
       vmovaps  xmm2, xmm4
       vmovaps  xmm6, xmm5
       vcmpeqps xmm7, xmm2, xmm6
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm6
       vandps   xmm7, xmm8, xmm7
       vcmpneqps xmm8, xmm2, xmm2
       vorps    xmm7, xmm8, xmm7
       vcmpltps xmm9, xmm6, xmm2
       vorps    xmm7, xmm9, xmm7
       vandps   xmm2, xmm2, xmm7
       vandnps  xmm6, xmm7, xmm6
       vorps    xmm2, xmm6, xmm2
       vcmpeqps xmm6, xmm4, xmm5
       vxorps   xmm7, xmm7, xmm7
       vpcmpgtd xmm7, xmm7, xmm4
       vandps   xmm6, xmm7, xmm6
       vorps    xmm6, xmm8, xmm6
       vcmpltps xmm7, xmm4, xmm5

G_M000_IG19:                ;; offset=0x11D1
       vorps    xmm6, xmm7, xmm6
       vandps   xmm4, xmm4, xmm6
       vandnps  xmm5, xmm6, xmm5
       vorps    xmm4, xmm5, xmm4
       vcmpeqps xmm5, xmm3, xmm1
       vxorps   xmm6, xmm6, xmm6
       vpcmpgtd xmm6, xmm6, xmm3
       vandps   xmm5, xmm6, xmm5
       vcmpneqps xmm6, xmm3, xmm3
       vorps    xmm5, xmm6, xmm5
       vcmpltps xmm6, xmm3, xmm1
       vorps    xmm5, xmm6, xmm5
       vblendvps xmm1, xmm1, xmm3, xmm5

G_M000_IG20:                ;; offset=0x120A
       mov      rax, gword ptr [rcx+0x20]
       lea      ecx, [r8+r9]
       cmp      ecx, dword ptr [rax+0x08]
       jae      G_M000_IG22
       add      r8d, r9d
       mov      ecx, r8d
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD00]
       vmulss   xmm3, xmm4, dword ptr [reloc @RWD04]
       vaddss   xmm0, xmm0, xmm3
       vmulss   xmm2, xmm2, dword ptr [reloc @RWD08]
       vmulss   xmm1, xmm1, dword ptr [reloc @RWD12]
       vaddss   xmm1, xmm2, xmm1
       vaddss   xmm0, xmm0, xmm1
       vmovss   dword ptr [rax+4*rcx+0x10], xmm0

G_M000_IG21:                ;; offset=0x1253
       vmovaps  xmm6, xmmword ptr [rsp+0xA0]
       vmovaps  xmm7, xmmword ptr [rsp+0x90]
       vmovaps  xmm8, xmmword ptr [rsp+0x80]
       vmovaps  xmm9, xmmword ptr [rsp+0x70]
       vmovaps  xmm10, xmmword ptr [rsp+0x60]
       vmovaps  xmm11, xmmword ptr [rsp+0x50]
       vmovaps  xmm12, xmmword ptr [rsp+0x40]
       vmovaps  xmm13, xmmword ptr [rsp+0x30]
       vmovaps  xmm14, xmmword ptr [rsp+0x20]
       add      rsp, 176
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r14
       ret

G_M000_IG22:                ;; offset=0x12A0
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	3E000000h		;     0.125
RWD04  	dd	3D99999Ah		;     0.075
RWD08  	dd	3D75C28Fh		;      0.06
RWD12  	dd	3D4CCCCDh		;      0.05

; Total bytes of code 4774

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ComputeMask(float):float (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000

G_M000_IG02:                ;; offset=0x0000
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD00]
       vbroadcastss xmm1, dword ptr [reloc @RWD04]
       vmaxss   xmm0, xmm1, xmm0
       vmulss   xmm1, xmm0, xmm0
       vaddss   xmm2, xmm1, dword ptr [reloc @RWD08]
       vmovss   xmm3, dword ptr [reloc @RWD12]
       vdivss   xmm2, xmm3, xmm2
       vmulss   xmm2, xmm2, dword ptr [reloc @RWD16]
       vaddss   xmm1, xmm1, dword ptr [reloc @RWD20]
       vdivss   xmm1, xmm3, xmm1
       vmulss   xmm1, xmm1, dword ptr [reloc @RWD24]
       vaddss   xmm0, xmm0, dword ptr [reloc @RWD28]
       vdivss   xmm0, xmm3, xmm0
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD32]
       vaddss   xmm0, xmm1, xmm0
       vaddss   xmm0, xmm2, xmm0
       vaddss   xmm0, xmm0, dword ptr [reloc @RWD36]

G_M000_IG03:                ;; offset=0x006D
       ret

RWD00  	dd	3F3F62FEh		;  0.747604
RWD04  	dd	3A83126Fh		;     0.001
RWD08  	dd	3F0C5322h		;  0.548144
RWD12  	dd	3F800000h		;         1
RWD16  	dd	404F0F94h		;   3.23533
RWD20  	dd	400C5322h		;   2.19257
RWD24  	dd	40A0B47Bh		;   5.02203
RWD28  	dd	4398852Ah		;    305.04
RWD32  	dd	414E7F18h		;    12.906
RWD36  	dd	BF3DE353h		;  -0.74175

; Total bytes of code 110

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ModulateScalar(System.ReadOnlySpan`1[float],int,float):float (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 0 single block inlinees; 2 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbx
       sub      rsp, 144
       vmovaps  xmmword ptr [rsp+0x80], xmm6
       vmovaps  xmmword ptr [rsp+0x70], xmm7
       vmovaps  xmmword ptr [rsp+0x60], xmm8
       vmovaps  xmmword ptr [rsp+0x50], xmm9
       vmovaps  xmmword ptr [rsp+0x40], xmm10
       vmovaps  xmmword ptr [rsp+0x30], xmm11
       vmovaps  xmmword ptr [rsp+0x20], xmm12
       vmovaps  xmm6, xmm2

G_M000_IG02:                ;; offset=0x0039
       mov      rax, bword ptr [rcx]
       mov      ecx, dword ptr [rcx+0x08]
       vxorps   xmm7, xmm7, xmm7
       vxorps   xmm0, xmm0, xmm0
       xor      r8d, r8d
       vmovss   xmm1, dword ptr [reloc @RWD00]
       vmovss   xmm2, dword ptr [reloc @RWD04]
       vmovss   xmm3, dword ptr [reloc @RWD08]
       vmovss   xmm4, dword ptr [reloc @RWD12]
       vmovss   xmm5, dword ptr [reloc @RWD16]

G_M000_IG03:                ;; offset=0x0072
       mov      r10d, r8d
       imul     r10d, edx
       cmp      r8d, 7
       jne      SHORT G_M000_IG05

G_M000_IG04:                ;; offset=0x007F
       mov      r9d, r10d
       jmp      SHORT G_M000_IG06
       align    [0 bytes for IG07]

G_M000_IG05:                ;; offset=0x0084
       lea      r9d, [r10+rdx]

G_M000_IG06:                ;; offset=0x0088
       cmp      r10d, ecx
       jae      G_M000_IG11
       mov      r11d, r10d
       vmovss   xmm8, dword ptr [rax+4*r11]
       xor      r11d, r11d

G_M000_IG07:                ;; offset=0x009D
       lea      ebx, [r10+r11+0x01]
       cmp      ebx, ecx
       jae      G_M000_IG11
       vmovss   xmm9, dword ptr [rax+4*rbx]
       lea      ebx, [r9+r11]
       cmp      ebx, ecx
       jae      G_M000_IG11
       vsubss   xmm10, xmm8, dword ptr [rax+4*rbx]
       vandps   xmm10, xmm10, xmmword ptr [reloc @RWD20]
       vsubss   xmm11, xmm8, xmm9
       vandps   xmm11, xmm11, xmmword ptr [reloc @RWD36]
       vaddss   xmm7, xmm11, xmm7
       vaddss   xmm7, xmm10, xmm7
       vaddss   xmm8, xmm8, xmm1
       vxorps   xmm10, xmm10, xmm10
       vcmpneqps xmm11, xmm8, xmm8
       vorps    xmm10, xmm11, xmm10
       vxorps   xmm11, xmm11, xmm11
       vcmpgtps xmm11, xmm8, xmm11
       vorps    xmm10, xmm11, xmm10
       vandps   xmm8, xmm10, xmm8
       vmulss   xmm10, xmm8, xmm8
       vmulss   xmm8, xmm8, xmm2
       vmulss   xmm8, xmm8, xmm10
       vaddss   xmm8, xmm8, xmm3
       vmulss   xmm10, xmm10, xmm4
       vaddss   xmm10, xmm10, xmm5
       vdivss   xmm8, xmm10, xmm8
       vaddss   xmm0, xmm8, xmm0
       vmovaps  xmm8, xmm9
       inc      r11d
       cmp      r11d, 7
       jl       G_M000_IG07

G_M000_IG08:                ;; offset=0x013B
       add      r9d, 7
       cmp      r9d, ecx
       jae      G_M000_IG11
       mov      r10d, r9d
       vsubss   xmm9, xmm8, dword ptr [rax+4*r10]
       vandps   xmm9, xmm9, xmmword ptr [reloc @RWD52]
       vaddss   xmm7, xmm9, xmm7
       vaddss   xmm8, xmm8, xmm1
       vxorps   xmm9, xmm9, xmm9
       vcmpneqps xmm10, xmm8, xmm8
       vorps    xmm9, xmm10, xmm9
       vxorps   xmm10, xmm10, xmm10
       vcmpgtps xmm10, xmm8, xmm10
       vorps    xmm9, xmm10, xmm9
       vandps   xmm8, xmm9, xmm8
       vmulss   xmm9, xmm8, xmm8
       vmulss   xmm8, xmm8, xmm2
       vmulss   xmm8, xmm8, xmm9
       vaddss   xmm8, xmm8, xmm3
       vmulss   xmm9, xmm9, xmm4
       vaddss   xmm9, xmm9, xmm5
       vdivss   xmm8, xmm9, xmm8
       vaddss   xmm0, xmm8, xmm0
       inc      r8d
       cmp      r8d, 8
       jl       G_M000_IG03

G_M000_IG09:                ;; offset=0x01B6
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD68]
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:FastLog2(float):float]
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD72]
       vmulss   xmm1, xmm7, dword ptr [reloc @RWD76]
       vaddss   xmm1, xmm1, xmm6
       vaddss   xmm0, xmm0, xmm1

G_M000_IG10:                ;; offset=0x01DC
       vmovaps  xmm6, xmmword ptr [rsp+0x80]
       vmovaps  xmm7, xmmword ptr [rsp+0x70]
       vmovaps  xmm8, xmmword ptr [rsp+0x60]
       vmovaps  xmm9, xmmword ptr [rsp+0x50]
       vmovaps  xmm10, xmmword ptr [rsp+0x40]
       vmovaps  xmm11, xmmword ptr [rsp+0x30]
       vmovaps  xmm12, xmmword ptr [rsp+0x20]
       add      rsp, 144
       pop      rbx
       ret

G_M000_IG11:                ;; offset=0x0212
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	42233332h		;      40.8
RWD04  	dd	3B1DEA78h		; 0.00240961
RWD08  	dd	449E380Dh		;   1265.75
RWD12  	dd	42EF0C23h		;   119.524
RWD16  	dd	44228FFEh		;    650.25
RWD20  	dq	7FFFFFFF7FFFFFFFh, 7FFFFFFF7FFFFFFFh
RWD36  	dq	7FFFFFFF7FFFFFFFh, 7FFFFFFF7FFFFFFFh
RWD52  	dq	7FFFFFFF7FFFFFFFh, 7FFFFFFF7FFFFFFFh
RWD68  	dd	38808081h		; 6.12745e-05
RWD72  	dd	BDDC6A08h		; -0.107624
RWD76  	dd	B8933E1Eh		; -7.02108e-05

; Total bytes of code 536

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:FastLog2(float):float (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000

G_M000_IG02:                ;; offset=0x0000
       vmovd    eax, xmm0
       lea      ecx, [rax+0xC0D55555]
       sar      ecx, 23
       mov      edx, ecx
       shl      edx, 23
       sub      eax, edx
       vmovd    xmm0, eax
       vsubss   xmm0, xmm0, dword ptr [reloc @RWD00]
       vmulss   xmm1, xmm0, dword ptr [reloc @RWD04]
       vaddss   xmm1, xmm1, dword ptr [reloc @RWD08]
       vmulss   xmm1, xmm1, xmm0
       vsubss   xmm1, xmm1, dword ptr [reloc @RWD12]
       vmulss   xmm2, xmm0, dword ptr [reloc @RWD16]
       vaddss   xmm2, xmm2, dword ptr [reloc @RWD20]
       vmulss   xmm0, xmm2, xmm0
       vaddss   xmm0, xmm0, dword ptr [reloc @RWD24]
       vdivss   xmm0, xmm1, xmm0
       vxorps   xmm1, xmm1, xmm1
       vcvtsi2ss xmm1, xmm1, ecx
       vaddss   xmm0, xmm0, xmm1

G_M000_IG03:                ;; offset=0x0068
       ret

RWD00  	dd	3F800000h		;         1
RWD04  	dd	3F3E11C7h		;  0.742459
RWD08  	dd	3FB6E02Bh		;   1.42872
RWD12  	dd	35F85AB0h		; 1.85038e-06
RWD16  	dd	3E32458Ch		;  0.174093
RWD20  	dd	3F813CEDh		;   1.00967
RWD24  	dd	3F7D8625h		;  0.990328

; Total bytes of code 105

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:FastPow2(float):float (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data

G_M000_IG01:                ;; offset=0x0000

G_M000_IG02:                ;; offset=0x0000
       vroundss xmm1, xmm1, xmm0, 9
       vmovaps  xmm2, xmm1
       vcmpordss xmm3, xmm2, xmm2
       vandps   xmm3, xmm3, xmm2
       mov      eax, 0x7FFFFFFF
       vcvttss2si ecx, xmm3
       vucomiss xmm2, dword ptr [reloc @RWD00]
       cmovb    eax, ecx
       add      eax, 127
       shl      eax, 23
       vmovd    xmm2, eax
       vsubss   xmm0, xmm0, xmm1
       vaddss   xmm1, xmm0, dword ptr [reloc @RWD04]
       vmulss   xmm1, xmm1, xmm0
       vaddss   xmm1, xmm1, dword ptr [reloc @RWD08]
       vmulss   xmm1, xmm1, xmm0
       vaddss   xmm1, xmm1, dword ptr [reloc @RWD12]
       vmulss   xmm1, xmm1, xmm2
       vmulss   xmm2, xmm0, dword ptr [reloc @RWD16]
       vsubss   xmm2, xmm2, dword ptr [reloc @RWD20]
       vmulss   xmm2, xmm2, xmm0
       vsubss   xmm2, xmm2, dword ptr [reloc @RWD24]
       vmulss   xmm0, xmm2, xmm0
       vaddss   xmm0, xmm0, dword ptr [reloc @RWD28]
       vdivss   xmm0, xmm1, xmm0

G_M000_IG03:                ;; offset=0x0085
       ret

RWD00  	dd	4F000000h		; 2.14748e+09
RWD04  	dd	4122CC6Bh		;   10.1749
RWD08  	dd	424379A1h		;   48.8688
RWD12  	dd	42C519F0h		;   98.5507
RWD16  	dd	3E5749EEh		;  0.210243
RWD20  	dd	3CB621BEh		; 0.0222329
RWD24  	dd	419B8831h		;   19.4415
RWD28  	dd	42C519F1h		;   98.5507

; Total bytes of code 134

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ComputeRowCore(DotJpegli.Internal.PixelSource,int,int,int,System.Span`1[float],bool,bool,System.Threading.CancellationToken):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rbp based frame
; partially interruptible
; No PGO data
; 9 inlinees with PGO data; 35 single block inlinees; 35 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbp
       push     r15
       push     r14
       push     r13
       push     r12
       push     rdi
       push     rsi
       push     rbx
       sub      rsp, 504
       vzeroupper
       vmovaps  xmmword ptr [rsp+0x1E0], xmm6
       vmovaps  xmmword ptr [rsp+0x1D0], xmm7
       vmovaps  xmmword ptr [rsp+0x1C0], xmm8
       vmovaps  xmmword ptr [rsp+0x1B0], xmm9
       vmovaps  xmmword ptr [rsp+0x1A0], xmm10
       vmovaps  xmmword ptr [rsp+0x190], xmm11
       vmovaps  xmmword ptr [rsp+0x180], xmm12
       lea      rbp, [rsp+0x40]
       xor      eax, eax
       mov      qword ptr [rbp+0x88], rax
       vxorps   xmm4, xmm4, xmm4
       vmovdqu  ymmword ptr [rbp+0x90], ymm4
       vmovdqu  ymmword ptr [rbp+0xB0], ymm4
       vmovdqu  ymmword ptr [rbp+0xD0], ymm4
       mov      qword ptr [rbp+0xF0], rax
       mov      rax, 0x6911662D01EE
       mov      qword ptr [rbp+0x138], rax
       mov      rax, bword ptr [rbp+0x228]
       mov      ebx, dword ptr [rbp+0x230]

G_M000_IG02:                ;; offset=0x00A4
       mov      r10, gword ptr [rbp+0x240]
       mov      gword ptr [rbp+0x88], r10
       mov      edi, r9d
       mov      dword ptr [rbp+0x90], edi
       mov      r14d, r8d
       mov      dword ptr [rbp+0x94], r14d
       mov      r15, rdx
       mov      r13, rcx
       mov      r12, bword ptr [rax]
       mov      bword ptr [rbp+0x20], r12
       mov      r10d, dword ptr [rax+0x08]
       mov      dword ptr [rbp+0xD8], r10d
       cmp      byte  ptr [r13+0x6A], 0
       jne      G_M000_IG141
       mov      r9d, dword ptr [r13+0x48]
       shl      r9d, 3
       mov      dword ptr [rbp+0x134], r9d
       mov      r11, gword ptr [r15+0x08]
       mov      gword ptr [rbp+0x78], r11
       test     r11, r11
       je       SHORT G_M000_IG08

G_M000_IG03:                ;; offset=0x0104
       cmp      r11, gword ptr [r13+0x38]
       je       SHORT G_M000_IG05

G_M000_IG04:                ;; offset=0x010A
       mov      r11, gword ptr [rbp+0x78]
       jmp      SHORT G_M000_IG08

G_M000_IG05:                ;; offset=0x0110
       mov      eax, r9d
       add      eax, dword ptr [r13+0x5C]
       cmp      eax, edi
       je       SHORT G_M000_IG07

G_M000_IG06:                ;; offset=0x011B
       mov      r11, gword ptr [rbp+0x78]
       jmp      SHORT G_M000_IG08

G_M000_IG07:                ;; offset=0x0121
       cmp      r14d, dword ptr [r13+0x60]
       mov      r11, gword ptr [rbp+0x78]
       mov      r14d, dword ptr [rbp+0x94]
       je       SHORT G_M000_IG09

G_M000_IG08:                ;; offset=0x0132
       xor      ecx, ecx
       jmp      SHORT G_M000_IG10

G_M000_IG09:                ;; offset=0x0136
       movzx    rcx, bl
       movzx    rax, byte  ptr [r13+0x6B]
       cmp      ecx, eax
       sete     cl
       movzx    rcx, cl

G_M000_IG10:                ;; offset=0x0146
       mov      dword ptr [rbp+0x130], ecx
       xor      rax, rax
       mov      gword ptr [r13+0x38], rax
       cmp      gword ptr [rbp+0x88], 0
       jne      G_M000_IG138

G_M000_IG11:                ;; offset=0x0160
       test     edi, edi
       jl       G_M000_IG143
       cmp      edi, dword ptr [r13+0x44]
       jge      G_M000_IG143
       mov      eax, edi
       cdq
       idiv     edx:eax, r9d
       test     edx, edx
       jne      G_M000_IG143
       mov      r8d, dword ptr [r13+0x64]
       imul     r8d, dword ptr [r13+0x48]
       cmp      r8d, r10d
       jg       G_M000_IG144
       lea      eax, [rdi-0x05]
       mov      dword ptr [rbp+0x12C], eax
       xor      edx, edx
       test     ecx, ecx
       je       G_M000_IG139

G_M000_IG12:                ;; offset=0x01A5
       mov      r8, gword ptr [r13+0x18]
       mov      gword ptr [rbp+0x18], r8
       mov      rdx, r8
       mov      r8d, dword ptr [r13+0x54]
       mov      dword ptr [rbp+0xA0], r8d
       imul     r8d, r9d
       mov      ecx, dword ptr [rbp+0xA0]
       lea      ecx, [rcx+4*rcx]
       add      ecx, ecx
       test     rdx, rdx
       jne      G_M000_IG140
       or       r8d, ecx
       jne      G_M000_IG145
       xor      rdx, rdx
       mov      bword ptr [rbp+0x48], rdx
       xor      ecx, ecx

G_M000_IG13:                ;; offset=0x01E4
       mov      r8, gword ptr [rbp+0x18]
       test     r8, r8
       je       SHORT G_M000_IG14
       lea      rdx, bword ptr [r8+0x10]
       mov      r8d, dword ptr [r8+0x08]
       mov      bword ptr [rbp+0x40], rdx
       mov      edx, r8d
       jmp      SHORT G_M000_IG15

G_M000_IG14:                ;; offset=0x01FE
       xor      r8, r8
       mov      bword ptr [rbp+0x40], r8
       xor      edx, edx

G_M000_IG15:                ;; offset=0x0207
       cmp      ecx, edx
       jg       G_M000_IG146
       mov      ecx, ecx
       lea      r8, [4*rcx]
       mov      rcx, bword ptr [rbp+0x40]
       mov      rdx, bword ptr [rbp+0x48]
       call     [System.SpanHelpers:Memmove(byref,byref,nuint)]
       mov      eax, 10
       mov      r10d, dword ptr [rbp+0x12C]
       lea      r8d, [r10+0x09]
       mov      edx, dword ptr [r13+0x44]
       dec      edx
       js       G_M000_IG147
       test     r8d, r8d
       jl       SHORT G_M000_IG16
       cmp      r8d, edx
       cmovg    r8d, edx
       jmp      SHORT G_M000_IG17

G_M000_IG16:                ;; offset=0x0251
       xor      r8d, r8d

G_M000_IG17:                ;; offset=0x0254
       mov      edx, eax
       mov      r9d, dword ptr [rbp+0x134]
       mov      r11, gword ptr [rbp+0x78]

G_M000_IG18:                ;; offset=0x0261
       jmp      G_M000_IG39

G_M000_IG19:                ;; offset=0x0266
       mov      rcx, gword ptr [rbp+0x88]
       cmp      dword ptr [rcx+0x20], 0
       je       G_M000_IG41
       jmp      G_M000_IG142

G_M000_IG20:                ;; offset=0x027C
       xor      r11d, r11d
       mov      qword ptr [rbp+0x98], rax

G_M000_IG21:                ;; offset=0x0286
       imul     r11d, dword ptr [r15+0x20]
       lea      edx, [r11+r9]
       mov      rcx, r15
       mov      r8d, dword ptr [rbp+0x104]
       call     [DotJpegli.Internal.PixelSource:ReadSample(int,int):float:this]
       mov      rdx, bword ptr [rbp+0x30]
       mov      r8, qword ptr [rbp+0x98]
       vmovss   dword ptr [rdx+4*r8], xmm0
       inc      r8d
       cmp      r8d, edi
       mov      rax, r8
       mov      ecx, dword ptr [rbp+0x104]
       mov      r9d, dword ptr [rbp+0xFC]
       jge      G_M000_IG35

G_M000_IG22:                ;; offset=0x02CC
       mov      r8d, dword ptr [rbp+0x10C]

G_M000_IG23:                ;; offset=0x02D3
       lea      r11d, [r8-0x01]
       test     r11d, r11d
       jl       G_M000_IG149
       mov      r8d, dword ptr [rbp+0x10C]
       test     eax, eax
       jl       SHORT G_M000_IG20

G_M000_IG24:                ;; offset=0x02EB
       cmp      eax, r11d
       jg       G_M000_IG57
       mov      qword ptr [rbp+0x98], rax
       mov      r11d, eax
       jmp      SHORT G_M000_IG21

G_M000_IG25:                ;; offset=0x0300
       xor      r9d, r9d
       mov      dword ptr [rsp+0x20], r9d
       mov      dword ptr [rsp+0x28], esi
       mov      rdi, bword ptr [rbp+0x38]
       mov      bword ptr [rbp+0xC8], rdi
       mov      r9d, dword ptr [rbp+0xDC]
       mov      dword ptr [rbp+0xD0], r9d
       lea      r9, [rbp+0xC8]
       mov      qword ptr [rsp+0x30], r9
       mov      edx, r8d
       mov      r8d, dword ptr [rbp+0x108]
       mov      r9d, ecx
       mov      rcx, r15
       call     [DotJpegli.Internal.PixelSource:ReadConvertedRow(int,int,int,int,int,System.Span`1[float]):this]
       jmp      G_M000_IG35

G_M000_IG26:                ;; offset=0x034C
       mov      r9d, dword ptr [r13+0x40]
       mov      r8d, r9d
       mov      dword ptr [rbp+0x100], r11d
       mov      eax, dword ptr [r15+0x1C]
       test     eax, eax
       jne      SHORT G_M000_IG29
       mov      ecx, dword ptr [rbp+0x100]
       cmp      byte  ptr [r15+0x26], 0
       je       SHORT G_M000_IG27
       mov      r11d, 4
       jmp      SHORT G_M000_IG30

G_M000_IG27:                ;; offset=0x0377
       cmp      dword ptr [r15+0x20], 1
       jne      SHORT G_M000_IG28
       mov      r11d, 1
       jmp      SHORT G_M000_IG30

G_M000_IG28:                ;; offset=0x0386
       mov      r11d, 2
       jmp      SHORT G_M000_IG30

G_M000_IG29:                ;; offset=0x038E
       mov      ecx, dword ptr [rbp+0x100]
       mov      r11d, eax

G_M000_IG30:                ;; offset=0x0397
       cmp      r11d, 3
       jne      SHORT G_M000_IG31
       xor      eax, eax
       jmp      SHORT G_M000_IG32

G_M000_IG31:                ;; offset=0x03A1
       cmp      r11d, 4
       setne    al
       movzx    rax, al

G_M000_IG32:                ;; offset=0x03AB
       test     eax, eax
       sete     al
       movzx    rax, al
       mov      r11, gword ptr [r13+0x18]
       test     r11, r11
       je       SHORT G_M000_IG33
       mov      edi, r14d
       mov      ecx, r9d
       add      rcx, rdi
       mov      edi, dword ptr [r11+0x08]
       cmp      rcx, rdi
       ja       G_M000_IG145
       mov      ecx, r14d
       lea      rcx, bword ptr [r11+4*rcx+0x10]
       mov      r11d, r9d
       mov      r9, rcx
       mov      ecx, dword ptr [rbp+0x100]
       jmp      SHORT G_M000_IG34

G_M000_IG33:                ;; offset=0x03E8
       or       r9d, r14d
       jne      G_M000_IG145
       xor      r9, r9
       xor      r11d, r11d

G_M000_IG34:                ;; offset=0x03F7
       mov      dword ptr [rsp+0x20], eax
       xor      eax, eax
       mov      dword ptr [rsp+0x28], eax
       mov      dword ptr [rsp+0x30], esi
       mov      bword ptr [rbp+0xC8], r9
       mov      dword ptr [rbp+0xD0], r11d
       lea      r9, [rbp+0xC8]
       mov      qword ptr [rsp+0x38], r9
       mov      edx, r8d
       mov      r8d, ecx
       mov      r9d, dword ptr [rbp+0x94]
       mov      rcx, r15
       call     [DotJpegli.Internal.PixelSource:ReadRowVector(int,int,int,int,int,int,System.Span`1[float]):this]

G_M000_IG35:                ;; offset=0x0435
       mov      rdx, gword ptr [r13+0x18]
       mov      rcx, rdx
       mov      rax, rdx
       mov      r8d, dword ptr [rax+0x08]
       cmp      r14d, r8d
       jae      G_M000_IG152
       mov      r10d, r14d
       vmovss   xmm2, dword ptr [rax+4*r10+0x10]
       lea      eax, [r14-0x01]
       cmp      eax, r8d
       jae      G_M000_IG152
       lea      eax, [r14-0x01]
       vmovss   dword ptr [rcx+4*rax+0x10], xmm2
       mov      rcx, rdx
       mov      eax, dword ptr [r13+0x40]
       add      r14d, eax
       mov      r10d, r14d
       mov      r9d, dword ptr [r13+0x4C]
       sub      r9d, eax
       inc      r9d
       test     rcx, rcx
       je       SHORT G_M000_IG36
       mov      eax, r10d
       mov      r11d, r9d
       add      rax, r11
       mov      r11d, r8d
       cmp      rax, r11
       ja       G_M000_IG145
       lea      rcx, bword ptr [rcx+4*r10+0x10]
       jmp      SHORT G_M000_IG37

G_M000_IG36:                ;; offset=0x04A5
       or       r10d, r9d
       jne      G_M000_IG145
       xor      rcx, rcx
       xor      r9d, r9d

G_M000_IG37:                ;; offset=0x04B3
       dec      r14d
       cmp      r14d, r8d
       jae      G_M000_IG152
       mov      eax, r14d
       vmovss   xmm2, dword ptr [rdx+4*rax+0x10]
       mov      edx, r9d
       call     [System.SpanHelpers:Fill[float](byref,nuint,float)]

G_M000_IG38:                ;; offset=0x04D1
       mov      r8d, esi
       mov      esi, dword ptr [rbp+0x124]
       inc      esi
       mov      edx, esi
       mov      r9d, dword ptr [rbp+0x134]
       mov      r11, gword ptr [rbp+0x78]
       mov      r14d, dword ptr [rbp+0x94]

G_M000_IG39:                ;; offset=0x04F0
       lea      ecx, [r9+0x0A]
       cmp      edx, ecx
       jge      G_M000_IG63

G_M000_IG40:                ;; offset=0x04FC
       cmp      gword ptr [rbp+0x88], 0
       jne      G_M000_IG19

G_M000_IG41:                ;; offset=0x050A
       mov      ecx, edx
       add      ecx, dword ptr [rbp+0x12C]
       mov      r11d, dword ptr [r13+0x44]
       lea      eax, [r11-0x01]
       mov      esi, eax
       test     esi, esi
       jl       G_M000_IG148
       test     ecx, ecx
       jl       SHORT G_M000_IG44

G_M000_IG42:                ;; offset=0x0528
       cmp      ecx, esi
       jg       SHORT G_M000_IG43
       mov      esi, ecx
       jmp      SHORT G_M000_IG45

G_M000_IG43:                ;; offset=0x0530
       jmp      SHORT G_M000_IG45

G_M000_IG44:                ;; offset=0x0532
       xor      esi, esi

G_M000_IG45:                ;; offset=0x0534
       mov      ecx, dword ptr [r13+0x54]
       mov      dword ptr [rbp+0x124], edx
       mov      r9d, edx
       imul     r9d, ecx
       lea      r14d, [r9+0x01]
       cmp      esi, r8d
       je       G_M000_IG58

G_M000_IG46:                ;; offset=0x0552
       test     bl, bl
       jne      G_M000_IG26
       mov      r9d, dword ptr [r13+0x40]
       mov      r8d, r9d
       mov      dword ptr [rbp+0x10C], r8d
       mov      dword ptr [rbp+0x108], r11d
       mov      ecx, dword ptr [r15+0x1C]
       test     ecx, ecx
       jne      SHORT G_M000_IG49
       cmp      byte  ptr [r15+0x26], 0
       je       SHORT G_M000_IG47
       mov      ecx, 4
       jmp      SHORT G_M000_IG49

G_M000_IG47:                ;; offset=0x0585
       cmp      dword ptr [r15+0x20], 1
       jne      SHORT G_M000_IG48
       mov      ecx, 1
       jmp      SHORT G_M000_IG49

G_M000_IG48:                ;; offset=0x0593
       mov      ecx, 2

G_M000_IG49:                ;; offset=0x0598
       cmp      ecx, 3
       jne      SHORT G_M000_IG50
       xor      ecx, ecx
       jmp      SHORT G_M000_IG51

G_M000_IG50:                ;; offset=0x05A1
       cmp      ecx, 4
       setne    cl
       movzx    rcx, cl

G_M000_IG51:                ;; offset=0x05AA
       test     ecx, ecx
       sete     cl
       movzx    rcx, cl
       mov      dword ptr [rbp+0x104], ecx
       mov      rdi, gword ptr [r13+0x18]
       test     rdi, rdi
       je       SHORT G_M000_IG52
       mov      r11d, r14d
       mov      qword ptr [rbp+0x08], r11
       mov      r11d, r9d
       add      r11, qword ptr [rbp+0x08]
       mov      qword ptr [rbp+0x08], r11
       mov      r11d, dword ptr [rdi+0x08]
       cmp      qword ptr [rbp+0x08], r11
       ja       G_M000_IG145
       mov      r11d, r14d
       lea      rdi, bword ptr [rdi+4*r11+0x10]
       jmp      SHORT G_M000_IG53

G_M000_IG52:                ;; offset=0x05EB
       or       r9d, r14d
       jne      G_M000_IG145
       xor      rdi, rdi
       xor      r9d, r9d

G_M000_IG53:                ;; offset=0x05F9
       mov      bword ptr [rbp+0x38], rdi
       mov      bword ptr [rbp+0x30], rdi
       mov      dword ptr [rbp+0xDC], r9d
       mov      edi, r9d
       mov      r9, gword ptr [r15+0x08]
       test     r9, r9
       je       SHORT G_M000_IG54
       cmp      byte  ptr [r9+0x2C], 0
       je       G_M000_IG25
       test     ecx, ecx
       mov      ecx, dword ptr [rbp+0x104]
       je       G_M000_IG25

G_M000_IG54:                ;; offset=0x062D
       cmp      esi, eax
       jg       SHORT G_M000_IG55
       mov      r9d, esi
       jmp      SHORT G_M000_IG56

G_M000_IG55:                ;; offset=0x0636
       mov      r9d, eax

G_M000_IG56:                ;; offset=0x0639
       mov      eax, dword ptr [rbp+0x94]
       imul     r9d, eax
       mov      dword ptr [rbp+0xFC], r9d
       xor      eax, eax
       test     edi, edi
       je       G_M000_IG35
       jmp      G_M000_IG23

G_M000_IG57:                ;; offset=0x0659
       mov      qword ptr [rbp+0x98], rax
       jmp      G_M000_IG21

G_M000_IG58:                ;; offset=0x0665
       mov      r8, gword ptr [r13+0x18]
       mov      edx, dword ptr [rbp+0x124]
       lea      eax, [rdx-0x01]
       imul     eax, ecx
       mov      r11d, ecx
       test     r8, r8
       je       SHORT G_M000_IG59
       mov      r14d, eax
       mov      edi, r11d
       add      rdi, r14
       mov      r14d, dword ptr [r8+0x08]
       cmp      rdi, r14
       ja       G_M000_IG145
       lea      rax, bword ptr [r8+4*rax+0x10]
       jmp      SHORT G_M000_IG60

G_M000_IG59:                ;; offset=0x069A
       or       eax, r11d
       jne      G_M000_IG145
       xor      rax, rax
       xor      r11d, r11d

G_M000_IG60:                ;; offset=0x06A8
       mov      r8, gword ptr [r13+0x18]
       test     r8, r8
       je       SHORT G_M000_IG61
       mov      r14d, r9d
       mov      edi, ecx
       add      rdi, r14
       mov      r14d, dword ptr [r8+0x08]
       cmp      rdi, r14
       ja       G_M000_IG145
       mov      r9d, r9d
       lea      r9, bword ptr [r8+4*r9+0x10]
       jmp      SHORT G_M000_IG62

G_M000_IG61:                ;; offset=0x06D0
       or       r9d, ecx
       jne      G_M000_IG145
       xor      r9, r9
       xor      ecx, ecx

G_M000_IG62:                ;; offset=0x06DE
       cmp      r11d, ecx
       jg       G_M000_IG146
       mov      r8d, r11d
       shl      r8, 2
       mov      rcx, r9
       mov      rdx, rax
       call     [System.SpanHelpers:Memmove(byref,byref,nuint)]
       jmp      G_M000_IG38

G_M000_IG63:                ;; offset=0x06FF
       mov      edi, dword ptr [rbp+0x90]
       mov      r15d, edi
       shr      r15d, 2
       dec      r15d
       mov      r9d, dword ptr [r13+0x50]
       mov      edx, r9d
       sar      edx, 31
       and      edx, 3
       add      r9d, edx
       sar      r9d, 2
       mov      dword ptr [rbp+0x128], r9d
       xor      edx, edx
       cmp      dword ptr [rbp+0x130], 0
       jne      SHORT G_M000_IG65

G_M000_IG64:                ;; offset=0x0735
       mov      ecx, -1
       jmp      G_M000_IG72

G_M000_IG65:                ;; offset=0x073F
       mov      r8, gword ptr [r13+0x20]
       mov      gword ptr [rbp+0x10], r8
       mov      rcx, r8
       mov      edx, dword ptr [r13+0x48]
       add      edx, edx
       mov      r8d, dword ptr [r13+0x58]
       imul     edx, r8d
       add      r8d, r8d
       test     rcx, rcx
       je       SHORT G_M000_IG66
       mov      edi, edx
       mov      qword ptr [rbp+0x08], rdi
       mov      edi, r8d
       add      rdi, qword ptr [rbp+0x08]
       mov      qword ptr [rbp+0x08], rdi
       mov      edi, dword ptr [rcx+0x08]
       cmp      qword ptr [rbp+0x08], rdi
       ja       G_M000_IG145
       lea      rdx, bword ptr [rcx+4*rdx+0x10]
       mov      bword ptr [rbp+0x28], rdx
       jmp      SHORT G_M000_IG67

G_M000_IG66:                ;; offset=0x0789
       or       edx, r8d
       jne      G_M000_IG145
       xor      rdx, rdx
       mov      bword ptr [rbp+0x28], rdx
       xor      r8d, r8d

G_M000_IG67:                ;; offset=0x079B
       mov      rcx, gword ptr [rbp+0x10]
       test     rcx, rcx
       je       SHORT G_M000_IG68
       lea      rdx, bword ptr [rcx+0x10]
       mov      ecx, dword ptr [rcx+0x08]
       mov      eax, ecx
       mov      rcx, rdx
       mov      edx, eax
       jmp      SHORT G_M000_IG69

G_M000_IG68:                ;; offset=0x07B4
       xor      rcx, rcx
       xor      edx, edx

G_M000_IG69:                ;; offset=0x07B8
       cmp      r8d, edx
       jg       G_M000_IG146
       mov      r8d, r8d
       shl      r8, 2
       mov      rdx, bword ptr [rbp+0x28]
       call     [System.SpanHelpers:Memmove(byref,byref,nuint)]
       mov      eax, 2
       lea      ecx, [r15+0x01]
       mov      r10d, dword ptr [rbp+0x128]
       lea      edx, [r10-0x01]
       test     edx, edx
       jl       G_M000_IG150
       test     ecx, ecx
       jl       SHORT G_M000_IG70
       cmp      ecx, edx
       cmovg    ecx, edx
       jmp      SHORT G_M000_IG71

G_M000_IG70:                ;; offset=0x07F9
       xor      ecx, ecx

G_M000_IG71:                ;; offset=0x07FB
       shl      ecx, 2
       mov      edx, eax
       mov      r11, gword ptr [rbp+0x78]

G_M000_IG72:                ;; offset=0x0804
       mov      r8d, edx
       jmp      G_M000_IG86

G_M000_IG73:                ;; offset=0x080C
       cmp      gword ptr [rbp+0x88], 0
       jne      G_M000_IG118

G_M000_IG74:                ;; offset=0x081A
       lea      edx, [r15+r8]
       mov      r9d, dword ptr [rbp+0x128]
       dec      r9d
       js       G_M000_IG151
       test     edx, edx
       jl       SHORT G_M000_IG77

G_M000_IG75:                ;; offset=0x0832
       cmp      edx, r9d
       jg       SHORT G_M000_IG76
       jmp      SHORT G_M000_IG78

G_M000_IG76:                ;; offset=0x0839
       mov      edx, r9d
       jmp      SHORT G_M000_IG78

G_M000_IG77:                ;; offset=0x083E
       xor      edx, edx

G_M000_IG78:                ;; offset=0x0840
       lea      r9d, [4*rdx]
       mov      dword ptr [rbp+0x11C], r9d
       cmp      r9d, ecx
       je       SHORT G_M000_IG80

G_M000_IG79:                ;; offset=0x0854
       mov      edx, r9d
       sub      edx, dword ptr [rbp+0x12C]
       movzx    r9, bl
       mov      rcx, r13
       mov      dword ptr [rbp+0x120], r8d
       call     [DotJpegli.Internal.AdaptiveQuantization:ComputePreErosion(int,int,bool):this]
       jmp      G_M000_IG85

G_M000_IG80:                ;; offset=0x0876
       mov      rcx, gword ptr [r13+0x20]
       mov      rdx, rcx
       lea      edi, [r8-0x01]
       mov      eax, dword ptr [r13+0x58]
       imul     edi, eax
       mov      esi, eax
       test     rdx, rdx
       je       SHORT G_M000_IG81
       mov      r12d, edi
       mov      r14d, esi
       add      r14, r12
       mov      r12d, dword ptr [rdx+0x08]
       cmp      r14, r12
       ja       G_M000_IG145
       lea      rdx, bword ptr [rdx+4*rdi+0x10]
       mov      r12, bword ptr [rbp+0x20]
       mov      r14d, dword ptr [rbp+0x94]
       jmp      SHORT G_M000_IG82

G_M000_IG81:                ;; offset=0x08B7
       or       edi, esi
       jne      G_M000_IG145
       xor      rdx, rdx
       xor      esi, esi

G_M000_IG82:                ;; offset=0x08C3
       mov      dword ptr [rbp+0x120], r8d
       mov      edi, r8d
       imul     edi, eax
       test     rcx, rcx
       je       SHORT G_M000_IG83
       mov      r12d, edi
       mov      r14d, eax
       add      r14, r12
       mov      r12d, dword ptr [rcx+0x08]
       cmp      r14, r12
       ja       G_M000_IG145
       lea      rcx, bword ptr [rcx+4*rdi+0x10]
       mov      r12, bword ptr [rbp+0x20]
       mov      r14d, dword ptr [rbp+0x94]
       jmp      SHORT G_M000_IG84

G_M000_IG83:                ;; offset=0x08FD
       or       edi, eax
       jne      G_M000_IG145
       xor      rcx, rcx
       xor      eax, eax

G_M000_IG84:                ;; offset=0x0909
       cmp      esi, eax
       jg       G_M000_IG146
       mov      eax, esi
       lea      r8, [4*rax]
       call     [System.SpanHelpers:Memmove(byref,byref,nuint)]

G_M000_IG85:                ;; offset=0x0921
       mov      ecx, dword ptr [rbp+0x11C]
       mov      r8d, dword ptr [rbp+0x120]
       inc      r8d
       mov      r11, gword ptr [rbp+0x78]

G_M000_IG86:                ;; offset=0x0935
       mov      edx, dword ptr [r13+0x48]
       lea      edx, [2*rdx+0x02]
       cmp      r8d, edx
       jl       G_M000_IG73

G_M000_IG87:                ;; offset=0x0949
       mov      esi, dword ptr [rbp+0x220]
       cmp      esi, 9
       jge      SHORT G_M000_IG89

G_M000_IG88:                ;; offset=0x0954
       vmovss   xmm6, dword ptr [reloc @RWD00]
       vmovaps  xmm0, xmm6
       jmp      SHORT G_M000_IG90

G_M000_IG89:                ;; offset=0x0962
       vxorps   xmm0, xmm0, xmm0
       vcvtsi2ss xmm0, xmm0, esi
       vsubss   xmm0, xmm0, dword ptr [reloc @RWD04]
       vdivss   xmm0, xmm0, dword ptr [reloc @RWD08]
       vmovss   xmm6, dword ptr [reloc @RWD00]
       vsubss   xmm0, xmm6, xmm0
       vxorps   xmm1, xmm1, xmm1
       vcmpeqps xmm1, xmm1, xmm0
       vxorps   xmm2, xmm2, xmm2
       vpcmpgtd xmm2, xmm2, xmm0
       vandps   xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpltps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandnps  xmm0, xmm1, xmm0

G_M000_IG90:                ;; offset=0x09AC
       vmulss   xmm7, xmm0, dword ptr [reloc @RWD12]
       vsubss   xmm0, xmm6, xmm0
       vmulss   xmm8, xmm0, dword ptr [reloc @RWD16]
       test     bl, bl
       je       SHORT G_M000_IG92

G_M000_IG91:                ;; offset=0x09C4
       cmp      dword ptr [r13+0x64], 8
       jge      SHORT G_M000_IG93

G_M000_IG92:                ;; offset=0x09CB
       xor      esi, esi
       jmp      SHORT G_M000_IG94

G_M000_IG93:                ;; offset=0x09CF
       movzx    rsi, byte  ptr [rbp+0x238]
       or       esi, 1

G_M000_IG94:                ;; offset=0x09DA
       test     esi, esi
       jne      SHORT G_M000_IG96

G_M000_IG95:                ;; offset=0x09DE
       xor      r15, r15
       xor      eax, eax
       jmp      SHORT G_M000_IG97

G_M000_IG96:                ;; offset=0x09E5
       test     dword ptr [rsp], esp
       sub      rsp, 512
       lea      rcx, [rsp+0x40]
       mov      qword ptr [rbp+0x80], rcx
       mov      edx, 512
       call     CORINFO_HELP_MEMZERO
       mov      rcx, qword ptr [rbp+0x80]
       mov      r15, rcx
       mov      eax, 16

G_M000_IG97:                ;; offset=0x0A14
       mov      dword ptr [rbp+0xE4], eax
       test     esi, esi
       jne      SHORT G_M000_IG99

G_M000_IG98:                ;; offset=0x0A1E
       xor      r10, r10
       xor      r11d, r11d
       jmp      SHORT G_M000_IG100

G_M000_IG99:                ;; offset=0x0A26
       test     dword ptr [rsp], esp
       sub      rsp, 64
       lea      r10, [rsp+0x40]
       vxorps   ymm0, ymm0, ymm0
       vmovdqu  ymmword ptr [r10], ymm0
       vmovdqu  ymmword ptr [r10+0x20], ymm0
       mov      r11d, 16

G_M000_IG100:                ;; offset=0x0A47
       mov      bword ptr [rbp+0x50], r10
       mov      dword ptr [rbp+0xE0], r11d
       xor      r8d, r8d
       vmovss   xmm9, dword ptr [reloc @RWD20]
       jmp      G_M000_IG128

G_M000_IG101:                ;; offset=0x0A62
       mov      rdx, gword ptr [rbp+0x88]
       cmp      dword ptr [rdx+0x20], 0
       je       G_M000_IG130
       jmp      G_M000_IG142

G_M000_IG102:                ;; offset=0x0A78
       mov      bword ptr [rbp+0xB8], r15
       mov      r10d, dword ptr [rbp+0xE4]
       mov      dword ptr [rbp+0xC0], r10d
       mov      r11, bword ptr [rbp+0x50]
       mov      bword ptr [rbp+0xC8], r11
       mov      ecx, dword ptr [rbp+0xE0]
       mov      dword ptr [rbp+0xD0], ecx
       lea      r9, [rbp+0xB8]
       lea      r8, [rbp+0xC8]
       mov      qword ptr [rsp+0x20], r8
       mov      edx, dword ptr [rbp+0x118]
       lea      r8d, [8*rdx+0x05]
       mov      dword ptr [rbp+0x114], eax
       lea      edx, [8*rax]
       mov      rcx, r13
       call     [DotJpegli.Internal.AdaptiveQuantization:ModulateEightBlocks(int,int,System.Span`1[System.Runtime.Intrinsics.Vector256`1[float]],System.Span`1[float]):this]
       xor      eax, eax
       cmp      dword ptr [rbp+0xE0], 8
       jge      G_M000_IG134

G_M000_IG103:                ;; offset=0x0AEA
       mov      dword ptr [rbp+0x110], eax
       mov      r8d, eax
       add      r8d, dword ptr [rbp+0x114]
       add      r8d, r8d
       mov      r9, gword ptr [r13+0x28]
       mov      r11, r9
       mov      edi, r8d
       add      edi, dword ptr [r13+0x58]
       mov      dword ptr [rbp+0xA4], edi
       mov      r14d, dword ptr [r11+0x08]
       cmp      edi, r14d
       jae      G_M000_IG152
       vmovss   xmm0, dword ptr [r11+4*rdi+0x10]
       mov      r11, r9
       mov      edi, dword ptr [rbp+0xA4]
       inc      edi
       cmp      edi, r14d
       jae      G_M000_IG152
       vaddss   xmm0, xmm0, dword ptr [r11+4*rdi+0x10]
       mov      r11, r9
       cmp      r8d, r14d
       jae      G_M000_IG152
       mov      edi, r8d
       vaddss   xmm0, xmm0, dword ptr [r11+4*rdi+0x10]
       lea      r11d, [r8+0x01]
       cmp      r11d, r14d
       jae      G_M000_IG152
       inc      r8d
       vaddss   xmm11, xmm0, dword ptr [r9+4*r8+0x10]
       movzx    rdx, byte  ptr [r13+0x69]
       mov      r8, gword ptr [r13+0x10]
       vmovaps  xmm0, xmm11
       call     [DotJpegli.Internal.AdaptiveQuantization:ComputeMask(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float]
       vmovaps  xmm11, xmm0
       mov      edi, dword ptr [rbp+0x110]
       mov      r14d, dword ptr [rbp+0xE0]
       cmp      edi, r14d
       jae      G_M000_IG152
       mov      edx, edi
       mov      rax, bword ptr [rbp+0x50]
       vaddss   xmm11, xmm11, dword ptr [rax+4*rdx]
       lea      edx, [rdi+0x08]
       cmp      edx, r14d
       jae      G_M000_IG152
       lea      edx, [rdi+0x08]
       vmovss   xmm0, dword ptr [rax+4*rdx]
       movzx    rdx, byte  ptr [r13+0x69]
       mov      r8, gword ptr [r13+0x10]
       call     [DotJpegli.Internal.AdaptiveQuantization:FastLog2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float]
       vmulss   xmm0, xmm9, xmm0
       vaddss   xmm11, xmm11, xmm0
       vmulss   xmm0, xmm11, xmm10

G_M000_IG104:                ;; offset=0x0BD6
       movzx    rdx, byte  ptr [r13+0x69]
       mov      r8, gword ptr [r13+0x10]
       call     [DotJpegli.Internal.AdaptiveQuantization:FastPow2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float]
       vmulss   xmm0, xmm0, xmm7
       vaddss   xmm1, xmm8, xmm0
       mov      eax, dword ptr [rbp+0x118]
       mov      ecx, eax
       imul     ecx, dword ptr [r13+0x64]
       add      ecx, dword ptr [rbp+0x114]
       add      ecx, edi
       cmp      ecx, dword ptr [rbp+0xD8]
       jae      G_M000_IG152
       mov      r8d, ecx
       lea      r9, bword ptr [r12+4*r8]
       mov      bword ptr [rbp+0x68], r9
       movzx    r11, byte  ptr [r13+0x69]
       mov      r8, gword ptr [r13+0x10]
       mov      gword ptr [rbp+0x60], r8
       mov      byte  ptr [rbp+0xF0], 0
       test     r11d, r11d
       je       SHORT G_M000_IG107

G_M000_IG105:                ;; offset=0x0C32
       vmovss   xmm0, dword ptr [reloc @RWD24]
       lea      r8, [rbp+0xF0]
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovaps  xmm12, xmm0
       mov      rcx, gword ptr [rbp+0x60]
       test     rcx, rcx
       je       SHORT G_M000_IG106
       movzx    r8, byte  ptr [rbp+0xF0]
       mov      edx, 4
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG106:                ;; offset=0x0C67
       jmp      SHORT G_M000_IG108

G_M000_IG107:                ;; offset=0x0C69
       vmovss   xmm0, dword ptr [reloc @RWD24]
       vdivss   xmm12, xmm0, xmm1

G_M000_IG108:                ;; offset=0x0C75
       vsubss   xmm0, xmm12, xmm6
       vxorps   xmm1, xmm1, xmm1
       vcmpeqps xmm1, xmm1, xmm0
       vxorps   xmm2, xmm2, xmm2
       vpcmpgtd xmm2, xmm2, xmm0
       vandps   xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpltps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandnps  xmm0, xmm1, xmm0
       mov      rcx, bword ptr [rbp+0x68]
       vmovss   dword ptr [rcx], xmm0
       inc      edi
       cmp      edi, 8
       mov      eax, edi
       jl       G_M000_IG103

G_M000_IG109:                ;; offset=0x0CB4
       mov      r14, bword ptr [rbp+0x50]

G_M000_IG110:                ;; offset=0x0CB8
       mov      edx, dword ptr [rbp+0x114]
       add      edx, 8
       mov      eax, edx
       mov      r14d, dword ptr [rbp+0x94]

G_M000_IG111:                ;; offset=0x0CCA
       mov      r9d, dword ptr [r13+0x64]
       add      r9d, -8
       cmp      eax, r9d
       jle      G_M000_IG102

G_M000_IG112:                ;; offset=0x0CDB
       cmp      eax, dword ptr [r13+0x64]
       jge      G_M000_IG127

G_M000_IG113:                ;; offset=0x0CE5
       mov      dword ptr [rbp+0x114], eax
       lea      edx, [rax+rax]
       mov      r8, gword ptr [r13+0x28]
       mov      ecx, edx
       add      ecx, dword ptr [r13+0x58]
       cmp      ecx, dword ptr [r8+0x08]
       jae      G_M000_IG152
       vmovss   xmm0, dword ptr [r8+4*rcx+0x10]
       mov      r8, gword ptr [r13+0x28]
       mov      ecx, dword ptr [r13+0x58]
       lea      ecx, [rcx+rdx+0x01]
       cmp      ecx, dword ptr [r8+0x08]
       jae      G_M000_IG152
       vaddss   xmm0, xmm0, dword ptr [r8+4*rcx+0x10]
       mov      r8, gword ptr [r13+0x28]
       cmp      edx, dword ptr [r8+0x08]
       jae      G_M000_IG152
       mov      ecx, edx
       vaddss   xmm0, xmm0, dword ptr [r8+4*rcx+0x10]
       mov      r8, gword ptr [r13+0x28]
       lea      ecx, [rdx+0x01]
       cmp      ecx, dword ptr [r8+0x08]
       jae      G_M000_IG152
       inc      edx
       vaddss   xmm0, xmm0, dword ptr [r8+4*rdx+0x10]
       movzx    rdx, byte  ptr [r13+0x69]
       mov      r8, gword ptr [r13+0x10]
       call     [DotJpegli.Internal.AdaptiveQuantization:ComputeMask(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float]
       test     bl, bl
       je       G_M000_IG119

G_M000_IG114:                ;; offset=0x0D6E
       mov      eax, dword ptr [rbp+0x118]
       lea      r8d, [8*rax+0x05]
       mov      r10d, dword ptr [rbp+0x114]
       lea      edx, [8*r10]
       mov      rcx, r13
       vmovaps  xmm3, xmm0
       call     [DotJpegli.Internal.AdaptiveQuantization:ModulateVector(int,int,float):float:this]
       jmp      G_M000_IG122

G_M000_IG115:                ;; offset=0x0D9D
       lea      rcx, bword ptr [r13+0x38]
       mov      rdx, gword ptr [rbp+0x78]
       call     CORINFO_HELP_ASSIGN_REF
       mov      edi, dword ptr [rbp+0x90]
       mov      dword ptr [r13+0x5C], edi
       mov      dword ptr [r13+0x60], r14d
       mov      byte  ptr [r13+0x6B], bl
       mov      r9, 0x6911662D01EE
       cmp      qword ptr [rbp+0x138], r9
       je       SHORT G_M000_IG116
       call     CORINFO_HELP_FAIL_FAST

G_M000_IG116:                ;; offset=0x0DD4
       nop

G_M000_IG117:                ;; offset=0x0DD5
       vmovaps  xmm6, xmmword ptr [rbp+0x1A0]
       vmovaps  xmm7, xmmword ptr [rbp+0x190]
       vmovaps  xmm8, xmmword ptr [rbp+0x180]
       vmovaps  xmm9, xmmword ptr [rbp+0x170]
       vmovaps  xmm10, xmmword ptr [rbp+0x160]
       vmovaps  xmm11, xmmword ptr [rbp+0x150]
       vmovaps  xmm12, xmmword ptr [rbp+0x140]
       lea      rsp, [rbp+0x1B8]
       pop      rbx
       pop      rsi
       pop      rdi
       pop      r12
       pop      r13
       pop      r14
       pop      r15
       pop      rbp
       ret

G_M000_IG118:                ;; offset=0x0E21
       mov      rdx, gword ptr [rbp+0x88]
       cmp      dword ptr [rdx+0x20], 0
       je       G_M000_IG74
       jmp      G_M000_IG142

G_M000_IG119:                ;; offset=0x0E37
       mov      eax, dword ptr [rbp+0x118]
       lea      ecx, [8*rax+0x05]
       imul     ecx, dword ptr [r13+0x54]
       mov      r8d, dword ptr [rbp+0x114]
       lea      ecx, [rcx+8*r8+0x01]
       mov      r9, gword ptr [r13+0x18]
       test     r9, r9
       je       SHORT G_M000_IG120
       cmp      dword ptr [r9+0x08], ecx
       jb       G_M000_IG145
       mov      edx, ecx
       lea      rdx, bword ptr [r9+4*rdx+0x10]
       mov      r9d, dword ptr [r9+0x08]
       sub      r9d, ecx
       jmp      SHORT G_M000_IG121

G_M000_IG120:                ;; offset=0x0E78
       test     ecx, ecx
       jne      G_M000_IG145
       xor      rdx, rdx
       xor      r9d, r9d

G_M000_IG121:                ;; offset=0x0E85
       mov      bword ptr [rbp+0xA8], rdx
       mov      dword ptr [rbp+0xB0], r9d
       mov      rcx, gword ptr [r13+0x10]
       mov      gword ptr [rsp+0x20], rcx
       lea      rcx, [rbp+0xA8]
       movzx    r9, byte  ptr [r13+0x69]
       mov      edx, dword ptr [r13+0x54]
       vmovaps  xmm2, xmm0
       call     [DotJpegli.Internal.AdaptiveQuantization:ModulateScalar(System.ReadOnlySpan`1[float],int,float,bool,DotJpegli.Internal.AqRationalDiagnostics):float]

G_M000_IG122:                ;; offset=0x0EB6
       vmovss   xmm10, dword ptr [reloc @RWD28]
       vmulss   xmm0, xmm10, xmm0
       movzx    rdx, byte  ptr [r13+0x69]
       mov      r8, gword ptr [r13+0x10]
       call     [DotJpegli.Internal.AdaptiveQuantization:FastPow2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float]
       vmulss   xmm0, xmm0, xmm7
       vaddss   xmm1, xmm8, xmm0
       mov      eax, dword ptr [rbp+0x118]
       mov      r8d, eax
       imul     r8d, dword ptr [r13+0x64]
       add      r8d, dword ptr [rbp+0x114]
       cmp      r8d, dword ptr [rbp+0xD8]
       jae      G_M000_IG152
       lea      r10, bword ptr [r12+4*r8]
       mov      bword ptr [rbp+0x70], r10
       movzx    r8, byte  ptr [r13+0x69]
       mov      r9, gword ptr [r13+0x10]
       mov      gword ptr [rbp+0x58], r9
       mov      byte  ptr [rbp+0xE8], 0
       test     r8d, r8d
       je       SHORT G_M000_IG125

G_M000_IG123:                ;; offset=0x0F1C
       vmovss   xmm0, dword ptr [reloc @RWD24]
       lea      r8, [rbp+0xE8]
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovaps  xmm10, xmm0
       mov      rcx, gword ptr [rbp+0x58]
       test     rcx, rcx
       je       SHORT G_M000_IG124
       movzx    r8, byte  ptr [rbp+0xE8]
       mov      edx, 4
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG124:                ;; offset=0x0F51
       jmp      SHORT G_M000_IG126

G_M000_IG125:                ;; offset=0x0F53
       vmovss   xmm0, dword ptr [reloc @RWD24]
       vdivss   xmm10, xmm0, xmm1

G_M000_IG126:                ;; offset=0x0F5F
       vsubss   xmm0, xmm10, xmm6
       vxorps   xmm1, xmm1, xmm1
       vcmpeqps xmm1, xmm1, xmm0
       vxorps   xmm2, xmm2, xmm2
       vpcmpgtd xmm2, xmm2, xmm0
       vandps   xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpltps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandnps  xmm0, xmm1, xmm0
       mov      r10, bword ptr [rbp+0x70]
       vmovss   dword ptr [r10], xmm0
       mov      ecx, dword ptr [rbp+0x114]
       inc      ecx
       cmp      ecx, dword ptr [r13+0x64]
       mov      eax, ecx
       jl       G_M000_IG113

G_M000_IG127:                ;; offset=0x0FA6
       mov      eax, dword ptr [rbp+0x118]
       inc      eax
       mov      r8d, eax

G_M000_IG128:                ;; offset=0x0FB1
       cmp      r8d, dword ptr [r13+0x48]
       jge      G_M000_IG115

G_M000_IG129:                ;; offset=0x0FBB
       cmp      gword ptr [rbp+0x88], 0
       jne      G_M000_IG101

G_M000_IG130:                ;; offset=0x0FC9
       mov      dword ptr [rbp+0x118], r8d
       lea      edx, [2*r8+0x01]
       movzx    r9, bl
       mov      rcx, r13
       xor      r8d, r8d
       call     [DotJpegli.Internal.AdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this]
       mov      eax, dword ptr [rbp+0x118]
       lea      edx, [2*rax+0x02]
       mov      r8d, dword ptr [r13+0x58]
       movzx    r9, bl
       mov      rcx, r13
       call     [DotJpegli.Internal.AdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this]
       xor      eax, eax
       test     esi, esi
       je       G_M000_IG112

G_M000_IG131:                ;; offset=0x1010
       vmovss   xmm10, dword ptr [reloc @RWD28]
       jmp      G_M000_IG111

G_M000_IG132:                ;; offset=0x101D
       vmovss   xmm0, dword ptr [reloc @RWD24]
       vdivss   xmm12, xmm0, xmm1

G_M000_IG133:                ;; offset=0x1029
       vsubss   xmm0, xmm12, xmm6
       vxorps   xmm1, xmm1, xmm1
       vcmpeqps xmm1, xmm1, xmm0
       vxorps   xmm2, xmm2, xmm2
       vpcmpgtd xmm2, xmm2, xmm0
       vandps   xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpltps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandnps  xmm0, xmm1, xmm0
       mov      r9, bword ptr [rbp+0x68]
       vmovss   dword ptr [r9], xmm0
       inc      edi
       cmp      edi, 8
       mov      eax, edi
       jge      G_M000_IG110

G_M000_IG134:                ;; offset=0x1069
       mov      dword ptr [rbp+0x110], eax
       mov      edx, eax
       add      edx, dword ptr [rbp+0x114]
       add      edx, edx
       mov      r9, gword ptr [r13+0x28]
       mov      r11, r9
       mov      r8d, edx
       add      r8d, dword ptr [r13+0x58]
       mov      edi, r8d
       mov      r14d, dword ptr [r11+0x08]
       cmp      edi, r14d
       jae      G_M000_IG152
       vmovss   xmm0, dword ptr [r11+4*rdi+0x10]
       mov      r11, r9
       inc      r8d
       cmp      r8d, r14d
       jae      G_M000_IG152
       vaddss   xmm0, xmm0, dword ptr [r11+4*r8+0x10]
       mov      r8, r9
       cmp      edx, r14d
       jae      G_M000_IG152
       mov      r11d, edx
       vaddss   xmm0, xmm0, dword ptr [r8+4*r11+0x10]
       inc      edx
       cmp      edx, r14d
       jae      G_M000_IG152
       vaddss   xmm11, xmm0, dword ptr [r9+4*rdx+0x10]
       movzx    rdx, byte  ptr [r13+0x69]
       mov      r8, gword ptr [r13+0x10]
       vmovaps  xmm0, xmm11
       call     [DotJpegli.Internal.AdaptiveQuantization:ComputeMask(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float]
       vmovaps  xmm11, xmm0
       mov      edi, dword ptr [rbp+0x110]
       mov      edx, edi
       mov      r14, bword ptr [rbp+0x50]
       vaddss   xmm11, xmm11, dword ptr [r14+4*rdx]
       lea      edx, [rdi+0x08]
       cmp      edx, dword ptr [rbp+0xE0]
       jae      G_M000_IG152
       vmovss   xmm0, dword ptr [r14+4*rdx]
       movzx    rdx, byte  ptr [r13+0x69]
       mov      r8, gword ptr [r13+0x10]
       call     [DotJpegli.Internal.AdaptiveQuantization:FastLog2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float]
       vmulss   xmm0, xmm9, xmm0
       vaddss   xmm11, xmm11, xmm0
       vmulss   xmm0, xmm11, xmm10
       movzx    rdx, byte  ptr [r13+0x69]
       mov      r8, gword ptr [r13+0x10]
       call     [DotJpegli.Internal.AdaptiveQuantization:FastPow2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float]
       vmulss   xmm0, xmm0, xmm7
       vaddss   xmm1, xmm8, xmm0

G_M000_IG135:                ;; offset=0x114E
       mov      eax, dword ptr [rbp+0x118]
       mov      ecx, eax
       imul     ecx, dword ptr [r13+0x64]
       add      ecx, dword ptr [rbp+0x114]
       add      ecx, edi
       cmp      ecx, dword ptr [rbp+0xD8]
       jae      G_M000_IG152
       mov      r8d, ecx
       lea      r9, bword ptr [r12+4*r8]
       mov      bword ptr [rbp+0x68], r9
       movzx    r11, byte  ptr [r13+0x69]
       mov      r8, gword ptr [r13+0x10]
       mov      gword ptr [rbp+0x60], r8
       mov      byte  ptr [rbp+0xF0], 0
       test     r11d, r11d
       je       G_M000_IG132

G_M000_IG136:                ;; offset=0x1197
       vmovss   xmm0, dword ptr [reloc @RWD24]
       lea      r8, [rbp+0xF0]
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovaps  xmm12, xmm0
       mov      rax, gword ptr [rbp+0x60]
       test     rax, rax
       je       SHORT G_M000_IG137
       movzx    r8, byte  ptr [rbp+0xF0]
       mov      rcx, rax
       mov      edx, 4
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG137:                ;; offset=0x11CF
       jmp      G_M000_IG133

G_M000_IG138:                ;; offset=0x11D4
       mov      rax, gword ptr [rbp+0x88]
       cmp      dword ptr [rax+0x20], 0
       je       G_M000_IG11
       jmp      SHORT G_M000_IG142

G_M000_IG139:                ;; offset=0x11E7
       mov      r8d, -1
       jmp      G_M000_IG18

G_M000_IG140:                ;; offset=0x11F2
       mov      r11d, r8d
       mov      qword ptr [rbp+0x08], r11
       mov      r11d, ecx
       add      r11, qword ptr [rbp+0x08]
       mov      qword ptr [rbp+0x08], r11
       mov      r11d, dword ptr [rdx+0x08]
       cmp      qword ptr [rbp+0x08], r11
       ja       G_M000_IG145
       mov      r8d, r8d
       lea      rdx, bword ptr [rdx+4*r8+0x10]
       mov      bword ptr [rbp+0x48], rdx
       mov      r11, gword ptr [rbp+0x78]
       jmp      G_M000_IG13

G_M000_IG141:                ;; offset=0x1227
       mov      rcx, r13
       call     [System.ThrowHelper:ThrowObjectDisposedException(System.Object)]
       int3

G_M000_IG142:                ;; offset=0x1231
       lea      rcx, [rbp+0x88]
       call     [System.Threading.CancellationToken:ThrowOperationCanceledException():this]
       int3

G_M000_IG143:                ;; offset=0x123F
       mov      rcx, 0x7FF882C3DDB8
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      rcx, rbx
       mov      rdx, 0x231A58E62C8
       call     [System.ArgumentOutOfRangeException:.ctor(System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3

G_M000_IG144:                ;; offset=0x126D
       mov      rcx, 0x7FF882B8F4B8
       call     CORINFO_HELP_NEWSFAST
       mov      r13, rax
       mov      rcx, r13
       mov      rdx, 0x231A58E6208
       mov      r8, 0x231A58E62A0
       call     [System.ArgumentException:.ctor(System.String,System.String):this]
       mov      rcx, r13
       call     CORINFO_HELP_THROW
       int3

G_M000_IG145:                ;; offset=0x12A5
       call     [System.ThrowHelper:ThrowArgumentOutOfRangeException()]
       int3

G_M000_IG146:                ;; offset=0x12AC
       call     [System.ThrowHelper:ThrowArgumentException_DestinationTooShort()]
       int3

G_M000_IG147:                ;; offset=0x12B3
       xor      ecx, ecx
       call     [System.Math:ThrowMinMaxException[int](int,int)]
       int3

G_M000_IG148:                ;; offset=0x12BC
       mov      edx, esi
       xor      ecx, ecx
       call     [System.Math:ThrowMinMaxException[int](int,int)]
       int3

G_M000_IG149:                ;; offset=0x12C7
       mov      edx, r11d
       xor      ecx, ecx
       call     [System.Math:ThrowMinMaxException[int](int,int)]
       int3

G_M000_IG150:                ;; offset=0x12D3
       xor      ecx, ecx
       call     [System.Math:ThrowMinMaxException[int](int,int)]
       int3

G_M000_IG151:                ;; offset=0x12DC
       mov      edx, r9d
       xor      ecx, ecx
       call     [System.Math:ThrowMinMaxException[int](int,int)]
       int3

G_M000_IG152:                ;; offset=0x12E8
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	3F800000h		;         1
RWD04  	dd	41100000h		;         9
RWD08  	dd	42600000h		;        56
RWD12  	dd	3F574BC7h		;     0.841
RWD16  	dd	3ECEAF25h		;   0.40368
RWD20  	dd	BDDC6A08h		; -0.107624
RWD24  	dd	3F19999Ah		;       0.6
RWD28  	dd	3FB8AA3Bh		;    1.4427

; Total bytes of code 4846

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ComputePreErosion(int,int,bool):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 56 single block inlinees; 42 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r15
       push     r14
       push     r13
       push     r12
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 0x528
       vmovaps  xmmword ptr [rsp+0x510], xmm6
       vmovaps  xmmword ptr [rsp+0x500], xmm7
       vmovaps  xmmword ptr [rsp+0x4F0], xmm8
       vmovaps  xmmword ptr [rsp+0x4E0], xmm9
       vmovaps  xmmword ptr [rsp+0x4D0], xmm10
       vmovaps  xmmword ptr [rsp+0x4C0], xmm11
       vmovaps  xmmword ptr [rsp+0x4B0], xmm12
       vmovaps  xmmword ptr [rsp+0x4A0], xmm13
       vmovaps  xmmword ptr [rsp+0x490], xmm14
       vmovaps  xmmword ptr [rsp+0x480], xmm15
       xor      eax, eax
       mov      qword ptr [rsp+0x168], rax
       vxorps   xmm4, xmm4, xmm4
       vmovdqa  xmmword ptr [rsp+0x170], xmm4
       vmovdqa  xmmword ptr [rsp+0x180], xmm4
       mov      rax, -576

G_M000_IG71:                ;; offset=0x0097
       vmovdqa  xmmword ptr [rsp+rax+0x3D0], xmm4
       vmovdqa  xmmword ptr [rsp+rax+0x3E0], xmm4
       vmovdqa  xmmword ptr [rsp+rax+0x3F0], xmm4
       add      rax, 48
       jne      SHORT G_M000_IG71
       mov      rbx, rcx

G_M000_IG02:                ;; offset=0x00BB
       vmovss   xmm6, dword ptr [reloc @RWD00]
       imul     edx, dword ptr [rbx+0x54]
       lea      esi, [rdx+0x01]
       mov      edi, r8d
       imul     edi, dword ptr [rbx+0x58]
       xor      ebp, ebp
       test     r9b, r9b
       je       G_M000_IG67

G_M000_IG03:                ;; offset=0x00DC
       mov      ecx, dword ptr [rbx+0x4C]
       add      ecx, -8
       js       G_M000_IG67
       jmp      G_M000_IG19

G_M000_IG04:                ;; offset=0x00ED
       vmovups  ymm0, ymmword ptr [rsp+0x390]
       vdivps   ymm0, ymm0, ymmword ptr [rsp+0x3B0]
       vmovups  ymmword ptr [rsp+0x370], ymm0

G_M000_IG05:                ;; offset=0x0108
       vsubps   ymm0, ymm8, ymm7
       vmulps   ymm0, ymm0, ymmword ptr [rsp+0x370]
       vmulps   ymm0, ymm0, ymm0
       vbroadcastss ymm7, dword ptr [reloc @RWD04]
       vcmpeqps ymm1, ymm7, ymm0
       vxorps   ymm2, ymm2, ymm2
       vpcmpgtd ymm2, ymm2, ymm0
       vandps   ymm1, ymm2, ymm1
       vcmpneqps ymm2, ymm0, ymm0
       vorps    ymm1, ymm2, ymm1
       vcmpltps ymm2, ymm0, ymm7
       vorps    ymm1, ymm2, ymm1
       vblendvps ymm0, ymm7, ymm0, ymm1
       vbroadcastss ymm1, dword ptr [reloc @RWD00]
       vmovups  ymmword ptr [rsp+0x30], ymm1
       vmulps   ymm0, ymm1, ymm0
       vbroadcastss ymm2, dword ptr [reloc @RWD08]
       vmovups  ymmword ptr [rsp+0x50], ymm2
       vaddps   ymm0, ymm0, ymm2
       vsqrtps  ymm0, ymm0
       vmulps   ymm0, ymm10, ymm0
       vmovups  ymmword ptr [rsp+0x410], ymm0
       mov      rcx, gword ptr [rbx+0x18]
       mov      rdx, rcx
       mov      r8d, dword ptr [rbx+0x54]
       lea      r9d, [r14+2*r8]
       test     r9d, r9d
       jl       G_M000_IG69
       mov      eax, dword ptr [rdx+0x08]
       mov      r10d, eax
       sub      r10d, r9d
       cmp      r10d, 8
       jl       G_M000_IG69
       cmp      r9d, eax
       jae      G_M000_IG70
       vmovups  ymm3, ymmword ptr [rdx+4*r9+0x10]
       mov      rdx, rcx
       add      r8d, r14d
       lea      r9d, [r8-0x01]
       test     r9d, r9d
       jl       G_M000_IG69
       mov      r10d, eax
       sub      r10d, r9d
       cmp      r10d, 8
       jl       G_M000_IG69
       cmp      r9d, eax
       jae      G_M000_IG70
       vmovups  ymm4, ymmword ptr [rdx+4*r9+0x10]
       inc      r8d
       js       G_M000_IG69
       mov      edx, eax
       sub      edx, r8d
       cmp      edx, 8
       jl       G_M000_IG69

G_M000_IG06:                ;; offset=0x0207
       cmp      r8d, eax
       jae      G_M000_IG70
       mov      edx, r8d
       vaddps   ymm4, ymm4, ymmword ptr [rcx+4*rdx+0x10]
       vmovups  ymmword ptr [rsp+0x430], ymm3
       vaddps   ymm5, ymm8, ymm3
       vaddps   ymm4, ymm5, ymm4
       vmulps   ymm8, ymm10, ymm4
       vmovups  ymmword ptr [rsp+0x350], ymm8
       vaddps   ymm4, ymm11, ymm9
       movzx    rcx, byte  ptr [rbx+0x69]
       mov      rdx, gword ptr [rbx+0x10]
       vcmpneqps ymm5, ymm4, ymm4
       vxorps   ymm8, ymm8, ymm8
       vcmpgtps ymm8, ymm4, ymm8
       vorps    ymm5, ymm8, ymm5
       vandps   ymm4, ymm5, ymm4
       vmulps   ymm5, ymm4, ymm4
       vmulps   ymm8, ymm12, ymm5
       vaddps   ymm8, ymm8, ymm13
       vmovups  ymmword ptr [rsp+0x330], ymm8
       vmulps   ymm4, ymm14, ymm4
       vmulps   ymm4, ymm4, ymm5
       vaddps   ymm4, ymm15, ymm4
       vmovups  ymmword ptr [rsp+0x310], ymm4
       test     ecx, ecx
       je       G_M000_IG08

G_M000_IG07:                ;; offset=0x028F
       mov      gword ptr [rsp+0x20], rdx
       lea      rcx, [rsp+0x2F0]
       lea      rdx, [rsp+0x310]
       lea      r8, [rsp+0x330]
       mov      r9d, 1
       vextractf128 xmm8, ymm9
       vextractf128 xmmword ptr [rsp+0x140], ymm7
       vextractf128 xmmword ptr [rsp+0x120], ymm10
       vextractf128 xmmword ptr [rsp+0x100], ymm11
       vextractf128 xmmword ptr [rsp+0xE0], ymm12
       vextractf128 xmmword ptr [rsp+0xC0], ymm13
       vextractf128 xmmword ptr [rsp+0xA0], ymm14
       vextractf128 xmmword ptr [rsp+0x80], ymm15
       call     [DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Numerics.Vector`1[float],System.Numerics.Vector`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Numerics.Vector`1[float]]
       vinsertf128 ymm9, ymm9, xmm8
       vinsertf128 ymm7, ymm7, xmmword ptr [rsp+0x140]
       vinsertf128 ymm10, ymm10, xmmword ptr [rsp+0x120]
       vinsertf128 ymm11, ymm11, xmmword ptr [rsp+0x100]
       vinsertf128 ymm12, ymm12, xmmword ptr [rsp+0xE0]
       vinsertf128 ymm13, ymm13, xmmword ptr [rsp+0xC0]
       vinsertf128 ymm14, ymm14, xmmword ptr [rsp+0xA0]
       vinsertf128 ymm15, ymm15, xmmword ptr [rsp+0x80]
       jmp      SHORT G_M000_IG09

G_M000_IG08:                ;; offset=0x0360
       vmovups  ymm4, ymmword ptr [rsp+0x310]
       vdivps   ymm4, ymm4, ymmword ptr [rsp+0x330]
       vmovups  ymmword ptr [rsp+0x2F0], ymm4

G_M000_IG09:                ;; offset=0x037B
       vsubps   ymm4, ymm9, ymmword ptr [rsp+0x350]
       vmulps   ymm4, ymm4, ymmword ptr [rsp+0x2F0]
       vmulps   ymm4, ymm4, ymm4
       vcmpeqps ymm5, ymm7, ymm4
       vxorps   ymm8, ymm8, ymm8
       vpcmpgtd ymm8, ymm8, ymm4
       vandps   ymm5, ymm8, ymm5
       vcmpneqps ymm8, ymm4, ymm4
       vorps    ymm5, ymm8, ymm5
       vcmpltps ymm8, ymm4, ymm7
       vorps    ymm5, ymm8, ymm5
       vblendvps ymm4, ymm7, ymm4, ymm5
       vmovups  ymm1, ymmword ptr [rsp+0x30]
       vmulps   ymm4, ymm1, ymm4
       vaddps   ymm4, ymm4, ymmword ptr [rsp+0x50]
       vsqrtps  ymm4, ymm4
       vmulps   ymm4, ymm10, ymm4
       vaddps   ymm0, ymm4, ymmword ptr [rsp+0x410]
       vmovups  ymmword ptr [rsp+0x410], ymm0
       vmovaps  ymm8, ymm9
       vmovups  ymm3, ymmword ptr [rsp+0x430]
       vmovaps  ymm9, ymm3
       mov      rcx, gword ptr [rbx+0x18]
       mov      rdx, rcx
       mov      r8d, dword ptr [rbx+0x54]
       lea      r9d, [r8+2*r8]
       add      r9d, r14d
       js       G_M000_IG69
       mov      eax, dword ptr [rdx+0x08]
       mov      r10d, eax
       sub      r10d, r9d
       cmp      r10d, 8
       jl       G_M000_IG69
       cmp      r9d, eax
       jae      G_M000_IG70
       vmovups  ymm3, ymmword ptr [rdx+4*r9+0x10]
       mov      rdx, rcx
       lea      r8d, [r14+2*r8]
       lea      r9d, [r8-0x01]
       test     r9d, r9d
       jl       G_M000_IG69
       mov      r10d, eax
       sub      r10d, r9d
       cmp      r10d, 8
       jl       G_M000_IG69
       cmp      r9d, eax
       jae      G_M000_IG70
       vmovups  ymm4, ymmword ptr [rdx+4*r9+0x10]
       inc      r8d
       js       G_M000_IG69
       mov      edx, eax
       sub      edx, r8d
       cmp      edx, 8
       jl       G_M000_IG69
       cmp      r8d, eax

G_M000_IG10:                ;; offset=0x0480
       jae      G_M000_IG70
       mov      edx, r8d
       vaddps   ymm4, ymm4, ymmword ptr [rcx+4*rdx+0x10]
       vmovups  ymmword ptr [rsp+0x430], ymm3
       vaddps   ymm5, ymm8, ymm3
       vaddps   ymm4, ymm5, ymm4
       vmulps   ymm8, ymm10, ymm4
       vmovups  ymmword ptr [rsp+0x2D0], ymm8
       vaddps   ymm4, ymm11, ymm9
       movzx    rcx, byte  ptr [rbx+0x69]
       mov      rdx, gword ptr [rbx+0x10]
       vcmpneqps ymm5, ymm4, ymm4
       vxorps   ymm8, ymm8, ymm8
       vcmpgtps ymm8, ymm4, ymm8
       vorps    ymm5, ymm8, ymm5
       vandps   ymm4, ymm5, ymm4
       vmulps   ymm5, ymm4, ymm4
       vmulps   ymm8, ymm12, ymm5
       vaddps   ymm8, ymm8, ymm13
       vmovups  ymmword ptr [rsp+0x2B0], ymm8
       vmulps   ymm4, ymm14, ymm4
       vmulps   ymm4, ymm4, ymm5
       vaddps   ymm4, ymm15, ymm4
       vmovups  ymmword ptr [rsp+0x290], ymm4
       test     ecx, ecx
       je       G_M000_IG12

G_M000_IG11:                ;; offset=0x0505
       mov      gword ptr [rsp+0x20], rdx
       lea      rcx, [rsp+0x270]
       lea      rdx, [rsp+0x290]
       lea      r8, [rsp+0x2B0]
       mov      r9d, 1
       vextractf128 xmm8, ymm9
       vextractf128 xmmword ptr [rsp+0x140], ymm7
       vextractf128 xmmword ptr [rsp+0x120], ymm10
       vextractf128 xmmword ptr [rsp+0x100], ymm11
       vextractf128 xmmword ptr [rsp+0xE0], ymm12
       vextractf128 xmmword ptr [rsp+0xC0], ymm13
       vextractf128 xmmword ptr [rsp+0xA0], ymm14
       vextractf128 xmmword ptr [rsp+0x80], ymm15
       call     [DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Numerics.Vector`1[float],System.Numerics.Vector`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Numerics.Vector`1[float]]
       vinsertf128 ymm9, ymm9, xmm8
       vinsertf128 ymm7, ymm7, xmmword ptr [rsp+0x140]
       vinsertf128 ymm10, ymm10, xmmword ptr [rsp+0x120]
       vinsertf128 ymm11, ymm11, xmmword ptr [rsp+0x100]
       vinsertf128 ymm12, ymm12, xmmword ptr [rsp+0xE0]
       vinsertf128 ymm13, ymm13, xmmword ptr [rsp+0xC0]
       vinsertf128 ymm14, ymm14, xmmword ptr [rsp+0xA0]
       vinsertf128 ymm15, ymm15, xmmword ptr [rsp+0x80]
       jmp      SHORT G_M000_IG13

G_M000_IG12:                ;; offset=0x05D6
       vmovups  ymm4, ymmword ptr [rsp+0x290]
       vdivps   ymm4, ymm4, ymmword ptr [rsp+0x2B0]
       vmovups  ymmword ptr [rsp+0x270], ymm4

G_M000_IG13:                ;; offset=0x05F1
       vsubps   ymm4, ymm9, ymmword ptr [rsp+0x2D0]
       vmulps   ymm4, ymm4, ymmword ptr [rsp+0x270]
       vmulps   ymm4, ymm4, ymm4
       vcmpeqps ymm5, ymm7, ymm4
       vxorps   ymm8, ymm8, ymm8
       vpcmpgtd ymm8, ymm8, ymm4
       vandps   ymm5, ymm8, ymm5
       vcmpneqps ymm8, ymm4, ymm4
       vorps    ymm5, ymm8, ymm5
       vcmpltps ymm8, ymm4, ymm7
       vorps    ymm5, ymm8, ymm5
       vblendvps ymm4, ymm7, ymm4, ymm5
       vmovups  ymm1, ymmword ptr [rsp+0x30]
       vmulps   ymm4, ymm1, ymm4
       vaddps   ymm4, ymm4, ymmword ptr [rsp+0x50]
       vsqrtps  ymm4, ymm4
       vmulps   ymm4, ymm10, ymm4
       vaddps   ymm0, ymm4, ymmword ptr [rsp+0x410]
       vmovups  ymmword ptr [rsp+0x410], ymm0
       vmovaps  ymm8, ymm9
       vmovups  ymm3, ymmword ptr [rsp+0x430]
       vmovaps  ymm9, ymm3
       mov      rcx, gword ptr [rbx+0x18]
       mov      rdx, rcx
       mov      r8d, dword ptr [rbx+0x54]
       lea      r9d, [r14+4*r8]
       test     r9d, r9d
       jl       G_M000_IG69
       mov      eax, dword ptr [rdx+0x08]
       mov      r10d, eax
       sub      r10d, r9d
       cmp      r10d, 8
       jl       G_M000_IG69
       cmp      r9d, eax
       jae      G_M000_IG70
       vmovups  ymm3, ymmword ptr [rdx+4*r9+0x10]
       mov      rdx, rcx
       lea      r8d, [r8+2*r8]
       add      r8d, r14d
       lea      r9d, [r8-0x01]
       test     r9d, r9d
       jl       G_M000_IG69
       mov      r10d, eax
       sub      r10d, r9d
       cmp      r10d, 8
       jl       G_M000_IG69
       cmp      r9d, eax
       jae      G_M000_IG70
       vmovups  ymm4, ymmword ptr [rdx+4*r9+0x10]
       inc      r8d
       js       G_M000_IG69
       mov      edx, eax
       sub      edx, r8d
       cmp      edx, 8
       jl       G_M000_IG69

G_M000_IG14:                ;; offset=0x06F6
       cmp      r8d, eax
       jae      G_M000_IG70
       mov      edx, r8d
       vaddps   ymm4, ymm4, ymmword ptr [rcx+4*rdx+0x10]
       vaddps   ymm3, ymm8, ymm3
       vaddps   ymm3, ymm3, ymm4
       vmulps   ymm8, ymm10, ymm3
       vaddps   ymm3, ymm11, ymm9
       movzx    rcx, byte  ptr [rbx+0x69]
       mov      rdx, gword ptr [rbx+0x10]
       vcmpneqps ymm4, ymm3, ymm3
       vxorps   ymm5, ymm5, ymm5
       vcmpgtps ymm5, ymm3, ymm5
       vorps    ymm4, ymm5, ymm4
       vandps   ymm3, ymm4, ymm3
       vmulps   ymm4, ymm3, ymm3
       vmulps   ymm5, ymm12, ymm4
       vaddps   ymm5, ymm13, ymm5
       vmovups  ymmword ptr [rsp+0x250], ymm5
       vmulps   ymm3, ymm14, ymm3
       vmulps   ymm3, ymm3, ymm4
       vaddps   ymm3, ymm15, ymm3
       vmovups  ymmword ptr [rsp+0x230], ymm3
       test     ecx, ecx
       je       SHORT G_M000_IG16

G_M000_IG15:                ;; offset=0x0765
       mov      gword ptr [rsp+0x20], rdx
       lea      rcx, [rsp+0x210]
       lea      rdx, [rsp+0x230]
       lea      r8, [rsp+0x250]
       mov      r9d, 1
       vextractf128 xmm11, ymm9
       vextractf128 xmm12, ymm7
       vextractf128 xmm13, ymm10
       vextractf128 xmm14, ymm8
       call     [DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Numerics.Vector`1[float],System.Numerics.Vector`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Numerics.Vector`1[float]]
       vinsertf128 ymm9, ymm9, xmm11
       vinsertf128 ymm7, ymm7, xmm12
       vinsertf128 ymm10, ymm10, xmm13
       vinsertf128 ymm8, ymm8, xmm14
       jmp      SHORT G_M000_IG17

G_M000_IG16:                ;; offset=0x07C0
       vmovups  ymm3, ymmword ptr [rsp+0x230]
       vdivps   ymm3, ymm3, ymmword ptr [rsp+0x250]
       vmovups  ymmword ptr [rsp+0x210], ymm3

G_M000_IG17:                ;; offset=0x07DB
       vsubps   ymm3, ymm9, ymm8
       vmulps   ymm3, ymm3, ymmword ptr [rsp+0x210]
       vmulps   ymm3, ymm3, ymm3
       vcmpeqps ymm4, ymm7, ymm3
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm3
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm3, ymm3
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm3, ymm7
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm3, ymm7, ymm3, ymm4
       vmovups  ymm1, ymmword ptr [rsp+0x30]
       vmulps   ymm1, ymm1, ymm3
       vaddps   ymm1, ymm1, ymmword ptr [rsp+0x50]
       vsqrtps  ymm1, ymm1
       vmulps   ymm1, ymm10, ymm1
       vaddps   ymm0, ymm1, ymmword ptr [rsp+0x410]
       mov      r8, gword ptr [rbx+0x20]
       mov      rax, r8
       vmovaps  ymm1, ymm0
       vmovaps  xmm2, xmm0
       vmovshdup xmm2, xmm2
       vaddss   xmm2, xmm2, xmm1
       vmovaps  xmm1, xmm0
       vunpckhps xmm1, xmm1, xmm1
       vaddss   xmm1, xmm1, xmm2
       vmovaps  xmm2, xmm0
       vshufps  xmm2, xmm2, xmm2, -1
       vaddss   xmm1, xmm2, xmm1
       vmovss   xmm7, dword ptr [reloc @RWD12]
       vmulss   xmm1, xmm1, xmm7
       mov      ecx, ebp
       sar      ecx, 31
       and      ecx, 3
       add      ecx, ebp
       sar      ecx, 2
       add      ecx, edi
       cmp      ecx, dword ptr [rax+0x08]
       jae      G_M000_IG70
       vmovss   dword ptr [rax+4*rcx+0x10], xmm1
       mov      rax, r8
       vextractf128 xmm1, ymm0
       vextractf128 xmm2, ymm0
       vmovshdup xmm2, xmm2
       vaddss   xmm2, xmm2, xmm1
       vextractf128 xmm1, ymm0
       vunpckhps xmm1, xmm1, xmm1
       vaddss   xmm1, xmm1, xmm2
       vextractf128 xmm0, ymm0
       vshufps  xmm0, xmm0, xmm0, -1
       vaddss   xmm0, xmm0, xmm1
       vmulss   xmm1, xmm0, xmm7
       lea      ecx, [rbp+0x04]
       mov      r8d, ecx
       sar      r8d, 31
       and      r8d, 3
       add      ecx, r8d
       sar      ecx, 2
       add      ecx, edi
       cmp      ecx, dword ptr [rax+0x08]
       jae      G_M000_IG70
       mov      r8d, ecx

G_M000_IG18:                ;; offset=0x08EB
       vmovss   dword ptr [rax+4*r8+0x10], xmm1
       add      ebp, 8
       mov      r8d, dword ptr [rbx+0x4C]
       add      r8d, -8
       cmp      ebp, r8d
       jg       G_M000_IG67

G_M000_IG19:                ;; offset=0x0906
       lea      r14d, [rsi+rbp]
       mov      rcx, gword ptr [rbx+0x18]
       mov      rdx, rcx
       mov      r8d, dword ptr [rbx+0x54]
       mov      r9d, r14d
       sub      r9d, r8d
       js       G_M000_IG69
       mov      eax, dword ptr [rdx+0x08]
       mov      r10d, eax
       sub      r10d, r9d
       cmp      r10d, 8
       jl       G_M000_IG69
       cmp      r9d, eax
       jae      G_M000_IG70
       vmovups  ymm7, ymmword ptr [rdx+4*r9+0x10]
       mov      rdx, rcx
       test     r14d, r14d
       jl       G_M000_IG69
       mov      r9d, eax
       sub      r9d, r14d
       cmp      r9d, 8
       jl       G_M000_IG69
       cmp      r14d, eax
       jae      G_M000_IG70
       mov      r9d, r14d
       vmovups  ymm8, ymmword ptr [rdx+4*r9+0x10]
       mov      rdx, rcx
       add      r8d, r14d
       js       G_M000_IG69
       mov      r9d, eax
       sub      r9d, r8d
       cmp      r9d, 8
       jl       G_M000_IG69
       cmp      r8d, eax
       jae      G_M000_IG70
       vmovups  ymm9, ymmword ptr [rdx+4*r8+0x10]
       mov      rdx, rcx
       lea      r8d, [r14-0x01]
       test     r8d, r8d
       jl       G_M000_IG69
       mov      r9d, eax
       sub      r9d, r8d
       cmp      r9d, 8
       jl       G_M000_IG69
       cmp      r8d, eax
       jae      G_M000_IG70
       vmovups  ymm0, ymmword ptr [rdx+4*r8+0x10]
       lea      edx, [r14+0x01]
       mov      r8d, eax
       sub      r8d, edx
       cmp      r8d, 8

G_M000_IG20:                ;; offset=0x09DD
       jl       G_M000_IG69
       cmp      edx, eax
       jae      G_M000_IG70
       vaddps   ymm0, ymm0, ymmword ptr [rcx+4*rdx+0x10]
       vaddps   ymm1, ymm9, ymm7
       vaddps   ymm0, ymm1, ymm0
       vbroadcastss ymm10, dword ptr [reloc @RWD12]
       vmulps   ymm7, ymm10, ymm0
       vbroadcastss ymm11, dword ptr [reloc @RWD16]
       vaddps   ymm0, ymm11, ymm8
       movzx    rcx, byte  ptr [rbx+0x69]
       mov      rdx, gword ptr [rbx+0x10]
       vcmpneqps ymm1, ymm0, ymm0
       vxorps   ymm2, ymm2, ymm2
       vcmpgtps ymm2, ymm0, ymm2
       vorps    ymm1, ymm2, ymm1
       vandps   ymm0, ymm1, ymm0
       vmulps   ymm1, ymm0, ymm0
       vbroadcastss ymm12, dword ptr [reloc @RWD20]
       vmulps   ymm2, ymm12, ymm1
       vbroadcastss ymm13, dword ptr [reloc @RWD24]
       vaddps   ymm2, ymm13, ymm2
       vmovups  ymmword ptr [rsp+0x3B0], ymm2
       vbroadcastss ymm14, dword ptr [reloc @RWD28]
       vmulps   ymm0, ymm14, ymm0
       vmulps   ymm0, ymm0, ymm1
       vbroadcastss ymm15, dword ptr [reloc @RWD32]
       vaddps   ymm0, ymm15, ymm0
       vmovups  ymmword ptr [rsp+0x390], ymm0
       test     ecx, ecx
       je       G_M000_IG04

G_M000_IG21:                ;; offset=0x0A88
       mov      gword ptr [rsp+0x20], rdx
       lea      rcx, [rsp+0x370]
       lea      rdx, [rsp+0x390]
       lea      r8, [rsp+0x3B0]
       mov      r9d, 1
       vextractf128 xmmword ptr [rsp+0x460], ymm8
       vextractf128 xmmword ptr [rsp+0x440], ymm9
       vextractf128 xmmword ptr [rsp+0x120], ymm10
       vextractf128 xmmword ptr [rsp+0x100], ymm11
       vextractf128 xmmword ptr [rsp+0xE0], ymm12
       vextractf128 xmmword ptr [rsp+0xC0], ymm13
       vextractf128 xmmword ptr [rsp+0xA0], ymm14
       vextractf128 xmmword ptr [rsp+0x80], ymm15
       vextractf128 xmmword ptr [rsp+0x3E0], ymm7
       call     [DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Numerics.Vector`1[float],System.Numerics.Vector`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Numerics.Vector`1[float]]
       vinsertf128 ymm8, ymm8, xmmword ptr [rsp+0x460]
       vinsertf128 ymm9, ymm9, xmmword ptr [rsp+0x440]
       vinsertf128 ymm10, ymm10, xmmword ptr [rsp+0x120]
       vinsertf128 ymm11, ymm11, xmmword ptr [rsp+0x100]
       vinsertf128 ymm12, ymm12, xmmword ptr [rsp+0xE0]
       vinsertf128 ymm13, ymm13, xmmword ptr [rsp+0xC0]
       vinsertf128 ymm14, ymm14, xmmword ptr [rsp+0xA0]
       vinsertf128 ymm15, ymm15, xmmword ptr [rsp+0x80]
       vinsertf128 ymm7, ymm7, xmmword ptr [rsp+0x3E0]
       jmp      G_M000_IG05

G_M000_IG22:                ;; offset=0x0B7C
       lea      r14d, [rsi+rbp]
       mov      r8, gword ptr [rbx+0x18]
       mov      rax, r8
       mov      ecx, dword ptr [rax+0x08]
       cmp      r14d, ecx
       jae      G_M000_IG70
       mov      edx, r14d
       vmovss   xmm8, dword ptr [rax+4*rdx+0x10]
       mov      rax, r8
       lea      edx, [r14-0x01]
       cmp      edx, ecx
       jae      G_M000_IG70
       lea      edx, [r14-0x01]
       vmovss   xmm0, dword ptr [rax+4*rdx+0x10]
       mov      rax, r8
       lea      edx, [r14+0x01]
       cmp      edx, ecx
       jae      G_M000_IG70
       lea      edx, [r14+0x01]
       vaddss   xmm0, xmm0, dword ptr [rax+4*rdx+0x10]
       mov      rax, r8
       mov      edx, dword ptr [rbx+0x54]
       mov      r10d, r14d
       sub      r10d, edx
       cmp      r10d, ecx
       jae      G_M000_IG70
       vmovss   xmm1, dword ptr [rax+4*r10+0x10]
       add      edx, r14d
       cmp      edx, ecx
       jae      G_M000_IG70
       mov      eax, edx
       vaddss   xmm1, xmm1, dword ptr [r8+4*rax+0x10]
       vaddss   xmm0, xmm0, xmm1
       vmovss   xmm7, dword ptr [reloc @RWD12]
       vmulss   xmm9, xmm0, xmm7
       vmovss   xmm10, dword ptr [reloc @RWD16]
       vaddss   xmm0, xmm8, xmm10
       movzx    r8, byte  ptr [rbx+0x69]
       mov      r15, gword ptr [rbx+0x10]
       vxorps   xmm1, xmm1, xmm1
       vcmpneqps xmm2, xmm0, xmm0
       vorps    xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpgtps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandps   xmm0, xmm1, xmm0
       vmulss   xmm1, xmm0, xmm0
       vmovss   xmm11, dword ptr [reloc @RWD20]
       vmulss   xmm2, xmm11, xmm1
       vmovss   xmm12, dword ptr [reloc @RWD24]
       vaddss   xmm2, xmm12, xmm2
       vmovss   xmm13, dword ptr [reloc @RWD28]
       vmulss   xmm0, xmm13, xmm0
       vmulss   xmm0, xmm0, xmm1
       vmovss   xmm14, dword ptr [reloc @RWD32]
       vaddss   xmm0, xmm14, xmm0
       mov      byte  ptr [rsp+0x208], 0

G_M000_IG23:                ;; offset=0x0C82
       test     r8d, r8d
       je       SHORT G_M000_IG26

G_M000_IG24:                ;; offset=0x0C87
       lea      r8, [rsp+0x208]
       vmovaps  xmm1, xmm2
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovaps  xmm15, xmm0
       test     r15, r15
       je       SHORT G_M000_IG25
       movzx    r8, byte  ptr [rsp+0x208]
       mov      rcx, r15
       mov      edx, 1
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG25:                ;; offset=0x0CB9
       jmp      SHORT G_M000_IG27

G_M000_IG26:                ;; offset=0x0CBB
       vdivss   xmm15, xmm0, xmm2

G_M000_IG27:                ;; offset=0x0CBF
       vsubss   xmm0, xmm8, xmm9
       vmulss   xmm0, xmm15, xmm0
       vmulss   xmm0, xmm0, xmm0
       vbroadcastss xmm8, dword ptr [reloc @RWD04]
       vminss   xmm0, xmm8, xmm0
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD00]
       vmovss   xmm9, dword ptr [reloc @RWD08]
       vaddss   xmm0, xmm9, xmm0
       vsqrtss  xmm0, xmm0, xmm0
       vmulss   xmm15, xmm0, xmm7
       mov      r15d, 1

G_M000_IG28:                ;; offset=0x0CFB
       mov      r8d, dword ptr [rbx+0x54]
       mov      eax, r15d
       imul     eax, r8d
       add      eax, r14d
       mov      rcx, gword ptr [rbx+0x18]
       mov      rdx, rcx
       mov      r10d, dword ptr [rdx+0x08]
       cmp      eax, r10d
       jae      G_M000_IG70
       mov      r9d, eax
       vmovss   xmm0, dword ptr [rdx+4*r9+0x10]
       vmovaps  xmm2, xmm0
       vmovss   dword ptr [rsp+0x200], xmm2
       mov      rdx, rcx
       lea      r9d, [rax-0x01]
       cmp      r9d, r10d
       jae      G_M000_IG70
       lea      r9d, [rax-0x01]
       vmovss   xmm0, dword ptr [rdx+4*r9+0x10]
       mov      rdx, rcx
       lea      r9d, [rax+0x01]
       cmp      r9d, r10d
       jae      G_M000_IG70
       lea      r9d, [rax+0x01]
       vaddss   xmm0, xmm0, dword ptr [rdx+4*r9+0x10]
       mov      rdx, rcx
       mov      r9d, eax
       sub      r9d, r8d
       cmp      r9d, r10d
       jae      G_M000_IG70
       vmovss   xmm1, dword ptr [rdx+4*r9+0x10]
       add      r8d, eax
       cmp      r8d, r10d
       jae      G_M000_IG70
       vaddss   xmm1, xmm1, dword ptr [rcx+4*r8+0x10]
       vaddss   xmm0, xmm0, xmm1
       vmulss   xmm0, xmm0, xmm7
       vmovss   dword ptr [rsp+0x204], xmm0
       vaddss   xmm0, xmm10, xmm2
       movzx    r8, byte  ptr [rbx+0x69]
       mov      r13, gword ptr [rbx+0x10]
       vxorps   xmm1, xmm1, xmm1
       vcmpneqps xmm4, xmm0, xmm0
       vorps    xmm1, xmm4, xmm1
       vxorps   xmm4, xmm4, xmm4
       vcmpgtps xmm4, xmm0, xmm4
       vorps    xmm1, xmm4, xmm1
       vandps   xmm0, xmm1, xmm0
       vmulss   xmm1, xmm0, xmm0
       vmulss   xmm4, xmm11, xmm1
       vaddss   xmm4, xmm12, xmm4
       vmulss   xmm0, xmm13, xmm0
       vmulss   xmm0, xmm0, xmm1
       vaddss   xmm0, xmm14, xmm0
       mov      byte  ptr [rsp+0x1F8], 0
       test     r8d, r8d
       je       SHORT G_M000_IG31

G_M000_IG29:                ;; offset=0x0DF7
       lea      r8, [rsp+0x1F8]
       vmovaps  xmm1, xmm4
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovss   dword ptr [rsp+0x1F4], xmm0
       test     r13, r13
       je       SHORT G_M000_IG30
       movzx    r8, byte  ptr [rsp+0x1F8]
       mov      rcx, r13
       mov      edx, 1
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG30:                ;; offset=0x0E2E
       vmovss   xmm0, dword ptr [rsp+0x1F4]
       jmp      SHORT G_M000_IG32

G_M000_IG31:                ;; offset=0x0E39
       vdivss   xmm0, xmm0, xmm4

G_M000_IG32:                ;; offset=0x0E3D
       vmovss   xmm2, dword ptr [rsp+0x200]
       vsubss   xmm1, xmm2, dword ptr [rsp+0x204]
       vmulss   xmm0, xmm1, xmm0
       vmulss   xmm0, xmm0, xmm0
       vminss   xmm0, xmm8, xmm0
       vmulss   xmm0, xmm0, xmm6
       vaddss   xmm0, xmm9, xmm0
       vsqrtss  xmm0, xmm0, xmm0
       vmulss   xmm0, xmm0, xmm7
       vaddss   xmm15, xmm15, xmm0
       inc      r15d
       cmp      r15d, 4
       jl       G_M000_IG28

G_M000_IG33:                ;; offset=0x0E7C
       lea      r15d, [r14+0x01]
       mov      r8, gword ptr [rbx+0x18]
       mov      rax, r8
       mov      ecx, dword ptr [rax+0x08]
       cmp      r15d, ecx
       jae      G_M000_IG70
       mov      edx, r15d
       vmovss   xmm0, dword ptr [rax+4*rdx+0x10]
       vmovaps  xmm2, xmm0
       vmovss   dword ptr [rsp+0x1E8], xmm2
       mov      rax, r8
       lea      edx, [r15-0x01]
       cmp      edx, ecx
       jae      G_M000_IG70
       lea      edx, [r15-0x01]
       vmovss   xmm0, dword ptr [rax+4*rdx+0x10]
       mov      rax, r8
       lea      edx, [r15+0x01]
       cmp      edx, ecx
       jae      G_M000_IG70
       lea      edx, [r15+0x01]
       vaddss   xmm0, xmm0, dword ptr [rax+4*rdx+0x10]
       mov      rax, r8
       mov      edx, dword ptr [rbx+0x54]
       mov      r10d, r15d
       sub      r10d, edx
       cmp      r10d, ecx
       jae      G_M000_IG70
       vmovss   xmm1, dword ptr [rax+4*r10+0x10]
       add      edx, r15d
       cmp      edx, ecx
       jae      G_M000_IG70
       mov      eax, edx
       vaddss   xmm1, xmm1, dword ptr [r8+4*rax+0x10]
       vaddss   xmm0, xmm0, xmm1
       vmulss   xmm0, xmm0, xmm7
       vmovss   dword ptr [rsp+0x1EC], xmm0
       vaddss   xmm0, xmm10, xmm2
       movzx    r8, byte  ptr [rbx+0x69]
       mov      r13, gword ptr [rbx+0x10]
       vxorps   xmm1, xmm1, xmm1
       vcmpneqps xmm4, xmm0, xmm0
       vorps    xmm1, xmm4, xmm1
       vxorps   xmm4, xmm4, xmm4
       vcmpgtps xmm4, xmm0, xmm4
       vorps    xmm1, xmm4, xmm1
       vandps   xmm0, xmm1, xmm0
       vmulss   xmm1, xmm0, xmm0
       vmulss   xmm4, xmm11, xmm1
       vaddss   xmm4, xmm12, xmm4
       vmulss   xmm0, xmm13, xmm0
       vmulss   xmm0, xmm0, xmm1
       vaddss   xmm0, xmm14, xmm0
       mov      byte  ptr [rsp+0x1E0], 0
       test     r8d, r8d
       je       SHORT G_M000_IG36

G_M000_IG34:                ;; offset=0x0F6C
       lea      r8, [rsp+0x1E0]
       vmovaps  xmm1, xmm4
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovss   dword ptr [rsp+0x1DC], xmm0
       test     r13, r13
       je       SHORT G_M000_IG35
       movzx    r8, byte  ptr [rsp+0x1E0]
       mov      rcx, r13
       mov      edx, 1
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG35:                ;; offset=0x0FA3
       vmovss   xmm0, dword ptr [rsp+0x1DC]
       jmp      SHORT G_M000_IG37

G_M000_IG36:                ;; offset=0x0FAE
       vdivss   xmm0, xmm0, xmm4

G_M000_IG37:                ;; offset=0x0FB2
       vmovss   xmm2, dword ptr [rsp+0x1E8]
       vsubss   xmm1, xmm2, dword ptr [rsp+0x1EC]
       vmulss   xmm0, xmm1, xmm0
       vmulss   xmm0, xmm0, xmm0
       vminss   xmm0, xmm8, xmm0
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD00]
       vaddss   xmm0, xmm9, xmm0
       vsqrtss  xmm0, xmm0, xmm0
       vmulss   xmm0, xmm0, xmm7
       vmovss   dword ptr [rsp+0x1F0], xmm0
       mov      r13d, 1

G_M000_IG38:                ;; offset=0x0FF3
       mov      r8d, dword ptr [rbx+0x54]
       mov      eax, r13d
       imul     eax, r8d
       add      eax, r15d
       mov      rcx, gword ptr [rbx+0x18]
       mov      rdx, rcx
       mov      r10d, dword ptr [rdx+0x08]
       cmp      eax, r10d
       jae      G_M000_IG70
       mov      r9d, eax
       vmovss   xmm0, dword ptr [rdx+4*r9+0x10]
       vmovaps  xmm3, xmm0
       vmovss   dword ptr [rsp+0x1D4], xmm3
       mov      rdx, rcx
       lea      r9d, [rax-0x01]
       cmp      r9d, r10d
       jae      G_M000_IG70
       lea      r9d, [rax-0x01]
       vmovss   xmm0, dword ptr [rdx+4*r9+0x10]
       mov      rdx, rcx
       lea      r9d, [rax+0x01]
       cmp      r9d, r10d
       jae      G_M000_IG70
       lea      r9d, [rax+0x01]
       vaddss   xmm0, xmm0, dword ptr [rdx+4*r9+0x10]
       mov      rdx, rcx
       mov      r9d, eax
       sub      r9d, r8d
       cmp      r9d, r10d
       jae      G_M000_IG70
       vmovss   xmm1, dword ptr [rdx+4*r9+0x10]
       add      r8d, eax
       cmp      r8d, r10d
       jae      G_M000_IG70
       vaddss   xmm1, xmm1, dword ptr [rcx+4*r8+0x10]
       vaddss   xmm0, xmm0, xmm1
       vmulss   xmm0, xmm0, xmm7
       vmovss   dword ptr [rsp+0x1D8], xmm0
       vaddss   xmm0, xmm10, xmm3
       movzx    r8, byte  ptr [rbx+0x69]
       mov      r12, gword ptr [rbx+0x10]
       vxorps   xmm1, xmm1, xmm1
       vcmpneqps xmm5, xmm0, xmm0
       vorps    xmm1, xmm5, xmm1
       vxorps   xmm5, xmm5, xmm5
       vcmpgtps xmm5, xmm0, xmm5
       vorps    xmm1, xmm5, xmm1
       vandps   xmm0, xmm1, xmm0
       vmulss   xmm1, xmm0, xmm0
       vmulss   xmm5, xmm11, xmm1
       vaddss   xmm5, xmm12, xmm5
       vmulss   xmm0, xmm13, xmm0
       vmulss   xmm0, xmm0, xmm1
       vaddss   xmm0, xmm14, xmm0
       mov      byte  ptr [rsp+0x1C8], 0
       test     r8d, r8d
       je       SHORT G_M000_IG41

G_M000_IG39:                ;; offset=0x10EF
       lea      r8, [rsp+0x1C8]
       vmovaps  xmm1, xmm5
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovss   dword ptr [rsp+0x1C4], xmm0
       test     r12, r12
       je       SHORT G_M000_IG40
       movzx    r8, byte  ptr [rsp+0x1C8]
       mov      rcx, r12
       mov      edx, 1
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG40:                ;; offset=0x1126
       vmovss   xmm0, dword ptr [rsp+0x1C4]
       jmp      SHORT G_M000_IG42

G_M000_IG41:                ;; offset=0x1131
       vdivss   xmm0, xmm0, xmm5

G_M000_IG42:                ;; offset=0x1135
       vmovss   xmm3, dword ptr [rsp+0x1D4]
       vsubss   xmm1, xmm3, dword ptr [rsp+0x1D8]
       vmulss   xmm0, xmm1, xmm0
       vmulss   xmm0, xmm0, xmm0
       vminss   xmm0, xmm8, xmm0
       vmulss   xmm0, xmm0, xmm6
       vaddss   xmm0, xmm9, xmm0
       vsqrtss  xmm0, xmm0, xmm0
       vmulss   xmm0, xmm0, xmm7
       vaddss   xmm2, xmm0, dword ptr [rsp+0x1F0]
       inc      r13d
       cmp      r13d, 4
       vmovss   dword ptr [rsp+0x1F0], xmm2
       jl       G_M000_IG38

G_M000_IG43:                ;; offset=0x1182
       vmovss   xmm2, dword ptr [rsp+0x1F0]
       vmovss   dword ptr [rsp+0x40C], xmm2
       lea      r15d, [r14+0x02]
       mov      r8, gword ptr [rbx+0x18]
       mov      rax, r8
       mov      ecx, dword ptr [rax+0x08]
       cmp      r15d, ecx
       jae      G_M000_IG70
       mov      edx, r15d
       vmovss   xmm0, dword ptr [rax+4*rdx+0x10]
       vmovaps  xmm3, xmm0
       vmovss   dword ptr [rsp+0x1B8], xmm3
       mov      rax, r8
       lea      edx, [r15-0x01]
       cmp      edx, ecx
       jae      G_M000_IG70
       lea      edx, [r15-0x01]
       vmovss   xmm0, dword ptr [rax+4*rdx+0x10]
       mov      rax, r8
       lea      edx, [r15+0x01]
       cmp      edx, ecx
       jae      G_M000_IG70
       lea      edx, [r15+0x01]
       vaddss   xmm0, xmm0, dword ptr [rax+4*rdx+0x10]
       mov      rax, r8
       mov      edx, dword ptr [rbx+0x54]
       mov      r10d, r15d
       sub      r10d, edx
       cmp      r10d, ecx
       jae      G_M000_IG70
       vmovss   xmm1, dword ptr [rax+4*r10+0x10]
       add      edx, r15d
       cmp      edx, ecx
       jae      G_M000_IG70
       mov      eax, edx
       vaddss   xmm1, xmm1, dword ptr [r8+4*rax+0x10]
       vaddss   xmm0, xmm0, xmm1
       vmulss   xmm0, xmm0, xmm7
       vmovss   dword ptr [rsp+0x1BC], xmm0
       vaddss   xmm0, xmm10, xmm3
       movzx    r8, byte  ptr [rbx+0x69]
       mov      r13, gword ptr [rbx+0x10]
       vxorps   xmm1, xmm1, xmm1
       vcmpneqps xmm5, xmm0, xmm0
       vorps    xmm1, xmm5, xmm1
       vxorps   xmm5, xmm5, xmm5
       vcmpgtps xmm5, xmm0, xmm5
       vorps    xmm1, xmm5, xmm1
       vandps   xmm0, xmm1, xmm0
       vmulss   xmm1, xmm0, xmm0
       vmulss   xmm5, xmm11, xmm1
       vaddss   xmm5, xmm12, xmm5
       vmulss   xmm0, xmm13, xmm0
       vmulss   xmm0, xmm0, xmm1
       vaddss   xmm0, xmm14, xmm0
       mov      byte  ptr [rsp+0x1B0], 0
       test     r8d, r8d

G_M000_IG44:                ;; offset=0x1282
       je       SHORT G_M000_IG47

G_M000_IG45:                ;; offset=0x1284
       lea      r8, [rsp+0x1B0]
       vmovaps  xmm1, xmm5
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovss   dword ptr [rsp+0x1AC], xmm0
       test     r13, r13
       je       SHORT G_M000_IG46
       movzx    r8, byte  ptr [rsp+0x1B0]
       mov      rcx, r13
       mov      edx, 1
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG46:                ;; offset=0x12BB
       vmovss   xmm0, dword ptr [rsp+0x1AC]
       jmp      SHORT G_M000_IG48

G_M000_IG47:                ;; offset=0x12C6
       vdivss   xmm0, xmm0, xmm5

G_M000_IG48:                ;; offset=0x12CA
       vmovss   xmm3, dword ptr [rsp+0x1B8]
       vsubss   xmm1, xmm3, dword ptr [rsp+0x1BC]
       vmulss   xmm0, xmm1, xmm0
       vmulss   xmm0, xmm0, xmm0
       vminss   xmm0, xmm8, xmm0
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD00]
       vaddss   xmm0, xmm9, xmm0
       vsqrtss  xmm0, xmm0, xmm0
       vmulss   xmm0, xmm0, xmm7
       vmovss   dword ptr [rsp+0x1C0], xmm0
       mov      r13d, 1

G_M000_IG49:                ;; offset=0x130B
       mov      r8d, r13d
       imul     r8d, dword ptr [rbx+0x54]
       add      r8d, r15d
       mov      rax, gword ptr [rbx+0x18]
       mov      rcx, rax
       mov      edx, dword ptr [rcx+0x08]
       cmp      r8d, edx
       jae      G_M000_IG70
       mov      r10d, r8d
       vmovss   xmm0, dword ptr [rcx+4*r10+0x10]
       vmovaps  xmm4, xmm0
       vmovss   dword ptr [rsp+0x1A4], xmm4
       mov      rcx, rax
       lea      r10d, [r8-0x01]
       cmp      r10d, edx
       jae      G_M000_IG70
       lea      r10d, [r8-0x01]
       vmovss   xmm0, dword ptr [rcx+4*r10+0x10]
       mov      rcx, rax
       lea      r10d, [r8+0x01]
       cmp      r10d, edx
       jae      G_M000_IG70
       lea      r10d, [r8+0x01]
       vaddss   xmm0, xmm0, dword ptr [rcx+4*r10+0x10]
       mov      rcx, rax
       mov      r10d, r8d
       sub      r10d, dword ptr [rbx+0x54]
       cmp      r10d, edx
       jae      G_M000_IG70
       vmovss   xmm1, dword ptr [rcx+4*r10+0x10]
       add      r8d, dword ptr [rbx+0x54]
       cmp      r8d, edx
       jae      G_M000_IG70
       vaddss   xmm1, xmm1, dword ptr [rax+4*r8+0x10]
       vaddss   xmm0, xmm0, xmm1
       vmulss   xmm0, xmm0, xmm7
       vmovss   dword ptr [rsp+0x1A8], xmm0
       vaddss   xmm0, xmm10, xmm4
       movzx    r8, byte  ptr [rbx+0x69]
       mov      r12, gword ptr [rbx+0x10]
       vxorps   xmm1, xmm1, xmm1
       vcmpneqps xmm2, xmm0, xmm0
       vorps    xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpgtps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandps   xmm0, xmm1, xmm0
       vmulss   xmm1, xmm0, xmm0
       vmulss   xmm2, xmm11, xmm1
       vaddss   xmm2, xmm12, xmm2
       vmulss   xmm0, xmm13, xmm0
       vmulss   xmm0, xmm0, xmm1
       vaddss   xmm0, xmm14, xmm0
       mov      byte  ptr [rsp+0x198], 0
       test     r8d, r8d
       je       SHORT G_M000_IG52

G_M000_IG50:                ;; offset=0x1405
       lea      r8, [rsp+0x198]
       vmovaps  xmm1, xmm2
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovss   dword ptr [rsp+0x194], xmm0
       test     r12, r12
       je       SHORT G_M000_IG51
       movzx    r8, byte  ptr [rsp+0x198]
       mov      rcx, r12
       mov      edx, 1
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG51:                ;; offset=0x143C
       vmovss   xmm0, dword ptr [rsp+0x194]
       jmp      SHORT G_M000_IG53

G_M000_IG52:                ;; offset=0x1447
       vdivss   xmm0, xmm0, xmm2

G_M000_IG53:                ;; offset=0x144B
       vmovss   xmm4, dword ptr [rsp+0x1A4]
       vsubss   xmm1, xmm4, dword ptr [rsp+0x1A8]
       vmulss   xmm0, xmm1, xmm0
       vmulss   xmm0, xmm0, xmm0
       vminss   xmm0, xmm8, xmm0
       vmulss   xmm0, xmm0, xmm6
       vaddss   xmm0, xmm9, xmm0
       vsqrtss  xmm0, xmm0, xmm0
       vmulss   xmm0, xmm0, xmm7
       vaddss   xmm3, xmm0, dword ptr [rsp+0x1C0]
       inc      r13d
       cmp      r13d, 4
       vmovss   dword ptr [rsp+0x1C0], xmm3
       jl       G_M000_IG49

G_M000_IG54:                ;; offset=0x1498
       vmovss   xmm3, dword ptr [rsp+0x1C0]
       vmovss   dword ptr [rsp+0x408], xmm3
       add      r14d, 3
       mov      r8, gword ptr [rbx+0x18]
       cmp      r14d, dword ptr [r8+0x08]
       jae      G_M000_IG70
       mov      eax, r14d
       vmovss   xmm0, dword ptr [r8+4*rax+0x10]
       vmovaps  xmm4, xmm0
       vmovss   dword ptr [rsp+0x188], xmm4
       mov      r8, gword ptr [rbx+0x18]
       lea      eax, [r14-0x01]
       cmp      eax, dword ptr [r8+0x08]
       jae      G_M000_IG70
       lea      eax, [r14-0x01]
       vmovss   xmm0, dword ptr [r8+4*rax+0x10]
       mov      r8, gword ptr [rbx+0x18]
       lea      eax, [r14+0x01]
       cmp      eax, dword ptr [r8+0x08]
       jae      G_M000_IG70
       lea      eax, [r14+0x01]
       vaddss   xmm0, xmm0, dword ptr [r8+4*rax+0x10]
       mov      r8, gword ptr [rbx+0x18]
       mov      eax, r14d
       sub      eax, dword ptr [rbx+0x54]
       cmp      eax, dword ptr [r8+0x08]
       jae      G_M000_IG70
       vmovss   xmm1, dword ptr [r8+4*rax+0x10]
       mov      r8, gword ptr [rbx+0x18]
       mov      eax, r14d
       add      eax, dword ptr [rbx+0x54]
       cmp      eax, dword ptr [r8+0x08]
       jae      G_M000_IG70
       vaddss   xmm1, xmm1, dword ptr [r8+4*rax+0x10]
       vaddss   xmm0, xmm0, xmm1
       vmulss   xmm0, xmm0, xmm7
       vmovss   dword ptr [rsp+0x18C], xmm0
       vaddss   xmm0, xmm10, xmm4
       movzx    r8, byte  ptr [rbx+0x69]
       mov      r15, gword ptr [rbx+0x10]
       vxorps   xmm1, xmm1, xmm1
       vcmpneqps xmm2, xmm0, xmm0
       vorps    xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpgtps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandps   xmm0, xmm1, xmm0
       vmulss   xmm1, xmm0, xmm0
       vmulss   xmm2, xmm11, xmm1
       vaddss   xmm2, xmm12, xmm2
       vmulss   xmm0, xmm13, xmm0
       vmulss   xmm0, xmm0, xmm1
       vaddss   xmm0, xmm14, xmm0
       mov      byte  ptr [rsp+0x180], 0

G_M000_IG55:                ;; offset=0x159F
       test     r8d, r8d
       je       SHORT G_M000_IG58

G_M000_IG56:                ;; offset=0x15A4
       lea      r8, [rsp+0x180]
       vmovaps  xmm1, xmm2
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovss   dword ptr [rsp+0x17C], xmm0
       test     r15, r15
       je       SHORT G_M000_IG57
       movzx    r8, byte  ptr [rsp+0x180]
       mov      rcx, r15
       mov      edx, 1
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG57:                ;; offset=0x15DB
       vmovss   xmm0, dword ptr [rsp+0x17C]
       jmp      SHORT G_M000_IG59

G_M000_IG58:                ;; offset=0x15E6
       vdivss   xmm0, xmm0, xmm2

G_M000_IG59:                ;; offset=0x15EA
       vmovss   xmm4, dword ptr [rsp+0x188]
       vsubss   xmm1, xmm4, dword ptr [rsp+0x18C]
       vmulss   xmm0, xmm1, xmm0
       vmulss   xmm0, xmm0, xmm0
       vminss   xmm0, xmm8, xmm0
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD00]
       vaddss   xmm0, xmm9, xmm0
       vsqrtss  xmm0, xmm0, xmm0
       vmulss   xmm0, xmm0, xmm7
       vmovss   dword ptr [rsp+0x190], xmm0
       mov      r15d, 1

G_M000_IG60:                ;; offset=0x162B
       mov      r8d, r15d
       imul     r8d, dword ptr [rbx+0x54]
       add      r8d, r14d
       mov      rax, gword ptr [rbx+0x18]
       cmp      r8d, dword ptr [rax+0x08]
       jae      G_M000_IG70
       mov      ecx, r8d
       vmovss   xmm0, dword ptr [rax+4*rcx+0x10]
       vmovaps  xmm4, xmm0
       vmovss   dword ptr [rsp+0x174], xmm4
       mov      rax, gword ptr [rbx+0x18]
       lea      ecx, [r8-0x01]
       cmp      ecx, dword ptr [rax+0x08]
       jae      G_M000_IG70
       lea      ecx, [r8-0x01]
       vmovss   xmm0, dword ptr [rax+4*rcx+0x10]
       mov      rax, gword ptr [rbx+0x18]
       lea      ecx, [r8+0x01]
       cmp      ecx, dword ptr [rax+0x08]
       jae      G_M000_IG70
       lea      ecx, [r8+0x01]
       vaddss   xmm0, xmm0, dword ptr [rax+4*rcx+0x10]
       mov      rax, gword ptr [rbx+0x18]
       mov      ecx, r8d
       sub      ecx, dword ptr [rbx+0x54]
       cmp      ecx, dword ptr [rax+0x08]
       jae      G_M000_IG70
       vmovss   xmm1, dword ptr [rax+4*rcx+0x10]
       mov      rax, gword ptr [rbx+0x18]
       add      r8d, dword ptr [rbx+0x54]
       cmp      r8d, dword ptr [rax+0x08]
       jae      G_M000_IG70
       vaddss   xmm1, xmm1, dword ptr [rax+4*r8+0x10]
       vaddss   xmm0, xmm0, xmm1
       vmulss   xmm0, xmm0, xmm7
       vmovss   dword ptr [rsp+0x178], xmm0
       vaddss   xmm0, xmm10, xmm4
       movzx    r8, byte  ptr [rbx+0x69]
       mov      r13, gword ptr [rbx+0x10]
       vxorps   xmm1, xmm1, xmm1
       vcmpneqps xmm3, xmm0, xmm0
       vorps    xmm1, xmm3, xmm1
       vxorps   xmm3, xmm3, xmm3
       vcmpgtps xmm3, xmm0, xmm3
       vorps    xmm1, xmm3, xmm1
       vandps   xmm0, xmm1, xmm0
       vmulss   xmm1, xmm0, xmm0
       vmulss   xmm3, xmm11, xmm1
       vaddss   xmm3, xmm12, xmm3
       vmulss   xmm0, xmm13, xmm0
       vmulss   xmm0, xmm0, xmm1
       vaddss   xmm0, xmm14, xmm0
       mov      byte  ptr [rsp+0x168], 0
       test     r8d, r8d

G_M000_IG61:                ;; offset=0x1721
       je       SHORT G_M000_IG64

G_M000_IG62:                ;; offset=0x1723
       lea      r8, [rsp+0x168]
       vmovaps  xmm1, xmm3
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovss   dword ptr [rsp+0x164], xmm0
       test     r13, r13
       je       SHORT G_M000_IG63
       movzx    r8, byte  ptr [rsp+0x168]
       mov      rcx, r13
       mov      edx, 1
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG63:                ;; offset=0x175A
       vmovss   xmm0, dword ptr [rsp+0x164]
       jmp      SHORT G_M000_IG65

G_M000_IG64:                ;; offset=0x1765
       vdivss   xmm0, xmm0, xmm3

G_M000_IG65:                ;; offset=0x1769
       vmovss   xmm4, dword ptr [rsp+0x174]
       vsubss   xmm1, xmm4, dword ptr [rsp+0x178]
       vmulss   xmm0, xmm1, xmm0
       vmulss   xmm0, xmm0, xmm0
       vminss   xmm0, xmm8, xmm0
       vmulss   xmm0, xmm0, xmm6
       vaddss   xmm0, xmm9, xmm0
       vsqrtss  xmm0, xmm0, xmm0
       vmulss   xmm0, xmm0, xmm7
       vaddss   xmm2, xmm0, dword ptr [rsp+0x190]
       inc      r15d
       cmp      r15d, 4
       vmovss   dword ptr [rsp+0x190], xmm2
       jl       G_M000_IG60

G_M000_IG66:                ;; offset=0x17B6
       mov      rax, gword ptr [rbx+0x20]
       mov      ecx, ebp
       sar      ecx, 31
       and      ecx, 3
       add      ecx, ebp
       sar      ecx, 2
       add      ecx, edi
       cmp      ecx, dword ptr [rax+0x08]
       jae      G_M000_IG70
       vaddss   xmm0, xmm15, dword ptr [rsp+0x40C]
       vaddss   xmm0, xmm0, dword ptr [rsp+0x408]
       vaddss   xmm0, xmm0, dword ptr [rsp+0x190]
       vmulss   xmm0, xmm0, xmm7
       vmovss   dword ptr [rax+4*rcx+0x10], xmm0
       add      ebp, 4

G_M000_IG67:                ;; offset=0x17FA
       cmp      ebp, dword ptr [rbx+0x4C]
       jl       G_M000_IG22

G_M000_IG68:                ;; offset=0x1803
       vzeroupper
       vmovaps  xmm6, xmmword ptr [rsp+0x510]
       vmovaps  xmm7, xmmword ptr [rsp+0x500]
       vmovaps  xmm8, xmmword ptr [rsp+0x4F0]
       vmovaps  xmm9, xmmword ptr [rsp+0x4E0]
       vmovaps  xmm10, xmmword ptr [rsp+0x4D0]
       vmovaps  xmm11, xmmword ptr [rsp+0x4C0]
       vmovaps  xmm12, xmmword ptr [rsp+0x4B0]
       vmovaps  xmm13, xmmword ptr [rsp+0x4A0]
       vmovaps  xmm14, xmmword ptr [rsp+0x490]
       vmovaps  xmm15, xmmword ptr [rsp+0x480]
       add      rsp, 0x528
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r12
       pop      r13
       pop      r14
       pop      r15
       ret

G_M000_IG69:                ;; offset=0x1874
       call     [System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()]
       int3

G_M000_IG70:                ;; offset=0x187B
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	480E0640h		;    145433
RWD04  	dd	3E4CCCCDh		;       0.2
RWD08  	dd	41E00000h		;        28
RWD12  	dd	3E800000h		;      0.25
RWD16  	dd	409B0A3Dh		;     4.845
RWD20  	dd	42EF0C23h		;   119.524
RWD24  	dd	44228FFEh		;    650.25
RWD28  	dd	3B1DEA78h		; 0.00240961
RWD32  	dd	449E380Dh		;   1265.75

; Total bytes of code 6273

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ComputeFuzzyErosion(int,int,bool):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 33 single block inlinees; 11 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r15
       push     r14
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 360
       vmovaps  xmmword ptr [rsp+0x150], xmm6
       vmovaps  xmmword ptr [rsp+0x140], xmm7
       vmovaps  xmmword ptr [rsp+0x130], xmm8
       vmovaps  xmmword ptr [rsp+0x120], xmm9
       vmovaps  xmmword ptr [rsp+0x110], xmm10
       vmovaps  xmmword ptr [rsp+0x100], xmm11
       vmovaps  xmmword ptr [rsp+0xF0], xmm12
       vmovaps  xmmword ptr [rsp+0xE0], xmm13
       vmovaps  xmmword ptr [rsp+0xD0], xmm14
       vmovaps  xmmword ptr [rsp+0xC0], xmm15
       mov      rbx, rcx
       mov      esi, edx
       mov      edi, r8d

G_M000_IG02:                ;; offset=0x0071
       xor      ebp, ebp
       test     r9b, r9b
       je       G_M000_IG23

G_M000_IG03:                ;; offset=0x007C
       cmp      dword ptr [rbx+0x58], 9
       jle      G_M000_IG23
       mov      ebp, 1
       mov      rcx, rbx
       mov      edx, esi
       mov      r8d, edi
       xor      r9d, r9d
       call     [DotJpegli.Internal.AdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this]
       mov      ecx, dword ptr [rbx+0x58]
       lea      edx, [rcx-0x08]
       cmp      edx, 1
       jle      G_M000_IG23
       align    [0 bytes for IG04]

G_M000_IG04:                ;; offset=0x00AB
       lea      edx, [rsi-0x01]
       imul     edx, ecx
       add      edx, ebp
       mov      r8d, esi
       imul     r8d, ecx
       add      r8d, ebp
       lea      r9d, [rsi+0x01]
       imul     r9d, ecx
       add      r9d, ebp
       mov      rax, gword ptr [rbx+0x20]
       mov      r10, rax
       lea      r11d, [rdx-0x01]
       test     r11d, r11d
       jl       G_M000_IG26
       mov      r14d, dword ptr [r10+0x08]
       mov      r15d, r14d
       sub      r15d, r11d
       cmp      r15d, 8
       jl       G_M000_IG26
       cmp      r11d, r14d
       jae      G_M000_IG29
       vmovups  ymm0, ymmword ptr [r10+4*r11+0x10]
       mov      r10, rax
       test     edx, edx
       jl       G_M000_IG26
       mov      r11d, r14d
       sub      r11d, edx
       cmp      r11d, 8
       jl       G_M000_IG26
       cmp      edx, r14d
       jae      G_M000_IG29
       mov      r11d, edx
       vmovups  ymm1, ymmword ptr [r10+4*r11+0x10]
       mov      r10, rax
       inc      edx
       mov      r11d, r14d
       sub      r11d, edx
       cmp      r11d, 8
       jl       G_M000_IG26
       cmp      edx, r14d
       jae      G_M000_IG29
       vmovups  ymm2, ymmword ptr [r10+4*rdx+0x10]
       mov      rdx, rax
       lea      r10d, [r8-0x01]
       test     r10d, r10d
       jl       G_M000_IG26
       mov      r11d, r14d
       sub      r11d, r10d
       cmp      r11d, 8
       jl       G_M000_IG26
       cmp      r10d, r14d
       jae      G_M000_IG29
       vmovups  ymm3, ymmword ptr [rdx+4*r10+0x10]
       mov      rdx, rax
       test     r8d, r8d

G_M000_IG05:                ;; offset=0x0189
       jl       G_M000_IG26
       mov      r10d, r14d
       sub      r10d, r8d
       cmp      r10d, 8
       jl       G_M000_IG26
       cmp      r8d, r14d
       jae      G_M000_IG29
       mov      r10d, r8d
       vmovups  ymm4, ymmword ptr [rdx+4*r10+0x10]
       mov      rdx, rax
       inc      r8d
       mov      r10d, r14d
       sub      r10d, r8d
       cmp      r10d, 8
       jl       G_M000_IG26
       cmp      r8d, r14d
       jae      G_M000_IG29
       vmovups  ymm5, ymmword ptr [rdx+4*r8+0x10]
       mov      rdx, rax
       lea      r8d, [r9-0x01]
       test     r8d, r8d
       jl       G_M000_IG26
       mov      r10d, r14d
       sub      r10d, r8d
       cmp      r10d, 8
       jl       G_M000_IG26
       cmp      r8d, r14d
       jae      G_M000_IG29
       vmovups  ymm6, ymmword ptr [rdx+4*r8+0x10]
       mov      rdx, rax
       test     r9d, r9d
       jl       G_M000_IG26
       mov      r8d, r14d
       sub      r8d, r9d
       cmp      r8d, 8
       jl       G_M000_IG26
       cmp      r9d, r14d
       jae      G_M000_IG29
       mov      r8d, r9d
       vmovups  ymm7, ymmword ptr [rdx+4*r8+0x10]
       inc      r9d
       mov      edx, r14d
       sub      edx, r9d
       cmp      edx, 8
       jl       G_M000_IG26
       cmp      r9d, r14d
       jae      G_M000_IG29
       mov      edx, r9d
       vmovups  ymm8, ymmword ptr [rax+4*rdx+0x10]
       cmp      byte  ptr [rbx+0x68], 0

G_M000_IG06:                ;; offset=0x025F
       jne      G_M000_IG15

G_M000_IG07:                ;; offset=0x0265
       vcmpeqps ymm9, ymm0, ymm1
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm1
       vandps   ymm10, ymm10, ymm9
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm10, ymm10, ymm11
       vcmpltps ymm12, ymm1, ymm0
       vorps    ymm10, ymm12, ymm10
       vandps   ymm12, ymm10, ymm0
       vandnps  ymm10, ymm10, ymm1
       vorps    ymm10, ymm10, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm0
       vandps   ymm9, ymm12, ymm9
       vorps    ymm11, ymm9, ymm11
       vcmpltps ymm9, ymm0, ymm1
       vorps    ymm9, ymm9, ymm11
       vandps   ymm0, ymm9, ymm0
       vandnps  ymm1, ymm9, ymm1
       vorps    ymm0, ymm1, ymm0
       vcmpeqps ymm1, ymm2, ymm3
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm3
       vandps   ymm9, ymm9, ymm1
       vcmpneqps ymm11, ymm2, ymm2
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm12, ymm3, ymm2
       vorps    ymm9, ymm12, ymm9
       vandps   ymm12, ymm9, ymm2
       vandnps  ymm9, ymm9, ymm3
       vorps    ymm9, ymm9, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm2
       vandps   ymm1, ymm12, ymm1
       vorps    ymm1, ymm11, ymm1
       vcmpltps ymm11, ymm2, ymm3
       vorps    ymm1, ymm11, ymm1
       vandps   ymm2, ymm2, ymm1
       vandnps  ymm3, ymm1, ymm3
       vorps    ymm2, ymm3, ymm2
       vcmpeqps ymm1, ymm0, ymm2
       vxorps   ymm3, ymm3, ymm3
       vpcmpgtd ymm3, ymm3, ymm2
       vandps   ymm3, ymm3, ymm1
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm12, ymm2, ymm0
       vorps    ymm3, ymm12, ymm3
       vandps   ymm12, ymm0, ymm3
       vandnps  ymm3, ymm3, ymm2
       vorps    ymm3, ymm12, ymm3
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm0
       vandps   ymm1, ymm12, ymm1
       vorps    ymm1, ymm11, ymm1
       vcmpltps ymm11, ymm0, ymm2
       vorps    ymm1, ymm11, ymm1
       vandps   ymm0, ymm0, ymm1
       vandnps  ymm2, ymm1, ymm2

G_M000_IG08:                ;; offset=0x036C
       vorps    ymm0, ymm2, ymm0
       vcmpeqps ymm1, ymm10, ymm9
       vxorps   ymm2, ymm2, ymm2
       vpcmpgtd ymm2, ymm2, ymm9
       vandps   ymm2, ymm2, ymm1
       vcmpneqps ymm11, ymm10, ymm10
       vorps    ymm2, ymm11, ymm2
       vcmpltps ymm12, ymm9, ymm10
       vorps    ymm2, ymm12, ymm2
       vandps   ymm12, ymm10, ymm2
       vandnps  ymm2, ymm2, ymm9
       vorps    ymm2, ymm12, ymm2
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm10
       vandps   ymm1, ymm12, ymm1
       vorps    ymm1, ymm11, ymm1
       vcmpltps ymm11, ymm10, ymm9
       vorps    ymm1, ymm11, ymm1
       vandps   ymm10, ymm10, ymm1
       vandnps  ymm1, ymm1, ymm9
       vorps    ymm1, ymm10, ymm1
       vcmpeqps ymm9, ymm1, ymm3
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm3
       vandps   ymm10, ymm10, ymm9
       vcmpneqps ymm11, ymm1, ymm1
       vorps    ymm10, ymm10, ymm11
       vcmpltps ymm12, ymm3, ymm1
       vorps    ymm10, ymm12, ymm10
       vandps   ymm12, ymm10, ymm1
       vandnps  ymm10, ymm10, ymm3
       vorps    ymm10, ymm10, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm1
       vandps   ymm9, ymm12, ymm9
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm11, ymm1, ymm3
       vorps    ymm9, ymm11, ymm9
       vandps   ymm1, ymm9, ymm1
       vandnps  ymm3, ymm9, ymm3
       vorps    ymm1, ymm3, ymm1
       vcmpeqps ymm3, ymm0, ymm4
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm4
       vandps   ymm9, ymm9, ymm3
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm12, ymm4, ymm0
       vorps    ymm9, ymm12, ymm9
       vandps   ymm12, ymm9, ymm0
       vandnps  ymm9, ymm9, ymm4
       vorps    ymm9, ymm9, ymm12
       vcmpeqps ymm12, ymm1, ymm9
       vxorps   ymm13, ymm13, ymm13
       vpcmpgtd ymm13, ymm13, ymm9
       vandps   ymm13, ymm13, ymm12
       vcmpneqps ymm14, ymm1, ymm1
       vmovups  ymmword ptr [rsp+0xA0], ymm14
       vorps    ymm13, ymm13, ymm14

G_M000_IG09:                ;; offset=0x0485
       vcmpltps ymm15, ymm9, ymm1
       vorps    ymm13, ymm15, ymm13
       vandps   ymm15, ymm13, ymm1
       vandnps  ymm13, ymm13, ymm9
       vorps    ymm13, ymm13, ymm15
       vcmpeqps ymm15, ymm10, ymm13
       vmovups  ymmword ptr [rsp+0x80], ymm15
       vxorps   ymm14, ymm14, ymm14
       vpcmpgtd ymm14, ymm14, ymm13
       vandps   ymm14, ymm14, ymm15
       vcmpneqps ymm15, ymm10, ymm10
       vmovups  ymmword ptr [rsp+0x60], ymm15
       vorps    ymm14, ymm14, ymm15
       vcmpltps ymm15, ymm13, ymm10
       vorps    ymm14, ymm15, ymm14
       vandps   ymm15, ymm10, ymm14
       vandnps  ymm14, ymm14, ymm13
       vorps    ymm14, ymm14, ymm15
       vxorps   ymm15, ymm15, ymm15
       vpcmpgtd ymm15, ymm15, ymm0
       vandps   ymm3, ymm15, ymm3
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm11, ymm0, ymm4
       vorps    ymm3, ymm11, ymm3
       vandps   ymm0, ymm0, ymm3
       vandnps  ymm4, ymm3, ymm4
       vorps    ymm0, ymm4, ymm0
       vxorps   ymm3, ymm3, ymm3
       vpcmpgtd ymm3, ymm3, ymm1
       vandps   ymm3, ymm12, ymm3
       vorps    ymm3, ymm3, ymmword ptr [rsp+0xA0]
       vcmpltps ymm4, ymm1, ymm9
       vorps    ymm3, ymm4, ymm3
       vandps   ymm1, ymm1, ymm3
       vandnps  ymm3, ymm3, ymm9
       vorps    ymm1, ymm3, ymm1
       vxorps   ymm3, ymm3, ymm3
       vpcmpgtd ymm3, ymm3, ymm10
       vandps   ymm3, ymm3, ymmword ptr [rsp+0x80]
       vorps    ymm3, ymm3, ymmword ptr [rsp+0x60]
       vcmpltps ymm4, ymm10, ymm13
       vorps    ymm3, ymm4, ymm3
       vandps   ymm4, ymm10, ymm3
       vandnps  ymm3, ymm3, ymm13
       vorps    ymm3, ymm3, ymm4
       vcmpeqps ymm4, ymm2, ymm14
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm2
       vandps   ymm4, ymm9, ymm4
       vcmpneqps ymm9, ymm2, ymm2
       vorps    ymm4, ymm9, ymm4
       vcmpltps ymm9, ymm2, ymm14
       vorps    ymm4, ymm9, ymm4
       vblendvps ymm2, ymm14, ymm2, ymm4
       vcmpeqps ymm4, ymm0, ymm5
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm5
       vandps   ymm9, ymm9, ymm4
       vcmpneqps ymm10, ymm0, ymm0

G_M000_IG10:                ;; offset=0x05AA
       vorps    ymm9, ymm9, ymm10
       vcmpltps ymm11, ymm5, ymm0
       vorps    ymm9, ymm11, ymm9
       vandps   ymm11, ymm9, ymm0
       vandnps  ymm9, ymm9, ymm5
       vorps    ymm9, ymm9, ymm11
       vcmpeqps ymm11, ymm1, ymm9
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm9
       vandps   ymm12, ymm12, ymm11
       vcmpneqps ymm13, ymm1, ymm1
       vorps    ymm12, ymm12, ymm13
       vcmpltps ymm14, ymm9, ymm1
       vorps    ymm12, ymm14, ymm12
       vandps   ymm14, ymm12, ymm1
       vandnps  ymm12, ymm12, ymm9
       vorps    ymm12, ymm12, ymm14
       vcmpeqps ymm14, ymm3, ymm12
       vmovups  ymmword ptr [rsp+0x40], ymm14
       vxorps   ymm15, ymm15, ymm15
       vpcmpgtd ymm15, ymm15, ymm12
       vandps   ymm15, ymm15, ymm14
       vcmpneqps ymm14, ymm3, ymm3
       vmovups  ymmword ptr [rsp+0x20], ymm14
       vorps    ymm15, ymm15, ymm14
       vcmpltps ymm14, ymm12, ymm3
       vorps    ymm14, ymm14, ymm15
       vandps   ymm15, ymm14, ymm3
       vandnps  ymm14, ymm14, ymm12
       vorps    ymm14, ymm14, ymm15
       vxorps   ymm15, ymm15, ymm15
       vpcmpgtd ymm15, ymm15, ymm0
       vandps   ymm4, ymm15, ymm4
       vorps    ymm4, ymm10, ymm4
       vcmpltps ymm10, ymm0, ymm5
       vorps    ymm4, ymm10, ymm4
       vandps   ymm0, ymm0, ymm4
       vandnps  ymm5, ymm4, ymm5
       vorps    ymm0, ymm5, ymm0
       vxorps   ymm4, ymm4, ymm4
       vpcmpgtd ymm4, ymm4, ymm1
       vandps   ymm4, ymm11, ymm4
       vorps    ymm4, ymm13, ymm4
       vcmpltps ymm5, ymm1, ymm9
       vorps    ymm4, ymm5, ymm4
       vandps   ymm1, ymm1, ymm4
       vandnps  ymm4, ymm4, ymm9
       vorps    ymm1, ymm4, ymm1
       vxorps   ymm4, ymm4, ymm4
       vpcmpgtd ymm4, ymm4, ymm3
       vandps   ymm4, ymm4, ymmword ptr [rsp+0x40]
       vorps    ymm4, ymm4, ymmword ptr [rsp+0x20]
       vcmpltps ymm5, ymm3, ymm12
       vorps    ymm4, ymm5, ymm4
       vandps   ymm3, ymm3, ymm4
       vandnps  ymm4, ymm4, ymm12
       vorps    ymm3, ymm4, ymm3
       vcmpeqps ymm4, ymm2, ymm14
       vxorps   ymm5, ymm5, ymm5

G_M000_IG11:                ;; offset=0x06C2
       vpcmpgtd ymm5, ymm5, ymm2
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm2, ymm2
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm2, ymm14
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm2, ymm14, ymm2, ymm4
       vcmpeqps ymm4, ymm0, ymm6
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm6
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm0, ymm0
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm6, ymm0
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm4, ymm6, ymm0, ymm4
       vcmpeqps ymm5, ymm1, ymm4
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm4
       vandps   ymm5, ymm9, ymm5
       vcmpneqps ymm9, ymm1, ymm1
       vorps    ymm5, ymm9, ymm5
       vcmpltps ymm9, ymm4, ymm1
       vorps    ymm5, ymm9, ymm5
       vblendvps ymm5, ymm4, ymm1, ymm5
       vcmpeqps ymm9, ymm3, ymm5
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm5
       vandps   ymm9, ymm10, ymm9
       vcmpneqps ymm10, ymm3, ymm3
       vorps    ymm9, ymm10, ymm9
       vcmpltps ymm10, ymm5, ymm3
       vorps    ymm9, ymm10, ymm9
       vblendvps ymm9, ymm5, ymm3, ymm9
       vcmpeqps ymm10, ymm0, ymm6
       vxorps   ymm11, ymm11, ymm11
       vpcmpgtd ymm11, ymm11, ymm0
       vandps   ymm10, ymm11, ymm10
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm10, ymm11, ymm10
       vcmpltps ymm11, ymm0, ymm6
       vorps    ymm10, ymm11, ymm10
       vblendvps ymm0, ymm6, ymm0, ymm10
       vcmpeqps ymm6, ymm1, ymm4
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm1
       vandps   ymm6, ymm10, ymm6
       vcmpneqps ymm10, ymm1, ymm1
       vorps    ymm6, ymm10, ymm6
       vcmpltps ymm10, ymm1, ymm4
       vorps    ymm6, ymm10, ymm6
       vblendvps ymm1, ymm4, ymm1, ymm6
       vcmpeqps ymm4, ymm3, ymm5
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm3
       vandps   ymm4, ymm6, ymm4
       vcmpneqps ymm6, ymm3, ymm3
       vorps    ymm4, ymm6, ymm4
       vcmpltps ymm6, ymm3, ymm5

G_M000_IG12:                ;; offset=0x07D9
       vorps    ymm4, ymm6, ymm4
       vblendvps ymm3, ymm5, ymm3, ymm4
       vcmpeqps ymm4, ymm2, ymm9
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm2
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm2, ymm2
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm2, ymm9
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm2, ymm9, ymm2, ymm4
       vcmpeqps ymm4, ymm0, ymm7
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm7
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm0, ymm0
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm7, ymm0
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm4, ymm7, ymm0, ymm4
       vcmpeqps ymm5, ymm1, ymm4
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm4
       vandps   ymm5, ymm6, ymm5
       vcmpneqps ymm6, ymm1, ymm1
       vorps    ymm5, ymm6, ymm5
       vcmpltps ymm6, ymm4, ymm1
       vorps    ymm5, ymm6, ymm5
       vblendvps ymm5, ymm4, ymm1, ymm5
       vcmpeqps ymm6, ymm3, ymm5
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm5
       vandps   ymm6, ymm9, ymm6
       vcmpneqps ymm9, ymm3, ymm3
       vorps    ymm6, ymm9, ymm6
       vcmpltps ymm9, ymm5, ymm3
       vorps    ymm6, ymm9, ymm6
       vblendvps ymm6, ymm5, ymm3, ymm6
       vcmpeqps ymm9, ymm0, ymm7
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm0
       vandps   ymm9, ymm10, ymm9
       vcmpneqps ymm10, ymm0, ymm0
       vorps    ymm9, ymm10, ymm9
       vcmpltps ymm10, ymm0, ymm7
       vorps    ymm9, ymm10, ymm9
       vblendvps ymm0, ymm7, ymm0, ymm9
       vcmpeqps ymm7, ymm1, ymm4
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm1
       vandps   ymm7, ymm9, ymm7
       vcmpneqps ymm9, ymm1, ymm1
       vorps    ymm7, ymm9, ymm7
       vcmpltps ymm9, ymm1, ymm4
       vorps    ymm7, ymm9, ymm7
       vblendvps ymm1, ymm4, ymm1, ymm7
       vcmpeqps ymm4, ymm3, ymm5
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm3

G_M000_IG13:                ;; offset=0x08EE
       vandps   ymm4, ymm7, ymm4
       vcmpneqps ymm7, ymm3, ymm3
       vorps    ymm4, ymm7, ymm4
       vcmpltps ymm7, ymm3, ymm5
       vorps    ymm4, ymm7, ymm4
       vblendvps ymm3, ymm5, ymm3, ymm4
       vcmpeqps ymm4, ymm2, ymm6
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm2
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm2, ymm2
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm2, ymm6
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm2, ymm6, ymm2, ymm4
       vcmpeqps ymm4, ymm0, ymm8
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm8
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm0, ymm0
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm8, ymm0
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm4, ymm8, ymm0, ymm4
       vcmpeqps ymm5, ymm1, ymm4
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm4
       vandps   ymm5, ymm6, ymm5
       vcmpneqps ymm6, ymm1, ymm1
       vorps    ymm5, ymm6, ymm5
       vcmpltps ymm6, ymm4, ymm1
       vorps    ymm5, ymm6, ymm5
       vblendvps ymm5, ymm4, ymm1, ymm5
       vcmpeqps ymm6, ymm3, ymm5
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm5
       vandps   ymm6, ymm7, ymm6
       vcmpneqps ymm7, ymm3, ymm3
       vorps    ymm6, ymm7, ymm6
       vcmpltps ymm7, ymm5, ymm3
       vorps    ymm6, ymm7, ymm6
       vblendvps ymm6, ymm5, ymm3, ymm6
       vcmpeqps ymm7, ymm0, ymm8
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm0
       vandps   ymm7, ymm9, ymm7
       vcmpneqps ymm9, ymm0, ymm0
       vorps    ymm7, ymm9, ymm7
       vcmpltps ymm9, ymm0, ymm8
       vorps    ymm7, ymm9, ymm7
       vblendvps ymm0, ymm8, ymm0, ymm7
       vcmpeqps ymm8, ymm1, ymm4
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm1
       vandps   ymm7, ymm8, ymm7
       vcmpneqps ymm8, ymm1, ymm1
       vorps    ymm7, ymm8, ymm7
       vcmpltps ymm8, ymm1, ymm4
       vorps    ymm7, ymm8, ymm7

G_M000_IG14:                ;; offset=0x09FF
       vblendvps ymm1, ymm4, ymm1, ymm7
       vcmpeqps ymm4, ymm3, ymm5
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm3
       vandps   ymm4, ymm7, ymm4
       vcmpneqps ymm7, ymm3, ymm3
       vorps    ymm4, ymm7, ymm4
       vcmpltps ymm7, ymm3, ymm5
       vorps    ymm4, ymm7, ymm4
       vblendvps ymm3, ymm5, ymm3, ymm4
       vcmpeqps ymm4, ymm2, ymm6
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm2
       vandps   ymm4, ymm5, ymm4
       vcmpneqps ymm5, ymm2, ymm2
       vorps    ymm4, ymm5, ymm4
       vcmpltps ymm5, ymm2, ymm6
       vorps    ymm4, ymm5, ymm4
       vblendvps ymm2, ymm6, ymm2, ymm4
       vmovaps  ymm5, ymm3
       jmp      G_M000_IG22

G_M000_IG15:                ;; offset=0x0A60
       vcmpeqps ymm9, ymm0, ymm3
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm3
       vandps   ymm10, ymm10, ymm9
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm10, ymm10, ymm11
       vcmpltps ymm12, ymm3, ymm0
       vorps    ymm10, ymm12, ymm10
       vandps   ymm12, ymm10, ymm0
       vandnps  ymm10, ymm10, ymm3
       vorps    ymm10, ymm10, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm0
       vandps   ymm9, ymm12, ymm9
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm11, ymm0, ymm3
       vorps    ymm9, ymm11, ymm9
       vandps   ymm0, ymm9, ymm0
       vandnps  ymm3, ymm9, ymm3
       vorps    ymm0, ymm3, ymm0
       vcmpeqps ymm3, ymm1, ymm7
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm7
       vandps   ymm9, ymm9, ymm3
       vcmpneqps ymm11, ymm1, ymm1
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm12, ymm7, ymm1
       vorps    ymm9, ymm12, ymm9
       vandps   ymm12, ymm9, ymm1
       vandnps  ymm9, ymm9, ymm7
       vorps    ymm9, ymm9, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm1
       vandps   ymm3, ymm12, ymm3
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm11, ymm1, ymm7
       vorps    ymm3, ymm11, ymm3
       vandps   ymm1, ymm1, ymm3
       vandnps  ymm3, ymm3, ymm7
       vorps    ymm1, ymm3, ymm1
       vcmpeqps ymm3, ymm2, ymm5
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm5
       vandps   ymm7, ymm7, ymm3
       vcmpneqps ymm11, ymm2, ymm2
       vorps    ymm7, ymm11, ymm7
       vcmpltps ymm12, ymm5, ymm2
       vorps    ymm7, ymm12, ymm7
       vandps   ymm12, ymm2, ymm7
       vandnps  ymm7, ymm7, ymm5
       vorps    ymm7, ymm12, ymm7
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm2
       vandps   ymm3, ymm12, ymm3
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm11, ymm2, ymm5
       vorps    ymm3, ymm11, ymm3
       vandps   ymm2, ymm2, ymm3
       vandnps  ymm3, ymm3, ymm5

G_M000_IG16:                ;; offset=0x0B67
       vorps    ymm2, ymm3, ymm2
       vcmpeqps ymm3, ymm4, ymm8
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm8
       vandps   ymm5, ymm5, ymm3
       vcmpneqps ymm11, ymm4, ymm4
       vorps    ymm5, ymm11, ymm5
       vcmpltps ymm12, ymm8, ymm4
       vorps    ymm5, ymm12, ymm5
       vandps   ymm12, ymm4, ymm5
       vandnps  ymm5, ymm5, ymm8
       vorps    ymm5, ymm12, ymm5
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm4
       vandps   ymm3, ymm12, ymm3
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm11, ymm4, ymm8
       vorps    ymm3, ymm11, ymm3
       vandps   ymm4, ymm4, ymm3
       vandnps  ymm3, ymm3, ymm8
       vorps    ymm3, ymm3, ymm4
       vcmpeqps ymm4, ymm0, ymm9
       vxorps   ymm8, ymm8, ymm8
       vpcmpgtd ymm8, ymm8, ymm9
       vandps   ymm8, ymm8, ymm4
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm8, ymm8, ymm11
       vcmpltps ymm12, ymm9, ymm0
       vorps    ymm8, ymm12, ymm8
       vandps   ymm12, ymm8, ymm0
       vandnps  ymm8, ymm8, ymm9
       vorps    ymm8, ymm8, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm0
       vandps   ymm4, ymm12, ymm4
       vorps    ymm4, ymm11, ymm4
       vcmpltps ymm11, ymm0, ymm9
       vorps    ymm4, ymm11, ymm4
       vandps   ymm0, ymm0, ymm4
       vandnps  ymm4, ymm4, ymm9
       vorps    ymm0, ymm4, ymm0
       vcmpeqps ymm4, ymm2, ymm3
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm3
       vandps   ymm9, ymm9, ymm4
       vcmpneqps ymm11, ymm2, ymm2
       vorps    ymm9, ymm9, ymm11
       vcmpltps ymm12, ymm3, ymm2
       vorps    ymm9, ymm12, ymm9
       vandps   ymm12, ymm9, ymm2
       vandnps  ymm9, ymm9, ymm3
       vorps    ymm9, ymm9, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm2
       vandps   ymm4, ymm12, ymm4
       vorps    ymm4, ymm11, ymm4
       vcmpltps ymm11, ymm2, ymm3
       vorps    ymm4, ymm11, ymm4
       vandps   ymm2, ymm2, ymm4

G_M000_IG17:                ;; offset=0x0C74
       vandnps  ymm3, ymm4, ymm3
       vorps    ymm2, ymm3, ymm2
       vcmpeqps ymm3, ymm10, ymm5
       vxorps   ymm4, ymm4, ymm4
       vpcmpgtd ymm4, ymm4, ymm5
       vandps   ymm4, ymm4, ymm3
       vcmpneqps ymm11, ymm10, ymm10
       vorps    ymm4, ymm11, ymm4
       vcmpltps ymm12, ymm5, ymm10
       vorps    ymm4, ymm12, ymm4
       vandps   ymm12, ymm10, ymm4
       vandnps  ymm4, ymm4, ymm5
       vorps    ymm4, ymm12, ymm4
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm10
       vandps   ymm3, ymm12, ymm3
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm11, ymm10, ymm5
       vorps    ymm3, ymm11, ymm3
       vandps   ymm10, ymm10, ymm3
       vandnps  ymm3, ymm3, ymm5
       vorps    ymm3, ymm10, ymm3
       vcmpeqps ymm5, ymm6, ymm7
       vxorps   ymm10, ymm10, ymm10
       vpcmpgtd ymm10, ymm10, ymm6
       vandps   ymm10, ymm10, ymm5
       vcmpneqps ymm11, ymm7, ymm7
       vorps    ymm10, ymm10, ymm11
       vcmpltps ymm12, ymm6, ymm7
       vorps    ymm10, ymm12, ymm10
       vandps   ymm12, ymm10, ymm7
       vandnps  ymm10, ymm10, ymm6
       vorps    ymm10, ymm10, ymm12
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm7
       vandps   ymm5, ymm12, ymm5
       vorps    ymm5, ymm11, ymm5
       vcmpltps ymm11, ymm7, ymm6
       vorps    ymm5, ymm11, ymm5
       vandps   ymm7, ymm7, ymm5
       vandnps  ymm5, ymm5, ymm6
       vorps    ymm5, ymm5, ymm7
       vcmpeqps ymm6, ymm0, ymm2
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm2
       vandps   ymm7, ymm7, ymm6
       vcmpneqps ymm11, ymm0, ymm0
       vorps    ymm7, ymm11, ymm7
       vcmpltps ymm12, ymm2, ymm0
       vorps    ymm7, ymm12, ymm7
       vandps   ymm12, ymm0, ymm7
       vandnps  ymm7, ymm7, ymm2
       vorps    ymm7, ymm12, ymm7
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm0
       vandps   ymm6, ymm12, ymm6
       vorps    ymm6, ymm11, ymm6
       vcmpltps ymm11, ymm0, ymm2
       vorps    ymm6, ymm11, ymm6

G_M000_IG18:                ;; offset=0x0D76
       vandps   ymm0, ymm0, ymm6
       vandnps  ymm2, ymm6, ymm2
       vorps    ymm0, ymm2, ymm0
       vcmpeqps ymm2, ymm1, ymm3
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm3
       vandps   ymm6, ymm6, ymm2
       vcmpneqps ymm11, ymm1, ymm1
       vorps    ymm6, ymm11, ymm6
       vcmpltps ymm12, ymm3, ymm1
       vorps    ymm6, ymm12, ymm6
       vandps   ymm12, ymm1, ymm6
       vandnps  ymm6, ymm6, ymm3
       vorps    ymm6, ymm12, ymm6
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm1
       vandps   ymm2, ymm12, ymm2
       vorps    ymm2, ymm11, ymm2
       vcmpltps ymm11, ymm1, ymm3
       vorps    ymm2, ymm11, ymm2
       vandps   ymm1, ymm1, ymm2
       vandnps  ymm3, ymm2, ymm3
       vorps    ymm1, ymm3, ymm1
       vcmpeqps ymm2, ymm5, ymm9
       vxorps   ymm3, ymm3, ymm3
       vpcmpgtd ymm3, ymm3, ymm5
       vandps   ymm3, ymm3, ymm2
       vcmpneqps ymm11, ymm9, ymm9
       vorps    ymm3, ymm11, ymm3
       vcmpltps ymm12, ymm5, ymm9
       vorps    ymm3, ymm12, ymm3
       vandps   ymm12, ymm9, ymm3
       vandnps  ymm3, ymm3, ymm5
       vorps    ymm3, ymm12, ymm3
       vxorps   ymm12, ymm12, ymm12
       vpcmpgtd ymm12, ymm12, ymm9
       vandps   ymm2, ymm12, ymm2
       vorps    ymm2, ymm11, ymm2
       vcmpltps ymm11, ymm9, ymm5
       vorps    ymm2, ymm11, ymm2
       vandps   ymm9, ymm9, ymm2
       vandnps  ymm5, ymm2, ymm5
       vorps    ymm2, ymm9, ymm5
       vcmpeqps ymm5, ymm8, ymm4
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm8
       vandps   ymm5, ymm9, ymm5
       vcmpneqps ymm9, ymm8, ymm8
       vorps    ymm5, ymm9, ymm5
       vcmpltps ymm9, ymm8, ymm4
       vorps    ymm5, ymm9, ymm5
       vblendvps ymm4, ymm4, ymm8, ymm5
       vcmpeqps ymm5, ymm1, ymm2
       vxorps   ymm8, ymm8, ymm8
       vpcmpgtd ymm8, ymm8, ymm2
       vandps   ymm8, ymm8, ymm5
       vcmpneqps ymm9, ymm1, ymm1
       vorps    ymm8, ymm8, ymm9
       vcmpltps ymm11, ymm2, ymm1

G_M000_IG19:                ;; offset=0x0E7D
       vorps    ymm8, ymm11, ymm8
       vandps   ymm11, ymm8, ymm1
       vandnps  ymm8, ymm8, ymm2
       vorps    ymm8, ymm8, ymm11
       vxorps   ymm11, ymm11, ymm11
       vpcmpgtd ymm11, ymm11, ymm1
       vandps   ymm5, ymm11, ymm5
       vorps    ymm5, ymm9, ymm5
       vcmpltps ymm9, ymm1, ymm2
       vorps    ymm5, ymm9, ymm5
       vandps   ymm1, ymm1, ymm5
       vandnps  ymm2, ymm5, ymm2
       vorps    ymm1, ymm2, ymm1
       vcmpeqps ymm2, ymm6, ymm10
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm6
       vandps   ymm2, ymm5, ymm2
       vcmpneqps ymm5, ymm6, ymm6
       vorps    ymm2, ymm5, ymm2
       vcmpltps ymm5, ymm6, ymm10
       vorps    ymm2, ymm5, ymm2
       vblendvps ymm2, ymm10, ymm6, ymm2
       vcmpeqps ymm5, ymm4, ymm3
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm3
       vandps   ymm5, ymm6, ymm5
       vcmpneqps ymm6, ymm3, ymm3
       vorps    ymm5, ymm6, ymm5
       vcmpltps ymm6, ymm3, ymm4
       vorps    ymm5, ymm6, ymm5
       vblendvps ymm5, ymm4, ymm3, ymm5
       vcmpeqps ymm3, ymm0, ymm1
       vxorps   ymm4, ymm4, ymm4
       vpcmpgtd ymm4, ymm4, ymm1
       vandps   ymm4, ymm4, ymm3
       vcmpneqps ymm6, ymm0, ymm0
       vorps    ymm4, ymm4, ymm6
       vcmpltps ymm9, ymm1, ymm0
       vorps    ymm4, ymm9, ymm4
       vandps   ymm9, ymm0, ymm4
       vandnps  ymm4, ymm4, ymm1
       vorps    ymm4, ymm9, ymm4
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm0
       vandps   ymm3, ymm9, ymm3
       vorps    ymm3, ymm3, ymm6
       vcmpltps ymm6, ymm0, ymm1
       vorps    ymm3, ymm6, ymm3
       vandps   ymm0, ymm0, ymm3
       vandnps  ymm1, ymm3, ymm1
       vorps    ymm0, ymm1, ymm0
       vcmpeqps ymm1, ymm7, ymm8
       vxorps   ymm3, ymm3, ymm3
       vpcmpgtd ymm3, ymm3, ymm8
       vandps   ymm3, ymm3, ymm1
       vcmpneqps ymm6, ymm7, ymm7
       vorps    ymm3, ymm3, ymm6
       vcmpltps ymm9, ymm8, ymm7
       vorps    ymm3, ymm9, ymm3

G_M000_IG20:                ;; offset=0x0F83
       vandps   ymm9, ymm7, ymm3
       vandnps  ymm3, ymm3, ymm8
       vorps    ymm3, ymm9, ymm3
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm7
       vandps   ymm1, ymm9, ymm1
       vorps    ymm1, ymm1, ymm6
       vcmpltps ymm6, ymm7, ymm8
       vorps    ymm1, ymm6, ymm1
       vandps   ymm6, ymm7, ymm1
       vandnps  ymm1, ymm1, ymm8
       vorps    ymm1, ymm1, ymm6
       vcmpeqps ymm6, ymm2, ymm5
       vxorps   ymm7, ymm7, ymm7
       vpcmpgtd ymm7, ymm7, ymm5
       vandps   ymm7, ymm7, ymm6
       vcmpneqps ymm8, ymm2, ymm2
       vorps    ymm7, ymm8, ymm7
       vcmpltps ymm9, ymm5, ymm2
       vorps    ymm7, ymm9, ymm7
       vandps   ymm9, ymm2, ymm7
       vandnps  ymm7, ymm7, ymm5
       vorps    ymm7, ymm9, ymm7
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm2
       vandps   ymm6, ymm9, ymm6
       vorps    ymm6, ymm8, ymm6
       vcmpltps ymm8, ymm2, ymm5
       vorps    ymm6, ymm8, ymm6
       vandps   ymm2, ymm2, ymm6
       vandnps  ymm5, ymm6, ymm5
       vorps    ymm2, ymm5, ymm2
       vcmpeqps ymm5, ymm1, ymm2
       vxorps   ymm6, ymm6, ymm6
       vpcmpgtd ymm6, ymm6, ymm2
       vandps   ymm6, ymm6, ymm5
       vcmpneqps ymm8, ymm1, ymm1
       vorps    ymm6, ymm8, ymm6
       vcmpltps ymm9, ymm2, ymm1
       vorps    ymm6, ymm9, ymm6
       vandps   ymm9, ymm1, ymm6
       vandnps  ymm6, ymm6, ymm2
       vorps    ymm6, ymm9, ymm6
       vxorps   ymm9, ymm9, ymm9
       vpcmpgtd ymm9, ymm9, ymm1
       vandps   ymm5, ymm9, ymm5
       vorps    ymm5, ymm8, ymm5
       vcmpltps ymm8, ymm1, ymm2
       vorps    ymm5, ymm8, ymm5
       vandps   ymm1, ymm1, ymm5
       vandnps  ymm2, ymm5, ymm2
       vorps    ymm1, ymm2, ymm1
       vcmpeqps ymm2, ymm3, ymm7
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm3
       vandps   ymm2, ymm5, ymm2
       vcmpneqps ymm5, ymm3, ymm3
       vorps    ymm2, ymm5, ymm2
       vcmpltps ymm5, ymm3, ymm7

G_M000_IG21:                ;; offset=0x1081
       vorps    ymm2, ymm5, ymm2
       vblendvps ymm2, ymm7, ymm3, ymm2
       vcmpeqps ymm3, ymm1, ymm4
       vxorps   ymm5, ymm5, ymm5
       vpcmpgtd ymm5, ymm5, ymm1
       vandps   ymm5, ymm5, ymm3
       vcmpneqps ymm7, ymm4, ymm4
       vorps    ymm5, ymm5, ymm7
       vcmpltps ymm8, ymm1, ymm4
       vorps    ymm5, ymm8, ymm5
       vandps   ymm8, ymm4, ymm5
       vandnps  ymm5, ymm5, ymm1
       vorps    ymm5, ymm8, ymm5
       vxorps   ymm8, ymm8, ymm8
       vpcmpgtd ymm8, ymm8, ymm4
       vandps   ymm3, ymm8, ymm3
       vorps    ymm3, ymm3, ymm7
       vcmpltps ymm7, ymm4, ymm1
       vorps    ymm3, ymm7, ymm3
       vandps   ymm4, ymm4, ymm3
       vandnps  ymm1, ymm3, ymm1
       vorps    ymm1, ymm1, ymm4
       vcmpeqps ymm3, ymm2, ymm6
       vxorps   ymm4, ymm4, ymm4
       vpcmpgtd ymm4, ymm4, ymm6
       vandps   ymm3, ymm4, ymm3
       vcmpneqps ymm4, ymm6, ymm6
       vorps    ymm3, ymm4, ymm3
       vcmpltps ymm4, ymm6, ymm2
       vorps    ymm3, ymm4, ymm3
       vblendvps ymm2, ymm2, ymm6, ymm3

G_M000_IG22:                ;; offset=0x1109
       vmulps   ymm0, ymm0, ymmword ptr [reloc @RWD00]
       vmulps   ymm1, ymm1, ymmword ptr [reloc @RWD32]
       vaddps   ymm0, ymm1, ymm0
       vmulps   ymm1, ymm5, ymmword ptr [reloc @RWD64]
       vmulps   ymm2, ymm2, ymmword ptr [reloc @RWD96]
       vaddps   ymm1, ymm2, ymm1
       vaddps   ymm0, ymm1, ymm0
       mov      rdx, gword ptr [rbx+0x28]
       lea      r8d, [rdi+rbp]
       cmp      dword ptr [rdx+0x08], r8d
       jbe      G_M000_IG27
       mov      r9d, dword ptr [rdx+0x08]
       sub      r9d, r8d
       cmp      r9d, 8
       jl       G_M000_IG28
       vmovups  ymmword ptr [rdx+4*r8+0x10], ymm0
       add      ebp, 8
       mov      edx, dword ptr [rbx+0x58]
       add      edx, -8
       cmp      ebp, edx
       jl       G_M000_IG04

G_M000_IG23:                ;; offset=0x1170
       cmp      ebp, dword ptr [rbx+0x58]
       jge      SHORT G_M000_IG25

G_M000_IG24:                ;; offset=0x1175
       mov      rcx, rbx
       mov      edx, esi
       mov      r8d, edi
       mov      r9d, ebp
       call     [DotJpegli.Internal.AdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this]
       inc      ebp
       cmp      ebp, dword ptr [rbx+0x58]
       jl       SHORT G_M000_IG24

G_M000_IG25:                ;; offset=0x118D
       vzeroupper
       vmovaps  xmm6, xmmword ptr [rsp+0x150]
       vmovaps  xmm7, xmmword ptr [rsp+0x140]
       vmovaps  xmm8, xmmword ptr [rsp+0x130]
       vmovaps  xmm9, xmmword ptr [rsp+0x120]
       vmovaps  xmm10, xmmword ptr [rsp+0x110]
       vmovaps  xmm11, xmmword ptr [rsp+0x100]
       vmovaps  xmm12, xmmword ptr [rsp+0xF0]
       vmovaps  xmm13, xmmword ptr [rsp+0xE0]
       vmovaps  xmm14, xmmword ptr [rsp+0xD0]
       vmovaps  xmm15, xmmword ptr [rsp+0xC0]
       add      rsp, 360
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r14
       pop      r15
       ret

G_M000_IG26:                ;; offset=0x11FA
       call     [System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()]
       int3

G_M000_IG27:                ;; offset=0x1201
       call     [System.ThrowHelper:ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_IndexMustBeLess()]
       int3

G_M000_IG28:                ;; offset=0x1208
       call     [System.ThrowHelper:ThrowArgumentException_DestinationTooShort()]
       int3

G_M000_IG29:                ;; offset=0x120F
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dq	3E0000003E000000h, 3E0000003E000000h, 3E0000003E000000h, 3E0000003E000000h
RWD32  	dq	3D99999A3D99999Ah, 3D99999A3D99999Ah, 3D99999A3D99999Ah, 3D99999A3D99999Ah
RWD64  	dq	3D75C28F3D75C28Fh, 3D75C28F3D75C28Fh, 3D75C28F3D75C28Fh, 3D75C28F3D75C28Fh
RWD96  	dq	3D4CCCCD3D4CCCCDh, 3D4CCCCD3D4CCCCDh, 3D4CCCCD3D4CCCCDh, 3D4CCCCD3D4CCCCDh

; Total bytes of code 4629

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ComputeFuzzyErosionScalar(int,int,int):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data
; 2 inlinees with PGO data; 29 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r14
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 176
       vmovaps  xmmword ptr [rsp+0xA0], xmm6
       vmovaps  xmmword ptr [rsp+0x90], xmm7
       vmovaps  xmmword ptr [rsp+0x80], xmm8
       vmovaps  xmmword ptr [rsp+0x70], xmm9
       vmovaps  xmmword ptr [rsp+0x60], xmm10
       vmovaps  xmmword ptr [rsp+0x50], xmm11
       vmovaps  xmmword ptr [rsp+0x40], xmm12
       vmovaps  xmmword ptr [rsp+0x30], xmm13
       vmovaps  xmmword ptr [rsp+0x20], xmm14

G_M000_IG02:                ;; offset=0x004C
       lea      eax, [r9-0x01]
       xor      r10d, r10d
       test     eax, eax
       cmovl    eax, r10d
       lea      r10d, [r9+0x01]
       mov      r11d, dword ptr [rcx+0x58]
       lea      ebx, [r11-0x01]
       cmp      r10d, ebx
       cmovg    r10d, ebx
       lea      ebx, [rdx-0x01]
       imul     ebx, r11d
       mov      esi, edx
       imul     esi, r11d
       inc      edx
       imul     edx, r11d
       mov      r11, gword ptr [rcx+0x20]
       mov      rdi, r11
       lea      ebp, [rbx+rax]
       mov      r14d, dword ptr [rdi+0x08]
       cmp      ebp, r14d
       jae      G_M000_IG22
       lea      ebp, [rbx+rax]
       vmovss   xmm0, dword ptr [rdi+4*rbp+0x10]
       mov      rdi, r11
       lea      ebp, [rbx+r9]
       cmp      ebp, r14d
       jae      G_M000_IG22
       lea      ebp, [rbx+r9]
       vmovss   xmm1, dword ptr [rdi+4*rbp+0x10]
       mov      rdi, r11
       lea      ebp, [rbx+r10]
       cmp      ebp, r14d
       jae      G_M000_IG22
       add      ebx, r10d
       vmovss   xmm2, dword ptr [rdi+4*rbx+0x10]
       mov      rbx, r11
       lea      edi, [rsi+rax]
       cmp      edi, r14d
       jae      G_M000_IG22
       lea      edi, [rsi+rax]
       vmovss   xmm3, dword ptr [rbx+4*rdi+0x10]
       mov      rbx, r11
       lea      edi, [rsi+r9]
       cmp      edi, r14d
       jae      G_M000_IG22
       lea      edi, [rsi+r9]
       vmovss   xmm4, dword ptr [rbx+4*rdi+0x10]
       mov      rbx, r11
       lea      edi, [rsi+r10]
       cmp      edi, r14d
       jae      G_M000_IG22
       add      esi, r10d
       vmovss   xmm5, dword ptr [rbx+4*rsi+0x10]
       mov      rbx, r11
       lea      esi, [rdx+rax]
       cmp      esi, r14d
       jae      G_M000_IG22
       add      eax, edx

G_M000_IG03:                ;; offset=0x012E
       vmovss   xmm6, dword ptr [rbx+4*rax+0x10]
       mov      rax, r11
       lea      ebx, [rdx+r9]
       cmp      ebx, r14d
       jae      G_M000_IG22
       lea      ebx, [rdx+r9]
       vmovss   xmm7, dword ptr [rax+4*rbx+0x10]
       lea      eax, [rdx+r10]
       cmp      eax, r14d
       jae      G_M000_IG22
       add      edx, r10d
       mov      eax, edx
       vmovss   xmm8, dword ptr [r11+4*rax+0x10]
       cmp      byte  ptr [rcx+0x68], 0
       jne      G_M000_IG12

G_M000_IG04:                ;; offset=0x0171
       vmovaps  xmm9, xmm0
       vmovaps  xmm10, xmm1
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm9
       vorps    xmm11, xmm13, xmm11
       vandps   xmm9, xmm9, xmm11
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm9, xmm10, xmm9
       vcmpeqps xmm10, xmm0, xmm1
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10
       vorps    xmm12, xmm10, xmm12
       vcmpltps xmm10, xmm0, xmm1
       vorps    xmm10, xmm10, xmm12
       vandps   xmm0, xmm10, xmm0
       vandnps  xmm1, xmm10, xmm1
       vorps    xmm0, xmm1, xmm0
       vmovaps  xmm1, xmm2
       vmovaps  xmm10, xmm3
       vcmpeqps xmm11, xmm1, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm1, xmm1
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm1
       vorps    xmm11, xmm13, xmm11
       vandps   xmm1, xmm11, xmm1
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm1, xmm10, xmm1
       vcmpeqps xmm10, xmm2, xmm3
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm2
       vandps   xmm10, xmm11, xmm10
       vorps    xmm12, xmm10, xmm12
       vcmpltps xmm10, xmm2, xmm3
       vorps    xmm10, xmm10, xmm12
       vandps   xmm2, xmm10, xmm2
       vandnps  xmm3, xmm10, xmm3
       vorps    xmm2, xmm3, xmm2
       vmovaps  xmm3, xmm0
       vmovaps  xmm10, xmm2
       vcmpeqps xmm11, xmm3, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm3, xmm3
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm3
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm3, xmm10, xmm3, xmm11
       vcmpeqps xmm10, xmm0, xmm2
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10

G_M000_IG05:                ;; offset=0x0297
       vcmpneqps xmm11, xmm0, xmm0
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm0, xmm2
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm0, xmm2, xmm0, xmm10
       vmovaps  xmm2, xmm9
       vmovaps  xmm10, xmm1
       vcmpeqps xmm11, xmm2, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm2, xmm2
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm2
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm2, xmm10, xmm2, xmm11
       vcmpeqps xmm10, xmm9, xmm1
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm9
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm9, xmm9
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm9, xmm1
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm1, xmm1, xmm9, xmm10
       vmovaps  xmm9, xmm1
       vmovaps  xmm10, xmm3
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm9
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm9, xmm10, xmm9, xmm11
       vcmpeqps xmm10, xmm1, xmm3
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm1
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm1, xmm1
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm1, xmm3
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm1, xmm3, xmm1, xmm10
       vmovaps  xmm3, xmm0
       vmovaps  xmm10, xmm4
       vcmpeqps xmm11, xmm3, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm3, xmm3
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm3
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm3, xmm10, xmm3, xmm11
       vmovaps  xmm10, xmm1
       vmovaps  xmm11, xmm3
       vcmpeqps xmm12, xmm10, xmm11
       vxorps   xmm13, xmm13, xmm13
       vpcmpgtd xmm13, xmm13, xmm11

G_M000_IG06:                ;; offset=0x03CD
       vandps   xmm12, xmm13, xmm12
       vcmpneqps xmm13, xmm10, xmm10
       vorps    xmm12, xmm13, xmm12
       vcmpltps xmm13, xmm11, xmm10
       vorps    xmm12, xmm13, xmm12
       vblendvps xmm10, xmm11, xmm10, xmm12
       vmovaps  xmm11, xmm9
       vmovaps  xmm12, xmm10
       vcmpeqps xmm13, xmm11, xmm12
       vxorps   xmm14, xmm14, xmm14
       vpcmpgtd xmm14, xmm14, xmm12
       vandps   xmm13, xmm14, xmm13
       vcmpneqps xmm14, xmm11, xmm11
       vorps    xmm13, xmm14, xmm13
       vcmpltps xmm14, xmm12, xmm11
       vorps    xmm13, xmm14, xmm13
       vblendvps xmm11, xmm12, xmm11, xmm13
       vcmpeqps xmm12, xmm0, xmm4
       vxorps   xmm13, xmm13, xmm13
       vpcmpgtd xmm13, xmm13, xmm0
       vandps   xmm12, xmm13, xmm12
       vcmpneqps xmm13, xmm0, xmm0
       vorps    xmm12, xmm13, xmm12
       vcmpltps xmm13, xmm0, xmm4
       vorps    xmm12, xmm13, xmm12
       vblendvps xmm0, xmm4, xmm0, xmm12
       vcmpeqps xmm4, xmm1, xmm3
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm1
       vandps   xmm4, xmm12, xmm4
       vcmpneqps xmm12, xmm1, xmm1
       vorps    xmm4, xmm12, xmm4
       vcmpltps xmm12, xmm1, xmm3
       vorps    xmm4, xmm12, xmm4
       vblendvps xmm1, xmm3, xmm1, xmm4
       vcmpeqps xmm3, xmm9, xmm10
       vxorps   xmm4, xmm4, xmm4
       vpcmpgtd xmm4, xmm4, xmm9
       vandps   xmm3, xmm4, xmm3
       vcmpneqps xmm4, xmm9, xmm9
       vorps    xmm3, xmm4, xmm3
       vcmpltps xmm4, xmm9, xmm10
       vorps    xmm3, xmm4, xmm3
       vblendvps xmm3, xmm10, xmm9, xmm3
       vcmpeqps xmm4, xmm2, xmm11
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm2
       vandps   xmm4, xmm9, xmm4
       vcmpneqps xmm9, xmm2, xmm2
       vorps    xmm4, xmm9, xmm4
       vcmpltps xmm9, xmm2, xmm11
       vorps    xmm4, xmm9, xmm4
       vblendvps xmm2, xmm11, xmm2, xmm4
       vmovaps  xmm4, xmm0
       vmovaps  xmm9, xmm5
       vcmpeqps xmm10, xmm4, xmm9
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm9
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm4, xmm4

G_M000_IG07:                ;; offset=0x04FB
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm9, xmm4
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm4, xmm9, xmm4, xmm10
       vmovaps  xmm9, xmm1
       vmovaps  xmm10, xmm4
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm9
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm9, xmm10, xmm9, xmm11
       vmovaps  xmm10, xmm3
       vmovaps  xmm11, xmm9
       vcmpeqps xmm12, xmm10, xmm11
       vxorps   xmm13, xmm13, xmm13
       vpcmpgtd xmm13, xmm13, xmm11
       vandps   xmm12, xmm13, xmm12
       vcmpneqps xmm13, xmm10, xmm10
       vorps    xmm12, xmm13, xmm12
       vcmpltps xmm13, xmm11, xmm10
       vorps    xmm12, xmm13, xmm12
       vblendvps xmm10, xmm11, xmm10, xmm12
       vcmpeqps xmm11, xmm0, xmm5
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm0
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm0, xmm0
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm0, xmm5
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm0, xmm5, xmm0, xmm11
       vcmpeqps xmm5, xmm1, xmm4
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm1
       vandps   xmm5, xmm11, xmm5
       vcmpneqps xmm11, xmm1, xmm1
       vorps    xmm5, xmm11, xmm5
       vcmpltps xmm11, xmm1, xmm4
       vorps    xmm5, xmm11, xmm5
       vblendvps xmm1, xmm4, xmm1, xmm5
       vcmpeqps xmm4, xmm3, xmm9
       vxorps   xmm5, xmm5, xmm5
       vpcmpgtd xmm5, xmm5, xmm3
       vandps   xmm4, xmm5, xmm4
       vcmpneqps xmm5, xmm3, xmm3
       vorps    xmm4, xmm5, xmm4
       vcmpltps xmm5, xmm3, xmm9
       vorps    xmm4, xmm5, xmm4
       vblendvps xmm3, xmm9, xmm3, xmm4
       vcmpeqps xmm4, xmm2, xmm10
       vxorps   xmm5, xmm5, xmm5
       vpcmpgtd xmm5, xmm5, xmm2
       vandps   xmm4, xmm5, xmm4
       vcmpneqps xmm5, xmm2, xmm2
       vorps    xmm4, xmm5, xmm4
       vcmpltps xmm5, xmm2, xmm10

G_M000_IG08:                ;; offset=0x0626
       vorps    xmm4, xmm5, xmm4
       vblendvps xmm2, xmm10, xmm2, xmm4
       vmovaps  xmm4, xmm0
       vmovaps  xmm5, xmm6
       vcmpeqps xmm9, xmm4, xmm5
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm5
       vandps   xmm9, xmm10, xmm9
       vcmpneqps xmm10, xmm4, xmm4
       vorps    xmm9, xmm10, xmm9
       vcmpltps xmm10, xmm5, xmm4
       vorps    xmm9, xmm10, xmm9
       vblendvps xmm4, xmm5, xmm4, xmm9
       vmovaps  xmm5, xmm1
       vmovaps  xmm9, xmm4
       vcmpeqps xmm10, xmm5, xmm9
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm9
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm5, xmm5
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm9, xmm5
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm5, xmm9, xmm5, xmm10
       vmovaps  xmm9, xmm3
       vmovaps  xmm10, xmm5
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm12, xmm11
       vcmpltps xmm12, xmm10, xmm9
       vorps    xmm11, xmm12, xmm11
       vblendvps xmm9, xmm10, xmm9, xmm11
       vcmpeqps xmm10, xmm0, xmm6
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm0, xmm0
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm0, xmm6
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm0, xmm6, xmm0, xmm10
       vcmpeqps xmm6, xmm1, xmm4
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm1
       vandps   xmm6, xmm10, xmm6
       vcmpneqps xmm10, xmm1, xmm1
       vorps    xmm6, xmm10, xmm6
       vcmpltps xmm10, xmm1, xmm4
       vorps    xmm6, xmm10, xmm6
       vblendvps xmm1, xmm4, xmm1, xmm6
       vcmpeqps xmm4, xmm3, xmm5
       vxorps   xmm6, xmm6, xmm6
       vpcmpgtd xmm6, xmm6, xmm3
       vandps   xmm4, xmm6, xmm4
       vcmpneqps xmm6, xmm3, xmm3
       vorps    xmm4, xmm6, xmm4
       vcmpltps xmm6, xmm3, xmm5
       vorps    xmm4, xmm6, xmm4

G_M000_IG09:                ;; offset=0x074F
       vblendvps xmm3, xmm5, xmm3, xmm4
       vcmpeqps xmm4, xmm2, xmm9
       vxorps   xmm5, xmm5, xmm5
       vpcmpgtd xmm5, xmm5, xmm2
       vandps   xmm4, xmm5, xmm4
       vcmpneqps xmm5, xmm2, xmm2
       vorps    xmm4, xmm5, xmm4
       vcmpltps xmm5, xmm2, xmm9
       vorps    xmm4, xmm5, xmm4
       vblendvps xmm2, xmm9, xmm2, xmm4
       vmovaps  xmm4, xmm0
       vmovaps  xmm5, xmm7
       vcmpeqps xmm6, xmm4, xmm5
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm5
       vandps   xmm6, xmm9, xmm6
       vcmpneqps xmm9, xmm4, xmm4
       vorps    xmm6, xmm9, xmm6
       vcmpltps xmm9, xmm5, xmm4
       vorps    xmm6, xmm9, xmm6
       vblendvps xmm4, xmm5, xmm4, xmm6
       vmovaps  xmm5, xmm1
       vmovaps  xmm6, xmm4
       vcmpeqps xmm9, xmm5, xmm6
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm6
       vandps   xmm9, xmm10, xmm9
       vcmpneqps xmm10, xmm5, xmm5
       vorps    xmm9, xmm10, xmm9
       vcmpltps xmm10, xmm6, xmm5
       vorps    xmm9, xmm10, xmm9
       vblendvps xmm5, xmm6, xmm5, xmm9
       vmovaps  xmm6, xmm3
       vmovaps  xmm9, xmm5
       vcmpeqps xmm10, xmm6, xmm9
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm9
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm6, xmm6
       vorps    xmm10, xmm11, xmm10
       vcmpltps xmm11, xmm9, xmm6
       vorps    xmm10, xmm11, xmm10
       vblendvps xmm6, xmm9, xmm6, xmm10
       vcmpeqps xmm9, xmm0, xmm7
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm0
       vandps   xmm9, xmm10, xmm9
       vcmpneqps xmm10, xmm0, xmm0
       vorps    xmm9, xmm10, xmm9
       vcmpltps xmm10, xmm0, xmm7
       vorps    xmm9, xmm10, xmm9
       vblendvps xmm0, xmm7, xmm0, xmm9
       vcmpeqps xmm7, xmm1, xmm4
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm1
       vandps   xmm7, xmm9, xmm7
       vcmpneqps xmm9, xmm1, xmm1
       vorps    xmm7, xmm9, xmm7
       vcmpltps xmm9, xmm1, xmm4
       vorps    xmm7, xmm9, xmm7
       vblendvps xmm1, xmm4, xmm1, xmm7

G_M000_IG10:                ;; offset=0x0875
       vcmpeqps xmm4, xmm3, xmm5
       vxorps   xmm7, xmm7, xmm7
       vpcmpgtd xmm7, xmm7, xmm3
       vandps   xmm4, xmm7, xmm4
       vcmpneqps xmm7, xmm3, xmm3
       vorps    xmm4, xmm7, xmm4
       vcmpltps xmm7, xmm3, xmm5
       vorps    xmm4, xmm7, xmm4
       vblendvps xmm3, xmm5, xmm3, xmm4
       vcmpeqps xmm4, xmm2, xmm6
       vxorps   xmm5, xmm5, xmm5
       vpcmpgtd xmm5, xmm5, xmm2
       vandps   xmm4, xmm5, xmm4
       vcmpneqps xmm5, xmm2, xmm2
       vorps    xmm4, xmm5, xmm4
       vcmpltps xmm5, xmm2, xmm6
       vorps    xmm4, xmm5, xmm4
       vblendvps xmm2, xmm6, xmm2, xmm4
       vmovaps  xmm4, xmm0
       vmovaps  xmm5, xmm8
       vcmpeqps xmm6, xmm4, xmm5
       vxorps   xmm7, xmm7, xmm7
       vpcmpgtd xmm7, xmm7, xmm5
       vandps   xmm6, xmm7, xmm6
       vcmpneqps xmm7, xmm4, xmm4
       vorps    xmm6, xmm7, xmm6
       vcmpltps xmm7, xmm5, xmm4
       vorps    xmm6, xmm7, xmm6
       vblendvps xmm4, xmm5, xmm4, xmm6
       vmovaps  xmm5, xmm1
       vmovaps  xmm6, xmm4
       vcmpeqps xmm7, xmm5, xmm6
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm6
       vandps   xmm7, xmm9, xmm7
       vcmpneqps xmm9, xmm5, xmm5
       vorps    xmm7, xmm9, xmm7
       vcmpltps xmm9, xmm6, xmm5
       vorps    xmm7, xmm9, xmm7
       vblendvps xmm5, xmm6, xmm5, xmm7
       vmovaps  xmm6, xmm3
       vmovaps  xmm7, xmm5
       vcmpeqps xmm9, xmm6, xmm7
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm7
       vandps   xmm9, xmm10, xmm9
       vcmpneqps xmm10, xmm6, xmm6
       vorps    xmm9, xmm10, xmm9
       vcmpltps xmm10, xmm7, xmm6
       vorps    xmm9, xmm10, xmm9
       vblendvps xmm6, xmm7, xmm6, xmm9
       vcmpeqps xmm7, xmm0, xmm8
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm0
       vandps   xmm7, xmm9, xmm7
       vcmpneqps xmm9, xmm0, xmm0
       vorps    xmm7, xmm9, xmm7
       vcmpltps xmm9, xmm0, xmm8
       vorps    xmm7, xmm9, xmm7
       vblendvps xmm0, xmm8, xmm0, xmm7
       vcmpeqps xmm7, xmm1, xmm4

G_M000_IG11:                ;; offset=0x0991
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm1
       vandps   xmm7, xmm8, xmm7
       vcmpneqps xmm8, xmm1, xmm1
       vorps    xmm7, xmm8, xmm7
       vcmpltps xmm8, xmm1, xmm4
       vorps    xmm7, xmm8, xmm7
       vblendvps xmm1, xmm4, xmm1, xmm7
       vcmpeqps xmm4, xmm3, xmm5
       vxorps   xmm7, xmm7, xmm7
       vpcmpgtd xmm7, xmm7, xmm3
       vandps   xmm4, xmm7, xmm4
       vcmpneqps xmm7, xmm3, xmm3
       vorps    xmm4, xmm7, xmm4
       vcmpltps xmm7, xmm3, xmm5
       vorps    xmm4, xmm7, xmm4
       vblendvps xmm3, xmm5, xmm3, xmm4
       vcmpeqps xmm4, xmm2, xmm6
       vxorps   xmm5, xmm5, xmm5
       vpcmpgtd xmm5, xmm5, xmm2
       vandps   xmm4, xmm5, xmm4
       vcmpneqps xmm5, xmm2, xmm2
       vorps    xmm4, xmm5, xmm4
       vcmpltps xmm5, xmm2, xmm6
       vorps    xmm4, xmm5, xmm4
       vblendvps xmm2, xmm6, xmm2, xmm4
       vmovaps  xmm4, xmm1
       vmovaps  xmm1, xmm2
       vmovaps  xmm2, xmm3
       jmp      G_M000_IG20

G_M000_IG12:                ;; offset=0x0A19
       vmovaps  xmm9, xmm0
       vmovaps  xmm10, xmm3
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm9
       vorps    xmm11, xmm13, xmm11
       vandps   xmm9, xmm9, xmm11
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm9, xmm10, xmm9
       vcmpeqps xmm10, xmm0, xmm3
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm0, xmm3
       vorps    xmm10, xmm11, xmm10
       vandps   xmm0, xmm10, xmm0
       vandnps  xmm3, xmm10, xmm3
       vorps    xmm0, xmm3, xmm0
       vmovaps  xmm3, xmm1
       vmovaps  xmm10, xmm7
       vcmpeqps xmm11, xmm3, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm3, xmm3
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm3
       vorps    xmm11, xmm13, xmm11
       vandps   xmm3, xmm11, xmm3
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm3, xmm10, xmm3
       vcmpeqps xmm10, xmm1, xmm7
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm1
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm1, xmm7
       vorps    xmm10, xmm11, xmm10
       vandps   xmm1, xmm10, xmm1
       vandnps  xmm7, xmm10, xmm7
       vorps    xmm1, xmm7, xmm1
       vmovaps  xmm7, xmm2
       vmovaps  xmm10, xmm5
       vcmpeqps xmm11, xmm7, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm7, xmm7
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm7
       vorps    xmm11, xmm13, xmm11
       vandps   xmm7, xmm11, xmm7
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm7, xmm10, xmm7
       vcmpeqps xmm10, xmm2, xmm5
       vxorps   xmm11, xmm11, xmm11

G_M000_IG13:                ;; offset=0x0B3D
       vpcmpgtd xmm11, xmm11, xmm2
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm2, xmm5
       vorps    xmm10, xmm11, xmm10
       vandps   xmm2, xmm10, xmm2
       vandnps  xmm5, xmm10, xmm5
       vorps    xmm2, xmm5, xmm2
       vmovaps  xmm5, xmm4
       vmovaps  xmm10, xmm8
       vcmpeqps xmm11, xmm5, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm5, xmm5
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm5
       vorps    xmm11, xmm13, xmm11
       vandps   xmm5, xmm11, xmm5
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm5, xmm10, xmm5
       vcmpeqps xmm10, xmm4, xmm8
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm4
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm4, xmm8
       vorps    xmm10, xmm11, xmm10
       vandps   xmm4, xmm10, xmm4
       vandnps  xmm8, xmm10, xmm8
       vorps    xmm4, xmm8, xmm4
       vmovaps  xmm8, xmm0
       vmovaps  xmm10, xmm3
       vcmpeqps xmm11, xmm8, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm8, xmm8
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm8
       vorps    xmm11, xmm13, xmm11
       vandps   xmm8, xmm8, xmm11
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm8, xmm10, xmm8
       vcmpeqps xmm10, xmm0, xmm3
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm0, xmm3
       vorps    xmm10, xmm11, xmm10
       vandps   xmm0, xmm10, xmm0
       vandnps  xmm3, xmm10, xmm3
       vorps    xmm0, xmm3, xmm0
       vmovaps  xmm3, xmm2
       vmovaps  xmm10, xmm4
       vcmpeqps xmm11, xmm3, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm3, xmm3

G_M000_IG14:                ;; offset=0x0C63
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm3
       vorps    xmm11, xmm13, xmm11
       vandps   xmm3, xmm11, xmm3
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm3, xmm10, xmm3
       vcmpeqps xmm10, xmm2, xmm4
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm2
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm2, xmm4
       vorps    xmm10, xmm11, xmm10
       vandps   xmm2, xmm10, xmm2
       vandnps  xmm4, xmm10, xmm4
       vorps    xmm2, xmm4, xmm2
       vmovaps  xmm4, xmm9
       vmovaps  xmm10, xmm5
       vcmpeqps xmm11, xmm4, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm4, xmm4
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm4
       vorps    xmm11, xmm13, xmm11
       vandps   xmm4, xmm11, xmm4
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm4, xmm10, xmm4
       vcmpeqps xmm10, xmm9, xmm5
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm9
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm9, xmm5
       vorps    xmm10, xmm11, xmm10
       vandps   xmm9, xmm9, xmm10
       vandnps  xmm5, xmm10, xmm5
       vorps    xmm5, xmm9, xmm5
       vmovaps  xmm9, xmm7
       vmovaps  xmm10, xmm6
       vcmpeqps xmm11, xmm9, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm9, xmm9
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm9
       vorps    xmm11, xmm13, xmm11
       vandps   xmm9, xmm9, xmm11
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm9, xmm10, xmm9
       vcmpeqps xmm10, xmm7, xmm6
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm7
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm7, xmm6
       vorps    xmm10, xmm11, xmm10
       vandps   xmm7, xmm10, xmm7

G_M000_IG15:                ;; offset=0x0D84
       vandnps  xmm6, xmm10, xmm6
       vorps    xmm6, xmm6, xmm7
       vmovaps  xmm7, xmm0
       vmovaps  xmm10, xmm2
       vcmpeqps xmm11, xmm7, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm7, xmm7
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm7
       vorps    xmm11, xmm13, xmm11
       vandps   xmm7, xmm11, xmm7
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm7, xmm10, xmm7
       vcmpeqps xmm10, xmm0, xmm2
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm0
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm0, xmm2
       vorps    xmm10, xmm11, xmm10
       vandps   xmm0, xmm10, xmm0
       vandnps  xmm2, xmm10, xmm2
       vorps    xmm0, xmm2, xmm0
       vmovaps  xmm2, xmm1
       vmovaps  xmm10, xmm5
       vcmpeqps xmm11, xmm2, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm2, xmm2
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm2
       vorps    xmm11, xmm13, xmm11
       vandps   xmm2, xmm11, xmm2
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm2, xmm10, xmm2
       vcmpeqps xmm10, xmm1, xmm5
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm1
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm1, xmm5
       vorps    xmm10, xmm11, xmm10
       vandps   xmm1, xmm10, xmm1
       vandnps  xmm5, xmm10, xmm5
       vorps    xmm1, xmm5, xmm1
       vmovaps  xmm5, xmm3
       vmovaps  xmm10, xmm6
       vcmpeqps xmm11, xmm5, xmm10
       vxorps   xmm12, xmm12, xmm12
       vpcmpgtd xmm12, xmm12, xmm10
       vandps   xmm11, xmm12, xmm11
       vcmpneqps xmm12, xmm5, xmm5
       vorps    xmm11, xmm11, xmm12
       vcmpltps xmm13, xmm10, xmm5
       vorps    xmm11, xmm13, xmm11
       vandps   xmm5, xmm11, xmm5
       vandnps  xmm10, xmm11, xmm10
       vorps    xmm5, xmm10, xmm5

G_M000_IG16:                ;; offset=0x0EA2
       vcmpeqps xmm10, xmm3, xmm6
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm3
       vandps   xmm10, xmm11, xmm10
       vorps    xmm10, xmm10, xmm12
       vcmpltps xmm11, xmm3, xmm6
       vorps    xmm10, xmm11, xmm10
       vandps   xmm3, xmm10, xmm3
       vandnps  xmm6, xmm10, xmm6
       vorps    xmm3, xmm6, xmm3
       vcmpeqps xmm6, xmm8, xmm4
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm8
       vandps   xmm6, xmm10, xmm6
       vcmpneqps xmm10, xmm8, xmm8
       vorps    xmm6, xmm10, xmm6
       vcmpltps xmm10, xmm8, xmm4
       vorps    xmm6, xmm10, xmm6
       vblendvps xmm4, xmm4, xmm8, xmm6
       vmovaps  xmm6, xmm1
       vmovaps  xmm8, xmm3
       vcmpeqps xmm10, xmm6, xmm8
       vxorps   xmm11, xmm11, xmm11
       vpcmpgtd xmm11, xmm11, xmm8
       vandps   xmm10, xmm11, xmm10
       vcmpneqps xmm11, xmm6, xmm6
       vorps    xmm10, xmm10, xmm11
       vcmpltps xmm12, xmm8, xmm6
       vorps    xmm10, xmm12, xmm10
       vandps   xmm6, xmm10, xmm6
       vandnps  xmm8, xmm10, xmm8
       vorps    xmm6, xmm8, xmm6
       vcmpeqps xmm8, xmm1, xmm3
       vxorps   xmm10, xmm10, xmm10
       vpcmpgtd xmm10, xmm10, xmm1
       vandps   xmm8, xmm10, xmm8
       vorps    xmm8, xmm8, xmm11
       vcmpltps xmm10, xmm1, xmm3
       vorps    xmm8, xmm10, xmm8
       vandps   xmm1, xmm8, xmm1
       vandnps  xmm3, xmm8, xmm3
       vorps    xmm1, xmm3, xmm1
       vcmpeqps xmm3, xmm2, xmm9
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm2
       vandps   xmm3, xmm8, xmm3
       vcmpneqps xmm8, xmm2, xmm2
       vorps    xmm3, xmm8, xmm3
       vcmpltps xmm8, xmm2, xmm9
       vorps    xmm3, xmm8, xmm3
       vblendvps xmm2, xmm9, xmm2, xmm3
       vcmpeqps xmm3, xmm5, xmm4
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm5
       vandps   xmm3, xmm8, xmm3
       vcmpneqps xmm8, xmm5, xmm5
       vorps    xmm3, xmm8, xmm3
       vcmpltps xmm8, xmm5, xmm4
       vorps    xmm3, xmm8, xmm3

G_M000_IG17:                ;; offset=0x0FB8
       vblendvps xmm3, xmm4, xmm5, xmm3
       vmovaps  xmm4, xmm0
       vmovaps  xmm5, xmm1
       vcmpeqps xmm8, xmm4, xmm5
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm5
       vandps   xmm8, xmm9, xmm8
       vcmpneqps xmm9, xmm4, xmm4
       vorps    xmm8, xmm8, xmm9
       vcmpltps xmm10, xmm5, xmm4
       vorps    xmm8, xmm10, xmm8
       vandps   xmm4, xmm8, xmm4
       vandnps  xmm5, xmm8, xmm5
       vorps    xmm4, xmm5, xmm4
       vcmpeqps xmm5, xmm0, xmm1
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm0
       vandps   xmm5, xmm8, xmm5
       vorps    xmm5, xmm9, xmm5
       vcmpltps xmm8, xmm0, xmm1
       vorps    xmm5, xmm8, xmm5
       vandps   xmm0, xmm0, xmm5
       vandnps  xmm1, xmm5, xmm1
       vorps    xmm0, xmm1, xmm0
       vmovaps  xmm1, xmm7
       vmovaps  xmm5, xmm6
       vcmpeqps xmm8, xmm1, xmm5
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm5
       vandps   xmm8, xmm9, xmm8
       vcmpneqps xmm9, xmm1, xmm1
       vorps    xmm8, xmm8, xmm9
       vcmpltps xmm10, xmm5, xmm1
       vorps    xmm8, xmm10, xmm8
       vandps   xmm1, xmm8, xmm1
       vandnps  xmm5, xmm8, xmm5
       vorps    xmm1, xmm5, xmm1
       vcmpeqps xmm5, xmm7, xmm6
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm7
       vandps   xmm5, xmm8, xmm5
       vorps    xmm5, xmm9, xmm5
       vcmpltps xmm8, xmm7, xmm6
       vorps    xmm5, xmm8, xmm5
       vandps   xmm7, xmm7, xmm5
       vandnps  xmm5, xmm5, xmm6
       vorps    xmm5, xmm5, xmm7
       vmovaps  xmm6, xmm2
       vmovaps  xmm7, xmm3
       vcmpeqps xmm8, xmm6, xmm7
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm7
       vandps   xmm8, xmm9, xmm8
       vcmpneqps xmm9, xmm6, xmm6
       vorps    xmm8, xmm8, xmm9
       vcmpltps xmm10, xmm7, xmm6
       vorps    xmm8, xmm10, xmm8
       vandps   xmm6, xmm8, xmm6
       vandnps  xmm7, xmm8, xmm7
       vorps    xmm6, xmm7, xmm6
       vcmpeqps xmm7, xmm2, xmm3

G_M000_IG18:                ;; offset=0x10CA
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm2
       vandps   xmm7, xmm8, xmm7
       vorps    xmm7, xmm9, xmm7
       vcmpltps xmm8, xmm2, xmm3
       vorps    xmm7, xmm8, xmm7
       vandps   xmm2, xmm2, xmm7
       vandnps  xmm3, xmm7, xmm3
       vorps    xmm2, xmm3, xmm2
       vmovaps  xmm3, xmm5
       vmovaps  xmm7, xmm2
       vcmpeqps xmm8, xmm3, xmm7
       vxorps   xmm9, xmm9, xmm9
       vpcmpgtd xmm9, xmm9, xmm7
       vandps   xmm8, xmm9, xmm8
       vcmpneqps xmm9, xmm3, xmm3
       vorps    xmm8, xmm8, xmm9
       vcmpltps xmm10, xmm7, xmm3
       vorps    xmm8, xmm10, xmm8
       vandps   xmm3, xmm8, xmm3
       vandnps  xmm7, xmm8, xmm7
       vorps    xmm3, xmm7, xmm3
       vcmpeqps xmm7, xmm5, xmm2
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm5
       vandps   xmm7, xmm8, xmm7
       vorps    xmm7, xmm9, xmm7
       vcmpltps xmm8, xmm5, xmm2
       vorps    xmm7, xmm8, xmm7
       vandps   xmm5, xmm5, xmm7
       vandnps  xmm2, xmm7, xmm2
       vorps    xmm5, xmm2, xmm5
       vcmpeqps xmm2, xmm1, xmm6
       vxorps   xmm7, xmm7, xmm7
       vpcmpgtd xmm7, xmm7, xmm1
       vandps   xmm2, xmm7, xmm2
       vcmpneqps xmm7, xmm1, xmm1
       vorps    xmm2, xmm7, xmm2
       vcmpltps xmm7, xmm1, xmm6
       vorps    xmm2, xmm7, xmm2
       vblendvps xmm1, xmm6, xmm1, xmm2
       vmovaps  xmm2, xmm4
       vmovaps  xmm6, xmm5
       vcmpeqps xmm7, xmm2, xmm6
       vxorps   xmm8, xmm8, xmm8
       vpcmpgtd xmm8, xmm8, xmm6
       vandps   xmm7, xmm8, xmm7
       vcmpneqps xmm8, xmm2, xmm2
       vorps    xmm7, xmm8, xmm7
       vcmpltps xmm9, xmm6, xmm2
       vorps    xmm7, xmm9, xmm7
       vandps   xmm2, xmm2, xmm7
       vandnps  xmm6, xmm7, xmm6
       vorps    xmm2, xmm6, xmm2
       vcmpeqps xmm6, xmm4, xmm5
       vxorps   xmm7, xmm7, xmm7
       vpcmpgtd xmm7, xmm7, xmm4
       vandps   xmm6, xmm7, xmm6
       vorps    xmm6, xmm8, xmm6
       vcmpltps xmm7, xmm4, xmm5

G_M000_IG19:                ;; offset=0x11D1
       vorps    xmm6, xmm7, xmm6
       vandps   xmm4, xmm4, xmm6
       vandnps  xmm5, xmm6, xmm5
       vorps    xmm4, xmm5, xmm4
       vcmpeqps xmm5, xmm3, xmm1
       vxorps   xmm6, xmm6, xmm6
       vpcmpgtd xmm6, xmm6, xmm3
       vandps   xmm5, xmm6, xmm5
       vcmpneqps xmm6, xmm3, xmm3
       vorps    xmm5, xmm6, xmm5
       vcmpltps xmm6, xmm3, xmm1
       vorps    xmm5, xmm6, xmm5
       vblendvps xmm1, xmm1, xmm3, xmm5

G_M000_IG20:                ;; offset=0x120A
       mov      rax, gword ptr [rcx+0x28]
       lea      ecx, [r8+r9]
       cmp      ecx, dword ptr [rax+0x08]
       jae      G_M000_IG22
       add      r8d, r9d
       mov      ecx, r8d
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD00]
       vmulss   xmm3, xmm4, dword ptr [reloc @RWD04]
       vaddss   xmm0, xmm0, xmm3
       vmulss   xmm2, xmm2, dword ptr [reloc @RWD08]
       vmulss   xmm1, xmm1, dword ptr [reloc @RWD12]
       vaddss   xmm1, xmm2, xmm1
       vaddss   xmm0, xmm0, xmm1
       vmovss   dword ptr [rax+4*rcx+0x10], xmm0

G_M000_IG21:                ;; offset=0x1253
       vmovaps  xmm6, xmmword ptr [rsp+0xA0]
       vmovaps  xmm7, xmmword ptr [rsp+0x90]
       vmovaps  xmm8, xmmword ptr [rsp+0x80]
       vmovaps  xmm9, xmmword ptr [rsp+0x70]
       vmovaps  xmm10, xmmword ptr [rsp+0x60]
       vmovaps  xmm11, xmmword ptr [rsp+0x50]
       vmovaps  xmm12, xmmword ptr [rsp+0x40]
       vmovaps  xmm13, xmmword ptr [rsp+0x30]
       vmovaps  xmm14, xmmword ptr [rsp+0x20]
       add      rsp, 176
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r14
       ret

G_M000_IG22:                ;; offset=0x12A0
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	3E000000h		;     0.125
RWD04  	dd	3D99999Ah		;     0.075
RWD08  	dd	3D75C28Fh		;      0.06
RWD12  	dd	3D4CCCCDh		;      0.05

; Total bytes of code 4774

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ComputeMask(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data
; 0 inlinees with PGO data; 0 single block inlinees; 3 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rsi
       push     rbx
       sub      rsp, 136
       vmovaps  xmmword ptr [rsp+0x70], xmm6
       vmovaps  xmmword ptr [rsp+0x60], xmm7
       vxorps   xmm4, xmm4, xmm4
       vmovdqu  ymmword ptr [rsp+0x30], ymm4
       xor      eax, eax
       mov      qword ptr [rsp+0x50], rax
       mov      rbx, r8

G_M000_IG02:                ;; offset=0x0029
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD00]
       vbroadcastss xmm1, dword ptr [reloc @RWD04]
       vmaxss   xmm6, xmm1, xmm0
       vaddss   xmm1, xmm6, dword ptr [reloc @RWD08]
       movzx    rsi, dl
       test     esi, esi
       je       SHORT G_M000_IG05

G_M000_IG03:                ;; offset=0x004E
       vmovss   xmm7, dword ptr [reloc @RWD12]
       vmovaps  xmm0, xmm7
       lea      r8, [rsp+0x50]
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovss   dword ptr [rsp+0x4C], xmm0
       test     rbx, rbx
       je       SHORT G_M000_IG04
       movzx    r8, byte  ptr [rsp+0x50]
       mov      rcx, rbx
       xor      edx, edx
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG04:                ;; offset=0x0081
       vmovss   xmm0, dword ptr [rsp+0x4C]
       vmovss   dword ptr [rsp+0x5C], xmm0
       jmp      SHORT G_M000_IG06

G_M000_IG05:                ;; offset=0x008F
       vmovss   xmm7, dword ptr [reloc @RWD12]
       vdivss   xmm0, xmm7, xmm1
       vmovss   dword ptr [rsp+0x5C], xmm0

G_M000_IG06:                ;; offset=0x00A1
       vmulss   xmm6, xmm6, xmm6
       vaddss   xmm1, xmm6, dword ptr [reloc @RWD16]
       test     esi, esi
       je       SHORT G_M000_IG09

G_M000_IG07:                ;; offset=0x00B1
       vmovaps  xmm0, xmm7
       lea      r8, [rsp+0x40]
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovss   dword ptr [rsp+0x3C], xmm0
       test     rbx, rbx
       je       SHORT G_M000_IG08
       movzx    r8, byte  ptr [rsp+0x40]
       mov      rcx, rbx
       xor      edx, edx
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG08:                ;; offset=0x00DC
       vmovss   xmm0, dword ptr [rsp+0x3C]
       vmovss   dword ptr [rsp+0x48], xmm0
       jmp      SHORT G_M000_IG10

G_M000_IG09:                ;; offset=0x00EA
       vdivss   xmm1, xmm7, xmm1
       vmovss   dword ptr [rsp+0x48], xmm1

G_M000_IG10:                ;; offset=0x00F4
       vaddss   xmm2, xmm6, dword ptr [reloc @RWD20]
       test     esi, esi
       je       SHORT G_M000_IG13

G_M000_IG11:                ;; offset=0x0100
       vmovaps  xmm0, xmm7
       lea      r8, [rsp+0x30]
       vmovaps  xmm1, xmm2
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovss   dword ptr [rsp+0x2C], xmm0
       test     rbx, rbx
       je       SHORT G_M000_IG12
       movzx    r8, byte  ptr [rsp+0x30]
       mov      rcx, rbx
       xor      edx, edx
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG12:                ;; offset=0x012F
       vmovss   xmm0, dword ptr [rsp+0x2C]
       vmovaps  xmm2, xmm0
       jmp      SHORT G_M000_IG14

G_M000_IG13:                ;; offset=0x013B
       vdivss   xmm2, xmm7, xmm2

G_M000_IG14:                ;; offset=0x013F
       vmovss   xmm0, dword ptr [rsp+0x5C]
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD24]
       vmovss   xmm1, dword ptr [rsp+0x48]
       vmulss   xmm1, xmm1, dword ptr [reloc @RWD28]
       vaddss   xmm0, xmm0, xmm1
       vmulss   xmm1, xmm2, dword ptr [reloc @RWD32]
       vaddss   xmm0, xmm0, xmm1
       vaddss   xmm0, xmm0, dword ptr [reloc @RWD36]

G_M000_IG15:                ;; offset=0x0173
       vmovaps  xmm6, xmmword ptr [rsp+0x70]
       vmovaps  xmm7, xmmword ptr [rsp+0x60]
       add      rsp, 136
       pop      rbx
       pop      rsi
       ret

RWD00  	dd	3F3F62FEh		;  0.747604
RWD04  	dd	3A83126Fh		;     0.001
RWD08  	dd	4398852Ah		;    305.04
RWD12  	dd	3F800000h		;         1
RWD16  	dd	400C5322h		;   2.19257
RWD20  	dd	3F0C5322h		;  0.548144
RWD24  	dd	414E7F18h		;    12.906
RWD28  	dd	40A0B47Bh		;   5.02203
RWD32  	dd	404F0F94h		;   3.23533
RWD36  	dd	BF3DE353h		;  -0.74175

; Total bytes of code 393

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ModulateScalar(System.ReadOnlySpan`1[float],int,float,bool,DotJpegli.Internal.AqRationalDiagnostics):float (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 0 single block inlinees; 4 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r15
       push     r14
       push     r13
       push     r12
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 184
       vmovaps  xmmword ptr [rsp+0xA0], xmm6
       vmovaps  xmmword ptr [rsp+0x90], xmm7
       vmovaps  xmmword ptr [rsp+0x80], xmm8
       vmovaps  xmmword ptr [rsp+0x70], xmm9
       vmovaps  xmmword ptr [rsp+0x60], xmm10
       vmovaps  xmmword ptr [rsp+0x50], xmm11
       vmovaps  xmmword ptr [rsp+0x40], xmm12
       xor      eax, eax
       mov      qword ptr [rsp+0x30], rax
       mov      qword ptr [rsp+0x28], rax
       mov      edi, edx
       vmovaps  xmm6, xmm2
       mov      ebx, r9d
       mov      rsi, gword ptr [rsp+0x120]

G_M000_IG02:                ;; offset=0x0063
       mov      rbp, bword ptr [rcx]
       mov      r14d, dword ptr [rcx+0x08]
       vxorps   xmm7, xmm7, xmm7
       vxorps   xmm8, xmm8, xmm8
       xor      r15d, r15d

G_M000_IG03:                ;; offset=0x0076
       mov      r13d, r15d
       imul     r13d, edi
       cmp      r15d, 7
       jne      SHORT G_M000_IG05

G_M000_IG04:                ;; offset=0x0083
       mov      r12d, r13d
       jmp      SHORT G_M000_IG06

G_M000_IG05:                ;; offset=0x0088
       lea      r12d, [rdi+r13]

G_M000_IG06:                ;; offset=0x008C
       cmp      r13d, r14d
       jae      G_M000_IG19
       mov      r8d, r13d
       vmovss   xmm9, dword ptr [rbp+4*r8]
       xor      eax, eax
       vmovss   xmm10, dword ptr [reloc @RWD00]
       vmovss   xmm11, dword ptr [reloc @RWD04]

G_M000_IG07:                ;; offset=0x00B1
       lea      r8d, [r13+rax+0x01]
       cmp      r8d, r14d
       jae      G_M000_IG19
       vmovss   xmm12, dword ptr [rbp+4*r8]
       mov      dword ptr [rsp+0x3C], eax
       lea      r8d, [r12+rax]
       cmp      r8d, r14d
       jae      G_M000_IG19
       vsubss   xmm0, xmm9, dword ptr [rbp+4*r8]
       vandps   xmm0, xmm0, xmmword ptr [reloc @RWD08]
       vsubss   xmm1, xmm9, xmm12
       vandps   xmm1, xmm1, xmmword ptr [reloc @RWD24]
       vaddss   xmm1, xmm1, xmm7
       vaddss   xmm7, xmm0, xmm1
       vaddss   xmm0, xmm9, xmm10
       vxorps   xmm1, xmm1, xmm1
       vcmpneqps xmm2, xmm0, xmm0
       vorps    xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpgtps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandps   xmm0, xmm1, xmm0
       vmulss   xmm1, xmm0, xmm0
       vmulss   xmm2, xmm11, xmm1
       vaddss   xmm2, xmm2, dword ptr [reloc @RWD40]
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD44]
       vmulss   xmm0, xmm0, xmm1
       vaddss   xmm1, xmm0, dword ptr [reloc @RWD48]
       mov      byte  ptr [rsp+0x30], 0
       test     bl, bl
       je       SHORT G_M000_IG10

G_M000_IG08:                ;; offset=0x014B
       lea      r8, [rsp+0x30]
       vmovaps  xmm0, xmm2
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovaps  xmm9, xmm0
       test     rsi, rsi
       je       SHORT G_M000_IG09
       movzx    r8, byte  ptr [rsp+0x30]
       mov      rcx, rsi
       mov      edx, 1
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG09:                ;; offset=0x0177
       jmp      SHORT G_M000_IG11

G_M000_IG10:                ;; offset=0x0179
       vdivss   xmm9, xmm2, xmm1

G_M000_IG11:                ;; offset=0x017D
       vaddss   xmm8, xmm8, xmm9
       vmovaps  xmm9, xmm12
       mov      eax, dword ptr [rsp+0x3C]
       inc      eax
       cmp      eax, 7
       jl       G_M000_IG07

G_M000_IG12:                ;; offset=0x0196
       add      r12d, 7
       cmp      r12d, r14d
       jae      G_M000_IG19
       mov      r8d, r12d
       vsubss   xmm0, xmm9, dword ptr [rbp+4*r8]
       vandps   xmm0, xmm0, xmmword ptr [reloc @RWD52]
       vaddss   xmm7, xmm0, xmm7
       vaddss   xmm0, xmm9, xmm10
       vxorps   xmm1, xmm1, xmm1
       vcmpneqps xmm2, xmm0, xmm0
       vorps    xmm1, xmm2, xmm1
       vxorps   xmm2, xmm2, xmm2
       vcmpgtps xmm2, xmm0, xmm2
       vorps    xmm1, xmm2, xmm1
       vandps   xmm0, xmm1, xmm0
       vmulss   xmm1, xmm0, xmm0
       vmulss   xmm2, xmm11, xmm1
       vaddss   xmm2, xmm2, dword ptr [reloc @RWD40]
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD44]
       vmulss   xmm0, xmm0, xmm1
       vaddss   xmm1, xmm0, dword ptr [reloc @RWD48]
       mov      byte  ptr [rsp+0x28], 0
       test     bl, bl
       je       SHORT G_M000_IG15

G_M000_IG13:                ;; offset=0x0209
       lea      r8, [rsp+0x28]
       vmovaps  xmm0, xmm2
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovaps  xmm9, xmm0
       test     rsi, rsi
       je       SHORT G_M000_IG14
       movzx    r8, byte  ptr [rsp+0x28]
       mov      rcx, rsi
       mov      edx, 1
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG14:                ;; offset=0x0235
       jmp      SHORT G_M000_IG16

G_M000_IG15:                ;; offset=0x0237
       vdivss   xmm9, xmm2, xmm1

G_M000_IG16:                ;; offset=0x023B
       vaddss   xmm8, xmm8, xmm9
       inc      r15d
       cmp      r15d, 8
       jl       G_M000_IG03

G_M000_IG17:                ;; offset=0x024D
       vmulss   xmm0, xmm8, dword ptr [reloc @RWD68]
       movzx    rdx, bl
       mov      r8, rsi
       call     [DotJpegli.Internal.AdaptiveQuantization:FastLog2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float]
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD72]
       vmulss   xmm1, xmm7, dword ptr [reloc @RWD76]
       vaddss   xmm1, xmm1, xmm6
       vaddss   xmm0, xmm0, xmm1

G_M000_IG18:                ;; offset=0x0279
       vmovaps  xmm6, xmmword ptr [rsp+0xA0]
       vmovaps  xmm7, xmmword ptr [rsp+0x90]
       vmovaps  xmm8, xmmword ptr [rsp+0x80]
       vmovaps  xmm9, xmmword ptr [rsp+0x70]
       vmovaps  xmm10, xmmword ptr [rsp+0x60]
       vmovaps  xmm11, xmmword ptr [rsp+0x50]
       vmovaps  xmm12, xmmword ptr [rsp+0x40]
       add      rsp, 184
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r12
       pop      r13
       pop      r14
       pop      r15
       ret

G_M000_IG19:                ;; offset=0x02C0
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	42233332h		;      40.8
RWD04  	dd	42EF0C23h		;   119.524
RWD08  	dq	7FFFFFFF7FFFFFFFh, 7FFFFFFF7FFFFFFFh
RWD24  	dq	7FFFFFFF7FFFFFFFh, 7FFFFFFF7FFFFFFFh
RWD40  	dd	44228FFEh		;    650.25
RWD44  	dd	3B1DEA78h		; 0.00240961
RWD48  	dd	449E380Dh		;   1265.75
RWD52  	dq	7FFFFFFF7FFFFFFFh, 7FFFFFFF7FFFFFFFh
RWD68  	dd	38808081h		; 6.12745e-05
RWD72  	dd	BDDC6A08h		; -0.107624
RWD76  	dd	B8933E1Eh		; -7.02108e-05

; Total bytes of code 710

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:FastLog2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data
; 0 inlinees with PGO data; 0 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rsi
       push     rbx
       sub      rsp, 56
       xor      eax, eax
       mov      qword ptr [rsp+0x30], rax
       mov      rbx, r8

G_M000_IG02:                ;; offset=0x0010
       vmovd    r8d, xmm0
       lea      esi, [r8+0xC0D55555]
       sar      esi, 23
       mov      eax, esi
       shl      eax, 23
       sub      r8d, eax
       vmovd    xmm0, r8d
       vsubss   xmm0, xmm0, dword ptr [reloc @RWD00]
       vmulss   xmm1, xmm0, dword ptr [reloc @RWD04]
       vaddss   xmm1, xmm1, dword ptr [reloc @RWD08]
       vmulss   xmm1, xmm1, xmm0
       vaddss   xmm1, xmm1, dword ptr [reloc @RWD12]
       vmulss   xmm2, xmm0, dword ptr [reloc @RWD16]
       vaddss   xmm2, xmm2, dword ptr [reloc @RWD20]
       vmulss   xmm0, xmm2, xmm0
       vsubss   xmm0, xmm0, dword ptr [reloc @RWD24]
       test     dl, dl
       je       SHORT G_M000_IG05

G_M000_IG03:                ;; offset=0x0070
       lea      r8, [rsp+0x30]
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovss   dword ptr [rsp+0x2C], xmm0
       test     rbx, rbx
       je       SHORT G_M000_IG04
       movzx    r8, byte  ptr [rsp+0x30]
       mov      rcx, rbx
       mov      edx, 2
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG04:                ;; offset=0x009A
       vmovss   xmm0, dword ptr [rsp+0x2C]
       jmp      SHORT G_M000_IG06

G_M000_IG05:                ;; offset=0x00A2
       vdivss   xmm0, xmm0, xmm1

G_M000_IG06:                ;; offset=0x00A6
       vxorps   xmm1, xmm1, xmm1
       vcvtsi2ss xmm1, xmm1, esi
       vaddss   xmm0, xmm0, xmm1

G_M000_IG07:                ;; offset=0x00B2
       add      rsp, 56
       pop      rbx
       pop      rsi
       ret

RWD00  	dd	3F800000h		;         1
RWD04  	dd	3E32458Ch		;  0.174093
RWD08  	dd	3F813CEDh		;   1.00967
RWD12  	dd	3F7D8625h		;  0.990328
RWD16  	dd	3F3E11C7h		;  0.742459
RWD20  	dd	3FB6E02Bh		;   1.42872
RWD24  	dd	35F85AB0h		; 1.85038e-06

; Total bytes of code 185

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:FastPow2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; partially interruptible
; No PGO data
; 0 inlinees with PGO data; 0 single block inlinees; 1 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rbx
       sub      rsp, 48
       xor      eax, eax
       mov      qword ptr [rsp+0x28], rax
       mov      rbx, r8

G_M000_IG02:                ;; offset=0x000F
       vroundss xmm1, xmm1, xmm0, 9
       vmovaps  xmm2, xmm1
       vcmpordss xmm3, xmm2, xmm2
       vandps   xmm3, xmm3, xmm2
       mov      r8d, 0x7FFFFFFF
       vcvttss2si eax, xmm3
       vucomiss xmm2, dword ptr [reloc @RWD00]
       cmovb    r8d, eax
       add      r8d, 127
       shl      r8d, 23
       vmovd    xmm2, r8d
       vsubss   xmm0, xmm0, xmm1
       vmulss   xmm1, xmm0, dword ptr [reloc @RWD04]
       vsubss   xmm1, xmm1, dword ptr [reloc @RWD08]
       vmulss   xmm1, xmm1, xmm0
       vsubss   xmm1, xmm1, dword ptr [reloc @RWD12]
       vmulss   xmm1, xmm1, xmm0
       vaddss   xmm1, xmm1, dword ptr [reloc @RWD16]
       vaddss   xmm3, xmm0, dword ptr [reloc @RWD20]
       vmulss   xmm3, xmm3, xmm0
       vaddss   xmm3, xmm3, dword ptr [reloc @RWD24]
       vmulss   xmm0, xmm3, xmm0
       vaddss   xmm0, xmm0, dword ptr [reloc @RWD28]
       vmulss   xmm0, xmm0, xmm2
       test     dl, dl
       je       SHORT G_M000_IG05

G_M000_IG03:                ;; offset=0x0099
       lea      r8, [rsp+0x28]
       call     [DotJpegli.Internal.AqReciprocal:Divide(float,float,byref):float]
       vmovss   dword ptr [rsp+0x24], xmm0
       test     rbx, rbx
       je       SHORT G_M000_IG04
       movzx    r8, byte  ptr [rsp+0x28]
       mov      rcx, rbx
       mov      edx, 3
       call     [DotJpegli.Internal.AqRationalDiagnostics:Record(int,bool):this]

G_M000_IG04:                ;; offset=0x00C3
       vmovss   xmm0, dword ptr [rsp+0x24]
       jmp      SHORT G_M000_IG06

G_M000_IG05:                ;; offset=0x00CB
       vdivss   xmm0, xmm0, xmm1

G_M000_IG06:                ;; offset=0x00CF
       add      rsp, 48
       pop      rbx
       ret

RWD00  	dd	4F000000h		; 2.14748e+09
RWD04  	dd	3E5749EEh		;  0.210243
RWD08  	dd	3CB621BEh		; 0.0222329
RWD12  	dd	419B8831h		;   19.4415
RWD16  	dd	42C519F1h		;   98.5507
RWD20  	dd	4122CC6Bh		;   10.1749
RWD24  	dd	424379A1h		;   48.8688
RWD28  	dd	42C519F0h		;   98.5507

; Total bytes of code 213

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ModulateVector(int,int,float):float:this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 30 single block inlinees; 4 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rsi
       push     rbx
       sub      rsp, 88
       vmovaps  xmmword ptr [rsp+0x40], xmm6
       vmovaps  xmmword ptr [rsp+0x30], xmm7
       vmovaps  xmmword ptr [rsp+0x20], xmm8
       vmovss   dword ptr [rsp+0x88], xmm3

G_M000_IG02:                ;; offset=0x0021
       vxorps   xmm6, xmm6, xmm6
       vxorps   xmm0, xmm0, xmm0
       mov      eax, dword ptr [rcx+0x54]
       imul     r8d, eax
       lea      edx, [r8+rdx+0x01]
       vbroadcastss ymm1, dword ptr [reloc @RWD00]
       mov      r8, gword ptr [rcx+0x10]
       test     edx, edx
       jl       G_M000_IG10
       mov      r10d, dword ptr [r8+0x08]
       mov      r9d, r10d
       sub      r9d, edx
       cmp      r9d, 8
       jl       G_M000_IG10
       cmp      edx, r10d
       jae      G_M000_IG11
       mov      r9d, edx
       vmovups  ymm2, ymmword ptr [r8+4*r9+0x10]
       xor      r8d, r8d

G_M000_IG03:                ;; offset=0x0074
       mov      r9, gword ptr [rcx+0x10]
       lea      r11d, [rdx+rax]
       test     r11d, r11d
       jl       G_M000_IG10
       mov      ebx, r10d
       sub      ebx, r11d
       cmp      ebx, 8
       jl       G_M000_IG10
       cmp      r11d, r10d
       jae      G_M000_IG11
       vmovups  ymm4, ymmword ptr [r9+4*r11+0x10]
       mov      r9, gword ptr [rcx+0x10]
       lea      r11d, [rdx+0x01]
       test     r11d, r11d
       jge      SHORT G_M000_IG06
       jmp      G_M000_IG10
       align    [0 bytes for IG04]

G_M000_IG04:                ;; offset=0x00B6
       cmp      r8d, 7
       jne      SHORT G_M000_IG03

G_M000_IG05:                ;; offset=0x00BC
       vmovaps  ymm4, ymm2
       mov      r9, gword ptr [rcx+0x10]
       lea      r11d, [rdx+0x01]
       test     r11d, r11d
       jl       G_M000_IG10

G_M000_IG06:                ;; offset=0x00D1
       mov      ebx, dword ptr [r9+0x08]
       mov      esi, ebx
       sub      esi, r11d
       cmp      esi, 8
       jl       G_M000_IG10
       cmp      r11d, ebx
       jae      G_M000_IG11
       mov      r11d, r11d
       vsubps   ymm5, ymm2, ymmword ptr [r9+4*r11+0x10]
       vbroadcastss ymm7, dword ptr [reloc @RWD04]
       vandps   ymm5, ymm5, ymm7
       vsubps   ymm8, ymm2, ymm4
       vandps   ymm7, ymm8, ymm7
       vmovaps  ymm8, ymm5
       vaddss   xmm6, xmm8, xmm6
       vmovaps  ymm8, ymm7
       vaddss   xmm6, xmm8, xmm6
       vmovaps  xmm8, xmm5
       vmovshdup xmm8, xmm8
       vaddss   xmm6, xmm8, xmm6
       vmovaps  xmm8, xmm7
       vmovshdup xmm8, xmm8
       vaddss   xmm6, xmm8, xmm6
       vmovaps  xmm8, xmm5
       vunpckhps xmm8, xmm8, xmm8
       vaddss   xmm6, xmm8, xmm6
       vmovaps  xmm8, xmm7
       vunpckhps xmm8, xmm8, xmm8
       vaddss   xmm6, xmm8, xmm6
       vmovaps  xmm8, xmm5
       vshufps  xmm8, xmm8, xmm8, -1
       vaddss   xmm6, xmm8, xmm6
       vmovaps  xmm8, xmm7
       vshufps  xmm8, xmm8, xmm8, -1
       vaddss   xmm6, xmm8, xmm6
       vextractf128 xmm8, ymm5
       vaddss   xmm6, xmm8, xmm6
       vextractf128 xmm8, ymm7
       vaddss   xmm6, xmm8, xmm6
       vextractf128 xmm8, ymm5
       vmovshdup xmm8, xmm8
       vaddss   xmm6, xmm8, xmm6
       vextractf128 xmm8, ymm7
       vmovshdup xmm8, xmm8
       vaddss   xmm6, xmm8, xmm6
       vextractf128 xmm5, ymm5
       vunpckhps xmm5, xmm5, xmm5
       vaddss   xmm6, xmm6, xmm5
       vextractf128 xmm5, ymm7
       vunpckhps xmm5, xmm5, xmm5
       vaddss   xmm5, xmm6, xmm5
       vextractf128 xmm6, ymm7
       vshufps  xmm6, xmm6, xmm6, -1
       vaddss   xmm6, xmm5, xmm6
       vaddps   ymm2, ymm2, ymm1
       vcmpneqps ymm5, ymm2, ymm2
       vxorps   ymm7, ymm7, ymm7
       vcmpgtps ymm7, ymm2, ymm7
       vorps    ymm5, ymm7, ymm5
       vandps   ymm2, ymm5, ymm2
       vmulps   ymm5, ymm2, ymm2
       vmulps   ymm2, ymm2, ymmword ptr [reloc @RWD08]
       vmulps   ymm2, ymm2, ymm5
       vaddps   ymm2, ymm2, ymmword ptr [reloc @RWD40]

G_M000_IG07:                ;; offset=0x01FA
       vmulps   ymm5, ymm5, ymmword ptr [reloc @RWD72]
       vaddps   ymm5, ymm5, ymmword ptr [reloc @RWD104]
       vdivps   ymm2, ymm5, ymm2
       vmovaps  ymm5, ymm2
       vaddss   xmm0, xmm0, xmm5
       vmovaps  xmm5, xmm2
       vmovshdup xmm5, xmm5
       vaddss   xmm0, xmm0, xmm5
       vmovaps  xmm5, xmm2
       vunpckhps xmm5, xmm5, xmm5
       vaddss   xmm0, xmm0, xmm5
       vmovaps  xmm5, xmm2
       vshufps  xmm5, xmm5, xmm5, -1
       vaddss   xmm0, xmm0, xmm5
       vextractf128 xmm5, ymm2
       vaddss   xmm0, xmm0, xmm5
       vextractf128 xmm5, ymm2
       vmovshdup xmm5, xmm5
       vaddss   xmm0, xmm0, xmm5
       vextractf128 xmm5, ymm2
       vunpckhps xmm5, xmm5, xmm5
       vaddss   xmm0, xmm0, xmm5
       vextractf128 xmm2, ymm2
       vshufps  xmm2, xmm2, xmm2, -1
       vaddss   xmm0, xmm0, xmm2
       vmovaps  ymm2, ymm4
       add      edx, eax
       inc      r8d
       cmp      r8d, 8
       jl       G_M000_IG04

G_M000_IG08:                ;; offset=0x0283
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD136]
       call     [DotJpegli.Internal.BaselineAdaptiveQuantization:FastLog2(float):float]
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD140]
       vmulss   xmm1, xmm6, dword ptr [reloc @RWD144]
       vaddss   xmm1, xmm1, dword ptr [rsp+0x88]
       vaddss   xmm0, xmm0, xmm1

G_M000_IG09:                ;; offset=0x02AE
       vzeroupper
       vmovaps  xmm6, xmmword ptr [rsp+0x40]
       vmovaps  xmm7, xmmword ptr [rsp+0x30]
       vmovaps  xmm8, xmmword ptr [rsp+0x20]
       add      rsp, 88
       pop      rbx
       pop      rsi
       ret

G_M000_IG10:                ;; offset=0x02CA
       call     [System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()]
       int3

G_M000_IG11:                ;; offset=0x02D1
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	42233332h		;      40.8
RWD04  	dd	7FFFFFFFh		;       nan
RWD08  	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD40  	dq	449E380D449E380Dh, 449E380D449E380Dh, 449E380D449E380Dh, 449E380D449E380Dh
RWD72  	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h, 42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD104 	dq	44228FFE44228FFEh, 44228FFE44228FFEh, 44228FFE44228FFEh, 44228FFE44228FFEh
RWD136 	dd	38808081h		; 6.12745e-05
RWD140 	dd	BDDC6A08h		; -0.107624
RWD144 	dd	B8933E1Eh		; -7.02108e-05

; Total bytes of code 727

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ModulateVector(int,int,float):float:this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 30 single block inlinees; 4 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rsi
       push     rbx
       sub      rsp, 88
       vmovaps  xmmword ptr [rsp+0x40], xmm6
       vmovaps  xmmword ptr [rsp+0x30], xmm7
       vmovaps  xmmword ptr [rsp+0x20], xmm8
       vmovss   dword ptr [rsp+0x88], xmm3

G_M000_IG02:                ;; offset=0x0021
       vxorps   xmm6, xmm6, xmm6
       vxorps   xmm0, xmm0, xmm0
       mov      eax, dword ptr [rcx+0x4C]
       imul     r8d, eax
       lea      edx, [r8+rdx+0x01]
       vbroadcastss ymm1, dword ptr [reloc @RWD00]
       mov      r8, gword ptr [rcx+0x10]
       test     edx, edx
       jl       G_M000_IG10
       mov      r10d, dword ptr [r8+0x08]
       mov      r9d, r10d
       sub      r9d, edx
       cmp      r9d, 8
       jl       G_M000_IG10
       cmp      edx, r10d
       jae      G_M000_IG11
       mov      r9d, edx
       vmovups  ymm2, ymmword ptr [r8+4*r9+0x10]
       xor      r8d, r8d

G_M000_IG03:                ;; offset=0x0074
       mov      r9, gword ptr [rcx+0x10]
       lea      r11d, [rdx+rax]
       test     r11d, r11d
       jl       G_M000_IG10
       mov      ebx, r10d
       sub      ebx, r11d
       cmp      ebx, 8
       jl       G_M000_IG10
       cmp      r11d, r10d
       jae      G_M000_IG11
       vmovups  ymm4, ymmword ptr [r9+4*r11+0x10]
       mov      r9, gword ptr [rcx+0x10]
       lea      r11d, [rdx+0x01]
       test     r11d, r11d
       jge      SHORT G_M000_IG06
       jmp      G_M000_IG10
       align    [0 bytes for IG04]

G_M000_IG04:                ;; offset=0x00B6
       cmp      r8d, 7
       jne      SHORT G_M000_IG03

G_M000_IG05:                ;; offset=0x00BC
       vmovaps  ymm4, ymm2
       mov      r9, gword ptr [rcx+0x10]
       lea      r11d, [rdx+0x01]
       test     r11d, r11d
       jl       G_M000_IG10

G_M000_IG06:                ;; offset=0x00D1
       mov      ebx, dword ptr [r9+0x08]
       mov      esi, ebx
       sub      esi, r11d
       cmp      esi, 8
       jl       G_M000_IG10
       cmp      r11d, ebx
       jae      G_M000_IG11
       mov      r11d, r11d
       vsubps   ymm5, ymm2, ymmword ptr [r9+4*r11+0x10]
       vbroadcastss ymm7, dword ptr [reloc @RWD04]
       vandps   ymm5, ymm5, ymm7
       vsubps   ymm8, ymm2, ymm4
       vandps   ymm7, ymm8, ymm7
       vmovaps  ymm8, ymm5
       vaddss   xmm6, xmm8, xmm6
       vmovaps  ymm8, ymm7
       vaddss   xmm6, xmm8, xmm6
       vmovaps  xmm8, xmm5
       vmovshdup xmm8, xmm8
       vaddss   xmm6, xmm8, xmm6
       vmovaps  xmm8, xmm7
       vmovshdup xmm8, xmm8
       vaddss   xmm6, xmm8, xmm6
       vmovaps  xmm8, xmm5
       vunpckhps xmm8, xmm8, xmm8
       vaddss   xmm6, xmm8, xmm6
       vmovaps  xmm8, xmm7
       vunpckhps xmm8, xmm8, xmm8
       vaddss   xmm6, xmm8, xmm6
       vmovaps  xmm8, xmm5
       vshufps  xmm8, xmm8, xmm8, -1
       vaddss   xmm6, xmm8, xmm6
       vmovaps  xmm8, xmm7
       vshufps  xmm8, xmm8, xmm8, -1
       vaddss   xmm6, xmm8, xmm6
       vextractf128 xmm8, ymm5
       vaddss   xmm6, xmm8, xmm6
       vextractf128 xmm8, ymm7
       vaddss   xmm6, xmm8, xmm6
       vextractf128 xmm8, ymm5
       vmovshdup xmm8, xmm8
       vaddss   xmm6, xmm8, xmm6
       vextractf128 xmm8, ymm7
       vmovshdup xmm8, xmm8
       vaddss   xmm6, xmm8, xmm6
       vextractf128 xmm5, ymm5
       vunpckhps xmm5, xmm5, xmm5
       vaddss   xmm6, xmm6, xmm5
       vextractf128 xmm5, ymm7
       vunpckhps xmm5, xmm5, xmm5
       vaddss   xmm5, xmm6, xmm5
       vextractf128 xmm6, ymm7
       vshufps  xmm6, xmm6, xmm6, -1
       vaddss   xmm6, xmm5, xmm6
       vaddps   ymm2, ymm2, ymm1
       vcmpneqps ymm5, ymm2, ymm2
       vxorps   ymm7, ymm7, ymm7
       vcmpgtps ymm7, ymm2, ymm7
       vorps    ymm5, ymm7, ymm5
       vandps   ymm2, ymm5, ymm2
       vmulps   ymm5, ymm2, ymm2
       vmulps   ymm2, ymm2, ymmword ptr [reloc @RWD08]
       vmulps   ymm2, ymm2, ymm5
       vaddps   ymm2, ymm2, ymmword ptr [reloc @RWD40]

G_M000_IG07:                ;; offset=0x01FA
       vmulps   ymm5, ymm5, ymmword ptr [reloc @RWD72]
       vaddps   ymm5, ymm5, ymmword ptr [reloc @RWD104]
       vdivps   ymm2, ymm5, ymm2
       vmovaps  ymm5, ymm2
       vaddss   xmm0, xmm0, xmm5
       vmovaps  xmm5, xmm2
       vmovshdup xmm5, xmm5
       vaddss   xmm0, xmm0, xmm5
       vmovaps  xmm5, xmm2
       vunpckhps xmm5, xmm5, xmm5
       vaddss   xmm0, xmm0, xmm5
       vmovaps  xmm5, xmm2
       vshufps  xmm5, xmm5, xmm5, -1
       vaddss   xmm0, xmm0, xmm5
       vextractf128 xmm5, ymm2
       vaddss   xmm0, xmm0, xmm5
       vextractf128 xmm5, ymm2
       vmovshdup xmm5, xmm5
       vaddss   xmm0, xmm0, xmm5
       vextractf128 xmm5, ymm2
       vunpckhps xmm5, xmm5, xmm5
       vaddss   xmm0, xmm0, xmm5
       vextractf128 xmm2, ymm2
       vshufps  xmm2, xmm2, xmm2, -1
       vaddss   xmm0, xmm0, xmm2
       vmovaps  ymm2, ymm4
       add      edx, eax
       inc      r8d
       cmp      r8d, 8
       jl       G_M000_IG04

G_M000_IG08:                ;; offset=0x0283
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD136]
       call     [DotJpegli.Internal.StencilAdaptiveQuantization:FastLog2(float):float]
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD140]
       vmulss   xmm1, xmm6, dword ptr [reloc @RWD144]
       vaddss   xmm1, xmm1, dword ptr [rsp+0x88]
       vaddss   xmm0, xmm0, xmm1

G_M000_IG09:                ;; offset=0x02AE
       vzeroupper
       vmovaps  xmm6, xmmword ptr [rsp+0x40]
       vmovaps  xmm7, xmmword ptr [rsp+0x30]
       vmovaps  xmm8, xmmword ptr [rsp+0x20]
       add      rsp, 88
       pop      rbx
       pop      rsi
       ret

G_M000_IG10:                ;; offset=0x02CA
       call     [System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()]
       int3

G_M000_IG11:                ;; offset=0x02D1
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	42233332h		;      40.8
RWD04  	dd	7FFFFFFFh		;       nan
RWD08  	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD40  	dq	449E380D449E380Dh, 449E380D449E380Dh, 449E380D449E380Dh, 449E380D449E380Dh
RWD72  	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h, 42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD104 	dq	44228FFE44228FFEh, 44228FFE44228FFEh, 44228FFE44228FFEh, 44228FFE44228FFEh
RWD136 	dd	38808081h		; 6.12745e-05
RWD140 	dd	BDDC6A08h		; -0.107624
RWD144 	dd	B8933E1Eh		; -7.02108e-05

; Total bytes of code 727

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ModulateVector(int,int,float):float:this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 30 single block inlinees; 5 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 264
       vmovaps  xmmword ptr [rsp+0xF0], xmm6
       vmovaps  xmmword ptr [rsp+0xE0], xmm7
       vmovaps  xmmword ptr [rsp+0xD0], xmm8
       vmovaps  xmmword ptr [rsp+0xC0], xmm9
       vmovaps  xmmword ptr [rsp+0xB0], xmm10
       vmovaps  xmmword ptr [rsp+0xA0], xmm11
       vxorps   xmm4, xmm4, xmm4
       vmovdqu  ymmword ptr [rsp+0x50], ymm4
       vmovdqu  ymmword ptr [rsp+0x70], ymm4
       vmovss   dword ptr [rsp+0x148], xmm3
       mov      rbx, rcx

G_M000_IG02:                ;; offset=0x005D
       vxorps   xmm6, xmm6, xmm6
       vxorps   xmm7, xmm7, xmm7
       imul     r8d, dword ptr [rbx+0x54]
       lea      esi, [r8+rdx+0x01]
       vbroadcastss ymm8, dword ptr [reloc @RWD00]
       mov      rcx, gword ptr [rbx+0x18]
       test     esi, esi
       jl       G_M000_IG13
       mov      edx, dword ptr [rcx+0x08]
       mov      r8d, edx
       sub      r8d, esi
       cmp      r8d, 8
       jl       G_M000_IG13
       cmp      esi, edx
       jae      G_M000_IG14
       mov      edx, esi
       vmovups  ymm9, ymmword ptr [rcx+4*rdx+0x10]
       xor      edi, edi

G_M000_IG03:                ;; offset=0x00A9
       mov      rax, gword ptr [rbx+0x18]
       mov      rcx, rax
       mov      edx, esi
       add      edx, dword ptr [rbx+0x54]
       js       G_M000_IG13
       mov      r8d, dword ptr [rcx+0x08]
       mov      r9d, r8d
       sub      r9d, edx
       cmp      r9d, 8
       jl       G_M000_IG13
       cmp      edx, r8d
       jae      G_M000_IG14
       vmovups  ymm10, ymmword ptr [rcx+4*rdx+0x10]
       lea      ebp, [rsi+0x01]
       test     ebp, ebp
       jge      SHORT G_M000_IG08
       jmp      G_M000_IG13

G_M000_IG04:                ;; offset=0x00EA
       vmovups  ymm0, ymmword ptr [rsp+0x70]
       vdivps   ymm0, ymm0, ymmword ptr [rsp+0x50]
       vmovups  ymmword ptr [rsp+0x30], ymm0

G_M000_IG05:                ;; offset=0x00FC
       vaddss   xmm0, xmm7, dword ptr [rsp+0x30]
       vaddss   xmm0, xmm0, dword ptr [rsp+0x34]
       vaddss   xmm0, xmm0, dword ptr [rsp+0x38]
       vaddss   xmm0, xmm0, dword ptr [rsp+0x3C]
       vaddss   xmm0, xmm0, dword ptr [rsp+0x40]
       vaddss   xmm0, xmm0, dword ptr [rsp+0x44]
       vaddss   xmm0, xmm0, dword ptr [rsp+0x48]
       vaddss   xmm7, xmm0, dword ptr [rsp+0x4C]
       vmovaps  ymm9, ymm10
       add      esi, dword ptr [rbx+0x54]
       inc      edi
       cmp      edi, 8
       jge      G_M000_IG11

G_M000_IG06:                ;; offset=0x013F
       cmp      edi, 7
       jne      G_M000_IG03

G_M000_IG07:                ;; offset=0x0148
       vmovaps  ymm10, ymm9
       mov      rax, gword ptr [rbx+0x18]
       lea      ebp, [rsi+0x01]
       test     ebp, ebp
       jl       G_M000_IG13

G_M000_IG08:                ;; offset=0x015C
       mov      ecx, dword ptr [rax+0x08]
       mov      edx, ecx
       sub      edx, ebp
       cmp      edx, 8
       jl       G_M000_IG13
       cmp      ebp, ecx
       jae      G_M000_IG14
       mov      ecx, ebp
       vsubps   ymm0, ymm9, ymmword ptr [rax+4*rcx+0x10]
       vbroadcastss ymm1, dword ptr [reloc @RWD04]
       vandps   ymm0, ymm0, ymm1
       vsubps   ymm2, ymm9, ymm10
       vandps   ymm1, ymm2, ymm1
       vmovaps  ymm2, ymm0
       vaddss   xmm2, xmm6, xmm2
       vmovaps  ymm4, ymm1
       vaddss   xmm2, xmm2, xmm4
       vmovaps  xmm4, xmm0
       vmovshdup xmm4, xmm4
       vaddss   xmm2, xmm2, xmm4
       vmovaps  xmm4, xmm1
       vmovshdup xmm4, xmm4
       vaddss   xmm2, xmm2, xmm4
       vmovaps  xmm4, xmm0
       vunpckhps xmm4, xmm4, xmm4
       vaddss   xmm2, xmm2, xmm4
       vmovaps  xmm4, xmm1
       vunpckhps xmm4, xmm4, xmm4
       vaddss   xmm2, xmm2, xmm4
       vmovaps  xmm4, xmm0
       vshufps  xmm4, xmm4, xmm4, -1
       vaddss   xmm2, xmm2, xmm4
       vmovaps  xmm4, xmm1
       vshufps  xmm4, xmm4, xmm4, -1
       vaddss   xmm2, xmm2, xmm4
       vextractf128 xmm4, ymm0
       vaddss   xmm2, xmm2, xmm4
       vextractf128 xmm4, ymm1
       vaddss   xmm2, xmm2, xmm4
       vextractf128 xmm4, ymm0
       vmovshdup xmm4, xmm4
       vaddss   xmm2, xmm2, xmm4
       vextractf128 xmm4, ymm1
       vmovshdup xmm4, xmm4
       vaddss   xmm2, xmm2, xmm4
       vextractf128 xmm0, ymm0
       vunpckhps xmm0, xmm0, xmm0
       vaddss   xmm2, xmm2, xmm0
       vextractf128 xmm0, ymm1
       vunpckhps xmm0, xmm0, xmm0
       vaddss   xmm0, xmm2, xmm0
       vextractf128 xmm1, ymm1
       vshufps  xmm1, xmm1, xmm1, -1
       vaddss   xmm6, xmm0, xmm1
       vaddps   ymm0, ymm9, ymm8
       movzx    rcx, byte  ptr [rbx+0x69]
       mov      rdx, gword ptr [rbx+0x10]
       vcmpneqps ymm1, ymm0, ymm0
       vxorps   ymm2, ymm2, ymm2
       vcmpgtps ymm2, ymm0, ymm2
       vorps    ymm1, ymm2, ymm1
       vandps   ymm0, ymm1, ymm0
       vmulps   ymm1, ymm0, ymm0
       vmulps   ymm2, ymm1, ymmword ptr [reloc @RWD08]

G_M000_IG09:                ;; offset=0x0276
       vaddps   ymm2, ymm2, ymmword ptr [reloc @RWD40]
       vmovups  ymmword ptr [rsp+0x70], ymm2
       vmulps   ymm0, ymm0, ymmword ptr [reloc @RWD72]
       vmulps   ymm0, ymm0, ymm1
       vaddps   ymm0, ymm0, ymmword ptr [reloc @RWD104]
       vmovups  ymmword ptr [rsp+0x50], ymm0
       test     ecx, ecx
       je       G_M000_IG04

G_M000_IG10:                ;; offset=0x02A6
       mov      gword ptr [rsp+0x20], rdx
       lea      rcx, [rsp+0x30]
       lea      rdx, [rsp+0x70]
       lea      r8, [rsp+0x50]
       mov      r9d, 1
       vextractf128 xmm9, ymm10
       vextractf128 xmm11, ymm8
       call     [DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Numerics.Vector`1[float],System.Numerics.Vector`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Numerics.Vector`1[float]]
       vinsertf128 ymm10, ymm10, xmm9
       vinsertf128 ymm8, ymm8, xmm11
       jmp      G_M000_IG05

G_M000_IG11:                ;; offset=0x02E3
       vmulss   xmm0, xmm7, dword ptr [reloc @RWD136]
       movzx    rdx, byte  ptr [rbx+0x69]
       mov      r8, gword ptr [rbx+0x10]
       call     [DotJpegli.Internal.AdaptiveQuantization:FastLog2(float,bool,DotJpegli.Internal.AqRationalDiagnostics):float]
       vmulss   xmm0, xmm0, dword ptr [reloc @RWD140]
       vmulss   xmm1, xmm6, dword ptr [reloc @RWD144]
       vaddss   xmm1, xmm1, dword ptr [rsp+0x148]
       vaddss   xmm0, xmm0, xmm1

G_M000_IG12:                ;; offset=0x0316
       vzeroupper
       vmovaps  xmm6, xmmword ptr [rsp+0xF0]
       vmovaps  xmm7, xmmword ptr [rsp+0xE0]
       vmovaps  xmm8, xmmword ptr [rsp+0xD0]
       vmovaps  xmm9, xmmword ptr [rsp+0xC0]
       vmovaps  xmm10, xmmword ptr [rsp+0xB0]
       vmovaps  xmm11, xmmword ptr [rsp+0xA0]
       add      rsp, 264
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       ret

G_M000_IG13:                ;; offset=0x035B
       call     [System.ThrowHelper:ThrowArgumentOutOfRange_IndexMustBeLessOrEqualException()]
       int3

G_M000_IG14:                ;; offset=0x0362
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	42233332h		;      40.8
RWD04  	dd	7FFFFFFFh		;       nan
RWD08  	dq	42EF0C2342EF0C23h, 42EF0C2342EF0C23h, 42EF0C2342EF0C23h, 42EF0C2342EF0C23h
RWD40  	dq	44228FFE44228FFEh, 44228FFE44228FFEh, 44228FFE44228FFEh, 44228FFE44228FFEh
RWD72  	dq	3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h, 3B1DEA783B1DEA78h
RWD104 	dq	449E380D449E380Dh, 449E380D449E380Dh, 449E380D449E380Dh, 449E380D449E380Dh
RWD136 	dd	38808081h		; 6.12745e-05
RWD140 	dd	BDDC6A08h		; -0.107624
RWD144 	dd	B8933E1Eh		; -7.02108e-05

; Total bytes of code 872

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:Dispose():this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 4 single block inlinees; 4 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rsi
       push     rbx
       sub      rsp, 40
       mov      rbx, rcx

G_M000_IG02:                ;; offset=0x0009
       cmp      byte  ptr [rbx+0x6A], 0
       jne      G_M000_IG08

G_M000_IG03:                ;; offset=0x0013
       mov      byte  ptr [rbx+0x6A], 1
       xor      rcx, rcx
       mov      gword ptr [rbx+0x38], rcx
       lea      rcx, bword ptr [rbx+0x18]
       mov      rdx, gword ptr [rcx]
       mov      rsi, 0x231A58E61B8
       mov      gword ptr [rcx], rsi
       cmp      dword ptr [rdx+0x08], 0
       je       SHORT G_M000_IG04
       mov      rcx, gword ptr [rbx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG04
       xor      r8d, r8d
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]
       call     [rax+0x28]System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG04:                ;; offset=0x004D
       lea      rcx, bword ptr [rbx+0x20]
       mov      rdx, gword ptr [rcx]
       mov      gword ptr [rcx], rsi
       cmp      dword ptr [rdx+0x08], 0
       je       SHORT G_M000_IG05
       mov      rcx, gword ptr [rbx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG05
       xor      r8d, r8d
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]
       call     [rax+0x28]System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG05:                ;; offset=0x0073
       lea      rcx, bword ptr [rbx+0x28]
       mov      rdx, gword ptr [rcx]
       mov      gword ptr [rcx], rsi
       cmp      dword ptr [rdx+0x08], 0
       je       SHORT G_M000_IG06
       mov      rcx, gword ptr [rbx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG06
       xor      r8d, r8d
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]
       call     [rax+0x28]System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG06:                ;; offset=0x0099
       lea      rcx, bword ptr [rbx+0x30]
       mov      rdx, gword ptr [rcx]
       mov      gword ptr [rcx], rsi
       cmp      dword ptr [rdx+0x08], 0
       je       SHORT G_M000_IG08
       mov      rcx, gword ptr [rbx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG08
       xor      r8d, r8d
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]

G_M000_IG07:                ;; offset=0x00BC
       add      rsp, 40
       pop      rbx
       pop      rsi
       tail.jmp [rax+0x28]System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG08:                ;; offset=0x00C6
       add      rsp, 40
       pop      rbx
       pop      rsi
       ret

; Total bytes of code 205

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:Dispose():this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 4 single block inlinees; 4 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rsi
       push     rbx
       sub      rsp, 40
       mov      rbx, rcx

G_M000_IG02:                ;; offset=0x0009
       cmp      byte  ptr [rbx+0x61], 0
       jne      G_M000_IG08

G_M000_IG03:                ;; offset=0x0013
       mov      byte  ptr [rbx+0x61], 1
       xor      rcx, rcx
       mov      gword ptr [rbx+0x30], rcx
       lea      rcx, bword ptr [rbx+0x10]
       mov      rdx, gword ptr [rcx]
       mov      rsi, 0x231A58E61B8
       mov      gword ptr [rcx], rsi
       cmp      dword ptr [rdx+0x08], 0
       je       SHORT G_M000_IG04
       mov      rcx, gword ptr [rbx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG04
       xor      r8d, r8d
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]
       call     [rax+0x28]System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG04:                ;; offset=0x004D
       lea      rcx, bword ptr [rbx+0x18]
       mov      rdx, gword ptr [rcx]
       mov      gword ptr [rcx], rsi
       cmp      dword ptr [rdx+0x08], 0
       je       SHORT G_M000_IG05
       mov      rcx, gword ptr [rbx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG05
       xor      r8d, r8d
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]
       call     [rax+0x28]System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG05:                ;; offset=0x0073
       lea      rcx, bword ptr [rbx+0x20]
       mov      rdx, gword ptr [rcx]
       mov      gword ptr [rcx], rsi
       cmp      dword ptr [rdx+0x08], 0
       je       SHORT G_M000_IG06
       mov      rcx, gword ptr [rbx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG06
       xor      r8d, r8d
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]
       call     [rax+0x28]System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG06:                ;; offset=0x0099
       lea      rcx, bword ptr [rbx+0x28]
       mov      rdx, gword ptr [rcx]
       mov      gword ptr [rcx], rsi
       cmp      dword ptr [rdx+0x08], 0
       je       SHORT G_M000_IG08
       mov      rcx, gword ptr [rbx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG08
       xor      r8d, r8d
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]

G_M000_IG07:                ;; offset=0x00BC
       add      rsp, 40
       pop      rbx
       pop      rsi
       tail.jmp [rax+0x28]System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG08:                ;; offset=0x00C6
       add      rsp, 40
       pop      rbx
       pop      rsi
       ret

; Total bytes of code 205

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:Dispose():this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 5 single block inlinees; 5 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     rsi
       push     rbx
       sub      rsp, 40
       mov      rbx, rcx

G_M000_IG02:                ;; offset=0x0009
       cmp      byte  ptr [rbx+0x69], 0
       jne      G_M000_IG09

G_M000_IG03:                ;; offset=0x0013
       mov      byte  ptr [rbx+0x69], 1
       xor      rcx, rcx
       mov      gword ptr [rbx+0x38], rcx
       lea      rcx, bword ptr [rbx+0x10]
       mov      rdx, gword ptr [rcx]
       mov      rsi, 0x231A58E61B8
       mov      gword ptr [rcx], rsi
       cmp      dword ptr [rdx+0x08], 0
       je       SHORT G_M000_IG04
       mov      rcx, gword ptr [rbx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG04
       xor      r8d, r8d
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]
       call     [rax+0x28]System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG04:                ;; offset=0x004D
       lea      rcx, bword ptr [rbx+0x18]
       mov      rdx, gword ptr [rcx]
       mov      gword ptr [rcx], rsi
       cmp      dword ptr [rdx+0x08], 0
       je       SHORT G_M000_IG05
       mov      rcx, gword ptr [rbx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG05
       xor      r8d, r8d
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]
       call     [rax+0x28]System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG05:                ;; offset=0x0073
       lea      rcx, bword ptr [rbx+0x20]
       mov      rdx, gword ptr [rcx]
       mov      gword ptr [rcx], rsi
       cmp      dword ptr [rdx+0x08], 0
       je       SHORT G_M000_IG06
       mov      rcx, gword ptr [rbx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG06
       xor      r8d, r8d
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]
       call     [rax+0x28]System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG06:                ;; offset=0x0099
       lea      rcx, bword ptr [rbx+0x28]
       mov      rdx, gword ptr [rcx]
       mov      gword ptr [rcx], rsi
       cmp      dword ptr [rdx+0x08], 0
       je       SHORT G_M000_IG07
       mov      rcx, gword ptr [rbx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG07
       xor      r8d, r8d
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]
       call     [rax+0x28]System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG07:                ;; offset=0x00BF
       lea      rcx, bword ptr [rbx+0x30]
       mov      rdx, gword ptr [rcx]
       mov      gword ptr [rcx], rsi
       cmp      dword ptr [rdx+0x08], 0
       je       SHORT G_M000_IG09
       mov      rcx, gword ptr [rbx+0x08]
       test     rcx, rcx
       je       SHORT G_M000_IG09
       xor      r8d, r8d
       mov      rax, qword ptr [rcx]
       mov      rax, qword ptr [rax+0x40]

G_M000_IG08:                ;; offset=0x00E2
       add      rsp, 40
       pop      rbx
       pop      rsi
       tail.jmp [rax+0x28]System.Buffers.ArrayPool`1[float]:Return(float[],bool):this

G_M000_IG09:                ;; offset=0x00EC
       add      rsp, 40
       pop      rbx
       pop      rsi
       ret

; Total bytes of code 243

; Assembly listing for method DotJpegli.Internal.BaselineAdaptiveQuantization:ModulateEightBlocks(int,int,System.Span`1[System.Runtime.Intrinsics.Vector256`1[float]],System.Span`1[float]):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 36 single block inlinees; 30 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r15
       push     r14
       push     r13
       push     r12
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 392
       vmovaps  xmmword ptr [rsp+0x170], xmm6
       vmovaps  xmmword ptr [rsp+0x160], xmm7
       vmovaps  xmmword ptr [rsp+0x150], xmm8
       vmovaps  xmmword ptr [rsp+0x140], xmm9
       vmovaps  xmmword ptr [rsp+0x130], xmm10
       vmovaps  xmmword ptr [rsp+0x120], xmm11
       vmovaps  xmmword ptr [rsp+0x110], xmm12
       vmovaps  xmmword ptr [rsp+0x100], xmm13
       vmovaps  xmmword ptr [rsp+0xF0], xmm14
       vmovaps  xmmword ptr [rsp+0xE0], xmm15
       mov      rax, bword ptr [rsp+0x1F0]

G_M000_IG02:                ;; offset=0x0075
       mov      r10, bword ptr [rax]
       mov      bword ptr [rsp+0x28], r10
       mov      eax, dword ptr [rax+0x08]
       mov      r11, bword ptr [r9]
       mov      r9d, dword ptr [r9+0x08]
       cmp      r9d, 8
       jl       G_M000_IG23
       mov      ebx, 8
       cmp      r9d, 16
       jl       G_M000_IG23
       lea      r9, bword ptr [r11+0x100]
       mov      esi, 8
       imul     r8d, dword ptr [rcx+0x54]
       lea      edx, [r8+rdx+0x01]
       xor      r8d, r8d
       align    [0 bytes for IG03]

G_M000_IG03:                ;; offset=0x00B9
       mov      rdi, r8
       shl      rdi, 5
       add      rdi, r11
       mov      rbp, gword ptr [rcx+0x10]
       lea      r14d, [rdx+8*r8]
       test     rbp, rbp
       je       G_M000_IG23
       mov      r15d, dword ptr [rbp+0x08]
       mov      r13d, r14d
       add      r13, 8
       cmp      r15, r13
       jb       G_M000_IG23
       lea      rbp, bword ptr [rbp+4*r14+0x10]
       vmovups  ymm0, ymmword ptr [rbp]
       vmovups  ymmword ptr [rdi], ymm0
       inc      r8d
       cmp      r8d, 8
       jl       SHORT G_M000_IG03

G_M000_IG04:                ;; offset=0x00FF
       lea      r8, bword ptr [r11+0x20]
       lea      rdi, bword ptr [r11+0x40]
       lea      rbp, bword ptr [r11+0x60]
       lea      r14, bword ptr [r11+0x80]
       lea      r15, bword ptr [r11+0xA0]
       lea      r13, bword ptr [r11+0xC0]
       lea      r12, bword ptr [r11+0xE0]
       vmovups  ymm0, ymmword ptr [r11]
       vmovaps  xmm1, xmm0
       vmovups  ymm2, ymmword ptr [r8]
       vmovaps  xmm3, xmm2
       vunpcklps xmm4, xmm1, xmm3
       vunpckhps xmm1, xmm1, xmm3
       vinsertf128 ymm1, ymm4, xmm1
       vextractf128 xmm0, ymm0
       vextractf128 xmm2, ymm2
       vunpcklps xmm3, xmm0, xmm2
       vunpckhps xmm0, xmm0, xmm2
       vinsertf128 ymm0, ymm3, xmm0
       vmovups  ymm2, ymmword ptr [rdi]
       vmovaps  xmm3, xmm2
       vmovups  ymm4, ymmword ptr [rbp]
       vmovaps  xmm5, xmm4
       vunpcklps xmm6, xmm3, xmm5
       vunpckhps xmm3, xmm3, xmm5
       vinsertf128 ymm3, ymm6, xmm3
       vextractf128 xmm2, ymm2
       vextractf128 xmm4, ymm4
       vunpcklps xmm5, xmm2, xmm4
       vunpckhps xmm2, xmm2, xmm4
       vinsertf128 ymm2, ymm5, xmm2
       vmovups  ymm4, ymmword ptr [r14]
       vmovaps  xmm5, xmm4
       vmovups  ymm6, ymmword ptr [r15]
       vmovaps  xmm7, xmm6
       vunpcklps xmm8, xmm5, xmm7
       vunpckhps xmm5, xmm5, xmm7
       vinsertf128 ymm5, ymm8, xmm5
       vextractf128 xmm4, ymm4
       vextractf128 xmm6, ymm6
       vunpcklps xmm7, xmm4, xmm6
       vunpckhps xmm4, xmm4, xmm6
       vinsertf128 ymm4, ymm7, xmm4
       vmovups  ymm6, ymmword ptr [r13]
       vmovaps  xmm7, xmm6
       vmovups  ymm8, ymmword ptr [r12]
       vmovaps  xmm9, xmm8
       vunpcklps xmm10, xmm7, xmm9
       vunpckhps xmm7, xmm7, xmm9
       vinsertf128 ymm7, ymm10, xmm7
       vextractf128 xmm6, ymm6
       vextractf128 xmm8, ymm8
       vunpcklps xmm9, xmm6, xmm8
       vunpckhps xmm6, xmm6, xmm8
       vinsertf128 ymm6, ymm9, xmm6
       vmovaps  xmm8, xmm1
       vmovaps  xmm9, xmm3
       vunpcklpd xmm10, xmm8, xmm9
       vunpckhpd xmm8, xmm8, xmm9
       vinsertf128 ymm8, ymm10, xmm8
       vextractf128 xmm1, ymm1
       vextractf128 xmm3, ymm3
       vunpcklpd xmm9, xmm1, xmm3
       vunpckhpd xmm1, xmm1, xmm3
       vinsertf128 ymm1, ymm9, xmm1
       vmovaps  xmm3, xmm0

G_M000_IG05:                ;; offset=0x024B
       vmovaps  xmm9, xmm2
       vunpcklpd xmm10, xmm3, xmm9
       vunpckhpd xmm3, xmm3, xmm9
       vinsertf128 ymm3, ymm10, xmm3
       vextractf128 xmm0, ymm0
       vextractf128 xmm2, ymm2
       vunpcklpd xmm9, xmm0, xmm2
       vunpckhpd xmm0, xmm0, xmm2
       vinsertf128 ymm0, ymm9, xmm0
       vmovaps  xmm2, xmm5
       vmovaps  xmm9, xmm7
       vunpcklpd xmm10, xmm2, xmm9
       vunpckhpd xmm2, xmm2, xmm9
       vinsertf128 ymm2, ymm10, xmm2
       vextractf128 xmm5, ymm5
       vextractf128 xmm7, ymm7
       vunpcklpd xmm9, xmm5, xmm7
       vunpckhpd xmm5, xmm5, xmm7
       vinsertf128 ymm5, ymm9, xmm5
       vmovaps  xmm7, xmm4
       vmovaps  xmm9, xmm6
       vunpcklpd xmm10, xmm7, xmm9
       vunpckhpd xmm7, xmm7, xmm9
       vinsertf128 ymm7, ymm10, xmm7
       vextractf128 xmm4, ymm4
       vextractf128 xmm6, ymm6
       vunpcklpd xmm9, xmm4, xmm6
       vunpckhpd xmm4, xmm4, xmm6
       vinsertf128 ymm4, ymm9, xmm4
       vinsertf128 ymm6, ymm8, xmm2
       vmovups  ymmword ptr [r11], ymm6
       vextractf128 xmm6, ymm8
       vextractf128 xmm2, ymm2
       vinsertf128 ymm2, ymm6, xmm2
       vmovups  ymmword ptr [r8], ymm2
       vinsertf128 ymm2, ymm1, xmm5
       vmovups  ymmword ptr [rdi], ymm2
       vextractf128 xmm1, ymm1
       vextractf128 xmm2, ymm5
       vinsertf128 ymm1, ymm1, xmm2
       vmovups  ymmword ptr [rbp], ymm1
       vinsertf128 ymm1, ymm3, xmm7
       vmovups  ymmword ptr [r14], ymm1
       vextractf128 xmm1, ymm3
       vextractf128 xmm2, ymm7
       vinsertf128 ymm1, ymm1, xmm2
       vmovups  ymmword ptr [r15], ymm1
       vinsertf128 ymm1, ymm0, xmm4
       vmovups  ymmword ptr [r13], ymm1
       vextractf128 xmm0, ymm0
       vextractf128 xmm1, ymm4
       vinsertf128 ymm0, ymm0, xmm1
       vmovups  ymmword ptr [r12], ymm0
       vxorps   ymm0, ymm0, ymm0
       vxorps   ymm1, ymm1, ymm1
       vbroadcastss ymm2, dword ptr [reloc @RWD00]
       xor      r8d, r8d
       vbroadcastss ymm3, dword ptr [reloc @RWD04]
       vbroadcastss ymm4, dword ptr [reloc @RWD08]
       vbroadcastss ymm5, dword ptr [reloc @RWD12]
       vbroadcastss ymm6, dword ptr [reloc @RWD16]
       vbroadcastss ymm7, dword ptr [reloc @RWD20]
       jmp      G_M000_IG09

G_M000_IG06:                ;; offset=0x03AC
       align    [0 bytes for IG07]

G_M000_IG07:                ;; offset=0x03AC
       lea      r14d, [rbp+0x01]
       cmp      r14d, ebx
       jae      G_M000_IG24
       mov      r15d, r14d
       shl      r15, 5
       vmovups  ymm9, ymmword ptr [r11+r15]
       vsubps   ymm10, ymm8, ymm9
       vandps   ymm10, ymm10, ymm3
       vaddps   ymm0, ymm10, ymm0
       cmp      ebp, edi
       jae      G_M000_IG24
       mov      ebp, ebp
       shl      rbp, 5
       vsubps   ymm10, ymm8, ymmword ptr [r10+rbp]
       vandps   ymm10, ymm10, ymm3
       vaddps   ymm0, ymm10, ymm0
       vaddps   ymm10, ymm8, ymm2
       vcmpneqps ymm8, ymm10, ymm10
       vxorps   ymm11, ymm11, ymm11
       vcmpgtps ymm11, ymm10, ymm11
       vorps    ymm8, ymm11, ymm8
       vandps   ymm10, ymm8, ymm10
       vmulps   ymm8, ymm10, ymm10
       vmulps   ymm10, ymm10, ymm4
       vmulps   ymm10, ymm10, ymm8
       vaddps   ymm10, ymm10, ymm5
       vmulps   ymm8, ymm8, ymm6
       vaddps   ymm8, ymm8, ymm7
       vdivps   ymm8, ymm8, ymm10
       vaddps   ymm1, ymm8, ymm1
       vmovaps  ymm8, ymm9
       mov      ebp, r14d
       cmp      ebp, 7
       jl       G_M000_IG07
       jmp      G_M000_IG18

G_M000_IG08:                ;; offset=0x0447
       cmp      r8d, 7
       mov      r10d, ebx
       mov      ebx, esi
       mov      esi, r10d
       mov      r10, r9
       mov      r9, r11
       mov      r11, r10
       je       G_M000_IG13

G_M000_IG09:                ;; offset=0x0462
       mov      edi, edx
       add      edi, dword ptr [rcx+0x54]
       xor      ebp, ebp
       cmp      esi, 8
       jl       G_M000_IG21
       align    [0 bytes for IG10]

G_M000_IG10:                ;; offset=0x0472
       mov      r14d, ebp
       shl      r14, 5
       add      r14, r9
       mov      r15, gword ptr [rcx+0x10]
       lea      r13d, [rdi+8*rbp]
       test     r15, r15
       je       G_M000_IG23
       mov      r12d, dword ptr [r15+0x08]
       mov      r10d, r13d
       add      r10, 8
       cmp      r12, r10
       jb       G_M000_IG23
       mov      r10d, r13d
       lea      r10, bword ptr [r15+4*r10+0x10]
       vmovups  ymm8, ymmword ptr [r10]
       vmovups  ymmword ptr [r14], ymm8
       inc      ebp
       cmp      ebp, 8
       jl       SHORT G_M000_IG10

G_M000_IG11:                ;; offset=0x04BA
       lea      rbp, bword ptr [r9+0x20]
       lea      r14, bword ptr [r9+0x40]
       lea      r15, bword ptr [r9+0x60]
       lea      rdi, bword ptr [r9+0x80]
       lea      r13, bword ptr [r9+0xA0]
       lea      r12, bword ptr [r9+0xC0]
       lea      r10, bword ptr [r9+0xE0]
       vmovups  ymm8, ymmword ptr [r9]
       vmovaps  xmm9, xmm8
       vmovups  ymm10, ymmword ptr [rbp]
       vmovaps  xmm11, xmm10
       vunpcklps xmm12, xmm9, xmm11
       vunpckhps xmm9, xmm9, xmm11
       vinsertf128 ymm9, ymm12, xmm9
       vextractf128 xmm8, ymm8
       vextractf128 xmm10, ymm10
       vunpcklps xmm11, xmm8, xmm10
       vunpckhps xmm8, xmm8, xmm10
       vinsertf128 ymm8, ymm11, xmm8
       vmovups  ymm10, ymmword ptr [r14]
       vmovaps  xmm11, xmm10
       vmovups  ymm12, ymmword ptr [r15]
       vmovaps  xmm13, xmm12
       vunpcklps xmm14, xmm11, xmm13
       vunpckhps xmm11, xmm11, xmm13
       vinsertf128 ymm11, ymm14, xmm11
       vextractf128 xmm10, ymm10
       vextractf128 xmm12, ymm12
       vunpcklps xmm13, xmm10, xmm12
       vunpckhps xmm10, xmm10, xmm12
       vinsertf128 ymm10, ymm13, xmm10
       vmovups  ymmword ptr [rsp+0xB0], ymm10
       vmovups  ymm12, ymmword ptr [rdi]
       vmovaps  xmm13, xmm12
       vmovups  ymm14, ymmword ptr [r13]
       vmovaps  xmm15, xmm14
       vunpcklps xmm10, xmm13, xmm15
       vunpckhps xmm13, xmm13, xmm15
       vinsertf128 ymm10, ymm10, xmm13
       vmovups  ymmword ptr [rsp+0x90], ymm10
       vextractf128 xmm12, ymm12
       vextractf128 xmm13, ymm14
       vunpcklps xmm14, xmm12, xmm13
       vunpckhps xmm12, xmm12, xmm13
       vinsertf128 ymm12, ymm14, xmm12
       vmovups  ymmword ptr [rsp+0x70], ymm12
       vmovups  ymm13, ymmword ptr [r12]
       vmovaps  xmm14, xmm13
       vmovups  ymm15, ymmword ptr [r10]
       vmovaps  xmm12, xmm15
       vunpcklps xmm10, xmm14, xmm12
       vunpckhps xmm12, xmm14, xmm12
       vinsertf128 ymm10, ymm10, xmm12
       vextractf128 xmm12, ymm13
       vextractf128 xmm13, ymm15
       vunpcklps xmm14, xmm12, xmm13
       vunpckhps xmm12, xmm12, xmm13
       vinsertf128 ymm12, ymm14, xmm12
       vmovaps  xmm13, xmm9
       vmovaps  xmm14, xmm11
       vunpcklpd xmm15, xmm13, xmm14
       vunpckhpd xmm13, xmm13, xmm14
       vinsertf128 ymm13, ymm15, xmm13
       vextractf128 xmm9, ymm9
       vextractf128 xmm11, ymm11

G_M000_IG12:                ;; offset=0x0621
       vunpcklpd xmm14, xmm9, xmm11
       vunpckhpd xmm9, xmm9, xmm11
       vinsertf128 ymm9, ymm14, xmm9
       vmovaps  xmm11, xmm8
       vmovupd  xmm15, xmmword ptr [rsp+0xB0]
       vunpcklpd xmm14, xmm11, xmm15
       vunpckhpd xmm11, xmm11, xmm15
       vinsertf128 ymm11, ymm14, xmm11
       vmovups  ymmword ptr [rsp+0x50], ymm11
       vextractf128 xmm8, ymm8
       vmovups  ymm14, ymmword ptr [rsp+0xB0]
       vextractf128 xmm14, ymm14
       vunpcklpd xmm15, xmm8, xmm14
       vunpckhpd xmm8, xmm8, xmm14
       vinsertf128 ymm8, ymm15, xmm8
       vmovups  ymmword ptr [rsp+0x30], ymm8
       vmovupd  xmm15, xmmword ptr [rsp+0x90]
       vmovaps  xmm14, xmm10
       vunpcklpd xmm8, xmm15, xmm14
       vunpckhpd xmm14, xmm15, xmm14
       vinsertf128 ymm8, ymm8, xmm14
       vmovups  ymm14, ymmword ptr [rsp+0x90]
       vextractf128 xmm14, ymm14
       vextractf128 xmm10, ymm10
       vunpcklpd xmm15, xmm14, xmm10
       vunpckhpd xmm10, xmm14, xmm10
       vinsertf128 ymm10, ymm15, xmm10
       vmovupd  xmm15, xmmword ptr [rsp+0x70]
       vmovaps  xmm14, xmm12
       vunpcklpd xmm11, xmm15, xmm14
       vunpckhpd xmm14, xmm15, xmm14
       vinsertf128 ymm11, ymm11, xmm14
       vmovups  ymm14, ymmword ptr [rsp+0x70]
       vextractf128 xmm14, ymm14
       vextractf128 xmm12, ymm12
       vunpcklpd xmm15, xmm14, xmm12
       vunpckhpd xmm12, xmm14, xmm12
       vinsertf128 ymm12, ymm15, xmm12
       vinsertf128 ymm14, ymm13, xmm8
       vmovups  ymmword ptr [r9], ymm14
       vextractf128 xmm13, ymm13
       vextractf128 xmm8, ymm8
       vinsertf128 ymm8, ymm13, xmm8
       vmovups  ymmword ptr [rbp], ymm8
       vinsertf128 ymm8, ymm9, xmm10
       vmovups  ymmword ptr [r14], ymm8
       vextractf128 xmm8, ymm9
       vextractf128 xmm9, ymm10
       vinsertf128 ymm8, ymm8, xmm9
       vmovups  ymmword ptr [r15], ymm8
       vmovups  ymm8, ymmword ptr [rsp+0x50]
       vinsertf128 ymm9, ymm8, xmm11
       vmovups  ymmword ptr [rdi], ymm9
       vextractf128 xmm8, ymm8
       vextractf128 xmm9, ymm11
       vinsertf128 ymm8, ymm8, xmm9
       vmovups  ymmword ptr [r13], ymm8
       vmovups  ymm8, ymmword ptr [rsp+0x30]
       vinsertf128 ymm9, ymm8, xmm12
       vmovups  ymmword ptr [r12], ymm9
       vextractf128 xmm8, ymm8
       vextractf128 xmm9, ymm12
       vinsertf128 ymm8, ymm8, xmm9
       vmovups  ymmword ptr [r10], ymm8

G_M000_IG13:                ;; offset=0x0795
       cmp      r8d, 7
       jne      SHORT G_M000_IG15

G_M000_IG14:                ;; offset=0x079B
       mov      r10, r11
       mov      edi, ebx
       jmp      SHORT G_M000_IG16
       align    [0 bytes for IG17]

G_M000_IG15:                ;; offset=0x07A2
       mov      r10, r9
       mov      edi, esi

G_M000_IG16:                ;; offset=0x07A7
       test     ebx, ebx
       je       G_M000_IG24
       vmovups  ymm8, ymmword ptr [r11]
       xor      ebp, ebp
       cmp      edi, 7
       jl       G_M000_IG07

G_M000_IG17:                ;; offset=0x07BF
       lea      r14d, [rbp+0x01]
       cmp      r14d, ebx
       jae      G_M000_IG24
       mov      r15d, r14d
       shl      r15, 5
       vmovups  ymm9, ymmword ptr [r11+r15]
       vsubps   ymm10, ymm8, ymm9
       vandps   ymm10, ymm10, ymm3
       vaddps   ymm0, ymm10, ymm0
       mov      ebp, ebp
       shl      rbp, 5
       vsubps   ymm10, ymm8, ymmword ptr [r10+rbp]
       vandps   ymm10, ymm10, ymm3
       vaddps   ymm0, ymm10, ymm0
       vaddps   ymm10, ymm8, ymm2
       vcmpneqps ymm8, ymm10, ymm10
       vxorps   ymm11, ymm11, ymm11
       vcmpgtps ymm11, ymm10, ymm11
       vorps    ymm8, ymm11, ymm8
       vandps   ymm10, ymm8, ymm10
       vmulps   ymm8, ymm10, ymm10
       vmulps   ymm10, ymm10, ymm4
       vmulps   ymm10, ymm10, ymm8
       vaddps   ymm10, ymm10, ymm5
       vmulps   ymm8, ymm8, ymm6
       vaddps   ymm8, ymm8, ymm7
       vdivps   ymm8, ymm8, ymm10
       vaddps   ymm1, ymm8, ymm1
       vmovaps  ymm8, ymm9
       mov      ebp, r14d
       cmp      ebp, 7
       jl       G_M000_IG17

G_M000_IG18:                ;; offset=0x084D
       cmp      edi, 7
       jbe      G_M000_IG24
       vsubps   ymm9, ymm8, ymmword ptr [r10+0xE0]
       vandps   ymm9, ymm9, ymm3
       vaddps   ymm0, ymm9, ymm0
       vaddps   ymm8, ymm8, ymm2
       vcmpneqps ymm9, ymm8, ymm8
       vxorps   ymm10, ymm10, ymm10
       vcmpgtps ymm10, ymm8, ymm10
       vorps    ymm9, ymm10, ymm9
       vandps   ymm8, ymm9, ymm8
       vmulps   ymm9, ymm8, ymm8
       vmulps   ymm8, ymm8, ymm4
       vmulps   ymm8, ymm8, ymm9
       vaddps   ymm8, ymm8, ymm5
       vmulps   ymm9, ymm9, ymm6
       vaddps   ymm9, ymm9, ymm7
       vdivps   ymm8, ymm9, ymm8
       vaddps   ymm1, ymm8, ymm1
       add      edx, dword ptr [rcx+0x54]
       inc      r8d
       cmp      r8d, 8
       jl       G_M000_IG08

G_M000_IG19:                ;; offset=0x08B9
       cmp      eax, 8
       jl       G_M000_IG23
       vmulps   ymm0, ymm0, ymmword ptr [reloc @RWD24]
       mov      r10, bword ptr [rsp+0x28]
       vmovups  ymmword ptr [r10], ymm0
       cmp      eax, 16
       jl       G_M000_IG23
       add      r10, 32
       vmulps   ymm0, ymm1, ymmword ptr [reloc @RWD56]
       vmovups  ymmword ptr [r10], ymm0

G_M000_IG20:                ;; offset=0x08EE
       vzeroupper
       vmovaps  xmm6, xmmword ptr [rsp+0x170]
       vmovaps  xmm7, xmmword ptr [rsp+0x160]
       vmovaps  xmm8, xmmword ptr [rsp+0x150]
       vmovaps  xmm9, xmmword ptr [rsp+0x140]
       vmovaps  xmm10, xmmword ptr [rsp+0x130]
       vmovaps  xmm11, xmmword ptr [rsp+0x120]
       vmovaps  xmm12, xmmword ptr [rsp+0x110]
       vmovaps  xmm13, xmmword ptr [rsp+0x100]
       vmovaps  xmm14, xmmword ptr [rsp+0xF0]
       vmovaps  xmm15, xmmword ptr [rsp+0xE0]
       add      rsp, 392
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r12
       pop      r13
       pop      r14
       pop      r15
       ret

G_M000_IG21:                ;; offset=0x095F
       cmp      ebp, esi
       jae      G_M000_IG24
       mov      r14d, ebp
       shl      r14, 5
       add      r14, r9
       mov      r15, gword ptr [rcx+0x10]
       lea      r13d, [rdi+8*rbp]
       test     r15, r15
       je       SHORT G_M000_IG23
       mov      eax, dword ptr [r15+0x08]
       mov      edx, r13d
       add      rdx, 8
       cmp      rax, rdx
       jb       SHORT G_M000_IG23
       mov      eax, r13d
       lea      r10, bword ptr [r15+4*rax+0x10]
       vmovups  ymm0, ymmword ptr [r10]
       vmovups  ymmword ptr [r14], ymm0
       inc      ebp
       cmp      ebp, 8
       jl       SHORT G_M000_IG21

G_M000_IG22:                ;; offset=0x09A7
       mov      rcx, 0x7FF882B8F4B8
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      rcx, rbx
       mov      rdx, 0x231A58E6350
       mov      r8, 0x231A58E63C8
       call     [System.ArgumentException:.ctor(System.String,System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3

G_M000_IG23:                ;; offset=0x09DF
       call     [System.ThrowHelper:ThrowArgumentOutOfRangeException()]
       int3

G_M000_IG24:                ;; offset=0x09E6
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	42233332h		;      40.8
RWD04  	dd	7FFFFFFFh		;       nan
RWD08  	dd	3B1DEA78h		; 0.00240961
RWD12  	dd	449E380Dh		;   1265.75
RWD16  	dd	42EF0C23h		;   119.524
RWD20  	dd	44228FFEh		;    650.25
RWD24  	dq	B8933E1EB8933E1Eh, B8933E1EB8933E1Eh, B8933E1EB8933E1Eh, B8933E1EB8933E1Eh
RWD56  	dq	3880808138808081h, 3880808138808081h, 3880808138808081h, 3880808138808081h

; Total bytes of code 2540

; Assembly listing for method DotJpegli.Internal.StencilAdaptiveQuantization:ModulateEightBlocks(int,int,System.Span`1[System.Runtime.Intrinsics.Vector256`1[float]],System.Span`1[float]):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 36 single block inlinees; 30 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r15
       push     r14
       push     r13
       push     r12
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 392
       vmovaps  xmmword ptr [rsp+0x170], xmm6
       vmovaps  xmmword ptr [rsp+0x160], xmm7
       vmovaps  xmmword ptr [rsp+0x150], xmm8
       vmovaps  xmmword ptr [rsp+0x140], xmm9
       vmovaps  xmmword ptr [rsp+0x130], xmm10
       vmovaps  xmmword ptr [rsp+0x120], xmm11
       vmovaps  xmmword ptr [rsp+0x110], xmm12
       vmovaps  xmmword ptr [rsp+0x100], xmm13
       vmovaps  xmmword ptr [rsp+0xF0], xmm14
       vmovaps  xmmword ptr [rsp+0xE0], xmm15
       mov      rax, bword ptr [rsp+0x1F0]

G_M000_IG02:                ;; offset=0x0075
       mov      r10, bword ptr [rax]
       mov      bword ptr [rsp+0x28], r10
       mov      eax, dword ptr [rax+0x08]
       mov      r11, bword ptr [r9]
       mov      r9d, dword ptr [r9+0x08]
       cmp      r9d, 8
       jl       G_M000_IG23
       mov      ebx, 8
       cmp      r9d, 16
       jl       G_M000_IG23
       lea      r9, bword ptr [r11+0x100]
       mov      esi, 8
       imul     r8d, dword ptr [rcx+0x4C]
       lea      edx, [r8+rdx+0x01]
       xor      r8d, r8d
       align    [0 bytes for IG03]

G_M000_IG03:                ;; offset=0x00B9
       mov      rdi, r8
       shl      rdi, 5
       add      rdi, r11
       mov      rbp, gword ptr [rcx+0x10]
       lea      r14d, [rdx+8*r8]
       test     rbp, rbp
       je       G_M000_IG23
       mov      r15d, dword ptr [rbp+0x08]
       mov      r13d, r14d
       add      r13, 8
       cmp      r15, r13
       jb       G_M000_IG23
       lea      rbp, bword ptr [rbp+4*r14+0x10]
       vmovups  ymm0, ymmword ptr [rbp]
       vmovups  ymmword ptr [rdi], ymm0
       inc      r8d
       cmp      r8d, 8
       jl       SHORT G_M000_IG03

G_M000_IG04:                ;; offset=0x00FF
       lea      r8, bword ptr [r11+0x20]
       lea      rdi, bword ptr [r11+0x40]
       lea      rbp, bword ptr [r11+0x60]
       lea      r14, bword ptr [r11+0x80]
       lea      r15, bword ptr [r11+0xA0]
       lea      r13, bword ptr [r11+0xC0]
       lea      r12, bword ptr [r11+0xE0]
       vmovups  ymm0, ymmword ptr [r11]
       vmovaps  xmm1, xmm0
       vmovups  ymm2, ymmword ptr [r8]
       vmovaps  xmm3, xmm2
       vunpcklps xmm4, xmm1, xmm3
       vunpckhps xmm1, xmm1, xmm3
       vinsertf128 ymm1, ymm4, xmm1
       vextractf128 xmm0, ymm0
       vextractf128 xmm2, ymm2
       vunpcklps xmm3, xmm0, xmm2
       vunpckhps xmm0, xmm0, xmm2
       vinsertf128 ymm0, ymm3, xmm0
       vmovups  ymm2, ymmword ptr [rdi]
       vmovaps  xmm3, xmm2
       vmovups  ymm4, ymmword ptr [rbp]
       vmovaps  xmm5, xmm4
       vunpcklps xmm6, xmm3, xmm5
       vunpckhps xmm3, xmm3, xmm5
       vinsertf128 ymm3, ymm6, xmm3
       vextractf128 xmm2, ymm2
       vextractf128 xmm4, ymm4
       vunpcklps xmm5, xmm2, xmm4
       vunpckhps xmm2, xmm2, xmm4
       vinsertf128 ymm2, ymm5, xmm2
       vmovups  ymm4, ymmword ptr [r14]
       vmovaps  xmm5, xmm4
       vmovups  ymm6, ymmword ptr [r15]
       vmovaps  xmm7, xmm6
       vunpcklps xmm8, xmm5, xmm7
       vunpckhps xmm5, xmm5, xmm7
       vinsertf128 ymm5, ymm8, xmm5
       vextractf128 xmm4, ymm4
       vextractf128 xmm6, ymm6
       vunpcklps xmm7, xmm4, xmm6
       vunpckhps xmm4, xmm4, xmm6
       vinsertf128 ymm4, ymm7, xmm4
       vmovups  ymm6, ymmword ptr [r13]
       vmovaps  xmm7, xmm6
       vmovups  ymm8, ymmword ptr [r12]
       vmovaps  xmm9, xmm8
       vunpcklps xmm10, xmm7, xmm9
       vunpckhps xmm7, xmm7, xmm9
       vinsertf128 ymm7, ymm10, xmm7
       vextractf128 xmm6, ymm6
       vextractf128 xmm8, ymm8
       vunpcklps xmm9, xmm6, xmm8
       vunpckhps xmm6, xmm6, xmm8
       vinsertf128 ymm6, ymm9, xmm6
       vmovaps  xmm8, xmm1
       vmovaps  xmm9, xmm3
       vunpcklpd xmm10, xmm8, xmm9
       vunpckhpd xmm8, xmm8, xmm9
       vinsertf128 ymm8, ymm10, xmm8
       vextractf128 xmm1, ymm1
       vextractf128 xmm3, ymm3
       vunpcklpd xmm9, xmm1, xmm3
       vunpckhpd xmm1, xmm1, xmm3
       vinsertf128 ymm1, ymm9, xmm1
       vmovaps  xmm3, xmm0

G_M000_IG05:                ;; offset=0x024B
       vmovaps  xmm9, xmm2
       vunpcklpd xmm10, xmm3, xmm9
       vunpckhpd xmm3, xmm3, xmm9
       vinsertf128 ymm3, ymm10, xmm3
       vextractf128 xmm0, ymm0
       vextractf128 xmm2, ymm2
       vunpcklpd xmm9, xmm0, xmm2
       vunpckhpd xmm0, xmm0, xmm2
       vinsertf128 ymm0, ymm9, xmm0
       vmovaps  xmm2, xmm5
       vmovaps  xmm9, xmm7
       vunpcklpd xmm10, xmm2, xmm9
       vunpckhpd xmm2, xmm2, xmm9
       vinsertf128 ymm2, ymm10, xmm2
       vextractf128 xmm5, ymm5
       vextractf128 xmm7, ymm7
       vunpcklpd xmm9, xmm5, xmm7
       vunpckhpd xmm5, xmm5, xmm7
       vinsertf128 ymm5, ymm9, xmm5
       vmovaps  xmm7, xmm4
       vmovaps  xmm9, xmm6
       vunpcklpd xmm10, xmm7, xmm9
       vunpckhpd xmm7, xmm7, xmm9
       vinsertf128 ymm7, ymm10, xmm7
       vextractf128 xmm4, ymm4
       vextractf128 xmm6, ymm6
       vunpcklpd xmm9, xmm4, xmm6
       vunpckhpd xmm4, xmm4, xmm6
       vinsertf128 ymm4, ymm9, xmm4
       vinsertf128 ymm6, ymm8, xmm2
       vmovups  ymmword ptr [r11], ymm6
       vextractf128 xmm6, ymm8
       vextractf128 xmm2, ymm2
       vinsertf128 ymm2, ymm6, xmm2
       vmovups  ymmword ptr [r8], ymm2
       vinsertf128 ymm2, ymm1, xmm5
       vmovups  ymmword ptr [rdi], ymm2
       vextractf128 xmm1, ymm1
       vextractf128 xmm2, ymm5
       vinsertf128 ymm1, ymm1, xmm2
       vmovups  ymmword ptr [rbp], ymm1
       vinsertf128 ymm1, ymm3, xmm7
       vmovups  ymmword ptr [r14], ymm1
       vextractf128 xmm1, ymm3
       vextractf128 xmm2, ymm7
       vinsertf128 ymm1, ymm1, xmm2
       vmovups  ymmword ptr [r15], ymm1
       vinsertf128 ymm1, ymm0, xmm4
       vmovups  ymmword ptr [r13], ymm1
       vextractf128 xmm0, ymm0
       vextractf128 xmm1, ymm4
       vinsertf128 ymm0, ymm0, xmm1
       vmovups  ymmword ptr [r12], ymm0
       vxorps   ymm0, ymm0, ymm0
       vxorps   ymm1, ymm1, ymm1
       vbroadcastss ymm2, dword ptr [reloc @RWD00]
       xor      r8d, r8d
       vbroadcastss ymm3, dword ptr [reloc @RWD04]
       vbroadcastss ymm4, dword ptr [reloc @RWD08]
       vbroadcastss ymm5, dword ptr [reloc @RWD12]
       vbroadcastss ymm6, dword ptr [reloc @RWD16]
       vbroadcastss ymm7, dword ptr [reloc @RWD20]
       jmp      G_M000_IG09

G_M000_IG06:                ;; offset=0x03AC
       align    [0 bytes for IG07]

G_M000_IG07:                ;; offset=0x03AC
       lea      r14d, [rbp+0x01]
       cmp      r14d, ebx
       jae      G_M000_IG24
       mov      r15d, r14d
       shl      r15, 5
       vmovups  ymm9, ymmword ptr [r11+r15]
       vsubps   ymm10, ymm8, ymm9
       vandps   ymm10, ymm10, ymm3
       vaddps   ymm0, ymm10, ymm0
       cmp      ebp, edi
       jae      G_M000_IG24
       mov      ebp, ebp
       shl      rbp, 5
       vsubps   ymm10, ymm8, ymmword ptr [r10+rbp]
       vandps   ymm10, ymm10, ymm3
       vaddps   ymm0, ymm10, ymm0
       vaddps   ymm10, ymm8, ymm2
       vcmpneqps ymm8, ymm10, ymm10
       vxorps   ymm11, ymm11, ymm11
       vcmpgtps ymm11, ymm10, ymm11
       vorps    ymm8, ymm11, ymm8
       vandps   ymm10, ymm8, ymm10
       vmulps   ymm8, ymm10, ymm10
       vmulps   ymm10, ymm10, ymm4
       vmulps   ymm10, ymm10, ymm8
       vaddps   ymm10, ymm10, ymm5
       vmulps   ymm8, ymm8, ymm6
       vaddps   ymm8, ymm8, ymm7
       vdivps   ymm8, ymm8, ymm10
       vaddps   ymm1, ymm8, ymm1
       vmovaps  ymm8, ymm9
       mov      ebp, r14d
       cmp      ebp, 7
       jl       G_M000_IG07
       jmp      G_M000_IG18

G_M000_IG08:                ;; offset=0x0447
       cmp      r8d, 7
       mov      r10d, ebx
       mov      ebx, esi
       mov      esi, r10d
       mov      r10, r9
       mov      r9, r11
       mov      r11, r10
       je       G_M000_IG13

G_M000_IG09:                ;; offset=0x0462
       mov      edi, edx
       add      edi, dword ptr [rcx+0x4C]
       xor      ebp, ebp
       cmp      esi, 8
       jl       G_M000_IG21
       align    [0 bytes for IG10]

G_M000_IG10:                ;; offset=0x0472
       mov      r14d, ebp
       shl      r14, 5
       add      r14, r9
       mov      r15, gword ptr [rcx+0x10]
       lea      r13d, [rdi+8*rbp]
       test     r15, r15
       je       G_M000_IG23
       mov      r12d, dword ptr [r15+0x08]
       mov      r10d, r13d
       add      r10, 8
       cmp      r12, r10
       jb       G_M000_IG23
       mov      r10d, r13d
       lea      r10, bword ptr [r15+4*r10+0x10]
       vmovups  ymm8, ymmword ptr [r10]
       vmovups  ymmword ptr [r14], ymm8
       inc      ebp
       cmp      ebp, 8
       jl       SHORT G_M000_IG10

G_M000_IG11:                ;; offset=0x04BA
       lea      rbp, bword ptr [r9+0x20]
       lea      r14, bword ptr [r9+0x40]
       lea      r15, bword ptr [r9+0x60]
       lea      rdi, bword ptr [r9+0x80]
       lea      r13, bword ptr [r9+0xA0]
       lea      r12, bword ptr [r9+0xC0]
       lea      r10, bword ptr [r9+0xE0]
       vmovups  ymm8, ymmword ptr [r9]
       vmovaps  xmm9, xmm8
       vmovups  ymm10, ymmword ptr [rbp]
       vmovaps  xmm11, xmm10
       vunpcklps xmm12, xmm9, xmm11
       vunpckhps xmm9, xmm9, xmm11
       vinsertf128 ymm9, ymm12, xmm9
       vextractf128 xmm8, ymm8
       vextractf128 xmm10, ymm10
       vunpcklps xmm11, xmm8, xmm10
       vunpckhps xmm8, xmm8, xmm10
       vinsertf128 ymm8, ymm11, xmm8
       vmovups  ymm10, ymmword ptr [r14]
       vmovaps  xmm11, xmm10
       vmovups  ymm12, ymmword ptr [r15]
       vmovaps  xmm13, xmm12
       vunpcklps xmm14, xmm11, xmm13
       vunpckhps xmm11, xmm11, xmm13
       vinsertf128 ymm11, ymm14, xmm11
       vextractf128 xmm10, ymm10
       vextractf128 xmm12, ymm12
       vunpcklps xmm13, xmm10, xmm12
       vunpckhps xmm10, xmm10, xmm12
       vinsertf128 ymm10, ymm13, xmm10
       vmovups  ymmword ptr [rsp+0xB0], ymm10
       vmovups  ymm12, ymmword ptr [rdi]
       vmovaps  xmm13, xmm12
       vmovups  ymm14, ymmword ptr [r13]
       vmovaps  xmm15, xmm14
       vunpcklps xmm10, xmm13, xmm15
       vunpckhps xmm13, xmm13, xmm15
       vinsertf128 ymm10, ymm10, xmm13
       vmovups  ymmword ptr [rsp+0x90], ymm10
       vextractf128 xmm12, ymm12
       vextractf128 xmm13, ymm14
       vunpcklps xmm14, xmm12, xmm13
       vunpckhps xmm12, xmm12, xmm13
       vinsertf128 ymm12, ymm14, xmm12
       vmovups  ymmword ptr [rsp+0x70], ymm12
       vmovups  ymm13, ymmword ptr [r12]
       vmovaps  xmm14, xmm13
       vmovups  ymm15, ymmword ptr [r10]
       vmovaps  xmm12, xmm15
       vunpcklps xmm10, xmm14, xmm12
       vunpckhps xmm12, xmm14, xmm12
       vinsertf128 ymm10, ymm10, xmm12
       vextractf128 xmm12, ymm13
       vextractf128 xmm13, ymm15
       vunpcklps xmm14, xmm12, xmm13
       vunpckhps xmm12, xmm12, xmm13
       vinsertf128 ymm12, ymm14, xmm12
       vmovaps  xmm13, xmm9
       vmovaps  xmm14, xmm11
       vunpcklpd xmm15, xmm13, xmm14
       vunpckhpd xmm13, xmm13, xmm14
       vinsertf128 ymm13, ymm15, xmm13
       vextractf128 xmm9, ymm9
       vextractf128 xmm11, ymm11

G_M000_IG12:                ;; offset=0x0621
       vunpcklpd xmm14, xmm9, xmm11
       vunpckhpd xmm9, xmm9, xmm11
       vinsertf128 ymm9, ymm14, xmm9
       vmovaps  xmm11, xmm8
       vmovupd  xmm15, xmmword ptr [rsp+0xB0]
       vunpcklpd xmm14, xmm11, xmm15
       vunpckhpd xmm11, xmm11, xmm15
       vinsertf128 ymm11, ymm14, xmm11
       vmovups  ymmword ptr [rsp+0x50], ymm11
       vextractf128 xmm8, ymm8
       vmovups  ymm14, ymmword ptr [rsp+0xB0]
       vextractf128 xmm14, ymm14
       vunpcklpd xmm15, xmm8, xmm14
       vunpckhpd xmm8, xmm8, xmm14
       vinsertf128 ymm8, ymm15, xmm8
       vmovups  ymmword ptr [rsp+0x30], ymm8
       vmovupd  xmm15, xmmword ptr [rsp+0x90]
       vmovaps  xmm14, xmm10
       vunpcklpd xmm8, xmm15, xmm14
       vunpckhpd xmm14, xmm15, xmm14
       vinsertf128 ymm8, ymm8, xmm14
       vmovups  ymm14, ymmword ptr [rsp+0x90]
       vextractf128 xmm14, ymm14
       vextractf128 xmm10, ymm10
       vunpcklpd xmm15, xmm14, xmm10
       vunpckhpd xmm10, xmm14, xmm10
       vinsertf128 ymm10, ymm15, xmm10
       vmovupd  xmm15, xmmword ptr [rsp+0x70]
       vmovaps  xmm14, xmm12
       vunpcklpd xmm11, xmm15, xmm14
       vunpckhpd xmm14, xmm15, xmm14
       vinsertf128 ymm11, ymm11, xmm14
       vmovups  ymm14, ymmword ptr [rsp+0x70]
       vextractf128 xmm14, ymm14
       vextractf128 xmm12, ymm12
       vunpcklpd xmm15, xmm14, xmm12
       vunpckhpd xmm12, xmm14, xmm12
       vinsertf128 ymm12, ymm15, xmm12
       vinsertf128 ymm14, ymm13, xmm8
       vmovups  ymmword ptr [r9], ymm14
       vextractf128 xmm13, ymm13
       vextractf128 xmm8, ymm8
       vinsertf128 ymm8, ymm13, xmm8
       vmovups  ymmword ptr [rbp], ymm8
       vinsertf128 ymm8, ymm9, xmm10
       vmovups  ymmword ptr [r14], ymm8
       vextractf128 xmm8, ymm9
       vextractf128 xmm9, ymm10
       vinsertf128 ymm8, ymm8, xmm9
       vmovups  ymmword ptr [r15], ymm8
       vmovups  ymm8, ymmword ptr [rsp+0x50]
       vinsertf128 ymm9, ymm8, xmm11
       vmovups  ymmword ptr [rdi], ymm9
       vextractf128 xmm8, ymm8
       vextractf128 xmm9, ymm11
       vinsertf128 ymm8, ymm8, xmm9
       vmovups  ymmword ptr [r13], ymm8
       vmovups  ymm8, ymmword ptr [rsp+0x30]
       vinsertf128 ymm9, ymm8, xmm12
       vmovups  ymmword ptr [r12], ymm9
       vextractf128 xmm8, ymm8
       vextractf128 xmm9, ymm12
       vinsertf128 ymm8, ymm8, xmm9
       vmovups  ymmword ptr [r10], ymm8

G_M000_IG13:                ;; offset=0x0795
       cmp      r8d, 7
       jne      SHORT G_M000_IG15

G_M000_IG14:                ;; offset=0x079B
       mov      r10, r11
       mov      edi, ebx
       jmp      SHORT G_M000_IG16
       align    [0 bytes for IG17]

G_M000_IG15:                ;; offset=0x07A2
       mov      r10, r9
       mov      edi, esi

G_M000_IG16:                ;; offset=0x07A7
       test     ebx, ebx
       je       G_M000_IG24
       vmovups  ymm8, ymmword ptr [r11]
       xor      ebp, ebp
       cmp      edi, 7
       jl       G_M000_IG07

G_M000_IG17:                ;; offset=0x07BF
       lea      r14d, [rbp+0x01]
       cmp      r14d, ebx
       jae      G_M000_IG24
       mov      r15d, r14d
       shl      r15, 5
       vmovups  ymm9, ymmword ptr [r11+r15]
       vsubps   ymm10, ymm8, ymm9
       vandps   ymm10, ymm10, ymm3
       vaddps   ymm0, ymm10, ymm0
       mov      ebp, ebp
       shl      rbp, 5
       vsubps   ymm10, ymm8, ymmword ptr [r10+rbp]
       vandps   ymm10, ymm10, ymm3
       vaddps   ymm0, ymm10, ymm0
       vaddps   ymm10, ymm8, ymm2
       vcmpneqps ymm8, ymm10, ymm10
       vxorps   ymm11, ymm11, ymm11
       vcmpgtps ymm11, ymm10, ymm11
       vorps    ymm8, ymm11, ymm8
       vandps   ymm10, ymm8, ymm10
       vmulps   ymm8, ymm10, ymm10
       vmulps   ymm10, ymm10, ymm4
       vmulps   ymm10, ymm10, ymm8
       vaddps   ymm10, ymm10, ymm5
       vmulps   ymm8, ymm8, ymm6
       vaddps   ymm8, ymm8, ymm7
       vdivps   ymm8, ymm8, ymm10
       vaddps   ymm1, ymm8, ymm1
       vmovaps  ymm8, ymm9
       mov      ebp, r14d
       cmp      ebp, 7
       jl       G_M000_IG17

G_M000_IG18:                ;; offset=0x084D
       cmp      edi, 7
       jbe      G_M000_IG24
       vsubps   ymm9, ymm8, ymmword ptr [r10+0xE0]
       vandps   ymm9, ymm9, ymm3
       vaddps   ymm0, ymm9, ymm0
       vaddps   ymm8, ymm8, ymm2
       vcmpneqps ymm9, ymm8, ymm8
       vxorps   ymm10, ymm10, ymm10
       vcmpgtps ymm10, ymm8, ymm10
       vorps    ymm9, ymm10, ymm9
       vandps   ymm8, ymm9, ymm8
       vmulps   ymm9, ymm8, ymm8
       vmulps   ymm8, ymm8, ymm4
       vmulps   ymm8, ymm8, ymm9
       vaddps   ymm8, ymm8, ymm5
       vmulps   ymm9, ymm9, ymm6
       vaddps   ymm9, ymm9, ymm7
       vdivps   ymm8, ymm9, ymm8
       vaddps   ymm1, ymm8, ymm1
       add      edx, dword ptr [rcx+0x4C]
       inc      r8d
       cmp      r8d, 8
       jl       G_M000_IG08

G_M000_IG19:                ;; offset=0x08B9
       cmp      eax, 8
       jl       G_M000_IG23
       vmulps   ymm0, ymm0, ymmword ptr [reloc @RWD24]
       mov      r10, bword ptr [rsp+0x28]
       vmovups  ymmword ptr [r10], ymm0
       cmp      eax, 16
       jl       G_M000_IG23
       add      r10, 32
       vmulps   ymm0, ymm1, ymmword ptr [reloc @RWD56]
       vmovups  ymmword ptr [r10], ymm0

G_M000_IG20:                ;; offset=0x08EE
       vzeroupper
       vmovaps  xmm6, xmmword ptr [rsp+0x170]
       vmovaps  xmm7, xmmword ptr [rsp+0x160]
       vmovaps  xmm8, xmmword ptr [rsp+0x150]
       vmovaps  xmm9, xmmword ptr [rsp+0x140]
       vmovaps  xmm10, xmmword ptr [rsp+0x130]
       vmovaps  xmm11, xmmword ptr [rsp+0x120]
       vmovaps  xmm12, xmmword ptr [rsp+0x110]
       vmovaps  xmm13, xmmword ptr [rsp+0x100]
       vmovaps  xmm14, xmmword ptr [rsp+0xF0]
       vmovaps  xmm15, xmmword ptr [rsp+0xE0]
       add      rsp, 392
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r12
       pop      r13
       pop      r14
       pop      r15
       ret

G_M000_IG21:                ;; offset=0x095F
       cmp      ebp, esi
       jae      G_M000_IG24
       mov      r14d, ebp
       shl      r14, 5
       add      r14, r9
       mov      r15, gword ptr [rcx+0x10]
       lea      r13d, [rdi+8*rbp]
       test     r15, r15
       je       SHORT G_M000_IG23
       mov      eax, dword ptr [r15+0x08]
       mov      edx, r13d
       add      rdx, 8
       cmp      rax, rdx
       jb       SHORT G_M000_IG23
       mov      eax, r13d
       lea      r10, bword ptr [r15+4*rax+0x10]
       vmovups  ymm0, ymmword ptr [r10]
       vmovups  ymmword ptr [r14], ymm0
       inc      ebp
       cmp      ebp, 8
       jl       SHORT G_M000_IG21

G_M000_IG22:                ;; offset=0x09A7
       mov      rcx, 0x7FF882B8F4B8
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      rcx, rbx
       mov      rdx, 0x231A58E6350
       mov      r8, 0x231A58E63C8
       call     [System.ArgumentException:.ctor(System.String,System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3

G_M000_IG23:                ;; offset=0x09DF
       call     [System.ThrowHelper:ThrowArgumentOutOfRangeException()]
       int3

G_M000_IG24:                ;; offset=0x09E6
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	42233332h		;      40.8
RWD04  	dd	7FFFFFFFh		;       nan
RWD08  	dd	3B1DEA78h		; 0.00240961
RWD12  	dd	449E380Dh		;   1265.75
RWD16  	dd	42EF0C23h		;   119.524
RWD20  	dd	44228FFEh		;    650.25
RWD24  	dq	B8933E1EB8933E1Eh, B8933E1EB8933E1Eh, B8933E1EB8933E1Eh, B8933E1EB8933E1Eh
RWD56  	dq	3880808138808081h, 3880808138808081h, 3880808138808081h, 3880808138808081h

; Total bytes of code 2540

; Assembly listing for method DotJpegli.Internal.AdaptiveQuantization:ModulateEightBlocks(int,int,System.Span`1[System.Runtime.Intrinsics.Vector256`1[float]],System.Span`1[float]):this (FullOpts)
; Emitting BLENDED_CODE for x64 + VEX on Windows
; FullOpts code
; optimized code
; rsp based frame
; fully interruptible
; No PGO data
; 0 inlinees with PGO data; 36 single block inlinees; 32 inlinees without PGO data

G_M000_IG01:                ;; offset=0x0000
       push     r15
       push     r14
       push     r13
       push     r12
       push     rdi
       push     rsi
       push     rbp
       push     rbx
       sub      rsp, 920
       vmovaps  xmmword ptr [rsp+0x380], xmm6
       vmovaps  xmmword ptr [rsp+0x370], xmm7
       vmovaps  xmmword ptr [rsp+0x360], xmm8
       vmovaps  xmmword ptr [rsp+0x350], xmm9
       vmovaps  xmmword ptr [rsp+0x340], xmm10
       vmovaps  xmmword ptr [rsp+0x330], xmm11
       vmovaps  xmmword ptr [rsp+0x320], xmm12
       vmovaps  xmmword ptr [rsp+0x310], xmm13
       vmovaps  xmmword ptr [rsp+0x300], xmm14
       vmovaps  xmmword ptr [rsp+0x2F0], xmm15
       vxorps   xmm4, xmm4, xmm4
       vmovdqu  ymmword ptr [rsp+0x140], ymm4
       vmovdqu  ymmword ptr [rsp+0x160], ymm4
       vmovdqu  ymmword ptr [rsp+0x180], ymm4
       vmovdqu  ymmword ptr [rsp+0x1A0], ymm4
       mov      rbx, rcx
       mov      rcx, bword ptr [rsp+0x400]

G_M000_IG02:                ;; offset=0x00A0
       mov      rsi, bword ptr [rcx]
       mov      bword ptr [rsp+0x30], rsi
       mov      edi, dword ptr [rcx+0x08]
       mov      rbp, bword ptr [r9]
       mov      ecx, dword ptr [r9+0x08]
       cmp      ecx, 8
       jl       G_M000_IG31
       mov      r14d, 8
       cmp      ecx, 16
       jl       G_M000_IG31
       lea      r15, bword ptr [rbp+0x100]
       mov      r13d, 8
       imul     r8d, dword ptr [rbx+0x54]
       lea      r12d, [r8+rdx+0x01]
       xor      ecx, ecx
       align    [0 bytes for IG03]

G_M000_IG03:                ;; offset=0x00E3
       mov      rdx, rcx
       shl      rdx, 5
       add      rdx, rbp
       mov      r8, gword ptr [rbx+0x18]
       lea      r9d, [r12+8*rcx]
       test     r8, r8
       je       G_M000_IG31
       mov      eax, dword ptr [r8+0x08]
       mov      r10d, r9d
       add      r10, 8
       cmp      rax, r10
       jb       G_M000_IG31
       lea      r8, bword ptr [r8+4*r9+0x10]
       vmovups  ymm0, ymmword ptr [r8]
       vmovups  ymmword ptr [rdx], ymm0
       inc      ecx
       cmp      ecx, 8
       jl       SHORT G_M000_IG03

G_M000_IG04:                ;; offset=0x0127
       lea      rcx, bword ptr [rbp+0x20]
       lea      rdx, bword ptr [rbp+0x40]
       lea      r8, bword ptr [rbp+0x60]
       lea      r9, bword ptr [rbp+0x80]
       lea      rax, bword ptr [rbp+0xA0]
       lea      r10, bword ptr [rbp+0xC0]
       lea      r11, bword ptr [rbp+0xE0]
       vmovups  ymm0, ymmword ptr [rbp]
       vmovaps  xmm1, xmm0
       vmovups  ymm2, ymmword ptr [rcx]
       vmovaps  xmm3, xmm2
       vunpcklps xmm4, xmm1, xmm3
       vunpckhps xmm1, xmm1, xmm3
       vinsertf128 ymm1, ymm4, xmm1
       vextractf128 xmm0, ymm0
       vextractf128 xmm2, ymm2
       vunpcklps xmm3, xmm0, xmm2
       vunpckhps xmm0, xmm0, xmm2
       vinsertf128 ymm0, ymm3, xmm0
       vmovups  ymm2, ymmword ptr [rdx]
       vmovaps  xmm3, xmm2
       vmovups  ymm4, ymmword ptr [r8]
       vmovaps  xmm5, xmm4
       vunpcklps xmm6, xmm3, xmm5
       vunpckhps xmm3, xmm3, xmm5
       vinsertf128 ymm3, ymm6, xmm3
       vextractf128 xmm2, ymm2
       vextractf128 xmm4, ymm4
       vunpcklps xmm5, xmm2, xmm4
       vunpckhps xmm2, xmm2, xmm4
       vinsertf128 ymm2, ymm5, xmm2
       vmovups  ymm4, ymmword ptr [r9]
       vmovaps  xmm5, xmm4
       vmovups  ymm6, ymmword ptr [rax]
       vmovaps  xmm7, xmm6
       vunpcklps xmm8, xmm5, xmm7
       vunpckhps xmm5, xmm5, xmm7
       vinsertf128 ymm5, ymm8, xmm5
       vextractf128 xmm4, ymm4
       vextractf128 xmm6, ymm6
       vunpcklps xmm7, xmm4, xmm6
       vunpckhps xmm4, xmm4, xmm6
       vinsertf128 ymm4, ymm7, xmm4
       vmovups  ymm6, ymmword ptr [r10]
       vmovaps  xmm7, xmm6
       vmovups  ymm8, ymmword ptr [r11]
       vmovaps  xmm9, xmm8
       vunpcklps xmm10, xmm7, xmm9
       vunpckhps xmm7, xmm7, xmm9
       vinsertf128 ymm7, ymm10, xmm7
       vextractf128 xmm6, ymm6
       vextractf128 xmm8, ymm8
       vunpcklps xmm9, xmm6, xmm8
       vunpckhps xmm6, xmm6, xmm8
       vinsertf128 ymm6, ymm9, xmm6
       vmovaps  xmm8, xmm1
       vmovaps  xmm9, xmm3
       vunpcklpd xmm10, xmm8, xmm9
       vunpckhpd xmm8, xmm8, xmm9
       vinsertf128 ymm8, ymm10, xmm8
       vextractf128 xmm1, ymm1
       vextractf128 xmm3, ymm3
       vunpcklpd xmm9, xmm1, xmm3
       vunpckhpd xmm1, xmm1, xmm3
       vinsertf128 ymm1, ymm9, xmm1
       vmovaps  xmm3, xmm0

G_M000_IG05:                ;; offset=0x026F
       vmovaps  xmm9, xmm2
       vunpcklpd xmm10, xmm3, xmm9
       vunpckhpd xmm3, xmm3, xmm9
       vinsertf128 ymm3, ymm10, xmm3
       vextractf128 xmm0, ymm0
       vextractf128 xmm2, ymm2
       vunpcklpd xmm9, xmm0, xmm2
       vunpckhpd xmm0, xmm0, xmm2
       vinsertf128 ymm0, ymm9, xmm0
       vmovaps  xmm2, xmm5
       vmovaps  xmm9, xmm7
       vunpcklpd xmm10, xmm2, xmm9
       vunpckhpd xmm2, xmm2, xmm9
       vinsertf128 ymm2, ymm10, xmm2
       vextractf128 xmm5, ymm5
       vextractf128 xmm7, ymm7
       vunpcklpd xmm9, xmm5, xmm7
       vunpckhpd xmm5, xmm5, xmm7
       vinsertf128 ymm5, ymm9, xmm5
       vmovaps  xmm7, xmm4
       vmovaps  xmm9, xmm6
       vunpcklpd xmm10, xmm7, xmm9
       vunpckhpd xmm7, xmm7, xmm9
       vinsertf128 ymm7, ymm10, xmm7
       vextractf128 xmm4, ymm4
       vextractf128 xmm6, ymm6
       vunpcklpd xmm9, xmm4, xmm6
       vunpckhpd xmm4, xmm4, xmm6
       vinsertf128 ymm4, ymm9, xmm4
       vinsertf128 ymm6, ymm8, xmm2
       vmovups  ymmword ptr [rbp], ymm6
       vextractf128 xmm6, ymm8
       vextractf128 xmm2, ymm2
       vinsertf128 ymm2, ymm6, xmm2
       vmovups  ymmword ptr [rcx], ymm2
       vinsertf128 ymm2, ymm1, xmm5
       vmovups  ymmword ptr [rdx], ymm2
       vextractf128 xmm1, ymm1
       vextractf128 xmm2, ymm5
       vinsertf128 ymm1, ymm1, xmm2
       vmovups  ymmword ptr [r8], ymm1
       vinsertf128 ymm1, ymm3, xmm7
       vmovups  ymmword ptr [r9], ymm1
       vextractf128 xmm1, ymm3
       vextractf128 xmm2, ymm7
       vinsertf128 ymm1, ymm1, xmm2
       vmovups  ymmword ptr [rax], ymm1
       vinsertf128 ymm1, ymm0, xmm4
       vmovups  ymmword ptr [r10], ymm1
       vextractf128 xmm0, ymm0
       vextractf128 xmm1, ymm4
       vinsertf128 ymm0, ymm0, xmm1
       vmovups  ymmword ptr [r11], ymm0
       vxorps   ymm6, ymm6, ymm6
       vxorps   ymm7, ymm7, ymm7
       vbroadcastss ymm8, dword ptr [reloc @RWD00]
       xor      eax, eax
       vbroadcastss ymm9, dword ptr [reloc @RWD04]
       vbroadcastss ymm10, dword ptr [reloc @RWD08]
       vbroadcastss ymm11, dword ptr [reloc @RWD12]
       vbroadcastss ymm12, dword ptr [reloc @RWD16]
       vbroadcastss ymm13, dword ptr [reloc @RWD20]
       jmp      G_M000_IG21

G_M000_IG06:                ;; offset=0x03CB
       mov      rsi, r15
       mov      r10d, r13d

G_M000_IG07:                ;; offset=0x03D1
       mov      dword ptr [rsp+0xFC], r10d
       test     r14d, r14d
       je       G_M000_IG32
       vmovups  ymm14, ymmword ptr [rbp]
       xor      r11d, r11d
       cmp      r10d, 7
       jl       G_M000_IG16

G_M000_IG08:                ;; offset=0x03F4
       lea      r9d, [r11+0x01]
       mov      dword ptr [rsp+0x3C], r9d
       cmp      r9d, r14d
       jae      G_M000_IG32
       mov      ecx, r9d
       shl      rcx, 5
       vmovups  ymm15, ymmword ptr [rcx+rbp]
       vsubps   ymm0, ymm14, ymm15
       vandps   ymm0, ymm9, ymm0
       vaddps   ymm0, ymm0, ymm6
       mov      ecx, r11d
       shl      rcx, 5
       vsubps   ymm1, ymm14, ymmword ptr [rsi+rcx]
       vandps   ymm1, ymm9, ymm1
       vaddps   ymm6, ymm1, ymm0
       vaddps   ymm0, ymm14, ymm8
       movzx    r8, byte  ptr [rbx+0x69]
       mov      rdx, gword ptr [rbx+0x10]
       vcmpneqps ymm1, ymm0, ymm0
       vxorps   ymm2, ymm2, ymm2
       vcmpgtps ymm2, ymm0, ymm2
       vorps    ymm1, ymm2, ymm1
       vandps   ymm0, ymm1, ymm0
       vmulps   ymm1, ymm0, ymm0
       vmulps   ymm0, ymm10, ymm0
       vmulps   ymm0, ymm0, ymm1
       vaddps   ymm0, ymm11, ymm0
       vmovups  ymmword ptr [rsp+0x1A0], ymm0
       vmulps   ymm0, ymm12, ymm1
       vaddps   ymm0, ymm13, ymm0
       vmovups  ymmword ptr [rsp+0x160], ymm0
       test     r8d, r8d
       je       G_M000_IG10

G_M000_IG09:                ;; offset=0x048A
       mov      gword ptr [rsp+0x20], rdx
       lea      rcx, [rsp+0x180]
       lea      rdx, [rsp+0x160]
       lea      r8, [rsp+0x1A0]
       mov      r9d, 1
       vextractf128 xmm14, ymm15
       vextractf128 xmmword ptr [rsp+0x2D0], ymm6
       vextractf128 xmmword ptr [rsp+0x2B0], ymm7
       vextractf128 xmmword ptr [rsp+0xD0], ymm9
       vextractf128 xmmword ptr [rsp+0x290], ymm8
       vextractf128 xmmword ptr [rsp+0xB0], ymm10
       vextractf128 xmmword ptr [rsp+0x90], ymm11
       vextractf128 xmmword ptr [rsp+0x70], ymm12
       vextractf128 xmmword ptr [rsp+0x50], ymm13
       call     [DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Runtime.Intrinsics.Vector256`1[float],System.Runtime.Intrinsics.Vector256`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Runtime.Intrinsics.Vector256`1[float]]
       vinsertf128 ymm15, ymm15, xmm14
       vinsertf128 ymm6, ymm6, xmmword ptr [rsp+0x2D0]
       vinsertf128 ymm7, ymm7, xmmword ptr [rsp+0x2B0]
       vinsertf128 ymm9, ymm9, xmmword ptr [rsp+0xD0]
       vinsertf128 ymm8, ymm8, xmmword ptr [rsp+0x290]
       vinsertf128 ymm10, ymm10, xmmword ptr [rsp+0xB0]
       vinsertf128 ymm11, ymm11, xmmword ptr [rsp+0x90]
       vinsertf128 ymm12, ymm12, xmmword ptr [rsp+0x70]
       vinsertf128 ymm13, ymm13, xmmword ptr [rsp+0x50]
       jmp      SHORT G_M000_IG11

G_M000_IG10:                ;; offset=0x0565
       vmovups  ymm0, ymmword ptr [rsp+0x160]
       vdivps   ymm0, ymm0, ymmword ptr [rsp+0x1A0]
       vmovups  ymmword ptr [rsp+0x180], ymm0

G_M000_IG11:                ;; offset=0x0580
       vaddps   ymm7, ymm7, ymmword ptr [rsp+0x180]
       vmovaps  ymm14, ymm15
       mov      r9d, dword ptr [rsp+0x3C]
       mov      r11d, r9d
       cmp      r11d, 7
       jl       G_M000_IG08

G_M000_IG12:                ;; offset=0x05A0
       cmp      dword ptr [rsp+0xFC], 7
       jbe      G_M000_IG32
       vsubps   ymm0, ymm14, ymmword ptr [rsi+0xE0]
       vandps   ymm0, ymm9, ymm0
       vaddps   ymm6, ymm0, ymm6
       vaddps   ymm0, ymm14, ymm8
       movzx    rcx, byte  ptr [rbx+0x69]
       mov      rdx, gword ptr [rbx+0x10]
       vcmpneqps ymm1, ymm0, ymm0
       vxorps   ymm2, ymm2, ymm2
       vcmpgtps ymm2, ymm0, ymm2
       vorps    ymm1, ymm2, ymm1
       vandps   ymm0, ymm1, ymm0
       vmulps   ymm1, ymm0, ymm0
       vmulps   ymm0, ymm10, ymm0
       vmulps   ymm0, ymm0, ymm1
       vaddps   ymm0, ymm11, ymm0
       vmovups  ymmword ptr [rsp+0x140], ymm0
       vmulps   ymm0, ymm12, ymm1
       vaddps   ymm0, ymm13, ymm0
       vmovups  ymmword ptr [rsp+0x100], ymm0
       test     ecx, ecx
       jne      G_M000_IG18

G_M000_IG13:                ;; offset=0x0613
       vmovups  ymm0, ymmword ptr [rsp+0x100]
       vdivps   ymm0, ymm0, ymmword ptr [rsp+0x140]
       vmovups  ymmword ptr [rsp+0x120], ymm0
       jmp      G_M000_IG19

G_M000_IG14:                ;; offset=0x0633
       vmovups  ymm0, ymmword ptr [rsp+0x160]
       vdivps   ymm0, ymm0, ymmword ptr [rsp+0x1A0]
       vmovups  ymmword ptr [rsp+0x180], ymm0

G_M000_IG15:                ;; offset=0x064E
       vaddps   ymm7, ymm7, ymmword ptr [rsp+0x180]
       vmovaps  ymm14, ymm15
       mov      r11d, dword ptr [rsp+0x278]
       inc      r11d
       cmp      r11d, 7
       jge      G_M000_IG12

G_M000_IG16:                ;; offset=0x0671
       lea      ecx, [r11+0x01]
       cmp      ecx, r14d
       jae      G_M000_IG32
       lea      ecx, [r11+0x01]
       shl      rcx, 5
       vmovups  ymm15, ymmword ptr [rcx+rbp]
       vsubps   ymm0, ymm14, ymm15
       vandps   ymm0, ymm9, ymm0
       vaddps   ymm0, ymm0, ymm6
       cmp      r11d, dword ptr [rsp+0xFC]
       jae      G_M000_IG32
       mov      dword ptr [rsp+0x278], r11d
       mov      ecx, r11d
       shl      rcx, 5
       vsubps   ymm1, ymm14, ymmword ptr [rsi+rcx]
       vandps   ymm1, ymm9, ymm1
       vaddps   ymm6, ymm1, ymm0
       vaddps   ymm0, ymm14, ymm8
       movzx    r8, byte  ptr [rbx+0x69]
       mov      rdx, gword ptr [rbx+0x10]
       vcmpneqps ymm1, ymm0, ymm0
       vxorps   ymm2, ymm2, ymm2
       vcmpgtps ymm2, ymm0, ymm2
       vorps    ymm1, ymm2, ymm1
       vandps   ymm0, ymm1, ymm0
       vmulps   ymm1, ymm0, ymm0
       vmulps   ymm0, ymm10, ymm0
       vmulps   ymm0, ymm0, ymm1
       vaddps   ymm0, ymm11, ymm0
       vmovups  ymmword ptr [rsp+0x1A0], ymm0
       vmulps   ymm0, ymm12, ymm1
       vaddps   ymm0, ymm13, ymm0
       vmovups  ymmword ptr [rsp+0x160], ymm0
       test     r8d, r8d
       je       G_M000_IG14

G_M000_IG17:                ;; offset=0x0719
       mov      gword ptr [rsp+0x20], rdx
       lea      rcx, [rsp+0x180]
       lea      rdx, [rsp+0x160]
       lea      r8, [rsp+0x1A0]
       mov      r9d, 1
       vextractf128 xmm14, ymm15
       vextractf128 xmmword ptr [rsp+0x2D0], ymm6
       vextractf128 xmmword ptr [rsp+0x2B0], ymm7
       vextractf128 xmmword ptr [rsp+0xD0], ymm9
       vextractf128 xmmword ptr [rsp+0x290], ymm8
       vextractf128 xmmword ptr [rsp+0xB0], ymm10
       vextractf128 xmmword ptr [rsp+0x90], ymm11
       vextractf128 xmmword ptr [rsp+0x70], ymm12
       vextractf128 xmmword ptr [rsp+0x50], ymm13
       call     [DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Runtime.Intrinsics.Vector256`1[float],System.Runtime.Intrinsics.Vector256`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Runtime.Intrinsics.Vector256`1[float]]
       vinsertf128 ymm15, ymm15, xmm14
       vinsertf128 ymm6, ymm6, xmmword ptr [rsp+0x2D0]
       vinsertf128 ymm7, ymm7, xmmword ptr [rsp+0x2B0]
       vinsertf128 ymm9, ymm9, xmmword ptr [rsp+0xD0]
       vinsertf128 ymm8, ymm8, xmmword ptr [rsp+0x290]
       vinsertf128 ymm10, ymm10, xmmword ptr [rsp+0xB0]
       vinsertf128 ymm11, ymm11, xmmword ptr [rsp+0x90]
       vinsertf128 ymm12, ymm12, xmmword ptr [rsp+0x70]
       vinsertf128 ymm13, ymm13, xmmword ptr [rsp+0x50]
       jmp      G_M000_IG15
       align    [3 bytes for IG22]

G_M000_IG18:                ;; offset=0x07FA
       mov      gword ptr [rsp+0x20], rdx
       lea      rcx, [rsp+0x120]
       lea      rdx, [rsp+0x100]
       lea      r8, [rsp+0x140]
       mov      r9d, 1
       vextractf128 xmm14, ymm6
       vextractf128 xmm15, ymm7
       vextractf128 xmmword ptr [rsp+0xD0], ymm9
       vextractf128 xmmword ptr [rsp+0x290], ymm8
       vextractf128 xmmword ptr [rsp+0xB0], ymm10
       vextractf128 xmmword ptr [rsp+0x90], ymm11
       vextractf128 xmmword ptr [rsp+0x70], ymm12
       vextractf128 xmmword ptr [rsp+0x50], ymm13
       call     [DotJpegli.Internal.AdaptiveQuantization:DivideReciprocal(System.Runtime.Intrinsics.Vector256`1[float],System.Runtime.Intrinsics.Vector256`1[float],int,DotJpegli.Internal.AqRationalDiagnostics):System.Runtime.Intrinsics.Vector256`1[float]]
       vinsertf128 ymm6, ymm6, xmm14
       vinsertf128 ymm7, ymm7, xmm15
       vinsertf128 ymm9, ymm9, xmmword ptr [rsp+0xD0]
       vinsertf128 ymm8, ymm8, xmmword ptr [rsp+0x290]
       vinsertf128 ymm10, ymm10, xmmword ptr [rsp+0xB0]
       vinsertf128 ymm11, ymm11, xmmword ptr [rsp+0x90]
       vinsertf128 ymm12, ymm12, xmmword ptr [rsp+0x70]
       vinsertf128 ymm13, ymm13, xmmword ptr [rsp+0x50]

G_M000_IG19:                ;; offset=0x08B3
       vaddps   ymm7, ymm7, ymmword ptr [rsp+0x120]
       add      r12d, dword ptr [rbx+0x54]
       mov      eax, dword ptr [rsp+0x27C]
       inc      eax
       cmp      eax, 8
       jge      G_M000_IG27

G_M000_IG20:                ;; offset=0x08D2
       cmp      eax, 7
       mov      rcx, rbp
       mov      rbp, r15
       mov      r15, rcx
       mov      ecx, r13d
       mov      r13d, r14d
       mov      r14d, ecx
       je       G_M000_IG25

G_M000_IG21:                ;; offset=0x08ED
       mov      r10d, r12d
       add      r10d, dword ptr [rbx+0x54]
       xor      ecx, ecx
       cmp      r13d, 8
       jl       G_M000_IG29

G_M000_IG22:                ;; offset=0x0900
       mov      r11d, ecx
       shl      r11, 5
       add      r11, r15
       mov      r9, gword ptr [rbx+0x18]
       lea      r8d, [r10+8*rcx]
       test     r9, r9
       je       G_M000_IG31
       mov      edx, dword ptr [r9+0x08]
       mov      esi, r8d
       add      rsi, 8
       cmp      rdx, rsi
       jb       G_M000_IG31
       mov      edx, r8d
       lea      rsi, bword ptr [r9+4*rdx+0x10]
       vmovups  ymm0, ymmword ptr [rsi]
       vmovups  ymmword ptr [r11], ymm0
       inc      ecx
       cmp      ecx, 8
       jl       SHORT G_M000_IG22

G_M000_IG23:                ;; offset=0x0947
       lea      rcx, bword ptr [r15+0x20]
       lea      rdx, bword ptr [r15+0x40]
       lea      r8, bword ptr [r15+0x60]
       lea      r9, bword ptr [r15+0x80]
       lea      r10, bword ptr [r15+0xA0]
       lea      r11, bword ptr [r15+0xC0]
       lea      rsi, bword ptr [r15+0xE0]
       vmovups  ymm0, ymmword ptr [r15]
       vmovaps  xmm1, xmm0
       vmovups  ymm2, ymmword ptr [rcx]
       vmovaps  xmm3, xmm2
       vunpcklps xmm4, xmm1, xmm3
       vunpckhps xmm1, xmm1, xmm3
       vinsertf128 ymm1, ymm4, xmm1
       vextractf128 xmm0, ymm0
       vextractf128 xmm2, ymm2
       vunpcklps xmm3, xmm0, xmm2
       vunpckhps xmm0, xmm0, xmm2
       vinsertf128 ymm0, ymm3, xmm0
       vmovups  ymm2, ymmword ptr [rdx]
       vmovaps  xmm3, xmm2
       vmovups  ymm4, ymmword ptr [r8]
       vmovaps  xmm5, xmm4
       vunpcklps xmm14, xmm3, xmm5
       vunpckhps xmm3, xmm3, xmm5
       vinsertf128 ymm3, ymm14, xmm3
       vextractf128 xmm2, ymm2
       vextractf128 xmm4, ymm4
       vunpcklps xmm5, xmm2, xmm4
       vunpckhps xmm2, xmm2, xmm4
       vinsertf128 ymm2, ymm5, xmm2
       vmovups  ymmword ptr [rsp+0x240], ymm2
       vmovups  ymm4, ymmword ptr [r9]
       vmovaps  xmm5, xmm4
       vmovups  ymm14, ymmword ptr [r10]
       vmovaps  xmm15, xmm14
       vunpcklps xmm2, xmm5, xmm15
       vunpckhps xmm5, xmm5, xmm15
       vinsertf128 ymm2, ymm2, xmm5
       vmovups  ymmword ptr [rsp+0x220], ymm2
       vextractf128 xmm4, ymm4
       vextractf128 xmm5, ymm14
       vunpcklps xmm14, xmm4, xmm5
       vunpckhps xmm4, xmm4, xmm5
       vinsertf128 ymm4, ymm14, xmm4
       vmovups  ymmword ptr [rsp+0x200], ymm4
       vmovups  ymm5, ymmword ptr [r11]
       vmovaps  xmm14, xmm5
       vmovups  ymm15, ymmword ptr [rsi]
       vmovaps  xmm4, xmm15
       vunpcklps xmm2, xmm14, xmm4
       vunpckhps xmm4, xmm14, xmm4
       vinsertf128 ymm2, ymm2, xmm4
       vextractf128 xmm4, ymm5
       vextractf128 xmm5, ymm15
       vunpcklps xmm14, xmm4, xmm5
       vunpckhps xmm4, xmm4, xmm5
       vinsertf128 ymm4, ymm14, xmm4
       vmovaps  xmm5, xmm1
       vmovaps  xmm14, xmm3
       vunpcklpd xmm15, xmm5, xmm14
       vunpckhpd xmm5, xmm5, xmm14
       vinsertf128 ymm5, ymm15, xmm5
       vextractf128 xmm1, ymm1
       vextractf128 xmm3, ymm3

G_M000_IG24:                ;; offset=0x0A97
       vunpcklpd xmm14, xmm1, xmm3
       vunpckhpd xmm1, xmm1, xmm3
       vinsertf128 ymm1, ymm14, xmm1
       vmovaps  xmm3, xmm0
       vmovupd  xmm15, xmmword ptr [rsp+0x240]
       vunpcklpd xmm14, xmm3, xmm15
       vunpckhpd xmm3, xmm3, xmm15
       vinsertf128 ymm3, ymm14, xmm3
       vmovups  ymmword ptr [rsp+0x1E0], ymm3
       vextractf128 xmm0, ymm0
       vmovups  ymm14, ymmword ptr [rsp+0x240]
       vextractf128 xmm14, ymm14
       vunpcklpd xmm15, xmm0, xmm14
       vunpckhpd xmm0, xmm0, xmm14
       vinsertf128 ymm0, ymm15, xmm0
       vmovups  ymmword ptr [rsp+0x1C0], ymm0
       vmovupd  xmm15, xmmword ptr [rsp+0x220]
       vmovaps  xmm14, xmm2
       vunpcklpd xmm0, xmm15, xmm14
       vunpckhpd xmm14, xmm15, xmm14
       vinsertf128 ymm0, ymm0, xmm14
       vmovups  ymm14, ymmword ptr [rsp+0x220]
       vextractf128 xmm14, ymm14
       vextractf128 xmm2, ymm2
       vunpcklpd xmm15, xmm14, xmm2
       vunpckhpd xmm2, xmm14, xmm2
       vinsertf128 ymm2, ymm15, xmm2
       vmovupd  xmm15, xmmword ptr [rsp+0x200]
       vmovaps  xmm14, xmm4
       vunpcklpd xmm3, xmm15, xmm14
       vunpckhpd xmm14, xmm15, xmm14
       vinsertf128 ymm3, ymm3, xmm14
       vmovups  ymm14, ymmword ptr [rsp+0x200]
       vextractf128 xmm14, ymm14
       vextractf128 xmm4, ymm4
       vunpcklpd xmm15, xmm14, xmm4
       vunpckhpd xmm4, xmm14, xmm4
       vinsertf128 ymm4, ymm15, xmm4
       vinsertf128 ymm14, ymm5, xmm0
       vmovups  ymmword ptr [r15], ymm14
       vextractf128 xmm5, ymm5
       vextractf128 xmm0, ymm0
       vinsertf128 ymm0, ymm5, xmm0
       vmovups  ymmword ptr [rcx], ymm0
       vinsertf128 ymm0, ymm1, xmm2
       vmovups  ymmword ptr [rdx], ymm0
       vextractf128 xmm0, ymm1
       vextractf128 xmm1, ymm2
       vinsertf128 ymm0, ymm0, xmm1
       vmovups  ymmword ptr [r8], ymm0
       vmovups  ymm0, ymmword ptr [rsp+0x1E0]
       vinsertf128 ymm1, ymm0, xmm3
       vmovups  ymmword ptr [r9], ymm1
       vextractf128 xmm0, ymm0
       vextractf128 xmm1, ymm3
       vinsertf128 ymm0, ymm0, xmm1
       vmovups  ymmword ptr [r10], ymm0
       vmovups  ymm0, ymmword ptr [rsp+0x1C0]
       vinsertf128 ymm1, ymm0, xmm4
       vmovups  ymmword ptr [r11], ymm1
       vextractf128 xmm0, ymm0
       vextractf128 xmm1, ymm4
       vinsertf128 ymm0, ymm0, xmm1
       vmovups  ymmword ptr [rsi], ymm0

G_M000_IG25:                ;; offset=0x0C10
       mov      dword ptr [rsp+0x27C], eax
       cmp      eax, 7
       jne      G_M000_IG06

G_M000_IG26:                ;; offset=0x0C20
       mov      rsi, rbp
       mov      r10d, r14d
       jmp      G_M000_IG07

G_M000_IG27:                ;; offset=0x0C2B
       cmp      edi, 8
       jl       G_M000_IG31
       vmulps   ymm0, ymm6, ymmword ptr [reloc @RWD24]
       mov      rsi, bword ptr [rsp+0x30]
       vmovups  ymmword ptr [rsi], ymm0
       cmp      edi, 16
       jl       G_M000_IG31
       add      rsi, 32
       vmulps   ymm0, ymm7, ymmword ptr [reloc @RWD56]
       vmovups  ymmword ptr [rsi], ymm0

G_M000_IG28:                ;; offset=0x0C5E
       vzeroupper
       vmovaps  xmm6, xmmword ptr [rsp+0x380]
       vmovaps  xmm7, xmmword ptr [rsp+0x370]
       vmovaps  xmm8, xmmword ptr [rsp+0x360]
       vmovaps  xmm9, xmmword ptr [rsp+0x350]
       vmovaps  xmm10, xmmword ptr [rsp+0x340]
       vmovaps  xmm11, xmmword ptr [rsp+0x330]
       vmovaps  xmm12, xmmword ptr [rsp+0x320]
       vmovaps  xmm13, xmmword ptr [rsp+0x310]
       vmovaps  xmm14, xmmword ptr [rsp+0x300]
       vmovaps  xmm15, xmmword ptr [rsp+0x2F0]
       add      rsp, 920
       pop      rbx
       pop      rbp
       pop      rsi
       pop      rdi
       pop      r12
       pop      r13
       pop      r14
       pop      r15
       ret

G_M000_IG29:                ;; offset=0x0CCF
       cmp      ecx, r13d
       jae      G_M000_IG32
       mov      r11d, ecx
       shl      r11, 5
       add      r11, r15
       mov      r9, gword ptr [rbx+0x18]
       lea      r8d, [r10+8*rcx]
       test     r9, r9
       je       SHORT G_M000_IG31
       mov      eax, dword ptr [r9+0x08]
       mov      edx, r8d
       add      rdx, 8
       cmp      rax, rdx
       jb       SHORT G_M000_IG31
       mov      eax, r8d
       lea      rsi, bword ptr [r9+4*rax+0x10]
       vmovups  ymm0, ymmword ptr [rsi]
       vmovups  ymmword ptr [r11], ymm0
       inc      ecx
       cmp      ecx, 8
       jl       SHORT G_M000_IG29

G_M000_IG30:                ;; offset=0x0D17
       mov      rcx, 0x7FF882B8F4B8
       call     CORINFO_HELP_NEWSFAST
       mov      rbx, rax
       mov      rcx, rbx
       mov      rdx, 0x231A58E6350
       mov      r8, 0x231A58E63C8
       call     [System.ArgumentException:.ctor(System.String,System.String):this]
       mov      rcx, rbx
       call     CORINFO_HELP_THROW
       int3

G_M000_IG31:                ;; offset=0x0D4F
       call     [System.ThrowHelper:ThrowArgumentOutOfRangeException()]
       int3

G_M000_IG32:                ;; offset=0x0D56
       call     CORINFO_HELP_RNGCHKFAIL
       int3

RWD00  	dd	42233332h		;      40.8
RWD04  	dd	7FFFFFFFh		;       nan
RWD08  	dd	3B1DEA78h		; 0.00240961
RWD12  	dd	449E380Dh		;   1265.75
RWD16  	dd	42EF0C23h		;   119.524
RWD20  	dd	44228FFEh		;    650.25
RWD24  	dq	B8933E1EB8933E1Eh, B8933E1EB8933E1Eh, B8933E1EB8933E1Eh, B8933E1EB8933E1Eh
RWD56  	dq	3880808138808081h, 3880808138808081h, 3880808138808081h, 3880808138808081h

; Total bytes of code 3420
