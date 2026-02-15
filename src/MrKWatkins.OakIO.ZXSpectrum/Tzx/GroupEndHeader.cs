namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class GroupEndHeader : TzxBlockHeader
{
    private const int Size = 0;

    internal GroupEndHeader()
        : base(TzxBlockType.GroupEnd, Size)
    {
    }

    internal GroupEndHeader(Stream stream)
        : base(TzxBlockType.GroupEnd, Size, stream)
    {
    }
}