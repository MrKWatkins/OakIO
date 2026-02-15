namespace MrKWatkins.OakIO.ZXSpectrum.Pzx;

public sealed class StopBlock(Stream stream) : PzxBlock<StopHeader>(new StopHeader(stream), stream)
{
    public override string ToString() => Header.Only48k ? "Stop: 48k only" : "Stop: Always";
}