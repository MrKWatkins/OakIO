namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for tilemap files whose concrete components represent their on-disk format.
/// </summary>
/// <param name="format">The file format.</param>
public abstract class TileMapFile(TileMapFormat format) : ResourceFile(format ?? throw new ArgumentNullException(nameof(format)))
{

    /// <summary>
    /// Gets the tilemap file format.
    /// </summary>
    public new TileMapFormat Format => (TileMapFormat)base.Format;

    /// <summary>
    /// Gets a shared convenience view derived from the file's concrete components.
    /// </summary>
    public abstract TileMapData TileMap { get; }
}