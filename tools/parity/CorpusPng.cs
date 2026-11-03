// Restricted, lossless RGB8 PNG decoder for the hash-pinned parity corpus.
// Loaded by PowerShell with Add-Type; no image library or color transforms.

#pragma warning disable IDE0130 // PowerShell loads this standalone type by its fixed DotJpegli.Tools name.
namespace DotJpegli.Tools;
#pragma warning restore IDE0130

using System;

public static class CorpusPng
{
    public static byte[] ToPpm(byte[] data)
    {
        byte[] signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
        if (data.Length < signature.Length ||
            !MemoryExtensions.SequenceEqual(data.AsSpan(0, 8), signature))
        {
            throw new System.IO.InvalidDataException("Not a PNG");
        }

        int width = 0, height = 0, position = 8;
        bool ended = false;
        using var compressed = new System.IO.MemoryStream();
        while (position < data.Length)
        {
            if (data.Length - position < 12)
            {
                throw new System.IO.InvalidDataException("Truncated PNG chunk");
            }

            uint length = ReadUInt32(data, position);
            if (length > (uint)(data.Length - position - 12))
            {
                throw new System.IO.InvalidDataException("Truncated PNG payload");
            }

            int size = (int)length;
            string kind = System.Text.Encoding.ASCII.GetString(data, position + 4, 4);
            int payload = position + 8;
            if (Crc32(data, position + 4, size + 4) != ReadUInt32(data, payload + size))
            {
                throw new System.IO.InvalidDataException("PNG CRC mismatch");
            }

            if (position == 8 && kind != "IHDR")
            {
                throw new System.IO.InvalidDataException("Missing IHDR");
            }

            if (kind == "IHDR")
            {
                if (width != 0 || size != 13)
                {
                    throw new System.IO.InvalidDataException("Invalid IHDR");
                }

                uint pngWidth = ReadUInt32(data, payload), pngHeight = ReadUInt32(data, payload + 4);
                if (pngWidth == 0 || pngHeight == 0 || (ulong)pngWidth * pngHeight > 1024 * 1024)
                {
                    throw new System.IO.InvalidDataException("Unexpected corpus dimensions");
                }

                width = (int)pngWidth;
                height = (int)pngHeight;
                if (data[payload + 8] != 8 || data[payload + 9] != 2 ||
                    data[payload + 10] != 0 || data[payload + 11] != 0 || data[payload + 12] != 0)
                {
                    throw new System.IO.InvalidDataException("Expected noninterlaced RGB8 PNG");
                }
            }
            else if (kind == "IDAT")
            {
                compressed.Write(data, payload, size);
            }
            else if (kind == "IEND")
            {
                if (size != 0)
                {
                    throw new System.IO.InvalidDataException("Invalid IEND");
                }

                ended = true;
                break;
            }
            else if (data[position + 4] is >= ((byte)'A') and <= ((byte)'Z'))
            {
                throw new System.IO.InvalidDataException("Unsupported critical chunk " + kind);
            }

            position += size + 12;
        }
        if (width == 0 || !ended)
        {
            throw new System.IO.InvalidDataException("Missing IHDR or IEND");
        }

        int stride = width * 3;
        byte[] raw = new byte[(stride + 1) * height];
        compressed.Position = 0;
        using (var inflater = new System.IO.Compression.ZLibStream(compressed, System.IO.Compression.CompressionMode.Decompress))
        {
            int read = 0, count;
            while (read < raw.Length && (count = inflater.Read(raw, read, raw.Length - read)) != 0)
            {
                read += count;
            }

            if (read != raw.Length || inflater.ReadByte() != -1)
            {
                throw new System.IO.InvalidDataException("Unexpected pixel extent");
            }
        }
        byte[] header = System.Text.Encoding.ASCII.GetBytes("P6\n" + width + " " + height + "\n255\n");
        byte[] ppm = new byte[header.Length + (stride * height)];
        Buffer.BlockCopy(header, 0, ppm, 0, header.Length);
        for (int row = 0; row < height; row++)
        {
            int start = row * (stride + 1), target = header.Length + (row * stride);
            int mode = raw[start];
            if (mode > 4)
            {
                throw new System.IO.InvalidDataException("Unknown PNG filter");
            }

            for (int x = 0; x < stride; x++)
            {
                int left = x >= 3 ? ppm[target + x - 3] : 0;
                int above = row > 0 ? ppm[target + x - stride] : 0;
                int upperLeft = row > 0 && x >= 3 ? ppm[target + x - stride - 3] : 0;
                int predictor = mode == 1 ? left : mode == 2 ? above :
                    mode == 3 ? (left + above) / 2 : mode == 4 ? Paeth(left, above, upperLeft) : 0;
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

    private static uint ReadUInt32(byte[] data, int offset) => ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) |
            ((uint)data[offset + 2] << 8) | data[offset + 3];

    private static uint Crc32(byte[] data, int offset, int length)
    {
        uint crc = 0xffffffff;
        for (int i = offset; i < offset + length; i++)
        {
            crc ^= data[i];
            for (int bit = 0; bit < 8; bit++)
            {
                crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xedb88320U : 0U);
            }
        }
        return crc ^ 0xffffffff;
    }
}
