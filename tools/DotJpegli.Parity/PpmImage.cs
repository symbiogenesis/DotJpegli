using System.Globalization;
using System.Text;

namespace DotJpegli.Parity;

internal sealed record PpmImage(int Width, int Height, byte[] Pixels)
{
    public static PpmImage Read(string path) => Read(File.ReadAllBytes(path));

    public static PpmImage Read(byte[] data)
    {
        int position = 0;
        string Token()
        {
            while (position < data.Length)
            {
                if (IsWhitespace(data[position])) { position++; continue; }
                if (data[position] == '#')
                {
                    while (position < data.Length && data[position] != '\n') position++;
                    continue;
                }
                break;
            }
            int start = position;
            while (position < data.Length && !IsWhitespace(data[position]) && data[position] != '#') position++;
            if (start == position) throw new InvalidDataException("Missing PPM header token.");
            return Encoding.ASCII.GetString(data, start, position - start);
        }
        if (Token() != "P6") throw new InvalidDataException("Corpus must contain binary RGB P6 PPM files.");
        if (!int.TryParse(Token(), NumberStyles.None, CultureInfo.InvariantCulture, out int width) || width <= 0 ||
            !int.TryParse(Token(), NumberStyles.None, CultureInfo.InvariantCulture, out int height) || height <= 0)
            throw new InvalidDataException("Invalid PPM dimensions.");
        if (Token() != "255") throw new InvalidDataException("PPM sample range must be 0..255.");
        if (position >= data.Length || !IsWhitespace(data[position])) throw new InvalidDataException("Missing PPM raster separator.");
        byte separator = data[position++];
        // Consume one separator only: the first pixel itself may be whitespace or '#'.
        if (separator == '\r' && position < data.Length && data[position] == '\n') position++;
        long length = (long)width * height * 3;
        if (length > int.MaxValue || data.Length - position != length)
            throw new InvalidDataException("PPM raster size does not match its dimensions.");
        return new(width, height, data.AsSpan(position).ToArray());
    }

    public void Write(string path)
    {
        using var stream = File.Create(path);
        stream.Write(Encoding.ASCII.GetBytes(FormattableString.Invariant($"P6\n{Width} {Height}\n255\n")));
        stream.Write(Pixels);
    }

    private static bool IsWhitespace(byte value) => value is 9 or 10 or 11 or 12 or 13 or 32;
}
