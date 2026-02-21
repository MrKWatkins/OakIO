namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

public sealed class GroupStartHeader : TzxBlockHeader
{
    private const int Size = 1;

    internal GroupStartHeader()
        : base(TzxBlockType.GroupStart, Size)
    {
    }

    internal GroupStartHeader(Stream stream)
        : base(TzxBlockType.GroupStart, Size, stream)
    {
    }

    public override int BlockLength => GetByte(0);
}