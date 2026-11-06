namespace DotJpegli.Parity;

using System.Globalization;
using System.Text.Unicode;

internal sealed record PpmImage(int Width, int Height, byte[] Pixels)
{
    public static PpmImage Read(string path) => Read(File.ReadAllBytes(path));

    public static PpmImage Read(ReadOnlySpan<byte> data)
    {
        int position = 0;
        if (!NextToken(data, ref position).SequenceEqual("P6"u8))
        {
            throw new InvalidDataException("Corpus must contain binary RGB P6 PPM files.");
        }

        if (!int.TryParse(NextToken(data, ref position), NumberStyles.None, CultureInfo.InvariantCulture, out int width) || width <= 0 ||
            !int.TryParse(NextToken(data, ref position), NumberStyles.None, CultureInfo.InvariantCulture, out int height) || height <= 0)
        {
            throw new InvalidDataException("Invalid PPM dimensions.");
        }

        if (!NextToken(data, ref position).SequenceEqual("255"u8))
        {
            throw new InvalidDataException("PPM sample range must be 0..255.");
        }

        if (position >= data.Length || !IsWhitespace(data[position]))
        {
            throw new InvalidDataException("Missing PPM raster separator.");
        }

        byte separator = data[position++];
        // Consume one separator only: the first pixel itself may be whitespace or '#'.
        if (separator == '\r' && position < data.Length && data[position] == '\n')
        {
            position++;
        }

        long length = (long)width * height * 3;
        if (length > int.MaxValue || data.Length - position != length)
        {
            throw new InvalidDataException("PPM raster size does not match its dimensions.");
        }

        return new(width, height, data[position..].ToArray());
    }

    public void Write(string path)
    {
        using FileStream stream = File.Create(path);
        Span<byte> header = stackalloc byte[32];
        _ = Utf8.TryWrite(header, CultureInfo.InvariantCulture, $"P6\n{this.Width} {this.Height}\n255\n", out int written);
        stream.Write(header[..written]);
        stream.Write(this.Pixels);
    }

    private static ReadOnlySpan<byte> NextToken(ReadOnlySpan<byte> data, ref int position)
    {
        while (position < data.Length)
        {
            if (IsWhitespace(data[position]))
            {
                position++;
                continue;
            }
            if (data[position] == '#')
            {
                int newline = data[position..].IndexOf((byte)'\n');
                position = newline < 0 ? data.Length : position + newline + 1;
                continue;
            }
            break;
        }
        int start = position;
        while (position < data.Length && !IsWhitespace(data[position]) && data[position] != '#')
        {
            position++;
        }

        if (start == position)
        {
            throw new InvalidDataException("Missing PPM header token.");
        }

        return data[start..position];
    }

    private static bool IsWhitespace(byte value) => value is 9 or 10 or 11 or 12 or 13 or 32;
}
