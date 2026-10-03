namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// Image pixels in top-left-origin, row-major order, independent of an on-disk layout.
/// </summary>
public abstract class ImageData
{
    /// <summary>
    /// Initializes image dimensions.
    /// </summary>
    /// <param name="width">The positive width in pixels.</param>
    /// <param name="height">The positive height in pixels.</param>
    protected ImageData(int width, int height)
    {
        PixelCount = ValidateDimensions(width, height);
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets the width in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Gets the total number of pixels.
    /// </summary>
    public int PixelCount { get; }

    /// <summary>
    /// Gets the colour at the specified coordinates.
    /// </summary>
    /// <param name="x">The horizontal coordinate, starting at zero on the left.</param>
    /// <param name="y">The vertical coordinate, starting at zero at the top.</param>
    /// <returns>The pixel colour.</returns>
    [Pure]
    public abstract Colour GetPixel(int x, int y);

    /// <summary>
    /// Gets the row-major pixel offset, validating the coordinates.
    /// </summary>
    /// <param name="x">The horizontal coordinate.</param>
    /// <param name="y">The vertical coordinate.</param>
    /// <returns>The pixel offset.</returns>
    [Pure]
    protected int GetPixelOffset(int x, int y)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, Width);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, Height);
        return y * Width + x;
    }

    [Pure]
    internal static int ValidateDimensions(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        return checked(width * height);
    }
}