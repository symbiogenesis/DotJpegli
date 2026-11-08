namespace DotJpegli.Automation;

using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

internal static class CliReleaseCommand
{
    internal static readonly string[] SupportedRids = ["win-x64", "win-arm64", "linux-x64", "linux-arm64", "osx-arm64"];
    internal static readonly string[] PortableNotices = ["LICENSE", "THIRD-PARTY-NOTICES.md", "LICENSE.dotnet.txt", "THIRD-PARTY-NOTICES.dotnet.txt", "README.md"];
    private static readonly string[] ManagedFiles = ["dotjpegli.dll", "dotjpegli.deps.json", "dotjpegli.runtimeconfig.json", "DotJpegli.Cli.dll", "DotJpegli.Cli.deps.json", "DotJpegli.Cli.runtimeconfig.json"];

    internal static async Task Run(Arguments args)
    {
        if (args.Has("self-test"))
        {
            CliReleaseTests.Run();
            Console.WriteLine("CLI release, binary header, JPEG frame and installer PATH self-test passed.");
        }
        else if (args.Has("assert-path-restored"))
        {
            AssertUserPathRestored(args.Optional("original") ?? string.Empty, args.Optional("current") ?? string.Empty, args.Get("installed-dir"));
        }
        else if (args.Has("artifact-root"))
        {
            await VerifyPortableReleaseAsync(args.Get("artifact-root"), args.Get("rid"));
        }
        else
        {
            await SmokeTestAsync(
                args.Get("smoke"),
                args.Optional("output-root") ?? Path.Combine(Path.GetTempPath(), $"DotJpegli-cli-smoke-{Guid.NewGuid():N}"),
                args.Optional("version"),
                args.Optional("rid"),
                args.Optional("portable-binary"),
                args.Optional("installed-binary"),
                args.Optional("doc-dir"),
                args.Values("extra-file"));
        }
    }

    internal static string[] GetExpectedReleaseAssetSet(string version, string targetRid)
    {
        string stem = $"DotJpegli.Cli-{version}-{targetRid}";
        if (targetRid is "win-x64" or "win-arm64")
        {
            return [$"{stem}.exe", $"{stem}.msi", $"{stem}.zip", $"{stem}-symbols.zip"];
        }

        if (targetRid is "linux-x64" or "linux-arm64")
        {
            return [$"{stem}.AppImage", $"{stem}.deb", $"{stem}.flatpak", $"{stem}.rpm", $"{stem}.tar.gz", $"{stem}-symbols.tar.gz"];
        }

        if (targetRid == "osx-arm64")
        {
            return [$"{stem}.pkg", $"{stem}.tar.gz", $"{stem}-symbols.tar.gz"];
        }

        throw new InvalidDataException($"Unsupported CLI RuntimeIdentifier: {targetRid}");
    }

    internal static void AssertUserPathRestored(string original, string current, string installedDirectory)
    {
        string[] originalEntries = original.Split(';', StringSplitOptions.RemoveEmptyEntries);
        string[] currentEntries = current.Split(';', StringSplitOptions.RemoveEmptyEntries);
        string trimmedInstalled = installedDirectory.TrimEnd('\\');
        Require(!currentEntries.Any(entry => string.Equals(entry.TrimEnd('\\'), trimmedInstalled, StringComparison.OrdinalIgnoreCase)),
            "Uninstall left the CLI directory in the user PATH.");
        if (!originalEntries.SequenceEqual(currentEntries, StringComparer.Ordinal))
        {
            throw new InvalidDataException($"Uninstall changed the original user PATH entries: {JsonSerializer.Serialize(new { original, current })}");
        }
    }

