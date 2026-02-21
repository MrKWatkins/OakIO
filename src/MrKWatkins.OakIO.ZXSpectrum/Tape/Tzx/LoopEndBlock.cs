namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

public sealed class LoopEndBlock(Stream stream) : TzxBlock<LoopEndHeader>(new LoopEndHeader(stream), stream);