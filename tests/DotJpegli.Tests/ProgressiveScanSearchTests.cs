namespace DotJpegli.Tests;

using System.Security.Cryptography;
using DotJpegli.Parity;
using StbImageSharp;
using Xunit;
using static DotJpegli.Tests.EncoderProgressiveTests;

public sealed class ProgressiveScanSearchTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    public void ConstrainedRefinedTablesPreserveBytesWithTokenReplayAndDirectEmission(int limit)
    {
        const int width = 65, height = 33;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        ProgressiveScanSearch.ScanCandidate candidate = Assert.Single(ProgressiveScanSearch.Candidates,
            item => item.Id == $"ends-2-63-depth-2-order-120-optimized-limit{limit}");
        JpegliEncoderOptions options = candidate.CreateOptions(1, ChromaSubsampling.Quarter);
        byte[] cached = Encode(pixels, width, height, PixelLayout.Rgb24, options);
        byte[] direct = Encode(pixels, width, height, PixelLayout.Rgb24,
            options with { EntropyTokenBudgetBytes = 0 });
        Assert.Equal(cached, direct);
    }

    [Theory]
    [InlineData(ChromaSubsampling.None, 1f)]
    [InlineData(ChromaSubsampling.None, 4f)]
    [InlineData(ChromaSubsampling.Quarter, 1f)]
    [InlineData(ChromaSubsampling.Quarter, 4f)]
    public void EveryCandidatePreservesIndependentCoefficientsQuantizersAndPixels(
        ChromaSubsampling subsampling, float distance)
    {
        const int width = 17, height = 9;
        byte[] pixels = Pattern(width, height, PixelLayout.Rgb24);
        IReadOnlyList<ProgressiveScanSearch.ScanCandidate> candidates = ProgressiveScanSearch.Candidates;
        Assert.Equal(181, candidates.Count);
        Assert.Equal(181, candidates.Select(candidate => candidate.Id).Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(145, candidates.Count(candidate => candidate.OptimizeHuffmanTables));
        Assert.Null(candidates[0].Script);
        JpegliEncoderOptions baselineOptions = candidates[0].CreateOptions(distance, subsampling) with { ProgressiveLevel = 0 };
        byte[] baseline = Encode(pixels, width, height, PixelLayout.Rgb24, baselineOptions);
        var reference = JpegDocument.Read(baseline, progressive: false, expectedInterval: 0);
        byte[] referencePixels = ImageResult.FromMemory(baseline, ColorComponents.RedGreenBlue).Data;
        var outputs = new List<byte[]>();
        var documents = new List<JpegDocument>();

        ProgressiveScanSearch.SearchResult result = ProgressiveScanSearch.Run(candidate =>
        {
            byte[] jpeg = Encode(pixels, width, height, PixelLayout.Rgb24,
                candidate.CreateOptions(distance, subsampling));
            JpegDocument document = candidate.Script is null
                ? JpegDocument.Read(jpeg, progressive: true, expectedInterval: 0,
                    expectedAcScanEnds: [2, 63], expectedAcRefinementBits: [0, 2],
                    expectedInterleaveDc: subsampling == ChromaSubsampling.None)
                : JpegDocument.Read(jpeg, progressive: true, expectedInterval: 0,
                    expectedScanScript: candidate.Script);
            Assert.Equal(reference.Width, document.Width);
            Assert.Equal(reference.Height, document.Height);
            Assert.Equal(reference.Components, document.Components);
            Assert.Equal(reference.Quantizers.Count, document.Quantizers.Count);
            foreach ((int selector, byte[] quantizer) in reference.Quantizers)
            {
                Assert.Equal(quantizer, document.Quantizers[selector]);
            }
            Assert.Equal(reference.Blocks.Count(pair => !pair.Value.IsDummy),
                document.Blocks.Count(pair => !pair.Value.IsDummy));
            foreach ((BlockKey key, Block block) in reference.Blocks.Where(pair => !pair.Value.IsDummy))
            {
                Assert.Equal(block.Coefficients, document.Blocks[key].Coefficients);
            }
            var decoded = ImageResult.FromMemory(jpeg, ColorComponents.RedGreenBlue);
            Assert.Equal(width, decoded.Width);
            Assert.Equal(height, decoded.Height);
            Assert.Equal(referencePixels, decoded.Data);
            outputs.Add(jpeg);
            documents.Add(document);
            return jpeg;
        }, TestContext.Current.CancellationToken);

        Assert.Equal(candidates.Count, result.EncodeCount);
        Assert.Equal(documents.Sum(document => document.Scans.Count), result.TotalScanCount);
        Assert.Equal(documents.Where((_, index) => candidates[index].OptimizeHuffmanTables)
            .Sum(document => document.Scans.Count), result.TotalHistogramScanCount);
        Assert.Equal(result.TotalScanCount + result.TotalHistogramScanCount, result.TotalEntropyPassCount);
        for (int index = 0; index < candidates.Count; index++)
        {
            ProgressiveScanSearch.Trial trial = result.Trials[index];
            Assert.Equal(candidates[index].Id, trial.Id);
            Assert.Equal(candidates[index].OptimizeHuffmanTables, trial.OptimizeHuffmanTables);
            Assert.Equal(candidates[index].HuffmanTableLimit, trial.HuffmanTableLimit);
            Assert.Equal(outputs[index].Length, trial.Bytes);
            Assert.Equal(Convert.ToHexString(SHA256.HashData(outputs[index])), trial.Sha256);
            Assert.Equal(documents[index].Scans.Count, trial.ScanCount);
            Assert.Equal(trial.ScanCount, trial.HuffmanSelectors.Length);
            for (int scan = 0; scan < trial.ScanCount; scan++)
            {
                Assert.Equal(documents[index].Scans[scan].Selectors
                    .Select(selector => (selector.Dc << 4) | (selector.Ac & 15)), trial.HuffmanSelectors[scan]);
            }
            Assert.True(trial.HuffmanDefinitionCount > 0);
            Assert.InRange(trial.HuffmanDefinitionBytes, (18 * trial.HuffmanDefinitionCount) + 4, trial.Bytes);
        }

        int minimum = outputs.Min(jpeg => jpeg.Length);
        int winner = outputs.FindIndex(jpeg => jpeg.Length == minimum);
        Assert.Same(candidates[winner], result.Winner);
        Assert.Same(outputs[winner], result.Jpeg);
        Assert.True(result.Jpeg.Length <= outputs[0].Length);
        Assert.Equal(result.Jpeg, Encode(pixels, width, height, PixelLayout.Rgb24,
            result.Winner.CreateOptions(distance, subsampling)));
    }

    [Fact]
    public void SearchUsesTheDeclaredCandidatesAndReturnsTheSameMinimum()
    {
        const int width = 17, height = 9;
        byte[] pixels = FlatPixels(width, height, PixelLayout.Rgb24);
        ProgressiveScanSearch.SearchResult expected = ProgressiveScanSearch.Run(candidate => Encode(pixels, width, height, PixelLayout.Rgb24,
            candidate.CreateOptions(4, ChromaSubsampling.Quarter)), TestContext.Current.CancellationToken);
        ProgressiveScanSearch.SearchResult actual = ProgressiveScanSearch.Search(pixels, width, height, 4, ChromaSubsampling.Quarter,
            TestContext.Current.CancellationToken);
        Assert.Equal(expected.Winner, actual.Winner);
        Assert.Equal(expected.Jpeg, actual.Jpeg);
        Assert.Equal(expected.Trials.Select(trial => (trial.Id, trial.Bytes, trial.Sha256)),
            actual.Trials.Select(trial => (trial.Id, trial.Bytes, trial.Sha256)));
    }

    [Fact]
    public void EqualCompleteFileSizesKeepTheControlAndFirstMinimum()
    {
        byte[] jpeg = Encode([16, 128, 240], 1, 1, PixelLayout.Rgb24,
            ProgressiveScanSearch.Candidates[0].CreateOptions(1, ChromaSubsampling.None));
        ProgressiveScanSearch.SearchResult tied = ProgressiveScanSearch.Run(_ => jpeg, TestContext.Current.CancellationToken);
        Assert.Same(ProgressiveScanSearch.Candidates[0], tied.Winner);
        Assert.Same(jpeg, tied.Jpeg);

        int call = 0;
        ProgressiveScanSearch.SearchResult minimum = ProgressiveScanSearch.Run(_ =>
        {
            int index = call++;
            return index is 7 or 19 ? jpeg : WithComment(jpeg, 100 + index);
        }, TestContext.Current.CancellationToken);
        Assert.Equal(181, call);
        Assert.Same(ProgressiveScanSearch.Candidates[7], minimum.Winner);
        Assert.Same(jpeg, minimum.Jpeg);
        Assert.Equal(jpeg.Length, minimum.Trials[7].Bytes);
        Assert.Equal(jpeg.Length, minimum.Trials[19].Bytes);
        Assert.Equal(jpeg.Length + 104, minimum.Trials[0].Bytes);
    }

    [Fact]
    public void CancellationStopsBeforeTheFirstOrNextCandidate()
    {
        using var canceled = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        canceled.Cancel();
        int calls = 0;
        _ = Assert.Throws<OperationCanceledException>(() => ProgressiveScanSearch.Run(_ =>
        {
            calls++;
            return [];
        }, canceled.Token));
        Assert.Equal(0, calls);

        using var between = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        _ = Assert.Throws<OperationCanceledException>(() => ProgressiveScanSearch.Run(_ =>
        {
            calls++;
            between.Cancel();
            return [];
        }, between.Token));
        Assert.Equal(1, calls);
    }

    [Fact]
    public void EncoderFailuresAndInvalidJpegsStopTheSearch()
    {
        var failure = new InvalidOperationException("candidate failure");
        int calls = 0;
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => ProgressiveScanSearch.Run(_ =>
        {
            calls++;
            throw failure;
        }, TestContext.Current.CancellationToken)));
        Assert.Equal(1, calls);
        _ = Assert.Throws<InvalidDataException>(() => ProgressiveScanSearch.Run(_ =>
        {
            calls++;
            return [0xff, 0xd8, 0xff, 0xd9];
        }, TestContext.Current.CancellationToken));
        Assert.Equal(2, calls);
    }

    [Theory]
    [InlineData(false, 40)]
    [InlineData(true, 44)]
    public void HuffmanAccountingIncludesEveryDefinitionAndSegmentHeader(bool separateSegments, int expectedBytes)
    {
        byte[] dc = MinimalHuffmanDefinition(0), ac = MinimalHuffmanDefinition(0x10);
        byte[][] segments = separateSegments ? [dc, ac] : [[.. dc, .. ac]];
        var structure = JpegStructure.Read(StructuralJpeg(segments, selector: 0x13));
        Assert.Equal(2, structure.HuffmanDefinitionCount);
        Assert.Equal(expectedBytes, structure.HuffmanDefinitionBytes);
        Assert.Equal(new[] { 0x13 }, Assert.Single(structure.HuffmanSelectors));
    }

    [Theory]
    [InlineData(0x04)]
    [InlineData(0x20)]
    [InlineData(0xff)]
    public void StructuralReaderRejectsInvalidHuffmanDescriptors(int descriptor) =>
        Assert.Throws<InvalidDataException>(() => JpegStructure.Read(
            StructuralJpeg([MinimalHuffmanDefinition((byte)descriptor)])));

    [Fact]
    public void StructuralReaderRejectsTruncatedHuffmanCountsAndSymbols()
    {
        byte[] definition = MinimalHuffmanDefinition(0);
        _ = Assert.Throws<InvalidDataException>(() => JpegStructure.Read(StructuralJpeg([definition[..16]])));
        _ = Assert.Throws<InvalidDataException>(() => JpegStructure.Read(StructuralJpeg([definition[..17]])));
        definition[1] = 0;
        _ = Assert.Throws<InvalidDataException>(() => JpegStructure.Read(StructuralJpeg([definition])));
        definition[1] = 2;
        _ = Assert.Throws<InvalidDataException>(() => JpegStructure.Read(StructuralJpeg([definition])));
    }

    [Theory]
    [InlineData(0x04)]
    [InlineData(0x40)]
    [InlineData(0xff)]
    public void StructuralReaderRejectsOutOfRangeSosHuffmanSelectors(int selector) =>
        Assert.Throws<InvalidDataException>(() => JpegStructure.Read(
            StructuralJpeg([MinimalHuffmanDefinition(0)], (byte)selector)));

    private static byte[] MinimalHuffmanDefinition(byte descriptor)
    {
        byte[] definition = new byte[18];
        definition[0] = descriptor;
        definition[1] = 1;
        return definition;
    }

    // A marker-only fixture tests accounting independently of the encoder's
    // table planner. Its selectors intentionally need not reference these tables.
    private static byte[] StructuralJpeg(byte[][] huffmanSegments, byte selector = 0)
    {
        var bytes = new List<byte>
        {
            0xff, 0xd8, 0xff, 0xdb, 0, 67, 0
        };
        bytes.AddRange(Enumerable.Repeat((byte)1, 64));
        bytes.AddRange([0xff, 0xc2, 0, 11, 8, 0, 1, 0, 1, 1, 1, 0x11, 0]);
        foreach (byte[] segment in huffmanSegments)
        {
            int length = segment.Length + 2;
            bytes.AddRange([0xff, 0xc4, (byte)(length >> 8), (byte)length]);
            bytes.AddRange(segment);
        }
        bytes.AddRange([0xff, 0xda, 0, 8, 1, 1, selector, 0, 0, 0, 0, 0xff, 0xd9]);
        return bytes.ToArray();
    }

    private static byte[] WithComment(byte[] jpeg, int payloadLength)
    {
        byte[] withComment = new byte[jpeg.Length + payloadLength + 4];
        jpeg.AsSpan(0, 2).CopyTo(withComment);
        withComment[2] = 0xff;
        withComment[3] = 0xfe;
        withComment[4] = (byte)((payloadLength + 2) >> 8);
        withComment[5] = (byte)(payloadLength + 2);
        jpeg.AsSpan(2).CopyTo(withComment.AsSpan(payloadLength + 6));
        return withComment;
    }
}