    internal static void VerifyBinaryHeader(ReadOnlySpan<byte> binary, string rid, bool isAppImage = false)
    {
        if (isAppImage)
        {
            ushort expectedElfMachine = rid == "linux-arm64" ? (ushort)0xb7 : (ushort)0x3e;
            Require(binary.Length >= 64 && binary[8] == 0x41 && binary[9] == 0x49 && binary[10] == 0x02 &&
                BinaryPrimitives.ReadUInt16LittleEndian(binary.Slice(18, 2)) == expectedElfMachine,
                "The AppImage does not have a valid Type 2 header for the target architecture.");
            return;
        }

        if (rid is "win-x64" or "win-arm64")
        {
            ushort expectedPeMachine = rid == "win-arm64" ? (ushort)0xaa64 : (ushort)0x8664;
            Require(binary.Length >= 0x40 && binary[0] == 'M' && binary[1] == 'Z',
                "The release executable must be a native 64-bit PE with no CLR header.");
            int peOffset = BinaryPrimitives.ReadInt32LittleEndian(binary.Slice(0x3c, 4));
            Require(peOffset >= 0 && peOffset + 24 + 112 + (15 * 8) <= binary.Length &&
                BinaryPrimitives.ReadUInt32LittleEndian(binary.Slice(peOffset, 4)) == 0x4550 &&
                BinaryPrimitives.ReadUInt16LittleEndian(binary.Slice(peOffset + 4, 2)) == expectedPeMachine &&
                BinaryPrimitives.ReadUInt16LittleEndian(binary.Slice(peOffset + 24, 2)) == 0x20b &&
                BinaryPrimitives.ReadUInt32LittleEndian(binary.Slice(peOffset + 24 + 112 + (14 * 8), 4)) == 0,
                "The release executable must be a native 64-bit PE for the target architecture with no CLR header.");
            return;
        }

        if (rid is "linux-x64" or "linux-arm64")
        {
            ushort expectedElfMachine = rid == "linux-arm64" ? (ushort)0xb7 : (ushort)0x3e;
            Require(binary.Length >= 64 && binary[0] == 0x7f && binary[1] == 'E' && binary[2] == 'L' && binary[3] == 'F' &&
                binary[4] == 2 && binary[5] == 1 &&
                BinaryPrimitives.ReadUInt16LittleEndian(binary.Slice(16, 2)) is 2 or 3 &&
                BinaryPrimitives.ReadUInt16LittleEndian(binary.Slice(18, 2)) == expectedElfMachine,
                "The release executable must be a native 64-bit little-endian ELF executable for the target architecture.");
            return;
        }

        if (rid == "osx-arm64")
        {
            Require(binary.Length >= 32 &&
                BinaryPrimitives.ReadUInt32LittleEndian(binary[..4]) == 0xfeedfacfu &&
                BinaryPrimitives.ReadUInt32LittleEndian(binary.Slice(4, 4)) == 0x0100000cu &&
                BinaryPrimitives.ReadUInt32LittleEndian(binary.Slice(12, 4)) == 2u,
                "The release executable must be a native 64-bit ARM64 Mach-O executable.");
            return;
        }

        throw new InvalidDataException($"Unsupported CLI RuntimeIdentifier: {rid}");
    }

    internal static void AssertJpegFrame(ReadOnlySpan<byte> jpeg, byte expectedFrameMarker)
    {
        Require(jpeg.Length >= 4 && jpeg[0] == 0xff && jpeg[1] == 0xd8 && jpeg[^2] == 0xff && jpeg[^1] == 0xd9,
            "Output is not a complete JPEG.");
        int offset = 2;
        while (offset + 3 < jpeg.Length)
        {
            Require(jpeg[offset] == 0xff, "Invalid JPEG marker prefix.");
            byte marker = jpeg[offset + 1];
            if (marker == 0xda)
            {
                break;
            }

            int length = BinaryPrimitives.ReadUInt16BigEndian(jpeg.Slice(offset + 2, 2));
            Require(length >= 2 && offset + 2 + length <= jpeg.Length, "Invalid JPEG segment length.");
            if (marker is 0xc0 or 0xc2)
            {
                Require(marker == expectedFrameMarker && offset + 8 < jpeg.Length &&
                    BinaryPrimitives.ReadUInt16BigEndian(jpeg.Slice(offset + 5, 2)) == 3 &&
                    BinaryPrimitives.ReadUInt16BigEndian(jpeg.Slice(offset + 7, 2)) == 5,
                    "Unexpected JPEG frame mode or dimensions.");
                return;
            }

            offset += 2 + length;
        }

        throw new InvalidDataException("Output has no JPEG frame.");
    }

