using System.Globalization;
using MrKWatkins.OakIO.Resources.Text;

namespace MrKWatkins.OakIO.Resources.Jasc;

/// <summary>
/// The three byte-backed JASC palette header lines.
/// </summary>
public sealed class JascHeader : Header
{
    internal JascHeader(byte[] data) : base(data)
    {
    }

    private IReadOnlyList<TextLine> Lines => PaletteText.ReadLines(AsReadOnlySpan().ToArray());

    /// <summary>
    /// Gets the JASC-PAL signature.
    /// </summary>
    public string Signature => Lines[0].Text;

    /// <summary>
    /// Gets the stored version identifier.
    /// </summary>
    public string Version => Lines[1].Text;

    /// <summary>
    /// Gets the declared colour count.
    /// </summary>
    public int ColourCount => int.Parse(Lines[2].Text.Trim(), CultureInfo.InvariantCulture);
}