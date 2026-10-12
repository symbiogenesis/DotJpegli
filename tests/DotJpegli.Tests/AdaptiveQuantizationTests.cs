using DotJpegli.Internal;
using Xunit;

namespace DotJpegli.Tests;

public sealed class AdaptiveQuantizationTests
{
    // Fixtures come from an independent portable C++ full-image scalar probe,
    // compiled with MSVC /O2 /fp:strict. The probe uses the pinned upstream
    // adaptive_quantization.cc expressions at google/jpegli revision
    // 031a0077f5799a6041004267fc12b956c1f52a20, materializes full pre/fuzzy maps,
    // and sorts each complete 3x3 neighborhood with std::sort. It does not use
    // this implementation's bounded MCU buffers. SIMD/native encoder parity
    // is outside the scope of these scalar spatial-analysis fixtures.
    public static TheoryData<int, int, float[]> MixedFixtures => new()
    {
        { 1, 4, [0.252179861f, 2.069556f, 1.5491066f, 0.262044787f, 2.70128822f,
            1.68419814f, 0.168017507f, 2.51836777f, 1.48105407f] },
        { 2, 4, [0.252179861f, 2.069556f, 1.5491066f, 0.262044787f, 2.70128822f,
            1.68419814f, 0.167868495f, 2.51248717f, 1.47358441f, 0.152432799f,
            1.19804502f, 0.567130327f] },
        { 2, 18, [0.284705877f, 1.62088084f, 1.28636456f, 0.293411613f, 1.98611236f,
            1.37639713f, 0.20951736f, 1.88124657f, 1.23499632f, 0.195597887f,
            1.04097724f, 0.553556561f] }
    };

    [Theory]
    [MemberData(nameof(MixedFixtures))]
    public void MixedSpatialFieldMatchesPinnedScalarCppFixtures(int sampling, int quant, float[] expected)
    {
        byte[] pixels = CreateMixedImage(19, 23, 23);
        var analysis = new AdaptiveQuantization(19, 23, sampling);
        Assert.Equal(3, analysis.BlocksWide);
        float[] actual = Analyze(analysis, pixels, 19, 23, 23, sampling, quant);
        AssertClose(expected, actual);
        // The smooth left region and textured right region must receive
        // materially different strengths within the same MCU row.
        Assert.True(actual[1] > actual[0] + 1f);

        // The same instance supports the encoder's second pass and arbitrary
        // row order without carrying the previous row's spatial state.
        float[] row = new float[3 * sampling];
        for (int top = ((23 - 1) / (sampling * 8)) * sampling * 8; top >= 0; top -= sampling * 8)
        {
            analysis.ComputeRow(pixels, 23, 1, PixelLayout.Gray8, top, quant, row, TestContext.Current.CancellationToken);
            AssertClose(expected.AsSpan(top / 8 * 3, row.Length), row);
        }
    }

    [Theory]
    [InlineData(0, 0f)]
    [InlineData(128, 0.0467278957f)]
    [InlineData(255, 0f)]
    public void ConstantLumaMatchesPinnedScalarCppFixture(byte sample, float expected)
    {
        byte[] pixels = Enumerable.Repeat(sample, 16 * 16).ToArray();
        var analysis = new AdaptiveQuantization(16, 16, 2);
        float[] actual = new float[4];
        analysis.ComputeRow(pixels, 16, 1, PixelLayout.Gray8, 0, 4, actual, TestContext.Current.CancellationToken);
        Assert.All(actual, value => Assert.InRange(MathF.Abs(value - expected), 0, 0.00001f));
    }

