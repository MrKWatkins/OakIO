namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class StandardSpeedDataHeader : TzxBlockHeader
{
    private const int Size = 4;

    internal StandardSpeedDataHeader()
        : base(TzxBlockType.StandardSpeedData, Size)
    {
    }

    internal StandardSpeedDataHeader(Stream stream)
        : base(TzxBlockType.StandardSpeedData, Size, stream)
    {
    }

    internal StandardSpeedDataHeader(byte[] data)
        : base(TzxBlockType.StandardSpeedData, data)
    {
    }

    public ushort PauseAfterBlockMs => GetWord(0);

    public TimeSpan PauseAfter => TimeSpan.FromMilliseconds(PauseAfterBlockMs);

    public override int BlockLength => GetWord(2);

    public override string ToString() =>
        $"{Type}: Length = {BlockLength}, pause after = {PauseAfter}";
}
