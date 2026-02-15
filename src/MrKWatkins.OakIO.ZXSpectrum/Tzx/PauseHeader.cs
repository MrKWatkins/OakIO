namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class PauseHeader : TzxBlockHeader
{
    private const int Size = 2;

    internal PauseHeader()
        : base(TzxBlockType.Pause, Size)
    {
    }

    internal PauseHeader(Stream stream)
        : base(TzxBlockType.Pause, Size, stream)
    {
    }

    public ushort PauseMs => GetWord(0);

    public TimeSpan Pause => TimeSpan.FromMilliseconds(PauseMs);

    public override string ToString() => $"{Type}: {Pause}";
}