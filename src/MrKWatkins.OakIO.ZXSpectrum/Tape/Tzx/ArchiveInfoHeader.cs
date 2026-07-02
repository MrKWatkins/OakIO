namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX archive info block.
/// </summary>
public sealed class ArchiveInfoHeader : TzxBlockHeader
{
    internal ArchiveInfoHeader(byte[] data)
        : base(TzxBlockType.ArchiveInfo, data)
    {
    }

    /// <summary>
    /// Gets the length of the whole archive info block in bytes.
    /// </summary>
    public ushort LengthOfWholeBlock => GetUInt16(0);

    /// <summary>
    /// Gets the number of text strings in this archive info block.
    /// </summary>
    public byte NumberOfTextStrings => GetByte(2);

    /// <inheritdoc />
    public override int BlockLength => LengthOfWholeBlock - 1;  // NumberOfTextStrings is included in LengthOfWholeBlock.

    /// <inheritdoc />
    [Pure]
    public override string ToString() => $"{Type}: {NumberOfTextStrings} entries";
}