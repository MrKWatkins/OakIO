using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;
namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr;

/// <summary>
/// Reads and writes headerless 16 by 16 sprite patterns without guessing their bit depth.
/// </summary>
/// <remarks>
/// Format reference: https://wiki.specnext.dev/Sprite_Pattern_Upload.
/// </remarks>
public sealed class SprFormat : TilesFormat<SprFile>
{
    /// <summary>
    /// The 4-bit format; callers must select the intended interpretation explicitly.
    /// </summary>
    public static readonly SprFormat FourBit = new(4);

    /// <summary>
    /// The 8-bit format; callers must select the intended interpretation explicitly.
    /// </summary>
    public static readonly SprFormat EightBit = new(8);

    private SprFormat(int bitsPerPixel) : base($"ZX Spectrum Next Sprites ({bitsPerPixel}-bit)", "spr")
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
    /// <param name="bitsPerPixel">The explicit interpretation: 4, 8 bits per pixel.</param>
    /// <returns>The selected format.</returns>
    [Pure]
    public static SprFormat ForBitsPerPixel(int bitsPerPixel) => bitsPerPixel switch
    {
        4 => FourBit,
        8 => EightBit,
        _ => throw new ArgumentOutOfRangeException(nameof(bitsPerPixel), bitsPerPixel, "Unsupported bit depth for this format.")
    };
    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new SprFile(await reader.ReadToEndAsync().ConfigureAwait(false), this);
    /// <inheritdoc />
    protected override async ValueTask WriteAsync(SprFile file, IBinaryWriter writer)
    {
        foreach (var entry in file.Patterns)
        {
            await entry.WriteAsync(writer).ConfigureAwait(false);
        }
    }
}