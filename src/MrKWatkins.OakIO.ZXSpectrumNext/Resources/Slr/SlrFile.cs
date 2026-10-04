using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Slr;

/// <summary>
/// A native SLR screen, retaining its optional header, bitmap and appended palette.
/// </summary>
public sealed class SlrFile : ScreenFile
{
    /// <summary>
    /// Creates a screen without resizing or reindexing. Palette colours are rounded to the selected hardware encoding.
    /// </summary>
    public SlrFile(IndexedImageData image, SlrFormat format,
        ScreenPaletteEncoding paletteEncoding = ScreenPaletteEncoding.Rgb333, bool includeHeader = false)
        : base(format ?? throw new ArgumentNullException(nameof(format)), image, format.Width, format.Height, format.BitsPerPixel, paletteEncoding, includeHeader)
    {
    }

    internal SlrFile(byte[] bytes, SlrFormat format) : base(format, bytes, format.Width, format.Height, format.BitsPerPixel)
    {
    }

    /// <summary>
    /// Gets the explicit screen mode.
    /// </summary>
    public new SlrFormat Format => (SlrFormat)base.Format;
}