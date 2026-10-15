#if NETSTANDARD2_1
namespace DotJpegli.Compatibility;

internal static class StandardAscii
{
    internal static bool IsValid(ReadOnlySpan<byte> value)
    {
        foreach (byte character in value)
            if (character > 127)
                return false;
        return true;
    }

    internal static bool IsValid(ReadOnlySpan<char> value)
    {
        foreach (char character in value)
            if (character > 127)
                return false;
        return true;
    }
}
#endif
