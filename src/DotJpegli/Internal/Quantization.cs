// Copyright (c) the JPEG XL Project Authors.
//
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.
// Ported directly from google/jpegli revision
// 031a0077f5799a6041004267fc12b956c1f52a20:
// lib/jpegli/quant.cc (YCbCr and standard tables, DistanceToScale,
// DistanceToLinearQuality, SetQuantMatrices,
// InitQuantizer), lib/jpegli/encode.cc (jpegli_quality_to_distance), and
// lib/jpegli/dct-inl.h (QuantizeBlock and ComputeCoefficientBlock).
// https://github.com/google/jpegli/tree/031a0077f5799a6041004267fc12b956c1f52a20

using System.Numerics;
using System.Runtime.InteropServices;

namespace DotJpegli.Internal;

internal static class Quantization
{
    private const int BlockSize = 64;
    private const float GlobalScale = 1.73966010f;
    private const float GlobalScale420 = 1.22f;
    private const float NonlinearThreshold = 1.5f;

    // ITU T.81 Annex K matrices, as used by upstream's explicit standard-table mode.
    private static ReadOnlySpan<byte> StandardMatrices =>
    [
        16, 11, 10, 16, 24, 40, 51, 61,
        12, 12, 14, 19, 26, 58, 60, 55,
        14, 13, 16, 24, 40, 57, 69, 56,
        14, 17, 22, 29, 51, 87, 80, 62,
        18, 22, 37, 56, 68, 109, 103, 77,
        24, 35, 55, 64, 81, 104, 113, 92,
        49, 64, 78, 87, 103, 121, 120, 101,
        72, 92, 95, 98, 112, 100, 103, 99,
        17, 18, 24, 47, 99, 99, 99, 99,
        18, 21, 26, 66, 99, 99, 99, 99,
        24, 26, 56, 99, 99, 99, 99, 99,
        47, 66, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
        99, 99, 99, 99, 99, 99, 99, 99,
    ];

    // Natural row-major frequency order, as used by SetQuantMatrices.
    private static ReadOnlySpan<float> FrequencyExponents =>
    [
        1.00f, 0.51f, 0.67f, 0.74f, 1.00f, 1.00f, 1.00f, 1.00f,
        0.51f, 0.66f, 0.69f, 0.87f, 1.00f, 1.00f, 1.00f, 1.00f,
        0.67f, 0.69f, 0.84f, 0.83f, 0.96f, 1.00f, 1.00f, 1.00f,
        0.74f, 0.87f, 0.83f, 1.00f, 1.00f, 0.91f, 0.91f, 1.00f,
        1.00f, 1.00f, 0.96f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f,
        1.00f, 1.00f, 1.00f, 0.91f, 1.00f, 1.00f, 1.00f, 1.00f,
        1.00f, 1.00f, 1.00f, 0.91f, 1.00f, 1.00f, 1.00f, 1.00f,
        1.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f, 1.00f,
    ];

    private static ReadOnlySpan<float> ChromaRescale420 =>
    [
        0.4093f, 0.3209f, 0.3477f, 0.3333f, 0.3144f, 0.2823f, 0.3214f, 0.3354f,
        0.3209f, 0.3111f, 0.3489f, 0.2801f, 0.3059f, 0.3119f, 0.4135f, 0.3445f,
        0.3477f, 0.3489f, 0.3586f, 0.3257f, 0.2727f, 0.3754f, 0.3369f, 0.3484f,
        0.3333f, 0.2801f, 0.3257f, 0.3020f, 0.3515f, 0.3410f, 0.3971f, 0.3839f,
        0.3144f, 0.3059f, 0.2727f, 0.3515f, 0.3105f, 0.3397f, 0.2716f, 0.3836f,
        0.2823f, 0.3119f, 0.3754f, 0.3410f, 0.3397f, 0.3212f, 0.3203f, 0.0726f,
        0.3214f, 0.4135f, 0.3369f, 0.3971f, 0.2716f, 0.3203f, 0.0798f, 0.0553f,
        0.3354f, 0.3445f, 0.3484f, 0.3839f, 0.3836f, 0.0726f, 0.0553f, 0.3368f,
    ];

