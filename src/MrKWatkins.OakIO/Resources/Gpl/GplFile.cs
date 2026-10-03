using System.Globalization;
using System.Text;
using MrKWatkins.OakIO.Resources.Text;

namespace MrKWatkins.OakIO.Resources.Gpl;

/// <summary>
/// A GPL palette retaining its header, colour names, comments and original text bytes.
/// </summary>
public sealed class GplFile : PaletteFile
{
    /// <summary>
    /// Creates an opaque GPL palette with a name, column count and optional colour names.
    /// </summary>
    /// <param name="palette">The colours in palette order.</param>
    /// <param name="name">The palette name.</param>
    /// <param name="columns">The display column count, from zero to 255.</param>
    /// <param name="colourNames">Optional names, one per colour.</param>
    public GplFile(PaletteData palette, string name = "OakIO", int columns = 0, IReadOnlyList<string>? colourNames = null)
        : this(CreateBytes(palette, name, columns, colourNames))
    {
    }

    internal GplFile(byte[] data) : base(GplFormat.Instance)
    {
        var lines = PaletteText.ReadLines(data);
        if (lines.Count == 0 || lines[0].Text != "GIMP Palette")
        {
            throw new InvalidDataException("A GPL palette must start with GIMP Palette.");
        }
        var headerLines = 1;
        if (lines.Count > 1 && lines[1].Text.StartsWith("Name:", StringComparison.Ordinal))
        {
            headerLines++;
            if (lines.Count > 2 && lines[2].Text.StartsWith("Columns:", StringComparison.Ordinal))
            {
                if (!byte.TryParse(lines[2].Text[8..].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out _))
                {
                    throw new InvalidDataException("GPL columns must be between zero and 255.");
                }
                headerLines++;
            }
        }
        Header = new GplHeader(PaletteText.Join(lines.Take(headerLines)));
        var body = new List<TextLine>();
        foreach (var line in lines.Skip(headerLines))
        {
            body.Add(string.IsNullOrWhiteSpace(line.Text) || line.Text.TrimStart().StartsWith('#')
                ? line : new GplColourEntry([.. line.AsReadOnlySpan()]));
        }
        Lines = body;
        if (Entries.Count == 0)
        {
            throw new InvalidDataException("A GPL palette must contain at least one colour.");
        }
    }

    /// <summary>
    /// Gets the format.
    /// </summary>
    public new GplFormat Format => (GplFormat)base.Format;

    /// <summary>
    /// Gets the GPL header.
    /// </summary>
    public GplHeader Header { get; }

    /// <summary>
    /// Gets the ordered body lines, including colour entries, comments and blank lines.
    /// </summary>
    public IReadOnlyList<TextLine> Lines { get; }

    /// <summary>
    /// Gets the colour entries in palette order.
    /// </summary>
    public IReadOnlyList<GplColourEntry> Entries => [.. Lines.OfType<GplColourEntry>()];

    /// <inheritdoc />
    public override PaletteData Palette => new(Entries.Select(entry => entry.Colour));

    [Pure]
    private static byte[] CreateBytes(PaletteData palette, string name, int columns, IReadOnlyList<string>? colourNames)
    {
        PaletteText.ValidateOpaque(palette);
        PaletteText.ValidateSingleLine(name, nameof(name));
        ArgumentOutOfRangeException.ThrowIfNegative(columns);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(columns, 255);
        if (colourNames != null && colourNames.Count != palette.Colours.Count)
        {
            throw new ArgumentException("There must be one name per colour.", nameof(colourNames));
        }
        var text = new StringBuilder();
        text.Append(CultureInfo.InvariantCulture, $"GIMP Palette\nName: {name}\nColumns: {columns}\n#\n");
        for (var i = 0; i < palette.Colours.Count; i++)
        {
            var colour = palette.Colours[i];
            text.Append(CultureInfo.InvariantCulture, $"{colour.Red} {colour.Green} {colour.Blue}");
            if (colourNames != null)
            {
                PaletteText.ValidateSingleLine(colourNames[i], nameof(colourNames));
                text.Append('\t').Append(colourNames[i]);
            }
            text.Append('\n');
        }
        return PaletteText.Encode(text.ToString());
    }
}