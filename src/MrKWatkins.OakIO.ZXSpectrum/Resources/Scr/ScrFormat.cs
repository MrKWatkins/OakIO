using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Bmp;

namespace MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

/// <summary>
/// Reads and writes standard SCR screens; extended Timex and ULAplus variants are not supported.
/// </summary>
/// <remarks>
/// Format reference: https://worldofspectrum.org/faq/reference/formats.htm.
/// </remarks>
public sealed class ScrFormat : ImageFormat<ScrFile>
{
    /// <summary>
    /// The singleton SCR format.
    /// </summary>
    public static readonly ScrFormat Instance = new();
    private ScrFormat() : base("ZX Spectrum Screen", "scr")
    {
    }
    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new ScrFile(await reader.ReadToEndAsync().ConfigureAwait(false));
    /// <inheritdoc />
    protected override async ValueTask WriteAsync(ScrFile file, IBinaryWriter writer)
    {
        await file.Bitmap.WriteAsync(writer).ConfigureAwait(false);
        await file.Attributes.WriteAsync(writer).ConfigureAwait(false);
    }
    /// <inheritdoc />
    [Pure]
    protected override IEnumerable<IOFileConverter> CreateConverters() => [new ImageToBmpConverter(this)];
}