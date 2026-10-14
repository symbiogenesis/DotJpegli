// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using System.Globalization;

namespace DotJpegli.Internal;

// Custom branches of lib/cms/jpegli_cms_internal.h and lib/base/matrix_ops.h,
// google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20.
internal static class CustomIccColorEncoding
{
    internal static string WhitePointName(IccChromaticity whitePoint) =>
        Coordinate(whitePoint.X) + ";" + Coordinate(whitePoint.Y);

    internal static string PrimariesName(IccRgbPrimaries primaries) =>
        WhitePointName(primaries.Red) + ";" + WhitePointName(primaries.Green) + ";" + WhitePointName(primaries.Blue);

    // Upstream's ToString uses snprintf("%g") on the original double, even
    // though matrix operations subsequently convert coordinates to float.
    private static string Coordinate(double value) => value.ToString("G6", CultureInfo.InvariantCulture).ToLowerInvariant();

    internal static void ValidateWhitePoint(IccChromaticity whitePoint)
    {
        if (!double.IsFinite(whitePoint.X) || !double.IsFinite(whitePoint.Y) ||
            whitePoint.X < 0 || whitePoint.X > 1 || whitePoint.Y <= 0 || whitePoint.Y > 1)
            throw new ArgumentOutOfRangeException(nameof(whitePoint),
                "White point coordinates must be finite, with x between 0 and 1 and y greater than 0 and at most 1.");
    }

    internal static float[] WhiteXyz(IccChromaticity whitePoint)
    {
        ValidateWhitePoint(whitePoint);
        if (whitePoint.Y < 1e-12)
            throw new ArgumentOutOfRangeException(nameof(whitePoint), "The white point y coordinate is too small.");
        // CIEXYZFromWhiteCIExy rounds the reciprocal before the double products.
        float factor = (float)(1.0 / whitePoint.Y);
        float[] xyz = [(float)(whitePoint.X * factor), 1f,
            (float)((1.0 - whitePoint.X - whitePoint.Y) * factor)];
        ValidateFixed(xyz, nameof(whitePoint));
        return xyz;
    }

    internal static float[] Colorants(IccRgbPrimaries primaries, IccChromaticity whitePoint,
        out float[] chromaticAdaptation)
    {
        ValidatePrimary(primaries.Red);
        ValidatePrimary(primaries.Green);
        ValidatePrimary(primaries.Blue);
        ValidateWhitePoint(whitePoint);
        float rx = (float)primaries.Red.X, ry = (float)primaries.Red.Y;
        float gx = (float)primaries.Green.X, gy = (float)primaries.Green.Y;
        float bx = (float)primaries.Blue.X, by = (float)primaries.Blue.Y;
        Span<float> matrix = stackalloc float[9]
        {
            rx, gx, bx,
            ry, gy, by,
            1f - rx - ry, 1f - gx - gy, 1f - bx - by
        };
        Span<float> inverse = stackalloc float[9];
        Invert(matrix, inverse);
        Span<float> white = stackalloc float[3];
        SourceWhite(whitePoint, white);
        Span<float> scales = stackalloc float[3];
        MultiplyVector(inverse, white, scales);
        Span<float> diagonal = stackalloc float[9];
        diagonal.Clear();
        diagonal[0] = scales[0];
        diagonal[4] = scales[1];
        diagonal[8] = scales[2];
        Span<float> toXyz = stackalloc float[9];
        Multiply(matrix, diagonal, toXyz);
        chromaticAdaptation = ChromaticAdaptation(whitePoint, white);
        Span<float> adapted = stackalloc float[9];
        Multiply(chromaticAdaptation, toXyz, adapted);
        ValidateFixed(adapted, nameof(primaries));
        // ICC XYZ tags contain columns, while matrix operations use rows.
        return [adapted[0], adapted[3], adapted[6], adapted[1], adapted[4], adapted[7],
            adapted[2], adapted[5], adapted[8]];
    }

    private static void ValidatePrimary(IccChromaticity primary)
    {
        // ACES P0 and other imaginary primaries can have negative coordinates;
        // upstream deliberately imposes no physical chromaticity triangle.
        if (!double.IsFinite(primary.X) || !double.IsFinite(primary.Y) ||
            !float.IsFinite((float)primary.X) || !float.IsFinite((float)primary.Y))
            throw new ArgumentOutOfRangeException("primaries", "Primary coordinates must be finite and representable as floats.");
    }

