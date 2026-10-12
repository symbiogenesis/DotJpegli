using DotJpegli.Internal;

namespace DotJpegli;

/// <summary>
/// Encodes eight-bit pixels to baseline or progressive JPEG using safe managed code.
/// Includes spatial adaptive quantization; XYB remains pending.
/// </summary>
public static class JpegliEncoder
{
    /// <summary>Encodes an image and returns its JPEG bytes.</summary>
    /// <param name="stride">Bytes between source rows, or zero for tightly packed rows.</param>
    public static byte[] Encode(ReadOnlySpan<byte> pixels, int width, int height,
        PixelLayout layout = PixelLayout.Rgb24, JpegliEncoderOptions? options = null,
        int stride = 0, CancellationToken cancellationToken = default)
    {
        using var output = new MemoryStream();
        Encode(output, pixels, width, height, layout, options, stride, cancellationToken);
        return output.ToArray();
    }

    /// <summary>
    /// Writes a JPEG to a writable stream without closing or seeking it.
    /// Cancellation or an I/O error can leave partial output in the stream.
    /// Source pixels must remain unchanged until encoding returns; Huffman optimization
    /// and progressive scans make multiple passes over them.
    /// </summary>
    public static void Encode(Stream output, ReadOnlySpan<byte> pixels, int width, int height,
        PixelLayout layout = PixelLayout.Rgb24, JpegliEncoderOptions? options = null,
        int stride = 0, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(output);
        if (!output.CanWrite)
            throw new ArgumentException("The destination stream must be writable.", nameof(output));
        options ??= new JpegliEncoderOptions();
        options.Validate();
        if (width is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(width), "JPEG dimensions must be between 1 and 65535.");
        if (height is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(height), "JPEG dimensions must be between 1 and 65535.");
        if ((long)width * height > options.MaxPixels)
            throw new ArgumentOutOfRangeException(nameof(width), "The image exceeds the configured pixel limit.");
        int bytesPerPixel = layout switch
        {
            PixelLayout.Rgb24 or PixelLayout.Bgr24 => 3,
            PixelLayout.Rgba32 or PixelLayout.Bgra32 => 4,
            PixelLayout.Gray8 => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(layout))
        };
        int rowBytes = checked(width * bytesPerPixel);
        if (stride == 0)
            stride = rowBytes;
        if (stride < rowBytes)
            throw new ArgumentOutOfRangeException(nameof(stride), "Stride must hold a complete row.");
        if ((long)(height - 1) * stride + rowBytes > pixels.Length)
            throw new ArgumentException("The source buffer does not contain all image rows.", nameof(pixels));
        cancellationToken.ThrowIfCancellationRequested();

        bool grayscale = layout == PixelLayout.Gray8;
        int componentCount = grayscale ? 1 : 3;
        (int horizontal, int vertical) = grayscale ? (1, 1) : options.Subsampling switch
        {
            ChromaSubsampling.None => (1, 1),
            ChromaSubsampling.HalfHorizontal => (2, 1),
            ChromaSubsampling.Quarter => (2, 2),
            ChromaSubsampling.HalfVertical => (1, 2),
            ChromaSubsampling.QuarterHorizontal => (4, 1),
            _ => throw new ArgumentOutOfRangeException(nameof(options.Subsampling))
        };
        var tables = new ushort[componentCount][];
        for (int component = 0; component < componentCount; component++)
            tables[component] = grayscale ? Quantization.CreateGrayscaleTable(options.Quality)
                : Quantization.CreateTable(options.Quality, component, horizontal, vertical);

        var adaptive = options.AdaptiveQuantization ? new AdaptiveQuantization(width, height, vertical) : null;
        float[][]? adaptiveMultipliers = options.AdaptiveQuantization
            ? Quantization.CreateAdaptiveZeroBiasMultipliers(tables, horizontal, vertical, grayscale) : null;

