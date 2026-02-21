namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

public sealed class PauseHeader : PzxBlockHeader
{
    internal PauseHeader()
        : base(PzxBlockType.Pause, 8)
    {
    }

    internal PauseHeader(Stream stream)
        : base(PzxBlockType.Pause, 8, stream)
    {
    }

    internal PauseHeader(byte[] data)
        : base(PzxBlockType.Pause, data)
    {
    }

    public uint Duration => GetUInt32(StartIndex) & 0x7FFFFFFF;

    public bool InitialPulseLevel => GetBit(StartIndex + 3, 7);
}