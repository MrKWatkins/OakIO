using MrKWatkins.OakIO.Resources;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr;

/// <summary>
/// Converts tiles into 16 by 16 sprite patterns at an explicit bit depth, without remapping indices.
/// </summary>
/// <param name="sourceFormat">The source tiles format.</param>
/// <param name="targetFormat">The target format, including its explicit bit depth.</param>
public sealed class TilesToSprConverter(TilesFormat sourceFormat, SprFormat targetFormat)
    : IOFileConverter<TilesFile, SprFile>(
        sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat)),
        targetFormat ?? throw new ArgumentNullException(nameof(targetFormat)))
{
    /// <inheritdoc />
    [Pure]
    public override SprFile Convert(TilesFile source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new SprFile(PackedPixels.FromTiles(source.Tiles, 16, ((SprFormat)TargetFormat).BitsPerPixel));
    }
}