namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

public sealed class PzxHeader : PzxBlockHeader
{
    internal PzxHeader()
        : base(PzxBlockType.Header, 6)
    {
    }

    internal PzxHeader(Stream stream)
        : base(PzxBlockType.Header, 6, stream)
    {
    }

    internal PzxHeader(byte[] data)
        : base(PzxBlockType.Header, data)
    {
    }

    public byte MajorVersionNumber => GetByte(StartIndex);

    public byte MinorVersionNumber => GetByte(StartIndex + 1);
}
