namespace DotJpegli.Parity;

internal static class ControlledCorpus
{
    public static IEnumerable<(string Name, PpmImage Image)> Create()
    {
        foreach (string name in new[] { "smooth-gradient", "low-contrast", "color-edges", "line-art", "structured-texture", "low-noise", "full-noise", "tiny-odd-edges" })
        {
            int width = name == "tiny-odd-edges" ? 19 : 257;
            int height = name == "tiny-odd-edges" ? 17 : 193;
            byte[] pixels = new byte[width * height * 3];
            uint random = 0x6d2b79f5;
            int NextNoise()
            {
                random ^= random << 13; random ^= random >> 17; random ^= random << 5;
                return (int)(random & 255);
            }
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int channel = 0; channel < 3; channel++)
                    {
                        int value = name switch
                        {
                            "smooth-gradient" => channel == 0 ? x * 255 / (width - 1) : channel == 1 ? y * 255 / (height - 1) : (x + y) * 255 / (width + height - 2),
                            "low-contrast" => 110 + (x * (channel + 1) + y * 2) % 29,
                            "color-edges" => (((x / 13) + (y / 17) + channel) % 3) == 0 ? 240 : 15,
                            "line-art" => (x % 19 < 2 || y % 23 < 2 || (x + y) % 41 < 2) ? 8 : 245,
                            "structured-texture" => 40 + ((x * x / 13 + y * y / 17 + x * y / 19 + channel * 43) % 177),
                            "low-noise" => 100 + NextNoise() / 8 + x * 60 / width,
                            "full-noise" => NextNoise(),
                            _ => (x * 29 + y * 53 + channel * 71 + (x == width - 1 || y == height - 1 ? 127 : 0)) & 255
                        };
                        pixels[(y * width + x) * 3 + channel] = (byte)value;
                    }
                }
            }
            yield return (name, new PpmImage(width, height, pixels));
        }
    }
}
