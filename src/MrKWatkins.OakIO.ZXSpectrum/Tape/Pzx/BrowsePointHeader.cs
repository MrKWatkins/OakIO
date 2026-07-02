namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// The header for a <see cref="BrowsePointBlock" />.
/// </summary>
public sealed class BrowsePointHeader : PzxBlockHeader
{
    internal BrowsePointHeader(byte[] data)
        : base(PzxBlockType.BrowsePoint, data)
    {
    }
}