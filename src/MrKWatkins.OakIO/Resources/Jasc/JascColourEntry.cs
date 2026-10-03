using MrKWatkins.OakIO.Resources.Text;

namespace MrKWatkins.OakIO.Resources.Jasc;

/// <summary>
/// A byte-backed JASC RGB colour line.
/// </summary>
public sealed class JascColourEntry : TextLine
{
    internal JascColourEntry(byte[] data) : base(data)
    {
        _ = PaletteText.ParseRgb(Text, allowName: false);
    }

    /// <summary>
    /// Gets the RGB colour.
    /// </summary>
    public Colour Colour => PaletteText.ParseRgb(Text, allowName: false).Colour;
}