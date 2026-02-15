namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class LoopStartBlock(Stream stream) : TzxBlock<LoopStartHeader>(new LoopStartHeader(stream), stream);