namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// The header for a <see cref="BrowsePointBlock" />.
/// </summary>
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

    internal BrowsePointHeader(byte[] data)
        : base(PzxBlockType.BrowsePoint, data)
    {
    }
}