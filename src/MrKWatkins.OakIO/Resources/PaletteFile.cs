namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for palette files whose concrete components represent their on-disk format.
/// </summary>
public abstract class PaletteFile : ResourceFile
{
    /// <summary>
    /// Initializes a palette file.
    /// </summary>
    /// <param name="format">The file format.</param>
    protected PaletteFile(PaletteFormat format)
        : base(format ?? throw new ArgumentNullException(nameof(format)))
    {
    }

    /// <summary>
    /// Gets the palette file format.
    /// </summary>
    public new PaletteFormat Format => (PaletteFormat)base.Format;

    /// <summary>
    /// Gets a shared convenience view derived from the file's concrete components.
    /// </summary>
    public abstract PaletteData Palette { get; }
}