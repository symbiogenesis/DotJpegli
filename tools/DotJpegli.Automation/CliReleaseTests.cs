namespace DotJpegli.Automation;

using System.Buffers.Binary;

internal static class CliReleaseTests
{
    internal static void Run()
    {
        CheckUserPathRestore();
        CheckReleaseAssetMatrix();
        CheckBinaryHeaders();
        CheckJpegFrames();
    }

    internal static void CheckUserPathRestore()
    {
        const string installed = @"C:\Programs\DotJpegli\x64";
        foreach (string restored in new[] { @"C:\Windows;C:\Tools", @"C:\Windows;C:\Tools;", @";C:\Windows;;C:\Tools;" })
        {
            CliReleaseCommand.AssertUserPathRestored(@"C:\Windows;C:\Tools;", restored, installed);
        }

        CliReleaseCommand.AssertUserPathRestored(string.Empty, ";", installed);
        foreach (string invalid in new[] { @"C:\Windows", @"C:\Tools;C:\Windows", @"C:\Windows;C:\Tools;C:\Unexpected", $@"C:\Windows;C:\Tools;{installed}\" })
        {
            Reject(() => CliReleaseCommand.AssertUserPathRestored(@"C:\Windows;C:\Tools;", invalid, installed), "PATH");
        }

        Reject(() => CliReleaseCommand.AssertUserPathRestored(@"C:\Tools;C:\Tools", @"C:\Tools", installed), "PATH");
    }

    internal static void CheckReleaseAssetMatrix()
    {
        if (CliReleaseCommand.GetExpectedReleaseAssetSet("1.2.3", "win-x64").Length != 4 ||
            CliReleaseCommand.GetExpectedReleaseAssetSet("1.2.3", "win-arm64").Length != 4 ||
            CliReleaseCommand.GetExpectedReleaseAssetSet("1.2.3", "linux-x64").Length != 6 ||
            CliReleaseCommand.GetExpectedReleaseAssetSet("1.2.3", "linux-arm64").Length != 6 ||
            CliReleaseCommand.GetExpectedReleaseAssetSet("1.2.3", "osx-arm64").Length != 3)
        {
            throw new InvalidOperationException("Release asset matrix self-test failed.");
        }

        Reject(() => _ = CliReleaseCommand.GetExpectedReleaseAssetSet("1.2.3", "freebsd-x64"), "Unsupported CLI RuntimeIdentifier");
    }

    internal static void CheckBinaryHeaders()
    {
        byte[] peX64 = CreatePe(0x8664);
        CliReleaseCommand.VerifyBinaryHeader(peX64, "win-x64");
        CliReleaseCommand.VerifyBinaryHeader(CreatePe(0xaa64), "win-arm64");
        Reject(() => CliReleaseCommand.VerifyBinaryHeader(peX64, "win-arm64"), "PE");
        Reject(() => CliReleaseCommand.VerifyBinaryHeader(CreatePe(0x8664, clrRva: 0x2000), "win-x64"), "CLR header");

        byte[] elfX64 = CreateElf(0x3e);
        CliReleaseCommand.VerifyBinaryHeader(elfX64, "linux-x64");
        CliReleaseCommand.VerifyBinaryHeader(CreateElf(0xb7), "linux-arm64");
        Reject(() => CliReleaseCommand.VerifyBinaryHeader(elfX64, "linux-arm64"), "ELF");

        byte[] appImage = CreateElf(0x3e);
        appImage[8] = 0x41;
        appImage[9] = 0x49;
        appImage[10] = 0x02;
        CliReleaseCommand.VerifyBinaryHeader(appImage, "linux-x64", isAppImage: true);
        Reject(() => CliReleaseCommand.VerifyBinaryHeader(elfX64, "linux-x64", isAppImage: true), "AppImage");

        byte[] machO = new byte[32];
        BinaryPrimitives.WriteUInt32LittleEndian(machO.AsSpan(0, 4), 0xfeedfacfu);
        BinaryPrimitives.WriteUInt32LittleEndian(machO.AsSpan(4, 4), 0x0100000cu);
        BinaryPrimitives.WriteUInt32LittleEndian(machO.AsSpan(12, 4), 2u);
        CliReleaseCommand.VerifyBinaryHeader(machO, "osx-arm64");
        machO[4] = 7;
        Reject(() => CliReleaseCommand.VerifyBinaryHeader(machO, "osx-arm64"), "Mach-O");
    }

    internal static void CheckJpegFrames()
    {
        byte[] baseline =
        [
            0xff, 0xd8,
            0xff, 0xe0, 0x00, 0x04, 0x00, 0x00,
            0xff, 0xc0, 0x00, 0x0b, 0x08, 0x00, 0x03, 0x00, 0x05, 0x01, 0x01, 0x11, 0x00,
            0xff, 0xda, 0x00, 0x02,
            0xff, 0xd9,
        ];
        CliReleaseCommand.AssertJpegFrame(baseline, 0xc0);
        Reject(() => CliReleaseCommand.AssertJpegFrame(baseline, 0xc2), "Unexpected JPEG frame");
        Reject(() => CliReleaseCommand.AssertJpegFrame([0xff, 0xd8, 0xff, 0xda, 0xff, 0xd9], 0xc0), "no JPEG frame");
        Reject(() => CliReleaseCommand.AssertJpegFrame([0xff, 0xd8], 0xc0), "complete JPEG");
    }

    private static byte[] CreatePe(ushort machine, uint clrRva = 0)
    {
        byte[] pe = new byte[320];
        pe[0] = (byte)'M';
        pe[1] = (byte)'Z';
        const int peOffset = 0x40;
        BinaryPrimitives.WriteInt32LittleEndian(pe.AsSpan(0x3c, 4), peOffset);
        BinaryPrimitives.WriteUInt32LittleEndian(pe.AsSpan(peOffset, 4), 0x4550);
        BinaryPrimitives.WriteUInt16LittleEndian(pe.AsSpan(peOffset + 4, 2), machine);
        BinaryPrimitives.WriteUInt16LittleEndian(pe.AsSpan(peOffset + 24, 2), 0x20b);
        BinaryPrimitives.WriteUInt32LittleEndian(pe.AsSpan(peOffset + 24 + 112 + (14 * 8), 4), clrRva);
        return pe;
    }

    private static byte[] CreateElf(ushort machine)
    {
        byte[] elf = new byte[64];
        elf[0] = 0x7f;
        elf[1] = (byte)'E';
        elf[2] = (byte)'L';
        elf[3] = (byte)'F';
        elf[4] = 2;
        elf[5] = 1;
        BinaryPrimitives.WriteUInt16LittleEndian(elf.AsSpan(16, 2), 2);
        BinaryPrimitives.WriteUInt16LittleEndian(elf.AsSpan(18, 2), machine);
        return elf;
    }

    private static void Reject(Action action, string label)
    {
        try
        {
            action();
        }
        catch (InvalidDataException exception) when (exception.Message.Contains(label, StringComparison.Ordinal))
        {
            return;
        }

        throw new InvalidOperationException($"CLI release verification accepted invalid input: {label}.");
    }
}
