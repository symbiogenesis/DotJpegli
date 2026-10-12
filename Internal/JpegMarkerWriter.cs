using System.Text;

namespace DotJpegli.Internal;

internal static class JpegMarkerWriter
{
    internal const int MaxMarkerPayloadBytes = 65533;
    private const int MaxIccChunkBytes = MaxMarkerPayloadBytes - 14;
    internal const int MaxIccProfileBytes = 255 * MaxIccChunkBytes;

    public static void WriteHeaders(Stream output, int width, int height, int horizontal, int vertical,
        ushort[][] quantization, HuffmanTable[] dcTables, HuffmanTable[] acTables,
        JpegliEncoderOptions options, CancellationToken cancellationToken)
    {
        Marker(output, 0xd8);
        // Preserve the default JFIF 1.01 header, with configurable densities and no thumbnail.
        Marker(output, 0xe0);
        Word(output, 16);
        output.Write([0x4a, 0x46, 0x49, 0x46, 0, 1, 1]);
        output.WriteByte((byte)options.DensityUnit);
        Word(output, options.HorizontalDensity);
        Word(output, options.VerticalDensity);
        output.Write([0, 0]);

        WriteMetadata(output, options, cancellationToken);

        Marker(output, 0xdb);
        Word(output, 2 + quantization.Length * 65);
        for (int component = 0; component < quantization.Length; component++)
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
            output.WriteByte((byte)(component + 1));
            output.WriteByte(component == 0 ? (byte)((horizontal << 4) | vertical) : (byte)0x11);
            output.WriteByte((byte)component);
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
        CancellationToken cancellationToken)
    {
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
        int acComponent = -1, int successiveHigh = 0, int successiveLow = 0)
    {
        // Progressive DC may be interleaved; AC scans must select one component.
        int scanComponents = acComponent < 0 ? componentCount : 1;
        Marker(output, 0xda);
        Word(output, 6 + scanComponents * 2);
        output.WriteByte((byte)scanComponents);
        for (int index = 0; index < scanComponents; index++)
        {
            int component = acComponent < 0 ? index : acComponent;
            output.WriteByte((byte)(component + 1));
            int tableIndex = component == 0 ? 0 : 1;
            // Unused DC/AC selectors are zero in progressive scan headers.
            int selector = (tableIndex << 4) | tableIndex;
            if (progressive)
                selector = acComponent >= 0 ? tableIndex : successiveHigh == 0 ? tableIndex << 4 : 0;
            output.WriteByte((byte)selector);
        }
        output.WriteByte((byte)(acComponent < 0 ? 0 : 1));
        output.WriteByte((byte)(progressive && acComponent < 0 ? 0 : 63));
        output.WriteByte((byte)((successiveHigh << 4) | successiveLow));
    }

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
}
