namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

public sealed class LoopStartHeader : TzxBlockHeader
{
    private const int Size = 2;

    internal LoopStartHeader()
        : base(TzxBlockType.LoopStart, Size)
    {
    }

    internal LoopStartHeader(Stream stream)
        : base(TzxBlockType.LoopStart, Size, stream)
    {
    }

    public ushort NumberOfRepetitions => GetWord(0);

    public override string ToString() => $"{Type}: {NumberOfRepetitions} repetitions";
}