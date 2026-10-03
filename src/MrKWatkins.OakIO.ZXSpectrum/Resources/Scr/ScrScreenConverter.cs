using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

// Adapted from OakEmu's FallbackScreenConverter and VectorizedScreenConverter:
// https://github.com/MrKWatkins/OakEmu/tree/main/src/MrKWatkins.OakEmu.Machines.ZXSpectrum/Screen
// Keep indexed colours (including bright black); RGB interpretation is a separate palette view.
internal static class ScrScreenConverter
{
    [Pure]
    internal static byte[] Convert(ReadOnlySpan<byte> bitmap, ReadOnlySpan<byte> attributes, bool flashPhase) =>
        Vector256.IsHardwareAccelerated ? ConvertVectorized(bitmap, attributes, flashPhase) : ConvertScalar(bitmap, attributes, flashPhase);

    [Pure]
    internal static byte[] ConvertScalar(ReadOnlySpan<byte> bitmap, ReadOnlySpan<byte> attributes, bool flashPhase)
    {
        var pixels = new byte[256 * 192];
        for (var y = 0; y < 192; y++)
        {
            for (var column = 0; column < 32; column++)
            {
                var attribute = attributes[y / 8 * 32 + column];
                var bright = (attribute & 64) >> 3;
                var ink = (byte)((attribute & 7) | bright);
                var paper = (byte)(((attribute >> 3) & 7) | bright);
                if (flashPhase && (attribute & 128) != 0)
                {
                    (ink, paper) = (paper, ink);
                }
                var bits = bitmap[ScrBitmap.GetByteOffset(column * 8, y)];
                for (var bit = 0; bit < 8; bit++)
                {
                    pixels[y * 256 + column * 8 + bit] = (bits & (128 >> bit)) != 0 ? ink : paper;
                }
            }
        }
        return pixels;
    }

    [Pure]
    internal static byte[] ConvertVectorized(ReadOnlySpan<byte> bitmap, ReadOnlySpan<byte> attributes, bool flashPhase)
    {
        var pixels = new byte[256 * 192];
        ref var destination = ref MemoryMarshal.GetArrayDataReference(pixels);
        var bitMask = Vector256.Create(128, 64, 32, 16, 8, 4, 2, 1,
            128, 64, 32, 16, 8, 4, 2, 1, 128, 64, 32, 16, 8, 4, 2, 1, 128, 64, 32, 16, 8, 4, 2, 1);
        var shuffleBase = Vector256.Create((byte)0, 0, 0, 0, 0, 0, 0, 0,
            1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3);
        for (var y = 0; y < 192; y++)
        {
            var attribute = Vector256.Create(attributes.Slice(y / 8 * 32, 32));
            var bright = Vector256.ShiftRightLogical(attribute & Vector256.Create((byte)64), 3);
            var ink = bright | (attribute & Vector256.Create((byte)7));
            var paper = bright | (Vector256.ShiftRightLogical(attribute, 3) & Vector256.Create((byte)7));
            if (flashPhase)
            {
                var flash = Vector256.Equals(attribute & Vector256.Create((byte)128), Vector256.Create((byte)128));
                var originalInk = ink;
                ink = Vector256.ConditionalSelect(flash, paper, ink);
                paper = Vector256.ConditionalSelect(flash, originalInk, paper);
            }
            // Load exactly one complete bitmap row: unlike OakEmu we have no adjacent attribute bytes to overread.
            var row = Vector256.Create(bitmap.Slice(ScrBitmap.GetByteOffset(0, y), 32));
            for (var section = 0; section < 8; section++)
            {
                var shuffle = shuffleBase + Vector256.Create((byte)(section * 4));
                var bits = Vector256.Shuffle(row, shuffle) & bitMask;
                var selection = Vector256.Equals(bits, bitMask);
                var result = Vector256.ConditionalSelect(selection, Vector256.Shuffle(ink, shuffle), Vector256.Shuffle(paper, shuffle));
                result.StoreUnsafe(ref destination, (nuint)(y * 256 + section * 32));
            }
        }
        return pixels;
    }
}