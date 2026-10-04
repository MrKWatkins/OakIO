using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Sl2;

/// <summary>
/// Native SL2 screen format with an explicitly selected mode.
/// </summary>
/// <remarks>
/// https://gitlab.com/varmfskii/showsimg/-/blob/c79616ac578fb9372c2b34a413f091290b549959/FORMAT.md
/// https://wiki.specnext.dev/File_Formats
/// </remarks>
public sealed class Sl2Format : ImageFormat<Sl2File>
{
    /// <summary>
    /// The standard screen mode.
    /// </summary>
    public static readonly Sl2Format Standard = new(256, 192, 8);
    /// <summary>
    /// 320 by 256 eight-bit screens.
    /// </summary>
    public static readonly Sl2Format Wide = new(320, 256, 8);
    /// <summary>
    /// 640 by 256 four-bit screens.
    /// </summary>
    public static readonly Sl2Format HighResolution = new(640, 256, 4);

    private Sl2Format(int width, int height, int bits) : base("ZX Spectrum Next Layer 2 Screen", "sl2")
    {
        Width = width;
        Height = height;
        BitsPerPixel = bits;
    }
    /// <summary>
    /// Gets the native width.
    /// </summary>
    public int Width { get; }
    /// <summary>
    /// Gets the native height.
    /// </summary>
    public int Height { get; }
    /// <summary>
    /// Gets the native bit depth.
    /// </summary>
    public int BitsPerPixel { get; }

    /// <summary>
    /// Selects an explicit Layer 2 screen mode.
    /// </summary>
    [Pure]
    public static Sl2Format ForDimensions(int width, int height) => (width, height) switch
    {
        (256, 192) => Standard,
        (320, 256) => Wide,
        (640, 256) => HighResolution,
        _ => throw new ArgumentException("Unsupported Layer 2 screen dimensions.")
    };

    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new Sl2File(await reader.ReadToEndAsync().ConfigureAwait(false), this);

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(Sl2File file, IBinaryWriter writer)
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