    internal static async Task VerifyPortableReleaseAsync(string artifactRoot, string rid)
    {
        artifactRoot = Path.GetFullPath(artifactRoot);
        string release = Path.Combine(artifactRoot, "release");
        FileInfo[] assets = new DirectoryInfo(release).GetFiles().OrderBy(file => file.Name, StringComparer.Ordinal).ToArray();
        Require(assets.Length > 0, "No CLI release assets were found.");
        Match versionMatch = Regex.Match(assets[0].Name, @"\ADotJpegli\.Cli-([0-9]+\.[0-9]+\.[0-9]+)-" + Regex.Escape(rid) + @"(?:-symbols)?\.", RegexOptions.CultureInvariant);
        Require(versionMatch.Success, $"Unexpected release asset filename: {assets[0].Name}");
        string version = versionMatch.Groups[1].Value;
        string assetName = $"DotJpegli.Cli-{version}-{rid}";
        string[] expectedNames = GetExpectedReleaseAssetSet(version, rid).OrderBy(name => name, StringComparer.Ordinal).ToArray();
        Require(assets.Select(asset => asset.Name).SequenceEqual(expectedNames, StringComparer.Ordinal),
            "CLI release files do not match the expected version, architecture and complete artifact set.");

        string extracted = Path.Combine(artifactRoot, "extracted");
        string symbols = Path.Combine(artifactRoot, "extracted-symbols");
        _ = Directory.CreateDirectory(extracted);
        _ = Directory.CreateDirectory(symbols);
        bool isWindows = rid.StartsWith("win-", StringComparison.Ordinal);
        if (isWindows)
        {
            ZipFile.ExtractToDirectory(Path.Combine(release, $"{assetName}.zip"), extracted);
            ZipFile.ExtractToDirectory(Path.Combine(release, $"{assetName}-symbols.zip"), symbols);
        }
        else
        {
            _ = await RunProcessAsync("tar", ["-xzf", Path.Combine(release, $"{assetName}.tar.gz"), "-C", extracted], expectFailure: false);
            _ = await RunProcessAsync("tar", ["-xzf", Path.Combine(release, $"{assetName}-symbols.tar.gz"), "-C", symbols], expectFailure: false);
        }

        string executableName = isWindows ? "dotjpegli.exe" : "dotjpegli";
        string portableExecutable = Path.Combine(extracted, executableName);
        Require(File.Exists(portableExecutable), $"Portable archive is missing {executableName}.");
        foreach (string notice in PortableNotices)
        {
            Require(File.Exists(Path.Combine(extracted, notice)), $"Portable archive is missing {notice}.");
        }

        if (isWindows)
        {
            Require(File.Exists(Path.Combine(symbols, "dotjpegli.pdb")), "Symbols archive is missing the NativeAOT PDB.");
            Require(HashFile(Path.Combine(release, $"{assetName}.exe")) == HashFile(portableExecutable),
                "The standalone executable differs from the portable executable.");
        }
        else if (rid == "osx-arm64")
        {
            Require(File.Exists(Path.Combine(symbols, "dotjpegli.dSYM/Contents/Resources/DWARF/dotjpegli")),
                "Symbols archive is missing the Mach-O DWARF bundle.");
        }
        else
        {
            string dbgPath = Path.Combine(symbols, "dotjpegli.dbg");
            Require(File.Exists(dbgPath), "Symbols archive is missing the NativeAOT ELF debug file.");
            VerifyBinaryHeader(File.ReadAllBytes(dbgPath), rid);
            VerifyBinaryHeader(File.ReadAllBytes(Path.Combine(release, $"{assetName}.AppImage")), rid, isAppImage: true);
        }

        await SmokeTestAsync(portableExecutable, Path.Combine(artifactRoot, "portable-smoke"), version, rid);
        Console.WriteLine(version);
    }

