using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Tests.Resources;

internal static class TileTestData
{
    internal static byte[] Bytes(int size, int bits, int count) =>
        [.. Enumerable.Range(0, size * size * bits / 8 * count).Select(index => (byte)(index * 37 + 0x69))];

    internal static byte[] Pixels(byte[] bytes, int bits) =>
    [
        .. bytes.SelectMany(value => bits switch
        {
            1 => Convert.ToString(value, 2).PadLeft(8, '0').Select(character => (byte)(character - '0')),
            4 => [(byte)(value / 16), (byte)(value % 16)],
            _ => [value]
        })
    ];

    internal static BmpFile Image(int size, int bits, out byte[] expectedTiles)
    {
        expectedTiles = Pixels(Bytes(size, bits, 4), bits);
        for (var tile = 0; tile < 4; tile++)
        {
            expectedTiles[tile * size * size + tile] = (byte)((1 << bits) - 1);
        }
        var pixels = new byte[expectedTiles.Length];
        for (var y = 0; y < size; y++)
        {
            Array.Copy(expectedTiles, y * size, pixels, y * size * 2, size);
            Array.Copy(expectedTiles, size * size + y * size, pixels, y * size * 2 + size, size);
            Array.Copy(expectedTiles, 2 * size * size + y * size, pixels, (y + size) * size * 2, size);
            Array.Copy(expectedTiles, 3 * size * size + y * size, pixels, (y + size) * size * 2 + size, size);
        }
        return new BmpFile(new IndexedImageData(size * 2, size * 2, pixels,
            new PaletteData(Enumerable.Repeat(new Colour(0, 0, 0), 256))));
    }
}