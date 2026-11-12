namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for tiles files whose concrete components represent their on-disk format.
/// </summary>
/// <param name="format">The file format.</param>
public abstract class TilesFile(TilesFormat format) : ResourceFile(format ?? throw new ArgumentNullException(nameof(format)))
{

    /// <summary>
    /// Gets the tiles file format.
    /// </summary>
    public new TilesFormat Format => (TilesFormat)base.Format;

    /// <summary>
    /// Gets a shared convenience view derived from the file's concrete components.
    /// </summary>
    public abstract TilesData Tiles { get; }
}