namespace DotJpegli.Automation;

using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

internal static class ReleaseCommand
{
    private static readonly string[] Frameworks = ["netstandard2.1", "net10.0", "net11.0"];
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    internal static void Run(Arguments args)
    {
        if (args.Has("self-test"))
        {
            ReleaseTests.Run();
            Console.WriteLine("Release verification self-test passed (package contents, provenance and artifact reproducibility).");
        }
        else if (args.Has("first-artifacts"))
        {
            CompareArtifacts(args.Get("first-artifacts"), args.Get("second-artifacts"), args.Get("hashes"));
        }
        else
        {
            string version = args.Get("version"), revision = args.Get("revision");
            VerifyPackage(args.Get("package"), version, revision);
            Console.WriteLine($"Verified DotJpegli {version} from {revision} (all three targets, licenses, readme, no runtime dependencies).");
        }
    }

    internal static void VerifyPackage(string packagePath, string expectedVersion, string expectedRevision)
    {
        Require(expectedRevision.Length == 40 && expectedRevision.All(char.IsAsciiHexDigitLower),
            "ExpectedRevision must be a full lowercase Git commit ID.");
        Require(Path.GetFileName(packagePath) == $"DotJpegli.{expectedVersion}.nupkg",
            "Package filename must match the requested DotJpegli version exactly.");
        using ZipArchive package = ZipFile.OpenRead(packagePath);
        string[] required = ["DotJpegli.nuspec", "README.md", "LICENSE", "THIRD-PARTY-NOTICES.md", .. Frameworks.Select(framework => $"lib/{framework}/DotJpegli.dll")];
        foreach (string name in required)
        {
            ZipArchiveEntry[] entries = package.Entries.Where(entry => entry.FullName == name).ToArray();
            Require(entries.Length == 1 && entries[0].Length > 0, $"Missing, empty or duplicate package entry: {name}");
        }

        Require(package.Entries.Count(entry => entry.FullName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) == Frameworks.Length,
            "Expected exactly the three managed library assemblies.");
        using Stream stream = package.GetEntry("DotJpegli.nuspec")!.Open();
        using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
        var manifest = XDocument.Load(reader);
        XElement? root = manifest.Root;
        Require(root?.Name.LocalName == "package", "Package manifest must have a package root.");
        XNamespace ns = root!.Name.Namespace;
        XElement? metadata = root.Element(ns + "metadata");
        Require(metadata is not null, "Package manifest must contain metadata.");
        Require(metadata!.Element(ns + "id")?.Value == "DotJpegli" && metadata.Element(ns + "version")?.Value == expectedVersion,
            "Package ID/version does not match the requested release.");
        Require(metadata.Element(ns + "projectUrl")?.Value == "https://symbiogenesis.github.io/DotJpegli/",
            "Package project URL does not match the homepage.");
        XElement? repository = metadata.Element(ns + "repository");
        Require(repository?.Attribute("type")?.Value == "git" &&
            repository?.Attribute("url")?.Value == "https://github.com/symbiogenesis/DotJpegli" &&
            repository?.Attribute("branch")?.Value == "refs/heads/main" &&
            repository?.Attribute("commit")?.Value == expectedRevision,
            "Package repository metadata does not match the tested main revision.");
        XElement? license = metadata.Element(ns + "license");
        Require(license?.Attribute("type")?.Value == "expression" && license?.Value == "BSD-3-Clause" &&
            metadata.Element(ns + "readme")?.Value == "README.md", "Package license or readme metadata is incorrect.");
        Require(!metadata.Descendants().Any(element => element.Name.LocalName == "dependency"),
            "The encoder package must have no runtime NuGet dependencies.");
    }

    internal static void CompareArtifacts(string firstDirectory, string secondDirectory, string hashesPath)
    {
        FileInfo[][] outputs = [Artifacts(firstDirectory), Artifacts(secondDirectory)];
        var hashes = new List<ArtifactHash>();
        for (int index = 0; index < outputs[0].Length; index++)
        {
            FileInfo first = outputs[0][index], second = outputs[1][index];
            Require(first.Name == second.Name, "Artifact filenames differ.");
            string firstHash = HashFile(first.FullName), secondHash = HashFile(second.FullName);
            Require(firstHash == secondHash, $"Artifact bytes differ: {first.Name}.");
            hashes.Add(new(first.Name, firstHash));
        }

        File.WriteAllText(hashesPath, JsonSerializer.Serialize(hashes, JsonOptions) + "\n");
        foreach (ArtifactHash hash in hashes)
        {
            Console.WriteLine($"{hash.File}  {hash.SHA256}");
        }
    }

    private static FileInfo[] Artifacts(string directory)
    {
        FileInfo[] files = new DirectoryInfo(directory).GetFiles().OrderBy(file => file.Name, StringComparer.Ordinal).ToArray();
        Require(files.Count(file => file.Extension.Equals(".nupkg", StringComparison.OrdinalIgnoreCase)) == 1,
            "Expected exactly one NuGet package.");
        Require(files.Length == Frameworks.Length + 1 && Frameworks.All(framework => files.Any(file => file.Name == $"DotJpegli.{framework}.dll")),
            "Expected all three target-framework assemblies and one package from each checkout.");
        return files;
    }

    private static string HashFile(string path)
    {
        using FileStream stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream));
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidDataException(message);
        }
    }

    private sealed record ArtifactHash(string File, string SHA256);
}
