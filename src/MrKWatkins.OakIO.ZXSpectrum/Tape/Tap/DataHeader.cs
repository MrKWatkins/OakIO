namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// Header for a data block in a TAP file.
/// </summary>
/// <param name="blockFlagAndChecksumLength">The length of the block including the flag and checksum bytes.</param>
public sealed class DataHeader(ushort blockFlagAndChecksumLength) : TapHeader(TapBlockType.Data, blockFlagAndChecksumLength);