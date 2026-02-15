namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class TurboSpeedDataBlock(Stream stream) : TzxBlock<TurboSpeedDataHeader>(new TurboSpeedDataHeader(stream), stream);