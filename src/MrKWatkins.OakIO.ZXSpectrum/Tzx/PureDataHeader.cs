namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class PureDataHeader : TzxBlockHeader
{
    private const int Size = 10;

    internal PureDataHeader()
        : base(TzxBlockType.PureData, Size)
    {
    }

    internal PureDataHeader(Stream stream)
        : base(TzxBlockType.PureData, Size, stream)
    {
    }

    internal PureDataHeader(byte[] data)
        : base(TzxBlockType.PureData, data)
    {
    }

    public ushort TStatesInZeroBitPulse => GetWord(0);

    public ushort TStatesInOneBitPulse => GetWord(2);

    public byte UsedBitsInLastByte => GetByte(4);

    public ushort PauseAfterBlockMs => GetWord(5);

    public TimeSpan PauseAfter => TimeSpan.FromMilliseconds(PauseAfterBlockMs);

    public override int BlockLength => GetUInt24(7);

    public override string ToString() =>
        $"{Type}: 1/0 = {TStatesInOneBitPulse}/{TStatesInZeroBitPulse} T-States, length = {BlockLength}, used bits in last byte = {UsedBitsInLastByte}, pause after = {PauseAfter}";
}