using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrum.Plus3Dos;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;

/// <summary>
/// Shared native screen components: optional +3DOS header, bitmap, then optional palette.
/// </summary>
public abstract class ScreenFile : ImageFile
{
    private protected ScreenFile(ImageFormat format, byte[] bytes, int width, int height, int bits) : base(format)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        var offset = 0;
        var length = width * height * bits / 8;
        var paletteLength = bytes.Length - length;
        var rawLength = paletteLength == 0 || paletteLength == 1 << bits || paletteLength == 2 << bits;
        // A valid raw bitmap can begin with the header signature; its exact raw layout takes precedence.
        if (!rawLength && bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual("PLUS3DOS"u8))
        {
            if (bytes.Length < 128)
            {
                throw new InvalidDataException("Truncated +3DOS header.");
            }
            Header = new Plus3DosHeader(bytes[..128]);
            if (Header.FileSize != bytes.Length)
            {
                throw new InvalidDataException("The +3DOS file size does not match the screen file.");
            }
            offset = 128;
        }
        if (bytes.Length - offset < length)
        {
            throw new InvalidDataException("Truncated screen bitmap.");
        }
        PixelData = new ScreenPixelData(bytes[offset..(offset + length)], width, height, bits);
        Palette = new ScreenPalette(bytes[(offset + length)..], 1 << bits);
    }

    private protected ScreenFile(ImageFormat format, IndexedImageData image, int width, int height, int bits,
        ScreenPaletteEncoding paletteEncoding, bool includeHeader) : base(format)
    {
        PixelData = ScreenPixelData.Create(image, width, height, bits);
        Palette = ScreenPalette.Create(image.Palette, 1 << bits, paletteEncoding);
        if (includeHeader)
        {
            Header = Plus3DosHeader.Create(PixelData.Length + Palette.Length);
        }
    }

    /// <summary>
    /// Gets the optional native +3DOS header.
    /// </summary>
    public Plus3DosHeader? Header { get; }
    /// <summary>
    /// Gets native pixel bytes.
    /// </summary>
    public ScreenPixelData PixelData { get; }
    /// <summary>
    /// Gets the appended palette, or a zero-byte component exposing default colours.
    /// </summary>
    public ScreenPalette Palette { get; }
    /// <inheritdoc />
    public override ImageData Image => new IndexedImageData(PixelData.Width, PixelData.Height, [.. PixelData.Pixels], Palette.Palette, PixelData.BitsPerPixel);
}