using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderProgressionLevelTests
{
    public static TheoryData<PixelLayout, ChromaSubsampling, JpegColorSpace, JpegSamplingFactors?, bool> EncodingModes
    {
        get
        {
            var modes = new TheoryData<PixelLayout, ChromaSubsampling, JpegColorSpace, JpegSamplingFactors?, bool>();
            var uniform = new JpegSamplingFactors(new(2, 1), new(2, 1), new(2, 1));
            var asymmetric = new JpegSamplingFactors(new(1, 1), new(2, 1), new(1, 1));
            var fullResolution = new JpegSamplingFactors(new(1, 1), new(1, 1), new(1, 1));
            foreach (bool optimize in new[] { false, true })
            {
                foreach (ChromaSubsampling sampling in Enum.GetValues<ChromaSubsampling>())
                    modes.Add(PixelLayout.Rgb24, sampling, JpegColorSpace.Automatic, null, optimize);
                modes.Add(PixelLayout.Gray8, ChromaSubsampling.Quarter, JpegColorSpace.Automatic, asymmetric, optimize);
                modes.Add(PixelLayout.Rgb24, ChromaSubsampling.Quarter, JpegColorSpace.Grayscale, uniform, optimize);
                modes.Add(PixelLayout.Gray8, ChromaSubsampling.Quarter, JpegColorSpace.YCbCr, null, optimize);
                modes.Add(PixelLayout.Rgb24, ChromaSubsampling.Quarter, JpegColorSpace.Rgb, null, optimize);
                modes.Add(PixelLayout.Rgb24, ChromaSubsampling.None, JpegColorSpace.Rgb, uniform, optimize);
                modes.Add(PixelLayout.Rgb24, ChromaSubsampling.None, JpegColorSpace.YCbCr, asymmetric, optimize);
                modes.Add(PixelLayout.YCbCr24, ChromaSubsampling.Quarter, JpegColorSpace.Automatic, null, optimize);
                modes.Add(PixelLayout.Xyb24, ChromaSubsampling.None, JpegColorSpace.Automatic, null, optimize);
                modes.Add(PixelLayout.Rgb24, ChromaSubsampling.Quarter, JpegColorSpace.Xyb, fullResolution, optimize);
            }
            return modes;
        }
    }

    [Theory]
    [MemberData(nameof(EncodingModes))]
    public void PresetsUsePinnedUpstreamDescriptorsAndPreserveBaselineCoefficientsAndPixels(
        PixelLayout layout, ChromaSubsampling subsampling, JpegColorSpace colorSpace,
        JpegSamplingFactors? sampling, bool optimize)
    {
        // Odd edges produce padding, and restart interval 1 wraps RST7 to RST0.
        const int width = 145, height = 17;
        byte[] pixels = Pattern(width, height, layout);
        var options = new JpegliEncoderOptions
        {
            Quality = 90, Subsampling = subsampling, SamplingFactors = sampling,
            ColorSpace = colorSpace, OptimizeHuffmanTables = optimize, RestartInterval = 1,
            IccProfile = layout == PixelLayout.Xyb24 || colorSpace == JpegColorSpace.Xyb
                ? JpegliIccProfiles.CreateXybProfile() : ReadOnlyMemory<byte>.Empty
        };
        byte[] baseline = Encode(pixels, width, height, layout, options);
        JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 1);
        // Read the effective output factors from SOF; source layout and an ignored
        // Subsampling setting cannot determine whether DC scans interleave.
        bool interleave = reference.Components.Values.All(component => component.H == 1 && component.V == 1);
        foreach (int level in new[] { 1, 2 })
        {
            byte[] jpeg = Encode(pixels, width, height, layout, options with { ProgressiveLevel = level });
            JpegDocument actual = ReadPreset(jpeg, level, options.RestartInterval, interleave);
            AssertParity(baseline, reference, jpeg, actual);
            Assert.Equal(jpeg, Encode(pixels, width, height, layout,
                options with { ProgressiveLevel = level, Progressive = true }));
            // These schedules are transcribed from encode.cc SetDefaultScanScript
            // at pinned google/jpegli revision 031a007, not an encoder helper.
            JpegliEncoderOptions explicitScript = options with
            {
                Progressive = true, ProgressiveInterleaveDc = interleave,
                ProgressiveAcScanEnds = level == 1 ? [63] : [2, 63],
                ProgressiveAcRefinementBits = level == 1 ? [1] : [0, 2]
            };
            Assert.Equal(Encode(pixels, width, height, layout, explicitScript), jpeg);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void LevelZeroForcesBaselineAndLevelsAboveTwoAliasLevelTwo(bool optimize)
    {
        foreach (PixelLayout layout in new[] { PixelLayout.Gray8, PixelLayout.Rgb24 })
            foreach (ChromaSubsampling sampling in new[] { ChromaSubsampling.None, ChromaSubsampling.Quarter })
                foreach ((int width, int height) in new[] { (1, 1), (1, 33), (33, 1), (17, 17) })
                {
                    byte[] pixels = Pattern(width, height, layout);
                    var options = new JpegliEncoderOptions
                    {
                        Subsampling = sampling, OptimizeHuffmanTables = optimize, RestartInterval = 2
                    };
                    byte[] baseline = Encode(pixels, width, height, layout, options);
                    foreach (bool progressive in new[] { false, true })
                        Assert.Equal(baseline, Encode(pixels, width, height, layout,
                            options with { ProgressiveLevel = 0, Progressive = progressive }));
                    byte[] expected = Encode(pixels, width, height, layout, options with { ProgressiveLevel = 2 });
                    foreach (int level in new[] { 3, 10, int.MaxValue })
                        Assert.Equal(expected, Encode(pixels, width, height, layout,
                            options with { ProgressiveLevel = level }));
                    JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 2);
                    bool interleave = layout == PixelLayout.Gray8 || sampling == ChromaSubsampling.None;
                    foreach (int level in new[] { 1, 2 })
                    {
                        byte[] jpeg = Encode(pixels, width, height, layout, options with { ProgressiveLevel = level });
                        AssertParity(baseline, reference, jpeg, ReadPreset(jpeg, level, 2, interleave));
                    }
                }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void ByteUshortAndFloatOverloadsHonorPresetsAndElementStride(int level)
    {
        const int width = 19, height = 17, stride = width * 4 + 13;
        byte[] compact = Pattern(width, height, PixelLayout.Rgb24);
        byte[] bytes = new byte[(height - 1) * stride + width * 4 + 17];
        ushort[] words = Enumerable.Repeat(ushort.MaxValue, bytes.Length).ToArray();
        float[] floats = Enumerable.Repeat(float.NaN, bytes.Length).ToArray();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                for (int channel = 0; channel < 3; channel++)
                {
                    byte value = compact[(y * width + x) * 3 + channel];
                    int offset = y * stride + x * 4 + 2 - channel;
                    bytes[offset] = value;
                    words[offset] = (ushort)(value * 257);
                    floats[offset] = value / 255f;
                }
        var options = new JpegliEncoderOptions { ProgressiveLevel = level, Progressive = true, RestartInterval = 2 };
        byte[] expected = Encode(compact, width, height, PixelLayout.Rgb24, options);
        foreach (Array source in new Array[] { bytes, words, floats })
        {
            byte[] actual = EncodeSource(source, width, height, options, stride);
            Assert.Equal(expected, actual);
            using var destination = new MemoryStream();
            using var output = new WriteOnlyStream(destination);
            EncodeSource(output, source, width, height, options, stride, TestContext.Current.CancellationToken);
            Assert.False(output.Closed);
            Assert.Equal(expected, destination.ToArray());
            ReadPreset(actual, level, 2, interleave: false);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void HighPrecisionSamplesPreserveBaselineCoefficientsAndDecodedPixels(bool floatingPoint)
    {
        const int width = 19, height = 17;
        ushort[] words = Enumerable.Range(0, width * height * 3)
            .Select(index => (ushort)(4000 + (977 * index + 59 * (index / 3)) % 55000)).ToArray();
        Array source = floatingPoint ? words.Select(value => value / 65535f).ToArray() : words;
        var options = new JpegliEncoderOptions { Quality = 90, RestartInterval = 2, SmoothingFactor = 37 };
        byte[] baseline = EncodeSource(source, width, height, options, 0, rgba: false);
        JpegDocument reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 2);
        foreach (int level in new[] { 0, 1, 2 })
        {
            byte[] jpeg = EncodeSource(source, width, height, options with { ProgressiveLevel = level }, 0, rgba: false);
            AssertParity(baseline, reference, jpeg, ReadPreset(jpeg, level, 2, interleave: false));
            using var destination = new MemoryStream();
            using var output = new WriteOnlyStream(destination);
            EncodeSource(output, source, width, height, options with { ProgressiveLevel = level }, 0,
                TestContext.Current.CancellationToken, rgba: false);
            Assert.Equal(jpeg, destination.ToArray());
            Assert.False(output.Closed);
        }
    }

    [Theory]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void CancellationDuringRefinementLeavesMatchingPartialOutputAndStreamOpen(int level, bool optimize)
    {
        const int width = 65, height = 33;
        byte[] bytes = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions
        {
            ProgressiveLevel = level, RestartInterval = 2, OptimizeHuffmanTables = optimize
        };
        byte[] complete = Encode(bytes, width, height, PixelLayout.Rgb24, options);
        JpegDocument document = ReadPreset(complete, level, 2, interleave: false);
        int cancelAfter = document.Scans.First(scan => scan.High != 0).DataStart + 1;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, () =>
        {
            if (destination.Length >= cancelAfter)
                cancellation.Cancel();
        });
        OperationCanceledException error = Assert.Throws<OperationCanceledException>(() =>
            JpegliEncoder.Encode(output, bytes, width, height, options: options, cancellationToken: cancellation.Token));
        Assert.Equal(cancellation.Token, error.CancellationToken);
        Assert.False(output.Closed);
        Assert.InRange(destination.Length, cancelAfter, complete.Length - 1);
        Assert.Equal(complete[..(int)destination.Length], destination.ToArray());
    }

    [Fact]
    public void NullLevelPreservesEveryExistingScriptAndPrecision()
    {
        Assert.Null(new JpegliEncoderOptions().ProgressiveLevel);
        byte[] bytes = Pattern(19, 17, PixelLayout.Rgb24);
        ushort[] words = bytes.Select(value => (ushort)(value * 257)).ToArray();
        float[] floats = bytes.Select(value => value / 255f).ToArray();
        foreach (bool optimize in new[] { false, true })
        {
            var scripts = new[]
            {
                new JpegliEncoderOptions { OptimizeHuffmanTables = optimize, RestartInterval = 2 },
                new JpegliEncoderOptions { OptimizeHuffmanTables = optimize, Progressive = true, RestartInterval = 2 },
                new JpegliEncoderOptions
                {
                    OptimizeHuffmanTables = optimize, Progressive = true, ProgressiveRefinementBits = 2,
                    ProgressiveInterleaveDc = false, ProgressiveAcScanEnds = [2, 63],
                    ProgressiveAcRefinementBits = [0, 2], RestartInterval = 2
                }
            };
            foreach (JpegliEncoderOptions options in scripts)
                foreach (Array source in new Array[] { bytes, words, floats })
                    Assert.Equal(EncodeSource(source, 19, 17, options, 0, rgba: false),
                        EncodeSource(source, 19, 17, options with { ProgressiveLevel = null }, 0, rgba: false));
        }
    }

    [Fact]
    public void NegativeLevelsAndExplicitScriptsAreRejectedBeforeOutputForEveryOverload()
    {
        AssertInvalidLevel(new JpegliEncoderOptions { ProgressiveLevel = -1 }, outOfRange: true);
        AssertInvalidLevel(new JpegliEncoderOptions { ProgressiveLevel = int.MinValue }, outOfRange: true);
        foreach (int level in new[] { 0, 1, 2, int.MaxValue })
            foreach (bool progressive in new[] { false, true })
            {
                var options = new JpegliEncoderOptions { ProgressiveLevel = level, Progressive = progressive };
                foreach (int[] list in new[] { Array.Empty<int>(), new[] { 63 }, new[] { -1, 999 } })
                {
                    AssertInvalidLevel(options with { ProgressiveAcScanEnds = list });
                    AssertInvalidLevel(options with { ProgressiveAcRefinementBits = list });
                }
                foreach (int bits in new[] { -1, 1, 11 })
                    AssertInvalidLevel(options with { ProgressiveRefinementBits = bits });
                AssertInvalidLevel(options with { ProgressiveInterleaveDc = false });
            }
    }

    [Fact]
    public void PresetsRetainUnrelatedOptionValidationAndHonorPreCancellation()
    {
        foreach (int level in new[] { 0, 1, 2 })
        {
            var options = new JpegliEncoderOptions { ProgressiveLevel = level };
            foreach ((JpegliEncoderOptions invalid, string parameter) in new[]
            {
                (options with { Quality = float.NaN }, nameof(JpegliEncoderOptions.Quality)),
                (options with { RestartInterval = 65536 }, nameof(JpegliEncoderOptions.RestartInterval)),
                (options with { SamplingFactors = new(new(0, 1), new(1, 1), new(1, 1)) }, nameof(JpegliEncoderOptions.SamplingFactors))
            })
            {
                using var destination = new MemoryStream();
                using var output = new WriteOnlyStream(destination);
                ArgumentException error = Assert.ThrowsAny<ArgumentException>(() =>
                    JpegliEncoder.Encode(output, new byte[3], 1, 1, options: invalid,
                        cancellationToken: TestContext.Current.CancellationToken));
                Assert.Equal(parameter, error.ParamName);
                Assert.Equal(0, destination.Length);
                Assert.False(output.Closed);
            }
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            foreach (Array source in new Array[] { new byte[3], new ushort[3], new float[3] })
            {
                using var destination = new MemoryStream();
                using var output = new WriteOnlyStream(destination);
                OperationCanceledException error = Assert.Throws<OperationCanceledException>(() =>
                    EncodeSource(output, source, 1, 1, options, 0, cancellation.Token, rgba: false));
                Assert.Equal(cancellation.Token, error.CancellationToken);
                Assert.Equal(0, destination.Length);
                Assert.False(output.Closed);
            }
        }
    }

    private static JpegDocument ReadPreset(byte[] jpeg, int level, int interval, bool interleave) =>
        JpegDocument.Read(jpeg, progressive: level != 0, expectedInterval: interval,
            expectedRefinementBits: 0, expectedAcScanEnds: level == 1 ? [63] : [2, 63],
            expectedAcRefinementBits: level == 1 ? [1] : [0, 2], expectedInterleaveDc: interleave);

    private static void AssertParity(byte[] baseline, JpegDocument reference, byte[] jpeg, JpegDocument actual)
    {
        Assert.Equal(reference.Width, actual.Width);
        Assert.Equal(reference.Height, actual.Height);
        Assert.Equal(reference.Components, actual.Components);
        foreach ((int selector, byte[] quantizer) in reference.Quantizers)
            Assert.Equal(quantizer, actual.Quantizers[selector]);
        Assert.Equal(reference.Blocks.Count(pair => !pair.Value.IsDummy), actual.Blocks.Count(pair => !pair.Value.IsDummy));
        foreach ((BlockKey key, Block block) in reference.Blocks.Where(pair => !pair.Value.IsDummy))
            Assert.Equal(block.Coefficients, actual.Blocks[key].Coefficients);
        ColorComponents channels = reference.Components.Count == 1 ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        Assert.Equal(ImageResult.FromMemory(baseline, channels).Data, ImageResult.FromMemory(jpeg, channels).Data);
    }

    private static byte[] EncodeSource(Array source, int width, int height, JpegliEncoderOptions options,
        int stride, bool rgba = true) => source switch
    {
        byte[] bytes => JpegliEncoder.Encode(bytes, width, height, rgba ? PixelLayout.Bgra32 : PixelLayout.Rgb24,
            options, stride, TestContext.Current.CancellationToken),
        ushort[] words => JpegliEncoder.Encode(words, width, height, rgba ? PixelComponentLayout.Bgra : PixelComponentLayout.Rgb,
            options, stride, TestContext.Current.CancellationToken),
        float[] floats => JpegliEncoder.Encode(floats, width, height, rgba ? PixelComponentLayout.Bgra : PixelComponentLayout.Rgb,
            options, stride, TestContext.Current.CancellationToken),
        _ => throw new ArgumentException("Unsupported test sample array.", nameof(source))
    };

    private static void EncodeSource(Stream output, Array source, int width, int height,
        JpegliEncoderOptions options, int stride, CancellationToken cancellationToken, bool rgba = true)
    {
        switch (source)
        {
            case byte[] bytes:
                JpegliEncoder.Encode(output, bytes, width, height, rgba ? PixelLayout.Bgra32 : PixelLayout.Rgb24,
                    options, stride, cancellationToken);
                break;
            case ushort[] words:
                JpegliEncoder.Encode(output, words, width, height, rgba ? PixelComponentLayout.Bgra : PixelComponentLayout.Rgb,
                    options, stride, cancellationToken);
                break;
            case float[] floats:
                JpegliEncoder.Encode(output, floats, width, height, rgba ? PixelComponentLayout.Bgra : PixelComponentLayout.Rgb,
                    options, stride, cancellationToken);
                break;
            default:
                throw new ArgumentException("Unsupported test sample array.", nameof(source));
        }
    }

    private static void AssertInvalidLevel(JpegliEncoderOptions options, bool outOfRange = false)
    {
        foreach (Array source in new Array[] { new byte[3], new ushort[3], new float[3] })
        {
            ArgumentException arrayError = Assert.ThrowsAny<ArgumentException>(() =>
                EncodeSource(source, 1, 1, options, 0, rgba: false));
            Assert.Equal(nameof(JpegliEncoderOptions.ProgressiveLevel), arrayError.ParamName);
            if (outOfRange)
                Assert.IsType<ArgumentOutOfRangeException>(arrayError);
            using var destination = new MemoryStream();
            using var output = new WriteOnlyStream(destination);
            ArgumentException streamError = Assert.ThrowsAny<ArgumentException>(() =>
                EncodeSource(output, source, 1, 1, options, 0, TestContext.Current.CancellationToken, rgba: false));
            Assert.Equal(nameof(JpegliEncoderOptions.ProgressiveLevel), streamError.ParamName);
            Assert.Equal(0, destination.Length);
            Assert.False(output.Closed);
        }
    }
}
