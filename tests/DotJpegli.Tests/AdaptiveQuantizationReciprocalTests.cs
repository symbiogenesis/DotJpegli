namespace DotJpegli.Tests;

using DotJpegli.Internal;
using Xunit;

public sealed class AdaptiveQuantizationReciprocalTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void ResearchScalarVectorAndBatchedFieldsPreserveStrengthBits(int kind)
    {
        foreach ((int width, int height) in new[] { (1, 9), (19, 17), (65, 35), (129, 19) })
        {
            byte[] bytes = Pattern(width, height, kind);
            PixelSource pixels = new(bytes, PixelLayout.Rgb24);
            foreach (int sampling in new[] { 1, 2, 4 })
            {
                var diagnostics = new AqRationalDiagnostics();
                using var original = new AdaptiveQuantization(width, height, sampling,
                    aqRationalKernel: KernelPreference.Original);
                using var research = new AdaptiveQuantization(width, height, sampling,
                    aqRationalKernel: KernelPreference.Optimized, aqRationalDiagnostics: diagnostics);
                foreach (int top in Enumerable.Range(0, ((height - 1) / (sampling * 8)) + 1).Select(row => row * sampling * 8))
                {
                    foreach (int quant in new[] { 4, 18, 65 })
                    {
                        foreach (int path in new[] { 0, 1, 2 })
                        {
                            float[] expected = Analyze(original, pixels, width * 3, top, quant, path, sampling);
                            float[] actual = Analyze(research, pixels, width * 3, top, quant, path, sampling);
                            AssertBits(expected, actual);
                        }
                    }
                }
                Assert.True(diagnostics.Calls > 0);
                foreach (AqRationalSite site in Enum.GetValues<AqRationalSite>())
                {
                    Assert.True(diagnostics.GetAccepted(site) + diagnostics.GetFallback(site) > 0);
                }
                Assert.True(diagnostics.VectorBatches > 0);
                Assert.InRange(diagnostics.FallbackVectorBatches, 0, diagnostics.VectorBatches);
                Assert.InRange(diagnostics.FallbackVector256Batches, 0, diagnostics.Vector256Batches);
            }
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void FloatFieldsRetainExceptionalAndSubnormalBits(int kind)
    {
        const int width = 65, height = 17;
        float[] source = new float[width * height * 3];
        var random = new Random(98713 + kind);
        for (int index = 0; index < source.Length; index++)
        {
            source[index] = kind switch
            {
                0 => (float)((random.NextDouble() * 4) - 2),
                1 => MathF.ScaleB((float)((random.NextDouble() * 2) - 1), 115),
                2 => BitConverter.Int32BitsToSingle((index % 2 == 0 ? int.MinValue : 0) | index),
                _ => (index % 7) switch
                {
                    0 => float.PositiveInfinity,
                    1 => float.NegativeInfinity,
                    2 => BitConverter.Int32BitsToSingle(0x7fc12345),
                    3 => float.MaxValue,
                    4 => -float.MaxValue,
                    5 => -0f,
                    _ => 0f
                }
            };
        }
        PixelSource pixels = new(source, PixelComponentLayout.Rgb);
        using var original = new AdaptiveQuantization(width, height, 2);
        var diagnostics = new AqRationalDiagnostics();
        using var research = new AdaptiveQuantization(width, height, 2,
            aqRationalKernel: KernelPreference.Optimized, aqRationalDiagnostics: diagnostics);
        foreach (int top in new[] { 0, 16, 0 })
        {
            foreach (int path in new[] { 0, 1, 2 })
            {
                AssertBits(Analyze(original, pixels, width * 3, top, 18, path, 2),
                    Analyze(research, pixels, width * 3, top, 18, path, 2));
            }
        }
        Assert.True(diagnostics.Calls > 0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OrdinaryPreferencesDoNotInvokeResearch(bool original)
    {
        var diagnostics = new AqRationalDiagnostics();
        _ = JpegliEncoder.Encode(Pattern(65, 19, 4), 65, 19,
            options: new()
            {
                AqRationalKernel = original ? KernelPreference.Original : KernelPreference.Automatic,
                AqRationalDiagnostics = diagnostics
            },
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(0, diagnostics.Calls);
        Assert.Equal(0, diagnostics.VectorBatches);
        Assert.Equal(0, diagnostics.Vector256Batches);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CompleteJpegsPreserveCustomScansTokenFallbackAndParallelReplay(int mode)
    {
        const int width = 67, height = 49;
        byte[] pixels = Pattern(width, height, 4);
        JpegliEncoderOptions options = Options(mode);
        foreach (long budget in new[] { 0L, 65599L, 64L * 1024 * 1024 })
        {
            foreach (int workers in new[] { 1, 2 })
            {
                JpegliEncoderOptions settings = options with { EntropyTokenBudgetBytes = budget, MaxDegreeOfParallelism = workers };
                byte[] expected = Encode(pixels, width, height, settings);
                var diagnostics = new AqRationalDiagnostics();
                byte[] actual = Encode(pixels, width, height, settings with
                {
                    AqRationalKernel = KernelPreference.Optimized,
                    AqRationalDiagnostics = diagnostics
                });
                Assert.Equal(expected, actual);
                Assert.True(diagnostics.Calls > 0);
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationAndDestinationFailurePreserveExactPrefixes(bool cancel)
    {
        const int width = 67, height = 49;
        byte[] pixels = Pattern(width, height, 4);
        JpegliEncoderOptions options = Options(3) with { EntropyTokenBudgetBytes = 0, MaxDegreeOfParallelism = 2 };
        byte[] expected = Encode(pixels, width, height, options);
        var prefixes = new List<byte[]>();
        var writes = new List<int[]>();
        foreach (KernelPreference preference in new[] { KernelPreference.Original, KernelPreference.Optimized })
        {
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            using var destination = new MemoryStream();
            var sizes = new List<int>();
            var failure = new IOException("Injected AQ research destination failure.");
            using var observed = new ObservedStream(destination, count =>
            {
                sizes.Add(count);
                if (destination.Length >= expected.Length / 2)
                {
                    if (cancel)
                    {
                        cancellation.Cancel();
                    }
                    else
                    {
                        throw failure;
                    }
                }
            });
            JpegliEncoderOptions settings = options with { AqRationalKernel = preference };
            if (cancel)
            {
                OperationCanceledException exception = Assert.Throws<OperationCanceledException>(() =>
                    JpegliEncoder.Encode(observed, pixels, width, height, options: settings, cancellationToken: cancellation.Token));
                Assert.Equal(cancellation.Token, exception.CancellationToken);
            }
            else
            {
                Assert.Same(failure, Assert.Throws<IOException>(() =>
                    JpegliEncoder.Encode(observed, pixels, width, height, options: settings, cancellationToken: cancellation.Token)));
            }
            Assert.True(observed.CanWrite);
            byte[] prefix = destination.ToArray();
            Assert.InRange(prefix.Length, expected.Length / 2, expected.Length - 1);
            Assert.Equal(expected[..prefix.Length], prefix);
            prefixes.Add(prefix);
            writes.Add(sizes.ToArray());
            Assert.Equal(expected, Encode(pixels, width, height, settings));
        }
        Assert.Equal(prefixes[0], prefixes[1]);
        Assert.Equal(writes[0], writes[1]);
    }

    [Fact]
    public void FloatEncodesPreserveBytesValidationAndOutputPrefixes()
    {
        const int width = 65, height = 19;
        float[] pixels = Pattern(width, height, 4).Select(value => value / 255f).ToArray();
        JpegliEncoderOptions options = Options(1);
        byte[] expected = JpegliEncoder.Encode(pixels, width, height, options: options,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(expected, JpegliEncoder.Encode(pixels, width, height,
            options: options with { AqRationalKernel = KernelPreference.Optimized },
            cancellationToken: TestContext.Current.CancellationToken));
        foreach (float invalid in new[] { -0.001f, 1.001f, float.NaN, float.NegativeInfinity, float.PositiveInfinity })
        {
            pixels[17] = invalid;
            using var original = new MemoryStream();
            using var research = new MemoryStream();
            ArgumentException reference = Assert.Throws<ArgumentException>(() => JpegliEncoder.Encode(original,
                pixels, width, height, options: options, cancellationToken: TestContext.Current.CancellationToken));
            ArgumentException actual = Assert.Throws<ArgumentException>(() => JpegliEncoder.Encode(research,
                pixels, width, height, options: options with { AqRationalKernel = KernelPreference.Optimized },
                cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(reference.ParamName, actual.ParamName);
            Assert.Equal(reference.Message, actual.Message);
            Assert.Equal(original.ToArray(), research.ToArray());
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SourceIccConversionPreservesBytesAcrossConcurrentPreferences(bool linear)
    {
        const int width = 65, height = 19;
        byte[] pixels = Pattern(width, height, 4);
        float[] floats = pixels.Select(value => MathF.Min(1f, (value / 255f) + 0.00013f)).ToArray();
        JpegliEncoderOptions options = Options(1) with
        {
            SourceIccProfile = linear ? JpegliIccProfiles.CreateLinearSrgbProfile() : JpegliIccProfiles.CreateDisplayP3Profile(),
            IccProfile = JpegliIccProfiles.CreateSrgbProfile()
        };
        byte[] expectedBytes = Encode(pixels, width, height, options);
        byte[] expectedFloats = JpegliEncoder.Encode(floats, width, height, options: options,
            cancellationToken: TestContext.Current.CancellationToken);
        Task<(byte[] Bytes, byte[] Floats, AqRationalDiagnostics ByteDiagnostics,
            AqRationalDiagnostics FloatDiagnostics, KernelPreference Preference)>[] encodes =
            new[] { KernelPreference.Original, KernelPreference.Optimized, KernelPreference.Automatic }
            .Select(preference => Task.Run(() =>
            {
                var byteDiagnostics = new AqRationalDiagnostics();
                var floatDiagnostics = new AqRationalDiagnostics();
                byte[] bytes = Encode(pixels, width, height, options with
                {
                    AqRationalKernel = preference,
                    AqRationalDiagnostics = byteDiagnostics
                });
                byte[] floatBytes = JpegliEncoder.Encode(floats, width, height, options: options with
                {
                    AqRationalKernel = preference,
                    AqRationalDiagnostics = floatDiagnostics
                }, cancellationToken: TestContext.Current.CancellationToken);
                return (Bytes: bytes, Floats: floatBytes, ByteDiagnostics: byteDiagnostics,
                    FloatDiagnostics: floatDiagnostics, Preference: preference);
            }, TestContext.Current.CancellationToken)).ToArray();
        foreach ((byte[] actualBytes, byte[] actualFloats, AqRationalDiagnostics byteDiagnostics,
            AqRationalDiagnostics floatDiagnostics, KernelPreference preference) in await Task.WhenAll(encodes))
        {
            Assert.Equal(expectedBytes, actualBytes);
            Assert.Equal(expectedFloats, actualFloats);
            if (preference == KernelPreference.Optimized)
            {
                Assert.True(byteDiagnostics.Calls > 0);
                Assert.True(floatDiagnostics.Calls > 0);
            }
            else
            {
                Assert.Equal(0, byteDiagnostics.Calls);
                Assert.Equal(0, floatDiagnostics.Calls);
            }
        }
    }

    private static float[] Analyze(AdaptiveQuantization analysis, PixelSource pixels, int stride,
        int top, int quant, int path, int sampling)
    {
        float[] values = new float[(analysis.BlocksWide * sampling) + 1];
        values[^1] = -123.25f;
        switch (path)
        {
            case 0:
                analysis.ComputeRowScalar(pixels, stride, top, quant, values, TestContext.Current.CancellationToken);
                break;
            case 1:
                analysis.ComputeRowVector(pixels, stride, top, quant, values, TestContext.Current.CancellationToken);
                break;
            default:
                analysis.ComputeRowBatched(pixels, stride, top, quant, values, TestContext.Current.CancellationToken);
                break;
        }
        Assert.Equal(-123.25f, values[^1]);
        return values;
    }

    private static void AssertBits(float[] expected, float[] actual)
        => Assert.Equal(expected.Select(BitConverter.SingleToInt32Bits), actual.Select(BitConverter.SingleToInt32Bits));

    private static byte[] Encode(byte[] pixels, int width, int height, JpegliEncoderOptions options)
        => JpegliEncoder.Encode(pixels, width, height, options: options, cancellationToken: TestContext.Current.CancellationToken);

    private static JpegliEncoderOptions Options(int mode)
    {
        var options = new JpegliEncoderOptions
        {
            Distance = mode % 2 == 0 ? 1 : 4,
            OptimizeHuffmanTables = true,
            AdaptiveQuantization = true,
            Subsampling = mode % 2 == 0 ? ChromaSubsampling.None : ChromaSubsampling.Quarter,
            RestartInterval = mode == 1 ? 0 : 3
        };
        return mode switch
        {
            0 => options,
            1 => options with { ProgressiveLevel = 2 },
            2 => options with { ScanScript = [new([2]), new([0, 1])], SmoothingFactor = 37 },
            _ => options with
            {
                ScanScript =
                [
                    new([2], 0, 0, 0, 1), new([0, 1], 0, 0, 0, 1),
                    new([0], 1, 31, 0, 1), new([0], 32, 63, 0, 1),
                    new([1], 1, 63, 0, 1), new([2], 1, 63, 0, 1),
                    new([0, 1, 2], 0, 0, 1, 0),
                    new([0], 1, 63, 1, 0), new([1], 1, 63, 1, 0), new([2], 1, 63, 1, 0)
                ]
            }
        };
    }

    private static byte[] Pattern(int width, int height, int kind)
    {
        byte[] pixels = new byte[width * height * 3];
        var random = new Random(741813 + kind);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int component = 0; component < 3; component++)
                {
                    pixels[(((y * width) + x) * 3) + component] = kind switch
                    {
                        0 => (byte)(x * 255 / Math.Max(1, width - 1)),
                        1 => (byte)(127 + ((x + y + component) % 3)),
                        2 => (byte)(128 + random.Next(-2, 3)),
                        3 => (byte)((x + y) % 2 == 0 ? 0 : 255),
                        4 => (byte)random.Next(256),
                        _ => (byte)(component * 63)
                    };
                }
            }
        }
        return pixels;
    }

    private sealed class ObservedStream(Stream inner, Action<int> onWrite) : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override void Flush() => inner.Flush();
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => this.Write(buffer.AsSpan(offset, count));
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            inner.Write(buffer);
            onWrite(buffer.Length);
        }
        public override void WriteByte(byte value)
        {
            inner.WriteByte(value);
            onWrite(1);
        }
    }
}
