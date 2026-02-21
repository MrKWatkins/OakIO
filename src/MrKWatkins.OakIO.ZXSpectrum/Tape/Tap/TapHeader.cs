using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

public abstract class TapHeader : Header
{
    internal TapHeader(TapBlockType type, ushort blockFlagAndChecksumLength)
        : base(3, [(byte)blockFlagAndChecksumLength, (byte)(blockFlagAndChecksumLength >> 8), (byte)type])
    {
        Type = type;
    }

    public TapBlockType Type { get; }

    public ushort BlockFlagAndChecksumLength => Data.GetWord(0);

    public ushort BlockLength => (ushort)(BlockFlagAndChecksumLength - 2);
}