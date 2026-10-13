using DotJpegli.Internal;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

namespace DotJpegli.Tests;

public sealed class EncoderExtendedQuantizationTests
{
    private static readonly JpegSamplingFactors FullResolution = new(new(1, 1), new(1, 1), new(1, 1));

    // ITU-T T.81 positions, independently specified in DQT/entropy wire order.
    private static readonly int[] Zigzag =
    [
        0,1,8,16,9,2,3,10,17,24,32,25,18,11,4,5,
        12,19,26,33,40,48,41,34,27,20,13,6,7,14,21,28,
        35,42,49,56,57,50,43,36,29,22,15,23,30,37,44,51,
        58,59,52,45,38,31,39,46,53,60,61,54,47,55,62,63
    ];

    // Annex K K.1/K.2, independently specified in natural order.
    private static readonly ushort[] Luma =
    [
        16,11,10,16,24,40,51,61,12,12,14,19,26,58,60,55,
        14,13,16,24,40,57,69,56,14,17,22,29,51,87,80,62,
        18,22,37,56,68,109,103,77,24,35,55,64,81,104,113,92,
        49,64,78,87,103,121,120,101,72,92,95,98,112,100,103,99
    ];
    private static readonly ushort[] Chroma =
    [
        17,18,24,47,99,99,99,99,18,21,26,66,99,99,99,99,
        24,26,56,99,99,99,99,99,47,66,99,99,99,99,99,99,
        99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,
        99,99,99,99,99,99,99,99,99,99,99,99,99,99,99,99
    ];

    [Theory]
    [InlineData(JpegColorSpace.Grayscale, false)]
    [InlineData(JpegColorSpace.YCbCr, false)]
    [InlineData(JpegColorSpace.YCbCr, true)]
    [InlineData(JpegColorSpace.Rgb, false)]
    [InlineData(JpegColorSpace.Xyb, false)]
    public void BaselineRemainsDefaultAndInRangeOptOutPreservesBytes(JpegColorSpace space, bool standard)
    {
        Assert.True(new JpegliEncoderOptions().ForceBaseline);
        byte[] pixels = Pattern(19, 17, Layout(space));
        var options = Options(space) with
        {
            UseStandardQuantizationTables = standard, Progressive = true,
            ProgressiveRefinementBits = 2, RestartInterval = 2
        };
        byte[] expected = Encode(pixels, 19, 17, space, options);
        Assert.Equal(expected, Encode(pixels, 19, 17, space, options with { ForceBaseline = true }));
        Header header = ReadHeader(expected);
        Assert.All(header.Precisions.Values, precision => Assert.Equal(0, precision));
        var inRange = options with { Distance = 0.5f };
        Assert.Equal(Encode(pixels, 19, 17, space, inRange),
            Encode(pixels, 19, 17, space, inRange with { ForceBaseline = false }));

        var coarse = options with { Distance = 30 };
        byte[] defaultCoarse = Encode(pixels, 19, 17, space, coarse);
        Assert.Equal(defaultCoarse, Encode(pixels, 19, 17, space, coarse with { ForceBaseline = true }));
        Assert.All(ReadHeader(defaultCoarse).Tables.Values,
            table => Assert.All(table, value => Assert.InRange(value, (ushort)1, (ushort)255)));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(255)]
    [InlineData(256)]
    [InlineData(32767)]
    public void QuantizerBoundaryControlsDqtPrecisionAndSequentialFrame(int quantizer)
    {
        ushort[] table = Enumerable.Repeat((ushort)quantizer, 64).ToArray();
        byte[] pixels = Enumerable.Repeat((byte)64, 64).ToArray();
        var options = Options(JpegColorSpace.Grayscale) with
        {
            ForceBaseline = false, QuantizationTables = Memories([table]), AdaptiveQuantization = false
        };
        foreach (bool progressive in new[] { false, true })
        {
            var mode = options with { Progressive = progressive, ProgressiveRefinementBits = progressive ? 2 : 0 };
            byte[] jpeg = Encode(pixels, 8, 8, JpegColorSpace.Grayscale, mode);
            Header header = ReadHeader(jpeg);
            Assert.Equal(progressive ? 0xc2 : quantizer > 255 ? 0xc1 : 0xc0, header.Frame);
            Assert.Equal(8, header.SamplePrecision);
            Assert.Equal(quantizer > 255 ? 1 : 0, Assert.Single(header.Precisions).Value);
            Assert.Equal(quantizer > 255 ? 131 : 67, header.DqtLength);
            Assert.Equal(table, Assert.Single(header.Tables).Value);
            Block block = Assert.Single(Read(jpeg, mode).Blocks).Value;
            Assert.Equal((int)Math.Round(-512d / quantizer, MidpointRounding.AwayFromZero), block.Coefficients[0]);
            Assert.All(block.Coefficients[1..], value => Assert.Equal(0, value));
        }
        if (quantizer <= 255)
            Assert.Equal(Encode(pixels, 8, 8, JpegColorSpace.Grayscale, options),
                Encode(pixels, 8, 8, JpegColorSpace.Grayscale, options with { ForceBaseline = true }));
    }

