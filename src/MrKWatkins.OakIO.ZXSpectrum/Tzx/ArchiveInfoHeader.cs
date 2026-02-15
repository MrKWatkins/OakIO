namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

public sealed class ArchiveInfoHeader : TzxBlockHeader
{
    private const int Size = 3;

    internal ArchiveInfoHeader()
        : base(TzxBlockType.ArchiveInfo, Size)
    {
    }

    internal ArchiveInfoHeader(Stream stream)
        : base(TzxBlockType.ArchiveInfo, Size, stream)
    {
    }

    public ushort LengthOfWholeBlock => GetWord(0);

    public byte NumberOfTextStrings => GetByte(2);

    public override int BlockLength => LengthOfWholeBlock - 1;  // NumberOfTextStrings is included in LengthOfWholeBlock.

    public override string ToString() => $"{Type}: {NumberOfTextStrings} entries";
}