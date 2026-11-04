namespace DotJpegli.Tests;

using System.Security.Cryptography;
using DotJpegli.Internal;
using DotJpegli.Parity;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

public sealed class HuffmanOrderingSearchTests
{
    [Theory]
    [InlineData(ChromaSubsampling.None, 0)]
    [InlineData(ChromaSubsampling.Quarter, 0)]
    [InlineData(ChromaSubsampling.None, 2)]
    [InlineData(ChromaSubsampling.Quarter, 2)]
    public void EveryOrderingPreservesIndependentCoefficientsAndPixelsWithBothEmissionPaths(
        ChromaSubsampling sampling, int progression)
    {
        const int width = 33, height = 17;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        Assert.Equal(4, HuffmanOrderingSearch.Candidates.Count);
        foreach (int interval in new[] { 0, 3 })
        {
            JpegliEncoderOptions settings = HuffmanOrderingSearch.Candidates[0].CreateOptions(1, sampling, progression) with
            {
                RestartInterval = interval
            };
            byte[] control = Encode(pixels, width, height, PixelLayout.Rgb24, settings);
            JpegDocument reference = Read(control, progression, interval, sampling);
            byte[] decoded = Decode(control).Pixels;
            foreach (HuffmanOrderingSearch.Candidate candidate in HuffmanOrderingSearch.Candidates)
            {
                byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24,
                    settings with { HuffmanSymbolOrder = candidate.Order });
                Assert.Equal(jpeg, Encode(pixels, width, height, PixelLayout.Rgb24,
                    settings with { HuffmanSymbolOrder = candidate.Order, EntropyTokenBudgetBytes = 0 }));
                JpegDocument actual = Read(jpeg, progression, interval, sampling);
                Assert.Equal(reference.Blocks.Count, actual.Blocks.Count);
                foreach ((BlockKey key, Block block) in reference.Blocks)
                {
                    Assert.Equal(block.Coefficients, actual.Blocks[key].Coefficients);
                }
                foreach ((int id, byte[] quantizer) in reference.Quantizers)
                {
                    Assert.Equal(quantizer, actual.Quantizers[id]);
                }
                Assert.Equal(decoded, Decode(jpeg).Pixels);
            }
        }
    }

    [Fact]
    public void CompleteFileMinimumKeepsControlAndFirstCandidateOnTies()
    {
        byte[] jpeg = Encode([16, 128, 240], 1, 1, PixelLayout.Rgb24,
            HuffmanOrderingSearch.Candidates[0].CreateOptions(1, ChromaSubsampling.None, 0));
        HuffmanOrderingSearch.SearchResult tied = HuffmanOrderingSearch.Run(_ => jpeg, Decode, 1, 1,
            TestContext.Current.CancellationToken);
        Assert.Same(HuffmanOrderingSearch.Candidates[0], tied.Winner);
        Assert.Same(jpeg, tied.Jpeg);
        int calls = 0;
        HuffmanOrderingSearch.SearchResult result = HuffmanOrderingSearch.Run(_ =>
            calls++ == 0 ? WithComment(jpeg) : jpeg, Decode, 1, 1, TestContext.Current.CancellationToken);
        Assert.Equal(4, calls);
        Assert.Same(HuffmanOrderingSearch.Candidates[1], result.Winner);
        Assert.Same(jpeg, result.Jpeg);
        Assert.Equal(4, result.EncodeCount);
        Assert.Equal(4, result.DecodeCount);
        Assert.Equal(result.Trials.Sum(trial => trial.ScanCount), result.TotalScanCount);
        Assert.All(result.Trials.Skip(1), trial =>
            Assert.Equal(Convert.ToHexString(SHA256.HashData(jpeg)), trial.Sha256));
        _ = Assert.Single(result.Trials.Select(trial => trial.DecodedPixelSha256).Distinct());
    }

    [Fact]
    public void ChangedPixelsAreRejectedEvenWhenDecoderReusesItsBuffer()
    {
        byte[] jpeg = Encode([16, 128, 240], 1, 1, PixelLayout.Rgb24,
            HuffmanOrderingSearch.Candidates[0].CreateOptions(1, ChromaSubsampling.None, 0));
        byte[] reused = new byte[3];
        int calls = 0;
        _ = Assert.Throws<InvalidDataException>(() => HuffmanOrderingSearch.Run(_ => jpeg, _ =>
        {
            reused[0] = (byte)calls++;
            return new(1, 1, reused);
        }, 1, 1, TestContext.Current.CancellationToken));
        Assert.Equal(2, calls);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        _ = Assert.Throws<OperationCanceledException>(() => HuffmanOrderingSearch.Run(
            _ => throw new InvalidOperationException(), Decode, 1, 1, cancellation.Token));
    }

    [Fact]
    public void NonOptimizedTablesRetainControlBytesForEveryOrdering()
    {
        byte[] pixels = Pattern(17, 9, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions { OptimizeHuffmanTables = false };
        byte[] control = Encode(pixels, 17, 9, PixelLayout.Rgb24, options);
        foreach (HuffmanSymbolOrder order in Enum.GetValues<HuffmanSymbolOrder>())
        {
            Assert.Equal(control, Encode(pixels, 17, 9, PixelLayout.Rgb24,
                options with { HuffmanSymbolOrder = order }));
        }
    }

    private static JpegDocument Read(byte[] jpeg, int progression, int interval, ChromaSubsampling sampling)
        => progression == 0 ? JpegDocument.Read(jpeg, false, interval)
        : JpegDocument.Read(jpeg, true, interval, expectedAcScanEnds: [2, 63],
            expectedAcRefinementBits: [0, 2], expectedInterleaveDc: sampling == ChromaSubsampling.None);

    private static PpmImage Decode(byte[] jpeg)
    {
        var image = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
        return new(image.Width, image.Height, image.Data);
    }

    private static byte[] WithComment(byte[] jpeg)
    {
        byte[] bytes = new byte[jpeg.Length + 5];
        jpeg.AsSpan(0, 2).CopyTo(bytes);
        bytes[2] = 0xff;
        bytes[3] = 0xfe;
        bytes[4] = 0;
        bytes[5] = 3;
        jpeg.AsSpan(2).CopyTo(bytes.AsSpan(7));
        return bytes;
    }
}
