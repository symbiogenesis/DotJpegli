namespace DotJpegli.Compatibility;
#if NETSTANDARD2_1
internal static class StandardSpanExtensions
{
    internal static bool Contains(this ReadOnlySpan<char> span, char value) => span.IndexOf(value) >= 0;

    internal static bool ContainsAny(this ReadOnlySpan<byte> span, byte first, byte second, byte third) =>
        span.IndexOfAny(first, second, third) >= 0;

    internal static bool ContainsAnyExceptInRange<T>(this ReadOnlySpan<T> span, T lowInclusive, T highInclusive)
        where T : IComparable<T>
    {
        foreach (T value in span)
        {
            if (value.CompareTo(lowInclusive) < 0 || value.CompareTo(highInclusive) > 0)
            {
                return true;
            }
        }

        return false;
    }

    internal static int Count<T>(this ReadOnlySpan<T> span, T value)
        where T : IEquatable<T>
    {
        int count = 0;
        foreach (T item in span)
        {
            if (item.Equals(value))
            {
                count++;
            }
        }

        return count;
    }

    internal static bool ContainsAnyInRange<T>(this ReadOnlySpan<T> span, T lowInclusive, T highInclusive)
        where T : IComparable<T>
    {
        foreach (T value in span)
        {
            if (value.CompareTo(lowInclusive) >= 0 && value.CompareTo(highInclusive) <= 0)
            {
                return true;
            }
        }

        return false;
    }

    internal static bool ContainsAnyExcept<T>(this ReadOnlySpan<T> span, T value)
        where T : IEquatable<T>
    {
        foreach (T item in span)
        {
            if (!item.Equals(value))
            {
                return true;
            }
        }

        return false;
    }

    internal static void Sort<T>(this Span<T> span)
        where T : IComparable<T> =>
        span.Sort(static (left, right) => left.CompareTo(right));

    // Huffman construction sorts at most 257 nodes. Heap sort keeps its existing
    // span storage and the same comparison without a second temporary array.
    internal static void Sort<T>(this Span<T> span, Comparison<T> comparison)
    {
        for (int parent = (span.Length / 2) - 1; parent >= 0; parent--)
        {
            SiftDown(span, parent, comparison);
        }

        for (int end = span.Length - 1; end > 0; end--)
        {
            (span[0], span[end]) = (span[end], span[0]);
            SiftDown(span[..end], 0, comparison);
        }
    }

    private static void SiftDown<T>(Span<T> span, int parent, Comparison<T> comparison)
    {
        while (parent < span.Length / 2)
        {
            int child = (parent * 2) + 1;
            if (child + 1 < span.Length && comparison(span[child], span[child + 1]) < 0)
            {
                child++;
            }

            if (comparison(span[parent], span[child]) >= 0)
            {
                return;
            }

            (span[parent], span[child]) = (span[child], span[parent]);
            parent = child;
        }
    }
}
#endif
