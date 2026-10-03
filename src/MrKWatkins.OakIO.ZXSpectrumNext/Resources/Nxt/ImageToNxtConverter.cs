using MrKWatkins.OakIO.Resources;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt;

/// <summary>
/// Converts an indexed image into 8 by 8 tiles at an explicit bit depth, without remapping indices.
/// </summary>
/// <param name="sourceFormat">The source image format.</param>
/// <param name="targetFormat">The target format, including its explicit bit depth.</param>
public sealed class ImageToNxtConverter(ImageFormat sourceFormat, NxtFormat targetFormat)
    : IOFileConverter<ImageFile, NxtFile>(
        sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat)),
        targetFormat ?? throw new ArgumentNullException(nameof(targetFormat)))
{
    /// <inheritdoc />
    [Pure]
    public override NxtFile Convert(ImageFile source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new NxtFile(PackedPixels.FromImage(source, 8, ((NxtFormat)TargetFormat).BitsPerPixel));
    }
}