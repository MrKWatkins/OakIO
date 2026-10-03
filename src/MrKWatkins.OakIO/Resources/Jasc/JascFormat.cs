using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources.Gpl;
using MrKWatkins.OakIO.Resources.PaintNet;

namespace MrKWatkins.OakIO.Resources.Jasc;

/// <summary>
/// Reads and writes JASC-PAL 0100 palettes; other .pal variants are not inferred.
/// </summary>
/// <remarks>
/// Format reference: https://developer.gimp.org/core/standards/.
/// </remarks>
public sealed class JascFormat : PaletteFormat<JascFile>
{
    /// <summary>
    /// The singleton JASC format.
    /// </summary>
    public static readonly JascFormat Instance = new();

    private JascFormat() : base("JASC Paint Shop Pro Palette", "pal")
    {
    }

    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new JascFile(await reader.ReadToEndAsync().ConfigureAwait(false));

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(JascFile file, IBinaryWriter writer)
    {
        await file.Header.WriteAsync(writer).ConfigureAwait(false);
        foreach (var line in file.Lines)
        {
            await line.WriteAsync(writer).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<IOFileConverter> CreateConverters() =>
        [new PaletteToGplConverter(this), new PaletteToPaintNetConverter(this)];
}