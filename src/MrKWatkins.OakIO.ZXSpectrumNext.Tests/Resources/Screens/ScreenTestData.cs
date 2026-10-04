using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources.Screens;

internal static class ScreenTestData
{
    public static IndexedImageData Image(int width, int height, int bits)
    {
        var palette = new ScreenPalette([], 1 << bits).Palette;
        var pixels = Enumerable.Range(0, width * height).Select(index => (byte)((index * 7 + index / width) % (1 << bits))).ToArray();
        return new IndexedImageData(width, height, pixels, palette, bits);
    }
}