namespace DotJpegli.Tests;

using DotJpegli.Internal;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

public sealed class EncoderRateDistortionTests
{
    [Theory]
    [InlineData(ChromaSubsampling.None, 0)]
    [InlineData(ChromaSubsampling.Quarter, 2)]
    public void SearchPreservesDcAndLimitsAcChoicesAndReplaysIdenticalCachedCoefficients(
        ChromaSubsampling sampling, int restart)
    {
        const int width = 37, height = 19;
        byte[] input = Pattern(width, height, PixelLayout.Rgb24);
        JpegliEncoderOptions control = Options(sampling) with { RestartInterval = restart };
        byte[] original = Encode(input, width, height, control);
        var reference = JpegDocument.Read(original, progressive: false, expectedInterval: restart);
        var diagnostics = new RateDistortionDiagnostics();
        JpegliEncoderOptions search = control with { RateDistortionLambda = 4096, RateDistortionDiagnostics = diagnostics };
        byte[] jpeg = Encode(input, width, height, search);
        var actual = JpegDocument.Read(jpeg, progressive: false, expectedInterval: restart);
        int changed = 0;
        long blocks = 0;
        foreach ((BlockKey key, Block block) in reference.Blocks.Where(pair => !pair.Value.IsDummy))
        {
            blocks++;
            int[] found = actual.Blocks[key].Coefficients;
            Assert.Equal(block.Coefficients[0], found[0]);
            for (int coefficient = 1; coefficient < 64; coefficient++)
            {
                int before = block.Coefficients[coefficient], after = found[coefficient];
                Assert.True(after == before || after == before - Math.Sign(before) || after == 0);
                if (after != before)
                {
                    changed++;
                }
            }
        }
        Assert.True(changed > 0);
        Assert.Equal(blocks, diagnostics.Blocks);
        Assert.Equal(changed, diagnostics.ChangedCoefficients);
        Assert.True(diagnostics.CandidateEvaluations >= blocks * 63);
        Assert.Equal(jpeg, Encode(input, width, height, search with { RateDistortionDiagnostics = null }));
        // The uncached-token emission path must use the same searched coefficient
        // cache as the histogram pass, including restarts and padded dummy blocks.
        Assert.Equal(jpeg, Encode(input, width, height, search with
        {
            EntropyTokenBudgetBytes = 0,
            RateDistortionDiagnostics = new RateDistortionDiagnostics()
        }));
        var decoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
        Assert.Equal(width, decoded.Width);
        Assert.Equal(height, decoded.Height);
        Assert.Equal(original, Encode(input, width, height, control with { RateDistortionLambda = 0 }));
    }

    [Fact]
    public void UnsupportedPoliciesAndInvalidLambdasFailBeforeOutput()
    {
        JpegliEncoderOptions supported = Options(ChromaSubsampling.Quarter) with { RateDistortionLambda = 1 };
        JpegliEncoderOptions[] invalid =
        [
            supported with { RateDistortionLambda = double.NaN },
            supported with { RateDistortionLambda = double.PositiveInfinity },
            supported with { RateDistortionLambda = -1 },
            supported with { ProgressiveLevel = 1 },
            supported with { OptimizeHuffmanTables = false },
            supported with { AdaptiveQuantization = false },
            supported with { ForceBaseline = false },
            supported with { MaxDegreeOfParallelism = 2 },
            supported with { ColorSpace = JpegColorSpace.Rgb },
            supported with { Subsampling = ChromaSubsampling.HalfHorizontal },
            supported with { SequentialInterleaveComponents = false }
        ];
        foreach (JpegliEncoderOptions options in invalid)
        {
            using var output = new MemoryStream();
            _ = Assert.ThrowsAny<ArgumentException>(() => JpegliEncoder.Encode(output, new byte[3], 1, 1,
                options: options, cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(0, output.Length);
        }
    }

    [Fact]
    public void OversizedSearchCacheIsRejectedWithoutReadingPixels()
    {
        ushort[][] tables = [new ushort[64], new ushort[64], new ushort[64]];
        Assert.Null(QuantizedImage.Create(new PixelSource([], PixelLayout.Rgb24), 65535, 65535, 65535 * 3,
            JpegSamplingFactors.FromSubsampling(ChromaSubsampling.None), tables, 1f, 0,
            null, null, TestContext.Current.CancellationToken, zigZagOrdered: true, rateDistortionLambda: 1));
    }

    [Fact]
    public void PreCancellationWritesNoOutputAndDoesNotRecordSearchWork()
    {
        var diagnostics = new RateDistortionDiagnostics();
        using var output = new MemoryStream();
        _ = Assert.Throws<OperationCanceledException>(() => JpegliEncoder.Encode(output, new byte[3], 1, 1,
            options: Options(ChromaSubsampling.None) with { RateDistortionLambda = 1, RateDistortionDiagnostics = diagnostics },
            cancellationToken: new CancellationToken(true)));
        Assert.Equal(0, output.Length);
        Assert.Equal(0, diagnostics.Blocks);
    }

    private static JpegliEncoderOptions Options(ChromaSubsampling sampling) => new()
    {
        ColorSpace = JpegColorSpace.YCbCr,
        Distance = 1,
        Subsampling = sampling,
        ForceBaseline = true,
        ProgressiveLevel = 0,
        AdaptiveQuantization = true,
        OptimizeHuffmanTables = true,
        MaxDegreeOfParallelism = 1
    };

    private static byte[] Encode(byte[] input, int width, int height, JpegliEncoderOptions options)
        => JpegliEncoder.Encode(input, width, height, PixelLayout.Rgb24, options,
            cancellationToken: TestContext.Current.CancellationToken);
}
