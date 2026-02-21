namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

public sealed class StopBlock : PzxBlock<StopHeader>
{
    public StopBlock(Stream stream) : base(new StopHeader(stream), stream)
    {
    }

    internal StopBlock(byte[] headerData) : base(new StopHeader(headerData), [])
    {
    }

    public override string ToString() => Header.Only48k ? "Stop: 48k only" : "Stop: Always";
}