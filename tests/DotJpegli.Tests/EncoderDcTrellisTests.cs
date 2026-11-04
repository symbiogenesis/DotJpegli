namespace DotJpegli.Tests;

using DotJpegli.Internal;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

public sealed class EncoderDcTrellisTests
{
    [Theory]
    [InlineData(ChromaSubsampling.None, 0)]
    [InlineData(ChromaSubsampling.Quarter, 3)]
    public void SearchChangesOnlyAdjacentDcValuesAndBothEmissionPathsUseTheSameCache(
        ChromaSubsampling sampling, int restart)
    {
        const int width = 37, height = 19;
        byte[] input = Pattern(width, height, PixelLayout.Rgb24);
        JpegliEncoderOptions control = Options(sampling) with { RestartInterval = restart };
        byte[] original = Encode(input, width, height, PixelLayout.Rgb24, control);
        JpegDocument reference = Read(original, restart);
        var diagnostics = new DcTrellisDiagnostics();
        JpegliEncoderOptions search = control with { DcTrellisLambda = 4096, DcTrellisDiagnostics = diagnostics };
        byte[] jpeg = Encode(input, width, height, PixelLayout.Rgb24, search);
        JpegDocument actual = Read(jpeg, restart);
        long changes = 0;
        foreach ((BlockKey key, Block block) in reference.Blocks)
        {
            int[] found = actual.Blocks[key].Coefficients;
            Assert.InRange(found[0] - block.Coefficients[0], -1, 1);
            Assert.Equal(block.Coefficients.Skip(1), found.Skip(1));
            changes += found[0] == block.Coefficients[0] ? 0 : 1;
        }
        Assert.Equal(3, diagnostics.Chains);
        Assert.Equal(reference.Blocks.Count, diagnostics.SourceTransformBlocks);
        Assert.Equal(changes, diagnostics.ChangedCoefficients);
        Assert.True(diagnostics.CandidateEvaluations >= diagnostics.SourceTransformBlocks);
        Assert.True(diagnostics.SelectedSquaredError + (4096 * diagnostics.SelectedBits)
            <= diagnostics.OriginalSquaredError + (4096 * diagnostics.OriginalBits));
        Assert.Equal(jpeg, Encode(input, width, height, PixelLayout.Rgb24,
            search with { EntropyTokenBudgetBytes = 0, DcTrellisDiagnostics = null }));
        var decoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
        Assert.Equal(width, decoded.Width);
        Assert.Equal(height, decoded.Height);
        Assert.Equal(original, Encode(input, width, height, PixelLayout.Rgb24, control with { DcTrellisLambda = 0 }));
    }

    [Fact]
    public void TemporarySourceTargetsAndBackpointersParticipateInTheUnchangedCap()
    {
        var sampling = JpegSamplingFactors.FromSubsampling(ChromaSubsampling.None);
        ushort[][] tables = Enumerable.Range(0, 3).Select(_ => Enumerable.Repeat((ushort)1, 64).ToArray()).ToArray();
        var cacheDiagnostics = new EncoderCacheDiagnostics();
        var budget = new EncoderCacheBudget(394, cacheDiagnostics);
        // Three 64-short cache blocks = 384 bytes; research peak scratch = 10 bytes.
        using (var image = QuantizedImage.Create(new PixelSource(new byte[192], PixelLayout.Rgb24),
            8, 8, 24, sampling, tables, 1, 0, null, null, TestContext.Current.CancellationToken,
            zigZagOrdered: true, maximumBytes: 394, cacheBudget: budget, dcTrellisLambda: 1))
        {
            Assert.NotNull(image);
            Assert.True(image.IsComplete);
            Assert.Equal(394, cacheDiagnostics.PeakBytes);
            Assert.Equal(384, cacheDiagnostics.CurrentBytes);
        }
        Assert.Equal(0, cacheDiagnostics.CurrentBytes);
        Assert.Null(QuantizedImage.Create(new PixelSource([], PixelLayout.Rgb24), 8, 8, 24, sampling,
            tables, 1, 0, null, null, TestContext.Current.CancellationToken, zigZagOrdered: true,
            maximumBytes: 393, dcTrellisLambda: 1));
    }

    [Fact]
    public void UnsupportedTraversalAndInvalidLambdasFailBeforeOutput()
    {
        JpegliEncoderOptions supported = Options(ChromaSubsampling.None) with { DcTrellisLambda = 1 };
        JpegliEncoderOptions[] invalid =
        [
            supported with { DcTrellisLambda = double.NaN },
            supported with { DcTrellisLambda = double.PositiveInfinity },
            supported with { DcTrellisLambda = -1 },
            supported with { SequentialInterleaveComponents = true },
            supported with { RestartRows = 1 },
            supported with { ProgressiveLevel = 2 },
            supported with { ForceBaseline = false },
            supported with { OptimizeHuffmanTables = false },
            supported with { AdaptiveQuantization = false },
            supported with { MaxDegreeOfParallelism = 2 }
        ];
        foreach (JpegliEncoderOptions options in invalid)
        {
            using var output = new MemoryStream();
            _ = Assert.ThrowsAny<ArgumentException>(() => JpegliEncoder.Encode(output, new byte[3], 1, 1,
                options: options, cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(0, output.Length);
        }
    }

    internal static JpegliEncoderOptions Options(ChromaSubsampling sampling) => new()
    {
        ColorSpace = JpegColorSpace.YCbCr,
        Distance = 1,
        Subsampling = sampling,
        ForceBaseline = true,
        ProgressiveLevel = 0,
        AdaptiveQuantization = true,
        OptimizeHuffmanTables = true,
        MaxDegreeOfParallelism = 1,
        SequentialInterleaveComponents = false
    };

    private static JpegDocument Read(byte[] jpeg, int restart)
        => JpegDocument.Read(jpeg, false, restart, expectedScanScript: [new([0]), new([1]), new([2])]);
}