    [Fact]
    public void TinyImageAndNarrowPaddedImageMatchPinnedScalarCppFixtures()
    {
        var tiny = new AdaptiveQuantization(1, 1, 1);
        float[] result = new float[1];
        tiny.ComputeRow(new byte[] { 0 }, 1, 1, PixelLayout.Gray8, 0, 4, result, TestContext.Current.CancellationToken);
        Assert.Equal(0f, result[0]);
        var narrow = new AdaptiveQuantization(3, 9, 2);
        float[] row = new float[2];
        narrow.ComputeRow(CreateMixedImage(3, 9, 5), 5, 1, PixelLayout.Gray8, 0, 4, row, TestContext.Current.CancellationToken);
        AssertClose(new float[] { 0.0213568211f, 0.0213568211f }, row);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void EdgePaddingMatchesExplicitlyReplicatedImage(int sampling)
    {
        const int width = 19, height = 23, paddedWidth = 24;
        int paddedHeight = ((height + sampling * 8 - 1) / (sampling * 8)) * sampling * 8;
        byte[] source = CreateMixedImage(width, height, width);
        byte[] padded = new byte[paddedWidth * paddedHeight];
        for (int y = 0; y < paddedHeight; y++)
            for (int x = 0; x < paddedWidth; x++)
                padded[y * paddedWidth + x] = source[Math.Min(y, height - 1) * width + Math.Min(x, width - 1)];
        float[] actual = Analyze(new AdaptiveQuantization(width, height, sampling),
            source, width, height, width, sampling, 4);
        float[] explicitPadding = Analyze(new AdaptiveQuantization(paddedWidth, paddedHeight, sampling),
            padded, paddedWidth, paddedHeight, paddedWidth, sampling, 4);
        Assert.Equal(explicitPadding, actual);
    }

    [Theory]
    [InlineData(PixelLayout.Rgb24, 3)]
    [InlineData(PixelLayout.Bgr24, 3)]
    [InlineData(PixelLayout.Rgba32, 4)]
    [InlineData(PixelLayout.Bgra32, 4)]
    public void ColorLayoutsUseUnshiftedLumaAndIgnoreAlphaAndStridePadding(PixelLayout layout, int bytesPerPixel)
    {
        const int width = 8, height = 8;
        int stride = width * bytesPerPixel + 7;
        byte[] pixels = Enumerable.Repeat((byte)0xEC, stride * height).ToArray();
        bool bgr = layout is PixelLayout.Bgr24 or PixelLayout.Bgra32;
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int offset = y * stride + x * bytesPerPixel;
                pixels[offset] = bgr ? (byte)229 : (byte)31;
                pixels[offset + 1] = 107;
                pixels[offset + 2] = bgr ? (byte)31 : (byte)229;
                if (bytesPerPixel == 4) pixels[offset + 3] = (byte)(x * 7 + y * 13);
            }
        var analysis = new AdaptiveQuantization(width, height, 1);
        float[] actual = new float[1];
        analysis.ComputeRow(pixels, stride, bytesPerPixel, layout, 0, 4, actual, TestContext.Current.CancellationToken);
        AssertClose(new float[] { 0.0663145781f }, actual);
    }

    [Theory]
    [InlineData(65)]
    [InlineData(255)]
    public void CoarseQuantizationFullyDampensSpatialVariation(int quant)
    {
        float[] actual = Analyze(new AdaptiveQuantization(19, 23, 2),
            CreateMixedImage(19, 23, 19), 19, 23, 19, 2, quant);
        Assert.All(actual, value => Assert.InRange(MathF.Abs(value - 0.48632586f), 0, 0.00001f));
    }

    [Fact]
    public void CancellationPreservesOutputAndAllowsReuse()
    {
        var analysis = new AdaptiveQuantization(19, 23, 2);
        byte[] pixels = CreateMixedImage(19, 23, 19);
        float[] actual = Enumerable.Repeat(-123f, 6).ToArray();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Throws<OperationCanceledException>(() =>
            analysis.ComputeRow(pixels, 19, 1, PixelLayout.Gray8, 0, 4, actual, cancellation.Token));
        Assert.All(actual, value => Assert.Equal(-123f, value));
        analysis.ComputeRow(pixels, 19, 1, PixelLayout.Gray8, 0, 4, actual, TestContext.Current.CancellationToken);
        AssertClose(new float[] { 0.252179861f, 2.069556f, 1.5491066f,
            0.262044787f, 2.70128822f, 1.68419814f }, actual);
    }

    private static byte[] CreateMixedImage(int width, int height, int stride)
    {
        byte[] pixels = Enumerable.Repeat((byte)0xEC, stride * height).ToArray();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                pixels[y * stride + x] = (byte)(x < 8 ? (y < 12 ? 16 : x * 5 + y * 3)
                    : (x * 17 + y * 29 + x * y * 7) % 256);
        return pixels;
    }

    private static float[] Analyze(AdaptiveQuantization analysis, byte[] pixels, int width, int height,
        int stride, int sampling, int quant)
    {
        int rows = (height + sampling * 8 - 1) / (sampling * 8);
        float[] result = new float[analysis.BlocksWide * sampling * rows];
        for (int row = 0; row < rows; row++)
            analysis.ComputeRow(pixels, stride, 1, PixelLayout.Gray8, row * sampling * 8, quant,
                result.AsSpan(row * analysis.BlocksWide * sampling), TestContext.Current.CancellationToken);
        return result;
    }

    private static void AssertClose(ReadOnlySpan<float> expected, ReadOnlySpan<float> actual)
    {
        Assert.Equal(expected.Length, actual.Length);
        for (int i = 0; i < expected.Length; i++)
            Assert.InRange(MathF.Abs(expected[i] - actual[i]), 0, 0.00001f);
    }
}