    // Three independent perceptual matrices: 64 Y, 64 Cb, then 64 Cr values.
    // This corresponds to SetQuantMatrices(add_two_chroma_tables = true).
    private static ReadOnlySpan<float> BaseMatrices =>
    [
        1.2397409345866273f, 1.7227115097630963f, 2.9212167156636855f, 2.812737435286529f, 3.339819711906184f, 3.463603762596166f, 3.840915217993518f, 3.86956f,
        1.7227115097630963f, 2.0928894413636874f, 2.8456760904429297f, 2.704506820909662f, 3.4407673520905337f, 3.166232352090534f, 4.025208741558432f, 4.035324490952577f,
        2.9212167156636855f, 2.8456760904429297f, 2.9587403520905338f, 3.3862948970669273f, 3.619523781336757f, 3.9046279999999998f, 3.757835838431854f, 4.237447515714274f,
        2.812737435286529f, 2.704506820909662f, 3.3862948970669273f, 3.380058821812233f, 4.1679867415584315f, 4.805510627261856f, 4.784259f, 4.605934f,
        3.339819711906184f, 3.4407673520905337f, 3.619523781336757f, 4.1679867415584315f, 4.579851258441568f, 4.923237f, 5.574107f, 5.48533336146308f,
        3.463603762596166f, 3.166232352090534f, 3.9046279999999998f, 4.805510627261856f, 4.923237f, 5.43936f, 5.093895741558431f, 6.0872254423617225f,
        3.840915217993518f, 4.025208741558432f, 3.757835838431854f, 4.784259f, 5.574107f, 5.093895741558431f, 5.438461f, 5.4037359493250845f,
        3.86956f, 4.035324490952577f, 4.237447515714274f, 4.605934f, 5.48533336146308f, 6.0872254423617225f, 5.4037359493250845f, 4.37787101190424f,
        2.8236197786377537f, 6.495639358561486f, 9.310489207538302f, 10.64747864717083f, 11.07419143098738f, 17.146390223910462f, 18.463982229408998f, 29.087001644203088f,
        6.495639358561486f, 8.890103846667353f, 8.976895794294748f, 13.666270550318826f, 16.547071905624193f, 16.63871382827686f, 26.778396930893695f, 21.33034294694781f,
        9.310489207538302f, 8.976895794294748f, 11.08737706005991f, 18.20548239870446f, 19.752481654011646f, 23.985660533114896f, 102.6457378402362f, 24.450989f,
        10.64747864717083f, 13.666270550318826f, 18.20548239870446f, 18.628012327860365f, 16.042509519487183f, 25.04918273242625f, 25.017140189353015f, 35.79788782635831f,
        11.07419143098738f, 16.547071905624193f, 19.752481654011646f, 16.042509519487183f, 19.373482748612577f, 14.677529999999999f, 19.94695960400931f, 51.094112f,
        17.146390223910462f, 16.63871382827686f, 23.985660533114896f, 25.04918273242625f, 14.677529999999999f, 31.320412426835304f, 46.357234000000005f, 67.48111451705412f,
        18.463982229408998f, 26.778396930893695f, 102.6457378402362f, 25.017140189353015f, 19.94695960400931f, 46.357234000000005f, 61.315764694388044f, 88.34665293823721f,
        29.087001644203088f, 21.33034294694781f, 24.450989f, 35.79788782635831f, 51.094112f, 67.48111451705412f, 88.34665293823721f, 112.16099098350989f,
        2.9217254961255255f, 4.497681013199305f, 7.356344520940414f, 6.583891506504051f, 8.535608740100237f, 8.799434353234647f, 9.188341534163023f, 9.482700481227672f,
        4.497681013199305f, 6.309548851989123f, 7.024608962670982f, 7.156445324163424f, 8.049059218663244f, 7.0124290657218555f, 6.711923184393611f, 8.380307846134853f,
        7.356344520940414f, 7.024608962670982f, 6.892101177327445f, 6.882819916277163f, 8.782226090078568f, 6.8774750000000004f, 7.8858175969577955f, 8.67909f,
        6.583891506504051f, 7.156445324163424f, 6.882819916277163f, 7.003072944847055f, 7.7223464701024875f, 7.955425720217421f, 7.4734110000000005f, 8.362933242943903f,
        8.535608740100237f, 8.049059218663244f, 8.782226090078568f, 7.7223464701024875f, 6.778005927001542f, 9.484922741558432f, 9.043702663686046f, 8.053178199770173f,
        8.799434353234647f, 7.0124290657218555f, 6.8774750000000004f, 7.955425720217421f, 9.484922741558432f, 8.607606527385098f, 9.922697394370815f, 64.25135180237939f,
        9.188341534163023f, 6.711923184393611f, 7.8858175969577955f, 7.4734110000000005f, 9.043702663686046f, 9.922697394370815f, 63.184936549738225f, 83.35294340273799f,
        9.482700481227672f, 8.380307846134853f, 8.67909f, 8.362933242943903f, 8.053178199770173f, 64.25135180237939f, 83.35294340273799f, 114.89202448569779f,
    ];

