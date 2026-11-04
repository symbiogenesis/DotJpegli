namespace DotJpegli.Performance;

using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using DotJpegli.Internal;

// A validation workload with a shared, well-distributed sampled destination
// curve. Built-in parametric destination profiles do not exercise curve indexing.
internal static class SampledIccScenario
{
    internal const int KnotCount = 1025;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static byte[] CreateProfile()
    {
        byte[] original = JpegliIccProfiles.CreateLinearSrgbProfile();
        int offset = (original.Length + 3) & ~3;
        int length = 12 + (KnotCount * sizeof(ushort));
        byte[] profile = new byte[offset + ((length + 3) & ~3)];
        original.CopyTo(profile, 0);
        BinaryPrimitives.WriteUInt32BigEndian(profile, (uint)profile.Length);
        "curv"u8.CopyTo(profile.AsSpan(offset));
        BinaryPrimitives.WriteUInt32BigEndian(profile.AsSpan(offset + 8), KnotCount);
        for (int knot = 0; knot < KnotCount; knot++)
        {
            BinaryPrimitives.WriteUInt16BigEndian(profile.AsSpan(offset + 12 + (knot * sizeof(ushort))), Sample(knot));
        }
        int count = checked((int)BinaryPrimitives.ReadUInt32BigEndian(profile.AsSpan(128)));
        int patched = 0;
        for (int tag = 0; tag < count; tag++)
        {
            int entry = 132 + (tag * 12);
            ReadOnlySpan<byte> signature = profile.AsSpan(entry, 4);
            if (signature.SequenceEqual("rTRC"u8) || signature.SequenceEqual("gTRC"u8) || signature.SequenceEqual("bTRC"u8))
            {
                BinaryPrimitives.WriteUInt32BigEndian(profile.AsSpan(entry + 4), (uint)offset);
                BinaryPrimitives.WriteUInt32BigEndian(profile.AsSpan(entry + 8), (uint)length);
                patched++;
            }
        }
        if (patched != 3)
        {
            throw new InvalidDataException("The sampled ICC fixture requires three RGB transfer tags.");
        }
        IccProfileWriter.ProfileId(profile);
        return profile;
    }

    internal static void SelfTest()
    {
        byte[] profile = CreateProfile(), fresh = CreateProfile();
        Assert(!ReferenceEquals(profile, fresh) && profile.AsSpan().SequenceEqual(fresh),
            "Sampled ICC profiles must have deterministic bytes and independent storage.");
        Assert(BinaryPrimitives.ReadUInt32BigEndian(profile) == profile.Length && (profile.Length & 3) == 0
            && profile.AsSpan(36, 4).SequenceEqual("acsp"u8), "The sampled profile header is invalid.");
        ValidateProfileId(profile);
        int count = checked((int)BinaryPrimitives.ReadUInt32BigEndian(profile.AsSpan(128)));
        int curveOffset = -1, curves = 0;
        for (int tag = 0; tag < count; tag++)
        {
            int entry = 132 + (tag * 12);
            int offset = checked((int)BinaryPrimitives.ReadUInt32BigEndian(profile.AsSpan(entry + 4)));
            int length = checked((int)BinaryPrimitives.ReadUInt32BigEndian(profile.AsSpan(entry + 8)));
            Assert(offset >= 132 + (count * 12) && (offset & 3) == 0 && length > 0 && length <= profile.Length - offset,
                "A sampled profile tag has invalid alignment or extent.");
            ReadOnlySpan<byte> signature = profile.AsSpan(entry, 4);
            if (signature.SequenceEqual("rTRC"u8) || signature.SequenceEqual("gTRC"u8) || signature.SequenceEqual("bTRC"u8))
            {
                Assert(curveOffset < 0 || curveOffset == offset, "All destination RGB channels must share one sampled curve.");
                curveOffset = offset;
                curves++;
                Assert(length == 12 + (KnotCount * sizeof(ushort)) && profile.AsSpan(offset, 4).SequenceEqual("curv"u8)
                    && BinaryPrimitives.ReadUInt32BigEndian(profile.AsSpan(offset + 8)) == KnotCount,
                    "The destination curve must contain exactly 1025 sampled knots.");
            }
        }
        Assert(curves == 3, "The sampled destination requires all three RGB curves.");
        double[] values = new double[KnotCount];
        int[] cells = new int[256];
        for (int knot = 0; knot < KnotCount; knot++)
        {
            ushort sample = BinaryPrimitives.ReadUInt16BigEndian(profile.AsSpan(curveOffset + 12 + (knot * 2)));
            Assert(sample == Sample(knot), "The sampled ramp's stored precision changed.");
            values[knot] = sample / 65535.0;
            cells[Math.Min(255, (int)(values[knot] * 256))]++;
            Assert(knot == 0 || values[knot] > values[knot - 1], "The sampled destination must be strictly increasing.");
        }
        Assert(values[0] == 0 && values[^1] == 1 && cells.Max() <= 32,
            "The destination ramp must cover both endpoints and satisfy the bounded index's density guard.");
        byte[] source = JpegliIccProfiles.CreateDisplayP3Profile();
        var transform = IccColorTransform.Create(source, profile, false, false,
            IccRenderingIntent.RelativeColorimetric, CancellationToken.None);
        var linear = IccColorTransform.Create(source, JpegliIccProfiles.CreateLinearSrgbProfile(), false, false,
            IccRenderingIntent.RelativeColorimetric, CancellationToken.None);
        for (int value = 0; value < 256; value++)
        {
            float sample = (float)(value * (1.0 / 255.0));
            Check(transform, linear, values, sample, 0, 0);
            Check(transform, linear, values, 0, sample, 0);
            Check(transform, linear, values, 0, 0, sample);
            Check(transform, linear, values, sample, sample, sample);
            Check(transform, linear, values, sample, (float)((255 - value) * (1.0 / 255.0)),
                (float)(((value * 73) & 255) * (1.0 / 255.0)));
        }
        profile[0] ^= 1;
        Assert(fresh.AsSpan().SequenceEqual(CreateProfile()), "A caller mutation must not alter another fixture or a later profile.");
    }

