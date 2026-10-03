using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt;

/// <summary>
/// Reads and writes headerless 8 by 8 tiles without guessing their bit depth.
/// </summary>
/// <remarks>
/// Format reference: https://github.com/benbaker76/Gfx2Next.
/// Native 8 by 8 tile layouts: https://wiki.specnext.dev/Tilemap.
/// </remarks>
public sealed class NxtFormat : TilesFormat<NxtFile>
{
    /// <summary>
    /// The 1-bit format; callers must select the intended interpretation explicitly.
    /// </summary>
    public static readonly NxtFormat OneBit = new(1);

    /// <summary>
    /// The 4-bit format; callers must select the intended interpretation explicitly.
    /// </summary>
    public static readonly NxtFormat FourBit = new(4);

    /// <summary>
    /// The 8-bit format; callers must select the intended interpretation explicitly.
    /// </summary>
    public static readonly NxtFormat EightBit = new(8);

    private NxtFormat(int bitsPerPixel) : base($"ZX Spectrum Next Tiles ({bitsPerPixel}-bit)", "nxt")
    {
        BitsPerPixel = bitsPerPixel;
    }
    /// <summary>
    /// Gets the bit depth used to interpret the headerless file.
    /// </summary>
    public int BitsPerPixel { get; }
    /// <summary>
    /// Gets the singleton for the specified bit depth.
    /// </summary>
    /// <param name="bitsPerPixel">The explicit interpretation: 1, 4, 8 bits per pixel.</param>
    /// <returns>The selected format.</returns>
    [Pure]
    public static NxtFormat ForBitsPerPixel(int bitsPerPixel) => bitsPerPixel switch
    {
        1 => OneBit,
        4 => FourBit,
        8 => EightBit,
        _ => throw new ArgumentOutOfRangeException(nameof(bitsPerPixel), bitsPerPixel, "Unsupported bit depth for this format.")
    };
    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new NxtFile(await reader.ReadToEndAsync().ConfigureAwait(false), this);
    /// <inheritdoc />
    protected override async ValueTask WriteAsync(NxtFile file, IBinaryWriter writer)
    {
        foreach (var entry in file.Entries)
        {
            await entry.WriteAsync(writer).ConfigureAwait(false);
        }
    }
}