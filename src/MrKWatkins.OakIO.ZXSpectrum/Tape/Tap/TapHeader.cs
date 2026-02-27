using MrKWatkins.BinaryPrimitives;

namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// Base class for headers in a TAP file.
/// </summary>
public abstract class TapHeader : Header
{
    internal TapHeader(TapBlockType type, ushort blockFlagAndChecksumLength)
        : base(3, [(byte)blockFlagAndChecksumLength, (byte)(blockFlagAndChecksumLength >> 8), (byte)type])
    {
        Type = type;
    }

    /// <summary>
    /// Gets the block type of this header.
    /// </summary>
    public TapBlockType Type { get; }

    /// <summary>
    /// Gets the length of the block including the flag and checksum bytes.
    /// </summary>
    public ushort BlockFlagAndChecksumLength => Data.GetWord(0);

    /// <summary>
    /// Gets the length of the block data excluding the flag and checksum bytes.
    /// </summary>
    public ushort BlockLength => (ushort)(BlockFlagAndChecksumLength - 2);
}