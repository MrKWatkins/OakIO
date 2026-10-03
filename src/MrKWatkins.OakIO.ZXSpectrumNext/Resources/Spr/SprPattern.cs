namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Spr;

/// <summary>
/// A byte-backed 16 by 16 sprite pattern, interpreted at an explicit bit depth.
/// </summary>
public sealed class SprPattern : IOFileComponent
{
    internal SprPattern(byte[] data, int bitsPerPixel) : base(data)
    {
        BitsPerPixel = SprFormat.ForBitsPerPixel(bitsPerPixel).BitsPerPixel;
        PackedPixels.ValidateLength(data.Length, 256 * bitsPerPixel / 8);
    }
    /// <summary>
    /// Gets the bit depth used to interpret the stored bytes.
    /// </summary>
    public int BitsPerPixel { get; }
    /// <summary>
    /// Gets unpacked palette indices derived from the original bytes, in row-major order.
    /// </summary>
    public IReadOnlyList<byte> Pixels => PackedPixels.Unpack(AsReadOnlySpan(), BitsPerPixel);
}