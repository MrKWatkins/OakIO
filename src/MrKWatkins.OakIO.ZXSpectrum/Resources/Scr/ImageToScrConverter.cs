using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

/// <summary>
/// Converts exactly representable images to SCR without cropping, colour quantization or dithering.
/// </summary>
/// <param name="sourceFormat">The source image format.</param>
public sealed class ImageToScrConverter(ImageFormat sourceFormat)
    : IOFileConverter<ImageFile, ScrFile>(sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat)), ScrFormat.Instance)
{
    /// <inheritdoc />
    [Pure]
    public override ScrFile Convert(ImageFile source)
    {
        ArgumentNullException.ThrowIfNull(source);
        // Preserve attributes, especially FLASH, instead of flattening a screen to one image phase.
        return source is ScrFile scr ? new ScrFile(scr.ToByteArray()) : new ScrFile(source.Image);
    }
}