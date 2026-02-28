namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX pure data block.
/// </summary>
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

    /// <summary>
    /// Gets the number of T-states per pulse for a zero bit.
    /// </summary>
    public ushort TStatesInZeroBitPulse => GetUInt16(0);

    /// <summary>
    /// Gets the number of T-states per pulse for a one bit.
    /// </summary>
    public ushort TStatesInOneBitPulse => GetUInt16(2);

    /// <summary>
    /// Gets the number of used bits in the last byte of data.
    /// </summary>
    public byte UsedBitsInLastByte => GetByte(4);

    /// <summary>
    /// Gets the pause duration after this block in milliseconds.
    /// </summary>
    public ushort PauseAfterBlockMs => GetUInt16(5);

    /// <summary>
    /// Gets the pause duration after this block as a <see cref="TimeSpan"/>.
    /// </summary>
    public TimeSpan PauseAfter => TimeSpan.FromMilliseconds(PauseAfterBlockMs);

    /// <inheritdoc />
    public override int BlockLength => GetUInt24(7);

    /// <inheritdoc />
    public override string ToString() =>
        $"{Type}: 1/0 = {TStatesInOneBitPulse}/{TStatesInZeroBitPulse} T-States, length = {BlockLength}, used bits in last byte = {UsedBitsInLastByte}, pause after = {PauseAfter}";
}