namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class EncoderCacheReuseTests
{
    [Theory]
    [InlineData(JpegColorSpace.Rgb, 0)]
    [InlineData(JpegColorSpace.Rgb, 1)]
    [InlineData(JpegColorSpace.Rgb, 2)]
    [InlineData(JpegColorSpace.Rgb, 3)]
    [InlineData(JpegColorSpace.Xyb, 0)]
    [InlineData(JpegColorSpace.Xyb, 1)]
    [InlineData(JpegColorSpace.Xyb, 2)]
    [InlineData(JpegColorSpace.Xyb, 3)]
    public void RgbAndXybCachesPreserveSharedHuffmanTablesAndScanBytes(JpegColorSpace colorSpace, int mode)
    {
        const int width = 33, height = 35;
        foreach (Array source in Sources(width, height))
        {
            foreach (int smoothing in new[] { 0, 37 })
            {
                JpegliEncoderOptions options = Options(colorSpace, mode) with { SmoothingFactor = smoothing };
                byte[] expected = Encode(source, width, height, options with { MaxCacheBytes = 0 });
                foreach (long limit in new[] { 8192L, EncoderCacheBudget.DefaultMaximumBytes })
                {
                    var diagnostics = new EncoderCacheDiagnostics();
                    byte[] actual = Encode(source, width, height,
                        options with { MaxCacheBytes = limit, CacheDiagnostics = diagnostics });
                    Assert.Equal(expected, actual);
                    Assert.True(diagnostics.CoefficientPeakBytes > 0);
                    Assert.InRange(diagnostics.PeakBytes, 1, limit);
                    Assert.Equal(0, diagnostics.CurrentBytes);
                }
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void PartialYCbCrCachesRetainZigzagOrderingAcrossScanModes(int mode)
    {
        const int width = 73, height = 47;
        byte[] source = TestImages.Gradient(width, height);
        JpegliEncoderOptions options = Options(JpegColorSpace.YCbCr, mode) with
        {
            SamplingFactors = new(new(3, 2), new(2, 1), new(1, 1)),
            SequentialInterleaveComponents = mode != 0
        };
        byte[] expected = Encode(source, width, height, options with { MaxCacheBytes = 0 });
        var diagnostics = new EncoderCacheDiagnostics();
        Assert.Equal(expected, Encode(source, width, height,
            options with { MaxCacheBytes = 8192, CacheDiagnostics = diagnostics }));
        Assert.True(diagnostics.CoefficientPeakBytes > 0);
        Assert.InRange(diagnostics.PeakBytes, 1, 8192);
        Assert.Equal(0, diagnostics.CurrentBytes);
    }

    [Theory]
    [InlineData(JpegColorSpace.Rgb)]
    [InlineData(JpegColorSpace.YCbCr)]
    [InlineData(JpegColorSpace.Grayscale)]
    [InlineData(JpegColorSpace.Xyb)]
    public void CachedSourceIccPreflightMatchesUncachedConversion(JpegColorSpace colorSpace)
    {
        const int width = 19, height = 17;
        foreach (Array source in Sources(width, height))
        {
            foreach (byte[] profile in new[] { JpegliIccProfiles.CreateLinearSrgbProfile(), JpegliIccProfiles.CreateDisplayP3Profile() })
            {
                JpegliEncoderOptions options = Options(colorSpace, 1) with { SourceIccProfile = profile };
                byte[] expected = Encode(source, width, height, options with { MaxCacheBytes = 0 });
                var diagnostics = new EncoderCacheDiagnostics();
                Assert.Equal(expected, Encode(source, width, height, options with { CacheDiagnostics = diagnostics }));
                Assert.True(diagnostics.ConvertedSamplePeakBytes > 0);
                Assert.Equal(0, diagnostics.CurrentBytes);
            }
        }
    }

    [Fact]
    public void CachedSourceIccPreflightReleasesItsLeaseWhenLaterValidationFails()
    {
        var diagnostics = new EncoderCacheDiagnostics();
        using var output = new MemoryStream();
        var options = new JpegliEncoderOptions
        {
            SourceIccProfile = JpegliIccProfiles.CreateLinearSrgbProfile(),
            ScanScript = [new([0], 0, 0)],
            CacheDiagnostics = diagnostics
        };
        _ = Assert.Throws<ArgumentException>(() => JpegliEncoder.Encode(output, TestImages.Gradient(19, 17),
            19, 17, options: options, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(0, output.Length);
        Assert.True(diagnostics.ConvertedSamplePeakBytes > 0);
        Assert.Equal(0, diagnostics.CurrentBytes);
    }

    [Theory]
    [InlineData(JpegColorSpace.Rgb)]
    [InlineData(JpegColorSpace.YCbCr)]
    [InlineData(JpegColorSpace.Grayscale)]
    [InlineData(JpegColorSpace.Xyb)]
    public void PartialSourceIccPreflightPreservesCompleteJpegBytes(JpegColorSpace colorSpace)
    {
        const int width = 65, height = 67;
        foreach (Array source in Sources(width, height))
        {
            JpegliEncoderOptions options = Options(colorSpace, 1) with
            {
                SourceIccProfile = JpegliIccProfiles.CreateLinearSrgbProfile()
            };
            byte[] expected = Encode(source, width, height, options with { MaxCacheBytes = 0 });
            var diagnostics = new EncoderCacheDiagnostics();
            Assert.Equal(expected, Encode(source, width, height, options with
            {
                MaxCacheBytes = colorSpace == JpegColorSpace.Grayscale ? 8192 : 32768,
                CacheDiagnostics = diagnostics
            }));
            Assert.InRange(diagnostics.ConvertedSampleCachedRows, 16, height - 1);
            Assert.Equal(height, diagnostics.ConvertedSampleTotalRows);
            Assert.Equal(0, diagnostics.CurrentBytes);
        }
    }

    private static JpegliEncoderOptions Options(JpegColorSpace colorSpace, int mode) => new()
    {
        ColorSpace = colorSpace,
        IccProfile = colorSpace == JpegColorSpace.Xyb ? JpegliIccProfiles.CreateXybProfile() : ReadOnlyMemory<byte>.Empty,
        Distance = 1.45f,
        RestartInterval = 2,
        ProgressiveLevel = mode == 1 ? 2 : null,
        Progressive = mode == 3,
        ProgressiveRefinementBits = mode == 3 ? 2 : 0,
        ScanScript = mode == 2 ? [new([2]), new([0, 1])] : null
    };

    private static Array[] Sources(int width, int height)
    {
        byte[] bytes = TestImages.Gradient(width, height);
        return [bytes, bytes.Select(value => (ushort)(value * 257)).ToArray(), bytes.Select(value => value / 255f).ToArray()];
    }

    private static byte[] Encode(Array source, int width, int height, JpegliEncoderOptions options) => source switch
    {
        byte[] bytes => JpegliEncoder.Encode(bytes, width, height, options: options, cancellationToken: TestContext.Current.CancellationToken),
        ushort[] words => JpegliEncoder.Encode(words, width, height, options: options, cancellationToken: TestContext.Current.CancellationToken),
        float[] floats => JpegliEncoder.Encode(floats, width, height, options: options, cancellationToken: TestContext.Current.CancellationToken),
        _ => throw new ArgumentException("Unsupported source fixture.", nameof(source))
    };
}
