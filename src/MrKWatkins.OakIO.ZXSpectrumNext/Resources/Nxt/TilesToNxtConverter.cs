using MrKWatkins.OakIO.Resources;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt;

/// <summary>
/// Converts tiles into 8 by 8 tiles at an explicit bit depth, without remapping indices.
/// </summary>
/// <param name="sourceFormat">The source tiles format.</param>
/// <param name="targetFormat">The target format, including its explicit bit depth.</param>
public sealed class TilesToNxtConverter(TilesFormat sourceFormat, NxtFormat targetFormat)
    : IOFileConverter<TilesFile, NxtFile>(
        sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat)),
        targetFormat ?? throw new ArgumentNullException(nameof(targetFormat)))
{
    /// <inheritdoc />
    [Pure]
    public override NxtFile Convert(TilesFile source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new NxtFile(PackedPixels.FromTiles(source.Tiles, 8, ((NxtFormat)TargetFormat).BitsPerPixel));
    }
}