// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by a BSD-style license that can be
// found in LICENSE.jpegli or at https://developers.google.com/open-source/licenses/bsd.

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;

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

    /// <summary>Loads and level-shifts an interior 8 by 8 block directly from an uncentered plane.</summary>
    internal static void TransformPlane(ReadOnlySpan<float> plane, int stride, int left, int top,
        Span<float> coefficients)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(stride, 8);
        ArgumentOutOfRangeException.ThrowIfNegative(left);
        ArgumentOutOfRangeException.ThrowIfNegative(top);
        if (left > stride - 8)
            throw new ArgumentOutOfRangeException(nameof(left));
        if (((long)top + 7) * stride + left + 8 > plane.Length)
            throw new ArgumentException("The plane must contain the complete 8 by 8 block.", nameof(plane));
        if (coefficients.Length < 64)
            throw new ArgumentException("The coefficient buffer requires at least 64 values.", nameof(coefficients));

        if (Vector.IsHardwareAccelerated && Vector<float>.Count == 8)
        {
            Span<Vector<float>> rows = stackalloc Vector<float>[8];
            var center = new Vector<float>(128f);
            // Consume the entire source before writing output, even when the
            // coefficient span overlaps any of these strided input rows.
            for (int row = 0; row < 8; row++)
                rows[row] = new Vector<float>(plane.Slice((top + row) * stride + left, 8)) - center;
            TransformEightLaneRows(rows, coefficients);
        }
        else if (Vector.IsHardwareAccelerated && Vector<float>.Count == 4)
        {
            TransformFourLane(plane[(top * stride + left)..], stride, centerInput: true, coefficients);
        }
        else
        {
            Span<float> samples = stackalloc float[64];
            for (int row = 0; row < 8; row++)
                for (int column = 0; column < 8; column++)
                    samples[row * 8 + column] = plane[(top + row) * stride + left + column] - 128f;
            Transform(samples, coefficients);
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
            TransformVectorCore(samples, coefficients);
        else
            TransformScalarCore(samples, coefficients);
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
        if (Vector<float>.Count == 8)
        {
            TransformEightLane(samples, coefficients);
            return;
        }
        TransformFourLane(samples, 8, centerInput: false, coefficients);
    }

    private static void TransformFourLane(ReadOnlySpan<float> samples, int stride, bool centerInput,
        Span<float> coefficients)
    {
        Span<Vector<float>> intermediate = stackalloc Vector<float>[16];
        Span<Vector<float>> lanes = stackalloc Vector<float>[8];
        var scale = new Vector<float>(0.125f);
        var center = new Vector<float>(128f);
        // Two four-column panels consume all source pixels before output.
        for (int panel = 0; panel < 2; panel++)
        {
            if (centerInput)
                for (int row = 0; row < 8; row++)
                    lanes[row] = new Vector<float>(samples.Slice(row * stride + panel * 4, 4)) - center;
            else
                for (int row = 0; row < 8; row++)
                    lanes[row] = new Vector<float>(samples.Slice(row * stride + panel * 4, 4));
            TransformRecursiveVector(lanes);
            for (int v = 0; v < 8; v++)
                intermediate[v * 2 + panel] = lanes[v] * scale;
        }
        // Four 4x4 quadrant transposes replace every scalar lane gather and
        // extraction. The .NET 10 and .NET 11 paths share this dataflow.
        for (int v = 0; v < 8; v += 4)
        {
            for (int row = 0; row < 4; row++)
            {
                lanes[row] = intermediate[(v + row) * 2];
                lanes[row + 4] = intermediate[(v + row) * 2 + 1];
            }
            TransposeFourRows(lanes, 0);
            TransposeFourRows(lanes, 4);
            TransformRecursiveVector(lanes);
            TransposeFourRows(lanes, 0);
            TransposeFourRows(lanes, 4);
            for (int row = 0; row < 4; row++)
            {
                lanes[row].CopyTo(coefficients.Slice((v + row) * 8, 4));
                lanes[row + 4].CopyTo(coefficients.Slice((v + row) * 8 + 4, 4));
            }
        }
    }

    private static void TransformEightLane(ReadOnlySpan<float> samples, Span<float> coefficients)
    {
        Span<Vector<float>> rows = stackalloc Vector<float>[8];
        for (int row = 0; row < 8; row++)
            rows[row] = new Vector<float>(samples.Slice(row * 8, 8));
        TransformEightLaneRows(rows, coefficients);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TransformEightLaneRows(Span<Vector<float>> rows, Span<float> coefficients)
    {
        TransformRecursiveVector(rows);
        for (int row = 0; row < 8; row++)
            rows[row] *= new Vector<float>(0.125f);
        TransposePortable(System.Runtime.InteropServices.MemoryMarshal.Cast<Vector<float>, Vector256<float>>(rows));
        TransformRecursiveVector(rows);
        TransposePortable(System.Runtime.InteropServices.MemoryMarshal.Cast<Vector<float>, Vector256<float>>(rows));
        for (int row = 0; row < 8; row++)
            rows[row].CopyTo(coefficients.Slice(row * 8, 8));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static void TransposePortable(Span<Vector256<float>> rows)
    {
        if (rows.Length < 8)
            throw new ArgumentException("An 8 by 8 transpose requires eight vector rows.", nameof(rows));
        Vector128<float> a0 = rows[0].GetLower(), a1 = rows[1].GetLower(), a2 = rows[2].GetLower(), a3 = rows[3].GetLower();
        Vector128<float> b0 = rows[0].GetUpper(), b1 = rows[1].GetUpper(), b2 = rows[2].GetUpper(), b3 = rows[3].GetUpper();
        Vector128<float> c0 = rows[4].GetLower(), c1 = rows[5].GetLower(), c2 = rows[6].GetLower(), c3 = rows[7].GetLower();
        Vector128<float> d0 = rows[4].GetUpper(), d1 = rows[5].GetUpper(), d2 = rows[6].GetUpper(), d3 = rows[7].GetUpper();
        Transpose4x4(ref a0, ref a1, ref a2, ref a3);
        Transpose4x4(ref b0, ref b1, ref b2, ref b3);
        Transpose4x4(ref c0, ref c1, ref c2, ref c3);
        Transpose4x4(ref d0, ref d1, ref d2, ref d3);
        Vector256<float> ac0 = Vector256.Create(a0, a1), ac1 = Vector256.Create(a2, a3);
        Vector256<float> ac2 = Vector256.Create(c0, c1), ac3 = Vector256.Create(c2, c3);
        Vector256<float> bd0 = Vector256.Create(b0, b1), bd1 = Vector256.Create(b2, b3);
        Vector256<float> bd2 = Vector256.Create(d0, d1), bd3 = Vector256.Create(d2, d3);
        rows[0] = ConcatLowerLower(ac0, ac2);
        rows[1] = ConcatUpperUpper(ac0, ac2);
        rows[2] = ConcatLowerLower(ac1, ac3);
        rows[3] = ConcatUpperUpper(ac1, ac3);
        rows[4] = ConcatLowerLower(bd0, bd2);
        rows[5] = ConcatUpperUpper(bd0, bd2);
        rows[6] = ConcatLowerLower(bd1, bd3);
        rows[7] = ConcatUpperUpper(bd1, bd3);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void TransposeFourRows(Span<Vector<float>> rows, int start)
    {
        Vector128<float> r0 = rows[start].AsVector128(), r1 = rows[start + 1].AsVector128();
        Vector128<float> r2 = rows[start + 2].AsVector128(), r3 = rows[start + 3].AsVector128();
        Transpose4x4(ref r0, ref r1, ref r2, ref r3);
        rows[start] = r0.AsVector();
        rows[start + 1] = r1.AsVector();
        rows[start + 2] = r2.AsVector();
        rows[start + 3] = r3.AsVector();
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
        => Vector128.Shuffle(Vector128.Create(left.GetLower(), right.GetLower()), Vector128.Create(0, 2, 1, 3));
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<float> ZipUpper(Vector128<float> left, Vector128<float> right)
#if NET11_0_OR_GREATER && DOTJPEGLI_NET11_SIMD
        => Vector128.ZipUpper(left, right);
#else
        => Vector128.Shuffle(Vector128.Create(left.GetUpper(), right.GetUpper()), Vector128.Create(0, 2, 1, 3));
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<double> ZipLower(Vector128<double> left, Vector128<double> right)
#if NET11_0_OR_GREATER && DOTJPEGLI_NET11_SIMD
        => Vector128.ZipLower(left, right);
#else
        => Vector128.Create(left.GetLower(), right.GetLower());
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<double> ZipUpper(Vector128<double> left, Vector128<double> right)
#if NET11_0_OR_GREATER && DOTJPEGLI_NET11_SIMD
        => Vector128.ZipUpper(left, right);
#else
        => Vector128.Create(left.GetUpper(), right.GetUpper());
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<float> ConcatLowerLower(Vector256<float> left, Vector256<float> right)
#if NET11_0_OR_GREATER && DOTJPEGLI_NET11_SIMD
        => Vector256.ConcatLowerLower(left, right);
#else
        => Vector256.Create(left.GetLower(), right.GetLower());
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<float> ConcatUpperUpper(Vector256<float> left, Vector256<float> right)
#if NET11_0_OR_GREATER && DOTJPEGLI_NET11_SIMD
        => Vector256.ConcatUpperUpper(left, right);
#else
        => Vector256.Create(left.GetUpper(), right.GetUpper());
#endif

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