    [Theory]
    [InlineData(JpegColorSpace.YCbCr, 1)]
    [InlineData(JpegColorSpace.YCbCr, 2)]
    [InlineData(JpegColorSpace.YCbCr, 3)]
    [InlineData(JpegColorSpace.Rgb, 3)]
    [InlineData(JpegColorSpace.Xyb, 3)]
    public void MixedTablePrecisionsUseBigEndianWordsInZigzagOrder(JpegColorSpace space, int count)
    {
        ushort[][] tables = MixedTables(count);
        // A shared single table must itself be extended; multi-table cases retain
        // a byte luma table alongside one or two sixteen-bit tables.
        if (count == 1)
            tables[0][63] = 32767;
        var options = Options(space) with
        {
            ForceBaseline = false, QuantizationTables = Memories(tables), AdaptiveQuantization = false
        };
        byte[] pixels = Pattern(8, 8, Layout(space));
        foreach (bool progressive in new[] { false, true })
        {
            var mode = options with { Progressive = progressive, ProgressiveRefinementBits = progressive ? 2 : 0 };
            byte[] jpeg = Encode(pixels, 8, 8, space, mode);
            Header header = ReadHeader(jpeg);
            Assert.Equal(progressive ? 0xc2 : 0xc1, header.Frame);
            Assert.Equal(8, header.SamplePrecision);
            Assert.Equal(count == 1 ? new[] { 0, 0, 0 } : count == 2 ? [0, 1, 1] : [0, 1, 2], header.Selectors);
            Assert.Equal(Enumerable.Range(0, count), header.Tables.Keys);
            int expectedLength = 2;
            for (int selector = 0; selector < count; selector++)
            {
                bool wide = tables[selector].Any(value => value > 255);
                Assert.Equal(wide ? 1 : 0, header.Precisions[selector]);
                Assert.Equal(Zigzag.Select(index => tables[selector][index]).ToArray(), header.Tables[selector]);
                expectedLength += 1 + (wide ? 128 : 64);
            }
            Assert.Equal(expectedLength, header.DqtLength);
            Read(jpeg, mode);
        }
    }

