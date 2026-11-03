namespace DotJpegli.Compatibility;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

internal static class Guard
{
#if NETSTANDARD2_1
    internal static void ThrowIfNull([NotNull] object? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(parameterName);
        }
    }
#else
    internal static void ThrowIfNull([NotNull] object? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        ArgumentNullException.ThrowIfNull(value, parameterName);
#endif


#if NETSTANDARD2_1
    internal static void ThrowIfNullOrEmpty([NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        ThrowIfNull(value, parameterName);
        if (value.Length == 0)
        {
            throw new ArgumentException("The value cannot be an empty string.", parameterName);
        }
    }
#else
    internal static void ThrowIfNullOrEmpty([NotNull] string? value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        ArgumentException.ThrowIfNullOrEmpty(value, parameterName);
#endif

#if NETSTANDARD2_1
    internal static void ThrowIfNegative(int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "The value must be non-negative.");
        }
    }
#else
    internal static void ThrowIfNegative(int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        ArgumentOutOfRangeException.ThrowIfNegative(value, parameterName);
#endif


#if NETSTANDARD2_1
    internal static void ThrowIfNegative(long value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "The value must be non-negative.");
        }
    }
#else
    internal static void ThrowIfNegative(long value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        ArgumentOutOfRangeException.ThrowIfNegative(value, parameterName);
#endif


#if NETSTANDARD2_1
    internal static void ThrowIfNegativeOrZero(int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "The value must be positive.");
        }
    }
#else
    internal static void ThrowIfNegativeOrZero(int value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value, parameterName);
#endif


#if NETSTANDARD2_1
    internal static void ThrowIfLessThan(int value, int minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value < minimum)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, $"The value must be at least {minimum}.");
        }
    }
#else
    internal static void ThrowIfLessThan(int value, int minimum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        ArgumentOutOfRangeException.ThrowIfLessThan(value, minimum, parameterName);
#endif


#if NETSTANDARD2_1
    internal static void ThrowIfDisposed(bool condition, object instance)
    {
        if (condition)
        {
            throw new ObjectDisposedException(instance.GetType().FullName);
        }
    }
#else
    internal static void ThrowIfDisposed(bool condition, object instance) =>
        ObjectDisposedException.ThrowIf(condition, instance);
#endif


#if NETSTANDARD2_1
    internal static void ThrowIfGreaterThan(int value, int maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value > maximum)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
#else
    internal static void ThrowIfGreaterThan(int value, int maximum,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, maximum, parameterName);
#endif


#if NETSTANDARD2_1
    internal static void ThrowIfZero(uint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null)
    {
        if (value == 0)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }
    }
#else
    internal static void ThrowIfZero(uint value,
        [CallerArgumentExpression(nameof(value))] string? parameterName = null) =>
        ArgumentOutOfRangeException.ThrowIfZero(value, parameterName);
#endif

}
