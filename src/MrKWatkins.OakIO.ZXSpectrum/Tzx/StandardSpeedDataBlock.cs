namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class StandardSpeedDataBlock : TzxBlock<StandardSpeedDataHeader>
{
    public StandardSpeedDataBlock(Stream stream) : base(new StandardSpeedDataHeader(stream), stream)
    {
    }

    internal StandardSpeedDataBlock(byte[] headerData, byte[] bodyData) : base(new StandardSpeedDataHeader(headerData), bodyData)
    {
    }
}