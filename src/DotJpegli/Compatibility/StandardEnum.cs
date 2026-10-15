namespace DotJpegli.Compatibility;

internal static class StandardEnum
{
    internal static T[] GetValues<T>() where T : struct, Enum =>
#if NETSTANDARD2_1
        (T[])Enum.GetValues(typeof(T));
#else
        Enum.GetValues<T>();
#endif

    internal static bool IsDefined<T>(T value) where T : struct, Enum =>
#if NETSTANDARD2_1
        Enum.IsDefined(typeof(T), value);
#else
        Enum.IsDefined(value);
#endif
}
