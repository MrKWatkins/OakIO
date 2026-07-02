namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX pulse sequence block.
/// </summary>
public sealed class PulseSequenceHeader : TzxBlockHeader
{
    internal PulseSequenceHeader(byte[] data)
        : base(TzxBlockType.PulseSequence, data)
    {
    }

    /// <summary>
    /// Gets the number of pulses in the sequence.
    /// </summary>
    public byte NumberOfPulses => GetByte(0);

    /// <inheritdoc />
    public override int BlockLength => NumberOfPulses * 2;  // Each is a word.

    /// <inheritdoc />
    [Pure]
    public override string ToString() => $"{Type}: {NumberOfPulses} pulses";
}