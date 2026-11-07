// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by a BSD-style license that can be
// found in THIRD-PARTY-NOTICES.md or at https://developers.google.com/open-source/licenses/bsd.

namespace DotJpegli.Internal;

using System.Numerics;
using System.Runtime.CompilerServices;
#if !NETSTANDARD2_1
using System.Runtime.Intrinsics;
#endif

/// <summary>Managed scalar and SIMD implementations of Google's recursive JPEGli DCT.</summary>
internal static class ForwardDct
{
    // Directly derived from google/jpegli lib/jpegli/dct-inl.h:
    // AddReverse, SubReverse, Multiply, B, InverseEvenOdd, DCT1DImpl,
    // StoreToBlockAndScale, and TransformFromPixels.
    // Source revision: 031a0077f5799a6041004267fc12b956c1f52a20.
    private const float Sqrt2 = 1.41421356237f;

    /// <summary>
    /// Transforms 64 row-major, level-shifted samples into standard JPEG
    /// orthonormal coefficients in row-major frequency order.
    /// </summary>
    internal static void Transform(ReadOnlySpan<float> samples, Span<float> coefficients)
    {
        ValidateBuffers(samples, coefficients);
        if (Vector.IsHardwareAccelerated && CanUseVector)
        {
            TransformVectorCore(samples, coefficients);
        }
        else
        {
            TransformScalarCore(samples, coefficients);
        }
    }

    /// <summary>Loads and level-shifts an interior 8 by 8 block directly from an uncentered plane.</summary>
    internal static void TransformPlane(ReadOnlySpan<float> plane, int stride, int left, int top,
        Span<float> coefficients)
    {
        ValidatePlane(plane, stride, left, top);
        if (coefficients.Length < 64)
        {
            throw new ArgumentException("The coefficient buffer requires at least 64 values.", nameof(coefficients));
        }

#if !NETSTANDARD2_1
        if (Vector.IsHardwareAccelerated && Vector<float>.Count == 8)
        {
            TransformEightLane(plane[((top * stride) + left)..], stride, centerInput: true, coefficients);
        }
        else if (Vector.IsHardwareAccelerated && Vector<float>.Count == 4)
        {
            TransformFourLane(plane[((top * stride) + left)..], stride, centerInput: true, coefficients);
        }
        else
#endif
        {
            Span<float> samples = stackalloc float[64];
            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    samples[(row * 8) + column] = plane[((top + row) * stride) + left + column] - 128f;
                }
            }

