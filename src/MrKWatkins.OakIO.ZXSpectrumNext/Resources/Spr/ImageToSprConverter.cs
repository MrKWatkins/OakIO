using MrKWatkins.OakIO.Resources;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr;

/// <summary>
/// Converts an indexed image into 16 by 16 sprite patterns at an explicit bit depth, without remapping indices.
/// </summary>
/// <param name="sourceFormat">The source image format.</param>
/// <param name="targetFormat">The target format, including its explicit bit depth.</param>
public sealed class ImageToSprConverter(ImageFormat sourceFormat, SprFormat targetFormat)
    : IOFileConverter<ImageFile, SprFile>(
        sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat)),
        targetFormat ?? throw new ArgumentNullException(nameof(targetFormat)))
{
    /// <inheritdoc />
    [Pure]
    public override SprFile Convert(ImageFile source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new SprFile(PackedPixels.FromImage(source, 16, ((SprFormat)TargetFormat).BitsPerPixel));
    }
}