    private static void Check(IccColorTransform transform, IccColorTransform linear, double[] values,
        float red, float green, float blue)
    {
        linear.Convert(red, green, blue, out float expectedRed, out float expectedGreen, out float expectedBlue);
        transform.Convert(red, green, blue, out float actualRed, out float actualGreen, out float actualBlue);
        Near(Inverse(values, expectedRed), actualRed);
        Near(Inverse(values, expectedGreen), actualGreen);
        Near(Inverse(values, expectedBlue), actualBlue);
    }

    // UInt16 cannot encode an exactly unit-endpoint linear ramp at 1025 knots:
    // 65535 is not divisible by 1024. Use the exact stored knots and an independent
    // linear-scan inverse, rather than claiming equality with an ideal linear TRC.
    private static float Inverse(double[] values, float input)
    {
        double y = Math.Clamp(input, 0, 1);
        int upper = 0;
        while (values[upper] < y)
        {
            upper++;
        }
        if (values[upper] == y)
        {
            return (float)((double)upper / (values.Length - 1));
        }
        int lower = upper - 1;
        return (float)((lower + ((y - values[lower]) / (values[upper] - values[lower]))) / (values.Length - 1));
    }

    private static void Near(float expected, float actual)
    {
        // The linear reference's public API rounds its intermediate RGB to float.
        // The indexed transform retains double until after the sampled inverse.
        // Its maximum inverse slope is 65535/(1024*63) < 1.016, bounding this
        // extra intermediate rounding by two final positive-float ULPs.
        long difference = Math.Abs((long)BitConverter.SingleToInt32Bits(expected) - BitConverter.SingleToInt32Bits(actual));
        Assert(float.IsFinite(actual) && actual is >= 0 and <= 1 && difference <= 2,
            "Sampled destination conversion disagrees with the stored-knot linear-transform oracle.");
    }

    [SuppressMessage("Security", "CA5351:Do Not Use Broken Cryptographic Algorithms",
        Justification = "ICC requires MD5 for profile identification; this validates a format identifier, not a security property.")]
    private static void ValidateProfileId(byte[] profile)
    {
        byte[] input = profile.ToArray();
        input.AsSpan(44, 4).Clear();
        input.AsSpan(64, 4).Clear();
        input.AsSpan(84, 16).Clear();
        Assert(MD5.HashData(input).AsSpan().SequenceEqual(profile.AsSpan(84, 16)), "The sampled ICC profile ID is stale.");
    }

    private static ushort Sample(int knot) => (ushort)((long)knot * ushort.MaxValue / (KnotCount - 1));

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidDataException(message);
        }
    }
}