    [Theory]
    [InlineData(JpegColorSpace.Grayscale)]
    [InlineData(JpegColorSpace.YCbCr)]
    [InlineData(JpegColorSpace.Rgb)]
    [InlineData(JpegColorSpace.Xyb)]
    public void ExtendedQuantizersMatchIndependentDcAndCompleteCosineDct(JpegColorSpace space)
    {
        int channels = Channels(space);
        ushort[][] tables = Enumerable.Range(0, channels).Select(channel => Enumerable.Range(0, 64)
            .Select(frequency => (ushort)(frequency == 0 ? 257 + channel : frequency == 63 ? 32767
                : frequency == 8 ? 256 : 2 + (frequency * 3 + channel * 5) % 11)).ToArray()).ToArray();
        byte[] pixels = Pattern(8, 8, Layout(space));
        for (int channel = 0; channel < channels; channel++)
            pixels[channel]++;
        var options = Options(space) with
        {
            ForceBaseline = false, QuantizationTables = Memories(tables), AdaptiveQuantization = false
        };
        byte[] jpeg = Encode(pixels, 8, 8, space, options);
        JpegDocument reference = Read(jpeg, options);
        int inputChannel = 0, nonzeroAc = 0;
        foreach ((int id, Component component) in reference.Components)
        {
            int[] coefficients = reference.Blocks[new(id, 0, 0)].Coefficients;
            for (int wire = 0; wire < 64; wire++)
            {
                int natural = Zigzag[wire];
                double value = Dct(pixels, channels, inputChannel, natural % 8, natural / 8)
                    / tables[component.Quantizer][natural];
                double threshold = space == JpegColorSpace.YCbCr && wire != 0
                    ? new[] { 0.59082d, 0.58146d, 0.57988d }[inputChannel] : 0d;
                Assert.True(Math.Abs(Math.Abs(value) % 1d - 0.5d) > 0.0001d);
                int expected = Math.Abs(value) < threshold ? 0 : (int)Math.Round(value,
                    wire == 0 ? MidpointRounding.AwayFromZero : MidpointRounding.ToEven);
                Assert.Equal(expected, coefficients[wire]);
                if (wire != 0 && expected != 0)
                    nonzeroAc++;
            }
            inputChannel++;
        }
        Assert.True(nonzeroAc > 20);
        var progressive = options with { Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 1 };
        byte[] refined = Encode(pixels, 8, 8, space, progressive);
        AssertCoefficients(reference, Read(refined, progressive));
        if (space is JpegColorSpace.Grayscale or JpegColorSpace.YCbCr)
        {
            ColorComponents decodedChannels = space == JpegColorSpace.Grayscale
                ? ColorComponents.Grey : ColorComponents.RedGreenBlue;
            Assert.Equal(ImageResult.FromMemory(jpeg, decodedChannels).Data,
                ImageResult.FromMemory(refined, decodedChannels).Data);
        }
    }

    [Theory]
    [InlineData(JpegColorSpace.Grayscale, false)]
    [InlineData(JpegColorSpace.Grayscale, true)]
    [InlineData(JpegColorSpace.YCbCr, false)]
    [InlineData(JpegColorSpace.YCbCr, true)]
    [InlineData(JpegColorSpace.Rgb, false)]
    [InlineData(JpegColorSpace.Xyb, false)]
    public void GeneratedCoarseTablesRetainValuesAbove255(JpegColorSpace space, bool standard)
    {
        byte[] pixels = Pattern(8, 8, Layout(space));
        var options = Options(space) with
        {
            ForceBaseline = false, Distance = 30, UseStandardQuantizationTables = standard,
            AdaptiveQuantization = false
        };
        byte[] jpeg = Encode(pixels, 8, 8, space, options);
        Header extended = ReadHeader(jpeg);
        Header baseline = ReadHeader(Encode(pixels, 8, 8, space, options with { ForceBaseline = true }));
        Assert.Equal(0xc1, extended.Frame);
        Assert.Equal(0xc0, baseline.Frame);
        Assert.Contains(extended.Tables.Values.SelectMany(table => table), value => value > 255);
        Assert.All(extended.Tables.Values, table => Assert.All(table, value => Assert.InRange(value, (ushort)1, (ushort)32767)));
        foreach ((int selector, ushort[] table) in extended.Tables)
            Assert.Equal(table.Select(value => (ushort)Math.Min(value, (ushort)255)), baseline.Tables[selector]);
        if (standard || space == JpegColorSpace.Rgb)
        {
            Assert.Equal(Zigzag.Select(index => (ushort)(Luma[index] * 50)), extended.Tables[0]);
            if (space == JpegColorSpace.YCbCr)
                Assert.Equal(Zigzag.Select(index => (ushort)(Chroma[index] * 50)), extended.Tables[1]);
        }
        Read(jpeg, options);
        var progressive = options with { Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 1 };
        AssertCoefficients(Read(jpeg, options), Read(Encode(pixels, 8, 8, space, progressive), progressive));
    }

