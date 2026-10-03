using MrKWatkins.OakIO.Resources.Text;

namespace MrKWatkins.OakIO.Resources.PaintNet;

/// <summary>
/// A byte-backed Paint.NET AARRGGBB hexadecimal colour line.
/// </summary>
public sealed class PaintNetColourEntry : TextLine
{
    internal PaintNetColourEntry(byte[] data) : base(data)
    {
        _ = PaletteText.ParseArgb(Text);
    }

    /// <summary>
    /// Gets the stored colour, including its alpha component.
    /// </summary>
    public Colour Colour => PaletteText.ParseArgb(Text);
}