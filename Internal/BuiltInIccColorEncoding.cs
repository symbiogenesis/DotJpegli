// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

namespace DotJpegli.Internal;

// Named color encodings from lib/cms/jpegli_cms_internal.h and
// color_encoding_cms.h at 031a0077f5799a6041004267fc12b956c1f52a20.
internal static class BuiltInIccColorEncoding
{
    internal static string WhitePointName(IccWhitePoint whitePoint) => whitePoint switch
    {
        IccWhitePoint.D65 => "D65",
        IccWhitePoint.EqualEnergy => "EER",
        IccWhitePoint.Dci => "DCI",
        _ => throw new ArgumentOutOfRangeException(nameof(whitePoint))
    };

    internal static (double X, double Y) WhitePoint(IccWhitePoint whitePoint) => whitePoint switch
    {
        IccWhitePoint.D65 => (0.3127, 0.3290),
        IccWhitePoint.EqualEnergy => (1.0 / 3, 1.0 / 3),
        IccWhitePoint.Dci => (0.314, 0.351),
        _ => throw new ArgumentOutOfRangeException(nameof(whitePoint))
    };

    internal static string PrimariesName(IccPrimaries primaries) => primaries switch
    {
        IccPrimaries.Srgb => "SRG",
        IccPrimaries.DisplayP3 => "DCI",
        IccPrimaries.Rec2020 => "202",
        _ => throw new ArgumentOutOfRangeException(nameof(primaries))
    };

    internal static string TransferFunctionName(IccTransferFunction transferFunction) => transferFunction switch
    {
        IccTransferFunction.Rec709 => "709",
        IccTransferFunction.Linear => "Lin",
        IccTransferFunction.Srgb => "SRG",
        IccTransferFunction.Pq => "PeQ",
        IccTransferFunction.Dci => "DCI",
        IccTransferFunction.Hlg => "HLG",
        _ => throw new ArgumentOutOfRangeException(nameof(transferFunction))
    };

    internal static byte CicpPrimaries(IccPrimaries primaries, IccWhitePoint whitePoint) =>
        whitePoint == IccWhitePoint.D65
            ? primaries == IccPrimaries.DisplayP3 ? (byte)12 : (byte)primaries
            : whitePoint == IccWhitePoint.Dci && primaries == IccPrimaries.DisplayP3 ? (byte)11 : (byte)0;

    // Columns of the pinned C++ builder's PrimariesToXYZD50. Precomputing the
    // canonical float results keeps every existing factory's serialized bytes
    // and avoids platform-dependent matrix inversion for these fixed encodings.
    internal static ReadOnlySpan<float> Colorants(IccPrimaries primaries, IccWhitePoint whitePoint) =>
        (primaries, whitePoint) switch
        {
            (IccPrimaries.Srgb, IccWhitePoint.D65) => RgbIccProfile.SrgbColorants,
            (IccPrimaries.DisplayP3, IccWhitePoint.D65) => RgbIccProfile.DisplayP3Colorants,
            (IccPrimaries.Rec2020, IccWhitePoint.D65) => RgbIccProfile.Rec2020Colorants,
            (IccPrimaries.DisplayP3, IccWhitePoint.Dci) => RgbIccProfile.DciP3Colorants,
            (IccPrimaries.Srgb, IccWhitePoint.EqualEnergy) =>
                [0.494048714638f, 0.255870878696f, 0.0188341848552f,
                 0.332185864449f, 0.686319589615f, 0.0991855859756f,
                 0.137985363603f, 0.0578096844256f, 0.707190155983f],
            (IccPrimaries.DisplayP3, IccWhitePoint.EqualEnergy) =>
                [0.561759591103f, 0.264826059341f, -0.000615746190306f,
                 0.249596685171f, 0.67112660408f, 0.0423707850277f,
                 0.152863636613f, 0.0640475228429f, 0.78345489502f],
            (IccPrimaries.Rec2020, IccWhitePoint.EqualEnergy) =>
                [0.702708184719f, 0.2894089818f, -0.00132157863118f,
                 0.135801881552f, 0.665047049522f, 0.0299687255174f,
                 0.125709921122f, 0.0455441139638f, 0.796562731266f],
            (IccPrimaries.Srgb, IccWhitePoint.Dci) =>
                [0.399037897587f, 0.201681733131f, 0.0142087852582f,
                 0.427097201347f, 0.741921722889f, 0.110215559602f,
                 0.138084888458f, 0.0563966669142f, 0.700785577297f],
            (IccPrimaries.Rec2020, IccWhitePoint.Dci) =>
                [0.651255309582f, 0.270788431168f, -0.00125795032363f,
                 0.18689134717f, 0.684432983398f, 0.0281420256943f,
                 0.126073271036f, 0.0447785966098f, 0.798325836658f],
            _ => throw new ArgumentOutOfRangeException(nameof(primaries))
        };

    // Unadapted PrimariesToXYZ Y rows for the four CICP-representable HDR
    // combinations. Tone mapping uses source luminance rather than D50 Y.
    internal static ReadOnlySpan<float> Luminances(IccPrimaries primaries, IccWhitePoint whitePoint) =>
        (primaries, whitePoint) switch
        {
            (IccPrimaries.Srgb, IccWhitePoint.D65) =>
                [0.212642297149f, 0.715167701244f, 0.0721900463104f],
            (IccPrimaries.DisplayP3, IccWhitePoint.D65) =>
                [0.228974580765f, 0.691738545895f, 0.0792869031429f],
            (IccPrimaries.Rec2020, IccWhitePoint.D65) =>
                [0.262700200081f, 0.677998006344f, 0.0593017116189f],
            (IccPrimaries.DisplayP3, IccWhitePoint.Dci) =>
                [0.209491685033f, 0.721595227718f, 0.0689130574465f],
            _ => throw new ArgumentOutOfRangeException(nameof(primaries))
        };
}
