namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Pzx;

/// <summary>
/// The header for a <see cref="PulseSequenceBlock" />.
/// </summary>
public sealed class PulseSequenceHeader : PzxBlockHeader
{
    internal PulseSequenceHeader()
        : base(PzxBlockType.PulseSequence, 4)
    {
    }

    internal PulseSequenceHeader(Stream stream)
        : base(PzxBlockType.PulseSequence, 4, stream)
    {
    }

    internal PulseSequenceHeader(byte[] data)
        : base(PzxBlockType.PulseSequence, data)
    {
    }
}