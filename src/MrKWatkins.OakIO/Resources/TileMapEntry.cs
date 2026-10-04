namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// A tile or block reference with optional palette, transformation and priority attributes.
/// </summary>
public readonly record struct TileMapEntry
{
    /// <summary>
    /// Initializes a map entry with a nonnegative index and palette offset.
    /// </summary>
    /// <param name="tileIndex">The index of the referenced tile or block.</param>
    /// <param name="paletteOffset">The palette offset; its unit and range depend on the target format.</param>
    public TileMapEntry(int tileIndex, int paletteOffset = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(tileIndex);
        ArgumentOutOfRangeException.ThrowIfNegative(paletteOffset);
        TileIndex = tileIndex;
        PaletteOffset = paletteOffset;
    }

    /// <summary>
    /// Gets the referenced tile or block index.
    /// </summary>
    public int TileIndex { get; }
    /// <summary>
    /// Gets the palette offset, interpreted in the target format's units.
    /// </summary>
    public int PaletteOffset { get; }
    /// <summary>
    /// Gets whether to mirror horizontally.
    /// </summary>
    public bool MirrorX { get; init; }
    /// <summary>
    /// Gets whether to mirror vertically.
    /// </summary>
    public bool MirrorY { get; init; }
    /// <summary>
    /// Gets whether to rotate clockwise by 90 degrees.
    /// </summary>
    public bool Rotate { get; init; }
    /// <summary>
    /// Gets the priority flag; the target format defines its relationship to other layers.
    /// </summary>
    public bool Priority { get; init; }
}