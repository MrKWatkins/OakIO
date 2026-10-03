using MrKWatkins.OakIO.Resources;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr;

/// <summary>
/// A headerless collection of 16 by 16 sprite patterns at an explicit bit depth.
/// </summary>
/// <remarks>
/// Format reference: https://wiki.specnext.dev/Sprite_Pattern_Upload.
/// </remarks>
public sealed class SprFile : TilesFile
{
    /// <summary>
    /// Creates a file from unpacked tiles, preserving their bit depth and indices.
    /// </summary>
    /// <param name="tiles">The 16 by 16 tiles, including their bit depth.</param>
    public SprFile(TilesData tiles) : this(Encode(tiles), SprFormat.ForBitsPerPixel(tiles.BitsPerPixel))
    {
    }

    internal SprFile(byte[] bytes, SprFormat format) : base(format)
    {
        var bytesPerTile = 256 * format.BitsPerPixel / 8;
        if (bytes.Length % bytesPerTile != 0)
        {
            throw new InvalidDataException("The file must contain complete 16 by 16 sprite patterns.");
        }
        Patterns = [.. bytes.Chunk(bytesPerTile).Select(data => new SprPattern(data, format.BitsPerPixel))];
    }
    /// <summary>
    /// Gets the format and its explicit bit depth.
    /// </summary>
    public new SprFormat Format => (SprFormat)base.Format;
    /// <summary>
    /// Gets the ordered byte-backed sprite patterns.
    /// </summary>
    public IReadOnlyList<SprPattern> Patterns { get; }
    /// <inheritdoc />
    public override TilesData Tiles => new(16, 16, [.. Patterns.SelectMany(entry => entry.Pixels)], Format.BitsPerPixel);

    [Pure]
    private static byte[] Encode(TilesData tiles)
    {
        ArgumentNullException.ThrowIfNull(tiles);
        var format = SprFormat.ForBitsPerPixel(tiles.BitsPerPixel);
        return PackedPixels.Pack(PackedPixels.FromTiles(tiles, 16, format.BitsPerPixel).Pixels, format.BitsPerPixel);
    }
}