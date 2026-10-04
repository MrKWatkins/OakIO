using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Screens;

/// <summary>
/// Byte-backed optional RGB332/RGB333 screen palette, including stored priority bits.
/// </summary>
/// <remarks>
/// https://gitlab.com/varmfskii/showsimg/-/blob/c79616ac578fb9372c2b34a413f091290b549959/FORMAT.md
/// https://wiki.specnext.dev/Palettes
/// </remarks>
public sealed class ScreenPalette : IOFileComponent
{
    internal ScreenPalette(byte[] bytes, int count) : base(bytes.Length, bytes)
    {
        Encoding = bytes.Length switch
        {
            0 => ScreenPaletteEncoding.None,
            var length when length == count => ScreenPaletteEncoding.Rgb332,
            var length when length == count * 2 => ScreenPaletteEncoding.Rgb333,
            _ => throw new InvalidDataException("The screen palette must be absent or contain one or two bytes per colour.")
        };
        Count = count;
        if (Encoding == ScreenPaletteEncoding.Rgb333)
        {
            for (var index = 1; index < bytes.Length; index += 2)
            {
                if ((bytes[index] & 0x7E) != 0)
                {
                    throw new InvalidDataException("Reserved RGB333 palette bits must be zero.");
                }
            }
        }
    }

    /// <summary>
    /// Gets the on-disk encoding.
    /// </summary>
    public ScreenPaletteEncoding Encoding { get; }
    /// <summary>
    /// Gets the number of colours, including implicit defaults.
    /// </summary>
    public int Count { get; }
    /// <summary>
    /// Gets a derived opaque colour view.
    /// </summary>
    public PaletteData Palette => new([.. Enumerable.Range(0, Count).Select(GetColour)]);

    /// <summary>
    /// Gets a stored Layer 2 priority flag.
    /// </summary>
    [Pure]
    public bool GetPriority(int index)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(index);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Count);
        return Encoding == ScreenPaletteEncoding.Rgb333 && (Data[index * 2 + 1] & 128) != 0;
    }

    [Pure]
    private Colour GetColour(int index)
    {
        var packed = Encoding == ScreenPaletteEncoding.None ? (byte)index : Data[index * (Encoding == ScreenPaletteEncoding.Rgb333 ? 2 : 1)];
        var blue = Encoding == ScreenPaletteEncoding.Rgb333 ? ((packed & 3) << 1) | (Data[index * 2 + 1] & 1) :
            ((packed & 3) << 1) | ((packed & 3) == 0 ? 0 : 1);
        return new Colour(Expand(packed >> 5), Expand((packed >> 2) & 7), Expand(blue));
    }

    [Pure]
    private static byte Expand(int value) => (byte)((value * 255 + 3) / 7);

    [Pure]
    internal static ScreenPalette Create(PaletteData palette, int count, ScreenPaletteEncoding encoding)
    {
        ArgumentNullException.ThrowIfNull(palette);
        if (palette.Colours.Count != count)
        {
            throw new ArgumentException("A screen requires exactly the palette size of its native mode.", nameof(palette));
        }
        if (!Enum.IsDefined(encoding))
        {
            throw new ArgumentOutOfRangeException(nameof(encoding));
        }
        var bytes = new byte[count * (int)encoding];
        var result = new ScreenPalette(bytes, count);
        for (var index = 0; index < count; index++)
        {
            var colour = palette.Colours[index];
            if (colour.Alpha != 255)
            {
                throw new ArgumentException("Screen palettes do not support alpha.", nameof(palette));
            }
            if (encoding == ScreenPaletteEncoding.None)
            {
                if (colour != result.GetColour(index))
                {
                    throw new ArgumentException("A palette-free screen requires the default RGB332 palette.", nameof(palette));
                }
                continue;
            }
            var red = (colour.Red * 7 + 127) / 255;
            var green = (colour.Green * 7 + 127) / 255;
            var blue = (colour.Blue * 7 + 127) / 255;
            if (encoding == ScreenPaletteEncoding.Rgb332)
            {
                var blue2 = Enumerable.Range(0, 4).MinBy(value => Math.Abs(Expand(value == 0 ? 0 : value * 2 + 1) - colour.Blue));
                bytes[index] = (byte)((red << 5) | (green << 2) | blue2);
            }
            else
            {
                bytes[index * 2] = (byte)((red << 5) | (green << 2) | (blue >> 1));
                bytes[index * 2 + 1] = (byte)(blue & 1);
            }
        }
        return new ScreenPalette(bytes, count);
    }
}