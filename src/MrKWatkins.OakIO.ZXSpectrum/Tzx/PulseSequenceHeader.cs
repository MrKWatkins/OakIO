namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class PulseSequenceHeader : TzxBlockHeader
{
    private const int Size = 1;

    internal PulseSequenceHeader()
        : base(TzxBlockType.PulseSequence, Size)
    {
    }

    internal PulseSequenceHeader(Stream stream)
        : base(TzxBlockType.PulseSequence, Size, stream)
    {
    }

    internal PulseSequenceHeader(byte[] data)
        : base(TzxBlockType.PulseSequence, data)
    {
    }

    public byte NumberOfPulses => GetByte(0);

    public override int BlockLength => NumberOfPulses * 2;  // Each is a word.

    public override string ToString() => $"{Type}: {NumberOfPulses} pulses";
}