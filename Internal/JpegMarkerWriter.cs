using System.Text;

namespace DotJpegli.Internal;

internal static class JpegMarkerWriter
{
    internal const int MaxMarkerPayloadBytes = 65533;
    internal const int MaxExifDataBytes = MaxMarkerPayloadBytes - 6;
    internal const int MaxXmpPacketBytes = MaxMarkerPayloadBytes - 29;
    // The 14-byte Photoshop signature and 12-byte resource header leave 65,507
    // bytes; an odd payload also needs one padding byte, so the limit is even.
    internal const int MaxIptcDataBytes = (MaxMarkerPayloadBytes - 26) & ~1;
    internal static readonly UTF8Encoding XmpEncoding = new(false, true);
    private const int MaxIccChunkBytes = MaxMarkerPayloadBytes - 14;
    internal const int MaxIccProfileBytes = 255 * MaxIccChunkBytes;

    public static void WriteHeaders(Stream output, int width, int height, JpegSamplingFactors sampling,
        ushort[][] quantization, HuffmanTable[] dcTables, HuffmanTable[] acTables,
        JpegliEncoderOptions options, ExtendedXmpMetadata? extendedXmp, CancellationToken cancellationToken,
        bool rgb = false, bool xyb = false)
    {
        Marker(output, 0xd8);
        if (!rgb)
        {
            // JFIF describes grayscale or YCbCr. RGB uses R/G/B frame identifiers instead.
            Marker(output, 0xe0);
            Word(output, 16);
            output.Write([0x4a, 0x46, 0x49, 0x46, 0, 1, 1]);
            output.WriteByte((byte)options.DensityUnit);
            Word(output, options.HorizontalDensity);
            Word(output, options.VerticalDensity);
            output.Write([0, 0]);
        }

        WriteMetadata(output, options, extendedXmp, cancellationToken);

        int tableCount = xyb ? 3 : rgb ? 1 : options.UseStandardQuantizationTables && quantization.Length == 3 ? 2 : quantization.Length;
        Marker(output, 0xdb);
        Word(output, 2 + tableCount * 65);
        for (int component = 0; component < tableCount; component++)
        {
            output.WriteByte((byte)component);
            foreach (int naturalIndex in JpegConstants.ZigZag)
                output.WriteByte(checked((byte)quantization[component][naturalIndex]));
        }

        Marker(output, options.Progressive ? (byte)0xc2 : (byte)0xc0);
        Word(output, 8 + quantization.Length * 3);
        output.WriteByte(8);
        Word(output, height);
        Word(output, width);
        output.WriteByte((byte)quantization.Length);
        for (int component = 0; component < quantization.Length; component++)
        {
            output.WriteByte(ComponentId(component, rgb));
            JpegComponentSampling factor = sampling[component];
            output.WriteByte((byte)((factor.Horizontal << 4) | factor.Vertical));
            output.WriteByte((byte)(xyb ? component : rgb ? 0 : options.UseStandardQuantizationTables && component > 0 ? 1 : component));
        }

        for (int index = 0; index < dcTables.Length; index++)
        {
            Huffman(output, dcTables[index], index);
            Huffman(output, acTables[index], 0x10 | index);
        }

        if (options.RestartInterval != 0)
        {
            Marker(output, 0xdd);
            Word(output, 4);
            Word(output, options.RestartInterval);
        }
    }

    private static void WriteMetadata(Stream output, JpegliEncoderOptions options,
        ExtendedXmpMetadata? extendedXmp, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!options.ExifData.IsEmpty)
        {
            // Exif's six-byte identifier precedes the caller's unmodified TIFF stream.
            // Keep APP0 first for JFIF interoperability, as in Adobe's JPEG handler.
            Marker(output, 0xe1);
            Word(output, options.ExifData.Length + 8);
            output.Write("Exif\0\0"u8);
            output.Write(options.ExifData.Span);
        }

        cancellationToken.ThrowIfCancellationRequested();
        string? standardXmp = extendedXmp?.StandardPacket ?? options.XmpPacket;
        if (!string.IsNullOrEmpty(standardXmp))
        {
            // A standard XMP packet occupies a single APP1 segment. Validation
            // bounds the UTF-8 allocation and rejects unpaired UTF-16 surrogates.
            byte[] packet = XmpEncoding.GetBytes(standardXmp);
            Marker(output, 0xe1);
            Word(output, packet.Length + 31);
            output.Write("http://ns.adobe.com/xap/1.0/\0"u8);
            output.Write(packet);
        }

