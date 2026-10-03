using MrKWatkins.OakIO.Resources;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt;

/// <summary>
/// A headerless collection of 8 by 8 tiles at an explicit bit depth.
/// </summary>
/// <remarks>
/// Format reference: https://github.com/benbaker76/Gfx2Next.
/// Native 8 by 8 tile layouts: https://wiki.specnext.dev/Tilemap.
/// </remarks>
public sealed class NxtFile : TilesFile
{
    /// <summary>
    /// Creates a file from unpacked tiles, preserving their bit depth and indices.
    /// </summary>
    /// <param name="tiles">The 8 by 8 tiles, including their bit depth.</param>
    public NxtFile(TilesData tiles) : this(Encode(tiles), NxtFormat.ForBitsPerPixel(tiles.BitsPerPixel))
    {
    }

    internal NxtFile(byte[] bytes, NxtFormat format) : base(format)
    {
        var bytesPerTile = 64 * format.BitsPerPixel / 8;
        if (bytes.Length % bytesPerTile != 0)
        {
            throw new InvalidDataException("The file must contain complete 8 by 8 tiles.");
        }
        Entries = [.. bytes.Chunk(bytesPerTile).Select(data => new NxtTile(data, format.BitsPerPixel))];
    }
    /// <summary>
    /// Gets the format and its explicit bit depth.
    /// </summary>
    public new NxtFormat Format => (NxtFormat)base.Format;
    /// <summary>
    /// Gets the ordered byte-backed tiles.
    /// </summary>
    public IReadOnlyList<NxtTile> Entries { get; }
    /// <inheritdoc />
    public override TilesData Tiles => new(8, 8, [.. Entries.SelectMany(entry => entry.Pixels)], Format.BitsPerPixel);

    [Pure]
    private static byte[] Encode(TilesData tiles)
    {
        ArgumentNullException.ThrowIfNull(tiles);
        var format = NxtFormat.ForBitsPerPixel(tiles.BitsPerPixel);
        return PackedPixels.Pack(PackedPixels.FromTiles(tiles, 8, format.BitsPerPixel).Pixels, format.BitsPerPixel);
    }
}