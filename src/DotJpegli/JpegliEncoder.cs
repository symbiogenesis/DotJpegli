using DotJpegli.Internal;

namespace DotJpegli;

/// <summary>
/// Encodes byte, 16-bit or floating-point RGB, grayscale or YCbCr pixels
/// to eight-bit baseline or progressive grayscale, YCbCr or RGB JPEG.
/// Converts sRGB/grayscale to scaled XYB or accepts already scaled XYB input,
/// with a matching caller-supplied output ICC profile. Includes spatial adaptive quantization.
/// SourceIccProfile optionally converts RGB/grayscale source samples into the output profile.
/// </summary>
public static class JpegliEncoder
{
    /// <summary>
    /// Maps Jpegli quality (finite 0..100) to its perceptual quantization distance.
    /// Supports fractional quality using the pinned upstream polynomial; quality 100
    /// maps to 0.01, which remains lossy. This is not libjpeg's quality scale or a
    /// measured perceptual error. The result can be supplied as JpegliEncoderOptions.Distance.
    /// </summary>
    public static float QualityToDistance(float quality) => Quantization.QualityToDistance(quality);

    /// <summary>Encodes an image and returns its JPEG bytes.</summary>
    /// <param name="stride">Bytes between source rows, or zero for tightly packed rows.</param>
    public static byte[] Encode(ReadOnlySpan<byte> pixels, int width, int height,
        PixelLayout layout = PixelLayout.Rgb24, JpegliEncoderOptions? options = null,
        int stride = 0, CancellationToken cancellationToken = default)
    {
        using var output = new PooledOutputStream();
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
    /// <param name="stride">Source elements between rows, or zero for tightly packed rows.</param>
    public static byte[] Encode(ReadOnlySpan<ushort> pixels, int width, int height,
        PixelComponentLayout layout = PixelComponentLayout.Rgb, JpegliEncoderOptions? options = null,
        int stride = 0, CancellationToken cancellationToken = default)
    {
        using var output = new PooledOutputStream();
        Encode(output, pixels, width, height, layout, options, stride, cancellationToken);
        return output.ToArray();
    }

    /// <summary>
    /// Writes an eight-bit JPEG from RGB/grayscale, full-range JPEG YCbCr or already scaled XYB 16-bit samples
    /// without closing or seeking the stream.
    /// Source pixels must remain unchanged until encoding returns. Cancellation or I/O errors may leave partial output.
    /// </summary>
    /// <param name="stride">Source elements between rows, or zero for tightly packed rows.</param>
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
    /// <param name="stride">Source elements between rows, or zero for tightly packed rows.</param>
    public static byte[] Encode(ReadOnlySpan<float> pixels, int width, int height,
        PixelComponentLayout layout = PixelComponentLayout.Rgb, JpegliEncoderOptions? options = null,
        int stride = 0, CancellationToken cancellationToken = default)
    {
        using var output = new PooledOutputStream();
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
    /// <param name="stride">Source elements between rows, or zero for tightly packed rows.</param>
    public static void Encode(Stream output, ReadOnlySpan<float> pixels, int width, int height,
        PixelComponentLayout layout = PixelComponentLayout.Rgb, JpegliEncoderOptions? options = null,
        int stride = 0, CancellationToken cancellationToken = default)
        => EncodeCore(output, new PixelSource(pixels, layout), width, height, options, stride, cancellationToken);

    private static void EncodeCore(Stream output, PixelSource pixels, int width, int height,
        JpegliEncoderOptions? options, int stride, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(output);
        if (!output.CanWrite)
            throw new ArgumentException("The destination stream must be writable.", nameof(output));
        options ??= new JpegliEncoderOptions();
        options.Validate();
        pixels = pixels.WithColorSpace(options.ColorSpace);
        if (pixels.IsXyb && options.IccProfile.IsEmpty && options.SourceIccProfile.IsEmpty)
            throw new ArgumentException("Scaled XYB encoding requires an ICC profile describing the stored channels.",
                nameof(JpegliEncoderOptions.IccProfile));
        if (width is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(width), "JPEG dimensions must be between 1 and 65535.");
        if (height is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(height), "JPEG dimensions must be between 1 and 65535.");
        if ((long)width * height > options.MaxPixels)
            throw new ArgumentOutOfRangeException(nameof(width), "The image exceeds the configured pixel limit.");
        int rowElements = checked(width * pixels.ComponentsPerPixel);
        if (stride == 0)
            stride = rowElements;
        if (stride < rowElements)
            throw new ArgumentOutOfRangeException(nameof(stride), "Stride must hold a complete row.");
        if ((long)(height - 1) * stride + rowElements > pixels.Length)
            throw new ArgumentException("The source buffer does not contain all image rows.", nameof(pixels));
        cancellationToken.ThrowIfCancellationRequested();
        if (options.MetadataMarkers is not null)
            options = options with { MetadataMarkers = options.PrepareMetadataMarkers(cancellationToken) };
        bool hasOutOfRangeFloatSamples = pixels.ValidateSamples(width, height, stride, cancellationToken,
            options.AllowOutOfRangeFloatSamples && options.SourceIccProfile.IsEmpty);
        if (!options.SourceIccProfile.IsEmpty)
        {
            if (!pixels.CanManageSourceColor)
                throw new ArgumentException("Source ICC conversion requires RGB/BGR or grayscale input, not direct YCbCr or scaled XYB.",
                    nameof(JpegliEncoderOptions.SourceIccProfile));
            byte[] destination = options.IccProfile.IsEmpty
                ? pixels.ColorSpace switch
                {
                    JpegColorSpace.Xyb => JpegliIccProfiles.CreateXybProfile(),
                    JpegColorSpace.Grayscale => JpegliIccProfiles.CreateSrgbGrayscaleProfile(),
                    _ => JpegliIccProfiles.CreateSrgbProfile()
                }
                : options.IccProfile.ToArray();
            if (pixels.IsXyb && !destination.AsSpan().SequenceEqual(JpegliIccProfiles.CreateXybProfile()))
                throw new ArgumentException("Source ICC conversion to XYB requires the built-in scaled XYB output profile.",
                    nameof(JpegliEncoderOptions.IccProfile));
            byte[] transformDestination = pixels.IsXyb ? JpegliIccProfiles.CreateSrgbProfile() : destination;
            var transform = IccColorTransform.Create(options.SourceIccProfile.Span, transformDestination,
                pixels.IsGrayscaleInput, pixels.IsGrayscale, options.ColorRenderingIntent, cancellationToken);
            pixels = pixels.WithColorTransform(transform);
            options = options with { IccProfile = destination };
            // Evaluate source-dependent nonlinear arithmetic before any output. The
            // conversion is preflighted before preparing reusable sample planes.
            for (int row = 0; row < height; row++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for (int column = 0; column < width; column++)
                    pixels.ReadSample(row * stride + column * pixels.ComponentsPerPixel, 0);
            }
        }
        ExtendedXmpMetadata? extendedXmp = ExtendedXmpMetadata.Prepare(options, cancellationToken);

        int transferFunction = options.CicpTransferFunction;
        if (options.InferCicpTransferFunctionFromIccProfile && transferFunction == 2)
            transferFunction = IccProfile.LookupTransferFunction(options.IccProfile.Span, cancellationToken);

        bool grayscale = pixels.IsGrayscale;
        bool rgb = pixels.IsRgb;
        bool xyb = pixels.IsXyb;
        int componentCount = grayscale ? 1 : 3;
        JpegScan[]? scanScript = null;
        if (options.ScanScript is { } requestedScript)
        {
            scanScript = JpegScanScript.Prepare(requestedScript, componentCount, cancellationToken, out bool progressive);
            options = options with { Progressive = progressive };
        }
        ushort[][]? customTables = options.PrepareQuantizationTables(componentCount);
        JpegSamplingFactors sampling = grayscale
            ? JpegSamplingFactors.FromSubsampling(ChromaSubsampling.None)
            : options.SamplingFactors ?? (xyb ? new JpegSamplingFactors(new(2, 2), new(2, 2), new(1, 1))
                : JpegSamplingFactors.FromSubsampling(rgb ? ChromaSubsampling.None : options.Subsampling));
        options = options.ResolveProgressiveLevel(sampling);
        bool interleaveInitialScan = grayscale || (options.Progressive
            ? options.ProgressiveInterleaveDc : options.SequentialInterleaveComponents);
        int mcuWidth = 8 * sampling.MaxHorizontal;
        int interleavedRestartInterval = options.ResolveRestartInterval((width + mcuWidth - 1) / mcuWidth);
        var componentRestartIntervals = new int[componentCount];
        for (int component = 0; component < componentCount; component++)
        {
            int blocksWide = (width * sampling[component].Horizontal + mcuWidth - 1) / mcuWidth;
            componentRestartIntervals[component] = options.ResolveRestartInterval(blocksWide);
        }
        int[] acScanEnds = options.PrepareProgressiveAcScanEnds();
        int[] acRefinementBits = options.PrepareProgressiveAcRefinementBits(acScanEnds.Length);
        int maxRefinementBits = Math.Max(options.ProgressiveRefinementBits, acRefinementBits.Max());
        // Quantization's sampling arguments select the upstream 4:2:0 table
        // compensation. Asymmetric schemes use uncompensated tables even when
        // the luma factors are 2x2.
        int quantizationSampling = !rgb && sampling.Is420 ? 2 : 1;
        float distance = options.Distance ?? QualityToDistance(options.Quality);
        var tables = new ushort[componentCount][];
        var quantizationSelectors = new int[componentCount];
        for (int component = 0; component < componentCount; component++)
        {
            int selector = customTables is not null ? Math.Min(component, customTables.Length - 1)
                : xyb ? component : rgb ? 0
                : options.UseStandardQuantizationTables && component > 0 ? 1 : component;
            quantizationSelectors[component] = selector;
            if (customTables is not null)
                tables[component] = customTables[selector];
            else if (xyb)
                tables[component] = XybQuantization.CreateTableForDistance(distance, component, options.ForceBaseline);
            else if (rgb)
                tables[component] = component == 0
                    ? Quantization.CreateStandardTableForDistance(distance, 0, options.ForceBaseline) : tables[0];
            else if (options.UseStandardQuantizationTables)
                tables[component] = component == 2 ? tables[1]
                    : Quantization.CreateStandardTableForDistance(distance, component, options.ForceBaseline);
            else
                tables[component] = grayscale
                    ? Quantization.CreateGrayscaleTableForDistance(distance, transferFunction, options.ForceBaseline)
                    : Quantization.CreateTableForDistance(distance, component, quantizationSampling, quantizationSampling,
                        transferFunction, options.ForceBaseline);
        }

        // ProcessCompressionParams uses Y for grayscale/YCbCr and green for RGB.
        // Disable analysis when that component is subsampled in either direction.
        JpegComponentSampling analysisFactor = sampling[pixels.AnalysisComponent];
        bool useAdaptiveQuantization = options.AdaptiveQuantization
            && analysisFactor.Horizontal == sampling.MaxHorizontal && analysisFactor.Vertical == sampling.MaxVertical;
        var adaptive = useAdaptiveQuantization ? new AdaptiveQuantization(width, height, sampling.MaxVertical) : null;
        float[][]? adaptiveMultipliers = useAdaptiveQuantization
            ? Quantization.CreateAdaptiveZeroBiasMultipliers(tables, quantizationSampling, quantizationSampling, grayscale,
                cicpTransferFunction: transferFunction, rgb: rgb, quantizationSelectors: quantizationSelectors,
                forceBaseline: options.ForceBaseline) : null;

        var dcTables = new HuffmanTable[grayscale || rgb ? 1 : 2];
        var acTables = new HuffmanTable[dcTables.Length];
        if (hasOutOfRangeFloatSamples)
        {
            // Validate full coefficients with baseline MCU ordering and restart
            // resets, even when the selected progressive preview shifts DC bits.
            // Recompute blocks instead of storing a full-image coefficient buffer.
            var dcValidation = new long[dcTables.Length][];
            var acValidation = new long[acTables.Length][];
            for (int index = 0; index < dcTables.Length; index++)
            {
                dcValidation[index] = new long[256];
                acValidation[index] = new long[256];
            }
            try
            {
                ProcessBlocks(pixels, width, height, stride, sampling,
                    tables, distance, options.SmoothingFactor, interleavedRestartInterval, adaptive, adaptiveMultipliers,
                    dcTables, acTables, null, dcValidation, acValidation, cancellationToken,
                    requireEightBitCoefficients: true);
            }
            catch (ArgumentException error) when (error.ParamName is "coefficients" or "adaptiveStrength")
            {
                throw new ArgumentException("The floating-point pixels cannot be encoded with finite computations and eight-bit JPEG coefficient limits using these settings.",
                    nameof(pixels), error);
            }
        }
        int workerLimit = options.MaxDegreeOfParallelism == -1
            ? Environment.ProcessorCount : options.MaxDegreeOfParallelism;
        pixels = workerLimit == 1 && sampling.Is420 && options.SmoothingFactor == 0 && options.OptimizeHuffmanTables
            ? pixels.WithConverted420Planes(width, height, stride, cancellationToken)
            : pixels.WithConvertedPlanes(width, height, stride, cancellationToken);
        using IDisposable? convertedStorage = pixels.ConvertedStorage;
        if (workerLimit > 1)
            pixels = pixels.WithParallelRows(new ParallelDctRows(pixels, width, height, stride,
                sampling, options.SmoothingFactor, workerLimit, cancellationToken));
        if (options.OptimizeHuffmanTables && !rgb)
        {
            JpegScan[] scans = scanScript ?? CreateDefaultScanScript(options, componentCount,
                interleaveInitialScan, acScanEnds, acRefinementBits, maxRefinementBits);
            EncodeOptimizedScans(pixels, width, height, stride, sampling, tables, quantizationSelectors,
                distance, options, extendedXmp, adaptive, adaptiveMultipliers, scans,
                interleavedRestartInterval, componentRestartIntervals, output, cancellationToken);
            return;
        }
        if (options.OptimizeHuffmanTables)
        {
            var dcFrequencies = new long[dcTables.Length][];
            var acFrequencies = new long[dcTables.Length][];
            for (int index = 0; index < dcTables.Length; index++)
            {
                dcFrequencies[index] = new long[256];
                acFrequencies[index] = new long[256];
            }
            if (scanScript is not null)
            {
                foreach (JpegScan scan in scanScript)
                {
                    // DC refinement carries raw bits and contributes no Huffman symbols.
                    if (scan.SpectralEnd == 0 && scan.SuccessiveHigh != 0)
                        continue;
                    int interval = scan.Components.Count == 1
                        ? componentRestartIntervals[scan.Components[0]] : interleavedRestartInterval;
                    ProcessScan(pixels, width, height, stride, sampling, tables, distance,
                        options.SmoothingFactor, interval, adaptive, adaptiveMultipliers,
                        dcTables, acTables, null, dcFrequencies, acFrequencies, cancellationToken,
                        scan, options.Progressive, aggregateEobRuns: true);
                }
            }
            else if (interleaveInitialScan)
                ProcessBlocks(pixels, width, height, stride, sampling,
                    tables, distance, options.SmoothingFactor, interleavedRestartInterval, adaptive, adaptiveMultipliers, dcTables, acTables,
                    null, dcFrequencies, acFrequencies, cancellationToken, dcOnly: options.Progressive,
                    successiveLow: options.ProgressiveRefinementBits);
            else
                for (int component = 0; component < componentCount; component++)
                    ProcessComponentBlocks(pixels, width, height, stride, sampling,
                        tables, distance, options.SmoothingFactor, componentRestartIntervals[component], adaptive, adaptiveMultipliers,
                        component, acTables, null, acFrequencies, cancellationToken, aggregateEobRuns: false,
                        successiveLow: options.ProgressiveRefinementBits, dcOnly: options.Progressive,
                        dcTables: dcTables, dcFrequencies: dcFrequencies, sequential: !options.Progressive);
            if (scanScript is null && options.Progressive)
            {
                for (int band = 0; band < acScanEnds.Length; band++)
                    for (int component = 0; component < componentCount; component++)
                        ProcessComponentBlocks(pixels, width, height, stride, sampling,
                            tables, distance, options.SmoothingFactor, componentRestartIntervals[component], adaptive, adaptiveMultipliers,
                            component, acTables, null, acFrequencies, cancellationToken, aggregateEobRuns: true,
                            successiveLow: acRefinementBits[band],
                            spectralStart: band == 0 ? 1 : acScanEnds[band - 1] + 1, spectralEnd: acScanEnds[band]);
                // DC refinement emits raw bits, so only AC refinement contributes
                // additional symbols to the shared optimized Huffman tables.
                for (int low = maxRefinementBits - 1; low >= 0; low--)
                    for (int band = 0; band < acScanEnds.Length; band++)
                    {
                        if (acRefinementBits[band] <= low)
                            continue;
                        for (int component = 0; component < componentCount; component++)
                            ProcessComponentBlocks(pixels, width, height, stride, sampling,
                                tables, distance, options.SmoothingFactor, componentRestartIntervals[component], adaptive, adaptiveMultipliers,
                                component, acTables, null, acFrequencies, cancellationToken,
                                aggregateEobRuns: true, successiveLow: low, refine: true,
                                spectralStart: band == 0 ? 1 : acScanEnds[band - 1] + 1, spectralEnd: acScanEnds[band]);
                    }
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
        JpegMarkerWriter.WriteHeaders(output, width, height, sampling, tables, quantizationSelectors, dcTables, acTables,
            options, extendedXmp, cancellationToken, rgb: rgb);
        var bits = new BitWriter(output, buffered: true);
        int lastRestartInterval = 0;
        void WriteRestartInterval(int interval)
        {
            if (interval == lastRestartInterval)
                return;
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
                    dcTables, acTables, bits, null, null, cancellationToken,
                    scan, options.Progressive, options.OptimizeHuffmanTables);
                bits.Flush();
            }
        }
        else if (interleaveInitialScan)
        {
            WriteRestartInterval(interleavedRestartInterval);
            JpegMarkerWriter.WriteScanHeader(output, componentCount, options.Progressive,
                successiveLow: options.ProgressiveRefinementBits, rgb: rgb);
            ProcessBlocks(pixels, width, height, stride, sampling,
                tables, distance, options.SmoothingFactor, interleavedRestartInterval, adaptive, adaptiveMultipliers, dcTables, acTables,
                bits, null, null, cancellationToken, dcOnly: options.Progressive,
                successiveLow: options.ProgressiveRefinementBits);
            bits.Flush();
        }
        else
            for (int component = 0; component < componentCount; component++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                WriteRestartInterval(componentRestartIntervals[component]);
                JpegMarkerWriter.WriteScanHeader(output, componentCount, options.Progressive,
                    successiveLow: options.ProgressiveRefinementBits, rgb: rgb, singleComponent: component);
                ProcessComponentBlocks(pixels, width, height, stride, sampling,
                    tables, distance, options.SmoothingFactor, componentRestartIntervals[component], adaptive, adaptiveMultipliers,
                    component, acTables, bits, null, cancellationToken, aggregateEobRuns: false,
                    successiveLow: options.ProgressiveRefinementBits, dcOnly: options.Progressive,
                    dcTables: dcTables, sequential: !options.Progressive);
                bits.Flush();
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
                        component, acTables, bits, null, cancellationToken,
                        aggregateEobRuns: options.OptimizeHuffmanTables,
                        successiveLow: acRefinementBits[band],
                        spectralStart: spectralStart, spectralEnd: acScanEnds[band]);
                    bits.Flush();
                }
            }
            for (int low = maxRefinementBits - 1; low >= 0; low--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (options.ProgressiveRefinementBits > low)
                {
                    if (options.ProgressiveInterleaveDc || grayscale)
                    {
                        WriteRestartInterval(interleavedRestartInterval);
                        JpegMarkerWriter.WriteScanHeader(output, componentCount, true,
                            successiveHigh: low + 1, successiveLow: low, rgb: rgb);
                        ProcessBlocks(pixels, width, height, stride, sampling,
                            tables, distance, options.SmoothingFactor, interleavedRestartInterval, adaptive, adaptiveMultipliers, dcTables, acTables,
                            bits, null, null, cancellationToken, dcOnly: true, successiveLow: low, refine: true);
                        bits.Flush();
                    }
                    else
                        for (int component = 0; component < componentCount; component++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            WriteRestartInterval(componentRestartIntervals[component]);
                            JpegMarkerWriter.WriteScanHeader(output, componentCount, true,
                                successiveHigh: low + 1, successiveLow: low, rgb: rgb, singleComponent: component);
                            ProcessComponentBlocks(pixels, width, height, stride, sampling,
                                tables, distance, options.SmoothingFactor, componentRestartIntervals[component], adaptive, adaptiveMultipliers,
                                component, acTables, bits, null, cancellationToken, aggregateEobRuns: false,
                                successiveLow: low, refine: true, dcOnly: true, dcTables: dcTables);
                            bits.Flush();
                        }
                }
                for (int band = 0; band < acScanEnds.Length; band++)
                {
                    if (acRefinementBits[band] <= low)
                        continue;
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
                            component, acTables, bits, null, cancellationToken,
                            aggregateEobRuns: options.OptimizeHuffmanTables, successiveLow: low, refine: true,
                            spectralStart: spectralStart, spectralEnd: acScanEnds[band]);
                        bits.Flush();
                    }
                }
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        output.WriteByte(0xff);
        output.WriteByte(0xd9);
    }

    private static JpegScan[] CreateDefaultScanScript(JpegliEncoderOptions options, int componentCount,
        bool interleaveInitialScan, int[] acScanEnds, int[] acRefinementBits, int maxRefinementBits)
    {
        var scans = new List<JpegScan>();
        int[] components = Enumerable.Range(0, componentCount).ToArray();
        void AddDc(int high, int low)
        {
            if (interleaveInitialScan)
                scans.Add(new(components, 0, options.Progressive ? 0 : 63, high, low));
            else
                foreach (int component in components)
                    scans.Add(new(new[] { component }, 0, options.Progressive ? 0 : 63, high, low));
        }
        AddDc(0, options.ProgressiveRefinementBits);
        if (!options.Progressive)
            return scans.ToArray();
        for (int band = 0; band < acScanEnds.Length; band++)
            foreach (int component in components)
                scans.Add(new(new[] { component }, band == 0 ? 1 : acScanEnds[band - 1] + 1,
                    acScanEnds[band], 0, acRefinementBits[band]));
        for (int low = maxRefinementBits - 1; low >= 0; low--)
        {
            if (options.ProgressiveRefinementBits > low)
                AddDc(low + 1, low);
            for (int band = 0; band < acScanEnds.Length; band++)
                if (acRefinementBits[band] > low)
                    foreach (int component in components)
                        scans.Add(new(new[] { component }, band == 0 ? 1 : acScanEnds[band - 1] + 1,
                            acScanEnds[band], low + 1, low));
        }
        return scans.ToArray();
    }

    private static void EncodeOptimizedScans(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables, int[] quantizationSelectors, float distance,
        JpegliEncoderOptions options, ExtendedXmpMetadata? extendedXmp,
        AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers, JpegScan[] scans,
        int interleavedRestartInterval, int[] componentRestartIntervals, Stream output,
        CancellationToken cancellationToken)
    {
        int componentCount = tables.Length;
        // One interleaved sequential scan needs only its entropy decisions after
        // collecting histograms. Record them as each block is quantized rather
        // than storing and then rereading the entire coefficient image. Parallel
        // and multi-scan encodes retain the reusable coefficient path.
        bool streamSequential = !options.Progressive && scans.Length == 1 && componentCount == 3
            && scans[0].Components.Count == 3 && pixels.ParallelRows is null;
        using QuantizedImage? quantizedImage = streamSequential ? null : QuantizedImage.Create(pixels, width, height, stride,
            sampling, tables, distance, options.SmoothingFactor, adaptive, adaptiveMultipliers, cancellationToken,
            zigZagOrdered: true);
        using EntropyTokenScans? tokenScans = streamSequential || quantizedImage is not null ? new(scans.Length) : null;
        var dcFrequencies = new long[componentCount][];
        for (int component = 0; component < componentCount; component++)
            dcFrequencies[component] = new long[256];
        var acFrequencies = new long[scans.Length][][];
        // These placeholder arrays are not read during frequency collection.
        var emptyTables = new HuffmanTable[componentCount];
        for (int scanIndex = 0; scanIndex < scans.Length; scanIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            acFrequencies[scanIndex] = new long[componentCount][];
            for (int component = 0; component < componentCount; component++)
                acFrequencies[scanIndex][component] = new long[256];
            JpegScan scan = scans[scanIndex];
            EntropyTokens? scanTokens = tokenScans?[scanIndex];
            // DC refinement has no histogram, but its raw bits can be replayed.
            if (scan.SpectralEnd == 0 && scan.SuccessiveHigh != 0 && scanTokens is not { IsComplete: true })
                continue;
            int interval = scan.Components.Count == 1
                ? componentRestartIntervals[scan.Components[0]] : interleavedRestartInterval;
            ProcessScan(pixels, width, height, stride, sampling, tables, distance,
                options.SmoothingFactor, interval, adaptive, adaptiveMultipliers,
                emptyTables, emptyTables, null, dcFrequencies, acFrequencies[scanIndex], cancellationToken,
                scan, options.Progressive, aggregateEobRuns: true, contextByComponent: true,
                quantizedImage: quantizedImage, entropyTokens: scanTokens, streamSequential: streamSequential);
        }
        HuffmanCodingPlan plan = HuffmanCodingPlan.Create(scans, dcFrequencies, acFrequencies,
            options.Progressive, options.ForceBaseline);
        cancellationToken.ThrowIfCancellationRequested();
        JpegMarkerWriter.WriteHeaders(output, width, height, sampling, tables, quantizationSelectors,
            emptyTables, emptyTables, options, extendedXmp, cancellationToken, rgb: pixels.IsRgb,
            writeHuffmanTables: false, extendedHuffman: plan.RequiresExtendedSequential);
        var bits = new BitWriter(output, buffered: true);
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
                scanTokens.Replay(bits, coding, cancellationToken);
            else
                ProcessScan(pixels, width, height, stride, sampling, tables, distance,
                    options.SmoothingFactor, interval, adaptive, adaptiveMultipliers,
                    coding.DcTables, coding.AcTables, bits, null, null, cancellationToken,
                    scan, options.Progressive, aggregateEobRuns: true, contextByComponent: true,
                    quantizedImage: quantizedImage, streamSequential: streamSequential);
            bits.Flush();
        }
        cancellationToken.ThrowIfCancellationRequested();
        output.WriteByte(0xff);
        output.WriteByte(0xd9);
    }

    private static void ProcessScan(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables, float distance, int smoothingFactor,
        int restartInterval, AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers,
        HuffmanTable[] dcTables, HuffmanTable[] acTables, BitWriter? writer,
        long[][]? dcFrequencies, long[][]? acFrequencies, CancellationToken cancellationToken,
        JpegScan scan, bool progressive, bool aggregateEobRuns, bool contextByComponent = false,
        QuantizedImage? quantizedImage = null, EntropyTokens? entropyTokens = null, bool streamSequential = false)
    {
        bool dcOnly = progressive && scan.SpectralStart == 0;
        bool refine = scan.SuccessiveHigh != 0;
        if (scan.Components.Count == 1)
            ProcessComponentBlocks(pixels, width, height, stride, sampling, tables, distance,
                smoothingFactor, restartInterval, adaptive, adaptiveMultipliers,
                scan.Components[0], acTables, writer, acFrequencies, cancellationToken,
                aggregateEobRuns, scan.SuccessiveLow, refine, scan.SpectralStart, scan.SpectralEnd,
                dcOnly, dcTables, dcFrequencies, sequential: !progressive, contextByComponent: contextByComponent,
                quantizedImage: quantizedImage, entropyTokens: entropyTokens);
        else
            ProcessBlocks(pixels, width, height, stride, sampling, tables, distance,
                smoothingFactor, restartInterval, adaptive, adaptiveMultipliers,
                dcTables, acTables, writer, dcFrequencies, acFrequencies, cancellationToken,
                dcOnly, scan.SuccessiveLow, refine, scanComponents: (int[])scan.Components,
                contextByComponent: contextByComponent, quantizedImage: quantizedImage, entropyTokens: entropyTokens,
                streamSequential: streamSequential);
    }

    private static void ProcessBlocks(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables,
        float distance, int smoothingFactor, int restartInterval, AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers,
        HuffmanTable[] dcTables, HuffmanTable[] acTables, BitWriter? writer,
        long[][]? dcFrequencies, long[][]? acFrequencies, CancellationToken cancellationToken,
        bool dcOnly = false, int successiveLow = 0, bool refine = false, bool requireEightBitCoefficients = false,
        ReadOnlySpan<int> scanComponents = default, bool contextByComponent = false,
        QuantizedImage? quantizedImage = null, EntropyTokens? entropyTokens = null, bool streamSequential = false)
    {
        Span<float> samples = stackalloc float[64];
        Span<float> coefficients = stackalloc float[64];
        Span<short> quantized = stackalloc short[64];
        Span<short> ordered = streamSequential ? stackalloc short[64] : default;
        Quantization.PreparedTable[]? quantizers = null;
        if (streamSequential)
        {
            quantizers = new Quantization.PreparedTable[tables.Length];
            for (int component = 0; component < tables.Length; component++)
                quantizers[component] = Quantization.PreparedTable.Create(tables[component], component, distance,
                    grayscale: pixels.IsGrayscale, adaptiveQuantization: adaptive is not null,
                    adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                    rgb: pixels.IsRgb);
        }
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
        float[]? strengths = adaptive is null ? null : new float[adaptive.BlocksWide * vertical];
        for (int top = 0; top < height; top += mcuHeight)
        {
            if (quantizedImage is null)
                pixels.ParallelRows?.PrepareRow(top, cancellationToken);
            if (adaptive is not null && quantizedImage is null)
                adaptive.ComputeRow(pixels, stride, top, tables[pixels.AnalysisComponent][1], strengths!, cancellationToken);
            for (int left = 0; left < width; left += mcuWidth)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (restartInterval != 0 && mcusRemaining == 0)
                {
                    // Reset in both passes so the optimized tables include the
                    // same DC categories used after each emitted restart marker.
                    writer?.WriteRestartMarker(restartIndex);
                    if (writer is null)
                        entropyTokens?.AddRestart(restartIndex);
                    previousDc.Clear();
                    previousScanDc.Clear();
                    restartIndex = (restartIndex + 1) & 7;
                    mcusRemaining = restartInterval;
                }
                int scanComponentCount = scanComponents.IsEmpty ? tables.Length : scanComponents.Length;
                for (int scanIndex = 0; scanIndex < scanComponentCount; scanIndex++)
                {
                    int component = scanComponents.IsEmpty ? scanIndex : scanComponents[scanIndex];
                    JpegComponentSampling factor = sampling[component];
                    int blocksWide = factor.Horizontal;
                    int blocksHigh = factor.Vertical;
                    int realBlocksWide = (width * blocksWide + mcuWidth - 1) / mcuWidth;
                    int realBlocksHigh = (height * blocksHigh + mcuHeight - 1) / mcuHeight;
                    ReadOnlySpan<float> plane = streamSequential && smoothingFactor == 0
                        && blocksWide == horizontal && blocksHigh == vertical ? pixels.GetConvertedPlane(component) : default;
                    int planeWidth = width, planeHeight = height;
                    if (streamSequential && smoothingFactor == 0 && sampling.Is420 && component != 0)
                        plane = pixels.GetConverted420ChromaPlane(component, out planeWidth, out planeHeight);
                    for (int blockY = 0; blockY < blocksHigh; blockY++)
                    {
                        for (int blockX = 0; blockX < blocksWide; blockX++)
                        {
                            int componentX = left / mcuWidth * blocksWide + blockX;
                            int componentY = top / mcuHeight * blocksHigh + blockY;
                            scoped ReadOnlySpan<short> encoded;
                            if (componentX >= realBlocksWide || componentY >= realBlocksHigh)
                            {
                                // Upstream's baseline streaming path emits a zero DC difference
                                // and AC EOB for a wholly padded MCU block. Keep the predictor,
                                // and bypass sample conversion, the DCT and adaptive quantization.
                                quantized.Clear();
                                quantized[0] = checked((short)previousDc[component]);
                                encoded = quantized;
                            }
                            else if (quantizedImage is not null)
                                encoded = quantizedImage.GetBlock(component, componentX, componentY);
                            else
                            {
                                if (pixels.ParallelRows is { } parallelRows)
                                    parallelRows.CopyBlock(component, componentX, componentY, coefficients);
                                else if (!plane.IsEmpty && componentX * 8 + 8 <= planeWidth && componentY * 8 + 8 <= planeHeight)
                                    ForwardDct.TransformPlane(plane, planeWidth, componentX * 8, componentY * 8, coefficients);
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
                                    blockY * adaptive.BlocksWide + componentX * horizontal / blocksWide];
                                if (streamSequential)
                                {
                                    Quantization.QuantizePrepared(coefficients, quantizers![component], quantized,
                                        adaptiveStrength: strength);
                                    EntropyEncoder.ValidateAc(quantized);
                                    ZigZagOrdering.Reorder(quantized, ordered);
                                    encoded = ordered;
                                }
                                else
                                {
                                    Quantization.Quantize(coefficients, tables[component], quantized, component, distance,
                                        grayscale: pixels.IsGrayscale, adaptiveQuantization: adaptive is not null,
                                        adaptiveStrength: strength,
                                        adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                                        previousDc: previousDc[component], requireEightBitCoefficients: requireEightBitCoefficients,
                                        rgb: pixels.IsRgb);
                                    encoded = quantized;
                                }
                            }
                            int tableIndex = contextByComponent ? component : pixels.IsRgb || component == 0 ? 0 : 1;
                            if (dcOnly)
                            {
                                if (refine)
                                {
                                    if (writer is null)
                                        entropyTokens?.AddBits((uint)((encoded[0] >> successiveLow) & 1), 1);
                                    else
                                        ProgressiveEntropyEncoder.WriteDcRefinement(writer, encoded, successiveLow);
                                }
                                else if (writer is null)
                                    ProgressiveEntropyEncoder.AccumulateDcFrequencies(encoded,
                                        ref previousScanDc[component], dcFrequencies![tableIndex], successiveLow,
                                        tokens: entropyTokens, component: component);
                                else
                                    ProgressiveEntropyEncoder.WriteDc(writer, encoded,
                                        ref previousScanDc[component], dcTables[tableIndex], successiveLow);
                                // Dummy blocks and quantization use the full coefficient,
                                // independently of the initial scan's shifted predictor.
                                previousDc[component] = encoded[0];
                            }
                            else if (writer is null)
                                EntropyEncoder.AccumulateFrequencies(encoded, ref previousDc[component],
                                    dcFrequencies![tableIndex], acFrequencies![tableIndex],
                                    coefficientsValidated: streamSequential || quantizedImage is not null,
                                    tokens: entropyTokens, component: component,
                                    zigZagOrdered: streamSequential || quantizedImage?.IsZigZagOrdered == true);
                            else
                                EntropyEncoder.WriteBlock(writer, encoded, ref previousDc[component],
                                    dcTables[tableIndex], acTables[tableIndex],
                                    coefficientsValidated: streamSequential || quantizedImage is not null,
                                    zigZagOrdered: streamSequential || quantizedImage?.IsZigZagOrdered == true);
                        }
                    }
                }
                if (restartInterval != 0)
                    mcusRemaining--;
            }
        }
    }

    private static void ProcessComponentBlocks(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables,
        float distance, int smoothingFactor, int restartInterval, AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers,
        int component, HuffmanTable[] acTables, BitWriter? writer, long[][]? acFrequencies,
        CancellationToken cancellationToken, bool aggregateEobRuns, int successiveLow = 0, bool refine = false,
        int spectralStart = 1, int spectralEnd = 63, bool dcOnly = false,
        HuffmanTable[]? dcTables = null, long[][]? dcFrequencies = null, bool sequential = false,
        bool contextByComponent = false, QuantizedImage? quantizedImage = null, EntropyTokens? entropyTokens = null)
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
        int blocksWide = (width * factor.Horizontal + 8 * horizontal - 1) / (8 * horizontal);
        int blocksHigh = (height * factor.Vertical + 8 * vertical - 1) / (8 * vertical);
        int tableIndex = contextByComponent ? component : pixels.IsRgb || component == 0 ? 0 : 1;
        float[]? strengths = adaptive is null ? null : new float[adaptive.BlocksWide * vertical];
        int lastAnalysisTop = -1;
        int mcusRemaining = restartInterval;
        int restartIndex = 0;
        int pendingEobRun = 0;
        int previousScanDc = 0;
        var refinementState = refine && !dcOnly ? new ProgressiveAcRefinementState() : null;

        void FlushEobRun()
        {
            if (dcOnly || sequential)
                return;
            if (refine)
            {
                if (writer is null)
                    ProgressiveEntropyEncoder.FlushAcRefinementFrequencies(acFrequencies![tableIndex], refinementState!,
                        tokens: entropyTokens, component: component);
                else
                    ProgressiveEntropyEncoder.FlushAcRefinement(writer, acTables[tableIndex], refinementState!);
            }
            else if (writer is null)
                ProgressiveEntropyEncoder.FlushAcFrequencies(acFrequencies![tableIndex], ref pendingEobRun,
                    tokens: entropyTokens, component: component);
            else
                ProgressiveEntropyEncoder.FlushAc(writer, acTables[tableIndex], ref pendingEobRun);
        }

        for (int blockY = 0; blockY < blocksHigh; blockY++)
        {
            int analysisTop = blockY / factor.Vertical * (8 * vertical);
            if (quantizedImage is null)
                pixels.ParallelRows?.PrepareRow(analysisTop, cancellationToken, component);
            if (adaptive is not null && quantizedImage is null && analysisTop != lastAnalysisTop)
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
                        entropyTokens?.AddRestart(restartIndex);
                    previousScanDc = 0;
                    restartIndex = (restartIndex + 1) & 7;
                    mcusRemaining = restartInterval;
                }
                scoped ReadOnlySpan<short> encoded;
                if (quantizedImage is not null)
                    encoded = quantizedImage.GetBlock(component, blockX, blockY);
                else
                {
                    if (pixels.ParallelRows is { } parallelRows)
                        parallelRows.CopyBlock(component, blockX, blockY, coefficients);
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
                    float strength = adaptive is null ? 0f : strengths![strengthRow * adaptive.BlocksWide
                        + blockX * horizontal / factor.Horizontal];
                    // AC thresholds and the zero DC threshold are independent of
                    // scan ordering. Each scan uses the same full coefficients.
                    Quantization.Quantize(coefficients, tables[component], quantized, component, distance,
                        grayscale: pixels.IsGrayscale, adaptiveQuantization: adaptive is not null,
                        adaptiveStrength: strength,
                        adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                        rgb: pixels.IsRgb);
                    encoded = quantized;
                }
                if (sequential)
                {
                    if (writer is null)
                        EntropyEncoder.AccumulateFrequencies(encoded, ref previousScanDc,
                            dcFrequencies![tableIndex], acFrequencies![tableIndex],
                            coefficientsValidated: quantizedImage is not null,
                            tokens: entropyTokens, component: component,
                            zigZagOrdered: quantizedImage?.IsZigZagOrdered == true);
                    else
                        EntropyEncoder.WriteBlock(writer, encoded, ref previousScanDc,
                            dcTables![tableIndex], acTables[tableIndex],
                            coefficientsValidated: quantizedImage is not null,
                            zigZagOrdered: quantizedImage?.IsZigZagOrdered == true);
                }
                else if (dcOnly)
                {
                    if (refine)
                    {
                        if (writer is null)
                            entropyTokens?.AddBits((uint)((encoded[0] >> successiveLow) & 1), 1);
                        else
                            ProgressiveEntropyEncoder.WriteDcRefinement(writer, encoded, successiveLow);
                    }
                    else if (writer is null)
                        ProgressiveEntropyEncoder.AccumulateDcFrequencies(encoded,
                            ref previousScanDc, dcFrequencies![tableIndex], successiveLow,
                            tokens: entropyTokens, component: component);
                    else
                        ProgressiveEntropyEncoder.WriteDc(writer, encoded,
                            ref previousScanDc, dcTables![tableIndex], successiveLow);
                }
                else if (refine)
                {
                    if (writer is null)
                        ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(encoded,
                            acFrequencies![tableIndex], refinementState!, successiveLow, spectralStart, spectralEnd,
                            coefficientsValidated: quantizedImage is not null, tokens: entropyTokens, component: component,
                            zigZagOrdered: quantizedImage?.IsZigZagOrdered == true);
                    else
                        ProgressiveEntropyEncoder.WriteAcRefinement(writer, encoded, acTables[tableIndex],
                            refinementState!, successiveLow, spectralStart, spectralEnd,
                            coefficientsValidated: quantizedImage is not null,
                            zigZagOrdered: quantizedImage?.IsZigZagOrdered == true);
                }
                else if (writer is null)
                    ProgressiveEntropyEncoder.AccumulateAcFrequencies(encoded, acFrequencies![tableIndex],
                        ref pendingEobRun, successiveLow, spectralStart, spectralEnd,
                        coefficientsValidated: quantizedImage is not null, tokens: entropyTokens, component: component,
                        zigZagOrdered: quantizedImage?.IsZigZagOrdered == true);
                else
                    ProgressiveEntropyEncoder.WriteAc(writer, encoded, acTables[tableIndex], ref pendingEobRun,
                        successiveLow, spectralStart, spectralEnd,
                        coefficientsValidated: quantizedImage is not null,
                        zigZagOrdered: quantizedImage?.IsZigZagOrdered == true);
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
