namespace MrKWatkins.OakIO.Resources.Jasc;

/// <summary>
/// Converts palette colours to Jasc without reordering or quantising them.
/// </summary>
/// <param name="sourceFormat">The source palette format.</param>
public sealed class PaletteToJascConverter(PaletteFormat sourceFormat)
    : IOFileConverter<PaletteFile, JascFile>(sourceFormat ?? throw new ArgumentNullException(nameof(sourceFormat)), JascFormat.Instance)
{
    /// <inheritdoc />
    [Pure]
    public override JascFile Convert(PaletteFile source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return new JascFile(source.Palette);
    }
}