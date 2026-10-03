namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Immutable indexed tiles of a common size. Each tile contains unpacked row-major indices; palettes are separate.
/// </summary>
public sealed class TilesData
{
    /// <summary>
    /// Initializes tiles by copying their pixels.
    /// </summary>
    /// <param name="width">The width of each tile in pixels.</param>
    /// <param name="height">The height of each tile in pixels.</param>
    /// <param name="pixels">Tile-major pixels, one byte per index. The length must be a multiple of the tile size.</param>
    /// <param name="bitsPerPixel">The index bit depth: one, two, four or eight.</param>
    public TilesData(int width, int height, byte[] pixels, int bitsPerPixel = 8)
    {
        var pixelsPerTile = ImageData.ValidateDimensions(width, height);
        ArgumentNullException.ThrowIfNull(pixels);
        IndexedImageData.ValidateBitsPerPixel(bitsPerPixel);
        if (pixels.Length % pixelsPerTile != 0)
        {
            throw new ArgumentException("The pixel count must be a multiple of the tile size.", nameof(pixels));
        }
        IndexedImageData.ValidatePixels(pixels, 1 << bitsPerPixel);
        Width = width;
        Height = height;
        BitsPerPixel = bitsPerPixel;
        Count = pixels.Length / pixelsPerTile;
        Pixels = Array.AsReadOnly([.. pixels]);
    }

    /// <summary>
    /// Gets the width of each tile in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of each tile in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the pixel index bit depth.
    /// </summary>
    public int BitsPerPixel { get; }

    /// <summary>
    /// Gets the number of tiles.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Gets tile-major pixels, with row-major unpacked indices within each tile.
    /// </summary>
    public IReadOnlyList<byte> Pixels { get; }
}