    internal static async Task SmokeTestAsync(
        string executablePath,
        string outputRoot,
        string? expectedVersion,
        string? rid = null,
        string? portableBinary = null,
        string? installedBinary = null,
        string? docDir = null,
        IReadOnlyList<string>? extraFiles = null)
    {
        executablePath = Path.GetFullPath(executablePath);
        Require(File.Exists(executablePath), $"Executable does not exist: {executablePath}");
        if (portableBinary is not null)
        {
            string candidate = installedBinary is not null ? Path.GetFullPath(installedBinary) : executablePath;
            Require(File.Exists(candidate) && HashFile(candidate) == HashFile(Path.GetFullPath(portableBinary)),
                "The installed or packaged executable differs from the portable executable.");
        }

        if (docDir is not null)
        {
            foreach (string notice in PortableNotices)
            {
                Require(File.Exists(Path.Combine(docDir, notice)), $"Package omitted documentation file {notice}.");
            }
        }

        if (extraFiles is not null)
        {
            foreach (string extraFile in extraFiles)
            {
                Require(File.Exists(extraFile), $"Package is missing {extraFile}.");
            }
        }

        bool isAppImage = executablePath.EndsWith(".AppImage", StringComparison.OrdinalIgnoreCase);
        VerifyBinaryHeader(File.ReadAllBytes(executablePath), rid ?? DefaultHostRid(), isAppImage);
        string executableDirectory = Path.GetDirectoryName(executablePath)!;
        foreach (string managedFile in ManagedFiles)
        {
            Require(!File.Exists(Path.Combine(executableDirectory, managedFile)),
                $"The native distribution unexpectedly contains {managedFile}.");
        }

        Require(!Path.Exists(outputRoot), "OutputRoot must not already exist.");
        _ = Directory.CreateDirectory(outputRoot);
        _ = await RunProcessAsync(executablePath, ["--help"], expectFailure: false);
        string versionText = await RunProcessAsync(executablePath, ["--version"], expectFailure: false);
        Require(string.IsNullOrEmpty(expectedVersion) || versionText.Contains(expectedVersion, StringComparison.Ordinal),
            $"CLI version does not contain {expectedVersion}.");

        string inputPath = Path.Combine(outputRoot, "tiny image.ppm");
        byte[] header = Encoding.ASCII.GetBytes("P6\n5 3\n255\n");
        byte[] ppm = new byte[header.Length + 45];
        header.CopyTo(ppm, 0);
        for (int index = 0; index < 45; index++)
        {
            ppm[header.Length + index] = (byte)(index * 37 % 256);
        }

        File.WriteAllBytes(inputPath, ppm);
        string baseline = Path.Combine(outputRoot, "baseline.jpg");
        string progressive = Path.Combine(outputRoot, "progressive.jpg");
        _ = await RunProcessAsync(executablePath, ["encode", inputPath, baseline, "--quality", "85", "--subsampling", "420"], expectFailure: false);
        AssertJpegFrame(File.ReadAllBytes(baseline), 0xc0);
        _ = await RunProcessAsync(executablePath, ["encode", inputPath, progressive, "--distance", "1", "--progressive", "--threads", "2"], expectFailure: false);
        AssertJpegFrame(File.ReadAllBytes(progressive), 0xc2);

        string baselineHash = HashFile(baseline);
        _ = await RunProcessAsync(executablePath, ["encode", inputPath, baseline], expectFailure: true);
        Require(HashFile(baseline) == baselineHash, "The CLI changed an existing destination without --overwrite.");
        _ = await RunProcessAsync(executablePath, ["encode", inputPath, baseline, "--overwrite"], expectFailure: false);
        AssertJpegFrame(File.ReadAllBytes(baseline), 0xc0);

        string inputHash = HashFile(inputPath);
        _ = await RunProcessAsync(executablePath, ["encode", inputPath, inputPath, "--overwrite"], expectFailure: true);
        Require(HashFile(inputPath) == inputHash, "The CLI changed its source image.");

        string invalidOutput = Path.Combine(outputRoot, "invalid.jpg");
        _ = await RunProcessAsync(executablePath, ["encode", inputPath, invalidOutput, "--quality", "101"], expectFailure: true);
        _ = await RunProcessAsync(executablePath, ["encode", Path.Combine(outputRoot, "missing.ppm"), invalidOutput], expectFailure: true);
        string invalidInput = Path.Combine(outputRoot, "invalid.txt");
        File.WriteAllText(invalidInput, "This is not an image.");
        _ = await RunProcessAsync(executablePath, ["encode", invalidInput, invalidOutput], expectFailure: true);
        Require(!File.Exists(invalidOutput), "An invalid invocation left an output file.");
        Console.WriteLine("Native CLI smoke passed: help/version, baseline/progressive, overwrite safety, and invalid inputs.");
    }

    internal static async Task<string> RunProcessAsync(string fileName, IReadOnlyList<string> arguments, bool expectFailure, string? workingDirectory = null)
    {
        var startInfo = new ProcessStartInfo(fileName)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        if (workingDirectory is not null)
        {
            startInfo.WorkingDirectory = workingDirectory;
        }

        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException($"Failed to start {fileName}.");
        Task<string> standardOutput = process.StandardOutput.ReadToEndAsync();
        Task<string> standardError = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        string message = await standardOutput + await standardError;
        if (process.ExitCode == 0 == expectFailure)
        {
            throw new InvalidDataException($"Unexpected exit {process.ExitCode}: {fileName} {string.Join(' ', arguments)}\n{message}");
        }

        return message;
    }

    private static string DefaultHostRid() =>
        OperatingSystem.IsWindows() ? RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "win-arm64" : "win-x64" :
        OperatingSystem.IsMacOS() ? "osx-arm64" :
        RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "linux-arm64" : "linux-x64";

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
}
