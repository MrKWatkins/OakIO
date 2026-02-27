namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX turbo speed data block.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed class TurboSpeedDataHeader : TzxBlockHeader
{
    private const int Size = 18;

    /// <summary>
    /// Initializes a new instance of the <see cref="TurboSpeedDataHeader"/> class with default values.
    /// </summary>
    public TurboSpeedDataHeader()
        : base(TzxBlockType.TurboSpeedData, Size)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TurboSpeedDataHeader"/> class by reading from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    public TurboSpeedDataHeader(Stream stream)
        : base(TzxBlockType.TurboSpeedData, Size, stream)
    {
    }

    /// <summary>
    /// Gets the number of T-states per pilot pulse.
    /// </summary>
    public ushort TStatesInPilotPulse => GetWord(0);

    /// <summary>
    /// Gets the number of T-states in the first sync pulse.
    /// </summary>
    public ushort TStatesInSyncFirstPulse => GetWord(2);

    /// <summary>
    /// Gets the number of T-states in the second sync pulse.
    /// </summary>
    public ushort TStatesInSyncSecondPulse => GetWord(4);

    /// <summary>
    /// Gets the number of T-states per pulse for a zero bit.
    /// </summary>
    public ushort TStatesInZeroBitPulse => GetWord(6);

    /// <summary>
    /// Gets the number of T-states per pulse for a one bit.
    /// </summary>
    public ushort TStatesInOneBitPulse => GetWord(8);

    /// <summary>
    /// Gets the number of pulses in the pilot tone.
    /// </summary>
    public ushort PulsesInPilotTone => GetWord(10);

    /// <summary>
    /// Gets the number of used bits in the last byte of data.
    /// </summary>
    public byte UsedBitsInLastByte => GetByte(12);

    /// <summary>
    /// Gets the pause duration after this block in milliseconds.
    /// </summary>
    public ushort PauseAfterBlockMs => GetWord(13);

    /// <summary>
    /// Gets the pause duration after this block as a <see cref="TimeSpan"/>.
    /// </summary>
    public TimeSpan PauseAfter => TimeSpan.FromMilliseconds(PauseAfterBlockMs);

    /// <inheritdoc />
    public override int BlockLength => GetUInt24(15);

    /// <inheritdoc />
    public override string ToString() =>
        $"{Type}: Pilot = {PulsesInPilotTone} x {TStatesInPilotPulse} T-States, sync = {TStatesInSyncFirstPulse}/{TStatesInSyncSecondPulse} T-States, " +
        $"1/0 = {TStatesInOneBitPulse}/{TStatesInZeroBitPulse} T-States, length = {BlockLength}, used bits in last byte = {UsedBitsInLastByte}, pause after = {PauseAfter}";
}