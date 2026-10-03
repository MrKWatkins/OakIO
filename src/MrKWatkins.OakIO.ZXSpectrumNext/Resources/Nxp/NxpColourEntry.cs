using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;

/// <summary>
/// A two-byte RGB333 colour entry in an NXP palette.
/// </summary>
public sealed class NxpColourEntry : IOFileComponent
{
    // NXP layout: RGB332 followed by the zero-extended lowest blue bit.
    // https://github.com/stefanbylund/zxnext_bmp_tools#nextraw
    internal NxpColourEntry(byte[] data) : base(data)
    {
        if (data.Length != 2 || data[1] > 1)
        {
            throw new InvalidDataException("An NXP colour must contain an RGB332 byte followed by a zero-extended blue bit.");
        }
    }

    /// <summary>
    /// Creates an opaque RGB333 entry, rounding each RGB8 component to the nearest three-bit level.
    /// </summary>
    /// <param name="colour">The opaque colour to encode.</param>
    public NxpColourEntry(Colour colour) : this(Encode(colour))
    {
    }

    /// <summary>
    /// Gets the three-bit red component.
    /// </summary>
    public byte Red => GetBits(0, 5, 7);

    /// <summary>
    /// Gets the three-bit green component.
    /// </summary>
    public byte Green => GetBits(0, 2, 4);

    /// <summary>
    /// Gets the three-bit blue component.
    /// </summary>
    public byte Blue => (byte)((GetBits(0, 0, 1) << 1) | GetByte(1));

    /// <summary>
    /// Gets the opaque RGB8 convenience value, expanding each level to the nearest RGB8 value.
    /// </summary>
    public Colour Colour => new(Expand(Red), Expand(Green), Expand(Blue));

    [Pure]
    private static byte Expand(byte value) => (byte)((value * 255 + 3) / 7);

    [Pure]
    private static byte[] Encode(Colour colour)
    {
        if (colour.Alpha != 255)
        {
            throw new ArgumentException("NXP palettes do not support alpha.", nameof(colour));
        }

        var red = (colour.Red * 7 + 127) / 255;
        var green = (colour.Green * 7 + 127) / 255;
        var blue = (colour.Blue * 7 + 127) / 255;
        return [(byte)((red << 5) | (green << 2) | (blue >> 1)), (byte)(blue & 1)];
    }
}