// Restricted, lossless RGB8 PNG decoder for the hash-pinned parity corpus.
// No image library or color transforms.

namespace DotJpegli.Automation;

using System.Buffers.Binary;
using System.Globalization;
using System.IO.Compression;
using System.Text;

internal static class CorpusPng
{
    internal static byte[] ToPpm(byte[] data)
    {
        if (!data.AsSpan().StartsWith((ReadOnlySpan<byte>)[137, 80, 78, 71, 13, 10, 26, 10]))
        {
            throw new InvalidDataException("Not a PNG");
        }

        int width = 0, height = 0, position = 8;
        bool ended = false;
        using var compressed = new MemoryStream();
        while (position < data.Length)
        {
            if (data.Length - position < 12)
            {
                throw new InvalidDataException("Truncated PNG chunk");
            }

            uint length = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(position, 4));
            if (length > (uint)(data.Length - position - 12))
            {
                throw new InvalidDataException("Truncated PNG payload");
            }

            int size = (int)length;
            ReadOnlySpan<byte> kind = data.AsSpan(position + 4, 4);
            int payload = position + 8;
            if (Crc32(data.AsSpan(position + 4, size + 4)) != BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(payload + size, 4)))
            {
                throw new InvalidDataException("PNG CRC mismatch");
            }

            if (position == 8 && !kind.SequenceEqual("IHDR"u8))
            {
                throw new InvalidDataException("Missing IHDR");
            }

            if (kind.SequenceEqual("IHDR"u8))
            {
                if (width != 0 || size != 13)
                {
                    throw new InvalidDataException("Invalid IHDR");
                }

                uint pngWidth = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(payload, 4));
                uint pngHeight = BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(payload + 4, 4));
                if (pngWidth == 0 || pngHeight == 0 || (ulong)pngWidth * pngHeight > 1024 * 1024)
                {
                    throw new InvalidDataException("Unexpected corpus dimensions");
                }

                width = (int)pngWidth;
                height = (int)pngHeight;
                if (!data.AsSpan(payload + 8, 5).SequenceEqual((ReadOnlySpan<byte>)[8, 2, 0, 0, 0]))
                {
                    throw new InvalidDataException("Expected noninterlaced RGB8 PNG");
                }
            }
            else if (kind.SequenceEqual("IDAT"u8))
            {
                compressed.Write(data, payload, size);
            }
            else if (kind.SequenceEqual("IEND"u8))
            {
                if (size != 0)
                {
                    throw new InvalidDataException("Invalid IEND");
                }

                ended = true;
                break;
            }
            else if (data[position + 4] is >= ((byte)'A') and <= ((byte)'Z'))
            {
                throw new InvalidDataException("Unsupported critical chunk " + Encoding.ASCII.GetString(kind));
            }

            position += size + 12;
        }
        if (width == 0 || !ended)
        {
            throw new InvalidDataException("Missing IHDR or IEND");
        }

        int stride = width * 3;
        byte[] raw = new byte[(stride + 1) * height];
        compressed.Position = 0;
        using (var inflater = new ZLibStream(compressed, CompressionMode.Decompress))
        {
            if (inflater.ReadAtLeast(raw, raw.Length, throwOnEndOfStream: false) != raw.Length || inflater.ReadByte() != -1)
            {
                throw new InvalidDataException("Unexpected pixel extent");
            }
        }
        byte[] header = Encoding.ASCII.GetBytes(string.Create(CultureInfo.InvariantCulture, $"P6\n{width} {height}\n255\n"));
        byte[] ppm = new byte[header.Length + (stride * height)];
        header.CopyTo(ppm, 0);
        for (int row = 0; row < height; row++)
        {
            int start = row * (stride + 1), target = header.Length + (row * stride);
            int mode = raw[start];
            for (int x = 0; x < stride; x++)
            {
                int left = x >= 3 ? ppm[target + x - 3] : 0;
                int above = row > 0 ? ppm[target + x - stride] : 0;
                int upperLeft = row > 0 && x >= 3 ? ppm[target + x - stride - 3] : 0;
                int predictor = mode switch
                {
                    0 => 0,
                    1 => left,
                    2 => above,
                    3 => (left + above) / 2,
                    4 => Paeth(left, above, upperLeft),
                    _ => throw new InvalidDataException("Unknown PNG filter"),
                };
                ppm[target + x] = unchecked((byte)(raw[start + 1 + x] + predictor));
            }
        }
        return ppm;
    }

    private static int Paeth(int left, int above, int upperLeft)
    {
        int prediction = left + above - upperLeft;
        int a = Math.Abs(prediction - left), b = Math.Abs(prediction - above), c = Math.Abs(prediction - upperLeft);
        return a <= b && a <= c ? left : b <= c ? above : upperLeft;
    }

    private static uint Crc32(ReadOnlySpan<byte> data)
    {
        uint crc = 0xffffffff;
        foreach (byte value in data)
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++)
            {
                crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xedb88320U : 0U);
            }
        }
        return crc ^ 0xffffffff;
    }
}
