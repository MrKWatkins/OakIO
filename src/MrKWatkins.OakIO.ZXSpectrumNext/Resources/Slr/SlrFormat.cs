using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Slr;

/// <summary>
/// Native SLR screen format with an explicitly selected mode.
/// </summary>
/// <remarks>
/// https://gitlab.com/varmfskii/showsimg/-/blob/c79616ac578fb9372c2b34a413f091290b549959/FORMAT.md
/// https://wiki.specnext.dev/File_Formats
/// </remarks>
public sealed class SlrFormat : ImageFormat<SlrFile>
{
    /// <summary>
    /// The standard screen mode.
    /// </summary>
    public static readonly SlrFormat EightBit = new(8);
    /// <summary>
    /// Packed four-bit Radastan screens.
    /// </summary>
    public static readonly SlrFormat FourBit = new(4);

    private SlrFormat(int bits) : base("ZX Spectrum Next Low Resolution Screen", "slr")
    {
        BitsPerPixel = bits;
    }
    /// <summary>
    /// Gets the screen width.
    /// </summary>
    public int Width { get; } = 128;
    /// <summary>
    /// Gets the screen height.
    /// </summary>
    public int Height { get; } = 96;
    /// <summary>
    /// Gets the native bit depth.
    /// </summary>
    public int BitsPerPixel { get; }

    /// <summary>
    /// Selects an explicit SLR depth.
    /// </summary>
    [Pure]
    public static SlrFormat ForBitsPerPixel(int bits) => bits switch
    {
        4 => FourBit,
        8 => EightBit,
        _ => throw new ArgumentOutOfRangeException(nameof(bits))
    };

    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new SlrFile(await reader.ReadToEndAsync().ConfigureAwait(false), this);

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(SlrFile file, IBinaryWriter writer)
    {
        if (file.Format != this)
        {
            throw new ArgumentException("The screen mode does not match the writing format.", nameof(file));
        }
        if (file.Header != null)
        {
            await file.Header.WriteAsync(writer).ConfigureAwait(false);
        }
        await file.PixelData.WriteAsync(writer).ConfigureAwait(false);
        await file.Palette.WriteAsync(writer).ConfigureAwait(false);
    }

    /// <inheritdoc />
    [Pure]
    protected override IEnumerable<IOFileConverter> CreateConverters() => [new ImageToBmpConverter(this)];
}