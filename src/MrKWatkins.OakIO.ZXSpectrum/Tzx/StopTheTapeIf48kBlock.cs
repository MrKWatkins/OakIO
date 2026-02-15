namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class StopTheTapeIf48KBlock(Stream stream) : TzxBlock<StopTheTapeIf48KHeader>(new StopTheTapeIf48KHeader(stream), stream);