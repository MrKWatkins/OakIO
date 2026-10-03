namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for palette files.
/// </summary>
public abstract class PaletteFile : ResourceFile
{
    /// <summary>
    /// Initializes a palette file.
    /// </summary>
    /// <param name="format">The file format.</param>
    /// <param name="palette">The shared palette data.</param>
    protected PaletteFile(PaletteFormat format, PaletteData palette)
        : base(format ?? throw new ArgumentNullException(nameof(format)))
    {
        ArgumentNullException.ThrowIfNull(palette);
        Palette = palette;
    }

    /// <summary>
    /// Gets the palette file format.
    /// </summary>
    public new PaletteFormat Format => (PaletteFormat)base.Format;

    /// <summary>
    /// Gets the shared palette data.
    /// </summary>
    public PaletteData Palette { get; }
}