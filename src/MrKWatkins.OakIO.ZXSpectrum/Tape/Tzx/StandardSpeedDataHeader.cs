namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX standard speed data block.
/// </summary>
public sealed class StandardSpeedDataHeader : TzxBlockHeader
{
    internal StandardSpeedDataHeader(byte[] data)
        : base(TzxBlockType.StandardSpeedData, data)
    {
    }

    /// <summary>
    /// Gets the pause duration after this block in milliseconds.
    /// </summary>
    public ushort PauseAfterBlockMs => GetUInt16(0);

    /// <summary>
    /// Gets the pause duration after this block as a <see cref="TimeSpan"/>.
    /// </summary>
    public TimeSpan PauseAfter => TimeSpan.FromMilliseconds(PauseAfterBlockMs);

    /// <inheritdoc />
    public override int BlockLength => GetUInt16(2);

    /// <inheritdoc />
    [Pure]
    public override string ToString() =>
        $"{Type}: Length = {BlockLength}, pause after = {PauseAfter}";
}