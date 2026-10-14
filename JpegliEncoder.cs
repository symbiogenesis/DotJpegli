using DotJpegli.Internal;

namespace DotJpegli;

/// <summary>
/// Encodes byte, 16-bit or floating-point RGB, grayscale or YCbCr pixels
/// to eight-bit baseline or progressive grayscale, YCbCr or RGB JPEG.
/// Converts sRGB/grayscale to scaled XYB or accepts already scaled XYB input,
/// with a matching caller-supplied output ICC profile. Includes spatial adaptive quantization.
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
        => EncodeCore(output, new PixelSource(pixels, layout), width, height, options, stride, cancellationToken);

    /// <summary>
    /// Encodes RGB/grayscale, full-range JPEG YCbCr or already scaled XYB 16-bit samples.
    /// Values 0..65535 map to JPEG sample units 0..255; neutral YCbCr chroma is 32896.
    /// Non-XYB output retains the source transfer function; use a matching output ICC profile.
    /// Explicit XYB conversion requires gamma-encoded sRGB input.
    /// Fractional precision is retained until quantization; output JPEG sample precision is eight bits.
    /// </summary>
    /// <param name="stride">Source elements between rows, or zero for tightly packed rows.</param>
    public static byte[] Encode(ReadOnlySpan<ushort> pixels, int width, int height,
        PixelComponentLayout layout = PixelComponentLayout.Rgb, JpegliEncoderOptions? options = null,
        int stride = 0, CancellationToken cancellationToken = default)
    {
        using var output = new MemoryStream();
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
    /// before its opsin transform. No HDR transform or source ICC transform is applied.
    /// Non-XYB output retains the source transfer function; use a matching output ICC profile.
    /// Fractional precision is retained until quantization; output JPEG sample precision is eight bits.
    /// </summary>
    /// <param name="stride">Source elements between rows, or zero for tightly packed rows.</param>
    public static byte[] Encode(ReadOnlySpan<float> pixels, int width, int height,
        PixelComponentLayout layout = PixelComponentLayout.Rgb, JpegliEncoderOptions? options = null,
        int stride = 0, CancellationToken cancellationToken = default)
    {
        using var output = new MemoryStream();
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
        if (pixels.IsXyb && options.IccProfile.IsEmpty)
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
            options.AllowOutOfRangeFloatSamples);
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
            int blockWidth = mcuWidth / sampling[component].Horizontal;
            componentRestartIntervals[component] = options.ResolveRestartInterval((width + blockWidth - 1) / blockWidth);
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
        var bits = new BitWriter(output);
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
        output.WriteByte(0xff);
        output.WriteByte(0xd9);
    }

    private static void ProcessScan(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables, float distance, int smoothingFactor,
        int restartInterval, AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers,
        HuffmanTable[] dcTables, HuffmanTable[] acTables, BitWriter? writer,
        long[][]? dcFrequencies, long[][]? acFrequencies, CancellationToken cancellationToken,
        JpegScan scan, bool progressive, bool aggregateEobRuns)
    {
        bool dcOnly = progressive && scan.SpectralStart == 0;
        bool refine = scan.SuccessiveHigh != 0;
        if (scan.Components.Count == 1)
            ProcessComponentBlocks(pixels, width, height, stride, sampling, tables, distance,
                smoothingFactor, restartInterval, adaptive, adaptiveMultipliers,
                scan.Components[0], acTables, writer, acFrequencies, cancellationToken,
                aggregateEobRuns, scan.SuccessiveLow, refine, scan.SpectralStart, scan.SpectralEnd,
                dcOnly, dcTables, dcFrequencies, sequential: !progressive);
        else
            ProcessBlocks(pixels, width, height, stride, sampling, tables, distance,
                smoothingFactor, restartInterval, adaptive, adaptiveMultipliers,
                dcTables, acTables, writer, dcFrequencies, acFrequencies, cancellationToken,
                dcOnly, scan.SuccessiveLow, refine, scanComponents: (int[])scan.Components);
    }

    private static void ProcessBlocks(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables,
        float distance, int smoothingFactor, int restartInterval, AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers,
        HuffmanTable[] dcTables, HuffmanTable[] acTables, BitWriter? writer,
        long[][]? dcFrequencies, long[][]? acFrequencies, CancellationToken cancellationToken,
        bool dcOnly = false, int successiveLow = 0, bool refine = false, bool requireEightBitCoefficients = false,
        ReadOnlySpan<int> scanComponents = default)
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
        float[]? strengths = adaptive is null ? null : new float[adaptive.BlocksWide * vertical];
        for (int top = 0; top < height; top += mcuHeight)
        {
            if (adaptive is not null)
                adaptive.ComputeRow(pixels, stride, top, tables[pixels.AnalysisComponent][1], strengths!, cancellationToken);
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
                int scanComponentCount = scanComponents.IsEmpty ? tables.Length : scanComponents.Length;
                for (int scanIndex = 0; scanIndex < scanComponentCount; scanIndex++)
                {
                    int component = scanComponents.IsEmpty ? scanIndex : scanComponents[scanIndex];
                    JpegComponentSampling factor = sampling[component];
                    int blocksWide = factor.Horizontal;
                    int blocksHigh = factor.Vertical;
                    int sampleWidth = horizontal / blocksWide;
                    int sampleHeight = vertical / blocksHigh;
                    for (int blockY = 0; blockY < blocksHigh; blockY++)
                    {
                        for (int blockX = 0; blockX < blocksWide; blockX++)
                        {
                            int blockLeft = left + blockX * 8 * sampleWidth;
                            int blockTop = top + blockY * 8 * sampleHeight;
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
                                Downsampling.FillBlock(pixels, width, height, stride, component,
                                    blockLeft, blockTop, sampleWidth, sampleHeight, samples, smoothingFactor);
                                ForwardDct.Transform(samples, coefficients);
                                // The pinned streaming path indexes the adaptive field by
                                // component-local block row and full-resolution column.
                                float strength = adaptive is null ? 0f : strengths![
                                    blockY * adaptive.BlocksWide + blockLeft / 8];
                                Quantization.Quantize(coefficients, tables[component], quantized, component, distance,
                                    grayscale: pixels.IsGrayscale, adaptiveQuantization: adaptive is not null,
                                    adaptiveStrength: strength,
                                    adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                                    previousDc: previousDc[component], requireEightBitCoefficients: requireEightBitCoefficients,
                                    rgb: pixels.IsRgb);
                            }
                            int tableIndex = pixels.IsRgb || component == 0 ? 0 : 1;
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

    private static void ProcessComponentBlocks(PixelSource pixels, int width, int height, int stride,
        JpegSamplingFactors sampling, ushort[][] tables,
        float distance, int smoothingFactor, int restartInterval, AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers,
        int component, HuffmanTable[] acTables, BitWriter? writer, long[][]? acFrequencies,
        CancellationToken cancellationToken, bool aggregateEobRuns, int successiveLow = 0, bool refine = false,
        int spectralStart = 1, int spectralEnd = 63, bool dcOnly = false,
        HuffmanTable[]? dcTables = null, long[][]? dcFrequencies = null, bool sequential = false)
    {
        // A noninterleaved scan visits only real component blocks in raster order.
        // In particular, 4:2:0 luma must finish one block row before the next,
        // rather than following the two-row ordering of interleaved MCUs.
        Span<float> samples = stackalloc float[64];
        Span<float> coefficients = stackalloc float[64];
        Span<short> quantized = stackalloc short[64];
        int vertical = sampling.MaxVertical;
        int sampleWidth = sampling.MaxHorizontal / sampling[component].Horizontal;
        int sampleHeight = vertical / sampling[component].Vertical;
        int tableIndex = pixels.IsRgb || component == 0 ? 0 : 1;
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
                adaptive.ComputeRow(pixels, stride, analysisTop,
                    tables[pixels.AnalysisComponent][1], strengths!, cancellationToken);
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
                    previousScanDc = 0;
                    restartIndex = (restartIndex + 1) & 7;
                    mcusRemaining = restartInterval;
                }
                Downsampling.FillBlock(pixels, width, height, stride, component,
                    left, top, sampleWidth, sampleHeight, samples, smoothingFactor);
                ForwardDct.Transform(samples, coefficients);
                // Reproduce the interleaved pass's component-local row index,
                // including chroma components with multiple blocks per MCU.
                int strengthRow = (top - analysisTop) / (8 * sampleHeight);
                float strength = adaptive is null ? 0f : strengths![strengthRow * adaptive.BlocksWide + left / 8];
                // AC thresholds and the zero DC threshold are independent of
                // scan ordering. Each scan uses the same full coefficients.
                Quantization.Quantize(coefficients, tables[component], quantized, component, distance,
                    grayscale: pixels.IsGrayscale, adaptiveQuantization: adaptive is not null,
                    adaptiveStrength: strength,
                    adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                    rgb: pixels.IsRgb);
                if (sequential)
                {
                    if (writer is null)
                        EntropyEncoder.AccumulateFrequencies(quantized, ref previousScanDc,
                            dcFrequencies![tableIndex], acFrequencies![tableIndex]);
                    else
                        EntropyEncoder.WriteBlock(writer, quantized, ref previousScanDc,
                            dcTables![tableIndex], acTables[tableIndex]);
                }
                else if (dcOnly)
                {
                    if (refine)
                        ProgressiveEntropyEncoder.WriteDcRefinement(writer!, quantized, successiveLow);
                    else if (writer is null)
                        ProgressiveEntropyEncoder.AccumulateDcFrequencies(quantized,
                            ref previousScanDc, dcFrequencies![tableIndex], successiveLow);
                    else
                        ProgressiveEntropyEncoder.WriteDc(writer, quantized,
                            ref previousScanDc, dcTables![tableIndex], successiveLow);
                }
                else if (refine)
                {
                    if (writer is null)
                        ProgressiveEntropyEncoder.AccumulateAcRefinementFrequencies(quantized,
                            acFrequencies![tableIndex], refinementState!, successiveLow, spectralStart, spectralEnd);
                    else
                        ProgressiveEntropyEncoder.WriteAcRefinement(writer, quantized, acTables[tableIndex],
                            refinementState!, successiveLow, spectralStart, spectralEnd);
                }
                else if (writer is null)
                    ProgressiveEntropyEncoder.AccumulateAcFrequencies(quantized, acFrequencies![tableIndex],
                        ref pendingEobRun, successiveLow, spectralStart, spectralEnd);
                else
                    ProgressiveEntropyEncoder.WriteAc(writer, quantized, acTables[tableIndex], ref pendingEobRun,
                        successiveLow, spectralStart, spectralEnd);
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