    // InitQuantizer uses natural frequency order: 64 Y, then Cb and Cr.
    private static ReadOnlySpan<float> ZeroBiasMultipliersLowQuality =>
    [
        // c = 0
        0.0000f, 0.0568f, 0.3880f, 0.6190f, 0.6190f, 0.4490f, 0.4490f, 0.6187f,
        0.0568f, 0.5829f, 0.6189f, 0.6190f, 0.6190f, 0.7190f, 0.6190f, 0.6189f,
        0.3880f, 0.6189f, 0.6190f, 0.6190f, 0.6190f, 0.6190f, 0.6187f, 0.6100f,
        0.6190f, 0.6190f, 0.6190f, 0.6190f, 0.5890f, 0.3839f, 0.7160f, 0.6190f,
        0.6190f, 0.6190f, 0.6190f, 0.5890f, 0.6190f, 0.3880f, 0.5860f, 0.4790f,
        0.4490f, 0.7190f, 0.6190f, 0.3839f, 0.3880f, 0.6190f, 0.6190f, 0.6190f,
        0.4490f, 0.6190f, 0.6187f, 0.7160f, 0.5860f, 0.6190f, 0.6204f, 0.6190f,
        0.6187f, 0.6189f, 0.6100f, 0.6190f, 0.4790f, 0.6190f, 0.6190f, 0.3480f,
        // c = 1
        0.0000f, 1.1640f, 0.9373f, 1.1319f, 0.8016f, 0.9136f, 1.1530f, 0.9430f,
        1.1640f, 0.9188f, 0.9160f, 1.1980f, 1.1830f, 0.9758f, 0.9430f, 0.9430f,
        0.9373f, 0.9160f, 0.8430f, 1.1720f, 0.7083f, 0.9430f, 0.9430f, 0.9430f,
        1.1319f, 1.1980f, 1.1720f, 1.1490f, 0.8547f, 0.9430f, 0.9430f, 0.9430f,
        0.8016f, 1.1830f, 0.7083f, 0.8547f, 0.9430f, 0.9430f, 0.9430f, 0.9430f,
        0.9136f, 0.9758f, 0.9430f, 0.9430f, 0.9430f, 0.9430f, 0.9430f, 0.9430f,
        1.1530f, 0.9430f, 0.9430f, 0.9430f, 0.9430f, 0.9430f, 0.9430f, 0.9480f,
        0.9430f, 0.9430f, 0.9430f, 0.9430f, 0.9430f, 0.9430f, 0.9480f, 0.9430f,
        // c = 2
        0.0000f, 1.3190f, 0.4308f, 0.4460f, 0.0661f, 0.0660f, 0.2660f, 0.2960f,
        1.3190f, 0.3280f, 0.3093f, 0.0750f, 0.0505f, 0.1594f, 0.3060f, 0.2113f,
        0.4308f, 0.3093f, 0.3060f, 0.1182f, 0.0500f, 0.3060f, 0.3915f, 0.2426f,
        0.4460f, 0.0750f, 0.1182f, 0.0512f, 0.0500f, 0.2130f, 0.3930f, 0.1590f,
        0.0661f, 0.0505f, 0.0500f, 0.0500f, 0.3055f, 0.3360f, 0.5148f, 0.5403f,
        0.0660f, 0.1594f, 0.3060f, 0.2130f, 0.3360f, 0.5060f, 0.5874f, 0.3060f,
        0.2660f, 0.3060f, 0.3915f, 0.3930f, 0.5148f, 0.5874f, 0.3060f, 0.3060f,
        0.2960f, 0.2113f, 0.2426f, 0.1590f, 0.5403f, 0.3060f, 0.3060f, 0.3060f,
    ];

    private static ReadOnlySpan<float> ZeroBiasMultipliersHighQuality =>
    [
        // c = 0
        0.0000f, 0.0044f, 0.2521f, 0.6547f, 0.8161f, 0.6130f, 0.8841f, 0.8155f,
        0.0044f, 0.6831f, 0.6553f, 0.6295f, 0.7848f, 0.7843f, 0.8474f, 0.7836f,
        0.2521f, 0.6553f, 0.7834f, 0.7829f, 0.8161f, 0.8072f, 0.7743f, 0.9242f,
        0.6547f, 0.6295f, 0.7829f, 0.8654f, 0.7829f, 0.6986f, 0.7818f, 0.7726f,
        0.8161f, 0.7848f, 0.8161f, 0.7829f, 0.7471f, 0.7827f, 0.7843f, 0.7653f,
        0.6130f, 0.7843f, 0.8072f, 0.6986f, 0.7827f, 0.7848f, 0.9508f, 0.7653f,
        0.8841f, 0.8474f, 0.7743f, 0.7818f, 0.7843f, 0.9508f, 0.7839f, 0.8437f,
        0.8155f, 0.7836f, 0.9242f, 0.7726f, 0.7653f, 0.7653f, 0.8437f, 0.7819f,
        // c = 1
        0.0000f, 1.0816f, 1.0556f, 1.2876f, 1.1554f, 1.1567f, 1.8851f, 0.5488f,
        1.0816f, 1.1537f, 1.1850f, 1.0712f, 1.1671f, 2.0719f, 1.0544f, 1.4764f,
        1.0556f, 1.1850f, 1.2870f, 1.1981f, 1.8181f, 1.2618f, 1.0564f, 1.1191f,
        1.2876f, 1.0712f, 1.1981f, 1.4753f, 2.0609f, 1.0564f, 1.2645f, 1.0564f,
        1.1554f, 1.1671f, 1.8181f, 2.0609f, 0.7324f, 1.1163f, 0.8464f, 1.0564f,
        1.1567f, 2.0719f, 1.2618f, 1.0564f, 1.1163f, 1.0040f, 1.0564f, 1.0564f,
        1.8851f, 1.0544f, 1.0564f, 1.2645f, 0.8464f, 1.0564f, 1.0564f, 1.0564f,
        0.5488f, 1.4764f, 1.1191f, 1.0564f, 1.0564f, 1.0564f, 1.0564f, 1.0564f,
        // c = 2
        0.0000f, 0.5392f, 0.6659f, 0.8968f, 0.6829f, 0.6328f, 0.5802f, 0.4836f,
        0.5392f, 0.6746f, 0.6760f, 0.6102f, 0.6015f, 0.6958f, 0.7327f, 0.4897f,
        0.6659f, 0.6760f, 0.6957f, 0.6543f, 0.4396f, 0.6330f, 0.7081f, 0.2583f,
        0.8968f, 0.6102f, 0.6543f, 0.5913f, 0.6457f, 0.5828f, 0.5139f, 0.3565f,
        0.6829f, 0.6015f, 0.4396f, 0.6457f, 0.5633f, 0.4263f, 0.6371f, 0.5949f,
        0.6328f, 0.6958f, 0.6330f, 0.5828f, 0.4263f, 0.2847f, 0.2909f, 0.6629f,
        0.5802f, 0.7327f, 0.7081f, 0.5139f, 0.6371f, 0.2909f, 0.6644f, 0.6644f,
        0.4836f, 0.4897f, 0.2583f, 0.3565f, 0.5949f, 0.6629f, 0.6644f, 0.6644f,
    ];

