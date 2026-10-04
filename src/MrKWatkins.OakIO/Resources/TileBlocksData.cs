namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Equally sized rectangular groups of map entries, stored block-major with row-major entries within each block.
/// </summary>
public sealed class TileBlocksData
{
    /// <summary>
    /// Initializes a collection of tile blocks by copying the entries.
    /// </summary>
    /// <param name="width">The positive width of each block in map entries.</param>
    /// <param name="height">The positive height of each block in map entries.</param>
    /// <param name="entries">Block-major entries; the length must be a multiple of the block area. Empty collections are allowed.</param>
    public TileBlocksData(int width, int height, TileMapEntry[] entries)
    {
        var entriesPerBlock = ImageData.ValidateDimensions(width, height);
        ArgumentNullException.ThrowIfNull(entries);
        if (entries.Length % entriesPerBlock != 0)
        {
            throw new ArgumentException("The entry count must be a multiple of the block area.", nameof(entries));
        }
        Width = width;
        Height = height;
        Count = entries.Length / entriesPerBlock;
        Entries = [.. entries];
    }

    /// <summary>
    /// Gets the width of each block in entries.
    /// </summary>
    public int Width { get; }
    /// <summary>
    /// Gets the height of each block in entries.
    /// </summary>
    public int Height { get; }
    /// <summary>
    /// Gets the number of blocks.
    /// </summary>
    public int Count { get; }
    /// <summary>
    /// Gets block-major entries, with row-major ordering within each block.
    /// </summary>
    public IReadOnlyList<TileMapEntry> Entries { get; }

    /// <summary>
    /// Gets a block as an independent row-major map view.
    /// </summary>
    /// <param name="index">The zero-based block index.</param>
    /// <returns>The selected map of entries.</returns>
    [Pure]
    public TileMapData GetBlock(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
        var area = Width * Height;
        return new TileMapData(Width, Height, [.. Entries.Skip(index * area).Take(area)]);
    }
}