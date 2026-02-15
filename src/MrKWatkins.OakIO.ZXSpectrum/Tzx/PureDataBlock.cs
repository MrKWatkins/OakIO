namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class PureDataBlock(Stream stream) : TzxBlock<PureDataHeader>(new PureDataHeader(stream), stream);