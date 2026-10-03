using MrKWatkins.OakIO.Binary;
using MrKWatkins.OakIO.Resources.Gpl;
using MrKWatkins.OakIO.Resources.Jasc;

namespace MrKWatkins.OakIO.Resources.PaintNet;

/// <summary>
/// Reads and writes Paint.NET hexadecimal text palettes without changing their stored lines.
/// </summary>
public sealed class PaintNetFormat : PaletteFormat<PaintNetFile>
{
    /// <summary>
    /// The singleton Paint.NET format.
    /// </summary>
    public static readonly PaintNetFormat Instance = new();

    private PaintNetFormat() : base("Paint.NET Palette", "txt")
    {
    }

    /// <inheritdoc />
    protected override async ValueTask<IOFile> ReadAsync(IBinaryReader reader) =>
        new PaintNetFile(await reader.ReadToEndAsync().ConfigureAwait(false));

    /// <inheritdoc />
    protected override async ValueTask WriteAsync(PaintNetFile file, IBinaryWriter writer)
    {
        foreach (var line in file.Lines)
        {
            await line.WriteAsync(writer).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<IOFileConverter> CreateConverters() =>
        [new PaletteToGplConverter(this), new PaletteToJascConverter(this)];
}