    /// <summary>Maps JPEGli quality to distance using the upstream polynomial.</summary>
    internal static float QualityToDistance(float quality)
    {
        if (!float.IsFinite(quality) || quality is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(quality), "Quality must be finite and between 0 and 100.");

        // Same polynomial as jpegli_quality_to_distance, extended to fractional
        // quality values. The C++ public function accepts integer quality.
        if (quality >= 100f)
            return 0.01f;
        if (quality >= 30f)
            return 0.1f + (100f - quality) * 0.09f;
        return 53f / 3000f * quality * quality - 23f / 20f * quality + 25f;
    }

    /// <summary>Creates one baseline YCbCr quantization table in natural frequency order.</summary>
    internal static ushort[] CreateTable(float quality, int component,
        int horizontalSampling = 1, int verticalSampling = 1)
        => CreateTableForDistance(QualityToDistance(quality), component, horizontalSampling, verticalSampling);

    /// <summary>Creates an upstream perceptual table directly from a positive finite distance.</summary>
    internal static ushort[] CreateTableForDistance(float distance, int component,
        int horizontalSampling = 1, int verticalSampling = 1, int cicpTransferFunction = 2,
        bool forceBaseline = true)
    {
        ValidateComponent(component);
        ValidateSampling(horizontalSampling, verticalSampling);
        if (!float.IsFinite(distance) || distance <= 0f)
            throw new ArgumentOutOfRangeException(nameof(distance), "Distance must be finite and greater than zero.");

        bool is420 = horizontalSampling == 2 && verticalSampling == 2;
        float globalScale = TransferFunctionGlobalScale(cicpTransferFunction);
        if (is420)
            globalScale *= GlobalScale420;
        ReadOnlySpan<float> matrix = BaseMatrices.Slice(component * BlockSize, BlockSize);
        ushort[] table = new ushort[BlockSize];
        for (int i = 0; i < table.Length; i++)
        {
            float scale = globalScale * DistanceToScale(distance, i);
            if (is420 && component > 0)
                scale *= ChromaRescale420[i];
            // SetQuantMatrices uses std::round (ties away from zero), then
            // clamps to 255 with force_baseline, or 32767 without it.
            float quant = MathF.Round(scale * matrix[i], MidpointRounding.AwayFromZero);
            table[i] = (ushort)Math.Clamp(quant, 1f, forceBaseline ? 255f : 32767f);
        }
        return table;
    }

    /// <summary>Creates the upstream default grayscale perceptual luma table.</summary>
    /// <remarks>
    /// SetQuantMatrices handles JCS_GRAYSCALE with the YCbCr perceptual Y matrix
    /// when use_std_tables is false, as initialized by jpegli_set_defaults.
    /// Grayscale has no chroma subsampling compensation. The Annex K matrix and
    /// DistanceToLinearQuality apply only to the explicit standard-table mode.
    /// </remarks>
    internal static ushort[] CreateGrayscaleTable(float quality) => CreateTable(quality, 0);

    internal static ushort[] CreateGrayscaleTableForDistance(float distance, int cicpTransferFunction = 2,
        bool forceBaseline = true)
        => CreateTableForDistance(distance, 0, cicpTransferFunction: cicpTransferFunction, forceBaseline: forceBaseline);

    /// <summary>Creates the pinned upstream's optional Annex K table.</summary>
    internal static ushort[] CreateStandardTableForDistance(float distance, int component, bool forceBaseline = true)
    {
        ValidateComponent(component);
        if (!float.IsFinite(distance) || distance <= 0f)
            throw new ArgumentOutOfRangeException(nameof(distance), "Distance must be finite and greater than zero.");

        float scale = 0.01f * DistanceToLinearQuality(distance);
        ReadOnlySpan<byte> matrix = StandardMatrices.Slice(component == 0 ? 0 : BlockSize, BlockSize);
        ushort[] table = new ushort[BlockSize];
        // Clamp before the integer conversion, including immediately below 25
        // where DistanceToLinearQuality can exceed upstream's int32 range.
        for (int i = 0; i < table.Length; i++)
            table[i] = (ushort)Math.Clamp(MathF.Round(scale * matrix[i], MidpointRounding.AwayFromZero),
                1f, forceBaseline ? 255f : 32767f);
        return table;
    }

