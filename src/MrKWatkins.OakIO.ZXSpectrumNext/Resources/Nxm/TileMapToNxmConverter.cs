using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxm;

/// <summary>
/// Converts maps to an explicit NXM layout without discarding indices or attributes.
/// </summary>
/// <param name="sourceFormat">The source map format.</param>
/// <param name="targetFormat">The target map layout.</param>
public sealed class TileMapToNxmConverter(TileMapFormat sourceFormat, NxmFormat targetFormat)
    : IOFileConverter<TileMapFile, NxmFile>(sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat)), targetFormat ?? throw new ArgumentNullException(nameof(targetFormat)))
{
    /// <inheritdoc />
    [Pure]
    public override NxmFile Convert(TileMapFile source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new NxmFile(source.TileMap, (NxmFormat)TargetFormat);
    }
}