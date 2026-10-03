using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

/// <summary>
/// Interprets an FBPPPIII attribute byte without discarding any bits.
/// </summary>
/// <param name="Value">The original attribute byte.</param>
/// <remarks>
/// Attribute reference: https://worldofspectrum.org/ZXBasicManual/zxmanchap16.html.
/// RGB8 values follow OakEmu: normal components are 192, bright components are 255, and black is identical at both levels.
/// Colour reference: https://github.com/MrKWatkins/OakEmu/blob/main/src/MrKWatkins.OakEmu.Machines.ZXSpectrum/Screen/PixelColourExtensions.cs.
/// </remarks>
[SuppressMessage("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "Attribute is the Spectrum's term for a character cell's colour and display flags, not a .NET annotation.")]
public readonly record struct ScrAttribute(byte Value)
{
    /// <summary>
    /// Gets the ink colour index, zero to seven.
    /// </summary>
    public byte Ink => (byte)(Value & 7);
    /// <summary>
    /// Gets the paper colour index, zero to seven.
    /// </summary>
    public byte Paper => (byte)((Value >> 3) & 7);
    /// <summary>
    /// Gets whether both colours are bright.
    /// </summary>
    public bool Bright => (Value & 64) != 0;
    /// <summary>
    /// Gets whether ink and paper alternate during FLASH.
    /// </summary>
    public bool Flash => (Value & 128) != 0;
    /// <summary>
    /// Gets the ink in the RGB8 rendering palette.
    /// </summary>
    public Colour InkColour => GetColour(Ink, Bright);
    /// <summary>
    /// Gets the paper in the RGB8 rendering palette.
    /// </summary>
    public Colour PaperColour => GetColour(Paper, Bright);

    [Pure]
    internal static Colour GetColour(int index, bool bright)
    {
        var level = bright ? (byte)255 : (byte)192;
        return new Colour((index & 2) != 0 ? level : (byte)0,
            (index & 4) != 0 ? level : (byte)0, (index & 1) != 0 ? level : (byte)0);
    }
}