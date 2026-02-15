namespace MrKWatkins.OakIO.ZXSpectrum.Tap;

public sealed class DataHeader(ushort blockFlagAndChecksumLength) : TapHeader(TapBlockType.Data, blockFlagAndChecksumLength);