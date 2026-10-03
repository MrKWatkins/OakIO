namespace MrKWatkins.OakIO.Resources.Bmp;

/// <summary>
/// A Windows BMP image retaining its headers, colour table and stored pixel rows.
/// </summary>
public sealed class BmpFile : ImageFile
{
    private readonly byte[] gap;
    private readonly byte[] trailingData;

    /// <summary>
    /// Creates a BMP file from opaque RGB colours or four/eight-bit indexed pixels.
    /// </summary>
    /// <param name="image">The image data.</param>
    /// <param name="isTopDown">Whether to store rows top-down rather than the default bottom-up.</param>
    public BmpFile(ImageData image, bool isTopDown = false)
        : this(BmpFormat.CreateBytes(image, isTopDown))
    {
    }

    internal BmpFile(byte[] data) : base(BmpFormat.Instance)
    {
        Header = new BmpHeader(data[..14]);
        InformationHeader = new BmpInformationHeader(data[14..54]);
        var colourCount = InformationHeader.BitsPerPixel == 24 ? (int)InformationHeader.ColoursUsed
            : InformationHeader.ColoursUsed == 0 ? 1 << InformationHeader.BitsPerPixel : (int)InformationHeader.ColoursUsed;
        var paletteEnd = 54 + colourCount * 4;
        Palette = colourCount == 0 ? null : new BmpPalette(data[54..paletteEnd]);
        var pixelOffset = (int)Header.PixelDataOffset;
        gap = data[paletteEnd..pixelOffset];
        var pixelEnd = pixelOffset + InformationHeader.RowStride * Math.Abs(InformationHeader.Height);
        PixelData = new BmpPixelData(data[pixelOffset..pixelEnd]);
        trailingData = data[pixelEnd..];
    }

    /// <summary>
    /// Gets the BMP format.
    /// </summary>
    public new BmpFormat Format => (BmpFormat)base.Format;

    /// <summary>
    /// Gets the file header.
    /// </summary>
    public BmpHeader Header { get; }

    /// <summary>
    /// Gets the BITMAPINFOHEADER.
    /// </summary>
    public BmpInformationHeader InformationHeader { get; }

    /// <summary>
    /// Gets the stored colour table, or null for images without a colour table.
    /// </summary>
    public BmpPalette? Palette { get; }

    /// <summary>
    /// Gets the stored packed pixel rows, including padding.
    /// </summary>
    public BmpPixelData PixelData { get; }

    /// <summary>
    /// Gets the retained bytes between the colour table and pixel data.
    /// </summary>
    public IReadOnlyList<byte> Gap => gap;

    internal ReadOnlyMemory<byte> GapMemory => gap;

    /// <summary>
    /// Gets the retained bytes following the pixel rows, including any bytes beyond the declared file size.
    /// </summary>
    public IReadOnlyList<byte> TrailingData => trailingData;

    internal ReadOnlyMemory<byte> TrailingDataMemory => trailingData;

    /// <inheritdoc />
    public override ImageData Image => PixelData.GetImage(InformationHeader, Palette);

    /// <summary>
    /// Gets the stored pixel bit depth.
    /// </summary>
    public int BitsPerPixel => InformationHeader.BitsPerPixel;

    /// <summary>
    /// Gets whether rows are stored top-down.
    /// </summary>
    public bool IsTopDown => InformationHeader.IsTopDown;

    [Pure]
    internal static int ValidateImage(ImageData image)
    {
        ArgumentNullException.ThrowIfNull(image);
        var colours = image switch
        {
            ColourImageData colourImage => colourImage.Pixels,
            IndexedImageData { BitsPerPixel: 4 or 8 } indexedImage => indexedImage.Palette.Colours,
            _ => throw new NotSupportedException("BMP supports colour images and four/eight-bit indexed images.")
        };
        foreach (var colour in colours)
        {
            if (colour.Alpha != 255)
            {
                throw new ArgumentException("BMP does not support alpha; all colours must be opaque.", nameof(image));
            }
        }
        return image is IndexedImageData indexed ? indexed.BitsPerPixel : 24;
    }
}