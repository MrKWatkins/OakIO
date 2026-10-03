using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxi;

/// <summary>
/// A headerless native Layer 2 screen with a prepended RGB333 palette and byte-backed bitmap.
/// </summary>
/// <remarks>
/// Format reference: https://github.com/stefanbylund/zxnext_bmp_tools#nextraw.
/// </remarks>
public sealed class NxiFile : ImageFile
{
    /// <summary>
    /// Creates an NXI image, preserving palette order and indices and rounding colours to RGB333.
    /// </summary>
    /// <param name="image">An indexed Layer 2-sized image with exactly 16 or 256 opaque palette entries, as required by its mode.</param>
    public NxiFile(IndexedImageData image) : base(NxiFormat.Instance)
    {
        ArgumentNullException.ThrowIfNull(image);
        var bits = NxiFormat.GetBitsPerPixel(image.Width, image.Height);
        if (image.Palette.Colours.Count != 1 << bits)
        {
            throw new ArgumentException($"This NXI screen requires exactly {1 << bits} palette entries.", nameof(image));
        }
        Palette = new NxpFile(image.Palette);
        PixelData = new NxiPixelData(image);
    }

    internal NxiFile(byte[] bytes) : base(NxiFormat.Instance)
    {
        var (width, height) = NxiFormat.GetDimensions(bytes.Length);
        var paletteBytes = (1 << NxiFormat.GetBitsPerPixel(width, height)) * 2;
        Palette = new NxpFile(bytes[..paletteBytes]);
        PixelData = new NxiPixelData(bytes[paletteBytes..], width, height);
    }

    /// <summary>
    /// Gets the NXI format.
    /// </summary>
    public new NxiFormat Format => (NxiFormat)base.Format;

    /// <summary>
    /// Gets the embedded palette in NXP encoding, retaining every original entry byte.
    /// </summary>
    public NxpFile Palette { get; }

    /// <summary>
    /// Gets the bitmap in native screen-memory order and packing.
    /// </summary>
    public NxiPixelData PixelData { get; }

    /// <inheritdoc />
    public override ImageData Image => new IndexedImageData(PixelData.Width, PixelData.Height, [.. PixelData.Pixels], Palette.Palette, PixelData.BitsPerPixel);
}