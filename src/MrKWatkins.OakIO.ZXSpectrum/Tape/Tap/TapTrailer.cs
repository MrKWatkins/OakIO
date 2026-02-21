namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

public sealed class TapTrailer : Trailer
{
    internal TapTrailer(byte checksum)
        : base(1, [checksum])
    {
    }

    public byte Checksum
    {
        get => Data[0];
        set => AsSpan()[0] = value;
    }
}