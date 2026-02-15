namespace MrKWatkins.OakIO.ZXSpectrum.Tzx;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class TurboSpeedDataHeader : TzxBlockHeader
{
    private const int Size = 18;

    public TurboSpeedDataHeader()
        : base(TzxBlockType.TurboSpeedData, Size)
    {
    }

    public TurboSpeedDataHeader(Stream stream)
        : base(TzxBlockType.TurboSpeedData, Size, stream)
    {
    }

    public ushort TStatesInPilotPulse => GetWord(0);

    public ushort TStatesInSyncFirstPulse => GetWord(2);

    public ushort TStatesInSyncSecondPulse => GetWord(4);

    public ushort TStatesInZeroBitPulse => GetWord(6);

    public ushort TStatesInOneBitPulse => GetWord(8);

    public ushort PulsesInPilotTone => GetWord(10);

    public byte UsedBitsInLastByte => GetByte(12);

    public ushort PauseAfterBlockMs => GetWord(13);

    public TimeSpan PauseAfter => TimeSpan.FromMilliseconds(PauseAfterBlockMs);

    public override int BlockLength => GetUInt24(15);

    public override string ToString() =>
        $"{Type}: Pilot = {PulsesInPilotTone} x {TStatesInPilotPulse} T-States, sync = {TStatesInSyncFirstPulse}/{TStatesInSyncSecondPulse} T-States, " +
        $"1/0 = {TStatesInOneBitPulse}/{TStatesInZeroBitPulse} T-States, length = {BlockLength}, used bits in last byte = {UsedBitsInLastByte}, pause after = {PauseAfter}";
}