    // Preserve the C++ float operation order, including at piecewise boundaries.
    private static float DistanceToLinearQuality(float distance)
    {
        if (distance <= 0.1f)
            return 1f;
        if (distance <= 4.6f)
            return (200f / 9f) * (distance - 0.1f);
        if (distance <= 6.4f)
            return 5000f / (100f - (distance - 0.1f) / 0.09f);
        if (distance < 25f)
            return 530000f / (3450f - 300f * MathF.Sqrt((848f * distance - 5330f) / 120f));
        return 5000f;
    }

    /// <summary>Creates upstream per-frequency adaptive zero-bias multipliers.</summary>
    internal static float[][] CreateAdaptiveZeroBiasMultipliers(ushort[][] tables,
        int horizontalSampling, int verticalSampling, bool grayscale, bool sharedChromaTable = false,
        int cicpTransferFunction = 2, bool rgb = false, ReadOnlySpan<int> quantizationSelectors = default,
        bool forceBaseline = true)
    {
        ArgumentNullException.ThrowIfNull(tables);
        ValidateSampling(horizontalSampling, verticalSampling);
        if (tables.Length != (grayscale ? 1 : 3))
            throw new ArgumentException("Supply one grayscale table or three color component tables.", nameof(tables));

        // InitQuantizer only performs table-derived distance inference for YCbCr.
        float distance = rgb ? 1f : QuantValuesToDistance(tables, sharedChromaTable, cicpTransferFunction,
            quantizationSelectors, forceBaseline);
        float lowQualityMix = Math.Clamp((distance - 1f) / 2f, 0f, 1f);
        float highQualityMix = 1f - lowQualityMix;
        float[][] multipliers = new float[tables.Length][];
        for (int component = 0; component < multipliers.Length; component++)
        {
            multipliers[component] = new float[BlockSize];
            for (int frequency = 1; frequency < BlockSize; frequency++)
            {
                int index = component * BlockSize + frequency;
                multipliers[component][frequency] = grayscale || rgb ? 0.5f
                    : lowQualityMix * ZeroBiasMultipliersLowQuality[index]
                        + highQualityMix * ZeroBiasMultipliersHighQuality[index];
            }
        }
        return multipliers;
    }

    private static void ValidateSampling(int horizontalSampling, int verticalSampling)
    {
        if (horizontalSampling is < 1 or > 4)
            throw new ArgumentOutOfRangeException(nameof(horizontalSampling));
        if (verticalSampling is < 1 or > 4 || horizontalSampling * verticalSampling > 8)
            throw new ArgumentOutOfRangeException(nameof(verticalSampling),
                "Sampling must use 1..4 horizontal and vertical samples, and at most 10 blocks per color MCU.");
    }

    /// <summary>Infers upstream quantizer distance from the emitted tables.</summary>
    /// <remarks>
    /// QuantValsToDistance uses the YCbCr base matrices and transfer-function global
    /// scale, without the 4:2:0 compensation, including for
    /// tables emitted with 4:2:0 rescaling or standard matrices. Shared chroma
    /// selectors use the base matrix indexed by each component's table selector,
    /// following upstream's quant_tbl_no indexing; their zero-bias multipliers
    /// remain component-specific. Without explicit selectors, sharedChromaTable
    /// selects the perceptual Cb base for both chroma components.
    /// It reconstructs an interval from
    /// rounded values, leaving saturated 1 and the selected maximum endpoints unbounded.
    /// </remarks>
    internal static float QuantValuesToDistance(ushort[][] tables, bool sharedChromaTable = false,
        int cicpTransferFunction = 2, ReadOnlySpan<int> quantizationSelectors = default, bool forceBaseline = true)
    {
        ArgumentNullException.ThrowIfNull(tables);
        if (tables.Length is not (1 or 3))
            throw new ArgumentException("Supply one grayscale table or three YCbCr tables.", nameof(tables));
        if (!quantizationSelectors.IsEmpty)
        {
            if (quantizationSelectors.Length != tables.Length)
                throw new ArgumentException("Supply one quantization selector per component.", nameof(quantizationSelectors));
            foreach (int selector in quantizationSelectors)
                if (selector is < 0 or > 2)
                    throw new ArgumentOutOfRangeException(nameof(quantizationSelectors));
        }
        float globalScale = TransferFunctionGlobalScale(cicpTransferFunction);
        const float distanceLimit = 10000f;
        float distanceMin = 0f;
        float distanceMax = distanceLimit;
        int quantMaximum = forceBaseline ? 255 : 32767;
        for (int component = 0; component < tables.Length; component++)
        {
            ushort[] table = tables[component];
            if (table is null || table.Length != BlockSize)
                throw new ArgumentException("A quantization table requires 64 values.", nameof(tables));
            for (int frequency = 0; frequency < BlockSize; frequency++)
            {
                int quant = table[frequency];
                if (quant < 1 || quant > quantMaximum)
                    throw new ArgumentException($"Quantization values must be between 1 and {quantMaximum}.", nameof(tables));
                int tableIndex = quantizationSelectors.IsEmpty
                    ? sharedChromaTable && component > 0 ? 1 : component
                    : quantizationSelectors[component];
                float inverseQuant = 1f / BaseMatrices[tableIndex * BlockSize + frequency] / globalScale;
                float min = quant > 1 ? ScaleToDistance((quant - 0.5f) * inverseQuant, frequency) : 0f;
                float max = quant < quantMaximum ? ScaleToDistance((quant + 0.5f) * inverseQuant, frequency) : distanceLimit;
                if (min <= distanceMax)
                    distanceMin = MathF.Max(min, distanceMin);
                if (max >= distanceMin)
                    distanceMax = MathF.Min(max, distanceMax);
            }
        }
        if (distanceMin == 0f)
            return distanceMax;
        if (distanceMax == distanceLimit)
            return distanceMin;
        return 0.5f * (distanceMin + distanceMax);
    }

