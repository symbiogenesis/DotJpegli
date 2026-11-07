namespace DotJpegli.Automation;

using System.IO.Compression;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

internal static class ReleaseTests
{
    private const string Version = "1.2.3";
    private const string Revision = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
    private static readonly string[] Entries = ["README.md", "LICENSE", "THIRD-PARTY-NOTICES.md", "lib/netstandard2.1/DotJpegli.dll", "lib/net10.0/DotJpegli.dll", "lib/net11.0/DotJpegli.dll"];

    internal static void Run()
    {
        string temporaryRoot = Path.GetFullPath(Path.GetTempPath());
        string root = Path.Combine(temporaryRoot, "DotJpegli-release-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(root);
        try
        {
            string path = Path.Combine(root, $"DotJpegli.{Version}.nupkg");
            WritePackage(path);
            Verify(path);
            Reject(() => ReleaseCommand.VerifyPackage(path, "9.9.9", Revision), "filename");
            Reject(() => ReleaseCommand.VerifyPackage(path, Version, Revision.ToUpperInvariant()), "lowercase");
            foreach (string name in Entries.Prepend("DotJpegli.nuspec"))
            {
                WritePackage(path, omitted: name);
                Reject(() => Verify(path), "package entry");
                WritePackage(path, empty: name);
                Reject(() => Verify(path), "package entry");
            }

            WritePackage(path, extra: "other.dll");
            Reject(() => Verify(path), "three managed");
            WritePackage(path, extra: "LICENSE");
            Reject(() => Verify(path), "duplicate package entry");
            foreach (string field in new[] { "id", "version", "projectUrl", "repository", "license", "readme" })
            {
                WritePackage(path, change: metadata => metadata.Element(metadata.Name.Namespace + field)!.Remove());
                Reject(() => Verify(path), field is "id" or "version" ? "ID/version" : field == "projectUrl" ? "project URL" : field == "repository" ? "repository metadata" : "license or readme");
            }

            WritePackage(path, change: metadata => metadata.Element(metadata.Name.Namespace + "projectUrl")!.Value = "https://example.invalid/");
            Reject(() => Verify(path), "project URL");
            foreach (string attribute in new[] { "type", "url", "branch", "commit" })
            {
                WritePackage(path, change: metadata => metadata.Element(metadata.Name.Namespace + "repository")!.SetAttributeValue(attribute, "wrong"));
                Reject(() => Verify(path), "repository metadata");
            }

            WritePackage(path, change: metadata => metadata.Add(new XElement(metadata.Name.Namespace + "dependencies", new XElement(metadata.Name.Namespace + "dependency", new XAttribute("id", "RuntimeDependency")))));
            Reject(() => Verify(path), "runtime NuGet dependencies");
            WritePackage(path, malformed: true);
            Reject(() => Verify(path), "malformed XML");
            File.WriteAllText(path, "not a ZIP archive");
            Reject(() => Verify(path), "invalid ZIP");
            WritePackage(path, namespaced: true);
            Verify(path);
            CheckArtifacts(root, path);
        }
        finally
        {
            DeleteFixture(root, temporaryRoot);
        }
    }

    private static void DeleteFixture(string root, string temporaryRoot)
    {
        if (!Path.GetFullPath(root).StartsWith(temporaryRoot, StringComparison.Ordinal) ||
            !Path.GetFileName(root).StartsWith("DotJpegli-release-", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Refusing to remove an unexpected release fixture directory.");
        }

        Directory.Delete(root, true);
    }

    private static void CheckArtifacts(string root, string package)
    {
        string first = Path.Combine(root, "first"), second = Path.Combine(root, "second"), hashes = Path.Combine(root, "hashes.json");
        _ = Directory.CreateDirectory(first);
        _ = Directory.CreateDirectory(second);
        foreach (string directory in new[] { first, second })
        {
            File.Copy(package, Path.Combine(directory, Path.GetFileName(package)));
            foreach (string framework in new[] { "netstandard2.1", "net10.0", "net11.0" })
            {
                File.WriteAllText(Path.Combine(directory, $"DotJpegli.{framework}.dll"), framework);
            }
        }

        ReleaseCommand.CompareArtifacts(first, second, hashes);
        using (var report = JsonDocument.Parse(File.ReadAllText(hashes)))
        {
            if (report.RootElement.GetArrayLength() != 4 || report.RootElement[0].GetProperty("SHA256").GetString()?.Length != 64)
            {
                throw new InvalidOperationException("Reproducibility report omitted artifact hashes.");
            }
        }

        File.AppendAllText(Path.Combine(second, "DotJpegli.net10.0.dll"), "changed");
        Reject(() => ReleaseCommand.CompareArtifacts(first, second, hashes), "bytes differ");
        File.Delete(Path.Combine(second, "DotJpegli.net10.0.dll"));
        Reject(() => ReleaseCommand.CompareArtifacts(first, second, hashes), "three target-framework");
        File.Delete(Path.Combine(second, Path.GetFileName(package)));
        Reject(() => ReleaseCommand.CompareArtifacts(first, second, hashes), "one NuGet package");
    }

    private static void Verify(string path) => ReleaseCommand.VerifyPackage(path, Version, Revision);

    private static void Reject(Action action, string label)
    {
        try
        {
            action();
        }
        catch (InvalidDataException exception) when (exception.Message.Contains(label, StringComparison.Ordinal) || label == "invalid ZIP") { return; }
        catch (XmlException) when (label == "malformed XML") { return; }
        throw new InvalidOperationException($"Release verification accepted invalid input: {label}.");
    }

    private static void WritePackage(string path, string? omitted = null, string? empty = null, string? extra = null,
        Action<XElement>? change = null, bool malformed = false, bool namespaced = false)
    {
        XNamespace ns = namespaced ? "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd" : XNamespace.None;
        var metadata = new XElement(ns + "metadata",
            new XElement(ns + "id", "DotJpegli"), new XElement(ns + "version", Version),
            new XElement(ns + "projectUrl", "https://symbiogenesis.github.io/DotJpegli/"),
            new XElement(ns + "repository", new XAttribute("type", "git"), new XAttribute("url", "https://github.com/symbiogenesis/DotJpegli"),
                new XAttribute("branch", "refs/heads/main"), new XAttribute("commit", Revision)),
            new XElement(ns + "license", new XAttribute("type", "expression"), "BSD-3-Clause"), new XElement(ns + "readme", "README.md"));
        change?.Invoke(metadata);
        File.Delete(path);
        using ZipArchive package = ZipFile.Open(path, ZipArchiveMode.Create);
        foreach (string name in Entries.Prepend("DotJpegli.nuspec").Concat(extra is null ? [] : [extra]))
        {
            if (name == omitted)
            {
                continue;
            }
            using var writer = new StreamWriter(package.CreateEntry(name).Open());
            if (name != empty)
            {
                writer.Write(name == "DotJpegli.nuspec" ? malformed ? "<malformed" : new XElement(ns + "package", metadata).ToString() : "synthetic content");
            }
        }
    }
}
