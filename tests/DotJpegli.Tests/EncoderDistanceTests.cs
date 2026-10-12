using DotJpegli.Internal;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderDistanceTests
{
    [Theory]
    [InlineData(0f)]
    [InlineData(0.125f)]
    [InlineData(12.5f)]
    [InlineData(29.75f)]
    [InlineData(30f)]
    [InlineData(30.125f)]
    [InlineData(85.25f)]
    [InlineData(99.5f)]
    [InlineData(100f)]
    public void PublicQualityMappingMatchesIndependentPolynomial(float quality)
    {
        // The pinned C++ API accepts an integer. The managed API retains the
        // same piecewise polynomial while preserving fractional quality.
        float expected = quality == 100f ? 0.01f
            : quality >= 30f ? 0.1f + (100f - quality) * 0.09f
            : (53f / 3000f) * quality * quality - (23f / 20f) * quality + 25f;
        Assert.Equal(expected, JpegliEncoder.QualityToDistance(quality));
        Assert.True(expected > 0f);
    }

    [Fact]
    public void QualityMappingPreservesExactBranchBoundaries()
    {
        Assert.Equal(25f, JpegliEncoder.QualityToDistance(0));
        Assert.Equal(0.1f + 70f * 0.09f, JpegliEncoder.QualityToDistance(30));
        Assert.Equal(0.01f, JpegliEncoder.QualityToDistance(100));
        foreach (float quality in new[] { MathF.BitDecrement(30f), MathF.BitIncrement(30f), MathF.BitDecrement(100f) })
            PublicQualityMappingMatchesIndependentPolynomial(quality);
        // The high-quality endpoint is intentionally discontinuous upstream.
        Assert.True(JpegliEncoder.QualityToDistance(MathF.BitDecrement(100f)) > 0.1f);
    }

    [Theory]
    [InlineData(float.NaN)]
    [InlineData(float.NegativeInfinity)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(-0.001f)]
    [InlineData(100.001f)]
    public void PublicQualityMappingRejectsInvalidQuality(float quality)
    {
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => JpegliEncoder.QualityToDistance(quality));
        Assert.Equal("quality", error.ParamName);
    }

    [Fact]
    public void NullDistanceRetainsDefaultEncoding()
    {
        Assert.Null(new JpegliEncoderOptions().Distance);
        for (int kind = 0; kind < 3; kind++)
        {
            Array pixels = Source(kind, 19, 17, PixelComponentLayout.Rgb);
            Assert.Equal(EncodeSource(pixels, 19, 17, PixelComponentLayout.Rgb, new()),
                EncodeSource(pixels, 19, 17, PixelComponentLayout.Rgb, new() { Distance = null }));
        }
    }

    [Theory]
    [InlineData(0, PixelComponentLayout.Rgb, 0, 0f)]
    [InlineData(0, PixelComponentLayout.Gray, 1, 100f)]
    [InlineData(0, PixelComponentLayout.YCbCr, 2, 85.25f)]
    [InlineData(1, PixelComponentLayout.Rgb, 3, 29.75f)]
    [InlineData(1, PixelComponentLayout.Gray, 4, 99.5f)]
    [InlineData(1, PixelComponentLayout.YCbCr, 5, 30f)]
    [InlineData(2, PixelComponentLayout.Rgb, 6, 12.5f)]
    [InlineData(2, PixelComponentLayout.Gray, 7, 75.125f)]
    [InlineData(2, PixelComponentLayout.YCbCr, 8, 90.5f)]
    public void MappedDistanceMatchesQualityAcrossInputAndEncodingModes(
        int kind, PixelComponentLayout layout, int mode, float quality)
    {
        const int width = 19, height = 17;
        Array pixels = Source(kind, width, height, layout);
        JpegliEncoderOptions options = Mode(mode) with { Quality = quality };
        byte[] expected = EncodeSource(pixels, width, height, layout, options);
        byte[] actual = EncodeSource(pixels, width, height, layout, options with
        {
            Distance = JpegliEncoder.QualityToDistance(quality), Quality = quality == 0f ? 100f : 0f
        });
        Assert.Equal(expected, actual);
        JpegDocument document = Read(actual, options);
        Assert.Equal(width, document.Width);
        Assert.Equal(height, document.Height);
        ImageResult decoded = ImageResult.FromMemory(actual,
            layout == PixelComponentLayout.Gray ? ColorComponents.Grey : ColorComponents.RedGreenBlue);
        Assert.Equal(width, decoded.Width);
        Assert.Equal(height, decoded.Height);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExplicitDistanceOverridesValidQuality(bool adaptive)
    {
        byte[] pixels = Pattern(19, 17, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions { Distance = 2.375f, Quality = 0f, AdaptiveQuantization = adaptive };
        byte[] expected = JpegliEncoder.Encode(pixels, 19, 17, options: options,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected, JpegliEncoder.Encode(pixels, 19, 17, options: options with { Quality = 100f },
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.False(expected.SequenceEqual(JpegliEncoder.Encode(pixels, 19, 17,
            options: options with { Distance = null }, cancellationToken: TestContext.Current.CancellationToken)));
    }

    [Fact]
    public void DirectDistanceTablesMatchPinnedCppFixtures()
    {
        // Natural-order tables from the verbatim pinned quant.cc constants,
        // DistanceToScale and baseline SetQuantMatrices loop, built with MSVC
        // 19.51 x64 /O2 /fp:precise. Upstream revision:
        // google/jpegli 031a0077f5799a6041004267fc12b956c1f52a20.
        ushort[] y444AtHalf =
        [
            1,1,3,2,3,3,3,3,1,2,2,2,3,3,4,4,3,2,3,3,3,3,3,4,2,2,3,3,4,4,4,4,
            3,3,3,4,4,4,5,5,3,3,3,4,4,5,4,5,3,4,3,4,5,4,5,5,3,4,4,4,5,5,5,4
        ];
        ushort[] y444AtThreshold =
        [
            3,4,8,7,9,9,10,10,4,5,7,7,9,8,11,11,8,7,8,9,9,10,10,11,7,7,9,9,11,13,12,12,
            9,9,9,11,12,13,15,14,9,8,10,13,13,14,13,16,10,11,10,12,15,13,14,14,10,11,11,12,14,16,14,11
        ];
        ushort[] cb420AtThirty =
        [
            74,66,103,113,222,255,255,255,66,88,100,165,255,255,255,255,
            103,100,157,227,255,255,255,255,113,165,227,255,255,255,255,255,
            222,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,
            255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255
        ];
        ushort[] cr420AtThirty =
        [
            76,46,81,70,171,158,188,203,46,62,78,86,157,139,177,184,
            81,78,97,86,135,164,169,193,70,86,86,135,173,132,144,204,
            171,157,135,173,134,205,156,197,158,139,164,132,205,176,202,255,
            188,177,169,144,156,202,255,255,203,184,193,204,197,255,255,255
        ];
        Assert.Equal(y444AtHalf, Quantization.CreateTableForDistance(0.5f, 0));
        Assert.Equal(y444AtHalf, Quantization.CreateGrayscaleTableForDistance(0.5f));
        Assert.Equal(y444AtThreshold, Quantization.CreateTableForDistance(1.5f, 0));
        Assert.Equal(cb420AtThirty, Quantization.CreateTableForDistance(30f, 1, 2, 2));
        Assert.Equal(cr420AtThirty, Quantization.CreateTableForDistance(30f, 2, 2, 2));
    }

    [Theory]
    [InlineData(0.001f)]
    [InlineData(30f)]
    [InlineData(float.Epsilon)]
    [InlineData(float.MaxValue)]
    public void DistancesOutsideQualityRangeEmitBaselineTablesAndIndependentDc(float distance)
    {
        Assert.True(distance < JpegliEncoder.QualityToDistance(100f)
            || distance > JpegliEncoder.QualityToDistance(0f));
        foreach (bool progressive in new[] { false, true })
        {
            var options = new JpegliEncoderOptions
            {
                Distance = distance, AdaptiveQuantization = false, Progressive = progressive,
                ProgressiveRefinementBits = progressive ? 2 : 0, RestartInterval = 1
            };
            byte[] pixels = Enumerable.Repeat((byte)64, 64).ToArray();
            byte[] grayscale = JpegliEncoder.Encode(pixels, 8, 8, PixelLayout.Gray8, options,
                cancellationToken: TestContext.Current.CancellationToken);
            JpegDocument document = Read(grayscale, options);
            ImageResult grayDecoded = ImageResult.FromMemory(grayscale, ColorComponents.Grey);
            Assert.Equal((8, 8), (grayDecoded.Width, grayDecoded.Height));
            byte[] table = document.Quantizers[document.Components[1].Quantizer];
            AssertTableEntries(table, distance, 0, false);
            Assert.All(table, value => Assert.InRange(value, (byte)1, (byte)255));
            if (distance < 0.01f || distance == float.MaxValue)
                Assert.All(table, value => Assert.Equal(distance < 0.01f ? (byte)1 : (byte)255, value));
            Block block = Assert.Single(document.Blocks).Value;
            int dc = (int)Math.Round(8d * (64 - 128) / table[0], MidpointRounding.AwayFromZero);
            Assert.Equal(dc, block.Coefficients[0]);
            Assert.All(block.Coefficients[1..], value => Assert.Equal(0, value));

            foreach (ChromaSubsampling sampling in new[] { ChromaSubsampling.None, ChromaSubsampling.Quarter })
            {
                Array color = Enumerable.Range(0, 8 * 8 * 3)
                    .Select(index => (byte)(index % 3 == 0 ? 64 : index % 3 == 1 ? 96 : 160)).ToArray();
                JpegliEncoderOptions colorOptions = options with { Subsampling = sampling };
                byte[] jpeg = EncodeSource(color, 8, 8, PixelComponentLayout.YCbCr, colorOptions);
                JpegDocument actual = Read(jpeg, colorOptions);
                ImageResult colorDecoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
                Assert.Equal((8, 8), (colorDecoded.Width, colorDecoded.Height));
                for (int component = 0; component < 3; component++)
                {
                    byte[] quantizer = actual.Quantizers[actual.Components[component + 1].Quantizer];
                    AssertTableEntries(quantizer, distance, component, sampling == ChromaSubsampling.Quarter);
                    Assert.All(quantizer, value => Assert.InRange(value, (byte)1, (byte)255));
                    Block colorBlock = actual.Blocks[new(component + 1, 0, 0)];
                    int sample = component == 0 ? 64 : component == 1 ? 96 : 160;
                    Assert.Equal((int)Math.Round(8d * (sample - 128) / quantizer[0], MidpointRounding.AwayFromZero),
                        colorBlock.Coefficients[0]);
                    Assert.All(colorBlock.Coefficients[1..], value => Assert.Equal(0, value));
                }
            }
        }
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-0.001f)]
    [InlineData(float.NaN)]
    [InlineData(float.NegativeInfinity)]
    [InlineData(float.PositiveInfinity)]
    public void InvalidDistanceIsRejectedByEveryInputOverloadBeforeOutput(float distance)
    {
        for (int kind = 0; kind < 3; kind++)
        {
            using var destination = new MemoryStream();
            using var output = new WriteOnlyStream(destination);
            Array pixels = Source(kind, 1, 1, PixelComponentLayout.Rgb);
            var error = Assert.Throws<ArgumentOutOfRangeException>(() => EncodeSource(output, pixels, 1, 1,
                PixelComponentLayout.Rgb, new() { Distance = distance }, TestContext.Current.CancellationToken));
            Assert.Equal(nameof(JpegliEncoderOptions.Distance), error.ParamName);
            Assert.Equal(0, destination.Length);
            Assert.False(output.Closed);
        }
        var tableError = Assert.Throws<ArgumentOutOfRangeException>(() => Quantization.CreateTableForDistance(distance, 0));
        Assert.Equal("distance", tableError.ParamName);
        Assert.Throws<ArgumentOutOfRangeException>(() => Quantization.CreateGrayscaleTableForDistance(distance));
    }

    [Theory]
    [InlineData(-1f)]
    [InlineData(101f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void DistanceOverrideStillValidatesQualityBeforeOutput(float quality)
    {
        using var destination = new MemoryStream();
        var error = Assert.Throws<ArgumentOutOfRangeException>(() => JpegliEncoder.Encode(destination, new byte[3], 1, 1,
            options: new() { Quality = quality, Distance = 1f }, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.Quality), error.ParamName);
        Assert.Equal(0, destination.Length);
    }

    [Theory]
    [InlineData(false, 0, false)]
    [InlineData(false, 0, true)]
    [InlineData(true, 0, false)]
    [InlineData(true, 2, true)]
    public void FloatCoefficientPreflightUsesExplicitDistance(bool progressive, int refinementBits, bool optimize)
    {
        float[] pixels = Enumerable.Repeat(-0.1f, 64).ToArray();
        var options = new JpegliEncoderOptions
        {
            Quality = 100f, Distance = 30f, AllowOutOfRangeFloatSamples = true,
            AdaptiveQuantization = false, Progressive = progressive,
            ProgressiveRefinementBits = refinementBits, OptimizeHuffmanTables = optimize
        };
        // At distance 30, the DC quantizer is 65 and the preserved sample -25.5
        // has representable DC -19. Quality 100 would instead give DC -1228.
        byte[] jpeg = JpegliEncoder.Encode(pixels, 8, 8, PixelComponentLayout.Gray, options,
            cancellationToken: TestContext.Current.CancellationToken);
        JpegDocument document = Read(jpeg, options);
        byte quantizer = document.Quantizers[document.Components[1].Quantizer][0];
        Assert.Equal((byte)65, quantizer);
        int dc = (int)Math.Round(8d * (pixels[0] * 255d - 128d) / quantizer, MidpointRounding.AwayFromZero);
        Assert.Equal(-19, dc);
        Assert.Equal(dc, Assert.Single(document.Blocks).Value.Coefficients[0]);

        using var destination = new MemoryStream();
        var error = Assert.Throws<ArgumentException>(() => JpegliEncoder.Encode(destination, pixels, 8, 8,
            PixelComponentLayout.Gray, options with { Quality = 0f, Distance = 0.001f },
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal("pixels", error.ParamName);
        Assert.Equal(0, destination.Length);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ExplicitDistancePreservesNonSeekableStreamAndCancellationContract(int kind)
    {
        const int width = 33, height = 19;
        Array pixels = Source(kind, width, height, PixelComponentLayout.YCbCr);
        var options = new JpegliEncoderOptions
        {
            Distance = 0.375f, Progressive = true, ProgressiveRefinementBits = 2,
            RestartInterval = 2, SmoothingFactor = 37
        };
        byte[] complete = EncodeSource(pixels, width, height, PixelComponentLayout.YCbCr, options);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        EncodeSource(output, pixels, width, height, PixelComponentLayout.YCbCr, options,
            TestContext.Current.CancellationToken);
        Assert.Equal(complete, destination.ToArray());
        Assert.False(output.Closed);

        using var emptyDestination = new MemoryStream();
        using var emptyOutput = new WriteOnlyStream(emptyDestination);
        var canceled = new CancellationToken(true);
        var preError = Assert.Throws<OperationCanceledException>(() => EncodeSource(emptyOutput, pixels,
            width, height, PixelComponentLayout.YCbCr, options, canceled));
        Assert.Equal(canceled, preError.CancellationToken);
        Assert.Equal(0, emptyDestination.Length);
        Assert.False(emptyOutput.Closed);

        int cancelAfter = Read(complete, options).Scans[2].DataStart + 1;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var partial = new MemoryStream();
        using var partialOutput = new WriteOnlyStream(partial, () =>
        {
            if (partial.Length >= cancelAfter)
                cancellation.Cancel();
        });
        var error = Assert.Throws<OperationCanceledException>(() => EncodeSource(partialOutput, pixels,
            width, height, PixelComponentLayout.YCbCr, options, cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.False(partialOutput.Closed);
        Assert.InRange(partial.Length, cancelAfter, complete.Length - 1);
        Assert.Equal(complete[..(int)partial.Length], partial.ToArray());
    }

    private static void AssertTableEntries(byte[] table, float distance, int component, bool is420)
    {
        // Independent subset of the pinned quant.cc natural-order matrices.
        // Frequencies 0, 1 and 63 are also emitted at zigzag positions 0, 1 and 63.
        float[][] matrix =
        [
            [1.2397409345866273f, 1.7227115097630963f, 4.37787101190424f],
            [2.8236197786377537f, 6.495639358561486f, 112.16099098350989f],
            [2.9217254961255255f, 4.497681013199305f, 114.89202448569779f]
        ];
        float[] exponents = [1f, 0.51f, 1f];
        float[] rescale = [0.4093f, 0.3209f, 0.3368f];
        int[] zigzag = [0, 1, 63];
        for (int index = 0; index < zigzag.Length; index++)
        {
            float scale = distance < 1.5f ? distance : MathF.Max(0.5f * distance,
                (float)Math.Pow(1.5f, 1d - exponents[index]) * MathF.Pow(distance, exponents[index]));
            scale *= is420 ? 1.73966010f * 1.22f : 1.73966010f;
            if (is420 && component > 0)
                scale *= rescale[index];
            float quantizer = MathF.Round(scale * matrix[component][index], MidpointRounding.AwayFromZero);
            Assert.Equal((byte)Math.Clamp(quantizer, 1f, 255f), table[zigzag[index]]);
        }
    }

    private static JpegliEncoderOptions Mode(int mode) => mode switch
    {
        0 => new() { AdaptiveQuantization = false, OptimizeHuffmanTables = false, Subsampling = ChromaSubsampling.None },
        1 => new() { Progressive = true, ProgressiveRefinementBits = 2, SmoothingFactor = 37, RestartInterval = 2 },
        2 => new() { RestartInterval = 1 },
        3 => new() { Progressive = true, AdaptiveQuantization = false, Subsampling = ChromaSubsampling.HalfVertical,
            SmoothingFactor = 100, RestartInterval = 3 },
        4 => new() { Subsampling = ChromaSubsampling.None, SmoothingFactor = 100, RestartInterval = 1 },
        5 => new() { Progressive = true, ProgressiveRefinementBits = 2, AdaptiveQuantization = false,
            SamplingFactors = new(new(2, 1), new(1, 2), new(2, 1)), SmoothingFactor = 37, RestartInterval = 2 },
        6 => new() { Progressive = true, ProgressiveRefinementBits = 2,
            SamplingFactors = new(new(1, 1), new(2, 1), new(2, 1)), RestartInterval = 1 },
        7 => new() { Progressive = true, AdaptiveQuantization = false, SmoothingFactor = 37, RestartInterval = 2 },
        _ => new() { OptimizeHuffmanTables = false, Subsampling = ChromaSubsampling.ThirdHorizontal,
            SmoothingFactor = 73, RestartInterval = 3 }
    };

    private static Array Source(int kind, int width, int height, PixelComponentLayout layout)
    {
        byte[] bytes = Pattern(width, height, layout == PixelComponentLayout.Gray ? PixelLayout.Gray8 : PixelLayout.Rgb24);
        return kind switch
        {
            0 => bytes,
            1 => bytes.Select((value, index) => (ushort)(value * 257 + index % 113)).ToArray(),
            _ => bytes.Select((value, index) => (value + index % 113 / 257f) / 255f).ToArray()
        };
    }

    private static PixelLayout ByteLayout(PixelComponentLayout layout) => layout switch
    {
        PixelComponentLayout.Gray => PixelLayout.Gray8,
        PixelComponentLayout.YCbCr => PixelLayout.YCbCr24,
        _ => PixelLayout.Rgb24
    };

    private static byte[] EncodeSource(Array pixels, int width, int height, PixelComponentLayout layout,
        JpegliEncoderOptions options)
    {
        using var destination = new MemoryStream();
        EncodeSource(destination, pixels, width, height, layout, options, TestContext.Current.CancellationToken);
        return destination.ToArray();
    }

    private static void EncodeSource(Stream output, Array pixels, int width, int height,
        PixelComponentLayout layout, JpegliEncoderOptions options, CancellationToken cancellationToken)
    {
        if (pixels is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, ByteLayout(layout), options, cancellationToken: cancellationToken);
        else if (pixels is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, layout, options, cancellationToken: cancellationToken);
        else
            JpegliEncoder.Encode(output, (float[])pixels, width, height, layout, options, cancellationToken: cancellationToken);
    }

    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options) =>
        JpegDocument.Read(jpeg, options.Progressive, options.RestartInterval, options.ProgressiveRefinementBits);
}
