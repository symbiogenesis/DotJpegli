namespace DotJpegli.Automation;

using System.Text.Json;

internal static class CorpusPreparationTests
{
    internal static async Task Run()
    {
        string temporaryRoot = Path.GetFullPath(Path.GetTempPath());
        string root = Path.Combine(temporaryRoot, "DotJpegli-corpus-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(root);
        try
        {
            string source = Path.Combine(root, "source");
            _ = Directory.CreateDirectory(source);
            byte[] png = CorpusPngTests.MakePng(new byte[120]);
            byte[] ppm = CorpusPng.ToPpm(png);
            File.WriteAllBytes(Path.Combine(source, "synthetic.png"), png);
            string manifestPath = Path.Combine(root, "manifest.json");
            string manifest = JsonSerializer.Serialize(new
            {
                images = new[] { new { name = "synthetic.png", pngSha256 = CorpusCommand.Hash(png), ppmSha256 = CorpusCommand.Hash(ppm), width = 4, height = 10 } },
            });
            File.WriteAllText(manifestPath, manifest + "\r\n");
            string output = Path.Combine(root, "valid");
            await CorpusCommand.Prepare(output, source, manifestPath);
            if (!File.ReadAllBytes(Path.Combine(output, "synthetic.ppm")).AsSpan().SequenceEqual(ppm) ||
                File.ReadAllText(Path.Combine(output, "corpus.json")) != manifest + "\n")
            {
                throw new InvalidOperationException("Corpus preparation changed pixels or the canonical manifest.");
            }

            await Reject(() => CorpusCommand.Prepare(output, source, manifestPath), "Output already exists");
            await Reject(() => CorpusCommand.Prepare("relative-output", source, manifestPath), "must be absolute");
            await Reject(() => CorpusCommand.Prepare(Path.Combine(root, "relative-source"), "relative", manifestPath), "must be absolute");
            File.WriteAllText(manifestPath, manifest.Replace(CorpusCommand.Hash(png), new string('0', 64), StringComparison.Ordinal));
            await Reject(() => CorpusCommand.Prepare(Path.Combine(root, "wrong-source"), source, manifestPath), "Source hash mismatch");
            File.WriteAllText(manifestPath, manifest.Replace(CorpusCommand.Hash(ppm), new string('0', 64), StringComparison.Ordinal));
            await Reject(() => CorpusCommand.Prepare(Path.Combine(root, "wrong-pixels"), source, manifestPath), "Decoded pixel hash mismatch");
            File.WriteAllText(manifestPath, manifest.Replace("synthetic.png", "../synthetic.png", StringComparison.Ordinal));
            await Reject(() => CorpusCommand.Prepare(Path.Combine(root, "unsafe-name"), source, manifestPath), "simple filenames");
        }
        finally
        {
            DeleteFixture(root, temporaryRoot);
        }
    }

    private static void DeleteFixture(string root, string temporaryRoot)
    {
        if (!Path.GetFullPath(root).StartsWith(temporaryRoot, StringComparison.Ordinal) ||
            !Path.GetFileName(root).StartsWith("DotJpegli-corpus-", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to remove an unexpected corpus fixture directory.");
        }

        Directory.Delete(root, true);
    }

    private static async Task Reject(Func<Task> action, string message)
    {
        try
        {
            await action();
        }
        catch (InvalidDataException exception) when (exception.Message.Contains(message, StringComparison.Ordinal)) { return; }
        throw new InvalidOperationException($"Corpus preparation accepted invalid input: {message}.");
    }
}
