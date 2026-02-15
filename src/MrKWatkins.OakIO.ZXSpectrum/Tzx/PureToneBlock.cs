namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class PureToneBlock(Stream stream) : TzxBlock<PureToneHeader>(new PureToneHeader(stream), stream);