# Historical investigation of added work in ordinary encoding

This archive preserves the untimed investigation from unpublished commit
`a873c2de2689a73a3b48ab9f5e7778a24210a9dc`. Its interface-based AQ math
strategy was an experiment and was not adopted wholesale. The production
[AQ implementation](../../../src/DotJpegli/Internal/AdaptiveQuantization.cs)
instead specializes marker types, and the
[transpose implementation](../../../src/DotJpegli/Internal/ForwardDct.cs)
retains a separately selectable full-width research schedule. The disassembly,
source hashes and validation results below describe the inspected historical
builds and JIT branches; they do not validate current production code or establish
its encoding latency. Historical reference inputs are retained as the repaired
[AQ source](aq-repaired-source.cs.txt),
[math strategies](aq-rational-math-repaired-source.cs.txt) and
[DCT source](forward-dct-repaired-source.cs.txt). These snapshots belong to the
historical commit and are not compiled as part of the current checkout.

The full measurements at `f52961b` and `d5f3226` used different CPU models, so
their latency ratios do not establish a code regression. Inspection of the
intervening changes nevertheless identified two concrete sources of added work
in shared encoding loops. This repair removes that work without changing the
AQ expressions, arithmetic order or JPEG encoding policy. No benchmarks ran
locally, and an end-to-end speed improvement has not been measured.

## Experimental AQ arithmetic in the ordinary path

Commit `222223c` threaded a runtime reciprocal preference and optional diagnostic
sink through the AQ rational expressions. Automatic still selected division,
but the generated default code contained branches and cold research calls.
Those call sites required spills even when the branches were never taken.

An untimed ARM64 JIT comparison isolates the last pre-integration implementation
(`59f0da0`) from the integrated implementation (`d5f3226`):

| Generated method | Before integration | Integrated default |
| --- | ---: | ---: |
| `ComputePreErosion` code bytes | 2,956 | 4,644 |
| `ComputePreErosion` frame bytes | 112 | 384 |
| `ComputePreErosion` stack-store sites | 7 | 41 |
| `ComputeMask` code bytes | 124 | 432 |
| `FastLog2` / `FastPow2` code bytes each | 120 | 216 |
| `ModulateEightBlocks` code bytes | 1,708 | 2,200 |
| `ModulateEightBlocks` stack-store sites | 6 | 22 |

These are static emitted-code counts, not execution counts or timing samples.
The [before disassembly](aq-before-arm64.asm) also includes the earlier
`f52961b` implementation, before the independent stencil-storage change.

The repair selects an ordinary or certified-reciprocal value-type math strategy
before entering an AQ row's pixel and block loops. Constrained generic calls
allow separate JIT specializations. Ordinary specializations contain direct
division, while explicit research retains certification, fallback and counters.
Ordinary instance interface members preserve .NET Standard 2.1 compatibility;
the strategy is not stored or passed as a boxed interface.

An x64 focused check caught one consequence of changing register allocation:
commutative vector additions propagated different NaN payloads between the two
specializations on exceptional float inputs. Explicit research now replays the
ordinary specialization if its completed strength field contains NaNs, returning
the ordinary field's exact bits. Its counters describe the completed certified
pass; the ordinary replay does not update them. Ordinary encoding performs no
such scan or replay. Historical NaN payload choices are not assumed stable across
JIT code layouts or CPU architectures; exact equality with the ordinary path
within the same build remains required.
These exceptional input fields bypass the public encoder's sample validation;
the public API rejects NaN and infinite source samples before producing JPEGs.

The repaired specialization was inspected using the actual repaired library.
The probe's copied library hash was checked against that library to avoid a
stale build reference. The ordinary generated methods contain no reciprocal or
diagnostic calls. After both repairs:

| Generated code | Integrated default | Repaired ordinary specialization |
| --- | ---: | ---: |
| ARM64 `ComputePreErosion` bytes | 4,644 | 2,964 |
| ARM64 `ComputePreErosion` stack-store sites | 41 | 8 |
| x64 AVX2 `ComputePreErosion` bytes | 6,273 | 4,040 |
| x64 AVX2 `ModulateEightBlocks` bytes | 3,420 | 2,628 |
| x64 AVX2 `ComputeMask` bytes | 393 | 110 |

Retained generated code: [before on x64](aq-before-x64.asm),
[repaired on x64](aq-repaired-x64.asm) and
[repaired on ARM64](aq-repaired-arm64.asm). `ModulateEightBlocks` also includes
the shared transpose, so its change includes both repairs.

