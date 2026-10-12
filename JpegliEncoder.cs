using DotJpegli.Internal;

namespace DotJpegli;

/// <summary>
/// Encodes eight-bit pixels to baseline JPEG using safe managed code.
/// Includes spatial adaptive quantization; progressive scans and XYB remain pending.
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
    /// Source pixels must remain unchanged during both passes when Huffman optimization is enabled.
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
        int horizontal = !grayscale && options.Subsampling != ChromaSubsampling.None ? 2 : 1;
        int vertical = !grayscale && options.Subsampling == ChromaSubsampling.Quarter ? 2 : 1;
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
                tables, options.Quality, adaptive, adaptiveMultipliers, dcTables, acTables,
                null, dcFrequencies, acFrequencies, cancellationToken);
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
        JpegMarkerWriter.WriteHeaders(output, width, height, horizontal, vertical, tables, dcTables, acTables);
        var bits = new BitWriter(output);
        ProcessBlocks(pixels, width, height, stride, bytesPerPixel, layout, horizontal, vertical,
            tables, options.Quality, adaptive, adaptiveMultipliers, dcTables, acTables,
            bits, null, null, cancellationToken);
        bits.Flush();
        output.WriteByte(0xff);
        output.WriteByte(0xd9);
    }

    private static void ProcessBlocks(ReadOnlySpan<byte> pixels, int width, int height, int stride,
        int bytesPerPixel, PixelLayout layout, int horizontal, int vertical, ushort[][] tables,
        float quality, AdaptiveQuantization? adaptive, float[][]? adaptiveMultipliers,
        HuffmanTable[] dcTables, HuffmanTable[] acTables, BitWriter? writer,
        long[][]? dcFrequencies, long[][]? acFrequencies, CancellationToken cancellationToken)
    {
        Span<float> samples = stackalloc float[64];
        Span<float> coefficients = stackalloc float[64];
        Span<short> quantized = stackalloc short[64];
        Span<int> previousDc = stackalloc int[3];
        previousDc.Clear();
        float distance = Quantization.QualityToDistance(quality);
        int mcuWidth = horizontal * 8;
        int mcuHeight = vertical * 8;
        float[]? strengths = adaptive is null ? null : new float[adaptive.BlocksWide * vertical];
        for (int top = 0; top < height; top += mcuHeight)
        {
            if (adaptive is not null)
                adaptive.ComputeRow(pixels, stride, bytesPerPixel, layout, top, tables[0][1], strengths!, cancellationToken);
            for (int left = 0; left < width; left += mcuWidth)
            {
                cancellationToken.ThrowIfCancellationRequested();
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
                            FillBlock(pixels, width, height, stride, bytesPerPixel, layout, component,
                                left + blockX * 8, top + blockY * 8, sampleWidth, sampleHeight, samples);
                            ForwardDct.Transform(samples, coefficients);
                            // Upstream samples the top-left luma field value for a chroma block.
                            // Edge blocks beyond the real luma width reuse its last field value.
                            float strength = adaptive is null ? 0f : strengths![
                                (component == 0 ? blockY : 0) * adaptive.BlocksWide +
                                Math.Min(left / 8 + (component == 0 ? blockX : 0), adaptive.BlocksWide - 1)];
                            Quantization.Quantize(coefficients, tables[component], quantized, component, distance,
                                grayscale: layout == PixelLayout.Gray8, adaptiveQuantization: adaptive is not null,
                                adaptiveStrength: strength,
                                adaptiveMultipliers: adaptiveMultipliers is null ? default : adaptiveMultipliers[component],
                                previousDc: previousDc[component]);
                            int tableIndex = component == 0 ? 0 : 1;
                            if (writer is null)
                                EntropyEncoder.AccumulateFrequencies(quantized, ref previousDc[component],
                                    dcFrequencies![tableIndex], acFrequencies![tableIndex]);
                            else
                                EntropyEncoder.WriteBlock(writer, quantized, ref previousDc[component],
                                    dcTables[tableIndex], acTables[tableIndex]);
                        }
                    }
                }
            }
        }
    }

    // Replicate edge pixels before box downsampling so odd dimensions remain valid.
    // This is a basic JPEG sampling path; SharpYUV and upstream streaming filters are pending.
    private static void FillBlock(ReadOnlySpan<byte> pixels, int width, int height, int stride,
        int bytesPerPixel, PixelLayout layout, int component, int left, int top,
        int sampleWidth, int sampleHeight, Span<float> samples)
    {
        bool bgr = layout is PixelLayout.Bgr24 or PixelLayout.Bgra32;
        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                float sum = 0;
                for (int dy = 0; dy < sampleHeight; dy++)
                {
                    int y = Math.Min(top + row * sampleHeight + dy, height - 1);
                    for (int dx = 0; dx < sampleWidth; dx++)
                    {
                        int x = Math.Min(left + column * sampleWidth + dx, width - 1);
                        int offset = y * stride + x * bytesPerPixel;
                        if (layout == PixelLayout.Gray8)
                            sum += pixels[offset];
                        else
                        {
                            byte r = pixels[offset + (bgr ? 2 : 0)];
                            byte g = pixels[offset + 1];
                            byte b = pixels[offset + (bgr ? 0 : 2)];
                            ColorConversion.RgbToYCbCr(r, g, b, out float luma, out float cb, out float cr);
                            sum += component switch { 0 => luma, 1 => cb, _ => cr };
                        }
                    }
                }
                samples[row * 8 + column] = sum / (sampleWidth * sampleHeight) - 128;
            }
        }
    }
}
