namespace DotJpegli.Internal;

using System.Buffers;
#if !NETSTANDARD2_1
using System.Numerics;
#endif
using System.Runtime.CompilerServices;

/// <summary>Reuses full coefficients across histogram, spectral and refinement passes.</summary>
internal sealed class QuantizedImage : IDisposable
{
    // Keep the streaming path for very large images rather than requiring an
    // unbounded image-sized allocation. The rented arrays are returned on every exit.
    internal const long MaximumBytes = 64L * 1024 * 1024;
    private const int SegmentLength = 65536;
    private readonly short[][] components;
    private readonly short[][][]? segments;
    private readonly ulong[][]? blockMasks;
    private readonly int[] widths;
    private readonly int[] cachedHeights;
    private readonly int cachedMcuRows;
    private readonly int mcuHeight;
    private readonly ArrayPool<short> pool;
    private readonly EncoderCacheBudget? cacheBudget;

    private QuantizedImage(int[] widths, int[] cachedHeights, int cachedMcuRows, int mcuHeight,
        bool complete, bool segmented, bool zigZagOrdered, bool trackBlockMasks,
        ArrayPool<short> pool, EncoderCacheBudget? cacheBudget)
    {
        this.components = new short[widths.Length][];
        this.segments = segmented ? new short[widths.Length][][] : null;
        this.blockMasks = trackBlockMasks ? new ulong[widths.Length][] : null;
        this.widths = widths;
        this.cachedHeights = cachedHeights;
        this.cachedMcuRows = cachedMcuRows;
        this.mcuHeight = mcuHeight;
        this.pool = pool;
        this.cacheBudget = cacheBudget;
        this.IsComplete = complete;
        this.IsZigZagOrdered = zigZagOrdered;
    }

    internal bool IsZigZagOrdered { get; }
    internal bool IsComplete { get; }
    internal bool HasBlockMasks => this.blockMasks is not null;
    internal long RentedBytes { get; private set; }

    internal bool ContainsRow(int component, int y) => (uint)y < (uint)this.cachedHeights[component];
    internal bool ContainsMcuRow(int top) => top >= 0 && top / this.mcuHeight < this.cachedMcuRows;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal ulong GetBlockMask(int component, int x, int y)
        => this.blockMasks![component][(y * this.widths[component]) + x];

