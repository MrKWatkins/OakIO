namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// An immutable indexed image with unpacked palette indices and a reusable palette.
/// </summary>
public sealed class IndexedImageData : ImageData
{
    /// <summary>
    /// Initializes an image by copying row-major palette indices.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="pixels">One byte per pixel, even for bit depths below eight.</param>
    /// <param name="palette">The palette. Its size must fit the bit depth and contain every referenced index.</param>
    /// <param name="bitsPerPixel">The index bit depth: one, two, four or eight.</param>
    public IndexedImageData(int width, int height, byte[] pixels, PaletteData palette, int bitsPerPixel = 8) : base(width, height)
    {
        ArgumentNullException.ThrowIfNull(pixels);
        ArgumentNullException.ThrowIfNull(palette);
        ValidateBitsPerPixel(bitsPerPixel);
        if (pixels.Length != PixelCount)
        {
            throw new ArgumentException("The pixel count must match the image dimensions.", nameof(pixels));
        }
        if (palette.Colours.Count > 1 << bitsPerPixel)
        {
            throw new ArgumentException("The palette is too large for the pixel bit depth.", nameof(palette));
        }
        ValidatePixels(pixels, palette.Colours.Count);
        Pixels = Array.AsReadOnly([.. pixels]);
        Palette = palette;
        BitsPerPixel = bitsPerPixel;
    }

    /// <summary>
    /// Gets the unpacked row-major palette indices.
    /// </summary>
    public IReadOnlyList<byte> Pixels { get; }

    /// <summary>
    /// Gets the reusable palette.
    /// </summary>
    public PaletteData Palette { get; }

    /// <summary>
    /// Gets the pixel index bit depth.
    /// </summary>
    public int BitsPerPixel { get; }

    /// <inheritdoc />
    [Pure]
    public override Colour GetPixel(int x, int y) => Palette.Colours[Pixels[GetPixelOffset(x, y)]];

    internal static void ValidateBitsPerPixel(int bitsPerPixel)
    {
        if (bitsPerPixel is not (1 or 2 or 4 or 8))
        {
            throw new ArgumentOutOfRangeException(nameof(bitsPerPixel), bitsPerPixel, "The bit depth must be one, two, four or eight.");
        }
    }

    internal static void ValidatePixels(ReadOnlySpan<byte> pixels, int colourCount)
    {
        foreach (var pixel in pixels)
        {
            if (pixel >= colourCount)
            {
                throw new ArgumentException("A pixel index is outside the available colour range.", nameof(pixels));
            }
        }
    }
}