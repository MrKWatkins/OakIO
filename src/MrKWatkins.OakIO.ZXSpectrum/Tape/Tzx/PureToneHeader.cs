namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

public sealed class PureToneHeader : TzxBlockHeader
{
    private const int Size = 4;

    internal PureToneHeader()
        : base(TzxBlockType.PureTone, Size)
    {
    }

    internal PureToneHeader(Stream stream)
        : base(TzxBlockType.PureTone, Size, stream)
    {
    }

    internal PureToneHeader(byte[] data)
        : base(TzxBlockType.PureTone, data)
    {
    }

    public ushort LengthOfPulse => GetWord(0);

    public ushort NumberOfPulses => GetWord(2);

    public override string ToString() => $"{Type}: {NumberOfPulses} x {LengthOfPulse} T-States";
}