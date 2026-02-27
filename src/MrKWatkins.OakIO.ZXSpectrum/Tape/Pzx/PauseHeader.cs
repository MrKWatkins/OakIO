namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// The header for a <see cref="PauseBlock" />.
/// </summary>
public sealed class PauseHeader : PzxBlockHeader
{
    internal PauseHeader()
        : base(PzxBlockType.Pause, 8)
    {
    }

    internal PauseHeader(Stream stream)
        : base(PzxBlockType.Pause, 8, stream)
    {
    }

    internal PauseHeader(byte[] data)
        : base(PzxBlockType.Pause, data)
    {
    }

    /// <summary>
    /// Gets the duration of the pause in T-states.
    /// </summary>
    public uint Duration => GetUInt32(StartIndex) & 0x7FFFFFFF;

    /// <summary>
    /// Gets a value indicating the initial pulse level.
    /// </summary>
    public bool InitialPulseLevel => GetBit(StartIndex + 3, 7);
}