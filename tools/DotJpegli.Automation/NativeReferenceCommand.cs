namespace DotJpegli.Automation;

using System.Security.Cryptography;
using System.Text.Json;

internal static class NativeReferenceCommand
{
    private const string Revision = "031a0077f5799a6041004267fc12b956c1f52a20";

    public static void Run(Arguments args)
    {
        if (args.Has("self-test"))
        {
            SelfTest();
            return;
        }

        Verify(args.Get("build-directory"));
    }

    internal static void Verify(string directory)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "provenance.json")));
        JsonElement metadata = document.RootElement;
        if (metadata.ValueKind != JsonValueKind.Object ||
            !metadata.TryGetProperty("sourceUnmodified", out JsonElement unmodified) || unmodified.ValueKind != JsonValueKind.True ||
            !metadata.TryGetProperty("revision", out JsonElement revision) || revision.ValueKind != JsonValueKind.String ||
            !string.Equals(revision.GetString(), Revision, StringComparison.Ordinal))
        {
            throw new InvalidDataException("Native reference provenance does not match the unmodified pinned source.");
        }

        string name = OperatingSystem.IsWindows() ? "jpegli-parity-native.exe" : "jpegli-parity-native";
        string executable = Path.Combine(directory, name);
        if (!File.Exists(executable))
        {
            executable = Path.Combine(directory, "Release", name);
        }

        if (!File.Exists(executable))
        {
            throw new InvalidDataException("Native reference executable is missing.");
        }

        using FileStream stream = File.OpenRead(executable);
        string hash = Convert.ToHexStringLower(SHA256.HashData(stream));
        if (!metadata.TryGetProperty("executableSha256", out JsonElement expected) || expected.ValueKind != JsonValueKind.String ||
            !string.Equals(hash, expected.GetString(), StringComparison.Ordinal))
        {
            throw new InvalidDataException("Native reference executable hash mismatch.");
        }
    }

    private static void SelfTest()
    {
        string directory = Path.Combine(Path.GetTempPath(), "DotJpegli-native-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(directory);
        try
        {
            string executable = Path.Combine(directory, OperatingSystem.IsWindows() ? "jpegli-parity-native.exe" : "jpegli-parity-native");
            string metadata = Path.Combine(directory, "provenance.json");
            File.WriteAllText(executable, "synthetic native binary");
            string hash = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(executable)));
            WriteMetadata(Revision, true, hash);
            Verify(directory);
            File.AppendAllText(executable, "changed");
            Reject("Native reference executable hash mismatch.");
            File.WriteAllText(executable, "synthetic native binary");
            WriteMetadata("wrong-revision", true, hash);
            Reject("Native reference provenance does not match the unmodified pinned source.");
            WriteMetadata(Revision, false, hash);
            Reject("Native reference provenance does not match the unmodified pinned source.");
            WriteMetadata(Revision, "true", hash);
            Reject("Native reference provenance does not match the unmodified pinned source.");
            WriteMetadata(Revision, true, hash.ToUpperInvariant());
            Reject("Native reference executable hash mismatch.");
            WriteMetadata(Revision, true, hash);
            string release = Path.Combine(directory, "Release");
            _ = Directory.CreateDirectory(release);
            string moved = Path.Combine(release, Path.GetFileName(executable));
            File.Move(executable, moved);
            Verify(directory);
            File.Delete(moved);
            Reject("Native reference executable is missing.");
            Console.WriteLine("Native reference self-test passed (provenance, binary hashes and configuration paths).");

            void WriteMetadata(string revision, object unmodified, string expectedHash) => File.WriteAllText(metadata,
                JsonSerializer.Serialize(new { revision, sourceUnmodified = unmodified, executableSha256 = expectedHash }));

            void Reject(string message)
            {
                try
                {
                    Verify(directory);
                }
                catch (InvalidDataException exception) when (string.Equals(exception.Message, message, StringComparison.Ordinal))
                {
                    return;
                }

                throw new InvalidOperationException($"Native reference self-test did not reject: {message}");
            }
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
