namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class LoopEndBlock(Stream stream) : TzxBlock<LoopEndHeader>(new LoopEndHeader(stream), stream);