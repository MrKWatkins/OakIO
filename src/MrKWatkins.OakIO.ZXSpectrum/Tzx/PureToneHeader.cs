namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

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

    public ushort LengthOfPulse => GetWord(0);

    public ushort NumberOfPulses => GetWord(2);

    public override string ToString() => $"{Type}: {NumberOfPulses} x {LengthOfPulse} T-States";
}