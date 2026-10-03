namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// An immutable image containing a colour for each pixel.
/// </summary>
public sealed class ColourImageData : ImageData
{
    /// <summary>
    /// Initializes an image by copying row-major pixels.
    /// </summary>
    /// <param name="width">The width in pixels.</param>
    /// <param name="height">The height in pixels.</param>
    /// <param name="pixels">Exactly width times height colours, starting at the top-left.</param>
    public ColourImageData(int width, int height, Colour[] pixels) : base(width, height)
    {
        ArgumentNullException.ThrowIfNull(pixels);
        if (pixels.Length != PixelCount)
        {
            throw new ArgumentException("The pixel count must match the image dimensions.", nameof(pixels));
        }
        Pixels = Array.AsReadOnly(pixels.ToArray());
    }

    /// <summary>
    /// Gets the row-major pixel colours.
    /// </summary>
    public IReadOnlyList<Colour> Pixels { get; }

    /// <inheritdoc />
    [Pure]
    public override Colour GetPixel(int x, int y) => Pixels[GetPixelOffset(x, y)];
}