    [Theory]
    [InlineData(JpegColorSpace.Grayscale, false)]
    [InlineData(JpegColorSpace.YCbCr, false)]
    [InlineData(JpegColorSpace.Xyb, false)]
    [InlineData(JpegColorSpace.YCbCr, true)]
    [InlineData(JpegColorSpace.Rgb, true)]
    public void ExtremeGeneratedScalesClampBeforeConversionAndFineTablesStayBaseline(JpegColorSpace space, bool standard)
    {
        byte[] pixels = Pattern(8, 8, Layout(space));
        var options = Options(space) with
        {
            ForceBaseline = false, Distance = standard ? MathF.BitDecrement(25f) : float.MaxValue,
            UseStandardQuantizationTables = standard, AdaptiveQuantization = false
        };
        byte[] jpeg = Encode(pixels, 8, 8, space, options);
        Header extreme = ReadHeader(jpeg);
        Assert.Equal(0xc1, extreme.Frame);
        Assert.All(extreme.Tables.Values, table => Assert.All(table, value => Assert.Equal((ushort)32767, value)));
        Assert.All(Read(jpeg, options).Blocks.Values, block => Assert.All(block.Coefficients, value => Assert.Equal(0, value)));
        var fine = options with { Distance = float.Epsilon };
        Header fineHeader = ReadHeader(Encode(pixels, 8, 8, space, fine));
        Assert.Equal(0xc0, fineHeader.Frame);
        Assert.All(fineHeader.Precisions.Values, precision => Assert.Equal(0, precision));
        Assert.All(fineHeader.Tables.Values, table => Assert.All(table, value => Assert.Equal((ushort)1, value)));
    }

