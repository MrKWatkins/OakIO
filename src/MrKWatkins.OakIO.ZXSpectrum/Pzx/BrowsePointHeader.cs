namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

public sealed class BrowsePointHeader : PzxBlockHeader
{
    internal BrowsePointHeader()
        : base(PzxBlockType.BrowsePoint, 4)
    {
    }

    internal BrowsePointHeader(Stream stream)
        : base(PzxBlockType.BrowsePoint, 4, stream)
    {
    }
}