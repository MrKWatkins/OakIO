using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources;
using MrKWatkins.OakIO.Resources.Gpl;
using MrKWatkins.OakIO.Resources.Jasc;
using MrKWatkins.OakIO.Resources.PaintNet;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;

/// <summary>
/// Reads and writes headerless NXP palettes with 16 or 256 RGB333 entries.
/// </summary>
/// <remarks>
/// Format reference: https://github.com/stefanbylund/zxnext_bmp_tools#nextraw.
/// </remarks>
public sealed class NxpFormat : PaletteFormat<NxpFile>
{
    /// <summary>
    /// The singleton NXP format.
    /// </summary>
    public static readonly NxpFormat Instance = new();

    private NxpFormat() : base("ZX Spectrum Next Palette", "nxp")
    {
    }

    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new NxpFile(await reader.ReadToEndAsync().ConfigureAwait(false));

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(NxpFile file, IBinaryWriter writer)
    {
        foreach (var entry in file.Entries)
        {
            await entry.WriteAsync(writer).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    [Pure]
    protected override IEnumerable<IOFileConverter> CreateConverters() =>
        [new PaletteToGplConverter(this), new PaletteToJascConverter(this), new PaletteToPaintNetConverter(this)];
}