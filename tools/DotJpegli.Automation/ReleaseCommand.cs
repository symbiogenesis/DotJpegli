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

    internal static async Task Run(Arguments args)
    {
        if (args.Has("self-test"))
        {
            ReleaseTests.Run();
            Console.WriteLine("Release verification self-test passed (package contents, provenance, release directory and artifact reproducibility).");
        }
        else if (args.Has("reproducibility"))
        {
            await VerifyReproducibilityAsync(args.Optional("output-root") ??
                Path.Combine(Path.GetTempPath(), $"DotJpegli-repro-{Guid.NewGuid():N}"));
        }
        else if (args.Has("first-artifacts"))
        {
            CompareArtifacts(args.Get("first-artifacts"), args.Get("second-artifacts"), args.Get("hashes"));
        }
        else if (args.Has("release-directory"))
        {
            VerifyReleaseDirectory(args.Get("release-directory"), args.Get("version"), args.Get("revision"), args.Optional("summary"));
        }
        else
        {
            string version = args.Get("version"), revision = args.Get("revision");
            VerifyPackage(args.Get("package"), version, revision);
            Console.WriteLine($"Verified DotJpegli {version} from {revision} (all three targets, licenses, readme, no runtime dependencies).");
        }
    }

    internal static void VerifyReleaseDirectory(string releaseDirectory, string expectedVersion, string expectedRevision, string? summaryPath = null)
    {
        FileInfo[] packages = new DirectoryInfo(releaseDirectory).GetFiles("*.nupkg");
        Require(packages.Length == 1, "Expected exactly one NuGet package.");
        VerifyPackage(packages[0].FullName, expectedVersion, expectedRevision);
        Console.WriteLine($"Verified DotJpegli {expectedVersion} from {expectedRevision} (all three targets, licenses, readme, no runtime dependencies).");
        string[] expectedNames =
        [
            packages[0].Name,
            .. CliReleaseCommand.SupportedRids.SelectMany(rid => CliReleaseCommand.GetExpectedReleaseAssetSet(expectedVersion, rid)),
        ];
        FileInfo[] files = new DirectoryInfo(releaseDirectory).GetFiles().OrderBy(file => file.Name, StringComparer.Ordinal).ToArray();
        Require(files.Select(file => file.Name).SequenceEqual(expectedNames.OrderBy(name => name, StringComparer.Ordinal), StringComparer.Ordinal),
            "CI release files do not match the requested version and complete artifact set.");
        string[] manifest = files.Select(file => $"{HashFile(file.FullName).ToLowerInvariant()}  {file.Name}").ToArray();
        File.WriteAllText(Path.Combine(releaseDirectory, "SHA256SUMS"), string.Join('\n', manifest) + "\n");
        string? summary = summaryPath ?? Environment.GetEnvironmentVariable("GITHUB_STEP_SUMMARY");
        if (!string.IsNullOrEmpty(summary))
        {
            File.AppendAllText(summary, $"Release version: {expectedVersion}\n\nRevision: {expectedRevision}\n\n{string.Join('\n', manifest)}\n");
        }
    }

    internal static async Task VerifyReproducibilityAsync(string outputRoot)
    {
        Require(Path.IsPathFullyQualified(outputRoot), "OutputRoot must be an absolute path.");
        outputRoot = Path.GetFullPath(outputRoot);
        Require(!Path.Exists(outputRoot), "OutputRoot must not already exist; each comparison needs fresh outputs.");
        string repository = FindRepositoryRoot();
        string revision = (await CliReleaseCommand.RunProcessAsync("git", ["-C", repository, "rev-parse", "HEAD"], expectFailure: false)).Trim();
        _ = Directory.CreateDirectory(outputRoot);
        List<string> worktrees = [];
        List<string> outputs = [];
        try
        {
            foreach (string lane in new[] { "first", "different-depth/second" })
            {
                string checkout = Path.Combine(outputRoot, lane, "checkout");
                string artifacts = Path.Combine(outputRoot, lane, "artifacts");
                // The second lane deliberately exercises a deeper checkout. Retained
                // evidence paths can exceed Win32's legacy limit beneath that prefix.
                _ = await CliReleaseCommand.RunProcessAsync("git", ["-c", "core.longpaths=true", "-C", repository, "worktree", "add", "--detach", checkout, revision], expectFailure: false);
                worktrees.Add(checkout);
                _ = Directory.CreateDirectory(artifacts);
                _ = await CliReleaseCommand.RunProcessAsync("dotnet", ["restore", "src/DotJpegli/DotJpegli.csproj", "--locked-mode"], expectFailure: false, workingDirectory: checkout);
                _ = await CliReleaseCommand.RunProcessAsync("dotnet", ["pack", "src/DotJpegli/DotJpegli.csproj", "-c", "Release", "--no-restore", "--output", artifacts, "-p:ContinuousIntegrationBuild=true", "-p:RepositoryBranch=refs/heads/main"], expectFailure: false, workingDirectory: checkout);
                foreach (string framework in Frameworks)
                {
                    File.Copy(Path.Combine(checkout, "src/DotJpegli/bin/Release", framework, "DotJpegli.dll"), Path.Combine(artifacts, $"DotJpegli.{framework}.dll"));
                }

                outputs.Add(artifacts);
            }

            CompareArtifacts(outputs[0], outputs[1], Path.Combine(outputRoot, "hashes.json"));
            Console.WriteLine($"Reproducible revision: {revision}");
            Console.WriteLine($"Evidence: {outputRoot}");
        }
        finally
        {
            foreach (string checkout in worktrees)
            {
                await RemoveWorktreeAsync(repository, outputRoot, checkout);
            }
        }
    }

    private static async Task RemoveWorktreeAsync(string repository, string outputRoot, string checkout)
    {
        string resolved = Path.GetFullPath(checkout);
        string relative = Path.GetRelativePath(outputRoot, resolved);
        if (Path.IsPathRooted(relative) || relative is "." or ".." ||
            relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to remove a checkout outside OutputRoot.");
        }

        _ = await CliReleaseCommand.RunProcessAsync("git", ["-c", "core.longpaths=true", "-C", repository, "worktree", "remove", "--force", resolved], expectFailure: false);
    }

    internal static string FindRepositoryRoot()
    {
        for (string? directory = AppContext.BaseDirectory; !string.IsNullOrEmpty(directory); directory = Path.GetDirectoryName(directory))
        {
            if (File.Exists(Path.Combine(directory, "DotJpegli.slnx")))
            {
                return directory;
            }
        }

        return Directory.GetCurrentDirectory();
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
