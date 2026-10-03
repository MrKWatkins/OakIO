using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxi;

/// <summary>
/// Converts indexed images to palette-prefixed NXI without resizing, generating a palette or remapping indices.
/// </summary>
/// <param name="sourceFormat">The source image format.</param>
public sealed class ImageToNxiConverter(ImageFormat sourceFormat)
    : IOFileConverter<ImageFile, NxiFile>(sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat)), NxiFormat.Instance)
{
    /// <inheritdoc />
    [Pure]
    public override NxiFile Convert(ImageFile source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source is NxiFile nxi)
        {
            return new NxiFile(nxi.ToByteArray());
        }
        return source.Image is IndexedImageData indexed
            ? new NxiFile(indexed)
            : throw new NotSupportedException("NXI conversion requires an indexed image; palette generation is a separate operation.");
    }
}