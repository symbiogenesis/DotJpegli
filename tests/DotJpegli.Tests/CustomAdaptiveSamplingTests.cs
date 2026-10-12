using DotJpegli.Internal;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class CustomAdaptiveSamplingTests
{
    // Independent full-image scalar C++ fixtures for the pattern below, built
    // with MSVC /O2 /fp:strict from google/jpegli adaptive_quantization.cc,
    // revision 031a0077f5799a6041004267fc12b956c1f52a20.
    // These have no component traversal or managed MCU-buffer dependencies.
    private static readonly float[] VerticalField =
    [
        0.230854392f, 0.288506269f, 0.231943727f, 0.879613161f, 0.804180264f, 0.344471812f,
        1.09839153f, 1.05328417f, 0.501490116f, 1.42142868f, 1.41078687f, 0.410299063f,
        1.52733493f, 1.43623662f, 0.507885218f, 1.11097455f, 1.09776831f, 0.417951226f,
        1.50498152f, 1.4264636f, 0.52168119f, 1.4671576f, 1.36475134f, 0.341248751f,
        0.328990102f, 0.17832303f, 0.0930894613f, 0.264642239f, 0.138762355f, 0.0824182034f,
        0.264642239f, 0.138762355f, 0.0824182034f, 0.264642239f, 0.138762355f, 0.0824182034f
    ];

    private static readonly float[] HorizontalField =
    [
        0.234130859f, 0.773663998f, 0.969280958f, 1.4041934f, 1.30952334f, 1.05474782f,
        1.43133044f, 1.22194791f, 0.120052218f, 0.275330901f, 0.780780673f, 1.13384151f,
        1.57001662f, 1.44022679f, 1.16640162f, 1.60527563f, 1.36409307f, 0.162204385f,
        0.154098153f, 0.41079843f, 0.62427175f, 0.801020741f, 0.67228651f, 0.763576388f,
        0.697763443f, 0.772921681f, 0.0808284283f
    ];

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    public void ChromaAcUsesPinnedComponentRowsAndScaledFieldColumns(
        bool vertical, bool progressive, bool optimize)
    {
        int width = vertical ? 17 : 65;
        int height = vertical ? 65 : 17;
        JpegSamplingFactors sampling = vertical
            ? new(new(1, 4), new(1, 2), new(1, 1))
            : new(new(4, 1), new(2, 1), new(1, 1));
        byte[] pixels = Pattern(width, height, vertical);
        var options = new JpegliEncoderOptions
        {
            Quality = 85, SamplingFactors = sampling, Progressive = progressive,
            OptimizeHuffmanTables = optimize, AdaptiveQuantization = true, RestartInterval = 2
        };
        JpegDocument document = JpegDocument.Read(
            Encode(pixels, width, height, PixelLayout.Rgb24, options), progressive, 2);
        ushort[][] tables = Enumerable.Range(0, 3)
            .Select(component => Quantization.CreateTable(85, component)).ToArray();
        Assert.Equal((ushort)4, tables[0][1]);
        float[][] multipliers = Quantization.CreateAdaptiveZeroBiasMultipliers(tables, 1, 1, false);
        float[] fixture = vertical ? VerticalField : HorizontalField;
        var analysis = new AdaptiveQuantization(width, height, sampling.Luma.Vertical);
        float[] row = new float[analysis.BlocksWide * sampling.Luma.Vertical];
        for (int top = 0; top < height; top += 8 * sampling.Luma.Vertical)
        {
            analysis.ComputeRow(pixels, width * 3, 3, PixelLayout.Rgb24, top, 4,
                row, TestContext.Current.CancellationToken);
            for (int i = 0; i < row.Length; i++)
                Assert.InRange(MathF.Abs(fixture[top / 8 * analysis.BlocksWide + i] - row[i]), 0, 0.00001f);
        }

        float[] samples = new float[64];
        float[] coefficients = new float[64];
        short[] expected = new short[64];
        short[] alternative = new short[64];
        int sensitiveBlocks = 0;
        foreach ((BlockKey key, Block block) in document.Blocks.Where(pair => pair.Key.Component != 1))
        {
            if (block.IsDummy)
            {
                Assert.All(block.Coefficients[1..], value => Assert.Equal(0, value));
                continue;
            }
            int component = key.Component - 1;
            JpegComponentSampling factor = component == 1 ? sampling.Cb : sampling.Cr;
            int sampleWidth = sampling.Luma.Horizontal / factor.Horizontal;
            int sampleHeight = sampling.Luma.Vertical / factor.Vertical;
            int mcuX = key.X / factor.Horizontal;
            int mcuY = key.Y / factor.Vertical;
            int localX = key.X % factor.Horizontal;
            int localY = key.Y % factor.Vertical;
            int fieldY = mcuY * sampling.Luma.Vertical + localY;
            int fieldX = key.X * sampleWidth;
            // Pinned encode_streaming.cc 191 uses qf[iy * stride + bx * h_factor]
            // in every initial coefficient path, including progressive output.
            float strength = fixture[fieldY * analysis.BlocksWide + fieldX];
            Downsampling.FillBlock(pixels, width, height, width * 3, 3, PixelLayout.Rgb24,
                component, key.X * 8 * sampleWidth, key.Y * 8 * sampleHeight,
                sampleWidth, sampleHeight, samples);
            ForwardDct.Transform(samples, coefficients);
            Quantization.Quantize(coefficients, tables[component], expected, component,
                Quantization.QualityToDistance(85), adaptiveQuantization: true,
                adaptiveStrength: strength, adaptiveMultipliers: multipliers[component]);
            for (int scanIndex = 1; scanIndex < 64; scanIndex++)
                Assert.Equal(expected[JpegConstants.ZigZag[scanIndex]], block.Coefficients[scanIndex]);

            // Prove the fixtures detect physically scaled vertical rows, or
            // columns that forget the within-MCU horizontal downsampling ratio.
            int alternativeY = vertical ? key.Y * sampleHeight : fieldY;
            int alternativeX = vertical ? fieldX : mcuX * sampling.Luma.Horizontal + localX;
            Quantization.Quantize(coefficients, tables[component], alternative, component,
                Quantization.QualityToDistance(85), adaptiveQuantization: true,
                adaptiveStrength: fixture[alternativeY * analysis.BlocksWide + alternativeX],
                adaptiveMultipliers: multipliers[component]);
            if (!expected.AsSpan(1).SequenceEqual(alternative.AsSpan(1)))
                sensitiveBlocks++;
        }
        Assert.True(sensitiveBlocks > 0, "The fixture must distinguish the alternative field mapping.");
    }

    private static byte[] Pattern(int width, int height, bool vertical)
    {
        byte[] pixels = new byte[width * height * 3];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int axis = vertical ? y : x;
                int basis = axis < 12 ? 64 + (x + y) % 5 : 48 + (x * 17 + y * 29 + x * y * 7) % 160;
                int offset = (y * width + x) * 3;
                pixels[offset] = (byte)(basis + (x * 5 + y * 11) % 61 - 30);
                pixels[offset + 1] = (byte)basis;
                pixels[offset + 2] = (byte)(basis + (x * 13 + y * 7) % 71 - 35);
            }
        return pixels;
    }
}
