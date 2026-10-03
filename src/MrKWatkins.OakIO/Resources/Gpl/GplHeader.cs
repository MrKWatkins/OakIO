using System.Globalization;
using MrKWatkins.OakIO.Resources.Text;

namespace MrKWatkins.OakIO.Resources.Gpl;

/// <summary>
/// The byte-backed GPL magic line and optional palette name and column count.
/// </summary>
public sealed class GplHeader : Header
{
    internal GplHeader(byte[] data) : base(data)
    {
    }

    private IReadOnlyList<TextLine> Lines => PaletteText.ReadLines(AsReadOnlySpan().ToArray());

    /// <summary>
    /// Gets the magic identifier.
    /// </summary>
    public string Signature => Lines[0].Text;

    /// <summary>
    /// Gets the stored palette name, or null for the old format without a name.
    /// </summary>
    public string? Name => Lines.Count > 1 ? Lines[1].Text[5..].Trim() : null;

    /// <summary>
    /// Gets whether the header explicitly includes a column count.
    /// </summary>
    public bool HasColumns => Lines.Count > 2;

    /// <summary>
    /// Gets the column count; zero means automatic layout or no stored value.
    /// </summary>
    public int Columns => HasColumns ? byte.Parse(Lines[2].Text[8..].Trim(), CultureInfo.InvariantCulture) : 0;
}