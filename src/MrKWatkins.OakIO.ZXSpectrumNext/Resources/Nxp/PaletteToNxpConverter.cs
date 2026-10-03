using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;

/// <summary>
/// Converts an opaque palette with 16 or 256 entries to RGB333 without changing entry order.
/// </summary>
/// <param name="sourceFormat">The source palette format.</param>
public sealed class PaletteToNxpConverter(PaletteFormat sourceFormat)
    : IOFileConverter<PaletteFile, NxpFile>(sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat)), NxpFormat.Instance)
{
    /// <inheritdoc />
    [Pure]
    public override NxpFile Convert(PaletteFile source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new NxpFile(source.Palette);
    }
}