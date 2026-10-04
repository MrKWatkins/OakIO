namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for tilemap files whose concrete components represent their on-disk format.
/// </summary>
public abstract class TileMapFile : ResourceFile
{
    /// <summary>
    /// Initializes a tilemap file.
    /// </summary>
    /// <param name="format">The file format.</param>
    protected TileMapFile(TileMapFormat format)
        : base(format ?? throw new ArgumentNullException(nameof(format)))
    {
    }

    /// <summary>
    /// Gets the tilemap file format.
    /// </summary>
    public new TileMapFormat Format => (TileMapFormat)base.Format;

    /// <summary>
    /// Gets a shared convenience view derived from the file's concrete components.
    /// </summary>
    public abstract TileMapData TileMap { get; }
}