    [Theory]
    [InlineData(1, 0.00750476215f)]
    [InlineData(255, 118.2346649f)]
    [InlineData(256, 118.6983261f)]
    [InlineData(32767, 8375.426758f)]
    public void AdaptiveDistanceInferenceUsesExtendedSaturationEndpoint(int quantizer, float expected)
    {
        // QuantValsToDistance fixtures independently compiled from pinned quant.cc,
        // revision 031a0077f5799a6041004267fc12b956c1f52a20, MSVC /O2 /fp:precise.
        // In extended mode 255 is an ordinary rounded value; only 32767 is saturated.
        ushort[][] tables = Enumerable.Range(0, 3)
            .Select(_ => Enumerable.Repeat((ushort)quantizer, 64).ToArray()).ToArray();
        float distance = Quantization.QuantValuesToDistance(tables, forceBaseline: false);
        Assert.InRange(distance, expected - 0.001f, expected + 0.001f);
        if (quantizer == 255)
        {
            float baseline = Quantization.QuantValuesToDistance(tables);
            Assert.InRange(baseline, 169.8403168f - 0.001f, 169.8403168f + 0.001f);
            Assert.True(distance < baseline);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExtendedYcbcrTablesMatchCompletePinnedCppFixtures(bool subsampled)
    {
        // Independently compiled SetQuantMatrices at distance 25, transfer 2,
        // force_baseline=false, using the pinned revision and flags above.
        ushort[][] expected = subsampled ?
        [
            [66,46,77,75,177,184,204,205,46,56,75,100,183,168,214,214,
             77,75,100,111,172,207,199,225,75,100,111,179,221,198,197,244,
             177,183,172,221,243,261,296,291,184,168,207,198,261,289,270,323,
             204,214,199,197,296,270,289,287,205,214,225,244,291,323,287,232],
            [61,55,86,94,185,257,315,518,55,73,83,141,269,275,588,390,
             86,83,134,195,255,478,1835,452,94,141,195,298,299,352,409,729,
             185,269,255,299,319,265,287,1040,257,275,478,352,265,534,788,260,
             315,588,1835,409,287,788,260,259,518,390,452,729,1040,260,259,2004],
            [63,38,68,58,142,132,157,169,38,52,65,74,131,116,147,153,
             68,65,84,74,114,137,141,160,58,74,74,112,144,112,122,170,
             142,131,114,144,112,171,130,164,132,116,137,112,171,147,169,248,
             157,147,141,122,130,169,268,245,169,153,160,170,164,248,245,2053]
        ] :
        [
            [54,37,64,61,145,151,167,168,37,46,62,82,150,138,175,176,
             64,62,82,91,141,170,163,184,61,82,91,147,181,162,162,200,
             145,150,141,181,199,214,242,239,151,138,170,162,214,237,222,265,
             167,175,163,162,242,222,237,235,168,176,184,200,239,265,235,190],
            [123,141,202,232,482,746,803,1265,141,193,195,412,720,724,1165,928,
             202,195,307,491,768,1043,4464,1063,232,412,491,810,698,846,845,1557,
             482,720,768,698,843,638,868,2222,746,724,1043,846,638,1362,2016,2935,
             803,1165,4464,845,868,2016,2667,3842,1265,928,1063,1557,2222,2935,3842,4878],
            [127,98,160,143,371,383,400,412,98,137,153,216,350,305,292,364,
             160,153,191,186,341,299,343,377,143,216,186,305,336,269,252,364,
             371,350,341,336,295,413,393,350,383,305,299,269,413,374,432,2794,
             400,292,343,252,393,432,2748,3625,412,364,377,364,350,2794,3625,4997]
        ];
        var options = Options(JpegColorSpace.YCbCr) with
        {
            ForceBaseline = false, Distance = 25,
            SamplingFactors = subsampled ? new(new(2, 2), new(1, 1), new(1, 1)) : FullResolution
        };
        Header header = ReadHeader(Encode(Pattern(17, 19, PixelLayout.YCbCr24), 17, 19, JpegColorSpace.YCbCr, options));
        for (int selector = 0; selector < expected.Length; selector++)
            Assert.Equal(Zigzag.Select(natural => expected[selector][natural]), header.Tables[selector]);
        float inferred = Quantization.QuantValuesToDistance(expected, forceBaseline: false);
        float expectedDistance = subsampled ? 30.49681473f : 25.00033569f;
        Assert.InRange(inferred, expectedDistance - 0.00005f, expectedDistance + 0.00005f);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OutOfRangeFloatPreflightUsesExtendedQuantizersBeforeWriting(bool progressive)
    {
        float[] pixels = Enumerable.Repeat(128.5f, 64).ToArray();
        var options = Options(JpegColorSpace.Grayscale) with
        {
            ForceBaseline = false, AllowOutOfRangeFloatSamples = true, AdaptiveQuantization = false,
            QuantizationTables = Memories([Enumerable.Repeat((ushort)256, 64).ToArray()]),
            Progressive = progressive, ProgressiveRefinementBits = progressive ? 2 : 0
        };
        JpegDocument document = Read(Encode(pixels, 8, 8, JpegColorSpace.Grayscale, options), options);
        Block block = Assert.Single(document.Blocks).Value;
        Assert.Equal(1020, block.Coefficients[0]);
        Assert.All(block.Coefficients[1..], coefficient => Assert.Equal(0, coefficient));
        var tooFine = options with
        {
            QuantizationTables = Memories([Enumerable.Repeat((ushort)255, 64).ToArray()])
        };
        using var output = new MemoryStream();
        ArgumentException error = Assert.Throws<ArgumentException>(() =>
            Encode(output, pixels, 8, 8, JpegColorSpace.Grayscale, tooFine));
        Assert.Equal("pixels", error.ParamName);
        Assert.Equal(0, output.Length);
    }

    [Theory]
    [InlineData(JpegColorSpace.Grayscale, 0, false)]
    [InlineData(JpegColorSpace.YCbCr, 1, true)]
    [InlineData(JpegColorSpace.Rgb, 2, false)]
    [InlineData(JpegColorSpace.Xyb, 2, true)]
    public void SourcePrecisionsAndScanRestartModesPreserveExtendedCoefficients(JpegColorSpace space, int kind, bool adaptive)
    {
        const int width = 33, height = 19;
        Array pixels = Source(Pattern(width, height, Layout(space)), kind);
        ushort[][] tables = MixedTables(Channels(space));
        tables[0][63] = 256;
        var options = Options(space) with
        {
            ForceBaseline = false, QuantizationTables = Memories(tables), AdaptiveQuantization = adaptive,
            OptimizeHuffmanTables = false, SmoothingFactor = 37,
            SamplingFactors = space is JpegColorSpace.YCbCr or JpegColorSpace.Xyb
                ? new(new(2, 2), new(1, 1), new(1, 1)) : FullResolution
        };
        JpegDocument reference = Read(Encode(pixels, width, height, space, options), options);
        foreach ((bool progressive, int bits, bool optimize, int interval) in new[]
            { (false, 0, true, 1), (true, 0, false, 2), (true, 2, true, 1) })
        {
            var variant = options with
            {
                Progressive = progressive, ProgressiveRefinementBits = bits,
                OptimizeHuffmanTables = optimize, RestartInterval = interval
            };
            AssertCoefficients(reference, Read(Encode(pixels, width, height, space, variant), variant));
        }
    }

    [Fact]
    public void ExplicitSequentialAndProgressiveScriptsChooseFrameFromResolvedMode()
    {
        byte[] pixels = Pattern(19, 17, PixelLayout.YCbCr24);
        var options = Options(JpegColorSpace.YCbCr) with
        {
            ForceBaseline = false, QuantizationTables = Memories(MixedTables(3)),
            AdaptiveQuantization = false, RestartInterval = 1
        };
        JpegDocument reference = Read(Encode(pixels, 19, 17, JpegColorSpace.YCbCr, options), options);
        var sequential = options with
        {
            Progressive = true, ScanScript = new JpegScan[] { new([0]), new([1]), new([2]) }
        };
        byte[] separate = Encode(pixels, 19, 17, JpegColorSpace.YCbCr, sequential);
        Assert.Equal(0xc1, ReadHeader(separate).Frame);
        AssertCoefficients(reference, JpegDocument.Read(separate, false, 1,
            expectedScanScript: sequential.ScanScript, extendedQuantization: true));
        var progressive = options with
        {
            Progressive = false,
            ScanScript = new JpegScan[] { new([0, 1, 2], 0, 0), new([0], 1), new([1], 1), new([2], 1) }
        };
        byte[] scans = Encode(pixels, 19, 17, JpegColorSpace.YCbCr, progressive);
        Assert.Equal(0xc2, ReadHeader(scans).Frame);
        AssertCoefficients(reference, JpegDocument.Read(scans, true, 1,
            expectedScanScript: progressive.ScanScript, extendedQuantization: true));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(32768)]
    [InlineData(65535)]
    public void InvalidExtendedValuesFailBeforeWritingForEverySourcePrecision(int invalid)
    {
        foreach (int kind in new[] { 0, 1, 2 })
            foreach ((int table, int frequency) in new[] { (0, 0), (1, 31), (2, 63) })
            {
                ushort[][] tables = MixedTables(3);
                tables[table][frequency] = (ushort)invalid;
                var options = Options(JpegColorSpace.YCbCr) with
                {
                    ForceBaseline = false, QuantizationTables = Memories(tables), Progressive = true,
                    ProgressiveRefinementBits = 2
                };
                using var destination = new MemoryStream();
                using var output = new WriteOnlyStream(destination);
                ArgumentException error = Assert.Throws<ArgumentException>(() => Encode(output,
                    Source(Pattern(8, 8, PixelLayout.YCbCr24), kind), 8, 8, JpegColorSpace.YCbCr, options));
                Assert.Equal(nameof(JpegliEncoderOptions.QuantizationTables), error.ParamName);
                Assert.Equal(0, destination.Length);
                Assert.False(output.Closed);
            }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void PaddedSourcesAndNonSeekableStreamUseSnapshottedExtendedTables(int kind)
    {
        const int width = 19, height = 17, stride = width * 3 + 13;
        Array pixels = Source(Pattern(width, height, PixelLayout.Rgb24), kind);
        Array padded = Array.CreateInstance(pixels.GetType().GetElementType()!, (height - 1) * stride + width * 3 + 17);
        if (padded is float[] floats)
            Array.Fill(floats, float.NaN);
        for (int row = 0; row < height; row++)
            Array.Copy(pixels, row * width * 3, padded, row * stride, width * 3);
        ushort[][] tables = MixedTables(3);
        List<ReadOnlyMemory<ushort>> list = Memories(tables).ToList();
        var options = Options(JpegColorSpace.Rgb) with
        {
            ForceBaseline = false, QuantizationTables = list, Progressive = true,
            ProgressiveRefinementBits = 2, RestartInterval = 2
        };
        byte[] expected = Encode(pixels, width, height, JpegColorSpace.Rgb, options);
        Assert.Equal(expected, Encode(padded, width, height, JpegColorSpace.Rgb, options, stride));
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
        });
        Encode(output, padded, width, height, JpegColorSpace.Rgb, options, stride);
        Assert.True(mutated);
        Assert.False(output.Closed);
        Assert.Equal(expected, destination.ToArray());
        Read(destination.ToArray(), options);
    }

    [Fact]
    public void CancellationBeforeAndDuringExtendedScansLeavesStreamOpen()
    {
        const int width = 65, height = 33;
        byte[] pixels = Pattern(width, height, PixelLayout.YCbCr24);
        var options = Options(JpegColorSpace.YCbCr) with
        {
            ForceBaseline = false, QuantizationTables = Memories(MixedTables(3)),
            Progressive = true, ProgressiveRefinementBits = 2, RestartInterval = 2
        };
        byte[] complete = Encode(pixels, width, height, JpegColorSpace.YCbCr, options);
        CancellationToken canceled = new(canceled: true);
        using var empty = new MemoryStream();
        OperationCanceledException before = Assert.Throws<OperationCanceledException>(() =>
            Encode(empty, pixels, width, height, JpegColorSpace.YCbCr, options, cancellation: canceled));
        Assert.Equal(canceled, before.CancellationToken);
        Assert.Equal(0, empty.Length);
        int cancelAfter = Read(complete, options).Scans[2].DataStart + 1;
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        using var destination = new MemoryStream();
        using var output = new WriteOnlyStream(destination, () =>
        {
            if (destination.Length >= cancelAfter)
                cancellation.Cancel();
        });
        OperationCanceledException during = Assert.Throws<OperationCanceledException>(() =>
            Encode(output, pixels, width, height, JpegColorSpace.YCbCr, options, cancellation: cancellation.Token));
        Assert.Equal(cancellation.Token, during.CancellationToken);
        Assert.False(output.Closed);
        Assert.InRange(destination.Length, cancelAfter, complete.Length - 1);
        Assert.Equal(complete[..(int)destination.Length], destination.ToArray());
    }

    private sealed record Header(int Frame, int SamplePrecision, int DqtLength,
        Dictionary<int, int> Precisions, Dictionary<int, ushort[]> Tables, int[] Selectors);

    private static Header ReadHeader(byte[] jpeg)
    {
        Assert.Equal(new byte[] { 0xff, 0xd8 }, jpeg[..2]);
        int position = 2, frame = 0, precision = 0, dqtLength = 0;
        var precisions = new Dictionary<int, int>();
        var tables = new Dictionary<int, ushort[]>();
        int[] selectors = [];
        while (position < jpeg.Length)
        {
            Assert.Equal(0xff, jpeg[position++]);
            int marker = jpeg[position++];
            int length = Word(jpeg, position), end = position + length;
            Assert.InRange(length, 2, jpeg.Length - position);
            position += 2;
            if (marker == 0xda)
                break;
            if (marker == 0xdb)
            {
                Assert.Equal(0, dqtLength);
                dqtLength = length;
                while (position < end)
                {
                    int selector = jpeg[position++], elementBytes = 1 + (selector >> 4);
                    Assert.InRange(selector >> 4, 0, 1);
                    Assert.InRange(selector & 15, 0, 2);
                    Assert.InRange(position + 64 * elementBytes, position, end);
                    ushort[] table = new ushort[64];
                    for (int wire = 0; wire < 64; wire++)
                    {
                        table[wire] = elementBytes == 1 ? jpeg[position] : (ushort)Word(jpeg, position);
                        position += elementBytes;
                    }
                    tables.Add(selector & 15, table);
                    precisions.Add(selector & 15, selector >> 4);
                }
                Assert.Equal(end, position);
            }
            else if (marker is 0xc0 or 0xc1 or 0xc2)
            {
                Assert.Equal(0, frame);
                frame = marker;
                precision = jpeg[position++];
                position += 4;
                int channels = jpeg[position++];
                Assert.Equal(8 + channels * 3, length);
                selectors = new int[channels];
                for (int channel = 0; channel < channels; channel++)
                {
                    position += 2;
                    selectors[channel] = jpeg[position++];
                }
                Assert.Equal(end, position);
            }
            position = end;
        }
        Assert.NotEqual(0, frame);
        Assert.NotEmpty(tables);
        return new(frame, precision, dqtLength, precisions, tables, selectors);
    }

    private static int Word(byte[] bytes, int position) => bytes[position] * 256 + bytes[position + 1];

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

    private static void AssertCoefficients(JpegDocument expected, JpegDocument actual)
    {
        Assert.Equal(expected.Width, actual.Width);
        Assert.Equal(expected.Height, actual.Height);
        Assert.Equal(expected.Components, actual.Components);
        Assert.Equal(expected.Quantizers.Keys, actual.Quantizers.Keys);
        foreach ((int selector, byte[] table) in expected.Quantizers)
            Assert.Equal(table, actual.Quantizers[selector]);
        Assert.Equal(expected.Blocks.Count, actual.Blocks.Count);
        foreach ((BlockKey key, Block block) in expected.Blocks.Where(pair => !pair.Value.IsDummy))
            Assert.Equal(block.Coefficients, actual.Blocks[key].Coefficients);
    }

    private static ushort[][] MixedTables(int count) => Enumerable.Range(0, count).Select(table => Enumerable.Range(0, 64)
        .Select(frequency => (ushort)(table == 0 ? frequency == 63 ? 255 : 1 + frequency * 3
            : frequency == 63 ? 32767 : frequency == 0 ? 256 : 257 + frequency * 5 + table * 11)).ToArray()).ToArray();

    private static ReadOnlyMemory<ushort>[] Memories(ushort[][] tables) =>
        tables.Select(table => new ReadOnlyMemory<ushort>(table)).ToArray();

    private static int Channels(JpegColorSpace space) => space == JpegColorSpace.Grayscale ? 1 : 3;
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
    private static JpegliEncoderOptions Options(JpegColorSpace space) => new()
    {
        ColorSpace = space, SamplingFactors = FullResolution,
        IccProfile = space == JpegColorSpace.Xyb ? new byte[] { 1, 2, 3, 4 } : ReadOnlyMemory<byte>.Empty
    };
    private static Array Source(byte[] pixels, int kind) => kind switch
    {
        0 => pixels,
        1 => pixels.Select(value => (ushort)(value * 257)).ToArray(),
        _ => pixels.Select(value => value / 255f).ToArray()
    };
    private static byte[] Encode(Array pixels, int width, int height, JpegColorSpace space,
        JpegliEncoderOptions options, int stride = 0)
    {
        using var output = new MemoryStream();
        Encode(output, pixels, width, height, space, options, stride);
        return output.ToArray();
    }
    private static void Encode(Stream output, Array pixels, int width, int height, JpegColorSpace space,
        JpegliEncoderOptions options, int stride = 0, CancellationToken? cancellation = null)
    {
        CancellationToken token = cancellation ?? TestContext.Current.CancellationToken;
        if (pixels is byte[] bytes)
            JpegliEncoder.Encode(output, bytes, width, height, Layout(space), options, stride, token);
        else if (pixels is ushort[] words)
            JpegliEncoder.Encode(output, words, width, height, ComponentLayout(space), options, stride, token);
        else
            JpegliEncoder.Encode(output, (float[])pixels, width, height, ComponentLayout(space), options, stride, token);
    }
    private static JpegDocument Read(byte[] jpeg, JpegliEncoderOptions options) =>
        JpegDocument.Read(jpeg, options.Progressive, options.RestartInterval, options.ProgressiveRefinementBits,
            extendedQuantization: true);
}
