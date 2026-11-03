namespace DotJpegli.Tests;

using System.ComponentModel;
using DotJpegli.Parity;
using Xunit;

public sealed class QuantizationSearchNativeTests
{
    [Fact]
    public async Task NativeCommandDrainsBothPipesBeforeReturning()
    {
        (string executable, string[] args) = Shell(
            "for /l %i in (1,1,3000) do @(echo stdout-%i & echo stderr-%i 1>&2)",
            "i=1; while [ \"$i\" -le 3000 ]; do echo stdout-$i; echo stderr-$i >&2; i=$((i+1)); done");
        string output = await QuantizationTableSearchCommand.NativeAsync(executable, Path.GetTempPath(),
            TimeSpan.FromSeconds(30), args);
        Assert.Contains("stdout-1", output, StringComparison.Ordinal);
        Assert.Contains("stdout-3000", output, StringComparison.Ordinal);
        Assert.DoesNotContain("stderr-", output, StringComparison.Ordinal);
    }

    [Fact]
    public async Task NativeFailureIncludesExitCodeAndStderr()
    {
        (string executable, string[] args) = Shell("echo known-decoder-error 1>&2 & exit /b 7",
            "echo known-decoder-error >&2; exit 7");
        InvalidDataException error = await Assert.ThrowsAsync<InvalidDataException>(() =>
            QuantizationTableSearchCommand.NativeAsync(executable, Path.GetTempPath(), TimeSpan.FromSeconds(30), args));
        Assert.Contains("exit 7", error.Message, StringComparison.Ordinal);
        Assert.Contains("known-decoder-error", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task NativeTimeoutKillsRunningCommandWithoutWaitingForPipeEof()
    {
        (string executable, string[] args) = Shell("for /l %i in (1,0,2) do @rem spin", "while :; do :; done");
        _ = await Assert.ThrowsAsync<TimeoutException>(() => QuantizationTableSearchCommand.NativeAsync(executable,
            Path.GetTempPath(), TimeSpan.FromMilliseconds(150), args));
    }

    [Fact]
    public async Task NativeDeadlineAlsoCoversPipesInheritedByExitedParentsChild()
    {
        // The finite child retains inherited pipe handles after its parent
        // exits. Closing the reader at the deadline does not wait for child EOF.
        (string executable, string[] args) = Shell(
            "start \"\" /b \"%ComSpec%\" /d /c \"ping -n 3 127.0.0.1 >nul\" & exit /b 0",
            "(sleep 2) & exit 0");
        _ = await Assert.ThrowsAsync<TimeoutException>(() => QuantizationTableSearchCommand.NativeAsync(executable,
            Path.GetTempPath(), TimeSpan.FromMilliseconds(150), args));
    }

    [Fact]
    public void WrongNativeRevisionLeavesOutputUncreatedAndRemovesScratch() =>
        WithInput((input, output, unusedScratch) =>
        {
            string? decoderScratch = null;
            _ = Assert.Throws<InvalidDataException>(() => QuantizationTableSearchCommand.Run(
                Arguments(input, output, typeof(QuantizationTableSearchCommand).Assembly.Location),
                (executable, workingDirectory, args) =>
                {
                    Assert.Equal(typeof(QuantizationTableSearchCommand).Assembly.Location, executable);
                    Assert.Equal(["version"], args);
                    Assert.True(Directory.Exists(workingDirectory));
                    decoderScratch = workingDirectory;
                    return "unexpected-upstream-revision\n";
                }));
            Assert.False(Directory.Exists(output));
            Assert.NotNull(decoderScratch);
            Assert.False(Directory.Exists(decoderScratch));
        });

    [Fact]
    public void NativeStartFailureLeavesOutputUncreated() =>
        WithInput((input, output, scratch) =>
        {
            string invalidExecutable = Path.Combine(scratch, "invalid-native.exe");
            File.WriteAllText(invalidExecutable, "This is not an executable.");
            try
            {
                _ = Assert.Throws<Win32Exception>(() => QuantizationTableSearchCommand.Run(
                    Arguments(input, output, invalidExecutable)));
                Assert.False(Directory.Exists(output));
            }
            finally
            {
                File.Delete(invalidExecutable);
            }
        });

    private static string[] Arguments(string input, string output, string native) =>
        ["--input", input, "--output", output, "--native", native, "--distance", "1", "--sampling", "420"];

    private static (string Executable, string[] Args) Shell(string windowsScript, string unixScript)
        => OperatingSystem.IsWindows()
            ? (Environment.GetEnvironmentVariable("ComSpec") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe"),
                ["/d", "/c", windowsScript])
            : ("/bin/sh", ["-c", unixScript]);

    private static void WithInput(Action<string, string, string> action)
    {
        string scratch = Path.Combine(Path.GetTempPath(), "DotJpegli-quantization-native-test-" + Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(scratch);
        string input = Path.Combine(scratch, "input.ppm"), output = Path.Combine(scratch, "result");
        try
        {
            File.WriteAllBytes(input, "P6\n1 1\n255\n"u8.ToArray().Concat(new byte[] { 31, 113, 207 }).ToArray());
            action(input, output, scratch);
        }
        finally
        {
            File.Delete(input);
            Directory.Delete(scratch);
        }
    }
}
