namespace DotJpegli.Performance;

using DotJpegli.Parity;

// These controls belong only to kernel diagnostics, never the fixed acceptance corpus.
internal static class KernelDiagnosticInputs
{
    internal static readonly string[] Ids = ["guard-below-32k", "guard-at-32k", "guard-above-32k", "narrow-tall"];

    internal static IEnumerable<(string Id, string Category, string SourcePath, PpmImage Image)> Create(IEnumerable<string> selectedIds)
    {
        foreach (string id in selectedIds.Where(id => Ids.Contains(id, StringComparer.Ordinal)))
        {
            (int width, int height) = id switch
            {
                "guard-below-32k" => (255, 128),
                "guard-at-32k" => (256, 128),
                "guard-above-32k" => (257, 128),
                _ => (8, 4096)
            };
            byte[] pixels = new byte[width * height * 3];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int channel = 0; channel < 3; channel++)
                    {
                        // Coordinate-based content keeps common pixels identical across guard sizes.
                        pixels[(((y * width) + x) * 3) + channel] = (byte)(((x * 29) + (y * 53) + (channel * 71)) & 255);
                    }
                }
            }

            yield return (id, "kernel-diagnostic", "generated:" + id, new PpmImage(width, height, pixels));
        }
    }
}