            Transform(samples, coefficients);
        }
    }

    /// <summary>Transforms, quantizes, validates AC, and optionally orders one interior plane block.</summary>
    internal static void TransformQuantizePlane(ReadOnlySpan<float> plane, int stride, int left, int top,
        Quantization.PreparedTable table, Span<short> output, float adaptiveStrength = 0f, bool zigZagOrdered = true)
    {
        ValidatePlane(plane, stride, left, top);
        TransformQuantizeCore(plane[((top * stride) + left)..], stride, centerInput: true,
            table, output, adaptiveStrength, zigZagOrdered);
    }

    internal static ulong TransformQuantizePlaneWithMask(ReadOnlySpan<float> plane, int stride, int left, int top,
        Quantization.PreparedTable table, Span<short> output, float adaptiveStrength = 0f, bool clearDcOnlyAc = true)
    {
        ValidatePlane(plane, stride, left, top);
        return TransformQuantizeCoreWithMask(plane[((top * stride) + left)..], stride, centerInput: true,
            table, output, adaptiveStrength, clearDcOnlyAc);
    }

    /// <summary>Transforms and quantizes already centered samples without materializing float coefficients.</summary>
    internal static void TransformQuantizeBlock(ReadOnlySpan<float> samples,
        Quantization.PreparedTable table, Span<short> output, float adaptiveStrength = 0f, bool zigZagOrdered = true)
    {
        if (samples.Length != 64)
        {
            throw new ArgumentException("An 8 by 8 block requires exactly 64 samples.", nameof(samples));
        }

        TransformQuantizeCore(samples, 8, centerInput: false, table, output, adaptiveStrength, zigZagOrdered);
    }

    internal static ulong TransformQuantizeBlockWithMask(ReadOnlySpan<float> samples,
        Quantization.PreparedTable table, Span<short> output, float adaptiveStrength = 0f, bool clearDcOnlyAc = true)
    {
        if (samples.Length != 64)
        {
            throw new ArgumentException("An 8 by 8 block requires exactly 64 samples.", nameof(samples));
        }

        return TransformQuantizeCoreWithMask(samples, 8, centerInput: false, table, output, adaptiveStrength, clearDcOnlyAc);
    }

    /// <summary>Replicates a partial plane block's edges before using the fused transform and quantizer.</summary>
    internal static void TransformQuantizePaddedPlane(ReadOnlySpan<float> plane, int width, int height, int left, int top,
        Quantization.PreparedTable table, Span<short> output, float adaptiveStrength = 0f)
        => _ = TransformQuantizePaddedPlaneWithMask(plane, width, height, left, top, table, output, adaptiveStrength,
            clearDcOnlyAc: true);

    internal static ulong TransformQuantizePaddedPlaneWithMask(ReadOnlySpan<float> plane, int width, int height, int left, int top,
        Quantization.PreparedTable table, Span<short> output, float adaptiveStrength = 0f, bool clearDcOnlyAc = true)
    {
        Guard.ThrowIfNegativeOrZero(width);
        Guard.ThrowIfNegativeOrZero(height);
        if ((uint)left >= (uint)width)
        {
            throw new ArgumentOutOfRangeException(nameof(left));
        }

        if ((uint)top >= (uint)height)
        {
            throw new ArgumentOutOfRangeException(nameof(top));
        }

        if ((long)width * height > plane.Length)
        {
            throw new ArgumentException("The plane must contain every image row.", nameof(plane));
        }

        Span<float> padded = stackalloc float[64];
        int copied = Math.Min(8, width - left);
        for (int row = 0; row < 8; row++)
        {
            Span<float> destination = padded.Slice(row * 8, 8);
            plane.Slice((Math.Min(top + row, height - 1) * width) + left, copied).CopyTo(destination);
            destination[copied..].Fill(destination[copied - 1]);
        }
        return TransformQuantizeCoreWithMask(padded, 8, centerInput: true, table, output, adaptiveStrength, clearDcOnlyAc);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TransformQuantizeCore(ReadOnlySpan<float> samples, int stride, bool centerInput,
        Quantization.PreparedTable table, Span<short> output, float adaptiveStrength, bool zigZagOrdered)
    {
        if (zigZagOrdered)
        {
            _ = TransformQuantizeCoreWithMask(samples, stride, centerInput, table, output, adaptiveStrength,
                clearDcOnlyAc: true, computeMask: false);
            return;
        }

        if (output.Length < 64)
        {
            throw new ArgumentException("A quantized block requires 64 values.", nameof(output));
        }

        Guard.ThrowIfNull(table);
        TransformQuantizeFallback(samples, stride, centerInput, table, output, adaptiveStrength, zigZagOrdered: false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong TransformQuantizeCoreWithMask(ReadOnlySpan<float> samples, int stride, bool centerInput,
        Quantization.PreparedTable table, Span<short> output, float adaptiveStrength, bool clearDcOnlyAc,
        bool computeMask = true)
    {
        if (output.Length < 64)
        {
            throw new ArgumentException("A quantized block requires 64 values.", nameof(output));
        }

        Guard.ThrowIfNull(table);
        if (table.TailZeroRunBias > 0f)
        {
            return TransformQuantizeBiasedWithMask(samples, stride, centerInput, table, output, adaptiveStrength, clearDcOnlyAc);
        }
#if !NETSTANDARD2_1
        if (Vector.IsHardwareAccelerated && Vector<float>.Count == 8
            && (!table.AdaptiveQuantization || (float.IsFinite(adaptiveStrength) && adaptiveStrength >= 0f)))
        {
            Quantization.TransposedTable transposed = table.GetTransposed();
            TransformEightLaneTransposed(samples, stride, centerInput,
                out Vector<float> r0, out Vector<float> r1, out Vector<float> r2, out Vector<float> r3,
                out Vector<float> r4, out Vector<float> r5, out Vector<float> r6, out Vector<float> r7);
            if (Quantization.TryQuantizeTransposed(r0.AsVector256(), r1.AsVector256(), r2.AsVector256(), r3.AsVector256(),
                r4.AsVector256(), r5.AsVector256(), r6.AsVector256(), r7.AsVector256(), table, transposed, adaptiveStrength,
                output, out ulong nonzeroAcMask, clearDcOnlyAc, computeMask))
            {
                return nonzeroAcMask;
            }
        }
        else if (Vector.IsHardwareAccelerated && Vector<float>.Count == 4
            && (!table.AdaptiveQuantization || (float.IsFinite(adaptiveStrength) && adaptiveStrength >= 0f))
            && TryTransformQuantizeFourLane(samples, stride, centerInput, table, output, adaptiveStrength))
        {
            return computeMask ? EntropyEncoder.NonzeroAcMask(output[..64]) : 0;
        }
#endif
        TransformQuantizeFallback(samples, stride, centerInput, table, output, adaptiveStrength, zigZagOrdered: true);
        return computeMask ? EntropyEncoder.NonzeroAcMask(output[..64]) : 0;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ulong TransformQuantizeBiasedWithMask(ReadOnlySpan<float> samples, int stride, bool centerInput,
        Quantization.PreparedTable table, Span<short> output, float adaptiveStrength, bool clearDcOnlyAc)
    {
#if !NETSTANDARD2_1
        if (Vector.IsHardwareAccelerated && Vector<float>.Count == 8
            && (!table.AdaptiveQuantization || (float.IsFinite(adaptiveStrength) && adaptiveStrength >= 0f)))
        {
            Quantization.TransposedTable transposed = table.GetTransposed();
            TransformEightLaneTransposed(samples, stride, centerInput,
                out Vector<float> r0, out Vector<float> r1, out Vector<float> r2, out Vector<float> r3,
                out Vector<float> r4, out Vector<float> r5, out Vector<float> r6, out Vector<float> r7);
            if (Quantization.TryQuantizeTransposed(r0.AsVector256(), r1.AsVector256(), r2.AsVector256(), r3.AsVector256(),
                r4.AsVector256(), r5.AsVector256(), r6.AsVector256(), r7.AsVector256(), table, transposed, adaptiveStrength,
                output, out ulong nonzeroAcMask, clearDcOnlyAc, computeMask: true))
            {
                if (Quantization.HasTailZeroRunCandidate(output, nonzeroAcMask))
                {
                    Quantization.PruneTailZeroRunTransposed(r0.AsVector256(), r1.AsVector256(), r2.AsVector256(), r3.AsVector256(),
                        r4.AsVector256(), r5.AsVector256(), r6.AsVector256(), r7.AsVector256(),
                        table, adaptiveStrength, output, ref nonzeroAcMask);
                }

                return nonzeroAcMask;
            }
        }
        else if (Vector.IsHardwareAccelerated && Vector<float>.Count == 4
            && (!table.AdaptiveQuantization || (float.IsFinite(adaptiveStrength) && adaptiveStrength >= 0f))
            && TryTransformQuantizeFourLane(samples, stride, centerInput, table, output, adaptiveStrength))
        {
            ulong nonzeroAcMask = EntropyEncoder.NonzeroAcMask(output[..64]);
            if (Quantization.HasTailZeroRunCandidate(output, nonzeroAcMask))
            {
                PruneTailZeroRunFromSamples(samples, stride, centerInput, table, adaptiveStrength, output, ref nonzeroAcMask);
            }

            return nonzeroAcMask;
        }
#endif
        TransformQuantizeFallback(samples, stride, centerInput, table, output, adaptiveStrength, zigZagOrdered: true);
        return EntropyEncoder.NonzeroAcMask(output[..64]);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PruneTailZeroRunFromSamples(ReadOnlySpan<float> samples, int stride, bool centerInput,
        Quantization.PreparedTable table, float adaptiveStrength, Span<short> output, ref ulong nonzeroAcMask)
    {
        Span<float> coefficients = stackalloc float[64];
        if (centerInput)
        {
            TransformPlane(samples, stride, 0, 0, coefficients);
        }
        else
        {
            Transform(samples, coefficients);
        }

        Quantization.PruneTailZeroRunZigZag(coefficients, table, adaptiveStrength, output, ref nonzeroAcMask);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void TransformQuantizeFallback(ReadOnlySpan<float> samples, int stride, bool centerInput,
        Quantization.PreparedTable table, Span<short> output, float adaptiveStrength, bool zigZagOrdered)
    {
        Span<float> coefficients = stackalloc float[64];
        Span<short> temporary = stackalloc short[64];
        if (centerInput)
        {
            TransformPlane(samples, stride, 0, 0, coefficients);
        }
        else
        {
            Transform(samples, coefficients);
        }

        Span<short> natural = zigZagOrdered ? temporary : output;
        Quantization.QuantizePrepared(coefficients, table, natural, adaptiveStrength);
        EntropyEncoder.ValidateAc(natural[..64]);
        if (zigZagOrdered)
        {
            ZigZagOrdering.Reorder(natural, output);
        }
    }

    private static void ValidatePlane(ReadOnlySpan<float> plane, int stride, int left, int top)
    {
        Guard.ThrowIfLessThan(stride, 8);
        Guard.ThrowIfNegative(left);
        Guard.ThrowIfNegative(top);
        Guard.ThrowIfGreaterThan(left, stride - 8);
        if ((((long)top + 7) * stride) + left + 8 > plane.Length)
        {
            throw new ArgumentException("The plane must contain the complete 8 by 8 block.", nameof(plane));
        }
    }

    // Explicit implementations also allow bitwise regression checks against
    // the scalar port, independently of the runtime's hardware dispatch.
    internal static void TransformScalar(ReadOnlySpan<float> samples, Span<float> coefficients)
    {
        ValidateBuffers(samples, coefficients);
        TransformScalarCore(samples, coefficients);
    }

    internal static void TransformVector(ReadOnlySpan<float> samples, Span<float> coefficients)
    {
        ValidateBuffers(samples, coefficients);
        if (CanUseVector)
        {
            TransformVectorCore(samples, coefficients);
        }
        else
        {
            TransformScalarCore(samples, coefficients);
        }
    }

    // A future runtime may use vectors wider than one eight-sample row.
    // Keep that case on the scalar path rather than reading outside a block.
    private static bool CanUseVector => Vector<float>.Count is 4 or 8;

    private static void ValidateBuffers(ReadOnlySpan<float> samples, Span<float> coefficients)
    {
        if (samples.Length != 64)
        {
            throw new ArgumentException("An 8 by 8 block requires exactly 64 samples.", nameof(samples));
        }

        if (coefficients.Length < 64)
        {
            throw new ArgumentException("The coefficient buffer requires at least 64 values.", nameof(coefficients));
        }
    }

    private static void TransformScalarCore(ReadOnlySpan<float> samples, Span<float> coefficients)
    {
        Span<float> intermediate = stackalloc float[64];
        Span<float> lane = stackalloc float[8];

        // TransformFromPixels first transforms down columns. All input is
        // consumed into intermediate before any output is written, so spans
        // may overlap. This preserves the C++ order without explicit transposes.
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                lane[y] = samples[(y * 8) + x];
            }

            TransformEightScalar(lane);
            for (int v = 0; v < 8; v++)
            {
                intermediate[(v * 8) + x] = lane[v] * 0.125f;
            }
        }

        // C++ StoreToBlockAndScale divides BOTH 1D passes by 8. Its result
        // is standard JPEG coefficients / 8. Omit the second division here:
        // the total gain is orthonormal and a constant block c has DC = 8*c.
        // Input is already level-shifted; C++ centers DC during quantization.
        for (int v = 0; v < 8; v++)
        {
            Span<float> row = intermediate.Slice(v * 8, 8);
            TransformEightScalar(row);
            row.CopyTo(coefficients.Slice(v * 8, 8));
        }
    }

#if !NETSTANDARD2_1
    private static void TransformVectorCore(ReadOnlySpan<float> samples, Span<float> coefficients)
    {
        if (Vector<float>.Count == 8)
        {
            TransformEightLane(samples, 8, centerInput: false, coefficients);
            return;
        }
        TransformFourLane(samples, 8, centerInput: false, coefficients);
    }

    private static void TransformFourLane(ReadOnlySpan<float> samples, int stride, bool centerInput,
        Span<float> coefficients)
    {
        // Keep both four-column panels in locals. Stack-allocated vector spans
        // require zeroing and force intermediate rows through memory per block.
        Vector<float> a0 = new(samples[..4]);
        Vector<float> a1 = new(samples.Slice(stride, 4));
        Vector<float> a2 = new(samples.Slice(2 * stride, 4));
        Vector<float> a3 = new(samples.Slice(3 * stride, 4));
        Vector<float> a4 = new(samples.Slice(4 * stride, 4));
        Vector<float> a5 = new(samples.Slice(5 * stride, 4));
        Vector<float> a6 = new(samples.Slice(6 * stride, 4));
        Vector<float> a7 = new(samples.Slice(7 * stride, 4));
        Vector<float> b0 = new(samples.Slice(4, 4));
        Vector<float> b1 = new(samples.Slice(stride + 4, 4));
        Vector<float> b2 = new(samples.Slice((2 * stride) + 4, 4));
        Vector<float> b3 = new(samples.Slice((3 * stride) + 4, 4));
        Vector<float> b4 = new(samples.Slice((4 * stride) + 4, 4));
        Vector<float> b5 = new(samples.Slice((5 * stride) + 4, 4));
        Vector<float> b6 = new(samples.Slice((6 * stride) + 4, 4));
        Vector<float> b7 = new(samples.Slice((7 * stride) + 4, 4));
        if (centerInput)
        {
            var center = new Vector<float>(128f);
            a0 -= center;
            b0 -= center;
            a1 -= center;
            b1 -= center;
            a2 -= center;
            b2 -= center;
            a3 -= center;
            b3 -= center;
            a4 -= center;
            b4 -= center;
            a5 -= center;
            b5 -= center;
            a6 -= center;
            b6 -= center;
            a7 -= center;
            b7 -= center;
        }
        TransformRecursiveVector(ref a0, ref a1, ref a2, ref a3, ref a4, ref a5, ref a6, ref a7);
        TransformRecursiveVector(ref b0, ref b1, ref b2, ref b3, ref b4, ref b5, ref b6, ref b7);
        var scale = new Vector<float>(0.125f);
        a0 *= scale;
        b0 *= scale;
        a1 *= scale;
        b1 *= scale;
        a2 *= scale;
        b2 *= scale;
        a3 *= scale;
        b3 *= scale;
        a4 *= scale;
        b4 *= scale;
        a5 *= scale;
        b5 *= scale;
        a6 *= scale;
        b6 *= scale;
        a7 *= scale;
        b7 *= scale;
        // Four quadrant transposes turn the column panels into horizontal DCTs.
        TransformFourRows(ref a0, ref a1, ref a2, ref a3, ref b0, ref b1, ref b2, ref b3, coefficients[..32]);
        TransformFourRows(ref a4, ref a5, ref a6, ref a7, ref b4, ref b5, ref b6, ref b7, coefficients.Slice(32, 32));
    }

    private static bool TryTransformQuantizeFourLane(ReadOnlySpan<float> samples, int stride, bool centerInput,
        Quantization.PreparedTable table, Span<short> output, float adaptiveStrength)
    {
        // Retain the four-column DCT schedule. Quantize its two transposed
        // frequency panels before their final float transpose and stores.
        Vector<float> a0 = new(samples[..4]);
        Vector<float> a1 = new(samples.Slice(stride, 4));
        Vector<float> a2 = new(samples.Slice(2 * stride, 4));
        Vector<float> a3 = new(samples.Slice(3 * stride, 4));
        Vector<float> a4 = new(samples.Slice(4 * stride, 4));
        Vector<float> a5 = new(samples.Slice(5 * stride, 4));
        Vector<float> a6 = new(samples.Slice(6 * stride, 4));
        Vector<float> a7 = new(samples.Slice(7 * stride, 4));
        Vector<float> b0 = new(samples.Slice(4, 4));
        Vector<float> b1 = new(samples.Slice(stride + 4, 4));
        Vector<float> b2 = new(samples.Slice((2 * stride) + 4, 4));
        Vector<float> b3 = new(samples.Slice((3 * stride) + 4, 4));
        Vector<float> b4 = new(samples.Slice((4 * stride) + 4, 4));
        Vector<float> b5 = new(samples.Slice((5 * stride) + 4, 4));
        Vector<float> b6 = new(samples.Slice((6 * stride) + 4, 4));
        Vector<float> b7 = new(samples.Slice((7 * stride) + 4, 4));
        if (centerInput)
        {
            var center = new Vector<float>(128f);
            a0 -= center;
            b0 -= center;
            a1 -= center;
            b1 -= center;
            a2 -= center;
            b2 -= center;
            a3 -= center;
            b3 -= center;
            a4 -= center;
            b4 -= center;
            a5 -= center;
            b5 -= center;
            a6 -= center;
            b6 -= center;
            a7 -= center;
            b7 -= center;
        }
        TransformRecursiveVector(ref a0, ref a1, ref a2, ref a3, ref a4, ref a5, ref a6, ref a7);
        TransformRecursiveVector(ref b0, ref b1, ref b2, ref b3, ref b4, ref b5, ref b6, ref b7);
        var scale = new Vector<float>(0.125f);
        a0 *= scale;
        b0 *= scale;
        a1 *= scale;
        b1 *= scale;
        a2 *= scale;
        b2 *= scale;
        a3 *= scale;
        b3 *= scale;
        a4 *= scale;
        b4 *= scale;
        a5 *= scale;
        b5 *= scale;
        a6 *= scale;
        b6 *= scale;
        a7 *= scale;
        b7 *= scale;

        // Staging shorts bounds live vector storage and keeps caller output
        // untouched if a coefficient requires the strict original fallback.
        Span<short> quantized = stackalloc short[64];
        Quantization.TransposedTable transposed = table.GetTransposed();
        TransformFourRowsTransposed(ref a0, ref a1, ref a2, ref a3, ref b0, ref b1, ref b2, ref b3);
        bool valid = Quantization.QuantizeTransposedFourRows(a0.AsVector128(), a1.AsVector128(), a2.AsVector128(), a3.AsVector128(),
            b0.AsVector128(), b1.AsVector128(), b2.AsVector128(), b3.AsVector128(), table, transposed, adaptiveStrength, 0, quantized);
        TransformFourRowsTransposed(ref a4, ref a5, ref a6, ref a7, ref b4, ref b5, ref b6, ref b7);
        valid &= Quantization.QuantizeTransposedFourRows(a4.AsVector128(), a5.AsVector128(), a6.AsVector128(), a7.AsVector128(),
            b4.AsVector128(), b5.AsVector128(), b6.AsVector128(), b7.AsVector128(), table, transposed, adaptiveStrength, 4, quantized);
        if (!valid)
        {
            return false;
        }

        Quantization.StoreTransposedZigZag128(Vector128.Create(quantized[..8]), Vector128.Create(quantized.Slice(8, 8)),
            Vector128.Create(quantized.Slice(16, 8)), Vector128.Create(quantized.Slice(24, 8)),
            Vector128.Create(quantized.Slice(32, 8)), Vector128.Create(quantized.Slice(40, 8)),
            Vector128.Create(quantized.Slice(48, 8)), Vector128.Create(quantized.Slice(56, 8)), output, table.NarrowZigZagKernel);
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TransformFourRows(ref Vector<float> a0, ref Vector<float> a1,
        ref Vector<float> a2, ref Vector<float> a3, ref Vector<float> b0, ref Vector<float> b1,
        ref Vector<float> b2, ref Vector<float> b3, Span<float> coefficients)
    {
        TransformFourRowsTransposed(ref a0, ref a1, ref a2, ref a3, ref b0, ref b1, ref b2, ref b3);
        TransposeFourRows(ref a0, ref a1, ref a2, ref a3);
        TransposeFourRows(ref b0, ref b1, ref b2, ref b3);
        a0.CopyTo(coefficients[..4]);
        b0.CopyTo(coefficients.Slice(4, 4));
        a1.CopyTo(coefficients.Slice(8, 4));
        b1.CopyTo(coefficients.Slice(12, 4));
        a2.CopyTo(coefficients.Slice(16, 4));
        b2.CopyTo(coefficients.Slice(20, 4));
        a3.CopyTo(coefficients.Slice(24, 4));
        b3.CopyTo(coefficients.Slice(28, 4));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TransformFourRowsTransposed(ref Vector<float> a0, ref Vector<float> a1,
        ref Vector<float> a2, ref Vector<float> a3, ref Vector<float> b0, ref Vector<float> b1,
        ref Vector<float> b2, ref Vector<float> b3)
    {
        TransposeFourRows(ref a0, ref a1, ref a2, ref a3);
        TransposeFourRows(ref b0, ref b1, ref b2, ref b3);
        TransformRecursiveVector(ref a0, ref a1, ref a2, ref a3, ref b0, ref b1, ref b2, ref b3);
    }

    private static void TransformEightLane(ReadOnlySpan<float> samples, int stride, bool centerInput,
        Span<float> coefficients)
    {
        TransformEightLaneTransposed(samples, stride, centerInput,
            out Vector<float> r0, out Vector<float> r1, out Vector<float> r2, out Vector<float> r3,
            out Vector<float> r4, out Vector<float> r5, out Vector<float> r6, out Vector<float> r7);
        TransposeEightRows(ref r0, ref r1, ref r2, ref r3, ref r4, ref r5, ref r6, ref r7);
        r0.CopyTo(coefficients[..8]);
        r1.CopyTo(coefficients.Slice(8, 8));
        r2.CopyTo(coefficients.Slice(16, 8));
        r3.CopyTo(coefficients.Slice(24, 8));
        r4.CopyTo(coefficients.Slice(32, 8));
        r5.CopyTo(coefficients.Slice(40, 8));
        r6.CopyTo(coefficients.Slice(48, 8));
        r7.CopyTo(coefficients.Slice(56, 8));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TransformEightLaneTransposed(ReadOnlySpan<float> samples, int stride, bool centerInput,
        out Vector<float> r0, out Vector<float> r1, out Vector<float> r2, out Vector<float> r3,
        out Vector<float> r4, out Vector<float> r5, out Vector<float> r6, out Vector<float> r7)
    {
        // Read every row before storing output, preserving overlapping spans.
        r0 = new(samples[..8]);
        r1 = new(samples.Slice(stride, 8));
        r2 = new(samples.Slice(2 * stride, 8));
        r3 = new(samples.Slice(3 * stride, 8));
        r4 = new(samples.Slice(4 * stride, 8));
        r5 = new(samples.Slice(5 * stride, 8));
        r6 = new(samples.Slice(6 * stride, 8));
        r7 = new(samples.Slice(7 * stride, 8));
        if (centerInput)
        {
            var center = new Vector<float>(128f);
            r0 -= center;
            r1 -= center;
            r2 -= center;
            r3 -= center;
            r4 -= center;
            r5 -= center;
            r6 -= center;
            r7 -= center;
        }
        TransformRecursiveVector(ref r0, ref r1, ref r2, ref r3, ref r4, ref r5, ref r6, ref r7);
        var scale = new Vector<float>(0.125f);
        r0 *= scale;
        r1 *= scale;
        r2 *= scale;
        r3 *= scale;
        r4 *= scale;
        r5 *= scale;
        r6 *= scale;
        r7 *= scale;
        TransposeEightRows(ref r0, ref r1, ref r2, ref r3, ref r4, ref r5, ref r6, ref r7);
        TransformRecursiveVector(ref r0, ref r1, ref r2, ref r3, ref r4, ref r5, ref r6, ref r7);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TransposeEightRows(ref Vector<float> r0, ref Vector<float> r1,
        ref Vector<float> r2, ref Vector<float> r3, ref Vector<float> r4, ref Vector<float> r5,
        ref Vector<float> r6, ref Vector<float> r7)
    {
        Vector256<float> v0 = r0.AsVector256();
        Vector256<float> v1 = r1.AsVector256();
        Vector256<float> v2 = r2.AsVector256();
        Vector256<float> v3 = r3.AsVector256();
        Vector256<float> v4 = r4.AsVector256();
        Vector256<float> v5 = r5.AsVector256();
        Vector256<float> v6 = r6.AsVector256();
        Vector256<float> v7 = r7.AsVector256();
        TransposeEightRows(ref v0, ref v1, ref v2, ref v3, ref v4, ref v5, ref v6, ref v7);
        r0 = v0.AsVector();
        r1 = v1.AsVector();
        r2 = v2.AsVector();
        r3 = v3.AsVector();
        r4 = v4.AsVector();
        r5 = v5.AsVector();
        r6 = v6.AsVector();
        r7 = v7.AsVector();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void TransposePortable(Span<Vector256<float>> rows)
    {
        if (rows.Length < 8)
        {
            throw new ArgumentException("An 8 by 8 transpose requires eight vector rows.", nameof(rows));
        }

        TransposeEightRows(ref rows[0], ref rows[1], ref rows[2], ref rows[3],
            ref rows[4], ref rows[5], ref rows[6], ref rows[7]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TransposeEightRows(ref Vector256<float> r0, ref Vector256<float> r1,
        ref Vector256<float> r2, ref Vector256<float> r3, ref Vector256<float> r4, ref Vector256<float> r5,
        ref Vector256<float> r6, ref Vector256<float> r7)
    {
#if NET11_0_OR_GREATER && DOTJPEGLI_NET11_SIMD && DOTJPEGLI_FULL_WIDTH_TRANSPOSE
        // Retain the full-width schedule for isolated same-host CI comparison.
        Vector256<float> t0 = Vector256.ZipLower(r0, r1), t1 = Vector256.ZipUpper(r0, r1);
        Vector256<float> t2 = Vector256.ZipLower(r2, r3), t3 = Vector256.ZipUpper(r2, r3);
        Vector256<float> t4 = Vector256.ZipLower(r4, r5), t5 = Vector256.ZipUpper(r4, r5);
        Vector256<float> t6 = Vector256.ZipLower(r6, r7), t7 = Vector256.ZipUpper(r6, r7);
        Vector256<float> u0 = Vector256.ZipLower(t0.AsDouble(), t2.AsDouble()).AsSingle();
        Vector256<float> u1 = Vector256.ZipUpper(t0.AsDouble(), t2.AsDouble()).AsSingle();
        Vector256<float> u2 = Vector256.ZipLower(t1.AsDouble(), t3.AsDouble()).AsSingle();
        Vector256<float> u3 = Vector256.ZipUpper(t1.AsDouble(), t3.AsDouble()).AsSingle();
        Vector256<float> u4 = Vector256.ZipLower(t4.AsDouble(), t6.AsDouble()).AsSingle();
        Vector256<float> u5 = Vector256.ZipUpper(t4.AsDouble(), t6.AsDouble()).AsSingle();
        Vector256<float> u6 = Vector256.ZipLower(t5.AsDouble(), t7.AsDouble()).AsSingle();
        Vector256<float> u7 = Vector256.ZipUpper(t5.AsDouble(), t7.AsDouble()).AsSingle();
        r0 = Vector256.Create(u0.GetLower(), u4.GetLower());
        r1 = Vector256.Create(u0.GetUpper(), u4.GetUpper());
        r2 = Vector256.Create(u1.GetLower(), u5.GetLower());
        r3 = Vector256.Create(u1.GetUpper(), u5.GetUpper());
        r4 = Vector256.Create(u2.GetLower(), u6.GetLower());
        r5 = Vector256.Create(u2.GetUpper(), u6.GetUpper());
        r6 = Vector256.Create(u3.GetLower(), u7.GetLower());
        r7 = Vector256.Create(u3.GetUpper(), u7.GetUpper());
#else
        // Keep the released four-panel schedule. Full-width Zip decomposes
        // into extra half extractions and joins on 256-bit x64 hosts; fewer
        // managed operations do not imply fewer emitted instructions.
        Vector128<float> a0 = r0.GetLower(), a1 = r1.GetLower(), a2 = r2.GetLower(), a3 = r3.GetLower();
        Vector128<float> b0 = r0.GetUpper(), b1 = r1.GetUpper(), b2 = r2.GetUpper(), b3 = r3.GetUpper();
        Vector128<float> c0 = r4.GetLower(), c1 = r5.GetLower(), c2 = r6.GetLower(), c3 = r7.GetLower();
        Vector128<float> d0 = r4.GetUpper(), d1 = r5.GetUpper(), d2 = r6.GetUpper(), d3 = r7.GetUpper();
        Transpose4x4(ref a0, ref a1, ref a2, ref a3);
        Transpose4x4(ref b0, ref b1, ref b2, ref b3);
        Transpose4x4(ref c0, ref c1, ref c2, ref c3);
        Transpose4x4(ref d0, ref d1, ref d2, ref d3);
        r0 = Vector256.Create(a0, c0);
        r1 = Vector256.Create(a1, c1);
        r2 = Vector256.Create(a2, c2);
        r3 = Vector256.Create(a3, c3);
        r4 = Vector256.Create(b0, d0);
        r5 = Vector256.Create(b1, d1);
        r6 = Vector256.Create(b2, d2);
        r7 = Vector256.Create(b3, d3);
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TransposeFourRows(ref Vector<float> r0, ref Vector<float> r1,
        ref Vector<float> r2, ref Vector<float> r3)
    {
        Vector128<float> v0 = r0.AsVector128(), v1 = r1.AsVector128();
        Vector128<float> v2 = r2.AsVector128(), v3 = r3.AsVector128();
        Transpose4x4(ref v0, ref v1, ref v2, ref v3);
        r0 = v0.AsVector();
        r1 = v1.AsVector();
        r2 = v2.AsVector();
        r3 = v3.AsVector();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Transpose4x4(ref Vector128<float> r0, ref Vector128<float> r1,
        ref Vector128<float> r2, ref Vector128<float> r3)
    {
        Vector128<float> t0 = ZipLower(r0, r1), t1 = ZipUpper(r0, r1);
        Vector128<float> t2 = ZipLower(r2, r3), t3 = ZipUpper(r2, r3);
        r0 = ZipLower(t0.AsDouble(), t2.AsDouble()).AsSingle();
        r1 = ZipUpper(t0.AsDouble(), t2.AsDouble()).AsSingle();
        r2 = ZipLower(t1.AsDouble(), t3.AsDouble()).AsSingle();
        r3 = ZipUpper(t1.AsDouble(), t3.AsDouble()).AsSingle();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> ZipLower(Vector128<float> left, Vector128<float> right)
#if NET11_0_OR_GREATER && DOTJPEGLI_NET11_SIMD
        => Vector128.ZipLower(left, right);
#else
        // Keep both inputs as full vectors. Constructing a Vector128 from
        // Vector64 halves can move these bits through scalar stack slots.
        => Vector128.ConditionalSelect(Vector128.Create(-1, 0, -1, 0).AsSingle(),
            Vector128.Shuffle(left, Vector128.Create(0, 0, 1, 1)),
            Vector128.Shuffle(right, Vector128.Create(0, 0, 1, 1)));
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> ZipUpper(Vector128<float> left, Vector128<float> right)
#if NET11_0_OR_GREATER && DOTJPEGLI_NET11_SIMD
        => Vector128.ZipUpper(left, right);
#else
        => Vector128.ConditionalSelect(Vector128.Create(-1, 0, -1, 0).AsSingle(),
            Vector128.Shuffle(left, Vector128.Create(2, 2, 3, 3)),
            Vector128.Shuffle(right, Vector128.Create(2, 2, 3, 3)));
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<double> ZipLower(Vector128<double> left, Vector128<double> right)
#if NET11_0_OR_GREATER && DOTJPEGLI_NET11_SIMD
        => Vector128.ZipLower(left, right);
#else
        => Vector128.ConditionalSelect(Vector128.Create(-1L, 0L).AsDouble(), left,
            Vector128.Shuffle(right, Vector128.Create(0L, 0L)));
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<double> ZipUpper(Vector128<double> left, Vector128<double> right)
#if NET11_0_OR_GREATER && DOTJPEGLI_NET11_SIMD
        => Vector128.ZipUpper(left, right);
#else
        => Vector128.ConditionalSelect(Vector128.Create(-1L, 0L).AsDouble(),
            Vector128.Shuffle(left, Vector128.Create(1L, 1L)), right);
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TransformRecursiveVector(ref Vector<float> v0, ref Vector<float> v1,
        ref Vector<float> v2, ref Vector<float> v3, ref Vector<float> v4, ref Vector<float> v5,
        ref Vector<float> v6, ref Vector<float> v7)
    {
        // Specialize the fixed eight-point recursion. This keeps each stage in
        // registers across both passes without a vector scratch span.
        Vector<float> e0 = v0 + v7;
        Vector<float> e1 = v1 + v6;
        Vector<float> e2 = v2 + v5;
        Vector<float> e3 = v3 + v4;
        Transform4Vector(ref e0, ref e1, ref e2, ref e3);
        Vector<float> o0 = (v0 - v7) * new Vector<float>(0.5097955791041592f);
        Vector<float> o1 = (v1 - v6) * new Vector<float>(0.6013448869350453f);
        Vector<float> o2 = (v2 - v5) * new Vector<float>(0.8999762231364156f);
        Vector<float> o3 = (v3 - v4) * new Vector<float>(2.5629154477415055f);
        Transform4Vector(ref o0, ref o1, ref o2, ref o3);
        o0 = FloatingPointMath.FusedMultiplyAdd(o0, new Vector<float>(Sqrt2), o1);
        o1 += o2;
        o2 += o3;
        v0 = e0;
        v1 = o0;
        v2 = e1;
        v3 = o1;
        v4 = e2;
        v5 = o2;
        v6 = e3;
        v7 = o3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Transform4Vector(ref Vector<float> v0, ref Vector<float> v1,
        ref Vector<float> v2, ref Vector<float> v3)
    {
        Vector<float> e0 = v0 + v3;
        Vector<float> e1 = v1 + v2;
        Vector<float> o0 = (v0 - v3) * new Vector<float>(0.541196100146197f);
        Vector<float> o1 = (v1 - v2) * new Vector<float>(1.3065629648763764f);
        Vector<float> first = o0 + o1;
        Vector<float> second = o0 - o1;
        v0 = e0 + e1;
        v1 = FloatingPointMath.FusedMultiplyAdd(first, new Vector<float>(Sqrt2), second);
        v2 = e0 - e1;
        v3 = second;
    }

#else
    private static void TransformVectorCore(ReadOnlySpan<float> samples, Span<float> coefficients)
        => TransformScalarCore(samples, coefficients);

#endif
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TransformEightScalar(Span<float> values)
    {
        // Specialize the fixed recursion without scratch spans. Each lane keeps
        // the recursive port's operation order, including its fused multiply-adds.
        float e0 = values[0] + values[7];
        float e1 = values[1] + values[6];
        float e2 = values[2] + values[5];
        float e3 = values[3] + values[4];
        TransformFourScalar(ref e0, ref e1, ref e2, ref e3);
        float o0 = (values[0] - values[7]) * 0.5097955791041592f;
        float o1 = (values[1] - values[6]) * 0.6013448869350453f;
        float o2 = (values[2] - values[5]) * 0.8999762231364156f;
        float o3 = (values[3] - values[4]) * 2.5629154477415055f;
        TransformFourScalar(ref o0, ref o1, ref o2, ref o3);
        o0 = FloatingPointMath.FusedMultiplyAdd(o0, Sqrt2, o1);
        o1 += o2;
        o2 += o3;
        values[0] = e0;
        values[1] = o0;
        values[2] = e1;
        values[3] = o1;
        values[4] = e2;
        values[5] = o2;
        values[6] = e3;
        values[7] = o3;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TransformFourScalar(ref float v0, ref float v1, ref float v2, ref float v3)
    {
        float e0 = v0 + v3;
        float e1 = v1 + v2;
        float o0 = (v0 - v3) * 0.541196100146197f;
        float o1 = (v1 - v2) * 1.3065629648763764f;
        float first = o0 + o1;
        float second = o0 - o1;
        v0 = e0 + e1;
        v1 = FloatingPointMath.FusedMultiplyAdd(first, Sqrt2, second);
        v2 = e0 - e1;
        v3 = second;
    }
}
