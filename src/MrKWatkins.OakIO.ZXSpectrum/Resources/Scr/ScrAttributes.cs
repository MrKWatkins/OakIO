namespace MrKWatkins.OakIO.ZXSpectrum.Resources.Scr;

/// <summary>
/// The 768 row-major attribute bytes for the 32 by 24 character cells.
/// </summary>
/// <remarks>
/// Layout reference: https://worldofspectrum.org/ZXBasicManual/zxmanchap24.html.
/// </remarks>
public sealed class ScrAttributes : IOFileComponent
{
    internal ScrAttributes(byte[] data) : base(768, data)
    {
    }
    /// <summary>
    /// Gets the attribute at a character-cell position, not a pixel position.
    /// </summary>
    /// <param name="x">The cell column, zero to 31.</param>
    /// <param name="y">The cell row, zero to 23.</param>
    /// <returns>The attribute, including BRIGHT and FLASH.</returns>
    [Pure]
    public ScrAttribute GetAttribute(int x, int y)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(x);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, 32);
        ArgumentOutOfRangeException.ThrowIfNegative(y);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, 24);
        return new ScrAttribute(GetByte(y * 32 + x));
    }
}