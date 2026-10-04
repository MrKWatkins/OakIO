using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Slr;

/// <summary>
/// Explicitly converts indexed images to a selected SLR mode, palette encoding and header variant.
/// </summary>
public sealed class ImageToSlrConverter : IOFileConverter<ImageFile, SlrFile>
{
    private readonly ScreenPaletteEncoding paletteEncoding;
    private readonly bool includeHeader;

    /// <summary>
    /// Creates a converter without implicit palette generation, resizing or reindexing.
    /// </summary>
    public ImageToSlrConverter(ImageFormat sourceFormat, SlrFormat targetFormat,
        ScreenPaletteEncoding paletteEncoding = ScreenPaletteEncoding.Rgb333, bool includeHeader = false)
        : base(sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat)), targetFormat ?? throw new ArgumentNullException(nameof(targetFormat)))
    {
        if (!Enum.IsDefined(paletteEncoding))
        {
            throw new ArgumentOutOfRangeException(nameof(paletteEncoding));
        }
        this.paletteEncoding = paletteEncoding;
        this.includeHeader = includeHeader;
    }

    /// <inheritdoc />
    [Pure]
    public override SlrFile Convert(ImageFile source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return source.Image is IndexedImageData indexed
            ? new SlrFile(indexed, (SlrFormat)TargetFormat, paletteEncoding, includeHeader)
            : throw new NotSupportedException("Native screen conversion requires an indexed image; palette generation is separate.");
    }
}