namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX pulse sequence block.
/// </summary>
public sealed class PulseSequenceHeader : TzxBlockHeader
{
    private const int Size = 1;

    internal PulseSequenceHeader()
        : base(TzxBlockType.PulseSequence, Size)
    {
    }

    internal PulseSequenceHeader(Stream stream)
        : base(TzxBlockType.PulseSequence, Size, stream)
    {
    }

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
    public override string ToString() => $"{Type}: {NumberOfPulses} pulses";
}