using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderScanScriptTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void SequentialComponentGroupsPreserveRealCoefficientsAndPixels(bool optimize, bool asymmetric)
    {
        const int width = 145, height = 33;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions
        {
            Quality = 90, OptimizeHuffmanTables = optimize, SmoothingFactor = 37,
            SamplingFactors = asymmetric ? new(new(1, 2), new(2, 1), new(1, 1)) : null,
            RestartRows = 1
        };
        byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        foreach (JpegScan[] script in new JpegScan[][]
        {
            [new([2]), new([0, 1])],
            [new([1, 2]), new([0])],
            [new([0]), new([2]), new([1])]
        })
        {
            // The script selects sequential output even when Progressive is true.
            byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24,
                options with { Progressive = true, ScanScript = script });
            AssertRealParity(baseline, jpeg, options, script, progressive: false);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void RegroupedDcAndOverlappingRefinementBandsPreserveEveryRealCoefficient(bool optimize, bool rows)
    {
        const int width = 65, height = 81;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        JpegScan[] script = RegroupedProgressiveScript();
        var options = new JpegliEncoderOptions
        {
            Quality = 90, OptimizeHuffmanTables = optimize, SmoothingFactor = 37,
            SamplingFactors = new(new(1, 2), new(2, 1), new(1, 1)),
            RestartInterval = rows ? 0 : 2, RestartRows = rows ? 1 : 0
        };
        byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, options with { ScanScript = script });
        JpegDocument document = AssertRealParity(baseline, jpeg, options, script, progressive: true);
        Assert.Contains(document.Scans, scan => scan.RestartMarkers.Count > 8);
        // Component 0 has two blocks per MCU vertically; regrouping its separately
        // initialized DC into an interleaved refinement introduces padded blocks.
        Assert.Contains(document.Blocks, pair => pair.Value.IsDummy);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GrayscaleScriptMaySplitAndRejoinSpectralBands(bool optimize)
    {
        const int width = 17, height = 19;
        byte[] pixels = Pattern(width, height, PixelLayout.Gray8);
        JpegScan[] script =
        [
            new([0], 0, 0, 0, 1), new([0], 32, 63, 0, 1), new([0], 1, 31, 0, 1),
            new([0], 0, 0, 1, 0), new([0], 1, 63, 1, 0)
        ];
        var options = new JpegliEncoderOptions { Quality = 90, OptimizeHuffmanTables = optimize, RestartInterval = 1 };
        byte[] baseline = Encode(pixels, width, height, PixelLayout.Gray8, options);
        byte[] jpeg = Encode(pixels, width, height, PixelLayout.Gray8, options with { ScanScript = script });
        AssertRealParity(baseline, jpeg, options, script, progressive: true, grayscale: true);
    }

    [Theory]
    [InlineData(JpegColorSpace.Grayscale)]
    [InlineData(JpegColorSpace.YCbCr)]
    [InlineData(JpegColorSpace.Rgb)]
    [InlineData(JpegColorSpace.Xyb)]
    public void OutputComponentIndicesPreserveColorConversionAndCustomQuantizationTables(JpegColorSpace colorSpace)
    {
        const int width = 17, height = 19;
        bool grayscale = colorSpace == JpegColorSpace.Grayscale;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var quantizers = Enumerable.Range(0, grayscale ? 1 : 3)
            .Select(component => (ReadOnlyMemory<ushort>)Enumerable.Repeat((ushort)(8 + component * 3), 64).ToArray()).ToArray();
        var options = new JpegliEncoderOptions
        {
            ColorSpace = colorSpace, QuantizationTables = quantizers, RestartRows = 1,
            IccProfile = colorSpace == JpegColorSpace.Xyb ? JpegliIccProfiles.CreateXybProfile() : default
        };
        JpegScan[] script = grayscale
            ? [new([0], 0, 0), new([0], 1, 63)] : RegroupedProgressiveScript();
        byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24, options with { ScanScript = script });
        JpegDocument document = AssertRealParity(baseline, jpeg, options, script, true, grayscale);
        Assert.Equal(colorSpace is JpegColorSpace.Rgb or JpegColorSpace.Xyb ? [82, 71, 66]
            : grayscale ? [1] : new[] { 1, 2, 3 }, document.Components.Keys);
        Assert.Equal(quantizers.Length, document.Quantizers.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MaximumValidScriptAndTenBitDcRefinementPreserveCoefficients(bool optimize)
    {
        byte[] pixels = [17, 121, 234];
        var script = new List<JpegScan>();
        for (int component = 0; component < 3; component++) script.Add(new([component], 0, 0, 0, 10));
        for (int low = 9; low >= 0; low--)
            for (int component = 0; component < 3; component++) script.Add(new([component], 0, 0, low + 1, low));
        for (int component = 0; component < 2; component++)
        {
            int last = component == 0 ? 63 : 62;
            for (int coefficient = 1; coefficient <= last; coefficient++) script.Add(new([component], coefficient, coefficient, 0, 1));
            if (last == 62) script.Add(new([component], 63, 63));
            for (int coefficient = 1; coefficient <= last; coefficient++) script.Add(new([component], coefficient, coefficient, 1, 0));
        }
        script.Add(new([2], 1, 63));
        Assert.Equal(285, script.Count);
        var options = new JpegliEncoderOptions { OptimizeHuffmanTables = optimize };
        byte[] baseline = Encode(pixels, 1, 1, PixelLayout.Rgb24, options);
        byte[] jpeg = Encode(pixels, 1, 1, PixelLayout.Rgb24, options with { ScanScript = script });
        AssertRealParity(baseline, jpeg, options, script, true);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CancellationDuringALaterCustomScanLeavesTheStreamOpenWithPartialOutput(bool optimize)
    {
        const int width = 65, height = 49;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        JpegScan[] script = RegroupedProgressiveScript();
        var options = new JpegliEncoderOptions { ScanScript = script, OptimizeHuffmanTables = optimize, RestartRows = 1 };
        byte[] complete = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        JpegDocument document = JpegDocument.Read(complete, true, 0, expectedRestartRows: 1, expectedScanScript: script);
        int cancelAfter = document.Scans[12].DataStart + 1;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, () =>
        {
            if (destination.Length >= cancelAfter) cancellation.Cancel();
        });
        Assert.Throws<OperationCanceledException>(() => JpegliEncoder.Encode(output, pixels, width, height,
            PixelLayout.Rgb24, options, cancellationToken: cancellation.Token));
        Assert.False(output.Closed);
        Assert.InRange(destination.Length, cancelAfter, complete.Length - 1);
        Assert.Equal(complete[..(int)destination.Length], destination.ToArray());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void PaddedTypedInputsAndNonseekableStreamsUseTheSameCustomScript(int precision)
    {
        const int width = 33, height = 19, stride = width * 4 + 7;
        byte[] rgb = Pattern(width, height, PixelLayout.Rgb24);
        var source = new byte[width * height * 4];
        for (int pixel = 0; pixel < width * height; pixel++)
        {
            rgb.AsSpan(pixel * 3, 3).CopyTo(source.AsSpan(pixel * 4));
            source[pixel * 4 + 3] = 17;
        }
        var bytes = new byte[(height - 1) * stride + width * 4];
        for (int row = 0; row < height; row++)
            source.AsSpan(row * width * 4, width * 4).CopyTo(bytes.AsSpan(row * stride));
        ushort[] words = bytes.Select(value => (ushort)(value * 257)).ToArray();
        float[] floats = bytes.Select(value => value / 255f).ToArray();
        JpegScan[] script = RegroupedProgressiveScript();
        var options = new JpegliEncoderOptions
        {
            Quality = 90, OptimizeHuffmanTables = true, RestartRows = 1,
            SamplingFactors = new(new(2, 1), new(1, 1), new(1, 2)),
            Comment = "custom scan script"
        };
        byte[] EncodeSource(JpegliEncoderOptions selected) => precision switch
        {
            0 => JpegliEncoder.Encode(bytes, width, height, PixelLayout.Rgba32, selected, stride, TestContext.Current.CancellationToken),
            1 => JpegliEncoder.Encode(words, width, height, PixelComponentLayout.Rgba, selected, stride, TestContext.Current.CancellationToken),
            _ => JpegliEncoder.Encode(floats, width, height, PixelComponentLayout.Rgba, selected, stride, TestContext.Current.CancellationToken)
        };
        byte[] baseline = EncodeSource(options);
        JpegliEncoderOptions custom = options with { ScanScript = script };
        byte[] jpeg = EncodeSource(custom);
        AssertRealParity(baseline, jpeg, options, script, progressive: true);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        switch (precision)
        {
            case 0: JpegliEncoder.Encode(output, bytes, width, height, PixelLayout.Rgba32, custom, stride, TestContext.Current.CancellationToken); break;
            case 1: JpegliEncoder.Encode(output, words, width, height, PixelComponentLayout.Rgba, custom, stride, TestContext.Current.CancellationToken); break;
            default: JpegliEncoder.Encode(output, floats, width, height, PixelComponentLayout.Rgba, custom, stride, TestContext.Current.CancellationToken); break;
        }
        Assert.False(output.Closed);
        Assert.Equal(jpeg, destination.ToArray());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExplicitLegacyAndPresetScriptsPreserveExistingBytes(bool optimize)
    {
        const int width = 33, height = 19;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var options = new JpegliEncoderOptions { OptimizeHuffmanTables = optimize, RestartRows = 1 };
        Assert.Equal(Encode(pixels, width, height, PixelLayout.Rgb24, options),
            Encode(pixels, width, height, PixelLayout.Rgb24, options with { ScanScript = [new([0, 1, 2])] }));
        Assert.Equal(Encode(pixels, width, height, PixelLayout.Rgb24, options with { SequentialInterleaveComponents = false }),
            Encode(pixels, width, height, PixelLayout.Rgb24, options with { ScanScript = [new([0]), new([1]), new([2])] }));
        Assert.Equal(Encode(pixels, width, height, PixelLayout.Rgb24, options with { Progressive = true }),
            Encode(pixels, width, height, PixelLayout.Rgb24, options with
            {
                ScanScript = [new([0, 1, 2], 0, 0), new([0], 1, 63), new([1], 1, 63), new([2], 1, 63)]
            }));
        foreach (ChromaSubsampling sampling in new[] { ChromaSubsampling.None, ChromaSubsampling.Quarter })
            foreach (int level in new[] { 1, 2 })
            {
                JpegliEncoderOptions preset = options with { Subsampling = sampling, ProgressiveLevel = level };
                JpegScan[] script = PresetScript(level, sampling == ChromaSubsampling.None);
                Assert.Equal(Encode(pixels, width, height, PixelLayout.Rgb24, preset),
                    Encode(pixels, width, height, PixelLayout.Rgb24,
                        options with { Subsampling = sampling, ScanScript = script }));
            }
        Assert.Equal(Encode(pixels, width, height, PixelLayout.Rgb24, options),
            Encode(pixels, width, height, PixelLayout.Rgb24, options with { ScanScript = null }));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CallerMutationAfterTheFirstWriteCannotChangeTheScriptOrItsComponents(bool optimize)
    {
        const int width = 33, height = 19;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        var selectors = new List<int> { 0, 1 };
        var script = new List<JpegScan> { new([2]), new(selectors) };
        var options = new JpegliEncoderOptions { ScanScript = script, OptimizeHuffmanTables = optimize, RestartRows = 1 };
        byte[] expected = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        bool mutated = false;
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, () =>
        {
            if (mutated) return;
            mutated = true;
            selectors.Clear();
            selectors.Add(2);
            script.Clear();
            script.Add(new([99]));
        });
        JpegliEncoder.Encode(output, pixels, width, height, PixelLayout.Rgb24, options,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.True(mutated);
        Assert.False(output.Closed);
        Assert.Equal(expected, destination.ToArray());
    }

    public static IEnumerable<object[]> InvalidScripts()
    {
        yield return [Array.Empty<JpegScan>()];
        yield return [new JpegScan[] { null! }];
        yield return [new JpegScan[] { new(null!) }];
        yield return [new JpegScan[] { new([]) }];
        yield return [new JpegScan[] { new([0, 0, 1]) }];
        yield return [new JpegScan[] { new([1, 0, 2]) }];
        yield return [new JpegScan[] { new([-1, 0, 1]) }];
        yield return [new JpegScan[] { new([0, 1, 3]) }];
        yield return [new JpegScan[] { new([0, 1]) }];
        yield return [new JpegScan[] { new([0, 1, 2]), new([0]) }];
        yield return [new JpegScan[] { new([0, 1, 2], 0, 63, 0, 1) }];
        yield return [new JpegScan[] { new([0, 1, 2], 0, 0), new([0, 1], 1, 63), new([2], 1, 63) }];
        yield return [new JpegScan[] { new([0], 1, 63), new([0, 1, 2], 0, 0), new([1], 1, 63), new([2], 1, 63) }];
        yield return [new JpegScan[] { new([0, 1, 2], 0, 0), new([0], 1, 32), new([0], 32, 63), new([1], 1, 63), new([2], 1, 63) }];
        yield return [new JpegScan[] { new([0, 1, 2], 0, 0), new([0], 1, 63, 1, 0), new([1], 1, 63), new([2], 1, 63) }];
        yield return [new JpegScan[] { new([0, 1, 2], 0, 0), new([0], 1, 63, 0, 2), new([0], 1, 63, 2, 0), new([1], 1, 63), new([2], 1, 63) }];
        yield return [new JpegScan[] { new([0, 1, 2], 0, 0), new([0], 1, 63, 0, 1), new([1], 1, 63), new([2], 1, 63) }];
        yield return [new JpegScan[] { new([0, 1, 2], 0, 0), new([0], 1, 62), new([1], 1, 63), new([2], 1, 63) }];
        yield return [new JpegScan[] { new([0, 1, 2], 0, 0), new([0], 1, 63, 0, 11), new([1], 1, 63), new([2], 1, 63) }];
        yield return [new JpegScan[] { new([0, 1, 2], -1, 63) }];
        yield return [new JpegScan[] { new([0, 1, 2], 0, 64) }];
        yield return [Enumerable.Repeat(new JpegScan([0], 0, 0), 286).ToArray()];
        // Separate initial AC scans exceed the pinned limit of 252 AC contexts,
        // even though their per-coefficient history and final coverage are valid.
        var contexts = new List<JpegScan> { new([0, 1, 2], 0, 0) };
        for (int component = 0; component < 2; component++)
            for (int coefficient = 1; coefficient <= 63; coefficient++)
                contexts.Add(new([component], coefficient, coefficient, 0, 1));
        for (int component = 0; component < 2; component++)
            for (int first = 1; first <= 63; first++)
                contexts.Add(new([component], first, first, 1, 0));
        contexts.Add(new([2], 1, 63));
        yield return [contexts.ToArray()];
    }

    [Theory]
    [MemberData(nameof(InvalidScripts))]
    public void InvalidScriptsAreRejectedBeforeAnyOutput(IReadOnlyList<JpegScan> script)
    {
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination);
        ArgumentException exception = Assert.ThrowsAny<ArgumentException>(() => JpegliEncoder.Encode(
            output, new byte[3], 1, 1, options: new() { ScanScript = script },
            cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.ScanScript), exception.ParamName);
        Assert.Equal(0, destination.Length);
        Assert.False(output.Closed);
    }

    [Fact]
    public void ExplicitScriptRejectsConflictingScanSettingsAndAbsentOutputComponents()
    {
        JpegScan[] script = [new([0, 1, 2])];
        var options = new JpegliEncoderOptions { ScanScript = script };
        foreach (JpegliEncoderOptions invalid in new[]
        {
            options with { ProgressiveLevel = 0 }, options with { ProgressiveRefinementBits = 1 },
            options with { ProgressiveAcScanEnds = [63] }, options with { ProgressiveAcRefinementBits = [0] },
            options with { ProgressiveInterleaveDc = false }, options with { SequentialInterleaveComponents = false }
        })
        {
            using var output = new MemoryStream();
            ArgumentException exception = Assert.ThrowsAny<ArgumentException>(() =>
                JpegliEncoder.Encode(output, new byte[3], 1, 1, options: invalid,
                    cancellationToken: TestContext.Current.CancellationToken));
            Assert.Equal(nameof(JpegliEncoderOptions.ScanScript), exception.ParamName);
            Assert.Equal(0, output.Length);
        }
        using var grayscale = new MemoryStream();
        ArgumentException missing = Assert.ThrowsAny<ArgumentException>(() =>
            JpegliEncoder.Encode(grayscale, new byte[1], 1, 1, PixelLayout.Gray8, options,
                cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.ScanScript), missing.ParamName);
        Assert.Equal(0, grayscale.Length);
    }

    private static JpegScan[] RegroupedProgressiveScript() =>
    [
        new([2], 0, 0, 0, 2), new([0], 0, 0, 0, 2), new([1], 0, 0, 0, 1),
        new([2], 21, 63, 0, 2), new([1], 1, 63, 0, 1),
        new([0], 1, 7, 0, 2), new([0], 8, 63, 0, 2), new([2], 1, 20, 0, 2),
        new([0, 2], 0, 0, 2, 1), new([0, 1, 2], 0, 0, 1, 0),
        new([0], 1, 63, 2, 1), new([2], 1, 63, 2, 1),
        new([1], 1, 3, 1, 0), new([0], 1, 32, 1, 0), new([2], 1, 10, 1, 0),
        new([1], 4, 63, 1, 0), new([0], 33, 63, 1, 0),
        new([2], 11, 40, 1, 0), new([2], 41, 63, 1, 0)
    ];

    private static JpegScan[] PresetScript(int level, bool interleaveDc)
    {
        var script = new List<JpegScan>();
        if (interleaveDc) script.Add(new([0, 1, 2], 0, 0));
        else for (int component = 0; component < 3; component++) script.Add(new([component], 0, 0));
        if (level == 2)
            for (int component = 0; component < 3; component++) script.Add(new([component], 1, 2));
        for (int component = 0; component < 3; component++) script.Add(new([component], level == 1 ? 1 : 3, 63, 0, level));
        for (int low = level - 1; low >= 0; low--)
            for (int component = 0; component < 3; component++) script.Add(new([component], level == 1 ? 1 : 3, 63, low + 1, low));
        return script.ToArray();
    }

    private static JpegDocument AssertRealParity(byte[] baseline, byte[] jpeg, JpegliEncoderOptions options,
        IReadOnlyList<JpegScan> script, bool progressive, bool grayscale = false)
    {
        JpegDocument reference = JpegDocument.Read(baseline, false, options.RestartInterval,
            expectedRestartRows: options.RestartRows);
        JpegDocument actual = JpegDocument.Read(jpeg, progressive, options.RestartInterval,
            expectedRestartRows: options.RestartRows, expectedScanScript: script);
        Assert.Equal(reference.Width, actual.Width);
        Assert.Equal(reference.Height, actual.Height);
        Assert.Equal(reference.Components, actual.Components);
        foreach ((int selector, byte[] quantizer) in reference.Quantizers)
            Assert.Equal(quantizer, actual.Quantizers[selector]);
        var real = reference.Blocks.Where(pair => !pair.Value.IsDummy).ToArray();
        Assert.Equal(real.Length, actual.Blocks.Count(pair => !pair.Value.IsDummy));
        foreach ((BlockKey key, Block expected) in real)
        {
            Assert.True(actual.Blocks.TryGetValue(key, out Block? block));
            Assert.False(block.IsDummy);
            Assert.Equal(expected.Coefficients, block.Coefficients);
        }
        ColorComponents channels = grayscale ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
        Assert.Equal(ImageResult.FromMemory(baseline, channels).Data, ImageResult.FromMemory(jpeg, channels).Data);
        return actual;
    }
}