    /// <summary>Quantizes a level-shifted block in standard JPEG DCT units.</summary>
    /// <remarks>
    /// YCbCr uses component-specific AC zero-bias offsets. Adaptive grayscale
    /// and RGB use a 0.5 offset; disabling AQ uses zero for those color spaces. DC uses
    /// ties-away-from-zero rounding; AC uses Highway's ties-to-even rounding.
    /// </remarks>
    internal static void Quantize(ReadOnlySpan<float> coefficients,
        ReadOnlySpan<ushort> table, Span<short> output, int component, float distance,
        bool grayscale = false, bool adaptiveQuantization = false,
        float adaptiveStrength = 0f, ReadOnlySpan<float> adaptiveMultipliers = default,
        int previousDc = 0, bool requireEightBitCoefficients = false, bool rgb = false)
        => QuantizeCore(coefficients, table, output, component, distance, grayscale,
            adaptiveQuantization, adaptiveStrength, adaptiveMultipliers, previousDc,
            requireEightBitCoefficients, rgb, Vector.IsHardwareAccelerated);

    // Explicit paths keep coefficient regressions independent of automatic
    // dispatch, including runtimes where hardware intrinsics are disabled.
    internal static void QuantizeScalar(ReadOnlySpan<float> coefficients,
        ReadOnlySpan<ushort> table, Span<short> output, int component, float distance,
        bool grayscale = false, bool adaptiveQuantization = false,
        float adaptiveStrength = 0f, ReadOnlySpan<float> adaptiveMultipliers = default,
        int previousDc = 0, bool requireEightBitCoefficients = false, bool rgb = false)
        => QuantizeCore(coefficients, table, output, component, distance, grayscale,
            adaptiveQuantization, adaptiveStrength, adaptiveMultipliers, previousDc,
            requireEightBitCoefficients, rgb, useVector: false);

    internal static void QuantizeVector(ReadOnlySpan<float> coefficients,
        ReadOnlySpan<ushort> table, Span<short> output, int component, float distance,
        bool grayscale = false, bool adaptiveQuantization = false,
        float adaptiveStrength = 0f, ReadOnlySpan<float> adaptiveMultipliers = default,
        int previousDc = 0, bool requireEightBitCoefficients = false, bool rgb = false)
        => QuantizeCore(coefficients, table, output, component, distance, grayscale,
            adaptiveQuantization, adaptiveStrength, adaptiveMultipliers, previousDc,
            requireEightBitCoefficients, rgb, useVector: true);

    private static void QuantizeCore(ReadOnlySpan<float> coefficients,
        ReadOnlySpan<ushort> table, Span<short> output, int component, float distance,
        bool grayscale, bool adaptiveQuantization, float adaptiveStrength,
        ReadOnlySpan<float> adaptiveMultipliers, int previousDc,
        bool requireEightBitCoefficients, bool rgb, bool useVector)
    {
        ValidateComponent(component);
        if (grayscale && component != 0)
            throw new ArgumentOutOfRangeException(nameof(component), "Grayscale has only component 0.");
        if (!float.IsFinite(distance) || distance <= 0)
            throw new ArgumentOutOfRangeException(nameof(distance));
        if (coefficients.Length < BlockSize)
            throw new ArgumentException("A DCT block requires 64 coefficients.", nameof(coefficients));
        if (table.Length < BlockSize)
            throw new ArgumentException("A quantization table requires 64 values.", nameof(table));
        if (output.Length < BlockSize)
            throw new ArgumentException("A quantized block requires 64 values.", nameof(output));
        if (adaptiveQuantization)
        {
            if (!float.IsFinite(adaptiveStrength) || adaptiveStrength < 0)
                throw new ArgumentOutOfRangeException(nameof(adaptiveStrength));
            if (adaptiveMultipliers.Length < BlockSize)
                throw new ArgumentException("Adaptive quantization requires 64 multipliers.", nameof(adaptiveMultipliers));
        }

        float acOffset = grayscale || rgb ? (adaptiveQuantization ? 0.5f : 0f) : component switch
        {
            0 => 0.59082f,
            1 => 0.58146f,
            _ => 0.57988f,
        };
        // A vector batch holds two float vectors, matching one ushort table
        // vector. Keep any future width larger than the block scalar.
        if (useVector && Vector<float>.Count < BlockSize &&
            !MemoryMarshal.AsBytes(coefficients).Overlaps(MemoryMarshal.AsBytes(output)) &&
            !MemoryMarshal.AsBytes(table).Overlaps(MemoryMarshal.AsBytes(output)) &&
            (!adaptiveQuantization ||
                !MemoryMarshal.AsBytes(adaptiveMultipliers).Overlaps(MemoryMarshal.AsBytes(output))))
            QuantizeVectorCore(coefficients, table, output, acOffset, adaptiveQuantization,
                adaptiveStrength, adaptiveMultipliers, previousDc, requireEightBitCoefficients);
        else
            QuantizeScalarCore(coefficients, table, output, acOffset, adaptiveQuantization,
                adaptiveStrength, adaptiveMultipliers, previousDc, requireEightBitCoefficients);
    }

