// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by a BSD-style license that can be
// found in LICENSE.jpegli or at https://developers.google.com/open-source/licenses/bsd.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace DotJpegli.Internal;

/// <summary>Managed scalar and SIMD implementations of Google's recursive JPEGli DCT.</summary>
internal static class ForwardDct
{
    // Directly derived from google/jpegli lib/jpegli/dct-inl.h:
    // AddReverse, SubReverse, Multiply, B, InverseEvenOdd, DCT1DImpl,
    // StoreToBlockAndScale, and TransformFromPixels.
    // Source revision: 031a0077f5799a6041004267fc12b956c1f52a20.
    private const float Sqrt2 = 1.41421356237f;

    // WcMultipliers<N>: 1 / (2*cos((i + 0.5)*pi/N)).
    private static ReadOnlySpan<float> Wc4 => [0.541196100146197f, 1.3065629648763764f];
    private static ReadOnlySpan<float> Wc8 =>
    [
        0.5097955791041592f,
        0.6013448869350453f,
        0.8999762231364156f,
        2.5629154477415055f
    ];

    /// <summary>
    /// Transforms 64 row-major, level-shifted samples into standard JPEG
    /// orthonormal coefficients in row-major frequency order.
    /// </summary>
    internal static void Transform(ReadOnlySpan<float> samples, Span<float> coefficients)
    {
        ValidateBuffers(samples, coefficients);
        if (Vector.IsHardwareAccelerated && CanUseVector)
            TransformVectorCore(samples, coefficients);
        else
            TransformScalarCore(samples, coefficients);
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
            TransformVectorCore(samples, coefficients);
        else
            TransformScalarCore(samples, coefficients);
    }

