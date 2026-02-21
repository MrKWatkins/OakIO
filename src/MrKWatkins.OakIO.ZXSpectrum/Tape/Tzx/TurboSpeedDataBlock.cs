namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

public sealed class TurboSpeedDataBlock(Stream stream) : TzxBlock<TurboSpeedDataHeader>(new TurboSpeedDataHeader(stream), stream);