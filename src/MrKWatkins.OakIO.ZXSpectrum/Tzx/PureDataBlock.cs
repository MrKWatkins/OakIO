namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class PureDataBlock : TzxBlock<PureDataHeader>
{
    public PureDataBlock(Stream stream) : base(new PureDataHeader(stream), stream)
    {
    }

    internal PureDataBlock(byte[] headerData, byte[] data) : base(new PureDataHeader(headerData), data)
    {
    }
}