using MrKWatkins.OakIO.Resources.Text;

namespace MrKWatkins.OakIO.Resources.Gpl;

/// <summary>
/// A byte-backed GPL RGB colour line with an optional UTF-8 name.
/// </summary>
public sealed class GplColourEntry : TextLine
{
    internal GplColourEntry(byte[] data) : base(data)
    {
        _ = PaletteText.ParseRgb(Text, allowName: true);
    }

    /// <summary>
    /// Gets the RGB colour.
    /// </summary>
    public Colour Colour => PaletteText.ParseRgb(Text, allowName: true).Colour;

    /// <summary>
    /// Gets the optional colour name, or an empty string.
    /// </summary>
    public string Name => PaletteText.ParseRgb(Text, allowName: true).Name;
}