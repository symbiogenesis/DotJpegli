namespace DotJpegli.Automation;

using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;

internal static class CorpusCommand
{
    internal static async Task Run(Arguments args)
    {
        if (args.Has("self-test"))
        {
            CorpusPngTests.Run(CorpusPng.ToPpm);
            await CorpusPreparationTests.Run();
            Console.WriteLine("Corpus preparation self-test passed (all PNG filters, invalid-input and preparation checks).");
            return;
        }

        await Prepare(
            args.Get("output"),
            args.Optional("source"),
            args.Optional("manifest") ?? Path.Combine(ReleaseCommand.FindRepositoryRoot(), "tools/parity/corpus.json"));
    }

    internal static async Task Prepare(string output, string? source, string manifestPath)
    {
        if (!Path.IsPathFullyQualified(output) || (source is not null && !Path.IsPathFullyQualified(source)))
        {
            throw new InvalidDataException("Output and source paths must be absolute.");
        }

        if (Path.Exists(output))
        {
            throw new InvalidDataException($"Output already exists: {output}");
        }

        _ = Directory.CreateDirectory(output);
        string manifestText = File.ReadAllText(manifestPath);
        using var manifest = JsonDocument.Parse(manifestText);
        using HttpClient? client = source is null ? new HttpClient { Timeout = TimeSpan.FromSeconds(60) } : null;
        foreach (JsonElement entry in manifest.RootElement.GetProperty("images").EnumerateArray())
        {
            string name = entry.GetProperty("name").GetString() ?? throw new InvalidDataException("Missing corpus image name.");
            if (name.Length == 0 || name != Path.GetFileName(name) || name.Contains('\\', StringComparison.Ordinal))
            {
                throw new InvalidDataException("Corpus image names must be simple filenames.");
            }

            byte[] data = source is not null
                ? File.ReadAllBytes(Path.Combine(source, name))
                : await client!.GetByteArrayAsync(manifest.RootElement.GetProperty("baseUrl").GetString() + name);
            if (!string.Equals(Hash(data), entry.GetProperty("pngSha256").GetString(), StringComparison.Ordinal))
            {
                throw new InvalidDataException($"Source hash mismatch: {name}");
            }

            byte[] ppm = CorpusPng.ToPpm(data);
            if (!string.Equals(Hash(ppm), entry.GetProperty("ppmSha256").GetString(), StringComparison.Ordinal))
            {
                throw new InvalidDataException($"Decoded pixel hash mismatch: {name}");
            }

            File.WriteAllBytes(Path.Combine(output, Path.ChangeExtension(name, ".ppm")), ppm);
            Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"Verified {name}: {entry.GetProperty("width").GetInt32()}x{entry.GetProperty("height").GetInt32()}"));
        }

        File.WriteAllText(Path.Combine(output, "corpus.json"), manifestText.ReplaceLineEndings("\n"));
    }

    internal static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
}
