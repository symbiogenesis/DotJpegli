// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using System.Globalization;

namespace DotJpegli.Internal;

internal readonly record struct GammaTransferFunction(string Description, float Exponent)
{
    internal static GammaTransferFunction Create(double gamma)
    {
        if (!double.IsFinite(gamma) || gamma < 1.0 || gamma > 8192.0)
            throw new ArgumentOutOfRangeException(nameof(gamma), "Gamma must be finite and between 1 and 8192.");

        // The public value is the decoding exponent. The pinned ICC builder's
        // JpegliColorEncoding.gamma is the reciprocal encoding exponent, which
        // it formats with snprintf("%g") and inverts into a float curve parameter.
        // Use the direct GAMMA branch without SetGamma's snapping or 1e7 rounding.
        double encodingGamma = 1.0 / gamma;
        return new("g" + encodingGamma.ToString("G6", CultureInfo.InvariantCulture),
            (float)(1.0 / encodingGamma));
    }
}