    // A future runtime may use vectors wider than one eight-sample row.
    // Keep that case on the scalar path rather than reading outside a block.
    private static bool CanUseVector => Vector<float>.Count <= 8 && 8 % Vector<float>.Count == 0;

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
                lane[y] = samples[y * 8 + x];
            }

            TransformRecursive(lane);
            for (int v = 0; v < 8; v++)
            {
                intermediate[v * 8 + x] = lane[v] * 0.125f;
            }
        }

        // C++ StoreToBlockAndScale divides BOTH 1D passes by 8. Its result
        // is standard JPEG coefficients / 8. Omit the second division here:
        // the total gain is orthonormal and a constant block c has DC = 8*c.
        // Input is already level-shifted; C++ centers DC during quantization.
        for (int v = 0; v < 8; v++)
        {
            Span<float> row = intermediate.Slice(v * 8, 8);
            TransformRecursive(row);
            row.CopyTo(coefficients.Slice(v * 8, 8));
        }
    }

    private static void TransformVectorCore(ReadOnlySpan<float> samples, Span<float> coefficients)
    {
        if (Vector<float>.Count == 8 && Avx.IsSupported)
        {
            TransformAvx(samples, coefficients);
            return;
        }
        Span<float> intermediate = stackalloc float[64];
        Span<Vector<float>> lanes = stackalloc Vector<float>[8];
        int width = Vector<float>.Count;
        var scale = new Vector<float>(0.125f);

        // Each vector holds independent columns. Transpose the first pass
        // into intermediate so the second pass can load independent rows.
        // Consume all source samples before writing any coefficients to
        // retain the scalar path's support for overlapping spans.
        for (int x = 0; x < 8; x += width)
        {
            for (int y = 0; y < 8; y++)
                lanes[y] = new Vector<float>(samples.Slice(y * 8 + x, width));
            TransformRecursiveVector(lanes);
            for (int v = 0; v < 8; v++)
            {
                Vector<float> scaled = lanes[v] * scale;
                for (int lane = 0; lane < width; lane++)
                    intermediate[(x + lane) * 8 + v] = scaled[lane];
            }
        }

        for (int v = 0; v < 8; v += width)
        {
            for (int x = 0; x < 8; x++)
                lanes[x] = new Vector<float>(intermediate.Slice(x * 8 + v, width));
            TransformRecursiveVector(lanes);
            for (int u = 0; u < 8; u++)
                for (int lane = 0; lane < width; lane++)
                    coefficients[(v + lane) * 8 + u] = lanes[u][lane];
        }
    }

    private static void TransformAvx(ReadOnlySpan<float> samples, Span<float> coefficients)
    {
        Span<Vector<float>> rows = stackalloc Vector<float>[8];
        for (int row = 0; row < 8; row++)
            rows[row] = new Vector<float>(samples.Slice(row * 8, 8));
        TransformRecursiveVector(rows);
        for (int row = 0; row < 8; row++)
            rows[row] *= new Vector<float>(0.125f);
        TransposeAvx(rows);
        TransformRecursiveVector(rows);
        TransposeAvx(rows);
        for (int row = 0; row < 8; row++)
            rows[row].CopyTo(coefficients.Slice(row * 8, 8));
    }

    // Two in-register transposes replace the scalar lane extraction/gathers
    // between the column and row transforms. Only data movement changes.
    private static void TransposeAvx(Span<Vector<float>> rows)
    {
        Vector256<float> t0 = Avx.UnpackLow(rows[0].AsVector256(), rows[1].AsVector256());
        Vector256<float> t1 = Avx.UnpackHigh(rows[0].AsVector256(), rows[1].AsVector256());
        Vector256<float> t2 = Avx.UnpackLow(rows[2].AsVector256(), rows[3].AsVector256());
        Vector256<float> t3 = Avx.UnpackHigh(rows[2].AsVector256(), rows[3].AsVector256());
        Vector256<float> t4 = Avx.UnpackLow(rows[4].AsVector256(), rows[5].AsVector256());
        Vector256<float> t5 = Avx.UnpackHigh(rows[4].AsVector256(), rows[5].AsVector256());
        Vector256<float> t6 = Avx.UnpackLow(rows[6].AsVector256(), rows[7].AsVector256());
        Vector256<float> t7 = Avx.UnpackHigh(rows[6].AsVector256(), rows[7].AsVector256());
        Vector256<float> s0 = Avx.Shuffle(t0, t2, 0x44);
        Vector256<float> s1 = Avx.Shuffle(t0, t2, 0xee);
        Vector256<float> s2 = Avx.Shuffle(t1, t3, 0x44);
        Vector256<float> s3 = Avx.Shuffle(t1, t3, 0xee);
        Vector256<float> s4 = Avx.Shuffle(t4, t6, 0x44);
        Vector256<float> s5 = Avx.Shuffle(t4, t6, 0xee);
        Vector256<float> s6 = Avx.Shuffle(t5, t7, 0x44);
        Vector256<float> s7 = Avx.Shuffle(t5, t7, 0xee);
        rows[0] = Avx.Permute2x128(s0, s4, 0x20).AsVector();
        rows[1] = Avx.Permute2x128(s1, s5, 0x20).AsVector();
        rows[2] = Avx.Permute2x128(s2, s6, 0x20).AsVector();
        rows[3] = Avx.Permute2x128(s3, s7, 0x20).AsVector();
        rows[4] = Avx.Permute2x128(s0, s4, 0x31).AsVector();
        rows[5] = Avx.Permute2x128(s1, s5, 0x31).AsVector();
        rows[6] = Avx.Permute2x128(s2, s6, 0x31).AsVector();
        rows[7] = Avx.Permute2x128(s3, s7, 0x31).AsVector();
    }

    private static void TransformRecursiveVector(Span<Vector<float>> values)
    {
        // Specialize the fixed eight-point recursion. This keeps each stage in
        // registers instead of recursively allocating and slicing vector scratch.
        Vector<float> e0 = values[0] + values[7];
        Vector<float> e1 = values[1] + values[6];
        Vector<float> e2 = values[2] + values[5];
        Vector<float> e3 = values[3] + values[4];
        Transform4Vector(ref e0, ref e1, ref e2, ref e3);
        Vector<float> o0 = (values[0] - values[7]) * new Vector<float>(0.5097955791041592f);
        Vector<float> o1 = (values[1] - values[6]) * new Vector<float>(0.6013448869350453f);
        Vector<float> o2 = (values[2] - values[5]) * new Vector<float>(0.8999762231364156f);
        Vector<float> o3 = (values[3] - values[4]) * new Vector<float>(2.5629154477415055f);
        Transform4Vector(ref o0, ref o1, ref o2, ref o3);
        o0 = Vector.FusedMultiplyAdd(o0, new Vector<float>(Sqrt2), o1);
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
        v1 = Vector.FusedMultiplyAdd(first, new Vector<float>(Sqrt2), second);
        v2 = e0 - e1;
        v3 = second;
    }

    private static void TransformRecursive(Span<float> values)
    {
        int size = values.Length;
        if (size == 2)
        {
            // DCT1DImpl<2>.
            float first = values[0];
            float second = values[1];
            values[0] = first + second;
            values[1] = first - second;
            return;
        }

        // DCT1DImpl<N>, where N is 4 or 8. The C++ templates operate on
        // parallel SIMD lanes; this span represents one scalar lane.
        int half = size / 2;
        Span<float> temporary = stackalloc float[8];
        Span<float> even = temporary[..half];
        Span<float> odd = temporary.Slice(half, half);
        for (int i = 0; i < half; i++)
        {
            even[i] = values[i] + values[size - 1 - i];
        }

        TransformRecursive(even);
        for (int i = 0; i < half; i++)
        {
            odd[i] = values[i] - values[size - 1 - i];
        }

        ReadOnlySpan<float> multipliers = size == 4 ? Wc4 : Wc8;
        for (int i = 0; i < half; i++)
        {
            odd[i] *= multipliers[i];
        }

        TransformRecursive(odd);

        // B<N/2> uses Highway MulAdd for this first operation.
        odd[0] = MathF.FusedMultiplyAdd(odd[0], Sqrt2, odd[1]);
        for (int i = 1; i + 1 < half; i++)
        {
            odd[i] += odd[i + 1];
        }

        // InverseEvenOdd<N> interleaves the recursively transformed halves.
        for (int i = 0; i < half; i++)
        {
            values[2 * i] = even[i];
            values[2 * i + 1] = odd[i];
        }
    }
}
