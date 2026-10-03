using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxi;

/// <summary>
/// Native Layer 2 bitmap bytes with dimensions and packing determined by the supported screen layout.
/// </summary>
/// <remarks>
/// Layout references: https://wiki.specnext.dev/Layer_2 and
/// https://github.com/stefanbylund/zxnext_bmp_tools/blob/master/src/nextraw.c.
/// </remarks>
public sealed class NxiPixelData : IOFileComponent
{
    internal NxiPixelData(byte[] data, int width, int height) : base(GetLength(width, height), data)
    {
        Width = width;
        Height = height;
        BitsPerPixel = NxiFormat.GetBitsPerPixel(width, height);
    }

    internal NxiPixelData(IndexedImageData image) : this(Encode(image), image.Width, image.Height)
    {
    }

    /// <summary>
    /// Gets the screen width, inferred from the supported file layout rather than a stored header.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the screen height, inferred from the supported file layout rather than a stored header.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the native pixel depth: four or eight.
    /// </summary>
    public int BitsPerPixel { get; }

    /// <summary>
    /// Gets a derived, unpacked top-left-origin row-major view of the pixel indices.
    /// </summary>
    public IReadOnlyList<byte> Pixels
    {
        get
        {
            if (Width == 256)
            {
                return [.. Data];
            }
            var pixels = new byte[Width * Height];
            var pixelsPerByte = 8 / BitsPerPixel;
            for (var x = 0; x < Width; x += pixelsPerByte)
            {
                for (var y = 0; y < Height; y++)
                {
                    var packed = GetByte(x / pixelsPerByte * Height + y);
                    if (BitsPerPixel == 8)
                    {
                        pixels[y * Width + x] = packed;
                    }
                    else
                    {
                        pixels[y * Width + x] = (byte)(packed >> 4);
                        pixels[y * Width + x + 1] = (byte)(packed & 15);
                    }
                }
            }
            return pixels;
        }
    }

    [Pure]
    private static int GetLength(int width, int height) => width * height * NxiFormat.GetBitsPerPixel(width, height) / 8;

    [Pure]
    private static byte[] Encode(IndexedImageData image)
    {
        ArgumentNullException.ThrowIfNull(image);
        var bits = NxiFormat.GetBitsPerPixel(image.Width, image.Height);
        // In particular, reject eight-bit source indices exceeding 15 in the four-bit mode; never mask them away.
        IndexedImageData.ValidatePixels([.. image.Pixels], 1 << bits);
        if (image.Width == 256)
        {
            return [.. image.Pixels];
        }
        var bytes = new byte[GetLength(image.Width, image.Height)];
        var pixelsPerByte = 8 / bits;
        for (var x = 0; x < image.Width; x += pixelsPerByte)
        {
            for (var y = 0; y < image.Height; y++)
            {
                var offset = y * image.Width + x;
                bytes[x / pixelsPerByte * image.Height + y] = bits == 8
                    ? image.Pixels[offset]
                    : (byte)((image.Pixels[offset] << 4) | image.Pixels[offset + 1]);
            }
        }
        return bytes;
    }
}