namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class FastCompressionOptionsTests
{
    [Theory]
    [InlineData(-0.01f)]
    [InlineData(0.51f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void TailZeroRunBiasValidationRejectsOutOfRangeValues(float bias)
    {
        var options = new JpegliEncoderOptions { TailZeroRunBias = bias };
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
        Assert.Equal(nameof(JpegliEncoderOptions.TailZeroRunBias), exception.ParamName);
    }

    [Theory]
    [InlineData(-0.01f)]
    [InlineData(0.51f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void ChromaDownsamplingSharpnessValidationRejectsOutOfRangeValues(float sharpness)
    {
        var options = new JpegliEncoderOptions { ChromaDownsamplingSharpness = sharpness };
        ArgumentOutOfRangeException exception = Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
        Assert.Equal(nameof(JpegliEncoderOptions.ChromaDownsamplingSharpness), exception.ParamName);
    }

    [Fact]
    public void TailZeroRunBiasPrunesBorderlineTrailingUnitCoefficientAfterZrl()
    {
        ushort[] rawTable = new ushort[64];
        Array.Fill(rawTable, (ushort)10);
        var defaultTable = Quantization.PreparedTable.Create(rawTable, component: 0, distance: 1f,
            adaptiveQuantization: false, tailZeroRunBias: 0f);
        var biasedTable = Quantization.PreparedTable.Create(rawTable, component: 0, distance: 1f,
            adaptiveQuantization: false, tailZeroRunBias: 0.2f);

        float[] coefficients = new float[64];
        coefficients[0] = 100f;
        int targetZigZag = 25;
        int targetNatural = JpegConstants.ZigZag[targetZigZag];
        // Unrounded quantized value = 0.60, which is >= AcOffset (0.5) so default rounds to 1,
        // but < AcOffset + 0.2 (0.7) so biasedTable prunes it because it is preceded by 24 zeros (>= 16).
        coefficients[targetNatural] = 6.0f;

        short[] defaultOutput = new short[64];
        short[] biasedOutput = new short[64];
        Quantization.QuantizePrepared(coefficients, defaultTable, defaultOutput);
        Quantization.QuantizePrepared(coefficients, biasedTable, biasedOutput);

        Assert.Equal(1, defaultOutput[targetNatural]);
        Assert.Equal(0, biasedOutput[targetNatural]);

        // Verify a trailing unit coefficient preceded by < 16 zeros is preserved.
        Array.Clear(coefficients);
        coefficients[0] = 100f;
        int shortRunNatural = JpegConstants.ZigZag[10];
        coefficients[shortRunNatural] = 6.0f;
        Quantization.QuantizePrepared(coefficients, biasedTable, biasedOutput);
        Assert.Equal(1, biasedOutput[shortRunNatural]);

        // Verify a trailing magnitude-2 coefficient preceded by >= 16 zeros is preserved.
        Array.Clear(coefficients);
        coefficients[0] = 100f;
        coefficients[targetNatural] = 16.0f;
        Quantization.QuantizePrepared(coefficients, biasedTable, biasedOutput);
        Assert.Equal(2, biasedOutput[targetNatural]);
    }

    [Fact]
    public void ChromaDownsamplingSharpnessPreservesQuadContrastAcrossAllPaths()
    {
        const float topLeft = 40f, topRight = 40f, bottomLeft = 40f, bottomRight = 200f;
        const float sharpness = 0.25f;
        // Mean = (40 + 40 + 40 + 200) * 0.25 = 80
        // Min = 40, Max = 200 -> Midpoint = 120
        // Result = 80 + 0.25 * (120 - 80) = 90
        float expected = Downsampling.Downsample2x2Quad(topLeft, topRight, bottomLeft, bottomRight, sharpness);
        Assert.Equal(90f, expected);
        Assert.Equal(80f, Downsampling.Downsample2x2Quad(topLeft, topRight, bottomLeft, bottomRight, 0f));

        const int width = 32, height = 32;
        byte[] rgb = new byte[width * height * 3];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int offset = ((y * width) + x) * 3;
                bool spike = (x & 1) == 1 && (y & 1) == 1;
                rgb[offset] = spike ? (byte)240 : (byte)30;
                rgb[offset + 1] = spike ? (byte)40 : (byte)160;
                rgb[offset + 2] = spike ? (byte)200 : (byte)50;
            }
        }

        PixelSource uncachedSource = new PixelSource(rgb, PixelLayout.Rgb24)
            .WithColorSpace(JpegColorSpace.YCbCr, chromaDownsamplingSharpness: sharpness, tailZeroRunBias: 0.15f);
        PixelSource fullCachedSource = uncachedSource.WithConvertedPlanes(width, height, width * 3, TestContext.Current.CancellationToken);
        PixelSource compact420Source = uncachedSource.WithConverted420Planes(width, height, width * 3, TestContext.Current.CancellationToken);

        float[] scalarSamples = new float[64];
        float[] vectorUncachedSamples = new float[64];
        float[] vectorFullCachedSamples = new float[64];
        float[] smoothedZeroSamples = new float[64];

        for (int component = 1; component <= 2; component++)
        {
            Downsampling.FillBlockScalar(uncachedSource, width, height, width * 3, component, 0, 0, 2, 2, scalarSamples);
            Downsampling.FillBlockVector(uncachedSource, width, height, width * 3, component, 0, 0, 2, 2, vectorUncachedSamples);
            Downsampling.FillBlockVector(fullCachedSource, width, height, width * 3, component, 0, 0, 2, 2, vectorFullCachedSamples);
            SmoothedDownsampling.FillBlock(uncachedSource, width, height, width * 3, component, 0, 0, 2, 2, smoothedZeroSamples, smoothingFactor: 0);

            Assert.Equal(scalarSamples, vectorUncachedSamples);
            Assert.Equal(scalarSamples, vectorFullCachedSamples);
            Assert.Equal(scalarSamples, smoothedZeroSamples);

            ReadOnlySpan<float> compactPlane = compact420Source.GetConverted420ChromaPlane(component, out int cw, out _);
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Assert.Equal(scalarSamples[(row * 8) + col], compactPlane[(row * cw) + col] - 128f);
                }
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(3)]
    public void FastCompressionOptionsAreBitIdenticalAcrossCacheAndParallelism(int progressiveLevel)
    {
        const int width = 144, height = 144;
        byte[] rgb = new byte[width * height * 3];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = ((y * width) + x) * 3;
                rgb[index] = (byte)(((x * 13) + (y * 7) + ((x ^ y) * 3)) & 0xFF);
                rgb[index + 1] = (byte)(((x * 5) + (y * 19) + ((x & 3) == 0 ? 90 : 10)) & 0xFF);
                rgb[index + 2] = (byte)(((x * 23) + (y * 11) + ((y & 3) == 0 ? 110 : 20)) & 0xFF);
            }
        }

        byte[] baselineCached = JpegliEncoder.Encode(rgb, width, height, PixelLayout.Rgb24, new JpegliEncoderOptions
        {
            Distance = 1.5f,
            Subsampling = ChromaSubsampling.Quarter,
            ProgressiveLevel = progressiveLevel,
            TailZeroRunBias = 0.15f,
            ChromaDownsamplingSharpness = 0.25f,
            MaxDegreeOfParallelism = 1
        }, cancellationToken: TestContext.Current.CancellationToken);

        byte[] uncached = JpegliEncoder.Encode(rgb, width, height, PixelLayout.Rgb24, new JpegliEncoderOptions
        {
            Distance = 1.5f,
            Subsampling = ChromaSubsampling.Quarter,
            ProgressiveLevel = progressiveLevel,
            TailZeroRunBias = 0.15f,
            ChromaDownsamplingSharpness = 0.25f,
            MaxCacheBytes = 0,
            MaxDegreeOfParallelism = 1
        }, cancellationToken: TestContext.Current.CancellationToken);

        byte[] parallel = JpegliEncoder.Encode(rgb, width, height, PixelLayout.Rgb24, new JpegliEncoderOptions
        {
            Distance = 1.5f,
            Subsampling = ChromaSubsampling.Quarter,
            ProgressiveLevel = progressiveLevel,
            TailZeroRunBias = 0.15f,
            ChromaDownsamplingSharpness = 0.25f,
            MaxDegreeOfParallelism = 2
        }, cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(baselineCached, uncached);
        Assert.Equal(baselineCached, parallel);

        byte[] defaultEncoded = JpegliEncoder.Encode(rgb, width, height, PixelLayout.Rgb24, new JpegliEncoderOptions
        {
            Distance = 1.5f,
            Subsampling = ChromaSubsampling.Quarter,
            ProgressiveLevel = progressiveLevel,
            MaxDegreeOfParallelism = 1
        }, cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotEqual(defaultEncoded, baselineCached);
    }

    [Fact]
    public void ForwardDctAndScalarQuantizationMatchWithTailZeroRunBias()
    {
        ushort[] rawTable = new ushort[64];
        for (int i = 0; i < 64; i++)
        {
            rawTable[i] = (ushort)(4 + (i % 9));
        }

        var prepared = Quantization.PreparedTable.Create(rawTable, component: 0, distance: 1.5f,
            adaptiveQuantization: false, tailZeroRunBias: 0.2f);
        float[] samples = new float[64];
        for (int i = 0; i < 64; i++)
        {
            samples[i] = (((i * 29) ^ (i * 7)) % 120) - 60f;
        }

        short[] fusedZigZag = new short[64];
        ForwardDct.TransformQuantizeBlock(samples, prepared, fusedZigZag, adaptiveStrength: 0f, zigZagOrdered: true);

        float[] coefficients = new float[64];
        short[] naturalOrder = new short[64];
        short[] reorderedZigZag = new short[64];
        ForwardDct.TransformScalar(samples, coefficients);
        Quantization.QuantizePrepared(coefficients, prepared, naturalOrder, adaptiveStrength: 0f);
        ZigZagOrdering.Reorder(naturalOrder, reorderedZigZag);

        Assert.Equal(reorderedZigZag, fusedZigZag);
    }
}
