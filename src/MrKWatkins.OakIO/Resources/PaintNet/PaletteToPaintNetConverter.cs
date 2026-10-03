namespace MrKWatkins.OakIO.Resources.PaintNet;

/// <summary>
/// Converts palette colours to PaintNet without reordering or quantizing them.
/// </summary>
/// <param name="sourceFormat">The source palette format.</param>
public sealed class PaletteToPaintNetConverter(PaletteFormat sourceFormat)
    : IOFileConverter<PaletteFile, PaintNetFile>(sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat)), PaintNetFormat.Instance)
{
    /// <inheritdoc />
    [Pure]
    public override PaintNetFile Convert(PaletteFile source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new PaintNetFile(source.Palette);
    }
}