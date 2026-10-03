using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxi;

/// <summary>
/// Reads and writes palette-prefixed NXI images in the three native Layer 2 screen layouts.
/// </summary>
/// <remarks>
/// Format reference: https://github.com/stefanbylund/zxnext_bmp_tools#nextraw.
/// Hardware layouts: https://wiki.specnext.dev/Layer_2.
/// Headerless custom-size, custom-order and palette-free exports are not interpreted by this format.
/// </remarks>
public sealed class NxiFormat : ImageFormat<NxiFile>
{
    /// <summary>
    /// The singleton NXI format.
    /// </summary>
    public static readonly NxiFormat Instance = new();

    private NxiFormat() : base("ZX Spectrum Next Image", "nxi")
    {
    }

    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new NxiFile(await reader.ReadToEndAsync().ConfigureAwait(false));

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(NxiFile file, IBinaryWriter writer)
    {
        foreach (var entry in file.Palette.Entries)
        {
            await entry.WriteAsync(writer).ConfigureAwait(false);
        }
        await file.PixelData.WriteAsync(writer).ConfigureAwait(false);
    }

    /// <inheritdoc />
    [Pure]
    protected override IEnumerable<IOFileConverter> CreateConverters() => [new ImageToBmpConverter(this)];

    [Pure]
    internal static (int Width, int Height) GetDimensions(int length) => length switch
    {
        49664 => (256, 192),
        82432 => (320, 256),
        81952 => (640, 256),
        _ => throw new InvalidDataException("An NXI screen must contain 49664, 82432 or 81952 bytes, including its palette.")
    };

    [Pure]
    internal static int GetBitsPerPixel(int width, int height) => (width, height) switch
    {
        (256, 192) or (320, 256) => 8,
        (640, 256) => 4,
        _ => throw new ArgumentException("NXI supports 256 by 192, 320 by 256 and 640 by 256 Layer 2 screens only.")
    };
}