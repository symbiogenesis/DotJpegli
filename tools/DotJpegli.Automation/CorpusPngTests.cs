namespace DotJpegli.Automation;

internal static class CorpusPngTests
{
    internal static void Run(Func<byte[], byte[]> decode)
    {
        // Each filter sees multiple RGB pixels and an earlier row. Values wrap
        // modulo 256, and the Paeth rows exercise left/above/upper-left choices.
        byte[] pixels = new byte[3 * 4 * 10];
#pragma warning disable CA5394 // Fixed-seed synthetic pixels must be reproducible; no security-sensitive randomness is used.
        new Random(1729).NextBytes(pixels);
#pragma warning restore CA5394
        byte[] expectedHeader = System.Text.Encoding.ASCII.GetBytes("P6\n4 10\n255\n");
        byte[] expected = new byte[expectedHeader.Length + pixels.Length];
        Buffer.BlockCopy(expectedHeader, 0, expected, 0, expectedHeader.Length);
        Buffer.BlockCopy(pixels, 0, expected, expectedHeader.Length, pixels.Length);
        byte[] png = MakePng(pixels);
        if (!MemoryExtensions.SequenceEqual(decode(png), expected))
        {
            throw new InvalidOperationException("Lossless PNG filters changed RGB samples.");
        }

        byte[] corrupt = (byte[])png.Clone();
        corrupt[29] ^= 1;
        Reject(decode, corrupt, "CRC");
        corrupt = (byte[])png.Clone();
        corrupt[0] = 0;
        Reject(decode, corrupt, "signature");
        Reject(decode, [137, 80, 78, 71, 13, 10, 26, 10, 0], "truncation");
        Reject(decode, MakePng(pixels, depth: 16), "RGB8 restriction");
        Reject(decode, MakePng(pixels, firstFilter: 5), "unknown filter");
        Reject(decode, MakePng(pixels, extraRawByte: true), "pixel extent");
        Reject(decode, MakePng(pixels, critical: true), "critical chunk");
        byte[] missingEnd = new byte[png.Length - 12];
        Buffer.BlockCopy(png, 0, missingEnd, 0, missingEnd.Length);
        Reject(decode, missingEnd, "missing IEND");
    }

    private static void Reject(Func<byte[], byte[]> decode, byte[] png, string label)
    {
        try
        { _ = decode(png); }
        catch (InvalidDataException) { return; }
        throw new InvalidOperationException("Accepted invalid PNG: " + label);
    }

    internal static byte[] MakePng(byte[] pixels, byte depth = 8, int firstFilter = 0, bool extraRawByte = false, bool critical = false)
    {
        byte[] raw = new byte[130 + (extraRawByte ? 1 : 0)];
        for (int row = 0; row < 10; row++)
        {
            int mode = row == 0 ? firstFilter : row % 5;
            raw[row * 13] = (byte)mode;
            for (int x = 0; x < 12; x++)
            {
                int index = (row * 12) + x;
                int left = x >= 3 ? pixels[index - 3] : 0;
                int above = row > 0 ? pixels[index - 12] : 0;
                int upperLeft = row > 0 && x >= 3 ? pixels[index - 15] : 0;
                int p = left + above - upperLeft;
                int a = Math.Abs(p - left), b = Math.Abs(p - above), c = Math.Abs(p - upperLeft);
                int paeth = a <= b && a <= c ? left : b <= c ? above : upperLeft;
                int predictor = mode == 1 ? left : mode == 2 ? above : mode == 3 ? (left + above) / 2 : mode == 4 ? paeth : 0;
                raw[(row * 13) + 1 + x] = unchecked((byte)(pixels[index] - predictor));
            }
        }
        using var output = new MemoryStream();
        output.Write([137, 80, 78, 71, 13, 10, 26, 10]);
        Chunk(output, "IHDR", [0, 0, 0, 4, 0, 0, 0, 10, depth, 2, 0, 0, 0]);
        Chunk(output, critical ? "TEST" : "tEXt", System.Text.Encoding.ASCII.GetBytes("Comment\0ignored"));
        using (var compressed = new MemoryStream())
        {
            using (var zlib = new System.IO.Compression.ZLibStream(compressed, System.IO.Compression.CompressionLevel.Optimal, true))
            {
                zlib.Write(raw);
            }

            byte[] all = compressed.ToArray();
            byte[] first = new byte[all.Length / 2], second = new byte[all.Length - first.Length];
            Buffer.BlockCopy(all, 0, first, 0, first.Length);
            Buffer.BlockCopy(all, first.Length, second, 0, second.Length);
            Chunk(output, "IDAT", first);
            Chunk(output, "IDAT", second);
        }
        Chunk(output, "IEND", []);
        return output.ToArray();
    }

    private static void Chunk(Stream output, string kind, byte[] payload)
    {
        UInt32(output, (uint)payload.Length);
        byte[] name = System.Text.Encoding.ASCII.GetBytes(kind);
        output.Write(name);
        output.Write(payload);
        uint crc = 0xffffffff;
        foreach (byte value in Enumerable.Concat(name, payload))
        {
            crc ^= value;
            for (int bit = 0; bit < 8; bit++)
            {
                crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xedb88320U : 0U);
            }
        }
        UInt32(output, crc ^ 0xffffffff);
    }

    private static void UInt32(Stream output, uint value) => output.Write([(byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value]);
}
