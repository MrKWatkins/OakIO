namespace MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

/// <summary>
/// The 6144 bitmap bytes in native interleaved screen order, with the left pixel in bit seven.
/// </summary>
/// <remarks>
/// Layout reference: https://worldofspectrum.org/ZXBasicManual/zxmanchap24.html.
/// </remarks>
public sealed class ScrBitmap : IOFileComponent
{
    internal ScrBitmap(byte[] data) : base(6144, data)
    {
    }
    /// <summary>
    /// Gets a pixel's ink-selection bit using top-left-origin screen coordinates.
    /// </summary>
    /// <param name="x">The pixel column, zero to 255.</param>
    /// <param name="y">The pixel row, zero to 191.</param>
    /// <returns>True for ink, false for paper.</returns>
    [Pure]
    public bool GetPixel(int x, int y)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, 256);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, 192);
        return (GetByte(GetByteOffset(x, y)) & (128 >> (x & 7))) != 0;
    }

    [Pure]
    internal static int GetByteOffset(int x, int y) =>
        ((y & 192) << 5) | ((y & 7) << 8) | ((y & 56) << 2) | (x >> 3);
}