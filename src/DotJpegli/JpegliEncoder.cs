namespace DotJpegli;

using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using DotJpegli.Internal;

/// <summary>
/// Encodes byte, 16-bit or floating-point RGB, grayscale or YCbCr pixels
/// to eight-bit baseline or progressive grayscale, YCbCr or RGB JPEG.
/// Converts sRGB/grayscale to scaled XYB or accepts already scaled XYB input,
/// with a matching caller-supplied output ICC profile. Includes spatial adaptive quantization.
/// SourceIccProfile optionally converts RGB/grayscale source samples into the output profile.
/// </summary>
public static class JpegliEncoder
{
    private static readonly int[] ZeroRestartIntervals1 = [0];
    private static readonly int[] ZeroRestartIntervals3 = [0, 0, 0];
    private static readonly HuffmanTable[] EmptyTables1 = new HuffmanTable[1];
    private static readonly HuffmanTable[] EmptyTables3 = new HuffmanTable[3];

    [ThreadStatic]
    private static long[][]? histogramPool;

    [ThreadStatic]
    private static int histogramPoolCount;

    private static long[] RentHistogram()
    {
        int count = histogramPoolCount;
        if (count > 0 && histogramPool is { } pool)
        {
            int next = count - 1;
            long[] rented = pool[next]!;
            pool[next] = null!;
            histogramPoolCount = next;
            Array.Clear(rented, 0, 256);
            return rented;
        }
        return new long[256];
    }

    private static void ReturnHistogram(long[] histogram)
    {
        if (histogram.Length != 256)
        {
            return;
        }
        long[][] pool = histogramPool ??= new long[32][];
        int count = histogramPoolCount;
        if (count < pool.Length)
        {
            pool[count] = histogram;
            histogramPoolCount = count + 1;
        }
    }

    private static int EstimateInitialOutputCapacity(int width, int height)
        => width is >= 1 and <= 65535 && height is >= 1 and <= 65535
            ? Math.Clamp((int)Math.Min((long)width * height / 2, 1024 * 1024), 4096, 1024 * 1024)
            : 4096;

    /// <summary>
    /// Maps Jpegli quality (finite 0..100) to its perceptual quantization distance.
    /// Supports fractional quality using the pinned upstream polynomial; quality 100
    /// maps to 0.01, which remains lossy. This is not libjpeg's quality scale or a
    /// measured perceptual error. The result can be supplied as JpegliEncoderOptions.Distance.
    /// </summary>
    public static float QualityToDistance(float quality) => Quantization.QualityToDistance(quality);

    /// <summary>Encodes an image and returns its JPEG bytes.</summary>
    /// <param name="pixels">Source image samples in the selected layout.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="layout">Source component order and channel layout.</param>
    /// <param name="options">Encoder settings, or null for defaults.</param>
    /// <param name="stride">Bytes between source rows, or zero for tightly packed rows.</param>
    /// <param name="cancellationToken">Cancellation for validation and encoding.</param>
    public static byte[] Encode(ReadOnlySpan<byte> pixels, int width, int height,
        PixelLayout layout = PixelLayout.Rgb24, JpegliEncoderOptions? options = null,
        int stride = 0, CancellationToken cancellationToken = default)
    {
        using var output = new PooledOutputStream(initialCapacity: EstimateInitialOutputCapacity(width, height));
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
        => EncodeCore(output, new PixelSource(pixels, layout), width, height, options, stride, cancellationToken);

    /// <summary>
    /// Encodes RGB/grayscale, full-range JPEG YCbCr or already scaled XYB 16-bit samples.
    /// Values 0..65535 map to JPEG sample units 0..255; neutral YCbCr chroma is 32896.
    /// Without SourceIccProfile, non-XYB output retains the source transfer function;
    /// use a matching output ICC profile. Explicit XYB conversion then requires gamma-encoded sRGB input.
    /// Fractional precision is retained until quantization; output JPEG sample precision is eight bits.
    /// </summary>
    /// <param name="pixels">Source image samples in the selected layout.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="layout">Source component order and channel layout.</param>
    /// <param name="options">Encoder settings, or null for defaults.</param>
    /// <param name="stride">Source elements between rows, or zero for tightly packed rows.</param>
    /// <param name="cancellationToken">Cancellation for validation and encoding.</param>
    public static byte[] Encode(ReadOnlySpan<ushort> pixels, int width, int height,
        PixelComponentLayout layout = PixelComponentLayout.Rgb, JpegliEncoderOptions? options = null,
        int stride = 0, CancellationToken cancellationToken = default)
    {
        using var output = new PooledOutputStream(initialCapacity: EstimateInitialOutputCapacity(width, height));
        Encode(output, pixels, width, height, layout, options, stride, cancellationToken);
        return output.ToArray();
    }

    /// <summary>
    /// Writes an eight-bit JPEG from RGB/grayscale, full-range JPEG YCbCr or already scaled XYB 16-bit samples
    /// without closing or seeking the stream.
    /// Source pixels must remain unchanged until encoding returns. Cancellation or I/O errors may leave partial output.
    /// </summary>
    /// <param name="output">The writable destination stream.</param>
    /// <param name="pixels">Source image samples in the selected layout.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="layout">Source component order and channel layout.</param>
    /// <param name="options">Encoder settings, or null for defaults.</param>
    /// <param name="stride">Source elements between rows, or zero for tightly packed rows.</param>
    /// <param name="cancellationToken">Cancellation for validation and encoding.</param>
    public static void Encode(Stream output, ReadOnlySpan<ushort> pixels, int width, int height,
        PixelComponentLayout layout = PixelComponentLayout.Rgb, JpegliEncoderOptions? options = null,
        int stride = 0, CancellationToken cancellationToken = default)
        => EncodeCore(output, new PixelSource(pixels, layout), width, height, options, stride, cancellationToken);

    /// <summary>
    /// Encodes RGB/grayscale, full-range JPEG YCbCr or already scaled XYB floating-point samples.
    /// Consumed samples must be finite and in 0..1 by default; neutral YCbCr chroma is 128f / 255f.
    /// AllowOutOfRangeFloatSamples permits out-of-range values that fit eight-bit JPEG coefficients.
    /// Alpha, row padding and trailing elements are ignored. Explicit XYB output linearizes sRGB
    /// before its opsin transform. SourceIccProfile enables source-to-destination ICC conversion.
    /// Otherwise non-XYB output retains the source transfer function; use a matching output ICC profile.
    /// Fractional precision is retained until quantization; output JPEG sample precision is eight bits.
    /// </summary>
    /// <param name="pixels">Source image samples in the selected layout.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="layout">Source component order and channel layout.</param>
    /// <param name="options">Encoder settings, or null for defaults.</param>
    /// <param name="stride">Source elements between rows, or zero for tightly packed rows.</param>
    /// <param name="cancellationToken">Cancellation for validation and encoding.</param>
    public static byte[] Encode(ReadOnlySpan<float> pixels, int width, int height,
        PixelComponentLayout layout = PixelComponentLayout.Rgb, JpegliEncoderOptions? options = null,
        int stride = 0, CancellationToken cancellationToken = default)
    {
        using var output = new PooledOutputStream(initialCapacity: EstimateInitialOutputCapacity(width, height));
        Encode(output, pixels, width, height, layout, options, stride, cancellationToken);
        return output.ToArray();
    }

    /// <summary>
    /// Writes an eight-bit JPEG from RGB/grayscale, full-range JPEG YCbCr or already scaled XYB floating-point samples
    /// without closing or seeking the stream.
    /// Samples must be in 0..1 unless AllowOutOfRangeFloatSamples is enabled.
    /// Consumed samples are validated before output. Source pixels must remain unchanged until encoding returns.
    /// Alpha and padding are ignored. Cancellation or I/O errors may leave partial output.
    /// </summary>
    /// <param name="output">The writable destination stream.</param>
    /// <param name="pixels">Source image samples in the selected layout.</param>
    /// <param name="width">Image width in pixels.</param>
    /// <param name="height">Image height in pixels.</param>
    /// <param name="layout">Source component order and channel layout.</param>
    /// <param name="options">Encoder settings, or null for defaults.</param>
    /// <param name="stride">Source elements between rows, or zero for tightly packed rows.</param>
    /// <param name="cancellationToken">Cancellation for validation and encoding.</param>
    public static void Encode(Stream output, ReadOnlySpan<float> pixels, int width, int height,
        PixelComponentLayout layout = PixelComponentLayout.Rgb, JpegliEncoderOptions? options = null,
        int stride = 0, CancellationToken cancellationToken = default)
        => EncodeCore(output, new PixelSource(pixels, layout), width, height, options, stride, cancellationToken);

    [SuppressMessage("Usage", "CA2208:Instantiate argument exceptions correctly", Justification = "Option validation identifies the invalid public option property in ParamName.")]
    private static void EncodeCore(Stream output, PixelSource pixels, int width, int height,
        JpegliEncoderOptions? options, int stride, CancellationToken cancellationToken)
    {
        Guard.ThrowIfNull(output);
        if (!output.CanWrite)
        {
            throw new ArgumentException("The destination stream must be writable.", nameof(output));
        }

        if (options is null)
        {
            options = JpegliEncoderOptions.Default;
        }
        else
        {
            options.Validate();
        }
        var cacheBudget = new EncoderCacheBudget(options.MaxCacheBytes, options.CacheDiagnostics);
        pixels = pixels.WithColorSpace(options.ColorSpace, options.ChromaDownsamplingSharpness, options.TailZeroRunBias);
        if (pixels.IsXyb && options.IccProfile.IsEmpty && options.SourceIccProfile.IsEmpty)
        {
            throw new ArgumentException("Scaled XYB encoding requires an ICC profile describing the stored channels.",
                nameof(JpegliEncoderOptions.IccProfile));
        }

        if (width is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "JPEG dimensions must be between 1 and 65535.");
        }

        if (height is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "JPEG dimensions must be between 1 and 65535.");
        }

