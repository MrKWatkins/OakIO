namespace MrKWatkins.OakIO.ZXSpectrum.Tape.Tzx;

/// <summary>
/// A TZX block representing a pause or silence in the tape.
/// </summary>
public sealed class PauseBlock : TzxBlock<PauseHeader>
{
    internal PauseBlock(byte[] headerData) : base(new PauseHeader(headerData), [])
    {
    }
}