    private static void SourceWhite(IccChromaticity whitePoint, Span<float> white)
    {
        float wx = (float)whitePoint.X, wy = (float)whitePoint.Y;
        white[0] = wx / wy;
        white[1] = 1f;
        white[2] = (1f - wx - wy) / wy;
        if (!float.IsFinite(white[0]) || !float.IsFinite(white[2]))
            throw new ArgumentOutOfRangeException(nameof(whitePoint), "The white point XYZ values are not representable.");
    }

    private static float[] ChromaticAdaptation(IccChromaticity whitePoint, ReadOnlySpan<float> white)
    {
        ReadOnlySpan<float> bradford = [0.8951f, 0.2664f, -0.1614f,
            -0.7502f, 1.7135f, 0.0367f, 0.0389f, -0.0685f, 1.0296f];
        ReadOnlySpan<float> inverse = [0.9869929f, -0.1470543f, 0.1599627f,
            0.4323053f, 0.5183603f, 0.0492912f, -0.0085287f, 0.0400428f, 0.9684867f];
        ReadOnlySpan<float> d50 = [0.96422f, 1f, 0.82521f];
        Span<float> source = stackalloc float[3];
        Span<float> destination = stackalloc float[3];
        MultiplyVector(bradford, white, source);
        MultiplyVector(bradford, d50, destination);
        Span<float> diagonal = stackalloc float[9];
        diagonal.Clear();
        for (int index = 0; index < 3; index++)
        {
            float scale = destination[index] / source[index];
            if (source[index] == 0 || !float.IsFinite(scale))
                throw new ArgumentOutOfRangeException(nameof(whitePoint), "The white point cannot be adapted to D50.");
            diagonal[index * 4] = scale;
        }
        Span<float> scaled = stackalloc float[9];
        Multiply(diagonal, bradford, scaled);
        float[] adapted = new float[9];
        Multiply(inverse, scaled, adapted);
        ValidateFixed(adapted, nameof(whitePoint));
        return adapted;
    }

    private static void Invert(ReadOnlySpan<float> matrix, Span<float> inverse)
    {
        Span<double> cofactors = stackalloc double[9]
        {
            (double)matrix[4] * matrix[8] - (double)matrix[5] * matrix[7],
            (double)matrix[2] * matrix[7] - (double)matrix[1] * matrix[8],
            (double)matrix[1] * matrix[5] - (double)matrix[2] * matrix[4],
            (double)matrix[5] * matrix[6] - (double)matrix[3] * matrix[8],
            (double)matrix[0] * matrix[8] - (double)matrix[2] * matrix[6],
            (double)matrix[2] * matrix[3] - (double)matrix[0] * matrix[5],
            (double)matrix[3] * matrix[7] - (double)matrix[4] * matrix[6],
            (double)matrix[1] * matrix[6] - (double)matrix[0] * matrix[7],
            (double)matrix[0] * matrix[4] - (double)matrix[1] * matrix[3]
        };
        double determinant = matrix[0] * cofactors[0] + matrix[1] * cofactors[3] + matrix[2] * cofactors[6];
        if (Math.Abs(determinant) < 1e-10)
            throw new ArgumentException("The primary matrix determinant is too close to zero.", "primaries");
        double reciprocal = 1.0 / determinant;
        for (int index = 0; index < 9; index++)
            inverse[index] = (float)(cofactors[index] * reciprocal);
    }

    private static void Multiply(ReadOnlySpan<float> left, ReadOnlySpan<float> right, Span<float> result)
    {
        for (int column = 0; column < 3; column++)
            for (int row = 0; row < 3; row++)
                result[row * 3 + column] = (float)(left[row * 3] * (double)right[column] +
                    left[row * 3 + 1] * (double)right[3 + column] + left[row * 3 + 2] * (double)right[6 + column]);
    }

    private static void MultiplyVector(ReadOnlySpan<float> matrix, ReadOnlySpan<float> vector, Span<float> result)
    {
        for (int row = 0; row < 3; row++)
        {
            double value = 0;
            for (int column = 0; column < 3; column++)
                value += (double)matrix[row * 3 + column] * vector[column];
            result[row] = (float)value;
        }
    }

    private static void ValidateFixed(ReadOnlySpan<float> values, string parameter)
    {
        // The pinned serializer uses this conservative s15Fixed16 bound to
        // keep float multiplication and rounding below signed-int overflow.
        foreach (float value in values)
            if (!float.IsFinite(value) || value < -32767.995f || value > 32767.995f)
                throw new ArgumentOutOfRangeException(parameter, "The color encoding exceeds the ICC s15Fixed16 range.");
    }
}
