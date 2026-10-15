namespace DotJpegli.Compatibility;

internal static class RuntimeLimits
{
    internal static int ArrayMaxLength =>
#if NETSTANDARD2_1
        0x7fffffc7;
#else
        Array.MaxLength;
#endif
}
