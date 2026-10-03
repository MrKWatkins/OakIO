namespace MrKWatkins.OakIO.ZXSpectrumNext.Resources.Nxt;

/// <summary>
/// A byte-backed 8 by 8 tile, interpreted at an explicit bit depth.
/// </summary>
public sealed class NxtTile : IOFileComponent
{
    internal NxtTile(byte[] data, int bitsPerPixel) : base(data)
    {
        BitsPerPixel = NxtFormat.ForBitsPerPixel(bitsPerPixel).BitsPerPixel;
        PackedPixels.ValidateLength(data.Length, 64 * bitsPerPixel / 8);
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