using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxb;

/// <summary>
/// Converts block collections to an explicit NXB layout, rejecting unrepresentable indices or attributes.
/// </summary>
/// <param name="sourceFormat">The source block format.</param>
/// <param name="targetFormat">The target block layout.</param>
public sealed class TileBlocksToNxbConverter(TileBlocksFormat sourceFormat, NxbFormat targetFormat)
    : IOFileConverter<TileBlocksFile, NxbFile>(sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat)), targetFormat ?? throw new ArgumentNullException(nameof(targetFormat)))
{
    /// <inheritdoc />
    [Pure]
    public override NxbFile Convert(TileBlocksFile source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new NxbFile(source.Blocks, (NxbFormat)TargetFormat);
    }
}