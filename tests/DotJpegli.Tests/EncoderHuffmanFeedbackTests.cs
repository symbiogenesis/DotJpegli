namespace DotJpegli.Tests;

using DotJpegli.Internal;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

public sealed class EncoderHuffmanFeedbackTests
{
    [Theory]
    [InlineData(ChromaSubsampling.None, 0)]
    [InlineData(ChromaSubsampling.Quarter, 3)]
    public void OneActualTablePassPreservesDcAndUsesOnlyBoundedAcChoicesWithIdenticalReplay(
        ChromaSubsampling sampling, int restart)
    {
        const int width = 37, height = 19;
        byte[] input = Pattern(width, height, PixelLayout.Rgb24);
        JpegliEncoderOptions standard = EncoderDcTrellisTests.Options(sampling) with
        {
            RateDistortionLambda = 256,
            RestartInterval = restart
        };
        byte[] unchanged = Encode(input, width, height, PixelLayout.Rgb24, standard with { RateDistortionLambda = 0 });
        byte[] control = Encode(input, width, height, PixelLayout.Rgb24, standard);
        JpegDocument source = Read(unchanged, restart), reference = Read(control, restart);
        var diagnostics = new RateDistortionDiagnostics();
        JpegliEncoderOptions feedback = standard with
        {
            RateDistortionHuffmanFeedback = true,
            RateDistortionDiagnostics = diagnostics
        };
        byte[] jpeg = Encode(input, width, height, PixelLayout.Rgb24, feedback);
        JpegDocument actual = Read(jpeg, restart);
        long feedbackChanges = 0;
        foreach ((BlockKey key, Block block) in reference.Blocks)
        {
            int[] found = actual.Blocks[key].Coefficients;
            Assert.Equal(source.Blocks[key].Coefficients[0], block.Coefficients[0]);
            Assert.Equal(block.Coefficients[0], found[0]);
            for (int position = 1; position < 64; position++)
            {
                int original = source.Blocks[key].Coefficients[position], before = block.Coefficients[position];
                Assert.True(before == original || before == original - Math.Sign(original) || before == 0);
                Assert.True(found[position] == before || found[position] == before - Math.Sign(before) || found[position] == 0);
                feedbackChanges += found[position] == before ? 0 : 1;
            }
        }
        Assert.Equal(reference.Blocks.Count, diagnostics.Blocks);
        Assert.Equal(reference.Blocks.Count, diagnostics.FeedbackHistogramBlocks);
        Assert.Equal(reference.Blocks.Count, diagnostics.FeedbackBlocks);
        Assert.Equal(feedbackChanges, diagnostics.FeedbackChanges);
        Assert.NotNull(diagnostics.FeedbackAcCodeLengths);
        Assert.Equal(3, diagnostics.FeedbackAcCodeLengths.Length);
        Assert.All(diagnostics.FeedbackAcCodeLengths, lengths => Assert.Equal(256, lengths.Length));
        Dictionary<int, byte[]> emittedModel = ReadAcCodeLengths(control);
        foreach ((int component, byte[] lengths) in emittedModel)
        {
            Assert.Equal(lengths, diagnostics.FeedbackAcCodeLengths[component - 1]);
        }
        Assert.True(diagnostics.FeedbackSelectedSquaredError + (256 * diagnostics.FeedbackSelectedBits)
            <= diagnostics.FeedbackOriginalSquaredError + (256 * diagnostics.FeedbackOriginalBits) + 1e-6);
        Assert.Equal(jpeg, Encode(input, width, height, PixelLayout.Rgb24,
            feedback with { EntropyTokenBudgetBytes = 0, RateDistortionDiagnostics = null }));
        var decoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
        Assert.Equal(width, decoded.Width);
        Assert.Equal(height, decoded.Height);
    }

    [Fact]
    public void UnsupportedPoliciesFailBeforeOutput()
    {
        JpegliEncoderOptions supported = EncoderDcTrellisTests.Options(ChromaSubsampling.None) with
        {
            RateDistortionLambda = 1,
            RateDistortionHuffmanFeedback = true
        };
        JpegliEncoderOptions[] invalid =
        [
            supported with { RateDistortionLambda = 0 },
            supported with { SequentialInterleaveComponents = true },
            supported with { RestartRows = 1 },
            supported with { HuffmanTableLimit = 1 },
            supported with { ProgressiveLevel = 2 },
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

    private static JpegDocument Read(byte[] jpeg, int restart)
        => JpegDocument.Read(jpeg, false, restart, expectedScanScript: [new([0]), new([1]), new([2])]);

    // Read actual DHT/SOS bytes rather than using any encoder table helper.
    private static Dictionary<int, byte[]> ReadAcCodeLengths(byte[] jpeg)
    {
        var tables = new Dictionary<int, byte[]>();
        var components = new Dictionary<int, byte[]>();
        int position = 2;
        while (jpeg[position + 1] != 0xd9)
        {
            int marker = jpeg[position + 1];
            position += 2;
            int length = (jpeg[position] << 8) | jpeg[position + 1], end = position + length;
            if (marker == 0xc4)
            {
                int offset = position + 2;
                while (offset < end)
                {
                    int selector = jpeg[offset++];
                    byte[] counts = jpeg.AsSpan(offset, 16).ToArray(), lengths = new byte[256];
                    offset += 16;
                    for (int bits = 1; bits <= 16; bits++)
                    {
                        for (int symbol = 0; symbol < counts[bits - 1]; symbol++)
                        {
                            lengths[jpeg[offset++]] = (byte)bits;
                        }
                    }
                    tables[selector] = lengths;
                }
                Assert.Equal(end, offset);
            }
            if (marker == 0xda)
            {
                int count = jpeg[position + 2];
                for (int component = 0; component < count; component++)
                {
                    int id = jpeg[position + 3 + (component * 2)];
                    int selector = 0x10 | (jpeg[position + 4 + (component * 2)] & 15);
                    components[id] = tables[selector];
                }
                position = end;
                while (position < jpeg.Length)
                {
                    if (jpeg[position] != 0xff)
                    {
                        position++;
                    }
                    else if (jpeg[position + 1] is 0 or (>= 0xd0 and <= 0xd7))
                    {
                        position += 2;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                position = end;
            }
        }
        Assert.Equal(3, components.Count);
        return components;
    }
}
