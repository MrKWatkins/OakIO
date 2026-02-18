namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class PauseBlock : TzxBlock<PauseHeader>
{
    public PauseBlock(Stream stream) : base(new PauseHeader(stream), stream)
    {
    }

    internal PauseBlock(byte[] headerData) : base(new PauseHeader(headerData), [])
    {
    }
}