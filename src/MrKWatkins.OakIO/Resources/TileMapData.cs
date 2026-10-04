namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// A rectangular map of tile or block references in top-left-origin row-major order.
/// </summary>
public sealed class TileMapData
{
    /// <summary>
    /// Initializes a map by copying its entries.
    /// </summary>
    /// <param name="width">The positive width in entries, not pixels.</param>
    /// <param name="height">The positive height in entries, not pixels.</param>
    /// <param name="entries">Exactly width times height row-major entries.</param>
    public TileMapData(int width, int height, TileMapEntry[] entries)
    {
        var count = ImageData.ValidateDimensions(width, height);
        ArgumentNullException.ThrowIfNull(entries);
        if (entries.Length != count)
        {
            throw new ArgumentException("The entry count must match the map dimensions.", nameof(entries));
        }
        Width = width;
        Height = height;
        Entries = [.. entries];
    }

    /// <summary>
    /// Gets the width in entries.
    /// </summary>
    public int Width { get; }
    /// <summary>
    /// Gets the height in entries.
    /// </summary>
    public int Height { get; }
    /// <summary>
    /// Gets the ordered map entries.
    /// </summary>
    public IReadOnlyList<TileMapEntry> Entries { get; }

    /// <summary>
    /// Gets an entry at top-left-origin map coordinates.
    /// </summary>
    /// <param name="x">The horizontal coordinate.</param>
    /// <param name="y">The vertical coordinate.</param>
    /// <returns>The map entry.</returns>
    [Pure]
    public TileMapEntry GetEntry(int x, int y)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, Width);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, Height);
        return Entries[y * Width + x];
    }
}