namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block that signals the tape should stop if the machine is a 48K ZX Spectrum.
/// </summary>
public sealed class StopTheTapeIf48KBlock : TzxBlock<StopTheTapeIf48KHeader>
{
    internal StopTheTapeIf48KBlock(byte[] headerData) : base(new StopTheTapeIf48KHeader(headerData), [])
    {
    }
}