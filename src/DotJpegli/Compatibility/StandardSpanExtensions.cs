#if NETSTANDARD2_1
namespace DotJpegli.Compatibility;

internal static class StandardSpanExtensions
{
    internal static bool Contains(this ReadOnlySpan<char> span, char value) => span.IndexOf(value) >= 0;
    internal static bool Contains(this Span<char> span, char value) => span.IndexOf(value) >= 0;

    internal static bool ContainsAny(this ReadOnlySpan<byte> span, byte first, byte second, byte third) =>
        span.IndexOfAny(first, second, third) >= 0;

    internal static bool ContainsAnyExceptInRange(this Span<ushort> span, ushort lowInclusive, ushort highInclusive)
    {
        foreach (ushort value in span)
            if (value < lowInclusive || value > highInclusive)
                return true;
        return false;
    }

    internal static int Count(this ReadOnlySpan<short> span, short value)
    {
        int count = 0;
        foreach (short item in span)
            if (item == value)
                count++;
        return count;
    }

    internal static bool ContainsAnyInRange(this Span<int> span, int lowInclusive, int highInclusive)
    {
        foreach (int value in span)
            if (value >= lowInclusive && value <= highInclusive)
                return true;
        return false;
    }

    // Huffman construction sorts at most 257 nodes. Heap sort keeps its existing
    // span storage and the same comparison without a second temporary array.
    internal static void Sort<T>(this Span<T> span, Comparison<T> comparison)
    {
        for (int parent = span.Length / 2 - 1; parent >= 0; parent--)
            SiftDown(span, parent, comparison);
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
            int child = parent * 2 + 1;
            if (child + 1 < span.Length && comparison(span[child], span[child + 1]) < 0)
                child++;
            if (comparison(span[parent], span[child]) >= 0)
                return;
            (span[parent], span[child]) = (span[child], span[parent]);
            parent = child;
        }
    }
}
#endif
