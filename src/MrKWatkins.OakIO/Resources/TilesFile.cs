namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for tiles files.
/// </summary>
public abstract class TilesFile : ResourceFile
{
    /// <summary>
    /// Initializes a tiles file.
    /// </summary>
    /// <param name="format">The file format.</param>
    /// <param name="tiles">The shared tiles data.</param>
    protected TilesFile(TilesFormat format, TilesData tiles)
        : base(format ?? throw new ArgumentNullException(nameof(format)))
    {
        ArgumentNullException.ThrowIfNull(tiles);
        Tiles = tiles;
    }

    /// <summary>
    /// Gets the tiles file format.
    /// </summary>
    public new TilesFormat Format => (TilesFormat)base.Format;

    /// <summary>
    /// Gets the shared tiles data.
    /// </summary>
    public TilesData Tiles { get; }
}