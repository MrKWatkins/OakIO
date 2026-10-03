using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxp;

/// <summary>
/// A headerless NXP palette containing 16 or 256 RGB333 colour entries.
/// </summary>
public sealed class NxpFile : PaletteFile
{
    /// <summary>
    /// Creates an NXP palette, rounding opaque RGB8 colours to the nearest RGB333 levels.
    /// </summary>
    /// <param name="palette">A palette containing exactly 16 or 256 opaque colours.</param>
    public NxpFile(PaletteData palette) : base(NxpFormat.Instance)
    {
        ArgumentNullException.ThrowIfNull(palette);
        if (palette.Colours.Count is not (16 or 256))
        {
            throw new ArgumentException("An NXP palette must contain exactly 16 or 256 colours.", nameof(palette));
        }

        Entries = palette.Colours.Select(colour => new NxpColourEntry(colour)).ToArray();
    }

    internal NxpFile(byte[] bytes) : base(NxpFormat.Instance)
    {
        // nextraw emits 32-byte four-bit palettes or 512-byte eight-bit palettes, with no header.
        // https://github.com/stefanbylund/zxnext_bmp_tools#nextraw
        if (bytes.Length is not (32 or 512))
        {
            throw new InvalidDataException("An NXP palette must contain exactly 32 or 512 bytes.");
        }

        var entries = new NxpColourEntry[bytes.Length / 2];
        for (var index = 0; index < entries.Length; index++)
        {
            entries[index] = new NxpColourEntry(bytes[(index * 2)..(index * 2 + 2)]);
        }

        Entries = entries;
    }

    /// <summary>
    /// Gets the NXP format.
    /// </summary>
    public new NxpFormat Format => (NxpFormat)base.Format;

    /// <summary>
    /// Gets the ordered byte-backed colour entries.
    /// </summary>
    public IReadOnlyList<NxpColourEntry> Entries { get; }

    /// <inheritdoc />
    public override PaletteData Palette => new(Entries.Select(entry => entry.Colour));
}