namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block that generates a single tone consisting of identical pulses.
/// </summary>
public sealed class PureToneBlock : TzxBlock<PureToneHeader>
{
    internal PureToneBlock(byte[] headerData) : base(new PureToneHeader(headerData), [])
    {
    }
}