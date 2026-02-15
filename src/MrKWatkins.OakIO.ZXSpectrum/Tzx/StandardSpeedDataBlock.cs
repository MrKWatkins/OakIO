namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class StandardSpeedDataBlock(Stream stream) : TzxBlock<StandardSpeedDataHeader>(new StandardSpeedDataHeader(stream), stream);