namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

public sealed class PureToneBlock : TzxBlock<PureToneHeader>
{
    public PureToneBlock(Stream stream) : base(new PureToneHeader(stream), stream)
    {
    }

    internal PureToneBlock(byte[] headerData) : base(new PureToneHeader(headerData), [])
    {
    }
}