using MrKWatkins.OakIO.Resources;

namespace MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

/// <summary>
/// A standard Spectrum screen dump with bitmap and attribute components, and no header or border colour.
/// </summary>
/// <remarks>
/// Format reference: https://worldofspectrum.org/faq/reference/formats.htm.
/// </remarks>
public sealed class ScrFile : ImageFile
{
    private static readonly PaletteData RenderingPalette = new(Enumerable.Range(0, 16).Select(index => ScrAttribute.GetColour(index & 7, index >= 8)));

    /// <summary>
    /// Creates a screen by copying exactly 6912 native screen-memory bytes.
    /// </summary>
    /// <param name="data">6144 bitmap bytes followed by 768 attribute bytes.</param>
    public ScrFile(byte[] data) : base(ScrFormat.Instance)
    {
        ArgumentNullException.ThrowIfNull(data);
        if (data.Length != 6912)
        {
            throw new InvalidDataException("A standard SCR screen must contain exactly 6912 bytes.");
        }
        Bitmap = new ScrBitmap(data[..6144]);
        Attributes = new ScrAttributes(data[6144..]);
    }
    /// <summary>
    /// Creates a screen from an exactly representable image without quantization or FLASH.
    /// </summary>
    /// <param name="image">A 256 by 192 image using the rendering palette and at most two compatible colours per cell.</param>
    public ScrFile(ImageData image) : this(Encode(image))
    {
    }
    /// <summary>
    /// Gets the SCR format.
    /// </summary>
    public new ScrFormat Format => (ScrFormat)base.Format;
    /// <summary>
    /// Gets the bitmap in native memory order.
    /// </summary>
    public ScrBitmap Bitmap { get; }
    /// <summary>
    /// Gets the row-major character attributes.
    /// </summary>
    public ScrAttributes Attributes { get; }
    /// <inheritdoc />
    public override ImageData Image => Render();
    /// <summary>
    /// Derives a top-left-origin indexed image for either FLASH phase.
    /// </summary>
    /// <param name="flashPhase">True to exchange ink and paper in cells with FLASH set.</param>
    /// <returns>An image using the 16-entry OakEmu rendering palette, including both black indices.</returns>
    [Pure]
    public IndexedImageData Render(bool flashPhase = false) =>
        new(256, 192, ScrScreenConverter.Convert(Bitmap.AsReadOnlySpan(), Attributes.AsReadOnlySpan(), flashPhase), RenderingPalette, 4);

    [Pure]
    private static byte[] Encode(ImageData image)
    {
        ArgumentNullException.ThrowIfNull(image);
        if (image.Width != 256 || image.Height != 192)
        {
            throw new ArgumentException("An SCR image must be 256 by 192 pixels.", nameof(image));
        }
        var bytes = new byte[6912];
        for (var cellY = 0; cellY < 24; cellY++)
        {
            for (var cellX = 0; cellX < 32; cellX++)
            {
                var paper = GetColourIndex(image.GetPixel(cellX * 8, cellY * 8));
                var ink = paper;
                for (var y = cellY * 8; y < cellY * 8 + 8; y++)
                {
                    for (var x = cellX * 8; x < cellX * 8 + 8; x++)
                    {
                        var colour = GetColourIndex(image.GetPixel(x, y));
                        if (colour == paper)
                        {
                            continue;
                        }
                        if (ink != paper && colour != ink)
                        {
                            throw new ArgumentException($"Cell ({cellX}, {cellY}) contains more than two colours.", nameof(image));
                        }
                        ink = colour;
                        bytes[ScrBitmap.GetByteOffset(x, y)] |= (byte)(128 >> (x & 7));
                    }
                }
                // Black has no brightness distinction and can accompany either level.
                if (paper != 0 && ink != 0 && paper / 8 != ink / 8)
                {
                    throw new ArgumentException($"Cell ({cellX}, {cellY}) mixes normal and bright colours.", nameof(image));
                }
                var bright = Math.Max(paper, ink) >= 8;
                bytes[6144 + cellY * 32 + cellX] = (byte)((bright ? 64 : 0) | ((paper & 7) << 3) | (ink & 7));
            }
        }
        return bytes;
    }

    [Pure]
    private static int GetColourIndex(Colour colour)
    {
        for (var index = 0; index < 16; index++)
        {
            if (colour == RenderingPalette.Colours[index])
            {
                return index;
            }
        }
        throw new ArgumentException("An SCR image must use opaque colours from its RGB8 rendering palette.", nameof(colour));
    }
}