    private static void QuantizeScalarCore(ReadOnlySpan<float> coefficients,
        ReadOnlySpan<ushort> table, Span<short> output, float acOffset,
        bool adaptiveQuantization, float adaptiveStrength, ReadOnlySpan<float> adaptiveMultipliers,
        int previousDc, bool requireEightBitCoefficients, int start = 0, int end = BlockSize)
    {
        for (int i = start; i < end; i++)
        {
            if (table[i] is < 1 or > 32767)
                throw new ArgumentException("Quantization values must be between 1 and 32767.", nameof(table));
            if (!float.IsFinite(coefficients[i]))
                throw new ArgumentException("DCT coefficients must be finite.", nameof(coefficients));
            if (adaptiveQuantization && (!float.IsFinite(adaptiveMultipliers[i]) || adaptiveMultipliers[i] < 0))
                throw new ArgumentException("Adaptive multipliers must be finite and nonnegative.", nameof(adaptiveMultipliers));

            // Input is in standard JPEG DCT units and is already level shifted.
            // C++ uses 1/64-scaled DCT values with an 8/quant multiplier.
            float quantized = coefficients[i] / table[i];
            float threshold = (i == 0 ? 0f : acOffset)
                + (adaptiveQuantization ? adaptiveStrength * adaptiveMultipliers[i] : 0f);
            float value;
            if (i == 0)
                // ComputeCoefficientBlock applies its DC threshold to the
                // difference from the preceding block. Pinned multipliers
                // and offsets are zero for DC, so this normally just rounds.
                value = MathF.Abs(quantized - previousDc) < threshold ? previousDc
                    : MathF.Round(quantized, MidpointRounding.AwayFromZero);
            else if (MathF.Abs(quantized) < threshold)
                value = 0f;
            else
                // Highway Round in QuantizeBlock uses ties-to-even for AC.
                value = MathF.Round(quantized, MidpointRounding.ToEven);
            StoreQuantizedValue(value, i, output, requireEightBitCoefficients);
        }
    }

    private static void QuantizeVectorCore(ReadOnlySpan<float> coefficients,
        ReadOnlySpan<ushort> table, Span<short> output, float acOffset,
        bool adaptiveQuantization, float adaptiveStrength, ReadOnlySpan<float> adaptiveMultipliers,
        int previousDc, bool requireEightBitCoefficients)
    {
        // DC thresholds compare with the previous block, and round ties away
        // from zero. Keep this single coefficient on the original scalar path.
        QuantizeScalarCore(coefficients, table, output, acOffset, adaptiveQuantization,
            adaptiveStrength, adaptiveMultipliers, previousDc, requireEightBitCoefficients, end: 1);

        short dc = output[0];
        int width = Vector<float>.Count;
        int batch = Vector<ushort>.Count;
        var offset = new Vector<float>(acOffset);
        var strength = new Vector<float>(adaptiveStrength);
        var finiteLimit = new Vector<float>(float.MaxValue);
        var minimum = new Vector<float>(short.MinValue);
        var maximum = new Vector<float>(short.MaxValue);
        int i = 0;
        for (; i <= BlockSize - batch; i += batch)
        {
            Vector<ushort> quantizers = new(table.Slice(i, batch));
            Vector<float> lower = new(coefficients.Slice(i, width));
            Vector<float> upper = new(coefficients.Slice(i + width, width));
            Vector<int> valid = Vector.LessThanOrEqual(Vector.Abs(lower), finiteLimit)
                & Vector.LessThanOrEqual(Vector.Abs(upper), finiteLimit);
            Vector<float> lowerThreshold = offset;
            Vector<float> upperThreshold = offset;
            if (adaptiveQuantization)
            {
                Vector<float> lowerMultipliers = new(adaptiveMultipliers.Slice(i, width));
                Vector<float> upperMultipliers = new(adaptiveMultipliers.Slice(i + width, width));
                valid &= Vector.GreaterThanOrEqual(lowerMultipliers, Vector<float>.Zero)
                    & Vector.LessThanOrEqual(lowerMultipliers, finiteLimit)
                    & Vector.GreaterThanOrEqual(upperMultipliers, Vector<float>.Zero)
                    & Vector.LessThanOrEqual(upperMultipliers, finiteLimit);
                // Preserve the separately rounded multiply and add; FMA would
                // move some strict zero-bias comparisons across their boundary.
                lowerThreshold += strength * lowerMultipliers;
                upperThreshold += strength * upperMultipliers;
            }
            // Subtraction wraps zero to ushort.MaxValue, so one comparison
            // rejects both zero and every table value above 32767.
            if (!Vector.LessThanAll(quantizers - Vector<ushort>.One, new Vector<ushort>(32767))
                || !Vector.EqualsAll(valid, new Vector<int>(-1)))
            {
                QuantizeScalarCore(coefficients, table, output, acOffset, adaptiveQuantization,
                    adaptiveStrength, adaptiveMultipliers, previousDc, requireEightBitCoefficients, start: Math.Max(i, 1));
                return;
            }

            // Division must match the scalar port exactly; multiplying by
            // precomputed reciprocals can change a half-integer boundary.
            Vector.Widen(quantizers, out Vector<uint> lowerDivisors, out Vector<uint> upperDivisors);
            lower /= Vector.ConvertToSingle(lowerDivisors);
            upper /= Vector.ConvertToSingle(upperDivisors);
            lower = Vector.ConditionalSelect(Vector.LessThan(Vector.Abs(lower), lowerThreshold),
                Vector<float>.Zero, Vector.Round(lower, MidpointRounding.ToEven));
            upper = Vector.ConditionalSelect(Vector.LessThan(Vector.Abs(upper), upperThreshold),
                Vector<float>.Zero, Vector.Round(upper, MidpointRounding.ToEven));
            // Reinsert the scalar DC result, including its previous-block
            // threshold, before validating or storing the complete batch.
            if (i == 0)
                lower = Vector.WithElement(lower, 0, dc);
            if (requireEightBitCoefficients)
            {
                var acMinimum = new Vector<float>(-1023f);
                Vector<float> lowerMinimum = i == 0 ? Vector.WithElement(acMinimum, 0, -1024f) : acMinimum;
                Vector<int> inRange = Vector.GreaterThanOrEqual(lower, lowerMinimum)
                    & Vector.LessThanOrEqual(lower, new Vector<float>(1023f))
                    & Vector.GreaterThanOrEqual(upper, acMinimum)
                    & Vector.LessThanOrEqual(upper, new Vector<float>(1023f));
                if (!Vector.EqualsAll(inRange, new Vector<int>(-1)))
                {
                    // Re-run the untouched batch in scalar order to retain
                    // the first exception and its exact written prefix.
                    QuantizeScalarCore(coefficients, table, output, acOffset, adaptiveQuantization,
                        adaptiveStrength, adaptiveMultipliers, previousDc, requireEightBitCoefficients, start: Math.Max(i, 1));
                    return;
                }
            }
            Vector<int> lowerIntegers = Vector.ConvertToInt32(Vector.Clamp(lower, minimum, maximum));
            Vector<int> upperIntegers = Vector.ConvertToInt32(Vector.Clamp(upper, minimum, maximum));
            Vector.Narrow(lowerIntegers, upperIntegers).CopyTo(output.Slice(i, batch));
        }
        QuantizeScalarCore(coefficients, table, output, acOffset, adaptiveQuantization,
            adaptiveStrength, adaptiveMultipliers, previousDc, requireEightBitCoefficients, start: i);
    }

