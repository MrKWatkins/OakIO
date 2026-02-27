namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tap;

/// <summary>
/// Header for a header block in a TAP file.
/// </summary>
public sealed class HeaderHeader : TapHeader
{
    /// <summary>
    /// Initialises a new instance of the <see cref="HeaderHeader" /> class.
    /// </summary>
    /// <param name="blockFlagAndChecksumLength">The length of the block including the flag and checksum bytes; must be 19.</param>
    public HeaderHeader(ushort blockFlagAndChecksumLength) : base(TapBlockType.Header, blockFlagAndChecksumLength)
    {
        if (blockFlagAndChecksumLength != 19)
        {
            throw new ArgumentOutOfRangeException(nameof(blockFlagAndChecksumLength), blockFlagAndChecksumLength, "Header + flag + checksum length must be 19.");
        }
    }
}