        if ((long)width * height > options.MaxPixels)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "The image exceeds the configured pixel limit.");
        }

        int rowElements = checked(width * pixels.ComponentsPerPixel);
        if (stride == 0)
        {
            stride = rowElements;
        }

        if (stride < rowElements)
        {
            throw new ArgumentOutOfRangeException(nameof(stride), "Stride must hold a complete row.");
        }

        if (((long)(height - 1) * stride) + rowElements > pixels.Length)
        {
            throw new ArgumentException("The source buffer does not contain all image rows.", nameof(pixels));
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (options.MetadataMarkers is not null)
        {
            options = options with { MetadataMarkers = options.PrepareMetadataMarkers(cancellationToken) };
        }

        bool hasOutOfRangeFloatSamples = pixels.ValidateSamples(width, height, stride, cancellationToken,
            options.AllowOutOfRangeFloatSamples && options.SourceIccProfile.IsEmpty);
        if (!options.SourceIccProfile.IsEmpty)
        {
            if (!pixels.CanManageSourceColor)
            {
                throw new ArgumentException("Source ICC conversion requires RGB/BGR or grayscale input, not direct YCbCr or scaled XYB.",
                    nameof(JpegliEncoderOptions.SourceIccProfile));
            }

            byte[] destination = options.IccProfile.IsEmpty
                ? pixels.ColorSpace switch
                {
                    JpegColorSpace.Xyb => JpegliIccProfiles.CreateXybProfile(),
                    JpegColorSpace.Grayscale => JpegliIccProfiles.CreateSrgbGrayscaleProfile(),
                    JpegColorSpace.Automatic => JpegliIccProfiles.CreateSrgbProfile(),
                    JpegColorSpace.YCbCr => JpegliIccProfiles.CreateSrgbProfile(),
                    JpegColorSpace.Rgb => JpegliIccProfiles.CreateSrgbProfile(),
                    _ => JpegliIccProfiles.CreateSrgbProfile()
                }
                : options.IccProfile.ToArray();
            if (pixels.IsXyb && !destination.AsSpan().SequenceEqual(JpegliIccProfiles.CreateXybProfile()))
            {
                throw new ArgumentException("Source ICC conversion to XYB requires the built-in scaled XYB output profile.",
                    nameof(JpegliEncoderOptions.IccProfile));
            }

            byte[] transformDestination = pixels.IsXyb ? JpegliIccProfiles.CreateSrgbProfile() : destination;
            var transform = IccColorTransform.Create(options.SourceIccProfile.Span, transformDestination,
                pixels.IsGrayscaleInput, pixels.IsGrayscale, options.ColorRenderingIntent, cancellationToken);
            pixels = pixels.WithColorTransform(transform);
            options = options with { IccProfile = destination };
            // Preparing immutable planes also preflights every nonlinear source
            // conversion. Reuse those values instead of evaluating the transform
            // once for validation and again for the cached samples.
            pixels = pixels.WithPreflightedColorPlanes(width, height, stride, cacheBudget, cancellationToken);
        }
        using IDisposable? preflightStorage = pixels.ConvertedStorage;
        var extendedXmp = ExtendedXmpMetadata.Prepare(options, cancellationToken);

        int transferFunction = options.CicpTransferFunction;
        if (options.InferCicpTransferFunctionFromIccProfile && transferFunction == 2)
        {
            transferFunction = IccProfile.LookupTransferFunction(options.IccProfile.Span, cancellationToken);
        }

        bool grayscale = pixels.IsGrayscale;
        bool rgb = pixels.IsRgb;
        bool xyb = pixels.IsXyb;
        int componentCount = grayscale ? 1 : 3;
        var scanConfiguration = JpegScanConfiguration.FromOptions(options);
        JpegScanConfiguration.Prepared? preparedScript = scanConfiguration.PrepareScript(componentCount, cancellationToken);
        ushort[][]? customTables = options.PrepareQuantizationTables(componentCount);
        JpegSamplingFactors sampling = grayscale
            ? JpegSamplingFactors.FromSubsampling(ChromaSubsampling.None)
            : options.SamplingFactors ?? (xyb ? JpegSamplingFactors.Xyb
                : JpegSamplingFactors.FromSubsampling(rgb ? ChromaSubsampling.None : options.Subsampling));
        JpegScanConfiguration.Prepared scanSettings = scanConfiguration.Prepare(sampling, grayscale, preparedScript);
        JpegScan[]? scanScript = scanSettings.Script;
        if (options.Progressive != scanSettings.Progressive)
        {
            options = options with { Progressive = scanSettings.Progressive };
        }
        bool interleaveInitialScan = scanSettings.InterleaveInitialScan;
        int mcuWidth = 8 * sampling.MaxHorizontal;
        int interleavedRestartInterval = options.ResolveRestartInterval((width + mcuWidth - 1) / mcuWidth);
        int[] componentRestartIntervals;
        if (options.RestartInterval == 0 && options.RestartRows == 0)
        {
            componentRestartIntervals = componentCount == 1 ? ZeroRestartIntervals1 : ZeroRestartIntervals3;
        }
        else
        {
            componentRestartIntervals = new int[componentCount];
            for (int component = 0; component < componentCount; component++)
            {
                int blocksWide = ((width * sampling[component].Horizontal) + mcuWidth - 1) / mcuWidth;
                componentRestartIntervals[component] = options.ResolveRestartInterval(blocksWide);
            }
        }
        int[] acScanEnds = scanSettings.AcScanEnds;
        int[] acRefinementBits = scanSettings.AcRefinementBits;
        int maxRefinementBits = scanSettings.MaxRefinementBits;
        // Quantization's sampling arguments select the upstream 4:2:0 table
        // compensation. Asymmetric schemes use uncompensated tables even when
        // the luma factors are 2x2.
        int quantizationSampling = !rgb && sampling.Is420 ? 2 : 1;
        float distance = options.Distance ?? QualityToDistance(options.Quality);
        // ProcessCompressionParams uses Y for grayscale/YCbCr and green for RGB.
        // Disable analysis when that component is subsampled in either direction.
        JpegComponentSampling analysisFactor = sampling[pixels.AnalysisComponent];
        bool useAdaptiveQuantization = options.AdaptiveQuantization
            && analysisFactor.Horizontal == sampling.MaxHorizontal && analysisFactor.Vertical == sampling.MaxVertical;
        using AdaptiveQuantization? adaptive = useAdaptiveQuantization
            ? new AdaptiveQuantization(width, height, sampling.MaxVertical, ArrayPool<float>.Shared,
                options.FuzzyErosionKernel, options.AqRationalKernel, options.AqRationalDiagnostics) : null;
        ushort[][] tables;
        int[] quantizationSelectors;
        float[][]? adaptiveMultipliers;
        Quantization.PreparedTable[] preparedTables;
        if (customTables is null)
        {
            Quantization.Configuration quantConfig = Quantization.GetOrCreateConfiguration(
                distance, grayscale, rgb, xyb, options.UseStandardQuantizationTables, options.ForceBaseline,
                quantizationSampling, transferFunction, useAdaptiveQuantization, options.NarrowZigZagKernel,
                width, height, options.TailZeroRunBias);
            tables = quantConfig.Tables;
            quantizationSelectors = quantConfig.Selectors;
            adaptiveMultipliers = quantConfig.AdaptiveMultipliers;
            preparedTables = quantConfig.PreparedTables;
        }
        else
        {
            tables = new ushort[componentCount][];
            quantizationSelectors = new int[componentCount];
            for (int component = 0; component < componentCount; component++)
            {
                int selector = Math.Min(component, customTables.Length - 1);
                quantizationSelectors[component] = selector;
                tables[component] = customTables[selector];
            }
            adaptiveMultipliers = useAdaptiveQuantization
                ? Quantization.CreateAdaptiveZeroBiasMultipliers(tables, quantizationSampling, quantizationSampling, grayscale,
                    cicpTransferFunction: transferFunction, rgb: rgb, quantizationSelectors: quantizationSelectors,
                    forceBaseline: options.ForceBaseline) : null;
            preparedTables = new Quantization.PreparedTable[componentCount];
            for (int component = 0; component < componentCount; component++)
            {
                preparedTables[component] = Quantization.PreparedTable.Create(tables[component], component, distance,
                    grayscale: grayscale, adaptiveQuantization: useAdaptiveQuantization,
                    adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                    rgb: rgb, narrowZigZagKernel: options.NarrowZigZagKernel, imageWidth: width, imageHeight: height,
                    tailZeroRunBias: options.TailZeroRunBias);
            }
        }

        if (hasOutOfRangeFloatSamples)
        {
            // Validate full coefficients with baseline MCU ordering and restart
            // resets, even when the selected progressive preview shifts DC bits.
            // Recompute blocks instead of storing a full-image coefficient buffer.
            HuffmanTable[] emptyValidationTables = grayscale || rgb ? EmptyTables1 : EmptyTables3;
            int validationTableCount = grayscale || rgb ? 1 : 2;
            long[][] dcValidation = new long[validationTableCount][];
            long[][] acValidation = new long[validationTableCount][];
            for (int index = 0; index < validationTableCount; index++)
            {
                dcValidation[index] = RentHistogram();
                acValidation[index] = RentHistogram();
            }
            try
            {
                ProcessBlocks(pixels, width, height, stride, sampling,
                    tables, distance, options.SmoothingFactor, interleavedRestartInterval, adaptive, adaptiveMultipliers,
                    emptyValidationTables, emptyValidationTables, null, dcValidation, acValidation, cancellationToken,
                    requireEightBitCoefficients: true, preparedTables: preparedTables);
            }
            catch (ArgumentException error) when (error.ParamName is "coefficients" or "adaptiveStrength")
            {
                throw new ArgumentException("The floating-point pixels cannot be encoded with finite computations and eight-bit JPEG coefficient limits using these settings.",
                    nameof(pixels), error);
            }
            finally
            {
                for (int index = 0; index < validationTableCount; index++)
                {
                    ReturnHistogram(dcValidation[index]);
                    ReturnHistogram(acValidation[index]);
                }
            }
        }
        int workerLimit = options.MaxDegreeOfParallelism == -1
            ? Environment.ProcessorCount : options.MaxDegreeOfParallelism;
        JpegScan[]? optimizedScans = options.OptimizeHuffmanTables && !rgb
            ? (scanScript ?? scanSettings.CreateDefaultScript(componentCount)) : null;
        if (CanUseRollingConvertedPlanes(pixels, width, height, sampling, componentCount, workerLimit,
            options, optimizedScans, cacheBudget))
        {
            pixels = pixels.WithRollingConvertedPlanes(width, height, stride, sampling.Is420, cacheBudget, cancellationToken);
        }
        else
        {
            long coefficientReservation = options.Progressive
                ? QuantizedImage.PlanFullCacheReservation(width, height, sampling, componentCount, cacheBudget.AvailableBytes) : 0;
            cacheBudget.Withhold(coefficientReservation);
            try
            {
                pixels = sampling.Is420 && options.SmoothingFactor == 0 && options.OptimizeHuffmanTables
                    ? pixels.WithConverted420Planes(width, height, stride, cancellationToken, cacheBudget, allowPartial: true)
                    : pixels.WithConvertedPlanes(width, height, stride, cancellationToken, cacheBudget, allowPartial: true);
            }
            finally
            {
                cacheBudget.Withhold(0);
            }
        }
        using IDisposable? convertedStorage = preflightStorage is null ? pixels.ConvertedStorage : null;
        if (workerLimit > 1 && ParallelDctRows.ShouldCreate(width, height, sampling, grayscale))
        {
            pixels = pixels.WithParallelRows(new ParallelDctRows(pixels, width, height, stride,
                sampling, options.SmoothingFactor, workerLimit, cancellationToken));
        }

        if (optimizedScans is not null)
        {
            EncodeOptimizedScans(pixels, width, height, stride, sampling, tables, quantizationSelectors,
                distance, options, extendedXmp, adaptive, adaptiveMultipliers, optimizedScans,
                interleavedRestartInterval, componentRestartIntervals, output, cacheBudget, cancellationToken,
                preparedTables);
            return;
        }
        var dcTables = new HuffmanTable[grayscale || rgb ? 1 : 2];
        var acTables = new HuffmanTable[dcTables.Length];
        // RGB/XYB retain their existing shared Huffman definitions and scan
        // headers while reusing the same full coefficients across both passes.
        // Unoptimized progressive output also benefits from this bounded cache.
        using QuantizedImage? genericQuantizedImage = options.OptimizeHuffmanTables || options.Progressive
            ? QuantizedImage.Create(pixels, width, height, stride, sampling, tables, distance,
                options.SmoothingFactor, adaptive, adaptiveMultipliers, cancellationToken,
                allowPartial: true, cacheBudget: cacheBudget, preparedTables: preparedTables)
            : null;
        if (options.OptimizeHuffmanTables)
        {
            long[][] dcFrequencies = new long[dcTables.Length][];
            long[][] acFrequencies = new long[dcTables.Length][];
            for (int index = 0; index < dcTables.Length; index++)
            {
                dcFrequencies[index] = RentHistogram();
                acFrequencies[index] = RentHistogram();
            }
            try
            {
                if (scanScript is not null)
                {
                    foreach (JpegScan scan in scanScript)
                    {
                        // DC refinement carries raw bits and contributes no Huffman symbols.
                        if (scan.SpectralEnd == 0 && scan.SuccessiveHigh != 0)
                        {
                            continue;
                        }

                        int interval = scan.Components.Count == 1
                            ? componentRestartIntervals[scan.Components[0]] : interleavedRestartInterval;
                        ProcessScan(pixels, width, height, stride, sampling, tables, distance,
                            options.SmoothingFactor, interval, adaptive, adaptiveMultipliers,
                            dcTables, acTables, null, dcFrequencies, acFrequencies,
                            scan, options.Progressive, aggregateEobRuns: true, cancellationToken,
                            quantizedImage: genericQuantizedImage, preparedTables: preparedTables);
                    }
                }
                else if (interleaveInitialScan)
                {
                    ProcessBlocks(pixels, width, height, stride, sampling,
                        tables, distance, options.SmoothingFactor, interleavedRestartInterval, adaptive, adaptiveMultipliers, dcTables, acTables,
                        null, dcFrequencies, acFrequencies, cancellationToken, dcOnly: options.Progressive,
                        successiveLow: scanSettings.DcRefinementBits, quantizedImage: genericQuantizedImage,
                        preparedTables: preparedTables);
                }
                else
                {
                    for (int component = 0; component < componentCount; component++)
                    {
                        ProcessComponentBlocks(pixels, width, height, stride, sampling,
                            tables, distance, options.SmoothingFactor, componentRestartIntervals[component], adaptive, adaptiveMultipliers,
                            component, acTables, null, acFrequencies, aggregateEobRuns: false, cancellationToken,
                            successiveLow: scanSettings.DcRefinementBits, dcOnly: options.Progressive,
                            dcTables: dcTables, dcFrequencies: dcFrequencies, sequential: !options.Progressive,
                            quantizedImage: genericQuantizedImage, preparedTables: preparedTables);
                    }
                }

                if (scanScript is null && options.Progressive)
                {
                    for (int band = 0; band < acScanEnds.Length; band++)
                    {
                        for (int component = 0; component < componentCount; component++)
                        {
                            ProcessComponentBlocks(pixels, width, height, stride, sampling,
                                tables, distance, options.SmoothingFactor, componentRestartIntervals[component], adaptive, adaptiveMultipliers,
                                component, acTables, null, acFrequencies, aggregateEobRuns: true, cancellationToken,
                                successiveLow: acRefinementBits[band],
                                spectralStart: band == 0 ? 1 : acScanEnds[band - 1] + 1, spectralEnd: acScanEnds[band],
                                quantizedImage: genericQuantizedImage, preparedTables: preparedTables);
                        }
                    }
                    // DC refinement emits raw bits, so only AC refinement contributes
                    // additional symbols to the shared optimized Huffman tables.
                    for (int low = maxRefinementBits - 1; low >= 0; low--)
                    {
                        for (int band = 0; band < acScanEnds.Length; band++)
                        {
                            if (acRefinementBits[band] <= low)
                            {
                                continue;
                            }

                            for (int component = 0; component < componentCount; component++)
                            {
                                ProcessComponentBlocks(pixels, width, height, stride, sampling,
                                    tables, distance, options.SmoothingFactor, componentRestartIntervals[component], adaptive, adaptiveMultipliers,
                                    component, acTables, null, acFrequencies,
                                    aggregateEobRuns: true, cancellationToken, successiveLow: low, refine: true,
                                    spectralStart: band == 0 ? 1 : acScanEnds[band - 1] + 1, spectralEnd: acScanEnds[band],
                                    quantizedImage: genericQuantizedImage, preparedTables: preparedTables);
                            }
                        }
                    }
                }
                for (int index = 0; index < dcTables.Length; index++)
                {
                    dcTables[index] = HuffmanTable.CreateOptimized(dcFrequencies[index], options.HuffmanSymbolOrder);
                    acTables[index] = HuffmanTable.CreateOptimized(acFrequencies[index], options.HuffmanSymbolOrder);
                }
            }
            finally
            {
                for (int index = 0; index < dcTables.Length; index++)
                {
                    ReturnHistogram(dcFrequencies[index]);
                    ReturnHistogram(acFrequencies[index]);
                }
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
        JpegMarkerWriter.WriteHeaders(output, width, height, sampling, tables, quantizationSelectors, dcTables, acTables,
            options, extendedXmp, cancellationToken, rgb: rgb);
        using var bits = new BitWriter(output, buffered: true, options.EntropyByteStuffingKernel);
        int lastRestartInterval = 0;
        void WriteRestartInterval(int interval)
        {
            if (interval == lastRestartInterval)
            {
                return;
            }

            JpegMarkerWriter.WriteRestartInterval(output, interval);
            lastRestartInterval = interval;
        }
        if (scanScript is not null)
        {
            foreach (JpegScan scan in scanScript)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int interval = scan.Components.Count == 1
                    ? componentRestartIntervals[scan.Components[0]] : interleavedRestartInterval;
                WriteRestartInterval(interval);
                JpegMarkerWriter.WriteScanHeader(output, scan, options.Progressive, rgb);
                ProcessScan(pixels, width, height, stride, sampling, tables, distance,
                    options.SmoothingFactor, interval, adaptive, adaptiveMultipliers,
                    dcTables, acTables, bits, null, null,
                    scan, options.Progressive, options.OptimizeHuffmanTables, cancellationToken,
                    quantizedImage: genericQuantizedImage, preparedTables: preparedTables);
                bits.Flush();
            }
        }
        else if (interleaveInitialScan)
        {
            WriteRestartInterval(interleavedRestartInterval);
            JpegMarkerWriter.WriteScanHeader(output, componentCount, options.Progressive,
                successiveLow: scanSettings.DcRefinementBits, rgb: rgb);
            ProcessBlocks(pixels, width, height, stride, sampling,
                tables, distance, options.SmoothingFactor, interleavedRestartInterval, adaptive, adaptiveMultipliers, dcTables, acTables,
                bits, null, null, cancellationToken, dcOnly: options.Progressive,
                successiveLow: scanSettings.DcRefinementBits, quantizedImage: genericQuantizedImage,
                preparedTables: preparedTables);
            bits.Flush();
        }
        else
        {
            for (int component = 0; component < componentCount; component++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                WriteRestartInterval(componentRestartIntervals[component]);
                JpegMarkerWriter.WriteScanHeader(output, componentCount, options.Progressive,
                    successiveLow: scanSettings.DcRefinementBits, rgb: rgb, singleComponent: component);
                ProcessComponentBlocks(pixels, width, height, stride, sampling,
                    tables, distance, options.SmoothingFactor, componentRestartIntervals[component], adaptive, adaptiveMultipliers,
                    component, acTables, bits, null, aggregateEobRuns: false, cancellationToken,
                    successiveLow: scanSettings.DcRefinementBits, dcOnly: options.Progressive,
                    dcTables: dcTables, sequential: !options.Progressive, quantizedImage: genericQuantizedImage,
                    preparedTables: preparedTables);
                bits.Flush();
            }
        }

        if (scanScript is null && options.Progressive)
        {
            for (int band = 0; band < acScanEnds.Length; band++)
            {
                int spectralStart = band == 0 ? 1 : acScanEnds[band - 1] + 1;
                for (int component = 0; component < componentCount; component++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    WriteRestartInterval(componentRestartIntervals[component]);
                    JpegMarkerWriter.WriteScanHeader(output, componentCount, true, component,
                        successiveLow: acRefinementBits[band], rgb: rgb,
                        spectralStart: spectralStart, spectralEnd: acScanEnds[band]);
                    ProcessComponentBlocks(pixels, width, height, stride, sampling,
                        tables, distance, options.SmoothingFactor, componentRestartIntervals[component], adaptive, adaptiveMultipliers,
                        component, acTables, bits, null,
                        aggregateEobRuns: options.OptimizeHuffmanTables, cancellationToken,
                        successiveLow: acRefinementBits[band],
                        spectralStart: spectralStart, spectralEnd: acScanEnds[band], quantizedImage: genericQuantizedImage,
                        preparedTables: preparedTables);
                    bits.Flush();
                }
            }
            for (int low = maxRefinementBits - 1; low >= 0; low--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (scanSettings.DcRefinementBits > low)
                {
                    if (interleaveInitialScan)
                    {
                        WriteRestartInterval(interleavedRestartInterval);
                        JpegMarkerWriter.WriteScanHeader(output, componentCount, true,
                            successiveHigh: low + 1, successiveLow: low, rgb: rgb);
                        ProcessBlocks(pixels, width, height, stride, sampling,
                            tables, distance, options.SmoothingFactor, interleavedRestartInterval, adaptive, adaptiveMultipliers, dcTables, acTables,
                            bits, null, null, cancellationToken, dcOnly: true, successiveLow: low, refine: true,
                            quantizedImage: genericQuantizedImage, preparedTables: preparedTables);
                        bits.Flush();
                    }
                    else
                    {
                        for (int component = 0; component < componentCount; component++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            WriteRestartInterval(componentRestartIntervals[component]);
                            JpegMarkerWriter.WriteScanHeader(output, componentCount, true,
                                successiveHigh: low + 1, successiveLow: low, rgb: rgb, singleComponent: component);
                            ProcessComponentBlocks(pixels, width, height, stride, sampling,
                                tables, distance, options.SmoothingFactor, componentRestartIntervals[component], adaptive, adaptiveMultipliers,
                                component, acTables, bits, null, aggregateEobRuns: false, cancellationToken,
                                successiveLow: low, refine: true, dcOnly: true, dcTables: dcTables,
                                quantizedImage: genericQuantizedImage, preparedTables: preparedTables);
                            bits.Flush();
                        }
                    }
                }
                for (int band = 0; band < acScanEnds.Length; band++)
                {
                    if (acRefinementBits[band] <= low)
                    {
                        continue;
                    }

                    int spectralStart = band == 0 ? 1 : acScanEnds[band - 1] + 1;
                    for (int component = 0; component < componentCount; component++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        WriteRestartInterval(componentRestartIntervals[component]);
                        JpegMarkerWriter.WriteScanHeader(output, componentCount, true, component,
                            successiveHigh: low + 1, successiveLow: low, rgb: rgb,
                            spectralStart: spectralStart, spectralEnd: acScanEnds[band]);
                        ProcessComponentBlocks(pixels, width, height, stride, sampling,
                            tables, distance, options.SmoothingFactor, componentRestartIntervals[component], adaptive, adaptiveMultipliers,
                            component, acTables, bits, null,
                            aggregateEobRuns: options.OptimizeHuffmanTables, cancellationToken, successiveLow: low, refine: true,
                            spectralStart: spectralStart, spectralEnd: acScanEnds[band], quantizedImage: genericQuantizedImage,
                            preparedTables: preparedTables);
                        bits.Flush();
                    }
                }
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        output.WriteByte(0xff);
        output.WriteByte(0xd9);
    }

    private static bool CanUseRollingConvertedPlanes(PixelSource pixels, int width, int height,
        JpegSamplingFactors sampling, int componentCount, int workerLimit,
        JpegliEncoderOptions options, JpegScan[]? optimizedScans, EncoderCacheBudget cacheBudget)
    {
        bool is444 = sampling.MaxHorizontal == 1 && sampling.MaxVertical == 1;
        if (height <= 32 || workerLimit != 1 || options.CacheDiagnostics is not null
            || options.SmoothingFactor != 0 || optimizedScans is null
            || options.RateDistortionLambda > 0 || options.DcTrellisLambda > 0
            || pixels.ConvertedStorage is not null
            || (!is444 && !sampling.Is420 && !pixels.IsGrayscale))
        {
            return false;
        }

        const long contiguousPlaneLimitBytes = (8L * 1024 * 1024) + 65536;
        bool use420 = sampling.Is420 && options.OptimizeHuffmanTables;
        long fullPlaneBytes = PixelSource.PlanFullConvertedBytes(width, height, componentCount, use420);
        long rollingBytes = PixelSource.PlanRollingConvertedBytes(width, componentCount, use420);
        bool streamSequential = !options.Progressive && optimizedScans.Length == 1 && componentCount == 3
            && optimizedScans[0].Components is { Count: 3 };
        if (streamSequential)
        {
            long mcusWide = (width + (8 * sampling.MaxHorizontal) - 1) / (8 * sampling.MaxHorizontal);
            long mcusHigh = (height + (8 * sampling.MaxVertical) - 1) / (8 * sampling.MaxVertical);
            long mcus = mcusWide * mcusHigh;
            int blocksPerMcu = (sampling[0].Horizontal * sampling[0].Vertical)
                + (sampling[1].Horizontal * sampling[1].Vertical)
                + (sampling[2].Horizontal * sampling[2].Vertical);
            long worstCaseTokenBytes = (((mcus * blocksPerMcu * 64) + mcus) * sizeof(uint)) + (512 * 1024);
            long tokenLimit = Math.Min(options.EntropyTokenBudgetBytes, EntropyTokenBudget.DefaultMaximumBytes);
            if (fullPlaneBytes <= contiguousPlaneLimitBytes
                && worstCaseTokenBytes <= cacheBudget.AvailableBytes - fullPlaneBytes)
            {
                return false;
            }
            return worstCaseTokenBytes <= tokenLimit
                && worstCaseTokenBytes <= cacheBudget.AvailableBytes - rollingBytes;
        }

        long coefficientBytes = QuantizedImage.PlanFullCacheReservation(width, height, sampling,
            componentCount, cacheBudget.AvailableBytes - rollingBytes);
        if (fullPlaneBytes <= contiguousPlaneLimitBytes
            && coefficientBytes > 0
            && coefficientBytes <= cacheBudget.AvailableBytes - fullPlaneBytes)
        {
            return false;
        }
        return coefficientBytes > 0 && coefficientBytes <= cacheBudget.AvailableBytes - rollingBytes;
    }

    private static void EncodeOptimizedScans(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables, int[] quantizationSelectors, float distance,
        JpegliEncoderOptions options, ExtendedXmpMetadata? extendedXmp,
        AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers, JpegScan[] scans,
        int interleavedRestartInterval, int[] componentRestartIntervals, Stream output,
        EncoderCacheBudget cacheBudget, CancellationToken cancellationToken,
        Quantization.PreparedTable[]? preparedTables = null)
    {
        int componentCount = tables.Length;
        int workerLimit = options.MaxDegreeOfParallelism == -1
            ? Environment.ProcessorCount : options.MaxDegreeOfParallelism;
        // A single sequential scan can retain entropy decisions without storing
        // coefficients. With 256-bit SIMD, separate component-row preparation
        // keeps the fused transform out of the MCU traversal's live state.
        // Its existing pooled-storage budget still falls back to streaming for
        // large images. Wider SIMD, scalar and 128-bit paths keep streaming.
        bool parallelSequential = !options.Progressive && scans.Length == 1 && componentCount == 3
            && scans[0].Components.Count == 3 && options.SamplingFactors is null
            && options.RateDistortionLambda == 0 && options.DcTrellisLambda == 0
            && pixels.ParallelRows is { CanPrepareQuantizedRows: true };
        bool streamSequential = !options.Progressive && scans.Length == 1 && componentCount == 3
            && scans[0].Components.Count == 3 && (pixels.ParallelRows is null || parallelSequential);
        using QuantizedImage? quantizedImage = streamSequential
            && options.RateDistortionLambda == 0 && options.DcTrellisLambda == 0 ? null : QuantizedImage.Create(pixels, width, height, stride,
            sampling, tables, distance, options.SmoothingFactor, adaptive, adaptiveMultipliers, cancellationToken,
            zigZagOrdered: true, narrowZigZagKernel: options.NarrowZigZagKernel,
            rateDistortionLambda: options.RateDistortionLambda, rateDistortionDiagnostics: options.RateDistortionDiagnostics,
            allowPartial: options.RateDistortionLambda == 0 && options.DcTrellisLambda == 0, cacheBudget: cacheBudget,
            dcTrellisLambda: options.DcTrellisLambda, dcTrellisDiagnostics: options.DcTrellisDiagnostics,
            dcRestartIntervals: componentRestartIntervals,
            rateDistortionHuffmanFeedback: options.RateDistortionHuffmanFeedback,
            preparedTables: preparedTables);
        if ((options.RateDistortionLambda > 0 || options.DcTrellisLambda > 0) && quantizedImage is null)
        {
            throw new ArgumentException("The offline rate-distortion experiment requires a coefficient cache within its unchanged 64 MiB limit.",
                nameof(options));
        }
        streamSequential &= quantizedImage is null;
        using EntropyTokenScans tokenScans = new(scans, options.Progressive, options.EntropyTokenBudgetBytes, cacheBudget: cacheBudget);
        long[][] dcFrequencies = new long[componentCount][];
        for (int component = 0; component < componentCount; component++)
        {
            dcFrequencies[component] = RentHistogram();
        }

        long[][][] acFrequencies = new long[scans.Length][][];
        HuffmanCodingPlan plan;
        HuffmanTable[] emptyTables = componentCount == 1 ? EmptyTables1 : EmptyTables3;
        try
        {
            bool fuseProgressiveAc = options.Progressive && ProgressiveAcCollector.ShouldFuse(scans, quantizedImage);
            for (int scanIndex = 0; scanIndex < scans.Length; scanIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                JpegScan scan = scans[scanIndex];
                acFrequencies[scanIndex] = new long[componentCount][];
                for (int component = 0; component < componentCount; component++)
                {
                    acFrequencies[scanIndex][component] = [];
                }
                // DC scans emit no AC symbols; other scans collect AC histograms
                // only for their participating components, including refinement.
                if (scan.SpectralEnd > 0)
                {
                    foreach (int component in scan.Components)
                    {
                        acFrequencies[scanIndex][component] = RentHistogram();
                    }
                }

                // Uncached progressive AC blocks share one component-raster traversal below.
                // DC scans keep their own possibly interleaved MCU traversal.
                if (fuseProgressiveAc && scan.SpectralStart > 0)
                {
                    continue;
                }

                EntropyTokens scanTokens = tokenScans[scanIndex];
                // DC refinement has no histogram, but its raw bits can be replayed.
                if (scan.SpectralEnd == 0 && scan.SuccessiveHigh != 0 && !scanTokens.TryBeginRecording())
                {
                    continue;
                }

                int interval = scan.Components.Count == 1
                    ? componentRestartIntervals[scan.Components[0]] : interleavedRestartInterval;
                if (!options.Progressive && workerLimit > 1 && quantizedImage is not null
                    && ParallelBaselineCollector.TryCollect(quantizedImage, width, height, sampling, scan,
                        interval, workerLimit, dcFrequencies, acFrequencies[scanIndex], scanTokens, cancellationToken))
                {
                    continue;
                }
                if (streamSequential)
                {
                    if (parallelSequential)
                    {
                        ParallelSequentialEncoder.Process(pixels, width, height, stride, sampling, tables, distance,
                            interval, adaptive, adaptiveMultipliers, scanTokens, dcFrequencies, acFrequencies[scanIndex],
                            options.NarrowZigZagKernel, cancellationToken, preparedTables: preparedTables);
                    }
                    else
                    {
                        ProcessSequentialInterleaved(pixels, width, height, stride, sampling, tables, distance,
                            options.SmoothingFactor, interval, adaptive, adaptiveMultipliers,
                            scanTokens, dcFrequencies, acFrequencies[scanIndex], cancellationToken,
                            narrowZigZagKernel: options.NarrowZigZagKernel, preparedTables: preparedTables);
                    }
                }
                else
                {
                    ProcessScan(pixels, width, height, stride, sampling, tables, distance,
                        options.SmoothingFactor, interval, adaptive, adaptiveMultipliers,
                        emptyTables, emptyTables, null, dcFrequencies, acFrequencies[scanIndex],
                        scan, options.Progressive, aggregateEobRuns: true, cancellationToken, contextByComponent: true,
                        quantizedImage: quantizedImage, entropyTokens: scanTokens, preparedTables: preparedTables);
                }
            }
            if (fuseProgressiveAc)
            {
                ProgressiveAcCollector.Collect(pixels, width, height, stride, sampling, tables, distance,
                    options.SmoothingFactor, adaptive, adaptiveMultipliers, scans, acFrequencies,
                    tokenScans, componentRestartIntervals, quantizedImage, cancellationToken,
                    preparedTables: preparedTables);
            }
            plan = HuffmanCodingPlan.Create(scans, dcFrequencies, acFrequencies,
                options.Progressive, options.ForceBaseline, options.HuffmanTableLimit, options.HuffmanSymbolOrder,
                reuseHistograms: true);
        }
        finally
        {
            for (int component = 0; component < componentCount; component++)
            {
                if (dcFrequencies[component] is { Length: 256 } dcHist)
                {
                    ReturnHistogram(dcHist);
                }
            }
            for (int scanIndex = 0; scanIndex < acFrequencies.Length; scanIndex++)
            {
                if (acFrequencies[scanIndex] is not { } scanHists)
                {
                    continue;
                }
                for (int component = 0; component < scanHists.Length; component++)
                {
                    if (scanHists[component] is { Length: 256 } acHist)
                    {
                        ReturnHistogram(acHist);
                    }
                }
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        JpegMarkerWriter.WriteHeaders(output, width, height, sampling, tables, quantizationSelectors,
            emptyTables, emptyTables, options, extendedXmp, cancellationToken, rgb: pixels.IsRgb,
            writeHuffmanTables: false, extendedHuffman: plan.RequiresExtendedSequential);
        using var bits = new BitWriter(output, buffered: true, options.EntropyByteStuffingKernel);
        int lastRestartInterval = 0;
        for (int scanIndex = 0; scanIndex < scans.Length; scanIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            JpegScan scan = scans[scanIndex];
            HuffmanCodingPlan.ScanCoding coding = plan.Scans[scanIndex];
            JpegMarkerWriter.WriteHuffmanTables(output, coding.Definitions);
            int interval = scan.Components.Count == 1
                ? componentRestartIntervals[scan.Components[0]] : interleavedRestartInterval;
            if (interval != lastRestartInterval)
            {
                JpegMarkerWriter.WriteRestartInterval(output, interval);
                lastRestartInterval = interval;
            }
            JpegMarkerWriter.WriteScanHeader(output, scan, options.Progressive, pixels.IsRgb, coding.Selectors);
            if (tokenScans?[scanIndex] is { IsComplete: true } scanTokens)
            {
                if (!scanTokens.TryReplayParallel(output, coding, workerLimit, cancellationToken,
                    stuffingKernel: options.EntropyByteStuffingKernel))
                {
                    scanTokens.Replay(bits, coding, cancellationToken);
                }
                scanTokens.Dispose();
            }
            else if (streamSequential)
            {
                if (parallelSequential)
                {
                    ParallelSequentialEncoder.Process(pixels, width, height, stride, sampling, tables, distance,
                        interval, adaptive, adaptiveMultipliers, null, null, null,
                        options.NarrowZigZagKernel, cancellationToken, bits, coding, preparedTables: preparedTables);
                }
                else
                {
                    ProcessSequentialInterleaved(pixels, width, height, stride, sampling, tables, distance,
                        options.SmoothingFactor, interval, adaptive, adaptiveMultipliers,
                        null, null, null, cancellationToken, bits, coding, options.NarrowZigZagKernel,
                        preparedTables: preparedTables);
                }
            }
            else
            {
                ProcessScan(pixels, width, height, stride, sampling, tables, distance,
                    options.SmoothingFactor, interval, adaptive, adaptiveMultipliers,
                    coding.DcTables, coding.AcTables, bits, null, null,
                    scan, options.Progressive, aggregateEobRuns: true, cancellationToken, contextByComponent: true,
                    quantizedImage: quantizedImage, preparedTables: preparedTables);
            }

            bits.Flush();
        }
        cancellationToken.ThrowIfCancellationRequested();
        output.WriteByte(0xff);
        output.WriteByte(0xd9);
    }

    private readonly record struct SequentialComponent(
        int Horizontal, int Vertical, int BlocksWide, int BlocksHigh,
        int PlaneWidth, int PlaneHeight, bool UsePlane, Quantization.PreparedTable Quantizer);

    private readonly ref struct SequentialBlockSource(
        PixelSource pixels, int width, int height, int stride, int horizontal, int vertical, int smoothingFactor,
        SequentialComponent[] components, ReadOnlySpan<float> plane0, ReadOnlySpan<float> plane1, ReadOnlySpan<float> plane2)
    {
        internal PixelSource Pixels { get; } = pixels;
        internal int Width { get; } = width;
        internal int Height { get; } = height;
        internal int Stride { get; } = stride;
        internal int Horizontal { get; } = horizontal;
        internal int Vertical { get; } = vertical;
        internal int SmoothingFactor { get; } = smoothingFactor;
        internal SequentialComponent[] Components { get; } = components;
        internal ReadOnlySpan<float> Plane0 { get; } = plane0;
        internal ReadOnlySpan<float> Plane1 { get; } = plane1;
        internal ReadOnlySpan<float> Plane2 { get; } = plane2;
    }

    private const int SequentialChunkBlocks = 64;

    // Segment lookup and raw readers have a separate frame so their extra
    // geometry and scratch do not enter the ordinary contiguous-plane path.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static ulong PrepareSequentialFallbackWithMask(in SequentialBlockSource source, int component,
        int componentX, int componentY, float strength, Span<float> floatScratch, Span<short> destination)
    {
        ref readonly SequentialComponent info = ref source.Components[component];
        if (info.UsePlane)
        {
            bool compactChroma = component != 0
                && (info.Horizontal != source.Horizontal || info.Vertical != source.Vertical);
            ReadOnlySpan<float> plane = source.Pixels.GetConvertedPlaneSegment(component, componentY * 8, compactChroma,
                out int planeWidth, out int planeHeight, out int planeY);
            if (!plane.IsEmpty)
            {
                if ((componentX * 8) + 8 <= planeWidth && planeY + 8 <= planeHeight)
                {
                    return ForwardDct.TransformQuantizePlaneWithMask(plane, planeWidth, componentX * 8, planeY,
                        info.Quantizer, destination, strength, clearDcOnlyAc: false);
                }

                return ForwardDct.TransformQuantizePaddedPlaneWithMask(plane, planeWidth, planeHeight,
                    componentX * 8, planeY, info.Quantizer, destination, strength, clearDcOnlyAc: false);
            }
        }
        Span<float> samples = floatScratch[..64];
        Downsampling.FillBlock(source.Pixels, source.Width, source.Height, source.Stride, component,
            componentX, componentY, source.Horizontal, source.Vertical, info.Horizontal, info.Vertical,
            samples, source.SmoothingFactor);
        return ForwardDct.TransformQuantizeBlockWithMask(samples, info.Quantizer, destination, strength,
            clearDcOnlyAc: false);
    }

    // Prepare a horizontal strip of MCUs component-by-component into stack
    // scratch so the DCT/quantizer keeps each component's plane and quantizer
    // hot while all prepared blocks remain resident in L1d for MCU emission.
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private static void PrepareSequentialChunk(in SequentialBlockSource source, int mcuY,
        int chunkStartMcuX, int chunkMcus, ReadOnlySpan<int> chunkBlockOffsets,
        AdaptiveQuantization? adaptive, ReadOnlySpan<float> strengths,
        Span<float> floatScratch, Span<short> chunkCoefficients, Span<ulong> chunkMasks)
    {
        int horizontal = source.Horizontal;
        int strengthStride = adaptive?.BlocksWide ?? 0;
        for (int component = 0; component < 3; component++)
        {
            ref readonly SequentialComponent info = ref source.Components[component];
            ReadOnlySpan<float> contiguousPlane = info.UsePlane
                ? component == 0 ? source.Plane0 : component == 1 ? source.Plane1 : source.Plane2 : default;
            int firstX = chunkStartMcuX * info.Horizontal;
            int firstY = mcuY * info.Vertical;
            int chunkColumns = chunkMcus * info.Horizontal;
            int realColumns = Math.Min(chunkColumns, Math.Max(0, info.BlocksWide - firstX));
            int componentBlockOffset = chunkBlockOffsets[component];
            for (int blockY = 0; blockY < info.Vertical; blockY++)
            {
                int componentY = firstY + blockY;
                if (componentY >= info.BlocksHigh || realColumns == 0)
                {
                    break;
                }

                int strengthRow = blockY * strengthStride;
                int rowBlockBase = componentBlockOffset + (blockY * chunkColumns);
                ReadOnlySpan<float> rowPlane = contiguousPlane;
                int planeWidth = info.PlaneWidth;
                int planeHeight = info.PlaneHeight;
                int top = componentY * 8;
                if (rowPlane.IsEmpty && info.UsePlane)
                {
                    bool compactChroma = component != 0
                        && (info.Horizontal != horizontal || info.Vertical != source.Vertical);
                    rowPlane = source.Pixels.GetConvertedPlaneSegment(component, top, compactChroma,
                        out int segWidth, out int segHeight, out int segTop);
                    planeWidth = segWidth;
                    planeHeight = segHeight;
                    top = segTop;
                }
                if (!rowPlane.IsEmpty)
                {
                    bool interiorY = top + 8 <= planeHeight;
                    int interiorColumns = interiorY
                        ? Math.Clamp((planeWidth / 8) - firstX, 0, realColumns) : 0;
                    for (int columnOffset = 0; columnOffset < interiorColumns; columnOffset++)
                    {
                        int componentX = firstX + columnOffset;
                        float strength = 0f;
                        if (adaptive is not null)
                        {
                            int column = info.Horizontal == horizontal ? componentX
                                : info.Horizontal == 1 ? componentX * horizontal
                                : componentX * horizontal / info.Horizontal;
                            strength = strengths[strengthRow + column];
                        }

                        int blockIndex = rowBlockBase + columnOffset;
                        chunkMasks[blockIndex] = ForwardDct.TransformQuantizePlaneWithMask(
                            rowPlane, planeWidth, componentX * 8, top,
                            info.Quantizer, chunkCoefficients.Slice(blockIndex * 64, 64), strength,
                            clearDcOnlyAc: false);
                    }

                    for (int columnOffset = interiorColumns; columnOffset < realColumns; columnOffset++)
                    {
                        int componentX = firstX + columnOffset;
                        float strength = 0f;
                        if (adaptive is not null)
                        {
                            int column = info.Horizontal == horizontal ? componentX
                                : info.Horizontal == 1 ? componentX * horizontal
                                : componentX * horizontal / info.Horizontal;
                            strength = strengths[strengthRow + column];
                        }

                        int blockIndex = rowBlockBase + columnOffset;
                        chunkMasks[blockIndex] = ForwardDct.TransformQuantizePaddedPlaneWithMask(
                            rowPlane, planeWidth, planeHeight, componentX * 8, top,
                            info.Quantizer, chunkCoefficients.Slice(blockIndex * 64, 64), strength,
                            clearDcOnlyAc: false);
                    }

                    continue;
                }

                for (int columnOffset = 0; columnOffset < realColumns; columnOffset++)
                {
                    int componentX = firstX + columnOffset;
                    float strength = 0f;
                    if (adaptive is not null)
                    {
                        int column = info.Horizontal == horizontal ? componentX
                            : info.Horizontal == 1 ? componentX * horizontal
                            : componentX * horizontal / info.Horizontal;
                        strength = strengths[strengthRow + column];
                    }

                    int blockIndex = rowBlockBase + columnOffset;
                    chunkMasks[blockIndex] = PrepareSequentialFallbackWithMask(in source, component,
                        componentX, componentY, strength, floatScratch, chunkCoefficients.Slice(blockIndex * 64, 64));
                }
            }
        }
    }

    private static void ProcessSequentialInterleaved(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables, float distance, int smoothingFactor, int restartInterval,
        AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers,
        EntropyTokens? tokens, long[][]? dcFrequencies, long[][]? acFrequencies, CancellationToken cancellationToken,
        BitWriter? writer = null, HuffmanCodingPlan.ScanCoding? coding = null,
        KernelPreference narrowZigZagKernel = KernelPreference.Automatic,
        Quantization.PreparedTable[]? preparedTables = null)
    {
        // This path is entered only for a validated full three-component scan,
        // whose component order is 0,1,2. Hoist immutable component state out of
        // the MCU loops rather than sharing the generic scan processor's state.
        int horizontal = sampling.MaxHorizontal, vertical = sampling.MaxVertical;
        int mcuWidth = horizontal * 8, mcuHeight = vertical * 8;
        int mcusWide = (width + mcuWidth - 1) / mcuWidth;
        int mcusHigh = (height + mcuHeight - 1) / mcuHeight;
        ReadOnlySpan<float> plane0 = smoothingFactor == 0 ? pixels.GetConvertedPlane(0) : default;
        ReadOnlySpan<float> plane1 = smoothingFactor == 0 ? pixels.GetConvertedPlane(1) : default;
        ReadOnlySpan<float> plane2 = smoothingFactor == 0 ? pixels.GetConvertedPlane(2) : default;
        int width1 = width, height1 = height, width2 = width, height2 = height;
        bool compact420 = smoothingFactor == 0 && sampling.Is420;
        if (compact420)
        {
            plane1 = pixels.GetConverted420ChromaPlane(1, out width1, out height1);
            plane2 = pixels.GetConverted420ChromaPlane(2, out width2, out height2);
        }
        var components = new SequentialComponent[3];
        int blocksPerMcu = 0;
        for (int component = 0; component < components.Length; component++)
        {
            JpegComponentSampling factor = sampling[component];
            Quantization.PreparedTable quantizer = preparedTables is not null ? preparedTables[component]
                : Quantization.PreparedTable.Create(tables[component], component, distance,
                    adaptiveQuantization: adaptive is not null,
                    adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                    narrowZigZagKernel: narrowZigZagKernel, imageWidth: width, imageHeight: height,
                    tailZeroRunBias: pixels.TailZeroRunBias);
            int blocksWide = ((width * factor.Horizontal) + mcuWidth - 1) / mcuWidth;
            components[component] = new(factor.Horizontal, factor.Vertical,
                blocksWide,
                ((height * factor.Vertical) + mcuHeight - 1) / mcuHeight,
                component == 0 ? width : component == 1 ? width1 : width2,
                component == 0 ? height : component == 1 ? height1 : height2,
                smoothingFactor == 0 && ((factor.Horizontal == horizontal && factor.Vertical == vertical)
                    || (compact420 && component != 0)), quantizer);
            blocksPerMcu += factor.Horizontal * factor.Vertical;
        }

        int maxChunkMcus = Math.Min(mcusWide, Math.Max(1, SequentialChunkBlocks / blocksPerMcu));
        Span<int> chunkBlockOffsets = stackalloc int[4];
        Span<short> chunkCoefficients = stackalloc short[SequentialChunkBlocks * 64];
        Span<ulong> chunkMasks = stackalloc ulong[SequentialChunkBlocks];
        var source = new SequentialBlockSource(pixels, width, height, stride, horizontal, vertical, smoothingFactor,
            components, plane0, plane1, plane2);
        Span<float> floatScratch = stackalloc float[64];
        Span<int> previousDc = stackalloc int[3];
        previousDc.Clear();
        Span<float> strengths = adaptive is null ? default : adaptive.Strengths;
        int mcusRemaining = restartInterval, restartIndex = 0;
        for (int mcuY = 0; mcuY < mcusHigh; mcuY++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            pixels.AdvanceConvertedWindow((mcuY * mcuHeight) + mcuHeight + 6, stride, cancellationToken);
            adaptive?.ComputeRow(pixels, stride, mcuY * mcuHeight,
                tables[pixels.AnalysisComponent][1], strengths!, cancellationToken);
            for (int chunkStartMcuX = 0; chunkStartMcuX < mcusWide; chunkStartMcuX += maxChunkMcus)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int chunkMcus = Math.Min(maxChunkMcus, mcusWide - chunkStartMcuX);
                chunkBlockOffsets[0] = 0;
                for (int component = 0; component < 3; component++)
                {
                    ref readonly SequentialComponent info = ref components[component];
                    chunkBlockOffsets[component + 1] = chunkBlockOffsets[component]
                        + (chunkMcus * info.Horizontal * info.Vertical);
                }

                PrepareSequentialChunk(in source, mcuY, chunkStartMcuX, chunkMcus, chunkBlockOffsets,
                    adaptive, strengths, floatScratch, chunkCoefficients, chunkMasks);

                for (int mcuOffset = 0; mcuOffset < chunkMcus; mcuOffset++)
                {
                    int mcuX = chunkStartMcuX + mcuOffset;
                    if (restartInterval != 0 && mcusRemaining == 0)
                    {
                        if (writer is null)
                        {
                            tokens?.AddRestart(restartIndex);
                        }
                        else
                        {
                            writer.WriteRestartMarker(restartIndex);
                        }

                        previousDc.Clear();
                        restartIndex = (restartIndex + 1) & 7;
                        mcusRemaining = restartInterval;
                    }
                    for (int component = 0; component < 3; component++)
                    {
                        ref readonly SequentialComponent info = ref components[component];
                        int firstX = mcuX * info.Horizontal, firstY = mcuY * info.Vertical;
                        int chunkColumns = chunkMcus * info.Horizontal;
                        int chunkColumnBase = mcuOffset * info.Horizontal;
                        int componentBlockOffset = chunkBlockOffsets[component];
                        for (int blockY = 0; blockY < info.Vertical; blockY++)
                        {
                            int componentY = firstY + blockY;
                            int rowBlockBase = componentBlockOffset + (blockY * chunkColumns) + chunkColumnBase;
                            for (int blockX = 0; blockX < info.Horizontal; blockX++)
                            {
                                int componentX = firstX + blockX;
                                int blockIndex = rowBlockBase + blockX;
                                short dcValue;
                                ulong mask;
                                if (componentX >= info.BlocksWide || componentY >= info.BlocksHigh)
                                {
                                    dcValue = checked((short)previousDc[component]);
                                    mask = 0;
                                }
                                else
                                {
                                    mask = chunkMasks[blockIndex];
                                    dcValue = chunkCoefficients[blockIndex * 64];
                                }
                                if (writer is null)
                                {
                                    if (mask == 0)
                                    {
                                        EntropyEncoder.AccumulateDcOnlyBlock(dcValue, ref previousDc[component],
                                            dcFrequencies![component], acFrequencies![component],
                                            tokens: tokens, component: component);
                                    }
                                    else
                                    {
                                        EntropyEncoder.AccumulateOrderedBlockWithMask(
                                            chunkCoefficients.Slice(blockIndex * 64, 64), mask,
                                            ref previousDc[component], dcFrequencies![component], acFrequencies![component],
                                            tokens: tokens, component: component);
                                    }
                                }
                                else
                                {
                                    if (mask == 0)
                                    {
                                        EntropyEncoder.WriteDcOnlyBlock(writer, dcValue, ref previousDc[component],
                                            coding!.DcTables[component], coding.AcTables[component]);
                                    }
                                    else
                                    {
                                        EntropyEncoder.WriteOrderedBlockWithMask(writer,
                                            chunkCoefficients.Slice(blockIndex * 64, 64), mask,
                                            ref previousDc[component], coding!.DcTables[component], coding.AcTables[component]);
                                    }
                                }
                            }
                        }
                    }
                    if (restartInterval != 0)
                    {
                        mcusRemaining--;
                    }
                }
            }
        }
    }

    private static void ProcessScan(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables, float distance, int smoothingFactor,
        int restartInterval, AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers,
        HuffmanTable[] dcTables, HuffmanTable[] acTables, BitWriter? writer,
        long[][]? dcFrequencies, long[][]? acFrequencies,
        JpegScan scan, bool progressive, bool aggregateEobRuns, CancellationToken cancellationToken,
        bool contextByComponent = false,
        QuantizedImage? quantizedImage = null, EntropyTokens? entropyTokens = null,
        Quantization.PreparedTable[]? preparedTables = null)
    {
        bool dcOnly = progressive && scan.SpectralStart == 0;
        bool refine = scan.SuccessiveHigh != 0;
        if (scan.Components.Count == 1)
        {
            ProcessComponentBlocks(pixels, width, height, stride, sampling, tables, distance,
                smoothingFactor, restartInterval, adaptive, adaptiveMultipliers,
                scan.Components[0], acTables, writer, acFrequencies,
                aggregateEobRuns, cancellationToken, scan.SuccessiveLow, refine, scan.SpectralStart, scan.SpectralEnd,
                dcOnly, dcTables, dcFrequencies, sequential: !progressive, contextByComponent: contextByComponent,
                quantizedImage: quantizedImage, entropyTokens: entropyTokens, preparedTables: preparedTables);
        }
        else
        {
            ProcessBlocks(pixels, width, height, stride, sampling, tables, distance,
                smoothingFactor, restartInterval, adaptive, adaptiveMultipliers,
                dcTables, acTables, writer, dcFrequencies, acFrequencies, cancellationToken,
                dcOnly, scan.SuccessiveLow, refine, scanComponents: (int[])scan.Components,
                contextByComponent: contextByComponent, quantizedImage: quantizedImage, entropyTokens: entropyTokens,
                preparedTables: preparedTables);
        }
    }

    private static void ProcessBlocks(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables,
        float distance, int smoothingFactor, int restartInterval, AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers,
        HuffmanTable[] dcTables, HuffmanTable[] acTables, BitWriter? writer,
        long[][]? dcFrequencies, long[][]? acFrequencies, CancellationToken cancellationToken,
        bool dcOnly = false, int successiveLow = 0, bool refine = false, bool requireEightBitCoefficients = false,
        ReadOnlySpan<int> scanComponents = default, bool contextByComponent = false,
        QuantizedImage? quantizedImage = null, EntropyTokens? entropyTokens = null,
        Quantization.PreparedTable[]? preparedTables = null)
    {
        Span<float> samples = stackalloc float[64];
        Span<float> coefficients = stackalloc float[64];
        Span<short> quantized = stackalloc short[64];
        Span<int> previousDc = stackalloc int[3];
        previousDc.Clear();
        Span<int> previousScanDc = stackalloc int[3];
        previousScanDc.Clear();
        int horizontal = sampling.MaxHorizontal;
        int vertical = sampling.MaxVertical;
        int mcuWidth = horizontal * 8;
        int mcuHeight = vertical * 8;
        int mcusRemaining = restartInterval;
        int restartIndex = 0;
        bool cacheComplete = quantizedImage?.IsComplete == true;
        bool hasBlockMasks = quantizedImage?.HasBlockMasks == true;
        Quantization.PreparedTable[]? quantizers = cacheComplete ? null : preparedTables;
        if (!cacheComplete && quantizers is null)
        {
            quantizers = new Quantization.PreparedTable[tables.Length];
            for (int component = 0; component < tables.Length; component++)
            {
                quantizers[component] = Quantization.PreparedTable.Create(tables[component], component, distance,
                    grayscale: pixels.IsGrayscale, adaptiveQuantization: adaptive is not null,
                    adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                    rgb: pixels.IsRgb, tailZeroRunBias: pixels.TailZeroRunBias);
            }
        }
        Span<short> ordered = quantizedImage?.IsZigZagOrdered == true && !cacheComplete ? stackalloc short[64] : default;
        Span<float> strengths = adaptive is null || cacheComplete
            ? default : adaptive.Strengths;
        int scanComponentCount = scanComponents.IsEmpty ? tables.Length : scanComponents.Length;
        Span<(int Component, int Width, int Height, int RealWidth, int RealHeight, int Table)> scanGeometry
            = stackalloc (int, int, int, int, int, int)[3];
        for (int scanIndex = 0; scanIndex < scanComponentCount; scanIndex++)
        {
            int component = scanComponents.IsEmpty ? scanIndex : scanComponents[scanIndex];
            JpegComponentSampling factor = sampling[component];
            scanGeometry[scanIndex] = (component, factor.Horizontal, factor.Vertical,
                ((width * factor.Horizontal) + mcuWidth - 1) / mcuWidth,
                ((height * factor.Vertical) + mcuHeight - 1) / mcuHeight,
                contextByComponent ? component : pixels.IsRgb || component == 0 ? 0 : 1);
        }
        for (int top = 0, mcuY = 0; top < height; top += mcuHeight, mcuY++)
        {
            bool cachedRow = cacheComplete || quantizedImage?.ContainsMcuRow(top) == true;
            if (!cachedRow)
            {
                pixels.ParallelRows?.PrepareRow(top, cancellationToken);
            }

            if (adaptive is not null && !cachedRow)
            {
                adaptive.ComputeRow(pixels, stride, top, tables[pixels.AnalysisComponent][1], strengths!, cancellationToken);
            }

            for (int left = 0, mcuX = 0; left < width; left += mcuWidth, mcuX++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (restartInterval != 0 && mcusRemaining == 0)
                {
                    // Reset in both passes so the optimized tables include the
                    // same DC categories used after each emitted restart marker.
                    writer?.WriteRestartMarker(restartIndex);
                    if (writer is null)
                    {
                        entropyTokens?.AddRestart(restartIndex);
                    }

                    previousDc.Clear();
                    previousScanDc.Clear();
                    restartIndex = (restartIndex + 1) & 7;
                    mcusRemaining = restartInterval;
                }
                for (int scanIndex = 0; scanIndex < scanComponentCount; scanIndex++)
                {
                    (int component, int blocksWide, int blocksHigh,
                        int realBlocksWide, int realBlocksHigh, int tableIndex) = scanGeometry[scanIndex];
                    for (int blockY = 0; blockY < blocksHigh; blockY++)
                    {
                        for (int blockX = 0; blockX < blocksWide; blockX++)
                        {
                            int componentX = (mcuX * blocksWide) + blockX;
                            int componentY = (mcuY * blocksHigh) + blockY;
                            bool outOfBounds = componentX >= realBlocksWide || componentY >= realBlocksHigh;
                            if (!dcOnly && outOfBounds)
                            {
                                // Upstream's baseline streaming path emits a zero DC difference
                                // and AC EOB for a wholly padded MCU block. Keep the predictor,
                                // and bypass sample conversion, the DCT and adaptive quantization.
                                short paddedDc = checked((short)previousDc[component]);
                                if (writer is null)
                                {
                                    EntropyEncoder.AccumulateDcOnlyBlock(paddedDc, ref previousDc[component],
                                        dcFrequencies![tableIndex], acFrequencies![tableIndex],
                                        tokens: entropyTokens, component: component);
                                }
                                else
                                {
                                    EntropyEncoder.WriteDcOnlyBlock(writer, paddedDc, ref previousDc[component],
                                        dcTables[tableIndex], acTables[tableIndex]);
                                }
                                continue;
                            }
                            bool blockCached = !outOfBounds
                                && (cacheComplete || quantizedImage?.ContainsRow(component, componentY) == true);
                            if (!dcOnly && blockCached && hasBlockMasks)
                            {
                                ReadOnlySpan<short> cachedBlock = quantizedImage!.GetBlock(component, componentX, componentY);
                                ulong mask = quantizedImage.GetBlockMask(component, componentX, componentY);
                                if (writer is null)
                                {
                                    if (mask == 0)
                                    {
                                        EntropyEncoder.AccumulateDcOnlyBlock(cachedBlock[0], ref previousDc[component],
                                            dcFrequencies![tableIndex], acFrequencies![tableIndex],
                                            tokens: entropyTokens, component: component);
                                    }
                                    else
                                    {
                                        EntropyEncoder.AccumulateOrderedBlockWithMask(cachedBlock, mask, ref previousDc[component],
                                            dcFrequencies![tableIndex], acFrequencies![tableIndex],
                                            tokens: entropyTokens, component: component);
                                    }
                                }
                                else if (mask == 0)
                                {
                                    EntropyEncoder.WriteDcOnlyBlock(writer, cachedBlock[0], ref previousDc[component],
                                        dcTables[tableIndex], acTables[tableIndex]);
                                }
                                else
                                {
                                    EntropyEncoder.WriteOrderedBlockWithMask(writer, cachedBlock, mask, ref previousDc[component],
                                        dcTables[tableIndex], acTables[tableIndex]);
                                }
                                continue;
                            }
                            scoped ReadOnlySpan<short> encoded;
                            if (outOfBounds)
                            {
                                quantized.Clear();
                                quantized[0] = checked((short)previousDc[component]);
                                encoded = quantized;
                            }
                            else if (blockCached)
                            {
                                encoded = quantizedImage!.GetBlock(component, componentX, componentY);
                            }
                            else
                            {
                                if (pixels.ParallelRows is { } parallelRows)
                                {
                                    parallelRows.CopyBlock(component, componentX, componentY, coefficients);
                                }
                                else
                                {
                                    Downsampling.FillBlock(pixels, width, height, stride, component,
                                        componentX, componentY, horizontal, vertical, blocksWide, blocksHigh,
                                        samples, smoothingFactor);
                                    ForwardDct.Transform(samples, coefficients);
                                }
                                // The pinned streaming path indexes the adaptive field by
                                // component-local block row and full-resolution column.
                                float strength = adaptive is null ? 0f : strengths![
                                    (blockY * adaptive.BlocksWide) + (componentX * horizontal / blocksWide)];
                                Quantization.QuantizePrepared(coefficients, quantizers![component], quantized,
                                    adaptiveStrength: strength, previousDc: previousDc[component],
                                    requireEightBitCoefficients: requireEightBitCoefficients);
                                if (quantizedImage is not null)
                                {
                                    EntropyEncoder.ValidateAc(quantized);
                                }
                                if (!ordered.IsEmpty)
                                {
                                    ZigZagOrdering.Reorder(quantized, ordered);
                                    encoded = ordered;
                                }
                                else
                                {
                                    encoded = quantized;
                                }
                            }
                            if (dcOnly)
                            {
                                if (refine)
                                {
                                    if (writer is null)
                                    {
                                        entropyTokens?.AddBits((uint)((encoded[0] >> successiveLow) & 1), 1);
                                    }
                                    else
                                    {
                                        ProgressiveEntropyEncoder.WriteDcRefinement(writer, encoded, successiveLow);
                                    }
                                }
                                else if (writer is null)
                                {
                                    ProgressiveEntropyEncoder.AccumulateDcFrequencies(encoded,
                                        ref previousScanDc[component], dcFrequencies![tableIndex], successiveLow,
                                        tokens: entropyTokens, component: component);
                                }
                                else
                                {
                                    ProgressiveEntropyEncoder.WriteDc(writer, encoded,
                                        ref previousScanDc[component], dcTables[tableIndex], successiveLow);
                                }
                                // Dummy blocks and quantization use the full coefficient,
                                // independently of the initial scan's shifted predictor.
                                previousDc[component] = encoded[0];
                            }
                            else if (writer is null)
                            {
                                EntropyEncoder.AccumulateFrequencies(encoded, ref previousDc[component],
                                    dcFrequencies![tableIndex], acFrequencies![tableIndex],
                                    coefficientsValidated: quantizedImage is not null,
                                    tokens: entropyTokens, component: component,
                                    zigZagOrdered: quantizedImage?.IsZigZagOrdered == true);
                            }
                            else
                            {
                                EntropyEncoder.WriteBlock(writer, encoded, ref previousDc[component],
                                    dcTables[tableIndex], acTables[tableIndex],
                                    coefficientsValidated: quantizedImage is not null,
                                    zigZagOrdered: quantizedImage?.IsZigZagOrdered == true);
                            }
                        }
                    }
                }
                if (restartInterval != 0)
                {
                    mcusRemaining--;
                }
            }
        }
    }

    private static void ProcessComponentBlocks(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables,
        float distance, int smoothingFactor, int restartInterval, AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers,
        int component, HuffmanTable[] acTables, BitWriter? writer, long[][]? acFrequencies, bool aggregateEobRuns,
        CancellationToken cancellationToken, int successiveLow = 0, bool refine = false,
        int spectralStart = 1, int spectralEnd = 63, bool dcOnly = false,
        HuffmanTable[]? dcTables = null, long[][]? dcFrequencies = null, bool sequential = false,
        bool contextByComponent = false, QuantizedImage? quantizedImage = null, EntropyTokens? entropyTokens = null,
        Quantization.PreparedTable[]? preparedTables = null)
    {
        // A noninterleaved scan visits only real component blocks in raster order.
        // In particular, 4:2:0 luma must finish one block row before the next,
        // rather than following the two-row ordering of interleaved MCUs.
        Span<float> samples = stackalloc float[64];
        Span<float> coefficients = stackalloc float[64];
        Span<short> quantized = stackalloc short[64];
        int horizontal = sampling.MaxHorizontal;
        int vertical = sampling.MaxVertical;
        JpegComponentSampling factor = sampling[component];
        int blocksWide = ((width * factor.Horizontal) + (8 * horizontal) - 1) / (8 * horizontal);
        int blocksHigh = ((height * factor.Vertical) + (8 * vertical) - 1) / (8 * vertical);
        int tableIndex = contextByComponent ? component : pixels.IsRgb || component == 0 ? 0 : 1;
        bool cacheComplete = quantizedImage?.IsComplete == true;
        Quantization.PreparedTable? quantizer = cacheComplete ? null
            : preparedTables is not null ? preparedTables[component]
            : Quantization.PreparedTable.Create(tables[component], component, distance,
                grayscale: pixels.IsGrayscale, adaptiveQuantization: adaptive is not null,
                adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                rgb: pixels.IsRgb, tailZeroRunBias: pixels.TailZeroRunBias);
        Span<short> ordered = quantizedImage?.IsZigZagOrdered == true && !cacheComplete ? stackalloc short[64] : default;
        Span<float> strengths = adaptive is null || cacheComplete
            ? default : adaptive.Strengths;
        int lastAnalysisTop = -1;
        int mcusRemaining = restartInterval;
        int restartIndex = 0;
        int pendingEobRun = 0;
        int previousScanDc = 0;
        using ProgressiveAcRefinementState? refinementState = refine && !dcOnly ? new ProgressiveAcRefinementState(pooled: true) : null;

        void FlushEobRun()
        {
            if (dcOnly || sequential)
            {
                return;
            }

            if (refine)
            {
                if (writer is null)
                {
                    ProgressiveEntropyEncoder.FlushAcRefinementFrequencies(acFrequencies![tableIndex], refinementState!,
                        tokens: entropyTokens, component: component);
                }
                else
                {
                    ProgressiveEntropyEncoder.FlushAcRefinement(writer, acTables[tableIndex], refinementState!);
                }
            }
            else if (writer is null)
            {
                ProgressiveEntropyEncoder.FlushAcFrequencies(acFrequencies![tableIndex], ref pendingEobRun,
                    tokens: entropyTokens, component: component);
            }
            else
            {
                ProgressiveEntropyEncoder.FlushAc(writer, acTables[tableIndex], ref pendingEobRun);
            }
        }

        bool hasBlockMasks = quantizedImage?.HasBlockMasks == true;
        ulong bandMask = !dcOnly && !sequential
            ? (ulong.MaxValue << spectralStart) & (ulong.MaxValue >> (63 - spectralEnd)) : 0;
        for (int blockY = 0; blockY < blocksHigh; blockY++)
        {
            int analysisTop = blockY / factor.Vertical * 8 * vertical;
            bool cachedRow = cacheComplete || quantizedImage?.ContainsRow(component, blockY) == true;
            if (!cachedRow)
            {
                pixels.ParallelRows?.PrepareRow(analysisTop, cancellationToken, component);
            }

            if (adaptive is not null && !cachedRow && analysisTop != lastAnalysisTop)
            {
                adaptive.ComputeRow(pixels, stride, analysisTop,
                    tables[pixels.AnalysisComponent][1], strengths!, cancellationToken);
                lastAnalysisTop = analysisTop;
            }
            for (int blockX = 0; blockX < blocksWide; blockX++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (restartInterval != 0 && mcusRemaining == 0)
                {
                    // EOB runs cannot cross restart intervals. The frequency
                    // pass must terminate runs at the same boundaries as output.
                    FlushEobRun();
                    writer?.WriteRestartMarker(restartIndex);
                    if (writer is null)
                    {
                        entropyTokens?.AddRestart(restartIndex);
                    }

                    previousScanDc = 0;
                    restartIndex = (restartIndex + 1) & 7;
                    mcusRemaining = restartInterval;
                }
                if (cachedRow && hasBlockMasks && !dcOnly && !sequential && aggregateEobRuns)
                {
                    ulong bandNonzero = quantizedImage!.GetBlockMask(component, blockX, blockY) & bandMask;
                    if (bandNonzero == 0)
                    {
                        if (refine)
                        {
                            ProgressiveEntropyEncoder.AccumulateEmptyAcRefinementBlock(refinementState!, writer,
                                writer is null ? null : acTables[tableIndex],
                                writer is null ? acFrequencies![tableIndex] : null, entropyTokens, component);
                        }
                        else
                        {
                            ProgressiveEntropyEncoder.AccumulateEmptyAcBlock(ref pendingEobRun, writer,
                                writer is null ? null : acTables[tableIndex],
                                writer is null ? acFrequencies![tableIndex] : null, entropyTokens, component);
                        }
                        if (restartInterval != 0)
                        {
                            mcusRemaining--;
                        }
                        continue;
                    }
                    ReadOnlySpan<short> maskEncoded = quantizedImage.GetBlock(component, blockX, blockY);
                    if (refine)
                    {
                        ProgressiveEntropyEncoder.ProcessOrderedAcRefinementWithMask(maskEncoded, bandNonzero,
                            refinementState!, successiveLow, spectralStart, spectralEnd, writer,
                            writer is null ? null : acTables[tableIndex],
                            writer is null ? acFrequencies![tableIndex] : null, entropyTokens, component);
                    }
                    else if (writer is null)
                    {
                        ProgressiveEntropyEncoder.AccumulateOrderedAcFrequenciesWithMask(maskEncoded, bandNonzero,
                            acFrequencies![tableIndex], ref pendingEobRun, successiveLow, spectralStart, spectralEnd,
                            entropyTokens, component);
                    }
                    else
                    {
                        ProgressiveEntropyEncoder.WriteOrderedAcWithMask(writer, maskEncoded, bandNonzero,
                            acTables[tableIndex], ref pendingEobRun, successiveLow, spectralStart, spectralEnd);
                    }
                    if (restartInterval != 0)
                    {
                        mcusRemaining--;
                    }
                    continue;
                }
                scoped ReadOnlySpan<short> encoded;
                if (cachedRow)
                {
                    encoded = quantizedImage!.GetBlock(component, blockX, blockY);
                }
                else
                {
                    if (pixels.ParallelRows is { } parallelRows)
                    {
                        parallelRows.CopyBlock(component, blockX, blockY, coefficients);
                    }
                    else
                    {
                        Downsampling.FillBlock(pixels, width, height, stride, component,
                            blockX, blockY, horizontal, vertical, factor.Horizontal, factor.Vertical,
                            samples, smoothingFactor);
                        ForwardDct.Transform(samples, coefficients);
                    }
                    // Reproduce the interleaved pass's component-local row index,
                    // including chroma components with multiple blocks per MCU.
                    int strengthRow = blockY % factor.Vertical;
                    float strength = adaptive is null ? 0f : strengths![(strengthRow * adaptive.BlocksWide)
                        + (blockX * horizontal / factor.Horizontal)];
                    // AC thresholds and the zero DC threshold are independent of
                    // scan ordering. Each scan uses the same full coefficients.
                    Quantization.QuantizePrepared(coefficients, quantizer!, quantized, adaptiveStrength: strength);
                    if (quantizedImage is not null)
                    {
                        EntropyEncoder.ValidateAc(quantized);
                    }
                    if (!ordered.IsEmpty)
                    {
                        ZigZagOrdering.Reorder(quantized, ordered);
                        encoded = ordered;
                    }
                    else
                    {
                        encoded = quantized;
                    }
                }
                if (sequential)
                {
                    if (cachedRow && hasBlockMasks)
                    {
                        ulong mask = quantizedImage!.GetBlockMask(component, blockX, blockY);
                        if (writer is null)
                        {
                            if (mask == 0)
                            {
                                EntropyEncoder.AccumulateDcOnlyBlock(encoded[0], ref previousScanDc,
                                    dcFrequencies![tableIndex], acFrequencies![tableIndex],
                                    tokens: entropyTokens, component: component);
                            }
                            else
                            {
                                EntropyEncoder.AccumulateOrderedBlockWithMask(encoded, mask, ref previousScanDc,
                                    dcFrequencies![tableIndex], acFrequencies![tableIndex],
                                    tokens: entropyTokens, component: component);
                            }
                        }
                        else if (mask == 0)
                        {
                            EntropyEncoder.WriteDcOnlyBlock(writer, encoded[0], ref previousScanDc,
                                dcTables![tableIndex], acTables[tableIndex]);
                        }
                        else
                        {
                            EntropyEncoder.WriteOrderedBlockWithMask(writer, encoded, mask, ref previousScanDc,
                                dcTables![tableIndex], acTables[tableIndex]);
                        }
                    }
                    else if (writer is null)
                    {
                        EntropyEncoder.AccumulateFrequencies(encoded, ref previousScanDc,
                            dcFrequencies![tableIndex], acFrequencies![tableIndex],
                            coefficientsValidated: quantizedImage is not null,
                            tokens: entropyTokens, component: component,
                            zigZagOrdered: quantizedImage?.IsZigZagOrdered == true);
                    }
                    else
                    {
                        EntropyEncoder.WriteBlock(writer, encoded, ref previousScanDc,
                            dcTables![tableIndex], acTables[tableIndex],
                            coefficientsValidated: quantizedImage is not null,
                            zigZagOrdered: quantizedImage?.IsZigZagOrdered == true);
                    }
                }
                else if (dcOnly)
                {
                    if (refine)
                    {
                        if (writer is null)
                        {
                            entropyTokens?.AddBits((uint)((encoded[0] >> successiveLow) & 1), 1);
                        }
                        else
                        {
                            ProgressiveEntropyEncoder.WriteDcRefinement(writer, encoded, successiveLow);
                        }
                    }
                    else if (writer is null)
                    {
                        ProgressiveEntropyEncoder.AccumulateDcFrequencies(encoded,
                            ref previousScanDc, dcFrequencies![tableIndex], successiveLow,
                            tokens: entropyTokens, component: component);
                    }
                    else
                    {
                        ProgressiveEntropyEncoder.WriteDc(writer, encoded,
                            ref previousScanDc, dcTables![tableIndex], successiveLow);
                    }
                }
                else if (refine)
                {
                    if (writer is null)
                    {
                        ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(encoded,
                            acFrequencies![tableIndex], refinementState!, successiveLow, spectralStart, spectralEnd,
                            coefficientsValidated: quantizedImage is not null, tokens: entropyTokens, component: component,
                            zigZagOrdered: quantizedImage?.IsZigZagOrdered == true);
                    }
                    else
                    {
                        ProgressiveEntropyEncoder.WriteAcRefinement(writer, encoded, acTables[tableIndex],
                            refinementState!, successiveLow, spectralStart, spectralEnd,
                            coefficientsValidated: quantizedImage is not null,
                            zigZagOrdered: quantizedImage?.IsZigZagOrdered == true);
                    }
                }
                else if (writer is null)
                {
                    ProgressiveEntropyEncoder.AccumulateAcFrequencies(encoded, acFrequencies![tableIndex],
                        ref pendingEobRun, successiveLow, spectralStart, spectralEnd,
                        coefficientsValidated: quantizedImage is not null, tokens: entropyTokens, component: component,
                        zigZagOrdered: quantizedImage?.IsZigZagOrdered == true);
                }
                else
                {
                    ProgressiveEntropyEncoder.WriteAc(writer, encoded, acTables[tableIndex], ref pendingEobRun,
                        successiveLow, spectralStart, spectralEnd,
                        coefficientsValidated: quantizedImage is not null,
                        zigZagOrdered: quantizedImage?.IsZigZagOrdered == true);
                }
                // Annex K default AC tables only contain the one-block EOB
                // symbol. Aggregated runs use the collected, optimized tables.
                if (!aggregateEobRuns)
                {
                    FlushEobRun();
                }

                if (restartInterval != 0)
                {
                    mcusRemaining--;
                }
            }
        }
        FlushEobRun();
    }

}
