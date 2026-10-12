// Copyright (c) the JPEG XL Project Authors.
// Use of this source code is governed by the BSD-3-Clause license in LICENSE.jpegli.

using System.Buffers.Binary;

namespace DotJpegli.Internal;

internal static class IccProfile
{
    // Derived from FindCICPTag and LookupCICPTransferFunctionFromICCProfile in
    // lib/extras/enc/jpegli.cc, revision 031a0077f5799a6041004267fc12b956c1f52a20.
    // This is a narrow metadata lookup, not validation of an ICC profile or a
    // color transform. Match upstream's last-entry rule and 12-byte payload check.
    internal static int LookupTransferFunction(ReadOnlySpan<byte> profile, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        const int unknown = 2;
        const int directoryStart = 132;
        const int entrySize = 12;
        if (profile.Length < directoryStart)
            return unknown;

        uint count = BinaryPrimitives.ReadUInt32BigEndian(profile.Slice(128, 4));
        // Divide the available length instead of multiplying an untrusted count.
        if (count > (uint)((profile.Length - directoryStart) / entrySize))
            return unknown;

        uint offset = 0;
        uint length = 0;
        for (int index = 0; index < (int)count; index++)
        {
            if ((index & 1023) == 0)
                cancellationToken.ThrowIfCancellationRequested();
            ReadOnlySpan<byte> entry = profile.Slice(directoryStart + entrySize * index, entrySize);
            if (entry[..4].SequenceEqual("cicp"u8))
            {
                offset = BinaryPrimitives.ReadUInt32BigEndian(entry.Slice(4, 4));
                length = BinaryPrimitives.ReadUInt32BigEndian(entry.Slice(8, 4));
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        // Only the first 12 bytes are consumed, even for a larger declared tag.
        // Check unsigned offsets before conversion or slicing to avoid wraparound.
        if (length < entrySize || offset > (uint)(profile.Length - entrySize))
            return unknown;
        ReadOnlySpan<byte> tag = profile.Slice((int)offset, entrySize);
        return tag[..4].SequenceEqual("cicp"u8) ? tag[9] : unknown;
    }
}
