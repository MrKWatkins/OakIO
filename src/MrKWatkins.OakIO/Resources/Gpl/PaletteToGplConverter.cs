namespace MrKWatkins.OakIO.Resources.Gpl;

/// <summary>
/// Converts palette colours to Gpl without reordering or quantising them.
/// </summary>
/// <param name="sourceFormat">The source palette format.</param>
public sealed class PaletteToGplConverter(PaletteFormat sourceFormat)
    : IOFileConverter<PaletteFile, GplFile>(sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat)), GplFormat.Instance)
{
    /// <inheritdoc />
    [Pure]
    public override GplFile Convert(PaletteFile source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new GplFile(source.Palette);
    }
}