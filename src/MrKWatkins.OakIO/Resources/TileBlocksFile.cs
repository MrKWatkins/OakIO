namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Base class for tile blocks files whose concrete components represent their on-disk format.
/// </summary>
public abstract class TileBlocksFile : ResourceFile
{
    /// <summary>
    /// Initializes a tile blocks file.
    /// </summary>
    /// <param name="format">The file format.</param>
    protected TileBlocksFile(TileBlocksFormat format)
        : base(format ?? throw new ArgumentNullException(nameof(format)))
    {
    }

    /// <summary>
    /// Gets the tile blocks file format.
    /// </summary>
    public new TileBlocksFormat Format => (TileBlocksFormat)base.Format;

    /// <summary>
    /// Gets a shared convenience view derived from the file's concrete components.
    /// </summary>
    public abstract TileBlocksData Blocks { get; }
}