        var dcTables = new HuffmanTable[grayscale ? 1 : 2];
        var acTables = new HuffmanTable[dcTables.Length];
        if (options.OptimizeHuffmanTables)
        {
            var dcFrequencies = new long[dcTables.Length][];
            var acFrequencies = new long[dcTables.Length][];
            for (int index = 0; index < dcTables.Length; index++)
            {
                dcFrequencies[index] = new long[256];
                acFrequencies[index] = new long[256];
            }
            ProcessBlocks(pixels, width, height, stride, bytesPerPixel, layout, horizontal, vertical,
                tables, options.Quality, options.SmoothingFactor, options.RestartInterval, adaptive, adaptiveMultipliers, dcTables, acTables,
                null, dcFrequencies, acFrequencies, cancellationToken, dcOnly: options.Progressive,
                successiveLow: options.ProgressiveRefinementBits);
            if (options.Progressive)
            {
                for (int component = 0; component < componentCount; component++)
                    ProcessAcBlocks(pixels, width, height, stride, bytesPerPixel, layout, horizontal, vertical,
                        tables, options.Quality, options.SmoothingFactor, options.RestartInterval, adaptive, adaptiveMultipliers,
                        component, acTables, null, acFrequencies, cancellationToken, aggregateEobRuns: true,
                        successiveLow: options.ProgressiveRefinementBits);
                // DC refinement emits raw bits, so only AC refinement contributes
                // additional symbols to the shared optimized Huffman tables.
                for (int low = options.ProgressiveRefinementBits - 1; low >= 0; low--)
                    for (int component = 0; component < componentCount; component++)
                        ProcessAcBlocks(pixels, width, height, stride, bytesPerPixel, layout, horizontal, vertical,
                            tables, options.Quality, options.SmoothingFactor, options.RestartInterval, adaptive, adaptiveMultipliers,
                            component, acTables, null, acFrequencies, cancellationToken,
                            aggregateEobRuns: true, successiveLow: low, refine: true);
            }
            for (int index = 0; index < dcTables.Length; index++)
            {
                dcTables[index] = HuffmanTable.CreateOptimized(dcFrequencies[index]);
                acTables[index] = HuffmanTable.CreateOptimized(acFrequencies[index]);
            }
        }
        else
        {
            for (int index = 0; index < dcTables.Length; index++)
            {
                dcTables[index] = HuffmanTable.CreateDefault(false, index != 0);
                acTables[index] = HuffmanTable.CreateDefault(true, index != 0);
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        JpegMarkerWriter.WriteHeaders(output, width, height, horizontal, vertical, tables, dcTables, acTables,
            options, cancellationToken);
        JpegMarkerWriter.WriteScanHeader(output, componentCount, options.Progressive,
            successiveLow: options.ProgressiveRefinementBits);
        var bits = new BitWriter(output);
        ProcessBlocks(pixels, width, height, stride, bytesPerPixel, layout, horizontal, vertical,
            tables, options.Quality, options.SmoothingFactor, options.RestartInterval, adaptive, adaptiveMultipliers, dcTables, acTables,
            bits, null, null, cancellationToken, dcOnly: options.Progressive,
            successiveLow: options.ProgressiveRefinementBits);
        bits.Flush();
        if (options.Progressive)
        {
            for (int component = 0; component < componentCount; component++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                JpegMarkerWriter.WriteScanHeader(output, componentCount, true, component,
                    successiveLow: options.ProgressiveRefinementBits);
                ProcessAcBlocks(pixels, width, height, stride, bytesPerPixel, layout, horizontal, vertical,
                    tables, options.Quality, options.SmoothingFactor, options.RestartInterval, adaptive, adaptiveMultipliers,
                    component, acTables, bits, null, cancellationToken,
                    aggregateEobRuns: options.OptimizeHuffmanTables,
                    successiveLow: options.ProgressiveRefinementBits);
                bits.Flush();
            }
            for (int low = options.ProgressiveRefinementBits - 1; low >= 0; low--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                JpegMarkerWriter.WriteScanHeader(output, componentCount, true,
                    successiveHigh: low + 1, successiveLow: low);
                ProcessBlocks(pixels, width, height, stride, bytesPerPixel, layout, horizontal, vertical,
                    tables, options.Quality, options.SmoothingFactor, options.RestartInterval, adaptive, adaptiveMultipliers, dcTables, acTables,
                    bits, null, null, cancellationToken, dcOnly: true, successiveLow: low, refine: true);
                bits.Flush();
                for (int component = 0; component < componentCount; component++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    JpegMarkerWriter.WriteScanHeader(output, componentCount, true, component,
                        successiveHigh: low + 1, successiveLow: low);
                    ProcessAcBlocks(pixels, width, height, stride, bytesPerPixel, layout, horizontal, vertical,
                        tables, options.Quality, options.SmoothingFactor, options.RestartInterval, adaptive, adaptiveMultipliers,
                        component, acTables, bits, null, cancellationToken,
                        aggregateEobRuns: options.OptimizeHuffmanTables, successiveLow: low, refine: true);
                    bits.Flush();
                }
            }
        }
        output.WriteByte(0xff);
        output.WriteByte(0xd9);
    }

    private static void ProcessBlocks(ReadOnlySpan<byte> pixels, int width, int height, int stride,
        int bytesPerPixel, PixelLayout layout, int horizontal, int vertical, ushort[][] tables,
        float quality, int smoothingFactor, int restartInterval, AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers,
        HuffmanTable[] dcTables, HuffmanTable[] acTables, BitWriter? writer,
        long[][]? dcFrequencies, long[][]? acFrequencies, CancellationToken cancellationToken,
        bool dcOnly = false, int successiveLow = 0, bool refine = false)
    {
        Span<float> samples = stackalloc float[64];
        Span<float> coefficients = stackalloc float[64];
        Span<short> quantized = stackalloc short[64];
        Span<int> previousDc = stackalloc int[3];
        previousDc.Clear();
        Span<int> previousScanDc = stackalloc int[3];
        previousScanDc.Clear();
        float distance = Quantization.QualityToDistance(quality);
        int mcuWidth = horizontal * 8;
        int mcuHeight = vertical * 8;
        int mcusRemaining = restartInterval;
        int restartIndex = 0;
        float[]? strengths = adaptive is null ? null : new float[adaptive.BlocksWide * vertical];
        for (int top = 0; top < height; top += mcuHeight)
        {
            if (adaptive is not null)
                adaptive.ComputeRow(pixels, stride, bytesPerPixel, layout, top, tables[0][1], strengths!, cancellationToken);
            for (int left = 0; left < width; left += mcuWidth)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (restartInterval != 0 && mcusRemaining == 0)
                {
                    // Reset in both passes so the optimized tables include the
                    // same DC categories used after each emitted restart marker.
                    writer?.WriteRestartMarker(restartIndex);
                    previousDc.Clear();
                    previousScanDc.Clear();
                    restartIndex = (restartIndex + 1) & 7;
                    mcusRemaining = restartInterval;
                }
                for (int component = 0; component < tables.Length; component++)
                {
                    int blocksWide = component == 0 ? horizontal : 1;
                    int blocksHigh = component == 0 ? vertical : 1;
                    int sampleWidth = component == 0 ? 1 : horizontal;
                    int sampleHeight = component == 0 ? 1 : vertical;
                    for (int blockY = 0; blockY < blocksHigh; blockY++)
                    {
                        for (int blockX = 0; blockX < blocksWide; blockX++)
                        {
                            int blockLeft = left + blockX * 8;
                            int blockTop = top + blockY * 8;
                            if (blockLeft >= width || blockTop >= height)
                            {
                                // Upstream's baseline streaming path emits a zero DC difference
                                // and AC EOB for a wholly padded MCU block. Keep the predictor,
                                // and bypass sample conversion, the DCT and adaptive quantization.
                                quantized.Clear();
                                quantized[0] = checked((short)previousDc[component]);
                            }
                            else
                            {
                                Downsampling.FillBlock(pixels, width, height, stride, bytesPerPixel, layout, component,
                                    blockLeft, blockTop, sampleWidth, sampleHeight, samples, smoothingFactor);
                                ForwardDct.Transform(samples, coefficients);
                                // Upstream samples the top-left luma field value for a chroma block.
                                float strength = adaptive is null ? 0f : strengths![
                                    (component == 0 ? blockY : 0) * adaptive.BlocksWide +
                                    left / 8 + (component == 0 ? blockX : 0)];
                                Quantization.Quantize(coefficients, tables[component], quantized, component, distance,
                                    grayscale: layout == PixelLayout.Gray8, adaptiveQuantization: adaptive is not null,
                                    adaptiveStrength: strength,
                                    adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                                    previousDc: previousDc[component]);
                            }
                            int tableIndex = component == 0 ? 0 : 1;
                            if (dcOnly)
                            {
                                if (refine)
                                    ProgressiveEntropyEncoder.WriteDcRefinement(writer!, quantized, successiveLow);
                                else if (writer is null)
                                    ProgressiveEntropyEncoder.AccumulateDcFrequencies(quantized,
                                        ref previousScanDc[component], dcFrequencies![tableIndex], successiveLow);
                                else
                                    ProgressiveEntropyEncoder.WriteDc(writer, quantized,
                                        ref previousScanDc[component], dcTables[tableIndex], successiveLow);
                                // Dummy blocks and quantization use the full coefficient,
                                // independently of the initial scan's shifted predictor.
                                previousDc[component] = quantized[0];
                            }
                            else if (writer is null)
                                EntropyEncoder.AccumulateFrequencies(quantized, ref previousDc[component],
                                    dcFrequencies![tableIndex], acFrequencies![tableIndex]);
                            else
                                EntropyEncoder.WriteBlock(writer, quantized, ref previousDc[component],
                                    dcTables[tableIndex], acTables[tableIndex]);
                        }
                    }
                }
                if (restartInterval != 0)
                    mcusRemaining--;
            }
        }
    }

