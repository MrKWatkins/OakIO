using MrKWatkins.OakIO.Resources;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources;

internal static class PackedPixels
{
    // Chunky packing: MSB-first pixels; high nibble is the left pixel at four bits.
    // https://wiki.specnext.dev/Sprite_Pattern_Upload
    // https://github.com/benbaker76/Gfx2Next/blob/master/src/gfx2next.c
    [Pure]
    internal static byte[] Pack(IReadOnlyList<byte> pixels, int bitsPerPixel)
    {
        ArgumentNullException.ThrowIfNull(pixels);
        ValidateBitsPerPixel(bitsPerPixel);
        var pixelsPerByte = 8 / bitsPerPixel;
        if (pixels.Count % pixelsPerByte != 0)
        {
            throw new ArgumentException("The pixels must fill complete bytes.", nameof(pixels));
        }
        var bytes = new byte[pixels.Count / pixelsPerByte];
        for (var index = 0; index < pixels.Count; index++)
        {
            if (pixels[index] >= 1 << bitsPerPixel)
            {
                throw new ArgumentException("A pixel index does not fit the target bit depth.", nameof(pixels));
            }
            var shift = 8 - bitsPerPixel - index % pixelsPerByte * bitsPerPixel;
            bytes[index / pixelsPerByte] |= (byte)(pixels[index] << shift);
        }
        return bytes;
    }

    [Pure]
    internal static byte[] Unpack(ReadOnlySpan<byte> bytes, int bitsPerPixel)
    {
        ValidateBitsPerPixel(bitsPerPixel);
        var pixelsPerByte = 8 / bitsPerPixel;
        var pixels = new byte[checked(bytes.Length * pixelsPerByte)];
        var mask = (1 << bitsPerPixel) - 1;
        for (var index = 0; index < pixels.Length; index++)
        {
            var shift = 8 - bitsPerPixel - index % pixelsPerByte * bitsPerPixel;
            pixels[index] = (byte)((bytes[index / pixelsPerByte] >> shift) & mask);
        }
        return pixels;
    }

    [Pure]
    internal static TilesData FromImage(ImageFile source, int tileSize, int bitsPerPixel)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.Image is not IndexedImageData image)
        {
            throw new NotSupportedException("Tile conversion requires an indexed image; palette generation is a separate operation.");
        }
        if (image.Width % tileSize != 0 || image.Height % tileSize != 0)
        {
            throw new ArgumentException("The image dimensions must be multiples of the tile size.", nameof(source));
        }
        var pixels = new byte[image.PixelCount];
        var destination = 0;
        for (var tileY = 0; tileY < image.Height; tileY += tileSize)
        {
            for (var tileX = 0; tileX < image.Width; tileX += tileSize)
            {
                for (var y = 0; y < tileSize; y++)
                {
                    for (var x = 0; x < tileSize; x++)
                    {
                        pixels[destination++] = image.Pixels[(tileY + y) * image.Width + tileX + x];
                    }
                }
            }
        }
        return new TilesData(tileSize, tileSize, pixels, bitsPerPixel);
    }

    [Pure]
    internal static TilesData FromTiles(TilesData tiles, int tileSize, int bitsPerPixel)
    {
        ArgumentNullException.ThrowIfNull(tiles);
        if (tiles.Width != tileSize || tiles.Height != tileSize)
        {
            throw new ArgumentException("The source tiles must have the target dimensions.", nameof(tiles));
        }
        return new TilesData(tileSize, tileSize, [.. tiles.Pixels], bitsPerPixel);
    }

    private static void ValidateBitsPerPixel(int bitsPerPixel)
    {
        if (bitsPerPixel is not (1 or 4 or 8))
        {
            throw new ArgumentOutOfRangeException(nameof(bitsPerPixel), bitsPerPixel, "The bit depth must be one, four or eight.");
        }
    }

    internal static void ValidateLength(int length, int expectedLength)
    {
        if (length != expectedLength)
        {
            throw new InvalidDataException("The data must contain a complete pattern or tile.");
        }
    }
}