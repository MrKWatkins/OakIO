namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX loop start block.
/// </summary>
public sealed class LoopStartHeader : TzxBlockHeader
{
    internal LoopStartHeader(byte[] data)
        : base(TzxBlockType.LoopStart, data)
    {
    }

    /// <summary>
    /// Gets the number of times the loop should be repeated.
    /// </summary>
    public ushort NumberOfRepetitions => GetUInt16(0);

    /// <inheritdoc />
    [Pure]
    public override string ToString() => $"{Type}: {NumberOfRepetitions} repetitions";
}