using System.Globalization;
using System.Text;
using MrKWatkins.OakIO.Resources.Text;

namespace MrKWatkins.OakIO.Resources.Jasc;

/// <summary>
/// A JASC-PAL 0100 palette retaining its header and original RGB text lines.
/// </summary>
public sealed class JascFile : PaletteFile
{
    /// <summary>
    /// Creates an opaque JASC palette.
    /// </summary>
    /// <param name="palette">The colours in palette order.</param>
    public JascFile(PaletteData palette) : this(CreateBytes(palette))
    {
    }

    internal JascFile(byte[] data) : base(JascFormat.Instance)
    {
        var lines = PaletteText.ReadLines(data);
        if (lines.Count < 3 || lines[0].Text != "JASC-PAL")
        {
            throw new InvalidDataException("A JASC palette requires a JASC-PAL header, version and colour count.");
        }
        if (lines[1].Text != "0100")
        {
            throw new NotSupportedException("Only JASC palette version 0100 is supported.");
        }
        if (!int.TryParse(lines[2].Text.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var count) || count <= 0)
        {
            throw new InvalidDataException("A JASC palette requires a positive colour count.");
        }
        Header = new JascHeader(PaletteText.Join(lines.Take(3)));
        Lines =
        [
            .. lines.Skip(3).Select(line => string.IsNullOrWhiteSpace(line.Text)
                ? line
                : new JascColourEntry([.. line.AsReadOnlySpan()]))
        ];
        if (Entries.Count != count)
        {
            throw new InvalidDataException("The JASC colour count does not match its entries.");
        }
    }

    /// <summary>
    /// Gets the format.
    /// </summary>
    public new JascFormat Format => (JascFormat)base.Format;

    /// <summary>
    /// Gets the signature, version and count header.
    /// </summary>
    public JascHeader Header { get; }

    /// <summary>
    /// Gets the ordered body lines, including any blank lines.
    /// </summary>
    public IReadOnlyList<TextLine> Lines { get; }

    /// <summary>
    /// Gets the RGB entries in palette order.
    /// </summary>
    public IReadOnlyList<JascColourEntry> Entries => [.. Lines.OfType<JascColourEntry>()];

    /// <inheritdoc />
    public override PaletteData Palette => new(Entries.Select(entry => entry.Colour));

    [Pure]
    private static byte[] CreateBytes(PaletteData palette)
    {
        PaletteText.ValidateOpaque(palette);
        var text = new StringBuilder();
        text.Append(CultureInfo.InvariantCulture, $"JASC-PAL\n0100\n{palette.Colours.Count}\n");
        foreach (var colour in palette.Colours)
        {
            text.Append(CultureInfo.InvariantCulture, $"{colour.Red} {colour.Green} {colour.Blue}\n");
        }
        return PaletteText.Encode(text.ToString());
    }
}