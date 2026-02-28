namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// The header for a <see cref="StopBlock" />.
/// </summary>
public sealed class StopHeader : PzxBlockHeader
{
    internal StopHeader()
        : base(PzxBlockType.Stop, 6)
    {
    }

    internal StopHeader(Stream stream)
        : base(PzxBlockType.Stop, 6, stream)
    {
    }

    internal StopHeader(byte[] data)
        : base(PzxBlockType.Stop, data)
    {
    }

    /// <summary>
    /// Gets a value indicating whether the stop applies only to 48K mode.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public bool Only48k => GetUInt16(StartIndex) == 1;
}