    private static void StoreQuantizedValue(float value, int index, Span<short> output,
        bool requireEightBitCoefficients)
    {
        // T.81 F.1.1.4 requires signed 11-bit quantized coefficients for
        // eight-bit input. AC entropy categories further exclude -1024.
        // Check before int16 demotion can hide a value through saturation.
        if (requireEightBitCoefficients &&
            (value < (index == 0 ? -1024f : -1023f) || value > 1023f))
            throw new ArgumentOutOfRangeException("coefficients",
                "A quantized coefficient exceeds eight-bit JPEG's supported range.");
        // Saturating int16 demotion matches StoreQuantizedValue's DemoteTo.
        output[index] = (short)Math.Clamp(value, short.MinValue, short.MaxValue);
    }

    internal static float DistanceToScale(float distance, int frequency)
    {
        if (distance < NonlinearThreshold)
            return distance;
        float exponent = FrequencyExponents[frequency];
        // C++ uses std::pow(float, double) for the first pow due to 1.0.
        float multiplier = (float)Math.Pow(NonlinearThreshold, 1.0 - exponent);
        return MathF.Max(0.5f * distance, multiplier * MathF.Pow(distance, exponent));
    }

    // SetQuantMatrices and QuantValsToDistance use this tuning only for PQ and
    // HLG. Apply it before 4:2:0 rescaling to preserve upstream float ordering.
    // Conventional tables have their own scale, but adaptive inference still
    // uses this perceptual global scale even when those tables are selected.
    private static float TransferFunctionGlobalScale(int cicpTransferFunction)
    {
        if (cicpTransferFunction is < 0 or > 255)
            throw new ArgumentOutOfRangeException(nameof(cicpTransferFunction));
        return cicpTransferFunction switch
        {
            16 => GlobalScale * 0.4f,
            18 => GlobalScale * 0.5f,
            _ => GlobalScale
        };
    }

    private static float ScaleToDistance(float scale, int frequency)
    {
        if (scale < NonlinearThreshold)
            return scale;
        float exponent = (float)(1.0 / FrequencyExponents[frequency]);
        float multiplier = (float)Math.Pow(NonlinearThreshold, 1.0 - exponent);
        return MathF.Min(2f * scale, multiplier * MathF.Pow(scale, exponent));
    }

    private static void ValidateComponent(int component)
    {
        if (component is < 0 or > 2)
            throw new ArgumentOutOfRangeException(nameof(component), "Component must be 0 (Y), 1 (Cb) or 2 (Cr).");
    }
}
