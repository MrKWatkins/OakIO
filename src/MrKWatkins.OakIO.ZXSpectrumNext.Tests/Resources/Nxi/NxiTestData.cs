using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Nxi;

internal static class NxiTestData
{
    internal static byte[] Bytes(int width, int height)
    {
        var paletteCount = width == 640 ? 16 : 256;
        return [.. Enumerable.Range(0, paletteCount).SelectMany(index => new[] { (byte)(index * 37), (byte)(index % 2) }),
            .. Enumerable.Range(0, width * height / (width == 640 ? 2 : 1)).Select(index => (byte)(index * 37 + index / 256 + 11))];
    }

    // Expected order intentionally uses column selection, not the production nested-coordinate loops.
    internal static byte[] Pixels(byte[] bytes, int width, int height)
    {
        var packed = bytes.Skip(width == 640 ? 32 : 512).ToArray();
        if (width == 256)
        {
            return packed;
        }
        return [.. Enumerable.Range(0, height).SelectMany(y =>
            packed.Where((_, index) => index % height == y).SelectMany(value => width == 640
                ? new[] { (byte)(value / 16), (byte)(value % 16) }
                : new[] { value }))];
    }

    internal static Colour[] Colours(byte[] bytes, int width)
    {
        byte[] levels = [0, 36, 73, 109, 146, 182, 219, 255];
        return [.. bytes.Take(width == 640 ? 32 : 512).Chunk(2).Select(entry =>
            new Colour(levels[entry[0] / 32], levels[entry[0] / 4 % 8], levels[entry[0] % 4 * 2 + entry[1]]))];
    }

    internal static IndexedImageData Image(int width, int height, int bits = 8)
    {
        var bytes = Bytes(width, height);
        return new IndexedImageData(width, height, Pixels(bytes, width, height), new PaletteData(Colours(bytes, width)), bits);
    }
}