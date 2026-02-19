namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class StopTheTapeIf48KBlock : TzxBlock<StopTheTapeIf48KHeader>
{
    public StopTheTapeIf48KBlock(Stream stream) : base(new StopTheTapeIf48KHeader(stream), stream)
    {
    }

    internal StopTheTapeIf48KBlock(byte[] headerData) : base(new StopTheTapeIf48KHeader(headerData), [])
    {
    }
}