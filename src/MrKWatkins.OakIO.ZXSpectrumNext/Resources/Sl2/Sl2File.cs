using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Sl2;

/// <summary>
/// A native SL2 screen, retaining its optional header, bitmap and appended palette.
/// </summary>
public sealed class Sl2File : ScreenFile
{
    /// <summary>
    /// Creates a screen without resizing or reindexing. Palette colours are rounded to the selected hardware encoding.
    /// </summary>
    public Sl2File(IndexedImageData image, Sl2Format format,
        ScreenPaletteEncoding paletteEncoding = ScreenPaletteEncoding.Rgb333, bool includeHeader = false)
        : base(format ?? throw new ArgumentNullException(nameof(format)), image, format.Width, format.Height, format.BitsPerPixel, paletteEncoding, includeHeader)
    {
    }

    internal Sl2File(byte[] bytes, Sl2Format format) : base(format, bytes, format.Width, format.Height, format.BitsPerPixel)
    {
    }

    /// <summary>
    /// Gets the explicit screen mode.
    /// </summary>
    public new Sl2Format Format => (Sl2Format)base.Format;
}