        if (extendedXmp is not null)
        {
            var chunks = new ExtendedXmpMetadata.Utf8Chunks(extendedXmp.Packet);
            byte[] digest = Encoding.ASCII.GetBytes(extendedXmp.Digest);
            uint offset = 0;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ReadOnlySpan<byte> chunk = chunks.Read();
                if (chunk.IsEmpty)
                    break;
                Marker(output, 0xe1);
                Word(output, chunk.Length + 77);
                output.Write("http://ns.adobe.com/xmp/extension/\0"u8);
                output.Write(digest);
                DoubleWord(output, extendedXmp.Length);
                DoubleWord(output, offset);
                output.Write(chunk);
                offset += (uint)chunk.Length;
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        ReadOnlySpan<byte> profile = options.IccProfile.Span;
        int chunkCount = (profile.Length + MaxIccChunkBytes - 1) / MaxIccChunkBytes;
        // ICC.1 Annex B: APP2 payloads begin with ICC_PROFILE\0 and the
        // one-based sequence number and total count. No entropy byte stuffing applies.
        for (int chunk = 0; chunk < chunkCount; chunk++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int offset = chunk * MaxIccChunkBytes;
            int length = Math.Min(MaxIccChunkBytes, profile.Length - offset);
            Marker(output, 0xe2);
            Word(output, length + 16);
            output.Write("ICC_PROFILE\0"u8);
            output.WriteByte((byte)(chunk + 1));
            output.WriteByte((byte)chunkCount);
            output.Write(profile.Slice(offset, length));
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (!options.IptcData.IsEmpty)
        {
            // Adobe's Photoshop image resource format: the size excludes the
            // even-byte data padding, and an empty Pascal name occupies two bytes.
            ReadOnlySpan<byte> iptc = options.IptcData.Span;
            int padding = iptc.Length & 1;
            Marker(output, 0xed);
            Word(output, iptc.Length + padding + 28);
            output.Write("Photoshop 3.0\0"u8);
            output.Write("8BIM"u8);
            Word(output, 0x0404);
            output.Write([0, 0]);
            DoubleWord(output, (uint)iptc.Length);
            output.Write(iptc);
            if (padding != 0)
                output.WriteByte(0);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (!string.IsNullOrEmpty(options.Comment))
        {
            // Validation bounds this allocation to one marker's payload.
            byte[] comment = Encoding.UTF8.GetBytes(options.Comment);
            Marker(output, 0xfe);
            Word(output, comment.Length + 2);
            output.Write(comment);
        }
        cancellationToken.ThrowIfCancellationRequested();
    }

    public static void WriteScanHeader(Stream output, int componentCount, bool progressive,
        int acComponent = -1, int successiveHigh = 0, int successiveLow = 0, bool rgb = false,
        int spectralStart = 1, int spectralEnd = 63)
    {
        // Progressive DC may be interleaved; AC scans must select one component.
        int scanComponents = acComponent < 0 ? componentCount : 1;
        Marker(output, 0xda);
        Word(output, 6 + scanComponents * 2);
        output.WriteByte((byte)scanComponents);
        for (int index = 0; index < scanComponents; index++)
        {
            int component = acComponent < 0 ? index : acComponent;
            output.WriteByte(ComponentId(component, rgb));
            int tableIndex = rgb || component == 0 ? 0 : 1;
            // Unused DC/AC selectors are zero in progressive scan headers.
            int selector = (tableIndex << 4) | tableIndex;
            if (progressive)
                selector = acComponent >= 0 ? tableIndex : successiveHigh == 0 ? tableIndex << 4 : 0;
            output.WriteByte((byte)selector);
        }
        output.WriteByte((byte)(acComponent < 0 ? 0 : spectralStart));
        output.WriteByte((byte)(acComponent >= 0 ? spectralEnd : progressive ? 0 : 63));
        output.WriteByte((byte)((successiveHigh << 4) | successiveLow));
    }

    private static byte ComponentId(int component, bool rgb)
        => rgb ? "RGB"u8[component] : (byte)(component + 1);

    private static void Huffman(Stream output, HuffmanTable table, int selector)
    {
        Marker(output, 0xc4);
        Word(output, 19 + table.Symbols.Length);
        output.WriteByte((byte)selector);
        output.Write(table.Counts);
        output.Write(table.Symbols);
    }

    private static void Marker(Stream output, byte code)
    {
        output.WriteByte(0xff);
        output.WriteByte(code);
    }

    private static void Word(Stream output, int value)
    {
        output.WriteByte((byte)(value >> 8));
        output.WriteByte((byte)value);
    }

    private static void DoubleWord(Stream output, uint value)
    {
        output.WriteByte((byte)(value >> 24));
        output.WriteByte((byte)(value >> 16));
        output.WriteByte((byte)(value >> 8));
        output.WriteByte((byte)value);
    }
}
