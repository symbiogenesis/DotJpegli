namespace DotJpegli.Internal;

internal static class JpegMarkerWriter
{
    public static void WriteHeaders(Stream output, int width, int height, int horizontal, int vertical,
        ushort[][] quantization, HuffmanTable[] dcTables, HuffmanTable[] acTables, int restartInterval)
    {
        Marker(output, 0xd8);
        // JFIF 1.01, unspecified density units, 1x1 density, no thumbnail.
        Marker(output, 0xe0);
        Word(output, 16);
        output.Write([0x4a, 0x46, 0x49, 0x46, 0, 1, 1, 0, 0, 1, 0, 1, 0, 0]);

        Marker(output, 0xdb);
        Word(output, 2 + quantization.Length * 65);
        for (int component = 0; component < quantization.Length; component++)
        {
            output.WriteByte((byte)component);
            foreach (int naturalIndex in JpegConstants.ZigZag)
                output.WriteByte(checked((byte)quantization[component][naturalIndex]));
        }

        Marker(output, 0xc0);
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

        if (restartInterval != 0)
        {
            Marker(output, 0xdd);
            Word(output, 4);
            Word(output, restartInterval);
        }

        Marker(output, 0xda);
        Word(output, 6 + quantization.Length * 2);
        output.WriteByte((byte)quantization.Length);
        for (int component = 0; component < quantization.Length; component++)
        {
            output.WriteByte((byte)(component + 1));
            output.WriteByte(component == 0 ? (byte)0 : (byte)0x11);
        }
        output.Write([0, 63, 0]);
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
