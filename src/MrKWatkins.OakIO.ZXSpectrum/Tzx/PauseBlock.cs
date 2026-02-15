namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class PauseBlock(Stream stream) : TzxBlock<PauseHeader>(new PauseHeader(stream), stream);