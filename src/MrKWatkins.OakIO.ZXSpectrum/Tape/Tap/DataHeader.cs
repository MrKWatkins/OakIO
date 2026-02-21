namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

public sealed class DataHeader(ushort blockFlagAndChecksumLength) : TapHeader(TapBlockType.Data, blockFlagAndChecksumLength);