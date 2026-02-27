namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// Trailer for a block in a TAP file containing the checksum byte.
/// </summary>
public sealed class TapTrailer : Trailer
{
    internal TapTrailer(byte checksum)
        : base(1, [checksum])
    {
    }

    /// <summary>
    /// Gets or sets the XOR checksum byte.
    /// </summary>
    public byte Checksum
    {
        get => Data[0];
        set => AsSpan()[0] = value;
    }
}