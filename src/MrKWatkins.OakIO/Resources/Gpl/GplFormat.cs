using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources.Jasc;
using MrKWatkins.OakIO.Resources.PaintNet;

namespace MrKWatkins.OakIO.Resources.Gpl;

/// <summary>
/// Reads and writes GIMP GPL palettes without changing their stored text.
/// </summary>
/// <remarks>
/// Format reference: https://developer.gimp.org/core/standards/gpl/.
/// </remarks>
public sealed class GplFormat : PaletteFormat<GplFile>
{
    /// <summary>
    /// The singleton GPL format.
    /// </summary>
    public static readonly GplFormat Instance = new();

    private GplFormat() : base("GIMP Palette", "gpl")
    {
    }

    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new GplFile(await reader.ReadToEndAsync().ConfigureAwait(false));

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(GplFile file, IBinaryWriter writer)
    {
        await file.Header.WriteAsync(writer).ConfigureAwait(false);
        foreach (var line in file.Lines)
        {
            await line.WriteAsync(writer).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<IOFileConverter> CreateConverters() =>
        [new PaletteToJascConverter(this), new PaletteToPaintNetConverter(this)];
}