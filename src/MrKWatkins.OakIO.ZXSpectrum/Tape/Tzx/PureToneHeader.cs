namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX pure tone block.
/// </summary>
public sealed class PureToneHeader : TzxBlockHeader
{
    private const int Size = 4;

    internal PureToneHeader()
        : base(TzxBlockType.PureTone, Size)
    {
    }

    internal PureToneHeader(Stream stream)
        : base(TzxBlockType.PureTone, Size, stream)
    {
    }

    internal PureToneHeader(byte[] data)
        : base(TzxBlockType.PureTone, data)
    {
    }

    /// <summary>
    /// Gets the length of each pulse in T-states.
    /// </summary>
    public ushort LengthOfPulse => GetUInt16(0);

    /// <summary>
    /// Gets the number of pulses in the tone.
    /// </summary>
    public ushort NumberOfPulses => GetUInt16(2);

    /// <inheritdoc />
    public override string ToString() => $"{Type}: {NumberOfPulses} x {LengthOfPulse} T-States";
}