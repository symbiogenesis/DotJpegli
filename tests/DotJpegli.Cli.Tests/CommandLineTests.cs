namespace DotJpegli.Cli.Tests;

using System.Globalization;
using System.Text;
using StbImageSharp;
using Xunit;

public sealed class CommandLineTests
{
    [Theory]
    [InlineData("--quality", "101")]
    [InlineData("--quality", "NaN")]
    [InlineData("--distance", "0")]
    [InlineData("--distance", "Infinity")]
    [InlineData("--subsampling", "411")]
    [InlineData("--threads", "0")]
    [InlineData("--unknown", "1")]
    public void InvalidOptionsFailBeforeOpeningFiles(string option, string value)
    {
        using var output = new StringWriter(CultureInfo.InvariantCulture);
        using var error = new StringWriter(CultureInfo.InvariantCulture);
        int exitCode = CliApplication.Run(["encode", "missing.png", "output.jpg", option, value], output, error);
        Assert.NotEqual(0, exitCode);
        Assert.NotEmpty(error.ToString());
        Assert.DoesNotContain("Could not find", error.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("--help")]
    [InlineData("--version")]
    public void HelpAndVersionSucceed(string option)
    {
        using var output = new StringWriter(CultureInfo.InvariantCulture);
        using var error = new StringWriter(CultureInfo.InvariantCulture);
        Assert.Equal(0, CliApplication.Run([option], output, error));
        Assert.NotEmpty(output.ToString());
        Assert.Empty(error.ToString());
    }

    [Fact]
    public void EncodeProducesDecodableJpegAndRequiresExplicitOverwrite()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"dotjpegli-cli-{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(directory);
        try
        {
            string input = Path.Combine(directory, "source.ppm");
            string destination = Path.Combine(directory, "output.jpg");
            File.WriteAllBytes(input, [.. Encoding.ASCII.GetBytes("P6\n2 1\n255\n"), 255, 0, 0, 0, 255, 0]);
            using var output = new StringWriter(CultureInfo.InvariantCulture);
            using var error = new StringWriter(CultureInfo.InvariantCulture);
            Assert.True(CliApplication.Run(["encode", input, destination, "--progressive", "--subsampling", "444", "--threads", "2"], output, error) == 0, error.ToString());
            byte[] original = File.ReadAllBytes(destination);
            var decoded = ImageResult.FromMemory(original, ColorComponents.RedGreenBlue);
            Assert.Equal(2, decoded.Width);
            Assert.Equal(1, decoded.Height);
            Assert.Equal(6, decoded.Data.Length);
            Assert.NotEqual(0, CliApplication.Run(["encode", input, destination], output, error));
            Assert.Equal(original, File.ReadAllBytes(destination));
            Assert.Equal(0, CliApplication.Run(["encode", input, destination, "--quality", "20", "--overwrite"], output, error));
            Assert.NotEqual(original, File.ReadAllBytes(destination));
            byte[] source = File.ReadAllBytes(input);
            Assert.NotEqual(0, CliApplication.Run(["encode", input, input, "--overwrite"], output, error));
            Assert.Equal(source, File.ReadAllBytes(input));
            Assert.Empty(Directory.GetFiles(directory, "*.tmp"));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void DirectoryAliasCannotReplaceTheInputFile()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"dotjpegli-cli-{Guid.NewGuid():N}");
        string actual = Path.Combine(directory, "actual");
        string nested = Path.Combine(actual, "nested");
        string alias = Path.Combine(directory, "alias");
        _ = Directory.CreateDirectory(nested);
        try
        {
            try
            {
                _ = Directory.CreateSymbolicLink(alias, actual);
            }
            catch (Exception exception) when (exception is UnauthorizedAccessException or PlatformNotSupportedException or IOException)
            {
                Assert.Skip($"Directory symlinks are unavailable: {exception.Message}");
            }
            string input = Path.Combine(nested, "source.ppm");
            string aliasInput = Path.Combine(alias, "nested", "source.ppm");
            byte[] pixels = [.. Encoding.ASCII.GetBytes("P6\n1 1\n255\n"), 255, 0, 0];
            File.WriteAllBytes(input, pixels);
            using var output = new StringWriter(CultureInfo.InvariantCulture);
            using var error = new StringWriter(CultureInfo.InvariantCulture);
            Assert.NotEqual(0, CliApplication.Run(["encode", input, aliasInput, "--overwrite"], output, error));
            Assert.Equal(pixels, File.ReadAllBytes(input));
            Assert.NotEqual(0, CliApplication.Run(["encode", aliasInput, input, "--overwrite"], output, error));
            Assert.Equal(pixels, File.ReadAllBytes(input));
            Assert.Contains("Input and output must be different", error.ToString(), StringComparison.Ordinal);
            Assert.Empty(Directory.GetFiles(nested, "*.tmp"));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void InvalidImagePreservesExistingOutput()
    {
        string directory = Path.Combine(Path.GetTempPath(), $"dotjpegli-cli-{Guid.NewGuid():N}");
        _ = Directory.CreateDirectory(directory);
        try
        {
            string input = Path.Combine(directory, "broken.png");
            string destination = Path.Combine(directory, "existing.jpg");
            File.WriteAllText(input, "not an image");
            File.WriteAllText(destination, "preserve me");
            using var output = new StringWriter(CultureInfo.InvariantCulture);
            using var error = new StringWriter(CultureInfo.InvariantCulture);
            Assert.NotEqual(0, CliApplication.Run(["encode", input, destination, "--overwrite"], output, error));
            Assert.Equal("preserve me", File.ReadAllText(destination));
            Assert.NotEmpty(error.ToString());
            Assert.Empty(Directory.GetFiles(directory, "*.tmp"));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public void ReleaseAssetsBinaryHeadersJpegFramesAndInstallerPathChecksPass() =>
        Automation.CliReleaseTests.Run();
}