The [AQ probe source](aq-probe.cs.txt), historical
[baseline](aq-baseline-source.cs.txt) and [stencil](aq-stencil-source.cs.txt)
copies retain the source used for inspection. Historical copies only rename the
type and constructor so all three implementations can run side by side against
the same unchanged helper types. The probe checks 18 bounded field configurations
(widths 1/19/65, sampling 1/2, scalar/vector/forced-batch paths) without timers.
The [exceptional-input probe](aq-exceptional-probe.cs.txt) and its
[repaired output](aq-exceptional-repaired.txt) record nine row/path comparisons.
To reproduce, copy the AQ driver as `Program.cs`, the historical copies as
`BaselineAdaptiveQuantization.cs` and `StencilAdaptiveQuantization.cs`, and the
[probe project](aq-probe.csproj.txt) as `AqProbe.csproj` in a temporary directory.
Build with the pinned SDK and `-p:DotJpegliReference=<absolute-library-path>`.
Run with tiered compilation disabled and `DOTNET_JitDisasm='*AdaptiveQuantization:*'`.
The project retains its friend-assembly name and explicit reference resolution.
To rebuild the historical repaired library, use a temporary checkout of the
recorded baseline `25ec5ff`, replace its `AdaptiveQuantization.cs` and
`ForwardDct.cs` with the repaired source snapshots linked above, and add the
`AqRationalMath.cs` snapshot alongside them. Build that library with the pinned
SDK before pointing the probe at it. Current production source is a different
implementation and cannot reproduce these repaired-build identities.

## Full-width transpose decomposition

Commit `2f806aa` replaced four 128-bit transpose panels with sixteen full-width
`Vector256.Zip` operations. Fewer managed operations did not mean fewer emitted
instructions on the AVX2 path used by the newer benchmark host.

An untimed x64 probe compares both schedules with the pinned .NET runtime
`11.0.0-rc.3.26522.148`, tiered compilation disabled and AVX-512 disabled:

| Emitted operation | Four panels | Full-width graph |
| --- | ---: | ---: |
| Unpacks | 32 | 32 |
| `vextractf128` | 8 | 24 |
| `vinsertf128` | 8 | 24 |
| Code bytes | 751 | 830 |

The full-width graph emits 32 additional cross-half operations. Windows ABI
register saves and temporary spills differ between the methods, so these
instruction counts alone do not establish latency. The repair restores the
previous four-panel schedule for both DCT and AQ modulation. .NET 11 retains
portable `Vector128.Zip` operations inside each panel. Executable transpose
source is identical to `f52961b`.

The [probe source](transpose-probe.cs.txt) contains both schedules and an
independent lane-map check; the [x64 disassembly](transpose-x64-avx2.asm) records
the emitted code. It ran under x64 emulation on Windows ARM64 solely to inspect
code generation and output bits. It has no timers, warmups or benchmark loops.
To reproduce, place the source in a temporary .NET 11 console project, build it
with the repository's pinned SDK, and run it with the pinned x64 runtime and
these process-local settings:

```powershell
$env:DOTNET_TieredCompilation = '0'
$env:DOTNET_EnableAVX512 = '0'
$env:DOTNET_JitDisasm = 'Program:Baseline Program:Candidate'
dotnet ./bin/Release/net11.0/TransposeProbe.dll
```

## Focused validation

Release builds completed without warnings for .NET 11, .NET 10 and .NET Standard
2.1. The final focused selections passed:

- 85 checks on .NET 11 x64 with AVX-512 and tiered compilation disabled, under
  Windows ARM64 emulation. This selection covers transpose payloads, DCT bits,
  AQ scalar/vector/batched fields, reciprocal diagnostics, exceptional floats,
  the independent staged stencil, and 33 existing complete-JPEG hash fixtures.
- The same 85 checks on native ARM64 with .NET 10.
- 66 checks against the .NET Standard 2.1 library on ARM64/.NET 10, covering
  reciprocal fields, workspace reuse, scalar modulation and JPEG hash fixtures.
- Focused formatting and whitespace checks on the changed source.

An additional [public float probe](public-float-probe.cs.txt) uses separate
before/after processes and the same program. All four 65×17 normalized-float
JPEG cases (444/420, sequential/progressive) preserve their exact bytes. The
exceptional source pattern is rejected with identical exceptions and empty
output prefixes. Retained [before](public-float-before.json),
[after](public-float-after.json) and [binary identities](public-float-identity.json)
record those untimed checks. [Provenance](provenance.json) records source and
artifact hashes. Generated text uses LF line endings with trailing whitespace
and blank lines at EOF removed; instructions are unchanged.

## Scope of the conclusion

The repairs remove demonstrated default-path spills and avoid the more expensive
AVX2 shuffle graph. The different historical hosts, inlining contexts and ABI
differences prevent assigning a fraction of the reported latency change to
either source change. Fresh paired CI measurements are needed to establish the
resulting complete-encode speed. The existing README charts remain measurements
of their named revision; they are not updated to claim an unmeasured gain.
