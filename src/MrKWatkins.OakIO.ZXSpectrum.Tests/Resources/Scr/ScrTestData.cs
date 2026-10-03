using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrum.Tests.Resources.Scr;

internal static class ScrTestData
{
    // Deliberately independent of the production palette and bitmap address calculation.
    internal static readonly Colour[] Colours =
    [
        new(0, 0, 0), new(0, 0, 192), new(192, 0, 0), new(192, 0, 192),
        new(0, 192, 0), new(0, 192, 192), new(192, 192, 0), new(192, 192, 192),
        new(0, 0, 0), new(0, 0, 255), new(255, 0, 0), new(255, 0, 255),
        new(0, 255, 0), new(0, 255, 255), new(255, 255, 0), new(255, 255, 255)
    ];

    internal static byte[] Bytes() => [.. Enumerable.Range(0, 6912).Select(index => (byte)(index * 37 + index / 256 + 11))];

    internal static byte[] Pixels(byte[] data, bool flashPhase)
    {
        var pixels = new byte[256 * 192];
        var source = 0;
        for (var third = 0; third < 3; third++)
        {
            for (var scan = 0; scan < 8; scan++)
            {
                for (var characterRow = 0; characterRow < 8; characterRow++)
                {
                    var y = third * 64 + characterRow * 8 + scan;
                    for (var column = 0; column < 32; column++)
                    {
                        var attribute = data[6144 + (third * 8 + characterRow) * 32 + column];
                        var ink = attribute % 8 + (attribute / 64 % 2) * 8;
                        var paper = attribute / 8 % 8 + (attribute / 64 % 2) * 8;
                        var bits = Convert.ToString(data[source++], 2).PadLeft(8, '0');
                        for (var bit = 0; bit < 8; bit++)
                        {
                            var isInk = bits[bit] == '1';
                            if (flashPhase && attribute >= 128)
                            {
                                isInk = !isInk;
                            }
                            pixels[y * 256 + column * 8 + bit] = (byte)(isInk ? ink : paper);
                        }
                    }
                }
            }
        }
        return pixels;
    }
}