    private static void ProcessAcBlocks(ReadOnlySpan<byte> pixels, int width, int height, int stride,
        int bytesPerPixel, PixelLayout layout, int horizontal, int vertical, ushort[][] tables,
        float quality, int smoothingFactor, int restartInterval, AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers,
        int component, HuffmanTable[] acTables, BitWriter? writer, long[][]? acFrequencies,
        CancellationToken cancellationToken, bool aggregateEobRuns, int successiveLow = 0, bool refine = false)
    {
        // A noninterleaved scan visits only real component blocks in raster order.
        // In particular, 4:2:0 luma must finish one block row before the next,
        // rather than following the two-row ordering of interleaved MCUs.
        Span<float> samples = stackalloc float[64];
        Span<float> coefficients = stackalloc float[64];
        Span<short> quantized = stackalloc short[64];
        int sampleWidth = component == 0 ? 1 : horizontal;
        int sampleHeight = component == 0 ? 1 : vertical;
        int tableIndex = component == 0 ? 0 : 1;
        float distance = Quantization.QualityToDistance(quality);
        float[]? strengths = adaptive is null ? null : new float[adaptive.BlocksWide * vertical];
        int lastAnalysisTop = -1;
        int mcusRemaining = restartInterval;
        int restartIndex = 0;
        int pendingEobRun = 0;
        var refinementState = refine ? new ProgressiveAcRefinementState() : null;

        void FlushEobRun()
        {
            if (refine)
            {
                if (writer is null)
                    ProgressiveEntropyEncoder.FlushAcRefinementFrequencies(acFrequencies![tableIndex], refinementState!);
                else
                    ProgressiveEntropyEncoder.FlushAcRefinement(writer, acTables[tableIndex], refinementState!);
            }
            else if (writer is null)
                ProgressiveEntropyEncoder.FlushAcFrequencies(acFrequencies![tableIndex], ref pendingEobRun);
            else
                ProgressiveEntropyEncoder.FlushAc(writer, acTables[tableIndex], ref pendingEobRun);
        }

        for (int top = 0; top < height; top += 8 * sampleHeight)
        {
            int analysisTop = top / (8 * vertical) * (8 * vertical);
            if (adaptive is not null && analysisTop != lastAnalysisTop)
            {
                adaptive.ComputeRow(pixels, stride, bytesPerPixel, layout, analysisTop,
                    tables[0][1], strengths!, cancellationToken);
                lastAnalysisTop = analysisTop;
            }
            for (int left = 0; left < width; left += 8 * sampleWidth)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (restartInterval != 0 && mcusRemaining == 0)
                {
                    // EOB runs cannot cross restart intervals. The frequency
                    // pass must terminate runs at the same boundaries as output.
                    FlushEobRun();
                    writer?.WriteRestartMarker(restartIndex);
                    restartIndex = (restartIndex + 1) & 7;
                    mcusRemaining = restartInterval;
                }
                Downsampling.FillBlock(pixels, width, height, stride, bytesPerPixel, layout, component,
                    left, top, sampleWidth, sampleHeight, samples, smoothingFactor);
                ForwardDct.Transform(samples, coefficients);
                int strengthRow = component == 0 ? (top - analysisTop) / 8 : 0;
                float strength = adaptive is null ? 0f : strengths![strengthRow * adaptive.BlocksWide + left / 8];
                // AC quantization is independent of the DC predictor. The DC
                // entry computed here is ignored by this scan.
                Quantization.Quantize(coefficients, tables[component], quantized, component, distance,
                    grayscale: layout == PixelLayout.Gray8, adaptiveQuantization: adaptive is not null,
                    adaptiveStrength: strength,
                    adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component]);
                if (refine)
                {
                    if (writer is null)
                        ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(quantized,
                            acFrequencies![tableIndex], refinementState!, successiveLow);
                    else
                        ProgressiveEntropyEncoder.WriteAcRefinement(writer, quantized, acTables[tableIndex],
                            refinementState!, successiveLow);
                }
                else if (writer is null)
                    ProgressiveEntropyEncoder.AccumulateAcFrequencies(quantized, acFrequencies![tableIndex],
                        ref pendingEobRun, successiveLow);
                else
                    ProgressiveEntropyEncoder.WriteAc(writer, quantized, acTables[tableIndex], ref pendingEobRun, successiveLow);
                // Annex K default AC tables only contain the one-block EOB
                // symbol. Aggregated runs use the collected, optimized tables.
                if (!aggregateEobRuns)
                    FlushEobRun();
                if (restartInterval != 0)
                    mcusRemaining--;
            }
        }
        FlushEobRun();
    }

}
