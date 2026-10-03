using System.Globalization;
using System.Text;
using MrKWatkins.OakIO.Resources.Text;

namespace MrKWatkins.OakIO.Resources.PaintNet;

/// <summary>
/// A Paint.NET text palette retaining all stored entries, comments and original text.
/// </summary>
public sealed class PaintNetFile : PaletteFile
{
    /// <summary>
    /// Creates a Paint.NET palette, preserving colour order and alpha.
    /// </summary>
    /// <param name="palette">The actual stored colours; no 96-slot UI padding or truncation is performed.</param>
    public PaintNetFile(PaletteData palette) : this(CreateBytes(palette))
    {
    }

    internal PaintNetFile(byte[] data) : base(PaintNetFormat.Instance)
    {
        Lines =
        [
            .. PaletteText.ReadLines(data).Select(line =>
                string.IsNullOrWhiteSpace(line.Text) || line.Text.TrimStart().StartsWith(';')
                    ? line
                    : new PaintNetColourEntry([.. line.AsReadOnlySpan()]))
        ];
        if (Entries.Count == 0)
        {
            throw new InvalidDataException("A Paint.NET palette must contain at least one colour.");
        }
    }

    /// <summary>
    /// Gets the format.
    /// </summary>
    public new PaintNetFormat Format => (PaintNetFormat)base.Format;

    /// <summary>
    /// Gets the ordered lines, including colour entries, comments and blank lines.
    /// </summary>
    public IReadOnlyList<TextLine> Lines { get; }

    /// <summary>
    /// Gets all stored colour entries, without padding or truncating for Paint.NET's UI.
    /// </summary>
    public IReadOnlyList<PaintNetColourEntry> Entries => [.. Lines.OfType<PaintNetColourEntry>()];

    /// <inheritdoc />
    public override PaletteData Palette => new(Entries.Select(entry => entry.Colour));

    [Pure]
    private static byte[] CreateBytes(PaletteData palette)
    {
        ArgumentNullException.ThrowIfNull(palette);
        var text = new StringBuilder("; Paint.NET Palette File\n");
        foreach (var colour in palette.Colours)
        {
            text.Append(CultureInfo.InvariantCulture, $"{colour.Alpha:X2}{colour.Red:X2}{colour.Green:X2}{colour.Blue:X2}\n");
        }
        return PaletteText.Encode(text.ToString());
    }
}