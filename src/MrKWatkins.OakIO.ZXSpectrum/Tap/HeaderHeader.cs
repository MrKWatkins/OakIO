namespace MrKWatkins.OakIO.ZXSpectrum.Tap;

public sealed class HeaderHeader : TapHeader
{
    public HeaderHeader(ushort blockFlagAndChecksumLength) : base(TapBlockType.Header, blockFlagAndChecksumLength)
    {
        if (blockFlagAndChecksumLength != 19)
        {
            throw new ArgumentOutOfRangeException(nameof(blockFlagAndChecksumLength), blockFlagAndChecksumLength, "Header + flag + checksum length must be 19.");
        }
    }
}