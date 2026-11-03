namespace DotJpegli.Tests;

using System.Buffers.Binary;
using DotJpegli.Internal;
using Xunit;

public sealed class EntropyByteStuffingTests
{
    [Fact]
    public void EveryStuffingMaskPreservesPayloadOrderAndBoundedWrites()
    {
        byte[] source = new byte[8];
        byte[] storage = new byte[37];
        foreach (int salt in new[] { 0, 1, 127, 128, 253, 254 })
        {
            for (int mask = 0; mask < 256; mask++)
            {
                var expected = new List<byte>(16);
                for (int lane = 0; lane < source.Length; lane++)
                {
                    byte value = (mask & (1 << lane)) != 0 ? (byte)0xff : (byte)((salt + (lane * 37)) % 255);
                    source[lane] = value;
                    expected.Add(value);
                    if (value == 0xff)
                    {
                        expected.Add(0);
                    }
                }

                ulong word = BinaryPrimitives.ReadUInt64BigEndian(source);
                foreach (int schedule in new[] { 0, 1, 2 })
                {
                    storage.AsSpan().Fill(0xa5);
                    int count = Stuff(schedule, word, storage.AsSpan(5, 16));
                    Assert.Equal(expected.Count, count);
                    Assert.Equal(expected, storage.Skip(5).Take(count));
                    Assert.All(storage.Take(5), value => Assert.Equal(0xa5, value));
                    Assert.All(storage.Skip(21), value => Assert.Equal(0xa5, value));
                    if (schedule == 0)
                    {
                        Assert.All(storage.Skip(5 + count).Take(16 - count), value => Assert.Equal(0xa5, value));
                    }
                }
            }
        }
    }

    [Fact]
    public void EveryPayloadByteAtEveryPositionMatchesIndependentStuffing()
    {
        byte[] source = [0x12, 0x34, 0x56, 0x78, 0x9a, 0xbc, 0xde, 0xf0];
        byte[] storage = new byte[32];
        for (int lane = 0; lane < source.Length; lane++)
        {
            byte original = source[lane];
            for (int payload = 0; payload < 256; payload++)
            {
                source[lane] = (byte)payload;
                byte[] expected = source.SelectMany(value => value == 0xff ? new byte[] { value, 0 } : [value]).ToArray();
                ulong word = BinaryPrimitives.ReadUInt64BigEndian(source);
                foreach (int schedule in new[] { 0, 1, 2 })
                {
                    storage.AsSpan().Fill(0x5a);
                    int count = Stuff(schedule, word, storage);
                    Assert.Equal(expected.Length, count);
                    Assert.Equal(expected, storage.Take(count));
                    Assert.All(storage.Skip(16), value => Assert.Equal(0x5a, value));
                }
            }

            source[lane] = original;
        }
    }

    [Fact]
    public void IncompleteDestinationIsRejectedBeforeWriting()
    {
        byte[] storage = new byte[17];
        for (int length = 0; length < 16; length++)
        {
            foreach (int schedule in new[] { 0, 1, 2 })
            {
                storage.AsSpan().Fill(0xc3);
                Assert.Equal("destination", Assert.Throws<ArgumentException>(() => Stuff(schedule, ulong.MaxValue,
                    storage.AsSpan(1, length))).ParamName);
                Assert.All(storage, value => Assert.Equal(0xc3, value));
            }
        }
    }

    private static int Stuff(int schedule, ulong word, Span<byte> destination) => schedule switch
    {
        0 => EntropyByteStuffing.StuffWordOriginal(word, destination),
        1 => EntropyByteStuffing.StuffWord(word, destination),
#if !DOTJPEGLI_NETSTANDARD_TESTS
        _ => EntropyByteStuffing.StuffWordVector(word, destination)
#else
        _ => EntropyByteStuffing.StuffWord(word, destination)
#endif
    };
}
