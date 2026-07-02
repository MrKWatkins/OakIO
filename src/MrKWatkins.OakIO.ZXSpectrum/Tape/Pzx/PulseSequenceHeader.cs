namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// The header for a <see cref="PulseSequenceBlock" />.
/// </summary>
public sealed class PulseSequenceHeader : PzxBlockHeader
{
    internal PulseSequenceHeader(byte[] data)
        : base(PzxBlockType.PulseSequence, data)
    {
    }
}