    internal static QuantizedImage? Create(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables, float distance, int smoothingFactor,
        AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers, CancellationToken cancellationToken,
        bool zigZagOrdered = false, KernelPreference narrowZigZagKernel = KernelPreference.Automatic,
        double rateDistortionLambda = 0, RateDistortionDiagnostics? rateDistortionDiagnostics = null,
        long maximumBytes = MaximumBytes, bool allowPartial = false, EncoderCacheBudget? cacheBudget = null,
        ArrayPool<short>? pool = null, double dcTrellisLambda = 0,
        DcTrellisDiagnostics? dcTrellisDiagnostics = null, int[]? dcRestartIntervals = null,
        bool rateDistortionHuffmanFeedback = false, Quantization.PreparedTable[]? preparedTables = null)
    {
        Guard.ThrowIfNegative(maximumBytes);
        cancellationToken.ThrowIfCancellationRequested();
        if (rateDistortionHuffmanFeedback)
        {
            return CreateHuffmanFeedbackResearch(pixels, width, height, stride, sampling, tables, distance,
                smoothingFactor, adaptive, adaptiveMultipliers, zigZagOrdered, narrowZigZagKernel,
                rateDistortionLambda, rateDistortionDiagnostics, maximumBytes, cacheBudget, pool,
                dcTrellisLambda, dcTrellisDiagnostics, dcRestartIntervals, cancellationToken, preparedTables);
        }
        if (!double.IsFinite(dcTrellisLambda) || dcTrellisLambda < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dcTrellisLambda));
        }
        if (dcTrellisLambda > 0)
        {
            return CreateDcResearch(pixels, width, height, stride, sampling, tables, distance, smoothingFactor,
                adaptive, adaptiveMultipliers, zigZagOrdered, narrowZigZagKernel,
                rateDistortionLambda, rateDistortionDiagnostics, maximumBytes, cacheBudget, pool,
                dcTrellisLambda, dcTrellisDiagnostics, dcRestartIntervals, cancellationToken, preparedTables);
        }
        if (!double.IsFinite(rateDistortionLambda) || rateDistortionLambda < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(rateDistortionLambda));
        }
        if (rateDistortionLambda > 0 && !zigZagOrdered)
        {
            throw new ArgumentException("The offline rate-distortion experiment requires zigzag-ordered cache blocks.", nameof(zigZagOrdered));
        }
        int[] widths = new int[tables.Length];
        int[] heights = new int[tables.Length];
        for (int component = 0; component < tables.Length; component++)
        {
            JpegComponentSampling factor = sampling[component];
            widths[component] = ((width * factor.Horizontal) + (8 * sampling.MaxHorizontal) - 1)
                / (8 * sampling.MaxHorizontal);
            heights[component] = ((height * factor.Vertical) + (8 * sampling.MaxVertical) - 1)
                / (8 * sampling.MaxVertical);
        }

        long available = Math.Min(Math.Min(maximumBytes, MaximumBytes), cacheBudget?.AvailableBytes ?? long.MaxValue);
        int mcuHeight = sampling.MaxVertical * 8;
        int totalMcuRows = (height + mcuHeight - 1) / mcuHeight;
        int cachedMcuRows = totalMcuRows;
        bool segmented = PlannedBytes(widths, heights, totalMcuRows, sampling, segmented: false) > available;
        if (segmented)
        {
            if (!allowPartial || rateDistortionLambda > 0)
            {
                return null;
            }

            // Fixed-size coefficient segments avoid whole-component pool bucket
            // jumps. Above the cap, retain a complete prefix of MCU rows across
            // every component so the remaining scans can still reuse that work.
            int lower = 0, upper = totalMcuRows;
            while (lower < upper)
            {
                int middle = lower + ((upper - lower + 1) / 2);
                if (PlannedBytes(widths, heights, middle, sampling, segmented: true) <= available)
                {
                    lower = middle;
                }
                else
                {
                    upper = middle - 1;
                }
            }
            cachedMcuRows = lower;
            if (cachedMcuRows == 0)
            {
                return null;
            }
        }

        for (int component = 0; component < heights.Length; component++)
        {
            heights[component] = Math.Min(heights[component], cachedMcuRows * sampling[component].Vertical);
        }

        bool trackBlockMasks = zigZagOrdered && rateDistortionLambda == 0;
        var image = new QuantizedImage(widths, heights, cachedMcuRows, mcuHeight,
            cachedMcuRows == totalMcuRows, segmented, zigZagOrdered, trackBlockMasks,
            pool ?? ArrayPool<short>.Shared, cacheBudget);
        try
        {
            Quantization.PreparedTable[] quantizers = preparedTables ?? new Quantization.PreparedTable[tables.Length];
            for (int component = 0; component < tables.Length; component++)
            {
                int blockCount = checked(widths[component] * heights[component]);
                int length = checked(blockCount * 64);
                if (!image.AllocateComponent(component, length, available, cancellationToken))
                {
                    image.Dispose();
                    return null;
                }
                image.blockMasks?[component] = ArrayPool<ulong>.Shared.Rent(blockCount);
                if (preparedTables is null)
                {
                    quantizers[component] = Quantization.PreparedTable.Create(tables[component], component, distance,
                        grayscale: pixels.IsGrayscale, adaptiveQuantization: adaptive is not null,
                        adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                        rgb: pixels.IsRgb, narrowZigZagKernel: narrowZigZagKernel,
                        imageWidth: width, imageHeight: height, tailZeroRunBias: pixels.TailZeroRunBias);
                }
            }
            Span<float> samples = stackalloc float[64];
            Span<short> naturalOrder = stackalloc short[64];
            int horizontal = sampling.MaxHorizontal;
            int vertical = sampling.MaxVertical;
            Span<float> strengths = adaptive is null ? default : adaptive.Strengths;
            ParallelDctRows? parallelQuantized = rateDistortionLambda == 0 && zigZagOrdered
                && pixels.ParallelRows is { CanPrepareQuantizedRows: true } candidate ? candidate : null;
            for (int top = 0; top < Math.Min(height, cachedMcuRows * mcuHeight); top += vertical * 8)
            {
                cancellationToken.ThrowIfCancellationRequested();
                pixels.AdvanceConvertedWindow(top + mcuHeight + 6, stride, cancellationToken);
                if (parallelQuantized is not null)
                {
                    adaptive?.ComputeRow(parallelQuantized.SourceForAnalysis, stride, top,
                        tables[pixels.AnalysisComponent][1], strengths, cancellationToken);
                    parallelQuantized.PrepareQuantizedRow(top, quantizers,
                        adaptive is null ? default : strengths, cancellationToken);
                    for (int component = 0; component < tables.Length; component++)
                    {
                        JpegComponentSampling factor = sampling[component];
                        int firstRow = top / (vertical * 8) * factor.Vertical;
                        for (int row = 0; row < factor.Vertical && firstRow + row < heights[component]; row++)
                        {
                            int y = firstRow + row;
                            Span<ulong> rowMasks = image.blockMasks is null ? default
                                : image.blockMasks[component].AsSpan(y * widths[component], widths[component]);
                            for (int x = 0; x < widths[component]; x++)
                            {
                                ReadOnlySpan<short> block = parallelQuantized.GetQuantizedBlock(component, x, y, out ulong nonzeroAcMask);
                                block.CopyTo(image.Block(component, x, y));
                                if (!rowMasks.IsEmpty)
                                {
                                    rowMasks[x] = nonzeroAcMask;
                                }
                            }
                        }
                    }
                    continue;
                }

                pixels.ParallelRows?.PrepareRow(top, cancellationToken);
                adaptive?.ComputeRow(pixels, stride, top, tables[pixels.AnalysisComponent][1], strengths, cancellationToken);

                for (int component = 0; component < tables.Length; component++)
                {
                    JpegComponentSampling factor = sampling[component];
                    ReadOnlySpan<float> plane = smoothingFactor == 0 && factor.Horizontal == horizontal
                        && factor.Vertical == vertical ? pixels.GetConvertedPlane(component) : default;
                    int planeWidth = width, planeHeight = height;
                    if (smoothingFactor == 0 && sampling.Is420 && component != 0)
                    {
                        plane = pixels.GetConverted420ChromaPlane(component, out planeWidth, out planeHeight);
                    }

                    int firstRow = top / (vertical * 8) * factor.Vertical;
                    for (int row = 0; row < factor.Vertical && firstRow + row < heights[component]; row++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        int y = firstRow + row;
                        ReadOnlySpan<float> rowPlane = plane;
                        int rowPlaneWidth = planeWidth, rowPlaneHeight = planeHeight, planeY = y * 8;
                        bool compactChroma = sampling.Is420 && component != 0;
                        if (rowPlane.IsEmpty && smoothingFactor == 0
                            && ((factor.Horizontal == horizontal && factor.Vertical == vertical) || compactChroma))
                        {
                            rowPlane = pixels.GetConvertedPlaneSegment(component, y * 8, compactChroma,
                                out rowPlaneWidth, out rowPlaneHeight, out planeY);
                        }
                        if (pixels.ParallelRows is null && rateDistortionLambda == 0 && !rowPlane.IsEmpty && zigZagOrdered)
                        {
                            int rowOffset = adaptive is null ? 0 : row * adaptive.BlocksWide;
                            int strengthStep = horizontal / factor.Horizontal;
                            int interiorBlocks = planeY + 8 <= rowPlaneHeight
                                ? Math.Min(widths[component], rowPlaneWidth / 8) : 0;
                            Span<ulong> rowMasks = image.blockMasks![component].AsSpan(y * widths[component], widths[component]);
                            if (image.segments is null)
                            {
                                Span<short> componentRow = image.components[component].AsSpan(y * widths[component] * 64,
                                    widths[component] * 64);
                                for (int x = 0; x < interiorBlocks; x++)
                                {
                                    float strength = adaptive is null ? 0f : strengths[rowOffset + (x * strengthStep)];
                                    rowMasks[x] = ForwardDct.TransformQuantizePlaneWithMask(rowPlane, rowPlaneWidth, x * 8, planeY,
                                        quantizers[component], componentRow.Slice(x * 64, 64), strength);
                                }
                                for (int x = interiorBlocks; x < widths[component]; x++)
                                {
                                    float strength = adaptive is null ? 0f : strengths[rowOffset + (x * strengthStep)];
                                    rowMasks[x] = ForwardDct.TransformQuantizePaddedPlaneWithMask(rowPlane, rowPlaneWidth, rowPlaneHeight,
                                        x * 8, planeY, quantizers[component], componentRow.Slice(x * 64, 64), strength);
                                }
                                continue;
                            }
                            for (int x = 0; x < interiorBlocks; x++)
                            {
                                float strength = adaptive is null ? 0f : strengths[rowOffset + (x * strengthStep)];
                                rowMasks[x] = ForwardDct.TransformQuantizePlaneWithMask(rowPlane, rowPlaneWidth, x * 8, planeY,
                                    quantizers[component], image.Block(component, x, y), strength);
                            }
                            for (int x = interiorBlocks; x < widths[component]; x++)
                            {
                                float strength = adaptive is null ? 0f : strengths[rowOffset + (x * strengthStep)];
                                rowMasks[x] = ForwardDct.TransformQuantizePaddedPlaneWithMask(rowPlane, rowPlaneWidth, rowPlaneHeight,
                                    x * 8, planeY, quantizers[component], image.Block(component, x, y), strength);
                            }
                            continue;
                        }
                        for (int x = 0; x < widths[component]; x++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            float strength = adaptive is null ? 0f : strengths[(row * adaptive.BlocksWide)
                                + (x * horizontal / factor.Horizontal)];
                            Span<short> quantized = image.Block(component, x, y);
                            if (rateDistortionLambda > 0)
                            {
                                // Retain the original AQ decisions as candidate values, then
                                // search AC only. Histogram collection and emission both read
                                // this same completed cache; the default fused path is unchanged.
                                Downsampling.FillBlock(pixels, width, height, stride, component, x, y,
                                    horizontal, vertical, factor.Horizontal, factor.Vertical, samples, smoothingFactor);
                                PrepareRateDistortionBlock(samples, quantizers[component], tables[component],
                                    quantized, strength, rateDistortionLambda, component != 0, rateDistortionDiagnostics);
                                continue;
                            }
                            if (pixels.ParallelRows is { } parallelRows)
                            {
                                parallelRows.CopyBlock(component, x, y, samples);
                            }
                            else if (!rowPlane.IsEmpty && (x * 8) + 8 <= rowPlaneWidth && planeY + 8 <= rowPlaneHeight)
                            {
                                if (zigZagOrdered)
                                {
                                    ulong mask = ForwardDct.TransformQuantizePlaneWithMask(rowPlane, rowPlaneWidth, x * 8, planeY,
                                        quantizers[component], quantized, strength);
                                    image.blockMasks?[component][(y * widths[component]) + x] = mask;
                                }
                                else
                                {
                                    ForwardDct.TransformQuantizePlane(rowPlane, rowPlaneWidth, x * 8, planeY,
                                        quantizers[component], quantized, strength, zigZagOrdered: false);
                                }
                                continue;
                            }
                            else if (!rowPlane.IsEmpty && zigZagOrdered)
                            {
                                ulong mask = ForwardDct.TransformQuantizePaddedPlaneWithMask(rowPlane, rowPlaneWidth, rowPlaneHeight,
                                    x * 8, planeY, quantizers[component], quantized, strength);
                                image.blockMasks?[component][(y * widths[component]) + x] = mask;
                                continue;
                            }
                            else
                            {
                                Downsampling.FillBlock(pixels, width, height, stride, component, x, y,
                                    horizontal, vertical, factor.Horizontal, factor.Vertical, samples, smoothingFactor);
                                if (zigZagOrdered)
                                {
                                    ulong mask = ForwardDct.TransformQuantizeBlockWithMask(samples, quantizers[component], quantized,
                                        strength);
                                    image.blockMasks?[component][(y * widths[component]) + x] = mask;
                                }
                                else
                                {
                                    ForwardDct.TransformQuantizeBlock(samples, quantizers[component], quantized,
                                        strength, zigZagOrdered: false);
                                }
                                continue;
                            }
                            // Built-in DC zero-bias multipliers are zero, so the full DC
                            // coefficient does not depend on scan order or restart predictors.
                            Span<short> natural = zigZagOrdered ? naturalOrder : quantized;
                            Quantization.QuantizePrepared(samples, quantizers[component], natural,
                                adaptiveStrength: strength);
                            EntropyEncoder.ValidateAc(natural);
                            if (zigZagOrdered)
                            {
                                ZigZagOrdering.Reorder(natural, quantized);
                                image.blockMasks?[component][(y * widths[component]) + x] = EntropyEncoder.NonzeroAcMask(quantized);
                            }
                        }
                    }
                }
            }
            return image;
        }
        catch
        {
            image.Dispose();
            throw;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static QuantizedImage? CreateHuffmanFeedbackResearch(PixelSource pixels, int width, int height,
        int stride, JpegSamplingFactors sampling, ushort[][] tables, float distance, int smoothingFactor,
        AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers, bool zigZagOrdered,
        KernelPreference narrowZigZagKernel, double lambda, RateDistortionDiagnostics? diagnostics,
        long maximumBytes, EncoderCacheBudget? cacheBudget, ArrayPool<short>? pool, double dcLambda,
        DcTrellisDiagnostics? dcDiagnostics, int[]? restartIntervals, CancellationToken cancellationToken,
        Quantization.PreparedTable[]? preparedTables = null)
    {
        if (!double.IsFinite(lambda) || lambda <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(lambda));
        }
        if (restartIntervals is not null && (restartIntervals.Length != tables.Length
            || restartIntervals.AsSpan().ContainsAnyExceptInRange(0, 65535)))
        {
            throw new ArgumentException("Each component requires a valid restart interval.", nameof(restartIntervals));
        }
        QuantizedImage? image = Create(pixels, width, height, stride, sampling, tables, distance, smoothingFactor,
            adaptive, adaptiveMultipliers, cancellationToken, zigZagOrdered, narrowZigZagKernel,
            lambda, diagnostics, maximumBytes, allowPartial: false, cacheBudget: cacheBudget, pool: pool,
            dcTrellisLambda: dcLambda, dcTrellisDiagnostics: dcDiagnostics, dcRestartIntervals: restartIntervals,
            preparedTables: preparedTables);
        if (image is null)
        {
            return null;
        }
        try
        {
            int components = tables.Length;
            var scans = new JpegScan[components];
            long[][] dcFrequencies = new long[components][];
            long[][][] acFrequencies = new long[components][][];
            long histogramBlocks = 0;
            for (int component = 0; component < components; component++)
            {
                scans[component] = new JpegScan([component]);
                dcFrequencies[component] = new long[256];
                acFrequencies[component] = new long[components][];
                for (int context = 0; context < components; context++)
                {
                    acFrequencies[component][context] = component == context ? new long[256] : [];
                }
                int predictor = 0, index = 0, interval = restartIntervals?[component] ?? 0;
                for (int y = 0; y < image.cachedHeights[component]; y++)
                {
                    for (int x = 0; x < image.widths[component]; x++, index++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (interval != 0 && index % interval == 0)
                        {
                            predictor = 0;
                        }
                        EntropyEncoder.AccumulateFrequencies(image.Block(component, x, y), ref predictor,
                            dcFrequencies[component], acFrequencies[component][component],
                            coefficientsValidated: true, zigZagOrdered: true);
                        histogramBlocks++;
                    }
                }
            }
            // This is the same clustered, baseline-limited table plan as output;
            // changing canonical symbol order cannot alter its per-symbol costs.
            var coding = HuffmanCodingPlan.Create(scans, dcFrequencies, acFrequencies, progressive: false,
                forceBaseline: true, reuseHistograms: true);
            byte[][] codeLengths = new byte[components][];
            for (int component = 0; component < components; component++)
            {
                codeLengths[component] = RateDistortionQuantization.CodeLengths(coding.Scans[component].AcTables[component]);
            }
            diagnostics?.RecordFeedbackModel(codeLengths, histogramBlocks);
            Span<float> samples = stackalloc float[64];
            for (int component = 0; component < components; component++)
            {
                JpegComponentSampling factor = sampling[component];
                for (int y = 0; y < image.cachedHeights[component]; y++)
                {
                    for (int x = 0; x < image.widths[component]; x++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        Downsampling.FillBlock(pixels, width, height, stride, component, x, y,
                            sampling.MaxHorizontal, sampling.MaxVertical, factor.Horizontal, factor.Vertical,
                            samples, smoothingFactor);
                        RefineFeedbackBlock(samples, tables[component], image.Block(component, x, y), lambda,
                            codeLengths[component], diagnostics);
                    }
                }
            }
            return image;
        }
        catch
        {
            image.Dispose();
            throw;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void RefineFeedbackBlock(ReadOnlySpan<float> samples, ReadOnlySpan<ushort> table,
        Span<short> quantized, double lambda, ReadOnlySpan<byte> codeLengths, RateDistortionDiagnostics? diagnostics)
    {
        Span<float> coefficients = stackalloc float[64];
        Span<short> original = stackalloc short[64];
        ForwardDct.Transform(samples, coefficients);
        if (diagnostics is not null)
        {
            quantized.CopyTo(original);
        }
        int evaluations = RateDistortionQuantization.Optimize(coefficients, table, quantized, lambda, codeLengths);
        if (diagnostics is not null)
        {
            int changes = 0;
            double originalError = 0, selectedError = 0;
            for (int position = 1; position < 64; position++)
            {
                changes += quantized[position] == original[position] ? 0 : 1;
                int natural = JpegConstants.ZigZag[position];
                double before = coefficients[natural] - ((double)original[position] * table[natural]);
                double after = coefficients[natural] - ((double)quantized[position] * table[natural]);
                originalError += before * before;
                selectedError += after * after;
            }
            diagnostics.RecordFeedbackBlock(evaluations, changes,
                RateDistortionQuantization.EstimateAcBits(original, codeLengths),
                RateDistortionQuantization.EstimateAcBits(quantized, codeLengths), originalError, selectedError);
        }
    }

    // A second transform pass obtains real floating DC targets without changing
    // the ordinary fused preparation loop's locals, stack frame or scheduling.
    // This research path supports complete component-raster sequential scans.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static QuantizedImage? CreateDcResearch(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables, float distance, int smoothingFactor,
        AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers,
        bool zigZagOrdered, KernelPreference narrowZigZagKernel, double rateDistortionLambda,
        RateDistortionDiagnostics? rateDistortionDiagnostics, long maximumBytes, EncoderCacheBudget? cacheBudget,
        ArrayPool<short>? pool, double lambda, DcTrellisDiagnostics? diagnostics, int[]? restartIntervals,
        CancellationToken cancellationToken, Quantization.PreparedTable[]? preparedTables = null)
    {
        if (!zigZagOrdered)
        {
            throw new ArgumentException("DC research requires zigzag-ordered cache blocks.", nameof(zigZagOrdered));
        }
        if (restartIntervals is not null && (restartIntervals.Length != tables.Length
            || restartIntervals.AsSpan().ContainsAnyExceptInRange(0, 65535)))
        {
            throw new ArgumentException("Each component requires a valid restart interval.", nameof(restartIntervals));
        }
        int maximumBlocks = 0;
        for (int component = 0; component < tables.Length; component++)
        {
            JpegComponentSampling factor = sampling[component];
            long columns = (((long)width * factor.Horizontal) + (8 * sampling.MaxHorizontal) - 1) / (8 * sampling.MaxHorizontal);
            long rows = (((long)height * factor.Vertical) + (8 * sampling.MaxVertical) - 1) / (8 * sampling.MaxVertical);
            long count = columns * rows;
            if (count > DcRateDistortionQuantization.MaximumBlocks)
            {
                return null;
            }
            maximumBlocks = Math.Max(maximumBlocks, (int)count);
        }
        // Reused float targets, short DC output, bool resets, and the optimizer's
        // three byte backpointers. Account the peak even with no shared budget.
        long scratchBytes = (long)maximumBlocks * (sizeof(float) + sizeof(short) + sizeof(bool) + 3);
        long available = Math.Min(Math.Min(maximumBytes, MaximumBytes), cacheBudget?.AvailableBytes ?? long.MaxValue);
        if (scratchBytes > available || (cacheBudget is not null
            && !cacheBudget.TryReserve(scratchBytes, EncoderCacheKind.QuantizedCoefficients)))
        {
            return null;
        }
        QuantizedImage? image = null;
        try
        {
            image = Create(pixels, width, height, stride, sampling, tables, distance, smoothingFactor,
                adaptive, adaptiveMultipliers, cancellationToken, zigZagOrdered, narrowZigZagKernel,
                rateDistortionLambda, rateDistortionDiagnostics, maximumBytes: available - scratchBytes,
                allowPartial: false, cacheBudget: cacheBudget, pool: pool, preparedTables: preparedTables);
            if (image is null)
            {
                return null;
            }
            float[] source = new float[maximumBlocks];
            short[] dc = new short[maximumBlocks];
            bool[] resets = new bool[maximumBlocks];
            Span<float> samples = stackalloc float[64], coefficients = stackalloc float[64];
            for (int component = 0; component < tables.Length; component++)
            {
                JpegComponentSampling factor = sampling[component];
                int count = image.widths[component] * image.cachedHeights[component];
                int interval = restartIntervals?[component] ?? 0;
                int index = 0;
                for (int y = 0; y < image.cachedHeights[component]; y++)
                {
                    for (int x = 0; x < image.widths[component]; x++, index++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        Downsampling.FillBlock(pixels, width, height, stride, component, x, y,
                            sampling.MaxHorizontal, sampling.MaxVertical, factor.Horizontal, factor.Vertical,
                            samples, smoothingFactor);
                        ForwardDct.Transform(samples, coefficients);
                        source[index] = coefficients[0];
                        dc[index] = image.Block(component, x, y)[0];
                        resets[index] = interval != 0 && index % interval == 0;
                    }
                }
                DcRateDistortionQuantization.Diagnostics result = DcRateDistortionQuantization.Optimize(
                    source.AsSpan(0, count), tables[component][0], dc.AsSpan(0, count),
                    DcRateDistortionQuantization.StandardCodeLengths(component != 0), resets.AsSpan(0, count),
                    lambda, cancellationToken);
                diagnostics?.RecordChain(count, result);
                index = 0;
                for (int y = 0; y < image.cachedHeights[component]; y++)
                {
                    for (int x = 0; x < image.widths[component]; x++)
                    {
                        image.Block(component, x, y)[0] = dc[index++];
                    }
                }
            }
            return image;
        }
        catch
        {
            image?.Dispose();
            throw;
        }
        finally
        {
            cacheBudget?.Release(scratchBytes, EncoderCacheKind.QuantizedCoefficients);
        }
    }

    // Keep the opt-in search's scratch and live state out of the ordinary cache
    // loop. Conditional stack allocations otherwise change that method's frame
    // even when the experiment is disabled for every block.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PrepareRateDistortionBlock(ReadOnlySpan<float> samples,
        Quantization.PreparedTable quantizer, ReadOnlySpan<ushort> table, Span<short> quantized,
        float strength, double lambda, bool chroma, RateDistortionDiagnostics? diagnostics)
    {
        Span<float> coefficients = stackalloc float[64];
        Span<short> natural = stackalloc short[64];
        Span<short> original = stackalloc short[64];
        ForwardDct.Transform(samples, coefficients);
        Quantization.QuantizePrepared(coefficients, quantizer, natural, adaptiveStrength: strength);
        EntropyEncoder.ValidateAc(natural);
        ZigZagOrdering.Reorder(natural, quantized);
        if (diagnostics is not null)
        {
            quantized.CopyTo(original);
        }
        int evaluations = RateDistortionQuantization.Optimize(coefficients, table, quantized, lambda, chroma);
        if (diagnostics is not null)
        {
            int changes = 0;
            for (int coefficient = 1; coefficient < 64; coefficient++)
            {
                if (quantized[coefficient] != original[coefficient])
                {
                    changes++;
                }
            }
            diagnostics.RecordBlock(evaluations, changes);
        }
    }

    // Plans only capacity; no pixels, coefficients or image-sized arrays are read.
    internal static long PlanFullCacheReservation(int width, int height, JpegSamplingFactors sampling,
        int componentCount, long availableBytes)
    {
        // The ordinary 192 MiB budget already admits both 64 MiB caches.
        // Reserve only complete coefficient storage under constrained budgets;
        // partial-cache priorities still use the existing streaming fallback.
        if (availableBytes is <= 0 or >= 2 * MaximumBytes)
        {
            return 0;
        }
        Span<int> widths = stackalloc int[componentCount];
        Span<int> heights = stackalloc int[componentCount];
        for (int component = 0; component < componentCount; component++)
        {
            JpegComponentSampling factor = sampling[component];
            widths[component] = (int)((((long)width * factor.Horizontal) + (8 * sampling.MaxHorizontal) - 1)
                / (8 * sampling.MaxHorizontal));
            heights[component] = (int)((((long)height * factor.Vertical) + (8 * sampling.MaxVertical) - 1)
                / (8 * sampling.MaxVertical));
        }
        int mcuRows = (height + (sampling.MaxVertical * 8) - 1) / (sampling.MaxVertical * 8);
        long bytes = Math.Min(PlannedBytes(widths, heights, mcuRows, sampling, segmented: false),
            PlannedBytes(widths, heights, mcuRows, sampling, segmented: true));
        return bytes <= Math.Min(availableBytes, MaximumBytes) ? bytes : 0;
    }

    internal static long PlannedBytes(ReadOnlySpan<int> widths, ReadOnlySpan<int> heights, int mcuRows,
        JpegSamplingFactors sampling, bool segmented)
    {
        long bytes = 0;
        for (int component = 0; component < widths.Length; component++)
        {
            long rows = Math.Min(heights[component], (long)mcuRows * sampling[component].Vertical);
            ulong length = (ulong)(widths[component] * rows * 64);
            if (segmented)
            {
                bytes += (long)(length / SegmentLength) * SegmentLength * sizeof(short);
                length %= SegmentLength;
            }
            if (length != 0)
            {
                bytes += (long)Math.Max(16UL, BitOperations.RoundUpToPowerOf2(length)) * sizeof(short);
            }
        }
        return bytes;
    }

    private bool AllocateComponent(int component, int length, long maximumBytes, CancellationToken cancellationToken)
    {
        if (this.segments is null)
        {
            short[]? rental = this.Rent(length, maximumBytes);
            if (rental is null)
            {
                return false;
            }
            this.components[component] = rental;
            return true;
        }

        int count = ((length - 1) / SegmentLength) + 1;
        short[][] chunks = new short[count][];
        this.segments[component] = chunks;
        for (int index = 0; index < count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            short[]? rental = this.Rent(Math.Min(SegmentLength, length - (index * SegmentLength)), maximumBytes);
            if (rental is null)
            {
                return false;
            }
            chunks[index] = rental;
        }
        return true;
    }

    private short[]? Rent(int length, long maximumBytes)
    {
        short[] rental = this.pool.Rent(length);
        long bytes = (long)rental.Length * sizeof(short);
        if (bytes > maximumBytes - this.RentedBytes
            || (this.cacheBudget is not null && !this.cacheBudget.TryReserve(bytes, EncoderCacheKind.QuantizedCoefficients)))
        {
            this.pool.Return(rental);
            return null;
        }
        this.RentedBytes += bytes;
        return rental;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Span<short> Block(int component, int x, int y)
    {
        // Accepted cache capacity bounds every valid block offset below Int32.MaxValue.
        // Callers visit only admitted rows and real component columns.
        int offset = ((y * this.widths[component]) + x) * 64;
        return this.segments is null ? this.components[component].AsSpan(offset, 64)
            : this.segments[component][offset / SegmentLength].AsSpan(offset % SegmentLength, 64);
    }

    internal ReadOnlySpan<short> GetBlock(int component, int x, int y)
        => this.Block(component, x, y);

    public void Dispose()
    {
        for (int component = 0; component < this.components.Length; component++)
        {
            if (this.components[component] is { } buffer)
            {
                this.components[component] = null!;
                this.pool.Return(buffer);
            }
            if (this.segments?[component] is { } chunks)
            {
                this.segments[component] = null!;
                foreach (short[]? chunk in chunks)
                {
                    if (chunk is not null)
                    {
                        this.pool.Return(chunk);
                    }
                }
            }
            if (this.blockMasks?[component] is { } masks)
            {
                this.blockMasks[component] = null!;
                ArrayPool<ulong>.Shared.Return(masks);
            }
        }
        this.cacheBudget?.Release(this.RentedBytes, EncoderCacheKind.QuantizedCoefficients);
        this.RentedBytes = 0;
    }
}
