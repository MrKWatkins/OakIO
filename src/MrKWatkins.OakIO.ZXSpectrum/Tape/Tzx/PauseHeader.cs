namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// Header for a TZX pause block.
/// </summary>
public sealed class PauseHeader : TzxBlockHeader
{
    private const int Size = 2;

    internal PauseHeader()
        : base(TzxBlockType.Pause, Size)
    {
    }

    internal PauseHeader(Stream stream)
        : base(TzxBlockType.Pause, Size, stream)
    {
    }

    internal PauseHeader(byte[] data)
        : base(TzxBlockType.Pause, data)
    {
    }

    /// <summary>
    /// Gets the pause duration in milliseconds.
    /// </summary>
    public ushort PauseMs => GetUInt16(0);

    /// <summary>
    /// Gets the pause duration as a <see cref="TimeSpan"/>.
    /// </summary>
    public TimeSpan Pause => TimeSpan.FromMilliseconds(PauseMs);

    /// <inheritdoc />
    public override string ToString() => $"{Type}: {Pause}";
}