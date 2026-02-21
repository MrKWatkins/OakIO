namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

public sealed class LoopStartBlock(Stream stream) : TzxBlock<LoopStartHeader>(new LoopStartHeader(stream), stream);