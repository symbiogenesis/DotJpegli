using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace DotJpegli.Compatibility;

internal static class Guard
{
    internal static void ThrowIfNull([NotNull] object? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
#if NETSTANDARD2_1
        if (value is null)
            throw new ArgumentNullException(parameterName);
#else
        ArgumentNullException.ThrowIfNull(value, parameterName);
#endif
    }

    internal static void ThrowIfNullOrEmpty([NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
#if NETSTANDARD2_1
        ThrowIfNull(value, parameterName);
        if (value.Length == 0)
            throw new ArgumentException("The value cannot be an empty string.", parameterName);
#else
        ArgumentException.ThrowIfNullOrEmpty(value, parameterName);
#endif
    }

    internal static void ThrowIfNegative(int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
#if NETSTANDARD2_1
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, value, "The value must be non-negative.");
#else
        ArgumentOutOfRangeException.ThrowIfNegative(value, parameterName);
#endif
    }

    internal static void ThrowIfNegative(long value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
#if NETSTANDARD2_1
        if (value < 0)
            throw new ArgumentOutOfRangeException(parameterName, value, "The value must be non-negative.");
#else
        ArgumentOutOfRangeException.ThrowIfNegative(value, parameterName);
#endif
    }

    internal static void ThrowIfNegativeOrZero(int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
#if NETSTANDARD2_1
        if (value <= 0)
            throw new ArgumentOutOfRangeException(parameterName, value, "The value must be positive.");
#else
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, parameterName);
#endif
    }

    internal static void ThrowIfLessThan(int value, int minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
#if NETSTANDARD2_1
        if (value < minimum)
            throw new ArgumentOutOfRangeException(parameterName, value, $"The value must be at least {minimum}.");
#else
        ArgumentOutOfRangeException.ThrowIfLessThan(value, minimum, parameterName);
#endif
    }

    internal static void ThrowIfDisposed(bool condition, object instance)
    {
#if NETSTANDARD2_1
        if (condition)
            throw new ObjectDisposedException(instance.GetType().FullName);
#else
        ObjectDisposedException.ThrowIf(condition, instance);
#endif
    }
}
