using DotJpegli.Internal;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderCustomQuantizationTests
{
    private static readonly JpegSamplingFactors FullResolution = new(new(1, 1), new(1, 1), new(1, 1));

    // ITU-T T.81 frequency positions, independently specified in wire order.
    private static readonly int[] Zigzag =
    [
        0,1,8,16,9,2,3,10,17,24,32,25,18,11,4,5,
        12,19,26,33,40,48,41,34,27,20,13,6,7,14,21,28,
        35,42,49,56,57,50,43,36,29,22,15,23,30,37,44,51,
        58,59,52,45,38,31,39,46,53,60,61,54,47,55,62,63
    ];

    [Theory]
    [InlineData(JpegColorSpace.Grayscale, 1)]
    [InlineData(JpegColorSpace.YCbCr, 1)]
    [InlineData(JpegColorSpace.YCbCr, 2)]
    [InlineData(JpegColorSpace.YCbCr, 3)]
    [InlineData(JpegColorSpace.Rgb, 1)]
    [InlineData(JpegColorSpace.Rgb, 2)]
    [InlineData(JpegColorSpace.Rgb, 3)]
    [InlineData(JpegColorSpace.Xyb, 1)]
    [InlineData(JpegColorSpace.Xyb, 2)]
    [InlineData(JpegColorSpace.Xyb, 3)]
    public void TablesAreWrittenInZigzagOrderAndSelectedByOrderedOutputChannel(JpegColorSpace space, int count)
    {
        ushort[][] tables = Tables(count, distinctive: true);
        var options = Options(space) with { QuantizationTables = Memories(tables), AdaptiveQuantization = false };
        byte[] pixels = Enumerable.Range(0, 64 * Channels(space))
            .Select(index => (byte)(64 + index % Channels(space) * 48)).ToArray();
        JpegDocument document = Read(EncodeSource(pixels, 8, 8, space, options), options);
        int[] selectors = count == 1 ? Enumerable.Repeat(0, Channels(space)).ToArray()
            : count == 2 ? [0, 1, 1] : [0, 1, 2];
        AssertTables(document, tables, selectors);
        Assert.Equal(space is JpegColorSpace.Rgb or JpegColorSpace.Xyb ? new[] { (int)'R', 'G', 'B' }
            : Enumerable.Range(1, Channels(space)).ToArray(), document.Components.Keys);
        int channel = 0;
        foreach ((int id, Component component) in document.Components)
        {
            Block block = document.Blocks[new(id, 0, 0)];
            int expectedDc = (int)Math.Round(8d * (64 + 48 * channel++ - 128) / tables[component.Quantizer][0],
                MidpointRounding.AwayFromZero);
            Assert.Equal(expectedDc, block.Coefficients[0]);
            Assert.All(block.Coefficients[1..], value => Assert.Equal(0, value));
        }
    }

    [Theory]
    [InlineData(JpegColorSpace.Grayscale)]
    [InlineData(JpegColorSpace.YCbCr)]
    [InlineData(JpegColorSpace.Rgb)]
    [InlineData(JpegColorSpace.Xyb)]
    public void AllQuantizedFrequenciesMatchIndependentDoubleCosineDct(JpegColorSpace space)
    {
        ushort[][] tables = Tables(Channels(space));
        var options = Options(space) with { QuantizationTables = Memories(tables), AdaptiveQuantization = false };
        byte[] pixels = Pattern(8, 8, space == JpegColorSpace.Grayscale ? PixelLayout.Gray8 : PixelLayout.Rgb24);
        // A corner impulse breaks the pattern's exact half-integer AC coefficients.
        for (int inputChannel = 0; inputChannel < Channels(space); inputChannel++)
            pixels[inputChannel]++;
        JpegDocument document = Read(EncodeSource(pixels, 8, 8, space, options), options);
        int channel = 0, nonzeroAc = 0;
        foreach ((int id, Component component) in document.Components)
        {
            int[] coefficients = document.Blocks[new(id, 0, 0)].Coefficients;
            for (int wire = 0; wire < 64; wire++)
            {
                int natural = Zigzag[wire];
                double value = Dct(pixels, Channels(space), channel, natural % 8, natural / 8)
                    / tables[component.Quantizer][natural];
                double threshold = space == JpegColorSpace.YCbCr && wire != 0
                    ? new[] { 0.59082d, 0.58146d, 0.57988d }[channel] : 0d;
                // Keep the oracle away from float DCT rounding and suppression boundaries.
                if (threshold > 0d)
                    Assert.True(Math.Abs(Math.Abs(value) - threshold) > 0.0001d);
                if (wire != 0)
                    Assert.True(Math.Abs(Math.Abs(value) % 1d - 0.5d) > 0.0001d,
                        $"Channel {channel}, frequency {natural} is too close to an AC rounding boundary.");
                int expected = Math.Abs(value) < threshold ? 0 : (int)Math.Round(value,
                    wire == 0 ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven);
                Assert.Equal(expected, coefficients[wire]);
                if (wire != 0 && expected != 0)
                    nonzeroAc++;
            }
            channel++;
        }
        Assert.True(nonzeroAc > 20);
    }

    [Theory]
    [InlineData(JpegColorSpace.Grayscale, false)]
    [InlineData(JpegColorSpace.Grayscale, true)]
    [InlineData(JpegColorSpace.YCbCr, false)]
    [InlineData(JpegColorSpace.YCbCr, true)]
    [InlineData(JpegColorSpace.Rgb, false)]
    [InlineData(JpegColorSpace.Rgb, true)]
    [InlineData(JpegColorSpace.Xyb, false)]
    [InlineData(JpegColorSpace.Xyb, true)]
    public void ExplicitTablesOverrideQualityDistanceAndStandardTableConstruction(JpegColorSpace space, bool adaptive)
    {
        byte[] pixels = Pattern(19, 17, space == JpegColorSpace.Grayscale ? PixelLayout.Gray8 : PixelLayout.Rgb24);
        var options = Options(space) with
        {
            QuantizationTables = Memories(Tables(Channels(space))), AdaptiveQuantization = adaptive,
            Quality = 0, Distance = float.Epsilon, Progressive = true, ProgressiveRefinementBits = 2,
            RestartInterval = 2
        };
        byte[] expected = EncodeSource(pixels, 19, 17, space, options);
        Assert.Equal(expected, EncodeSource(pixels, 19, 17, space, options with
        {
            Quality = 100, Distance = float.MaxValue, UseStandardQuantizationTables = true
        }));
        Assert.Equal(expected, EncodeSource(pixels, 19, 17, space, options with { Quality = 50, Distance = null }));
    }

    [Theory]
    [InlineData(JpegColorSpace.Grayscale, 1, false)]
    [InlineData(JpegColorSpace.YCbCr, 1, true)]
    [InlineData(JpegColorSpace.YCbCr, 2, false)]
    [InlineData(JpegColorSpace.YCbCr, 3, true)]
    [InlineData(JpegColorSpace.Rgb, 2, true)]
    [InlineData(JpegColorSpace.Xyb, 3, true)]
    public void TypedSourcesScansHuffmanAndRestartsPreserveCustomTableCoefficients(
        JpegColorSpace space, int count, bool adaptive)
    {
        const int width = 33, height = 19;
        byte[] pixels = Pattern(width, height, space == JpegColorSpace.Grayscale ? PixelLayout.Gray8 : PixelLayout.Rgb24);
        var options = Options(space) with
        {
            QuantizationTables = Memories(Tables(count)), AdaptiveQuantization = adaptive,
            OptimizeHuffmanTables = false, SmoothingFactor = 37,
            SamplingFactors = space is JpegColorSpace.Grayscale or JpegColorSpace.Rgb ? FullResolution
                : new(new(2, 2), new(1, 1), new(1, 1))
        };
        byte[] baseline = EncodeSource(pixels, width, height, space, options);
        JpegDocument reference = Read(baseline, options);
        foreach (int kind in new[] { 0, 1, 2 })
        {
            Array typed = Source(pixels, kind);
            foreach ((bool progressive, int bits, bool optimize, int interval) in new[]
                { (false, 0, true, 1), (true, 0, false, 2), (true, 2, true, 1) })
            {
                var variant = options with
                {
                    Progressive = progressive, ProgressiveRefinementBits = bits,
                    OptimizeHuffmanTables = optimize, RestartInterval = interval
                };
                byte[] encoded = EncodeSource(typed, width, height, space, variant);
                AssertCoefficients(reference, Read(encoded, variant));
                if (space is JpegColorSpace.Grayscale or JpegColorSpace.YCbCr)
                {
                    ColorComponents channels = space == JpegColorSpace.Grayscale ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
                    Assert.Equal(ImageResult.FromMemory(baseline, channels).Data, ImageResult.FromMemory(encoded, channels).Data);
                }
            }
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AdaptiveChromaInferenceUsesActualSharedOrSeparateTableSelectors(bool progressive)
    {
        // Independent pinned quant.cc fixtures used by EncoderStandardQuantizationTests:
        // selectors [0,1,1] infer distance 1.79198956, while [0,1,2] infer 1.83893085.
        // Identical supplied Cb/Cr tables must therefore yield different Cr zero bias.
        const float strength = 0.0467278957f, sharedCrMultiplier = 0.847996712f, separateCrMultiplier = 0.866299152f;
        double target = 0.57988f + strength * (sharedCrMultiplier + separateCrMultiplier) / 2d;
        double amplitude = target * 5 / (4d * Math.Sqrt(2d));
        float[] pixels = new float[8 * 8 * 3];
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
            {
                int offset = (y * 8 + x) * 3;
                pixels[offset] = 128f / 255f;
                pixels[offset + 1] = pixels[offset + 2] =
                    (float)((128d + amplitude * Math.Cos(Math.PI * (2 * x + 1) / 16d)) / 255d);
            }
        // Annex K at pinned Jpegli distance 1.351, specified independently in natural order.
        ushort[] yTable =
        [
            4,3,3,4,7,11,14,17,3,3,4,5,7,16,17,15,
            4,4,4,7,11,16,19,16,4,5,6,8,14,24,22,17,
            5,6,10,16,19,30,29,21,7,10,15,18,23,29,31,26,
            14,18,22,24,29,34,33,28,20,26,26,27,31,28,29,28
        ];
        ushort[] chroma =
        [
            5,5,7,13,28,28,28,28,5,6,7,18,28,28,28,28,
            7,7,16,28,28,28,28,28,13,18,28,28,28,28,28,28,
            28,28,28,28,28,28,28,28,28,28,28,28,28,28,28,28,
            28,28,28,28,28,28,28,28,28,28,28,28,28,28,28,28
        ];
        var options = Options(JpegColorSpace.YCbCr) with
        {
            QuantizationTables = Memories([yTable, chroma]), Progressive = progressive,
            ProgressiveRefinementBits = progressive ? 2 : 0, RestartInterval = 1
        };
        JpegDocument shared = Read(EncodeSource(pixels, 8, 8, JpegColorSpace.YCbCr, options), options);
        Assert.Equal(0, shared.Blocks[new(2, 0, 0)].Coefficients[1]);
        Assert.Equal(1, shared.Blocks[new(3, 0, 0)].Coefficients[1]);
        var separateOptions = options with { QuantizationTables = Memories([yTable, chroma, chroma.ToArray()]) };
        JpegDocument separate = Read(EncodeSource(pixels, 8, 8, JpegColorSpace.YCbCr, separateOptions), separateOptions);
        Assert.Equal(0, separate.Blocks[new(3, 0, 0)].Coefficients[1]);
        var uniformOptions = options with { AdaptiveQuantization = false };
        JpegDocument uniform = Read(EncodeSource(pixels, 8, 8, JpegColorSpace.YCbCr, uniformOptions), uniformOptions);
        Assert.Equal(1, uniform.Blocks[new(2, 0, 0)].Coefficients[1]);
        Assert.Equal(1, uniform.Blocks[new(3, 0, 0)].Coefficients[1]);
    }

    [Fact]
    public void NullPreservesExistingDefaultTableSelectionAndBytes()
    {
        Assert.Null(new JpegliEncoderOptions().QuantizationTables);
        foreach (JpegColorSpace space in new[] { JpegColorSpace.Grayscale, JpegColorSpace.YCbCr, JpegColorSpace.Rgb, JpegColorSpace.Xyb })
        {
            byte[] pixels = Pattern(17, 19, space == JpegColorSpace.Grayscale ? PixelLayout.Gray8 : PixelLayout.Rgb24);
            var options = Options(space);
            byte[] expected = EncodeSource(pixels, 17, 19, space, options);
            Assert.Equal(expected, EncodeSource(pixels, 17, 19, space, options with { QuantizationTables = null }));
            Assert.Equal(space == JpegColorSpace.Grayscale ? new[] { 0 }
                : space == JpegColorSpace.Rgb ? [0, 0, 0] : [0, 1, 2],
                Read(expected, options).Components.Values.Select(component => component.Quantizer));
        }
    }

    [Theory]
    [InlineData(JpegColorSpace.Grayscale, false)]
    [InlineData(JpegColorSpace.YCbCr, false)]
    [InlineData(JpegColorSpace.YCbCr, true)]
    [InlineData(JpegColorSpace.Rgb, false)]
    [InlineData(JpegColorSpace.Xyb, false)]
    public void CopiesOfDefaultTablesPreserveFullEncodedBytes(JpegColorSpace space, bool standard)
    {
        byte[] pixels = Pattern(19, 17, space == JpegColorSpace.Grayscale ? PixelLayout.Gray8 : PixelLayout.Rgb24);
        foreach (bool adaptive in new[] { false, true })
        {
            var options = Options(space) with
            {
                UseStandardQuantizationTables = standard, AdaptiveQuantization = adaptive,
                Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
            };
            byte[] expected = EncodeSource(pixels, 19, 17, space, options);
            JpegDocument document = Read(expected, options);
            ushort[][] tables = document.Quantizers.OrderBy(pair => pair.Key).Select(pair =>
            {
                ushort[] natural = new ushort[64];
                for (int wire = 0; wire < 64; wire++)
                    natural[Zigzag[wire]] = pair.Value[wire];
                return natural;
            }).ToArray();
            Assert.Equal(expected, EncodeSource(pixels, 19, 17, space,
                options with { QuantizationTables = Memories(tables) }));
        }
    }

    [Fact]
    public void SingleSharedTableInferenceHasTheSameConstraintsAsTheLumaTableAlone()
    {
        ushort[] table = Tables(1)[0];
        float expected = Quantization.QuantValuesToDistance([table]);
        float shared = Quantization.QuantValuesToDistance([table, table, table], quantizationSelectors: [0, 0, 0]);
        Assert.Equal(expected, shared);
        Assert.NotEqual(shared, Quantization.QuantValuesToDistance([table, table, table]));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OutOfRangeFloatPreflightUsesCustomQuantizersBeforeWriting(bool progressive)
    {
        float[] pixels = Enumerable.Repeat(-0.5f, 64).ToArray();
        var coarseOptions = Options(JpegColorSpace.Grayscale) with
        {
            QuantizationTables = Memories([Enumerable.Repeat((ushort)4, 64).ToArray()]),
            Quality = 100, AllowOutOfRangeFloatSamples = true, AdaptiveQuantization = false,
            Progressive = progressive, ProgressiveRefinementBits = progressive ? 2 : 0
        };
        JpegDocument coarse = Read(EncodeSource(pixels, 8, 8, JpegColorSpace.Grayscale, coarseOptions), coarseOptions);
        Assert.Equal(-511, coarse.Blocks[new(1, 0, 0)].Coefficients[0]);
        var fineOptions = coarseOptions with
        {
            QuantizationTables = Memories([Enumerable.Repeat((ushort)1, 64).ToArray()]), Quality = 0
        };
        using var output = new MemoryStream();
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            EncodeSource(output, pixels, 8, 8, JpegColorSpace.Grayscale, fineOptions));
        Assert.Equal("pixels", exception.ParamName);
        Assert.Equal(0, output.Length);
    }

    [Theory]
    [InlineData(JpegColorSpace.Grayscale, 0)]
    [InlineData(JpegColorSpace.Grayscale, 2)]
    [InlineData(JpegColorSpace.Grayscale, 3)]
    [InlineData(JpegColorSpace.YCbCr, 0)]
    [InlineData(JpegColorSpace.YCbCr, 4)]
    [InlineData(JpegColorSpace.Rgb, 4)]
    [InlineData(JpegColorSpace.Xyb, 4)]
    public void InvalidTableCountIsRejectedBeforeAnyOutput(JpegColorSpace space, int count) =>
        AssertInvalidTables(space, Memories(Tables(count)));

    [Theory]
    [InlineData(0)]
    [InlineData(63)]
    [InlineData(65)]
    public void EveryTableMustContainExactly64Values(int length)
    {
        foreach (int table in new[] { 0, 1, 2 })
        {
            ReadOnlyMemory<ushort>[] tables = Memories(Tables(3));
            tables[table] = Enumerable.Repeat((ushort)1, length).ToArray();
            AssertInvalidTables(JpegColorSpace.YCbCr, tables);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(256)]
    [InlineData(65535)]
    public void EveryValueMustFitTheBaselineRange(int invalid)
    {
        foreach ((int table, int frequency) in new[] { (0, 0), (1, 31), (2, 63) })
        {
            ushort[][] tables = Tables(3);
            tables[table][frequency] = (ushort)invalid;
            AssertInvalidTables(JpegColorSpace.YCbCr, Memories(tables));
        }
    }

    [Fact]
    public void TableCountFollowsResolvedOutputRatherThanSourceChannels()
    {
        byte[] rgb = Pattern(8, 8, PixelLayout.Rgb24);
        var options = Options(JpegColorSpace.Grayscale) with { QuantizationTables = Memories(Tables(2)) };
        using var output = new MemoryStream();
        ArgumentException exception = Assert.Throws<ArgumentException>(() =>
            JpegliEncoder.Encode(output, rgb, 8, 8, PixelLayout.Rgb24, options, cancellationToken: TestContext.Current.CancellationToken));
        Assert.Equal(nameof(JpegliEncoderOptions.QuantizationTables), exception.ParamName);
        Assert.Equal(0, output.Length);
        var colorOptions = Options(JpegColorSpace.YCbCr) with { QuantizationTables = Memories(Tables(2)) };
        byte[] jpeg = JpegliEncoder.Encode(Pattern(8, 8, PixelLayout.Gray8), 8, 8, PixelLayout.Gray8, colorOptions,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.Equal(new[] { 0, 1, 1 }, Read(jpeg, colorOptions).Components.Values.Select(component => component.Quantizer));
    }

    [Fact]
    public void OverriddenQualityDistanceAndOtherOptionsRemainValidated()
    {
        var valid = Options(JpegColorSpace.YCbCr) with { QuantizationTables = Memories(Tables(2)) };
        (JpegliEncoderOptions Options, string Parameter)[] invalid =
        [
            (valid with { Quality = -1 }, nameof(JpegliEncoderOptions.Quality)),
            (valid with { Quality = 101 }, nameof(JpegliEncoderOptions.Quality)),
            (valid with { Quality = float.NaN }, nameof(JpegliEncoderOptions.Quality)),
            (valid with { Quality = float.PositiveInfinity }, nameof(JpegliEncoderOptions.Quality)),
            (valid with { Distance = 0 }, nameof(JpegliEncoderOptions.Distance)),
            (valid with { Distance = -1 }, nameof(JpegliEncoderOptions.Distance)),
            (valid with { Distance = float.NaN }, nameof(JpegliEncoderOptions.Distance)),
            (valid with { Distance = float.PositiveInfinity }, nameof(JpegliEncoderOptions.Distance)),
            (valid with { CicpTransferFunction = 256 }, nameof(JpegliEncoderOptions.CicpTransferFunction)),
            (valid with { Subsampling = (ChromaSubsampling)999 }, nameof(JpegliEncoderOptions.Subsampling))
        ];
        byte[] pixels = Pattern(8, 8, PixelLayout.Rgb24);
        foreach ((JpegliEncoderOptions options, string parameter) in invalid)
        {
            using var output = new MemoryStream();
            ArgumentException exception = Assert.ThrowsAny<ArgumentException>(() =>
                EncodeSource(output, pixels, 8, 8, JpegColorSpace.YCbCr, options));
            Assert.Equal(parameter, exception.ParamName);
            Assert.Equal(0, output.Length);
        }
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(2, true)]
    public void EveryTableAndTheListAreSnapshottedBeforeFirstStreamWrite(int kind, bool progressive)
    {
        const int width = 33, height = 19;
        Array pixels = Source(Pattern(width, height, PixelLayout.Rgb24), kind);
        ushort[][] tables = Tables(3);
        List<ReadOnlyMemory<ushort>> list = Memories(tables).ToList();
        var options = Options(JpegColorSpace.YCbCr) with
        {
            QuantizationTables = list, Progressive = progressive, ProgressiveRefinementBits = progressive ? 2 : 0,
            OptimizeHuffmanTables = progressive, RestartInterval = 2
        };
        byte[] expected = EncodeSource(pixels, width, height, JpegColorSpace.YCbCr, options);
        bool mutated = false;
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, () =>
        {
            if (mutated)
                return;
            mutated = true;
            foreach (ushort[] table in tables)
                Array.Fill(table, (ushort)0);
            list.Clear();
            list.Add(ReadOnlyMemory<ushort>.Empty);
        });
        EncodeSource(output, pixels, width, height, JpegColorSpace.YCbCr, options);
        Assert.True(mutated);
        Assert.False(output.Closed);
        Assert.Equal(expected, destination.ToArray());
    }

    private static double Dct(byte[] pixels, int channels, int channel, int u, int v)
    {
        double sum = 0d;
        for (int y = 0; y < 8; y++)
            for (int x = 0; x < 8; x++)
                sum += (pixels[(y * 8 + x) * channels + channel] - 128d)
                    * Math.Cos(Math.PI * (2 * x + 1) * u / 16d)
                    * Math.Cos(Math.PI * (2 * y + 1) * v / 16d);
        return sum * 0.25d * (u == 0 ? 1d / Math.Sqrt(2d) : 1d) * (v == 0 ? 1d / Math.Sqrt(2d) : 1d);
    }

    private static void AssertTables(JpegDocument document, ushort[][] tables, int[] selectors)
    {
        Assert.Equal(Enumerable.Range(0, tables.Length), document.Quantizers.Keys);
        Assert.Equal(selectors, document.Components.Values.Select(component => component.Quantizer));
        for (int table = 0; table < tables.Length; table++)
            Assert.Equal(Zigzag.Select(frequency => (byte)tables[table][frequency]).ToArray(), document.Quantizers[table]);
    }

    private static void AssertCoefficients(JpegDocument expected, JpegDocument actual)
    {
        Assert.Equal(expected.Components, actual.Components);
        Assert.Equal(expected.Quantizers.Keys, actual.Quantizers.Keys);
        foreach ((int selector, byte[] table) in expected.Quantizers)
            Assert.Equal(table, actual.Quantizers[selector]);
        Assert.Equal(expected.Blocks.Count, actual.Blocks.Count);
        foreach ((BlockKey key, Block block) in expected.Blocks.Where(pair => !pair.Value.IsDummy))
            Assert.Equal(block.Coefficients, actual.Blocks[key].Coefficients);
    }

    private static void AssertInvalidTables(JpegColorSpace space, ReadOnlyMemory<ushort>[] tables)
    {
        var options = Options(space) with { QuantizationTables = tables };
        byte[] bytes = Pattern(8, 8, space == JpegColorSpace.Grayscale ? PixelLayout.Gray8 : PixelLayout.Rgb24);
        for (int kind = 0; kind < 3; kind++)
        {
            using var output = new MemoryStream();
            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                EncodeSource(output, Source(bytes, kind), 8, 8, space, options));
            Assert.Equal(nameof(JpegliEncoderOptions.QuantizationTables), exception.ParamName);
            Assert.Equal(0, output.Length);
        }
    }

    private static ushort[][] Tables(int count, bool distinctive = false) => Enumerable.Range(0, count)
        .Select(table => Enumerable.Range(0, 64).Select(frequency =>
            distinctive ? (ushort)(frequency == 63 ? 255 : 1 + frequency * 3 + table * 11)
                : (ushort)(2 + (frequency * 3 + table * 5) % 11)).ToArray()).ToArray();

    private static ReadOnlyMemory<ushort>[] Memories(ushort[][] tables) =>
        tables.Select(table => new ReadOnlyMemory<ushort>(table)).ToArray();

    private static int Channels(JpegColorSpace space) => space == JpegColorSpace.Grayscale ? 1 : 3;

    private static JpegliEncoderOptions Options(JpegColorSpace space) => new()
    {
        ColorSpace = space, SamplingFactors = FullResolution,
        IccProfile = space == JpegColorSpace.Xyb ? new byte[] { 1, 2, 3, 4 } : ReadOnlyMemory<byte>.Empty
    };

    private static PixelLayout Layout(JpegColorSpace space) => space switch
    {
        JpegColorSpace.Grayscale => PixelLayout.Gray8,
        JpegColorSpace.YCbCr => PixelLayout.YCbCr24,
        JpegColorSpace.Xyb => PixelLayout.Xyb24,
        _ => PixelLayout.Rgb24
    };

    private static PixelComponentLayout ComponentLayout(JpegColorSpace space) => space switch
    {
        JpegColorSpace.Grayscale => PixelComponentLayout.Gray,
        JpegColorSpace.YCbCr => PixelComponentLayout.YCbCr,
        JpegColorSpace.Xyb => PixelComponentLayout.Xyb,
        _ => PixelComponentLayout.Rgb
    };

    private static Array Source(byte[] pixels, int kind) => kind switch
    {
        0 => pixels,
        1 => pixels.Select(value => (ushort)(value * 257)).ToArray(),
        _ => pixels.Select(value => value / 255f).ToArray()
    };

    private static byte[] EncodeSource(Array pixels, int width, int height, JpegColorSpace space, JpegliEncoderOptions options)
    {
        using var output = new MemoryStream();
        EncodeSource(output, pixels, width, height, space, options);
        return output.ToArray();
    }

    private static void EncodeSource(Stream output, Array pixels, int width, int height, JpegColorSpace space, JpegliEncoderOptions options)
    {
        CancellationToken cancellation = TestContext.Current.CancellationToken;
        if (pixels is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, Layout(space), options, cancellationToken: cancellation);
        else if (pixels is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, ComponentLayout(space), options, cancellationToken: cancellation);
        else
            JpegliEncoder.Encode(output, (float[])pixels, width, height, ComponentLayout(space), options, cancellationToken: cancellation);
    }

    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options) =>
        JpegDocument.Read(jpeg, options.Progressive, options.RestartInterval, options.ProgressiveRefinementBits);
}
